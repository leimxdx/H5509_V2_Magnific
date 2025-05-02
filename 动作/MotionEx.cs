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
using static System.Windows.Forms.LinkLabel;
using static GTN.mc;
using static H5509_V2.APPmaster;

namespace H5509_V2
{
    public class MotionEx
    {
        //创建一个Motion对象
        public Motion Motion = new Motion();

        //创建一个静态对象保存当前的实例
        private static volatile MotionEx instance;

        //定义一个锁
        private static readonly object objlock = new object();

        //创建一个私有的构造方法
        private MotionEx() { }

        public static MotionEx GetInstance()
        {
            if (instance == null)
            {
                lock (objlock)
                {
                    if (instance == null)
                    {
                        instance = new MotionEx();
                    }
                }
            }
            return instance;
        }

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
        private string WaitAxisHome(List<AxisHomeInfo> axisHomes)
        {
            bool[] bools = new bool[axisHomes.Count];
            for (int i = 0; i < axisHomes.Count; i++)
            {
                string result = WaitAxisHome(axisHomes[i]);
                if (result != "Success")
                {
                    Debug.WriteLine($"{axisHomes[i].轴名称}回零失败!" + result);
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
        private string WaitAxisHome(AxisHomeInfo axisHome)
        {
            string result = Motion.CommonInitedValidate();

            if (result != "Success") return result;

            try
            {
                THomeStatus tHomeSts;
                do
                {
                    short error = GTN_GetHomeStatus(axisHome.核号, axisHome.轴号, out tHomeSts);
                    Motion.ErrorHandler($"GTN_GetHomeStatus({axisHome.核号}, {axisHome.轴号}, out tHomeSts)", error);
                    if (APPmaster.IsPause)
                    {
                        Motion.AxisStop(axisHome.核号, axisHome.轴号, 1);
                        return "回零被停止";
                    }
                    Thread.Sleep(1); //减少轴卡和CPU压力
                }
                while (tHomeSts.run == 1);

                if (tHomeSts.error != HOME_ERROR_NONE) //未发生错误
                {
                    if (tHomeSts.error == HOME_ERROR_NOT_TRAP_MODE) return "执行Smart Home回原点的轴不是处于点位运动模式";
                    else if (tHomeSts.error == HOME_ERROR_DISABLE) return "执行Smart Home回原点的轴未使能";
                    else if (tHomeSts.error == HOME_ERROR_ALARM) return "执行Smart Home回原点的轴驱动报警";
                    else if (tHomeSts.error == HOME_ERROR_STOP) return "未完成回原点，轴停止运动";
                    else if (tHomeSts.error == HOME_ERROR_STAGE) return "回原点阶段错误";
                    else if (tHomeSts.error == HOME_ERROR_HOME_MODE) return "模式错误，如已经启动Smart Home，勿重复调用回原点指令";
                    else if (tHomeSts.error == HOME_ERROR_SET_CAPTURE_HOME) return "设置Home捕获模式失败";
                    else if (tHomeSts.error == HOME_ERROR_NO_HOME) return "未找到Home";
                    else if (tHomeSts.error == HOME_ERROR_SET_CAPTURE_INDEX) return "设置Index捕获模式失败";
                    else if (tHomeSts.error == HOME_ERROR_NO_INDEX) return "未找到Index";
                    else return "回零异常";
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
                                return "未在原点";
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
        public string AxisHome(string axisName)
        {
            string result = APPmaster.GetAxisHomeInfo(axisName, out AxisHomeInfo axisHome);
            if (result == "Success")
            {
                result = Motion.UniaxialHome(axisHome);
                if (result == "Success")
                {
                    Task<string> task = Task.Run(() =>
                    {
                        return WaitAxisHome(axisHome);//等待回零完成
                    });

                    result = task.Result;
                    if (result != "Success")
                    {
                        Debug.WriteLine($"{axisName}回零失败!" + result);
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
        private string WaitAxisMove(AxisRun axisRun, int type = 0)
        {
            string result = Motion.CommonInitedValidate();

            if (result != "Success") return result;

            try
            {
                int sts;
                do
                {
                    short error = GTN_GetSts(axisRun.核号, axisRun.轴号, out sts, 1, out uint pClock);
                    Motion.ErrorHandler($"GTN_GetSts({axisRun.核号}, {axisRun.轴号}, out {sts}, 1, out uint pClock)", error);
                    if (APPmaster.IsPause && type == 0)
                    {
                        GTN_Stop(axisRun.核号, 1 << (axisRun.轴号 - 1), 1);
                        while (true)
                        {
                            if (APPmaster.IsForecePause) break;
                            if (APPmaster.IsPause) Thread.Sleep(10);
                            else
                            {
                                result = Motion.UniaxialAbsoluteMoveAbs(axisRun);
                                break;
                            }
                        }
                    }
                    Thread.Sleep(1); //减少轴卡和CPU压力
                }
                while ((sts & 0x400) != 0);

                Motion.GetCmdEncPos(axisRun.核号, axisRun.轴号, out double encPos);
                Motion.GetCmdPrfPos(axisRun.核号, axisRun.轴号, out int prfPos);
                double posvalue = Math.Abs(prfPos - encPos);
                //Debug.WriteLine($"{axisRun.轴名称}规划位置({prfPos})和实际位置({encPos})的跟随误差={posvalue}");
                if (posvalue > 跟随误差)
                {
                    return $"{axisRun.轴名称}规划位置({prfPos})和实际位置({encPos})的跟随误差={posvalue}>{跟随误差}";
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
        private string WaitAxisMove(List<AxisRun> axisRuns)
        {
            string result = Motion.CommonInitedValidate();

            if (result != "Success") return result;

            try
            {
                bool value = true;
                int[] sts = new int[axisRuns.Count];
                do
                {
                    for (int i = 0; i < axisRuns.Count; i++)
                    {
                        short error = GTN_GetSts(axisRuns[i].核号, axisRuns[i].轴号, out sts[i], 1, out uint pClock);
                        Motion.ErrorHandler($"GTN_GetSts({axisRuns[i].核号}, {axisRuns[i].轴号}, out {sts[i]}, 1, out uint pClock)", error);
                    }

                    if (APPmaster.IsPause)
                    {
                        for (int i = 0; i < axisRuns.Count; i++)
                        {
                            GTN_Stop(axisRuns[i].核号, 1 << (axisRuns[i].轴号 - 1), 1);
                        }
                        while (true)
                        {
                            if (APPmaster.IsForecePause) break;
                            if (APPmaster.IsPause) Thread.Sleep(10);
                            else
                            {
                                result = Motion.MultiaxisAbsoluteMoveAbs(axisRuns);
                                break;
                            }
                        }
                    }

                    value = false;

                    for (int i = 0; i < sts.Length; i++)
                    {
                        if ((sts[i] & 0x400) != 0)
                        {
                            value = true;
                            break;
                        }
                    }

                    Thread.Sleep(1); //减少轴卡和CPU压力
                }
                while (value);

                for (int i = 0; i < axisRuns.Count; i++)
                {
                    Motion.GetCmdEncPos(axisRuns[i].核号, axisRuns[i].轴号, out double encPos);
                    Motion.GetCmdPrfPos(axisRuns[i].核号, axisRuns[i].轴号, out int prfPos);
                    double posvalue = Math.Abs(prfPos - encPos);
                    //Debug.WriteLine($"{axisRuns[i].轴名称}规划位置({prfPos})和实际位置({encPos})的跟随误差={posvalue}");
                    if (posvalue > 跟随误差)
                    {
                        return $"{axisRuns[i].轴名称}规划位置({prfPos})和实际位置({encPos})的跟随误差={posvalue}>{跟随误差}";
                    }
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
        public string AxisAbsoluteMove(string axisName, double pos)
        {
            string result = GetAxisRunInfo(axisName, out AxisRunInfo axisRunInfo);
            if (result != "Success") return result;

            AxisRun axisRun = new AxisRun()
            {
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
                return WaitAxisMove(axisRun);
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
                核号 = axisRunInfo.核号,
                轴号 = axisRunInfo.轴号,
                目标位置 = Convert.ToInt32(Math.Round(pos / axisRunInfo.步长mm)),
                目标速度 = axisRunInfo.运行速度,
                目标加速度 = axisRunInfo.加速度,
                目标减速度 = axisRunInfo.减速度,
                平滑时间 = axisRunInfo.平滑时间,
            };

            result = Motion.UniaxialRelativeMove(axisRun);
            if (result != "Success") return result;

            Task<string> task = Task.Run(() =>
            {
                return WaitAxisMove(axisRun, 1);//等待回零完成
            });

            return task.Result;
        }

        /// <summary>
        /// 多轴移动
        /// </summary>
        /// <param name="axisNames"></param>
        /// <param name="poss"></param>
        /// <returns></returns>
        public string AxisMove(string[] axisNames, double[] poss)
        {
            List<AxisRun> axisRuns = new List<AxisRun>();
            for (int i = 0; i < axisNames.Length; i++)
            {
                string result = GetAxisRunInfo(axisNames[i], out AxisRunInfo axisRunInfo);
                if (result != "Success") return result;

                axisRuns.Add(new AxisRun()
                {
                    核号 = axisRunInfo.核号,
                    轴号 = axisRunInfo.轴号,
                    目标位置 = Convert.ToInt32(Math.Round(poss[i] / axisRunInfo.步长mm)),
                    目标速度 = axisRunInfo.运行速度,
                    目标加速度 = axisRunInfo.加速度,
                    目标减速度 = axisRunInfo.减速度,
                    平滑时间 = axisRunInfo.平滑时间,
                });
            }

            string result2 = Motion.MultiaxisAbsoluteMoveAbs(axisRuns);
            if (result2 != "Success") return result2;

            Task<string> task = Task.Run(() =>
            {
                return WaitAxisMove(axisRuns);//等待回零完成
            });

            return task.Result;

        }

        /// <summary>
        /// Jog运动
        /// </summary>
        /// <param name="axisName"></param>
        /// <param name="jogspeed"></param>
        /// <returns></returns>
        public string AxisJogMove(string axisName, double jogspeed)
        {
            string result = GetAxisRunInfo(axisName, out AxisRunInfo axisRunInfo);
            if (result != "Success") return result;

            AxisRun axisRun = new AxisRun()
            {
                核号 = axisRunInfo.核号,
                轴号 = axisRunInfo.轴号,
                目标速度 = jogspeed,
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
        private string GetIOInfo(string ioName, out IOInfo ioInfo)
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
