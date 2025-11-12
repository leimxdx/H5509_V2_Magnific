using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.EMMA;
using IHardwareGXN.GXNCard;
using MotionCommon;
using Sunny.UI.Win32;
using static System.Windows.Forms.LinkLabel;
using static H5509_V2.APPmaster;
using static MotionCommon.AxisRunInfo;

namespace H5509_V2
{
    public class MotionEx //单例模式实现类
    {
        ////创建一个Motion对象(后续需改为private,不能被外部访问)
        //public Motion Motion = new Motion();

        ////(1)//懒汉式单例

        //////创建一个静态对象保存当前的实例
        ////private static volatile MotionEx instance;

        //////定义一个锁
        ////private static readonly object objlock = new object(); //为了避免多个线程同时调用GetLoadBalancer方法，在C#中，我们可以使用关键字lock/Moniter.Enter+Exit等来实现

        //////创建一个私有的构造方法
        ////private MotionEx() { } //为了防止在外部对单例类实例化，它的构造函数被设为private；

        ////public static MotionEx GetInstance() //在单例类的内部实现只生成一个实例，同时它提供一个静态的GetInstance()方法，让客户可以访问它的唯一实例；
        ////{
        ////    if (instance == null) 
        ////    {
        ////        lock (objlock) //线程A先进入lock代码块内，执行实例化代码。此时线程B排队吃瓜等待，必须等待线程A执行完毕后才能进入lock代码块
        ////        {
        ////            if (instance == null) //当A执行完毕时，线程B并不知道实例已经被创建，将继续创建新的实例，从而导致多个单例对象。因此，开发人员需要进一步改进，于是就有了双重检查锁定（Double-Check Locking）
        ////            {
        ////                instance = new MotionEx();
        ////            }
        ////        }
        ////    }
        ////    return instance;
        ////}

        ////(2)一种更好的单例实现

        ///*
        //    该实现方法在内部定义了一个私有类型Nested。当第一次用到这个嵌套类型的时候，会调用静态构造函数创建LoadBalancer的实例instance。如果我们不调用属性MotionEx.GetMotionExInstance()，
        //  那么就不会触发.NET运行时（CLR）调用Nested，也就不会创建实例，因此也就保证了按需创建实例（或延迟初始化）。
        //可见，此方法既可以实现延迟加载，又可以保证线程安全，不影响系统性能。但其缺点是与具体编程语言本身的特性相关，有一些面向对象的编程语言并不支持此种方式。
        //*/

        //// 公共静态成员方法，返回唯一实例

        //public static MotionEx GetInstance()
        //{
        //    return Nested.instance;
        //}


        //// 使用内部类+静态构造函数实现延迟初始化(只有在 GetMotionExInstance() 被调用时才会被加载，从而触发实例化)

        //class Nested //静态内部类Nested只有在被引用的时候才会被加载，而GetMotionExInstance方法第一次被调用时才会访问Nested.instance，这时候才会触发Nested类的加载和初始化
        //{
        //    static Nested() { }
        //    internal static readonly MotionEx instance = new MotionEx();
        //}

        //单例模式的主要缺点
        /*
          （1）单例模式中没有抽象层，因此单例类的扩展有很大的困难。
　　      （2）单例类的职责过重，在一定程度上违背了单一职责的原则。因为单例类既提供了业务方法，又提供了创建对象的方法（工厂方法），将对象的创建和对象本身的功能耦合在一起。不够，很多时候我们都需要取得平衡。
　　      （3）很多高级面向对象编程语言如C#和Java等都提供了垃圾回收机制，如果实例化的共享对象长时间不被利用，系统则会认为它是垃圾，于是会自动销毁并回收资源，下次利用时又得重新实例化，这将导致共享的单例对象状态的丢失。
        */

        //单例模式的适用场景
        /*
          （1）系统只需要一个实例对象。例如：系统要求提供一个唯一的序列号生成器或者资源管理器，又或者需要考虑资源消耗太大而只允许创建一个对象。
　　      （2）客户调用类的单个实例只允许使用一个公共访问点，除了该公共访问点，不能通过其他途径访问该实例
        */

