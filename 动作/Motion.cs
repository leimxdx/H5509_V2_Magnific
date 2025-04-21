using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using static GTN.mc;

namespace H5509_V2
{
    public class Motion
    {
        public void ErrorHandler(string command, short error)
        {
            string result = string.Empty;

            switch (error)
            {
                case 0: break;
                case 1: result = $"执行指令:{command}\n指令执行错误({error})"; break;
                case 2: result = $"执行指令:{command}\nlicense不支持({error})"; break;
                case 7: result = $"执行指令:{command}\n指令参数错误({error})"; break;
                case -1: result = $"执行指令: {command}\n主机和运动控制器通讯失败({error})"; break;
                case -6: result = $"执行指令:  {command}\n打开控制器失败({error})"; break;
                case -7: result = $"执行指令: {command}\n运动控制器没有响应({error})"; break;
                case -10: result = $"执行指令:{command}\n主机和运动控制器通讯失败({error})"; break;
                case -13: result = $"执行指令: {command}\n编码器初始化失败({error})"; break;
                case -14: result = $"执行指令:{command}\n编码器初始化失败({error})"; break;
                case -15: result = $"执行指令:{command}\n动态库版本不匹配({error})"; break;
                case 15: result = $"执行指令:{command}\n动态库版本不匹配({error})"; break;
                case 16: result = $"执行指令:{command}\n不具备版本匹配功能({error})"; break;
                default: result = $"执行指令:{command}\n未知错误({error})"; break;
            }

            if (result.Length > 0)
            {
                throw new Exception(result);
            }
        }

        /// <summary>
        /// 板卡初始化
        /// </summary>
        /// <returns>操作结果</returns>
        public string InitCard()
        {
            try
            {
                //打开运动控制器
                short error = GTN_Open(5, 1);
                //writeDataHelper.RunLog($"GTN_Open(5,1):({error})");
                ErrorHandler("GT_Open", error);

                //复位运动控制器(核1)
                error = GTN_Reset(1);
                //writeDataHelper.RunLog($"GTN_Reset(1):({error})");
                ErrorHandler("GT_Reset", error);

                //复位运动控制器(核2)
                error = GTN_Reset(2);
                //writeDataHelper.RunLog($"GTN_Reset(1):({error})");
                ErrorHandler("GT_Reset", error);

                // 注意：配置文件取消了各轴的报警和限位
                // 配置运动控制器(核1)
                error = GTN_LoadConfig(1, "gtn_core1.cfg");
                //writeDataHelper.RunLog($"GTN_StartEcatComm(1):({error})");
                ErrorHandler("GTN_LoadConfig(1, gtn_core1.cfg)", error);

                // 配置运动控制器(核2)
                error = GTN_LoadConfig(2, "gtn_core2.cfg");
                //writeDataHelper.RunLog($"GTN_StartEcatComm(1):({error})");
                ErrorHandler("GTN_LoadConfig(2, gtn_core2.cfg)", error);

                //清除轴标志
                error = GTN_ClrSts(1, 1, 24);
                //writeDataHelper.RunLog($"GTN_ClrSts(1, 1, {advancedParam.AxisCount}):({error})");
                ErrorHandler("GTN_ClrSts", error);

                error = GTN_ExtModuleInit(1);        //初始化gLink模块
                //writeDataHelper.RunLog($"GT_GLinkInit(0):({error})");
                ErrorHandler("GT_GLinkInit", error);

                if (error == 0)
                {
                    error = GTN_GetExtModuleCount(1, 0, out short gLinkCout);
                    ErrorHandler("GTN_GetExtModuleCount", error);
                    Console.WriteLine(gLinkCout + ("个IO模块初始化成功！"));
                    //writeDataHelper.RunLog(slavenum + ("个IO模块初始化成功！"));
                }

                //for (int i = 0; i < 17; i++)
                //{
                //    error = GTN_SetExtDoBit(1, (short)(i * 16 + 8), 1);
                //    error = GTN_SetExtDoBit(1, (short)(i * 16 + 9), 1);
                //}
                //GTN_SetExtDoBit(1, (short)(2 * 16 + 10), 1);
                //GTN_SetExtDoBit(1, (short)(3 * 16 + 10), 1);
                //GTN_SetExtDoBit(1, (short)(4 * 16 + 10), 1);
                //GTN_SetExtDoBit(1, (short)(5 * 16 + 10), 1);
                //GTN_SetExtDoBit(1, (short)(6 * 16 + 10), 1);
                //GTN_SetExtDoBit(1, (short)(7 * 16 + 10), 1);

                //GTN_SetDoBit(1, GTN.mc.MC_GPO, 1, 0); //0
                //GTN_SetDoBit(1, GTN.mc.MC_GPO, 2, 0); //1

                //GTN_SetDoBit(1, GTN.mc.MC_GPO, 8, 0); //7
                //GTN_SetDoBit(1, GTN.mc.MC_GPO, 9, 0); //8
                //GTN_SetDoBit(1, GTN.mc.MC_GPO, 10, 0); //9

                //GTN_SetDoBit(1, GTN.mc.MC_GPO, 11, 0); //0
                //GTN_SetDoBit(1, GTN.mc.MC_GPO, 12, 0); //1

                //GTN_SetDoBit(1, GTN.mc.MC_GPO, 17, 0); //6

                //GTN_SetDoBit(2, GTN.mc.MC_GPO, 1, 0); //0
                //GTN_SetDoBit(2, GTN.mc.MC_GPO, 2, 0); //1
                //GTN_SetDoBit(2, GTN.mc.MC_GPO, 3, 0); //2

                //GTN_SetDoBit(2, GTN.mc.MC_GPO, 13, 0); //2
                //GTN_SetDoBit(2, GTN.mc.MC_GPO, 14, 0); //3
                //GTN_SetDoBit(2, GTN.mc.MC_GPO, 15, 0); //4

                //bool[] bools;
                //string binary32 = "";
                //List<bool[]> listbool = new List<bool[]>();
                //for (int i = 0; i < 8; i++)
                //{
                //    error = GTN_GetExtDo(1, (short)(i * 32 + 1), out int pValue);
                //    binary32 = Convert.ToString(pValue, 2).PadLeft(32, '0');
                //    bools = binary32.Select(c => c == '1').ToArray();
                //    listbool.Add(bools);
                //}

                APPmaster.InitedOK = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[error][" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]：" + "InitCard() " + ex.ToString());
                return ex.Message;
            }

            return "Success";
        }

