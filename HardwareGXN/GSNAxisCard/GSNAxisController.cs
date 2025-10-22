using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GSN_GTN;
using IHardwareGXN.GXNCard;
using static GSN_GTN.mc;
using MotionCommon;

namespace HardwareGXN
{
    public class GSNAxisController : AbstractGXN, IGXNInterface
    {
        public override event Action<string> RunLog;

        public override string PrintGXN()
        {
            return "GSN轴卡";
        }

        /// <summary>
        /// 板卡初始化
        /// </summary>
        /// <returns>操作结果</returns>
        public override string InitCard()
        {
            try
            {
                //打开运动控制器
                short error = GTN_Open(5, 1);
                RunLog($"GTN_Open(5,1):({error})");
                ErrorHandler("GT_Open", error);

                //复位运动控制器(核1)
                error = GTN_Reset(1);
                RunLog($"GTN_Reset(1):({error})");
                ErrorHandler("GT_Reset", error);

                //复位运动控制器(核2)
                error = GTN_Reset(2);
                RunLog($"GTN_Reset(2):({error})");
                ErrorHandler("GT_Reset", error);

                // 注意：配置文件取消了各轴的报警和限位
                // 配置运动控制器(核1)
                error = GTN_LoadConfig(1, "gtn_core1.cfg");
                RunLog($"GTN_LoadConfig(1):({error})");
                ErrorHandler("GTN_LoadConfig(1, gtn_core1.cfg)", error);

                // 配置运动控制器(核2)
                error = GTN_LoadConfig(2, "gtn_core2.cfg");
                RunLog($"GTN_LoadConfig(2):({error})");
                ErrorHandler("GTN_LoadConfig(2, gtn_core2.cfg)", error);

                //清除核1轴标志
                error = GTN_ClrSts(1, 1, 12);
                RunLog($"GTN_ClrSts(1, 1, 12):({error})");
                ErrorHandler("GTN_ClrSts", error);

                //清除核2轴标志
                error = GTN_ClrSts(2, 1, 12);
                RunLog($"GTN_ClrSts(2, 1, 12):({error})");
                ErrorHandler("GTN_ClrSts", error);

                error = GTN_ExtModuleInit(1);        //初始化gLink模块
                RunLog($"GT_GLinkInit(1):({error})");
                ErrorHandler("GT_GLinkInit", error);

                short glinkCout = 0;

                if (error == 0)
                {
                    error = GTN_GetExtModuleCount(1, 0, out glinkCout);
                    ErrorHandler("GTN_GetExtModuleCount", error);
                    RunLog(glinkCout + ("个IO模块初始化成功！"));
                }

                InitedOK = glinkCout > 0;

            }
            catch (Exception ex)
            {
                RunLog("[error][" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]：" + "InitCard() " + ex.ToString());
                return ex.Message;
            }

            return "Success";
        }