        //public MotionEx(Motion motion)
        //{
        //    Motion = motion;
        //}

        public static IGXNInterface IMotion;

        //public static MotionEx GetInstance()
        //{
        //    return instance;
        //}

        public MotionEx(IGXNInterface motion)
        {
            IMotion = motion;
        }

        #region 轴卡初始化

        public string GSNInitCard()
        {
            return IMotion.InitCard();
        }

        public string AllAxisStop()
        {
            return IMotion.AllAxisStop();
        }

        #endregion


        #region 轴运行信息

        /// <summary>
        /// 获取轴状态
        /// </summary>
        /// <returns></returns>
        public string GetAxisStatus(string axisName, out int sts)
        {
            sts = 0;
            string result = GetAxisRunInfo(axisName, out AxisRunInfo axisRun);
            if (result != "Success") return result;

            return IMotion.GetAxisStatus(axisRun.核号, axisRun.轴号, out sts);
        }

        /// <summary>
        /// 轴是否使能
        /// </summary>
        /// <param name="axisName"></param>
        /// <param name="enable"></param>
        /// <returns></returns>
        public string GetAxisEnable(string axisName, out bool enable)
        {
            enable = false;
            string result = GetAxisRunInfo(axisName, out AxisRunInfo axisRun);
            if (result != "Success") return result;
            return IMotion.IsEnable(axisRun.核号, axisRun.轴号, out enable);
        }

        /// <summary>
        /// 获取实时位置
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <returns>实时位置</returns>
        public string GetCmdEncPos(string axisName, out double pos)
        {
            pos = 0;

            string result = GetAxisRunInfo(axisName, out AxisRunInfo axisRun);
            if (result != "Success") return result;

            return IMotion.GetCmdEncPos(axisRun.核号, axisRun.轴号, out pos);
        }

        /// <summary>
        /// 获取实时速度
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        public string GetCmdEncVel(string axisName, out double pos)
        {
            pos = 0;

            string result = GetAxisRunInfo(axisName, out AxisRunInfo axisRun);
            if (result != "Success") return result;

            return IMotion.GetCmdEncVel(axisRun.核号, axisRun.轴号, out pos);
        }

        /// <summary>
        /// 获取规划位置
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        public string GetCmdPrfPos(string axisName, out int pos)
        {
            pos = 0;

            string result = GetAxisRunInfo(axisName, out AxisRunInfo axisRun);
            if (result != "Success") return result;

            return IMotion.GetCmdPrfPos(axisRun.核号, axisRun.轴号, out pos);
        }

        /// <summary>
        /// 获取规划速度
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        public string GetCmdPrfVel(string axisName, out double pos)
        {
            pos = 0;

            string result = GetAxisRunInfo(axisName, out AxisRunInfo axisRun);
            if (result != "Success") return result;

            return IMotion.GetCmdPrfVel(axisRun.核号, axisRun.轴号, out pos);
        }

        #endregion

        #region 轴回零

        /// <summary>
        /// 多轴回零
        /// </summary>
        /// <param name="axisNames"></param>
        /// <returns></returns>
        public string AxisHome(string[] axisNames, int type = 0)
        {
            List<AxisHomeInfo> axisHomes = new List<AxisHomeInfo>();
            string result = GetAxisHomeInfo(axisNames, out axisHomes);
            if (result == "Success")
            {
                result = IMotion.MultiaxisHome(axisHomes);
                if (result == "Success")
                {
                    Task<string> task = Task.Run(() =>
                    {
                        return IMotion.WaitAxisHome(axisHomes, type);
                    });

                    result = task.Result;
                }
            }
            return result;
        }

