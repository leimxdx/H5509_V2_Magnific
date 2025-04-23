using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using Microsoft.SqlServer.Server;
using Sunny.UI;
using Sunny.UI.Win32;
using static System.Windows.Forms.AxHost;
using static H5509_V2.APPmaster;

namespace H5509_V2
{
    public class Handler
    {
        public MotionEx motionEx = MotionEx.GetInstance();

        #region 轴移动
        public string 入料手臂X移动(double pos)
        {
            return motionEx.AxisMove("M22_入料手臂X轴", pos);
        }

        public string 入料手臂Y移动(double pos)
        {
            return motionEx.AxisMove("M16_入料手臂Y轴", pos);
        }

        public string 入料手臂P移动(double pos)
        {
            return motionEx.AxisMove("M5_入料手臂P轴", pos);
        }

        public string 入料手臂移动(double xPos, double yPos, double pPos)
        {
            double[] pos = new double[] { xPos, yPos, pPos };
            string[] axisNames = new string[] { "M22_入料手臂X轴", "M16_入料手臂Y轴", "M5_入料手臂P轴" };
            return motionEx.AxisMove(axisNames, pos);
        }

        public string 出料手臂X移动(double pos)
        {
            return motionEx.AxisMove("M23_出料手臂X轴", pos);
        }

        public string 出料手臂Y移动(double pos)
        {
            return motionEx.AxisMove("M17_出料手臂Y轴", pos);
        }

        public string 出料手臂P移动(double pos)
        {
            return motionEx.AxisMove("M10_出料手臂P轴", pos);
        }

        public string 出料手臂移动(double xPos, double yPos, double pPos)
        {
            double[] pos = new double[] { xPos, yPos, pPos };
            string[] axisNames = new string[] { "M23_出料手臂X轴", "M17_出料手臂Y轴", "M10_出料手臂P轴" };
            return motionEx.AxisMove(axisNames, pos);
        }

        public string 入料手臂Z移动(bool[] isopen, double pos, bool isdown)
        {
            List<string> axisNames = new List<string>();
            List<double> pPoss = new List<double>();

            if (isopen[0])
            {
                double zpos = isdown ? pos + 0 : pos; //加上硬件补偿
                pPoss.Add(zpos);
                axisNames.Add("M1_入料手臂Z1轴");
            }
            if (isopen[1])
            {
                double zpos = isdown ? pos + 0 : pos; //加上硬件补偿
                pPoss.Add(zpos);
                axisNames.Add("M2_入料手臂Z2轴");
            }
            if (isopen[2])
            {
                double zpos = isdown ? pos + 0 : pos; //加上硬件补偿
                pPoss.Add(zpos);
                axisNames.Add("M3_入料手臂Z3轴");
            }
            if (isopen[3])
            {
                double zpos = isdown ? pos + 0 : pos; //加上硬件补偿
                pPoss.Add(zpos);
                axisNames.Add("M4_入料手臂Z4轴");
            }

            return motionEx.AxisMove(axisNames.ToArray(), pPoss.ToArray());
        }

        public string 入料手臂Z移动(bool[] isopen, bool isdown)
        {
            List<string> axisNames = new List<string>();
            List<double> pPoss = new List<double>();

            double pos;
            if (isdown) pos = 1000;
            else pos = -0.2;

            if (isopen[0])
            {
                double zpos = isdown ? pos + 0 : pos; //加上硬件补偿
                pPoss.Add(zpos);
                axisNames.Add("M1_入料手臂Z1轴");
            }
            if (isopen[1])
            {
                double zpos = isdown ? pos + 0 : pos; //加上硬件补偿
                pPoss.Add(zpos);
                axisNames.Add("M2_入料手臂Z2轴");
            }
            if (isopen[2])
            {
                double zpos = isdown ? pos + 0 : pos; //加上硬件补偿
                pPoss.Add(zpos);
                axisNames.Add("M3_入料手臂Z3轴");
            }
            if (isopen[3])
            {
                double zpos = isdown ? pos + 0 : pos; //加上硬件补偿
                pPoss.Add(zpos);
                axisNames.Add("M4_入料手臂Z4轴");
            }

            return motionEx.AxisMove(axisNames.ToArray(), pPoss.ToArray());
        }

        public string 出料手臂Z移动(bool[] isopen, double pos, bool isdown)
        {
            List<string> axisNames = new List<string>();
            List<double> pPoss = new List<double>();

            if (isopen[0])
            {
                double zpos = isdown ? pos + 0 : pos; //加上硬件补偿
                pPoss.Add(zpos);
                axisNames.Add("M6_出料手臂Z1轴");
            }
            if (isopen[1])
            {
                double zpos = isdown ? pos + 0 : pos; //加上硬件补偿
                pPoss.Add(zpos);
                axisNames.Add("M7_出料手臂Z2轴");
            }
            if (isopen[2])
            {
                double zpos = isdown ? pos + 0 : pos; //加上硬件补偿
                pPoss.Add(zpos);
                axisNames.Add("M8_出料手臂Z3轴");
            }
            if (isopen[3])
            {
                double zpos = isdown ? pos + 0 : pos; //加上硬件补偿
                pPoss.Add(zpos);
                axisNames.Add("M9_出料手臂Z4轴");
            }

            return motionEx.AxisMove(axisNames.ToArray(), pPoss.ToArray());
        }

        public string 测试手臂X移动(double pos)
        {

            return motionEx.AxisMove("M21_测试手臂X轴", pos);
        }

        public string 测试手臂Z移动(bool isdown)
        {
            return motionEx.AxisMove("M19_测试手臂Z轴", isdown ? 100 : -0.2);
        }

        public string 放Tary盘轴移动(double pos)
        {
            return motionEx.AxisMove("M18_放Tary盘轴", pos);
        }

        public string 飞梭轴移动(double pos)
        {
            return motionEx.AxisMove("M20_飞梭轴", pos);
        }

        public string Fix3放IC盘轴移动(double pos)
        {
            return motionEx.AxisMove("M11_Fix3放IC轴", pos);
        }

        public string Empty顶盘轴移动(double pos)
        {
            return motionEx.AxisMove("M12_Empty顶盘轴", pos);
        }

        public string Loader顶盘轴移动(double pos)
        {
            return motionEx.AxisMove("M13_Loader顶盘轴", pos);
        }

        public string Auto1顶盘轴移动(double pos)
        {
            return motionEx.AxisMove("M14_Auto1顶盘轴", pos);
        }

        public string Auto2顶盘轴移动(double pos)
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

        public string 测试吸嘴_张开气缸输出(bool value)
        {
            string result = motionEx.SetDo("Y094_测试吸嘴张开气缸", value);
            if (result != "Success") Debug.WriteLine($"测试吸嘴张开气缸{(value ? "到位" : "原位")}输出失败！" + result);
            return result;
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

            return 测试吸嘴张开气缸到位输入读取();
        }
        public string 测试吸嘴_张开气缸原位输出()
        {
            string result = motionEx.SetDo("Y094_测试吸嘴张开气缸", false);
            if (result != "Success")
            {
                Debug.WriteLine("测试吸嘴张开气缸原位输出失败！" + result);
                return result;
            }

            return 测试吸嘴张开气缸原位输入读取();
        }

        public string 测试吸嘴张开气缸到位输入读取()
        {
            string result = 读取气缸输入信号("X173_测试吸嘴张开气缸到位", "1", out long ms);
            if (result == "Success") Debug.WriteLine($"测试吸嘴张开气缸到位输出超时({ms}ms)");

            else Debug.WriteLine($"测试吸嘴张开气缸到位输出成功({ms}ms)");

            return result;
        }


