using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static GTN.mc;

namespace H5509_V2
{
    public class Motion
    {
        private void ErrorHandler(string command, short error)
        {
            string result = string.Empty;

            switch (error)
            {
                case 0: break;
                case 1: result = $"{command}指令执行错误({error})"; break;
                case 2: result = $"{command}license不支持({error})"; break;
                case 7: result = $"{command}指令参数错误({error})"; break;
                case -1: result = $"{command}主机和运动控制器通讯失败({error})"; break;
                case -6: result = $"{command}打开控制器失败({error})"; break;
                case -7: result = $"{command}运动控制器没有响应({error})"; break;
                case -10: result = $"{command}主机和运动控制器通讯失败({error})"; break;
                case -13: result = $"{command}编码器初始化失败({error})"; break;
                case -14: result = $"{command}编码器初始化失败({error})"; break;
                case -15: result = $"{command}动态库版本不匹配({error})"; break;
                case 15: result = $"{command}动态库版本不匹配({error})"; break;
                case 16: result = $"{command}不具备版本匹配功能({error})"; break;
                default: result = $"{command}未知错误({error})"; break;
            }

            if (result.Length > 0)
            {
                throw new Exception(result);
            }
        }

        /// <summary>
        /// 通用初始化验证
        /// </summary>
        /// <returns>操作结果</returns>
        public OperationResult CommonInitedValidate()
        {
            OperationResult result = new OperationResult();

            if (APPmaster.InitedOK == false)
            {
                result.IsSuccess = false;
                result.ErrorMsg = "未初始化";
                return result;
            }
            return OperationResult.CreateSuccessResult();
        }

        /// <summary>
        /// 通用运动初始化验证
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        private OperationResult CommonMotionValidate(short core, short axis)
        {
            OperationResult result = CommonInitedValidate();

            if (!result.IsSuccess) return result;

            if (IsMoving(core, axis))
            {
                result.IsSuccess = false;
                result.ErrorMsg = "正在运动";
                return result;
            };

            return OperationResult.CreateSuccessResult();
        }

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
        /// 板卡初始化
        /// </summary>
        /// <returns>操作结果</returns>
        public OperationResult InitCard()
        {
            //定义运动控制指令的返回值
            short error;

            try
            {
                //打开运动控制器
                error = GTN_Open(5, 1);
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
                //writeDataHelper.RunLog($"GTN_ClrSts(1, 1, {advancedParam.AxisCount}):({error})");正面帮忙拍一下
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
                for (int i = 0; i < 17; i++)
                {
                    error = GTN_SetExtDoBit(1, (short)(i * 16 + 8), 1);
                    error = GTN_SetExtDoBit(1, (short)(i * 16 + 9), 1);
                }
                GTN_SetExtDoBit(1, (short)(2 * 16 + 10), 1);
                GTN_SetExtDoBit(1, (short)(3 * 16 + 10), 1);
                GTN_SetExtDoBit(1, (short)(4 * 16 + 10), 1);
                GTN_SetExtDoBit(1, (short)(5 * 16 + 10), 1);
                GTN_SetExtDoBit(1, (short)(6 * 16 + 10), 1);
                GTN_SetExtDoBit(1, (short)(7 * 16 + 10), 1);
                bool[] bools;
                string binary32 = "";
                List<bool[]> listbool = new List<bool[]>();
                for (int i = 0; i < 8; i++)
                {
                    error = GTN_GetExtDo(1, (short)(i * 32 + 1), out int pValue);
                    binary32 = Convert.ToString(pValue, 2).PadLeft(32, '0');
                    bools = binary32.Select(c => c == '1').ToArray();
                    listbool.Add(bools);
                }
                
                APPmaster.InitedOK = true;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new OperationResult()
                {
                    IsSuccess = false,
                    ErrorMsg = ex.Message,
                };
            }

            return OperationResult.CreateSuccessResult();
        }

        /// <summary>
        /// 关闭某个轴
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <returns>操作结果</returns>
        public OperationResult CloseAxis(short axis)
        {
            //判断是否初始化成功
            OperationResult result = CommonInitedValidate();

            if (!result.IsSuccess) return result;

            //创建指令返回值
            short error;
            try
            {
                error = GTN_AxisOff(1, axis);
                ErrorHandler("GTN_AxisOff", error);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMsg = ex.Message;
                return result;
            }
            return OperationResult.CreateSuccessResult();
        }

        /// <summary>
        /// 关闭所有轴
        /// </summary>
        /// <returns></returns>
        public OperationResult CloseAllAxis()
        {
            //判断是否初始化成功
            OperationResult result = CommonInitedValidate();

            if (!result.IsSuccess) return result;

            for (short i = 1; i <= 24; i++)
            {
                result = CloseAxis(i);
                if (!result.IsSuccess) return result;
            }

            return result;
        }

        /// <summary>
        /// 关闭板卡
        /// </summary>
        /// <returns></returns>
        public OperationResult CloseCard()
        {
            //判断是否初始化成功
            OperationResult result = CommonInitedValidate();

            if (!result.IsSuccess) return result;

            //创建指令返回值
            short error;

            //关闭所有轴
            CloseAllAxis();

            //关闭卡
            try
            {
                error = GTN_Close(); //关闭轴卡
                ErrorHandler("GTN_Close", error);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMsg = ex.Message;
                return result;
            }

            return OperationResult.CreateSuccessResult();
        }

