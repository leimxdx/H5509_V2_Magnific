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
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Office2019.Excel.ThreadedComments;
using DocumentFormat.OpenXml.Spreadsheet;
using Sunny.UI.Win32;
using static System.Windows.Forms.LinkLabel;
using static GTN.mc;
using static H5509_V2.APPmaster;

namespace H5509_V2
{
    public class MotionEx //单例模式实现类
    {
        //创建一个Motion对象(后续需改为private,不能被外部访问)
        public Motion Motion = new Motion();

        //(1)//懒汉式单例

        ////创建一个静态对象保存当前的实例
        //private static volatile MotionEx instance;

        ////定义一个锁
        //private static readonly object objlock = new object(); //为了避免多个线程同时调用GetLoadBalancer方法，在C#中，我们可以使用关键字lock/Moniter.Enter+Exit等来实现

        ////创建一个私有的构造方法
        //private MotionEx() { } //为了防止在外部对单例类实例化，它的构造函数被设为private；

        //public static MotionEx GetInstance() //在单例类的内部实现只生成一个实例，同时它提供一个静态的GetInstance()方法，让客户可以访问它的唯一实例；
        //{
        //    if (instance == null) 
        //    {
        //        lock (objlock) //线程A先进入lock代码块内，执行实例化代码。此时线程B排队吃瓜等待，必须等待线程A执行完毕后才能进入lock代码块
        //        {
        //            if (instance == null) //当A执行完毕时，线程B并不知道实例已经被创建，将继续创建新的实例，从而导致多个单例对象。因此，开发人员需要进一步改进，于是就有了双重检查锁定（Double-Check Locking）
        //            {
        //                instance = new MotionEx();
        //            }
        //        }
        //    }
        //    return instance;
        //}

        //(2)一种更好的单例实现

        /*
            该实现方法在内部定义了一个私有类型Nested。当第一次用到这个嵌套类型的时候，会调用静态构造函数创建LoadBalancer的实例instance。如果我们不调用属性MotionEx.GetMotionExInstance()，
          那么就不会触发.NET运行时（CLR）调用Nested，也就不会创建实例，因此也就保证了按需创建实例（或延迟初始化）。
        可见，此方法既可以实现延迟加载，又可以保证线程安全，不影响系统性能。但其缺点是与具体编程语言本身的特性相关，有一些面向对象的编程语言并不支持此种方式。
        */

        // 公共静态成员方法，返回唯一实例

        public static MotionEx GetInstance()
        {
            return Nested.instance;
        }

        // 使用内部类+静态构造函数实现延迟初始化(只有在 GetMotionExInstance() 被调用时才会被加载，从而触发实例化)

        class Nested //静态内部类Nested只有在被引用的时候才会被加载，而GetMotionExInstance方法第一次被调用时才会访问Nested.instance，这时候才会触发Nested类的加载和初始化
        {
            static Nested() { }
            internal static readonly MotionEx instance = new MotionEx();
        }

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

        #region 轴卡初始化

        public string GSNInitCard()
        {
            return Motion.InitCard();
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

            return Motion.GetAxisStatus(axisRun.核号, axisRun.轴号, out sts);
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
            return Motion.IsEnable(axisRun.核号, axisRun.轴号, out enable);
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

            return Motion.GetCmdEncPos(axisRun.核号, axisRun.轴号, out pos);
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

            return Motion.GetCmdEncVel(axisRun.核号, axisRun.轴号, out pos);
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

            return Motion.GetCmdPrfPos(axisRun.核号, axisRun.轴号, out pos);
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

            return Motion.GetCmdPrfVel(axisRun.核号, axisRun.轴号, out pos);
        }

        #endregion

        #region 轴回零

        /// <summary>
        /// 等待多轴回零
        /// </summary>
        /// <param name="axisHomes"></param>
        /// <returns></returns>
        private string WaitAxisHome(List<AxisHomeInfo> axisHomes, int type = 0)
        {
            bool[] bools = new bool[axisHomes.Count];
            for (int i = 0; i < axisHomes.Count; i++)
            {
                string result = WaitAxisHome(axisHomes[i], type);
                if (result != "Success")
                {
                    RunLog(result);
                    bools[i] = false;
                }
                else bools[i] = true;
            }
            if (bools.Contains(false))
            {
                return "Fail";
            }
            else return "Success";
        }

