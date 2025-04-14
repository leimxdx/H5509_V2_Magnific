using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace H5509_V2
{
    public class Handler
    {
        private MotionEx motionEx = MotionEx.GetInstance();

        #region 轴移动
        public string 入料手臂移动(int xPos, int yPos, int pPos)
        {
            int[] pos = new int[] { xPos, yPos, pPos };
            string[] axisNames = new string[] { "M22_入料手臂X轴", "M16_入料手臂Y轴", "M5_入料手臂P轴" };
            return motionEx.AxisMove(axisNames, pos);
        }

        public string 出料手臂移动(int xPos, int yPos, int pPos)
        {
            int[] pos = new int[] { xPos, yPos, pPos };
            string[] axisNames = new string[] { "M23_出料手臂X轴", "M17_出料手臂Y轴", "M10_出料手臂P轴" };
            return motionEx.AxisMove(axisNames, pos);
        }

        public string 入料手臂Z移动(bool[] isopen, int pos, bool isdown)
        {
            List<string> axisNames = new List<string>();
            List<int> pPoss = new List<int>();

            if (isopen[0])
            {
                int zpos = isdown ? pos + 0 : -20; //加上硬件补偿
                pPoss.Add(zpos);
                axisNames.Add("M1_入料手臂Z1轴");
            }
            if (isopen[1])
            {
                int zpos = isdown ? pos + 0 : -20; //加上硬件补偿
                pPoss.Add(zpos);
                axisNames.Add("M2_入料手臂Z2轴");
            }
            if (isopen[2])
            {
                int zpos = isdown ? pos + 0 : -20; //加上硬件补偿
                pPoss.Add(zpos);
                axisNames.Add("M3_入料手臂Z3轴");
            }
            if (isopen[3])
            {
                int zpos = isdown ? pos + 0 : -20; //加上硬件补偿
                pPoss.Add(zpos);
                axisNames.Add("M4_入料手臂Z4轴");
            }

            return motionEx.AxisMove(axisNames.ToArray(), pPoss.ToArray());
        }

        public string 出料手臂Z移动(bool[] isopen, int pos, bool isdown)
        {
            List<string> axisNames = new List<string>();
            List<int> pPoss = new List<int>();

            if (isopen[0])
            {
                int zpos = isdown ? pos + 0 : -20; //加上硬件补偿
                pPoss.Add(zpos);
                axisNames.Add("M6_出料手臂Z1轴");
            }
            if (isopen[1])
            {
                int zpos = isdown ? pos + 0 : -20; //加上硬件补偿
                pPoss.Add(zpos);
                axisNames.Add("M7_出料手臂Z2轴");
            }
            if (isopen[2])
            {
                int zpos = isdown ? pos + 0 : -20; //加上硬件补偿
                pPoss.Add(zpos);
                axisNames.Add("M8_出料手臂Z3轴");
            }
            if (isopen[3])
            {
                int zpos = isdown ? pos + 0 : -20; //加上硬件补偿
                pPoss.Add(zpos);
                axisNames.Add("M9_出料手臂Z4轴");
            }

            return motionEx.AxisMove(axisNames.ToArray(), pPoss.ToArray());
        }

        public string 测试手臂X移动(int pos)
        {
            return motionEx.AxisMove("M21_测试手臂X轴", pos);
        }

        public string 测试手臂Z移动(int pos)
        {
            return motionEx.AxisMove("M19_测试手臂Z轴", pos);
        }

        public string 放Tary盘轴移动(int pos)
        {
            return motionEx.AxisMove("M18_放Tary盘轴", pos);
        }

        public string 飞梭轴移动(int pos)
        {
            return motionEx.AxisMove("M20_飞梭轴", pos);
        }

        public string Fix3放IC盘轴移动(int pos)
        {
            return motionEx.AxisMove("M11_Fix3放IC轴", pos);
        }

        public string Empty顶盘轴移动(int pos)
        {
            return motionEx.AxisMove("M12_Empty顶盘轴", pos);
        }

        public string Loader顶盘轴移动(int pos)
        {
            return motionEx.AxisMove("M13_Loader顶盘轴", pos);
        }

        public string Auto1顶盘轴移动(int pos)
        {
            return motionEx.AxisMove("M14_Auto1顶盘轴", pos);
        }

        public string Auto2顶盘轴移动(int pos)
        {
            return motionEx.AxisMove("M15_Auto2顶盘轴", pos);
        }

        #endregion 轴移动

        #region 轴回零

        public string 入料手臂X回零()
        {
            return motionEx.AxisHome("M22_入料手臂X轴");
        }

        public string 入料手臂Y回零()
        {
            return motionEx.AxisHome("M16_入料手臂Y轴");
        }

        public string 入料手臂P回零()
        {
            return motionEx.AxisHome("M5_入料手臂P轴");
        }

        public string 入料手臂回零()
        {
            string[] axisNames = { "M22_入料手臂X轴", "M16_入料手臂Y轴", "M5_入料手臂P轴" };
            return motionEx.AxisHome(axisNames);
        }

        public string 入料手臂Z回零(bool[] isopen)
        {
            List<string> axisHomes = new List<string>();

            if (isopen[0]) axisHomes.Add("M1_入料手臂Z1轴");
            if (isopen[1]) axisHomes.Add("M2_入料手臂Z2轴");
            if (isopen[2]) axisHomes.Add("M3_入料手臂Z3轴");
            if (isopen[3]) axisHomes.Add("M4_入料手臂Z4轴");

            return motionEx.AxisHome(axisHomes.ToArray());
        }

        public string 出料手臂X回零()
        {
            return motionEx.AxisHome("M23_出料手臂X轴");
        }

        public string 出料手臂Y回零()
        {
            return motionEx.AxisHome("M17_出料手臂Y轴");
        }

        public string 出料手臂P回零()
        {
            return motionEx.AxisHome("M10_出料手臂P轴");
        }

        public string 出料手臂回零()
        {
            string[] axisNames = { "M23_出料手臂X轴", "M17_出料手臂Y轴", "M10_出料手臂P轴" };
            return motionEx.AxisHome(axisNames);
        }

        public string 出料手臂Z回零(bool[] isopen)
        {
            List<string> axisHomes = new List<string>();

            if (isopen[0]) axisHomes.Add("M6_出料手臂Z1轴");
            if (isopen[1]) axisHomes.Add("M7_出料手臂Z2轴");
            if (isopen[2]) axisHomes.Add("M8_出料手臂Z3轴");
            if (isopen[3]) axisHomes.Add("M9_出料手臂Z4轴");

            return motionEx.AxisHome(axisHomes.ToArray());
        }

        public string 测试手臂X回零()
        {
            return motionEx.AxisHome("M21_测试手臂X轴");
        }

        public string 测试手臂Z回零()
        {
            return motionEx.AxisHome("M19_测试手臂Z轴");
        }

        public string Empty顶盘轴回零()
        {
            return motionEx.AxisHome("M12_Empty顶盘轴");
        }

        public string Loader顶盘轴回零()
        {
            return motionEx.AxisHome("M13_Loader顶盘轴");
        }

        public string Auto1顶盘轴回零()
        {
            return motionEx.AxisHome("M14_Auto1顶盘轴");
        }

        public string Auto2顶盘轴回零()
        {
            return motionEx.AxisHome("M15_Auto2顶盘轴");
        }

        public string 顶盘轴回零()
        {
            string[] axisNames = { "M12_Empty顶盘轴", "M13_Loader顶盘轴", "M14_Auto1顶盘轴", "M15_Auto2顶盘轴" };
            return motionEx.AxisHome(axisNames);
        }

        public string Fix3放IC轴回零()
        {
            return motionEx.AxisHome("M11_Fix3放IC轴");
        }

        public string 放Tary盘轴回零()
        {
            return motionEx.AxisHome("M18_放Tary盘轴");
        }

        public string 飞梭轴回零()
        {
            return motionEx.AxisHome("M20_飞梭轴");
        }

        #endregion 轴回零

        #region IO控制与读取

        public string 读取气缸输入信号(string ioName, string value, out long ms)
        {
            bool ispass = true;
            string isinplace = "";
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (isinplace != value)
            {
                Thread.Sleep(20);
                if (stopwatch.ElapsedMilliseconds > APPmaster.气缸动作超时时间)
                {
                    ispass = false;
                    break;
                }
                motionEx.GetDi(ioName, out isinplace);
            }
            ms = stopwatch.ElapsedMilliseconds;
            stopwatch.Stop();
            stopwatch.Reset();
            if (ispass) return "Success";
            else return "读取超时";
        }

        #region Y094_测试吸嘴张开气缸

        public string 测试吸嘴_张开气缸到位输出()
        {
            string result = motionEx.SetDo("Y094_测试吸嘴张开气缸", true);
            if (result != "Success")
            {
                Debug.WriteLine("测试吸嘴张开气缸到位输出失败！" + result);
                return result;
            }

            result = 读取气缸输入信号("X173_测试吸嘴张开气缸到位", "1", out long ms);
            if (result == "Success") Debug.WriteLine($"测试吸嘴张开气缸到位输出超时({ms}ms)");

            else Debug.WriteLine($"测试吸嘴张开气缸到位输出成功({ms}ms)");

            return result;
        }

        public string 测试吸嘴_张开气缸原位输出()
        {
            string result = motionEx.SetDo("Y094_测试吸嘴张开气缸", false);
            if (result != "Success")
            {
                Debug.WriteLine("测试吸嘴张开气缸原位输出失败！" + result);
                return result;
            }

            result = 读取气缸输入信号("X174_测试吸嘴张开气缸原位", "0", out long ms);

            if (result != "Success") Debug.WriteLine($"测试吸嘴张开气缸原位输出超时({ms}ms)");
            else Debug.WriteLine($"测试吸嘴张开气缸原位输出成功({ms}ms)");

            return result;
        }

        #endregion

        #region  Y050_移Tray上下气缸

        public string 移Tray上下气缸到位输出()
        {
            string result = motionEx.SetDo("Y050_移Tray上下气缸", true);
            if (result != "Success")
            {
                Debug.WriteLine("移Tray上下气缸到位输出失败！" + result);
                return result;
            }

            result = 读取气缸输入信号("X064_移Tray上下缸下感应", "1", out long ms);

            if (result == "Success") Debug.WriteLine($"移Tray上下气缸到位输出超时({ms}ms)");
            else Debug.WriteLine($"移Tray上下气缸到位输出成功({ms}ms)");

            return result;
        }

        public string 移Tray上下气缸原位输出()
        {
            string result = motionEx.SetDo("Y050_移Tray上下气缸", true);
            if (result != "Success")
            {
                Debug.WriteLine("移Tray上下气缸原位输出失败！" + result);
                return result;
            }

            result = 读取气缸输入信号("X065_移Tray上下缸上感应", "0", out long ms);

            if (result == "Success") Debug.WriteLine($"移Tray上下气缸原位输出超时({ms}ms)");
            else Debug.WriteLine($"移Tray上下气缸原位输出成功({ms}ms)");

            return result;
        }

        #endregion

        #region Y051_移Tray夹爪气缸

        public string 移Tray夹爪气缸到位输出()
        {
            string result = motionEx.SetDo("Y051_移Tray夹爪气缸", true);
            if (result != "Success")
            {
                Debug.WriteLine("移Tray夹爪气缸到位输出失败！" + result);
                return result;
            }

            result = 读取气缸输入信号("X067_移Tray夹紧缸夹紧感应", "1", out long ms);

            if (result == "Success") Debug.WriteLine($"移Tray夹爪气缸到位输出超时({ms}ms)");
            else Debug.WriteLine($"移Tray夹爪气缸到位输出成功({ms}ms)");

            return result;
        }

        public string 移Tray夹爪气缸原位输出()
        {
            string result = motionEx.SetDo("Y051_移Tray夹爪气缸", true);
            if (result != "Success")
            {
                Debug.WriteLine("移Tray夹爪气缸原位输出失败！" + result);
                return result;
            }

            result = 读取气缸输入信号("X066_移Tray夹紧缸松开感应", "0", out long ms);

            if (result == "Success") Debug.WriteLine($"移Tray夹爪气缸原位输出超时({ms}ms)");
            else Debug.WriteLine($"移Tray夹爪气缸原位输出成功({ms}ms)");

            return result;
        }

        #endregion

        #region 测试区位移和上下位移气缸

        public string 测试区_位移气缸到位输出(string yIoName, string xIoName)
        {
            string result = motionEx.SetDo(yIoName, true);
            if (result != "Success")
            {
                Debug.WriteLine($"{yIoName}到位输出失败！" + result);
                return result;
            }

            result = 读取气缸输入信号(xIoName, "1", out long ms);
            if (result == "Success") Debug.WriteLine($"{yIoName}到位输出超时({ms}ms)");
            else Debug.WriteLine($"{yIoName}到位输出成功({ms}ms)");
            return result;
        }

        public string 测试区_位移气缸原位输出(string yIoName, string xIoName)
        {
            string result = motionEx.SetDo(yIoName, false);
            if (result != "Success")
            {
                Debug.WriteLine($"{yIoName}原位输出失败！" + result);
                return result;
            }

            result = 读取气缸输入信号(xIoName, "0", out long ms);
            if (result == "Success") Debug.WriteLine($"{yIoName}原位输出超时({ms}ms)");
            else Debug.WriteLine($"{yIoName}原位输出成功({ms}ms)");
            return result;
        }

        public string 测试区_下压气缸到位输出(string yIoName, string xIoName)
        {
            string result = motionEx.SetDo(yIoName, true);
            if (result != "Success")
            {
                Debug.WriteLine($"{yIoName}到位输出失败！" + result);
                return result;
            }

            result = 读取气缸输入信号(xIoName, "1", out long ms);
            if (result == "Success") Debug.WriteLine($"{yIoName}到位输出超时({ms}ms)");
            else Debug.WriteLine($"{yIoName}到位输出成功({ms}ms)");
            return result;
        }

        public string 测试区_下压气缸原位输出(string yIoName, string xIoName)
        {
            string result = motionEx.SetDo(yIoName, false);
            if (result != "Success")
            {
                Debug.WriteLine($"{yIoName}原位输出失败！" + result);
                return result;
            }
            result = 读取气缸输入信号(xIoName, "0", out long ms);

            if (result == "Success") Debug.WriteLine($"{yIoName}原位输出超时({ms}ms)");
            else Debug.WriteLine($"{yIoName}原位输出成功({ms}ms)");
            return result;
        }

        public string 测试区_位移气缸到位输出(short site)
        {
            string yIoName;
            string xIoName;
            switch (site)
            {
                case 0: yIoName = "Y096_1_位移气缸"; xIoName = "X092_1_测试模组位移气缸后感应"; break;
                case 1: yIoName = "Y098_2_位移气缸"; xIoName = "X097_2_测试模组位移气缸后感应"; break;
                case 2: yIoName = "Y100_3_位移气缸"; xIoName = "X102_3_测试模组位移气缸后感应"; break;
                case 3: yIoName = "Y102_4_位移气缸"; xIoName = "X107_4_测试模组位移气缸后感应"; break;
                case 4: yIoName = "Y104_5_位移气缸"; xIoName = "X112_5_测试模组位移气缸后感应"; break;
                case 5: yIoName = "Y106_6_位移气缸"; xIoName = "X117_6_测试模组位移气缸后感应"; break;
                case 6: yIoName = "Y108_7_位移气缸"; xIoName = "X122_7_测试模组位移气缸后感应"; break;
                case 7: yIoName = "Y110_8_位移气缸"; xIoName = "X127_8_测试模组位移气缸后感应"; break;
                case 8: yIoName = "Y112_9_位移气缸"; xIoName = "X132_9_测试模组位移气缸后感应"; break;
                case 9: yIoName = "Y114_10_位移气缸"; xIoName = "X137_10_测试模组位移气缸后感应"; break;
                case 10: yIoName = "Y116_11_位移气缸"; xIoName = "X142_11_测试模组位移气缸后感应"; break;
                case 11: yIoName = "Y118_12_位移气缸"; xIoName = "X147_12_测试模组位移气缸后感应"; break;
                case 12: yIoName = "Y120_13_位移气缸"; xIoName = "X152_13_测试模组位移气缸后感应"; break;
                case 13: yIoName = "Y122_14_位移气缸"; xIoName = "X157_14_测试模组位移气缸后感应"; break;
                case 14: yIoName = "Y124_15_位移气缸"; xIoName = "X162_15_测试模组位移气缸后感应"; break;
                case 15: yIoName = "Y126_16_位移气缸"; xIoName = "X167_16_测试模组位移气缸后感应"; break;
                default: return $"site:{site}不存在";
            }
            return 测试区_位移气缸到位输出(yIoName, xIoName);
        }

        public string 测试区_位移气缸原位输出(short site)
        {
            string yIoName;
            string xIoName;
            switch (site)
            {
                case 0: yIoName = "Y096_1_位移气缸"; xIoName = "X093_1_测试模组位移气缸前感应"; break;
                case 1: yIoName = "Y098_2_位移气缸"; xIoName = "X098_2_测试模组位移气缸前感应"; break;
                case 2: yIoName = "Y100_3_位移气缸"; xIoName = "X103_3_测试模组位移气缸前感应"; break;
                case 3: yIoName = "Y102_4_位移气缸"; xIoName = "X108_4_测试模组位移气缸前感应"; break;
                case 4: yIoName = "Y104_5_位移气缸"; xIoName = "X113_5_测试模组位移气缸前感应"; break;
                case 5: yIoName = "Y106_6_位移气缸"; xIoName = "X118_6_测试模组位移气缸前感应"; break;
                case 6: yIoName = "Y108_7_位移气缸"; xIoName = "X123_7_测试模组位移气缸前感应"; break;
                case 7: yIoName = "Y110_8_位移气缸"; xIoName = "X128_8_测试模组位移气缸前感应"; break;
                case 8: yIoName = "Y112_9_位移气缸"; xIoName = "X133_9_测试模组位移气缸前感应"; break;
                case 9: yIoName = "Y114_10_位移气缸"; xIoName = "X138_10_测试模组位移气缸前感应"; break;
                case 10: yIoName = "Y116_11_位移气缸"; xIoName = "X143_11_测试模组位移气缸前感应"; break;
                case 11: yIoName = "Y118_12_位移气缸"; xIoName = "X148_12_测试模组位移气缸前感应"; break;
                case 12: yIoName = "Y120_13_位移气缸"; xIoName = "X153_13_测试模组位移气缸前感应"; break;
                case 13: yIoName = "Y122_14_位移气缸"; xIoName = "X158_14_测试模组位移气缸前感应"; break;
                case 14: yIoName = "Y124_15_位移气缸"; xIoName = "X163_15_测试模组位移气缸前感应"; break;
                case 15: yIoName = "Y126_16_位移气缸"; xIoName = "X168_16_测试模组位移气缸前感应"; break;
                default: return $"site:{site}不存在";
            }
            return 测试区_位移气缸原位输出(yIoName, xIoName);
        }

        public string 测试区_下压气缸到位输出(short site)
        {
            string yIoName;
            string xIoName;
            switch (site)
            {
                case 0: yIoName = "Y097_1_下压气缸"; xIoName = "X094_1_测试模组上升感应"; break;
                case 1: yIoName = "Y099_2_下压气缸"; xIoName = "X099_2_测试模组上升感应"; break;
                case 2: yIoName = "Y101_3_下压气缸"; xIoName = "X104_3_测试模组上升感应"; break;
                case 3: yIoName = "Y103_4_下压气缸"; xIoName = "X109_4_测试模组上升感应"; break;
                case 4: yIoName = "Y105_5_下压气缸"; xIoName = "X114_5_测试模组上升感应"; break;
                case 5: yIoName = "Y107_6_下压气缸"; xIoName = "X119_6_测试模组上升感应"; break;
                case 6: yIoName = "Y109_7_下压气缸"; xIoName = "X124_7_测试模组上升感应"; break;
                case 7: yIoName = "Y111_8_下压气缸"; xIoName = "X129_8_测试模组上升感应"; break;
                case 8: yIoName = "Y113_9_下压气缸"; xIoName = "X139_10_测试模组上升感应"; break;
                case 9: yIoName = "Y115_10_下压气缸"; xIoName = "X139_10_测试模组上升感应"; break;
                case 10: yIoName = "Y117_11_下压气缸"; xIoName = "X144_11_测试模组上升感应"; break;
                case 11: yIoName = "Y119_12_下压气缸"; xIoName = "X149_12_测试模组上升感应"; break;
                case 12: yIoName = "Y121_13_下压气缸"; xIoName = "X154_13_测试模组上升感应"; break;
                case 13: yIoName = "Y123_14_下压气缸"; xIoName = "X159_14_测试模组上升感应"; break;
                case 14: yIoName = "Y125_15_下压气缸"; xIoName = "X164_15_测试模组上升感应"; break;
                case 15: yIoName = "Y127_16_下压气缸"; xIoName = "X169_16_测试模组上升感应"; break;
                default: return $"site:{site}不存在";
            }
            return 测试区_下压气缸到位输出(yIoName, xIoName);
        }

        public string 测试区_下压气缸原位输出(short site)
        {
            string yIoName;
            string xIoName;
            switch (site)
            {
                case 0: yIoName = "Y097_1_下压气缸"; xIoName = "X095_1_测试模组下压感应"; break;
                case 1: yIoName = "Y099_2_下压气缸"; xIoName = "X100_2_测试模组下压感应"; break;
                case 2: yIoName = "Y101_3_下压气缸"; xIoName = "X105_3_测试模组下压感应"; break;
                case 3: yIoName = "Y103_4_下压气缸"; xIoName = "X110_4_测试模组下压感应"; break;
                case 4: yIoName = "Y105_5_下压气缸"; xIoName = "X115_5_测试模组下压感应"; break;
                case 5: yIoName = "Y107_6_下压气缸"; xIoName = "X120_6_测试模组下压感应"; break;
                case 6: yIoName = "Y109_7_下压气缸"; xIoName = "X125_7_测试模组下压感应"; break;
                case 7: yIoName = "Y111_8_下压气缸"; xIoName = "X130_8_测试模组下压感应"; break;
                case 8: yIoName = "Y113_9_下压气缸"; xIoName = "X135_9_测试模组下压感应"; break;
                case 9: yIoName = "Y115_10_下压气缸"; xIoName = "X140_10_测试模组下压感应"; break;
                case 10: yIoName = "Y117_11_下压气缸"; xIoName = "X145_11_测试模组下压感应"; break;
                case 11: yIoName = "Y119_12_下压气缸"; xIoName = "X150_12_测试模组下压感应"; break;
                case 12: yIoName = "Y121_13_下压气缸"; xIoName = "X155_13_测试模组下压感应"; break;
                case 13: yIoName = "Y123_14_下压气缸"; xIoName = "X160_14_测试模组下压感应"; break;
                case 14: yIoName = "Y125_15_下压气缸"; xIoName = "X165_15_测试模组下压感应"; break;
                case 15: yIoName = "Y127_16_下压气缸"; xIoName = "X170_16_测试模组下压感应"; break;
                default: return $"site:{site}不存在";
            }
            return 测试区_下压气缸原位输出(yIoName, xIoName);
        }

        #endregion

        #region 入料吸嘴

        public string 入料吸嘴_吸料(bool[] isopen)
        {
            List<bool> values = new List<bool>();
            List<string> ioNames = new List<string>();
            if (isopen[0])
            {
                values.Add(true);
                ioNames.Add("Y056_上料1_吸真空");
            }
            if (isopen[1])
            {
                values.Add(true);
                ioNames.Add("Y057_上料2_吸真空");
            }
            if (isopen[2])
            {
                values.Add(true);
                ioNames.Add("Y058_上料3_吸真空");
            }
            if (isopen[3])
            {
                values.Add(true);
                ioNames.Add("Y059_上料4_吸真空");
            }
            string result = motionEx.SetDo(ioNames.ToArray(), values.ToArray());
            if (result != "Success") Debug.WriteLine("入料吸嘴_吸真空IO输出失败!" + result);
            return result;
        }

        public string 入料吸嘴_放料(bool[] isopen)
        {
            List<bool> values = new List<bool>();
            List<string> ioNames = new List<string>();
            if (isopen[0])
            {
                values.Add(true);
                ioNames.Add("Y060_上料1_破真空");
            }
            if (isopen[1])
            {
                values.Add(true);
                ioNames.Add("Y061_上料2_破真空");
            }
            if (isopen[2])
            {
                values.Add(true);
                ioNames.Add("Y062_上料3_破真空");
            }
            if (isopen[3])
            {
                values.Add(true);
                ioNames.Add("Y063_上料4_破真空");
            }
            string result = motionEx.SetDo(ioNames.ToArray(), values.ToArray());
            if (result != "Success") Debug.WriteLine("入料吸嘴_破真空IO输出失败!" + result);
            return result;
        }

        public string 入料吸嘴_真空检测(out string[] iovalues)
        {
            List<string> ioNames = new List<string>();

            ioNames.Add("X068_IC上料_1_吸真空检测");
            ioNames.Add("X069_IC上料_2_吸真空检测");
            ioNames.Add("X070_IC上料_3_吸真空检测");
            ioNames.Add("X071_IC上料_4_吸真空检测");

            string result = motionEx.GetDi(ioNames.ToArray(), out iovalues);
            if (result != "Success") Debug.WriteLine("入料吸嘴_真空检测IO读取失败!" + result);
            return result;
        }

        #endregion

        #region 出料吸嘴
        public string 出料吸嘴_吸料(bool[] isopen)
        {
            List<bool> values = new List<bool>();
            List<string> ioNames = new List<string>();
            if (isopen[0])
            {
                values.Add(true);
                ioNames.Add("Y068_下料1_吸真空");
            }
            if (isopen[1])
            {
                values.Add(true);
                ioNames.Add("Y069_下料2_吸真空");
            }
            if (isopen[2])
            {
                values.Add(true);
                ioNames.Add("Y070_下料3_吸真空");
            }
            if (isopen[3])
            {
                values.Add(true);
                ioNames.Add("Y071_下料4_吸真空");
            }
            string result = motionEx.SetDo(ioNames.ToArray(), values.ToArray());
            if (result != "Success") Debug.WriteLine("出料吸嘴_吸真空IO输出失败!" + result);
            return result;
        }

        public string 出料吸嘴_放料(bool[] isopen)
        {
            List<bool> values = new List<bool>();
            List<string> ioNames = new List<string>();
            if (isopen[0])
            {
                values.Add(true);
                ioNames.Add("Y072_下料1_破真空");
            }
            if (isopen[1])
            {
                values.Add(true);
                ioNames.Add("Y073_下料2_破真空");
            }
            if (isopen[2])
            {
                values.Add(true);
                ioNames.Add("Y074_下料3_破真空");
            }
            if (isopen[3])
            {
                values.Add(true);
                ioNames.Add("Y075_下料4_破真空");
            }
            string result = motionEx.SetDo(ioNames.ToArray(), values.ToArray());
            if (result != "Success") Debug.WriteLine("出料吸嘴_破真空IO输出失败!" + result);
            return result;
        }

        public string 出料吸嘴_真空检测(out string[] iovalues)
        {
            List<string> ioNames = new List<string>();

            ioNames.Add("X072_IC下料_1_吸真空检测");
            ioNames.Add("X073_IC下料_2_吸真空检测");
            ioNames.Add("X074_IC下料_3_吸真空检测");
            ioNames.Add("X075_IC下料_4_吸真空检测");

            string result = motionEx.GetDi(ioNames.ToArray(), out iovalues);
            if (result != "Success") Debug.WriteLine("出料吸嘴_真空检测IO读取失败!" + result);
            return result;
        }

        #endregion

        #region 测试吸嘴

        public string 测试吸嘴_吸料(bool[] isopen)
        {
            List<bool> values = new List<bool>();
            List<string> ioNames = new List<string>();
            if (isopen[0])
            {
                values.Add(true);
                ioNames.Add("Y078_测试1_吸真空");
            }
            if (isopen[1])
            {
                values.Add(true);
                ioNames.Add("Y079_测试2_吸真空");
            }
            if (isopen[2])
            {
                values.Add(true);
                ioNames.Add("Y080_测试3_吸真空");
            }
            if (isopen[3])
            {
                values.Add(true);
                ioNames.Add("Y081_测试4_吸真空");
            }
            if (isopen[4])
            {
                values.Add(true);
                ioNames.Add("Y082_测试5_吸真空");
            }
            if (isopen[5])
            {
                values.Add(true);
                ioNames.Add("Y083_测试6_吸真空");
            }
            if (isopen[6])
            {
                values.Add(true);
                ioNames.Add("Y084_测试7_吸真空");
            }
            if (isopen[7])
            {
                values.Add(true);
                ioNames.Add("Y085_测试8_吸真空");
            }
            string result = motionEx.SetDo(ioNames.ToArray(), values.ToArray());
            if (result != "Success") Debug.WriteLine("测试吸嘴_吸真空IO输出失败!" + result);
            return result;
        }

        public string 测试吸嘴_放料(bool[] isopen)
        {
            List<bool> values = new List<bool>();
            List<string> ioNames = new List<string>();
            if (isopen[0])
            {
                values.Add(true);
                ioNames.Add("Y086_测试1_破真空");
            }
            if (isopen[1])
            {
                values.Add(true);
                ioNames.Add("Y087_测试2_破真空");
            }
            if (isopen[2])
            {
                values.Add(true);
                ioNames.Add("Y088_测试3_破真空");
            }
            if (isopen[3])
            {
                values.Add(true);
                ioNames.Add("Y089_测试4_破真空");
            }
            if (isopen[4])
            {
                values.Add(true);
                ioNames.Add("Y090_测试5_破真空");
            }
            if (isopen[5])
            {
                values.Add(true);
                ioNames.Add("Y091_测试6_破真空");
            }
            if (isopen[6])
            {
                values.Add(true);
                ioNames.Add("Y092_测试7_破真空");
            }
            if (isopen[7])
            {
                values.Add(true);
                ioNames.Add("Y093_测试8_破真空");
            }
            string result = motionEx.SetDo(ioNames.ToArray(), values.ToArray());
            if (result != "Success") Debug.WriteLine("测试吸嘴_破真空IO输出失败!" + result);
            return result;
        }

        public string 测试吸嘴_真空检测(out string[] iovalues)
        {
            List<string> ioNames = new List<string>();

            ioNames.Add("X084_测试1_吸真空检测");
            ioNames.Add("X085_测试2_吸真空检测");
            ioNames.Add("X086_测试3_吸真空检测");
            ioNames.Add("X087_测试4_吸真空检测");
            ioNames.Add("X088_测试5_吸真空检测");
            ioNames.Add("X089_测试6_吸真空检测");
            ioNames.Add("X090_测试7_吸真空检测");
            ioNames.Add("X091_测试8_吸真空检测");

            string result = motionEx.GetDi(ioNames.ToArray(), out iovalues);
            if (result != "Success") Debug.WriteLine("测试吸嘴_真空检测IO读取失败!" + result);
            return result;
        }

        #endregion

        #region 飞梭前后检测产品感应

        public string 飞梭_前后检测产品感应(out string[] iovalues)
        {
            List<string> ioNames = new List<string>();

            ioNames.Add("X076_飞梭后检测产品感应1");
            ioNames.Add("X077_飞梭后检测产品感应2");
            ioNames.Add("X078_飞梭后检测产品感应3");
            ioNames.Add("X079_飞梭后检测产品感应4");
            ioNames.Add("X080_飞梭前检测产品感应1");
            ioNames.Add("X081_飞梭前检测产品感应2");
            ioNames.Add("X082_飞梭前检测产品感应3");
            ioNames.Add("X083_飞梭前检测产品感应4");

            string result = motionEx.GetDi(ioNames.ToArray(), out iovalues);
            if (result != "Success") Debug.WriteLine("飞梭前后_检测产品感应IO读取失败!" + result);
            return result;
        }

        #endregion

        #region 测试区左右对射感应

        public string 测试区_左右对射感应(short site, out string[] iovalues)
        {
            string[] ioNames;
            iovalues = null;
            switch (site)
            {
                case 0: case 1: case 2: case 3: ioNames = new string[] { "X048_1_1_模组左右对射感应", "X049_1_2_模组左右对射感应", "X050_1_3_模组左右对射感应", "X051_1_4_模组左右对射感应" }; break;
                case 4: case 5: case 6: case 7: ioNames = new string[] { "X052_2_1_模组左右对射感应", "X053_2_2_模组左右对射感应", "X054_2_3_模组左右对射感应", "X055_2_4_模组左右对射感应" }; break;
                case 8: case 9: case 10: case 11: ioNames = new string[] { "X056_3_1_模组左右对射感应", "X057_3_2_模组左右对射感应", "X058_3_3_模组左右对射感应", "X059_3_4_模组左右对射感应" }; break;
                case 12: case 13: case 14: case 15: ioNames = new string[] { "X060_4_1_模组左右对射感应", "X061_4_2_模组左右对射感应", "X062_4_3_模组左右对射感应", "X063_4_4_模组左右对射感应" }; break;
                default: return $"site:{site}不存在";
            }

            string result = motionEx.GetDi(ioNames, out iovalues);
            if (result != "Success") Debug.WriteLine("测试区左右对射感应IO读取失败!" + result);
            return result;
        }

        public string 测试区_上下对射感应(short site, out string[] iovalues)
        {
            string[] ioNames;
            iovalues = null;
            switch (site)
            {
                case 0: ioNames = new string[] { "X096_1_1_测试模组对射感应", "X176_1_2测试模组对射感应" }; break;
                case 1: ioNames = new string[] { "X101_2_1_测试模组对射感应", "X177_2_2测试模组对射感应" }; break;
                case 2: ioNames = new string[] { "X106_3_1_测试模组对射感应", "X178_3_2测试模组对射感应" }; break;
                case 3: ioNames = new string[] { "X111_4_1_测试模组对射感应", "X179_4_2测试模组对射感应" }; break;
                case 4: ioNames = new string[] { "X116_5_1_测试模组对射感应", "X180_5_2测试模组对射感应" }; break;
                case 5: ioNames = new string[] { "X121_6_1_测试模组对射感应", "X181_6_2测试模组对射感应" }; break;
                case 6: ioNames = new string[] { "X126_7_1_测试模组对射感应", "X182_7_2测试模组对射感应" }; break;
                case 7: ioNames = new string[] { "X131_8_1_测试模组对射感应", "X183_8_2测试模组对射感应" }; break;
                case 8: ioNames = new string[] { "X136_9_1_测试模组对射感应", "X184_9_2测试模组对射感应" }; break;
                case 9: ioNames = new string[] { "X141_10_1_测试模组对射感应", "X185_10_2测试模组对射感应" }; break;
                case 10: ioNames = new string[] { "X146_11_1_测试模组对射感应", "X186_11_2测试模组对射感应" }; break;
                case 11: ioNames = new string[] { "X151_12_1_测试模组对射感应", "X187_12_2测试模组对射感应" }; break;
                case 12: ioNames = new string[] { "X156_13_1_测试模组对射感应", "X188_13_2测试模组对射感应" }; break;
                case 13: ioNames = new string[] { "X161_14_1_测试模组对射感应", "X189_14_2测试模组对射感应" }; break;
                case 14: ioNames = new string[] { "X166_15_1_测试模组对射感应", "X190_15_2测试模组对射感应" }; break;
                case 15: ioNames = new string[] { "X171_16_1_测试模组对射感应", "X191_16_2测试模组对射感应" }; break;
                default: return $"site:{site}不存在";
            }

            string result = motionEx.GetDi(ioNames, out iovalues);
            if (result != "Success") Debug.WriteLine("测试区上下对射感应IO读取失败!" + result);
            return result;
        }

        #endregion

        #region 加热模块

        public string 加热盘加热开启()
        {
            string result = motionEx.SetDo("Y095_加热盘加热", true);
            if (result != "Success")
            {
                Debug.WriteLine("加热盘加热开启输出失败！" + result);
                return result;
            }
            Thread.Sleep(300); //没有感应器，固定延时处理
            return result;
        }

        public string 加热盘加热关闭()
        {
            string result = motionEx.SetDo("Y095_加热盘加热", false);
            if (result != "Success")
            {
                Debug.WriteLine("加热盘加热关闭输出失败！" + result);
                return result;
            }
            Thread.Sleep(300); //没有感应器，固定延时处理
            return result;
        }

        public string 测试区_模块加热(short site, bool value)
        {
            string yIoName;
            switch (site)
            {
                case 0: yIoName = "Y128_1_模组加热"; break;
                case 1: yIoName = "Y129_2_模组加热"; break;
                case 2: yIoName = "Y130_3_模组加热"; break;
                case 3: yIoName = "Y131_4_模组加热"; break;
                case 4: yIoName = "Y132_5_模组加热"; break;
                case 5: yIoName = "Y133_6_模组加热"; break;
                case 6: yIoName = "Y134_7_模组加热"; break;
                case 7: yIoName = "Y135_8_模组加热"; break;
                case 8: yIoName = "Y136_9_模组加热"; break;
                case 9: yIoName = "Y137_10_模组加热"; break;
                case 10: yIoName = "Y138_11_模组加热"; break;
                case 11: yIoName = "Y139_12_模组加热"; break;
                case 12: yIoName = "Y140_13_模组加热"; break;
                case 13: yIoName = "Y141_14_模组加热"; break;
                case 14: yIoName = "Y142_15_模组加热"; break;
                case 15: yIoName = "Y143_16_模组加热"; break;
                default: return $"site:{site}不存在";
            }
            string result = motionEx.SetDo(yIoName, value);
            if (result != "Success") Debug.WriteLine($"Site:{site + 1:D2}加热IO输出失败!" + result);
            return result;
        }

        #endregion

        #region Empty区

        public string Empty_前Tray存在感应(out string value)
        {
            string result = motionEx.GetDi("X016_空Tray区对射1_前", out value);
            if (result != "Success") Debug.WriteLine("X016_空Tray区对射1_前IO读取失败!" + result);
            return result;
        }

        public string Empty_后Tray放歪感应(out string value)
        {
            string result = motionEx.GetDi("X017_空Tray区对射2_后", out value);
            if (result != "Success") Debug.WriteLine("X017_空Tray区对射2_后IO读取失败!" + result);
            return result;
        }

        public string Empty_门感应(out string value)
        {
            string result = motionEx.GetDi("X018_空Tray门感应", out value);
            if (result != "Success") Debug.WriteLine("X018_空Tray门感应IO读取失败!" + result);
            return result;
        }

        #endregion

        #region Load区

        public string Load_Tray前侧定位气缸到位输出()
        {
            string result = motionEx.SetDo("Y018_Load区Tray前侧定位", true);
            if (result != "Success")
            {
                Debug.WriteLine("Load区Tray前侧定位气缸到位输出失败！" + result);
                return result;
            }
            Thread.Sleep(300); //没有感应器，固定延时处理
            return result;
        }

        public string Load_Tray前侧定位气缸原位输出()
        {
            string result = motionEx.SetDo("Y018_Load区Tray前侧定位", false);
            if (result != "Success")
            {
                Debug.WriteLine("Load区Tray前侧定位气缸原位输出失败！" + result);
                return result;
            }
            Thread.Sleep(300); //没有感应器，固定延时处理
            return result;
        }

        public string Load_前Tray存在感应(out string value)
        {
            string result = motionEx.GetDi("X022_Load区对射1_前", out value);
            if (result != "Success") Debug.WriteLine("X022_Load区对射1_前IO读取失败!" + result);
            return result;
        }

        public string Load_后Tray放歪感应(out string value)
        {
            string result = motionEx.GetDi("X023_Load区对射1_后", out value);
            if (result != "Success") Debug.WriteLine("X023_Load区对射1_后IO读取失败!" + result);
            return result;
        }

        public string Load_门感应(out string value)
        {
            string result = motionEx.GetDi("X024_Load门感应", out value);
            if (result != "Success") Debug.WriteLine("X024_Load门感应IO读取失败!" + result);
            return result;
        }

        #endregion

        #region Auto1区

        public string Auto1_Tray前定位气缸到位输出()
        {
            string result = motionEx.SetDo("Y023_BIN_A区Tray前定位", true);
            if (result != "Success")
            {
                Debug.WriteLine("BIN_A区Tray前定位气缸到位输出失败！" + result);
                return result;
            }
            Thread.Sleep(300); //没有感应器，固定延时处理
            return result;
        }

        public string Auto1_Tray前定位气缸原位输出()
        {
            string result = motionEx.SetDo("Y023_BIN_A区Tray前定位", false);
            if (result != "Success")
            {
                Debug.WriteLine("BIN_A区Tray前定位气缸原位输出失败！" + result);
                return result;
            }
            Thread.Sleep(300); //没有感应器，固定延时处理
            return result;
        }

        public string Auto1_Tray侧定位气缸到位输出()
        {
            string result = motionEx.SetDo("Y024_BIN_A区Tray侧定位", true);
            if (result != "Success")
            {
                Debug.WriteLine("BIN_A区Tray侧定位气缸到位输出失败！" + result);
                return result;
            }
            Thread.Sleep(300); //没有感应器，固定延时处理
            return result;
        }

        public string Auto1_Tray侧定位气缸原位输出()
        {
            string result = motionEx.SetDo("Y024_BIN_A区Tray侧定位", false);
            if (result != "Success")
            {
                Debug.WriteLine("BIN_A区Tray侧定位气缸原位输出失败！" + result);
                return result;
            }
            Thread.Sleep(300); //没有感应器，固定延时处理
            return result;
        }

        public string Auto1_Tray上顶气缸到位输出()
        {
            string result = motionEx.SetDo("Y027_BIN_A区Tray上顶", true);
            if (result != "Success")
            {
                Debug.WriteLine("BIN_A区Tray上顶气缸到位输出失败！" + result);
                return result;
            }
            Thread.Sleep(300); //没有感应器，固定延时处理
            return result;
        }

        public string Auto1_Tray上顶气缸原位输出()
        {
            string result = motionEx.SetDo("Y027_BIN_A区Tray上顶", false);
            if (result != "Success")
            {
                Debug.WriteLine("BIN_A区Tray上顶气缸原位输出失败！" + result);
                return result;
            }
            Thread.Sleep(300); //没有感应器，固定延时处理
            return result;
        }

        public string Auto1_前Tray存在感应(out string value)
        {
            string result = motionEx.GetDi("X034_BIN_A区对射1_前", out value);
            if (result != "Success") Debug.WriteLine("X034_Auto1对射1_前IO读取失败!" + result);
            return result;
        }

        public string Auto1_后Tray放歪感应(out string value)
        {
            string result = motionEx.GetDi("X035_BIN_A区对射2_后", out value);
            if (result != "Success") Debug.WriteLine("X035_Auto1对射2_后IO读取失败!" + result);
            return result;
        }

        public string Auto1_门感应(out string value)
        {
            string result = motionEx.GetDi("X036_BIN_A门感应", out value);
            if (result != "Success") Debug.WriteLine("X036_Auto1门感应IO读取失败!" + result);
            return result;
        }

        #endregion

        #region Auto2

        public string Auto2_Tray前定位气缸到位输出()
        {
            string result = motionEx.SetDo("Y025_BIN_B区Tray前定位", true);
            if (result != "Success")
            {
                Debug.WriteLine("BIN_B区Tray前定位气缸到位输出失败！" + result);
                return result;
            }
            Thread.Sleep(300); //没有感应器，固定延时处理
            return result;
        }

        public string Auto2_Tray前定位气缸原位输出()
        {
            string result = motionEx.SetDo("Y025_BIN_B区Tray前定位", false);
            if (result != "Success")
            {
                Debug.WriteLine("BIN_B区Tray前定位气缸原位输出失败！" + result);
                return result;
            }
            Thread.Sleep(300); //没有感应器，固定延时处理
            return result;
        }

        public string Auto2_Tray侧定位气缸到位输出()
        {
            string result = motionEx.SetDo("Y026_BIN_B区Tray侧定位", true);
            if (result != "Success")
            {
                Debug.WriteLine("BIN_B区Tray侧定位气缸到位输出失败！" + result);
                return result;
            }
            Thread.Sleep(300); //没有感应器，固定延时处理
            return result;
        }

        public string Auto2_Tray侧定位气缸原位输出()
        {
            string result = motionEx.SetDo("Y026_BIN_B区Tray侧定位", false);
            if (result != "Success")
            {
                Debug.WriteLine("BIN_B区Tray侧定位气缸原位输出失败！" + result);
                return result;
            }
            Thread.Sleep(300); //没有感应器，固定延时处理
            return result;
        }

        public string Auto2_Tray上顶气缸到位输出()
        {
            string result = motionEx.SetDo("Y032_BIN_B区Tray上顶", true);
            if (result != "Success")
            {
                Debug.WriteLine("BIN_B区Tray上顶气缸到位输出失败！" + result);
                return result;
            }
            Thread.Sleep(300); //没有感应器，固定延时处理
            return result;
        }

        public string Auto2_Tray上顶气缸原位输出()
        {
            string result = motionEx.SetDo("Y032_BIN_B区Tray上顶", false);
            if (result != "Success")
            {
                Debug.WriteLine("BIN_B区Tray上顶气缸原位输出失败！" + result);
                return result;
            }
            Thread.Sleep(300); //没有感应器，固定延时处理
            return result;
        }

        public string Auto2_前Tray存在感应(out string value)
        {
            string result = motionEx.GetDi("X040_BIN_B区对射1_前", out value);
            if (result != "Success") Debug.WriteLine("X040_Auto2对射1_前IO读取失败!" + result);
            return result;
        }

        public string Auto2_后Tray放歪感应(out string value)
        {
            string result = motionEx.GetDi("X041_BIN_B区对射2_后", out value);
            if (result != "Success") Debug.WriteLine("X041_Auto2对射2_后IO读取失败!" + result);
            return result;
        }

        public string Auto2_门感应(out string value)
        {
            string result = motionEx.GetDi("X042_BIN_B门感应", out value);
            if (result != "Success") Debug.WriteLine("X042_Auto2门感应IO读取失败!" + result);
            return result;
        }

        #endregion

        public string Fix1_Tray感应(out string value)
        {
            string result = motionEx.GetDi("X046_BIN3区Tray感应", out value);
            if (result != "Success") Debug.WriteLine("X046_Fix1_Tray感应IO读取失败!" + result);
            return result;
        }

        public string Fix2_Tray感应(out string value)
        {
            string result = motionEx.GetDi("X047_BIN4区Tray感应", out value);
            if (result != "Success") Debug.WriteLine("X047_Fix2_Tray感应IO读取失败!" + result);
            return result;
        }

        public string Fix3_Tray感应(out string value)
        {
            string result = motionEx.GetDi("X045_BIN5区Tray感应", out value);
            if (result != "Success") Debug.WriteLine("X045_Fix3_Tray感应IO读取失败!" + result);
            return result;
        }

        #endregion IO控制与读取


        #region 动作流程

        public static int Data_入料手臂吸料state;
        public static ST_ProcessManage PS_入料手臂吸料; 

        public void 入料手臂吸料_Load()
        {
            PS_入料手臂吸料.StepID = 500;

            while (true)
            {
                switch (PS_入料手臂吸料.StepID)
                {
                    case 500:

                        PS_入料手臂吸料.StepID = 510;
                        break;
                    case 510:
                        PS_入料手臂吸料.StepID = 525;
                        break;
                    case 525:
                        PS_入料手臂吸料.StepID = 530;
                        break;
                    case 530:
                        PS_入料手臂吸料.StepID = 535;
                        break;
                    case 535:
                        PS_入料手臂吸料.StepID = 540;
                        break;
                    case 540:
                        PS_入料手臂吸料.StepID = 545;
                        break;
                    case 545:
                        PS_入料手臂吸料.StepID = 550;
                        break;
                    case 550:
                        PS_入料手臂吸料.StepID = 555;
                        break;
                    case 555:
                        PS_入料手臂吸料.StepID = 560;
                        break;
                    case 560:
                        PS_入料手臂吸料.StepID = 565;
                        break;
                    case 565:
                        PS_入料手臂吸料.StepID = 570;
                        break;
                    case 570:
                        PS_入料手臂吸料.StepID = 599;
                        break;
                    default:
                        Debug.WriteLine(("PS_入料手臂吸料.StepID =" + PS_入料手臂吸料.StepID.ToString() + "对应流程不存在"));
                        break;
                }
            }
        }

        #endregion
    }
}
