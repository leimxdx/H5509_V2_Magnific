using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
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
using static System.Windows.Forms.MonthCalendar;
using static H5509_V2.APPmaster;

namespace H5509_V2
{
    public class Handler
    {
        public MotionEx motionEx = MotionEx.GetInstance();

        #region 轴移动
        public string 入料手臂X移动(double pos)
        {
            return motionEx.AxisAbsoluteMove("M22_入料手臂X轴", pos);
        }

        public string 入料手臂Y移动(double pos)
        {
            return motionEx.AxisAbsoluteMove("M16_入料手臂Y轴", pos);
        }

        public string 入料手臂P移动(double pos)
        {
            return motionEx.AxisAbsoluteMove("M5_入料手臂P轴", pos);
        }

        public string 入料手臂移动(double xPos, double yPos, double pPos)
        {
            double[] pos = new double[] { xPos, yPos, pPos };
            string[] axisNames = new string[] { "M22_入料手臂X轴", "M16_入料手臂Y轴", "M5_入料手臂P轴" };
            return motionEx.AxisMove(axisNames, pos);
        }

        public string 出料手臂X移动(double pos)
        {
            return motionEx.AxisAbsoluteMove("M23_出料手臂X轴", pos);
        }

        public string 出料手臂Y移动(double pos)
        {
            return motionEx.AxisAbsoluteMove("M17_出料手臂Y轴", pos);
        }