        public string 测试吸嘴张开气缸原位输入读取()
        {
            string result = 读取气缸输入信号("X174_测试吸嘴张开气缸原位", "0", out long ms);

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

        /// <summary>
        /// 入料吸嘴_吸放料
        /// </summary>
        /// <param name="isopen"></param>
        /// <param name="ison">true:吸真空 false:破真空</param>
        /// <param name="value">true:开吸真空/开破真空 false:关吸真空/关破真空</param>
        /// <returns></returns>
        public string 入料吸嘴_吸放料(bool[] isopen, bool ison, bool value)
        {
            List<bool> znozzle = new List<bool>();
            List<string> ioNames = new List<string>();
            if (isopen[0])
            {
                znozzle.Add(value);
                if (ison) ioNames.Add("Y056_上料1_吸真空");
                else ioNames.Add("Y060_上料1_破真空");
            }
            if (isopen[1])
            {
                znozzle.Add(value);
                if (ison) ioNames.Add("Y057_上料2_吸真空");
                else ioNames.Add("Y061_上料2_破真空");
            }
            if (isopen[2])
            {
                znozzle.Add(value);
                if (ison) ioNames.Add("Y058_上料3_吸真空");
                else ioNames.Add("Y062_上料3_破真空");
            }
            if (isopen[3])
            {
                znozzle.Add(value);
                if (ison) ioNames.Add("Y059_上料4_吸真空");
                else ioNames.Add("Y063_上料4_破真空");
            }
            string result = motionEx.SetDo(ioNames.ToArray(), znozzle.ToArray());
            if (result != "Success") Debug.WriteLine("入料吸嘴_吸真空IO输出失败!" + result);
            return result;
        }

        public string 入料吸嘴_吸料(bool[] isopen, bool ison)
        {
            List<bool> values = new List<bool>();
            List<string> ioNames = new List<string>();
            if (isopen[0])
            {
                values.Add(ison);
                ioNames.Add("Y056_上料1_吸真空");
            }
            if (isopen[1])
            {
                values.Add(ison);
                ioNames.Add("Y057_上料2_吸真空");
            }
            if (isopen[2])
            {
                values.Add(ison);
                ioNames.Add("Y058_上料3_吸真空");
            }
            if (isopen[3])
            {
                values.Add(ison);
                ioNames.Add("Y059_上料4_吸真空");
            }
            string result = motionEx.SetDo(ioNames.ToArray(), values.ToArray());
            if (result != "Success") Debug.WriteLine("入料吸嘴_吸真空IO输出失败!" + result);
            return result;
        }

        public string 入料吸嘴_放料(bool[] isopen, bool ison)
        {
            List<bool> values = new List<bool>();
            List<string> ioNames = new List<string>();
            if (isopen[0])
            {
                values.Add(ison);
                ioNames.Add("Y060_上料1_破真空");
            }
            if (isopen[1])
            {
                values.Add(ison);
                ioNames.Add("Y061_上料2_破真空");
            }
            if (isopen[2])
            {
                values.Add(ison);
                ioNames.Add("Y062_上料3_破真空");
            }
            if (isopen[3])
            {
                values.Add(ison);
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

        public string 入料吸嘴_回原检测(out string[] home)
        {
            string[] axisNames =
            {
                "M1_入料手臂Z1轴", "M2_入料手臂Z2轴",
                "M3_入料手臂Z3轴", "M4_入料手臂Z4轴"
            };
            string result = motionEx.GetDiHome(axisNames, out home);

            if (result != "Success")
            {
                Debug.WriteLine($"入料Z回原：{home[0]} {home[1]} {home[2]} {home[3]}");
                return result;
            }
            return "Success";
        }

        #endregion

        #region 出料吸嘴

        public string 出料吸嘴_吸放料(bool[] isopen, bool ison, bool value)
        {
            List<bool> znozzle = new List<bool>();
            List<string> ioNames = new List<string>();
            if (isopen[0])
            {
                znozzle.Add(value);
                if (ison) ioNames.Add("Y068_下料1_吸真空");
                else ioNames.Add("Y072_下料1_破真空");
            }
            if (isopen[1])
            {
                znozzle.Add(value);
                if (ison) ioNames.Add("Y069_下料2_吸真空");
                else ioNames.Add("Y073_下料2_破真空");
            }
            if (isopen[2])
            {
                znozzle.Add(value);
                if (ison) ioNames.Add("Y070_下料3_吸真空");
                else ioNames.Add("Y074_下料3_破真空");
            }
            if (isopen[3])
            {
                znozzle.Add(value);
                if (ison) ioNames.Add("Y071_下料4_吸真空");
                else ioNames.Add("Y075_下料4_破真空");
            }

            string result = motionEx.SetDo(ioNames.ToArray(), znozzle.ToArray());
            if (result != "Success") Debug.WriteLine($"出料吸嘴_吸放料({isopen},{ison})" + result);
            return result;
        }

        public string 出料吸嘴_吸料(bool[] isopen, bool ison)
        {
            List<bool> values = new List<bool>();
            List<string> ioNames = new List<string>();
            if (isopen[0])
            {
                values.Add(ison);
                ioNames.Add("Y068_下料1_吸真空");
            }
            if (isopen[1])
            {
                values.Add(ison);
                ioNames.Add("Y069_下料2_吸真空");
            }
            if (isopen[2])
            {
                values.Add(ison);
                ioNames.Add("Y070_下料3_吸真空");
            }
            if (isopen[3])
            {
                values.Add(ison);
                ioNames.Add("Y071_下料4_吸真空");
            }
            string result = motionEx.SetDo(ioNames.ToArray(), values.ToArray());
            if (result != "Success") Debug.WriteLine("出料吸嘴_吸真空IO输出失败!" + result);
            return result;
        }

        public string 出料吸嘴_放料(bool[] isopen, bool ison)
        {
            List<bool> values = new List<bool>();
            List<string> ioNames = new List<string>();
            if (isopen[0])
            {
                values.Add(ison);
                ioNames.Add("Y072_下料1_破真空");
            }
            if (isopen[1])
            {
                values.Add(ison);
                ioNames.Add("Y073_下料2_破真空");
            }
            if (isopen[2])
            {
                values.Add(ison);
                ioNames.Add("Y074_下料3_破真空");
            }
            if (isopen[3])
            {
                values.Add(ison);
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

        public string 出料吸嘴_回原检测(out string[] home)
        {
            string[] axisNames =
            {
                "M6_出料手臂Z1轴", "M7_出料手臂Z2轴",
                "M8_出料手臂Z3轴", "M9_出料手臂Z4轴"
            };
            string result = motionEx.GetDiHome(axisNames, out home);

            if (result != "Success")
            {
                Debug.WriteLine($"出料Z回原：{home[0]} {home[1]} {home[2]} {home[3]}");
                return result;
            }
            return "Success";
        }

        #endregion

        #region 测试吸嘴
        /// <summary>
        /// 
        /// </summary>
        /// <param name="isopen"></param>
        /// <param name="ison">true:吸真空 false:破真空</param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string 测试吸嘴_吸放料(bool[] isopen, bool ison, bool value)
        {
            List<bool> znozzle = new List<bool>();
            List<string> ioNames = new List<string>();
            if (isopen[0])
            {
                znozzle.Add(value);
                if (ison) ioNames.Add("Y078_测试1_吸真空");
                else ioNames.Add("Y086_测试1_破真空");
            }
            if (isopen[1])
            {
                znozzle.Add(value);
                if (ison) ioNames.Add("Y079_测试2_吸真空");
                else ioNames.Add("Y087_测试2_破真空");
            }
            if (isopen[2])
            {
                znozzle.Add(value);
                if (ison) ioNames.Add("Y080_测试3_吸真空");
                else ioNames.Add("Y088_测试3_破真空");
            }
            if (isopen[3])
            {
                znozzle.Add(value);
                if (ison) ioNames.Add("Y081_测试4_吸真空");
                else ioNames.Add("Y089_测试4_破真空");
            }
            if (isopen[4])
            {
                znozzle.Add(value);
                if (ison) ioNames.Add("Y082_测试5_吸真空");
                else ioNames.Add("Y090_测试5_破真空");
            }
            if (isopen[5])
            {
                znozzle.Add(value);
                if (ison) ioNames.Add("Y083_测试6_吸真空");
                else ioNames.Add("Y091_测试6_破真空");
            }
            if (isopen[6])
            {
                znozzle.Add(value);
                if (ison) ioNames.Add("Y084_测试7_吸真空");
                else ioNames.Add("Y092_测试7_破真空");
            }
            if (isopen[7])
            {
                znozzle.Add(value);
                if (ison) ioNames.Add("Y085_测试8_吸真空");
                else ioNames.Add("Y093_测试8_破真空");
            }

            string result = motionEx.SetDo(ioNames.ToArray(), znozzle.ToArray());
            if (result != "Success") Debug.WriteLine("测试吸嘴_吸真空IO输出失败!" + result);
            return result;
        }

        public string 测试吸嘴_吸料(bool[] isopen, bool ison)
        {
            List<bool> values = new List<bool>();
            List<string> ioNames = new List<string>();
            if (isopen[0])
            {
                values.Add(ison);
                ioNames.Add("Y078_测试1_吸真空");
            }
            if (isopen[1])
            {
                values.Add(ison);
                ioNames.Add("Y079_测试2_吸真空");
            }
            if (isopen[2])
            {
                values.Add(ison);
                ioNames.Add("Y080_测试3_吸真空");
            }
            if (isopen[3])
            {
                values.Add(ison);
                ioNames.Add("Y081_测试4_吸真空");
            }
            if (isopen[4])
            {
                values.Add(ison);
                ioNames.Add("Y082_测试5_吸真空");
            }
            if (isopen[5])
            {
                values.Add(ison);
                ioNames.Add("Y083_测试6_吸真空");
            }
            if (isopen[6])
            {
                values.Add(ison);
                ioNames.Add("Y084_测试7_吸真空");
            }
            if (isopen[7])
            {
                values.Add(ison);
                ioNames.Add("Y085_测试8_吸真空");
            }
            string result = motionEx.SetDo(ioNames.ToArray(), values.ToArray());
            if (result != "Success") Debug.WriteLine("测试吸嘴_吸真空IO输出失败!" + result);
            return result;
        }

        public string 测试吸嘴_放料(bool[] isopen, bool ison)
        {
            List<bool> values = new List<bool>();
            List<string> ioNames = new List<string>();
            if (isopen[0])
            {
                values.Add(ison);
                ioNames.Add("Y086_测试1_破真空");
            }
            if (isopen[1])
            {
                values.Add(ison);
                ioNames.Add("Y087_测试2_破真空");
            }
            if (isopen[2])
            {
                values.Add(ison);
                ioNames.Add("Y088_测试3_破真空");
            }
            if (isopen[3])
            {
                values.Add(ison);
                ioNames.Add("Y089_测试4_破真空");
            }
            if (isopen[4])
            {
                values.Add(ison);
                ioNames.Add("Y090_测试5_破真空");
            }
            if (isopen[5])
            {
                values.Add(ison);
                ioNames.Add("Y091_测试6_破真空");
            }
            if (isopen[6])
            {
                values.Add(ison);
                ioNames.Add("Y092_测试7_破真空");
            }
            if (isopen[7])
            {
                values.Add(ison);
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

        public string 测试吸嘴_回原检测(out string[] home)
        {
            string[] axisNames = { "M19_测试手臂Z轴" };

            string result = motionEx.GetDiHome(axisNames, out home);

            if (result != "Success")
            {
                Debug.WriteLine($"测试Z回原：{home[0]} ");
                return result;
            }
            return "Success";
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

        public string DebugIO(out string[] iovalues)
        {
            List<string> ioNames = new List<string>();

            ioNames.Add("X016_空Tray区对射1_前");
            ioNames.Add("X099_2_测试模组上升感应");
            ioNames.Add("X161_14_1_测试模组对射感应");
            ioNames.Add("Y0199_2_下压气缸");
            //ioNames.Add("X080_飞梭前检测产品感应1");
            //ioNames.Add("X081_飞梭前检测产品感应2");
            //ioNames.Add("X082_飞梭前检测产品感应3");
            //ioNames.Add("X083_飞梭前检测产品感应4");

            string result = motionEx.GetDi(ioNames.ToArray(), out iovalues);
            if (result != "Success") Debug.WriteLine("飞梭前后_检测产品感应IO读取失败!" + result);
            return result;
        }

        #endregion IO控制与读取


        #region 动作流程
        public static bool 结批标记;

        public static short 入料手臂吸料状态;
        public static bool 入料手臂抓料等待标记;

        public static short 飞梭移动状态;
        public static short 飞梭前入料状态;
        public static short 飞梭前出料状态;
        public static short 飞梭后入料状态;
        public static short 飞梭后出料状态;

        public static bool[][] 飞梭前入料IC;
        public static bool[][] 飞梭前出料IC;
        public static bool[][] 飞梭后入料IC;
        public static bool[][] 飞梭后出料IC;

        public static bool[][] 放料IC目标值;

        public static string[][] 飞梭前出料BIN项;
        public static string[][] 飞梭后出料BIN项;

        public static ST_ProcessManage PS入料手臂吸放料流程;

        public static ST_ProcessManage PS出料手臂吸放料流程;

        public static ST_ProcessManage PS测试手臂取待测料流程;

        public static ST_ProcessManage PS测试手臂取已测料流程;

        public static ST_ProcessManage PS测试区气缸下压动作流程;

        public static ST_ProcessManage PS测试区气缸上抬动作流程;

        public static ST_ProcessManage PS入料手臂放料流程;

        public static ST_ProcessManage PS入飞梭移动流程;

        public static ST_ProcessManage PS整机回零流程;

        public static int 飞梭2位置 = 0; //0:后面 1:前面

        public static int[] SiteTestState = new int[16];

        /// <summary>
        /// 检测沿信号
        /// </summary>
        /// <param name="current">当前值</param>
        /// <param name="cache">缓存值</param>
        /// <param name="isRiseOrFall">上升沿或下降沿，True为上升沿，False为下降沿</param>
        /// <returns>是否检测到沿信号</returns>
        public bool CheckEdgeSingal(bool current, ref bool cache, bool isRiseOrFall = true)
        {
            if (isRiseOrFall)
            {
                if (current && !cache)
                {
                    cache = current;
                    return true;
                }
                else
                {
                    cache = current;
                    return false;
                }
            }
            else
            {
                if (!current && cache)
                {
                    cache = current;
                    return true;
                }
                else
                {
                    cache = current;
                    return false;
                }
            }
        }

        #region 入料手臂流程

        public void 参数初始化()
        {
            飞梭前入料IC = new bool[][] { new bool[] { false, false, false, false }, new bool[] { false, false, false, false } };
            飞梭前出料IC = new bool[][] { new bool[] { true, true, true, true }, new bool[] { true, true, true, true } };
            飞梭后入料IC = new bool[][] { new bool[] { false, false, false, false }, new bool[] { false, false, false, false } };
            飞梭后出料IC = new bool[][] { new bool[] { false, false, false, false }, new bool[] { false, false, false, false } };

            飞梭前出料状态 = 1;
            放料IC目标值 = new bool[][] { new bool[] { true, true, true, true }, new bool[] { true, true, true, true } };
            飞梭前出料BIN项 = new string[][] { new string[] { "01", "01", "01", "01" }, new string[] { "01", "01", "01", "01" } };
        }

        private bool 飞梭是否放置IC(bool[][] bools)
        {
            if (bools[0].Contains(false)) return true;
            else if (bools[1].Contains(false)) return true;
            else return false;
        }

        private int[] 吸嘴是否吸料(bool[] open)
        {
            int[] opens = new int[4];
            opens[0] = open[0] ? 0 : 2;
            opens[1] = open[1] ? 0 : 2;
            opens[2] = open[2] ? 0 : 2;
            opens[3] = open[3] ? 0 : 2;
            return opens;
        }

        private short[] 吸嘴是否放料(bool[][] open, short colums)
        {
            short[] opens = new short[4];
            opens[0] = (short)(open[colums][0] ? 1 : 0);
            opens[1] = (short)(open[colums][0] ? 1 : 0);
            opens[2] = (short)(open[colums][0] ? 1 : 0);
            opens[3] = (short)(open[colums][0] ? 1 : 0);
            return opens;
        }

        private bool 参数对比(bool[] 实际值, bool[] 目标值)
        {
            for (int i = 0; i < 实际值.Length; i++)
            {
                if (目标值[i] == false) break; //如果不需要放
                if (实际值[i] != 目标值[i]) return false;
            }
            return true;
        }

        private bool[] 获取放料IC(bool[] 实际值, bool[] 目标值)
        {
            bool[] open = new bool[实际值.Length];
            for (int i = 0; i < 实际值.Length; i++)
            {
                if (目标值[i])
                {
                    if (!实际值[i])
                    {
                        open[i] = true;
                    }
                }
            }
            return open;
        }

        private void 交换飞梭前后入料IC数据()
        {
            bool[][] temp = new bool[飞梭前入料IC.Length][];
            for (int i = 0; i < 飞梭前入料IC.Length; i++)
            {
                temp[i] = new bool[飞梭前入料IC[i].Length];
                Array.Copy(飞梭前入料IC[i], temp[i], 飞梭前入料IC[i].Length);
            }

            飞梭前入料IC = new bool[飞梭后入料IC.Length][];
            for (int i = 0; i < 飞梭后入料IC.Length; i++)
            {
                飞梭前入料IC[i] = new bool[飞梭后入料IC[i].Length];
                Array.Copy(飞梭后入料IC[i], 飞梭前入料IC[i], 飞梭后入料IC[i].Length);
            }

            飞梭后入料IC = new bool[temp.Length][];
            for (int i = 0; i < temp.Length; i++)
            {
                飞梭后入料IC[i] = new bool[temp[i].Length];
                Array.Copy(temp[i], 飞梭后入料IC[i], temp[i].Length);
            }
        }

        private void 交换飞梭前后出料IC数据()
        {
            飞梭前出料IC = new bool[飞梭后出料IC.Length][];
            for (int i = 0; i < 飞梭后出料IC.Length; i++)
            {
                飞梭前出料IC[i] = new bool[飞梭后出料IC[i].Length];
                Array.Copy(飞梭后出料IC[i], 飞梭前出料IC[i], 飞梭后出料IC[i].Length);
            }

            飞梭后出料IC = new bool[2][] { new bool[] { false, false, false, false }, new bool[] { false, false, false, false } };
        }

        private void 交换飞梭前后出料BIN数据()
        {
            string[][] temp = new string[飞梭前出料BIN项.Length][];
            for (int i = 0; i < 飞梭前出料BIN项.Length; i++)
            {
                temp[i] = new string[飞梭前出料BIN项[i].Length];
                Array.Copy(飞梭前出料BIN项[i], temp[i], 飞梭前出料BIN项[i].Length);
            }

            飞梭前出料BIN项 = new string[飞梭后出料BIN项.Length][];
            for (int i = 0; i < 飞梭后出料BIN项.Length; i++)
            {
                飞梭前出料BIN项[i] = new string[飞梭后出料BIN项[i].Length];
                Array.Copy(飞梭后出料BIN项[i], 飞梭前出料BIN项[i], 飞梭后出料BIN项[i].Length);
            }

            飞梭后出料BIN项 = new string[temp.Length][];
            for (int i = 0; i < temp.Length; i++)
            {
                飞梭后出料BIN项[i] = new string[temp[i].Length];
                Array.Copy(temp[i], 飞梭后出料BIN项[i], temp[i].Length);
            }
        }

        #endregion

        public string 入料手臂依次回零()
        {
            string result = 入料手臂Y回零();
            if (result != "Success") return result;

            result = 入料手臂P回零();
            if (result != "Success") return result;

            result = 入料手臂X回零();
            if (result != "Success") return result;

            return "Success";
        }

        public string 出料手臂依次回零()
        {
            string result = 出料手臂Y回零();
            if (result != "Success") return result;

            result = 出料手臂P回零();
            if (result != "Success") return result;

            result = 出料手臂X回零();
            if (result != "Success") return result;

            return "Success";
        }

        public string 整机回零流程()
        {
            //所有Z先回零
            string one = 入料手臂Z回零(new bool[] { true, true, true, true });
            string one1 = 出料手臂Z回零(new bool[] { true, true, true, true });
            string one2 = 测试手臂Z回零();
            if (one == "Success" && one1 == "Success" && one2 == "Success")
            {
                string two = 入料手臂依次回零();
                string two1 = 出料手臂依次回零();
                string two2 = 飞梭轴回零();
                string two3 = 放Tary盘轴回零();
                if (two == "Success" && two1 == "Success" && two2 == "Success" && two3 == "Success")
                {
                    string three = 放Tary盘轴回零();
                    if (three == "Success")
                    {
                        return "Success";
                    }
                    else return three;
                }
                else
                {
                    if (two != "Success") return two;
                    else if (two1 != "Success") return two1;
                    else if (two2 != "Success") return two2;
                    else return two3;
                }
            }
            else
            {
                if (one != "Success") return one;
                else if (one1 != "Success") return one1;
                else return one2;
            }
        }

        public void 入料手臂吸放料流程1()
        {
            string result;
            short xPoint = 0;
            short yPoint = 0;
            short colums = 0;
            bool[] open = new bool[4];
            int[] opens = new int[4];
            bool[] lsopen = new bool[4];
            int index = 0;
            double xPos = 0;
            double yPos = 0;
            string[] home = new string[4];
            string[] ioval = new string[4];
            PS入料手臂吸放料流程.StepID = 500;
            while (true)
            {
                Thread.Sleep(5);
                switch (PS入料手臂吸放料流程.StepID)
                {
                    case 500:
                        PS入料手臂吸放料流程.StepName = "路径规划";
                        PS入料手臂吸放料流程.Condition = !结批标记;
                        PS入料手臂吸放料流程.Permit = !PS入料手臂放料流程.Processing;
                        if (PS入料手臂吸放料流程.Condition && PS入料手臂吸放料流程.Permit)
                        {
                            PS入料手臂吸放料流程.Processing = true;
                            PS入料手臂吸放料流程.StepID = 505;
                        }
                        break;

                    case 505:
                        PS入料手臂吸放料流程.StepName = "计算放料IC颗数";
                        if (!参数对比(飞梭前入料IC[0], 放料IC目标值[0])) //飞梭移动状态 == 1
                        {
                            if (入料手臂抓料等待标记)
                            {
                                PS入料手臂吸放料流程.StepID = 710;
                            }
                            else
                            {
                                open = 获取放料IC(飞梭前入料IC[0], 放料IC目标值[0]);
                                if (open.Contains(true))
                                {
                                    colums = 0;
                                    opens = 吸嘴是否吸料(open);
                                    入料手臂抓料等待标记 = false;
                                    PS入料手臂吸放料流程.StepID = 515;
                                }
                            }
                        }
                        else if (!参数对比(飞梭前入料IC[1], 放料IC目标值[1])) //飞梭移动状态 == 1
                        {
                            if (入料手臂抓料等待标记)
                            {
                                PS入料手臂吸放料流程.StepID = 710;
                            }
                            else
                            {
                                open = 获取放料IC(飞梭前入料IC[1], 放料IC目标值[1]);
                                if (open.Contains(true))
                                {
                                    colums = 1;
                                    opens = 吸嘴是否吸料(open);
                                    入料手臂抓料等待标记 = false;
                                    PS入料手臂吸放料流程.StepID = 515;
                                }
                            }
                        }
                        else
                        {
                            if (!入料手臂抓料等待标记)
                            {
                                飞梭前入料状态 = 1; //入料手臂目标达成
                                PS入料手臂吸放料流程.StepID = 510;
                            }
                            else Thread.Sleep(10);
                        }
                        break;

                    case 510:
                        PS入料手臂吸放料流程.StepName = "计算抓料标记";
                        if (参数对比(飞梭后入料IC[0], 放料IC目标值[0]) || 参数对比(飞梭后入料IC[1], 放料IC目标值[1]))
                        {
                            Thread.Sleep(10);
                        }
                        else
                        {
                            if (!参数对比(飞梭后入料IC[0], 放料IC目标值[0]))
                            {
                                open = 获取放料IC(飞梭后入料IC[0], 放料IC目标值[0]);
                                if (open.Contains(true))
                                {
                                    colums = 0;
                                    opens = 吸嘴是否吸料(open);
                                    入料手臂抓料等待标记 = true;
                                    PS入料手臂吸放料流程.StepID = 515;
                                }
                            }
                            else if (!参数对比(飞梭后入料IC[1], 放料IC目标值[1]))
                            {
                                open = 获取放料IC(飞梭后入料IC[1], 放料IC目标值[1]);
                                if (open.Contains(true))
                                {
                                    colums = 1;
                                    opens = 吸嘴是否吸料(open);
                                    入料手臂抓料等待标记 = true;
                                    PS入料手臂吸放料流程.StepID = 515;
                                }
                            }
                        }

                        break;

                    case 515:
                        PS入料手臂吸放料流程.StepName = "获取Load盘坐标";
                        bool 坐标 = APPmaster.获取Load盘坐标(true, out xPoint, out yPoint);
                        if (坐标)
                        {
                            PS入料手臂吸放料流程.StepID = 520;
                        }
                        else
                        {
                            Debug.WriteLine("进入换盘程序");
                        }
                        break;

                    case 520:
                        PS入料手臂吸放料流程.StepName = "获取吸料信息";
                        result = APPmaster.获取Load盘Open(opens, xPoint, yPoint, out lsopen, out index, out xPos, out yPos);
                        if (result == "Success")
                        {
                            PS入料手臂吸放料流程.StepID = 525;
                        }
                        break;

                    case 525:
                        PS入料手臂吸放料流程.StepName = "XYP轴移动到Load吸料位置";
                        result = 入料手臂移动(xPos, yPos, 10); //写去哪里吸料，至于去哪个位置吸，走多少由APPmaster决定
                        //if (result == "Success")
                        {
                            PS入料手臂吸放料流程.StepID = 530;
                        }
                        break;

                    case 530:
                        PS入料手臂吸放料流程.StepName = "入料手臂Z向下移动";
                        result = 入料手臂Z移动(lsopen, true);
                        //if (result == "Success")
                        {
                            PS入料手臂吸放料流程.StepID = 535;
                        }
                        break;

                    case 535:
                        PS入料手臂吸放料流程.StepName = "入料吸嘴吸放料";
                        result = 入料吸嘴_吸放料(lsopen, true, true);
                        //if (result == "Success")
                        {
                            PS入料手臂吸放料流程.StepID = 540;
                        }

                        break;

                    case 540:
                        PS入料手臂吸放料流程.StepName = "入料手臂Z真空检测";
                        result = 入料吸嘴_真空检测(out ioval);
                        //if (result == "Success")
                        {
                            PS入料手臂吸放料流程.StepID = 545;
                        }
                        break;

                    case 545:
                        PS入料手臂吸放料流程.StepName = "入料手臂Z向上移动";
                        result = 入料手臂Z移动(lsopen, false);
                        //if (result == "Success")
                        {
                            PS入料手臂吸放料流程.StepID = 550;
                        }
                        break;

                    case 550:
                        PS入料手臂吸放料流程.StepName = "入料手臂Z回原感应";
                        result = 入料吸嘴_回原检测(out home);
                        //if (result == "Success")
                        {
                            PS入料手臂吸放料流程.StepID = 599;
                        }
                        break;

                    case 599:
                        PS入料手臂吸放料流程.StepName = "入料手臂_吸料流程完成";
                        PS入料手臂吸放料流程.Processing = false;
                        PS入料手臂吸放料流程.StepID = 705;
                        break;

                    case 705: ////放料流程////
                        PS入料手臂吸放料流程.StepName = "XYP轴移动到飞梭放料位置";
                        result = 入料手臂移动(xPos, yPos, 10); //写去哪里吸料，至于去哪个位置吸，走多少由APPmaster决定
                        //if (result == "Success")
                        {
                            if (入料手臂抓料等待标记) PS入料手臂吸放料流程.StepID = 505;
                            else PS入料手臂吸放料流程.StepID = 710;
                        }
                        break;

                    case 710:
                        PS入料手臂吸放料流程.StepName = "入料手臂Z向下移动";
                        if (飞梭移动状态 == 0) //梭车停止状态才能放
                        {
                            result = 入料手臂Z移动(lsopen, true);
                            //if (result == "Success")
                            {
                                PS入料手臂吸放料流程.StepID = 715;
                            }
                        }
                        break;

                    case 715:
                        PS入料手臂吸放料流程.StepName = "入料吸嘴放料";
                        result = 入料吸嘴_吸放料(lsopen, true, false); //关吸真空
                        //if (result == "Success")
                        {
                            result = 入料吸嘴_吸放料(lsopen, false, true); //开破真空
                            //if (result == "Success")
                            {
                                result = 入料吸嘴_吸放料(lsopen, false, false); //关破真空
                                PS入料手臂吸放料流程.StepID = 720;
                            }
                        }

                        break;

                    case 720:
                        PS入料手臂吸放料流程.StepName = "入料手臂Z真空检测";
                        result = 入料吸嘴_真空检测(out ioval);
                        //if (result == "Success")
                        {
                            PS入料手臂吸放料流程.StepID = 725;
                        }
                        break;

                    case 725:
                        PS入料手臂吸放料流程.StepName = "入料手臂Z向上移动";
                        result = 入料手臂Z移动(lsopen, false);
                        //if (result == "Success")
                        {
                            PS入料手臂吸放料流程.StepID = 730;
                        }
                        break;

                    case 730:
                        PS入料手臂吸放料流程.StepName = "入料手臂Z回原感应";
                        result = 入料吸嘴_回原检测(out home);
                        //if (result == "Success")
                        {
                            PS入料手臂吸放料流程.StepID = 799;
                        }
                        break;

                    case 799:
                        for (int i = 0; i < lsopen.Length; i++)
                        {
                            if (lsopen[i])
                            {
                                飞梭前入料IC[colums][i] = true;
                            }
                        }
                        入料手臂抓料等待标记 = false;
                        PS入料手臂吸放料流程.StepName = "入料手臂_放料流程完成";
                        PS入料手臂吸放料流程.StepID = 505;
                        break;

                    default:
                        Debug.WriteLine(("PS入料手臂吸放料流程.StepID =" + PS入料手臂吸放料流程.StepID.ToString() + "对应流程不存在"));
                        break;

                }
            }
        }

        //public void 入料手臂吸放料流程()
        //{
        //    string result;
        //    short xPoint = 0;
        //    short yPoint = 0;
        //    short colums = 0;
        //    bool[] open = new bool[4];
        //    int[] opens = new int[4];
        //    bool[] lsopen = new bool[4];
        //    int index = 0;
        //    double xPos = 0;
        //    double yPos = 0;


        //    string[] axisNames =
        //        {
        //        "M1_入料手臂Z1轴", "M2_入料手臂Z2轴",
        //        "M3_入料手臂Z3轴", "M4_入料手臂Z4轴"
        //    }; PS入料手臂吸放料流程.StepID = 500;

        //    while (true)
        //    {
        //        Thread.Sleep(5);
        //        switch (PS入料手臂吸放料流程.StepID)
        //        {
        //            case 500:
        //                PS入料手臂吸放料流程.StepName = "路径规划";
        //                PS入料手臂吸放料流程.Condition = !结批标记;
        //                PS入料手臂吸放料流程.Permit = !PS入料手臂放料流程.Processing;
        //                if (PS入料手臂吸放料流程.Condition && PS入料手臂吸放料流程.Permit)
        //                {
        //                    PS入料手臂吸放料流程.Processing = true;
        //                    PS入料手臂吸放料流程.StepID = 510;
        //                }
        //                break;

        //            case 501:
        //                PS入料手臂吸放料流程.StepName = "计算放料IC颗数";
        //                if (!参数对比(飞梭前入料IC[0], 放料IC目标值[0]))
        //                {
        //                    open = 获取放料IC(飞梭前入料IC[0], 放料IC目标值[0]);
        //                    if (open.Contains(true))
        //                    {
        //                        colums = 0;
        //                        opens = 吸嘴是否吸料(open);
        //                        PS入料手臂吸放料流程.StepID = 515;
        //                    }
        //                }
        //                else if (!参数对比(飞梭前入料IC[1], 放料IC目标值[1]))
        //                {
        //                    open = 获取放料IC(飞梭前入料IC[1], 放料IC目标值[1]);
        //                    if (open.Contains(true))
        //                    {
        //                        colums = 1;
        //                        opens = 吸嘴是否吸料(open);
        //                        PS入料手臂吸放料流程.StepID = 515;
        //                    }
        //                }
        //                else
        //                {
        //                    PS入料手臂吸放料流程.StepID = 599;
        //                }

        //                break;

        //            case 505:
        //                PS入料手臂吸放料流程.StepName = "计算抓料标记";
        //                if (入料手臂吸料状态 == 1)
        //                {
        //                    Thread.Sleep(10);
        //                    PS入料手臂吸放料流程.StepID = 510;
        //                }
        //                else
        //                {
        //                    if (参数对比(飞梭后入料IC[0], 放料IC目标值[0]) || 参数对比(飞梭后入料IC[1], 放料IC目标值[1]))
        //                    {
        //                        Thread.Sleep(10);
        //                    }
        //                    else
        //                    {
        //                        PS入料手臂吸放料流程.StepID = 511;
        //                    }
        //                }

        //                break;

        //            case 511:
        //                PS入料手臂吸放料流程.StepName = "计算后放料IC颗数";
        //                if (!参数对比(飞梭后入料IC[0], 放料IC目标值[0]))
        //                {
        //                    open = 获取放料IC(飞梭后入料IC[0], 放料IC目标值[0]);
        //                    if (open.Contains(true))
        //                    {
        //                        colums = 0;
        //                        opens = 吸嘴是否吸料(open);
        //                        入料手臂抓料等待标记 = true;
        //                        PS入料手臂吸放料流程.StepID = 515;
        //                    }
        //                }
        //                else if (!参数对比(飞梭后入料IC[1], 放料IC目标值[1]))
        //                {
        //                    open = 获取放料IC(飞梭后入料IC[1], 放料IC目标值[1]);
        //                    if (open.Contains(true))
        //                    {
        //                        colums = 1;
        //                        opens = 吸嘴是否吸料(open);
        //                        入料手臂抓料等待标记 = true;
        //                        PS入料手臂吸放料流程.StepID = 515;
        //                    }
        //                }
        //                else
        //                {
        //                    PS入料手臂吸放料流程.StepID = 599;
        //                }

        //                break;


        //            case 512:
        //                if (入料手臂抓料等待标记)
        //                {
        //                    PS入料手臂吸放料流程.StepID = 705;
        //                }
        //                else
        //                {
        //                    PS入料手臂吸放料流程.StepID = 515;
        //                }
        //                break;

        //            case 515:
        //                PS入料手臂吸放料流程.StepName = "获取Load盘坐标";
        //                bool 坐标 = APPmaster.获取Load盘坐标(true, out xPoint, out yPoint);
        //                if (坐标)
        //                {
        //                    PS入料手臂吸放料流程.StepID = 520;
        //                }
        //                else
        //                {
        //                    Debug.WriteLine("进入换盘程序");
        //                }
        //                break;

        //            case 520:
        //                PS入料手臂吸放料流程.StepName = "获取吸料信息";
        //                result = APPmaster.获取Load盘Open(opens, xPoint, yPoint, out lsopen, out index, out xPos, out yPos);
        //                if (result == "Success")
        //                {
        //                    PS入料手臂吸放料流程.StepID = 525;
        //                }
        //                break;

        //            case 525:
        //                PS入料手臂吸放料流程.StepName = "XYP轴移动到Load吸料位置";
        //                result = 入料手臂移动(xPos, yPos, 10);
        //                //if (result == "Success")
        //                {
        //                    PS入料手臂吸放料流程.StepID = 530;
        //                }
        //                break;

        //            case 530:
        //                PS入料手臂吸放料流程.StepName = "Z轴向下移动";
        //                result = 入料手臂Z移动(lsopen, 1000, true);
        //                //if (result == "Success")
        //                {
        //                    PS入料手臂吸放料流程.StepID = 535;
        //                }
        //                break;

        //            case 535:
        //                PS入料手臂吸放料流程.StepName = "入料吸嘴吸料";
        //                result = 入料吸嘴_吸料(lsopen, true);
        //                //if (result == "Success")
        //                {
        //                    PS入料手臂吸放料流程.StepID = 540;
        //                }
        //                break;

        //            case 540:
        //                PS入料手臂吸放料流程.StepName = "入料吸嘴真空检测";
        //                result = 入料吸嘴_真空检测(out string[] io);
        //                //if (result == "Success")
        //                {
        //                    PS入料手臂吸放料流程.StepID = 545;
        //                    APPmaster.更新Load盘Map(lsopen, index, xPoint, yPoint);
        //                    for (int i = 0; i < io.Length; i++)
        //                    {
        //                        if (lsopen[i] && io[i] == null)
        //                        {
        //                            opens[i] = 1;
        //                        }
        //                    }
        //                }
        //                break;

        //            case 545:
        //                PS入料手臂吸放料流程.StepName = "Z轴向上移动";
        //                result = 入料手臂Z移动(lsopen, -0.2, false);
        //                //if (result == "Success")
        //                {
        //                    for (int i = 0; i < lsopen.Length; i++)
        //                    {
        //                        if (i + index >= lsopen.Length) break;
        //                        if (lsopen[i + index])
        //                        {
        //                            if (opens[i + index] == 1)
        //                            {
        //                                lsopen[i + index] = false;
        //                                //Debug.WriteLine($"入料吸料Z{i + index + 1}吸料成功");
        //                            }
        //                            //else Debug.WriteLine($"入料吸料Z{i + index + 1}吸料失败");
        //                        }
        //                    }
        //                    PS入料手臂吸放料流程.StepID = 550;
        //                }
        //                break;

        //            case 550:
        //                PS入料手臂吸放料流程.StepName = "入料Z是否在原点";
        //                result = motionEx.GetDiHome(axisNames, out string[] home);
        //                //if (result == "Success")
        //                {
        //                    Debug.WriteLine($"入料Z回原：{home[0]} {home[1]} {home[2]} {home[3]}");

        //                    if (lsopen.Contains(true))
        //                    {
        //                        PS入料手臂吸放料流程.StepID = 530;
        //                    }
        //                    else
        //                    {
        //                        PS入料手臂吸放料流程.StepID = 560;
        //                    }
        //                }
        //                break;

        //            case 560:
        //                PS入料手臂吸放料流程.StepName = "吸完检测";
        //                if (opens.Contains(0)) //如果还有吸嘴没吸
        //                {
        //                    PS入料手臂吸放料流程.StepID = 515;
        //                }
        //                else
        //                {
        //                    lsopen = new bool[] { opens[0] == 1, opens[1] == 1, opens[2] == 1, opens[3] == 1 };
        //                    PS入料手臂吸放料流程.StepID = 700;
        //                }
        //                break;

        //            case 599:
        //                PS入料手臂吸放料流程.StepName = "入料手臂_吸放料流程完成";
        //                PS入料手臂吸放料流程.Processing = false;
        //                PS入料手臂吸放料流程.StepID = 500;
        //                break;

        //            case 700:
        //                PS入料手臂吸放料流程.StepName = "XYP轴移动到飞梭放料位置";
        //                xPos = colums == 0 ? 1000 : 1200;
        //                yPos = colums == 0 ? 200 : 400;
        //                result = 入料手臂移动(xPos, yPos, 10);
        //                //if (result == "Success")
        //                {
        //                    PS入料手臂吸放料流程.StepID = 705;
        //                }
        //                break;


        //            case 705:
        //                result = 入料手臂Z移动(lsopen, 100, true);
        //                //if (result == "Success")
        //                {
        //                    PS入料手臂吸放料流程.StepID = 710;
        //                }
        //                break;

        //            case 710:
        //                result = 入料吸嘴_吸料(lsopen, false);
        //                //if (result == "Success")
        //                {
        //                    result = 入料吸嘴_放料(lsopen, true);
        //                    //if (result == "Success")
        //                    {
        //                        Thread.Sleep(200);
        //                        result = 入料吸嘴_放料(lsopen, false);
        //                        //if (result == "Success")
        //                        {
        //                            PS入料手臂吸放料流程.StepID = 715;
        //                        }
        //                    }
        //                }
        //                break;

        //            case 715:
        //                result = 入料手臂Z移动(lsopen, -0.2, false);
        //                //if (result == "Success")
        //                {
        //                    PS入料手臂吸放料流程.StepID = 720;
        //                }
        //                break;

        //            case 720:
        //                PS入料手臂吸放料流程.StepName = "入料Z是否在原点";

        //                result = motionEx.GetDiHome(axisNames, out home);
        //                //if (result == "Success")
        //                {
        //                    Debug.WriteLine($"入料Z回原：{home[0]} {home[1]} {home[2]} {home[3]}");

        //                    PS入料手臂吸放料流程.StepID = 510;
        //                    for (int i = 0; i < lsopen.Length; i++)
        //                    {
        //                        if (lsopen[i])
        //                        {
        //                            飞梭前入料IC[colums][i] = true;
        //                        }
        //                    }
        //                }
        //                break;

        //            default:
        //                Debug.WriteLine(("PS入料手臂吸料流程.StepID =" + PS入料手臂吸放料流程.StepID.ToString() + "对应流程不存在"));
        //                break;
        //        }
        //    }
        //}

        public void 出料手臂吸放料流程()
        {
            string result = "";
            int[] open = new int[4];
            bool[][] opens = new bool[2][];
            bool[] lsopen = new bool[4];
            string[][] bins = new string[2][];
            string[] bin = new string[4];
            short colums = 0;
            double shuttlex = 0;
            double shuttley = 0;
            double shuttlep = 0;
            int index = 0;
            double xPos = 0;
            double yPos = 0;
            bool ispass = false;
            short trayid = 0;
            short xPoint = 0;
            short yPoint = 0;
            PS出料手臂吸放料流程.StepID = 500;
            while (true)
            {
                Thread.Sleep(5);
                switch (PS出料手臂吸放料流程.StepID)
                {
                    case 500:
                        PS出料手臂吸放料流程.StepName = "路径规划";
                        PS出料手臂吸放料流程.Condition = 飞梭前出料状态 == 1;
                        PS出料手臂吸放料流程.Permit = !PS入飞梭移动流程.Processing;
                        if (PS出料手臂吸放料流程.Condition && PS出料手臂吸放料流程.Permit)
                        {
                            ispass = false;
                            opens = new bool[2][]
                            {
                               new bool[]{ 飞梭前出料IC[0][0], 飞梭前出料IC[0][1], 飞梭前出料IC[0][2], 飞梭前出料IC[0][3]},
                               new bool[]{ 飞梭前出料IC[1][0], 飞梭前出料IC[1][1], 飞梭前出料IC[1][2], 飞梭前出料IC[1][3]},
                            };
                            bins = new string[2][]
                            {
                                new string[]{ 飞梭前出料BIN项[0][0], 飞梭前出料BIN项[0][1], 飞梭前出料BIN项[0][2], 飞梭前出料BIN项[0][3]},
                                new string[]{ 飞梭前出料BIN项[1][0], 飞梭前出料BIN项[1][1], 飞梭前出料BIN项[1][2], 飞梭前出料BIN项[1][3]},
                            };
                            PS出料手臂吸放料流程.Processing = true;
                            PS出料手臂吸放料流程.StepID = 505;
                        }
                        break;

                    case 505:
                        PS出料手臂吸放料流程.StepName = "计算放料IC颗数";
                        if (opens[0].Contains(true))
                        {
                            colums = 0;
                            open = new int[] { opens[0][0] ? 1 : 0, opens[0][1] ? 1 : 0, opens[0][2] ? 1 : 0, opens[0][3] ? 1 : 0 };
                            bin = new string[] { bins[0][0], bins[0][1], bins[0][2], bins[0][3] };
                            PS出料手臂吸放料流程.StepID = 510;
                        }
                        else if (opens[1].Contains(true))
                        {
                            colums = 1;
                            open = new int[] { opens[1][0] ? 1 : 0, opens[1][1] ? 1 : 0, opens[1][2] ? 1 : 0, opens[1][3] ? 1 : 0 };
                            bin = new string[] { bins[1][0], bins[1][1], bins[1][2], bins[1][3] };
                            PS出料手臂吸放料流程.StepID = 515;
                        }
                        else //放料完成
                        {
                            if (!ispass) //如果第二列没有料就会是为false
                            {
                                飞梭前出料状态 = 0;
                            }
                            PS出料手臂吸放料流程.StepID = 599;
                        }
                        break;

                    case 510:
                        PS出料手臂吸放料流程.StepName = "出料手臂移动到飞梭第1列";
                        result = 出料手臂移动(shuttlex, shuttley, shuttlep);
                        //if (result == "Success")
                        {
                            PS出料手臂吸放料流程.StepID = 520;
                        }
                        break;

                    case 515:
                        PS出料手臂吸放料流程.StepName = "出料手臂移动到飞梭第2列";
                        result = 出料手臂移动(shuttlex, shuttley, shuttlep);
                        //if (result == "Success")
                        {
                            PS出料手臂吸放料流程.StepID = 520;
                        }
                        break;

                    case 520:
                        result = 出料手臂Z移动(opens[colums], 100, true);
                        //if (result == "Success")
                        {
                            PS出料手臂吸放料流程.StepID = 525;
                        }
                        break;

                    case 525:
                        result = 出料吸嘴_吸放料(opens[colums], true, true);
                        //if (result == "Success")
                        {
                            PS出料手臂吸放料流程.StepID = 530;
                        }
                        break;

                    case 530:
                        result = 出料吸嘴_真空检测(out string[] io);
                        //if (result == "Success")
                        {
                            PS出料手臂吸放料流程.StepID = 535;
                        }
                        break;

                    case 535:
                        result = 出料手臂Z移动(opens[colums], -0.2, false);
                        //if (result == "Success")
                        {
                            PS出料手臂吸放料流程.StepID = 540;
                        }
                        break;

                    case 540:
                        PS出料手臂吸放料流程.StepName = "出料Z是否在原点";
                        result = 出料吸嘴_回原检测(out string[] home);
                        //if (result == "Success")
                        {
                            Debug.WriteLine($"入料Z回原：{home[0]} {home[1]} {home[2]} {home[3]}");
                            if (colums == 1)
                            {
                                ispass = true;
                                飞梭前出料状态 = 0;
                            }
                            PS出料手臂吸放料流程.StepID = 705;
                        }
                        break;

                    case 705: //放料
                        PS出料手臂吸放料流程.StepName = "计算放料盘位置";
                        if (APPmaster.判断BIN项(open, APPmaster.Auto1, bin, out index))
                        {
                            if (APPmaster.获取Auto1盘坐标(false, out xPoint, out yPoint))
                            {
                                result = APPmaster.获取Auto1盘Open(open, xPoint, yPoint, out lsopen, out index, out xPos, out yPos);
                                if (result == "Success")
                                {
                                    trayid = 0;
                                    PS出料手臂吸放料流程.StepID = 710;
                                }
                            }
                            else PS出料手臂吸放料流程.StepID = 800; //执行Auto1换盘程序
                        }
                        else if (APPmaster.判断BIN项(open, APPmaster.Auto2, bin, out index))
                        {
                            if (APPmaster.获取Auto2盘坐标(false, out xPoint, out yPoint))
                            {
                                result = APPmaster.获取Auto2盘Open(open, xPoint, yPoint, out lsopen, out index, out xPos, out yPos);

                                if (result == "Success")
                                {
                                    trayid = 1;
                                    PS出料手臂吸放料流程.StepID = 710;
                                }
                            }
                            else PS出料手臂吸放料流程.StepID = 900; //执行Auto2换盘程序
                        }
                        else if (APPmaster.判断BIN项(open, APPmaster.Fix1, bin, out index))
                        {
                            if (APPmaster.获取Fix1盘坐标(false, out xPoint, out yPoint))
                            {
                                result = APPmaster.获取Fix1盘Open(open, xPoint, yPoint, out lsopen, out index, out xPos, out yPos);

                                if (result == "Success")
                                {
                                    trayid = 2;
                                    PS出料手臂吸放料流程.StepID = 710;
                                }
                            }
                            else PS出料手臂吸放料流程.StepID = 1000; //执行Fix1满盘报警程序
                        }
                        else if (APPmaster.判断BIN项(open, APPmaster.Fix2, bin, out index))
                        {
                            if (APPmaster.获取Fix2盘坐标(false, out xPoint, out yPoint))
                            {
                                result = APPmaster.获取Fix2盘Open(open, xPoint, yPoint, out lsopen, out index, out xPos, out yPos);

                                if (result == "Success")
                                {
                                    trayid = 3;
                                    PS出料手臂吸放料流程.StepID = 710;
                                }
                            }
                            else PS出料手臂吸放料流程.StepID = 1100; //执行Fix2满盘报警程序
                        }
                        else if (APPmaster.判断BIN项(open, APPmaster.Fix3, bin, out index))
                        {
                            if (APPmaster.获取Fix3盘坐标(false, out xPoint, out yPoint))
                            {
                                result = APPmaster.获取Fix3盘Open(open, xPoint, yPoint, out lsopen, out index, out xPos, out yPos);

                                if (result == "Success")
                                {
                                    trayid = 4;
                                    PS出料手臂吸放料流程.StepID = 710;
                                }
                            }
                            else PS出料手臂吸放料流程.StepID = 1200; //执行Fix3满盘报警程序
                        }
                        else
                        {
                            PS出料手臂吸放料流程.StepID = 505;
                            break;
                        }

                        //if (result == "Success")
                        //{
                        //    PS出料手臂吸放料流程.StepID = 710;
                        //}

                        break;

                    case 700:

                        break;

                    case 710:
                        PS出料手臂吸放料流程.StepName = "出料手臂走到放料位";
                        result = 出料手臂移动(xPos, yPos, 10); //yPitch
                        //if (result == "Success")
                        {
                            PS出料手臂吸放料流程.StepID = 715;
                        }
                        break;

                    case 715:
                        PS出料手臂吸放料流程.StepName = "出料手臂Z向下";
                        result = 出料手臂Z移动(lsopen, 100, true);
                        //if (result == "Success")
                        {
                            PS出料手臂吸放料流程.StepID = 720;
                        }
                        break;

                    case 720:
                        PS出料手臂吸放料流程.StepName = "出料吸嘴放料";
                        result = 出料吸嘴_吸放料(lsopen, true, false);
                        //if (result == "Success")
                        {
                            result = 出料吸嘴_吸放料(lsopen, false, true);
                            //if (result == "Success")
                            {
                                Thread.Sleep(200);
                                result = 出料吸嘴_吸放料(lsopen, false, false);
                                //if (result == "Success")
                                {
                                    PS出料手臂吸放料流程.StepID = 725;
                                }
                            }
                        }
                        break;

                    case 725:
                        PS出料手臂吸放料流程.StepName = "出料手臂Z向上";
                        result = 出料手臂Z移动(lsopen, -0.2, false);
                        //if (result == "Success")
                        {
                            PS出料手臂吸放料流程.StepID = 730;
                        }
                        break;

                    case 730:
                        PS出料手臂吸放料流程.StepName = "出料Z是否在原点";
                        result = 出料吸嘴_回原检测(out home);
                        //if (result == "Success")
                        {
                            Debug.WriteLine($"入料Z回原：{home[0]} {home[1]} {home[2]} {home[3]}");
                            for (int i = 0; i < lsopen.Length; i++)
                            {
                                if (index + i == lsopen.Length) break;
                                if (lsopen[i + index])
                                {
                                    open[i + index] = 0;
                                    opens[colums][i + index] = false;
                                    if (trayid == 0) Auto1DicPoint[$"{xPoint},{yPoint + i * Interval}"]= true;
                                    if (trayid == 1) Auto2DicPoint[$"{xPoint},{yPoint + i * Interval}"] = true;
                                    if (trayid == 2) Fix1DicPoint[$"{xPoint},{yPoint + i * Interval}"] = true;
                                    if (trayid == 3) Fix2DicPoint[$"{xPoint},{yPoint + i * Interval}"] = true;
                                    else Fix3DicPoint[$"{xPoint},{yPoint + i * Interval}"] = true;
                                }
                            }
                            PS出料手臂吸放料流程.StepID = 700;
                        }
                        break;

                    case 599:
                        PS出料手臂吸放料流程.StepName = "出料手臂_吸放料流程完成";
                        PS出料手臂吸放料流程.Processing = false;
                        PS出料手臂吸放料流程.StepID = 500;
                        break;

                    default:
                        Debug.WriteLine(("PS出料手臂吸放料流程.StepID =" + PS出料手臂吸放料流程.StepID.ToString() + "对应流程不存在"));
                        break;
                }
            }
        }

        public void 飞梭移动流程()
        {
            string result;
            PS入飞梭移动流程.StepID = 500;
            while (true)
            {
                Thread.Sleep(5);

                switch (PS入飞梭移动流程.StepID)
                {
                    case 500:
                        PS入飞梭移动流程.StepName = "路径规划";
                        PS入飞梭移动流程.Condition = 飞梭前入料状态 == 1 && 飞梭后入料状态 == 0 && 飞梭前出料状态 == 0;

                        PS入飞梭移动流程.Permit = true;// !PS入料手臂吸放料流程.Processing;
                        if (PS入飞梭移动流程.Condition && PS入飞梭移动流程.Permit)
                        {
                            飞梭移动状态 = 1;
                            PS入飞梭移动流程.Processing = true;
                            PS入飞梭移动流程.StepID = 505;
                        }
                        break;

                    case 505:
                        if (飞梭2位置 == 0)
                        {
                            飞梭2位置 = 1;
                            result = 飞梭轴移动(200);
                        }
                        else
                        {
                            飞梭2位置 = 0;
                            result = 飞梭轴移动(100);
                        }
                        //if (result == "Success")
                        {
                            PS入飞梭移动流程.StepID = 515;
                        }
                        break;

                    case 515:

                        result = 飞梭_前后检测产品感应(out string[] iovalues);
                        //if (result == "Success")
                        {

                            //if (iovalues.Contains("0"))
                            {
                                交换飞梭前后入料IC数据();
                                交换飞梭前后出料IC数据();

                                飞梭后入料状态 = 1;
                                飞梭前入料状态 = 0;

                                if (飞梭后出料状态 == 2)
                                {
                                    交换飞梭前后出料BIN数据();
                                    飞梭前出料状态 = 1;
                                    飞梭后出料状态 = 0;
                                }

                                PS入飞梭移动流程.StepID = 599;
                            }
                        }
                        break;

                    case 599:
                        飞梭移动状态 = 0;
                        PS入飞梭移动流程.StepName = $"飞梭2移动到{(飞梭2位置 == 1 ? "前方" : "后方")}完成";
                        PS入飞梭移动流程.Processing = false;
                        PS入飞梭移动流程.StepID = 500;
                        break;

                    default:
                        Debug.WriteLine(("PS入飞梭移动流程.StepID =" + PS入飞梭移动流程.StepID.ToString() + "对应流程不存在"));
                        break;
                }
            }
        }

        public void 测试手臂取待测料流程()
        {
            string result;
            bool[] open = new bool[8];
            PS测试手臂取待测料流程.StepID = 500;
            while (true)
            {
                Thread.Sleep(5);

                switch (PS测试手臂取待测料流程.StepID)
                {
                    case 500:
                        PS测试手臂取待测料流程.StepName = "路径规划";
                        PS测试手臂取待测料流程.Condition = 飞梭后入料状态 == 1 && (飞梭后出料状态 == 1 || APPmaster.SiteSaveQueue.Count == 0);
                        PS测试手臂取待测料流程.Permit = !PS入料手臂吸放料流程.Processing;
                        if (PS测试手臂取待测料流程.Condition && PS测试手臂取待测料流程.Permit)
                        {
                            open = new bool[]
                            {
                                飞梭后入料IC[0][0],飞梭后入料IC[0][1],飞梭后入料IC[0][2],飞梭后入料IC[0][3],
                                飞梭后入料IC[1][0],飞梭后入料IC[1][1],飞梭后入料IC[1][2],飞梭后入料IC[1][3]
                            };
                            PS测试手臂取待测料流程.Processing = true;
                            PS测试手臂取待测料流程.StepID = 505;
                        }
                        break;

                    case 505:
                        PS测试手臂取待测料流程.StepName = "测试吸嘴_张卡气缸输出";
                        result = 测试吸嘴_张开气缸输出(false);
                        //if (result == "Success")
                        {
                            PS测试手臂取待测料流程.StepID = 510;
                        }
                        break;

                    case 510:
                        PS测试手臂取待测料流程.StepName = "测试手臂X轴到飞梭后入料吸料";
                        result = 测试手臂X移动(100);
                        //if (result == "Success")
                        {
                            PS测试手臂取待测料流程.StepID = 515;
                        }
                        break;

                    case 515:
                        PS测试手臂取待测料流程.StepName = "测试吸嘴张开气缸原位输入读取";
                        result = 测试吸嘴张开气缸原位输入读取();
                        //if (result == "Success")
                        {
                            PS测试手臂取待测料流程.StepID = 520;
                        }
                        break;

                    case 520:
                        PS测试手臂取待测料流程.StepName = "测试Z向下移动";
                        result = 测试手臂Z移动(true);
                        //if (result == "Success")
                        {
                            PS测试手臂取待测料流程.StepID = 525;
                        }
                        break;

                    case 525:
                        PS测试手臂取待测料流程.StepName = "测试吸嘴吸料";
                        result = 测试吸嘴_吸料(open, true);
                        //if (result == "Success")
                        {
                            PS测试手臂取待测料流程.StepID = 525;
                        }
                        break;

                    case 530:
                        PS测试手臂取待测料流程.StepName = "测试吸嘴真空检测";
                        result = 测试吸嘴_真空检测(out string[] io);
                        //if (result == "Success")
                        {
                            PS测试手臂取待测料流程.StepID = 530;
                        }
                        break;

                    case 535:
                        PS测试手臂取待测料流程.StepName = "测试Z向上移动";
                        result = 测试手臂Z移动(false);
                        //if (result == "Success")
                        {
                            PS测试手臂取待测料流程.StepID = 530;
                        }
                        break;

                    case 540:
                        PS测试手臂取待测料流程.StepName = "测试吸嘴_张开";
                        result = 测试吸嘴_张开气缸输出(true);
                        if (result == "Success")
                        {
                            PS测试手臂取待测料流程.StepID = 545;
                        }
                        break;

                    case 545:
                        PS测试手臂取待测料流程.StepName = "测试手臂X移动到测试区放料";
                        result = 测试手臂X移动(120);
                        if (result == "Success")
                        {
                            PS测试手臂取待测料流程.StepID = 555;
                        }
                        break;

                    case 550:
                        PS测试手臂取待测料流程.StepName = "测试吸嘴张开气缸到位输入读取";
                        result = 测试吸嘴张开气缸到位输入读取();
                        if (result == "Success")
                        {
                            PS测试手臂取待测料流程.StepID = 560;
                        }
                        break;

                    case 555:
                        PS测试手臂取待测料流程.StepName = "测试Z向下移动";
                        if (SiteTestState[0] == 0)
                        {
                            result = 测试手臂Z移动(true);
                            if (result == "Success")
                            {
                                PS测试手臂取待测料流程.StepID = 565;
                            }
                        }
                        break;

                    case 560:
                        PS测试手臂取待测料流程.StepName = "测试吸嘴放料";
                        result = 测试吸嘴_吸料(open, false);
                        if (result == "Success")
                        {
                            result = 测试吸嘴_放料(open, true);
                            if (result == "Success")
                            {
                                Thread.Sleep(200);
                                result = 测试吸嘴_放料(open, false);
                                if (result == "Success")
                                {
                                    PS测试手臂取待测料流程.StepID = 565;
                                }
                            }
                        }
                        break;

                    case 565:
                        PS测试手臂取待测料流程.StepName = "测试Z向上移动";
                        result = 测试手臂Z移动(false);
                        if (result == "Success")
                        {
                            PS测试手臂取待测料流程.StepID = 560;
                        }
                        break;


                    case 570:
                        PS入料手臂吸放料流程.StepName = "测试Z是否在原点";

                        result = motionEx.GetDiHome(new string[] { "M19_测试手臂Z轴" }, out string[] home);
                        //if (result == "Success")
                        {
                            Debug.WriteLine($"测试Z回原：{home[0]}");

                            //for (int i = 0; i < lsopen.Length; i++)
                            //{
                            //    if (lsopen[i])
                            //    {
                            //        飞梭前入料IC[colums][i] = true;
                            //    }
                            //}

                            PS入料手臂吸放料流程.StepID = 575;
                        }
                        break;

                    case 599:
                        PS测试手臂取待测料流程.StepName = $"测试手臂取待测料流程完成";
                        PS测试手臂取待测料流程.Processing = false;
                        PS测试手臂取待测料流程.StepID = 500;
                        break;

                    default:
                        Debug.WriteLine(("PS测试手臂取待测料流程.StepID =" + PS测试手臂取待测料流程.StepID.ToString() + "对应流程不存在"));
                        break;
                }
            }
        }

        public void 测试手臂取已测料流程()
        {
            string result;
            bool[] open = new bool[8];
            PS测试手臂取已测料流程.StepID = 500;
            while (true)
            {
                Thread.Sleep(5);

                switch (PS测试手臂取已测料流程.StepID)
                {
                    case 500:
                        PS测试手臂取已测料流程.StepName = "路径规划";
                        PS测试手臂取已测料流程.Condition = 飞梭前入料状态 == 2 && 飞梭后入料状态 == 0;
                        PS测试手臂取已测料流程.Permit = !PS入料手臂吸放料流程.Processing;
                        if (PS测试手臂取已测料流程.Condition && PS测试手臂取已测料流程.Permit)
                        {
                            open = new bool[]
                            {
                                飞梭后入料IC[0][0],飞梭后入料IC[0][1],飞梭后入料IC[0][2],飞梭后入料IC[0][3],
                                飞梭后入料IC[1][0],飞梭后入料IC[1][1],飞梭后入料IC[1][2],飞梭后入料IC[1][3]
                            };
                            PS测试手臂取已测料流程.Processing = true;
                            PS测试手臂取已测料流程.StepID = 505;
                        }
                        break;

                    case 505:
                        PS测试手臂取已测料流程.StepName = "测试吸嘴_张卡气缸输出";
                        result = 测试吸嘴_张开气缸输出(true);
                        if (result == "Success")
                        {
                            PS测试手臂取已测料流程.StepID = 510;
                        }
                        break;

                    case 510:
                        PS测试手臂取已测料流程.StepName = "测试手臂X轴到测试区吸料";
                        result = 测试手臂X移动(100);
                        if (result == "Success")
                        {
                            PS测试手臂取已测料流程.StepID = 515;
                        }
                        break;

                    case 515:
                        PS测试手臂取已测料流程.StepName = "测试吸嘴_张开气缸到位输出";
                        result = 测试吸嘴_张开气缸到位输出();
                        if (result == "Success")
                        {
                            PS测试手臂取已测料流程.StepID = 520;
                        }
                        break;

                    case 520:
                        PS测试手臂取已测料流程.StepName = "测试Z向下移动";
                        if (SiteTestState[0] == 4) //待取出
                        {
                            result = 测试手臂Z移动(true);
                            if (result == "Success")
                            {
                                PS测试手臂取已测料流程.StepID = 525;
                            }
                        }
                        break;

                    case 525:
                        PS测试手臂取已测料流程.StepName = "测试吸嘴吸料";
                        result = 测试吸嘴_吸料(open, true);
                        if (result == "Success")
                        {
                            PS测试手臂取已测料流程.StepID = 525;
                        }
                        break;

                    case 530:
                        PS测试手臂取已测料流程.StepName = "测试吸嘴真空检测";
                        result = 测试吸嘴_真空检测(out string[] io);
                        if (result == "Success")
                        {
                            PS测试手臂取已测料流程.StepID = 530;
                        }
                        break;

                    case 535:
                        PS测试手臂取已测料流程.StepName = "测试Z向上移动";
                        result = 测试手臂Z移动(false);
                        if (result == "Success")
                        {
                            PS测试手臂取已测料流程.StepID = 530;
                        }
                        break;

                    case 540:
                        PS测试手臂取已测料流程.StepName = "测试吸嘴_缩小";
                        result = 测试吸嘴_张开气缸输出(false);
                        if (result == "Success")
                        {
                            PS测试手臂取已测料流程.StepID = 545;
                        }
                        break;

                    case 545:
                        PS测试手臂取已测料流程.StepName = "测试手臂X移动到梭车放料";
                        result = 测试手臂X移动(120);
                        if (result == "Success")
                        {
                            PS测试手臂取已测料流程.StepID = 555;
                        }
                        break;

                    case 550:
                        PS测试手臂取已测料流程.StepName = "测试吸嘴_缩小并检测输入";
                        result = 测试吸嘴_张开气缸原位输出();
                        if (result == "Success")
                        {
                            PS测试手臂取已测料流程.StepID = 560;
                        }
                        break;

                    case 555:
                        PS测试手臂取已测料流程.StepName = "测试Z向下移动";
                        result = 测试手臂Z移动(true);
                        if (result == "Success")
                        {
                            PS测试手臂取已测料流程.StepID = 565;
                        }
                        break;

                    case 560:
                        PS测试手臂取已测料流程.StepName = "测试吸嘴放料";
                        result = 测试吸嘴_吸料(open, false);
                        if (result == "Success")
                        {
                            result = 测试吸嘴_放料(open, true);
                            if (result == "Success")
                            {
                                Thread.Sleep(200);
                                result = 测试吸嘴_放料(open, false);
                                if (result == "Success")
                                {
                                    PS测试手臂取已测料流程.StepID = 565;
                                }
                            }
                        }
                        break;

                    case 565:
                        PS测试手臂取已测料流程.StepName = "测试Z向上移动";
                        result = 测试手臂Z移动(false);
                        if (result == "Success")
                        {
                            PS测试手臂取已测料流程.StepID = 560;
                        }
                        break;


                    case 570:
                        PS测试手臂取已测料流程.StepName = "测试Z是否在原点";

                        result = motionEx.GetDiHome(new string[] { "M19_测试手臂Z轴" }, out string[] home);
                        //if (result == "Success")
                        {
                            Debug.WriteLine($"测试Z回原：{home[0]}");

                            //for (int i = 0; i < lsopen.Length; i++)
                            //{
                            //    if (lsopen[i])
                            //    {
                            //        飞梭前入料IC[colums][i] = true;
                            //    }
                            //}

                            PS测试手臂取已测料流程.StepID = 575;
                        }
                        break;

                    case 599:
                        PS测试手臂取已测料流程.StepName = $"测试手臂取已测料流程完成";
                        PS测试手臂取已测料流程.Processing = false;
                        PS测试手臂取已测料流程.StepID = 500;
                        break;

                    default:
                        Debug.WriteLine(("PS测试手臂取已测料流程.StepID =" + PS测试手臂取已测料流程.StepID.ToString() + "对应流程不存在"));
                        break;
                }
            }
        }

        public void 测试区气缸下压动作流程()
        {
            string result;
            short site = 0;
            List<bool[]> open = new List<bool[]>();
            while (true)
            {
                Thread.Sleep(5);

                switch (PS测试区气缸下压动作流程.StepID)
                {
                    case 500:
                        PS测试区气缸下压动作流程.StepName = "路径规划";
                        PS测试区气缸下压动作流程.Condition = 飞梭前入料状态 == 2 && 飞梭后入料状态 == 0;
                        PS测试区气缸下压动作流程.Permit = !PS入料手臂吸放料流程.Processing;
                        if (PS测试区气缸下压动作流程.Condition && PS测试区气缸下压动作流程.Permit)
                        {
                            bool[][] siteopne = APPmaster.SiteSave[site];
                            open = new List<bool[]>()
                            {
                                new bool[] { siteopne[0][0],siteopne[0][1],siteopne[0][2] ,siteopne[0][3]  },
                                new bool[] { siteopne[1][0],siteopne[1][1],siteopne[1][2] ,siteopne[1][3]  },
                            };
                            PS测试区气缸下压动作流程.Processing = true;
                            PS测试区气缸下压动作流程.StepID = 505;
                        }
                        break;

                    case 505:
                        PS测试区气缸下压动作流程.StepName = $"循环检测Site状态";
                        if (site >= 16)
                        {
                            site = 0;
                        }
                        if (SiteTestState[site] == 1) //有料
                        {
                            PS测试区气缸下压动作流程.StepID = 510;
                        }
                        else site++;
                        break;

                    case 510:
                        PS测试区气缸下压动作流程.StepName = $"测试区气缸前移";
                        result = 测试区_位移气缸到位输出(site);
                        if (result == "Success")
                        {
                            PS测试区气缸下压动作流程.StepID = 515;
                        }
                        break;

                    case 515:
                        PS测试区气缸下压动作流程.StepName = $"测试区气缸下压";
                        result = 测试区_下压气缸到位输出(site);
                        if (result == "Success")
                        {
                            PS测试区气缸下压动作流程.StepID = 520;
                        }
                        break;

                    case 520:
                        PS测试区气缸下压动作流程.StepName = $"发送测试信号";
                        result = APPmaster.Com.Send(site, open);
                        if (result == "Success")
                        {
                            SiteTestState[site] = 2;
                            PS测试区气缸下压动作流程.StepID = 599;
                        }
                        break;

                    case 599:
                        site++;
                        PS测试区气缸下压动作流程.StepName = $"测试区气缸动作流程完成";
                        PS测试区气缸下压动作流程.Processing = false;
                        PS测试区气缸下压动作流程.StepID = 500;
                        break;

                    default:
                        Debug.WriteLine(("PS测试区气缸动作流程.StepID =" + PS测试区气缸下压动作流程.StepID.ToString() + "对应流程不存在"));
                        break;
                        /*
                         * if (site1状态 == 1:有料) //如果需要测试				
                         {				
                            TestAircylinderStarttest(testnum); //触发下压动作	
                            com.Send(testnum, testgetsitestatus[testnum]);				
                            site1状态 = 2: 测试中			
                         }				
                         else if(site1状态 == 3:已测试)				
                         {				
                            TestAircylinderEndtest(testnum); //触发抬升动作			
                            site1状态 = 2: 待取料				
                         }				
                        */
                }
            }
        }

        public void 测试区气缸上抬动作流程()
        {
            string result;
            short site = 0;
            List<bool[]> open = new List<bool[]>();
            while (true)
            {
                Thread.Sleep(5);

                switch (PS测试区气缸上抬动作流程.StepID)
                {
                    case 500:
                        PS测试区气缸上抬动作流程.StepName = "路径规划";
                        PS测试区气缸上抬动作流程.Condition = 飞梭前入料状态 == 2 && 飞梭后入料状态 == 0;
                        PS测试区气缸上抬动作流程.Permit = !PS入料手臂吸放料流程.Processing;
                        if (PS测试区气缸上抬动作流程.Condition && PS测试区气缸上抬动作流程.Permit)
                        {
                            bool[][] siteopne = APPmaster.SiteSave[site];
                            open = new List<bool[]>()
                            {
                                new bool[] { siteopne[0][0],siteopne[0][1],siteopne[0][2] ,siteopne[0][3]  },
                                new bool[] { siteopne[1][0],siteopne[1][1],siteopne[1][2] ,siteopne[1][3]  },
                            };
                            PS测试区气缸上抬动作流程.Processing = true;
                            PS测试区气缸上抬动作流程.StepID = 505;
                        }
                        break;

                    case 505:
                        PS测试区气缸上抬动作流程.StepName = $"循环检测Site状态";
                        if (site >= 16)
                        {
                            site = 0;
                        }
                        if (SiteTestState[site] == 3) //已测试
                        {
                            PS测试区气缸上抬动作流程.StepID = 510;
                        }
                        else site++;
                        break;

                    case 510:
                        PS测试区气缸上抬动作流程.StepName = $"测试区气缸抬升";
                        result = 测试区_下压气缸原位输出(site);
                        if (result == "Success")
                        {
                            PS测试区气缸上抬动作流程.StepID = 515;
                        }
                        break;

                    case 515:
                        PS测试区气缸上抬动作流程.StepName = $"测试区气缸后移";
                        result = 测试区_位移气缸原位输出(site);
                        if (result == "Success")
                        {
                            SiteTestState[site] = 4;
                            PS测试区气缸上抬动作流程.StepID = 599;
                        }
                        break;

                    case 599:
                        site++;
                        PS测试区气缸上抬动作流程.StepName = $"测试区气缸上抬动作流程完成";
                        PS测试区气缸上抬动作流程.Processing = false;
                        PS测试区气缸上抬动作流程.StepID = 500;
                        break;

                    default:
                        Debug.WriteLine(("PS测试区气缸上抬动作流程.StepID =" + PS测试区气缸上抬动作流程.StepID.ToString() + "对应流程不存在"));
                        break;
                        /*
                         * if (site1状态 == 1:有料) //如果需要测试				
                         {				
                            TestAircylinderStarttest(testnum); //触发下压动作	
                            com.Send(testnum, testgetsitestatus[testnum]);				
                            site1状态 = 2: 测试中			
                         }				
                         else if(site1状态 == 3:已测试)				
                         {				
                            TestAircylinderEndtest(testnum); //触发抬升动作			
                            site1状态 = 2: 待取料				
                         }				
                        */
                }
            }
        }

        public void 系统按钮检测流程()
        {
            string[] ioInfos =
            {
                "X001_开始",
                "X002_暂停",
                "X003_安全钮",
                "X004_重试",
                "X005_忽略",
                "X006_定点停止",
                "X007_定点开始",
                "X008_结批",
                "X009_一个循环",
                "X010_归零",
                "X011_换盘",
                "X012_前门禁",
                "X013_后门禁",
                "X014_左门禁",
                "X015_右门禁",
                "X018_空盘门感应",
                "X024_Load门感应",
            };

            while (true)
            {
                string result = motionEx.GetDi(ioInfos, out string[] io);
                if (result == "Success")
                {
                    bool start = io[0] == "1"; //"X001_开始"
                    if (CheckEdgeSingal(start, ref StartRiseCache, true))
                    {
                        Debug.WriteLine("按下开始");
                    }
                    bool stop = io[1] == "1"; //"X002_暂停"
                    if (CheckEdgeSingal(stop, ref StopRiseCache, true))
                    {
                        Debug.WriteLine("按下暂停");
                    }
                    bool safesw = io[2] == "1"; //"X003_安全钮"
                    if (CheckEdgeSingal(safesw, ref SafeswRiseCache, true))
                    {
                        Debug.WriteLine("按下安全钮");
                    }
                    bool again = io[3] == "1"; //X004_重试
                    if (CheckEdgeSingal(again, ref AgainRiseCache, true))
                    {
                        Debug.WriteLine("按下重试");
                        //motionEx.SetGNME601_Output(GNME601IO.Y004_重试灯, true);
                        //motionEx.SetGNME601_Output(GNME601IO.Y005_忽略灯, false);
                        //motionEx.SetGNME601_Output(GNME601IO.Y008_结批灯, false);
                        //motionEx.SetGNME601_Output(GNME601IO.Y011_换TRAY灯, false);
                    }
                    bool ignore = io[4] == "1"; //X005_忽略/跳过
                    if (CheckEdgeSingal(ignore, ref IgnoreRiseCache, true))
                    {
                        Debug.WriteLine("按下跳过");
                        //motionEx.SetGNME601_Output(GNME601IO.Y004_重试灯, false);
                        //motionEx.SetGNME601_Output(GNME601IO.Y005_忽略灯, true);
                        //motionEx.SetGNME601_Output(GNME601IO.Y008_结批灯, false);
                        //motionEx.SetGNME601_Output(GNME601IO.Y011_换TRAY灯, false);
                    }
                    bool fixedstop = io[5] == "1"; //X006_定点停止
                    if (CheckEdgeSingal(fixedstop, ref FixedStopRiseCache, true))
                    {
                        Debug.WriteLine("按下定点停止");
                    }
                    bool fixedstart = io[6] == "1"; //X007_定点开始
                    if (CheckEdgeSingal(fixedstart, ref FixedStartRiseCache, true))
                    {
                        Debug.WriteLine("按下定点开始");
                    }

                    bool stopbatch = io[7] == "1"; //X008_结批
                    if (CheckEdgeSingal(stopbatch, ref StopBatchRiseCache, true))
                    {
                        Debug.WriteLine("按下结批");
                        //motionEx.SetGNME601_Output(GNME601IO.Y004_重试灯, false);
                        //motionEx.SetGNME601_Output(GNME601IO.Y005_忽略灯, false);
                        //motionEx.SetGNME601_Output(GNME601IO.Y008_结批灯, true);
                        //motionEx.SetGNME601_Output(GNME601IO.Y011_换TRAY灯, false);
                    }
                    if (CheckEdgeSingal(stopbatch, ref StopBatchFallCache, false))
                    {
                        Debug.WriteLine("已松开结批按钮停止计时");
                    }

                    bool onecycle = io[8] == "1"; //X009_一个循环
                    if (CheckEdgeSingal(onecycle, ref OnecycleRiseCache, true))
                    {
                        Debug.WriteLine("按下一次循环");
                    }
                    bool home = io[9] == "1"; //X010_归零
                    if (CheckEdgeSingal(home, ref HomeRiseCache, true))
                    {
                        Debug.WriteLine("按下归零");

                    }
                    bool exchangetray = io[10] == "1"; //X011_换盘
                    if (CheckEdgeSingal(exchangetray, ref ExchangetrayRiseCache, true))
                    {
                        Debug.WriteLine("按下换Tray");
                    }

                    bool x18 = io[15] == "1";  //X018_空盘门感应
                    if (CheckEdgeSingal(x18, ref EmptyDoorRiseCache, true))
                    {
                        Debug.WriteLine("Empty仓门已关闭");
                    }
                    else if (CheckEdgeSingal(x18, ref EmptyDoorFallCache, false))
                    {
                        Debug.WriteLine("Empty仓门被打开");
                    }

                    bool x24 = io[16] == "1"; //X024_Load门感应
                    if (CheckEdgeSingal(x24, ref LoaderDoorRiseCache, true))
                    {
                        Debug.WriteLine("Load仓门已关闭");
                    }
                    else if (CheckEdgeSingal(x24, ref LoaderDoorFallCache, false))
                    {
                        Debug.WriteLine("Load仓门被打开");
                    }
                }
                Thread.Sleep(5);
            }
        }

        //public void 入料()
        //{

        //    while (true)
        //    {
        //        switch (PS入料手臂吸料流程.StepID)
        //        {
        //            case 500:
        //                {

        //                    if (!参数对比(Data_飞梭前入料IC[0], Data_入料目标值[0]))
        //                        PS入料手臂吸料流程.StepID = 510;
        //                    else
        //                        PS入料手臂吸料流程.StepID = 540;
        //                }
        //                break;

        //            case 510:
        //                {
        //                    if (入料手臂抓料等待标记)
        //                        PS入料手臂吸料流程.StepID = 520;
        //                    else
        //                        PS入料手臂吸料流程.StepID = 530;
        //                }
        //                break;
        //            case 520:
        //                {
        //                    入料手臂放料在前梭车入料区1
        //                    入料手臂抓料等待标记 = false;
        //                    PS入料手臂吸料流程.StepID = 540;
        //                }
        //                break;
        //            case 530:
        //                {
        //                    入料手臂从loder区吸料放到前梭车入料区1
        //                        PS入料手臂吸料流程.StepID = 540;
        //                }
        //                break;
        //            case 540:
        //                {
        //                    if (!参数对比(Data_飞梭前入料IC[1], Data_入料目标值[1]))
        //                        PS入料手臂吸料流程.StepID = 550;
        //                    else
        //                        PS入料手臂吸料流程.StepID = 580;
        //                }
        //                break;
        //            case 550:
        //                {
        //                    if (入料手臂抓料等待标记)
        //                        PS入料手臂吸料流程.StepID = 560;
        //                    else
        //                        PS入料手臂吸料流程.StepID = 570;
        //                }
        //                break;
        //            case 560:
        //                {
        //                    入料手臂放料在前梭车入料区2;
        //                    入料手臂抓料等待标记 = false;
        //                    PS入料手臂吸料流程.StepID = 580;
        //                }
        //                break;
        //            case 570:
        //                {
        //                    入料手臂从loder区吸料放到前梭车入料区2
        //                        PS入料手臂吸料流程.StepID = 580;
        //                }
        //                break;
        //            case 580:
        //                {
        //                    入料手臂目标达成
        //                    if (入料手臂抓料等待标记)
        //                    {
        //                        PS入料手臂吸料流程.StepID = 500;
        //                        Thread.Sleep(10);
        //                    }
        //                    else
        //                        PS入料手臂吸料流程.StepID = 590;

        //                }
        //                break;
        //            case 590:
        //                {

        //                    if (参数对比(Data_飞梭后入料IC[0], Data_入料目标值[0]) && 参数对比(Data_飞梭后入料IC[1], Data_入料目标值[1]))
        //                    {
        //                        PS入料手臂吸料流程.StepID = 500;
        //                        Thread.Sleep(10);
        //                    }
        //                    else
        //                        PS入料手臂吸料流程.StepID = 600;
        //                }
        //                break;
        //            case 600:
        //                {
        //                    if (!参数对比(Data_飞梭后入料IC[0], Data_入料目标值[0]))
        //                        入料手臂根据Data_飞梭后入料IC[0]与目标目标值[0]差值吸料
        //                    else
        //                        入料手臂根据Data_飞梭后入料IC[1]与目标目标值[1]差值吸料


        //                    PS入料手臂吸料流程.StepID = 610;
        //                }
        //                break;
        //            case 610:
        //                {
        //                    入料手臂移动至梭车上方;
        //                    入料手臂抓料等待标记 = true;
        //                    PS入料手臂吸料流程.StepID = 500;
        //                    Thread.Sleep(10);
        //                }
        //                break;
        //        }
        //    }
        //}

        #endregion
    }

    //public class 草稿
    //{
    //    public static int 飞梭Shuttle2位置; //1:后面 2:前面

    //    public static int Data_入料手臂state;
    //    public static int Data_出料手臂state;
    //    public static int Data_测试手臂state;
    //    public static int Data_飞梭state;

    //    public static int Data_飞梭前入料state;
    //    public static int Data_飞梭前出料state;
    //    public static int Data_飞梭后入料state;
    //    public static int Data_飞梭后出料state;

    //    public static int[] Data_入料手臂吸料坐标;
    //    public static short Data_吸料飞梭列;

    //    public static bool[][] Data_入料手臂吸料IC;
    //    public static bool[][] Data_出料手臂吸料IC;
    //    public static bool[][] Data_测试手臂吸料IC;
    //    public static bool[][] Data_测试手臂放料IC;

    //    public static bool[][] Data_飞梭前入料IC;
    //    public static bool[][] Data_飞梭前出料IC;
    //    public static bool[][] Data_飞梭后入料IC;
    //    public static bool[][] Data_飞梭后出料IC;

    //    public static string[][] Data_飞梭前出料BIN项;
    //    public static string[][] Data_飞梭后出料BIN项;
    //    public static string[][] Data_测试手臂出料BIN项;

    //    public static int[] Data_Site1_16state;

    //    public static bool[][] Data_入料目标值;

    //    public static bool 入料手臂抓料等待标记 = false;
    //    public void 入料()
    //    {

    //        while (true)
    //        {
    //            switch (PS_入料手臂_Load吸料.StepID)
    //            {
    //                case 500:
    //                    {

    //                        if (!参数对比(Data_飞梭前入料IC[0], Data_入料目标值[0]))
    //                            PS_入料手臂_Load吸料.StepID = 510;
    //                        else
    //                            PS_入料手臂_Load吸料.StepID = 540;
    //                    }
    //                    break;

    //                case 510:
    //                    {
    //                        if (入料手臂抓料等待标记)
    //                            PS_入料手臂_Load吸料.StepID = 520;
    //                        else
    //                            PS_入料手臂_Load吸料.StepID = 530;
    //                    }
    //                    break;
    //                case 520:
    //                    {
    //                        入料手臂放料在前梭车入料区1
    //                        入料手臂抓料等待标记 = false;
    //                        PS_入料手臂_Load吸料.StepID = 540;
    //                    }
    //                    break;
    //                case 530:
    //                    {
    //                        入料手臂从loder区吸料放到前梭车入料区1
    //                            PS_入料手臂_Load吸料.StepID = 540;
    //                    }
    //                    break;
    //                case 540:
    //                    {
    //                        if (!参数对比(Data_飞梭前入料IC[1], Data_入料目标值[1]))
    //                            PS_入料手臂_Load吸料.StepID = 550;
    //                        else
    //                            PS_入料手臂_Load吸料.StepID = 580;
    //                    }
    //                    break;
    //                case 550:
    //                    {
    //                        if (入料手臂抓料等待标记)
    //                            PS_入料手臂_Load吸料.StepID = 560;
    //                        else
    //                            PS_入料手臂_Load吸料.StepID = 570;
    //                    }
    //                    break;
    //                case 560:
    //                    {
    //                        入料手臂放料在前梭车入料区2;
    //                        入料手臂抓料等待标记 = false;
    //                        PS_入料手臂_Load吸料.StepID = 580;
    //                    }
    //                    break;
    //                case 570:
    //                    {
    //                        入料手臂从loder区吸料放到前梭车入料区2
    //                            PS_入料手臂_Load吸料.StepID = 580;
    //                    }
    //                    break;
    //                case 580:
    //                    {
    //                        入料手臂目标达成
    //                        if (入料手臂抓料等待标记)
    //                        {
    //                            PS_入料手臂_Load吸料.StepID = 500;
    //                            Thread.Sleep(10);
    //                        }
    //                        else
    //                            PS_入料手臂_Load吸料.StepID = 590;

    //                    }
    //                    break;
    //                case 590:
    //                    {

    //                        if (参数对比(Data_飞梭后入料IC[0], Data_入料目标值[0]) && 参数对比(Data_飞梭后入料IC[1], Data_入料目标值[1]))
    //                        {
    //                            PS_入料手臂_Load吸料.StepID = 500;
    //                            Thread.Sleep(10);
    //                        }
    //                        else
    //                            PS_入料手臂_Load吸料.StepID = 600;
    //                    }
    //                    break;
    //                case 600:
    //                    {
    //                        if (!参数对比(Data_飞梭后入料IC[0], Data_入料目标值[0]))
    //                            入料手臂根据Data_飞梭后入料IC[0]与目标目标值[0]差值吸料
    //                        else
    //                            入料手臂根据Data_飞梭后入料IC[1]与目标目标值[1]差值吸料


    //                        PS_入料手臂_Load吸料.StepID = 610;
    //                    }
    //                    break;
    //                case 610:
    //                    {
    //                        入料手臂移动至梭车上方;
    //                        入料手臂抓料等待标记 = true;
    //                        PS_入料手臂_Load吸料.StepID = 500;
    //                        Thread.Sleep(10);
    //                    }
    //                    break;
    //            }
    //        }
    //    }

    //    public void 入料手臂吸料_Load()
    //    {
    //        string result = "";
    //        short colums = 0;

    //        short index = 0;

    //        double xPos = 0;
    //        double yPos = 0;

    //        bool[] lsopen = new bool[4];
    //        short[] opens = new short[4];
    //        bool[][] open = new bool[2][];

    //        while (true)
    //        {
    //            switch (PS_入料手臂_Load吸料.StepID)
    //            {
    //                case 500:
    //                    PS_入料手臂_Load吸料.StepName = "路径规划";
    //                    PS_入料手臂_Load吸料.Condition = Data_入料手臂state == 0 && Data_飞梭前入料state == 0;
    //                    PS_入料手臂_Load吸料.Permit = !PS_入料手臂_飞梭放料.Processing;
    //                    if (PS_入料手臂_Load吸料.Condition && PS_入料手臂_Load吸料.Permit)
    //                    {
    //                        colums = 0;
    //                        PS_入料手臂_Load吸料.Processing = true;
    //                        PS_入料手臂_Load吸料.StepID = 505;
    //                    }
    //                    break;

    //                case 505:
    //                    PS_入料手臂_Load吸料.StepName = "检查开Site情况";
    //                    if (Site是否开启(APPmaster.Sitestatus[0]))
    //                    {
    //                        open = APPmaster.Sitestatus[0];
    //                        if (open[colums].Contains(true))
    //                        {
    //                            opens = 吸嘴是否吸料(open, colums);
    //                            PS_入料手臂_Load吸料.StepID = 510;
    //                        }
    //                        colums++;
    //                    }
    //                    break;

    //                case 510:
    //                    PS_入料手臂_Load吸料.StepName = "获取吸料吸嘴";
    //                    APPmaster.获取Load盘Open(opens, out lsopen, out index);
    //                    Debug.WriteLine($"PS_入料手臂吸料_Load.StepID={PS_入料手臂_Load吸料.StepID}->{PS_入料手臂_Load吸料.StepName}");

    //                    break;

    //                case 515:
    //                    PS_入料手臂_Load吸料.StepName = "获取IC位置";
    //                    if (APPmaster.获取Load盘位置(out xPos, out yPos))
    //                    {
    //                        Debug.WriteLine($"PS_左吸料_梭车吸.StepID={PS_入料手臂_Load吸料.StepID}->{PS_入料手臂_Load吸料.StepName}");
    //                        PS_入料手臂_Load吸料.StepID = 515;
    //                    }
    //                    break;

    //                case 520:
    //                    PS_入料手臂_Load吸料.StepName = "入料手臂到Load盘吸料位";
    //                    result = 入料手臂移动(xPos, yPos, 100);
    //                    if (result == "Success")
    //                    {
    //                        Debug.WriteLine($"PS_左吸料_梭车吸.StepID={PS_入料手臂_Load吸料.StepID}->{PS_入料手臂_Load吸料.StepName}");
    //                        PS_入料手臂_Load吸料.StepID = 520;
    //                    }
    //                    break;

    //                case 525:
    //                    PS_入料手臂_Load吸料.StepName = "入料手臂Z向下移动";

    //                    result = 入料手臂Z移动(lsopen, 10, true);
    //                    if (result == "Success")
    //                    {
    //                        Debug.WriteLine($"PS_左吸料_梭车吸.StepID={PS_入料手臂_Load吸料.StepID}->{PS_入料手臂_Load吸料.StepName}");
    //                        PS_入料手臂_Load吸料.StepID = 530;
    //                    }

    //                    break;

    //                case 530:
    //                    PS_入料手臂_Load吸料.StepName = "入料手臂Z向上移动";
    //                    result = 入料手臂Z移动(lsopen, -0.2, false);
    //                    if (result == "Success")
    //                    {
    //                        Debug.WriteLine($"PS_左吸料_梭车吸.StepID={PS_入料手臂_Load吸料.StepID}->{PS_入料手臂_Load吸料.StepName}");
    //                        PS_入料手臂_Load吸料.StepID = 599;
    //                    }
    //                    break;

    //                case 535:
    //                    PS_入料手臂_Load吸料.StepName = "判断入料Z是否在原点";
    //                    string[] axisNames = { "M1_入料手臂Z1轴", "M2_入料手臂Z2轴", "M3_入料手臂Z3轴", "M4_入料手臂Z4轴" };
    //                    result = motionEx.GetDiHome(axisNames, out string[] iovalues);
    //                    if (result == "Success")
    //                    {
    //                        PS_入料手臂_Load吸料.StepID = 599;
    //                    }
    //                    break;

    //                case 599:
    //                    PS_入料手臂_Load吸料.StepName = "入料手臂Load吸料完成";
    //                    PS_入料手臂_Load吸料.Processing = false;
    //                    PS_入料手臂_Load吸料.StepID = 500;
    //                    break;

    //                default:
    //                    Debug.WriteLine(("PS_入料手臂吸料.StepID =" + PS_入料手臂_Load吸料.StepID.ToString() + "对应流程不存在"));
    //                    break;
    //            }

    //            switch (PS_入料手臂_飞梭放料.StepID)
    //            {
    //                case 500:
    //                    PS_入料手臂_飞梭放料.StepName = "路径规划";
    //                    PS_入料手臂_飞梭放料.Condition = Data_入料手臂state == 0 && Data_飞梭前入料state == 0;
    //                    PS_入料手臂_飞梭放料.Permit = !PS_入料手臂_Load吸料.Processing;
    //                    if (PS_入料手臂_飞梭放料.Condition && PS_入料手臂_飞梭放料.Permit)
    //                    {
    //                        PS_入料手臂_飞梭放料.Processing = true;
    //                        PS_入料手臂_飞梭放料.StepID = 505;
    //                    }
    //                    break;

    //                case 510:
    //                    result = 入料手臂移动(100, 100, 100);
    //                    if (result == "Success")
    //                    {

    //                    }
    //                    break;

    //                case 515:
    //                    lsopen = Data_入料手臂吸料IC[Data_吸料飞梭列];
    //                    result = 入料手臂Z移动(lsopen, 100, true);
    //                    if (result == "Success")
    //                    {

    //                    }
    //                    break;

    //                case 520:
    //                    result = 入料吸嘴_吸料(lsopen, false);
    //                    if (result == "Success")
    //                    {
    //                        result = 入料吸嘴_放料(lsopen, true);
    //                        if (result == "Success")
    //                        {
    //                            Thread.Sleep(200);
    //                            result = 入料吸嘴_放料(lsopen, false);
    //                            if (result == "Success")
    //                            {

    //                            }
    //                        }
    //                    }
    //                    break;

    //                case 525:
    //                    result = 入料手臂Z移动(lsopen, -0.2, false);
    //                    if (result == "Success")
    //                    {

    //                    }
    //                    break;

    //                case 530:
    //                    PS_入料手臂_Load吸料.StepName = "判断入料Z是否在原点";
    //                    string[] axisNames = { "M1_入料手臂Z1轴", "M2_入料手臂Z2轴", "M3_入料手臂Z3轴", "M4_入料手臂Z4轴" };
    //                    result = motionEx.GetDiHome(axisNames, out string[] iovalues);
    //                    if (result == "Success")
    //                    {
    //                        PS_入料手臂_飞梭放料.StepID = 599;
    //                    }
    //                    break;

    //                case 599:
    //                    PS_入料手臂_飞梭放料.StepName = "入料手臂_放料飞梭完成";
    //                    PS_入料手臂_飞梭放料.Processing = false;
    //                    PS_入料手臂_飞梭放料.StepID = 999;
    //                    break;

    //                default:
    //                    Debug.WriteLine(("PS_入料手臂_放料飞梭.StepID =" + PS_入料手臂_飞梭放料.StepID.ToString() + "对应流程不存在"));
    //                    break;
    //            }
    //        }
    //    }

    //    public void 飞梭手臂移动()
    //    {
    //        string result;
    //        PS_飞梭手臂移动.StepID = 500;
    //        while (true)
    //        {
    //            switch (PS_飞梭手臂移动.StepID)
    //            {
    //                case 500:
    //                    PS_飞梭手臂移动.StepName = "路径规划";
    //                    PS_飞梭手臂移动.Condition = Data_飞梭前入料state == 2 && Data_飞梭前出料state == 0
    //                                            && Data_飞梭后入料state == 0;

    //                    PS_飞梭手臂移动.Permit = !PS_入料手臂放料_Shuttle.Processing;
    //                    if (PS_飞梭手臂移动.Condition && PS_飞梭手臂移动.Permit)
    //                    {
    //                        Data_飞梭state = 1;
    //                        PS_飞梭手臂移动.Processing = true;
    //                        PS_飞梭手臂移动.StepID = 505;
    //                    }
    //                    break;

    //                case 510:
    //                    result = 飞梭_前后检测产品感应(out string[] iovalues);
    //                    if (result == "Success")
    //                    {
    //                        if (iovalues.Contains("0"))
    //                        {
    //                            if (Data_飞梭前入料state == 2)
    //                            {
    //                                Data_飞梭后入料state = 1;
    //                                Data_飞梭前入料state = 0;
    //                            }
    //                            if (Data_飞梭后出料state == 2)
    //                            {
    //                                Data_飞梭前出料BIN项 = Data_飞梭后出料BIN项;
    //                                Data_飞梭前出料state = 1;
    //                                Data_飞梭后出料state = 0;
    //                            }
    //                            PS_飞梭手臂移动.StepID = 515;
    //                        }
    //                    }
    //                    break;

    //                case 515:
    //                    if (飞梭Shuttle2位置 == 1)
    //                    {
    //                        飞梭Shuttle2位置 = 2;
    //                        result = 飞梭轴移动(60);
    //                    }
    //                    else
    //                    {
    //                        飞梭Shuttle2位置 = 1;
    //                        result = 飞梭轴移动(200);
    //                    }
    //                    if (result == "Success")
    //                    {
    //                        Data_飞梭state = 0;
    //                        PS_飞梭手臂移动.StepID = 599;
    //                    }
    //                    break;

    //                case 599:
    //                    PS_飞梭手臂移动.StepID = 500;
    //                    break;

    //                default:
    //                    Debug.WriteLine(("PS_飞梭手臂移动.StepID =" + PS_飞梭手臂移动.StepID.ToString() + "对应流程不存在"));
    //                    break;
    //            }
    //        }
    //    }

    //    public void 入料手臂_放料飞梭()
    //    {
    //        string result;
    //        bool[] lsopen = new bool[4];
    //        while (true)
    //        {
    //            if (PS_入料手臂_飞梭放料.StepID == 999) break;

    //        }
    //    }

    //    public void 入料手臂吸料_Heat()
    //    {

    //        PS_入料手臂吸料_Heat.StepID = 500;
    //        while (true)
    //        {
    //            switch (PS_入料手臂吸料_Heat.StepID)
    //            {
    //                default:
    //                    Debug.WriteLine(("PS_入料手臂吸料_Heat.StepID =" + PS_入料手臂吸料_Heat.StepID.ToString() + "对应流程不存在"));
    //                    break;
    //            }
    //        }
    //    }

    //    #region 出料手臂流程

    //    public void 出料手臂吸料_Shuttle()
    //    {
    //        string result = "";
    //        short colums = 0;

    //        bool[] lsopen = new bool[4];
    //        short[] opens = new short[4];
    //        bool[][] open = new bool[2][];
    //        string[][] bins = new string[2][];

    //        short index = 0;

    //        PS_出料手臂吸料_Shuttle.StepID = 500;
    //        while (true)
    //        {
    //            switch (PS_出料手臂吸料_Shuttle.StepID)
    //            {
    //                case 500:
    //                    PS_出料手臂吸料_Shuttle.StepName = "路径规划";
    //                    PS_出料手臂吸料_Shuttle.Condition = Data_飞梭前出料state == 1;
    //                    PS_出料手臂吸料_Shuttle.Permit = !PS_入料手臂放料_Shuttle.Processing;
    //                    if (PS_出料手臂吸料_Shuttle.Condition && PS_出料手臂吸料_Shuttle.Permit)
    //                    {
    //                        open = new bool[2][];
    //                        open[0] = new bool[] { Data_飞梭前出料IC[0][0], Data_飞梭前出料IC[0][1], Data_飞梭前出料IC[0][2], Data_飞梭前出料IC[0][3] };
    //                        open[1] = new bool[] { Data_飞梭前出料IC[1][0], Data_飞梭前出料IC[1][1], Data_飞梭前出料IC[1][2], Data_飞梭前出料IC[1][3] };
    //                        bins = new string[2][];
    //                        bins[0] = new string[] { Data_飞梭前出料BIN项[0][0], Data_飞梭前出料BIN项[0][1], Data_飞梭前出料BIN项[0][2], Data_飞梭前出料BIN项[0][3] };
    //                        bins[1] = new string[] { Data_飞梭前出料BIN项[1][0], Data_飞梭前出料BIN项[1][1], Data_飞梭前出料BIN项[1][2], Data_飞梭前出料BIN项[1][3] };

    //                        PS_出料手臂吸料_Shuttle.Processing = true;
    //                        PS_出料手臂吸料_Shuttle.StepID = 505;
    //                    }
    //                    break;

    //                case 505:
    //                    PS_出料手臂吸料_Shuttle.StepName = "检查开Site情况";
    //                    if (Site是否开启(open))
    //                    {
    //                        if (open[colums].Contains(true))
    //                        {
    //                            opens = 吸嘴是否放料(open, colums);
    //                            PS_入料手臂_Load吸料.StepID = 510;
    //                        }
    //                        colums++;
    //                    }
    //                    break;

    //                case 510:
    //                    PS_入料手臂_Load吸料.StepName = "获取吸料吸嘴";
    //                    APPmaster.获取Load盘Open(opens, out lsopen, out index);
    //                    Debug.WriteLine($"PS_入料手臂吸料_Load.StepID={PS_入料手臂_Load吸料.StepID}->{PS_入料手臂_Load吸料.StepName}");
    //                    break;

    //                case 599:
    //                    PS_出料手臂吸料_Shuttle.StepID = 500;
    //                    break;

    //                default:
    //                    Debug.WriteLine(("PS_出料手臂吸料_Shuttle.StepID =" + PS_出料手臂吸料_Shuttle.StepID.ToString() + "对应流程不存在"));
    //                    break;
    //            }
    //        }
    //    }

    //    public void 出料手臂放料_Tray()
    //    {

    //        PS_出料手臂放料_Tray.StepID = 500;
    //        while (true)
    //        {
    //            switch (PS_出料手臂放料_Tray.StepID)
    //            {
    //                default:
    //                    Debug.WriteLine(("PS_出料手臂放料_Tray.StepID =" + PS_出料手臂放料_Tray.StepID.ToString() + "对应流程不存在"));
    //                    break;
    //            }
    //        }
    //    }
    //    #endregion

    //    #region 测试手臂流程

    //    public void 测试手臂吸料_Shuttle()
    //    {
    //        string result;

    //        PS_测试手臂吸料_Shuttle.StepID = 500;
    //        while (true)
    //        {
    //            switch (PS_测试手臂吸料_Shuttle.StepID)
    //            {
    //                case 500:
    //                    PS_测试手臂吸料_Shuttle.StepName = "路径规划";
    //                    PS_测试手臂吸料_Shuttle.Condition = Data_飞梭后入料state == 1;
    //                    PS_测试手臂吸料_Shuttle.Permit = !PS_入料手臂放料_Shuttle.Processing;
    //                    if (PS_测试手臂吸料_Shuttle.Condition && PS_测试手臂吸料_Shuttle.Permit)
    //                    {
    //                        PS_测试手臂吸料_Shuttle.Processing = true;
    //                        PS_测试手臂吸料_Shuttle.StepID = 505;
    //                    }
    //                    break;

    //                case 505:
    //                    result = 测试吸嘴_张卡气缸输出(false);
    //                    if (result == "Success")
    //                    {
    //                        PS_测试手臂吸料_Shuttle.StepID = 510;
    //                    }

    //                    break;

    //                case 510:
    //                    result = 测试手臂X移动(120);
    //                    if (result == "Success")
    //                    {
    //                        PS_测试手臂吸料_Shuttle.StepID = 515;
    //                    }
    //                    break;


    //                case 511:
    //                    result = 测试吸嘴_张开气缸原位输出();
    //                    if (result == "Success")
    //                    {
    //                        PS_测试手臂吸料_Shuttle.StepID = 515;
    //                    }
    //                    break;

    //                case 515:
    //                    result = 测试手臂Z移动(100);
    //                    if (result == "Success")
    //                    {
    //                        PS_测试手臂吸料_Shuttle.StepID = 520;
    //                    }
    //                    break;

    //                case 520:
    //                    result = 测试手臂Z移动(0);
    //                    if (result == "Success")
    //                    {
    //                        PS_测试手臂吸料_Shuttle.StepID = 525;
    //                    }
    //                    break;

    //                case 525:
    //                    result = 测试吸嘴_张卡气缸输出(true);
    //                    if (result == "Success")
    //                    {
    //                        PS_测试手臂吸料_Shuttle.StepID = 510;
    //                    }
    //                    break;

    //                case 530:
    //                    result = 测试手臂X移动(120);
    //                    if (result == "Success")
    //                    {
    //                        PS_测试手臂吸料_Shuttle.StepID = 515;
    //                    }
    //                    break;

    //                case 531:
    //                    result = 测试吸嘴_张开气缸到位输出();
    //                    if (result == "Success")
    //                    {
    //                        PS_测试手臂吸料_Shuttle.StepID = 515;
    //                    }
    //                    break;

    //                case 535:
    //                    result = 测试手臂Z移动(100);
    //                    if (result == "Success")
    //                    {
    //                        PS_测试手臂吸料_Shuttle.StepID = 520;
    //                    }
    //                    break;

    //                case 540:
    //                    result = 测试手臂Z移动(0);
    //                    if (result == "Success")
    //                    {
    //                        PS_测试手臂吸料_Shuttle.StepID = 525;
    //                    }
    //                    break;

    //                case 599:
    //                    PS_测试手臂吸料_Shuttle.StepID = 500;
    //                    break;

    //                default:
    //                    Debug.WriteLine(("PS_测试手臂吸料_Shuttle.StepID =" + PS_测试手臂吸料_Shuttle.StepID.ToString() + "对应流程不存在"));
    //                    break;
    //            }
    //        }
    //    }

    //    public void 测试手臂吸料_TestArea()
    //    {
    //        string result;
    //        PS_测试手臂吸料_TestArea.StepID = 500;
    //        while (true)
    //        {
    //            switch (PS_测试手臂吸料_TestArea.StepID)
    //            {
    //                case 500:
    //                    PS_测试手臂吸料_TestArea.StepName = "路径规划";
    //                    PS_测试手臂吸料_TestArea.Condition = Data_飞梭后入料state == 1;
    //                    PS_测试手臂吸料_TestArea.Permit = !PS_入料手臂放料_Shuttle.Processing;
    //                    if (PS_测试手臂吸料_TestArea.Condition && PS_测试手臂吸料_TestArea.Permit)
    //                    {
    //                        PS_测试手臂吸料_TestArea.Processing = true;
    //                        PS_测试手臂吸料_TestArea.StepID = 505;
    //                    }
    //                    break;

    //                case 505:
    //                    result = 测试吸嘴_张卡气缸输出(true);
    //                    if (result == "Success")
    //                    {
    //                        PS_测试手臂吸料_Shuttle.StepID = 510;
    //                    }

    //                    break;

    //                case 510:
    //                    result = 测试手臂X移动(120);
    //                    if (result == "Success")
    //                    {
    //                        PS_测试手臂吸料_Shuttle.StepID = 515;
    //                    }
    //                    break;


    //                case 511:
    //                    result = 测试吸嘴_张开气缸到位输出();
    //                    if (result == "Success")
    //                    {
    //                        PS_测试手臂吸料_Shuttle.StepID = 515;
    //                    }
    //                    break;

    //                case 515:
    //                    result = 测试手臂Z移动(100);
    //                    if (result == "Success")
    //                    {
    //                        PS_测试手臂吸料_Shuttle.StepID = 520;
    //                    }
    //                    break;

    //                case 520:
    //                    result = 测试手臂Z移动(0);
    //                    if (result == "Success")
    //                    {
    //                        PS_测试手臂吸料_Shuttle.StepID = 525;
    //                    }
    //                    break;

    //                case 525:
    //                    result = 测试吸嘴_张卡气缸输出(true);
    //                    if (result == "Success")
    //                    {
    //                        PS_测试手臂吸料_Shuttle.StepID = 510;
    //                    }
    //                    break;

    //                case 530:
    //                    result = 测试手臂X移动(120);
    //                    if (result == "Success")
    //                    {
    //                        PS_测试手臂吸料_Shuttle.StepID = 515;
    //                    }
    //                    break;

    //                case 531:
    //                    result = 测试吸嘴_张开气缸到位输出();
    //                    if (result == "Success")
    //                    {
    //                        PS_测试手臂吸料_Shuttle.StepID = 515;
    //                    }
    //                    break;

    //                case 535:
    //                    result = 测试手臂Z移动(100);
    //                    if (result == "Success")
    //                    {
    //                        PS_测试手臂吸料_Shuttle.StepID = 520;
    //                    }
    //                    break;

    //                case 540:
    //                    result = 测试手臂Z移动(0);
    //                    if (result == "Success")
    //                    {
    //                        PS_测试手臂吸料_Shuttle.StepID = 525;
    //                    }
    //                    break;

    //                default:
    //                    Debug.WriteLine(("PS_测试手臂吸料_TestArea.StepID =" + PS_测试手臂吸料_TestArea.StepID.ToString() + "对应流程不存在"));
    //                    break;
    //            }
    //        }
    //    }

    //    public void 测试手臂放料_Shuttle()
    //    {

    //        PS_测试手臂放料_Shuttle.StepID = 500;
    //        while (true)
    //        {
    //            switch (PS_测试手臂放料_Shuttle.StepID)
    //            {
    //                default:
    //                    Debug.WriteLine(("PS_测试手臂放料_Shuttle.StepID =" + PS_测试手臂放料_Shuttle.StepID.ToString() + "对应流程不存在"));
    //                    break;
    //            }
    //        }
    //    }

    //    public void 测试手臂放料_TestArea()
    //    {

    //        PS_测试手臂放料_TestArea.StepID = 500;
    //        while (true)
    //        {
    //            switch (PS_测试手臂放料_TestArea.StepID)
    //            {
    //                default:
    //                    Debug.WriteLine(("PS_测试手臂放料_TestArea.StepID =" + PS_测试手臂放料_TestArea.StepID.ToString() + "对应流程不存在"));
    //                    break;
    //            }
    //        }
    //    }

    //    #endregion 测试手臂流程
    //}
}
