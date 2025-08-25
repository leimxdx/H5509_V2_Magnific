using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IHardwareGXN.GXNCard;
using MotionCommon;

namespace IHardwareGXN.GXNCard
{
    public abstract class AbstractGXN : IGXNInterface
    {
        public bool InitedOK { get; set; }

        public abstract event Action<string> RunLog;

        public bool IsPause { get; set; }

        public bool IsLoadPause { get; set; }

        public bool IsEmptyPause { get; set; }

        public bool IsForecePause { get; set; }

        public abstract string PrintGXN();

        public abstract string InitCard();

        public abstract string CloseCard();

        public string CommonInitedValidate()
        {
            if (InitedOK == false) return "未初始化";
            else return "Success";
        }

        public string CommonMotionValidate(short core, short axis)
        {
            if (InitedOK == false) return "未初始化";

            string result = IsMoving(core, axis, out bool ismove);
            if (result == "Success")
            {
                if (ismove) return "正在运动";
                else return "Success";
            }
            else return result;
        }

        public abstract void ErrorHandler(string command, short error);

        #region 轴状态信息


        public abstract string IsMoving(short core, short axis, out bool ismove);

        public abstract string IsEnable(short core, short axis, out bool isEnable);

        public abstract string GetAxisStatus(short core, short axis, out int sts);

        public abstract string GetAxisTerminalStatus(short core, short index);

        public abstract string GetCmdEncPos(short core, short axis, out double pPos);

        public abstract string GetCmdEncVel(short core, short axis, out double pPos);

        public abstract string GetCmdPrfPos(short core, short axis, out int pPos);

        public abstract string GetCmdPrfVel(short core, short axis, out double pPos);

        #endregion

        #region 轴控制

        #endregion

        public abstract string AllAxisOff();

        public abstract string AllAxisStop();

        public abstract string AxisClrSts(short core, short axis);

        public abstract string AxisOn(short core, short axis, bool ison);

        public abstract string AxisStop(short core, short axis, int isSmoothStop);

        public abstract string AxisZeroPos(short core, short axis);

        #region 轴动作

        public abstract string UniaxialRelativeMove(AxisRun axisRun, out int posPulse);

        public abstract string UniaxialAbsoluteMoveAbs(AxisRun axisRun);

        public abstract string WaitAxisMove(AxisRun axisRun, int prfPos, int type = 0, int move = 0, int pos2 = 0);

        public abstract string MultiaxisAbsoluteMoveAbs(List<AxisRun> listAxisRun);

        public abstract string WaitAxisMove(List<AxisRun> axisRuns, List<int> prfPos, int type = 0, int move = 0, int pos2 = 0);

        public abstract string JogMove(AxisRun axisRun);

        public abstract string UniaxialHome(AxisHomeInfo axisHome);

        public abstract string MultiaxisHome(List<AxisHomeInfo> listAxisHome);

        public abstract string WaitAxisHome(AxisHomeInfo axisHome, int type = 0);

        public abstract string WaitAxisHome(List<AxisHomeInfo> axisHomes, int type = 0);

        #endregion

        #region gLink200模块 型号:HCB5-1616-DTD01

        public abstract string GLink_GetExtDi(short core, short diIndex, out string value);

        public abstract string GLink_GetExtDO(short core, short doIndex, out string value);

        public abstract string GLink_SetExtDoBit(short core, short doIndex, bool value);

        #endregion

        #region 网络型轴模块 型号:GNM-601-00

        public abstract string GNM601_GetDi(short core, short diType, out string value);

        public abstract string GNM601_GetDiBit(short core, short diType, short diIndex, out short value);

        public abstract string GNM601_GetDo(short core, short doType, out string value);

        public abstract string GNM601_SetDoBit(short core, short doType, short doIndex, bool value);

        #endregion

    }
}