        public string 出料手臂P移动(double pos)
        {
            return motionEx.AxisAbsoluteMove("M10_出料手臂P轴", pos);
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
                double zpos = isdown ? pos + 初始点位.入料_Z1_硬件补偿 : pos; //加上硬件补偿
                pPoss.Add(zpos);
                axisNames.Add("M1_入料手臂Z1轴");
            }
            if (isopen[1])
            {
                double zpos = isdown ? pos + 初始点位.入料_Z2_硬件补偿 : pos; //加上硬件补偿
                pPoss.Add(zpos);
                axisNames.Add("M2_入料手臂Z2轴");
            }
            if (isopen[2])
            {
                double zpos = isdown ? pos + 初始点位.入料_Z3_硬件补偿 : pos; //加上硬件补偿
                pPoss.Add(zpos);
                axisNames.Add("M3_入料手臂Z3轴");
            }
            if (isopen[3])
            {
                double zpos = isdown ? pos + 初始点位.入料_Z4_硬件补偿 : pos; //加上硬件补偿
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
            if (isdown) pos = 10;
            else pos = -0.3;

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

        public string 出料手臂Z移动(bool[] isopen, bool isdown)
        {
            List<string> axisNames = new List<string>();
            List<double> pPoss = new List<double>();

            double pos;
            if (isdown) pos = 10;
            else pos = -0.3;

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

        public string 出料手臂Z移动(bool[] isopen, double pos, bool isdown)
        {
            List<string> axisNames = new List<string>();
            List<double> pPoss = new List<double>();

            if (isopen[0])
            {
                double zpos = isdown ? pos + 初始点位.出料_Z1_硬件补偿 : pos; //加上硬件补偿
                pPoss.Add(zpos);
                axisNames.Add("M6_出料手臂Z1轴");
            }
            if (isopen[1])
            {
                double zpos = isdown ? pos + 初始点位.出料_Z2_硬件补偿 : pos; //加上硬件补偿
                pPoss.Add(zpos);
                axisNames.Add("M7_出料手臂Z2轴");
            }
            if (isopen[2])
            {
                double zpos = isdown ? pos + 初始点位.出料_Z3_硬件补偿 : pos; //加上硬件补偿
                pPoss.Add(zpos);
                axisNames.Add("M8_出料手臂Z3轴");
            }
            if (isopen[3])
            {
                double zpos = isdown ? pos + 初始点位.出料_Z4_硬件补偿 : pos; //加上硬件补偿
                pPoss.Add(zpos);
                axisNames.Add("M9_出料手臂Z4轴");
            }

            return motionEx.AxisMove(axisNames.ToArray(), pPoss.ToArray());
        }

        public string 测试手臂X移动(double pos)
        {

            return motionEx.AxisAbsoluteMove("M21_测试手臂X轴", pos);
        }

        public string 测试手臂Z移动(double pos, bool isdown)
        {
            return motionEx.AxisAbsoluteMove("M19_测试手臂Z轴", isdown ? pos : pos);
        }

        public string 放Tary盘轴移动(double pos)
        {
            return motionEx.AxisAbsoluteMove("M18_放Tary盘轴", pos);
        }

        public string 飞梭轴移动(double pos)
        {
            return motionEx.AxisAbsoluteMove("M20_飞梭轴", pos);
        }

        public string Fix3放IC盘轴移动(double pos, int type = 0)
        {
            return motionEx.AxisAbsoluteMove("M11_Fix3放IC轴", pos, type);
        }

        public string Empty顶盘轴移动(double pos)
        {
            return motionEx.AxisAbsoluteMove("M12_Empty顶盘轴", pos);
        }

        public string Empty顶盘轴相对移动(double pos)
        {
            return motionEx.AxisRelativeMove("M12_Empty顶盘轴", pos);
        }

        public string Load顶盘轴移动(double pos)
        {
            return motionEx.AxisAbsoluteMove("M13_Loader顶盘轴", pos);
        }

        public string Load顶盘轴相对移动(double pos)
        {
            return motionEx.AxisRelativeMove("M13_Loader顶盘轴", pos);
        }

        public string Auto1顶盘轴移动(double pos)
        {
            return motionEx.AxisAbsoluteMove("M14_Auto1顶盘轴", pos);
        }

        public string Auto2顶盘轴移动(double pos)
        {
            return motionEx.AxisAbsoluteMove("M15_Auto2顶盘轴", pos);
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

        public string Load顶盘轴回零()
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

        public string 飞梭回零()
        {
            return motionEx.AxisHome("M20_飞梭轴");
        }

        #endregion 轴回零

        #region IO控制与读取

        public string 读取气缸输入信号(string ioName, out string value)
        {
            string result = motionEx.GetDi(ioName, out value);
            if (result != "Success") Debug.WriteLine($"{ioName}读取失败！" + result);
            return result;
        }

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
            if (result != "Success") Debug.WriteLine($"测试吸嘴张开气缸到位输出超时({ms}ms)");

            else Debug.WriteLine($"测试吸嘴张开气缸到位输出成功({ms}ms)");

            return result;
        }

        public string 测试吸嘴张开气缸原位输入读取()
        {
            string result = 读取气缸输入信号("X174_测试吸嘴张开气缸原位", "1", out long ms);

            if (result != "Success") Debug.WriteLine($"测试吸嘴张开气缸原位输出超时({ms}ms)");
            else Debug.WriteLine($"测试吸嘴张开气缸原位输出成功({ms}ms)");

            return result;
        }

        #endregion

        #region  Y050_移Tray上下气缸

        public string 移Tray上下气缸原位输入读取(out string value)
        {
            string result = 读取气缸输入信号("X064_移Tray上下缸上感应", out value);
            if (result != "Success") Debug.WriteLine($"移Tray上下气缸原位输入读取失败！" + result);

            return result;
        }

        public string 移Tray上下气缸到位输入读取()
        {
            string result = 读取气缸输入信号("X065_移Tray上下缸下感应", "1", out long ms);

            if (result != "Success") Debug.WriteLine($"移Tray上下气缸原位输出超时({ms}ms)");
            else Debug.WriteLine($"移Tray上下气缸原位输出成功({ms}ms)");

            return result;
        }

        public string 移Tray上下气缸原位输入读取()
        {
            string result = 读取气缸输入信号("X064_移Tray上下缸上感应", "1", out long ms);

            if (result != "Success") Debug.WriteLine($"移Tray上下气缸到位输出超时({ms}ms)");
            else Debug.WriteLine($"移Tray上下气缸到位输出成功({ms}ms)");

            return result;
        }

        public string 移Tray上下气缸到位输出()
        {
            string result = motionEx.SetDo("Y050_移Tray上下气缸", true);
            if (result != "Success")
            {
                Debug.WriteLine("移Tray上下气缸到位输出失败！" + result);
                return result;
            }

            return 移Tray上下气缸到位输入读取();
        }

        public string 移Tray上下气缸原位输出()
        {
            string result = motionEx.SetDo("Y050_移Tray上下气缸", false);
            if (result != "Success")
            {
                Debug.WriteLine("移Tray上下气缸原位输出失败！" + result);
                return result;
            }

            return 移Tray上下气缸原位输入读取();
        }

        #endregion

        #region Y051_移Tray夹爪气缸

        public string 移Tray夹爪气缸原位输入读取(out string value)
        {
            string result = 读取气缸输入信号("X066_移Tray夹紧缸松开感应", out value);
            if (result != "Success") Debug.WriteLine($"移Tray夹爪气缸原位输入读取失败！" + result);

            return result;
        }

        public string 移Tray夹爪气缸到位输入读取(out string value)
        {
            string result = 读取气缸输入信号("X067_移Tray夹紧缸夹紧感应", out value);
            if (result != "Success") Debug.WriteLine($"移Tray夹爪气缸到位输入读取失败！" + result);

            return result;
        }

        public string 移Tray夹爪气缸到位输入读取()
        {
            string result = 读取气缸输入信号("X067_移Tray夹紧缸夹紧感应", "1", out long ms);

            if (result != "Success") Debug.WriteLine($"移Tray夹爪气缸到位输出超时({ms}ms)");
            else Debug.WriteLine($"移Tray夹爪气缸到位输出成功({ms}ms)");

            return result;
        }

        public string 移Tray夹爪气缸原位输入读取()
        {
            string result = 读取气缸输入信号("X066_移Tray夹紧缸松开感应", "1", out long ms);

            if (result != "Success") Debug.WriteLine($"移Tray夹爪气缸原位输出超时({ms}ms)");
            else Debug.WriteLine($"移Tray夹爪气缸原位输出成功({ms}ms)");

            return result;
        }

        public string 移Tray夹爪气缸到位输出()
        {
            string result = motionEx.SetDo("Y051_移Tray夹爪气缸", true);
            if (result != "Success")
            {
                Debug.WriteLine("移Tray夹爪气缸到位输出失败！" + result);
                return result;
            }

            return 移Tray夹爪气缸到位输入读取();
        }

        public string 移Tray夹爪气缸原位输出()
        {
            string result = motionEx.SetDo("Y051_移Tray夹爪气缸", false);
            if (result != "Success")
            {
                Debug.WriteLine("移Tray夹爪气缸原位输出失败！" + result);
                return result;
            }

            return 移Tray夹爪气缸原位输入读取();
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
            if (result != "Success") Debug.WriteLine($"{yIoName}到位输出超时({ms}ms)");
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
            if (result != "Success") Debug.WriteLine($"{yIoName}原位输出超时({ms}ms)");
            else Debug.WriteLine($"{yIoName}原位输出成功({ms}ms)");
            return result;
        }

        public string 测试区_下压气缸到位输出(string yIoName, string xIoName)
        {
            string result = motionEx.SetDo(yIoName, false);
            if (result != "Success")
            {
                Debug.WriteLine($"{yIoName}到位输出失败！" + result);
                return result;
            }

            result = 读取气缸输入信号(xIoName, "0", out long ms);
            if (result != "Success") Debug.WriteLine($"{yIoName}到位输出超时({ms}ms)");
            else Debug.WriteLine($"{yIoName}到位输出成功({ms}ms)");
            return result;
        }

        public string 测试区_下压气缸原位输出(string yIoName, string xIoName)
        {
            string result = motionEx.SetDo(yIoName, true);
            if (result != "Success")
            {
                Debug.WriteLine($"{yIoName}原位输出失败！" + result);
                return result;
            }
            result = 读取气缸输入信号(xIoName, "1", out long ms);

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

        public string 测试区_下压气缸原位输出(short site)
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

        public string 测试区_下压气缸到位输出(short site)
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

        public string Auto1_Tray前侧定位气缸到位输出()
        {
            string result = motionEx.SetDo("Y023_AUTO1_Tray前定位", true);
            if (result != "Success")
            {
                Debug.WriteLine("Auto1_Tray前定位气缸到位输出失败！" + result);
                return result;
            }

            string result2 = motionEx.SetDo("Y024_AUTO1_Tray侧定位", true);
            if (result2 != "Success")
            {
                Debug.WriteLine("Auto1_Tray侧定位气缸到位输出失败！" + result2);
                return result2;
            }

            Thread.Sleep(300); //没有感应器，固定延时处理
            return result;
        }

        public string Auto1_Tray前侧定位气缸原位输出()
        {
            string result = motionEx.SetDo("Y023_AUTO1_Tray前定位", false);
            if (result != "Success")
            {
                Debug.WriteLine("Auto1_Tray前定位气缸原位输出失败！" + result);
                return result;
            }

            string result2 = motionEx.SetDo("Y024_AUTO1_Tray侧定位", false);
            if (result2 != "Success")
            {
                Debug.WriteLine("Auto1_Tray侧定位气缸原位输出失败！" + result2);
                return result2;
            }

            Thread.Sleep(300); //没有感应器，固定延时处理
            return result;
        }

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

        public string Auto2_Tray前侧定位气缸到位输出()
        {
            string result = motionEx.SetDo("Y025_AUTO2_Tray前定位", true);
            if (result != "Success")
            {
                Debug.WriteLine("Auto2_Tray前定位气缸到位输出失败！" + result);
                return result;
            }

            string result2 = motionEx.SetDo("Y026_AUTO2_Tray侧定位", true);
            if (result2 != "Success")
            {
                Debug.WriteLine("Auto2_Tray侧定位气缸到位输出失败！" + result2);
                return result2;
            }

            Thread.Sleep(300); //没有感应器，固定延时处理
            return result;
        }

        public string Auto2_Tray前侧定位气缸原位输出()
        {
            string result = motionEx.SetDo("Y025_AUTO2_Tray前定位", false);
            if (result != "Success")
            {
                Debug.WriteLine("Auto2_Tray前定位气缸原位输出失败！" + result);
                return result;
            }

            string result2 = motionEx.SetDo("Y026_AUTO2_Tray侧定位", false);
            if (result2 != "Success")
            {
                Debug.WriteLine("Auto2_Tray侧定位气缸原位输出失败！" + result2);
                return result2;
            }

            Thread.Sleep(300); //没有感应器，固定延时处理
            return result;
        }

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

        public string Empty向上移动()
        {
            string axisName = "M12_Empty顶盘轴";

            string result = motionEx.AxisJogMove(axisName, 20);
            if (result != "Success") return result;

            while (true)
            {
                if (APPmaster.IsPause)
                {
                    motionEx.AxisStopMove(axisName, 1);
                    while (true)
                    {
                        if (APPmaster.IsPause) Thread.Sleep(10);
                        else
                        {
                            result = motionEx.AxisJogMove(axisName, 20);
                            if (result != "Success") return result;
                            break;
                        }
                    }
                }
                else
                {
                    result = Empty_前Tray存在感应(out string value);
                    if (result != "Success")
                    {
                        motionEx.AxisStopMove(axisName, 1);
                        return result;
                    }

                    if (value == "0") //返回0是挡到感应器
                    {
                        motionEx.AxisStopMove(axisName, 1);
                        return "Success";
                    }

                    result = Empty_后Tray放歪感应(out string value2);
                    if (result != "Success")
                    {
                        motionEx.AxisStopMove(axisName, 1);
                        return result;
                    }

                    result = motionEx.GetCmdEncPos(axisName, out double prfpos);
                    if (result != "Success")
                    {
                        motionEx.AxisStopMove(axisName, 1);
                        return result;
                    }

                    if (prfpos > 空盘上顶最大值 || value2 == "0")
                    {
                        motionEx.AxisStopMove(axisName, 1);
                        //Thread.Sleep(300);//解决轴:Empty第1次回零失败，详细信息：正在运动
                        return $"{axisName}当前上升行程{prfpos}>{空盘上顶最大值} || {value2}";
                    }

                    Thread.Sleep(1); //减少轴卡和CPU压力
                }
            }
        }

        public string Empty向下移动()
        {
            string axisName = "M12_Empty顶盘轴";

            string result = motionEx.AxisJogMove(axisName, -5);
            if (result != "Success") return result;

            while (true)
            {
                if (APPmaster.IsPause)
                {
                    motionEx.AxisStopMove(axisName, 1);
                    while (true)
                    {
                        if (APPmaster.IsPause) Thread.Sleep(10);
                        else
                        {
                            result = motionEx.AxisJogMove(axisName, -5);
                            if (result != "Success") return result;
                            break;
                        }
                    }
                }
                else
                {
                    result = Empty_前Tray存在感应(out string value);
                    if (result != "Success")
                    {
                        motionEx.AxisStopMove(axisName, 1);
                        return result;
                    }

                    if (value == "1")
                    {
                        motionEx.AxisStopMove(axisName, 1);
                        return "Success";
                    }

                    result = motionEx.GetCmdEncPos(axisName, out double prfpos);
                    if (result != "Success")
                    {
                        motionEx.AxisStopMove(axisName, 1);
                        return result;
                    }

                    if (prfpos < 空盘下降最小值)
                    {
                        motionEx.AxisStopMove(axisName, 1);
                        //Thread.Sleep(300);//解决轴:Empty第1次回零失败，详细信息：正在运动
                        return $"{axisName}当前下降行程{prfpos} < {空盘下降最小值}";
                    }
                }
            }
        }

        public string Load向上移动()
        {
            string axisName = "M13_Loader顶盘轴";

            string result = motionEx.AxisJogMove(axisName, 20);
            if (result != "Success") return result;

            while (true)
            {
                if (APPmaster.IsPause)
                {
                    motionEx.AxisStopMove(axisName, 1);
                    while (true)
                    {
                        if (APPmaster.IsPause) Thread.Sleep(10);
                        else
                        {
                            result = motionEx.AxisJogMove(axisName, 20);
                            if (result != "Success") return result;
                            break;
                        }
                    }
                }
                else
                {
                    result = Load_前Tray存在感应(out string value);
                    if (result != "Success")
                    {
                        motionEx.AxisStopMove(axisName, 1);
                        return result;
                    }

                    if (value == "0")
                    {
                        motionEx.AxisStopMove(axisName, 1);
                        return "Success";
                    }

                    result = Load_后Tray放歪感应(out string value2);
                    if (result != "Success")
                    {
                        motionEx.AxisStopMove(axisName, 1);
                        return result;
                    }

                    result = motionEx.GetCmdEncPos(axisName, out double prfpos);
                    if (result != "Success")
                    {
                        motionEx.AxisStopMove(axisName, 1);
                        return result;
                    }

                    if (prfpos > 待测盘上顶最大值 || value2 == "0")
                    {
                        motionEx.AxisStopMove(axisName, 1);
                        //Thread.Sleep(300);//解决轴:Empty第1次回零失败，详细信息：正在运动
                        return $"{axisName}当前上升行程{prfpos}>{待测盘上顶最大值} || {value2}";
                    }
                    Thread.Sleep(1); //减少轴卡和CPU压力
                }
            }
        }

        public string Load向下移动()
        {
            string axisName = "M13_Loader顶盘轴";

            string result = motionEx.AxisJogMove(axisName, -5);
            if (result != "Success") return result;

            while (true)
            {
                if (APPmaster.IsPause)
                {
                    motionEx.AxisStopMove(axisName, 1);
                    while (true)
                    {
                        if (APPmaster.IsPause) Thread.Sleep(10);
                        else
                        {
                            result = motionEx.AxisJogMove(axisName, -5);
                            if (result != "Success") return result;
                            break;
                        }
                    }
                }
                else
                {
                    result = Load_前Tray存在感应(out string value);
                    if (result != "Success")
                    {
                        motionEx.AxisStopMove(axisName, 1);
                        return result;
                    }

                    if (value == "1")
                    {
                        motionEx.AxisStopMove(axisName, 1);
                        return "Success";
                    }

                    result = motionEx.GetCmdEncPos(axisName, out double prfpos);
                    if (result != "Success")
                    {
                        motionEx.AxisStopMove(axisName, 1);
                        return result;
                    }

                    if (prfpos < 30000)
                    {
                        motionEx.AxisStopMove(axisName, 1);
                        return $"{axisName}当前下降行程{prfpos} < 30000";
                    }
                }
            }
        }

        public string Auto1向上移动()
        {
            string axisName = "M14_Auto1顶盘轴";
            string result = motionEx.AxisJogMove(axisName, 20);
            if (result != "Success") return result;
            while (true)
            {
                if (APPmaster.IsPause)
                {
                    motionEx.AxisStopMove(axisName, 1);
                    while (true)
                    {
                        if (APPmaster.IsPause) Thread.Sleep(10);
                        else
                        {
                            result = motionEx.AxisJogMove(axisName, 20);
                            if (result != "Success") return result;
                            break;
                        }
                    }
                }
                else
                {
                    result = Auto1_后Tray放歪感应(out string value);
                    if (result != "Success")
                    {
                        motionEx.AxisStopMove(axisName, 1);
                        return result;
                    }

                    if (value == "0")
                    {
                        motionEx.AxisStopMove(axisName, 1);
                        return "Success";
                    }

                    result = motionEx.GetCmdEncPos(axisName, out double prfpos);
                    if (result != "Success")
                    {
                        motionEx.AxisStopMove(axisName, 1);
                        return result;
                    }
                    if (prfpos > Auto1上顶最大值)
                    {
                        motionEx.AxisStopMove(axisName, 1);
                        //Thread.Sleep(300);//解决轴:Empty第1次回零失败，详细信息：正在运动
                        return $"{axisName}当前上升行程{prfpos}>{Auto1上顶最大值}";
                    }
                    Thread.Sleep(1); //减少轴卡和CPU压力
                }
            }
        }

        public string Auto1向下移动()
        {
            bool iostatus = false;

            string axisName = "M14_Auto1顶盘轴";

            string result = Auto1_前Tray存在感应(out string value);
            if (result != "Success")
            {
                motionEx.AxisStopMove(axisName, 1);
                return result;
            }

            if (value == "1") //1:未挡到感应器
            {
                iostatus = true; //当只有一个盘时
            }

            result = motionEx.AxisJogMove(axisName, -15);
            if (result != "Success") return result;

            while (true)
            {
                if (APPmaster.IsPause)
                {
                    motionEx.AxisStopMove(axisName, 1);
                    while (true)
                    {
                        if (APPmaster.IsPause) Thread.Sleep(10);
                        else
                        {
                            result = motionEx.AxisJogMove(axisName, -15);
                            if (result != "Success") return result;
                            break;
                        }
                    }
                }
                else
                {
                    result = Auto1_前Tray存在感应(out value);
                    if (result != "Success")
                    {
                        motionEx.AxisStopMove(axisName, 1);
                        return result;
                    }

                    if (iostatus)
                    {
                        if (value == "0") //0:挡到感应器
                        {
                            iostatus = false;
                        }
                    }
                    else
                    {
                        if (value == "1") //1:未挡到感应器
                        {
                            motionEx.AxisStopMove(axisName, 1);
                            return "Success";
                        }
                    }

                    result = motionEx.GetCmdEncPos(axisName, out double prfpos);
                    if (result != "Success")
                    {
                        motionEx.AxisStopMove(axisName, 1);
                        return result;
                    }

                    if (prfpos < Auto1下降最小值)
                    {
                        motionEx.AxisStopMove(axisName, 1);
                        //Thread.Sleep(300);//解决轴:Empty第1次回零失败，详细信息：正在运动
                        return $"{axisName}当前下降行程{prfpos} < {Auto1下降最小值}";
                    }
                }
            }
        }

        public string Auto2向上移动()
        {
            string axisName = "M15_Auto2顶盘轴";
            string result = motionEx.AxisJogMove(axisName, 20);
            if (result != "Success") return result;
            while (true)
            {
                if (APPmaster.IsPause)
                {
                    motionEx.AxisStopMove(axisName, 1);
                    while (true)
                    {
                        if (APPmaster.IsPause) Thread.Sleep(10);
                        else
                        {
                            result = motionEx.AxisJogMove(axisName, 20);
                            if (result != "Success") return result;
                            break;
                        }
                    }
                }
                else
                {
                    result = Auto2_后Tray放歪感应(out string value);
                    if (result != "Success")
                    {
                        motionEx.AxisStopMove(axisName, 1);
                        return result;
                    }

                    if (value == "0")
                    {
                        motionEx.AxisStopMove(axisName, 1);
                        return "Success";
                    }

                    result = motionEx.GetCmdEncPos(axisName, out double prfpos);
                    if (result != "Success")
                    {
                        motionEx.AxisStopMove(axisName, 1);
                        return result;
                    }
                    if (prfpos > Auto2上顶最大值)
                    {
                        motionEx.AxisStopMove(axisName, 1);
                        //Thread.Sleep(300);//解决轴:Empty第1次回零失败，详细信息：正在运动
                        return $"{axisName}当前上升行程{prfpos}>{Auto2上顶最大值}";
                    }

                    Thread.Sleep(1); //减少轴卡和CPU压力
                }
            }
        }

        public string Auto2向下移动()
        {
            bool iostatus = false;

            string axisName = "M15_Auto2顶盘轴";

            string result = Auto2_前Tray存在感应(out string value);
            if (result != "Success")
            {
                motionEx.AxisStopMove(axisName, 1);
                return result;
            }

            if (value == "1") //1:未挡到感应器
            {
                iostatus = true; //当只有一个盘时
            }

            result = motionEx.AxisJogMove(axisName, -15);
            if (result != "Success") return result;

            while (true)
            {
                if (APPmaster.IsPause)
                {
                    motionEx.AxisStopMove(axisName, 1);
                    while (true)
                    {
                        if (APPmaster.IsPause) Thread.Sleep(10);
                        else
                        {
                            result = motionEx.AxisJogMove(axisName, -15);
                            if (result != "Success") return result;
                            break;
                        }
                    }
                }
                else
                {
                    result = Auto2_前Tray存在感应(out value);
                    if (result != "Success")
                    {
                        motionEx.AxisStopMove(axisName, 1);
                        return result;
                    }

                    if (iostatus)
                    {
                        if (value == "0") //0:挡到感应器
                        {
                            iostatus = false;
                        }
                    }
                    else
                    {
                        if (value == "1") //1:未挡到感应器
                        {
                            motionEx.AxisStopMove(axisName, 1);
                            return "Success";
                        }
                    }

                    result = motionEx.GetCmdEncPos(axisName, out double prfpos);
                    if (result != "Success")
                    {
                        motionEx.AxisStopMove(axisName, 1);
                        return result;
                    }

                    if (prfpos < Auto2下降最小值)
                    {
                        motionEx.AxisStopMove(axisName, 1);
                        //Thread.Sleep(300);//解决轴:Empty第1次回零失败，详细信息：正在运动
                        return $"{axisName}当前下降行程{prfpos} < {Auto2下降最小值}";
                    }
                }
            }
        }

        #region 入料手臂流程

        public void 参数初始化()
        {
            Empty上顶标记 = 1;
            Load上顶标记 = 1;
            Auto1上顶标记 = 1;
            Auto2上顶标记 = 1;

            飞梭前入料IC = new bool[][] { new bool[] { false, false, false, false }, new bool[] { false, false, false, false } };
            //飞梭前出料IC = new bool[][] { new bool[] { true, true, true, true }, new bool[] { true, true, true, true } };
            飞梭前出料IC = new bool[][] { new bool[] { false, false, false, false }, new bool[] { false, false, false, false } };
            飞梭后入料IC = new bool[][] { new bool[] { false, false, false, false }, new bool[] { false, false, false, false } };
            飞梭后出料IC = new bool[][] { new bool[] { false, false, false, false }, new bool[] { false, false, false, false } };

            放料IC目标值 = new bool[][] { new bool[] { true, true, true, true }, new bool[] { true, true, true, true } };
            //飞梭前出料BIN项 = new string[][] { new string[] { "01", "01", "01", "01" }, new string[] { "01", "01", "01", "01" } };
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
        }

        private void 交换飞梭前后出料BIN数据()
        {
            飞梭前出料BIN项 = new string[飞梭后出料BIN项.Length][];
            for (int i = 0; i < 飞梭后出料BIN项.Length; i++)
            {
                飞梭前出料BIN项[i] = new string[飞梭后出料BIN项[i].Length];
                Array.Copy(飞梭后出料BIN项[i], 飞梭前出料BIN项[i], 飞梭后出料BIN项[i].Length);
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
            Task<string> task1 = Task.Run(() => { return 入料手臂Z回零(new bool[] { true, true, true, true }); });
            Task<string> task2 = Task.Run(() => { return 出料手臂Z回零(new bool[] { true, true, true, true }); });
            Task<string> task3 = Task.Run(() => { return 测试手臂Z回零(); });
            if (task1.Result != "Success" || task2.Result != "Success" || task3.Result != "Success")
            {
                return "Fail";
            }

            string result = motionEx.GetDiHome(new string[] { "M18_放Tary盘轴" }, out string[] home);
            if (result != "Success")
            {
                Debug.WriteLine("读取放Tary盘轴原点信号失败！" + result);
                return result;
            }

            if (home[0] == "0") //如果挡住了原点则出入料手臂同时回零
            {
                task1 = Task.Run(() => { return 入料手臂回零(); });
                task2 = Task.Run(() => { return 出料手臂回零(); });
            }
            else //没有挡住原点则出入料依次回零
            {
                task1 = Task.Run(() => { return 入料手臂依次回零(); });
                task2 = Task.Run(() => { return 出料手臂依次回零(); });
            }

            task3 = Task.Run(() => { return 测试手臂X回零(); });

            Task<string> task4 = Task.Run(() => { return 顶盘轴回零(); });

            Task<string> task5 = Task.Run(() => { return 飞梭回零(); });

            Task<string> task6 = Task.Run(() => { return Fix3放IC轴回零(); });

            if (task1.Result != "Success" || task2.Result != "Success" || task3.Result != "Success" || task4.Result != "Success" || task5.Result != "Success" || task6.Result != "Success")
            {
                return "Fail";
            }

            task1 = Task.Run(() => { return 放Tary盘轴回零(); });

            if (task1.Result != "Success")
            {
                return "Fail";
            }

            return "Success";

            //所有Z先回零
            //string one = 入料手臂Z回零(new bool[] { true, true, true, true });
            //string one1 = 出料手臂Z回零(new bool[] { true, true, true, true });
            //string one2 = 测试手臂Z回零();
            //if (one == "Success" && one1 == "Success" && one2 == "Success")
            //{
            //    string two = 入料手臂依次回零();
            //    string two1 = 出料手臂依次回零();
            //    string two2 = 飞梭轴回零();
            //    string two3 = 放Tary盘轴回零();
            //    if (two == "Success" && two1 == "Success" && two2 == "Success" && two3 == "Success")
            //    {
            //        string three = 放Tary盘轴回零();
            //        if (three == "Success")
            //        {
            //            return "Success";
            //        }
            //        else return three;
            //    }
            //    else
            //    {
            //        if (two != "Success") return two;
            //        else if (two1 != "Success") return two1;
            //        else if (two2 != "Success") return two2;
            //        else return two3;
            //    }
            //}
            //else
            //{
            //    if (one != "Success") return one;
            //    else if (one1 != "Success") return one1;
            //    else return one2;
            //}
        }

        public void 初始化流程()
        {
            PS初始化流程.StepID = 500;
            while (true)
            {
                Thread.Sleep(10);

                if (!是否可以初始化())
                {
                    continue;
                }

                switch (PS初始化流程.StepID)
                {
                    case 500:
                        PS初始化流程.StepName = "路径规划";
                        PS初始化流程.Condition = !H5509初始化 && !H5509正在初始化 && 设备状态 == 1;
                        PS初始化流程.Permit = !PS初始化流程.Processing;
                        if (PS初始化流程.Condition && PS初始化流程.Permit)
                        {
                            H5509正在初始化 = true;
                            Empty上顶标记 = 2;
                            Load上顶标记 = 2;
                            PS初始化流程.Processing = true;
                            PS初始化流程.StepID = 505;
                        }
                        break;

                    case 505:
                        PS初始化流程.StepName = "各轴移动到初始点";
                        Debug.WriteLine($"PS初始化流程.StepID={PS初始化流程.StepID}->{PS初始化流程.StepName}");
                        Task<string> auto1 = Task.Run(() => { return Auto1顶盘轴移动(183); });
                        Task<string> auto2 = Task.Run(() => { return Auto2顶盘轴移动(183); });//182.715
                        Task<string> testp = Task.Run(() => { return 测试吸嘴_张开气缸输出(true); });
                        Task<string> testx = Task.Run(() => { return 测试手臂X移动(初始点位.测试_梭车1吸_X); });
                        Task<string> shuttle = Task.Run(() => { return 飞梭轴移动(初始点位.梭车2_后_Y); });
                        Task<string> inputp = Task.Run(() => { return 入料手臂P移动(7.5); });
                        Task<string> outputxyp = Task.Run(() => { return 出料手臂移动(256.05, 222.55, 5.3); });
                        Task<string> fix3 = Task.Run(() => { return Fix3放IC盘轴移动(初始点位.出料_Fix3_右移动); });
                        if (auto1.Result == "Success" && auto2.Result == "Success" && testp.Result == "Success" && testx.Result == "Success" && shuttle.Result == "Success" && inputp.Result == "Success" && outputxyp.Result == "Success" && fix3.Result == "Success")
                        {
                            飞梭2位置 = 0;
                            Tray平移左右标记 = 1;
                            PS初始化流程.StepID = 510;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            if (auto1.Result != "Success") Debug.WriteLine("Auto1顶盘轴移动失败!" + auto1.Result);
                            if (auto2.Result != "Success") Debug.WriteLine("Auto2顶盘轴移动失败!" + auto2.Result);
                            if (testp.Result != "Success") Debug.WriteLine("测试吸嘴_张开气缸闭合失败!" + testp.Result);
                            if (testx.Result != "Success") Debug.WriteLine("测试手臂X移动失败!" + testx.Result);
                            if (shuttle.Result != "Success") Debug.WriteLine("入料手臂P移动失败!" + shuttle.Result);
                            if (inputp.Result != "Success") Debug.WriteLine("入料手臂P移动失败!" + inputp.Result);
                            if (outputxyp.Result != "Success") Debug.WriteLine("出料手臂移动失败!" + outputxyp.Result);
                            if (fix3.Result != "Success") Debug.WriteLine("Fix3放IC盘轴移动失败!" + fix3.Result);
                        }
                        break;

                    case 510:
                        PS初始化流程.StepName = "检测Auto1/Auto2是否有盘";
                        Debug.WriteLine($"PS初始化流程.StepID={PS初始化流程.StepID}->{PS初始化流程.StepName}");
                        string result = Auto1_前Tray存在感应(out string value);
                        string result2 = Auto2_前Tray存在感应(out string value2);
                        if (result == "Success" && result2 == "Success")
                        {
                            if (value == "1" && value2 == "1") //返回0是挡到感应器,1是没有挡到感应器
                            {
                                PS初始化流程.StepID = 515;
                                Debug.WriteLine($"PS初始化流程.StepID={PS初始化流程.StepID}->等待Empty上顶");
                            }
                            else //没有挡到感应器或者放歪感应器被挡到说明没有放盘成功，需重新放盘
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;

                                if (value != "0")
                                {
                                    Debug.WriteLine($"Auto1前有盘感应到有Tray盘,value:" + value);
                                }
                                if (value2 != "0")
                                {
                                    Debug.WriteLine($"Auto2前有盘感应到有Tray盘,value:" + value2);
                                }
                            }
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            if (result != "Success") Debug.WriteLine($"读取Auto1_前Tray存在感应失败！" + result);
                            if (result2 != "Success") Debug.WriteLine($"读取Auto2_前Tray存在感应失败！" + result2);
                        }
                        break;

                    case 515:
                        PS初始化流程.StepName = "等待Empty上顶";
                        if (Empty上顶标记 == 1)
                        {
                            Auto等待换盘标记 = true;
                            抓盘手臂放空盘到Auto1标记 = 2;
                            PS初始化流程.StepID = 520;
                            Debug.WriteLine($"PS初始化流程.StepID={PS初始化流程.StepID}->抓盘手臂放空盘到Auto1");
                        }
                        else Thread.Sleep(10);
                        break;

                    case 520:
                        PS初始化流程.StepName = "抓盘手臂放空盘到Auto1";
                        if (抓盘手臂放空盘到Auto1标记 == 1)
                        {
                            抓盘手臂放空盘到Auto2标记 = 2;
                            PS初始化流程.StepID = 525;
                            Debug.WriteLine($"PS初始化流程.StepID={PS初始化流程.StepID}->抓盘手臂放空盘到Auto2");
                        }
                        else Thread.Sleep(10);
                        break;

                    case 525:
                        PS初始化流程.StepName = "抓盘手臂放空盘到Auto2";
                        if (抓盘手臂放空盘到Auto2标记 == 1)
                        {
                            PS初始化流程.StepID = 530;
                            Debug.WriteLine($"PS初始化流程.StepID={PS初始化流程.StepID}->等待Load上顶");
                        }
                        else Thread.Sleep(10);
                        break;

                    case 530:
                        PS初始化流程.StepName = "等待Load上顶";
                        if (Load上顶标记 == 1)
                        {
                            Auto等待换盘标记 = false;
                            H5509初始化 = true;
                            H5509正在初始化 = false;
                            PS初始化流程.StepID = 599;
                        }
                        else Thread.Sleep(10);
                        break;

                    case 599:
                        PS初始化流程.StepName = $"初始化流程完成";
                        Debug.WriteLine($"PS初始化流程.StepID={PS初始化流程.StepID}->{PS初始化流程.StepName}");
                        PS初始化流程.Processing = false;
                        PS初始化流程.StepID = 500;
                        break;

                    default:
                        Debug.WriteLine(("PS初始化流程.StepID =" + PS初始化流程.StepID.ToString() + "对应流程不存在"));
                        break;
                }
            }
        }

        public void 结批流程()
        {
            PS结批流程.StepID = 500;
            while (true)
            {
                Thread.Sleep(10);

                if (!是否可以初始化())
                {
                    continue;
                }

                switch (PS结批流程.StepID)
                {
                    case 500:
                        PS结批流程.StepName = "路径规划";
                        PS结批流程.Condition = 结批标记 && !正在结批 && 设备状态 == 1;
                        PS结批流程.Permit = !PS结批流程.Processing;
                        if (PS结批流程.Condition && PS结批流程.Permit)
                        {
                            正在结批 = true;
                            PS结批流程.Processing = true;
                            PS结批流程.StepID = 505;
                        }
                        break;

                    case 505:
                        PS结批流程.StepName = $"停止入料，等待出料完成";
                        if (OutSiteQueue.Count == 0)
                        {

                        }
                        Thread.Sleep(10);
                        break;

                    case 599:
                        PS结批流程.StepName = $"结批流程完成";
                        Debug.WriteLine($"PS结批流程.StepID={PS结批流程.StepID}->{PS结批流程.StepName}");
                        PS结批流程.Processing = false;
                        PS结批流程.StepID = 500;
                        break;

                    default:
                        Debug.WriteLine(("PS结批流程.StepID =" + PS结批流程.StepID.ToString() + "对应流程不存在"));
                        break;
                }
            }
        }

        public void 入料手臂吸放料流程()
        {
            string result = "";
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

            bool firstdraw = true; //是否第一次重吸
            bool firstdzhome = true; //是否第一次Z回原
            Stopwatch stopwatch = new Stopwatch();
            PS入料手臂吸放料流程.StepID = 500;
            while (true)
            {
                Thread.Sleep(10);

                if (!是否可以开始运行())
                {
                    continue;
                }

                switch (PS入料手臂吸放料流程.StepID)
                {
                    case 500:
                        PS入料手臂吸放料流程.StepName = "路径规划";
                        PS入料手臂吸放料流程.Condition = Load上顶标记 == 1 && !Auto1换盘标记 && !Auto2换盘标记 && !结批标记;
                        PS入料手臂吸放料流程.Permit = !PS入料手臂放料流程.Processing;
                        if (PS入料手臂吸放料流程.Condition && PS入料手臂吸放料流程.Permit)
                        {
                            PS入料手臂吸放料流程.Processing = true;
                            PS入料手臂吸放料流程.StepID = 505;
                        }
                        else
                        {
                            if (Auto1换盘标记 || Auto2换盘标记)
                            {
                                PS入料手臂吸放料流程.StepID = 905;
                            }
                            Thread.Sleep(10);
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
                        else if (!参数对比(飞梭前入料IC[1], 放料IC目标值[1]))
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
                                if (飞梭前入料状态 == 0)
                                {
                                    飞梭前入料状态 = 1; //入料手臂目标达成
                                    PS入料手臂吸放料流程.StepID = 500;
                                    PS入料手臂吸放料流程.Processing = false;
                                }
                                else PS入料手臂吸放料流程.StepID = 510;
                            }
                            else Thread.Sleep(10);
                        }
                        break;

                    case 510:
                        if (参数对比(飞梭后入料IC[0], 放料IC目标值[0]) || 参数对比(飞梭后入料IC[1], 放料IC目标值[1]))
                        {
                            PS入料手臂吸放料流程.StepName = "梭车后入料IC被取走";
                            Thread.Sleep(10);
                            PS入料手臂吸放料流程.StepID = 500;
                            PS入料手臂吸放料流程.Processing = false;
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
                        Debug.WriteLine($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                        bool 坐标 = APPmaster.获取Load盘坐标(true, out xPoint, out yPoint);
                        if (坐标)
                        {
                            PS入料手臂吸放料流程.StepID = 520;
                        }
                        else
                        {
                            //Thread.Sleep(1000);
                            //Debug.WriteLine("进入换盘程序");
                            //LoaderTrayClear();
                            Load换盘标记 = true;
                            PS入料手臂吸放料流程.StepID = 805;
                        }
                        break;

                    case 520:
                        PS入料手臂吸放料流程.StepName = "获取吸料信息";
                        Debug.WriteLine($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                        result = APPmaster.获取Load盘Open(opens, xPoint, yPoint, out lsopen, out index, out xPos, out yPos);
                        if (result == "Success")
                        {
                            PS入料手臂吸放料流程.StepID = 525;
                        }
                        break;

                    case 525:
                        PS入料手臂吸放料流程.StepName = "XYP轴移动到Load吸料位置";
                        Debug.WriteLine($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                        result = 入料手臂移动(xPos, yPos, 入料吸料间距); //50, 300, 10 写去哪里吸料，至于去哪个位置吸，走多少由APPmaster决定
                        if (result == "Success")
                        {
                            firstdraw = true;
                            PS入料手臂吸放料流程.StepID = 530;
                        }
                        break;

                    case 530:
                        PS入料手臂吸放料流程.StepName = "入料手臂Z向下移动";
                        Debug.WriteLine($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                        result = 入料手臂Z移动(lsopen, 初始点位.入料_Load_Z, true);
                        if (result == "Success")
                        {
                            firstdzhome = true;
                            PS入料手臂吸放料流程.StepID = 535;
                        }
                        break;

                    case 535:
                        PS入料手臂吸放料流程.StepName = "入料吸嘴吸料";
                        Debug.WriteLine($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                        result = 入料吸嘴_吸放料(lsopen, true, true);
                        if (result == "Success")
                        {
                            PS入料手臂吸放料流程.StepID = 540;
                        }

                        break;

                    case 540:
                        PS入料手臂吸放料流程.StepName = "入料手臂Z真空检测";
                        Debug.WriteLine($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                        stopwatch.Start();
                        while (JudgeNozzleSts(lsopen, opens, 0))
                        {
                            if (stopwatch.ElapsedMilliseconds > 2000)
                            {
                                Debug.WriteLine($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->入料吸料超时");
                                break;
                            }

                            result = 入料吸嘴_真空检测(out ioval);
                            if (result != "Success") break;

                            Debug.WriteLine($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{ioval[0]},{ioval[1]},{ioval[2]},{ioval[3]}:{stopwatch.ElapsedMilliseconds}ms");

                            for (int i = 0; i < ioval.Length; i++)
                            {
                                if (lsopen[i])
                                {
                                    if (ioval[i] == "1") opens[i] = 1;
                                }
                            }

                            Thread.Sleep(20);
                        }

                        stopwatch.Stop();
                        stopwatch.Reset();

                        if (result == "Success")
                        {
                            PS入料手臂吸放料流程.StepID = 545;
                        }
                        else
                        {
                            ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->入料真空检测失败" + result);
                        }
                        break;

                    case 545:
                        PS入料手臂吸放料流程.StepName = "入料手臂Z向上移动";
                        Debug.WriteLine($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                        result = 入料手臂Z移动(lsopen, -0.3, false);
                        if (result == "Success")
                        {
                            for (int i = 0; i < lsopen.Length; i++)
                            {
                                if (i + index == lsopen.Length) break;
                                if (lsopen[i + index])
                                {
                                    if (opens[i + index] == 1)
                                    {
                                        lsopen[i + index] = false;
                                        LoaderDicPoint[$"{xPoint},{yPoint + i * Interval}"] = false;
                                        Debug.WriteLine($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->入料Z{i + index + 1}吸料成功");
                                    }
                                    else Debug.WriteLine($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->入料Z{i + index + 1}吸料失败");
                                }
                            }
                            PS入料手臂吸放料流程.StepID = 550;
                        }
                        break;

                    case 550:
                        PS入料手臂吸放料流程.StepName = "入料手臂Z回原感应";
                        Debug.WriteLine($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                        result = 入料吸嘴_回原检测(out home);
                        if (result == "Success")
                        {
                            if (!home.Contains("1") && !lsopen.Contains(true)) //如果有1说明有吸嘴没有挡住原点
                            {
                                PS入料手臂吸放料流程.StepID = 552;
                            }
                            else
                            {
                                if (home.Contains("1"))
                                {
                                    for (int i = 0; i < home.Length; i++)
                                    {
                                        if (home[i] == "1") { Debug.WriteLine($"入料Z{i + 1}没有挡住原点"); }
                                    }
                                    if (firstdzhome)
                                    {
                                        firstdzhome = false;
                                    }
                                    else
                                    {
                                        ProgramPause = true;
                                        APPmaster.IsPause = true;
                                    }
                                }
                                PS入料手臂吸放料流程.StepID = 551;
                            }
                        }
                        break;

                    case 551:
                        PS入料手臂吸放料流程.StepName = "入料手臂Z回原";
                        bool[] homeopen = new bool[] { home[0] == "1", home[1] == "1", home[2] == "1", home[3] == "1" };
                        result = 入料手臂Z回零(homeopen);
                        if (result == "Success")
                        {
                            if (home.Contains("1"))
                            {
                                PS入料手臂吸放料流程.StepID = 550;
                            }
                            else
                            {
                                PS入料手臂吸放料流程.StepID = 552;
                            }
                        }
                        break;

                    case 552:
                        PS入料手臂吸放料流程.StepName = "更新LoadMap";
                        Debug.WriteLine($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                        if (lsopen.Contains(true)) //有ture说明还有吸嘴没吸上来
                        {
                            if (firstdraw)
                            {
                                firstdraw = false;
                                PS入料手臂吸放料流程.StepID = 530; //重吸
                            }
                            else
                            {
                                for (int i = 0; i < lsopen.Length; i++)
                                {
                                    if (lsopen[i] && opens[i] == 0)
                                    {
                                        Debug.WriteLine($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->302{i + 1:D2}00,入料Z{i + 1}吸料失败");
                                    }
                                }
                                入料IC忽略 = false;
                                ProgramPause = true;
                                IsPause = true;
                                PS入料手臂吸放料流程.StepID = 553;
                            }
                        }
                        else
                        {
                            PS入料手臂吸放料流程.StepID = 555;
                        }
                        break;

                    case 553:
                        if (入料IC忽略)
                        {
                            for (int i = 0; i < lsopen.Length; i++)
                            {
                                if (i + index == lsopen.Length) break;
                                if (lsopen[i + index])
                                {
                                    lsopen[i + index] = false;
                                    LoaderDicPoint[$"{xPoint},{yPoint + i * Interval}"] = false;
                                }
                            }
                            PS入料手臂吸放料流程.StepID = 555;
                        }
                        else if (结批标记)
                        {

                        }
                        else //直接点开始默认重吸
                        {
                            PS入料手臂吸放料流程.StepID = 530;
                        }
                        break;

                    case 555:
                        PS入料手臂吸放料流程.StepName = "更新opens";
                        if (opens.Contains(0)) ////如果有0说明还有吸嘴需要吸料
                        {
                            Debug.WriteLine($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{opens[0]} , {opens[1]} , {opens[2]} , {opens[3]}");
                            PS入料手臂吸放料流程.StepID = 515;
                        }
                        else
                        {
                            Debug.WriteLine($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{opens[0]} , {opens[1]} , {opens[2]} , {opens[3]}");
                            for (int i = 0; i < opens.Length; i++)
                            {
                                if (opens[i] == 1)
                                {
                                    载入数++;
                                    lsopen[i] = true;
                                }
                                else lsopen[i] = false;
                            }
                            PS入料手臂吸放料流程.StepID = 599;
                        }
                        break;


                    case 599:
                        PS入料手臂吸放料流程.StepName = "入料手臂_吸料流程完成";
                        Debug.WriteLine($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                        PS入料手臂吸放料流程.StepID = 705;
                        break;

                    case 705: ////放料流程////
                        PS入料手臂吸放料流程.StepName = "XYP轴移动到飞梭放料位置";
                        Debug.WriteLine($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                        double[] xypPos = 获取梭车xyp放料点位(colums);
                        result = 入料手臂移动(xypPos[0], xypPos[1], xypPos[2]); //入料手臂移动(xPos, yPos, 10); //写去哪里吸料，至于去哪个位置吸，走多少由APPmaster决定
                        if (result == "Success")
                        {
                            if (入料手臂抓料等待标记) PS入料手臂吸放料流程.StepID = 505;
                            else PS入料手臂吸放料流程.StepID = 710;
                        }
                        else
                        {
                            Debug.WriteLine($"XYP轴移动到飞梭放料位置失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 710:
                        PS入料手臂吸放料流程.StepName = "入料手臂Z向下移动";
                        Debug.WriteLine($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                        if (飞梭移动状态 == 0) //梭车停止状态才能放
                        {
                            double zpos = 获取梭车Z放料点位(colums);
                            result = 入料手臂Z移动(lsopen, zpos, true);
                            if (result == "Success")
                            {
                                firstdraw = true;
                                PS入料手臂吸放料流程.StepID = 715;
                            }
                            else
                            {
                                Debug.WriteLine($"入料手臂Z向下移动失败!" + result);
                                ProgramPause = true;
                                IsPause = true;
                            }
                        }
                        else Thread.Sleep(10);
                        break;

                    case 715:
                        PS入料手臂吸放料流程.StepName = "入料吸嘴放料";
                        Debug.WriteLine($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                        result = 入料吸嘴_吸放料(lsopen, true, false); //关吸真空
                        if (result == "Success")
                        {
                            result = 入料吸嘴_吸放料(lsopen, false, true); //开破真空
                            if (result == "Success")
                            {
                                Thread.Sleep(200);
                                result = 入料吸嘴_吸放料(lsopen, false, false); //关破真空
                                if (result == "Success")
                                {
                                    PS入料手臂吸放料流程.StepID = 725;
                                }
                                else
                                {
                                    Debug.WriteLine($"入料吸嘴_关破真空失败!" + result);
                                    ProgramPause = true;
                                    IsPause = true;
                                }
                            }
                            else
                            {
                                Debug.WriteLine($"入料吸嘴_开破真空失败!" + result);
                                ProgramPause = true;
                                IsPause = true;
                            }
                        }
                        else
                        {
                            Debug.WriteLine($"入料吸嘴_关吸真空失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 720:
                        PS入料手臂吸放料流程.StepName = "入料手臂Z真空检测";
                        Debug.WriteLine($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                        result = 入料吸嘴_真空检测(out ioval);
                        if (result == "Success")
                        {
                            PS入料手臂吸放料流程.StepID = 725;
                        }
                        else
                        {
                            Debug.WriteLine($"入料手臂Z真空检测失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 725:
                        PS入料手臂吸放料流程.StepName = "入料手臂Z向上移动";
                        Debug.WriteLine($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                        result = 入料手臂Z移动(lsopen, -0.3, false);
                        if (result == "Success")
                        {
                            PS入料手臂吸放料流程.StepID = 730;
                        }
                        else
                        {
                            Debug.WriteLine($"入料手臂Z向上移动失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 730:
                        PS入料手臂吸放料流程.StepName = "入料手臂Z回原感应";
                        Debug.WriteLine($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                        result = 入料吸嘴_回原检测(out home);
                        if (result == "Success")
                        {
                            if (!home.Contains("1")) //如果有1说明有吸嘴没有挡住原点
                            {
                                bool[] temp = new bool[4] { 飞梭前入料IC[colums][0], 飞梭前入料IC[colums][1], 飞梭前入料IC[colums][2], 飞梭前入料IC[colums][3] };
                                for (int i = 0; i < lsopen.Length; i++)
                                {
                                    if (lsopen[i]) //为了后续防止移动过程中忽略了
                                    {
                                        temp[i] = true;
                                    }
                                }
                                飞梭前入料IC[colums] = temp;
                                入料手臂抓料等待标记 = false;

                                PS入料手臂吸放料流程.StepID = 799;
                            }
                            else
                            {
                                for (int i = 0; i < home.Length; i++)
                                {
                                    if (home[i] == "1") { Debug.WriteLine($"入料Z{i + 1}没有挡住原点"); }
                                }
                                if (firstdzhome)
                                {
                                    firstdzhome = false;
                                }
                                else
                                {
                                    ///listcode.add("入料Z回原失败");
                                    ProgramPause = true;
                                    APPmaster.IsPause = true;
                                }
                                PS入料手臂吸放料流程.StepID = 731;
                            }
                        }
                        else
                        {
                            Debug.WriteLine($"入料吸嘴_回原检测失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 731:
                        PS入料手臂吸放料流程.StepName = "入料手臂Z回原";
                        Debug.WriteLine($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                        homeopen = new bool[] { home[0] == "1", home[1] == "1", home[2] == "1", home[3] == "1" };
                        result = 入料手臂Z回零(homeopen);
                        if (result == "Success")
                        {
                            PS入料手臂吸放料流程.StepID = 730;
                        }
                        else
                        {
                            Debug.WriteLine($"入料手臂Z回原失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 799:
                        PS入料手臂吸放料流程.StepName = $"入料手臂_放料第{colums + 1}列流程完成";
                        Debug.WriteLine($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                        PS入料手臂吸放料流程.StepID = 505;
                        break;


                    case 805:
                        PS入料手臂吸放料流程.StepName = "入料手臂移到避让位";
                        Debug.WriteLine($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                        result = 入料手臂移动(初始点位.入料_Heat_X, 0, 7.5);
                        if (result == "Success")
                        {
                            PS入料手臂吸放料流程.StepID = 810;
                            Debug.WriteLine($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->判断抓盘手臂放空盘到Empty标记");
                        }
                        else
                        {
                            Debug.WriteLine($"入料手臂移到避让位失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 810:
                        PS入料手臂吸放料流程.StepName = "判断抓盘手臂放空盘到Empty标记";
                        if (抓盘手臂放空盘到Empty标记 == 1)
                        {
                            抓盘手臂取Load空盘标记 = 2;
                            PS入料手臂吸放料流程.StepID = 815;
                            Debug.WriteLine($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->等待抓盘手臂取Load空盘完成");
                        }
                        else Thread.Sleep(10);
                        break;

                    case 815:
                        PS入料手臂吸放料流程.StepName = "等待抓盘手臂取Load空盘完成";
                        if (抓盘手臂取Load空盘标记 == 1)
                        {
                            PS入料手臂吸放料流程.StepID = 820;
                            Debug.WriteLine($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->等待Load上顶完成");
                        }
                        else Thread.Sleep(10);
                        break;

                    case 820:
                        PS入料手臂吸放料流程.StepName = "等待Load上顶完成";
                        if (Load上顶标记 == 1)
                        {
                            Load换盘标记 = false;
                            LoaderTrayClear();
                            Debug.WriteLine($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->Load上顶完成Map更新");
                            PS入料手臂吸放料流程.StepID = 515;
                        }
                        else Thread.Sleep(10);
                        break;

                    case 905:
                        PS入料手臂吸放料流程.StepName = "入料手臂移到避让位";
                        Debug.WriteLine($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                        result = 入料手臂移动(初始点位.入料_Heat_X, 0, 7.5);
                        if (result == "Success")
                        {
                            Auto等待换盘标记 = true;
                            PS入料手臂吸放料流程.StepID = 910;
                            Debug.WriteLine($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->等待Auto1和Auto2换盘");
                        }
                        else
                        {
                            Debug.WriteLine($"入料手臂移到避让位失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 910:
                        PS入料手臂吸放料流程.StepName = "等待Auto1和Auto2换盘";
                        if (Auto1换盘标记 == false && Auto2换盘标记 == false)
                        {
                            Auto等待换盘标记 = false;
                            if (!Auto1换盘标记) Debug.WriteLine($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->Auto1换盘完成");
                            if (!Auto2换盘标记) Debug.WriteLine($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->Auto2换盘完成");
                            PS入料手臂吸放料流程.StepID = 500;
                        }
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
            int[] opens = new int[4];
            bool[][] open = new bool[2][];
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
            int xPoint = 0;
            int yPoint = 0;
            bool firstdraw = true;
            bool firstdzhome = true;
            string[] home = new string[4];
            Stopwatch stopwatch = new Stopwatch();
            PS出料手臂吸放料流程.StepID = 500;
            while (true)
            {
                Thread.Sleep(10);

                if (!是否可以开始运行())
                {
                    continue;
                }

                switch (PS出料手臂吸放料流程.StepID)
                {
                    case 500:
                        PS出料手臂吸放料流程.StepName = "路径规划";
                        PS出料手臂吸放料流程.Condition = 飞梭前出料状态 == 1;
                        PS出料手臂吸放料流程.Permit = !PS入飞梭移动流程.Processing;
                        if (PS出料手臂吸放料流程.Condition && PS出料手臂吸放料流程.Permit)
                        {
                            ispass = false;
                            open = new bool[2][]
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
                        PS出料手臂吸放料流程.StepName = "计算吸料IC颗数";
                        if (open[0].Contains(true))
                        {
                            colums = 0;
                            opens = new int[] { open[0][0] ? 0 : 2, open[0][1] ? 0 : 2, open[0][2] ? 0 : 2, open[0][3] ? 0 : 2 };
                            lsopen = new bool[] { open[0][0], open[0][1], open[0][2], open[0][3] };
                            bin = new string[] { bins[0][0], bins[0][1], bins[0][2], bins[0][3] };
                            PS出料手臂吸放料流程.StepID = 510;
                        }
                        else if (open[1].Contains(true))
                        {
                            colums = 1;
                            opens = new int[] { open[1][0] ? 0 : 2, open[1][1] ? 0 : 2, open[1][2] ? 0 : 2, open[1][3] ? 0 : 2 };
                            lsopen = new bool[] { open[1][0], open[1][1], open[1][2], open[1][3] };
                            bin = new string[] { bins[1][0], bins[1][1], bins[1][2], bins[1][3] };
                            PS出料手臂吸放料流程.StepID = 515;
                        }
                        else //放料完成
                        {
                            if (!ispass) //如果第二列没有料就会是为false
                            {
                                飞梭前出料状态 = 0;
                            }
                            PS出料手臂吸放料流程.StepID = 799;
                        }
                        break;

                    case 510:
                        PS出料手臂吸放料流程.StepName = "出料手臂移动到飞梭第1列";
                        Debug.WriteLine($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");
                        double[] xpos = 获取梭车xyp吸料点位(colums);
                        result = 出料手臂移动(xpos[0], xpos[1], xpos[2]);
                        if (result == "Success")
                        {
                            firstdraw = true;
                            firstdzhome = true;
                            PS出料手臂吸放料流程.StepID = 520;
                        }
                        else
                        {
                            Debug.WriteLine($"出料手臂移动到梭车第1列失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 515:
                        PS出料手臂吸放料流程.StepName = "出料手臂移动到飞梭第2列";
                        Debug.WriteLine($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");
                        double[] xpos2 = 获取梭车xyp吸料点位(colums);
                        result = 出料手臂移动(xpos2[0], xpos2[1], xpos2[2]);
                        if (result == "Success")
                        {
                            firstdraw = true;
                            firstdzhome = true;
                            PS出料手臂吸放料流程.StepID = 520;
                        }
                        else
                        {
                            Debug.WriteLine($"出料手臂移动到梭车第2列失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 520:
                        PS出料手臂吸放料流程.StepName = "出料手臂Z向下移动";
                        Debug.WriteLine($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");
                        double zpos = 飞梭2位置 == 0 ? 初始点位.出料_梭车1_Z : 初始点位.出料_梭车2_Z;
                        result = 出料手臂Z移动(lsopen, zpos, true);
                        if (result == "Success")
                        {
                            PS出料手臂吸放料流程.StepID = 525;
                        }
                        else
                        {
                            Debug.WriteLine($"出料手臂Z移动失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 525:
                        PS出料手臂吸放料流程.StepName = "出料吸嘴_吸料";
                        Debug.WriteLine($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");
                        result = 出料吸嘴_吸放料(lsopen, true, true);
                        if (result == "Success")
                        {
                            PS出料手臂吸放料流程.StepID = 530;
                        }
                        else
                        {
                            Debug.WriteLine($"出料吸嘴_吸料失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 530:
                        PS出料手臂吸放料流程.StepName = "出料吸嘴_真空检测";
                        Debug.WriteLine($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");

                        stopwatch.Start();
                        while (JudgeNozzleSts(lsopen, opens, 0))
                        {
                            if (stopwatch.ElapsedMilliseconds > 2000)
                            {
                                Debug.WriteLine($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->出料吸料超时");
                                break;
                            }

                            result = 出料吸嘴_真空检测(out string[] ioval);
                            if (result != "Success") break;

                            Debug.WriteLine($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{ioval[0]},{ioval[1]},{ioval[2]},{ioval[3]}:{stopwatch.ElapsedMilliseconds}ms");

                            for (int i = 0; i < ioval.Length; i++)
                            {
                                if (lsopen[i])
                                {
                                    if (ioval[i] == "1") opens[i] = 1;
                                }
                            }

                            Thread.Sleep(20);
                        }

                        stopwatch.Stop();
                        stopwatch.Reset();

                        if (result == "Success")
                        {
                            PS出料手臂吸放料流程.StepID = 535;
                        }
                        else
                        {
                            ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->出料吸嘴_真空检测失败" + result);
                        }
                        break;

                    case 535:
                        PS出料手臂吸放料流程.StepName = "出料手臂Z向上移动";
                        Debug.WriteLine($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");
                        result = 出料手臂Z移动(lsopen, -0.3, false);
                        if (result == "Success")
                        {
                            for (int i = 0; i < lsopen.Length; i++)
                            {
                                if (lsopen[i])
                                {
                                    if (opens[i] == 1)
                                    {
                                        lsopen[i] = false;
                                        Debug.WriteLine($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->出料Z{i + index + 1}吸料成功");
                                    }
                                    else Debug.WriteLine($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->出料Z{i + index + 1}吸料失败");
                                }
                            }
                            PS出料手臂吸放料流程.StepID = 540;
                        }
                        else
                        {
                            Debug.WriteLine($"出料手臂Z向上移动失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 540:
                        PS出料手臂吸放料流程.StepName = "出料Z是否在原点";
                        Debug.WriteLine($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");
                        result = 出料吸嘴_回原检测(out home);
                        if (result == "Success")
                        {
                            if (!home.Contains("1") && !lsopen.Contains(true)) //如果有1说明有吸嘴没有挡住原点
                            {
                                PS出料手臂吸放料流程.StepID = 545;
                            }
                            else
                            {
                                if (home.Contains("1"))
                                {
                                    for (int i = 0; i < home.Length; i++)
                                    {
                                        if (home[i] == "1") { Debug.WriteLine($"出料吸嘴Z{i + 1}没有挡住原点"); }
                                    }
                                    if (firstdzhome)
                                    {
                                        firstdzhome = false;
                                    }
                                    else
                                    {
                                        //for (int i = 0; i < home.Length; i++)
                                        //{
                                        //    if (home[i] == "1") { liscode.Add($"出料吸嘴Z{i + 1}没有挡住原点"); }
                                        //}
                                        ProgramPause = true;
                                        APPmaster.IsPause = true;
                                    }
                                }
                                PS出料手臂吸放料流程.StepID = 541;
                            }
                        }
                        break;

                    case 541:
                        PS出料手臂吸放料流程.StepName = "出料手臂Z回原";
                        bool[] homeopen = new bool[] { home[0] == "1" || lsopen[0], home[1] == "1" || lsopen[1], home[2] == "1" || lsopen[2], home[3] == "1" || lsopen[3] };
                        result = 出料手臂Z回零(homeopen);
                        if (result == "Success")
                        {
                            if (home.Contains("1")) //如果是因为挡住原点才回的原点
                            {
                                PS出料手臂吸放料流程.StepID = 540;
                            }
                            else PS出料手臂吸放料流程.StepID = 545;
                        }
                        else
                        {
                            Debug.WriteLine($"出料手臂Z回零失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 545:
                        PS出料手臂吸放料流程.StepName = "更新吸嘴状态";
                        if (lsopen.Contains(true)) //有ture说明还有吸嘴没吸上来
                        {
                            if (firstdraw) //是否自动重吸
                            {
                                firstdraw = false;
                                PS出料手臂吸放料流程.StepID = 520; //重吸
                            }
                            else
                            {
                                for (int i = 0; i < lsopen.Length; i++)
                                {
                                    if (lsopen[i] && opens[i] == 0)
                                    {
                                        Debug.WriteLine($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->302{i + 1:D2}00,出料Z{i + 1}吸料失败");
                                    }
                                }
                                出料IC忽略 = false;
                                ProgramPause = true;
                                IsPause = true;
                                PS出料手臂吸放料流程.StepID = 546;
                            }
                        }
                        else
                        {
                            if (colums == 1)
                            {
                                ispass = true;
                                飞梭前出料状态 = 0;
                            }
                            PS出料手臂吸放料流程.StepID = 599;
                        }
                        break;

                    case 546:
                        if (出料IC忽略)
                        {
                            if (opens.Contains(1)) //如果有吸嘴吸了IC则放置
                            {
                                for (int i = 0; i < lsopen.Length; i++)
                                {
                                    if (lsopen[i]) open[colums][i] = false;
                                }
                                PS出料手臂吸放料流程.StepID = 599;
                            }
                            else //吸嘴上没有IC,则直接返回
                            {
                                open[colums] = new bool[] { false, false, false, false };
                                PS出料手臂吸放料流程.StepID = 505;
                            }
                        }
                        else //直接点开始默认重吸
                        {
                            PS出料手臂吸放料流程.StepID = 520;
                        }
                        break;

                    case 599:
                        PS出料手臂吸放料流程.StepName = $"出料手臂_吸料第{colums + 1}列流程完成";
                        Debug.WriteLine($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");
                        PS出料手臂吸放料流程.StepID = 705;
                        break;

                    case 705: //放料
                        PS出料手臂吸放料流程.StepName = "计算放料盘位置";

                        if (判断BIN项(opens, Auto1, bin, out index))
                        {
                            if (获取Auto1盘坐标(false, out xPoint, out yPoint))
                            {
                                Debug.WriteLine($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->获取Auto1坐标位置");
                                APPmaster.获取Auto1盘Open(opens, xPoint, yPoint, out lsopen, index, bin);
                                xPos = Auto1XPoint[xPoint];
                                yPos = Auto1YPoint[yPoint] + index * Interval * Tray_Y_Pitch; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                trayid = 0;
                                PS出料手臂吸放料流程.StepID = 710;
                                break;

                            }
                            else
                            {
                                Auto等待换盘标记 = false;
                                Auto1换盘标记 = true;
                                Debug.WriteLine("进入Auto1换盘程序");
                                PS出料手臂吸放料流程.StepID = 805;
                                break;
                            }
                        }
                        else if (判断BIN项(opens, Auto2, bin, out index))
                        {
                            if (获取Auto2盘坐标(false, out xPoint, out yPoint))
                            {
                                Debug.WriteLine($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->获取Auto2坐标位置");
                                APPmaster.获取Auto2盘Open(opens, xPoint, yPoint, out lsopen, index, bin);
                                xPos = Auto2XPoint[xPoint];
                                yPos = Auto2YPoint[yPoint] + index * Interval * Tray_Y_Pitch; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                trayid = 1;
                                PS出料手臂吸放料流程.StepID = 710;
                                break;

                            }
                            else
                            {
                                Auto等待换盘标记 = false;
                                Auto2换盘标记 = true;
                                Debug.WriteLine("进入Auto2换盘程序");
                                PS出料手臂吸放料流程.StepID = 905;
                                break;
                            }
                        }
                        else if (判断Fix1BIN项(opens, bin) == "Success")
                        {
                            if (Fix1Num == 1)
                            {
                                if (判断BIN项(opens, Fix1, bin, out index))
                                {
                                    FixMinMax(1, 1, Fix1YPoint.Length, out int min, out int max);
                                    if (获取Fix1盘坐标(false, min, max, out xPoint, out yPoint))
                                    {
                                        获取Fix1盘Open(opens, xPoint, yPoint, Fix1, bin, max, out lsopen, index);
                                        xPos = Fix1XPoint[xPoint];
                                        yPos = Fix1YPoint[yPoint] + index * Interval * Tray_Y_Pitch; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                        trayid = 2;
                                        PS出料手臂吸放料流程.StepID = 710;
                                    }
                                    else
                                    {
                                        Debug.WriteLine("进入Fix1满盘报警程序");
                                        PS出料手臂吸放料流程.StepID = 1005;
                                        break;
                                    }
                                }
                            }
                            else if (Fix1Num == 2)
                            {
                                if (判断BIN项(opens, Fix1_1, bin, out index))
                                {
                                    FixMinMax(2, 1, Fix1YPoint.Length, out int min, out int max);
                                    if (获取Fix1盘坐标(false, min, max, out xPoint, out yPoint))
                                    {
                                        获取Fix1盘Open(opens, xPoint, yPoint, Fix1_1, bin, max, out lsopen, index);
                                        xPos = Fix1XPoint[xPoint];
                                        yPos = Fix1YPoint[yPoint] + index * Interval * Tray_Y_Pitch; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                        trayid = 2;
                                        PS出料手臂吸放料流程.StepID = 710;
                                    }
                                    else
                                    {
                                        Debug.WriteLine("进入Fix1满盘报警程序");
                                        PS出料手臂吸放料流程.StepID = 1005;
                                        break;
                                    }
                                }
                                else if (判断BIN项(opens, Fix1_2, bin, out index))
                                {
                                    FixMinMax(2, 2, Fix1YPoint.Length, out int min, out int max);
                                    if (获取Fix1盘坐标(false, min, max, out xPoint, out yPoint))
                                    {
                                        获取Fix1盘Open(opens, xPoint, yPoint, Fix1_2, bin, max, out lsopen, index);
                                        xPos = Fix1XPoint[xPoint];
                                        yPos = Fix1YPoint[yPoint] + index * Interval * Tray_Y_Pitch; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                        trayid = 2;
                                        PS出料手臂吸放料流程.StepID = 710;
                                    }
                                    else
                                    {
                                        Debug.WriteLine("进入Fix1满盘报警程序");
                                        PS出料手臂吸放料流程.StepID = 1005;
                                        break;
                                    }
                                }
                            }
                            else if (Fix1Num == 3)
                            {
                                if (判断BIN项(opens, Fix1_1, bin, out index))
                                {
                                    FixMinMax(3, 1, Fix1YPoint.Length, out int min, out int max);
                                    if (获取Fix1盘坐标(false, min, max, out xPoint, out yPoint))
                                    {
                                        获取Fix1盘Open(opens, xPoint, yPoint, Fix1_1, bin, max, out lsopen, index);
                                        xPos = Fix1XPoint[xPoint];
                                        yPos = Fix1YPoint[yPoint] + index * Interval * Tray_Y_Pitch; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                        trayid = 2;
                                        PS出料手臂吸放料流程.StepID = 710;
                                    }
                                    else
                                    {
                                        Debug.WriteLine("进入Fix1满盘报警程序");
                                        PS出料手臂吸放料流程.StepID = 1005;
                                        break;
                                    }
                                }
                                else if (判断BIN项(opens, Fix1_2, bin, out index))
                                {
                                    FixMinMax(3, 2, Fix1YPoint.Length, out int min, out int max);
                                    if (获取Fix1盘坐标(false, min, max, out xPoint, out yPoint))
                                    {
                                        获取Fix1盘Open(opens, xPoint, yPoint, Fix1_2, bin, max, out lsopen, index);
                                        xPos = Fix1XPoint[xPoint];
                                        yPos = Fix1YPoint[yPoint] + index * Interval * Tray_Y_Pitch; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                        trayid = 2;
                                        PS出料手臂吸放料流程.StepID = 710;
                                    }
                                    else
                                    {
                                        Debug.WriteLine("进入Fix1满盘报警程序");
                                        PS出料手臂吸放料流程.StepID = 1005;
                                        break;
                                    }
                                }
                                else if (判断BIN项(opens, Fix1_3, bin, out index))
                                {
                                    FixMinMax(3, 3, Fix1YPoint.Length, out int min, out int max);
                                    if (获取Fix1盘坐标(false, min, max, out xPoint, out yPoint))
                                    {
                                        获取Fix1盘Open(opens, xPoint, yPoint, Fix1_3, bin, max, out lsopen, index);
                                        xPos = Fix1XPoint[xPoint];
                                        yPos = Fix1YPoint[yPoint] + index * Interval * Tray_Y_Pitch; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                        trayid = 2;
                                        PS出料手臂吸放料流程.StepID = 710;
                                    }
                                    else
                                    {
                                        Debug.WriteLine("进入Fix1满盘报警程序");
                                        PS出料手臂吸放料流程.StepID = 1005;
                                        break;
                                    }
                                }
                            }
                        }
                        else if (判断Fix2BIN项(opens, bin) == "Success")
                        {
                            if (Fix2Num == 1)
                            {
                                if (判断BIN项(opens, Fix2, bin, out index))
                                {
                                    FixMinMax(1, 1, Fix2YPoint.Length, out int min, out int max);
                                    if (获取Fix2盘坐标(false, min, max, out xPoint, out yPoint))
                                    {
                                        获取Fix2盘Open(opens, xPoint, yPoint, Fix2, bin, max, out lsopen, index);
                                        xPos = Fix2XPoint[xPoint];
                                        yPos = Fix2YPoint[yPoint] + index * Interval * Tray_Y_Pitch; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                        trayid = 3;
                                        PS出料手臂吸放料流程.StepID = 710;
                                    }
                                    else
                                    {
                                        Debug.WriteLine("进入Fix2满盘报警程序");
                                        PS出料手臂吸放料流程.StepID = 1105;
                                        break;
                                    }
                                }
                            }
                            else if (Fix2Num == 2)
                            {
                                if (判断BIN项(opens, Fix2_1, bin, out index))
                                {
                                    FixMinMax(2, 1, Fix1YPoint.Length, out int min, out int max);
                                    if (获取Fix2盘坐标(false, min, max, out xPoint, out yPoint))
                                    {
                                        获取Fix2盘Open(opens, xPoint, yPoint, Fix2_1, bin, max, out lsopen, index);
                                        xPos = Fix2XPoint[xPoint];
                                        yPos = Fix2YPoint[yPoint] + index * Interval * Tray_Y_Pitch; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                        trayid = 3;
                                        PS出料手臂吸放料流程.StepID = 710;
                                    }
                                    else
                                    {
                                        Debug.WriteLine("进入Fix2满盘报警程序");
                                        PS出料手臂吸放料流程.StepID = 1105;
                                        break;
                                    }
                                }
                                else if (判断BIN项(opens, Fix2_2, bin, out index))
                                {
                                    FixMinMax(2, 2, Fix1YPoint.Length, out int min, out int max);
                                    if (获取Fix2盘坐标(false, min, max, out xPoint, out yPoint))
                                    {
                                        获取Fix2盘Open(opens, xPoint, yPoint, Fix2_2, bin, max, out lsopen, index);
                                        xPos = Fix2XPoint[xPoint];
                                        yPos = Fix2YPoint[yPoint] + index * Interval * Tray_Y_Pitch; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                        trayid = 3;
                                        PS出料手臂吸放料流程.StepID = 710;
                                    }
                                    else
                                    {
                                        Debug.WriteLine("进入Fix2满盘报警程序");
                                        PS出料手臂吸放料流程.StepID = 1105;
                                        break;
                                    }
                                }
                            }
                            else if (Fix2Num == 3)
                            {
                                if (判断BIN项(opens, Fix2_1, bin, out index))
                                {
                                    FixMinMax(3, 1, Fix1YPoint.Length, out int min, out int max);
                                    if (获取Fix2盘坐标(false, min, max, out xPoint, out yPoint))
                                    {
                                        获取Fix2盘Open(opens, xPoint, yPoint, Fix2_1, bin, max, out lsopen, index);
                                        xPos = Fix2XPoint[xPoint];
                                        yPos = Fix2YPoint[yPoint] + index * Interval * Tray_Y_Pitch; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                        trayid = 3;
                                        PS出料手臂吸放料流程.StepID = 710;
                                    }
                                    else
                                    {
                                        Debug.WriteLine("进入Fix2满盘报警程序");
                                        PS出料手臂吸放料流程.StepID = 1105;
                                        break;
                                    }
                                }
                                else if (判断BIN项(opens, Fix2_2, bin, out index))
                                {
                                    FixMinMax(3, 2, Fix1YPoint.Length, out int min, out int max);
                                    if (获取Fix2盘坐标(false, min, max, out xPoint, out yPoint))
                                    {
                                        获取Fix2盘Open(opens, xPoint, yPoint, Fix2_2, bin, max, out lsopen, index);
                                        xPos = Fix2XPoint[xPoint];
                                        yPos = Fix2YPoint[yPoint] + index * Interval * Tray_Y_Pitch; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                        trayid = 3;
                                        PS出料手臂吸放料流程.StepID = 710;
                                    }
                                    else
                                    {
                                        Debug.WriteLine("进入Fix2满盘报警程序");
                                        PS出料手臂吸放料流程.StepID = 1105;
                                        break;
                                    }
                                }
                                else if (判断BIN项(opens, Fix2_3, bin, out index))
                                {
                                    FixMinMax(3, 3, Fix1YPoint.Length, out int min, out int max);
                                    if (获取Fix2盘坐标(false, min, max, out xPoint, out yPoint))
                                    {
                                        获取Fix2盘Open(opens, xPoint, yPoint, Fix2_3, bin, max, out lsopen, index);
                                        xPos = Fix2XPoint[xPoint];
                                        yPos = Fix2YPoint[yPoint] + index * Interval * Tray_Y_Pitch; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                        trayid = 3;
                                        PS出料手臂吸放料流程.StepID = 710;
                                    }
                                    else
                                    {
                                        Debug.WriteLine("进入Fix2满盘报警程序");
                                        PS出料手臂吸放料流程.StepID = 1105;
                                        break;
                                    }
                                }
                            }
                        }
                        else if (判断Fix3BIN项(opens, bin) == "Success")
                        {
                            if (Fix3Num == 1)
                            {
                                if (判断BIN项(opens, Fix3, bin, out index))
                                {
                                    FixMinMax(1, 1, Fix3YPoint.Length, out int min, out int max);
                                    if (获取Fix3盘坐标(false, min, max, out xPoint, out yPoint))
                                    {
                                        获取Fix3盘Open(opens, xPoint, yPoint, Fix3, bin, max, out lsopen, index);
                                        xPos = Fix3XPoint[xPoint];
                                        yPos = Fix3YPoint[yPoint] + index * Interval * Tray_Y_Pitch; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                        trayid = 4;
                                        PS出料手臂吸放料流程.StepID = 710;
                                    }
                                    else
                                    {
                                        Debug.WriteLine("进入Fix3满盘报警程序");
                                        PS出料手臂吸放料流程.StepID = 1205;
                                        break;
                                    }
                                }
                            }
                            else if (Fix3Num == 2)
                            {
                                if (判断BIN项(opens, Fix3_1, bin, out index))
                                {
                                    FixMinMax(2, 1, Fix3YPoint.Length, out int min, out int max);
                                    if (获取Fix3盘坐标(false, min, max, out xPoint, out yPoint))
                                    {
                                        获取Fix3盘Open(opens, xPoint, yPoint, Fix3_1, bin, max, out lsopen, index);
                                        xPos = Fix3XPoint[xPoint];
                                        yPos = Fix3YPoint[yPoint] + index * Interval * Tray_Y_Pitch; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                        trayid = 4;
                                        PS出料手臂吸放料流程.StepID = 710;
                                    }
                                    else
                                    {
                                        Debug.WriteLine("进入Fix3满盘报警程序");
                                        PS出料手臂吸放料流程.StepID = 1205;
                                        break;
                                    }
                                }
                                else if (判断BIN项(opens, Fix3_2, bin, out index))
                                {
                                    FixMinMax(2, 2, Fix3YPoint.Length, out int min, out int max);
                                    if (获取Fix3盘坐标(false, min, max, out xPoint, out yPoint))
                                    {
                                        获取Fix3盘Open(opens, xPoint, yPoint, Fix3_2, bin, max, out lsopen, index);
                                        xPos = Fix3XPoint[xPoint];
                                        yPos = Fix3YPoint[yPoint] + index * Interval * Tray_Y_Pitch; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                        trayid = 4;
                                        PS出料手臂吸放料流程.StepID = 710;
                                    }
                                    else
                                    {
                                        Debug.WriteLine("进入Fix3满盘报警程序");
                                        PS出料手臂吸放料流程.StepID = 1205;
                                        break;
                                    }
                                }
                            }
                            else if (Fix3Num == 3)
                            {
                                if (判断BIN项(opens, Fix3_1, bin, out index))
                                {
                                    FixMinMax(3, 1, Fix3YPoint.Length, out int min, out int max);
                                    if (获取Fix3盘坐标(false, min, max, out xPoint, out yPoint))
                                    {
                                        获取Fix3盘Open(opens, xPoint, yPoint, Fix3_1, bin, max, out lsopen, index);
                                        xPos = Fix3XPoint[xPoint];
                                        yPos = Fix3YPoint[yPoint] + index * Interval * Tray_Y_Pitch; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                        trayid = 4;
                                        PS出料手臂吸放料流程.StepID = 710;
                                    }
                                    else
                                    {
                                        Debug.WriteLine("进入Fix3满盘报警程序");
                                        PS出料手臂吸放料流程.StepID = 1205;
                                        break;
                                    }
                                }
                                else if (判断BIN项(opens, Fix3_2, bin, out index))
                                {
                                    FixMinMax(3, 2, Fix3YPoint.Length, out int min, out int max);
                                    if (获取Fix3盘坐标(false, min, max, out xPoint, out yPoint))
                                    {
                                        获取Fix3盘Open(opens, xPoint, yPoint, Fix3_2, bin, max, out lsopen, index);
                                        xPos = Fix3XPoint[xPoint];
                                        yPos = Fix3YPoint[yPoint] + index * Interval * Tray_Y_Pitch; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                        trayid = 4;
                                        PS出料手臂吸放料流程.StepID = 710;
                                    }
                                    else
                                    {
                                        Debug.WriteLine("进入Fix3满盘报警程序");
                                        PS出料手臂吸放料流程.StepID = 1205;
                                        break;
                                    }
                                }
                                else if (判断BIN项(opens, Fix3_3, bin, out index))
                                {
                                    FixMinMax(3, 3, Fix3YPoint.Length, out int min, out int max);
                                    if (获取Fix3盘坐标(false, min, max, out xPoint, out yPoint))
                                    {
                                        获取Fix3盘Open(opens, xPoint, yPoint, Fix3_3, bin, max, out lsopen, index);
                                        xPos = Fix3XPoint[xPoint];
                                        yPos = Fix3YPoint[yPoint] + index * Interval * Tray_Y_Pitch; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                        trayid = 4;
                                        PS出料手臂吸放料流程.StepID = 710;
                                    }
                                    else
                                    {
                                        Debug.WriteLine("进入Fix3满盘报警程序");
                                        PS出料手臂吸放料流程.StepID = 1205;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            PS出料手臂吸放料流程.StepID = 505;
                            break;
                        }
                        break;

                    case 710:
                        PS出料手臂吸放料流程.StepName = "出料手臂走到放料位";
                        Debug.WriteLine($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");
                        Task<string> fix3move = Task.Run(() => { return "Success"; });
                        if (trayid == 3)
                        {
                            fix3move = Task.Run(() => { return Fix3放IC盘轴移动(0); });
                            Tray平移左右标记 = 0;
                        }
                        else
                        {
                            if (trayid == 2 || trayid == 4) //如果不是往Fix放料则不用移动
                            {
                                fix3move = Task.Run(() => { return Fix3放IC盘轴移动(初始点位.出料_Fix3_右移动); });
                                Tray平移左右标记 = 1;
                            }
                        }
                        Task<string> outputx = Task.Run(() => { return 出料手臂移动(xPos, yPos, 出料放料间距); });
                        if (outputx.Result == "Success" && fix3move.Result == "Success")
                        {
                            PS出料手臂吸放料流程.StepID = 715;
                        }
                        else
                        {
                            if (outputx.Result != "Success")
                            {
                                Debug.WriteLine("出料手臂移动失败!" + outputx.Result);
                            }
                            if (fix3move.Result != "Success")
                            {
                                Debug.WriteLine("Fix3放IC盘轴移动失败!" + fix3move.Result);
                            }
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 715:
                        PS出料手臂吸放料流程.StepName = "出料手臂Z向下";
                        Debug.WriteLine($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");
                        double zPos = 获取出料Z放料点位(trayid);
                        result = 出料手臂Z移动(lsopen, zPos, true);
                        if (result == "Success")
                        {
                            PS出料手臂吸放料流程.StepID = 720;
                        }
                        break;

                    case 720:
                        PS出料手臂吸放料流程.StepName = "出料吸嘴放料";
                        Debug.WriteLine($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");
                        result = 出料吸嘴_吸放料(lsopen, true, false);
                        if (result == "Success")
                        {
                            result = 出料吸嘴_吸放料(lsopen, false, true);
                            if (result == "Success")
                            {
                                Thread.Sleep(200);
                                result = 出料吸嘴_吸放料(lsopen, false, false);
                                if (result == "Success")
                                {
                                    PS出料手臂吸放料流程.StepID = 725;
                                }
                            }
                        }
                        break;

                    case 725:
                        PS出料手臂吸放料流程.StepName = "出料手臂Z向上";
                        Debug.WriteLine($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");
                        result = 出料手臂Z移动(lsopen, false);
                        if (result == "Success")
                        {
                            PS出料手臂吸放料流程.StepID = 730;
                        }
                        break;

                    case 730:
                        PS出料手臂吸放料流程.StepName = "出料Z是否在原点";
                        Debug.WriteLine($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");
                        result = 出料吸嘴_回原检测(out home);
                        if (result == "Success")
                        {
                            if (!home.Contains("1")) //如果有1说明有吸嘴没有挡住原点
                            {
                                for (int i = 0; i < lsopen.Length; i++)
                                {
                                    if (index + i == lsopen.Length) break;
                                    if (lsopen[i + index])
                                    {
                                        opens[i + index] = 0;
                                        open[colums][i + index] = false;
                                        if (trayid == 0) Auto1DicPoint[$"{xPoint},{yPoint + i * Interval}"] = true;
                                        else if (trayid == 1) Auto2DicPoint[$"{xPoint},{yPoint + i * Interval}"] = true;
                                        else if (trayid == 2) Fix1DicPoint[$"{xPoint},{yPoint + i * Interval}"] = true;
                                        else if (trayid == 3) Fix2DicPoint[$"{xPoint},{yPoint + i * Interval}"] = true;
                                        else Fix3DicPoint[$"{xPoint},{yPoint + i * Interval}"] = true;
                                    }
                                }
                                PS出料手臂吸放料流程.StepID = 705;
                            }
                            else
                            {
                                for (int i = 0; i < home.Length; i++)
                                {
                                    if (home[i] == "1") { Debug.WriteLine($"出料吸嘴{i + 1}没有挡住原点"); }
                                }
                                ProgramPause = true;
                                APPmaster.IsPause = true;
                            }
                        }
                        break;

                    case 799:
                        PS出料手臂吸放料流程.StepName = "出料手臂_吸放料流程完成";
                        Debug.WriteLine($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");
                        PS出料手臂吸放料流程.Processing = false;
                        PS出料手臂吸放料流程.StepID = 500;
                        break;



                    case 805:
                        PS出料手臂吸放料流程.StepName = "出料手臂移到避让位";
                        Debug.WriteLine($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");
                        result = 出料手臂移动(256.05, 初始点位.出料_Auto1_Y, 出料放料间距);
                        if (result == "Success")
                        {
                            Auto1上顶标记 = 2;
                            PS出料手臂吸放料流程.StepID = 810;
                            Debug.WriteLine($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->判断Load未在换盘");
                        }
                        else
                        {
                            Debug.WriteLine($"出料手臂移到避让位失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 810:
                        PS出料手臂吸放料流程.StepName = "判断Load未在换盘";
                        if (Load换盘标记 == false && 抓盘手臂放空盘到Empty标记 == 1)
                        {
                            抓盘手臂放空盘到Auto1标记 = 2;
                            PS出料手臂吸放料流程.StepID = 815;
                            Debug.WriteLine($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->抓盘手臂放空盘到Auto1");
                        }
                        else Thread.Sleep(10);
                        break;

                    case 815:
                        PS出料手臂吸放料流程.StepName = "抓盘手臂放空盘到Auto1";
                        if (抓盘手臂放空盘到Auto1标记 == 1)
                        {
                            Auto1TrayClear(); 
                            Auto1换盘标记 = false;
                            Debug.WriteLine($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->Auto1换盘完成Map更新");
                            PS出料手臂吸放料流程.StepID = 705;
                        }
                        else Thread.Sleep(10);
                        break;

                    case 905:
                        PS出料手臂吸放料流程.StepName = "出料手臂移到避让位";
                        result = 出料手臂移动(256.05, 初始点位.出料_Auto2_Y, 出料放料间距);
                        Debug.WriteLine($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");
                        if (result == "Success")
                        {
                            Auto2上顶标记 = 2;
                            PS出料手臂吸放料流程.StepID = 910;
                            Debug.WriteLine($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->判断Load是否未在换盘");
                        }
                        else
                        {
                            Debug.WriteLine($"出料手臂移到避让位失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 910:
                        PS出料手臂吸放料流程.StepName = "判断Load是否未在换盘";
                        if (Load换盘标记 == false && 抓盘手臂放空盘到Empty标记 == 1)
                        {
                            抓盘手臂放空盘到Auto2标记 = 2;
                            PS出料手臂吸放料流程.StepID = 915;
                            Debug.WriteLine($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->判断Load是否未在换盘");
                        }
                        else Thread.Sleep(10);
                        break;

                    case 915:
                        PS出料手臂吸放料流程.StepName = "抓盘手臂放空盘到Auto2";
                        if (抓盘手臂放空盘到Auto2标记 == 1)
                        {
                            Auto2TrayClear();
                            Auto2换盘标记 = false;
                            Debug.WriteLine($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->Auto2换盘完成Map更新");
                            PS出料手臂吸放料流程.StepID = 705;
                        }
                        else Thread.Sleep(10);
                        break;


                    case 1005:
                        PS出料手臂吸放料流程.StepName = "Fix1满盘报警";
                        //if ()
                        {
                            Fix1TrayClear();
                            PS出料手臂吸放料流程.StepID = 705;
                        }
                        //else Thread.Sleep(10);
                        break;

                    case 1105:
                        PS出料手臂吸放料流程.StepName = "Fix2满盘报警";
                        //if ()
                        {
                            Fix2TrayClear();
                            PS出料手臂吸放料流程.StepID = 705;
                        }
                        //else Thread.Sleep(10);
                        break;

                    case 1205:
                        PS出料手臂吸放料流程.StepName = "Fix3满盘报警";
                        //if ()
                        {
                            Fix3TrayClear();
                            PS出料手臂吸放料流程.StepID = 705;
                        }
                        //else Thread.Sleep(10);
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
                Thread.Sleep(10);

                if (!是否可以开始运行())
                {
                    continue;
                }

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
                        PS入飞梭移动流程.StepName = $"飞梭_前后检测产品感应";
                        Debug.WriteLine($"PS入飞梭移动流程.StepID={PS入飞梭移动流程.StepID}->{PS入飞梭移动流程.StepName}");
                        result = 飞梭_前后检测产品感应(out string[] iovalues);
                        //if (result == "Success")
                        {
                            //if (iovalues.Contains("0"))
                            {
                                PS入飞梭移动流程.StepID = 515;
                            }
                        }
                        break;

                    case 515:
                        PS入飞梭移动流程.StepName = $"飞梭轴移动";
                        Debug.WriteLine($"PS入飞梭移动流程.StepID={PS入飞梭移动流程.StepID}->{PS入飞梭移动流程.StepName}");
                        if (飞梭2位置 == 0)
                        {
                            result = 飞梭轴移动(初始点位.梭车2_前_Y);
                            if (result == "Success")
                            {
                                飞梭2位置 = 1;
                                PS入飞梭移动流程.StepID = 520;
                            }
                            else
                            {
                                ProgramPause = true;
                                IsPause = true;
                                Debug.WriteLine("飞梭2前移失败");
                            }
                        }
                        else
                        {
                            result = 飞梭轴移动(初始点位.梭车2_后_Y);
                            if (result == "Success")
                            {
                                飞梭2位置 = 0;
                                PS入飞梭移动流程.StepID = 520;
                            }
                            else
                            {
                                ProgramPause = true;
                                IsPause = true;
                                Debug.WriteLine("飞梭2后移失败");
                            }
                        }
                        break;

                    case 520:
                        PS入飞梭移动流程.StepName = $"数据交换";
                        Debug.WriteLine($"PS入飞梭移动流程.StepID={PS入飞梭移动流程.StepID}->{PS入飞梭移动流程.StepName}");

                        交换飞梭前后入料IC数据();

                        飞梭后入料状态 = 1;
                        飞梭前入料状态 = 0;

                        if (飞梭后出料状态 == 1)
                        {
                            交换飞梭前后出料IC数据();
                            交换飞梭前后出料BIN数据();
                            飞梭前出料状态 = 1;
                            飞梭后出料状态 = 0;
                        }

                        飞梭移动状态 = 0;
                        PS入飞梭移动流程.StepID = 599;

                        break;

                    case 599:
                        PS入飞梭移动流程.StepName = $"飞梭2移动到{(飞梭2位置 == 1 ? "前方" : "后方")}流程完成";
                        Debug.WriteLine($"PS入飞梭移动流程.StepID={PS入飞梭移动流程.StepID}->{PS入飞梭移动流程.StepName}");
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
            string result = "";
            string[] home = new string[1];
            int[] opens = new int[8];
            bool[] lsopen = new bool[8];
            Stopwatch stopwatch = new Stopwatch();
            bool firstdraw = true;
            bool firstdzhome = true;
            int site = 0;
            PS测试手臂取待测料流程.StepID = 500;
            while (true)
            {
                Thread.Sleep(10);

                if (!是否可以开始运行())
                {
                    continue;
                }

                switch (PS测试手臂取待测料流程.StepID)
                {
                    case 500:
                        PS测试手臂取待测料流程.StepName = "路径规划";
                        PS测试手臂取待测料流程.Condition = (飞梭后入料状态 == 1 && APPmaster.InSiteQueue.Count > 0)
                                                        && (飞梭后出料状态 == 1 || APPmaster.OutSiteQueue.Count == 0);
                        PS测试手臂取待测料流程.Permit = !PS测试手臂取已测料流程.Processing;
                        if (PS测试手臂取待测料流程.Condition && PS测试手臂取待测料流程.Permit)
                        {
                            PS测试手臂取待测料流程.Processing = true;
                            PS测试手臂取待测料流程.StepID = 501;
                        }
                        break;

                    case 501:
                        PS测试手臂取待测料流程.StepName = "获取开Site信息";
                        Debug.WriteLine($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");
                        if (InSiteQueue.TryDequeue(out site))
                        {
                            bool[] open = GetSiteOpen(site);
                            opens = new int[]
                            {
                                open[0] ? 0 : 2, open[1] ? 0 : 2, open[2] ? 0 : 2, open[3] ? 0 : 2,
                                open[4] ? 0 : 2, open[5] ? 0 : 2, open[6] ? 0 : 2, open[7] ? 0 : 2
                            };
                            lsopen = new bool[8] { open[0], open[1], open[2], open[3], open[4], open[5], open[6], open[7] };
                            PS测试手臂取待测料流程.StepID = 505;
                        }
                        else
                        {
                            Debug.WriteLine($"获取开Site信息失败!");
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 505:
                        PS测试手臂取待测料流程.StepName = "测试吸嘴_张卡气缸闭合";
                        Debug.WriteLine($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");
                        result = 测试吸嘴_张开气缸输出(true);
                        if (result == "Success")
                        {
                            PS测试手臂取待测料流程.StepID = 510;
                        }
                        else
                        {
                            Debug.WriteLine($"测试吸嘴_张卡气缸闭合失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 510:
                        PS测试手臂取待测料流程.StepName = "测试手臂X轴到飞梭后入料吸料";
                        Debug.WriteLine($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");
                        double xpos = 飞梭2位置 == 0 ? 初始点位.测试_梭车2吸_X : 初始点位.测试_梭车1吸_X;
                        result = 测试手臂X移动(xpos);
                        if (result == "Success")
                        {
                            firstdraw = true;
                            firstdzhome = true;
                            PS测试手臂取待测料流程.StepID = 515;
                        }
                        else
                        {
                            Debug.WriteLine($"测试手臂X移动到梭车失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 515:
                        PS测试手臂取待测料流程.StepName = "测试吸嘴张开气缸到位输入读取";
                        Debug.WriteLine($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");
                        result = 测试吸嘴张开气缸到位输入读取();
                        if (result == "Success")
                        {
                            PS测试手臂取待测料流程.StepID = 520;
                        }
                        else
                        {
                            Debug.WriteLine($"测试吸嘴张开气缸到位输入读取失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 520:
                        PS测试手臂取待测料流程.StepName = "测试Z向下移动"; //飞梭吸
                        Debug.WriteLine($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");
                        double zpos = 飞梭2位置 == 0 ? 初始点位.测试_梭车2吸_Z : 初始点位.测试_梭车1吸_Z;
                        result = 测试手臂Z移动(zpos, true);
                        if (result == "Success")
                        {
                            PS测试手臂取待测料流程.StepID = 525;
                        }
                        else
                        {
                            Debug.WriteLine($"测试Z向下移动失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 525:
                        PS测试手臂取待测料流程.StepName = "测试吸嘴吸料";
                        Debug.WriteLine($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");
                        result = 测试吸嘴_吸放料(lsopen, true, true);
                        if (result == "Success")
                        {
                            PS测试手臂取待测料流程.StepID = 530;
                        }
                        else
                        {
                            Debug.WriteLine($"测试吸嘴开吸真空失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 530:
                        PS测试手臂取待测料流程.StepName = "测试吸嘴真空检测";
                        Debug.WriteLine($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");

                        stopwatch.Start();
                        while (JudgeNozzleSts(lsopen, opens, 0))
                        {
                            if (stopwatch.ElapsedMilliseconds > 2000)
                            {
                                Debug.WriteLine($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->测试吸料超时");
                                break;
                            }

                            result = 测试吸嘴_真空检测(out string[] ioval);
                            if (result != "Success") break;

                            Debug.WriteLine($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{ioval[0]},{ioval[1]},{ioval[2]},{ioval[3]},{ioval[4]},{ioval[5]},{ioval[6]},{ioval[7]}:{stopwatch.ElapsedMilliseconds}ms");

                            for (int i = 0; i < ioval.Length; i++)
                            {
                                if (lsopen[i])
                                {
                                    if (ioval[i] == "1") opens[i] = 1;
                                }
                            }

                            Thread.Sleep(20);
                        }

                        stopwatch.Stop();
                        stopwatch.Reset();

                        if (result == "Success")
                        {
                            PS测试手臂取待测料流程.StepID = 535;
                        }
                        else
                        {
                            ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->测试吸嘴_真空检测失败" + result);
                        }
                        break;

                    case 535:
                        PS测试手臂取待测料流程.StepName = "测试Z向上移动";
                        Debug.WriteLine($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");
                        result = 测试手臂Z移动(-0.2, false);
                        if (result == "Success")
                        {
                            for (int i = 0; i < lsopen.Length; i++)
                            {
                                if (lsopen[i])
                                {
                                    if (opens[i] == 1)
                                    {
                                        lsopen[i] = false;
                                        Debug.WriteLine($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->测试Z{i + 1}吸料成功");
                                    }
                                    else Debug.WriteLine($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->测试Z{i + 1}吸料失败");
                                }
                            }
                            PS测试手臂取待测料流程.StepID = 540;
                        }
                        else
                        {
                            Debug.WriteLine($"测试Z向上移动失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;


                    case 540:
                        PS测试手臂取待测料流程.StepName = "测试Z是否在原点";
                        Debug.WriteLine($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");
                        result = motionEx.GetDiHome(new string[] { "M19_测试手臂Z轴" }, out home);
                        if (result == "Success")
                        {
                            if (home[0] == "0") //如果有1说明有吸嘴没有挡住原点
                            {
                                PS测试手臂取待测料流程.StepID = 542;
                            }
                            else
                            {
                                if (home.Contains("1"))
                                {
                                    Debug.WriteLine($"测试Z没有挡住原点");

                                    if (firstdzhome)
                                    {
                                        firstdzhome = false;
                                    }
                                    else
                                    {
                                        // liscode.Add($"出料吸嘴Z{i + 1}没有挡住原点");
                                        ProgramPause = true;
                                        APPmaster.IsPause = true;
                                    }
                                }
                                PS测试手臂取待测料流程.StepID = 541;
                            }
                        }
                        else
                        {
                            Debug.WriteLine($"读取测试Z原点信号失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 541:
                        PS测试手臂取待测料流程.StepName = "测试手臂Z回原";
                        result = 测试手臂Z回零();
                        if (result == "Success")
                        {
                            PS测试手臂取待测料流程.StepID = 540;
                        }
                        else
                        {
                            Debug.WriteLine($"测试手臂Z回零失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 542:
                        PS测试手臂取待测料流程.StepName = "更新吸嘴状态";
                        if (lsopen.Contains(true)) //有ture说明还有吸嘴没吸上来
                        {
                            if (firstdraw) //是否自动重吸
                            {
                                firstdraw = false;
                                PS测试手臂取待测料流程.StepID = 520; //重吸
                            }
                            else
                            {
                                for (int i = 0; i < lsopen.Length; i++)
                                {
                                    if (lsopen[i] && opens[i] == 0)
                                    {
                                        Debug.WriteLine($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->302{i + 1:D2}00,测试Z{i + 1}吸料失败");
                                    }
                                }
                                测试IC忽略 = false;
                                ProgramPause = true;
                                IsPause = true;
                                PS测试手臂取待测料流程.StepID = 543;
                            }
                        }
                        else
                        {
                            bool[] temp = new bool[8]
                            {
                                飞梭后入料IC[0][0], 飞梭后入料IC[0][1], 飞梭后入料IC[0][2], 飞梭后入料IC[0][3],
                                飞梭后入料IC[1][0], 飞梭后入料IC[1][1], 飞梭后入料IC[1][2], 飞梭后入料IC[1][3]
                            };

                            for (int i = 0; i < opens.Length; i++)
                            {
                                if (opens[i] == 1) temp[i] = false;
                            }

                            飞梭后入料IC = new bool[][]
                            {
                                new bool[] { temp[0], temp[1], temp[2], temp[3] },
                                new bool[] { temp[4], temp[5], temp[6], temp[7] }
                            };

                            飞梭后入料状态 = 0;
                            PS测试手臂取待测料流程.StepID = 599;
                        }
                        break;

                    case 543:
                        if (测试IC忽略)
                        {
                            if (opens.Contains(1)) //如果有吸嘴吸了IC则放置
                            {
                                for (int i = 0; i < lsopen.Length; i++)
                                {
                                    if (lsopen[i])
                                    {
                                        lsopen[i] = false;
                                        opens[i] = 2;
                                    }
                                }
                                PS测试手臂取待测料流程.StepID = 542;
                            }
                            else //吸嘴上没有IC,则直接返回
                            {
                                测试IC忽略 = false;
                                ProgramPause = true;
                                IsPause = true;
                                Debug.WriteLine("测试Z吸嘴至少吸上1颗IC");
                                //opens = new bool[] { false, false, false, false };
                                //PS测试手臂取待测料流程.StepID = 505;
                            }
                        }
                        else //直接点开始默认重吸
                        {
                            PS测试手臂取待测料流程.StepID = 520;
                        }
                        break;

                    case 599:
                        lsopen = new bool[] { opens[0] == 1, opens[1] == 1, opens[2] == 1, opens[3] == 1, opens[4] == 1, opens[5] == 1, opens[6] == 1, opens[7] == 1 };
                        PS测试手臂取待测料流程.StepName = "测试手臂取待测料流程吸料完成";
                        Debug.WriteLine($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");
                        PS测试手臂取待测料流程.StepID = 705;

                        break;

                    case 705:
                        PS测试手臂取待测料流程.StepName = "测试吸嘴_张开";
                        Debug.WriteLine($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");
                        result = 测试吸嘴_张开气缸输出(false);
                        if (result == "Success")
                        {
                            PS测试手臂取待测料流程.StepID = 710;
                        }
                        else
                        {
                            Debug.WriteLine($"测试吸嘴_张开失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 710:
                        PS测试手臂取待测料流程.StepName = "测试手臂X移动到测试区放料";
                        Debug.WriteLine($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");
                        result = 测试手臂X移动(TestXPoint[site]);
                        if (result == "Success")
                        {
                            firstdraw = true;
                            firstdzhome = true;
                            PS测试手臂取待测料流程.StepID = 715;
                        }
                        else
                        {
                            Debug.WriteLine($"测试手臂X轴移动到Site{site + 1}上方失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 715:
                        PS测试手臂取待测料流程.StepName = "测试吸嘴张开气缸到位输入读取";
                        Debug.WriteLine($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");
                        result = 测试吸嘴张开气缸原位输入读取();
                        if (result == "Success")
                        {
                            PS测试手臂取待测料流程.StepID = 720;
                            Debug.WriteLine($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->测试Z等待气缸动作完成");
                        }
                        else
                        {
                            Debug.WriteLine($"测试吸嘴张开气缸原位输入读取失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 720:
                        PS测试手臂取待测料流程.StepName = "测试Z向下移动";
                        if (SiteTestState[site] == 0)
                        {
                            Debug.WriteLine($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");
                            zpos = 初始点位.测试_Test区放_Z;
                            result = 测试手臂Z移动(zpos, true); //测试区放
                            if (result == "Success")
                            {
                                PS测试手臂取待测料流程.StepID = 725;
                            }
                            else
                            {
                                Debug.WriteLine($"测试Z向下移动失败!" + result);
                                ProgramPause = true;
                                IsPause = true;
                            }
                        }
                        else Thread.Sleep(10);
                        break;

                    case 725:
                        PS测试手臂取待测料流程.StepName = "测试吸嘴放料";
                        Debug.WriteLine($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");
                        result = 测试吸嘴_吸料(lsopen, false);
                        if (result == "Success")
                        {
                            result = 测试吸嘴_放料(lsopen, true);
                            if (result == "Success")
                            {
                                Thread.Sleep(200);
                                result = 测试吸嘴_放料(lsopen, false);
                                if (result == "Success")
                                {
                                    PS测试手臂取待测料流程.StepID = 730;
                                }
                                else
                                {
                                    Debug.WriteLine($"测试吸嘴_关破真空失败!" + result);
                                    ProgramPause = true;
                                    IsPause = true;
                                }
                            }
                            else
                            {
                                Debug.WriteLine($"测试吸嘴_开破真空失败!" + result);
                                ProgramPause = true;
                                IsPause = true;
                            }
                        }
                        else
                        {
                            Debug.WriteLine($"测试吸嘴_关吸真空失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 730:
                        PS测试手臂取待测料流程.StepName = "测试Z向上移动";
                        Debug.WriteLine($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");
                        result = 测试手臂Z移动(-0.2, false);
                        if (result == "Success")
                        {
                            PS测试手臂取待测料流程.StepID = 735;
                        }
                        else
                        {
                            Debug.WriteLine($"测试Z向上移动失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;


                    case 735:
                        PS测试手臂取待测料流程.StepName = "测试Z是否在原点";
                        Debug.WriteLine($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");
                        result = motionEx.GetDiHome(new string[] { "M19_测试手臂Z轴" }, out home);
                        if (result == "Success")
                        {
                            if (home[0] == "0") //如果有1说明有吸嘴没有挡住原点
                            {
                                SiteTestIC[site] = new bool[][]
                                {
                                    new bool[] { opens[0] == 1, opens[1] == 1, opens[2] == 1, opens[3] == 1 },
                                    new bool[] { opens[4] == 1, opens[5] == 1, opens[6] == 1, opens[7] == 1 }
                                };
                                SiteTestState[site] = 1; //有料
                                PS测试手臂取待测料流程.StepID = 799;
                            }
                            else
                            {

                                Debug.WriteLine($"测试Z没有挡住原点");

                                if (firstdzhome)
                                {
                                    firstdzhome = false;
                                }
                                else
                                {
                                    // liscode.Add($"出料吸嘴Z{i + 1}没有挡住原点");
                                    ProgramPause = true;
                                    APPmaster.IsPause = true;
                                }

                                PS测试手臂取待测料流程.StepID = 736;
                            }

                            //for (int i = 0; i < lsopen.Length; i++)
                            //{
                            //    if (lsopen[i])
                            //    {
                            //        飞梭前入料IC[colums][i] = true;
                            //    }
                            //}
                        }
                        else
                        {
                            Debug.WriteLine($"测试Z回原失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 736:
                        PS测试手臂取待测料流程.StepName = "测试手臂Z回原";
                        result = 测试手臂Z回零();
                        if (result == "Success")
                        {
                            PS测试手臂取待测料流程.StepID = 735;
                        }
                        else
                        {
                            Debug.WriteLine($"测试手臂Z回零失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 799:
                        PS测试手臂取待测料流程.StepName = $"测试手臂取待测料流程完成";
                        Debug.WriteLine($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");
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
            string result = "";
            int[] opens = new int[8];
            bool[] lsopen = new bool[8];
            int site = 0;
            bool firstdraw = true;
            bool firstdzhome = true;
            Stopwatch stopwatch = new Stopwatch();
            PS测试手臂取已测料流程.StepID = 500;
            while (true)
            {
                Thread.Sleep(10);

                if (!是否可以开始运行())
                {
                    continue;
                }

                switch (PS测试手臂取已测料流程.StepID)
                {
                    case 500:
                        PS测试手臂取已测料流程.StepName = "路径规划";
                        PS测试手臂取已测料流程.Condition = ((飞梭后入料状态 == 0 && 飞梭后出料状态 == 1) || (飞梭后入料状态 == 1 && 飞梭后出料状态 == 0)) && APPmaster.OutSiteQueue.Count > 0;
                        PS测试手臂取已测料流程.Permit = !PS测试手臂取待测料流程.Processing;
                        if (PS测试手臂取已测料流程.Condition && PS测试手臂取已测料流程.Permit)
                        {
                            PS测试手臂取已测料流程.Processing = true;
                            PS测试手臂取已测料流程.StepID = 501;
                        }
                        break;

                    case 501:
                        PS测试手臂取已测料流程.StepName = "获取Site信息";
                        Debug.WriteLine($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
                        if (OutSiteQueue.TryDequeue(out site))
                        {
                            bool[][] open = SiteTestIC[site];
                            opens = new int[8]
                            {
                                open[0][0] ? 0 : 2, open[0][1] ? 0 : 2, open[0][2] ? 0 : 2, open[0][3] ? 0 : 2,
                                open[1][0] ? 0 : 2, open[1][1] ? 0 : 2, open[1][2] ? 0 : 2, open[1][3] ? 0 : 2,
                            };
                            lsopen = new bool[] { open[0][0], open[0][1], open[0][2], open[0][3], open[1][0], open[1][1], open[1][2], open[1][3] };
                            PS测试手臂取已测料流程.StepID = 505;
                        }
                        else
                        {
                            Debug.WriteLine($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->获取Site信息失败!");
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 505:
                        PS测试手臂取已测料流程.StepName = "测试吸嘴_张卡气缸张开";
                        Debug.WriteLine($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
                        result = 测试吸嘴_张开气缸输出(false);
                        if (result == "Success")
                        {
                            PS测试手臂取已测料流程.StepID = 510;
                        }
                        else
                        {
                            Debug.WriteLine($"测试吸嘴_张卡气缸张开失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 510:
                        PS测试手臂取已测料流程.StepName = "测试手臂X轴移动到Site上方";
                        Debug.WriteLine($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
                        result = 测试手臂X移动(TestXPoint[site]);
                        if (result == "Success")
                        {
                            firstdraw = true;
                            firstdzhome = true;
                            PS测试手臂取已测料流程.StepID = 515;
                        }
                        else
                        {
                            Debug.WriteLine($"测试手臂X轴移动到Site{site + 1}上方失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 515:
                        PS测试手臂取已测料流程.StepName = "测试吸嘴_张开气缸到位信号读取";
                        Debug.WriteLine($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
                        result = 测试吸嘴张开气缸原位输入读取();
                        if (result == "Success")
                        {
                            PS测试手臂取已测料流程.StepID = 520;
                            Debug.WriteLine($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->测试Z等待气缸动作完成");
                        }
                        else
                        {
                            Debug.WriteLine($"测试吸嘴_张开气缸到位信号读取失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 520:
                        PS测试手臂取已测料流程.StepName = "测试Z向下移动";
                        if (SiteTestState[site] == 4) //待取出
                        {
                            Debug.WriteLine($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
                            double testzpos = 初始点位.测试_Test区吸_Z; //后续还要加上offset
                            result = 测试手臂Z移动(testzpos, true);
                            if (result == "Success")
                            {
                                PS测试手臂取已测料流程.StepID = 525;
                            }
                            else
                            {
                                Debug.WriteLine($"测试Z向下移动失败!" + result);
                                ProgramPause = true;
                                IsPause = true;
                            }
                        }
                        else Thread.Sleep(10);
                        break;

                    case 525:
                        PS测试手臂取已测料流程.StepName = "测试吸嘴吸料";
                        Debug.WriteLine($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
                        result = 测试吸嘴_吸放料(lsopen, true, true);
                        if (result == "Success")
                        {
                            PS测试手臂取已测料流程.StepID = 530;
                        }
                        else
                        {
                            Debug.WriteLine($"测试吸嘴开吸真空失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 530:
                        PS测试手臂取已测料流程.StepName = "测试吸嘴真空检测";
                        Debug.WriteLine($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");


                        stopwatch.Start();
                        while (JudgeNozzleSts(lsopen, opens, 0))
                        {
                            if (stopwatch.ElapsedMilliseconds > 2000)
                            {
                                Debug.WriteLine($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->测试吸料超时");
                                break;
                            }

                            result = 测试吸嘴_真空检测(out string[] ioval);
                            if (result != "Success") break;

                            Debug.WriteLine($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{ioval[0]},{ioval[1]},{ioval[2]},{ioval[3]},{ioval[4]},{ioval[5]},{ioval[6]},{ioval[7]}:{stopwatch.ElapsedMilliseconds}ms");

                            for (int i = 0; i < ioval.Length; i++)
                            {
                                if (lsopen[i])
                                {
                                    if (ioval[i] == "1") opens[i] = 1;
                                }
                            }

                            Thread.Sleep(20);
                        }

                        stopwatch.Stop();
                        stopwatch.Reset();

                        if (result == "Success")
                        {
                            PS测试手臂取已测料流程.StepID = 535;
                        }
                        else
                        {
                            ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->测试吸嘴_真空检测失败" + result);
                        }
                        break;

                    case 535:
                        PS测试手臂取已测料流程.StepName = "测试Z向上移动";
                        Debug.WriteLine($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
                        result = 测试手臂Z移动(-0.2, false);
                        if (result == "Success")
                        {
                            for (int i = 0; i < lsopen.Length; i++)
                            {
                                if (lsopen[i])
                                {
                                    if (opens[i] == 1)
                                    {
                                        lsopen[i] = false;
                                        Debug.WriteLine($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->测试Z{i + 1}吸料成功");
                                    }
                                    else Debug.WriteLine($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->测试Z{i + 1}吸料失败");
                                }
                            }
                            PS测试手臂取已测料流程.StepID = 540;
                        }
                        else
                        {
                            Debug.WriteLine($"测试Z向上移动失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 540:
                        PS测试手臂取已测料流程.StepName = "测试Z是否在原点";
                        Debug.WriteLine($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
                        result = motionEx.GetDiHome(new string[] { "M19_测试手臂Z轴" }, out string[] home);
                        if (result == "Success")
                        {
                            Debug.WriteLine($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->测试Z回原：{home[0]}");
                            if (home[0] == "0") //如果有1说明有吸嘴没有挡住原点
                            {
                                PS测试手臂取已测料流程.StepID = 542;
                            }
                            else
                            {
                                if (home.Contains("1"))
                                {
                                    Debug.WriteLine($"测试Z没有挡住原点");

                                    if (firstdzhome)
                                    {
                                        firstdzhome = false;
                                    }
                                    else
                                    {
                                        // liscode.Add($"出料吸嘴Z{i + 1}没有挡住原点");
                                        ProgramPause = true;
                                        APPmaster.IsPause = true;
                                    }
                                }
                                PS测试手臂取已测料流程.StepID = 541;
                            }
                        }
                        else
                        {
                            Debug.WriteLine($"测试Z回原失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 541:
                        PS测试手臂取已测料流程.StepName = "测试手臂Z回原";
                        result = 测试手臂Z回零();
                        if (result == "Success")
                        {
                            PS测试手臂取已测料流程.StepID = 540;
                        }
                        else
                        {
                            Debug.WriteLine($"测试手臂Z回零失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 542:
                        PS测试手臂取已测料流程.StepName = "更新吸嘴状态";
                        if (lsopen.Contains(true)) //有ture说明还有吸嘴没吸上来
                        {
                            if (firstdraw) //是否自动重吸
                            {
                                firstdraw = false;
                                PS测试手臂取已测料流程.StepID = 520; //重吸
                            }
                            else
                            {
                                for (int i = 0; i < lsopen.Length; i++)
                                {
                                    if (lsopen[i] && opens[i] == 0)
                                    {
                                        Debug.WriteLine($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->302{i + 1:D2}00,测试Z{i + 1}吸料失败");
                                    }
                                }
                                测试IC忽略 = false;
                                ProgramPause = true;
                                IsPause = true;
                                PS测试手臂取已测料流程.StepID = 543;
                            }
                        }
                        else
                        {
                            SiteTestState[site] = 0; //无料
                            PS测试手臂取已测料流程.StepID = 599;
                        }
                        break;

                    case 543:
                        if (测试IC忽略)
                        {
                            if (opens.Contains(1)) //如果有吸嘴吸了IC则放置
                            {
                                for (int i = 0; i < lsopen.Length; i++)
                                {
                                    if (lsopen[i])
                                    {
                                        lsopen[i] = false;
                                        opens[i] = 2;
                                    }
                                }
                                PS测试手臂取已测料流程.StepID = 542;
                            }
                            else //吸嘴上没有IC,则直接返回
                            {
                                测试IC忽略 = false;
                                ProgramPause = true;
                                IsPause = true;
                                Debug.WriteLine("测试Z吸嘴至少吸上1颗IC");
                                //opens = new bool[] { false, false, false, false };
                                //PS测试手臂取已测料流程.StepID = 505;
                            }
                        }
                        else //直接点开始默认重吸
                        {
                            PS测试手臂取已测料流程.StepID = 520;
                        }
                        break;

                    case 599:
                        lsopen = new bool[] { opens[0] == 1, opens[1] == 1, opens[2] == 1, opens[3] == 1, opens[4] == 1, opens[5] == 1, opens[6] == 1, opens[7] == 1 };
                        PS测试手臂取已测料流程.StepName = "测试手臂取已测料流程吸料完成";
                        Debug.WriteLine($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
                        PS测试手臂取已测料流程.StepID = 705;
                        break;

                    case 705:
                        PS测试手臂取已测料流程.StepName = "测试吸嘴_闭合";
                        Debug.WriteLine($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
                        result = 测试吸嘴_张开气缸输出(true);
                        if (result == "Success")
                        {
                            PS测试手臂取已测料流程.StepID = 710;
                        }
                        else
                        {
                            Debug.WriteLine($"测试吸嘴_闭合失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 710:
                        PS测试手臂取已测料流程.StepName = "测试手臂X移动到梭车";
                        Debug.WriteLine($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
                        double xpos = 飞梭2位置 == 0 ? 初始点位.测试_梭车2放_X : 初始点位.测试_梭车1放_X;
                        result = 测试手臂X移动(xpos);
                        if (result == "Success")
                        {
                            firstdraw = true;
                            firstdzhome = true;
                            PS测试手臂取已测料流程.StepID = 715;
                        }
                        else
                        {
                            Debug.WriteLine($"测试手臂X移动到梭车失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 715:
                        PS测试手臂取已测料流程.StepName = "测试吸嘴张开气缸到位输入读取";
                        Debug.WriteLine($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
                        result = 测试吸嘴张开气缸到位输入读取();
                        if (result == "Success")
                        {
                            PS测试手臂取已测料流程.StepID = 720;
                            Debug.WriteLine($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->等待梭车到达");
                        }
                        else
                        {
                            Debug.WriteLine($"测试吸嘴张开气缸到位输入读取失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 720:
                        PS测试手臂取已测料流程.StepName = "测试Z向下移动";
                        if (飞梭后出料状态 == 0) //等梭车过来再放
                        {
                            Debug.WriteLine($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
                            double zpos = 飞梭2位置 == 0 ? 初始点位.测试_梭车2放_Z : 初始点位.测试_梭车1放_Z;
                            result = 测试手臂Z移动(zpos, true);
                            if (result == "Success")
                            {
                                PS测试手臂取已测料流程.StepID = 725;
                            }
                            else
                            {
                                Debug.WriteLine($"测试Z向下移动失败!" + result);
                                ProgramPause = true;
                                IsPause = true;
                            }
                        }
                        else Thread.Sleep(10);
                        break;

                    case 725:
                        PS测试手臂取已测料流程.StepName = "测试吸嘴放料";
                        Debug.WriteLine($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
                        result = 测试吸嘴_吸放料(lsopen, true, false);
                        if (result == "Success")
                        {
                            result = 测试吸嘴_吸放料(lsopen, false, true);
                            if (result == "Success")
                            {
                                Thread.Sleep(200);
                                result = 测试吸嘴_吸放料(lsopen, false, false);
                                if (result == "Success")
                                {
                                    PS测试手臂取已测料流程.StepID = 730;
                                }
                                else
                                {
                                    Debug.WriteLine($"测试吸嘴_关破真空失败!" + result);
                                    ProgramPause = true;
                                    IsPause = true;
                                }
                            }
                            else
                            {
                                Debug.WriteLine($"测试吸嘴_开破真空失败!" + result);
                                ProgramPause = true;
                                IsPause = true;
                            }
                        }
                        else
                        {
                            Debug.WriteLine($"测试吸嘴_关吸真空失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 730:
                        PS测试手臂取已测料流程.StepName = "测试Z向上移动";
                        Debug.WriteLine($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
                        result = 测试手臂Z移动(-0.2, false);
                        if (result == "Success")
                        {
                            PS测试手臂取已测料流程.StepID = 735;
                        }
                        else
                        {
                            Debug.WriteLine($"测试Z向上移动失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;


                    case 735:
                        PS测试手臂取已测料流程.StepName = "测试Z是否在原点";
                        Debug.WriteLine($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
                        result = motionEx.GetDiHome(new string[] { "M19_测试手臂Z轴" }, out home);
                        if (result == "Success")
                        {
                            Debug.WriteLine($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->测试Z回原：{home[0]}");
                            if (home[0] == "0") //如果有1说明有吸嘴没有挡住原点
                            {
                                PS测试手臂取已测料流程.StepID = 740;
                            }
                            else
                            {
                                Debug.WriteLine($"测试Z没有挡住原点");

                                if (firstdzhome)
                                {
                                    firstdzhome = false;
                                }
                                else
                                {
                                    // liscode.Add($"出料吸嘴Z{i + 1}没有挡住原点");
                                    ProgramPause = true;
                                    APPmaster.IsPause = true;
                                }

                                PS测试手臂取已测料流程.StepID = 736;
                            }
                            //for (int i = 0; i < lsopen.Length; i++)
                            //{
                            //    if (lsopen[i])
                            //    {
                            //        飞梭前入料IC[colums][i] = true;
                            //    }
                            //}
                        }
                        else
                        {
                            Debug.WriteLine($"测试Z回原失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 736:
                        PS测试手臂取已测料流程.StepName = "测试手臂Z回原";
                        result = 测试手臂Z回零();
                        if (result == "Success")
                        {
                            PS测试手臂取已测料流程.StepID = 735;
                        }
                        else
                        {
                            Debug.WriteLine($"测试手臂Z回零失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 740:
                        PS测试手臂取已测料流程.StepName = "BIN项保存";
                        Debug.WriteLine($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
                        if (BinSaveQueue.TryDequeue(out string[] bin))
                        {
                            飞梭后出料状态 = 1;
                            飞梭后出料IC = new bool[][] { new bool[] { lsopen[0], lsopen[1], lsopen[2], lsopen[3] }, new bool[] { lsopen[4], lsopen[5], lsopen[6], lsopen[7] } }; //SitesTestIC
                            飞梭后出料BIN项 = new string[][] { new string[] { bin[0], bin[1], bin[2], bin[3] }, new string[] { bin[4], bin[5], bin[6], bin[7] } };
                            InSiteQueue.Enqueue(site);
                            PS测试手臂取已测料流程.StepID = 799;
                        }
                        else
                        {
                            Debug.WriteLine($"Site{site + 1}BIN项保存失败!");
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 799:
                        PS测试手臂取已测料流程.StepName = $"测试手臂取已测料流程完成";
                        Debug.WriteLine($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
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
            PS测试区气缸下压动作流程.StepID = 500;
            while (true)
            {
                Thread.Sleep(10);

                if (!是否可以开始运行())
                {
                    continue;
                }

                switch (PS测试区气缸下压动作流程.StepID)
                {
                    case 500:
                        PS测试区气缸下压动作流程.StepName = "路径规划";
                        PS测试区气缸下压动作流程.Condition = true;
                        PS测试区气缸下压动作流程.Permit = true;
                        if (PS测试区气缸下压动作流程.Condition && PS测试区气缸下压动作流程.Permit)
                        {
                            PS测试区气缸下压动作流程.Processing = true;
                            PS测试区气缸下压动作流程.StepID = 505;
                        }
                        break;

                    case 505:
                        PS测试区气缸下压动作流程.StepName = $"循环检测Site状态";
                        if (SiteTestState[site] == 1) //有料
                        {
                            Debug.WriteLine($"PS测试区气缸下压动作流程.StepID={PS测试区气缸下压动作流程.StepID}->循环检测Site状态:{site + 1}有料");
                            bool[][] siteopne = APPmaster.SiteTestIC[site];
                            open = new List<bool[]>()
                            {
                                new bool[] { siteopne[0][0],siteopne[0][1],siteopne[0][2] ,siteopne[0][3]  },
                                new bool[] { siteopne[1][0],siteopne[1][1],siteopne[1][2] ,siteopne[1][3]  },
                            };
                            PS测试区气缸下压动作流程.StepID = 510;
                        }
                        else
                        {
                            site++;
                            if (site >= 16)
                            {
                                site = 0;
                                //PS测试区气缸下压动作流程.StepID = 500;
                            }
                        }
                        break;

                    case 510:
                        PS测试区气缸下压动作流程.StepName = $"测试区气缸前移";
                        Debug.WriteLine($"PS测试区气缸下压动作流程.StepID={PS测试区气缸下压动作流程.StepID}->{PS测试区气缸下压动作流程.StepName}");
                        result = 测试区_位移气缸到位输出(site);
                        if (result == "Success")
                        {
                            PS测试区气缸下压动作流程.StepID = 515;
                        }
                        else
                        {
                            Debug.WriteLine($"测试区Site{site + 1}气缸前移失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 515:
                        PS测试区气缸下压动作流程.StepName = $"测试区气缸下压";
                        Debug.WriteLine($"PS测试区气缸下压动作流程.StepID={PS测试区气缸下压动作流程.StepID}->{PS测试区气缸下压动作流程.StepName}");
                        result = 测试区_下压气缸到位输出(site);
                        if (result == "Success")
                        {
                            PS测试区气缸下压动作流程.StepID = 520;
                        }
                        else
                        {
                            Debug.WriteLine($"测试区Site{site + 1}气缸下压失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 520:
                        PS测试区气缸下压动作流程.StepName = $"发送测试信号";
                        Debug.WriteLine($"PS测试区气缸下压动作流程.StepID={PS测试区气缸下压动作流程.StepID}->{PS测试区气缸下压动作流程.StepName}");
                        result = APPmaster.Com.Send(site, open);
                        if (result == "Success")
                        {
                            SiteTestState[site] = 2;
                            PS测试区气缸下压动作流程.StepID = 599;
                        }
                        else
                        {
                            Debug.WriteLine($"发送Site{site + 1}测试信号失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 599:
                        PS测试区气缸下压动作流程.StepName = $"测试区{site + 1}气缸动作流程完成";
                        Debug.WriteLine($"PS测试区气缸下压动作流程.StepID={PS测试区气缸下压动作流程.StepID}->{PS测试区气缸下压动作流程.StepName}");
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
            PS测试区气缸上抬动作流程.StepID = 500;
            while (true)
            {
                Thread.Sleep(10);

                if (!是否可以开始运行())
                {
                    continue;
                }

                switch (PS测试区气缸上抬动作流程.StepID)
                {
                    case 500:
                        PS测试区气缸上抬动作流程.StepName = "路径规划";
                        PS测试区气缸上抬动作流程.Condition = true;
                        PS测试区气缸上抬动作流程.Permit = true;
                        if (PS测试区气缸上抬动作流程.Condition && PS测试区气缸上抬动作流程.Permit)
                        {
                            PS测试区气缸上抬动作流程.Processing = true;
                            PS测试区气缸上抬动作流程.StepID = 505;
                        }
                        break;

                    case 505:
                        PS测试区气缸上抬动作流程.StepName = $"循环检测Site状态";
                        if (SiteTestState[site] == 3) //已测试
                        {
                            Debug.WriteLine($"PS测试区气缸上抬动作流程.StepID={PS测试区气缸上抬动作流程.StepID}->循环检测Site状态:{site + 1}测试完成");
                            PS测试区气缸上抬动作流程.StepID = 510;
                        }
                        else
                        {
                            site++;
                            if (site >= 16)
                            {
                                site = 0;
                            }
                        }
                        break;

                    case 510:
                        PS测试区气缸上抬动作流程.StepName = $"测试区气缸抬升";
                        Debug.WriteLine($"PS测试区气缸上抬动作流程.StepID={PS测试区气缸上抬动作流程.StepID}->{PS测试区气缸上抬动作流程.StepName}");
                        result = 测试区_下压气缸原位输出(site);
                        if (result == "Success")
                        {
                            PS测试区气缸上抬动作流程.StepID = 515;
                        }
                        else
                        {
                            Debug.WriteLine($"测试区Site{site + 1}气缸抬升失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 515:
                        PS测试区气缸上抬动作流程.StepName = $"测试区气缸后移";
                        Debug.WriteLine($"PS测试区气缸上抬动作流程.StepID={PS测试区气缸上抬动作流程.StepID}->{PS测试区气缸上抬动作流程.StepName}");
                        result = 测试区_位移气缸原位输出(site);
                        if (result == "Success")
                        {
                            SiteTestState[site] = 4;
                            PS测试区气缸上抬动作流程.StepID = 599;
                        }
                        else
                        {
                            Debug.WriteLine($"测试区Site{site + 1}气缸后移失败!" + result);
                            ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 599:
                        PS测试区气缸上抬动作流程.StepName = $"测试区Site{site + 1}气缸上抬动作流程完成";
                        Debug.WriteLine($"PS测试区气缸上抬动作流程.StepID={PS测试区气缸上抬动作流程.StepID}->{PS测试区气缸上抬动作流程.StepName}");
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
                "X038_BIN平移按键",
                "X036_AUTO1门感应",
                "X042_AUTO2门感应"
            };

            while (true)
            {
                string result = motionEx.GetDi(ioInfos, out string[] io);
                if (result == "Success")
                {
                    bool start = io[0] == "1"; //"X001_开始"
                    if (CheckEdgeSingal(start, ref StartRiseCache, true) || ProgramStart)
                    {
                        Debug.WriteLine("按下开始");
                        ProgramStart = false;
                        APPmaster.IsPause = false;
                        APPmaster.IsStart = true;
                        设备状态 = 1;

                        motionEx.SetDo("Y001_开始灯", true);
                        motionEx.SetDo("Y002_暂停灯", false);
                    }
                    bool stop = io[1] == "1"; //"X002_暂停"
                    if (CheckEdgeSingal(stop, ref StopRiseCache, true) || ProgramPause)
                    {
                        Debug.WriteLine("按下暂停");

                        ProgramPause = false;
                        APPmaster.IsPause = true;
                        APPmaster.IsStart = false;

                        if (设备状态 == 1) 设备状态 = 2;

                        motionEx.Motion.AllAxisStop();

                        motionEx.SetDo("Y001_开始灯", false);
                        motionEx.SetDo("Y002_暂停灯", true);
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
                        if (入料IC忽略 == false) 入料IC忽略 = true;
                        if (出料IC忽略 == false) 出料IC忽略 = true;
                        if (测试IC忽略 == false) 测试IC忽略 = true;
                        //motionEx.SetGNME601_Output(GNME601IO.Y004_重试灯, false);
                        //motionEx.SetGNME601_Output(GNME601IO.Y005_忽略灯, true);
                        //motionEx.SetGNME601_Output(GNME601IO.Y008_结批灯, false);
                        //motionEx.SetGNME601_Output(GNME601IO.Y011_换TRAY灯, false);
                    }
                    bool fixedstop = io[5] == "1"; //X006_定点停止
                    if (CheckEdgeSingal(fixedstop, ref FixedStopRiseCache, true))
                    {
                        Debug.WriteLine("按下定点停止");
                        定点停止允许 = false;
                    }
                    bool fixedstart = io[6] == "1"; //X007_定点开始
                    if (CheckEdgeSingal(fixedstart, ref FixedStartRiseCache, true))
                    {
                        Debug.WriteLine("按下定点开始");
                        定点停止允许 = true;
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
                        if (!H5509正在归零 && 设备状态 == 0)
                        {
                            H5509正在归零 = true;
                            H5509归零 = false;
                            IsPause = false;
                            motionEx.SetDo("Y001_开始灯", false);
                            motionEx.SetDo("Y002_暂停灯", false);
                            motionEx.SetDo("Y010_归零灯", true);
                            Task.Run(() =>
                            {
                                result = 整机回零流程();
                                if (result == "Success")
                                {
                                    H5509归零 = true;
                                    Debug.WriteLine("归零完成");
                                }
                                else Debug.WriteLine("归零失败");

                                H5509正在归零 = false;
                                motionEx.SetDo("Y010_归零灯", false);
                                motionEx.SetDo("Y001_开始灯", false);
                                motionEx.SetDo("Y002_暂停灯", false);
                            });
                        }
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
                    bool traymove = io[17] == "1"; //X038_BIN平移按键
                    if (CheckEdgeSingal(traymove, ref TrayMoveRiseCache, true))
                    {
                        Debug.WriteLine("按下Tray平移");
                        if (!Tray平移运动标记 && 设备状态 == 0)
                        {
                            Tray平移运动标记 = true;

                            motionEx.SetDo("Y040_BIN平移按键灯", true);
                            Task.Run(() =>
                            {
                                if (Tray平移左右标记 == 0)
                                {
                                    result = Fix3放IC盘轴移动(初始点位.出料_Fix3_右移动, 1);
                                    if (result == "Success")
                                    {
                                        Tray平移左右标记 = 1;
                                    }
                                    else Debug.WriteLine("放Tary盘轴移动失败!" + result);
                                }

                                else if (Tray平移左右标记 == 1)
                                {
                                    result = Fix3放IC盘轴移动(0, 1);
                                    if (result == "Success")
                                    {
                                        Tray平移左右标记 = 0;
                                    }
                                    else Debug.WriteLine("放Tary盘轴移动失败!" + result);
                                }

                                motionEx.SetDo("Y040_BIN平移按键灯", false);

                                Tray平移运动标记 = false;
                            });
                        }
                    }


                    bool x36 = io[18] == "1";  //X018_空盘门感应
                    if (CheckEdgeSingal(x36, ref Auto1DoorRiseCache, true))
                    {
                        Debug.WriteLine("Auto1仓门已关闭");
                    }
                    else if (CheckEdgeSingal(x36, ref Auto1DoorFallCache, false))
                    {
                        Auto1料仓打开标记 = false;
                        Debug.WriteLine("Auto1仓门被打开");
                    }

                    bool x42 = io[19] == "1"; //X024_Load门感应
                    if (CheckEdgeSingal(x42, ref Auto2DoorRiseCache, true))
                    {
                        Debug.WriteLine("Auto2仓门已关闭");
                    }
                    else if (CheckEdgeSingal(x42, ref Auto2DoorFallCache, false))
                    {
                        Auto2料仓打开标记 = false;
                        Debug.WriteLine("Auto2仓门被打开");
                    }
                }
                Thread.Sleep(5);
            }
        }

        public void Empty顶盘流程()
        {
            string result = "";
            PS空盘上顶流程.StepID = 500;
            while (true)
            {
                //if(!是否可以开始运行())
                //{
                //    continue;
                //}

                Thread.Sleep(10);

                if (APPmaster.IsPause)
                {
                    continue;
                }

                switch (PS空盘上顶流程.StepID)
                {
                    case 500:
                        PS空盘上顶流程.StepName = "路径规划";
                        PS空盘上顶流程.Condition = Empty上顶标记 == 2;
                        PS空盘上顶流程.Permit = !PS空盘上顶流程.Processing;
                        if (PS空盘上顶流程.Condition && PS空盘上顶流程.Permit)
                        {
                            PS空盘上顶流程.Processing = true;
                            PS空盘上顶流程.StepID = 505;
                        }
                        else Thread.Sleep(10);
                        break;

                    case 505:
                        PS空盘上顶流程.StepName = "Empty向上移动";
                        Debug.WriteLine($"PS空盘上顶流程.StepID={PS空盘上顶流程.StepID}->{PS空盘上顶流程.StepName}");
                        result = Empty向上移动();
                        if (result == "Success")
                        {
                            PS空盘上顶流程.StepID = 510;
                        }
                        else
                        {
                            PS空盘上顶流程.StepID = 600;
                            Debug.WriteLine("Empty向上移动失败!" + result);
                        }
                        break;

                    case 510:
                        PS空盘上顶流程.StepName = "Empty向下移动";
                        Debug.WriteLine($"PS空盘上顶流程.StepID={PS空盘上顶流程.StepID}->{PS空盘上顶流程.StepName}");
                        result = Empty向下移动();
                        if (result == "Success")
                        {
                            PS空盘上顶流程.StepID = 515;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine("Empty向下移动失败!" + result);
                        }
                        break;

                    case 515:
                        PS空盘上顶流程.StepName = "Empty向上移动固定距离";
                        Debug.WriteLine($"PS空盘上顶流程.StepID={PS空盘上顶流程.StepID}->{PS空盘上顶流程.StepName}");
                        result = motionEx.AxisRelativeMove("M12_Empty顶盘轴", 2.5);//3175 1.5875
                        if (result == "Success")
                        {
                            Empty上顶标记 = 1;
                            PS空盘上顶流程.StepID = 599;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine("Empty向上移动固定距离失败!" + result);
                        }
                        break;

                    case 599:
                        PS空盘上顶流程.StepName = $"空盘上顶流程完成";
                        Debug.WriteLine($"PS空盘上顶流程.StepID={PS空盘上顶流程.StepID}->{PS空盘上顶流程.StepName}");
                        PS空盘上顶流程.Processing = false;
                        PS空盘上顶流程.StepID = 500;
                        break;

                    case 600:
                        PS空盘上顶流程.StepName = "Empty顶盘轴移动到原点";
                        Debug.WriteLine($"PS空盘上顶流程.StepID={PS空盘上顶流程.StepID}->{PS空盘上顶流程.StepName}");
                        result = Empty顶盘轴移动(1);
                        if (result == "Success")
                        {
                            PS空盘上顶流程.StepID = 605;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Empty顶盘轴移动到原点失败！" + result);
                        }
                        break;

                    case 605:
                        PS空盘上顶流程.StepName = "Empty顶盘轴回零";
                        Debug.WriteLine($"PS空盘上顶流程.StepID={PS空盘上顶流程.StepID}->{PS空盘上顶流程.StepName}");
                        result = Empty顶盘轴回零();
                        if (result == "Success")
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine("空盘区没有空盘");
                            PS空盘上顶流程.StepID = 505;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Empty顶盘轴回零失败！" + result);
                        }
                        break;

                    default:
                        Debug.WriteLine(("PS空盘上顶流程.StepID =" + PS空盘上顶流程.StepID.ToString() + "对应流程不存在"));
                        break;
                }
            }
        }

        public void Empty下降流程()
        {
            string result = "";
            string result2 = "";
            double movepos = 0;
            PS空盘下降流程.StepID = 500;
            while (true)
            {
                //if(!是否可以开始运行())
                //{
                //    continue;
                //}

                Thread.Sleep(10);

                if (APPmaster.IsPause)
                {
                    continue;
                }

                switch (PS空盘下降流程.StepID)
                {
                    case 500:
                        PS空盘下降流程.StepName = "路径规划";
                        PS空盘下降流程.Condition = Empty下降标记 == 2;
                        PS空盘下降流程.Permit = !PS空盘下降流程.Processing;
                        if (PS空盘下降流程.Condition && PS空盘下降流程.Permit)
                        {
                            PS空盘下降流程.Processing = true;
                            PS空盘下降流程.StepID = 501;
                        }
                        else Thread.Sleep(10);
                        break;

                    case 501:
                        PS空盘下降流程.StepName = "获取Empty下移绝对位置";
                        result = motionEx.GetCmdEncPos("M12_Empty顶盘轴", out double prfpos1);
                        if (result == "Success")
                        {
                            result = GetAxisRunInfo("M12_Empty顶盘轴", out AxisRunInfo axisRunInfo);
                            if (result == "Success")
                            {
                                movepos = Convert.ToDouble(prfpos1 * axisRunInfo.步长mm) - Tray_厚度; //6.35
                                PS空盘下降流程.StepID = 505;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Debug.WriteLine("M12_Empty顶盘轴信息获取失败!" + result);
                            }
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine("M12_Empty顶盘轴实时位置失败!" + result);
                        }
                        break;

                    case 505:
                        PS空盘下降流程.StepName = $"Empty向下移动{Tray_厚度}mm";
                        Debug.WriteLine($"PS空盘下降流程.StepID={PS空盘下降流程.StepID}->{PS空盘下降流程.StepName}");
                        result = Empty顶盘轴移动(movepos);
                        if (result == "Success")
                        {
                            PS空盘下降流程.StepID = 510;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine("Empty向下移动固定距离失败!" + result);
                        }
                        break;

                    case 510:
                        PS空盘下降流程.StepName = "判断Empty顶盘轴是否走到最小值";
                        Debug.WriteLine($"PS空盘下降流程.StepID={PS空盘下降流程.StepID}->{PS空盘下降流程.StepName}");
                        result = motionEx.GetCmdEncPos("M12_Empty顶盘轴", out double prfpos);
                        if (result == "Success")
                        {
                            if (prfpos < 空盘满盘判断值)
                            {
                                Debug.WriteLine($"M12_Empty顶盘轴当前行程{prfpos}<{空盘满盘判断值}");
                                Debug.WriteLine("空盘区空盘放满");
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Empty料仓打开标记 = true;
                                PS空盘下降流程.StepID = 600; //空盘满盘报警
                            }
                            else PS空盘下降流程.StepID = 599;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"判断Empty顶盘轴是否走到最大值失败！" + result);
                        }
                        break;

                    case 599:
                        Empty下降标记 = 1;
                        PS空盘下降流程.StepName = $"空盘下降流程完成";
                        Debug.WriteLine($"PS空盘下降流程.StepID={PS空盘下降流程.StepID}->{PS空盘下降流程.StepName}");
                        PS空盘下降流程.Processing = false;
                        PS空盘下降流程.StepID = 500;
                        break;

                    case 600:
                        PS空盘下降流程.StepName = $"检测空盘感应门有没有被打开";
                        Debug.WriteLine($"PS空盘下降流程.StepID={PS空盘下降流程.StepID}->{PS空盘下降流程.StepName}");
                        if (Empty料仓打开标记 == false)
                        {
                            PS空盘下降流程.StepID = 605;
                        }
                        else
                        {
                            Debug.WriteLine("空盘区空盘放满");
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 605:
                        PS空盘下降流程.StepName = "检测Empty是否拿走空盘";
                        result = Empty_前Tray存在感应(out string value);
                        result2 = Empty_后Tray放歪感应(out string value2);
                        if (result == "Success" && result2 == "Success")
                        {
                            if (value == "1" && value2 == "1") //返回0是挡到感应器,1是没有挡到感应器
                            {
                                Empty上顶标记 = 2; //触发上顶
                                PS空盘下降流程.StepID = 599;
                            }
                            else //没有挡到感应器或者放歪感应器被挡到说明没有放盘成功，需重新放盘
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;

                                if (value != "0")
                                {
                                    Debug.WriteLine($"抓盘手臂放盘失败！Empty前有盘感应到有Tray盘,value:" + value);
                                }
                                if (value2 != "0")
                                {
                                    Debug.WriteLine($"抓盘手臂放盘失败！Empty后放歪感应到有Tray盘,value:" + value2);
                                }
                                Debug.WriteLine("空盘区空盘放满");
                            }
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            if (result != "Success") Debug.WriteLine($"读取Empty前Tray存在感应失败！" + result);
                            if (result2 != "Success") Debug.WriteLine($"读取Empty后Tray放歪感应失败！" + result2);
                        }
                        break;

                    default:
                        Debug.WriteLine(("PS空盘下降流程.StepID =" + PS空盘下降流程.StepID.ToString() + "对应流程不存在"));
                        break;
                }
            }
        }

        public void Load顶盘流程()
        {
            string result = "";
            PS待测盘上顶流程.StepID = 500;
            while (true)
            {
                Thread.Sleep(10);
                //if (!是否可以开始运行())
                //{
                //    continue;
                //}

                if (APPmaster.IsPause)
                {
                    continue;
                }

                switch (PS待测盘上顶流程.StepID)
                {
                    case 500:
                        PS待测盘上顶流程.StepName = "路径规划";
                        PS待测盘上顶流程.Condition = Load上顶标记 == 2;
                        PS待测盘上顶流程.Permit = !PS待测盘上顶流程.Processing;
                        if (PS待测盘上顶流程.Condition && PS待测盘上顶流程.Permit)
                        {
                            PS待测盘上顶流程.Processing = true;
                            PS待测盘上顶流程.StepID = 505;
                        }
                        break;

                    case 505:
                        PS待测盘上顶流程.StepName = "Load_Tray前侧定位气缸原位输出";
                        Debug.WriteLine($"PS待测盘上顶流程.StepID={PS待测盘上顶流程.StepID}->{PS待测盘上顶流程.StepName}");
                        result = Load_Tray前侧定位气缸原位输出();
                        if (result == "Success")
                        {
                            PS待测盘上顶流程.StepID = 510;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Load_Tray前侧定位气缸原位输出失败！" + result);
                        }
                        break;

                    case 510:
                        PS待测盘上顶流程.StepName = "Load向上移动";
                        Debug.WriteLine($"PS待测盘上顶流程.StepID={PS待测盘上顶流程.StepID}->{PS待测盘上顶流程.StepName}");
                        result = Load向上移动();
                        if (result == "Success")
                        {
                            PS待测盘上顶流程.StepID = 515;
                        }
                        else
                        {
                            PS待测盘上顶流程.StepID = 600;
                            Debug.WriteLine($"Load向上移动失败！" + result);
                        }
                        break;

                    case 515:
                        PS待测盘上顶流程.StepName = "Load向下移动";
                        Debug.WriteLine($"PS待测盘上顶流程.StepID={PS待测盘上顶流程.StepID}->{PS待测盘上顶流程.StepName}");
                        result = Load向下移动();
                        if (result == "Success")
                        {
                            PS待测盘上顶流程.StepID = 520;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Load向下移动失败！" + result);
                        }
                        break;

                    case 520:
                        PS待测盘上顶流程.StepName = "Load向上移动固定距离";
                        Debug.WriteLine($"PS待测盘上顶流程.StepID={PS待测盘上顶流程.StepID}->{PS待测盘上顶流程.StepName}");
                        result = motionEx.AxisRelativeMove("M13_Loader顶盘轴", 0.5);//3175 1.5875
                        if (result == "Success")
                        {
                            PS待测盘上顶流程.StepID = 525;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Load向上移动固定距离失败！" + result);
                        }
                        break;

                    case 525:
                        PS待测盘上顶流程.StepName = "Load_Tray前侧定位气缸到位输出";
                        Debug.WriteLine($"PS待测盘上顶流程.StepID={PS待测盘上顶流程.StepID}->{PS待测盘上顶流程.StepName}");
                        result = Load_Tray前侧定位气缸到位输出();
                        if (result == "Success")
                        {
                            Load上顶标记 = 1;
                            PS待测盘上顶流程.StepID = 599;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Load_Tray前侧定位气缸到位输出失败！" + result);
                        }
                        break;

                    case 599:
                        PS待测盘上顶流程.StepName = $"待测盘上顶流程完成";
                        Debug.WriteLine($"PS待测盘上顶流程.StepID={PS待测盘上顶流程.StepID}->{PS待测盘上顶流程.StepName}");
                        PS待测盘上顶流程.Processing = false;
                        PS待测盘上顶流程.StepID = 500;
                        break;


                    case 600:
                        PS待测盘上顶流程.StepName = "Load顶盘轴移动到原点";
                        Debug.WriteLine($"PS待测盘上顶流程.StepID={PS待测盘上顶流程.StepID}->{PS待测盘上顶流程.StepName}");
                        result = Load顶盘轴移动(1);
                        if (result == "Success")
                        {
                            PS待测盘上顶流程.StepID = 605;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Load顶盘轴移动到原点失败！" + result);
                        }
                        break;

                    case 605:
                        PS待测盘上顶流程.StepName = "Load顶盘轴回零";
                        Debug.WriteLine($"PS待测盘上顶流程.StepID={PS待测盘上顶流程.StepID}->{PS待测盘上顶流程.StepName}");
                        result = Load顶盘轴回零();
                        if (result == "Success")
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine("待测区没有盘");
                            PS待测盘上顶流程.StepID = 610;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Load顶盘轴回零失败！" + result);
                        }
                        break;

                    case 610:
                        PS待测盘上顶流程.StepName = "待测区没有盘";
                        if (结批标记)
                        {
                            Load上顶标记 = 1;
                            PS待测盘上顶流程.StepID = 599;
                        }
                        else
                        {
                            PS待测盘上顶流程.StepID = 505;
                        }
                        break;

                    default:
                        Debug.WriteLine(("PS待测盘上顶流程.StepID =" + PS待测盘上顶流程.StepID.ToString() + "对应流程不存在"));
                        break;
                }
            }
        }

        public void Load下降流程()
        {

        }

        public void Auto1顶盘流程()
        {
            string result = "";
            PS手动盘1上顶流程.StepID = 500;
            while (true)
            {
                Thread.Sleep(10);
                //if (!是否可以开始运行())
                //{
                //    continue;
                //}

                if (APPmaster.IsPause)
                {
                    continue;
                }

                switch (PS手动盘1上顶流程.StepID)
                {
                    case 500:
                        PS手动盘1上顶流程.StepName = "路径规划";
                        PS手动盘1上顶流程.Condition = Auto1上顶标记 == 2;
                        PS手动盘1上顶流程.Permit = !PS手动盘1上顶流程.Processing;
                        if (PS手动盘1上顶流程.Condition && PS手动盘1上顶流程.Permit)
                        {
                            PS手动盘1上顶流程.Processing = true;
                            PS手动盘1上顶流程.StepID = 505;
                        }
                        else Thread.Sleep(10);
                        break;

                    case 505:
                        PS手动盘1上顶流程.StepName = "Auto1_Tray前侧定位气缸原位输出";
                        Debug.WriteLine($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
                        result = Auto1_Tray前侧定位气缸原位输出();
                        if (result == "Success")
                        {
                            PS手动盘1上顶流程.StepID = 510;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Auto1_Tray前侧定位气缸原位输出失败!" + result);
                        }
                        break;

                    case 510:
                        PS手动盘1上顶流程.StepName = "Auto1向上移动";
                        Debug.WriteLine($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
                        result = Auto1向上移动();
                        if (result == "Success")
                        {
                            PS手动盘1上顶流程.StepID = 515;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Auto1向上移动失败!" + result);
                        }
                        break;

                    case 515:
                        PS手动盘1上顶流程.StepName = "Auto1向上移动固定距离";
                        Debug.WriteLine($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
                        result = motionEx.AxisRelativeMove("M14_Auto1顶盘轴", 21);//3175 1.5875
                        if (result == "Success")
                        {
                            PS手动盘1上顶流程.StepID = 520;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Auto1向上移动固定距离失败!" + result);
                        }
                        break;

                    case 520:
                        PS手动盘1上顶流程.StepName = "Auto1_Tray上顶气缸到位输出";
                        Debug.WriteLine($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
                        result = Auto1_Tray上顶气缸到位输出();//3175 1.5875
                        if (result == "Success")
                        {
                            PS手动盘1上顶流程.StepID = 525;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Auto1_Tray上顶气缸到位输出失败!" + result);
                        }
                        break;

                    case 525:
                        PS手动盘1上顶流程.StepName = "Auto1向下移动";
                        Debug.WriteLine($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
                        result = Auto1向下移动();
                        if (result == "Success")
                        {
                            PS手动盘1上顶流程.StepID = 530;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Auto1向下移动失败!" + result);
                        }
                        break;

                    case 530:
                        PS手动盘1上顶流程.StepName = "Auto1向下移动固定距离";
                        Debug.WriteLine($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
                        result = motionEx.AxisRelativeMove("M14_Auto1顶盘轴", -15);//3175 1.5875
                        if (result == "Success")
                        {
                            PS手动盘1上顶流程.StepID = 535;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Auto1向下移动固定距离失败!" + result);
                        }
                        break;

                    case 535:
                        PS手动盘1上顶流程.StepName = "Auto1_Tray上顶气缸原位输出";
                        Debug.WriteLine($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
                        result = Auto1_Tray上顶气缸原位输出();
                        if (result == "Success")
                        {
                            PS手动盘1上顶流程.StepID = 540;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Auto1_Tray上顶气缸原位输出失败!" + result);
                        }
                        break;

                    case 540:
                        PS手动盘1上顶流程.StepName = "判断Auto1料仓是否满盘";
                        Debug.WriteLine($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
                        result = motionEx.GetCmdEncPos("M14_Auto1顶盘轴", out double prfpos);
                        if (result == "Success")
                        {
                            if (prfpos < Auto1满盘判断值)
                            {
                                Auto1料仓满盘标记 = true;
                                Debug.WriteLine($"M14_Auto1顶盘轴当前行程{prfpos}<Auto1满盘判断值{Auto1满盘判断值}");
                            }
                            else Auto1料仓满盘标记 = false;

                            Auto1上顶标记 = 1;
                            PS手动盘1上顶流程.StepID = 599;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"判断Auto1料仓是否满盘失败！" + result);
                        }
                        break;

                    case 599:
                        PS手动盘1上顶流程.StepName = $"手动盘1上顶流程完成";
                        Debug.WriteLine($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
                        PS手动盘1上顶流程.Processing = false;
                        PS手动盘1上顶流程.StepID = 500;
                        break;

                    default:
                        Debug.WriteLine(("PS手动盘1上顶流程.StepID =" + PS手动盘1上顶流程.StepID.ToString() + "对应流程不存在"));
                        break;
                }
            }
        }

        public void Auto1顶盘流程2()
        {
            string result = "";
            double movepos = 0;

            PS手动盘1上顶流程.StepID = 500;
            while (true)
            {
                Thread.Sleep(10);
                //if (!是否可以开始运行())
                //{
                //    continue;
                //}

                if (APPmaster.IsPause)
                {
                    continue;
                }

                switch (PS手动盘1上顶流程.StepID)
                {
                    case 500:
                        PS手动盘1上顶流程.StepName = "路径规划";
                        PS手动盘1上顶流程.Condition = Auto1上顶标记 == 2;
                        PS手动盘1上顶流程.Permit = !PS手动盘1上顶流程.Processing;
                        if (PS手动盘1上顶流程.Condition && PS手动盘1上顶流程.Permit)
                        {
                            PS手动盘1上顶流程.Processing = true;
                            PS手动盘1上顶流程.StepID = 505;
                        }
                        else Thread.Sleep(10);
                        break;

                    case 505:
                        PS手动盘1上顶流程.StepName = "Auto1_Tray前侧定位气缸原位输出";
                        Debug.WriteLine($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
                        result = Auto1_Tray前侧定位气缸原位输出();
                        if (result == "Success")
                        {
                            PS手动盘1上顶流程.StepID = 506;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Auto1_Tray前侧定位气缸原位输出失败!" + result);
                        }
                        break;

                    case 506:
                        PS手动盘1上顶流程.StepName = "获取Auto1上移绝对位置";
                        result = motionEx.GetCmdEncPos("M14_Auto1顶盘轴", out double prfpos1);
                        if (result == "Success")
                        {
                            result = GetAxisRunInfo("M14_Auto1顶盘轴", out AxisRunInfo axisRunInfo);
                            if (result == "Success")
                            {
                                movepos = Convert.ToDouble(prfpos1 * axisRunInfo.步长mm) + Auto1上移固定距离; //6.35
                                PS手动盘1上顶流程.StepID = 510;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Debug.WriteLine("M14_Auto1顶盘轴信息获取失败!" + result);
                            }
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine("M14_Auto1顶盘轴实时位置失败!" + result);
                        }
                        break;

                    case 510:
                        PS手动盘1上顶流程.StepName = $"Auto1向上移动绝对位置{movepos}mm";
                        Debug.WriteLine($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
                        result = Auto1顶盘轴移动(movepos);
                        if (result == "Success")
                        {
                            PS手动盘1上顶流程.StepID = 515;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Auto1向上移动失败!" + result);
                        }
                        break;

                    case 515:
                        PS手动盘1上顶流程.StepName = "Auto1_Tray上顶气缸到位输出";
                        Debug.WriteLine($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
                        result = Auto1_Tray上顶气缸到位输出();//3175 1.5875
                        if (result == "Success")
                        {
                            PS手动盘1上顶流程.StepID = 520;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Auto1_Tray上顶气缸到位输出失败!" + result);
                        }
                        break;

                    case 520:
                        PS手动盘1上顶流程.StepName = "获取Auto1下移绝对位置";
                        Debug.WriteLine($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
                        result = motionEx.GetCmdEncPos("M14_Auto1顶盘轴", out double prfpos2);
                        if (result == "Success")
                        {
                            result = GetAxisRunInfo("M14_Auto1顶盘轴", out AxisRunInfo axisRunInfo);
                            if (result == "Success")
                            {
                                movepos = Convert.ToDouble(prfpos2 * axisRunInfo.步长mm) - Auto1上移固定距离 - Tray_厚度; //6.35
                                PS手动盘1上顶流程.StepID = 525;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Debug.WriteLine("M14_Auto1顶盘轴信息获取失败!" + result);
                            }
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine("M14_Auto1顶盘轴实时位置失败!" + result);
                        }
                        break;

                    case 525:
                        PS手动盘1上顶流程.StepName = $"Auto1向下移动绝对位置{movepos}mm";
                        Debug.WriteLine($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
                        result = Auto1顶盘轴移动(movepos);
                        if (result == "Success")
                        {
                            PS手动盘1上顶流程.StepID = 530;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Auto1向下移动失败!" + result);
                        }
                        break;

                    case 530:
                        PS手动盘1上顶流程.StepName = "Auto1_Tray上顶气缸原位输出";
                        Debug.WriteLine($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
                        result = Auto1_Tray上顶气缸原位输出();
                        if (result == "Success")
                        {
                            PS手动盘1上顶流程.StepID = 535;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Auto1_Tray上顶气缸原位输出失败!" + result);
                        }
                        break;

                    case 535:
                        PS手动盘1上顶流程.StepName = "判断Auto1料仓是否满盘";
                        Debug.WriteLine($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
                        result = motionEx.GetCmdEncPos("M14_Auto1顶盘轴", out double prfpos);
                        if (result == "Success")
                        {
                            if (prfpos < Auto1满盘判断值)
                            {
                                Auto1料仓满盘标记 = true;
                                Debug.WriteLine($"M14_Auto1顶盘轴当前行程{prfpos}<Auto1满盘判断值{Auto1满盘判断值}");
                            }
                            else Auto1料仓满盘标记 = false;

                            Auto1上顶标记 = 1;
                            PS手动盘1上顶流程.StepID = 599;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"判断Auto1料仓是否满盘失败！" + result);
                        }
                        break;

                    case 599:
                        PS手动盘1上顶流程.StepName = $"手动盘1上顶流程完成";
                        Debug.WriteLine($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
                        PS手动盘1上顶流程.Processing = false;
                        PS手动盘1上顶流程.StepID = 500;
                        break;

                    default:
                        Debug.WriteLine(("PS手动盘1上顶流程.StepID =" + PS手动盘1上顶流程.StepID.ToString() + "对应流程不存在"));
                        break;
                }
            }
        }

        public void Auto2顶盘流程()
        {
            string result = "";
            PS手动盘2上顶流程.StepID = 500;
            while (true)
            {
                Thread.Sleep(10);
                //if (!是否可以开始运行())
                //{
                //    continue;
                //}

                if (APPmaster.IsPause)
                {
                    continue;
                }

                switch (PS手动盘2上顶流程.StepID)
                {
                    case 500:
                        PS手动盘2上顶流程.StepName = "路径规划";
                        PS手动盘2上顶流程.Condition = Auto2上顶标记 == 2;
                        PS手动盘2上顶流程.Permit = !PS手动盘2上顶流程.Processing;
                        if (PS手动盘2上顶流程.Condition && PS手动盘2上顶流程.Permit)
                        {
                            PS手动盘2上顶流程.Processing = true;
                            PS手动盘2上顶流程.StepID = 505;
                        }
                        else Thread.Sleep(10);
                        break;

                    case 505:
                        PS手动盘2上顶流程.StepName = "Auto2_Tray前侧定位气缸原位输出";
                        Debug.WriteLine($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
                        result = Auto2_Tray前侧定位气缸原位输出();
                        if (result == "Success")
                        {
                            PS手动盘2上顶流程.StepID = 510;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Auto2_Tray前侧定位气缸原位输出失败!" + result);
                        }
                        break;

                    case 510:
                        PS手动盘2上顶流程.StepName = "Auto2向上移动";
                        Debug.WriteLine($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
                        result = Auto2向上移动();
                        if (result == "Success")
                        {
                            PS手动盘2上顶流程.StepID = 515;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Auto2向上移动失败!" + result);
                        }
                        break;

                    case 515:
                        PS手动盘2上顶流程.StepName = "Auto2向上移动固定距离";
                        Debug.WriteLine($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
                        result = motionEx.AxisRelativeMove("M15_Auto2顶盘轴", 21);//3175 1.5875
                        if (result == "Success")
                        {
                            PS手动盘2上顶流程.StepID = 520;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Auto2向上移动固定距离失败!" + result);
                        }
                        break;

                    case 520:
                        PS手动盘2上顶流程.StepName = "Auto2_Tray上顶气缸到位输出";
                        Debug.WriteLine($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
                        result = Auto2_Tray上顶气缸到位输出();//3175 1.5875
                        if (result == "Success")
                        {
                            PS手动盘2上顶流程.StepID = 525;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Auto2_Tray上顶气缸到位输出失败!" + result);
                        }
                        break;

                    case 525:
                        PS手动盘2上顶流程.StepName = "Auto2向下移动";
                        Debug.WriteLine($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
                        result = Auto2向下移动();
                        if (result == "Success")
                        {
                            PS手动盘2上顶流程.StepID = 530;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Auto2向下移动失败!" + result);
                        }
                        break;

                    case 530:
                        PS手动盘2上顶流程.StepName = "Auto2向下移动固定距离";
                        Debug.WriteLine($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
                        result = motionEx.AxisRelativeMove("M15_Auto2顶盘轴", -15);//3175 1.5875
                        if (result == "Success")
                        {
                            PS手动盘2上顶流程.StepID = 535;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Auto2向下移动固定距离失败!" + result);
                        }
                        break;

                    case 535:
                        PS手动盘2上顶流程.StepName = "Auto2_Tray上顶气缸原位输出";
                        Debug.WriteLine($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
                        result = Auto2_Tray上顶气缸原位输出();
                        if (result == "Success")
                        {
                            PS手动盘2上顶流程.StepID = 540;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Auto2_Tray上顶气缸原位输出失败!" + result);
                        }
                        break;


                    case 540:
                        PS手动盘2上顶流程.StepName = "判断Auto2料仓是否满盘";
                        Debug.WriteLine($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
                        result = motionEx.GetCmdEncPos("M15_Auto2顶盘轴", out double prfpos);
                        if (result == "Success")
                        {
                            if (prfpos < Auto2满盘判断值)
                            {
                                Auto2料仓满盘标记 = true;
                                Debug.WriteLine($"M15_Auto2顶盘轴当前行程{prfpos}<Auto2满盘判断值{Auto2满盘判断值}");
                            }
                            else Auto2料仓满盘标记 = false;

                            Auto2上顶标记 = 1;
                            PS手动盘2上顶流程.StepID = 599;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"判断Auto2料仓是否满盘失败！" + result);
                        }
                        break;


                    case 599:
                        PS手动盘2上顶流程.StepName = $"手动盘2上顶流程完成";
                        Debug.WriteLine($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
                        PS手动盘2上顶流程.Processing = false;
                        PS手动盘2上顶流程.StepID = 500;
                        break;


                    default:
                        Debug.WriteLine(("PS手动盘2上顶流程.StepID =" + PS手动盘2上顶流程.StepID.ToString() + "对应流程不存在"));
                        break;
                }
            }
        }
        public void Auto2顶盘流程2()
        {
            string result = "";
            double movepos = 0;
            PS手动盘2上顶流程.StepID = 500;
            while (true)
            {
                Thread.Sleep(10);
                //if (!是否可以开始运行())
                //{
                //    continue;
                //}

                if (APPmaster.IsPause)
                {
                    continue;
                }

                switch (PS手动盘2上顶流程.StepID)
                {
                    case 500:
                        PS手动盘2上顶流程.StepName = "路径规划";
                        PS手动盘2上顶流程.Condition = Auto2上顶标记 == 2;
                        PS手动盘2上顶流程.Permit = !PS手动盘2上顶流程.Processing;
                        if (PS手动盘2上顶流程.Condition && PS手动盘2上顶流程.Permit)
                        {
                            PS手动盘2上顶流程.Processing = true;
                            PS手动盘2上顶流程.StepID = 505;
                        }
                        else Thread.Sleep(10);
                        break;

                    case 505:
                        PS手动盘2上顶流程.StepName = "Auto2_Tray前侧定位气缸原位输出";
                        Debug.WriteLine($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
                        result = Auto2_Tray前侧定位气缸原位输出();
                        if (result == "Success")
                        {
                            PS手动盘2上顶流程.StepID = 506;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Auto2_Tray前侧定位气缸原位输出失败!" + result);
                        }
                        break;

                    case 506:
                        PS手动盘2上顶流程.StepName = "获取Auto2上移绝对位置";
                        result = motionEx.GetCmdEncPos("M15_Auto2顶盘轴", out double prfpos1);
                        if (result == "Success")
                        {
                            result = GetAxisRunInfo("M15_Auto2顶盘轴", out AxisRunInfo axisRunInfo);
                            if (result == "Success")
                            {
                                movepos = Convert.ToDouble(prfpos1 * axisRunInfo.步长mm) + Auto2上移固定距离; //6.35
                                PS手动盘2上顶流程.StepID = 510;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Debug.WriteLine("M15_Auto2顶盘轴信息获取失败!" + result);
                            }
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine("M15_Auto2顶盘轴实时位置失败!" + result);
                        }
                        break;

                    case 510:
                        PS手动盘2上顶流程.StepName = $"Auto2向上移动绝对位置{movepos}mm";
                        Debug.WriteLine($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
                        result = Auto2顶盘轴移动(movepos);
                        if (result == "Success")
                        {
                            PS手动盘2上顶流程.StepID = 515;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Auto2向上移动失败!" + result);
                        }
                        break;

                    case 515:
                        PS手动盘2上顶流程.StepName = "Auto2_Tray上顶气缸到位输出";
                        Debug.WriteLine($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
                        result = Auto2_Tray上顶气缸到位输出();//3175 1.5875
                        if (result == "Success")
                        {
                            PS手动盘2上顶流程.StepID = 520;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Auto2_Tray上顶气缸到位输出失败!" + result);
                        }
                        break;

                    case 520:
                        PS手动盘2上顶流程.StepName = "获取Auto2下移绝对位置";
                        Debug.WriteLine($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
                        result = motionEx.GetCmdEncPos("M15_Auto2顶盘轴", out double prfpos2);
                        if (result == "Success")
                        {
                            result = GetAxisRunInfo("M15_Auto2顶盘轴", out AxisRunInfo axisRunInfo);
                            if (result == "Success")
                            {
                                movepos = Convert.ToDouble(prfpos2 * axisRunInfo.步长mm) - Auto2上移固定距离 - Tray_厚度; //6.35
                                PS手动盘2上顶流程.StepID = 525;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Debug.WriteLine("M15_Auto2顶盘轴信息获取失败!" + result);
                            }
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine("M15_Auto2顶盘轴实时位置失败!" + result);
                        }
                        break;

                    case 525:
                        PS手动盘2上顶流程.StepName = $"Auto2向下移动绝对位置{movepos}mm";
                        Debug.WriteLine($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
                        result = Auto2顶盘轴移动(movepos);
                        if (result == "Success")
                        {
                            PS手动盘2上顶流程.StepID = 530;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Auto2向下移动失败!" + result);
                        }
                        break;

                    case 530:
                        PS手动盘2上顶流程.StepName = "Auto2_Tray上顶气缸原位输出";
                        Debug.WriteLine($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
                        result = Auto2_Tray上顶气缸原位输出();
                        if (result == "Success")
                        {
                            PS手动盘2上顶流程.StepID = 535;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Auto2_Tray上顶气缸原位输出失败!" + result);
                        }
                        break;

                    case 535:
                        PS手动盘2上顶流程.StepName = "判断Auto2料仓是否满盘";
                        Debug.WriteLine($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
                        result = motionEx.GetCmdEncPos("M15_Auto2顶盘轴", out double prfpos);
                        if (result == "Success")
                        {
                            if (prfpos < Auto2满盘判断值)
                            {
                                Auto2料仓满盘标记 = true;
                                Debug.WriteLine($"M15_Auto2顶盘轴当前行程{prfpos}<Auto2满盘判断值{Auto2满盘判断值}");
                            }
                            else Auto2料仓满盘标记 = false;

                            Auto2上顶标记 = 1;
                            PS手动盘2上顶流程.StepID = 599;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"判断Auto2料仓是否满盘失败！" + result);
                        }
                        break;


                    case 599:
                        PS手动盘2上顶流程.StepName = $"手动盘2上顶流程完成";
                        Debug.WriteLine($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
                        PS手动盘2上顶流程.Processing = false;
                        PS手动盘2上顶流程.StepID = 500;
                        break;


                    default:
                        Debug.WriteLine(("PS手动盘2上顶流程.StepID =" + PS手动盘2上顶流程.StepID.ToString() + "对应流程不存在"));
                        break;
                }
            }
        }

        public void 抓盘手臂取Empty盘()
        {
            //PS抓盘手臂取Empty空盘流程
            string result = "";
            PS抓盘手臂取Empty空盘流程.StepID = 500;
            while (true)
            {
                //if (!是否可以开始运行())
                //{
                //    continue;
                //}
                Thread.Sleep(10);
                if (APPmaster.IsPause)
                {
                    continue;
                }

                switch (PS抓盘手臂取Empty空盘流程.StepID)
                {
                    case 500:
                        PS抓盘手臂取Empty空盘流程.StepName = "路径规划";
                        PS抓盘手臂取Empty空盘流程.Condition = 抓盘手臂取Empty空盘标记 == 2 && Empty上顶标记 == 1;
                        PS抓盘手臂取Empty空盘流程.Permit = !PS抓盘手臂取Empty空盘流程.Processing;
                        if (PS抓盘手臂取Empty空盘流程.Condition && PS抓盘手臂取Empty空盘流程.Permit)
                        {
                            PS抓盘手臂取Empty空盘流程.Processing = true;
                            PS抓盘手臂取Empty空盘流程.StepID = 505;
                        }
                        else Thread.Sleep(10);
                        break;

                    case 505:
                        PS抓盘手臂取Empty空盘流程.StepName = "移Tray上下气缸原位输入读取";
                        Debug.WriteLine($"PS抓盘手臂取Empty空盘流程.StepID={PS抓盘手臂取Empty空盘流程.StepID}->{PS抓盘手臂取Empty空盘流程.StepName}");
                        result = 移Tray上下气缸原位输入读取(out string value);
                        if (result == "Success")
                        {
                            if (value == "1")
                            {
                                PS抓盘手臂取Empty空盘流程.StepID = 510;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Debug.WriteLine($"移Tray上下气缸未在原位");
                            }
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"移Tray上下气缸原位输入读取失败！" + result);
                        }
                        break;

                    case 510:
                        PS抓盘手臂取Empty空盘流程.StepName = "移Tray夹爪气缸原位输入读取";
                        Debug.WriteLine($"PS抓盘手臂取Empty空盘流程.StepID={PS抓盘手臂取Empty空盘流程.StepID}->{PS抓盘手臂取Empty空盘流程.StepName}");
                        result = 移Tray夹爪气缸原位输入读取(out value);
                        if (result == "Success")
                        {
                            if (value == "1")
                            {
                                PS抓盘手臂取Empty空盘流程.StepID = 515;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Debug.WriteLine($"移Tray夹爪气缸未在原位");
                            }
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"移Tray夹爪气缸原位输入读取失败！" + result);
                        }
                        break;

                    case 515:
                        PS抓盘手臂取Empty空盘流程.StepName = "放Tary盘轴移动到Empty";
                        Debug.WriteLine($"PS抓盘手臂取Empty空盘流程.StepID={PS抓盘手臂取Empty空盘流程.StepID}->{PS抓盘手臂取Empty空盘流程.StepName}");
                        result = 放Tary盘轴移动(初始点位.抓盘_Empty_X);
                        if (result == "Success")
                        {
                            PS抓盘手臂取Empty空盘流程.StepID = 520;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"放Tary盘轴移动到Empty失败！" + result);
                        }
                        break;

                    case 520:
                        PS抓盘手臂取Empty空盘流程.StepName = "移Tray上下气缸到位输出";
                        Debug.WriteLine($"PS抓盘手臂取Empty空盘流程.StepID={PS抓盘手臂取Empty空盘流程.StepID}->{PS抓盘手臂取Empty空盘流程.StepName}");
                        result = 移Tray上下气缸到位输出();
                        if (result == "Success")
                        {
                            PS抓盘手臂取Empty空盘流程.StepID = 525;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"移Tray上下气缸到位输出失败！" + result);
                        }
                        break;

                    case 525:
                        PS抓盘手臂取Empty空盘流程.StepName = "移Tray夹爪气缸到位输出";
                        Debug.WriteLine($"PS抓盘手臂取Empty空盘流程.StepID={PS抓盘手臂取Empty空盘流程.StepID}->{PS抓盘手臂取Empty空盘流程.StepName}");
                        result = 移Tray夹爪气缸到位输出();
                        if (result == "Success")
                        {
                            PS抓盘手臂取Empty空盘流程.StepID = 530;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"移Tray夹爪气缸到位输出失败！" + result);
                        }
                        break;

                    case 530:
                        PS抓盘手臂取Empty空盘流程.StepName = "移Tray上下气缸原位输出";
                        Debug.WriteLine($"PS抓盘手臂取Empty空盘流程.StepID={PS抓盘手臂取Empty空盘流程.StepID}->{PS抓盘手臂取Empty空盘流程.StepName}");
                        result = 移Tray上下气缸原位输出();
                        if (result == "Success")
                        {
                            PS抓盘手臂取Empty空盘流程.StepID = 535;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"移Tray上下气缸原位输出失败！" + result);
                        }
                        break;

                    case 535:
                        PS抓盘手臂取Empty空盘流程.StepName = "检测Empty是否无空盘";
                        result = Empty_前Tray存在感应(out value);
                        if (result == "Success")
                        {
                            if (value == "1") //返回0是挡到感应器,1是没有挡到感应器
                            {
                                //这时抓取手臂已完成任务，可以去放盘或者换盘了
                                //转盘手臂抓盘标记
                                Empty上顶标记 = 2; //空盘上顶
                                抓盘手臂取Empty空盘标记 = 1;
                                PS抓盘手臂取Empty空盘流程.StepID = 599;
                            }
                            else //挡到感应器说明没有抓盘成功，需重新抓盘
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                PS抓盘手臂取Empty空盘流程.StepID = 536; //松开夹爪重新再抓盘
                                Debug.WriteLine($"抓盘手臂抓盘失败！" + result);
                            }
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"检测Empty是否无空盘失败！" + result);
                        }
                        break;

                    case 536:
                        PS抓盘手臂取Empty空盘流程.StepName = "移Tray夹爪气缸原位输出";
                        Debug.WriteLine($"PS抓盘手臂取Empty空盘流程.StepID={PS抓盘手臂取Empty空盘流程.StepID}->{PS抓盘手臂取Empty空盘流程.StepName}");
                        result = 移Tray夹爪气缸原位输出();
                        if (result == "Success")
                        {
                            PS抓盘手臂取Empty空盘流程.StepID = 520;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"移Tray夹爪气缸原位输出失败！" + result);
                        }
                        break;

                    //case 540:
                    //    PS抓盘手臂取Empty空盘流程.StepName = "判断Empty顶盘轴是否走到最大值";
                    //    Debug.WriteLine($"PS抓盘手臂取Empty空盘流程.StepID={PS抓盘手臂取Empty空盘流程.StepID}->{PS抓盘手臂取Empty空盘流程.StepName}");
                    //    result = motionEx.GetCmdEncPos("M12_Empty顶盘轴", out double prfpos);
                    //    if (result == "Success")
                    //    {
                    //        if (prfpos + 12700 > 空盘上顶最大值)
                    //        {
                    //            Debug.WriteLine($"M12_Empty顶盘轴当前上升行程{prfpos + 12700}>{空盘上顶最大值}");
                    //            PS抓盘手臂取Empty空盘流程.StepID = 700; //执行回零再触发上顶
                    //        }
                    //        else PS抓盘手臂取Empty空盘流程.StepID = 545; //上顶一个Tray盘高度
                    //    }
                    //    else
                    //    {
                    //        APPmaster.ProgramPause = true;
                    //        IsPause = true;
                    //        Debug.WriteLine($"判断Empty顶盘轴是否走到最大值失败！" + result);
                    //    }
                    //    break;

                    //case 545:
                    //    PS抓盘手臂取Empty空盘流程.StepName = "Empty盘轴上顶一个Tray盘";
                    //    Debug.WriteLine($"PS抓盘手臂取Empty空盘流程.StepID={PS抓盘手臂取Empty空盘流程.StepID}->{PS抓盘手臂取Empty空盘流程.StepName}");
                    //    result = Empty顶盘轴相对移动(6.35);
                    //    if (result == "Success")
                    //    {
                    //        PS抓盘手臂取Empty空盘流程.StepID = 550; //后续需要检测传感器是否灭了
                    //    }
                    //    else
                    //    {
                    //        APPmaster.ProgramPause = true;
                    //        IsPause = true;
                    //        Debug.WriteLine($"Empty盘轴上顶一个Tray盘失败！" + result);
                    //    }
                    //    break;

                    //case 550:
                    //    PS抓盘手臂取Empty空盘流程.StepName = "检测Empty是否有空盘";
                    //    Debug.WriteLine($"PS抓盘手臂取Empty空盘流程.StepID={PS抓盘手臂取Empty空盘流程.StepID}->{PS抓盘手臂取Empty空盘流程.StepName}");
                    //    result = Empty_前Tray存在感应(out value);
                    //    if (result == "Success")
                    //    {
                    //        if (value == "0") //返回0是挡到感应器,1是没有挡到感应器
                    //        {
                    //            PS抓盘手臂取Empty空盘流程.StepID = 599;
                    //        }
                    //        else PS抓盘手臂取Empty空盘流程.StepID = 565; //上顶一个Tray盘高度后检测到没有空盘，操作员可能拿走了空盘或者其他原因，那是执行上顶呢还是一个一个上顶呢？ 先下降一个盘高度再执行上顶
                    //    }
                    //    else
                    //    {
                    //        APPmaster.ProgramPause = true;
                    //        IsPause = true;
                    //        Debug.WriteLine($"检测Empty是否有空盘失败！" + result);
                    //    }
                    //    break;

                    //case 565:
                    //    PS抓盘手臂取Empty空盘流程.StepName = "Empty盘轴下降一个Tray盘";
                    //    Debug.WriteLine($"PS抓盘手臂取Empty空盘流程.StepID={PS抓盘手臂取Empty空盘流程.StepID}->{PS抓盘手臂取Empty空盘流程.StepName}");
                    //    result = Empty顶盘轴相对移动(-6.35);
                    //    if (result == "Success")
                    //    {
                    //        Empty上顶标记 = 2;
                    //        PS抓盘手臂取Empty空盘流程.StepID = 705;
                    //    }
                    //    else
                    //    {
                    //        APPmaster.ProgramPause = true;
                    //        IsPause = true;
                    //        Debug.WriteLine($"Empty盘轴下降一个Tray盘失败！" + result);
                    //    }
                    //    break;

                    case 599:
                        PS抓盘手臂取Empty空盘流程.StepName = $"抓盘手臂取Empty空盘流程完成";
                        Debug.WriteLine($"PS待测盘上顶流程.StepID={PS抓盘手臂取Empty空盘流程.StepID}->{PS抓盘手臂取Empty空盘流程.StepName}");
                        PS抓盘手臂取Empty空盘流程.Processing = false;
                        PS抓盘手臂取Empty空盘流程.StepID = 500;
                        break;

                    //case 700:
                    //    PS抓盘手臂取Empty空盘流程.StepName = "Empty顶盘轴回零";
                    //    Debug.WriteLine($"PS抓盘手臂取Empty空盘流程.StepID={PS抓盘手臂取Empty空盘流程.StepID}->{PS抓盘手臂取Empty空盘流程.StepName}");
                    //    result = Empty顶盘轴回零();
                    //    if (result == "Success")
                    //    {
                    //        Empty上顶标记 = 2;
                    //        PS抓盘手臂取Empty空盘流程.StepID = 705;
                    //    }
                    //    else
                    //    {
                    //        APPmaster.ProgramPause = true;
                    //        IsPause = true;
                    //        Debug.WriteLine($"Empty顶盘轴回零失败！" + result);
                    //    }
                    //    break;

                    //case 705:
                    //    if (Empty上顶标记 == 1)
                    //    {
                    //        PS抓盘手臂取Empty空盘流程.StepID = 599;
                    //    }
                    //    else Thread.Sleep(10);
                    //    break;

                    default:
                        Debug.WriteLine(("PS抓盘手臂取Empty空盘流程.StepID =" + PS抓盘手臂取Empty空盘流程.StepID.ToString() + "对应流程不存在"));
                        break;
                }
            }

            //移Tray上下气缸原位输入读取
        }

        public void 抓盘手臂取Load空盘()
        {
            string result = "";
            PS抓盘手臂取Load空盘流程.StepID = 500;
            while (true)
            {
                Thread.Sleep(10);
                //if (!是否可以开始运行())
                //{
                //    continue;
                //}

                if (APPmaster.IsPause)
                {
                    continue;
                }

                switch (PS抓盘手臂取Load空盘流程.StepID)
                {
                    case 500:
                        PS抓盘手臂取Load空盘流程.StepName = "路径规划";
                        PS抓盘手臂取Load空盘流程.Condition = 抓盘手臂取Load空盘标记 == 2 && Load上顶标记 == 1;
                        PS抓盘手臂取Load空盘流程.Permit = !PS抓盘手臂取Load空盘流程.Processing;
                        if (PS抓盘手臂取Load空盘流程.Condition && PS抓盘手臂取Load空盘流程.Permit)
                        {
                            Empty下降标记 = 2;
                            PS抓盘手臂取Load空盘流程.Processing = true;
                            PS抓盘手臂取Load空盘流程.StepID = 505;
                        }
                        else Thread.Sleep(10);
                        break;

                    case 505:
                        PS抓盘手臂取Load空盘流程.StepName = "移Tray上下气缸原位输入读取";
                        Debug.WriteLine($"PS抓盘手臂取Load空盘流程.StepID={PS抓盘手臂取Load空盘流程.StepID}->{PS抓盘手臂取Load空盘流程.StepName}");
                        result = 移Tray上下气缸原位输入读取(out string value);
                        if (result == "Success")
                        {
                            if (value == "1")
                            {
                                PS抓盘手臂取Load空盘流程.StepID = 510;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Debug.WriteLine($"移Tray上下气缸未在原位");
                            }
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"移Tray上下气缸原位输入读取失败！" + result);
                        }
                        break;

                    case 510:
                        PS抓盘手臂取Load空盘流程.StepName = "移Tray夹爪气缸原位输入读取";
                        Debug.WriteLine($"PS抓盘手臂取Load空盘流程.StepID={PS抓盘手臂取Load空盘流程.StepID}->{PS抓盘手臂取Load空盘流程.StepName}");
                        result = 移Tray夹爪气缸原位输入读取(out value);
                        if (result == "Success")
                        {
                            if (value == "1")
                            {
                                PS抓盘手臂取Load空盘流程.StepID = 515;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Debug.WriteLine($"移Tray夹爪气缸未在原位");
                            }
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"移Tray夹爪气缸原位输入读取失败！" + result);
                        }
                        break;

                    case 515:
                        PS抓盘手臂取Load空盘流程.StepName = "放Tary盘轴移动到Load";
                        Debug.WriteLine($"PS抓盘手臂取Load空盘流程.StepID={PS抓盘手臂取Load空盘流程.StepID}->{PS抓盘手臂取Load空盘流程.StepName}");
                        result = 放Tary盘轴移动(初始点位.抓盘_Load_X);
                        if (result == "Success")
                        {
                            PS抓盘手臂取Load空盘流程.StepID = 520;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"放Tary盘轴移动到Load失败！" + result);
                        }
                        break;

                    case 520:
                        PS抓盘手臂取Load空盘流程.StepName = "Load_Tray前侧定位气缸原位输出";
                        Debug.WriteLine($"PS抓盘手臂取Load空盘流程.StepID={PS抓盘手臂取Load空盘流程.StepID}->{PS抓盘手臂取Load空盘流程.StepName}");
                        result = Load_Tray前侧定位气缸原位输出();
                        if (result == "Success")
                        {
                            PS抓盘手臂取Load空盘流程.StepID = 521;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Load_Tray前侧定位气缸原位输出失败！" + result);
                        }
                        break;

                    case 521:
                        PS抓盘手臂取Load空盘流程.StepName = "移Tray上下气缸到位输出";
                        Debug.WriteLine($"PS抓盘手臂取Load空盘流程.StepID={PS抓盘手臂取Load空盘流程.StepID}->{PS抓盘手臂取Load空盘流程.StepName}");
                        result = 移Tray上下气缸到位输出();
                        if (result == "Success")
                        {
                            PS抓盘手臂取Load空盘流程.StepID = 525;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"移Tray上下气缸到位输出失败！" + result);
                        }
                        break;

                    case 525:
                        PS抓盘手臂取Load空盘流程.StepName = "移Tray夹爪气缸到位输出";
                        Debug.WriteLine($"PS抓盘手臂取Load空盘流程.StepID={PS抓盘手臂取Load空盘流程.StepID}->{PS抓盘手臂取Load空盘流程.StepName}");
                        result = 移Tray夹爪气缸到位输出();
                        if (result == "Success")
                        {
                            PS抓盘手臂取Load空盘流程.StepID = 530;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"移Tray夹爪气缸到位输出失败！" + result);
                        }
                        break;

                    case 530:
                        PS抓盘手臂取Load空盘流程.StepName = "移Tray上下气缸原位输出";
                        Debug.WriteLine($"PS抓盘手臂取Load空盘流程.StepID={PS抓盘手臂取Load空盘流程.StepID}->{PS抓盘手臂取Load空盘流程.StepName}");
                        result = 移Tray上下气缸原位输出();
                        if (result == "Success")
                        {
                            PS抓盘手臂取Load空盘流程.StepID = 535;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"移Tray上下气缸原位输出失败！" + result);
                        }
                        break;

                    case 535:
                        PS抓盘手臂取Load空盘流程.StepName = "检测Load是否无空盘";
                        result = Load_前Tray存在感应(out value);
                        if (result == "Success")
                        {
                            if (value == "1") //返回0是挡到感应器,1是没有挡到感应器
                            {
                                Load上顶标记 = 2;
                                PS抓盘手臂取Load空盘流程.StepID = 540;
                            }
                            else //挡到感应器说明没有抓盘成功，需重新抓盘
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                PS抓盘手臂取Load空盘流程.StepID = 536; //松开夹爪重新再抓盘
                                Debug.WriteLine($"抓盘手臂抓盘失败！" + result);
                            }
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"检测Empty是否无空盘失败！" + result);
                        }
                        break;

                    case 536:
                        PS抓盘手臂取Load空盘流程.StepName = "移Tray夹爪气缸原位输出";
                        Debug.WriteLine($"PS抓盘手臂取Load空盘流程.StepID={PS抓盘手臂取Load空盘流程.StepID}->{PS抓盘手臂取Load空盘流程.StepName}");
                        result = 移Tray夹爪气缸原位输出();
                        if (result == "Success")
                        {
                            PS抓盘手臂取Load空盘流程.StepID = 515;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"移Tray夹爪气缸原位输出失败！" + result);
                        }
                        break;

                    case 540:
                        PS抓盘手臂取Load空盘流程.StepName = "放Tary盘轴移动到Empty";
                        Debug.WriteLine($"PS抓盘手臂取Load空盘流程.StepID={PS抓盘手臂取Load空盘流程.StepID}->{PS抓盘手臂取Load空盘流程.StepName}");
                        result = 放Tary盘轴移动(初始点位.抓盘_Empty_X);
                        if (result == "Success")
                        {
                            抓盘手臂取Load空盘标记 = 1;
                            抓盘手臂放空盘到Empty标记 = 2;
                            PS抓盘手臂取Load空盘流程.StepID = 599;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"放Tary盘轴移动到Empty失败！" + result);
                        }
                        break;

                    //case 540:
                    //    PS抓盘手臂取Load空盘流程.StepName = "判断Load顶盘轴是否走到最大值";
                    //    Debug.WriteLine($"PS抓盘手臂取Load空盘流程.StepID={PS抓盘手臂取Load空盘流程.StepID}->{PS抓盘手臂取Load空盘流程.StepName}");
                    //    result = motionEx.GetCmdEncPos("M13_Loader顶盘轴", out double prfpos);
                    //    if (result == "Success")
                    //    {
                    //        if (prfpos + 12700 > 待测盘上顶最大值)
                    //        {
                    //            Debug.WriteLine($"M13_Loader顶盘轴当前上升行程{prfpos + 12700}>{待测盘上顶最大值}");
                    //            PS抓盘手臂取Load空盘流程.StepID = 700; //执行回零再触发上顶
                    //        }
                    //        else PS抓盘手臂取Load空盘流程.StepID = 545; //上顶一个Tray盘高度
                    //    }
                    //    else
                    //    {
                    //        APPmaster.ProgramPause = true;
                    //        IsPause = true;
                    //        Debug.WriteLine($"判断Load顶盘轴是否走到最大值失败！" + result);
                    //    }
                    //    break;

                    //case 545:
                    //    PS抓盘手臂取Load空盘流程.StepName = "Load盘轴上顶一个Tray盘";
                    //    Debug.WriteLine($"PS抓盘手臂取Load空盘流程.StepID={PS抓盘手臂取Load空盘流程.StepID}->{PS抓盘手臂取Load空盘流程.StepName}");
                    //    result = Load顶盘轴相对移动(6.35);
                    //    if (result == "Success")
                    //    {
                    //        PS抓盘手臂取Load空盘流程.StepID = 550; //后续需要检测传感器是否灭了
                    //    }
                    //    else
                    //    {
                    //        APPmaster.ProgramPause = true;
                    //        IsPause = true;
                    //        Debug.WriteLine($"Load盘轴上顶一个Tray盘失败！" + result);
                    //    }
                    //    break;

                    //case 550:
                    //    PS抓盘手臂取Load空盘流程.StepName = "检测Load是否有空盘";
                    //    Debug.WriteLine($"PS抓盘手臂取Load空盘流程.StepID={PS抓盘手臂取Load空盘流程.StepID}->{PS抓盘手臂取Load空盘流程.StepName}");
                    //    result = Load_前Tray存在感应(out value);
                    //    if (result == "Success")
                    //    {
                    //        if (value == "0") //返回0是挡到感应器,1是没有挡到感应器
                    //        {
                    //            PS抓盘手臂取Load空盘流程.StepID = 555;
                    //        }
                    //        else PS抓盘手臂取Load空盘流程.StepID = 565; //上顶一个Tray盘高度后检测到没有空盘，操作员可能拿走了空盘或者其他原因，那是执行上顶呢还是一个一个上顶呢？ 先下降一个盘高度再执行上顶
                    //    }
                    //    else
                    //    {
                    //        APPmaster.ProgramPause = true;
                    //        IsPause = true;
                    //        Debug.WriteLine($"检测Load是否有空盘失败！" + result);
                    //    }
                    //    break;

                    //case 555:
                    //    PS抓盘手臂取Load空盘流程.StepName = "Load_Tray前侧定位气缸到位输出";
                    //    Debug.WriteLine($"PS抓盘手臂取Load空盘流程.StepID={PS抓盘手臂取Load空盘流程.StepID}->{PS抓盘手臂取Load空盘流程.StepName}");
                    //    result = Load_Tray前侧定位气缸到位输出();
                    //    if (result == "Success")
                    //    {
                    //        PS抓盘手臂取Load空盘流程.StepID = 599;
                    //    }
                    //    else
                    //    {
                    //        APPmaster.ProgramPause = true;
                    //        IsPause = true;
                    //        Debug.WriteLine($"Load_Tray前侧定位气缸到位输出失败！" + result);
                    //    }
                    //    break;

                    //case 565:
                    //    PS抓盘手臂取Load空盘流程.StepName = "Load盘轴下降一个Tray盘";
                    //    Debug.WriteLine($"PS抓盘手臂取Load空盘流程.StepID={PS抓盘手臂取Load空盘流程.StepID}->{PS抓盘手臂取Load空盘流程.StepName}");
                    //    result = Load顶盘轴相对移动(-6.35);
                    //    if (result == "Success")
                    //    {
                    //        Load上顶标记 = 2;
                    //        PS抓盘手臂取Load空盘流程.StepID = 705;
                    //    }
                    //    else
                    //    {
                    //        APPmaster.ProgramPause = true;
                    //        IsPause = true;
                    //        Debug.WriteLine($"Load盘轴下降一个Tray盘失败！" + result);
                    //    }
                    //    break;

                    case 599:
                        PS抓盘手臂取Load空盘流程.StepName = $"抓盘手臂取Load空盘流程完成";
                        Debug.WriteLine($"PS抓盘手臂取Load空盘流程.StepID={PS抓盘手臂取Load空盘流程.StepID}->{PS抓盘手臂取Load空盘流程.StepName}");
                        PS抓盘手臂取Load空盘流程.Processing = false;
                        PS抓盘手臂取Load空盘流程.StepID = 500;
                        break;

                    //case 700:
                    //    PS抓盘手臂取Load空盘流程.StepName = "Load顶盘轴回零";
                    //    Debug.WriteLine($"PS抓盘手臂取Load空盘流程.StepID={PS抓盘手臂取Load空盘流程.StepID}->{PS抓盘手臂取Load空盘流程.StepName}");
                    //    result = Load顶盘轴回零();
                    //    if (result == "Success")
                    //    {
                    //        Load上顶标记 = 2;
                    //        PS抓盘手臂取Load空盘流程.StepID = 705;
                    //    }
                    //    else
                    //    {
                    //        APPmaster.ProgramPause = true;
                    //        IsPause = true;
                    //        Debug.WriteLine($"Load顶盘轴回零失败！" + result);
                    //    }
                    //    break;

                    //case 705:
                    //    if (Load上顶标记 == 1)
                    //    {
                    //        PS抓盘手臂取Load空盘流程.StepID = 599;
                    //    }
                    //    else Thread.Sleep(10);
                    //    break;

                    default:
                        Debug.WriteLine(("PS待测盘上顶流程.StepID =" + PS抓盘手臂取Load空盘流程.StepID.ToString() + "对应流程不存在"));
                        break;
                }
            }
        }

        public void 抓盘手臂放空盘到Empty()
        {
            string result = "";
            string result2 = "";
            PS抓盘手臂放空盘到Empty流程.StepID = 500;
            while (true)
            {
                //if (!是否可以开始运行())
                //{
                //    continue;
                //}
                Thread.Sleep(10);
                if (APPmaster.IsPause)
                {
                    continue;
                }

                switch (PS抓盘手臂放空盘到Empty流程.StepID)
                {
                    case 500:
                        PS抓盘手臂放空盘到Empty流程.StepName = "路径规划";
                        PS抓盘手臂放空盘到Empty流程.Condition = 抓盘手臂放空盘到Empty标记 == 2;
                        PS抓盘手臂放空盘到Empty流程.Permit = true;
                        if (PS抓盘手臂放空盘到Empty流程.Condition && PS抓盘手臂放空盘到Empty流程.Permit)
                        {
                            PS抓盘手臂放空盘到Empty流程.Processing = true;
                            PS抓盘手臂放空盘到Empty流程.StepID = 505;
                        }
                        break;

                    case 505:
                        PS抓盘手臂放空盘到Empty流程.StepName = "移Tray上下气缸原位输入读取";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Empty流程.StepID={PS抓盘手臂放空盘到Empty流程.StepID}->{PS抓盘手臂放空盘到Empty流程.StepName}");
                        result = 移Tray上下气缸原位输入读取(out string value);
                        if (result == "Success")
                        {
                            if (value == "1")
                            {
                                PS抓盘手臂放空盘到Empty流程.StepID = 510;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Debug.WriteLine($"移Tray上下气缸未在原位");
                            }
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"移Tray上下气缸原位输入读取失败！" + result);
                        }
                        break;

                    case 510:
                        PS抓盘手臂放空盘到Empty流程.StepName = "移Tray夹爪气缸到位输入读取";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Empty流程.StepID={PS抓盘手臂放空盘到Empty流程.StepID}->{PS抓盘手臂放空盘到Empty流程.StepName}");
                        result = 移Tray夹爪气缸到位输入读取(out value);
                        if (result == "Success")
                        {
                            if (value == "1")
                            {
                                PS抓盘手臂放空盘到Empty流程.StepID = 515;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Debug.WriteLine($"移Tray夹爪气缸未夹紧");
                            }
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"移Tray夹爪气缸原位输入读取失败！" + result);
                        }
                        break;

                    case 515:
                        PS抓盘手臂放空盘到Empty流程.StepName = "放Tary盘轴移动到Empty";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Empty流程.StepID={PS抓盘手臂放空盘到Empty流程.StepID}->{PS抓盘手臂放空盘到Empty流程.StepName}");
                        result = 放Tary盘轴移动(初始点位.抓盘_Empty_X);
                        if (result == "Success")
                        {
                            PS抓盘手臂放空盘到Empty流程.StepID = 516;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"放Tary盘轴移动到Empty失败！" + result);
                        }
                        break;

                    case 516:
                        PS抓盘手臂放空盘到Empty流程.StepName = "判断Empty下降是否完成";
                        if (Empty下降标记 == 1)
                        {
                            PS抓盘手臂放空盘到Empty流程.StepID = 520;
                        }
                        else
                        {
                            Thread.Sleep(10);
                        }
                        break;

                    case 520:
                        PS抓盘手臂放空盘到Empty流程.StepName = "移Tray上下气缸到位输出";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Empty流程.StepID={PS抓盘手臂放空盘到Empty流程.StepID}->{PS抓盘手臂放空盘到Empty流程.StepName}");
                        result = 移Tray上下气缸到位输出();
                        if (result == "Success")
                        {
                            PS抓盘手臂放空盘到Empty流程.StepID = 525;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"移Tray上下气缸到位输出失败！" + result);
                        }
                        break;

                    case 525:
                        PS抓盘手臂放空盘到Empty流程.StepName = "移Tray夹爪气缸原位输出";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Empty流程.StepID={PS抓盘手臂放空盘到Empty流程.StepID}->{PS抓盘手臂放空盘到Empty流程.StepName}");
                        result = 移Tray夹爪气缸原位输出();
                        if (result == "Success")
                        {
                            PS抓盘手臂放空盘到Empty流程.StepID = 530;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"移Tray夹爪气缸原位输出失败！" + result);
                        }
                        break;

                    case 530:
                        PS抓盘手臂放空盘到Empty流程.StepName = "移Tray上下气缸原位输出";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Empty流程.StepID={PS抓盘手臂放空盘到Empty流程.StepID}->{PS抓盘手臂放空盘到Empty流程.StepName}");
                        result = 移Tray上下气缸原位输出();
                        if (result == "Success")
                        {
                            PS抓盘手臂放空盘到Empty流程.StepID = 535;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"移Tray上下气缸原位输出失败！" + result);
                        }
                        break;

                    case 535:
                        PS抓盘手臂放空盘到Empty流程.StepName = "检测Empty是否有空盘";
                        result = Empty_前Tray存在感应(out value);
                        result2 = Empty_后Tray放歪感应(out string value2);
                        if (result == "Success" && result2 == "Success")
                        {
                            if (value == "0" && value2 == "1") //返回0是挡到感应器,1是没有挡到感应器
                            {
                                PS抓盘手臂放空盘到Empty流程.StepID = 540;
                            }
                            else //没有挡到感应器或者放歪感应器被挡到说明没有放盘成功，需重新放盘
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;

                                if (value != "0")
                                {
                                    Debug.WriteLine($"抓盘手臂放盘失败！Empty没有感应到Tray盘,value:" + value);
                                }
                                if (value2 != "1")
                                {
                                    Debug.WriteLine($"抓盘手臂放盘失败！Empty感应到Tray盘放歪,value:" + value2);
                                }
                            }
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            if (result != "Success") Debug.WriteLine($"读取Empty前Tray存在感应失败！" + result);
                            if (result2 != "Success") Debug.WriteLine($"读取Empty后Tray放歪感应失败！" + result2);
                        }
                        break;

                    case 540:
                        PS抓盘手臂放空盘到Empty流程.StepName = "放Tary盘轴回到原点";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Empty流程.StepID={PS抓盘手臂放空盘到Empty流程.StepID}->{PS抓盘手臂放空盘到Empty流程.StepName}");
                        result = 放Tary盘轴移动(0);
                        if (result == "Success")
                        {
                            抓盘手臂放空盘到Empty标记 = 1;
                            PS抓盘手臂放空盘到Empty流程.StepID = 599;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"放Tary盘轴回到原点失败！" + result);
                        }
                        break;

                    case 599:
                        PS抓盘手臂放空盘到Empty流程.StepName = $"抓盘手臂放空盘到Empty流程完成";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Empty流程.StepID={PS抓盘手臂放空盘到Empty流程.StepID}->{PS抓盘手臂放空盘到Empty流程.StepName}");
                        PS抓盘手臂放空盘到Empty流程.Processing = false;
                        PS抓盘手臂放空盘到Empty流程.StepID = 500;
                        break;

                    default:
                        Debug.WriteLine(("PS抓盘手臂放空盘到Empty流程.StepID =" + PS抓盘手臂放空盘到Empty流程.StepID.ToString() + "对应流程不存在"));
                        break;
                }
            }
        }

        public void 抓盘手臂放空盘到Auto1()
        {
            string result = "";
            string result2 = "";
            PS抓盘手臂放空盘到Auto1流程.StepID = 500;
            while (true)
            {
                //if (!是否可以开始运行())
                //{
                //    continue;
                //}

                Thread.Sleep(10);

                if (APPmaster.IsPause)
                {
                    continue;
                }

                switch (PS抓盘手臂放空盘到Auto1流程.StepID)
                {
                    case 500:
                        PS抓盘手臂放空盘到Auto1流程.StepName = "路径规划";
                        PS抓盘手臂放空盘到Auto1流程.Condition = 抓盘手臂放空盘到Auto1标记 == 2;
                        PS抓盘手臂放空盘到Auto1流程.Permit = true;
                        if (PS抓盘手臂放空盘到Auto1流程.Condition && PS抓盘手臂放空盘到Auto1流程.Permit)
                        {
                            抓盘手臂取Empty空盘标记 = 2;
                            PS抓盘手臂放空盘到Auto1流程.Processing = true;
                            PS抓盘手臂放空盘到Auto1流程.StepID = 501;
                        }
                        break;

                    case 501:
                        PS抓盘手臂放空盘到Auto1流程.StepName = "等待抓盘手臂取Empty空盘";
                        if (抓盘手臂取Empty空盘标记 == 1 && Auto等待换盘标记)
                        {
                            PS抓盘手臂放空盘到Auto1流程.StepID = 505;
                        }
                        else
                        {
                            Thread.Sleep(10);
                        }
                        break;

                    case 505:
                        PS抓盘手臂放空盘到Auto1流程.StepName = "移Tray上下气缸原位输入读取";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
                        result = 移Tray上下气缸原位输入读取(out string value);
                        if (result == "Success")
                        {
                            if (value == "1")
                            {
                                PS抓盘手臂放空盘到Auto1流程.StepID = 510;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Debug.WriteLine($"移Tray上下气缸未在原位");
                            }
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"移Tray上下气缸原位输入读取失败！" + result);
                        }
                        break;

                    case 510:
                        PS抓盘手臂放空盘到Auto1流程.StepName = "移Tray夹爪气缸到位输入读取";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
                        result = 移Tray夹爪气缸到位输入读取(out value);
                        if (result == "Success")
                        {
                            if (value == "1")
                            {
                                PS抓盘手臂放空盘到Auto1流程.StepID = 511;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Debug.WriteLine($"移Tray夹爪气缸未夹紧");
                            }
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"移Tray夹爪气缸原位输入读取失败！" + result);
                        }
                        break;

                    case 511:
                        if (Auto1上顶标记 == 1)
                        {
                            PS抓盘手臂放空盘到Auto1流程.StepID = 515;
                        }
                        else
                        {
                            PS抓盘手臂放空盘到Auto1流程.StepID = 512;
                        }
                        break;

                    case 512:
                        PS抓盘手臂放空盘到Auto1流程.StepName = "放Tary盘轴移动到等待位";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
                        result = 放Tary盘轴移动(初始点位.抓盘_Auto1_X - 200);//656.78
                        if (result == "Success")
                        {
                            PS抓盘手臂放空盘到Auto1流程.StepID = 513;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"放Tary盘轴移动到等待位失败！" + result);
                        }
                        break;

                    case 513:
                        if (Auto1上顶标记 == 1)
                        {
                            PS抓盘手臂放空盘到Auto1流程.StepID = 515;
                        }
                        else
                        {
                            Thread.Sleep(10);
                        }
                        break;

                    case 515:
                        PS抓盘手臂放空盘到Auto1流程.StepName = "放Tary盘轴移动到Auto1";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
                        result = 放Tary盘轴移动(初始点位.抓盘_Auto1_X);
                        if (result == "Success")
                        {
                            PS抓盘手臂放空盘到Auto1流程.StepID = 520;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"放Tary盘轴移动到Auto1失败！" + result);
                        }
                        break;

                    case 520:
                        PS抓盘手臂放空盘到Auto1流程.StepName = "移Tray上下气缸到位输出";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
                        result = 移Tray上下气缸到位输出();
                        if (result == "Success")
                        {
                            PS抓盘手臂放空盘到Auto1流程.StepID = 525;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"移Tray上下气缸到位输出失败！" + result);
                        }
                        break;

                    case 525:
                        PS抓盘手臂放空盘到Auto1流程.StepName = "移Tray夹爪气缸原位输出";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
                        result = 移Tray夹爪气缸原位输出();
                        if (result == "Success")
                        {
                            PS抓盘手臂放空盘到Auto1流程.StepID = 530;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"移Tray夹爪气缸原位输出失败！" + result);
                        }
                        break;

                    case 530:
                        PS抓盘手臂放空盘到Auto1流程.StepName = "移Tray上下气缸原位输出";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
                        result = 移Tray上下气缸原位输出();
                        if (result == "Success")
                        {
                            PS抓盘手臂放空盘到Auto1流程.StepID = 535;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"移Tray上下气缸原位输出失败！" + result);
                        }
                        break;

                    case 535:
                        PS抓盘手臂放空盘到Auto1流程.StepName = "检测Auto1是否有空盘";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
                        result = Auto1_前Tray存在感应(out value);
                        result2 = Auto1_后Tray放歪感应(out string value2);
                        if (result == "Success" && result2 == "Success")
                        {
                            if (value == "0" && value2 == "1") //返回0是挡到感应器,1是没有挡到感应器
                            {
                                PS抓盘手臂放空盘到Auto1流程.StepID = 540;
                            }
                            else //没有挡到感应器或者放歪感应器被挡到说明没有放盘成功，需重新放盘
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;

                                if (value != "0")
                                {
                                    Debug.WriteLine($"抓盘手臂放盘失败！Auto1没有感应到Tray盘,value:" + value);
                                }
                                if (value2 != "1")
                                {
                                    Debug.WriteLine($"抓盘手臂放盘失败！Auto1感应到Tray盘放歪,value:" + value2);
                                }
                            }
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            if (result != "Success") Debug.WriteLine($"读取Auto1前Tray存在感应失败！" + result);
                            if (result2 != "Success") Debug.WriteLine($"读取Auto1后Tray放歪感应失败！" + result2);
                        }
                        break;

                    case 540:
                        PS抓盘手臂放空盘到Auto1流程.StepName = $"Auto1_Tray前侧定位气缸到位输出";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
                        result = Auto1_Tray前侧定位气缸到位输出();
                        if (result == "Success")
                        {
                            PS抓盘手臂放空盘到Auto1流程.StepID = 545;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Auto1_Tray前侧定位气缸到位输出失败！" + result);
                        }
                        break;

                    case 545:
                        PS抓盘手臂放空盘到Auto1流程.StepName = "放Tary盘轴回到原点";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
                        result = 放Tary盘轴移动(0);
                        if (result == "Success")
                        {
                            PS抓盘手臂放空盘到Auto1流程.StepID = 550;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"放Tary盘轴回到原点失败！" + result);
                        }
                        break;

                    case 550:
                        PS抓盘手臂放空盘到Auto1流程.StepName = "判断Auto1料仓是否满盘";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
                        if (Auto1料仓满盘标记)
                        {
                            Auto1上顶标记 = 2;
                            PS抓盘手臂放空盘到Auto1流程.StepID = 551;
                        }
                        else PS抓盘手臂放空盘到Auto1流程.StepID = 599;
                        break;


                    case 551:
                        PS抓盘手臂放空盘到Auto1流程.StepName = "等待Auto1上顶盖空盘";
                        if (Auto1上顶标记 == 1)
                        {
                            Auto1料仓打开标记 = true;
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Auto1料仓满料！");
                            PS抓盘手臂放空盘到Auto1流程.StepID = 555;
                        }
                        else Thread.Sleep(10);
                        break;

                    case 555:
                        PS抓盘手臂放空盘到Auto1流程.StepName = "等待料仓料拿走";
                        if (Auto1料仓打开标记 == false) //料仓被打开
                        {
                            抓盘手臂取Empty空盘标记 = 2;
                            PS抓盘手臂放空盘到Auto1流程.StepID = 560;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Auto1料仓满料");
                        }
                        break;

                    case 560:
                        PS抓盘手臂放空盘到Auto1流程.StepName = "等待抓盘手臂取Empty空盘";
                        if (抓盘手臂取Empty空盘标记 == 1 && Auto等待换盘标记)
                        {
                            PS抓盘手臂放空盘到Auto1流程.StepID = 565;
                        }
                        Thread.Sleep(10);
                        break;

                    case 565:
                        PS抓盘手臂放空盘到Auto1流程.StepName = "放Tary盘轴和Auto1顶盘轴移动到等待位";
                        Task<string> trayx = Task.Run(() => { return 放Tary盘轴移动(初始点位.抓盘_Auto1_X - 200); });
                        Task<string> auto1u = Task.Run(() => { return Auto1顶盘轴移动(182.715); });
                        if (trayx.Result == "Success" && auto1u.Result == "Success")
                        {
                            PS抓盘手臂放空盘到Auto1流程.StepID = 570;
                        }
                        else
                        {
                            if (trayx.Result != "Success")
                            {
                                Debug.WriteLine($"放Tary盘轴移动到等待位失败！" + trayx.Result);
                            }
                            if (auto1u.Result != "Success")
                            {
                                Debug.WriteLine($"Auto1顶盘轴移动到等待位失败！" + trayx.Result);
                            }
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 570:
                        PS抓盘手臂放空盘到Auto1流程.StepName = "检测Auto1是否无空盘";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
                        result = Auto1_前Tray存在感应(out value);
                        result2 = Auto1_后Tray放歪感应(out value2);
                        if (result == "Success" && result2 == "Success")
                        {
                            if (value == "1" && value2 == "1") //返回0是挡到感应器,1是没有挡到感应器
                            {
                                PS抓盘手臂放空盘到Auto1流程.StepID = 575;
                            }
                            else //没有挡到感应器或者放歪感应器被挡到说明没有放盘成功，需重新放盘
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;

                                if (value != "1")
                                {
                                    Debug.WriteLine($"抓盘手臂放盘失败！Auto1有盘感应到Tray盘,value:" + value);
                                }
                                if (value2 != "1")
                                {
                                    Debug.WriteLine($"抓盘手臂放盘失败！Auto1放歪感应到Tray盘,value:" + value2);
                                }
                            }
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            if (result != "Success") Debug.WriteLine($"读取Auto1前Tray存在感应失败！" + result);
                            if (result2 != "Success") Debug.WriteLine($"读取Auto1后Tray放歪感应失败！" + result2);
                        }
                        break;


                    case 575:
                        PS抓盘手臂放空盘到Auto1流程.StepName = "放Tary盘轴移动到Auto1";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
                        result = 放Tary盘轴移动(初始点位.抓盘_Auto1_X);
                        if (result == "Success")
                        {
                            PS抓盘手臂放空盘到Auto1流程.StepID = 580;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"放Tary盘轴移动到Auto1失败！" + result);
                        }
                        break;

                    case 580:
                        PS抓盘手臂放空盘到Auto1流程.StepName = "移Tray上下气缸到位输出";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
                        result = 移Tray上下气缸到位输出();
                        if (result == "Success")
                        {
                            PS抓盘手臂放空盘到Auto1流程.StepID = 585;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"移Tray上下气缸到位输出失败！" + result);
                        }
                        break;

                    case 585:
                        PS抓盘手臂放空盘到Auto1流程.StepName = "移Tray夹爪气缸原位输出";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
                        result = 移Tray夹爪气缸原位输出();
                        if (result == "Success")
                        {
                            PS抓盘手臂放空盘到Auto1流程.StepID = 590;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"移Tray夹爪气缸原位输出失败！" + result);
                        }
                        break;

                    case 590:
                        PS抓盘手臂放空盘到Auto1流程.StepName = "移Tray上下气缸原位输出";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
                        result = 移Tray上下气缸原位输出();
                        if (result == "Success")
                        {
                            PS抓盘手臂放空盘到Auto1流程.StepID = 591;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"移Tray上下气缸原位输出失败！" + result);
                        }
                        break;

                    case 591:
                        PS抓盘手臂放空盘到Auto1流程.StepName = "检测Auto1是否有空盘";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
                        result = Auto1_前Tray存在感应(out value);
                        result2 = Auto1_后Tray放歪感应(out value2);
                        if (result == "Success" && result2 == "Success")
                        {
                            if (value == "0" && value2 == "1") //返回0是挡到感应器,1是没有挡到感应器
                            {
                                PS抓盘手臂放空盘到Auto1流程.StepID = 592;
                            }
                            else //没有挡到感应器或者放歪感应器被挡到说明没有放盘成功，需重新放盘
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;

                                if (value != "0")
                                {
                                    Debug.WriteLine($"抓盘手臂放盘失败！Auto1没有感应到Tray盘,value:" + value);
                                }
                                if (value2 != "1")
                                {
                                    Debug.WriteLine($"抓盘手臂放盘失败！Auto1感应到Tray盘放歪,value:" + value2);
                                }
                            }
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            if (result != "Success") Debug.WriteLine($"读取Auto1前Tray存在感应失败！" + result);
                            if (result2 != "Success") Debug.WriteLine($"读取Auto1后Tray放歪感应失败！" + result2);
                        }
                        break;

                    case 592:
                        PS抓盘手臂放空盘到Auto1流程.StepName = $"Auto1_Tray前侧定位气缸到位输出";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
                        result = Auto1_Tray前侧定位气缸到位输出();
                        if (result == "Success")
                        {
                            PS抓盘手臂放空盘到Auto1流程.StepID = 595;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Auto1_Tray前侧定位气缸到位输出失败！" + result);
                        }
                        break;

                    case 595:
                        PS抓盘手臂放空盘到Auto1流程.StepName = "放Tary盘轴回到原点";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
                        result = 放Tary盘轴移动(0);
                        if (result == "Success")
                        {
                            PS抓盘手臂放空盘到Auto1流程.StepID = 599;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"放Tary盘轴回到原点失败！" + result);
                        }
                        break;

                    case 599:
                        抓盘手臂放空盘到Auto1标记 = 1;
                        PS抓盘手臂放空盘到Auto1流程.StepName = $"抓盘手臂放空盘到Auto1流程完成";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
                        PS抓盘手臂放空盘到Auto1流程.Processing = false;
                        PS抓盘手臂放空盘到Auto1流程.StepID = 500;
                        break;

                    default:
                        Debug.WriteLine(("PS抓盘手臂放空盘到Auto1流程.StepID =" + PS抓盘手臂放空盘到Auto1流程.StepID.ToString() + "对应流程不存在"));
                        break;
                }

            }
        }

        public void 抓盘手臂放空盘到Auto2()
        {
            string result = "";
            string result2 = "";
            PS抓盘手臂放空盘到Auto2流程.StepID = 500;
            while (true)
            {
                Thread.Sleep(10);
                //if (!是否可以开始运行())
                //{
                //    continue;
                //}
                if (APPmaster.IsPause)
                {
                    continue;
                }
                switch (PS抓盘手臂放空盘到Auto2流程.StepID)
                {
                    case 500:
                        PS抓盘手臂放空盘到Auto2流程.StepName = "路径规划";
                        PS抓盘手臂放空盘到Auto2流程.Condition = 抓盘手臂放空盘到Auto2标记 == 2;
                        PS抓盘手臂放空盘到Auto2流程.Permit = true;
                        if (PS抓盘手臂放空盘到Auto2流程.Condition && PS抓盘手臂放空盘到Auto2流程.Permit)
                        {
                            抓盘手臂取Empty空盘标记 = 2;
                            PS抓盘手臂放空盘到Auto2流程.Processing = true;
                            PS抓盘手臂放空盘到Auto2流程.StepID = 501;
                        }
                        break;

                    case 501:
                        PS抓盘手臂放空盘到Auto1流程.StepName = "等待抓盘手臂取Empty空盘";
                        if (抓盘手臂取Empty空盘标记 == 1 && Auto等待换盘标记)
                        {
                            PS抓盘手臂放空盘到Auto2流程.StepID = 505;
                        }
                        else
                        {
                            Thread.Sleep(10);
                        }
                        break;

                    case 505:
                        PS抓盘手臂放空盘到Auto2流程.StepName = "移Tray上下气缸原位输入读取";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
                        result = 移Tray上下气缸原位输入读取(out string value);
                        if (result == "Success")
                        {
                            if (value == "1")
                            {
                                PS抓盘手臂放空盘到Auto2流程.StepID = 510;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Debug.WriteLine($"移Tray上下气缸未在原位");
                            }
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"移Tray上下气缸原位输入读取失败！" + result);
                        }
                        break;

                    case 510:
                        PS抓盘手臂放空盘到Auto2流程.StepName = "移Tray夹爪气缸到位输入读取";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
                        result = 移Tray夹爪气缸到位输入读取(out value);
                        if (result == "Success")
                        {
                            if (value == "1")
                            {
                                PS抓盘手臂放空盘到Auto2流程.StepID = 511;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Debug.WriteLine($"移Tray夹爪气缸未夹紧");
                            }
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"移Tray夹爪气缸原位输入读取失败！" + result);
                        }
                        break;

                    case 511:
                        if (Auto2上顶标记 == 1)
                        {
                            PS抓盘手臂放空盘到Auto2流程.StepID = 515;
                        }
                        else
                        {
                            PS抓盘手臂放空盘到Auto2流程.StepID = 512;
                        }
                        break;

                    case 512:
                        PS抓盘手臂放空盘到Auto2流程.StepName = "放Tary盘轴移动到等待位";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
                        result = 放Tary盘轴移动(初始点位.抓盘_Auto2_X - 200);//859.35
                        if (result == "Success")
                        {
                            PS抓盘手臂放空盘到Auto2流程.StepID = 513;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"放Tary盘轴移动到等待位失败！" + result);
                        }
                        break;

                    case 513:
                        if (Auto2上顶标记 == 1)
                        {
                            PS抓盘手臂放空盘到Auto2流程.StepID = 515;
                        }
                        else
                        {
                            Thread.Sleep(10);
                        }
                        break;

                    case 515:
                        PS抓盘手臂放空盘到Auto2流程.StepName = "放Tary盘轴移动到Auto2";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
                        result = 放Tary盘轴移动(初始点位.抓盘_Auto2_X);
                        if (result == "Success")
                        {
                            PS抓盘手臂放空盘到Auto2流程.StepID = 520;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"放Tary盘轴移动到Auto2失败！" + result);
                        }
                        break;

                    case 520:
                        PS抓盘手臂放空盘到Auto2流程.StepName = "移Tray上下气缸到位输出";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
                        result = 移Tray上下气缸到位输出();
                        if (result == "Success")
                        {
                            PS抓盘手臂放空盘到Auto2流程.StepID = 525;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"移Tray上下气缸到位输出失败！" + result);
                        }
                        break;

                    case 525:
                        PS抓盘手臂放空盘到Auto2流程.StepName = "移Tray夹爪气缸原位输出";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
                        result = 移Tray夹爪气缸原位输出();
                        if (result == "Success")
                        {
                            PS抓盘手臂放空盘到Auto2流程.StepID = 530;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"移Tray夹爪气缸原位输出失败！" + result);
                        }
                        break;

                    case 530:
                        PS抓盘手臂放空盘到Auto2流程.StepName = "移Tray上下气缸原位输出";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
                        result = 移Tray上下气缸原位输出();
                        if (result == "Success")
                        {
                            PS抓盘手臂放空盘到Auto2流程.StepID = 535;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"移Tray上下气缸原位输出失败！" + result);
                        }
                        break;

                    case 535:
                        PS抓盘手臂放空盘到Auto2流程.StepName = "检测Auto2是否有空盘";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
                        result = Auto2_前Tray存在感应(out value);
                        result2 = Auto2_后Tray放歪感应(out string value2);
                        if (result == "Success" && result2 == "Success")
                        {
                            if (value == "0" && value2 == "1") //返回0是挡到感应器,1是没有挡到感应器
                            {
                                PS抓盘手臂放空盘到Auto2流程.StepID = 540;
                            }
                            else //没有挡到感应器或者放歪感应器被挡到说明没有放盘成功，需重新放盘
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;

                                if (value != "0")
                                {
                                    Debug.WriteLine($"抓盘手臂放盘失败！Auto2没有感应到Tray盘,value:" + value);
                                }
                                if (value2 != "1")
                                {
                                    Debug.WriteLine($"抓盘手臂放盘失败！Auto2感应到Tray盘放歪,value:" + value2);
                                }
                            }
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            if (result != "Success") Debug.WriteLine($"读取Auto2前Tray存在感应失败！" + result);
                            if (result2 != "Success") Debug.WriteLine($"读取Auto2后Tray放歪感应失败！" + result2);
                        }
                        break;

                    case 540:
                        PS抓盘手臂放空盘到Auto2流程.StepName = $"Auto2_Tray前侧定位气缸到位输出";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
                        result = Auto2_Tray前侧定位气缸到位输出();
                        if (result == "Success")
                        {
                            PS抓盘手臂放空盘到Auto2流程.StepID = 545;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Auto2_Tray前侧定位气缸到位输出失败！" + result);
                        }
                        break;

                    case 545:
                        PS抓盘手臂放空盘到Auto2流程.StepName = "放Tary盘轴回到原点";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
                        result = 放Tary盘轴移动(0);
                        if (result == "Success")
                        {
                            PS抓盘手臂放空盘到Auto2流程.StepID = 550;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"放Tary盘轴回到原点失败！" + result);
                        }
                        break;

                    case 550:
                        PS抓盘手臂放空盘到Auto2流程.StepName = "判断Auto2料仓是否满盘";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
                        if (Auto2料仓满盘标记)
                        {
                            Auto2上顶标记 = 2;
                            PS抓盘手臂放空盘到Auto2流程.StepID = 551;
                        }
                        else PS抓盘手臂放空盘到Auto2流程.StepID = 599;
                        break;

                    case 551:
                        PS抓盘手臂放空盘到Auto2流程.StepName = "等待Auto2上顶盖空盘";
                        if (Auto2上顶标记 == 1)
                        {
                            Auto2料仓打开标记 = true;
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Auto2料仓满料！");
                            PS抓盘手臂放空盘到Auto2流程.StepID = 555;
                        }
                        else Thread.Sleep(10);
                        break;

                    case 555:
                        PS抓盘手臂放空盘到Auto2流程.StepName = "等待料仓料拿走";
                        if (Auto2料仓打开标记 == false) //料仓被打开
                        {
                            抓盘手臂取Empty空盘标记 = 2;
                            PS抓盘手臂放空盘到Auto2流程.StepID = 560;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Auto1料仓满料");
                        }
                        break;

                    case 560:
                        PS抓盘手臂放空盘到Auto2流程.StepName = "等待抓盘手臂取Empty空盘";
                        if (抓盘手臂取Empty空盘标记 == 1 && Auto等待换盘标记)
                        {
                            PS抓盘手臂放空盘到Auto2流程.StepID = 565;
                        }
                        Thread.Sleep(10);
                        break;

                    case 565:
                        PS抓盘手臂放空盘到Auto2流程.StepName = "放Tary盘轴和Auto2顶盘轴移动到等待位";
                        Task<string> trayx = Task.Run(() => { return 放Tary盘轴移动(初始点位.抓盘_Auto2_X - 200); });
                        Task<string> auto2u = Task.Run(() => { return Auto2顶盘轴移动(182.715); });
                        if (trayx.Result == "Success" && auto2u.Result == "Success")
                        {
                            PS抓盘手臂放空盘到Auto2流程.StepID = 570;
                        }
                        else
                        {
                            if (trayx.Result != "Success")
                            {
                                Debug.WriteLine($"放Tary盘轴移动到等待位失败！" + trayx.Result);
                            }
                            if (auto2u.Result != "Success")
                            {
                                Debug.WriteLine($"Auto2顶盘轴移动到等待位失败！" + trayx.Result);
                            }
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                        }
                        break;

                    case 570:
                        PS抓盘手臂放空盘到Auto2流程.StepName = "检测Auto2是否无空盘";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
                        result = Auto2_前Tray存在感应(out value);
                        result2 = Auto2_后Tray放歪感应(out value2);
                        if (result == "Success" && result2 == "Success")
                        {
                            if (value == "1" && value2 == "1") //返回0是挡到感应器,1是没有挡到感应器
                            {
                                PS抓盘手臂放空盘到Auto2流程.StepID = 575;
                            }
                            else //没有挡到感应器或者放歪感应器被挡到说明没有放盘成功，需重新放盘
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;

                                if (value != "1")
                                {
                                    Debug.WriteLine($"抓盘手臂放盘失败！Auto2有盘感应到Tray盘,value:" + value);
                                }
                                if (value2 != "1")
                                {
                                    Debug.WriteLine($"抓盘手臂放盘失败！Auto2放歪感应到Tray盘,value:" + value2);
                                }
                            }
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            if (result != "Success") Debug.WriteLine($"读取Auto2前Tray存在感应失败！" + result);
                            if (result2 != "Success") Debug.WriteLine($"读取Auto2后Tray放歪感应失败！" + result2);
                        }
                        break;

                    case 575:
                        PS抓盘手臂放空盘到Auto2流程.StepName = "放Tary盘轴移动到Auto2";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
                        result = 放Tary盘轴移动(初始点位.抓盘_Auto2_X);
                        if (result == "Success")
                        {
                            PS抓盘手臂放空盘到Auto2流程.StepID = 580;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"放Tary盘轴移动到Auto2失败！" + result);
                        }
                        break;

                    case 580:
                        PS抓盘手臂放空盘到Auto2流程.StepName = "移Tray上下气缸到位输出";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
                        result = 移Tray上下气缸到位输出();
                        if (result == "Success")
                        {
                            PS抓盘手臂放空盘到Auto2流程.StepID = 585;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"移Tray上下气缸到位输出失败！" + result);
                        }
                        break;

                    case 585:
                        PS抓盘手臂放空盘到Auto2流程.StepName = "移Tray夹爪气缸原位输出";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
                        result = 移Tray夹爪气缸原位输出();
                        if (result == "Success")
                        {
                            PS抓盘手臂放空盘到Auto2流程.StepID = 590;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"移Tray夹爪气缸原位输出失败！" + result);
                        }
                        break;

                    case 590:
                        PS抓盘手臂放空盘到Auto2流程.StepName = "移Tray上下气缸原位输出";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
                        result = 移Tray上下气缸原位输出();
                        if (result == "Success")
                        {
                            PS抓盘手臂放空盘到Auto2流程.StepID = 591;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"移Tray上下气缸原位输出失败！" + result);
                        }
                        break;

                    case 591:
                        PS抓盘手臂放空盘到Auto2流程.StepName = "检测Auto2是否有空盘";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
                        result = Auto2_前Tray存在感应(out value);
                        result2 = Auto2_后Tray放歪感应(out value2);
                        if (result == "Success" && result2 == "Success")
                        {
                            if (value == "0" && value2 == "1") //返回0是挡到感应器,1是没有挡到感应器
                            {
                                PS抓盘手臂放空盘到Auto2流程.StepID = 592;
                            }
                            else //没有挡到感应器或者放歪感应器被挡到说明没有放盘成功，需重新放盘
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;

                                if (value != "0")
                                {
                                    Debug.WriteLine($"抓盘手臂放盘失败！Auto2没有感应到Tray盘,value:" + value);
                                }
                                if (value2 != "1")
                                {
                                    Debug.WriteLine($"抓盘手臂放盘失败！Auto2感应到Tray盘放歪,value:" + value2);
                                }
                            }
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            if (result != "Success") Debug.WriteLine($"读取Auto2前Tray存在感应失败！" + result);
                            if (result2 != "Success") Debug.WriteLine($"读取Auto2后Tray放歪感应失败！" + result2);
                        }
                        break;

                    case 592:
                        PS抓盘手臂放空盘到Auto2流程.StepName = $"Auto2_Tray前侧定位气缸到位输出";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
                        result = Auto2_Tray前侧定位气缸到位输出();
                        if (result == "Success")
                        {
                            PS抓盘手臂放空盘到Auto2流程.StepID = 595;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"Auto2_Tray前侧定位气缸到位输出失败！" + result);
                        }
                        break;

                    case 595:
                        PS抓盘手臂放空盘到Auto2流程.StepName = "放Tary盘轴回到原点";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
                        result = 放Tary盘轴移动(0);
                        if (result == "Success")
                        {
                            PS抓盘手臂放空盘到Auto2流程.StepID = 599;
                        }
                        else
                        {
                            APPmaster.ProgramPause = true;
                            IsPause = true;
                            Debug.WriteLine($"放Tary盘轴回到原点失败！" + result);
                        }
                        break;

                    case 599:
                        抓盘手臂放空盘到Auto2标记 = 1;
                        PS抓盘手臂放空盘到Auto2流程.StepName = $"抓盘手臂放空盘到Auto2流程完成";
                        Debug.WriteLine($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
                        PS抓盘手臂放空盘到Auto2流程.Processing = false;
                        PS抓盘手臂放空盘到Auto2流程.StepID = 500;
                        break;

                    default:
                        Debug.WriteLine(("PS抓盘手臂放空盘到Auto2流程.StepID =" + PS抓盘手臂放空盘到Auto2流程.StepID.ToString() + "对应流程不存在"));
                        break;
                }
            }
        }

        //public string 抓盘手臂换Auto1空盘()
        //{
        //    string result = "";
        //    string result2 = "";
        //    PS抓盘手臂换Auto1空盘流程.StepID = 500;
        //    while (true)
        //    {
        //        //if (!是否可以开始运行())
        //        //{
        //        //    continue;
        //        //}

        //        if (APPmaster.IsPause)
        //        {
        //            continue;
        //        }

        //        switch (PS抓盘手臂换Auto1空盘流程.StepID)
        //        {
        //            case 500:
        //                PS抓盘手臂换Auto1空盘流程.StepName = "路径规划";
        //                PS抓盘手臂换Auto1空盘流程.Condition = 抓盘手臂换Auto1空盘标记 == 2;
        //                PS抓盘手臂换Auto1空盘流程.Permit = !PS抓盘手臂换Auto1空盘流程.Processing;
        //                if (PS抓盘手臂换Auto1空盘流程.Condition && PS抓盘手臂换Auto1空盘流程.Permit)
        //                {
        //                    Auto1上顶标记 = 2;
        //                    抓盘手臂放空盘到Auto1标记 = 2;
        //                    PS抓盘手臂换Auto1空盘流程.Processing = true;
        //                    PS抓盘手臂换Auto1空盘流程.StepID = 505;
        //                }
        //                break;

        //            case 505:
        //                PS抓盘手臂换Auto1空盘流程.StepName = "等待抓盘手臂放空盘到Auto1";
        //                if (抓盘手臂放空盘到Auto1标记 == 1)
        //                {
        //                    PS抓盘手臂换Auto1空盘流程.StepID = 510;
        //                }
        //                else
        //                {
        //                    Thread.Sleep(10);
        //                }
        //                break;

        //            case 510:
        //                PS抓盘手臂换Auto1空盘流程.StepName = "判断Auto1料仓是否满盘";
        //                Debug.WriteLine($"PS抓盘手臂换Auto1空盘流程.StepID={PS抓盘手臂换Auto1空盘流程.StepID}->{PS抓盘手臂换Auto1空盘流程.StepName}");
        //                result = motionEx.GetCmdEncPos("M14_Auto1顶盘轴", out double prfpos);
        //                if (result == "Success")
        //                {
        //                    if (prfpos < Auto1满盘判断值)
        //                    {
        //                        Auto1上顶标记 = 2;
        //                        抓盘手臂放空盘到Auto1标记 = 2;
        //                        Debug.WriteLine($"M14_Auto1顶盘轴当前行程{prfpos}<Auto1满盘判断值{Auto1满盘判断值}");
        //                        PS抓盘手臂换Auto1空盘流程.StepID = 515;
        //                    }
        //                    else PS抓盘手臂换Auto1空盘流程.StepID = 599;
        //                }
        //                else
        //                {
        //                    APPmaster.ProgramPause = true;
        //                    IsPause = true;
        //                    Debug.WriteLine($"判断Auto1料仓是否满盘失败！" + result);
        //                }
        //                break;

        //            case 515:
        //                PS抓盘手臂换Auto1空盘流程.StepName = "等待抓盘手臂放空盘到Auto1";
        //                if (抓盘手臂放空盘到Auto1标记 == 1)
        //                {
        //                    Auto1料仓打开标记 = true;
        //                    PS抓盘手臂换Auto1空盘流程.StepID = 520;
        //                }
        //                else
        //                {
        //                    Thread.Sleep(10);
        //                }
        //                break;

        //            case 520:
        //                PS抓盘手臂换Auto1空盘流程.StepName = "等待料仓料拿走";
        //                if (Auto1料仓打开标记 == false) //料仓被打开
        //                {
        //                    PS抓盘手臂换Auto1空盘流程.StepID = 525;
        //                }
        //                else
        //                {
        //                    APPmaster.ProgramPause = true;
        //                    IsPause = true;
        //                    Debug.WriteLine($"Auto1料仓满料");
        //                }
        //                break;

        //            case 525:
        //                PS抓盘手臂换Auto1空盘流程.StepName = "Auto1向上移动到等待位";
        //                result = Auto1顶盘轴移动(15);//3175 1.5875
        //                if (result == "Success")
        //                {
        //                    PS抓盘手臂换Auto1空盘流程.StepID = 530;
        //                }
        //                else
        //                {
        //                    APPmaster.ProgramPause = true;
        //                    IsPause = true;
        //                    Debug.WriteLine($"Auto1向上移动到等待位失败!" + result);
        //                }
        //                break;

        //            case 530:
        //                PS抓盘手臂换Auto1空盘流程.StepName = "检测Auto1是否有空盘";
        //                result = Auto1_前Tray存在感应(out string value);
        //                result2 = Auto1_后Tray放歪感应(out string value2);
        //                Debug.WriteLine($"PS抓盘手臂换Auto1空盘流程.StepID={PS抓盘手臂换Auto1空盘流程.StepID}->{PS抓盘手臂换Auto1空盘流程.StepName}");
        //                if (result == "Success" && result2 == "Success")
        //                {
        //                    if (value == "0" && value2 == "1") //返回0是挡到感应器,1是没有挡到感应器
        //                    {
        //                        抓盘手臂换Auto1空盘标记 = 1;
        //                        PS抓盘手臂换Auto1空盘流程.StepID = 599;
        //                    }
        //                    else //没有挡到感应器或者放歪感应器被挡到说明没有放盘成功，需重新放盘
        //                    {
        //                        APPmaster.ProgramPause = true;
        //                        IsPause = true;

        //                        if (value != "0")
        //                        {
        //                            Debug.WriteLine($"抓盘手臂放盘失败！Auto1没有感应到Tray盘" + result);
        //                        }
        //                        if (value2 != "1")
        //                        {
        //                            Debug.WriteLine($"抓盘手臂放盘失败！Auto1感应到Tray盘放歪" + result2);
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    APPmaster.ProgramPause = true;
        //                    IsPause = true;
        //                    if (result != "Success") Debug.WriteLine($"读取Auto1前Tray存在感应失败！" + result);
        //                    if (result2 != "Success") Debug.WriteLine($"读取Auto1后Tray放歪感应失败！" + result);
        //                }
        //                break;

        //            case 599:
        //                PS抓盘手臂换Auto1空盘流程.StepName = $"抓盘手臂换Auto1空盘流程完成";
        //                Debug.WriteLine($"PS抓盘手臂换Auto1空盘流程.StepID={PS抓盘手臂换Auto1空盘流程.StepID}->{PS抓盘手臂换Auto1空盘流程.StepName}");
        //                PS抓盘手臂换Auto1空盘流程.Processing = false;
        //                PS抓盘手臂换Auto1空盘流程.StepID = 500;
        //                return "Success";

        //            default:
        //                Debug.WriteLine(("PS抓盘手臂换Auto1空盘流程.StepID =" + PS抓盘手臂换Auto1空盘流程.StepID.ToString() + "对应流程不存在"));
        //                return "Fail";
        //        }
        //    }
        //}

        //public string 抓盘手臂换Auto2空盘()
        //{
        //    string result = "";
        //    string result2 = "";
        //    PS抓盘手臂换Auto2空盘流程.StepID = 500;
        //    while (true)
        //    {
        //        //if (!是否可以开始运行())
        //        //{
        //        //    continue;
        //        //}

        //        if (APPmaster.IsPause)
        //        {
        //            continue;
        //        }

        //        switch (PS抓盘手臂换Auto2空盘流程.StepID)
        //        {
        //            case 500:
        //                PS抓盘手臂换Auto2空盘流程.StepName = "路径规划";
        //                PS抓盘手臂换Auto2空盘流程.Condition = 抓盘手臂换Auto2空盘标记 == 2;
        //                PS抓盘手臂换Auto2空盘流程.Permit = !PS抓盘手臂换Auto2空盘流程.Processing;
        //                if (PS抓盘手臂换Auto2空盘流程.Condition && PS抓盘手臂换Auto2空盘流程.Permit)
        //                {
        //                    Auto2上顶标记 = 2;
        //                    抓盘手臂放空盘到Auto2标记 = 2;
        //                    PS抓盘手臂换Auto2空盘流程.Processing = true;
        //                    PS抓盘手臂换Auto2空盘流程.StepID = 505;
        //                }
        //                break;

        //            case 505:
        //                PS抓盘手臂换Auto2空盘流程.StepName = "等待抓盘手臂放空盘到Auto2";
        //                if (抓盘手臂放空盘到Auto2标记 == 1)
        //                {
        //                    PS抓盘手臂换Auto2空盘流程.StepID = 510;
        //                }
        //                else
        //                {
        //                    Thread.Sleep(10);
        //                }
        //                break;

        //            case 510:
        //                PS抓盘手臂换Auto2空盘流程.StepName = "判断Auto2料仓是否满盘";
        //                Debug.WriteLine($"PS抓盘手臂换Auto2空盘流程.StepID={PS抓盘手臂换Auto2空盘流程.StepID}->{PS抓盘手臂换Auto2空盘流程.StepName}");
        //                result = motionEx.GetCmdEncPos("M15_Auto2顶盘轴", out double prfpos);
        //                if (result == "Success")
        //                {
        //                    if (prfpos < Auto2满盘判断值)
        //                    {
        //                        Auto2上顶标记 = 2;
        //                        抓盘手臂放空盘到Auto2标记 = 2;
        //                        Debug.WriteLine($"M15_Auto2顶盘轴当前行程{prfpos}<Auto2满盘判断值{Auto2满盘判断值}");
        //                        PS抓盘手臂换Auto2空盘流程.StepID = 515;
        //                    }
        //                    else PS抓盘手臂换Auto2空盘流程.StepID = 599;
        //                }
        //                else
        //                {
        //                    APPmaster.ProgramPause = true;
        //                    IsPause = true;
        //                    Debug.WriteLine($"判断Auto2料仓是否满盘失败！" + result);
        //                }
        //                break;

        //            case 515:
        //                PS抓盘手臂换Auto2空盘流程.StepName = "等待抓盘手臂放空盘到Auto2";
        //                if (抓盘手臂放空盘到Auto2标记 == 1)
        //                {
        //                    Auto2料仓打开标记 = true;
        //                    PS抓盘手臂换Auto2空盘流程.StepID = 520;
        //                }
        //                else
        //                {
        //                    Thread.Sleep(10);
        //                }
        //                break;

        //            case 520:
        //                PS抓盘手臂换Auto2空盘流程.StepName = "等待料仓料拿走";
        //                if (Auto2料仓打开标记 == false) //料仓被打开
        //                {
        //                    PS抓盘手臂换Auto2空盘流程.StepID = 525;
        //                }
        //                else
        //                {
        //                    APPmaster.ProgramPause = true;
        //                    IsPause = true;
        //                    Debug.WriteLine($"Auto2料仓满料");
        //                }
        //                break;

        //            case 525:
        //                PS抓盘手臂换Auto2空盘流程.StepName = "Auto2向上移动到等待位";
        //                result = Auto2顶盘轴移动(15);//3175 1.5875
        //                if (result == "Success")
        //                {
        //                    PS抓盘手臂换Auto2空盘流程.StepID = 530;
        //                }
        //                else
        //                {
        //                    APPmaster.ProgramPause = true;
        //                    IsPause = true;
        //                    Debug.WriteLine($"Auto2向上移动到等待位失败!" + result);
        //                }
        //                break;

        //            case 530:
        //                PS抓盘手臂换Auto2空盘流程.StepName = "检测Auto2是否有空盘";
        //                result = Auto2_前Tray存在感应(out string value);
        //                result2 = Auto2_后Tray放歪感应(out string value2);
        //                if (result == "Success" && result2 == "Success")
        //                {
        //                    if (value == "0" && value2 == "1") //返回0是挡到感应器,1是没有挡到感应器
        //                    {
        //                        抓盘手臂换Auto2空盘标记 = 1;
        //                        PS抓盘手臂换Auto2空盘流程.StepID = 599;
        //                    }
        //                    else //没有挡到感应器或者放歪感应器被挡到说明没有放盘成功，需重新放盘
        //                    {
        //                        APPmaster.ProgramPause = true;
        //                        IsPause = true;

        //                        if (value != "0")
        //                        {
        //                            Debug.WriteLine($"抓盘手臂放盘失败！Auto2没有感应到Tray盘" + result);
        //                        }
        //                        if (value2 != "1")
        //                        {
        //                            Debug.WriteLine($"抓盘手臂放盘失败！Auto2感应到Tray盘放歪" + result2);
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    APPmaster.ProgramPause = true;
        //                    IsPause = true;
        //                    if (result != "Success") Debug.WriteLine($"读取Auto2前Tray存在感应失败！" + result);
        //                    if (result2 != "Success") Debug.WriteLine($"读取Auto2后Tray放歪感应失败！" + result);
        //                }
        //                break;

        //            case 599:
        //                PS抓盘手臂换Auto2空盘流程.StepName = $"抓盘手臂换Auto2空盘流程完成";
        //                Debug.WriteLine($"PS抓盘手臂换Auto2空盘流程.StepID={PS抓盘手臂换Auto2空盘流程.StepID}->{PS抓盘手臂换Auto2空盘流程.StepName}");
        //                PS抓盘手臂换Auto2空盘流程.Processing = false;
        //                PS抓盘手臂换Auto2空盘流程.StepID = 500;
        //                return "Success";

        //            default:
        //                Debug.WriteLine(("PS抓盘手臂换Auto2空盘流程.StepID =" + PS抓盘手臂换Auto2空盘流程.StepID.ToString() + "对应流程不存在"));
        //                return "Fail";
        //        }
        //    }
        //}

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
