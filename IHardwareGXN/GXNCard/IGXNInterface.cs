using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MotionCommon;

namespace IHardwareGXN.GXNCard
{
    public interface IGXNInterface
    {
        /// <summary>
        /// 轴卡是否初始化成功
        /// </summary>
        bool InitedOK { get; set; }

        /// <summary>
        /// 日志输出事件
        /// </summary>
        event Action<string> RunLog;

        bool IsPause { get; set; }

        bool IsLoadPause { get; set; } //停止

        bool IsEmptyPause { get; set; } //停止

        bool IsForecePause { get; set; }

        //public event Action<LogLevel, string> OnLogWithLevel;

        string PrintGXN();

        /// <summary>
        /// 板卡初始化
        /// </summary>
        /// <returns>操作结果</returns>
        string InitCard();

        /// <summary>
        /// 关闭板卡
        /// </summary>
        /// <returns></returns>
        string CloseCard();

        /// <summary>
        /// 通用初始化验证
        /// </summary>
        /// <returns>操作结果</returns>
        string CommonInitedValidate();

        /// <summary>
        /// 通用运动初始化验证
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        string CommonMotionValidate(short core, short axis);

        /// <summary>
        /// 报警代码
        /// </summary>
        /// <param name="command"></param>
        /// <param name="error"></param>
        void ErrorHandler(string command, short error);

        #region 轴状态信息

        /// <summary>
        /// 判断某个轴是否运行
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        string IsMoving(short core, short axis, out bool ismove);

        /// <summary>
        /// 判断某个轴是否使能
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        string IsEnable(short core, short axis, out bool isEnable);

        /// <summary>
        /// 获取轴状态
        /// </summary>
        /// <returns></returns>
        string GetAxisStatus(short core, short axis, out int sts);

        /// <summary>
        /// 获取轴模块通讯状态
        /// </summary>
        /// <returns></returns>
        string GetAxisTerminalStatus(short core, short index); //out TTerminalStatus terminalStatus

        /// <summary>
        /// 获取实时位置
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <returns>实时位置</returns>
        string GetCmdEncPos(short core, short axis, out double pPos);

        /// <summary>
        /// 获取实时速度
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        string GetCmdEncVel(short core, short axis, out double pPos);

        /// <summary>
        /// 获取规划位置
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        string GetCmdPrfPos(short core, short axis, out int pPos);

        /// <summary>
        /// 获取规划速度
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        string GetCmdPrfVel(short core, short axis, out double pPos);

        #endregion

        #region 轴控制

        /// <summary>
        /// 所有轴下使能
        /// </summary>
        /// <returns></returns>
        string AllAxisOff();

        /// <summary>
        /// 所有轴停止运动
        /// </summary>
        /// <returns></returns>
        string AllAxisStop();

        /// <summary>
        /// 清除轴状态
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        string AxisClrSts(short core, short axis);

        /// <summary>
        /// 轴开启/关闭使能
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        string AxisOn(short core, short axis, bool ison);

        /// <summary>
        /// 轴停止运动
        /// </summary>
        /// <param name="core"></param>
        /// <param name="axis"></param>
        /// <param name="isSmoothStop">0:平滑停止 1:急停停止</param>
        /// <returns></returns>
        string AxisStop(short core, short axis, int isSmoothStop);

        /// <summary>
        /// 轴位置清零
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        string AxisZeroPos(short core, short axis);

        #endregion


        #region 轴动作

        /// <summary>
        /// 单轴相对定位
        /// </summary>
        /// <param name="axisRun">轴运行信息</param>
        /// <returns></returns>
        string UniaxialRelativeMove(AxisRun axisRun, out int posPulse);

        /// <summary>
        /// 单轴绝对定位
        /// </summary>
        /// <param name="axisRun">轴运行信息</param>
        /// <returns></returns>
        string UniaxialAbsoluteMoveAbs(AxisRun axisRun);