        /// <summary>
        /// 关闭板卡
        /// </summary>
        /// <returns></returns>
        public override string CloseCard()
        {
            if (InitedOK == false) return "未初始化";

            //关闭所有轴
            AllAxisOff();

            //关闭卡
            try
            {
                short error = GTN_Close(); //关闭轴卡
                ErrorHandler("GTN_Close", error);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "Success";
        }

        /// <summary>
        /// 报警代码
        /// </summary>
        /// <param name="command"></param>
        /// <param name="error"></param>
        public override void ErrorHandler(string command, short error)
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

        #region 轴状态信息

        /// <summary>
        /// 判断某个轴是否运行
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        public override string IsMoving(short core, short axis, out bool ismove)
        {
            try
            {
                short error = 0;

                error = GTN_GetSts(core, axis, out int sts, 1, out uint pClock);

                ErrorHandler("GTN_GetSts", error);

                ismove = (sts & 0x400) == 1;

                return "Success";
            }
            catch (Exception ex)
            {
                ismove = true;

                return ex.Message;
            }
        }

        /// <summary>
        /// 判断某个轴是否使能
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        public override string IsEnable(short core, short axis, out bool isEnable)
        {
            try
            {
                short error = 0;

                error = GTN_GetSts(core, axis, out int sts, 1, out uint pClock);

                ErrorHandler("GTN_GetSts", error);

                isEnable = !((sts & 0x200) == 0);

                return "Success";
            }
            catch (Exception ex)
            {
                isEnable = false;

                return ex.Message;
            }
        }

        /// <summary>
        /// 获取轴状态
        /// </summary>
        /// <returns></returns>
        public override string GetAxisStatus(short core, short axis, out int sts)
        {
            sts = 0;

            if (InitedOK == false) return "未初始化";

            try
            {
                short error = GTN_GetSts(core, axis, out sts, 1, out uint pClock);
                //ErrorInfo(error, "GTN_GetSts");
                return "Success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        /// <summary>
        /// 获取轴模块通讯状态
        /// </summary>
        /// <returns></returns>
        public override string GetAxisTerminalStatus(short core, short index)//, out GSN_GTN.mc_ringnet.TTerminalStatus terminalStatus
        {
            mc_ringnet.TTerminalStatus terminalStatus = new mc_ringnet.TTerminalStatus();
            try
            {
                short error = mc_ringnet.GTN_GetTerminalStatus(core, index, out terminalStatus);
                ErrorHandler("GTN_GetTerminalStatus", error);
                return "Success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 获取实时位置
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <returns>实时位置</returns>
        public override string GetCmdEncPos(short core, short axis, out double pPos)
        {
            pPos = 0;

            if (InitedOK == false) return "未初始化";

            try
            {
                short error = GTN_GetEncPos(core, axis, out pPos, 1, out uint pClock);
                //ErrorInfo(error, "GTN_GetPrfPos");
                return "Success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 获取实时速度
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        public override string GetCmdEncVel(short core, short axis, out double pPos)
        {
            pPos = 0;

            if (InitedOK == false) return "未初始化";

            try
            {
                short error = GTN_GetEncVel(core, axis, out pPos, 1, out uint pClock);
                //ErrorHandler("GTN_GetEncVel", error);
                //return GetpValue(axis, pValue);
                return "Success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 获取规划位置
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        public override string GetCmdPrfPos(short core, short axis, out int pPos)
        {
            pPos = 0;

            if (InitedOK == false) return "未初始化";

            try
            {
                short error = GTN_GetPos(core, axis, out pPos);
                //ErrorInfo(error, "GTN_GetPos");
                return "Success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 获取规划速度
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        public override string GetCmdPrfVel(short core, short axis, out double pPos)
        {
            pPos = 0;

            if (InitedOK == false) return "未初始化";

            try
            {
                short error = GTN_GetPrfVel(core, axis, out pPos, 1, out uint pClock);
                //ErrorHandler("GTN_GetPrfVel", error);
                //return GetpValue(axis, pValue);
                return "Success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        #endregion

        #region 轴控制

        /// <summary>
        /// 所有轴下使能
        /// </summary>
        /// <returns></returns>
        public override string AllAxisOff()
        {
            //判断是否初始化成功

            if (InitedOK == false) return "未初始化";

            for (short core = 1; core <= 2; core++)
            {
                for (short axis = 1; axis <= 12; axis++)
                {
                    AxisOn(core, axis, false);
                }
            }
            return "Success";
        }

        /// <summary>
        /// 所有轴停止运动
        /// </summary>
        /// <returns></returns>
        public override string AllAxisStop()
        {
            //判断是否初始化成功

            if (InitedOK == false) return "未初始化";

            for (short core = 1; core <= 2; core++)
            {
                for (short axis = 1; axis <= 12; axis++)
                {
                    AxisStop(core, axis, 1);
                }
            }
            return "Success";
        }

        /// <summary>
        /// 清除轴状态
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        public override string AxisClrSts(short core, short axis)
        {
            if (InitedOK == false) return "未初始化";

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
        public override string AxisOn(short core, short axis, bool ison)
        {
            if (InitedOK == false) return "未初始化";

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
        public override string AxisStop(short core, short axis, int isSmoothStop)
        {
            if (InitedOK == false) return "未初始化";

            try
            {
                int sts = 0;
                do
                {
                    short error = GTN_Stop(core, 1 << (axis - 1), isSmoothStop); //1 << (axis - 1)

                    ErrorHandler("GTN_Stop", error);

                    error = GTN_GetSts(core, axis, out sts, 1, out uint pClock);

                    ErrorHandler("GTN_GetSts", error);

                }
                while ((sts & 0x400) != 0);
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
        public override string AxisZeroPos(short core, short axis)
        {
            if (InitedOK == false) return "未初始化";

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
        /// 轴运动过程中修改速度
        /// </summary>
        /// <param name="axisRun"></param>
        /// <returns></returns>
        private string SetAxisTargetSpeed(AxisRun axisRun)
        {
            try
            {
                // 设置AXIS轴的目标速度
                short error = GTN_SetVel(axisRun.核号, axisRun.轴号, axisRun.目标速度);
                ErrorHandler("GTN_SetVel", error);

                // 启动AXIS轴的运动
                error = GTN_Update(axisRun.核号, 1 << (axisRun.轴号 - 1));
                ErrorHandler("GTN_Update", error);

                return "Success";

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private void SetAxisTargetSpeed(List<AxisRun> axisRuns)
        {

            for (int se = 0; se < axisRuns.Count; se++)
            {
                //RunLog($"{axisRuns[se].轴名称}修改前目标速度:" + axisRuns[se].目标速度);


                //RunLog($"{axisRuns[se].轴名称}修改后目标速度:" + axisRuns[se].目标速度);

                // 设置AXIS轴的目标速度
                short error = GTN_SetVel(axisRuns[se].核号, axisRuns[se].轴号, axisRuns[se].目标速度);
                ErrorHandler("GTN_SetVel", error);
            }

            for (int se = 0; se < axisRuns.Count; se++)
            {
                // 启动AXIS轴的运动
                short error1 = GTN_Update(axisRuns[se].核号, 1 << (axisRuns[se].轴号 - 1));
                ErrorHandler("GTN_Update", error1);
            }
        }

        /// <summary>
        /// 单轴相对定位
        /// </summary>
        /// <param name="axisRun">轴运行信息</param>
        /// <returns></returns>
        public override string UniaxialRelativeMove(AxisRun axisRun, out int posPulse)
        {
            posPulse = 0;

            //通用运行初始化验证
            string result = CommonMotionValidate(axisRun.核号, axisRun.轴号);

            if (result != "Success") return result;

            result = IsEnable(axisRun.核号, axisRun.轴号, out bool isEnable);

            if (result != "Success") return result;

            if (!isEnable) return axisRun.轴名称 + "未使能";

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
                result = GetCmdPrfPos(axisRun.核号, axisRun.轴号, out int pPos);
                if (result != "Success") return result;

                posPulse = Convert.ToInt32(Math.Round(((double)pPos + axisRun.目标位置)));

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
        public override string UniaxialAbsoluteMoveAbs(AxisRun axisRun)
        {
            //通用运行初始化验证
            string result = CommonMotionValidate(axisRun.核号, axisRun.轴号);

            if (result != "Success") return result;

            result = IsEnable(axisRun.核号, axisRun.轴号, out bool isEnable);

            if (result != "Success") return result;

            if (!isEnable) return axisRun.轴名称 + "未使能";

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
        /// 等待单轴移动
        /// </summary>
        /// <param name="axisRun"></param>
        /// <param name="prfPos"></param>
        /// <param name="type"></param>
        /// <param name="move"></param>
        /// <param name="pos2"></param>
        /// <returns></returns>
        public override string WaitAxisMove(AxisRun axisRun, int prfPos, int type = 0, int move = 0, int pos2 = 0)
        {
            string result = "";

            if (InitedOK == false) return "未初始化";

            try
            {
                int sts;
                bool isException = false;
                string errortxt = "";
                bool issecond = false;
                bool issecond2 = false;

                GetCmdPrfPos(axisRun.核号, axisRun.轴号, out int prfPos1); //规划位置
                GetCmdEncPos(axisRun.核号, axisRun.轴号, out double encPos1); //实时位置
                double initposvalue = Math.Abs(prfPos1 - encPos1);

                do
                {
                    short error = GTN_GetSts(axisRun.核号, axisRun.轴号, out sts, 1, out uint pClock);
                    ErrorHandler($"GTN_GetSts({axisRun.核号}, {axisRun.轴号}, out {sts}, 1, out uint pClock)", error);

                    if (axisRun.二段速方式 != 0 && (!issecond || !issecond2))
                    {
                        GetCmdPrfPos(axisRun.核号, axisRun.轴号, out prfPos1); //规划位置
                        GetCmdEncPos(axisRun.核号, axisRun.轴号, out encPos1); //实时位置
                        double posvalue1 = Math.Abs(prfPos1 - encPos1);

                        if (axisRun.二段速方式 == 1 && !issecond)
                        {
                            if (posvalue1 <= axisRun.二段速结束时减速距离) //进入减速区域开始减速
                            {
                                issecond = true;
                                axisRun.目标速度 = axisRun.二段速结束时减速速度;
                                result = SetAxisTargetSpeed(axisRun);
                                if (result != "Success") RunLog(result);
                            }
                        }
                        else if (axisRun.二段速方式 == 2 && !issecond2)
                        {
                            if (posvalue1 <= initposvalue - axisRun.二段速启动时减速距离) //离开减速区域开始加速
                            {
                                issecond2 = true;
                                axisRun.目标速度 = axisRun.二段速启动结束后恢复速度;
                                result = SetAxisTargetSpeed(axisRun);
                                if (result != "Success") RunLog(result);
                            }
                        }
                        else if (axisRun.二段速方式 == 3 && (!issecond || !issecond2))
                        {
                            if (!issecond2)
                            {
                                if (posvalue1 <= initposvalue - axisRun.二段速启动时减速距离) //离开减速区域开始加速
                                {
                                    issecond2 = true;
                                    axisRun.目标速度 = axisRun.二段速启动结束后恢复速度;
                                    result = SetAxisTargetSpeed(axisRun);
                                    if (result != "Success") RunLog(result);
                                }
                            }
                            else if (!issecond && issecond2)
                            {
                                if (posvalue1 <= axisRun.二段速结束时减速距离) //进入减速区域开始减速
                                {
                                    issecond = true;
                                    axisRun.目标速度 = axisRun.二段速结束时减速速度;
                                    result = SetAxisTargetSpeed(axisRun);
                                    if (result != "Success") RunLog(result);
                                }
                            }
                        }

                        //if (axisRun.二段速方式 == 1 || axisRun.二段速方式 == 2)
                        //{
                        //    if (posvalue1 < axisRun.多段速减速距离)
                        //    {
                        //        issecond = true;
                        //        axisRun.目标速度 = axisRun.多段速速度; //axisRun.目标速度 * 第二段速度;

                        //        // 设置AXIS轴的目标速度
                        //        error = GTN_SetVel(axisRun.核号, axisRun.轴号, axisRun.目标速度);
                        //        ErrorHandler("GTN_SetVel", error);

                        //        // 启动AXIS轴的运动
                        //        error = GTN_Update(axisRun.核号, 1 << (axisRun.轴号 - 1));
                        //        ErrorHandler("GTN_Update", error);
                        //    }
                        //}
                        //else if (axisRun.二段速方式 == 3)
                        //{
                        //    //后续再写，暂时没需求
                        //}
                    }

                    if ((IsPause && axisRun.暂停类型 == 0)) //type == 0 : 暂停有效  == 1 暂停无效
                    {
                        GTN_Stop(axisRun.核号, 1 << (axisRun.轴号 - 1), axisRun.停止类型);
                        while (true)
                        {
                            if (IsForecePause) return "强制停止";
                            if (IsPause) Thread.Sleep(10);
                            else
                            {
                                result = UniaxialAbsoluteMoveAbs(axisRun);
                                break;
                            }
                        }
                    }

                    if (IsEmptyPause && axisRun.轴名称 == "M12_Empty顶盘轴") //type == 0 : 暂停有效  == 1 暂停无效
                    {
                        GTN_Stop(axisRun.核号, 1 << (axisRun.轴号 - 1), axisRun.停止类型);
                        while (true)
                        {
                            if (IsForecePause) return "强制停止";
                            if (IsEmptyPause) Thread.Sleep(10);
                            else
                            {
                                result = UniaxialAbsoluteMoveAbs(axisRun);
                                break;
                            }
                        }
                    }

                    if (IsLoadPause && axisRun.轴名称 == "M13_Loader顶盘轴") //type == 0 : 暂停有效  == 1 暂停无效
                    {
                        GTN_Stop(axisRun.核号, 1 << (axisRun.轴号 - 1), axisRun.停止类型);
                        while (true)
                        {
                            if (IsForecePause) return "强制停止";
                            if (IsLoadPause) Thread.Sleep(10);
                            else
                            {
                                result = UniaxialAbsoluteMoveAbs(axisRun);
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

                if (axisRun.跟随误差检测) //只有伺服轴才需要确认编码器位置和规划位置，步进电机没有编码器
                {
                    Stopwatch stopwatch = new Stopwatch();
                    bool isvalue = true;

                    int errorvalue = axisRun.跟随误差;
                    //if (axisRun.轴名称 == "M19_测试手臂Z轴")
                    //{
                    //    errorvalue = 测试Z跟随误差;
                    //}

                    double encPos = 0;
                    stopwatch.Start();
                    while (isvalue)
                    {
                        GetCmdEncPos(axisRun.核号, axisRun.轴号, out encPos);
                        double posvalue = Math.Abs(prfPos - encPos);

                        if (stopwatch.ElapsedMilliseconds > axisRun.轴停后等待超时时间)
                        {
                            RunLog($"{axisRun.轴名称}移动超时{axisRun.轴停后等待超时时间}ms");
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
                    //RunLog($"{axisRun.轴名称}移动完成,等待时间{stopwatch.ElapsedMilliseconds}ms,当前目标位置{prfPos},实际位置{encPos}");
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
        /// 多轴绝对定位
        /// </summary>
        /// <param name="axisRun">轴运行信息</param>
        /// <returns></returns>
        public override string MultiaxisAbsoluteMoveAbs(List<AxisRun> listAxisRun)
        {
            string result = "";

            //通用运行初始化验证
            for (int i = 0; i < listAxisRun.Count; i++)
            {
                result = CommonMotionValidate(listAxisRun[i].核号, listAxisRun[i].轴号);

                if (result != "Success") return result;

                result = IsEnable(listAxisRun[i].核号, listAxisRun[i].轴号, out bool isEnable);

                if (result != "Success") return result;

                if (!isEnable) return listAxisRun[i].轴名称 + "未使能";
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
        /// 等待多轴移动
        /// </summary>
        /// <param name="axisRuns"></param>
        /// <param name="prfPos"></param>
        /// <param name="type"></param>
        /// <param name="secondType"></param>
        /// <param name="secondpos"></param>
        /// <returns></returns>
        public override string WaitAxisMove(List<AxisRun> axisRuns, List<int> prfPos, int type = 0, int secondType = 0, int secondpos = 0)
        {
            string result = "";

            if (InitedOK == false) return "未初始化";

            try
            {
                bool value = true;
                bool isException = false;
                string errortxt = "";

                int[] sts = new int[axisRuns.Count];
                bool[] issecond = new bool[axisRuns.Count];
                bool[] issecond2 = new bool[axisRuns.Count];

                double[] initposvalue = new double[axisRuns.Count];
                for (int i = 0; i < axisRuns.Count; i++)
                {
                    GetCmdPrfPos(axisRuns[i].核号, axisRuns[i].轴号, out int prfPos1); //规划位置
                    GetCmdEncPos(axisRuns[i].核号, axisRuns[i].轴号, out double encPos1); //实时位置
                    initposvalue[i] = Math.Abs(prfPos1 - encPos1);
                }

                do
                {
                    value = false;
                    isException = false;
                    errortxt = "";

                    for (int i = 0; i < axisRuns.Count; i++)
                    {
                        short error = GTN_GetSts(axisRuns[i].核号, axisRuns[i].轴号, out sts[i], 1, out uint pClock);
                        ErrorHandler($"GTN_GetSts({axisRuns[i].核号}, {axisRuns[i].轴号}, out {sts[i]}, 1, out uint pClock)", error);
                    }

                    for (int i = 0; i < axisRuns.Count; i++)
                    {
                        GetCmdPrfPos(axisRuns[i].核号, axisRuns[i].轴号, out int prfPos1); //规划位置
                        GetCmdEncPos(axisRuns[i].核号, axisRuns[i].轴号, out double encPos1); //实时位置
                        double posvalue1 = Math.Abs(prfPos1 - encPos1);

                        if (axisRuns[i].二段速方式 != 0 && (issecond.Contains(false) || issecond2.Contains(false)))
                        {
                            if (axisRuns[i].二段速方式 == 1 && issecond.Contains(false)) //只要有一个Z到达减速区间则全部减速，这样才会一致
                            {
                                if (posvalue1 <= axisRuns[i].二段速结束时减速距离) //进入减速区域开始减速
                                {
                                    if (axisRuns[i].二段速统一更改)
                                    {
                                        for (int se = 0; se < axisRuns.Count; se++)
                                        {
                                            issecond[se] = true;
                                            axisRuns[se].目标速度 = axisRuns[se].二段速结束时减速速度;
                                        }
                                        SetAxisTargetSpeed(axisRuns);
                                        break;
                                    }
                                    else
                                    {
                                        issecond[i] = true;
                                        axisRuns[i].目标速度 = axisRuns[i].二段速结束时减速速度;
                                        result = SetAxisTargetSpeed(axisRuns[i]);
                                        if (result != "Success") RunLog(result);
                                    }
                                }
                            }
                            else if (axisRuns[i].二段速方式 == 2 && issecond2.Contains(false))
                            {
                                if (posvalue1 <= initposvalue[i] - axisRuns[i].二段速启动时减速距离) //进入减速区域开始减速
                                {
                                    if (axisRuns[i].二段速统一更改)
                                    {
                                        for (int se = 0; se < axisRuns.Count; se++)
                                        {
                                            issecond2[se] = true;
                                            axisRuns[se].目标速度 = axisRuns[se].二段速启动结束后恢复速度;
                                        }
                                        SetAxisTargetSpeed(axisRuns);
                                        break;
                                    }
                                    else
                                    {
                                        issecond2[i] = true;
                                        axisRuns[i].目标速度 = axisRuns[i].二段速启动结束后恢复速度;
                                        result = SetAxisTargetSpeed(axisRuns[i]);
                                        if (result != "Success") RunLog(result);
                                    }
                                }
                            }
                            else if (axisRuns[i].二段速方式 == 3 && (issecond.Contains(false) || issecond2.Contains(false)))
                            {
                                if (issecond2.Contains(false))
                                {
                                    if (posvalue1 <= initposvalue[i] - axisRuns[i].二段速启动时减速距离) //进入减速区域开始减速
                                    {
                                        if (axisRuns[i].二段速统一更改)
                                        {
                                            for (int se = 0; se < axisRuns.Count; se++)
                                            {
                                                issecond2[se] = true;
                                                axisRuns[se].目标速度 = axisRuns[se].二段速启动结束后恢复速度;
                                            }
                                            SetAxisTargetSpeed(axisRuns);
                                            break;
                                        }
                                        else
                                        {
                                            issecond2[i] = true;
                                            axisRuns[i].目标速度 = axisRuns[i].二段速启动结束后恢复速度;
                                            result = SetAxisTargetSpeed(axisRuns[i]);
                                            if (result != "Success") RunLog(result);
                                        }
                                    }
                                }
                                else if (issecond.Contains(false) && !issecond2.Contains(false))
                                {
                                    if (posvalue1 <= axisRuns[i].二段速结束时减速距离) //进入减速区域开始减速
                                    {
                                        if (axisRuns[i].二段速统一更改)
                                        {
                                            for (int se = 0; se < axisRuns.Count; se++)
                                            {
                                                issecond[se] = true;
                                                axisRuns[se].目标速度 = axisRuns[se].二段速结束时减速速度;
                                            }
                                            SetAxisTargetSpeed(axisRuns);
                                            break;
                                        }
                                        else
                                        {
                                            issecond[i] = true;
                                            axisRuns[i].目标速度 = axisRuns[i].二段速结束时减速速度;
                                            result = SetAxisTargetSpeed(axisRuns[i]);
                                            if (result != "Success") RunLog(result);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (IsPause && type == 0)
                    {
                        for (int i = 0; i < axisRuns.Count; i++)
                        {
                            GTN_Stop(axisRuns[i].核号, 1 << (axisRuns[i].轴号 - 1), 1);
                        }
                        while (true)
                        {
                            if (IsForecePause) return "强制停止";
                            if (IsPause) Thread.Sleep(10);
                            else
                            {
                                result = MultiaxisAbsoluteMoveAbs(axisRuns);
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

                    if (axisRuns[i].跟随误差检测) //只有伺服轴才需要确认编码器位置和规划位置，步进电机没有编码器
                    {
                        Stopwatch stopwatch = new Stopwatch();
                        bool isvalue = true;

                        int errorvalue = axisRuns[i].跟随误差;
                        //if (axisRuns[i].轴名称 == "M19_测试手臂Z轴")
                        //{
                        //    errorvalue = 测试Z跟随误差;
                        //}
                        double encPos = 0;
                        stopwatch.Start();
                        while (isvalue)
                        {
                            GetCmdEncPos(axisRuns[i].核号, axisRuns[i].轴号, out encPos);
                            double posvalue = Math.Abs(prfPos[i] - encPos);

                            if (stopwatch.ElapsedMilliseconds > axisRuns[i].轴停后等待超时时间)
                            {
                                RunLog($"{axisRuns[i].轴名称}移动超时{axisRuns[i].轴停后等待超时时间}ms");
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
                        //RunLog($"{axisRuns[i].轴名称}移动完成,等待时间{stopwatch.ElapsedMilliseconds}ms,当前目标位置{prfPos[i]},实际位置{encPos}");
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
        /// 点动JOG
        /// </summary>
        /// <param name="axisRun"></param>
        /// <returns></returns>
        public override string JogMove(AxisRun axisRun)
        {
            string result = CommonMotionValidate(axisRun.核号, axisRun.轴号);

            if (result != "Success") return result;

            result = IsEnable(axisRun.核号, axisRun.轴号, out bool isEnable);

            if (result != "Success") return result;

            if (!isEnable) return axisRun.轴名称 + "未使能";

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
        public override string UniaxialHome(AxisHomeInfo axisHome)
        {
            //判断是否运动
            string result = CommonMotionValidate(axisHome.核号, axisHome.轴号);

            if (result != "Success") return result;

            try
            {
                //设置Smart Home回原点参数
                THomePrm tHomePrm;

                int delay = 400;

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

                    if (axisHome.使能后回零延时 > delay) delay = axisHome.使能后回零延时;
                    //if (axisHome.轴名称 == "M22_入料手臂X轴" || axisHome.轴名称 == "M23_出料手臂X轴") delay = 5000;
                }

                Thread.Sleep(delay);

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
                tHomePrm.velLow = axisHome.定位速度;
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
        /// 等待单轴回零完成
        /// </summary>
        /// <param name="axisHome"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public override string WaitAxisHome(AxisHomeInfo axisHome, int type = 0)
        {
            string result;

            if (InitedOK == false) return "未初始化";

            try
            {
                THomeStatus tHomeSts;
                do
                {
                    short error = GTN_GetHomeStatus(axisHome.核号, axisHome.轴号, out tHomeSts);
                    ErrorHandler($"GTN_GetHomeStatus({axisHome.核号}, {axisHome.轴号}, out {tHomeSts})", error);

                    error = GTN_GetSts(axisHome.核号, axisHome.轴号, out int sts, 1, out uint pClock);
                    ErrorHandler($"GTN_GetSts({axisHome.核号}, {axisHome.轴号}, out {sts}, 1, out uint {pClock})", error);

                    if (IsPause && type == 0)
                    {
                        AxisStop(axisHome.核号, axisHome.轴号, 1);
                        return $"{axisHome.轴名称}回零被停止";
                    }

                    if ((sts & (1 << 1)) == 1)
                    {
                        AxisStop(axisHome.核号, axisHome.轴号, 1);
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
                    if (axisHome.回零后检测原点信号) //(axisHome.轴名称 == "M22_入料手臂X轴" || axisHome.轴名称 == "M23_出料手臂X轴")
                    {
                        result = GNM601_GetDi(axisHome.核号, GSN_GTN.mc.MC_HOME, out string value);
                        if (result == "Success")
                        {
                            string iovalue = value[axisHome.轴号 - 1].ToString();
                            if (iovalue != "0")
                            {
                                return $"{axisHome.轴名称}回零失败，回零后未挡住原点";
                            }
                        }
                        else return result;
                    }
                }

                result = AxisZeroPos(axisHome.核号, axisHome.轴号);
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
        /// <param name="ListAxisHome"></param>
        /// <returns></returns>
        public override string MultiaxisHome(List<AxisHomeInfo> listAxisHome)
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
                int delay = 500;

                THomePrm[] tHomePrm = new THomePrm[listAxisHome.Count];

                for (int i = 0; i < listAxisHome.Count; i++)
                {
                    error = GTN_ClrSts(listAxisHome[i].核号, listAxisHome[i].轴号, 1);
                    ErrorHandler($"GTN_ClrSts({listAxisHome[i].核号}, {listAxisHome[i].轴号}, 1)", error);

                    int lAxisStatus = 0;
                    error = GTN_GetSts(listAxisHome[i].核号, listAxisHome[i].轴号, out lAxisStatus, 1, out uint pClock);
                    ErrorHandler($"GTN_GetSts({listAxisHome[i].核号}, {listAxisHome[i].轴号}, out {lAxisStatus}, 1)", error);

                    if ((lAxisStatus & 0x200) == 0) //如果已经使能了则不用再使能
                    {
                        error = GTN_AxisOn(listAxisHome[i].核号, listAxisHome[i].轴号);
                        do
                        {
                            error = GTN_GetSts(listAxisHome[i].核号, listAxisHome[i].轴号, out lAxisStatus, 1, out pClock);
                            Thread.Sleep(1); //减少CPU和轴卡的压力
                        } while ((lAxisStatus & 0x200) == 0);

                        if (listAxisHome[i].使能后回零延时 > delay) delay = listAxisHome[i].使能后回零延时;
                        //要注意第一次上电驱动器会寻相，要等5秒 针对H5509 "M22_入料手臂X轴" 和 "M23_出料手臂X轴"
                        //如果寻相没完成就回零位置就不准
                        //上电上使能，等5秒，再执行回零
                    }
                }

                Thread.Sleep(delay);

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
                    tHomePrm[i].velLow = listAxisHome[i].定位速度; //必须赋值,否则报错
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

        /// <summary>
        /// 等待多轴回零完成
        /// </summary>
        /// <param name="axisHomes"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public override string WaitAxisHome(List<AxisHomeInfo> axisHomes, int type = 0)
        {
            string error = "";
            bool[] bools = new bool[axisHomes.Count];
            for (int i = 0; i < axisHomes.Count; i++)
            {
                string result = WaitAxisHome(axisHomes[i], type);
                if (result != "Success")
                {
                    error += result + ";";
                    RunLog(result);
                    bools[i] = false;
                }
                else bools[i] = true;
            }
            if (bools.Contains(false))
            {
                return error;
            }
            else return "Success";
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
        public override string GLink_GetExtDi(short core, short diIndex, out string value)
        {
            value = "";

            if (InitedOK == false) return "未初始化";

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
        public override string GLink_GetExtDO(short core, short doIndex, out string value)
        {
            value = "";

            if (InitedOK == false) return "未初始化";

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
        public override string GLink_SetExtDoBit(short core, short doIndex, bool value)
        {
            if (InitedOK == false) return "未初始化";

            try
            {
                short error = GTN_SetExtDoBit(core, doIndex, (short)(value ? 1 : 0));
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
        public override string GNM601_GetDi(short core, short diType, out string value)
        {
            value = "";


            if (InitedOK == false) return "未初始化";

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
        public override string GNM601_GetDiBit(short core, short diType, short diIndex, out short value)
        {
            value = 0;

            if (InitedOK == false) return "未初始化";

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
        public override string GNM601_GetDo(short core, short doType, out string value)
        {
            value = "";

            if (InitedOK == false) return "未初始化";

            try
            {
                short error = GTN_GetDo(core, doType, out int pValue);


                ErrorHandler($"GTN_GetDo({core}, {doType}, out int {pValue})", error);
                string pValuestr = Convert.ToString(pValue, 2).PadLeft(32, '0');
                string v = new string(pValuestr.Reverse().ToArray());
                for (int i = 0; i < v.Length; i++)
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
        public override string GNM601_SetDoBit(short core, short doType, short doIndex, bool value)
        {

            if (InitedOK == false) return "未初始化";

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