        /// <summary>
        /// 关闭板卡
        /// </summary>
        /// <returns></returns>
        public string CloseCard()
        {
            //判断是否初始化成功
            string result = CommonInitedValidate();

            if (result != "Success") return result;

            //创建指令返回值
            short error;

            //关闭所有轴
            AxisAllClose();

            //关闭卡
            try
            {
                error = GTN_Close(); //关闭轴卡
                ErrorHandler("GTN_Close", error);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "Success";
        }


        /// <summary>
        /// 通用初始化验证
        /// </summary>
        /// <returns>操作结果</returns>
        public string CommonInitedValidate()
        {
            if (APPmaster.InitedOK == false) return "未初始化";
            else return "Success";
        }

        /// <summary>
        /// 通用运动初始化验证
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        private string CommonMotionValidate(short core, short axis)
        {
            string result = CommonInitedValidate();
            if (result != "Success") return result;
            if (IsMoving(core, axis)) return "正在运动";
            else return "Success";
        }

        #region 轴信息

        /// <summary>
        /// 判断某个轴是否运行
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        public bool IsMoving(short core, short axis)
        {
            try
            {
                short error = 0;

                error = GTN_GetSts(core, axis, out int sts, 1, out uint pClock);

                ErrorHandler("GTN_GetSts", error);

                return (sts & 0x400) != 0;
            }
            catch (Exception)
            {
                return true;
            }
        }

        /// <summary>
        /// 关闭所有轴使能
        /// </summary>
        /// <returns></returns>
        public string AxisAllClose()
        {
            //判断是否初始化成功
            string result = CommonInitedValidate();

            if (result != "Success") return result;
            for (short core = 1; core <= 2; core++)
            {
                for (short axis = 1; axis <= 12; axis++)
                {
                    result = AxisOn(core, axis, false);
                    if (result != "Success") return result;
                }
            }
            return result;
        }

        /// <summary>
        /// 获取轴状态
        /// </summary>
        /// <returns></returns>
        public string GetAxisStatus(short core, short axis, out int sts)
        {
            sts = 0;
            string result = CommonInitedValidate();
            if (result == "Success")
            {
                try
                {
                    short error = GTN_GetSts(core, axis, out sts, 1, out uint pClock);
                    //ErrorInfo(error, "GTN_GetSts");
                    return result;
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
            return result;
        }

        /// <summary>
        /// 获取实时位置
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <returns>实时位置</returns>
        public double GetCmdEncPos(short core, short axis)
        {
            string result = CommonInitedValidate();

            if (result != "Success") return -999999.0;

            try
            {
                short error = GTN_GetEncPos(core, axis, out double pValue, 1, out uint pClock);
                //ErrorInfo(error, "GTN_GetPrfPos");
                return pValue;
            }
            catch (Exception)
            {
                return -999999.0;
            }
        }

        /// <summary>
        /// 获取实时速度
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        public double GetCmdEncVel(short core, short axis)
        {
            string result = CommonInitedValidate();

            if (result != "Success") return -999999.0;

            try
            {
                short error = GTN_GetEncVel(core, axis, out double pValue, 1, out uint pClock);
                //ErrorHandler("GTN_GetEncVel", error);
                //return GetpValue(axis, pValue);
                return pValue;
            }
            catch (Exception)
            {
                return -999999.0;
            }
        }

        /// <summary>
        /// 获取规划位置
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        public double GetCmdPrfPos(short core, short axis)
        {
            string result = CommonInitedValidate();
            if (result != "Success") return -999999.0;

            try
            {
                short error = GTN_GetPos(core, axis, out int pPos);
                //ErrorInfo(error, "GTN_GetPos");
                return pPos;
            }
            catch (Exception)
            {
                return -999999.0;
            }
        }

        /// <summary>
        /// 获取规划速度
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        public double GetCmdPrfVel(short core, short axis)
        {
            //判断是否初始化成功

            string result = CommonInitedValidate();

            if (result != "Success") return -999999.0;

            try
            {
                short error = GTN_GetPrfVel(core, axis, out double pValue, 1, out uint pClock);
                //ErrorHandler("GTN_GetPrfVel", error);
                //return GetpValue(axis, pValue);
                return pValue;
            }
            catch (Exception)
            {
                return -999999.0;
            }
        }

        /// <summary>
        /// 清除轴状态
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        public string AxisClrSts(short core, short axis)
        {
            string result = CommonInitedValidate();
            if (result != "Success") return result;
            try
            {
                //清除标志
                short error = GTN_ClrSts(core, axis, 1);
                ErrorHandler("GTN_ClrSts", error);

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Success";
        }

        /// <summary>
        /// 轴开启/关闭使能
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        public string AxisOn(short core, short axis, bool ison)
        {
            string result = CommonInitedValidate();
            if (result != "Success") return result;
            short error = 0;
            try
            {
                if (ison) error = GTN_AxisOn(core, axis);
                else error = GTN_AxisOff(core, axis);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Success";
        }


        /// <summary>
        /// 轴停止运动
        /// </summary>
        /// <param name="core"></param>
        /// <param name="axis"></param>
        /// <param name="isSmoothStop">0:平滑停止 1:急停停止</param>
        /// <returns></returns>
        public string AxisStop(short core, short axis, int isSmoothStop)
        {
            string result = CommonInitedValidate();

            if (result != "Success") return result;

            try
            {
                short error = GTN_Stop(core, 1 << (axis - 1), isSmoothStop);//1 << (axis - 1)

                ErrorHandler("GTN_Stop", error);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Success";
        }

        /// <summary>
        /// 轴位置清零
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        public string AxisZeroPos(short core, short axis)
        {
            string result = CommonInitedValidate();

            if (result != "Success") return result;

            try
            {
                short error = GTN_ZeroPos(core, axis, 1);
                ErrorHandler("GTN_ZeroPos", error);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Success";
        }

        #endregion

        #region 轴动作

        /// <summary>
        /// 单轴相对定位
        /// </summary>
        /// <param name="axisRun">轴运行信息</param>
        /// <returns></returns>
        public string UniaxialRelativeMove(AxisRun axisRun)
        {
            //通用运行初始化验证
            string result = CommonMotionValidate(axisRun.核号, axisRun.轴号);

            if (result != "Success") return result;

            try
            {
                //清除标志
                short error = GTN_ClrSts(axisRun.核号, axisRun.轴号, 1);
                ErrorHandler("GTN_ClrSts", error);

                // 将轴设为点位模式
                error = GTN_PrfTrap(axisRun.核号, axisRun.轴号);
                ErrorHandler("GTN_PrfTrap", error);

                //创建一个TJogPrm对象
                TTrapPrm trap;

                // 读取点位运动参数
                error = GTN_GetTrapPrm(axisRun.核号, axisRun.轴号, out trap);
                ErrorHandler("GTN_GetTrapPrm", error);

                trap.acc = axisRun.目标加速度;
                trap.dec = axisRun.目标减速度;
                trap.smoothTime = axisRun.平滑时间;//[0,50]

                // 设置点位运动参数
                error = GTN_SetTrapPrm(axisRun.核号, axisRun.轴号, ref trap);
                ErrorHandler("GTN_SetTrapPrm", error);

                //获取当前规划位置
                double current = GetCmdPrfPos(axisRun.核号, axisRun.轴号);

                int posPulse = Convert.ToInt32(Math.Round((current + axisRun.目标位置)));

                // 设置AXIS轴的目标位置
                error = GTN_SetPos(axisRun.核号, axisRun.轴号, posPulse);
                ErrorHandler("GTN_SetPos", error);

                // 设置AXIS轴的目标速度
                error = GTN_SetVel(axisRun.核号, axisRun.轴号, axisRun.目标速度);
                ErrorHandler("GTN_SetVel", error);

                // 启动AXIS轴的运动
                error = GTN_Update(axisRun.核号, 1 << (axisRun.轴号 - 1));
                ErrorHandler("GTN_Update", error);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Success";
        }

        /// <summary>
        /// 单轴绝对定位
        /// </summary>
        /// <param name="axisRun">轴运行信息</param>
        /// <returns></returns>
        public string UniaxialAbsoluteMoveAbs(AxisRun axisRun)
        {
            //通用运行初始化验证
            string result = CommonMotionValidate(axisRun.核号, axisRun.轴号);

            if (result != "Success") return result;

            try
            {
                //清除标志
                short error = GTN_ClrSts(axisRun.核号, axisRun.轴号, 1);
                ErrorHandler("GTN_ClrSts", error);

                // 将轴设为点位模式
                error = GTN_PrfTrap(axisRun.核号, axisRun.轴号);
                ErrorHandler("GTN_PrfTrap", error);

                //创建一个TJogPrm对象
                TTrapPrm trap;

                // 读取点位运动参数
                error = GTN_GetTrapPrm(axisRun.核号, axisRun.轴号, out trap);
                ErrorHandler("GTN_GetTrapPrm", error);

                trap.acc = axisRun.目标加速度;
                trap.dec = axisRun.目标减速度;
                trap.smoothTime = axisRun.平滑时间;//[0,50]

                // 设置点位运动参数
                error = GTN_SetTrapPrm(axisRun.核号, axisRun.轴号, ref trap);
                ErrorHandler("GTN_SetTrapPrm", error);

                // 设置AXIS轴的目标位置
                error = GTN_SetPos(axisRun.核号, axisRun.轴号, axisRun.目标位置);
                ErrorHandler("GTN_SetPos", error);

                // 设置AXIS轴的目标速度
                error = GTN_SetVel(axisRun.核号, axisRun.轴号, axisRun.目标速度);
                ErrorHandler("GTN_SetVel", error);

                // 启动AXIS轴的运动
                error = GTN_Update(axisRun.核号, 1 << (axisRun.轴号 - 1));
                ErrorHandler("GTN_Update", error);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Success";
        }

        /// <summary>
        /// 多轴绝对定位
        /// </summary>
        /// <param name="axisRun">轴运行信息</param>
        /// <returns></returns>
        public string MultiaxisAbsoluteMoveAbs(List<AxisRun> listAxisRun)
        {
            string result = "";

            //通用运行初始化验证
            for (int i = 0; i < listAxisRun.Count; i++)
            {
                result = CommonMotionValidate(listAxisRun[i].核号, listAxisRun[i].轴号);

                if (result != "Success") return result;
            }

            //定义一个命令返回值
            short error = 0;

            try
            {
                List<short[]> intmask = new List<short[]> { new short[32], new short[32] };

                for (int i = 0; i < listAxisRun.Count; i++)
                {
                    intmask[listAxisRun[i].核号 - 1][listAxisRun[i].轴号 - 1] = 1;
                    //清除标志
                    error = GTN_ClrSts(listAxisRun[i].核号, listAxisRun[i].轴号, 1);
                    ErrorHandler("GTN_ClrSts", error);

                    // 将轴设为点位模式
                    error = GTN_PrfTrap(listAxisRun[i].核号, listAxisRun[i].轴号);
                    ErrorHandler("GTN_PrfTrap", error);

                    //创建一个TJogPrm对象
                    TTrapPrm trap;

                    // 读取点位运动参数
                    error = GTN_GetTrapPrm(listAxisRun[i].核号, listAxisRun[i].轴号, out trap);
                    ErrorHandler("GTN_GetTrapPrm", error);

                    trap.acc = listAxisRun[i].目标加速度;
                    trap.dec = listAxisRun[i].目标减速度;
                    trap.smoothTime = listAxisRun[i].平滑时间;//[0,50]

                    // 设置点位运动参数
                    error = GTN_SetTrapPrm(listAxisRun[i].核号, listAxisRun[i].轴号, ref trap);
                    ErrorHandler("GTN_SetTrapPrm", error);

                    // 设置AXIS轴的目标位置
                    error = GTN_SetPos(listAxisRun[i].核号, listAxisRun[i].轴号, listAxisRun[i].目标位置);
                    ErrorHandler("GTN_SetPos", error);

                    // 设置AXIS轴的目标速度
                    error = GTN_SetVel(listAxisRun[i].核号, listAxisRun[i].轴号, listAxisRun[i].目标速度);
                    ErrorHandler("GTN_SetVel", error);
                }

                string mask1Str = new string(string.Join("", intmask[0]).Reverse().ToArray());
                int maskcore1 = Convert.ToInt32(mask1Str, 2); // 标识了输入字符串的进制类型,是实现二进制到十进制转换的关键设置

                string mask2Str = new string(string.Join("", intmask[1]).Reverse().ToArray());
                int maskcore2 = Convert.ToInt32(mask2Str, 2); // 标识了输入字符串的进制类型,是实现二进制到十进制转换的关键设置

                if (maskcore1 != 0) //如果核1有轴要同时运动
                {
                    error = GTN_Update(1, maskcore1);
                    ErrorHandler($"GTN_Update(1,{maskcore1})", error);
                }

                if (maskcore2 != 0) //如果核2有轴要同时运动
                {
                    error = GTN_Update(2, maskcore2);
                    ErrorHandler($"GTN_Update(2,{maskcore2})", error);
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Success";
        }

        /// <summary>
        /// 点动JOG
        /// </summary>
        /// <param name="axisRun"></param>
        /// <returns></returns>
        public string JogMove(AxisRun axisRun)
        {
            string result = CommonMotionValidate(axisRun.核号, axisRun.轴号);

            if (result != "Success") return result;

            try
            {
                //设置为JOG模式
                short error = GTN_PrfJog(axisRun.核号, axisRun.轴号);

                ErrorHandler("GTN_PrfJog", error);

                // 读取Jog运动参数
                error = GTN_GetJogPrm(axisRun.核号, axisRun.轴号, out TJogPrm jog);
                ErrorHandler("GTN_GetJogPrm", error);

                //给jog赋值
                jog.acc = axisRun.目标加速度;
                jog.dec = axisRun.目标减速度;
                //jog.smooth = axisRun.Jog平滑时间;//0-1之间，0.8越接近1，越平滑 //设置平滑时间会报指令错误

                // 设置Jog运动参数
                error = GTN_SetJogPrm(axisRun.核号, axisRun.轴号, ref jog);
                ErrorHandler("GTN_SetJogPrm", error);

                // 设置轴的目标速度
                error = GTN_SetVel(axisRun.核号, axisRun.轴号, axisRun.目标速度);
                ErrorHandler("GTN_SetVel", error);

                // 启动AXIS轴的运动
                error = GTN_Update(axisRun.核号, 1 << (axisRun.轴号 - 1));
                ErrorHandler("GTN_Update", error);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "Success";
        }

        /// <summary>
        /// 单轴回零
        /// </summary>
        /// <param name="axisHome">轴回零信息</param>
        /// <returns></returns>
        public string UniaxialHome(AxisHomeInfo axisHome)
        {
            //判断是否运动
            string result = CommonMotionValidate(axisHome.核号, axisHome.轴号);

            if (result != "Success") return result;

            try
            {
                //设置Smart Home回原点参数
                THomePrm tHomePrm;

                short error = GTN_ClrSts(axisHome.核号, axisHome.轴号, 1);
                ErrorHandler($"GTN_ClrSts({axisHome.核号}, {axisHome.轴号}, 1)", error);

                int lAxisStatus = 0;
                error = GTN_GetSts(axisHome.核号, axisHome.轴号, out lAxisStatus, 1, out uint pClock);
                ErrorHandler($"GTN_GetSts({axisHome.核号}, {axisHome.轴号}, out {lAxisStatus}, 1)", error);

                if ((lAxisStatus & 0x200) == 0) //如果已经使能了则不用再使能
                {
                    error = GTN_AxisOn(axisHome.核号, axisHome.轴号);
                    do
                    {
                        error = GTN_GetSts(axisHome.核号, axisHome.轴号, out lAxisStatus, 1, out pClock);
                        Thread.Sleep(1); //减少CPU和轴卡的压力
                    } while ((lAxisStatus & 0x200) == 0);

                    //if (axis == advancedParam.Axis_InputArm_X || axis == advancedParam.Axis_OutputArm_X) Thread.Sleep(3000);
                    //else Thread.Sleep(400); 
                    Thread.Sleep(400);
                }

                error = GTN_GetHomePrm(axisHome.核号, axisHome.轴号, out tHomePrm);
                ErrorHandler($"GTN_GetHomePrm({axisHome.核号}, {axisHome.轴号}, {tHomePrm})", error);

                //int HOME_MODE_LIMIT = 10;  限位回原点
                //int HOME_MODE_LIMIT_HOME = 11;  *限位+Home回原点*
                //int HOME_MODE_LIMIT_INDEX = 12;  限位+Index回原点
                //int HOME_MODE_LIMIT_HOME_INDEX = 13;  限位+Home+Index回原点
                //int HOME_MODE_HOME = 20;  *Home回原点*
                //int HOME_MODE_HOME_INDEX = 22;  Home+Index回原点
                //int HOME_MODE_INDEX = 30;  Index回原点

                tHomePrm.mode = axisHome.模式;
                tHomePrm.moveDir = axisHome.回零启动方向;
                tHomePrm.indexDir = 1; //必须赋值,否则报错
                tHomePrm.edge = axisHome.捕获触发沿;
                tHomePrm.velHigh = axisHome.搜索速度;
                tHomePrm.velLow = 1; //必须赋值,否则报错
                tHomePrm.acc = axisHome.加速度;
                tHomePrm.dec = axisHome.减速度;
                tHomePrm.smoothTime = axisHome.平滑时间;
                tHomePrm.homeOffset = axisHome.零点偏置;
                //tHomePrm.searchHomeDistance = 200000;
                //tHomePrm.searchIndexDistance = 30000;
                tHomePrm.escapeStep = axisHome.反向脱离步长;

                //寻原点
                error = GTN_GoHome(axisHome.核号, axisHome.轴号, ref tHomePrm);//启动Smart Home回原点
                ErrorHandler($"GTN_GoHome({axisHome.核号}, {axisHome.轴号}, ref tHomePrm)", error);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Success";
        }

        /// <summary>
        /// 多轴回零
        /// </summary>
        /// <param name="ListAxisHome"></param>
        /// <returns></returns>
        public string MultiaxisHome(List<AxisHomeInfo> listAxisHome)
        {
            string result = "";

            //通用运行初始化验证
            for (int i = 0; i < listAxisHome.Count; i++)
            {
                result = CommonMotionValidate(listAxisHome[i].核号, listAxisHome[i].轴号);

                if (result != "Success") return result;
            }

            try
            {
                short error;

                THomePrm[] tHomePrm = new THomePrm[listAxisHome.Count];

                for (int i = 0; i < listAxisHome.Count; i++)
                {
                    //设置Smart Home回原点参数
                    tHomePrm[i] = new THomePrm();

                    error = GTN_GetHomePrm(listAxisHome[i].核号, listAxisHome[i].轴号, out tHomePrm[i]);
                    ErrorHandler($"GTN_GetHomePrm({listAxisHome[i].核号}, {listAxisHome[i].轴号}, out tHomePrm[{i}])", error);

                    //int HOME_MODE_LIMIT = 10;  限位回原点
                    //int HOME_MODE_LIMIT_HOME = 11;  *限位+Home回原点*
                    //int HOME_MODE_LIMIT_INDEX = 12;  限位+Index回原点
                    //int HOME_MODE_LIMIT_HOME_INDEX = 13;  限位+Home+Index回原点
                    //int HOME_MODE_HOME = 20;  *Home回原点*
                    //int HOME_MODE_HOME_INDEX = 22;  Home+Index回原点
                    //int HOME_MODE_INDEX = 30;  Index回原点

                    tHomePrm[i].mode = listAxisHome[i].模式;
                    tHomePrm[i].moveDir = listAxisHome[i].回零启动方向;
                    tHomePrm[i].indexDir = 1; //必须赋值,否则报错
                    tHomePrm[i].edge = listAxisHome[i].捕获触发沿;
                    tHomePrm[i].velHigh = listAxisHome[i].搜索速度;
                    tHomePrm[i].velLow = 1; //必须赋值,否则报错
                    tHomePrm[i].acc = listAxisHome[i].加速度;
                    tHomePrm[i].dec = listAxisHome[i].减速度;
                    tHomePrm[i].smoothTime = listAxisHome[i].平滑时间;
                    tHomePrm[i].homeOffset = listAxisHome[i].零点偏置;
                    //tHomePrm.searchHomeDistance = 200000;
                    //tHomePrm.searchIndexDistance = 30000;
                    tHomePrm[i].escapeStep = listAxisHome[i].反向脱离步长;
                }

                for (int i = 0; i < listAxisHome.Count; i++)
                {
                    //寻原点
                    error = GTN_GoHome(listAxisHome[i].核号, listAxisHome[i].轴号, ref tHomePrm[i]);//启动Smart Home回原点
                    ErrorHandler($"GTN_GoHome({listAxisHome[i].核号}, {listAxisHome[i].轴号}, ref tHomePrm[{i}])", error);
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Success";
        }

        #endregion

        #region gLink200模块 型号:HCB5-1616-DTD01

        /// <summary>
        /// 读取GlinK模块DI输入值
        /// </summary>
        /// <param name="core">核号</param>
        /// <param name="diIndex">IO索引,1-32767</param>
        /// <param name="value">32位二进制字符串</param>
        /// <returns></returns>
        public string GLink_GetExtDi(short core, short diIndex, out string value)
        {
            value = "";

            string result = CommonInitedValidate();

            if (result != "Success") return result;

            try
            {
                short error = GTN_GetExtDi(core, diIndex, out int pValue);
                ErrorHandler($"GTN_GetExtDi({core}, {diIndex}, out int {pValue})", error);
                string pValuestr = Convert.ToString(pValue, 2).PadLeft(32, '0');
                value = new string(pValuestr.Reverse().ToArray());
                //value = Convert.ToString(pValue, 2).PadLeft(32, '0').Reverse().ToArray().ToString();
                //bool[] bools = value.Select(c => c == '1').ToArray();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Success";
        }

        /// <summary>
        /// 读取GlinK模块DO输出值  备注：读取的用户设置值，而不是实际硬件输出值
        /// </summary>
        /// <param name="core">核号</param>
        /// <param name="doIndex">IO索引,1-32767</param>
        /// <param name="value">32位二进制字符串</param>
        /// <returns></returns>
        public string GLink_GetExtDO(short core, short doIndex, out string value)
        {
            value = "";

            string result = CommonInitedValidate();

            if (result != "Success") return result;

            try
            {
                short error = GTN_GetExtDo(core, doIndex, out int pValue);
                ErrorHandler($"GTN_GetExtDo({core}, {doIndex}, {pValue})", error);
                string pValuestr = Convert.ToString(pValue, 2).PadLeft(32, '0');
                value = new string(pValuestr.Reverse().ToArray());
                //bool[] bools = value.Select(c => c == '1').ToArray();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Success";
        }

        /// <summary>
        /// 按位设置Glink模块DO输出值
        /// </summary>
        /// <param name="core">核号</param>
        /// <param name="doIndex">IO索引,1-32767</param>
        /// <param name="value">输出值:true亮/fasle灭</param>
        /// <returns></returns>
        public string GLink_SetExtDoBit(short core, short doIndex, bool value)
        {
            string result = CommonInitedValidate();

            if (result != "Success") return result;

            short error;

            try
            {
                error = GTN_SetExtDoBit(core, doIndex, (short)(value ? 1 : 0));
                ErrorHandler($"GTN_SetEcatAxisDOBit({core}, {doIndex}, {value})", error);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Success";
        }
        #endregion

        #region 网络型轴模块 型号:GNM-601-00

        //diType -指定数字 IO 类型。 
        //MC_LIMIT_POSITIVE(该宏定义为 0)：正限位。
        //MC_LIMIT_NEGATIVE(该宏定义为 1)：负限位。
        //MC_ALARM(该宏定义为 2)：驱动报警。
        //MC_HOME(该宏定义为 3)：原点开关。
        //MC_GPI(该宏定义为 4)：通用输入。
        //MC_ARRIVE(该宏定义为 5)：电机到位信号。
        //MC_MPG(该宏定义为 6)：手轮 MPG 轴选和倍率信号（24 电平输入）。

        /// <summary>
        /// 读取数字IO输入状态
        /// </summary>
        /// <param name="core">核号</param>
        /// <param name="diType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string GNM601_GetDi(short core, short diType, out string value)
        {
            value = "";

            string result = CommonInitedValidate();

            if (result != "Success") return result;

            try
            {
                short error = GTN_GetDi(core, diType, out int pValue);
                ErrorHandler($"GTN_GetDi({core}, {diType}, out int {pValue})", error);
                string pValuestr = Convert.ToString(pValue, 2).PadLeft(32, '0');
                value = new string(pValuestr.Reverse().ToArray());
                //bool[] bools = value.Select(c => c == '1').ToArray();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Success";
        }

        /// <summary>
        /// 按位读取数字IO输入状态
        /// </summary>
        /// <param name="core"></param>
        /// <param name="diType"></param>
        /// <param name="diIndex"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string GNM601_GetDiBit(short core, short diType, short diIndex, out short value)
        {
            value = 0;

            string result = CommonInitedValidate();

            if (result != "Success") return result;

            try
            {
                short error = GTN_GetDiBit(core, diType, diIndex, out short v);
                value = (short)(v == 1 ? 0 : 1);
                ErrorHandler($"GTN_GetDi({core}, {diIndex}, {v})", error);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Success";
        }

        //doType -指定数字 IO 类型。
        //MC_ENABLE(该宏定义为 10)：驱动器使能。
        //MC_CLEAR(该宏定义为 11)：报警清除。
        //MC_GPO(该宏定义为 12)：通用输出。

        /// <summary>
        /// 读取数字IO输出状态
        /// </summary>
        /// <param name="core"></param>
        /// <param name="doType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string GNM601_GetDo(short core, short doType, out string value)
        {
            value = "";

            string result = CommonInitedValidate();

            if (result != "Success") return result;

            try
            {
                short error = GTN_GetDo(core, doType, out int pValue);


                ErrorHandler($"GTN_GetDo({core}, {doType}, out int {pValue})", error);
                string pValuestr = Convert.ToString(pValue, 2).PadLeft(32, '0');
                string v = new string(pValuestr.Reverse().ToArray());
                for(int i = 0;i<v.Length; i++)
                {
                    if (v[i] == '1')
                    {
                        value += "0";
                    }
                    else
                    {
                        value += "1";
                    }
                }

                //bool[] bools = value.Select(c => c == '1').ToArray();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Success";
        }

        /// <summary>
        /// 按位设置数字IO输出状态
        /// </summary>
        /// <param name="core"></param>
        /// <param name="doType"></param>
        /// <param name="doIndex"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string GNM601_SetDoBit(short core, short doType, short doIndex, bool value)
        {
            string result = CommonInitedValidate();

            if (result != "Success") return result;

            try
            {
                short error = GTN_SetDoBit(core, doType, doIndex, (short)(value ? 0 : 1)); //GNM601模块 写0灯亮 与 Glink模块相反写1灯亮
                ErrorHandler($"GTN_SetDoBit({core}, {doType}, {doIndex}, {value})", error);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Success";
        }

        #endregion
    }
}