        /// <summary>
        /// 获取轴状态
        /// </summary>
        /// <returns></returns>
        public OperationResult GetAxisStatus(short core, short axis, out int sts)
        {
            sts = 0;
            OperationResult result = CommonInitedValidate();
            if (result.IsSuccess)
            {
                try
                {
                    short error = GTN_GetSts(core, axis, out sts, 1, out uint pClock);
                    //ErrorInfo(error, "GTN_GetSts");
                    return result;
                }
                catch (Exception ex)
                {
                    result.IsSuccess = false;
                    result.ErrorMsg = ex.Message;
                    return result;
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
            OperationResult result = CommonInitedValidate();
            if (!result.IsSuccess) return -999999.0;
            //定义指令返回值
            short error = 0;
            try
            {
                error = GTN_GetEncPos(core, axis, out double pValue, 1, out uint pClock);
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
            //判断是否初始化成功

            OperationResult result = CommonInitedValidate();

            if (!result.IsSuccess) return -999999.0;

            //定义指令返回值
            short error = 0;

            try
            {
                error = GTN_GetEncVel(core, axis, out double pValue, 1, out uint pClock);
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
            OperationResult result = CommonInitedValidate();
            if (!result.IsSuccess) return -999999.0;

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

            OperationResult result = CommonInitedValidate();

            if (!result.IsSuccess) return -999999.0;

            //定义指令返回值
            short error = 0;

            try
            {
                error = GTN_GetPrfVel(core, axis, out double pValue, 1, out uint pClock);
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
        public OperationResult AxisClrSts(short core, short axis)
        {
            //通用运行初始化验证

            OperationResult result = CommonInitedValidate();
            if (!result.IsSuccess) return result;
            try
            {
                //清除标志
                short error = GTN_ClrSts(core, axis, 1);
                ErrorHandler("GTN_ClrSts", error);

            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMsg = ex.Message;
                return result;
            }
            return OperationResult.CreateSuccessResult();
        }

        /// <summary>
        /// 轴使能
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        public OperationResult AxisOn(short core, short axis, bool ison)
        {
            //通用运行初始化验证

            OperationResult result = CommonInitedValidate();
            if (!result.IsSuccess) return result;
            short error = 0;
            try
            {
                if (ison) error = GTN_AxisOn(core, axis);
                else error = GTN_AxisOff(core, axis);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMsg = ex.Message;
                return result;
            }
            return OperationResult.CreateSuccessResult();
        }

        /// <summary>
        /// 轴位置清零
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        public OperationResult AxisZeroPos(short core, short axis)
        {
            OperationResult result = CommonInitedValidate();

            if (!result.IsSuccess) return result;

            //定义指令返回值
            short error = 0;
            try
            {
                error = GTN_ZeroPos(core, axis, 1);
                ErrorHandler("GTN_ZeroPos", error);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMsg = ex.Message;
                return result;
            }
            return OperationResult.CreateSuccessResult();
        }

        /// <summary>
        /// 单轴相对定位
        /// </summary>
        /// <param name="axisRun">轴运行信息</param>
        /// <returns></returns>
        public OperationResult UniaxialRelativeMove(AxisRun axisRun)
        {
            //通用运行初始化验证
            OperationResult result = CommonMotionValidate(axisRun.核号, axisRun.轴号);

            if (!result.IsSuccess) return result;

            //定义一个命令返回值
            short error = 0;

            try
            {
                //清除标志
                error = GTN_ClrSts(axisRun.核号, axisRun.轴号, 1);
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
                result.IsSuccess = false;
                result.ErrorMsg = ex.Message;
                return result;
            }
            return OperationResult.CreateSuccessResult();
        }

        /// <summary>
        /// 单轴绝对定位
        /// </summary>
        /// <param name="axisRun">轴运行信息</param>
        /// <returns></returns>
        public OperationResult UniaxialAbsoluteMoveAbs(AxisRun axisRun)
        {
            //通用运行初始化验证
            OperationResult result = CommonMotionValidate(axisRun.核号, axisRun.轴号);

            if (!result.IsSuccess) return result;

            //定义一个命令返回值
            short error = 0;

            try
            {
                //清除标志
                error = GTN_ClrSts(axisRun.核号, axisRun.轴号, 1);
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
                result.IsSuccess = false;
                result.ErrorMsg = ex.Message;
                return result;
            }
            return OperationResult.CreateSuccessResult();
        }

        /// <summary>
        /// 多轴绝对定位
        /// </summary>
        /// <param name="axisRun">轴运行信息</param>
        /// <returns></returns>
        public OperationResult MultiaxisAbsoluteMoveAbs(List<AxisRun> listAxisRun, int mask = 0)
        {
            OperationResult result = OperationResult.CreateFailResult();

            //通用运行初始化验证
            for (int i = 0; i < listAxisRun.Count; i++)
            {
                result = CommonMotionValidate(listAxisRun[i].核号, listAxisRun[i].轴号);

                if (!result.IsSuccess) return result;
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
                result.IsSuccess = false;
                result.ErrorMsg = ex.Message;
                return result;
            }
            return OperationResult.CreateSuccessResult();
        }

        /// <summary>
        /// 点动JOG
        /// </summary>
        /// <param name="axisRun"></param>
        /// <returns></returns>
        public OperationResult JogMove(AxisRun axisRun)
        {

            //通用运行初始化验证

            OperationResult result = CommonMotionValidate(axisRun.核号, axisRun.轴号);

            if (!result.IsSuccess) return result;


            //定义一个命令返回值
            short error = 0;

            try
            {
                //设置为JOG模式
                error = GTN_PrfJog(axisRun.核号, axisRun.轴号);

                ErrorHandler("GTN_PrfJog", error);

                //创建一个TJogPrm对象
                TJogPrm jog;

                // 读取Jog运动参数
                error = GTN_GetJogPrm(axisRun.核号, axisRun.轴号, out jog);
                ErrorHandler("GTN_GetJogPrm", error);

                //给jog赋值
                jog.acc = axisRun.目标加速度;
                jog.dec = axisRun.目标减速度;
                jog.smooth = axisRun.Jog平滑时间;//0-1之间，0.8越接近1，越平滑

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
                result.IsSuccess = false;
                result.ErrorMsg = ex.Message;
                return result;
            }

            return OperationResult.CreateSuccessResult();
        }

        public OperationResult UniaxialHome(AxisHome axisHome)
        {
            //判断是否运动
            OperationResult result = CommonMotionValidate(axisHome.核号, axisHome.轴号);

            if (!result.IsSuccess) return result;

            try
            {
                //设置Smart Home回原点参数
                THomePrm tHomePrm;

                short error = GTN_GetHomePrm(axisHome.核号, axisHome.轴号, out tHomePrm);
                ErrorHandler($"GTN_GetHomePrm({axisHome.核号}, {axisHome.轴号}, {tHomePrm})", error);

                //int HOME_MODE_LIMIT = 10;  限位回原点
                //int HOME_MODE_LIMIT_HOME = 11;  *限位+Home回原点*
                //int HOME_MODE_LIMIT_INDEX = 12;  限位+Index回原点
                //int HOME_MODE_LIMIT_HOME_INDEX = 13;  限位+Home+Index回原点
                //int HOME_MODE_HOME = 20;  *Home回原点*
                //int HOME_MODE_HOME_INDEX = 22;  Home+Index回原点
                //int HOME_MODE_INDEX = 30;  Index回原点

                tHomePrm.mode = axisHome.模式;
                tHomePrm.moveDir = axisHome.Home搜索方向;
                //tHomePrm.indexDir = 1;
                tHomePrm.edge = axisHome.捕获触发沿;
                tHomePrm.velHigh = axisHome.搜索Home速度;
                //tHomePrm.velLow = 1;
                tHomePrm.acc = axisHome.加速度;
                tHomePrm.dec = axisHome.减速度;
                tHomePrm.searchHomeDistance = axisHome.Home最大搜索距离;
                //tHomePrm.searchIndexDistance = axisHome.Index最大搜索距离;
                tHomePrm.escapeStep = axisHome.反向脱离步长;

                //寻原点
                error = GTN_GoHome(axisHome.核号, axisHome.轴号, ref tHomePrm);//启动Smart Home回原点
                ErrorHandler($"GTN_GoHome({axisHome.核号}, {axisHome.轴号}, ref tHomePrm)", error);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMsg = ex.Message;
                return result;
            }
            return OperationResult.CreateSuccessResult();
        }

        /// <summary>
        /// 多轴回零
        /// </summary>
        /// <param name="ListAxisHome"></param>
        /// <returns></returns>
        public OperationResult MultiaxisHome(List<AxisHome> listAxisHome)
        {
            OperationResult result = OperationResult.CreateFailResult();

            //通用运行初始化验证
            for (int i = 0; i < listAxisHome.Count; i++)
            {
                result = CommonMotionValidate(listAxisHome[i].核号, listAxisHome[i].轴号);

                if (!result.IsSuccess) return result;
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
                    tHomePrm[i].moveDir = listAxisHome[i].Home搜索方向;
                    //tHomePrm[i].indexDir = listAxisHome[i].Index搜索方向;
                    tHomePrm[i].edge = listAxisHome[i].捕获触发沿;
                    tHomePrm[i].velHigh = listAxisHome[i].搜索Home速度;
                    //tHomePrm[i].velLow = listAxisHome[i].搜索Index速度;
                    tHomePrm[i].acc = listAxisHome[i].加速度;
                    tHomePrm[i].dec = listAxisHome[i].减速度;
                    tHomePrm[i].searchHomeDistance = listAxisHome[i].Home最大搜索距离;
                    //tHomePrm[i].searchIndexDistance = axisHome[i].Index最大搜索距离;
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
                result.IsSuccess = false;
                result.ErrorMsg = ex.Message;
                return result;
            }
            return OperationResult.CreateSuccessResult();
        }
    }
}