        /// <summary>
        /// 等待单轴移动
        /// </summary>
        /// <param name="axisRun"></param>
        /// <param name="prfPos"></param>
        /// <param name="type"></param>
        /// <param name="move"></param>
        /// <param name="pos2"></param>
        /// <returns></returns>
        string WaitAxisMove(AxisRun axisRun, int prfPos, int type = 0, int move = 0, int pos2 = 0);

        /// <summary>
        /// 多轴绝对定位
        /// </summary>
        /// <param name="axisRun">轴运行信息</param>
        /// <returns></returns>
        string MultiaxisAbsoluteMoveAbs(List<AxisRun> listAxisRun);

        /// <summary>
        /// 等待多轴移动
        /// </summary>
        /// <param name="axisRuns"></param>
        /// <param name="prfPos"></param>
        /// <param name="type"></param>
        /// <param name="move"></param>
        /// <param name="pos2"></param>
        /// <returns></returns>
        string WaitAxisMove(List<AxisRun> axisRuns, List<int> prfPos, int type = 0, int move = 0, int pos2 = 0);

        /// <summary>
        /// 点动JOG
        /// </summary>
        /// <param name="axisRun"></param>
        /// <returns></returns>
        string JogMove(AxisRun axisRun);

        /// <summary>
        /// 单轴回零
        /// </summary>
        /// <param name="axisHome">轴回零信息</param>
        /// <returns></returns>
        string UniaxialHome(AxisHomeInfo axisHome);

        /// <summary>
        /// 等待单轴回零完成
        /// </summary>
        /// <param name="axisHome"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        string WaitAxisHome(AxisHomeInfo axisHome, int type = 0);

        /// <summary>
        /// 多轴回零
        /// </summary>
        /// <param name="ListAxisHome"></param>
        /// <returns></returns>
        string MultiaxisHome(List<AxisHomeInfo> listAxisHome);

        /// <summary>
        /// 等待多轴回零完成
        /// </summary>
        /// <param name="axisHomes"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        string WaitAxisHome(List<AxisHomeInfo> axisHomes, int type = 0);

        #endregion

        #region gLink200模块 型号:HCB5-1616-DTD01

        /// <summary>
        /// 读取GlinK模块DI输入值
        /// </summary>
        /// <param name="core">核号</param>
        /// <param name="diIndex">IO索引,1-32767</param>
        /// <param name="value">32位二进制字符串</param>
        /// <returns></returns>
        string GLink_GetExtDi(short core, short diIndex, out string value);

        /// <summary>
        /// 读取GlinK模块DO输出值  备注：读取的用户设置值，而不是实际硬件输出值
        /// </summary>
        /// <param name="core">核号</param>
        /// <param name="doIndex">IO索引,1-32767</param>
        /// <param name="value">32位二进制字符串</param>
        /// <returns></returns>
        string GLink_GetExtDO(short core, short doIndex, out string value);

        /// <summary>
        /// 按位设置Glink模块DO输出值
        /// </summary>
        /// <param name="core">核号</param>
        /// <param name="doIndex">IO索引,1-32767</param>
        /// <param name="value">输出值:true亮/fasle灭</param>
        /// <returns></returns>
        string GLink_SetExtDoBit(short core, short doIndex, bool value);
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
        string GNM601_GetDi(short core, short diType, out string value);

        /// <summary>
        /// 按位读取数字IO输入状态
        /// </summary>
        /// <param name="core"></param>
        /// <param name="diType"></param>
        /// <param name="diIndex"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        string GNM601_GetDiBit(short core, short diType, short diIndex, out short value);

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
        string GNM601_GetDo(short core, short doType, out string value);

        /// <summary>
        /// 按位设置数字IO输出状态
        /// </summary>
        /// <param name="core"></param>
        /// <param name="doType"></param>
        /// <param name="doIndex"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        string GNM601_SetDoBit(short core, short doType, short doIndex, bool value);

        #endregion
    }

    //public enum LogLevel { INFO, DEBUG, WARNING, ERROR }
}