        /// <summary>
        /// 单轴回零
        /// </summary>
        /// <param name="axisName"></param>
        /// <returns></returns>
        public string AxisHome(string axisName, int type = 0)
        {
            string result = APPmaster.GetAxisHomeInfo(axisName, out AxisHomeInfo axisHome);
            if (result == "Success")
            {
                result = IMotion.UniaxialHome(axisHome);
                if (result == "Success")
                {
                    Task<string> task = Task.Run(() =>
                    {
                        return IMotion.WaitAxisHome(axisHome, type);//等待回零完成
                    });

                    result = task.Result;
                    if (result != "Success")
                    {
                        RunLog(result);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 单轴回零
        /// </summary>
        /// <param name="axisName"></param>
        /// <returns></returns>
        public string AxisHome(AxisHomeInfo axisHome, int type = 0)
        {
            string result = IMotion.UniaxialHome(axisHome);
            if (result == "Success")
            {
                Task<string> task = Task.Run(() =>
                {
                    return IMotion.WaitAxisHome(axisHome, type);//等待回零完成
                });

                result = task.Result;
                if (result != "Success")
                {
                    RunLog(result);
                }
            }
            return result;
        }

        public string GetDiHome(string[] axisNames, out string[] iovalues)
        {
            iovalues = new string[axisNames.Length];
            string result = GetAxisHomeInfo(axisNames, out List<AxisHomeInfo> axisHomes);
            if (result == "Success")
            {
                for (int i = 0; i < axisHomes.Count; i++)
                {
                    result = IMotion.GNM601_GetDi(axisHomes[i].核号, 3, out string value);
                    if (result == "Success")
                    {
                        iovalues[i] = value[axisHomes[i].轴号 - 1].ToString();
                    }
                    else return result;
                }
            }
            else return result;
            return "Success";
        }

        #endregion

        #region 动作

        /// <summary>
        /// 检查轴当前位置与目标位置的差值是否大于跟随误差
        /// </summary>
        /// <param name="axisName"></param>
        /// <param name="mm"></param>
        /// <returns></returns>
        public bool AxisPoinitData(string axisName, double mm)
        {
            string result = GetAxisRunInfo(axisName, out AxisRunInfo axisRun);
            if (result != "Success")
            {
                RunLog($"执行AxisPoinitData({axisName},{mm})异常，{result}");
                return false;
            }

            int prfPos = Convert.ToInt32(Math.Round(mm / axisRun.步长mm));
            IMotion.GetCmdEncPos(axisRun.核号, axisRun.轴号, out double encPos);
            double posvalue = Math.Abs(prfPos - encPos);
            return posvalue > axisRun.跟随误差;
        }

        /// <summary>
        /// 单轴绝对移动
        /// </summary>
        /// <param name="axisName">轴名称</param>
        /// <param name="pos">目标位值</param>
        /// <param name="type">暂停类型</param>
        /// <param name="isend">开始或结束时减速</param>
        /// <param name="move"></param>
        /// <param name="pos2"></param>
        /// <returns></returns>
        public string AxisAbsoluteMove(string axisName, double pos, int type = 0, int isdown = 0)
        {
            string result = GetAxisRunInfo(axisName, out AxisRunInfo axisRunInfo); //停止类型，跟随误差，二段速使能，二段速开始时速度，二段速开始时减速距离，二段速结束时速度，二段速结束时减速距离
            if (result != "Success") return result;

            double targetvel = axisRunInfo.运行速度;
            if(isdown == 0 && axisRunInfo.轴二段速方式 >= 2) targetvel = axisRunInfo.启动时减速速度; //如果是起点慢，则开始时速度慢 如果是上升才需要慢 0 ：无需约束， 1：

            AxisRun axisRun = new AxisRun() //数据库待添加字段：停止类型，跟随误差，跟随误差检测，轴二段速方式，二段速启动时减速速度，二段速启动时减速距离，二段速结束时减速速度，二段速结束时减速距离
            {                                             //轴停止后等待超时时间
                轴名称 = axisRunInfo.轴名称,
                核号 = axisRunInfo.核号,
                轴号 = axisRunInfo.轴号,
                目标位置 = Convert.ToInt32(Math.Round(pos / axisRunInfo.步长mm)),
                目标速度 = targetvel,
                目标加速度 = axisRunInfo.加速度,
                目标减速度 = axisRunInfo.减速度,
                平滑时间 = axisRunInfo.平滑时间,

                停止类型 = axisRunInfo.停止类型, //0:平滑停止 1:急停停止
                跟随误差 = axisRunInfo.跟随误差,
                跟随误差检测 = axisRunInfo.跟随误差检测,
                暂停类型 = type, //0:允许暂停 1:不允许暂停
                二段速方式 = axisRunInfo.轴二段速方式, //0：无多段速  1：终点慢 2：起点慢 3：终点慢和起点慢

                二段速结束时减速速度 = axisRunInfo.结束时减速速度,
                二段速结束时减速距离 = axisRunInfo.二段速结束时减速距离,
                二段速启动结束后恢复速度 = axisRunInfo.运行速度,
                二段速启动时减速距离 = axisRunInfo.二段速启动时减速距离,

                二段速统一更改 = false, //出入料Z吸嘴专用
                轴动作超时时间 = 60, //60s
                轴停后等待超时时间 = axisRunInfo.轴停后等待超时时间, //2S
            };

            result = IMotion.UniaxialAbsoluteMoveAbs(axisRun);
            if (result != "Success") return result;

            Task<string> task = Task.Run(() =>
            {
                return IMotion.WaitAxisMove(axisRun, axisRun.目标位置);//, type, move, pos2
            });

            return task.Result;
        }

        /// <summary>
        /// 单轴相对移动
        /// </summary>
        /// <param name="axisName"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public string AxisRelativeMove(string axisName, double pos)
        {
            string result = GetAxisRunInfo(axisName, out AxisRunInfo axisRunInfo);
            if (result != "Success") return result;

            AxisRun axisRun = new AxisRun()
            {
                轴名称 = axisRunInfo.轴名称,
                核号 = axisRunInfo.核号,
                轴号 = axisRunInfo.轴号,
                目标位置 = Convert.ToInt32(Math.Round(pos / axisRunInfo.步长mm)),
                目标速度 = axisRunInfo.运行速度,
                目标加速度 = axisRunInfo.加速度,
                目标减速度 = axisRunInfo.减速度,
                平滑时间 = axisRunInfo.平滑时间,
            };

            result = IMotion.UniaxialRelativeMove(axisRun, out int posPulse);
            if (result != "Success") return result;

            Task<string> task = Task.Run(() =>
            {
                return IMotion.WaitAxisMove(axisRun, posPulse, 1);
            });

            return task.Result;
        }

        /// <summary>
        /// 多轴绝对移动
        /// </summary>
        /// <param name="axisNames"></param>
        /// <param name="poss"></param>
        /// <returns></returns>
        public string AxisMove(string[] axisNames, double[] poss, int type = 0, int isdown = 0, bool isunifyChange = false)//
        {
            List<int> prfPos = new List<int>();
            List<AxisRun> axisRuns = new List<AxisRun>();
            for (int i = 0; i < axisNames.Length; i++)
            {
                string result = GetAxisRunInfo(axisNames[i], out AxisRunInfo axisRunInfo);
                if (result != "Success") return result;

                double targetstartvel = axisRunInfo.运行速度;
                if (isdown == 0 && axisRunInfo.轴二段速方式 >= 2) //(开始时减速)如果是上升(isdown = 0)开始时减速，如果是下降(isdown = 1)则不减速
                {                                                
                    targetstartvel = axisRunInfo.启动时减速速度; //如果是起点慢，则开始时减速
                }

                double targetendvel = axisRunInfo.运行速度;
                if (isdown == 1 && axisRunInfo.轴二段速方式 != 0 && axisRunInfo.轴二段速方式 != 2) //(结束时减速)如果是上升(isdown = 0)结束时不减速，如果是下降(isdown = 1)则才减速
                {
                    targetendvel = axisRunInfo.结束时减速速度; //如果是终点慢，则结束时减速
                }

                int movepos = Convert.ToInt32(Math.Round(poss[i] / axisRunInfo.步长mm));
                axisRuns.Add(new AxisRun()
                {
                    轴名称 = axisRunInfo.轴名称,
                    核号 = axisRunInfo.核号,
                    轴号 = axisRunInfo.轴号,
                    目标位置 = movepos,
                    目标速度 = targetstartvel,
                    目标加速度 = axisRunInfo.加速度,
                    目标减速度 = axisRunInfo.减速度,
                    平滑时间 = axisRunInfo.平滑时间,

                    停止类型 = axisRunInfo.停止类型, //0:平滑停止 1:急停停止
                    跟随误差 = axisRunInfo.跟随误差,
                    跟随误差检测 = axisRunInfo.跟随误差检测,
                    暂停类型 = type, //0:允许暂停 1:不允许暂停
                    二段速方式 = axisRunInfo.轴二段速方式, //0：无多段速  1：终点慢 2：起点慢 3：终点慢和起点慢

                    二段速结束时减速速度 = targetendvel,
                    二段速结束时减速距离 = axisRunInfo.二段速结束时减速距离,
                    二段速启动结束后恢复速度 = axisRunInfo.运行速度,
                    二段速启动时减速距离 = axisRunInfo.二段速启动时减速距离,

                    二段速统一更改 = isunifyChange, //出入料Z吸嘴专用
                    轴动作超时时间 = 60, //60s
                    轴停后等待超时时间 = axisRunInfo.轴停后等待超时时间, //2S
                });
                prfPos.Add(movepos);
            }

            string result2 = IMotion.MultiaxisAbsoluteMoveAbs(axisRuns);
            if (result2 != "Success") return result2;

            Task<string> task = Task.Run(() =>
            {
                return IMotion.WaitAxisMove(axisRuns, prfPos, type);//等待回零完成
            });

            return task.Result;

        }

        /// <summary>
        /// Jog运动
        /// </summary>
        /// <param name="axisName"></param>
        /// <param name="jogspeed"></param>
        /// <returns></returns>
        public string AxisJogMove(string axisName, bool ispositive)
        {
            string result = GetAxisRunInfo(axisName, out AxisRunInfo axisRunInfo);
            if (result != "Success") return result;

            int pn = ispositive ? 1 : -1; //1:正方向Jog运动 -1:负方向运动

            AxisRun axisRun = new AxisRun()
            {
                轴名称 = axisRunInfo.轴名称,
                核号 = axisRunInfo.核号,
                轴号 = axisRunInfo.轴号,
                目标速度 = axisRunInfo.运行速度 * pn, //jogspeed
                目标加速度 = axisRunInfo.加速度,
                目标减速度 = axisRunInfo.减速度,
                Jog平滑时间 = axisRunInfo.平滑时间,
            };

            return IMotion.JogMove(axisRun);
        }

        /// <summary>
        /// 轴停止移动
        /// </summary>
        /// <param name="axisName"></param>
        /// <param name="isSmoothStop"></param>
        /// <returns></returns>
        public string AxisStopMove(string axisName, int isSmoothStop)
        {
            string result = GetAxisRunInfo(axisName, out AxisRunInfo axisRunInfo);
            if (result != "Success") return result;

            return IMotion.AxisStop(axisRunInfo.核号, axisRunInfo.轴号, isSmoothStop);
        }

        #endregion

        #region IO

        /// <summary>
        /// 获取单个IO信息
        /// </summary>
        /// <param name="ioName"></param>
        /// <returns></returns>
        public string GetIOInfo(string ioName, out IOInfo ioInfo)
        {
            ioInfo = null;
            for (int i = 0; i < APPmaster.IOList.Count; i++)
            {
                if (ioName.Substring(0, 4) == APPmaster.IOList[i].IO编号)
                {
                    ioInfo = APPmaster.IOList[i];
                    break;
                }
            }
            if (ioInfo != null) return "Success";
            else return "IO编号不存在";

        }

        /// <summary>
        /// 获取多个IO信息
        /// </summary>
        /// <returns></returns>
        private string GetIOInfo(string[] ioNames, out List<IOInfo> ioInfos)
        {
            ioInfos = new List<IOInfo>();

            foreach (var item in APPmaster.IOList)
            {
                for (int i = 0; i < ioNames.Length; i++)
                {
                    if (ioNames[i].Split('_')[0] == item.IO编号)
                    {
                        ioInfos.Add(item);
                        break;
                    }
                }
                if (ioInfos.Count == ioNames.Length)
                {
                    break;
                }
            }
            if (ioInfos.Count == ioNames.Length) return "Success";
            else return "有IO编号不存在";
        }

        public string GetDi(string 模块类型, short 核号, short 索引, short 类型, out string value)
        {
            value = "";
            if (模块类型 == "gLink200")
            {
                return IMotion.GLink_GetExtDi(核号, 索引, out value);
            }
            else if (模块类型 == "GNM601")
            {
                return IMotion.GNM601_GetDi(核号, 类型, out value);
            }
            else return "未知IO模块";
        }

        /// <summary>
        /// 获取输入IO
        /// </summary>
        /// <param name="iOInfo"></param>
        /// <param name="iovalue"></param>
        /// <returns></returns>
        public string GetDi(IOInfo iOInfo, out string iovalue)
        {
            iovalue = "";
            if (iOInfo.模块类型 == "gLink200")
            {
                string result = IMotion.GLink_GetExtDi(iOInfo.核号, iOInfo.软件索引, out string value);
                if (result == "Success")
                {
                    char[] chars = value.ToCharArray();
                    iovalue = chars[0].ToString();
                }
                else return result;
            }
            else if (iOInfo.模块类型 == "GNM601")
            {
                string result = IMotion.GNM601_GetDiBit(iOInfo.核号, 4, iOInfo.软件索引, out short value);
                if (result == "Success")
                {
                    iovalue = value.ToString();
                }
                else return result;
            }
            else return "未知IO模块";

            return "Success";
        }

        /// <summary>
        /// 获取一个输入IO
        /// </summary>
        /// <param name="ioName"></param>
        /// <param name="iovalue"></param>
        /// <returns></returns>
        public string GetDi(string ioName, out string iovalue)
        {
            iovalue = "";
            string result = GetIOInfo(ioName, out IOInfo iOInfo);
            if (result == "Success")
            {
                return GetDi(iOInfo, out iovalue);
            }
            else return result;
        }

        /// <summary>
        /// 获取多个输入IO
        /// </summary>
        /// <param name="ioNames"></param>
        /// <param name="iovalues"></param>
        /// <returns></returns>
        public string GetDi(string[] ioNames, out string[] iovalues)
        {
            iovalues = new string[ioNames.Length];

            string result = GetIOInfo(ioNames, out List<IOInfo> ioInfos);

            if (result == "Success")
            {
                for (int i = 0; i < ioInfos.Count; i++)
                {
                    result = GetDi(ioNames[i], out string iovalue);
                    if (result == "Success")
                    {
                        iovalues[i] = iovalue;
                    }
                    else return result;
                }

                return "Success";
            }
            else return result;
        }

        public string GetDO(string 模块类型, short 核号, short 索引, short 类型, out string value)
        {
            value = "";
            if (模块类型 == "gLink200")
            {
                return IMotion.GLink_GetExtDO(核号, 索引, out value);
            }
            else if (模块类型 == "GNM601")
            {
                return IMotion.GNM601_GetDo(核号, 类型, out value);
            }
            else return "未知IO模块";
        }

        /// <summary>
        /// 获取输出IO
        /// </summary>
        /// <param name="iOInfo"></param>
        /// <param name="iovalue"></param>
        /// <returns></returns>
        public string GetDO(IOInfo iOInfo, out string iovalue)
        {
            iovalue = "";
            if (iOInfo.模块类型 == "gLink200")
            {
                string result = IMotion.GLink_GetExtDO(iOInfo.核号, iOInfo.软件索引, out string value);
                if (result == "Success")
                {
                    char[] chars = value.ToCharArray();
                    iovalue = chars[0].ToString();
                }
                else return result;
            }
            else if (iOInfo.模块类型 == "GNM601")
            {
                string result = IMotion.GNM601_GetDo(iOInfo.核号, 12, out string value);
                if (result == "Success")
                {
                    char[] chars = value.ToCharArray();
                    iovalue = chars[iOInfo.软件索引 - 1].ToString();
                }
                else return result;
            }
            else return "未知IO模块";

            return "Success";
        }

        /// <summary>
        /// 获取一个输出IO
        /// </summary>
        /// <param name="ioName"></param>
        /// <param name="iovalue"></param>
        /// <returns></returns>
        public string GetDO(string ioName, out string iovalue)
        {
            iovalue = "";
            string result = GetIOInfo(ioName, out IOInfo iOInfo);
            if (result == "Success")
            {
                return GetDO(iOInfo, out iovalue);
            }
            else return result;
        }

        /// <summary>
        /// 获取多个输出IO
        /// </summary>
        /// <param name="ioNames"></param>
        /// <param name="iovalues"></param>
        /// <returns></returns>
        public string GetDO(string[] ioNames, out string[] iovalues)
        {
            iovalues = new string[ioNames.Length];

            string result = GetIOInfo(ioNames, out List<IOInfo> ioInfos);

            if (result == "Success")
            {
                for (int i = 0; i < ioInfos.Count; i++)
                {
                    result = GetDO(ioNames[i], out string iovalue);
                    if (result == "Success")
                    {
                        iovalues[i] = iovalue;
                    }
                    else return result;
                }

                return "Success";
            }
            else return result;
        }

        /// <summary>
        /// 设置IO输出
        /// </summary>
        /// <param name="iOInfo"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string SetDo(IOInfo iOInfo, bool value)
        {
            if (iOInfo.模块类型 == "gLink200")
            {
                return IMotion.GLink_SetExtDoBit(iOInfo.核号, iOInfo.软件索引, value);
            }
            else if (iOInfo.模块类型 == "GNM601")
            {
                return IMotion.GNM601_SetDoBit(iOInfo.核号, 12, iOInfo.软件索引, value);
            }
            else return "未知IO模块";
        }

        //public string GetGNM601_DI_1()
        //{

        //}

        /// <summary>
        /// 单个IO输出
        /// </summary>
        /// <param name="ioName">IO编号</param>
        /// <param name="value">输出值，1：设置输出为低电平， 0：设置输出为高电平。</param>
        /// <returns></returns>
        public string SetDo(string ioName, bool value)
        {
            string result = GetIOInfo(ioName, out IOInfo iOInfo);
            if (result == "Success")
            {
                return SetDo(iOInfo, value);
            }
            else return result;
        }

        //public string SetDo(string ioName, int num, bool value)
        //{
        //string result = GetIOInfo(ioName, out IOInfo iOInfo);
        //if (！"是否同模块")
        //{
        //    return "执行失败，不在同以模块"；
        //}

        //string sql = "select count(*) from ioinfo where 核='" + iOInfo.核号 + "',模块类型='" + iOInfo.模块类型 + "'";


        //if (result == "Success")
        //{
        //    for (int i = 0; i < num; i++)
        //    {
        //        SetDo(iOInfo, value);
        //        iOInfo.软件索引++;
        //    }
        //    return SetDo(iOInfo, value);
        //}
        //else return result;
        //}

        /// <summary>
        /// 多个IO输出
        /// </summary>
        /// <param name="ioNames"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public string SetDo(string[] ioNames, bool[] values)
        {
            string result = GetIOInfo(ioNames, out List<IOInfo> ioInfos);
            if (result == "Success")
            {
                for (int i = 0; i < ioInfos.Count; i++)
                {
                    result = SetDo(ioInfos[i], values[i]);
                    if (result != "Success")
                    {
                        return result;
                    }
                }
                return "Success";
            }
            else return result;
        }

        #endregion
    }
}