        /// <summary>
        /// 等待单轴回零
        /// </summary>
        /// <param name="axisHome"></param>
        /// <returns></returns>
        private string WaitAxisHome(AxisHomeInfo axisHome, int type = 0)
        {
            string result = Motion.CommonInitedValidate();

            if (result != "Success") return result;

            try
            {
                THomeStatus tHomeSts;
                do
                {
                    short error = GTN_GetHomeStatus(axisHome.核号, axisHome.轴号, out tHomeSts);
                    Motion.ErrorHandler($"GTN_GetHomeStatus({axisHome.核号}, {axisHome.轴号}, out {tHomeSts})", error);

                    error = GTN_GetSts(axisHome.核号, axisHome.轴号, out int sts, 1, out uint pClock);
                    Motion.ErrorHandler($"GTN_GetSts({axisHome.核号}, {axisHome.轴号}, out {sts}, 1, out uint {pClock})", error);

                    if (APPmaster.IsPause && type == 0)
                    {
                        Motion.AxisStop(axisHome.核号, axisHome.轴号, 1);
                        return $"{axisHome.轴名称}回零被停止";
                    }

                    if ((sts & (1 << 1)) == 1)
                    {
                        Motion.AxisStop(axisHome.核号, axisHome.轴号, 1);
                        return $"{axisHome.轴名称}回零被停止,轴报警";
                    }

                    Thread.Sleep(1); //减少轴卡和CPU压力
                }
                while (tHomeSts.run == 1);

                if (tHomeSts.error != HOME_ERROR_NONE) //未发生错误
                {
                    if (tHomeSts.error == HOME_ERROR_NOT_TRAP_MODE) return $"{axisHome.轴名称}回零失败，执行Smart Home回原点的轴不是处于点位运动模式";
                    else if (tHomeSts.error == HOME_ERROR_DISABLE) return $"{axisHome.轴名称}回零失败，执行Smart Home回原点的轴未使能";
                    else if (tHomeSts.error == HOME_ERROR_ALARM) return $"{axisHome.轴名称}回零失败，执行Smart Home回原点的轴驱动报警";
                    else if (tHomeSts.error == HOME_ERROR_STOP) return $"{axisHome.轴名称}回零失败，未完成回原点，轴停止运动";
                    else if (tHomeSts.error == HOME_ERROR_STAGE) return $"{axisHome.轴名称}回零失败，回原点阶段错误";
                    else if (tHomeSts.error == HOME_ERROR_HOME_MODE) return $"{axisHome.轴名称}回零失败，模式错误，如已经启动Smart Home，勿重复调用回原点指令";
                    else if (tHomeSts.error == HOME_ERROR_SET_CAPTURE_HOME) return $"{axisHome.轴名称}回零失败，设置Home捕获模式失败";
                    else if (tHomeSts.error == HOME_ERROR_NO_HOME) return $"{axisHome.轴名称}回零失败，未找到Home";
                    else if (tHomeSts.error == HOME_ERROR_SET_CAPTURE_INDEX) return $"{axisHome.轴名称}回零失败，设置Index捕获模式失败";
                    else if (tHomeSts.error == HOME_ERROR_NO_INDEX) return $"{axisHome.轴名称}回零失败，未找到Index";
                    else return $"{axisHome.轴名称}回零异常";
                }
                else
                {
                    if (axisHome.轴名称 == "M22_入料手臂X轴" || axisHome.轴名称 == "M23_出料手臂X轴")
                    {
                        result = Motion.GNM601_GetDi(axisHome.核号, GTN.mc.MC_HOME, out string value);
                        if (result == "Success")
                        {
                            string iovalue = value[axisHome.轴号 - 1].ToString();
                            if (iovalue != "0")
                            {
                                return $"{axisHome.轴名称}回零失败，回原后未在原点";
                            }
                        }
                        else return result;
                    }
                }

                result = Motion.AxisZeroPos(axisHome.核号, axisHome.轴号);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 多轴回零
        /// </summary>
        /// <param name="axisNames"></param>
        /// <returns></returns>
        public string AxisHome(string[] axisNames)
        {
            List<AxisHomeInfo> axisHomes = new List<AxisHomeInfo>();
            string result = GetAxisHomeInfo(axisNames, out axisHomes);
            if (result == "Success")
            {
                result = Motion.MultiaxisHome(axisHomes);
                if (result == "Success")
                {
                    Task<string> task = Task.Run(() =>
                    {
                        return WaitAxisHome(axisHomes);
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
                result = Motion.UniaxialHome(axisHome);
                if (result == "Success")
                {
                    Task<string> task = Task.Run(() =>
                    {
                        return WaitAxisHome(axisHome, type);//等待回零完成
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

        public string GetDiHome(string[] axisNames, out string[] iovalues)
        {
            iovalues = new string[axisNames.Length];
            string result = GetAxisHomeInfo(axisNames, out List<AxisHomeInfo> axisHomes);
            if (result == "Success")
            {
                for (int i = 0; i < axisHomes.Count; i++)
                {
                    result = Motion.GNM601_GetDi(axisHomes[i].核号, GTN.mc.MC_HOME, out string value);
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
        /// 等待单轴移动
        /// </summary>
        /// <param name="axisRun"></param>
        /// <returns></returns>
        private string WaitAxisMove(AxisRun axisRun, int prfPos, int type = 0, int move = 0, int pos2 = 0)
        {
            string result = Motion.CommonInitedValidate();

            if (result != "Success") return result;

            try
            {
                int sts;
                bool isException = false;
                string errortxt = "";
                bool issecond = false;
                do
                {
                    short error = GTN_GetSts(axisRun.核号, axisRun.轴号, out sts, 1, out uint pClock);
                    Motion.ErrorHandler($"GTN_GetSts({axisRun.核号}, {axisRun.轴号}, out {sts}, 1, out uint pClock)", error);

                    if (move == 1 && !issecond)
                    {
                        GetCmdPrfPos(axisRun.轴名称, out int prfPos1); //规划位置
                        GetCmdEncPos(axisRun.轴名称, out double encPos1); //实时位置
                        double posvalue1 = Math.Abs(prfPos1 - encPos1);
                        if (posvalue1 < pos2)
                        {
                            issecond = true;
                            axisRun.目标速度 = axisRun.目标速度 * 第二段速度;

                            // 设置AXIS轴的目标速度
                            error = GTN_SetVel(axisRun.核号, axisRun.轴号, axisRun.目标速度);
                            Motion.ErrorHandler("GTN_SetVel", error);

                            // 启动AXIS轴的运动
                            error = GTN_Update(axisRun.核号, 1 << (axisRun.轴号 - 1));
                            Motion.ErrorHandler("GTN_Update", error);
                        }
                    }

                    if (APPmaster.IsPause && type == 0)
                    {
                        GTN_Stop(axisRun.核号, 1 << (axisRun.轴号 - 1), 1);
                        while (true)
                        {
                            if (APPmaster.IsForecePause) return "强制停止";
                            if (APPmaster.IsPause) Thread.Sleep(10);
                            else
                            {
                                result = Motion.UniaxialAbsoluteMoveAbs(axisRun);
                                break;
                            }
                        }
                    }
                    else if (APPmaster.IsEmptyPause && axisRun.轴名称 == "M12_Empty顶盘轴")
                    {
                        GTN_Stop(axisRun.核号, 1 << (axisRun.轴号 - 1), 1);
                        while (true)
                        {
                            if (APPmaster.IsForecePause) return "强制停止";
                            if (APPmaster.IsEmptyPause) Thread.Sleep(10);
                            else
                            {
                                result = Motion.UniaxialAbsoluteMoveAbs(axisRun);
                                break;
                            }
                        }
                    }
                    else if (APPmaster.IsLoadPause && axisRun.轴名称 == "M13_Loader顶盘轴")
                    {
                        GTN_Stop(axisRun.核号, 1 << (axisRun.轴号 - 1), 1);
                        while (true)
                        {
                            if (APPmaster.IsForecePause) return "强制停止";
                            if (APPmaster.IsLoadPause) Thread.Sleep(10);
                            else
                            {
                                result = Motion.UniaxialAbsoluteMoveAbs(axisRun);
                                break;
                            }
                        }
                    }

                    if ((sts & (1 << 1)) == 1)
                    {
                        GTN_Stop(axisRun.核号, 1 << (axisRun.轴号 - 1), 1);
                        errortxt = $"{axisRun.轴名称}运动被停止,轴报警";
                        isException = true;
                    }

                    //if ((sts & (1 << 5)) == 1)
                    //{
                    //    GTN_Stop(axisRun.核号, 1 << (axisRun.轴号 - 1), 1);
                    //    return $"{axisRun.轴名称}运动被停止,正限位报警";
                    //}

                    //if ((sts & (1 << 6)) == 1)
                    //{
                    //    GTN_Stop(axisRun.核号, 1 << (axisRun.轴号 - 1), 1);
                    //    return $"{axisRun.轴名称}运动被停止,负限位报警";
                    //}

                    Thread.Sleep(1); //减少轴卡和CPU压力
                }
                while ((sts & 0x400) != 0);

                if (isException) return errortxt;

                #region 等待位置

                if (ServoAxis.Contains(axisRun.轴名称)) //只有伺服轴才需要确认编码器位置和规划位置，步进电机没有编码器
                {
                    Stopwatch stopwatch = new Stopwatch();
                    bool isvalue = true;

                    int errorvalue = 跟随误差;
                    if (axisRun.轴名称 == "M19_测试手臂Z轴")
                    {
                        errorvalue = 测试Z跟随误差;
                    }

                    double encPos = 0;
                    stopwatch.Start();
                    while (isvalue)
                    {
                        Motion.GetCmdEncPos(axisRun.核号, axisRun.轴号, out encPos);
                        double posvalue = Math.Abs(prfPos - encPos);

                        if (stopwatch.ElapsedMilliseconds > 5000)
                        {
                            RunLog($"{axisRun.轴名称}移动超时5s");
                            return $"{axisRun.轴名称}规划位置({prfPos})和实际位置({encPos})的跟随误差={posvalue}>{errorvalue}";
                        }
                        else
                        {
                            if (posvalue < errorvalue)
                            {
                                isvalue = false;
                            }
                        }
                        Thread.Sleep(10);
                    }

                    stopwatch.Stop();
                    RunLog($"{axisRun.轴名称}移动完成,等待时间{stopwatch.ElapsedMilliseconds}ms,当前实际位置{encPos}");
                    stopwatch.Reset();

                    #endregion
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "Success";
        }

        /// <summary>
        /// 等待多轴移动
        /// </summary>
        /// <param name="axisRuns"></param>
        /// <returns></returns>
        private string WaitAxisMove(List<AxisRun> axisRuns, List<int> prfPos, int type = 0, int move = 0, int pos2 = 0)
        {
            string result = Motion.CommonInitedValidate();

            if (result != "Success") return result;

            try
            {
                bool value = true;
                bool isException = false;
                string errortxt = "";

                int[] sts = new int[axisRuns.Count];
                bool[] issecond = new bool[axisRuns.Count];
                do
                {
                    value = false;
                    isException = false;
                    errortxt = "";

                    for (int i = 0; i < axisRuns.Count; i++)
                    {
                        short error = GTN_GetSts(axisRuns[i].核号, axisRuns[i].轴号, out sts[i], 1, out uint pClock);
                        Motion.ErrorHandler($"GTN_GetSts({axisRuns[i].核号}, {axisRuns[i].轴号}, out {sts[i]}, 1, out uint pClock)", error);
                    }

                    if (move == 1 && issecond.Contains(false))
                    {
                        for (int i = 0; i < axisRuns.Count; i++)
                        {
                            if (!issecond[i]) //只要有一个Z到达减速区间则全部减速，这样才会一致
                            {
                                GetCmdPrfPos(axisRuns[i].轴名称, out int prfPos1); //规划位置
                                GetCmdEncPos(axisRuns[i].轴名称, out double encPos1); //实时位置
                                double posvalue1 = Math.Abs(prfPos1 - encPos1);

                                if (posvalue1 < pos2)
                                {
                                    if (axisRuns[i].轴名称.Contains("入料手臂Z") || axisRuns[i].轴名称.Contains("出料手臂Z"))
                                    {
                                        for (int se = 0; se < axisRuns.Count; se++)
                                        {
                                            RunLog($"{axisRuns[se].轴名称}修改前目标速度:" + axisRuns[se].目标速度);

                                            issecond[se] = true;
                                            axisRuns[se].目标速度 = axisRuns[se].目标速度 * 出入料Z第二段速度;

                                            RunLog($"{axisRuns[se].轴名称}修改后目标速度:" + axisRuns[se].目标速度);

                                            // 设置AXIS轴的目标速度
                                            short error = GTN_SetVel(axisRuns[se].核号, axisRuns[se].轴号, axisRuns[se].目标速度);
                                            Motion.ErrorHandler("GTN_SetVel", error);
                                        }

                                        for (int se = 0; se < issecond.Length; se++)
                                        {
                                            // 启动AXIS轴的运动
                                            short error1 = GTN_Update(axisRuns[se].核号, 1 << (axisRuns[se].轴号 - 1));
                                            Motion.ErrorHandler("GTN_Update", error1);
                                        }

                                        break;
                                    }
                                    else
                                    {
                                        RunLog($"{axisRuns[i].轴名称}修改前目标速度:" + axisRuns[i].目标速度);

                                        issecond[i] = true;
                                        axisRuns[i].目标速度 = axisRuns[i].目标速度 * 第二段速度;

                                        RunLog($"{axisRuns[i].轴名称}修改后目标速度:" + axisRuns[i].目标速度);

                                        // 设置AXIS轴的目标速度
                                        short error = GTN_SetVel(axisRuns[i].核号, axisRuns[i].轴号, axisRuns[i].目标速度);
                                        Motion.ErrorHandler("GTN_SetVel", error);

                                        // 启动AXIS轴的运动
                                        short error1 = GTN_Update(axisRuns[i].核号, 1 << (axisRuns[i].轴号 - 1));
                                        Motion.ErrorHandler("GTN_Update", error1);
                                    }
                                }
                            }
                        }
                    }

                    if (APPmaster.IsPause && type == 0)
                    {
                        for (int i = 0; i < axisRuns.Count; i++)
                        {
                            GTN_Stop(axisRuns[i].核号, 1 << (axisRuns[i].轴号 - 1), 1);
                        }
                        while (true)
                        {
                            if (APPmaster.IsForecePause) return "强制停止";
                            if (APPmaster.IsPause) Thread.Sleep(10);
                            else
                            {
                                result = Motion.MultiaxisAbsoluteMoveAbs(axisRuns);
                                break;
                            }
                        }
                    }

                    for (int i = 0; i < sts.Length; i++)
                    {
                        if (((sts[i] & (1 << 1)) == 1))
                        {
                            GTN_Stop(axisRuns[i].核号, 1 << (axisRuns[i].轴号 - 1), 1);
                            errortxt += $"{axisRuns[i].轴名称}运动被停止,轴报警";
                            isException = true;
                        }

                        if ((sts[i] & 0x400) != 0)
                        {
                            value = true;
                            break;
                        }
                    }

                    Thread.Sleep(1); //减少轴卡和CPU压力
                }
                while (value);

                if (isException) return errortxt;

                for (int i = 0; i < axisRuns.Count; i++)
                {
                    #region 等待位置

                    if (ServoAxis.Contains(axisRuns[i].轴名称)) //只有伺服轴才需要确认编码器位置和规划位置，步进电机没有编码器
                    {
                        Stopwatch stopwatch = new Stopwatch();
                        bool isvalue = true;

                        int errorvalue = 跟随误差;
                        if (axisRuns[i].轴名称 == "M19_测试手臂Z轴")
                        {
                            errorvalue = 测试Z跟随误差;
                        }
                        double encPos = 0;
                        stopwatch.Start();
                        while (isvalue)
                        {
                            Motion.GetCmdEncPos(axisRuns[i].核号, axisRuns[i].轴号, out encPos);
                            double posvalue = Math.Abs(prfPos[i] - encPos);

                            if (stopwatch.ElapsedMilliseconds > 5000)
                            {
                                RunLog($"{axisRuns[i].轴名称}移动超时5s");
                                return $"{axisRuns[i].轴名称}规划位置({prfPos[i]})和实际位置({encPos})的跟随误差={posvalue}>{errorvalue}";
                            }
                            else
                            {
                                if (posvalue < errorvalue)
                                {
                                    isvalue = false;
                                }
                            }
                            Thread.Sleep(10);
                        }

                        stopwatch.Stop();
                        RunLog($"{axisRuns[i].轴名称}移动完成,等待时间{stopwatch.ElapsedMilliseconds}ms,当前实际位置{encPos}");
                        stopwatch.Reset();
                    }

                    #endregion
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "Success";
        }

        /// <summary>
        /// 单轴绝对移动
        /// </summary>
        /// <param name="axisName"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public string AxisAbsoluteMove(string axisName, double pos, int type = 0, int move = 0, int pos2 = 0)
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

            result = Motion.UniaxialAbsoluteMoveAbs(axisRun);
            if (result != "Success") return result;

            Task<string> task = Task.Run(() =>
            {
                return WaitAxisMove(axisRun, axisRun.目标位置, type, move, pos2);
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

            result = Motion.UniaxialRelativeMove(axisRun, out int posPulse);
            if (result != "Success") return result;

            Task<string> task = Task.Run(() =>
            {
                return WaitAxisMove(axisRun, posPulse, 1);
            });

            return task.Result;
        }

        /// <summary>
        /// 多轴移动
        /// </summary>
        /// <param name="axisNames"></param>
        /// <param name="poss"></param>
        /// <returns></returns>
        public string AxisMove(string[] axisNames, double[] poss, int type = 0, int move = 0, int pos2 = 0)//
        {
            List<int> prfPos = new List<int>();
            List<AxisRun> axisRuns = new List<AxisRun>();
            for (int i = 0; i < axisNames.Length; i++)
            {
                string result = GetAxisRunInfo(axisNames[i], out AxisRunInfo axisRunInfo);
                if (result != "Success") return result;

                int movepos = Convert.ToInt32(Math.Round(poss[i] / axisRunInfo.步长mm));
                axisRuns.Add(new AxisRun()
                {
                    轴名称 = axisRunInfo.轴名称,
                    核号 = axisRunInfo.核号,
                    轴号 = axisRunInfo.轴号,
                    目标位置 = movepos,
                    目标速度 = axisRunInfo.运行速度,
                    目标加速度 = axisRunInfo.加速度,
                    目标减速度 = axisRunInfo.减速度,
                    平滑时间 = axisRunInfo.平滑时间,
                });
                prfPos.Add(movepos);
            }

            string result2 = Motion.MultiaxisAbsoluteMoveAbs(axisRuns);
            if (result2 != "Success") return result2;

            Task<string> task = Task.Run(() =>
            {
                return WaitAxisMove(axisRuns, prfPos, type, move, pos2);//等待回零完成
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

            return Motion.JogMove(axisRun);
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

            return Motion.AxisStop(axisRunInfo.核号, axisRunInfo.轴号, isSmoothStop);
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
                return Motion.GLink_GetExtDi(核号, 索引, out value);
            }
            else if (模块类型 == "GNM601")
            {
                return Motion.GNM601_GetDi(核号, 类型, out value);
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
                string result = Motion.GLink_GetExtDi(iOInfo.核号, iOInfo.软件索引, out string value);
                if (result == "Success")
                {
                    char[] chars = value.ToCharArray();
                    iovalue = chars[0].ToString();
                }
                else return result;
            }
            else if (iOInfo.模块类型 == "GNM601")
            {
                string result = Motion.GNM601_GetDiBit(iOInfo.核号, MC_GPI, iOInfo.软件索引, out short value);
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
                return Motion.GLink_GetExtDO(核号, 索引, out value);
            }
            else if (模块类型 == "GNM601")
            {
                return Motion.GNM601_GetDo(核号, 类型, out value);
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
                string result = Motion.GLink_GetExtDO(iOInfo.核号, iOInfo.软件索引, out string value);
                if (result == "Success")
                {
                    char[] chars = value.ToCharArray();
                    iovalue = chars[0].ToString();
                }
                else return result;
            }
            else if (iOInfo.模块类型 == "GNM601")
            {
                string result = Motion.GNM601_GetDo(iOInfo.核号, GTN.mc.MC_GPO, out string value);
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
                return Motion.GLink_SetExtDoBit(iOInfo.核号, iOInfo.软件索引, value);
            }
            else if (iOInfo.模块类型 == "GNM601")
            {
                return Motion.GNM601_SetDoBit(iOInfo.核号, MC_GPO, iOInfo.软件索引, value);
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
