using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IHardwareGXN.GXNCard;
using GEN_GTN;
using static GEN_GTN.mc;
using MotionCommon;
using IHardwareGXN;

namespace HardwareGXN
{
    public class GENAxisController : AbstractGXN, IGXNInterface
    {
        public override event Action<string> RunLog;

        public override string PrintGXN()
        {
            return "GEN轴卡";
        }

        public override string InitCard()
        {
            throw new NotImplementedException();
        }

        public override string CloseCard()
        {
            throw new NotImplementedException();
        }

        public override void ErrorHandler(string command, short error)
        {
            throw new NotImplementedException();
        }

        public override string IsMoving(short core, short axis, out bool ismove)
        {
            throw new NotImplementedException();
        }

        public override string IsEnable(short core, short axis, out bool isEnable)
        {
            throw new NotImplementedException();
        }

        public override string GetAxisStatus(short core, short axis, out int sts)
        {
            throw new NotImplementedException();
        }

        public override string GetAxisTerminalStatus(short core, short index)
        {
            throw new NotImplementedException();
        }

        public override string GetCmdEncPos(short core, short axis, out double pPos)
        {
            throw new NotImplementedException();
        }

        public override string GetCmdEncVel(short core, short axis, out double pPos)
        {
            throw new NotImplementedException();
        }

        public override string GetCmdPrfPos(short core, short axis, out int pPos)
        {
            throw new NotImplementedException();
        }

        public override string GetCmdPrfVel(short core, short axis, out double pPos)
        {
            throw new NotImplementedException();
        }

        public override string AllAxisOff()
        {
            throw new NotImplementedException();
        }

        public override string AllAxisStop()
        {
            throw new NotImplementedException();
        }

        public override string AxisClrSts(short core, short axis)
        {
            throw new NotImplementedException();
        }

        public override string AxisOn(short core, short axis, bool ison)
        {
            throw new NotImplementedException();
        }

        public override string AxisStop(short core, short axis, int isSmoothStop)
        {
            throw new NotImplementedException();
        }

        public override string AxisZeroPos(short core, short axis)
        {
            throw new NotImplementedException();
        }

        public override string UniaxialRelativeMove(AxisRun axisRun, out int posPulse)
        {
            throw new NotImplementedException();
        }

        public override string UniaxialAbsoluteMoveAbs(AxisRun axisRun)
        {
            throw new NotImplementedException();
        }

        public override string WaitAxisMove(AxisRun axisRun, int prfPos, int type = 0, int move = 0, int pos2 = 0)
        {
            throw new NotImplementedException();
        }

        public override string MultiaxisAbsoluteMoveAbs(List<AxisRun> listAxisRun)
        {
            throw new NotImplementedException();
        }

        public override string WaitAxisMove(List<AxisRun> axisRuns, List<int> prfPos, int type = 0, int move = 0, int pos2 = 0)
        {
            throw new NotImplementedException();
        }

        public override string JogMove(AxisRun axisRun)
        {
            throw new NotImplementedException();
        }

        public override string UniaxialHome(AxisHomeInfo axisHome)
        {
            throw new NotImplementedException();
        }

        public override string MultiaxisHome(List<AxisHomeInfo> listAxisHome)
        {
            throw new NotImplementedException();
        }

        public override string WaitAxisHome(AxisHomeInfo axisHome, int type = 0)
        {
            throw new NotImplementedException();
        }

        public override string WaitAxisHome(List<AxisHomeInfo> axisHomes, int type = 0)
        {
            throw new NotImplementedException();
        }

        public override string GLink_GetExtDi(short core, short diIndex, out string value)
        {
            throw new NotImplementedException();
        }

        public override string GLink_GetExtDO(short core, short doIndex, out string value)
        {
            throw new NotImplementedException();
        }

        public override string GLink_SetExtDoBit(short core, short doIndex, bool value)
        {
            throw new NotImplementedException();
        }

        public override string GNM601_GetDi(short core, short diType, out string value)
        {
            throw new NotImplementedException();
        }

        public override string GNM601_GetDiBit(short core, short diType, short diIndex, out short value)
        {
            throw new NotImplementedException();
        }

        public override string GNM601_GetDo(short core, short doType, out string value)
        {
            throw new NotImplementedException();
        }

        public override string GNM601_SetDoBit(short core, short doType, short doIndex, bool value)
        {
            throw new NotImplementedException();
        }
    }
}

