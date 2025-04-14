using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H5509_V2
{
    public class APPmaster
    {
        /// <summary>
        /// 板卡初始成功标志
        /// </summary>
        public static bool InitedOK = false;

        public static List<IOInfo> IOList;

        public static List<AxisInfo> AxisInfoList;

        public static List<AxisRunInfo> AxisRunInfoList;

        public static List<AxisHomeInfo> AxisHomeInfoList;

        public static bool IsPause = false; //停止

        public static bool IsForecePause = false; //强制停止

        public static int 气缸动作超时时间 = 3000; //ms

        public static void GetAxisCore(string axisname, out short core, out short axis)
        {
            switch (axisname)
            {
                case "M1_入料手臂Z1轴": core = 1; axis = 1; break;
                case "M2_入料手臂Z2轴": core = 1; axis = 2; break;
                case "M3_入料手臂Z3轴": core = 1; axis = 3; break;
                case "M4_入料手臂Z4轴": core = 1; axis = 4; break;
                case "M5_入料首手臂P轴": core = 1; axis = 5; break;
                case "M6_出料手臂Z1轴": core = 1; axis = 6; break;
                case "M7_出料手臂Z2轴": core = 1; axis = 7; break;
                case "M8_出料手臂Z3轴": core = 1; axis = 8; break;
                case "M9_出料手臂Z4轴": core = 1; axis = 9; break;
                case "M10_出料手臂P轴": core = 1; axis = 10; break;
                case "M11_Fix3放IC轴": core = 1; axis = 11; break;
                case "M12_Empty顶盘轴": core = 1; axis = 12; break;
                case "M13_Loader顶盘轴": core = 2; axis = 1; break;
                case "M14_Auto1顶盘轴": core = 2; axis = 2; break;
                case "M15_Auto2顶盘轴": core = 2; axis = 3; break;
                case "M16_入料手臂Y轴": core = 2; axis = 4; break;
                case "M17_出料手臂Y轴": core = 2; axis = 5; break;
                case "M18_放Tary盘轴": core = 2; axis = 6; break;
                case "M19_测试手臂Z轴": core = 2; axis = 7; break;
                case "M20_飞梭轴": core = 2; axis = 8; break;
                case "M21_测试手臂X轴": core = 2; axis = 9; break;
                case "M22_入料手臂X轴": core = 2; axis = 10; break;
                case "M23_出料手臂X轴": core = 2; axis = 11; break;
                default: core = 0; axis = 0; break;
            }
        }

    }
}
