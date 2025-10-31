using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using Microsoft.SqlServer.Server;
using MotionCommon;
using SQLiteHelper;
using SQLServerHelper;
using Sunny.UI;
using Sunny.UI.Win32;
using TextFileLogger;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.MonthCalendar;
using static H5509_V2.APPmaster;

namespace H5509_V2
{
    public class Handler
    {
        //public MotionEx motionEx;

        public bool Approvallamp { get; set; } = false;

        public bool Onecyclelamp { get; set; } = false;

        public System.Timers.Timer 结批触发定时器 = new System.Timers.Timer();

        public System.Timers.Timer 结批按钮闪烁的定时器 = new System.Timers.Timer(); //结批按钮灯闪烁定时器

        public System.Timers.Timer OneCycle按钮闪烁的定时器 = new System.Timers.Timer(); //OneCycle按钮闪烁的定时器

        public System.Timers.Timer Load门开启定时器 = new System.Timers.Timer();

        public Action 刷新批次信息;

        public Action 刷新分BIN信息;

        public Action 刷新权限;

        public Action 显示Dout颜色;

        public Func<bool> 检查输入工单输入;

        public Handler()
        {
            //this.motionEx = motionEx;

            结批触发定时器.Interval = 2 * 1000; //2秒钟
            结批触发定时器.Elapsed += ForcedBatchTick;

            结批按钮闪烁的定时器.Interval = 400;
            结批按钮闪烁的定时器.Elapsed += finalLampFlickertimerTick;

            OneCycle按钮闪烁的定时器.Interval = 400;
            OneCycle按钮闪烁的定时器.Elapsed += onecycletimerTick;

            Load门开启定时器.Interval = Load门开门时间 * 1000;
            Load门开启定时器.Elapsed += Load门开启定时器_Tick;
        }

        private void Load门开启定时器_Tick(object sender, EventArgs eventArgs)
        {
            Load门开启定时器.Stop();
            门已打开超设定时间 = true;
            RunLog("Load门开启时间达到");
        }

        private void ForcedBatchTick(object sender, EventArgs eventArgs)
        {
            结批触发定时器.Stop();
            RunLog("结批按钮已按下5s");
            if (!结批标记 && !一个循环标记)
            {
                if (温度模式 == "高温")
                {
                    结批选项选择中 = true;
                    ProgramPause = true;
                    IsPause = true;
                    DialogResult dialog = MessageBox.Show("请确认是否清空加热盘?\n是:将加热盘剩余IC取完\n否:直接结批\n取消:不做任何操作", "提示", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    if (dialog == DialogResult.Yes)
                    {
                        强制结批标记 = true;
                        结批标记 = true;
                        按下结批时间 = DateTime.Now;
                        结批按钮闪烁的定时器.Start();
                    }
                    else if (dialog == DialogResult.No)
                    {
                        强制结批标记 = true;
                        结批标记 = true;
                        按下结批时间 = DateTime.Now;
                        加热盘停止入料标记 = true;
                        结批按钮闪烁的定时器.Start();
                    }
                    结批选项选择中 = false;
                    ProgramStart = true;
                }
                else
                {
                    强制结批标记 = true;
                    结批标记 = true;
                    按下结批时间 = DateTime.Now;
                    结批按钮闪烁的定时器.Start();
                }
            }
            else
            {
                if (结批标记) RunLog("无法触发结批,正在结批中...");
                if (一个循环标记) RunLog("无法触发结批,正在一个循环中...");
            }
        }

        private void finalLampFlickertimerTick(object sender, EventArgs eventArgs)
        {
            Approvallamp = !Approvallamp;
            motionEx.SetDo("Y008_结批灯", Approvallamp);
        }

        private void onecycletimerTick(object sender, EventArgs eventArgs)
        {
            Onecyclelamp = !Onecyclelamp;
            motionEx.SetDo("Y009_一个循环灯", Onecyclelamp);
        }

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

            return motionEx.AxisMove(axisNames.ToArray(), pPoss.ToArray(), 0, isdown ? 1 : 0, true);
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

            return motionEx.AxisMove(axisNames.ToArray(), pPoss.ToArray(), 0, isdown ? 1 : 0, true);
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
            return motionEx.AxisAbsoluteMove("M20_飞梭轴", pos);//, 梭车第二段距离
        }

        public string Fix3放IC盘轴移动(double pos, int type = 0)
        {
            return motionEx.AxisAbsoluteMove("M11_Fix3放IC轴", pos, type);
        }

        public string Empty顶盘轴移动(double pos, int type = 0)
        {
            return motionEx.AxisAbsoluteMove("M12_Empty顶盘轴", pos, type);
        }

        public string Empty顶盘轴移动2(double pos, int type = 0)
        {
            string result = motionEx.GetCmdEncPos("M12_Empty顶盘轴", out double prfpos1);
            if (result == "Success")
            {
                result = GetAxisRunInfo("M12_Empty顶盘轴", out AxisRunInfo axisRunInfo);
                if (result == "Success")
                {
                    double movepos = Convert.ToDouble(prfpos1 * axisRunInfo.步长mm) + pos; //6.35
                    return motionEx.AxisAbsoluteMove("M12_Empty顶盘轴", movepos, type);
                }
                else
                {
                    RunLog("M12_Empty顶盘轴信息获取失败!" + result);
                    return "M12_Empty顶盘轴信息获取失败!" + result;
                }
            }
            else
            {
                RunLog("M12_Empty顶盘轴实时位置失败!" + result);
                return "M12_Empty顶盘轴实时位置失败!" + result;
            }
        }

        public string Load顶盘轴移动(double pos, int type = 0)
        {
            return motionEx.AxisAbsoluteMove("M13_Loader顶盘轴", pos, type);
        }

        public string Load顶盘轴移动2(double pos, int type = 0)
        {
            string result = motionEx.GetCmdEncPos("M13_Loader顶盘轴", out double prfpos1);
            if (result == "Success")
            {
                result = GetAxisRunInfo("M13_Loader顶盘轴", out AxisRunInfo axisRunInfo);
                if (result == "Success")
                {
                    double movepos = Convert.ToDouble(prfpos1 * axisRunInfo.步长mm) + pos; //6.35
                    return motionEx.AxisAbsoluteMove("M13_Loader顶盘轴", movepos, type);
                }
                else
                {
                    RunLog("M13_Loader顶盘轴信息获取失败!" + result);
                    return "M13_Loader顶盘轴信息获取失败!" + result;
                }
            }
            else
            {
                RunLog("M13_Loader顶盘轴实时位置失败!" + result);
                return "M13_Loader顶盘轴实时位置失败!" + result;
            }
        }

        public string Auto1顶盘轴移动(double pos, int type = 0)
        {
            return motionEx.AxisAbsoluteMove("M14_Auto1顶盘轴", pos, type);
        }

        public string Auto2顶盘轴移动(double pos, int type = 0)
        {
            return motionEx.AxisAbsoluteMove("M15_Auto2顶盘轴", pos, type);
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

        public string Empty顶盘轴回零(int type = 0)
        {
            return motionEx.AxisHome("M12_Empty顶盘轴", type);
        }

        public string Load顶盘轴回零(int type = 0)
        {
            return motionEx.AxisHome("M13_Loader顶盘轴", type);
        }

        public string Auto1顶盘轴回零(int type = 0)
        {
            return motionEx.AxisHome("M14_Auto1顶盘轴", type);
        }

        public string Auto2顶盘轴回零(int type = 0)
        {
            return motionEx.AxisHome("M15_Auto2顶盘轴", type);
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

        #endregion 轴回零

        #region IO控制与读取

        public string 读取气缸输入信号(string ioName, out string value)
        {
            string result = motionEx.GetDi(ioName, out value);
            if (result != "Success") RunLog($"{ioName}读取失败！" + result);
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
            if (result != "Success") RunLog($"测试吸嘴张开气缸{(value ? "到位" : "原位")}输出失败！" + result);
            return result;
        }

        #region Y094_测试吸嘴张开气缸

        public string 测试吸嘴_张开气缸到位输出()
        {
            string result = motionEx.SetDo("Y094_测试吸嘴张开气缸", true);
            if (result != "Success")
            {
                RunLog("测试吸嘴张开气缸到位输出失败！" + result);
                return result;
            }

            return 测试吸嘴张开气缸到位输入读取();
        }

        public string 测试吸嘴_张开气缸原位输出()
        {
            string result = motionEx.SetDo("Y094_测试吸嘴张开气缸", false);
            if (result != "Success")
            {
                RunLog("测试吸嘴张开气缸原位输出失败！" + result);
                return result;
            }

            return 测试吸嘴张开气缸原位输入读取();
        }

        public string 测试吸嘴张开气缸到位输入读取()
        {
            string result = 读取气缸输入信号("X173_测试吸嘴张开气缸到位", "1", out long ms);
            if (result != "Success") RunLog($"测试吸嘴张开气缸到位输出超时({ms}ms)");

            //else RunLog($"测试吸嘴张开气缸到位输出成功({ms}ms)");

            return result;
        }

        public string 测试吸嘴张开气缸原位输入读取()
        {
            string result = 读取气缸输入信号("X174_测试吸嘴张开气缸原位", "1", out long ms);

            if (result != "Success") RunLog($"测试吸嘴张开气缸原位输出超时({ms}ms)");
            //else RunLog($"测试吸嘴张开气缸原位输出成功({ms}ms)");

            return result;
        }

        #endregion

        #region  Y050_移Tray上下气缸

        public string 移Tray上下气缸原位输入读取(out string value)
        {
            string result = 读取气缸输入信号("X064_移Tray上下缸上感应", out value);
            if (result != "Success") RunLog($"移Tray上下气缸原位输入读取失败！" + result);

            return result;
        }

        public string 移Tray上下气缸到位输入读取()
        {
            string result = 读取气缸输入信号("X065_移Tray上下缸下感应", "1", out long ms);

            if (result != "Success") RunLog($"移Tray上下气缸原位输出超时({ms}ms)");
            //else RunLog($"移Tray上下气缸原位输出成功({ms}ms)");

            return result;
        }

        public string 移Tray上下气缸原位输入读取()
        {
            string result = 读取气缸输入信号("X064_移Tray上下缸上感应", "1", out long ms);

            if (result != "Success") RunLog($"移Tray上下气缸到位输出超时({ms}ms)");
            //else RunLog($"移Tray上下气缸到位输出成功({ms}ms)");

            return result;
        }

        public string 移Tray上下气缸到位输出()
        {
            string result = motionEx.SetDo("Y050_移Tray上下气缸", true);
            if (result != "Success")
            {
                RunLog("移Tray上下气缸到位输出失败！" + result);
                return result;
            }

            return 移Tray上下气缸到位输入读取();
        }

        public string 移Tray上下气缸原位输出()
        {
            string result = motionEx.SetDo("Y050_移Tray上下气缸", false);
            if (result != "Success")
            {
                RunLog("移Tray上下气缸原位输出失败！" + result);
                return result;
            }

            return 移Tray上下气缸原位输入读取();
        }

        #endregion

        #region Y051_移Tray夹爪气缸

        public string 移Tray夹爪气缸原位输入读取(out string value)
        {
            string result = 读取气缸输入信号("X066_移Tray夹紧缸松开感应", out value);
            if (result != "Success") RunLog($"移Tray夹爪气缸原位输入读取失败！" + result);

            return result;
        }

        public string 移Tray夹爪气缸到位输入读取(out string value)
        {
            string result = 读取气缸输入信号("X067_移Tray夹紧缸夹紧感应", out value);
            if (result != "Success") RunLog($"移Tray夹爪气缸到位输入读取失败！" + result);

            return result;
        }

        public string 移Tray夹爪气缸到位输入读取()
        {
            string result = 读取气缸输入信号("X067_移Tray夹紧缸夹紧感应", "1", out long ms);

            if (result != "Success") RunLog($"移Tray夹爪气缸到位输出超时({ms}ms)");
            //else RunLog($"移Tray夹爪气缸到位输出成功({ms}ms)");

            return result;
        }

        public string 移Tray夹爪气缸原位输入读取()
        {
            string result = 读取气缸输入信号("X066_移Tray夹紧缸松开感应", "1", out long ms);

            if (result != "Success") RunLog($"移Tray夹爪气缸原位输出超时({ms}ms)");
            //else RunLog($"移Tray夹爪气缸原位输出成功({ms}ms)");

            return result;
        }

        public string 移Tray夹爪气缸到位输出()
        {
            string result = motionEx.SetDo("Y051_移Tray夹爪气缸", true);
            if (result != "Success")
            {
                RunLog("移Tray夹爪气缸到位输出失败！" + result);
                return result;
            }

            return 移Tray夹爪气缸到位输入读取();
        }

        public string 移Tray夹爪气缸原位输出()
        {
            string result = motionEx.SetDo("Y051_移Tray夹爪气缸", false);
            if (result != "Success")
            {
                RunLog("移Tray夹爪气缸原位输出失败！" + result);
                return result;
            }

            return 移Tray夹爪气缸原位输入读取();
        }

        #endregion

        #region 测试区位移和上下位移气缸

        public string 测试区_位移气缸原位信号读取(int site)
        {
            string result = 获取位移气缸(site, out string yIoName);
            string result2 = 获取位移气缸原位感应(site, out string xIoName);
            if (result != "Success") return result;
            if (result2 != "Success") return result2;

            result = motionEx.GetDi(xIoName, out string value);

            //result = 读取气缸输入信号(xIoName, "1", out long ms);
            if (result != "Success") return $"{yIoName}位移气缸原位信号读取失败!" + result;

            if (value == "1") return "Success";
            else return "Fail";

        }

        public string 测试区_升降气缸原位信号读取(int site)
        {
            string result = 获取下压气缸(site, out string yIoName);
            string result2 = 获取下压气缸原位感应(site, out string xIoName);
            if (result != "Success") return result;
            if (result2 != "Success") return result2;

            result = motionEx.GetDi(xIoName, out string value);

            //result = 读取气缸输入信号(xIoName, "1", out long ms);
            if (result != "Success") return $"{yIoName}升降气缸原位信号读取失败!" + result;

            if (value == "1") return "Success";
            else return "Fail";
        }

        public string 测试区_位移气缸到位输出(string yIoName, string xIoName)
        {
            string result = motionEx.SetDo(yIoName, true);
            if (result != "Success")
            {
                RunLog($"{yIoName}到位输出失败！" + result);
                return result;
            }

            result = 读取气缸输入信号(xIoName, "1", out long ms);
            if (result != "Success") RunLog($"{yIoName}到位输出超时({ms}ms)");
            //else RunLog($"{yIoName}到位输出成功({ms}ms)");
            return result;
        }

        public string 测试区_位移气缸原位输出(string yIoName, string xIoName)
        {
            string result = motionEx.SetDo(yIoName, false);
            if (result != "Success")
            {
                RunLog($"{yIoName}原位输出失败！" + result);
                return result;
            }

            result = 读取气缸输入信号(xIoName, "1", out long ms);
            if (result != "Success") RunLog($"{yIoName}原位输出超时({ms}ms)");
            //else RunLog($"{yIoName}原位输出成功({ms}ms)");
            return result;
        }

        public string 测试区_下压气缸到位输出(string yIoName, string xIoName)
        {
            string result = motionEx.SetDo(yIoName, true);
            if (result != "Success")
            {
                RunLog($"{yIoName}到位输出失败！" + result);
                return result;
            }

            result = 读取气缸输入信号(xIoName, "1", out long ms);
            if (result != "Success") RunLog($"{yIoName}到位输出超时({ms}ms)");
            //else RunLog($"{yIoName}到位输出成功({ms}ms)");
            return result;
        }

        public string 测试区_下压气缸原位输出(string yIoName, string xIoName)
        {
            string result = motionEx.SetDo(yIoName, false);
            if (result != "Success")
            {
                RunLog($"{yIoName}原位输出失败！" + result);
                return result;
            }
            result = 读取气缸输入信号(xIoName, "1", out long ms);

            if (result != "Success") RunLog($"{yIoName}原位输出超时({ms}ms)");
            //else RunLog($"{yIoName}原位输出成功({ms}ms)");
            return result;
        }

        public string 获取位移气缸(int site, out string yIoName)
        {
            yIoName = "Fail";
            switch (site)
            {
                case 0: yIoName = "Y096_1_位移气缸"; break;
                case 1: yIoName = "Y098_2_位移气缸"; break;
                case 2: yIoName = "Y100_3_位移气缸"; break;
                case 3: yIoName = "Y102_4_位移气缸"; break;
                case 4: yIoName = "Y104_5_位移气缸"; break;
                case 5: yIoName = "Y106_6_位移气缸"; break;
                case 6: yIoName = "Y108_7_位移气缸"; break;
                case 7: yIoName = "Y110_8_位移气缸"; break;
                case 8: yIoName = "Y112_9_位移气缸"; break;
                case 9: yIoName = "Y114_10_位移气缸"; break;
                case 10: yIoName = "Y116_11_位移气缸"; break;
                case 11: yIoName = "Y118_12_位移气缸"; break;
                case 12: yIoName = "Y120_13_位移气缸"; break;
                case 13: yIoName = "Y122_14_位移气缸"; break;
                case 14: yIoName = "Y124_15_位移气缸"; break;
                case 15: yIoName = "Y126_16_位移气缸"; break;
                default: return $"site:{site}不存在";
            }
            return "Success";
        }

        public string 获取下压气缸(int site, out string yIoName)
        {
            yIoName = "Fail";
            switch (site)
            {
                case 0: yIoName = "Y097_1_下压气缸"; break;
                case 1: yIoName = "Y099_2_下压气缸"; break;
                case 2: yIoName = "Y101_3_下压气缸"; break;
                case 3: yIoName = "Y103_4_下压气缸"; break;
                case 4: yIoName = "Y105_5_下压气缸"; break;
                case 5: yIoName = "Y107_6_下压气缸"; break;
                case 6: yIoName = "Y109_7_下压气缸"; break;
                case 7: yIoName = "Y111_8_下压气缸"; break;
                case 8: yIoName = "Y113_9_下压气缸"; break;
                case 9: yIoName = "Y115_10_下压气缸"; break;
                case 10: yIoName = "Y117_11_下压气缸"; break;
                case 11: yIoName = "Y119_12_下压气缸"; break;
                case 12: yIoName = "Y121_13_下压气缸"; break;
                case 13: yIoName = "Y123_14_下压气缸"; break;
                case 14: yIoName = "Y125_15_下压气缸"; break;
                case 15: yIoName = "Y127_16_下压气缸"; break;
                default: return $"site:{site}不存在";
            }
            return "Success";
        }

        public string 获取位移气缸到位感应(int site, out string xIoName)
        {
            xIoName = "Fail";
            switch (site)
            {
                case 0: xIoName = "X093_1_测试模组位移气缸前感应"; break;
                case 1: xIoName = "X098_2_测试模组位移气缸前感应"; break;
                case 2: xIoName = "X103_3_测试模组位移气缸前感应"; break;
                case 3: xIoName = "X108_4_测试模组位移气缸前感应"; break;
                case 4: xIoName = "X113_5_测试模组位移气缸前感应"; break;
                case 5: xIoName = "X118_6_测试模组位移气缸前感应"; break;
                case 6: xIoName = "X123_7_测试模组位移气缸前感应"; break;
                case 7: xIoName = "X128_8_测试模组位移气缸前感应"; break;
                case 8: xIoName = "X133_9_测试模组位移气缸前感应"; break;
                case 9: xIoName = "X138_10_测试模组位移气缸前感应"; break;
                case 10: xIoName = "X143_11_测试模组位移气缸前感应"; break;
                case 11: xIoName = "X148_12_测试模组位移气缸前感应"; break;
                case 12: xIoName = "X153_13_测试模组位移气缸前感应"; break;
                case 13: xIoName = "X158_14_测试模组位移气缸前感应"; break;
                case 14: xIoName = "X163_15_测试模组位移气缸前感应"; break;
                case 15: xIoName = "X168_16_测试模组位移气缸前感应"; break;
                default: return $"site:{site}不存在";
            }
            return "Success";
        }

        public string 获取位移气缸原位感应(int site, out string xIoName)
        {
            xIoName = "Fail";
            switch (site)
            {
                case 0: xIoName = "X092_1_测试模组位移气缸后感应"; break;
                case 1: xIoName = "X097_2_测试模组位移气缸后感应"; break;
                case 2: xIoName = "X102_3_测试模组位移气缸后感应"; break;
                case 3: xIoName = "X107_4_测试模组位移气缸后感应"; break;
                case 4: xIoName = "X112_5_测试模组位移气缸后感应"; break;
                case 5: xIoName = "X117_6_测试模组位移气缸后感应"; break;
                case 6: xIoName = "X122_7_测试模组位移气缸后感应"; break;
                case 7: xIoName = "X127_8_测试模组位移气缸后感应"; break;
                case 8: xIoName = "X132_9_测试模组位移气缸后感应"; break;
                case 9: xIoName = "X137_10_测试模组位移气缸后感应"; break;
                case 10: xIoName = "X142_11_测试模组位移气缸后感应"; break;
                case 11: xIoName = "X147_12_测试模组位移气缸后感应"; break;
                case 12: xIoName = "X152_13_测试模组位移气缸后感应"; break;
                case 13: xIoName = "X157_14_测试模组位移气缸后感应"; break;
                case 14: xIoName = "X162_15_测试模组位移气缸后感应"; break;
                case 15: xIoName = "X167_16_测试模组位移气缸后感应"; break;
                default: return $"site:{site}不存在";
            }
            return "Success";
        }

        public string 获取下压气缸到位感应(int site, out string xIoName)
        {
            xIoName = "Fail";
            switch (site)
            {
                case 0: xIoName = "X095_1_测试模组下压感应"; break;
                case 1: xIoName = "X100_2_测试模组下压感应"; break;
                case 2: xIoName = "X105_3_测试模组下压感应"; break;
                case 3: xIoName = "X110_4_测试模组下压感应"; break;
                case 4: xIoName = "X115_5_测试模组下压感应"; break;
                case 5: xIoName = "X120_6_测试模组下压感应"; break;
                case 6: xIoName = "X125_7_测试模组下压感应"; break;
                case 7: xIoName = "X130_8_测试模组下压感应"; break;
                case 8: xIoName = "X135_9_测试模组下压感应"; break;
                case 9: xIoName = "X140_10_测试模组下压感应"; break;
                case 10: xIoName = "X145_11_测试模组下压感应"; break;
                case 11: xIoName = "X150_12_测试模组下压感应"; break;
                case 12: xIoName = "X155_13_测试模组下压感应"; break;
                case 13: xIoName = "X160_14_测试模组下压感应"; break;
                case 14: xIoName = "X165_15_测试模组下压感应"; break;
                case 15: xIoName = "X170_16_测试模组下压感应"; break;
                default: return $"site:{site}不存在";
            }
            return "Success";
        }

        public string 获取下压气缸原位感应(int site, out string xIoName)
        {
            xIoName = "Fail";
            switch (site)
            {
                case 0: xIoName = "X094_1_测试模组上升感应"; break;
                case 1: xIoName = "X099_2_测试模组上升感应"; break;
                case 2: xIoName = "X104_3_测试模组上升感应"; break;
                case 3: xIoName = "X109_4_测试模组上升感应"; break;
                case 4: xIoName = "X114_5_测试模组上升感应"; break;
                case 5: xIoName = "X119_6_测试模组上升感应"; break;
                case 6: xIoName = "X124_7_测试模组上升感应"; break;
                case 7: xIoName = "X129_8_测试模组上升感应"; break;
                case 8: xIoName = "X134_10_测试模组上升感应"; break;
                case 9: xIoName = "X139_10_测试模组上升感应"; break;
                case 10: xIoName = "X144_11_测试模组上升感应"; break;
                case 11: xIoName = "X149_12_测试模组上升感应"; break;
                case 12: xIoName = "X154_13_测试模组上升感应"; break;
                case 13: xIoName = "X159_14_测试模组上升感应"; break;
                case 14: xIoName = "X164_15_测试模组上升感应"; break;
                case 15: xIoName = "X169_16_测试模组上升感应"; break;
                default: return $"site:{site}不存在";
            }
            return "Success";
        }

        public string 测试区_位移气缸到位输出(short site)
        {
            string result = 获取位移气缸(site, out string yIoName);
            string result2 = 获取位移气缸到位感应(site, out string xIoName);
            if (result == "Success" && result2 == "Success") return 测试区_位移气缸到位输出(yIoName, xIoName);
            return result;
        }

        public string 测试区_位移气缸原位输出(short site)
        {
            string result = 获取位移气缸(site, out string yIoName);
            string result2 = 获取位移气缸原位感应(site, out string xIoName);
            if (result == "Success" && result2 == "Success") return 测试区_位移气缸原位输出(yIoName, xIoName);
            return result;
        }

        public string 测试区_下压气缸到位输出(short site)
        {
            string result = 获取下压气缸(site, out string yIoName);
            string result2 = 获取下压气缸到位感应(site, out string xIoName);
            if (result == "Success" && result2 == "Success") return 测试区_下压气缸到位输出(yIoName, xIoName);
            return result;
        }

        public string 测试区_下压气缸原位输出(short site)
        {
            string result = 获取下压气缸(site, out string yIoName);
            string result2 = 获取下压气缸原位感应(site, out string xIoName);
            if (result == "Success" && result2 == "Success") return 测试区_下压气缸原位输出(yIoName, xIoName);
            return result;
        }

        public bool CheckAircyStatus()
        {
            bool iscanstart = true;

            for (short site = 0; site < 16; site++)
            {
                //1:判断测试模组上升感应是否亮起
                string result = 测试区_位移气缸原位信号读取(site);
                if (result != "Success")
                {
                    iscanstart = false;
                    Listcode.Add($"10{site + 14:D2}001");
                    RunLog($"[Code]10{site + 14:D2}001,[Msg](按下开始)site{site + 1}升降气缸未在上方", LogType.AlarmLog);

                }

                //2:判断测试模组后位移气缸感应是否亮起
                result = 测试区_升降气缸原位信号读取(site);
                if (result != "Success")
                {
                    iscanstart = false;
                    Listcode.Add($"10{site + 30:D2}001");
                    RunLog($"[Code]10{site + 30:D2}001,[Msg](按下开始)site{site + 1}前后气缸未在后方", LogType.AlarmLog);
                }
            }
            return iscanstart;
        }

        #endregion

        #region 入料吸嘴

        public string 入料吸嘴_吸放料(int znozzle, bool ison, bool value)
        {
            if (ison)
            {
                string[] ioNames = { "Y056_上料1_吸真空", "Y057_上料2_吸真空", "Y058_上料3_吸真空", "Y059_上料4_吸真空" };
                string result = motionEx.SetDo(ioNames[znozzle], value);
                if (result != "Success") RunLog($"入料吸嘴Z{znozzle + 1}_吸真空IO输出失败!" + result);
                return result;
            }
            else
            {
                string[] ioNames = { "Y060_上料1_破真空", "Y061_上料2_破真空", "Y062_上料3_破真空", "Y063_上料4_破真空" };
                string result = motionEx.SetDo(ioNames[znozzle], value);
                if (result != "Success") RunLog($"入料吸嘴Z{znozzle + 1}_破真空IO输出失败!" + result);
                return result;
            }
        }

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
            if (result != "Success") RunLog("入料吸嘴_吸真空IO输出失败!" + result);
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
            if (result != "Success") RunLog("入料吸嘴_吸真空IO输出失败!" + result);
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
            if (result != "Success") RunLog("入料吸嘴_破真空IO输出失败!" + result);
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
            if (result != "Success") RunLog("入料吸嘴_真空检测IO读取失败!" + result);
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
                RunLog($"入料Z回原：{home[0]} {home[1]} {home[2]} {home[3]}");
                return result;
            }
            return "Success";
        }

        #endregion

        #region 出料吸嘴

        public string 出料吸嘴_吸放料(int znozzle, bool ison, bool value)
        {
            if (ison)
            {
                string[] ioNames = { "Y068_下料1_吸真空", "Y069_下料2_吸真空", "Y070_下料3_吸真空", "Y071_下料4_吸真空" };
                string result = motionEx.SetDo(ioNames[znozzle], value);
                if (result != "Success") RunLog($"出料吸嘴Z{znozzle + 1}_吸真空IO输出失败!" + result);
                return result;
            }
            else
            {
                string[] ioNames = { "Y072_下料1_破真空", "Y073_下料2_破真空", "Y074_下料3_破真空", "Y075_下料4_破真空" };
                string result = motionEx.SetDo(ioNames[znozzle], value);
                if (result != "Success") RunLog($"出料吸嘴Z{znozzle + 1}_破真空IO输出失败!" + result);
                return result;
            }
        }

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
            if (result != "Success") RunLog($"出料吸嘴_吸放料({isopen},{ison})" + result);
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
            if (result != "Success") RunLog("出料吸嘴_吸真空IO输出失败!" + result);
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
            if (result != "Success") RunLog("出料吸嘴_破真空IO输出失败!" + result);
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
            if (result != "Success") RunLog("出料吸嘴_真空检测IO读取失败!" + result);
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
                RunLog($"出料Z回原：{home[0]} {home[1]} {home[2]} {home[3]}");
                return result;
            }
            return "Success";
        }

        #endregion

        #region 测试吸嘴

        public string 测试吸嘴_吸放料(int znozzle, bool ison, bool value)
        {
            if (ison)
            {
                string[] ioNames = { "Y078_测试1_吸真空", "Y079_测试2_吸真空", "Y080_测试3_吸真空", "Y081_测试4_吸真空", "Y082_测试5_吸真空", "Y083_测试6_吸真空", "Y084_测试7_吸真空", "Y085_测试8_吸真空" };
                string result = motionEx.SetDo(ioNames[znozzle], value);
                if (result != "Success") RunLog($"测试吸嘴Z{znozzle + 1}_吸真空IO输出失败!" + result);
                return result;
            }
            else
            {
                string[] ioNames = { "Y086_测试1_破真空", "Y087_测试2_破真空", "Y088_测试3_破真空", "Y089_测试4_破真空", "Y090_测试5_破真空", "Y091_测试6_破真空", "Y092_测试7_破真空", "Y093_测试8_破真空" };
                string result = motionEx.SetDo(ioNames[znozzle], value);
                if (result != "Success") RunLog($"测试吸嘴Z{znozzle + 1}_破真空IO输出失败!" + result);
                return result;
            }
        }

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
            if (result != "Success") RunLog("测试吸嘴_吸真空IO输出失败!" + result);
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
            if (result != "Success") RunLog("测试吸嘴_吸真空IO输出失败!" + result);
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
            if (result != "Success") RunLog("测试吸嘴_破真空IO输出失败!" + result);
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
            if (result != "Success") RunLog("测试吸嘴_真空检测IO读取失败!" + result);
            return result;
        }

        public string 测试吸嘴_回原检测(out string[] home)
        {
            string[] axisNames = { "M19_测试手臂Z轴" };

            string result = motionEx.GetDiHome(axisNames, out home);

            if (result != "Success")
            {
                RunLog($"测试Z回原：{home[0]} ");
                return result;
            }
            return "Success";
        }

        #endregion

        #region 飞梭前后检测产品感应

        public string 飞梭_前后检测产品感应(out string[] iovalues)
        {
            List<string> ioNames = new List<string>();

            ioNames.Add("X079_飞梭后检测产品感应4");
            ioNames.Add("X078_飞梭后检测产品感应3");
            ioNames.Add("X077_飞梭后检测产品感应2");
            ioNames.Add("X076_飞梭后检测产品感应1");
            ioNames.Add("X083_飞梭前检测产品感应4");
            ioNames.Add("X082_飞梭前检测产品感应3");
            ioNames.Add("X081_飞梭前检测产品感应2");
            ioNames.Add("X080_飞梭前检测产品感应1");

            string result = motionEx.GetDi(ioNames.ToArray(), out iovalues);
            if (result != "Success") RunLog("飞梭前后_检测产品感应IO读取失败!" + result);
            return result;
        }

        public void 飞梭_前后放歪报警(string[] iovalues)
        {
            if (飞梭2位置 == 0)
            {
                if (iovalues[0] == "0")
                {
                    Listcode.Add("5470004"); //梭车2-后-左边1号位IC放歪
                    Listcode.Add("5470005"); //梭车2-后-右边1号位IC放歪
                    RunLog($"[Code]5470004,[Msg]梭车2-后-左边1号位IC放歪", LogType.AlarmLog);
                    RunLog($"[Code]5470005,[Msg]梭车2-后-右边1号位IC放歪", LogType.AlarmLog);
                }
                if (iovalues[1] == "0")
                {
                    Listcode.Add("5480004"); //梭车2-后-左边2号位IC放歪
                    Listcode.Add("5480005"); //梭车2-后-右边2号位IC放歪
                    RunLog($"[Code]5480004,[Msg]梭车2-后-左边2号位IC放歪", LogType.AlarmLog);
                    RunLog($"[Code]5480005,[Msg]梭车2-后-右边2号位IC放歪", LogType.AlarmLog);
                }
                if (iovalues[2] == "0")
                {
                    Listcode.Add("5490004"); //梭车2-后-左边3号位IC放歪
                    Listcode.Add("5490005"); //梭车2-后-右边3号位IC放歪
                    RunLog($"[Code]5490004,[Msg]梭车2-后-左边3号位IC放歪", LogType.AlarmLog);
                    RunLog($"[Code]5490005,[Msg]梭车2-后-右边3号位IC放歪", LogType.AlarmLog);
                }
                if (iovalues[3] == "0")
                {
                    Listcode.Add("5500004"); //梭车2-后-左边4号位IC放歪
                    Listcode.Add("5500005"); //梭车2-后-右边4号位IC放歪
                    RunLog($"[Code]5500004,[Msg]梭车2-后-左边4号位IC放歪", LogType.AlarmLog);
                    RunLog($"[Code]5500005,[Msg]梭车2-后-右边4号位IC放歪", LogType.AlarmLog);
                }
                if (iovalues[4] == "0")
                {
                    Listcode.Add("5430000"); //梭车1-前-左边1号位IC放歪
                    Listcode.Add("5430001"); //梭车1-前-右边1号位IC放歪
                    RunLog($"[Code]5430000,[Msg]梭车1-前-左边1号位IC放歪", LogType.AlarmLog);
                    RunLog($"[Code]5430001,[Msg]梭车1-前-右边1号位IC放歪", LogType.AlarmLog);
                }
                if (iovalues[5] == "0")
                {
                    Listcode.Add("5440000"); //梭车1-前-左边2号位IC放歪
                    Listcode.Add("5440001"); //梭车1-前-右边2号位IC放歪
                    RunLog($"[Code]5440000,[Msg]梭车1-前-左边2号位IC放歪", LogType.AlarmLog);
                    RunLog($"[Code]5440001,[Msg]梭车1-前-右边2号位IC放歪", LogType.AlarmLog);
                }
                if (iovalues[6] == "0")
                {
                    Listcode.Add("5450000"); //梭车1-前-左边3号位IC放歪
                    Listcode.Add("5450001"); //梭车1-前-右边3号位IC放歪
                    RunLog($"[Code]5450000,[Msg]梭车1-前-左边3号位IC放歪", LogType.AlarmLog);
                    RunLog($"[Code]5450001,[Msg]梭车1-前-右边3号位IC放歪", LogType.AlarmLog);
                }
                if (iovalues[7] == "0")
                {
                    Listcode.Add("5460000"); //梭车1-前-左边4号位IC放歪
                    Listcode.Add("5460001"); //梭车1-前-右边4号位IC放歪
                    RunLog($"[Code]5460000,[Msg]梭车1-前-左边4号位IC放歪", LogType.AlarmLog);
                    RunLog($"[Code]5460001,[Msg]梭车1-前-右边4号位IC放歪", LogType.AlarmLog);
                }
            }
            else
            {
                if (iovalues[0] == "0")
                {
                    Listcode.Add("5470002"); //梭车1-后-左边1号位IC放歪
                    Listcode.Add("5470003"); //梭车1-后-右边1号位IC放歪
                    RunLog($"[Code]5470002,[Msg]梭车1-后-左边1号位IC放歪", LogType.AlarmLog);
                    RunLog($"[Code]5470003,[Msg]梭车1-后-右边1号位IC放歪", LogType.AlarmLog);
                }
                if (iovalues[1] == "0")
                {
                    Listcode.Add("5480002"); //梭车1-后-左边2号位IC放歪
                    Listcode.Add("5480003"); //梭车1-后-右边2号位IC放歪
                    RunLog($"[Code]5480002,[Msg]梭车1-后-左边2号位IC放歪", LogType.AlarmLog);
                    RunLog($"[Code]5480003,[Msg]梭车1-后-右边2号位IC放歪", LogType.AlarmLog);
                }
                if (iovalues[2] == "0")
                {
                    Listcode.Add("5490002"); //梭车1-后-左边3号位IC放歪
                    Listcode.Add("5490003"); //梭车1-后-右边3号位IC放歪
                    RunLog($"[Code]5490002,[Msg]梭车1-后-左边3号位IC放歪", LogType.AlarmLog);
                    RunLog($"[Code]5490003,[Msg]梭车1-后-右边3号位IC放歪", LogType.AlarmLog);
                }
                if (iovalues[3] == "0")
                {
                    Listcode.Add("5500002"); //梭车1-后-左边4号位IC放歪
                    Listcode.Add("5500003"); //梭车1-后-右边4号位IC放歪
                    RunLog($"[Code]5500002,[Msg]梭车1-后-左边4号位IC放歪", LogType.AlarmLog);
                    RunLog($"[Code]5500003,[Msg]梭车1-后-右边4号位IC放歪", LogType.AlarmLog);
                }
                if (iovalues[4] == "0")
                {
                    Listcode.Add("5430002"); //梭车2-前-左边1号位IC放歪
                    Listcode.Add("5430003"); //梭车2-前-右边1号位IC放歪
                    RunLog($"[Code]5430002,[Msg]梭车2-前-左边1号位IC放歪", LogType.AlarmLog);
                    RunLog($"[Code]5430003,[Msg]梭车2-前-右边1号位IC放歪", LogType.AlarmLog);
                }
                if (iovalues[5] == "0")
                {
                    Listcode.Add("5440002"); //梭车2-前-左边2号位IC放歪
                    Listcode.Add("5440003"); //梭车2-前-右边2号位IC放歪
                    RunLog($"[Code]5440002,[Msg]梭车2-前-左边2号位IC放歪", LogType.AlarmLog);
                    RunLog($"[Code]5440003,[Msg]梭车2-前-右边2号位IC放歪", LogType.AlarmLog);
                }
                if (iovalues[6] == "0")
                {
                    Listcode.Add("5450002"); //梭车2-前-左边3号位IC放歪
                    Listcode.Add("5450003"); //梭车2-前-右边3号位IC放歪
                    RunLog($"[Code]5450002,[Msg]梭车2-前-左边3号位IC放歪", LogType.AlarmLog);
                    RunLog($"[Code]5450003,[Msg]梭车2-前-右边3号位IC放歪", LogType.AlarmLog);
                }
                if (iovalues[7] == "0")
                {
                    Listcode.Add("5460002"); //梭车2-前-左边4号位IC放歪
                    Listcode.Add("5460003"); //梭车2-前-右边4号位IC放歪
                    RunLog($"[Code]5460002,[Msg]梭车2-前-左边4号位IC放歪", LogType.AlarmLog);
                    RunLog($"[Code]5460003,[Msg]梭车2-前-右边4号位IC放歪", LogType.AlarmLog);
                }
            }
        }

        #endregion

        #region 测试区左右对射感应

        public string 测试区_左右对射感应(int site, out string[] iovalues)
        {
            string[] ioNames;
            iovalues = null;
            switch (site)
            {
                case 0: case 1: case 2: case 3: ioNames = new string[] { "X051_1_4_模组左右对射感应", "X050_1_3_模组左右对射感应", "X049_1_2_模组左右对射感应", "X048_1_1_模组左右对射感应" }; break;
                case 4: case 5: case 6: case 7: ioNames = new string[] { "X055_2_4_模组左右对射感应", "X054_2_3_模组左右对射感应", "X053_2_2_模组左右对射感应", "X052_2_1_模组左右对射感应" }; break;
                case 8: case 9: case 10: case 11: ioNames = new string[] { "X059_3_4_模组左右对射感应", "X058_3_3_模组左右对射感应", "X057_3_2_模组左右对射感应", "X056_3_1_模组左右对射感应" }; break;
                case 12: case 13: case 14: case 15: ioNames = new string[] { "X063_4_4_模组左右对射感应", "X062_4_3_模组左右对射感应", "X061_4_2_模组左右对射感应", "X060_4_1_模组左右对射感应" }; break;
                default: return $"site:{site}不存在";
            }

            string result = motionEx.GetDi(ioNames, out iovalues);
            if (result != "Success") RunLog("测试区左右对射感应IO读取失败!" + result);
            return result;
        }

        public string 测试区_上下对射感应(int site, out string[] iovalues)
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
            if (result != "Success") RunLog("测试区上下对射感应IO读取失败!" + result);
            return result;
        }

        #endregion

        #region 门禁

        public string 门禁感应(out string[] iovalues)
        {
            List<string> ioNames = new List<string>();

            ioNames.Add("X012_前门禁");
            ioNames.Add("X013_后门禁");
            ioNames.Add("X014_左门禁");
            ioNames.Add("X015_右门禁");

            string result = motionEx.GetDi(ioNames.ToArray(), out iovalues);
            if (result != "Success") RunLog("门禁感应IO读取失败!" + result);
            return result;
        }

        #endregion

        #region 加热模块

        public string 加热盘加热开启()
        {
            string result = motionEx.SetDo("Y095_加热盘加热", true);
            if (result != "Success")
            {
                RunLog("加热盘加热开启失败！" + result);
                return result;
            }
            return result;
        }

        public string 加热盘加热关闭()
        {
            string result = motionEx.SetDo("Y095_加热盘加热", false);
            if (result != "Success")
            {
                RunLog("加热盘加热关闭失败！" + result);
                return result;
            }
            return result;
        }

        public string 梭车加热开启()
        {
            string result = motionEx.SetDo("Y151_梭车加热", true);
            if (result != "Success")
            {
                RunLog("梭车加热打开失败！" + result);
                return result;
            }
            return result;
        }

        public string 梭车加热关闭()
        {
            string result = motionEx.SetDo("Y151_梭车加热", false);
            if (result != "Success")
            {
                RunLog("梭车加热关闭失败！" + result);
                return result;
            }
            return result;
        }

        public string 获取Site加热IO(int site, out string ioName)
        {
            ioName = "";
            switch (site)
            {
                case 0: ioName = "Y128_Site1加热"; break;
                case 1: ioName = "Y129_Site2加热"; break;
                case 2: ioName = "Y130_Site3加热"; break;
                case 3: ioName = "Y131_Site4加热"; break;
                case 4: ioName = "Y132_Site5加热"; break;
                case 5: ioName = "Y133_Site6加热"; break;
                case 6: ioName = "Y134_Site7加热 "; break;
                case 7: ioName = "Y135_Site8加热"; break;
                case 8: ioName = "Y136_Site9加热"; break;
                case 9: ioName = "Y137_Site10加热"; break;
                case 10: ioName = "Y138_Site11加热"; break;
                case 11: ioName = "Y139_Site12加热"; break;
                case 12: ioName = "Y140_Site13加热"; break;
                case 13: ioName = "Y141_Site14加热"; break;
                case 14: ioName = "Y142_Site15加热"; break;
                case 15: ioName = "Y143_Site16加热"; break;
                default: return "Fail";
            }
            return "Success";
        }

        public string Site加热开启(int site)
        {
            string result = 获取Site加热IO(site, out string ioName);
            if (result == "Success")
            {
                result = motionEx.SetDo(ioName, true);
                if (result != "Success")
                {
                    RunLog($"{ioName}开启失败！" + result);
                    return result;
                }
            }
            return result;
        }

        public string Site加热关闭(int site)
        {
            string result = 获取Site加热IO(site, out string ioName);
            if (result == "Success")
            {
                result = motionEx.SetDo(ioName, false);
                if (result != "Success")
                {
                    RunLog($"{ioName}关闭失败！" + result);
                    return result;
                }
            }
            return result;
        }

        public string Site加热开启()
        {
            for (int site = 0; site < 16; site++)
            {
                if (GetSiteOpen(site).Contains(true))
                {
                    string result = Site加热开启(site);
                    if (result != "Success") return result;
                }
            }
            return "Success";
        }

        public string Site加热关闭()
        {
            for (int site = 0; site < 16; site++)
            {
                //if (GetSiteOpen(site).Contains(true))
                //{
                string result = Site加热关闭(site);
                if (result != "Success") return result;
                //}
            }
            return "Success";
        }

        #endregion

        #region Empty区

        public string Empty_前Tray存在感应(out string value)
        {
            string result = motionEx.GetDi("X016_空Tray区对射1_前", out value);
            if (result != "Success") RunLog("X016_空Tray区对射1_前IO读取失败!" + result);
            return result;
        }

        public string Empty_后Tray放歪感应(out string value)
        {
            string result = motionEx.GetDi("X017_空Tray区对射2_后", out value);
            if (result != "Success") RunLog("X017_空Tray区对射2_后IO读取失败!" + result);
            return result;
        }

        public string Empty_门感应(out string value)
        {
            string result = motionEx.GetDi("X018_空Tray门感应", out value);
            if (result != "Success") RunLog("X018_空Tray门感应IO读取失败!" + result);
            return result;
        }

        #endregion

        #region Load区

        public string Load_Tray前侧定位气缸到位输出()
        {
            string result = motionEx.SetDo("Y018_Load区Tray前侧定位", true);
            if (result != "Success")
            {
                RunLog("Load区Tray前侧定位气缸到位输出失败！" + result);
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
                RunLog("Load区Tray前侧定位气缸原位输出失败！" + result);
                return result;
            }
            Thread.Sleep(400); //没有感应器，固定延时处理
            return result;
        }

        public string Load_前Tray存在感应(out string value)
        {
            string result = motionEx.GetDi("X022_Load区对射1_前", out value);
            if (result != "Success") RunLog("X022_Load区对射1_前IO读取失败!" + result);
            return result;
        }

        public string Load_后Tray放歪感应(out string value)
        {
            string result = motionEx.GetDi("X023_Load区对射1_后", out value);
            if (result != "Success") RunLog("X023_Load区对射1_后IO读取失败!" + result);
            return result;
        }

        public string Load_门感应(out string value)
        {
            string result = motionEx.GetDi("X024_Load门感应", out value);
            if (result != "Success") RunLog("X024_Load门感应IO读取失败!" + result);
            return result;
        }

        #endregion

        #region Auto1区

        public string Auto1_Tray前侧定位气缸到位输出(int sleep = 300)
        {
            string result = motionEx.SetDo("Y023_AUTO1_Tray前定位", true);
            if (result != "Success")
            {
                RunLog("Auto1_Tray前定位气缸到位输出失败！" + result);
                return result;
            }

            string result2 = motionEx.SetDo("Y024_AUTO1_Tray侧定位", true);
            if (result2 != "Success")
            {
                RunLog("Auto1_Tray侧定位气缸到位输出失败！" + result2);
                return result2;
            }

            Thread.Sleep(sleep); //没有感应器，固定延时处理
            return result;
        }

        public string Auto1_Tray前侧定位气缸原位输出(int sleep = 300)
        {
            string result = motionEx.SetDo("Y023_AUTO1_Tray前定位", false);
            if (result != "Success")
            {
                RunLog("Auto1_Tray前定位气缸原位输出失败！" + result);
                return result;
            }

            string result2 = motionEx.SetDo("Y024_AUTO1_Tray侧定位", false);
            if (result2 != "Success")
            {
                RunLog("Auto1_Tray侧定位气缸原位输出失败！" + result2);
                return result2;
            }

            Thread.Sleep(sleep); //没有感应器，固定延时处理
            return result;
        }

        public string Auto1_Tray前定位气缸到位输出()
        {
            string result = motionEx.SetDo("Y023_BIN_A区Tray前定位", true);
            if (result != "Success")
            {
                RunLog("BIN_A区Tray前定位气缸到位输出失败！" + result);
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
                RunLog("BIN_A区Tray前定位气缸原位输出失败！" + result);
                return result;
            }
            Thread.Sleep(300); //没有感应器，固定延时处理
            return result;
        }

        public string Auto1_Tray侧定位气缸到位输出(int sleep = 300)
        {
            string result = motionEx.SetDo("Y024_BIN_A区Tray侧定位", true);
            if (result != "Success")
            {
                RunLog("BIN_A区Tray侧定位气缸到位输出失败！" + result);
                return result;
            }
            Thread.Sleep(sleep); //没有感应器，固定延时处理
            return result;
        }

        public string Auto1_Tray侧定位气缸原位输出(int sleep = 300)
        {
            string result = motionEx.SetDo("Y024_BIN_A区Tray侧定位", false);
            if (result != "Success")
            {
                RunLog("BIN_A区Tray侧定位气缸原位输出失败！" + result);
                return result;
            }
            Thread.Sleep(sleep); //没有感应器，固定延时处理
            return result;
        }

        public string Auto1_Tray上顶气缸到位输出()
        {
            string result = motionEx.SetDo("Y027_BIN_A区Tray上顶", true);
            if (result != "Success")
            {
                RunLog("BIN_A区Tray上顶气缸到位输出失败！" + result);
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
                RunLog("BIN_A区Tray上顶气缸原位输出失败！" + result);
                return result;
            }
            Thread.Sleep(300); //没有感应器，固定延时处理
            return result;
        }

        public string Auto1_前Tray存在感应(out string value)
        {
            string result = motionEx.GetDi("X034_BIN_A区对射1_前", out value);
            if (result != "Success") RunLog("X034_Auto1对射1_前IO读取失败!" + result);
            return result;
        }

        public string Auto1_后Tray放歪感应(out string value)
        {
            string result = motionEx.GetDi("X035_BIN_A区对射2_后", out value);
            if (result != "Success") RunLog("X035_Auto1对射2_后IO读取失败!" + result);
            return result;
        }

        public string Auto1_门感应(out string value)
        {
            string result = motionEx.GetDi("X036_BIN_A门感应", out value);
            if (result != "Success") RunLog("X036_Auto1门感应IO读取失败!" + result);
            return result;
        }

        #endregion

        #region Auto2

        public string Auto2_Tray前侧定位气缸到位输出(int sleep = 300)
        {
            string result = motionEx.SetDo("Y025_AUTO2_Tray前定位", true);
            if (result != "Success")
            {
                RunLog("Auto2_Tray前定位气缸到位输出失败！" + result);
                return result;
            }

            string result2 = motionEx.SetDo("Y026_AUTO2_Tray侧定位", true);
            if (result2 != "Success")
            {
                RunLog("Auto2_Tray侧定位气缸到位输出失败！" + result2);
                return result2;
            }

            Thread.Sleep(sleep); //没有感应器，固定延时处理
            return result;
        }

        public string Auto2_Tray前侧定位气缸原位输出(int sleep = 300)
        {
            string result = motionEx.SetDo("Y025_AUTO2_Tray前定位", false);
            if (result != "Success")
            {
                RunLog("Auto2_Tray前定位气缸原位输出失败！" + result);
                return result;
            }

            string result2 = motionEx.SetDo("Y026_AUTO2_Tray侧定位", false);
            if (result2 != "Success")
            {
                RunLog("Auto2_Tray侧定位气缸原位输出失败！" + result2);
                return result2;
            }

            Thread.Sleep(sleep); //没有感应器，固定延时处理
            return result;
        }

        public string Auto2_Tray前定位气缸到位输出()
        {
            string result = motionEx.SetDo("Y025_BIN_B区Tray前定位", true);
            if (result != "Success")
            {
                RunLog("BIN_B区Tray前定位气缸到位输出失败！" + result);
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
                RunLog("BIN_B区Tray前定位气缸原位输出失败！" + result);
                return result;
            }
            Thread.Sleep(300); //没有感应器，固定延时处理
            return result;
        }

        public string Auto2_Tray侧定位气缸到位输出(int sleep = 300)
        {
            string result = motionEx.SetDo("Y026_BIN_B区Tray侧定位", true);
            if (result != "Success")
            {
                RunLog("BIN_B区Tray侧定位气缸到位输出失败！" + result);
                return result;
            }
            Thread.Sleep(sleep); //没有感应器，固定延时处理
            return result;
        }

        public string Auto2_Tray侧定位气缸原位输出(int sleep = 300)
        {
            string result = motionEx.SetDo("Y026_BIN_B区Tray侧定位", false);
            if (result != "Success")
            {
                RunLog("BIN_B区Tray侧定位气缸原位输出失败！" + result);
                return result;
            }
            Thread.Sleep(sleep); //没有感应器，固定延时处理
            return result;
        }

        public string Auto2_Tray上顶气缸到位输出()
        {
            string result = motionEx.SetDo("Y032_BIN_B区Tray上顶", true);
            if (result != "Success")
            {
                RunLog("BIN_B区Tray上顶气缸到位输出失败！" + result);
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
                RunLog("BIN_B区Tray上顶气缸原位输出失败！" + result);
                return result;
            }
            Thread.Sleep(300); //没有感应器，固定延时处理
            return result;
        }

        public string Auto2_前Tray存在感应(out string value)
        {
            string result = motionEx.GetDi("X040_BIN_B区对射1_前", out value);
            if (result != "Success") RunLog("X040_Auto2对射1_前IO读取失败!" + result);
            return result;
        }

        public string Auto2_后Tray放歪感应(out string value)
        {
            string result = motionEx.GetDi("X041_BIN_B区对射2_后", out value);
            if (result != "Success") RunLog("X041_Auto2对射2_后IO读取失败!" + result);
            return result;
        }

        public string Auto2_门感应(out string value)
        {
            string result = motionEx.GetDi("X042_BIN_B门感应", out value);
            if (result != "Success") RunLog("X042_Auto2门感应IO读取失败!" + result);
            return result;
        }

        #endregion

        public string Fix1_Tray感应(out string value)
        {
            string result = motionEx.GetDi("X046_BIN3区Tray感应", out value);
            if (result != "Success") RunLog("X046_Fix1_Tray感应IO读取失败!" + result);
            return result;
        }

        public string Fix2_Tray感应(out string value)
        {
            string result = motionEx.GetDi("X047_BIN4区Tray感应", out value);
            if (result != "Success") RunLog("X047_Fix2_Tray感应IO读取失败!" + result);
            return result;
        }

        public string Fix3_Tray感应(out string value)
        {
            string result = motionEx.GetDi("X045_BIN5区Tray感应", out value);
            if (result != "Success") RunLog("X045_Fix3_Tray感应IO读取失败!" + result);
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
            if (result != "Success") RunLog("飞梭前后_检测产品感应IO读取失败!" + result);
            return result;
        }

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

        public void HandlerDebug()
        {
            int site = 12;
            string result = "Success";
            bool[] lsopen = new bool[8] { true, true, true, true, true, true, true, true };
            int[] sites = GetSites(site);
            StationManagers.SetStationIC(site, new bool[][]
            {
                                        new bool[] { true, lsopen[1], lsopen[2], lsopen[3] },
                                        new bool[] { lsopen[4], lsopen[5], lsopen[6], lsopen[7] }
            });
            StationManagers.SetStationIC(site + 1, new bool[][]
            {
                                        new bool[] { true, lsopen[1], lsopen[2], lsopen[3] },
                                        new bool[] { lsopen[4], lsopen[5], lsopen[6], lsopen[7] }
            });
            bool[][] siteic1 = StationManagers.GetStationIC((int)sites[0]);
            bool[][] siteic2 = StationManagers.GetStationIC((int)sites[1]);
            bool[] site1 = new bool[] { siteic1[0][0], siteic1[0][1], siteic1[0][2], siteic1[0][3], siteic1[1][0], siteic1[1][1], siteic1[1][2], siteic1[1][3] };
            bool[] site2 = new bool[] { siteic2[0][0], siteic2[0][1], siteic2[0][2], siteic2[0][3], siteic2[1][0], siteic2[1][1], siteic2[1][2], siteic2[1][3] };
            int[] states = new int[] { 10, 10 };

            bool[] sitew1 = GetSiteOpen(sites[0]);
            bool[] sitew2 = GetSiteOpen(sites[1]);

            if (((states[0] == 10 || !sitew1.Contains(true)) && (states[1] == 10 || !sitew2.Contains(true))) || (测试手臂入料停止工作标记 && (结批标记 || 一个循环标记) && (states[0] == 10 || states[0] == 0) && (states[1] == 10 || states[1] == 0)))
            {
                List<List<bool[]>> opens = new List<List<bool[]>>()
                                    {
                                        states[0] != 0 ? GetOpens(site1) : new List<bool[]>() { new bool[] { false, false, false, false }, new bool[] { false, false, false, false } },
                                        states[1] != 0 ? GetOpens(site2) : new List<bool[]>() { new bool[] { false, false, false, false }, new bool[] { false, false, false, false } }
                                    };
                //result = APPmaster.Com.Send(sites, opens, 无测试机模式);
                Thread.Sleep(设置信息.测试机信息.发送开始信号延时);
                RunLog($"PS测试区气缸下压动作流程.StepID={PS测试区气缸下压动作流程.StepID}->{PS测试区气缸下压动作流程.StepName}");
                result = StationManagers.Send(site, states, opens, 无测试机模式);
                if (result == "Success")
                {
                    if (states[0] == 10) StationManagers.SetStationState(sites[0], 2);
                    if (states[1] == 10) StationManagers.SetStationState(sites[1], 2);
                    PS测试区气缸下压动作流程.StepID = 599;
                }
                else
                {
                    Listcode.Add("800007");
                    RunLog($"[Code]800007,[Msg]发送Site{site + 1}测试信号失败!" + result, LogType.AlarmLog);
                    ProgramPause = true;
                    IsPause = true;
                }
            }
            else PS测试区气缸下压动作流程.StepID = 599;
        }

        #endregion IO控制与读取

        #region 辅助方法

        public void 参数初始化()
        {
            //温度模式 = 0;
            设备状态 = 0;
            温控达温 = false;
            初次达温 = false;
            H5509归零 = false;
            H5509正在归零 = false;
            H5509初始化 = false;
            H5509正在初始化 = false;
            结批标记 = false;
            正在结批 = false;
            结批完成 = false;
            强制结批标记 = false;
            一个循环标记 = false;
            一个循环中 = false;
            一个循环完成 = false;
            结批选项选择中 = false;
            一个循环选项选择中 = false;
            后方定点停止允许 = false;
            前方定点停止允许 = false;
            开舱门上顶标记 = false;
            Load舱门上顶标记 = false;
            Empty舱门上顶标记 = false;
            Auto1舱门上顶标记 = false;
            Auto2舱门上顶标记 = false;
            Load舱门开启标记 = 0;
            Empty舱门开启标记 = 0;
            Auto1舱门开启标记 = 0;
            Auto2舱门开启标记 = 0;
            Empty下降回零开关 = false;
            Load下降回零开关 = false;
            Auto1下降回零开关 = false;
            Auto2下降回零开关 = false;
            Empty正在下降回零 = false;
            Load正在下降回零 = false;
            Auto1正在下降回零 = false;
            Auto2正在下降回零 = false;
            Empty舱门正在上顶 = false;
            Load舱门正在上顶 = false;
            Auto1舱门正在上顶 = false;
            Auto2舱门正在上顶 = false;
            Load换盘标记 = false;
            Auto1换盘标记 = false;
            Auto2换盘标记 = false;
            Auto等待换盘标记 = false;

            按了跳过或编辑标记 = false;
            门已打开超设定时间 = false;
            等待Load门开启 = false;

            保存Auto1放料个数 = 0;
            保存Auto2放料个数 = 0;
            出料正在Auto1放料 = false;
            出料正在Auto2放料 = false;
            Auto1正在侧敲 = false;
            Auto2正在侧敲 = false;

            Fix1Tray盘放置标记 = false;
            Fix1Tray盘拿走标记 = false;
            Fix2Tray盘放置标记 = false;
            Fix2Tray盘拿走标记 = false;
            Fix3Tray盘放置标记 = false;
            Fix3Tray盘拿走标记 = false;
            总良率报警间隔计数 = 0;
            Socket使用次数超限数 = 0;
            报警次数 = 0;
            正在入料的Site = 99;
            trayTemp = "";
            Dout128PassNumber = new int[128];
            Dout128FailNumber = new int[128];
            SiteTestState = new int[16];
            doutContinueFail = new List<int[]>();
            单dut总不良计数 = new List<int[]>();
            单dut总不良累计计数 = new List<int[]>();
            //SiteTestIC = new List<bool[][]>();
            //SiteTestBin = new List<string[]>();
            for (int i = 0; i < 16; i++)
            {
                //Com.Iswaitout[i] = false;
                //Com.Isreceive[i] = false;
                StationManagers.SetStationState(i, 0);
                StationManagers.SetStationIC(i, new bool[2][] { new bool[] { false, false, false, false }, new bool[] { false, false, false, false } });
                StationManagers.SetStationBin(i, new string[] { "", "", "", "", "", "", "", "" });
                //SiteTestIC.Add(new bool[2][] { new bool[] { false, false, false, false }, new bool[] { false, false, false, false } });
                doutContinueFail.Add(new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });
                单dut总不良计数.Add(new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });
                单dut总不良累计计数.Add(new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });
                //SiteTestBin.Add(new string[] { "", "", "", "", "", "", "", "" });
                //显示Dout颜色(i, SiteTestIC[i], SiteTestBin[i]);
            }

            for (int i = 0; i < SocketSaves.Length; i++)
            {
                SocketSaves[i] = new SocketSave()
                {
                    Auto1数量 = 0,
                    Auto2数量 = 0,
                    Fix1数量 = 0,
                    Fix2数量 = 0,
                    Fix3数量 = 0,
                };
            }

            入料IC忽略 = false;
            入料IC吸不起结批 = false;
            无待测盘结批 = false;
            出料IC忽略 = false;
            测试IC忽略 = false;
            测试区IC忽略 = false;
            //Com.IsconnectTester = false;
            //BinSaveQueue = new ConcurrentQueue<string[]>();
            InSiteQueue = new ConcurrentQueue<int>();
            //OutSiteQueue = new ConcurrentQueue<int>();

            OutSiteQueue = new ConcurrentQueue<(int SiteId, TestDataInfo Data)>();
            OutSiteQueue2 = new ConcurrentQueue<(int SiteId, TestDataInfo Data)>();
            载入数 = 0;
            UphNumber = 0;
            Auto1数量 = 0;
            Auto2数量 = 0;
            Fix1数量 = 0;
            Fix2数量 = 0;
            Fix3数量 = 0;
            OtherNum = 0;
            HeatTrayClear();
            Auto1TrayClear();
            Auto2TrayClear();
            Fix1TrayClear();
            Fix2TrayClear();
            Fix3TrayClear();
            空盘上顶到最大值 = false;
            空盘下降到最小值 = false;
            Auto1Tray盘数量 = 0;
            Auto2Tray盘数量 = 0;
            测试手臂吸不起数量 = 0;
            //测试区吸不起数量 = 0;
            入料手臂吸料不起数量 = 0;
            出料手臂吸料不起数量 = 0;
            Auto1料仓满盘标记 = false;
            Auto2料仓满盘标记 = false;
            Auto1放盘等待标记 = false;
            Auto2放盘等待标记 = false;
            Empty料仓打开标记 = false;
            Load料仓打开标记 = false;
            Auto1料仓打开标记 = false;
            Auto2料仓打开标记 = false;
            入料手臂工作完成标记 = false;
            加热盘停止入料标记 = false;
            入料手臂停止工作标记 = false;
            测试手臂入料停止工作标记 = false;
            测试手臂出料停止工作标记 = false;
            出料手臂出料停止工作标记 = false;
            飞梭停止工作 = false;
            出料手臂工作完成标记 = false;
            测试手臂工作完成标记 = false;
            入料手臂抓料等待标记 = false;
            飞梭移动状态 = 0;
            飞梭前入料状态 = 0;
            飞梭前出料状态 = 0;
            飞梭后入料状态 = 0;
            飞梭后出料状态 = 0;
            Empty上顶标记 = 1;
            Load上顶标记 = 1;
            Auto1上顶标记 = 1;
            Auto2上顶标记 = 1;
            Empty下降标记 = 1;
            Load下降标记 = 1;
            Empty下降回零标记 = 1;
            Load下降回零标记 = 1;
            Auto1下降回零标记 = 1;
            Auto2下降回零标记 = 1;
            抓盘手臂取Empty空盘标记 = 1;
            抓盘手臂取Load空盘标记 = 1;
            抓盘手臂给Auto1空盘标记 = 1;
            抓盘手臂给Auto2空盘标记 = 1;
            抓盘手臂放空盘到Empty标记 = 1;
            抓盘手臂放空盘到Auto1标记 = 1;
            抓盘手臂放空盘到Auto2标记 = 1;
            飞梭前出料BIN项 = new string[2][];
            飞梭后出料BIN项 = new string[2][];

            InNozzleIC = false;
            InNozzleICOpen = new bool[4];
            OutNozzleIC = false;
            OutNozzleICOpen = new bool[4];
            TestNozzleIC = false;
            TestNozzleICOpen = new bool[8];
            正在吸取后入料 = new bool[] { false, false, false, false, false, false, false, false };
            正在放置前入料 = new bool[][] { new bool[] { false, false, false, false }, new bool[] { false, false, false, false } };
            //放料IC目标值 = new bool[][] { new bool[] { false, false, false, false }, new bool[] { false, false, false, false } };
            飞梭前入料IC = new bool[][] { new bool[] { false, false, false, false }, new bool[] { false, false, false, false } };
            飞梭前出料IC = new bool[][] { new bool[] { false, false, false, false }, new bool[] { false, false, false, false } };
            飞梭后入料IC = new bool[][] { new bool[] { false, false, false, false }, new bool[] { false, false, false, false } };
            飞梭后出料IC = new bool[][] { new bool[] { false, false, false, false }, new bool[] { false, false, false, false } };

            PS初始化流程.StepID = 500;
            PS初始化流程.Processing = false;
            PS结批流程.StepID = 500;
            PS结批流程.Processing = false;
            PS入料手臂吸放料流程.StepID = 500;
            PS入料手臂吸放料流程.Processing = false;
            PS出料手臂吸放料流程.StepID = 500;
            PS出料手臂吸放料流程.Processing = false;
            PS入飞梭移动流程.StepID = 500;
            PS入飞梭移动流程.Processing = false;
            PS测试手臂取待测料流程.StepID = 500;
            PS测试手臂取待测料流程.Processing = false;
            PS测试手臂取已测料流程.StepID = 500;
            PS测试手臂取已测料流程.Processing = false;
            PS测试区气缸下压动作流程.StepID = 500;
            PS测试区气缸下压动作流程.Processing = false;
            PS测试区气缸上抬动作流程.StepID = 500;
            PS测试区气缸上抬动作流程.Processing = false;
            PS空盘上顶流程.StepID = 500;
            PS空盘上顶流程.Processing = false;
            PS空盘下降流程.StepID = 500;
            PS空盘下降流程.Processing = false;
            PS空盘下降回零流程.StepID = 500;
            PS空盘下降回零流程.Processing = false;
            PS待测盘上顶流程.StepID = 500;
            PS待测盘上顶流程.Processing = false;
            PS待测盘下降回零流程.StepID = 500;
            PS待测盘下降回零流程.Processing = false;
            PS手动盘1上顶流程.StepID = 500;
            PS手动盘1上顶流程.Processing = false;
            PS手动盘1下降回零流程.StepID = 500;
            PS手动盘1下降回零流程.Processing = false;
            PS手动盘2上顶流程.StepID = 500;
            PS手动盘2上顶流程.Processing = false;
            PS手动盘2下降回零流程.StepID = 500;
            PS手动盘2下降回零流程.Processing = false;
            PS抓盘手臂取Empty空盘流程.StepID = 500;
            PS抓盘手臂取Empty空盘流程.Processing = false;
            PS抓盘手臂取Load空盘流程.StepID = 500;
            PS抓盘手臂取Load空盘流程.Processing = false;
            PS抓盘手臂放空盘到Empty流程.StepID = 500;
            PS抓盘手臂放空盘到Empty流程.Processing = false;
            PS抓盘手臂放空盘到Auto1流程.StepID = 500;
            PS抓盘手臂放空盘到Auto1流程.Processing = false;
            PS抓盘手臂放空盘到Auto2流程.StepID = 500;
            PS抓盘手臂放空盘到Auto2流程.Processing = false;
        }

        private bool 开始检查(string[] io)
        {
            bool isstart = true;

            string result = motionEx.GetDi("X020_进气压", out string value);
            if (result == "Success")
            {
                if (value == 气压报警)
                {
                    isstart = false;
                    Listcode.Add("1400020"); //系统错误--气压不足
                    RunLog("[Code]1400020,[Msg](按下开始)整机气压异常", LogType.AlarmLog);
                }
            }
            else
            {
                isstart = false;
                Listcode.Add("1400020"); //系统错误--气压不足
                RunLog("[Code]1400020,[Msg](按下开始)气压读取失败!" + result, LogType.AlarmLog);
            }

            if (!H5509归零)
            {
                isstart = false;
                Listcode.Add("1400445");
                RunLog("[Code]1400445,[Msg](按下开始)设备未归零", LogType.AlarmLog);
            }

            if (!IsDutAllCLose()) //当初次开始时检测有没有开dut
            {
                if (!结批标记)
                {
                    isstart = false;
                    Listcode.Add("1400442");
                    RunLog("[Code]1400442,[Msg](按下开始)Dut全部被关闭，请至少开1个Dut进行测试或长按结批后再按开始", LogType.AlarmLog);
                }
            }

            if (io[0] == "0") //15
            {
                isstart = false;
                Listcode.Add("4018008");
                RunLog("[Code]4018008,[Msg](按下开始)Empty门未关闭", LogType.AlarmLog);
            }

            if (io[1] == "0")//16
            {
                isstart = false;
                Listcode.Add("4019008");
                RunLog("[Code]4019008,[Msg](按下开始)Load门未关闭", LogType.AlarmLog);
            }

            if (io[2] == "0")//18
            {
                isstart = false;
                Listcode.Add("4020008");
                RunLog("[Code]4020008,[Msg](按下开始)Auto1门未关闭", LogType.AlarmLog);
            }

            if (io[3] == "0")//19
            {
                isstart = false;
                Listcode.Add("4021008");
                RunLog("[Code]4021008,[Msg](按下开始)Auto2门未关闭", LogType.AlarmLog);
            }

            if (Tray盘模式)
            {
                if (io[4] == "0")//20
                {
                    isstart = false;
                    Listcode.Add("4030009");
                    RunLog("[Code]4030009,[Msg](按下开始)Fix1请放置空盘", LogType.AlarmLog);
                }

                if (io[5] == "0")//21
                {
                    isstart = false;
                    Listcode.Add("4031009");
                    RunLog("[Code]4031009,[Msg](按下开始)Fix2请放置空盘", LogType.AlarmLog);
                }

                if (io[6] == "1")//22
                {
                    isstart = false;
                    Listcode.Add("4032009");
                    RunLog("[Code]4032009,[Msg](按下开始)Fix3请放置空盘", LogType.AlarmLog);
                }
            }

            //if (io[7] != "0")//23
            //{
            //    isstart = false;
            //    Listcode.Add("4020009");
            //    RunLog("[Code]4020009,[Msg](按下开始)AUTO1前对射感应到无Tray盘", LogType.AlarmLog);
            //}

            //if (io[9] != "0")//25
            //{
            //    isstart = false;
            //    Listcode.Add("4021009");
            //    RunLog("[Code]4021009,[Msg](按下开始)AUTO2前对射感应到无Tray盘", LogType.AlarmLog);
            //}


            if (!安全门禁用)
            {
                result = 门禁感应(out string[] iovalue);
                if (result == "Success")
                {
                    if (iovalue[0] == "0")
                    {
                        isstart = false;
                        Listcode.Add("9000000");
                        RunLog("[Code]9000000,[Msg](按下开始)系统错误--前安全门打开", LogType.AlarmLog);
                    }
                    else if (iovalue[1] == "0")
                    {
                        isstart = false;
                        Listcode.Add("9000010");
                        RunLog("[Code]9000010,[Msg](按下开始)系统错误--后安全门打开", LogType.AlarmLog);
                    }
                    else if (iovalue[2] == "0")
                    {
                        isstart = false;
                        Listcode.Add("9000020");
                        RunLog("[Code]9000020,[Msg](按下开始)系统错误--左安全门打开", LogType.AlarmLog);
                    }
                    else if (iovalue[3] == "0")
                    {
                        isstart = false;
                        Listcode.Add("9000040");
                        RunLog("[Code]9000040,[Msg](按下开始)系统错误--右安全门打开", LogType.AlarmLog);
                    }
                }
            }

            if (!无测试机模式) //如果是在线模式
            {
                bool[] check = StationManagers.CheckComConnect();
                if (check.Contains(false))
                {
                    for (int i = 0; i < check.Length; i++)
                    {
                        if (!check[i])
                        {
                            Debug.WriteLine($"测试机{i + 1}通讯未连接");
                        }
                    }
                    isstart = false;
                    Listcode.Add("1400430");
                    RunLog("[Code]1400430,[Msg](按下开始)串口未连接");
                }

                if (设置信息.通讯信息.发送开批信息)
                {
                    check = StationManagers.CheckTesterConnect();
                    if (check.Contains(false))
                    {
                        for (int i = 0; i < check.Length; i++)
                        {
                            if (!check[i])
                            {
                                Debug.WriteLine($"未发送开批信息给测试机{i + 1}");
                            }
                        }
                        isstart = false;
                        Listcode.Add("1400440");
                        RunLog("[Code]1400440,[Msg](按下开始)未发送生产信息给测试机"); //listcode.Add("1400440");
                    }
                }
                else
                {
                    if (检查输入工单输入())
                    {
                        isstart = false;
                        Listcode.Add("1400489");
                        RunLog("[Code]1400489,[Msg](按下开始)请输入用户名,客批号,机器编号,客户,测试模式,厂内批号和批次数量"); //listcode.Add("1400440");
                    }
                }
            }
            else
            {
                if (检查输入工单输入())
                {
                    isstart = false;
                    Listcode.Add("1400489");
                    RunLog("[Code]1400489,[Msg](按下开始)请输入用户名,客批号,机器编号,客户,测试模式,厂内批号和批次数量"); //listcode.Add("1400440");
                }
            }

            //if (Com.IsOpenCom == false)
            //{
            //    isstart = false;
            //    Listcode.Add("1400430");
            //    RunLog("[Code]1400430,[Msg](按下开始)串口未连接");
            //}
            //else
            //{
            //    if (Com.IsconnectTester == false)
            //    {
            //        isstart = false;
            //        Listcode.Add("1400440");
            //        RunLog("[Code]1400430,[Msg](按下开始)未发送生产信息给测试机"); //listcode.Add("1400440");
            //    }
            //}

            if (isstart)
            {
                if (TCPConnect)
                {
                    if (温度模式 == "高温")
                    {
                        if (温控器读取_设定温度报警)
                        {
                            if (温控器读取Site1_Site8探测温度异常)
                            {
                                isstart = false;
                                Listcode.Add($"1400449");
                                //温控器读取Site1_Site8探测温度异常 = false;
                                RunLog($"[Code]1400449,[Msg](按下开始)读取Site1-Site8探测温度失败!详细信息:" + 读取Site1_Site8探测温度Error, LogType.AlarmLog);
                            }

                            if (温控器读取Site9_Site16探测温度异常)
                            {
                                isstart = false;
                                Listcode.Add($"1400450");
                                //温控器读取Site9_Site16探测温度异常 = false;
                                RunLog($"[Code]1400450,[Msg](按下开始)读取Site9-Site16探测温度失败!详细信息:" + 读取Site9_Site16探测温度Error, LogType.AlarmLog);
                            }

                            if (温控器读取加热盘探测温度异常)
                            {
                                isstart = false;
                                Listcode.Add($"1400451");
                                //温控器读取加热盘探测温度异常 = false;
                                RunLog($"[Code]1400451,[Msg](按下开始)读取加热盘探测温度失败!详细信息:" + 读取加热盘探测温度Error, LogType.AlarmLog);
                            }

                            if (温控器读取Site1_Site8设定温度异常)
                            {
                                isstart = false;
                                Listcode.Add($"1400455");
                                //温控器读取Site1_Site8设定温度异常 = false;
                                RunLog($"[Code]1400455,[Msg](按下开始)读取Site1-Site8设定温度失败!详细信息:" + 读取Site1_Site8设定温度Error, LogType.AlarmLog);
                            }

                            if (温控器读取Site9_Site16设定温度异常)
                            {
                                isstart = false;
                                Listcode.Add($"1400456");
                                //温控器读取Site9_Site16设定温度异常 = false;
                                RunLog($"[Code]1400456,[Msg](按下开始)读取Site9-Site16设定温度失败!详细信息:" + 读取Site9_Site16设定温度Error, LogType.AlarmLog);
                            }

                            if (温控器读取加热盘设定温度异常)
                            {
                                isstart = false;
                                Listcode.Add($"1400457");
                                //温控器读取加热盘设定温度异常 = false;
                                RunLog($"[Code]1400457,[Msg](按下开始)读取加热盘设定温度失败!详细信息:" + 读取加热盘设定温度Error, LogType.AlarmLog);
                            }

                            if (温控器设定Site1_Site8温度异常)
                            {
                                isstart = false;
                                Listcode.Add($"1400452");
                                //温控器设定Site1_Site8温度异常 = false;
                                RunLog($"[Code]1400452,[Msg](按下开始)设定Site1-Site8温度失败!详细信息:" + 设定Site1_Site8温度Error, LogType.AlarmLog);
                            }

                            if (温控器设定Site9_Site16温度异常)
                            {
                                isstart = false;
                                Listcode.Add($"1400453");
                                //温控器设定Site9_Site16温度异常 = false;
                                RunLog($"[Code]1400453,[Msg](按下开始)设定Site9-Site16温度失败!详细信息:" + 设定Site9_Site16温度Error, LogType.AlarmLog);
                            }

                            if (温控器设定加热盘温度异常)
                            {
                                isstart = false;
                                Listcode.Add($"1400454");
                                //温控器设定加热盘温度异常 = false;
                                RunLog($"[Code]1400454,[Msg](按下开始)设定加热盘温度失败!详细信息:" + 设定加热盘温度Error, LogType.AlarmLog);
                            }
                        }

                        for (int i = 0; i < SiteSvTemp.Length; i++)
                        {
                            if (SiteSvTemp[i] != 设定温度)
                            {
                                //if (设定温控器温度() != "Success")
                                isstart = false;
                                Listcode.Add("1400447");
                                RunLog("[Code]1400447,[Msg](按下开始)设定温度失败", LogType.AlarmLog);
                                break;
                            }
                        }

                        if (isstart && !温控加热)
                        {
                            result = 加热盘加热开启();
                            string result2 = H5509归零 ? 梭车加热开启() : " 设备未归零";
                            string result3 = Site加热开启();
                            if (result == "Success" && result2 == "Success" && result3 == "Success")
                            {
                                //label10.Text = "停止";
                                温控加热 = true;
                                温控达温 = false;
                            }
                            else
                            {
                                if (result != "Success") RunLog("加热盘加热开启失败!" + result);
                                if (result2 != "Success") RunLog("梭车加热开启失败!" + result2);
                                if (result3 != "Success") RunLog("Site加热开启失败!" + result3);
                                isstart = false;
                            }
                        }
                    }
                    else
                    {
                        for (int site = 0; site < SiteTemp.Length; site++)
                        {
                            if (SiteTemp[site] == 32770) continue;

                            if (GetSiteOpen(site).Contains(true))
                            {
                                if (SiteTemp[site] > 常温上限)
                                {
                                    isstart = false;
                                    Listcode.Add($"8{site:D2}319");
                                    if (site == 16) RunLog($"[Code]8{site:D2}319,[Msg]温控器:温度[{SiteTemp[site]}]℃高于常温上限[{常温上限}]℃", LogType.AlarmLog);
                                    else RunLog($"[Code]8{site:D2}319,[Msg]Site{site + 1}:温度[{SiteTemp[site]}]℃高于常温上限[{常温上限}]℃", LogType.AlarmLog);
                                }
                            }
                        }
                    }
                }
                else
                {
                    isstart = false;
                    Listcode.Add("1400448");
                    RunLog($"[Code]1400448,[Msg]温控器连接失败!", LogType.AlarmLog);
                }
            }

            return isstart;
        }

        private bool 初次开始检查(string[] io)
        {
            bool isstart = true;

            if (io[0] == "0") //15
            {
                isstart = false;
                Listcode.Add("4018008");
                RunLog("[Code]4018008,[Msg](按下开始)Empty门未关闭", LogType.AlarmLog);
            }

            if (io[1] == "0")//16
            {
                isstart = false;
                Listcode.Add("4019008");
                RunLog("[Code]4019008,[Msg](按下开始)Load门未关闭", LogType.AlarmLog);
            }

            if (io[2] == "0")//18
            {
                isstart = false;
                Listcode.Add("4020008");
                RunLog("[Code]4020008,[Msg](按下开始)Auto1门未关闭", LogType.AlarmLog);
            }

            if (io[3] == "0")//19
            {
                isstart = false;
                Listcode.Add("4021008");
                RunLog("[Code]4021008,[Msg](按下开始)Auto2门未关闭", LogType.AlarmLog);
            }

            if (Tray盘模式)
            {
                if (io[4] == "0")//20
                {
                    isstart = false;
                    Listcode.Add("4030009");
                    RunLog("[Code]4030009,[Msg](按下开始)Fix1请放置空盘", LogType.AlarmLog);
                }

                if (io[5] == "0")//21
                {
                    isstart = false;
                    Listcode.Add("4031009");
                    RunLog("[Code]4031009,[Msg](按下开始)Fix2请放置空盘", LogType.AlarmLog);
                }

                if (io[6] == "1")//22
                {
                    isstart = false;
                    Listcode.Add("4032009");
                    RunLog("[Code]4032009,[Msg](按下开始)Fix3请放置空盘", LogType.AlarmLog);
                }

                if (io[7] == "0")//23
                {
                    isstart = false;
                    Listcode.Add("4020001");
                    RunLog("[Code]4020001,[Msg](按下开始)AUTO1前对射感应到有Tray盘", LogType.AlarmLog);
                }

                if (io[8] == "0")//24
                {
                    isstart = false;
                    Listcode.Add("4020001");
                    RunLog("[Code]4020001,[Msg](按下开始)AUTO1后对射感应到有Tray盘", LogType.AlarmLog);
                }

                if (io[9] == "0")//25
                {
                    isstart = false;
                    Listcode.Add("4021001");
                    RunLog("[Code]4021001,[Msg](按下开始)AUTO2前对射感应到有Tray盘", LogType.AlarmLog);
                }

                if (io[10] == "0")//26
                {
                    isstart = false;
                    Listcode.Add("4021001");
                    RunLog("[Code]4021001,[Msg](按下开始)AUTO2后对射感应到有Tray盘", LogType.AlarmLog);
                }
            }

            if (!CheckAircyStatus())
            {
                isstart = false;
            }

            return isstart;
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

            飞梭后出料IC = new bool[][] { new bool[] { false, false, false, false }, new bool[] { false, false, false, false } };
        }

        private void 交换飞梭前后出料BIN数据()
        {
            飞梭前出料TestSite = 飞梭后出料TestSite;
            飞梭前出料TestTime = 飞梭后出料TestTime;
            飞梭前出料BIN项 = new string[飞梭后出料BIN项.Length][];
            for (int i = 0; i < 飞梭后出料BIN项.Length; i++)
            {
                飞梭前出料BIN项[i] = new string[飞梭后出料BIN项[i].Length];
                Array.Copy(飞梭后出料BIN项[i], 飞梭前出料BIN项[i], 飞梭后出料BIN项[i].Length);
            }
        }

        private void 检测梭车是否有IC遗留()
        {
            //检测梭车是否有IC遗留
            if (飞梭后入料IC[0].Contains(true) || 飞梭后入料IC[1].Contains(true)) //飞梭后入料IC第1/2列无料
            {
                for (int i = 0; i < 飞梭后入料IC[0].Length; i++)
                {
                    if (飞梭后入料IC[0][i] || 飞梭后入料IC[1][i])
                    {
                        if (飞梭2位置 == 0)
                        {
                            if (飞梭后入料IC[0][i])
                            {
                                Listcode.Add($"5{i + 47}0102");
                                RunLog($"[Code][Msg]后梭车(梭车2)Dut{i + 1:D2}有IC遗留", LogType.AlarmLog);
                            }
                            if (飞梭后入料IC[1][i])
                            {
                                Listcode.Add($"5{i + 47}0103");
                                RunLog($"[Code][Msg]后梭车(梭车2)Dut{i + 5:D2}有IC遗留", LogType.AlarmLog);
                            }
                        }
                        else
                        {
                            if (飞梭后入料IC[0][i])
                            {
                                Listcode.Add($"5{i + 47}0100");
                                RunLog($"[Code][Msg]后梭车(梭车1)Dut{i + 1:D2}有IC遗留", LogType.AlarmLog);
                            }
                            if (飞梭后入料IC[1][i])
                            {
                                Listcode.Add($"5{i + 47}0101");
                                RunLog($"[Code][Msg]后梭车(梭车1)Dut{i + 5:D2}有IC遗留", LogType.AlarmLog);
                            }
                        }
                    }
                }
            }

            if (飞梭前入料IC[0].Contains(true) || 飞梭前入料IC[1].Contains(true)) //飞梭前入料IC第1/2列无料
            {
                for (int i = 0; i < 飞梭前入料IC[0].Length; i++)
                {
                    if (飞梭前入料IC[0][i] || 飞梭前入料IC[1][i])
                    {
                        if (飞梭2位置 == 0)
                        {
                            if (飞梭前入料IC[0][i])
                            {
                                Listcode.Add($"5{i + 43}0100");
                                RunLog($"[Code][Msg]前梭车(梭车1)Dut{i + 1:D2}有IC遗留", LogType.AlarmLog);
                            }
                            if (飞梭前入料IC[1][i])
                            {
                                Listcode.Add($"5{i + 43}0101");
                                RunLog($"[Code][Msg]前梭车(梭车1)Dut{i + 5:D2}有IC遗留", LogType.AlarmLog);
                            }
                        }
                        else
                        {
                            if (飞梭前入料IC[0][i])
                            {
                                Listcode.Add($"5{i + 43}0102");
                                RunLog($"[Code][Msg]前梭车(梭车2)Dut{i + 1:D2}有IC遗留", LogType.AlarmLog);
                            }
                            if (飞梭前入料IC[1][i])
                            {
                                Listcode.Add($"5{i + 43}0103");
                                RunLog($"[Code][Msg]前梭车(梭车2)Dut{i + 5:D2}有IC遗留", LogType.AlarmLog);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region 动作流程

        public string Empty向上移动(int type = 0)
        {
            string axisName = "M12_Empty顶盘轴";

            string result = GetAxisRunInfo(axisName, out AxisRunInfo axisRunInfo);
            if (result != "Success") return result;

            result = motionEx.AxisJogMove(axisName, true);
            if (result != "Success") return result;

            while (true)
            {
                if (APPmaster.IsPause && type == 0)
                {
                    motionEx.AxisStopMove(axisName, 1);
                    while (true)
                    {
                        if (APPmaster.IsPause) Thread.Sleep(10);
                        else
                        {
                            result = motionEx.AxisJogMove(axisName, true);
                            if (result != "Success") return result;
                            break;
                        }
                    }
                }
                else if (APPmaster.IsEmptyPause && axisName == "M12_Empty顶盘轴")
                {
                    motionEx.AxisStopMove(axisName, 1);
                    while (true)
                    {
                        if (APPmaster.IsEmptyPause) Thread.Sleep(10);
                        else
                        {
                            result = motionEx.AxisJogMove(axisName, true);
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

                    double maxpoint = 初始点位.Empty_最高上顶 / axisRunInfo.步长mm;
                    if (prfpos > maxpoint || value2 == "0")
                    {
                        空盘上顶到最大值 = true;
                        motionEx.AxisStopMove(axisName, 1);
                        //Thread.Sleep(300);//解决轴:Empty第1次回零失败，详细信息：正在运动
                        return $"{axisName}当前上升行程{prfpos}>{maxpoint} || {value2}";
                    }

                    Thread.Sleep(1); //减少轴卡和CPU压力
                }
            }
        }

        public string Empty向下移动(int type = 0)
        {
            string axisName = "M12_Empty顶盘轴";

            string result = motionEx.AxisJogMove(axisName, false);
            if (result != "Success") return result;

            while (true)
            {
                if (APPmaster.IsPause && type == 0)
                {
                    motionEx.AxisStopMove(axisName, 1);
                    while (true)
                    {
                        if (APPmaster.IsPause) Thread.Sleep(10);
                        else
                        {
                            result = motionEx.AxisJogMove(axisName, false);
                            if (result != "Success") return result;
                            break;
                        }
                    }
                }
                else if (APPmaster.IsEmptyPause)
                {
                    motionEx.AxisStopMove(axisName, 1);
                    while (true)
                    {
                        if (APPmaster.IsEmptyPause) Thread.Sleep(10);
                        else
                        {
                            result = motionEx.AxisJogMove(axisName, true);
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
                        空盘下降到最小值 = false;
                        motionEx.AxisStopMove(axisName, 1);
                        //Thread.Sleep(300);//解决轴:Empty第1次回零失败，详细信息：正在运动
                        return $"{axisName}当前下降行程{prfpos} < {空盘下降最小值}";
                    }
                }
            }
        }

        public string Load向上移动(int type = 0)
        {
            待测盘上顶到最大值 = false;

            string axisName = "M13_Loader顶盘轴";

            string result = GetAxisRunInfo(axisName, out AxisRunInfo axisRunInfo);
            if (result != "Success") return result;

            result = motionEx.AxisJogMove(axisName, true);
            if (result != "Success") return result;

            while (true)
            {
                if (APPmaster.IsPause && type == 0)
                {
                    motionEx.AxisStopMove(axisName, 1);
                    while (true)
                    {
                        if (APPmaster.IsPause) Thread.Sleep(10);
                        else
                        {
                            result = motionEx.AxisJogMove(axisName, true);
                            if (result != "Success") return result;
                            break;
                        }
                    }
                }
                else if (APPmaster.IsLoadPause && axisName == "M13_Loader顶盘轴")
                {
                    motionEx.AxisStopMove(axisName, 1);
                    while (true)
                    {
                        if (APPmaster.IsLoadPause) Thread.Sleep(10);
                        else
                        {
                            result = motionEx.AxisJogMove(axisName, true);
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

                    double maxpoint = 初始点位.Load_最高上顶 / axisRunInfo.步长mm;
                    if (prfpos > maxpoint || value2 == "0") //待测盘上顶最大值
                    {
                        motionEx.AxisStopMove(axisName, 1);
                        待测盘上顶到最大值 = true;
                        //Thread.Sleep(300);//解决轴:Empty第1次回零失败，详细信息：正在运动
                        return $"{axisName}当前上升行程{prfpos}>{maxpoint} || {value2}";
                    }
                    Thread.Sleep(1); //减少轴卡和CPU压力
                }
            }
        }

        public string Load向下移动(int type = 0)
        {
            string axisName = "M13_Loader顶盘轴";

            string result = motionEx.AxisJogMove(axisName, false);
            if (result != "Success") return result;

            while (true)
            {
                if (APPmaster.IsPause && type == 0)
                {
                    motionEx.AxisStopMove(axisName, 1);
                    while (true)
                    {
                        if (APPmaster.IsPause) Thread.Sleep(10);
                        else
                        {
                            result = motionEx.AxisJogMove(axisName, false);
                            if (result != "Success") return result;
                            break;
                        }
                    }
                }
                else if (APPmaster.IsLoadPause)
                {
                    motionEx.AxisStopMove(axisName, 1);
                    while (true)
                    {
                        if (APPmaster.IsLoadPause) Thread.Sleep(10);
                        else
                        {
                            result = motionEx.AxisJogMove(axisName, true);
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

        public string Auto1向上移动(int type = 0)
        {
            string axisName = "M14_Auto1顶盘轴";
            string result = motionEx.AxisJogMove(axisName, true);
            if (result != "Success") return result;
            while (true)
            {
                if (APPmaster.IsPause && type == 0)
                {
                    motionEx.AxisStopMove(axisName, 1);
                    while (true)
                    {
                        if (APPmaster.IsPause) Thread.Sleep(10);
                        else
                        {
                            result = motionEx.AxisJogMove(axisName, true);
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

                    if (value == "0") //返回0是挡到感应器,1是没有挡到感应器
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

        public string Auto1向下移动(int type = 0)
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

            result = motionEx.AxisJogMove(axisName, false);
            if (result != "Success") return result;

            while (true)
            {
                if (APPmaster.IsPause && type == 0)
                {
                    motionEx.AxisStopMove(axisName, 1);
                    while (true)
                    {
                        if (APPmaster.IsPause) Thread.Sleep(10);
                        else
                        {
                            result = motionEx.AxisJogMove(axisName, false);
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

        public string Auto2向上移动(int type = 0)
        {
            string axisName = "M15_Auto2顶盘轴";
            string result = motionEx.AxisJogMove(axisName, true);
            if (result != "Success") return result;
            while (true)
            {
                if (APPmaster.IsPause && type == 0)
                {
                    motionEx.AxisStopMove(axisName, 1);
                    while (true)
                    {
                        if (APPmaster.IsPause) Thread.Sleep(10);
                        else
                        {
                            result = motionEx.AxisJogMove(axisName, true);
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

        public string Auto2向下移动(int type = 0)
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

            result = motionEx.AxisJogMove(axisName, false);
            if (result != "Success") return result;

            while (true)
            {
                if (APPmaster.IsPause && type == 0)
                {
                    motionEx.AxisStopMove(axisName, 1);
                    while (true)
                    {
                        if (APPmaster.IsPause) Thread.Sleep(10);
                        else
                        {
                            result = motionEx.AxisJogMove(axisName, false);
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

        #endregion

        #region Task系统任务流程

        public void UpdateInNozzleIC()
        {
            string result = 入料吸嘴_真空检测(out string[] iovalue);
            if (result == "Success" && iovalue.Contains("0"))
            {
                for (int i = 0; i < iovalue.Length; i++)
                {
                    if (InNozzleICOpen[i] && InNozzleIC)
                    {
                        if (iovalue[i] == "0")
                        {
                            InNozzleICOpen[i] = false;
                            入料吸嘴_吸放料(i, true, false);
                        }
                    }
                }
            }
        }

        public void UpdateOutNozzleIC()
        {
            string result = 出料吸嘴_真空检测(out string[] iovalue);
            if (result == "Success" && iovalue.Contains("0"))
            {
                for (int i = 0; i < iovalue.Length; i++)
                {
                    if (OutNozzleICOpen[i] && OutNozzleIC)
                    {
                        if (iovalue[i] == "0")
                        {
                            OutNozzleICOpen[i] = false;
                            出料吸嘴_吸放料(i, true, false);
                        }
                    }
                }
            }
        }

        public void UpdateTestNozzleIC()
        {
            string result = 测试吸嘴_真空检测(out string[] iovalue);

            if (result == "Success" && iovalue.Contains("0"))
            {
                for (int i = 0; i < iovalue.Length; i++)
                {
                    if (TestNozzleICOpen[i] && TestNozzleIC)
                    {
                        if (iovalue[i] == "0")
                        {
                            TestNozzleICOpen[i] = false;
                            测试吸嘴_吸放料(i, true, false);
                        }
                    }
                }
            }
        }

        public bool CheckInNozzleIC()
        {
            bool ispass = false;
            if (InNozzleIC)
            {
                string result = 入料吸嘴_真空检测(out string[] iovalue);
                if (result == "Success" && iovalue.Contains("0"))
                {
                    for (int i = 0; i < iovalue.Length; i++)
                    {
                        if (InNozzleICOpen[i] && InNozzleIC)
                        {
                            if (iovalue[i] == "0")
                            {
                                入料IC移动过程中忽略 = true;
                                ispass = true;
                                Listcode.Add($"30{i + 1}0001"); //出料手臂-吸--1号吸嘴丢失IC
                                RunLog($"[Code]30{i + 1}0001,[Msg]入料手臂-吸--{i + 1}号吸嘴丢失IC", LogType.AlarmLog);

                                motionEx.SetDo("Y005_忽略灯", true);
                            }
                        }
                    }
                }
            }
            return ispass;
        }

        public bool CheckOutNozzleIC()
        {
            bool ispass = false;
            if (OutNozzleIC)
            {
                string result = 出料吸嘴_真空检测(out string[] iovalue);
                if (result == "Success" && iovalue.Contains("0"))
                {
                    for (int i = 0; i < iovalue.Length; i++)
                    {
                        if (OutNozzleICOpen[i] && OutNozzleIC)
                        {
                            if (iovalue[i] == "0")
                            {
                                出料IC移动过程中忽略 = true;
                                ispass = true;
                                Listcode.Add($"30{i + 5}0001"); //出料手臂-吸--1号吸嘴丢失IC
                                RunLog($"[Code]30{i + 5}0001,[Msg]出料手臂-吸--{i + 1}号吸嘴丢失IC", LogType.AlarmLog);

                                motionEx.SetDo("Y005_忽略灯", true);
                            }
                        }
                    }
                }
            }
            return ispass;
        }

        public bool CheckTestNozzleIC()
        {
            bool ispass = false;
            if (TestNozzleIC)
            {
                string result = 测试吸嘴_真空检测(out string[] iovalue);

                if (result == "Success" && iovalue.Contains("0"))
                {
                    for (int i = 0; i < iovalue.Length; i++)
                    {
                        if (TestNozzleICOpen[i] && TestNozzleIC)
                        {
                            if (iovalue[i] == "0")
                            {
                                测试IC移动过程中忽略 = true;
                                ispass = true;
                                Listcode.Add($"3{i + 9:D2}9901"); //出料手臂-吸--1号吸嘴丢失IC
                                RunLog($"[Code]3{i + 9:D2}9901,[Msg]测试手臂-吸--{i + 1}号吸嘴丢失IC", LogType.AlarmLog);

                                motionEx.SetDo("Y005_忽略灯", true);
                            }
                        }
                    }
                }
            }
            return ispass;
        }

        public bool CheckTimeoutBin()
        {
            string oth = 设置信息.测试机信息.超时Bin字符串显示;

            if (Auto1 != "" && Auto1.Split(",").Contains(oth)) return false;
            else if (Auto2 != "" && Auto2.Split(",").Contains(oth)) return false;
            else if (Fix1 != "" && Fix1.Split(",").Contains(oth)) return false;
            else if (Fix2 != "" && Fix2.Split(",").Contains(oth)) return false;
            else if (Fix3 != "" && Fix3.Split(",").Contains(oth)) return false;
            else
            {
                未分配超时BIN = oth;
                Listcode.Add("1400488");
                RunLog($"[Code]1400488,[Msg](按下开始)超时BIN({oth})未分配！请前往分BIN界面添加", LogType.AlarmLog);
                return true;
            }
        }

        public void 系统按钮检测流程()
        {
            bool safekey = true;
            string[] ioInfos =
            {
                "X001_开始", //0
                "X002_暂停", //1
                "X003_安全钮", //2
                "X004_重试", //3
                "X005_忽略", //4
                "X006_定点停止", //5
                "X007_定点开始", //6
                "X008_结批", //7
                "X009_一个循环", //8
                "X010_归零", //9
                "X011_换盘", //10
                "X012_前门禁", //11
                "X013_后门禁", //12
                "X014_左门禁", //13
                "X015_右门禁", //14
                "X018_空盘门感应", //15
                "X024_Load门感应", //16
                "X038_BIN平移按键", //17
                "X036_AUTO1门感应", //18
                "X042_AUTO2门感应", //19
                "X046_FIX1盘感应", //20
                "X047_FIX2盘感应", //21
                "X045_FIX3盘对射", //22
                "X034_AUTO1前对射", //23
                "X035_AUTO1后对射", //24
                "X040_AUTO2前对射", //25
                "X041_AUTO2后对射", //26
                "X020_进气压",//27
            };

            while (true)
            {
                if (!InitedOK)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                string result = motionEx.GetDi(ioInfos, out string[] io);
                if (result == "Success")
                {
                    bool safesw = io[2] == "1"; //"X003_安全钮"
                    if (CheckEdgeSingal(safesw, ref SafeswRiseCache, true))
                    {
                        safekey = false;
                        motionEx.SetDo("Y003_安全钮灯", true);
                        //RunLog("按下安全钮");
                    }

                    if (CheckEdgeSingal(safesw, ref SafeswFallCache, false))
                    {
                        safekey = true;
                        motionEx.SetDo("Y003_安全钮灯", false);
                        //RunLog("松开安全钮");
                    }

                    bool stop = io[1] == "1"; //"X002_暂停"
                    if (CheckEdgeSingal(stop, ref StopRiseCache, true) || ProgramPause || ProgramBtnPause)
                    {
                        if (ProgramPause)
                        {
                            ProgramPause = false;
                            RunLog("按下暂停(程序)");//, LogType.OperateLog
                        }
                        else if (ProgramBtnPause)
                        {
                            ProgramBtnPause = false;
                            RunLog("按下暂停(界面)", LogType.OperateLog);//, LogType.OperateLog
                        }
                        else
                        {
                            RunLog("按下暂停", LogType.OperateLog);
                        }

                        APPmaster.IsPause = true;
                        APPmaster.IsStart = false;
                        if (Empty正在下降回零 || Empty舱门上顶标记) APPmaster.IsEmptyPause = true;
                        if (Load正在下降回零 || Load舱门上顶标记) APPmaster.IsLoadPause = true;

                        if (设备状态 == 1) 设备状态 = 2;

                        if (设备状态 == 0 || 设备状态 == 2) motionEx.AllAxisStop();

                        form_alarm.AlarmLight(0, 1, 0, "无音乐"); //按下暂停
                        //motionEx.Motion.AllAxisStop();

                        if (设备状态 != 0 && H5509归零 && H5509初始化 && Tray盘模式 && !正在结批)
                        {
                            if (!Empty正在下降回零 && !Load正在下降回零 && !Empty舱门正在上顶 && !Load舱门正在上顶)
                            {
                                if (!Load换盘标记 && Load上顶标记 == 1 && Load下降回零标记 == 1)
                                {
                                    Load下降回零开关 = true;
                                    //RunLog("Load下降开关打开");
                                }
                                if (!Auto1换盘标记 && !Auto2换盘标记 && Empty上顶标记 == 1 && Empty下降标记 == 1)
                                {
                                    Empty下降回零开关 = true;
                                    Auto1下降回零开关 = true;
                                    Auto2下降回零开关 = true;
                                    //RunLog("Empty/Auto下降开关打开");
                                }
                            }
                        }

                        motionEx.SetDo("Y001_开始灯", false);
                        motionEx.SetDo("Y002_暂停灯", true);
                        motionEx.SetDo("Y006_定点停止灯", false);
                        motionEx.SetDo("Y007_定点开始灯", false);
                        motionEx.SetDo("Y009_一个循环灯", false);
                    }

                    if (safekey)
                    {
                        bool start = io[0] == "1"; //"X001_开始"
                        if (CheckEdgeSingal(start, ref StartRiseCache, true) || ProgramStart || ProgramBtnStart)
                        {
                            if (ProgramStart)
                            {
                                ProgramStart = false;
                                RunLog("按下开始(程序)", LogType.RunLog);
                            }
                            else if (ProgramBtnStart)
                            {
                                ProgramBtnStart = false;
                                RunLog("按下开始(界面)", LogType.OperateLog);
                            }
                            else RunLog("按下开始", LogType.OperateLog);

                            bool isstart = true;

                            form_alarm.CancelAllAlarm();

                            if (Load正在下降回零 || Empty正在下降回零)
                            {
                                isstart = false;
                                RunLog("(按下开始)Load或Empty正在下降回零，无法开始", LogType.OperateLog);

                                if (Empty正在下降回零) IsEmptyPause = false;
                                if (Load正在下降回零) IsLoadPause = false;
                            }

                            if (H5509正在归零)
                            {
                                isstart = false;
                                RunLog("(按下开始)设备正在回零...,无法开始", LogType.OperateLog);
                            }

                            if (结批选项选择中)
                            {
                                isstart = false;
                                RunLog("(按下开始)结批选项选择中...,无法开始", LogType.OperateLog);
                            }

                            if (一个循环选项选择中)
                            {
                                isstart = false;
                                RunLog("(按下开始)一个循环选项选择中...,无法开始", LogType.OperateLog);
                            }

                            if (入料IC移动过程中忽略)
                            {
                                if (CheckInNozzleIC())
                                {
                                    isstart = false;
                                    RunLog("(按下开始)入料手臂移动过程中有IC掉落,无法开始", LogType.OperateLog);
                                }
                            }

                            if (出料IC移动过程中忽略)
                            {
                                if (CheckOutNozzleIC())
                                {
                                    isstart = false;
                                    RunLog("(按下开始)出料手臂移动过程中有IC掉落,无法开始", LogType.OperateLog);
                                }
                            }

                            if (测试IC移动过程中忽略)
                            {
                                if (CheckTestNozzleIC())
                                {
                                    isstart = false;
                                    RunLog("(按下开始)测试手臂移动过程中有IC掉落,无法开始", LogType.OperateLog);
                                }
                            }

                            if (等待Load门开启)
                            {
                                if (!门已打开超设定时间)
                                {
                                    isstart = false;
                                    Listcode.Add("1400472");
                                    RunLog($"[Code]1400472,[Msg]请打开前安全门{Load门开门时间}秒并确认是否有料剩余在Tray盘！", LogType.AlarmLog);
                                }
                            }

                            if (CheckTimeoutBin())
                            {
                                isstart = false;
                            }

                            string[] iov = { io[15], io[16], io[18], io[19], io[20], io[21], io[22], io[23], io[24], io[25], io[26] };

                            if (isstart && 设备状态 == 0)
                            {
                                if (!初次开始检查(iov))
                                {
                                    StationManagers.ConnectAll(); //清空缓存区
                                    isstart = false;
                                }
                            }

                            if (isstart && 开始检查(iov))
                            {
                                if (设备状态 == 0)
                                {
                                    开始时间 = DateTime.Now;

                                    Summary_LotNo = (LotNo[0] ?? "") + "_" + $"{开始时间.Year:D2}-{开始时间.Month:D2}-{开始时间.Day:D2}_{开始时间.Hour:D2}h{开始时间.Minute:D2}m{开始时间.Second:D2}s";
                                    //Summary_LotNo = LotNo[0] ?? "" + "_" + 开始时间.ToString("yyyyMMdd") + $"_{开始时间.Hour:D2}h{开始时间.Minute:D2}m{开始时间.Second:D2}s";

                                    Sqlite.runSqlNonQuery(SQLiteRunningString, "update DutCurrent set pass=0,total=0,usedTime=0");
                                    string sql = $"create table f_{(LotNo[0] ?? "").Replace("-", "_") + "_" + 开始时间.ToString("yyyyMMdd_HH_mm_ss")}(批次开始日期 nvarchar(50) NULL,获取MAP时间 nvarchar(50) NULL,料号 nvarchar(50) NULL,批号 nvarchar(20) NULL,MAP信息 nvarchar(50) NULL )";
                                    string sql2 = $"create table o_{(LotNo[0] ?? "").Replace("-", "_") + "_" + 开始时间.ToString("yyyyMMdd_HH_mm_ss")}(时间 nvarchar(50) NULL,名称 nvarchar(50) NULL,大小 nvarchar(50) NULL,数据 nvarchar(MAX) NULL,测试时间 nvarchar(MAX) NULL,测试工位 nvarchar(MAX) NULL)";
                                    //string sql3 = $"create table otime_{(LotNo[0] ?? "").Replace("-", "_") + "_" + 开始时间.ToString("yyyyMMdd_HH_mm_ss")}(时间 nvarchar(50) NULL,名称 nvarchar(50) NULL,大小 nvarchar(50) NULL,数据 nvarchar(MAX) NULL)";
                                    //string sql4 = $"create table osite_{(LotNo[0] ?? "").Replace("-", "_") + "_" + 开始时间.ToString("yyyyMMdd_HH_mm_ss")}(时间 nvarchar(50) NULL,名称 nvarchar(50) NULL,大小 nvarchar(50) NULL,数据 nvarchar(MAX) NULL)";
                                    int line = SqlServer.runSqlNonQuery(SQLServerConnString, sql);
                                    int line1 = SqlServer.runSqlNonQuery(SQLServerConnString, sql2);
                                    //int line2 = SqlServer.runSqlNonQuery(SQLServerConnString, sql3);
                                    //int line3 = SqlServer.runSqlNonQuery(SQLServerConnString, sql4);

                                    if (line <= -2)
                                    {
                                        isstart = false;
                                        Listcode.Add("1400330");
                                        RunLog("执行SQl异常：" + sql);
                                        RunLog("[Code]1400330,[Msg](按下开始)创建MAP信息数据库失败!", LogType.AlarmLog);
                                    }

                                    if (line1 <= -2)
                                    {
                                        isstart = false;
                                        Listcode.Add("1400330");
                                        if (line1 <= -2) RunLog("执行SQl异常：" + sql2);
                                        //if (line2 <= -2) RunLog("执行SQl异常：" + sql3);
                                        //if (line3 <= -2) RunLog("执行SQl异常：" + sql4);
                                        RunLog("[Code]1400330,[Msg](按下开始)创建BIN信息数据库失败!", LogType.AlarmLog);
                                    }

                                    if (isstart)
                                    {
                                        StartRunIn(); //根据开Site情况添加入料队列

                                        //开始前的IO检查
                                        StartSite数量 = 开Site数量;

                                        if (当前权限 != "Operator")//&& CuPersonnel != "supplier")
                                        {
                                            刷新权限();
                                        }

                                        刷新分BIN信息();

                                        报警次数 = 0;
                                    }
                                }

                                if (isstart)
                                {
                                    if (一个循环完成)
                                    {
                                        一个循环完成 = false;
                                        StartRunIn(); //根据开Site情况添加入料队列
                                    }

                                    if (Empty舱门上顶标记)
                                    {
                                        if (!Empty舱门正在上顶)
                                        {
                                            Empty舱门正在上顶 = true;
                                            IsEmptyPause = false;
                                            Empty舱门开启标记 = 1;
                                            Empty上顶标记 = 2;
                                            RunLog("Empty正在执行上顶...", LogType.OperateLog);
                                            Task task = Task.Run(() =>
                                            {
                                                while (true)
                                                {
                                                    if (Empty上顶标记 == 1)
                                                    {
                                                        Empty舱门开启标记 = 0;
                                                        Empty舱门正在上顶 = false;
                                                        Empty舱门上顶标记 = false;
                                                        break;
                                                    }
                                                    else Thread.Sleep(10);
                                                }
                                                Empty舱门正在上顶 = false;
                                                RunLog("Empty执行上顶完成", LogType.OperateLog);
                                            });
                                        }
                                        else IsEmptyPause = false;
                                    }

                                    if (Load舱门上顶标记)
                                    {
                                        if (!Load舱门正在上顶)
                                        {
                                            Load舱门正在上顶 = true;
                                            IsLoadPause = false;
                                            Load舱门开启标记 = 1;
                                            Load上顶标记 = 2;
                                            RunLog("Load正在执行上顶...", LogType.OperateLog);
                                            Task task = Task.Run(() =>
                                            {
                                                while (true)
                                                {
                                                    if (Load上顶标记 == 1)
                                                    {
                                                        Load舱门开启标记 = 0;
                                                        Load舱门正在上顶 = false;
                                                        Load舱门上顶标记 = false;
                                                        break;
                                                    }
                                                    else Thread.Sleep(10);
                                                }
                                                Load舱门正在上顶 = false;
                                                RunLog("Load执行上顶完成", LogType.OperateLog);
                                            });
                                        }
                                        else IsLoadPause = false;
                                    }

                                    if (Load舱门上顶标记 || Empty舱门上顶标记)
                                    {
                                        if (!开舱门上顶标记)
                                        {
                                            开舱门上顶标记 = true;
                                            Task task = Task.Run(() =>
                                            {
                                                while (true)
                                                {
                                                    if (!Load舱门上顶标记 && !Empty舱门正在上顶)
                                                    {
                                                        APPmaster.IsPause = false;
                                                        APPmaster.IsStart = true;
                                                        APPmaster.IsLoadPause = false;
                                                        APPmaster.IsEmptyPause = false;
                                                        break;
                                                    }
                                                    Thread.Sleep(10);
                                                }
                                                开舱门上顶标记 = false;
                                            });
                                        }
                                    }
                                    else
                                    {
                                        APPmaster.IsPause = false;
                                        APPmaster.IsStart = true;
                                        APPmaster.IsLoadPause = false;
                                        APPmaster.IsEmptyPause = false;
                                    }

                                    设备状态 = 1;
                                    结批完成 = false;
                                    Load下降回零开关 = false;
                                    Empty下降回零开关 = false;
                                    Auto1下降回零开关 = false;
                                    Auto2下降回零开关 = false;

                                    入料IC移动过程中忽略 = false; //报警后有可能会把芯片放回吸嘴上，直接点击了开始
                                    出料IC移动过程中忽略 = false;
                                    测试IC移动过程中忽略 = false;

                                    motionEx.SetDo("Y001_开始灯", true);
                                    motionEx.SetDo("Y002_暂停灯", false);
                                    motionEx.SetDo("Y004_重试灯", false);
                                    motionEx.SetDo("Y005_忽略灯", false);
                                    motionEx.SetDo("Y008_结批灯", false);
                                    motionEx.SetDo("Y009_一个循环灯", false);
                                    motionEx.SetDo("Y011_换盘灯", false);
                                    motionEx.SetDo("Y013_黄灯", false);
                                    motionEx.SetDo("Y008_结批灯", false);
                                    motionEx.SetDo("Y014_绿灯", true);
                                }
                                else
                                {
                                    ProgramPause = true;
                                    IsPause = true;
                                }
                            }
                            else
                            {
                                ProgramPause = true;
                                IsPause = true;
                            }
                        }

                        bool again = io[3] == "1"; //X004_重试
                        if (CheckEdgeSingal(again, ref AgainRiseCache, true))
                        {
                            RunLog("按下重试", LogType.OperateLog);

                            if (设备状态 == 1) continue;

                            入料IC忽略 = false;
                            出料IC忽略 = false;
                            测试IC忽略 = false;
                            入料IC吸不起结批 = false;
                            测试区IC忽略 = false;
                            motionEx.SetDo("Y004_重试灯", true);
                            motionEx.SetDo("Y005_忽略灯", false);
                            motionEx.SetDo("Y008_结批灯", false);
                            motionEx.SetDo("Y011_换盘灯", false);
                            //motionEx.SetGNME601_Output(GNME601IO.Y004_重试灯, true);
                            //motionEx.SetGNME601_Output(GNME601IO.Y005_忽略灯, false);
                            //motionEx.SetGNME601_Output(GNME601IO.Y008_结批灯, false);
                            //motionEx.SetGNME601_Output(GNME601IO.Y011_换TRAY灯, false);
                        }

                        bool ignore = io[4] == "1"; //X005_忽略/跳过
                        if (CheckEdgeSingal(ignore, ref IgnoreRiseCache, true))
                        {
                            RunLog("按下跳过", LogType.OperateLog);

                            if (设备状态 == 1) continue;

                            if (设备状态 != 0 && 测试IC忽略 == false && 测试区IC忽略)
                            {
                                form_alarm.CancelAllAlarm();
                                form_alarm.AlarmLight(0, 1, 0, "无音乐"); //按下暂停
                                FrmInputPwd frmInputPwd = new FrmInputPwd();
                                if (frmInputPwd.ShowDialog() == DialogResult.OK)
                                {
                                    string getEngineerwd = GetPassWord("Engineer", out string 密码);
                                    string getAdministratorwd = GetPassWord("Administrator", out string 密码2);
                                    string getSupplierwd = GetPassWord("Supplier", out string 密码3);
                                    if (getEngineerwd == "Success" && getAdministratorwd == "Success" && getSupplierwd == "Success")
                                    {
                                        if (frmInputPwd.Password == 密码 || frmInputPwd.Password == 密码2 || frmInputPwd.Password == 密码3)
                                        {
                                            RunLog($"密码输出正确");
                                        }
                                        else
                                        {
                                            RunLog($"密码错误！");//{frmInputPwd.Password}
                                            MessageBox.Show($"密码错误！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);//{frmInputPwd.Password}
                                        }
                                    }
                                    else
                                    {
                                        if (getEngineerwd != "Success")
                                        {
                                            RunLog($"获取Engineer密码失败！" + getEngineerwd);
                                            MessageBox.Show($"获取Engineer密码失败！" + getEngineerwd, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }
                                        if (getAdministratorwd != "Success")
                                        {
                                            RunLog($"获取Administrator密码失败！" + getAdministratorwd);
                                            MessageBox.Show($"获取Administrator密码失败！" + getAdministratorwd, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }
                                        if (getSupplierwd != "Success")
                                        {
                                            RunLog($"获取Supplier密码失败！" + getSupplierwd);
                                            MessageBox.Show($"获取Supplier密码失败！" + getSupplierwd, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }
                                        continue;
                                    }
                                }
                                else continue;
                            }

                            if (入料IC忽略 == false) 入料IC忽略 = true;
                            if (出料IC忽略 == false) 出料IC忽略 = true;
                            if (测试IC忽略 == false) 测试IC忽略 = true;
                            if (入料IC移动过程中忽略) UpdateInNozzleIC();
                            if (出料IC移动过程中忽略) UpdateOutNozzleIC();
                            if (测试IC移动过程中忽略) UpdateTestNozzleIC();
                            入料IC移动过程中忽略 = false;
                            出料IC移动过程中忽略 = false;
                            测试IC移动过程中忽略 = false;
                            motionEx.SetDo("Y004_重试灯", false);
                            motionEx.SetDo("Y005_忽略灯", true);
                            motionEx.SetDo("Y008_结批灯", false);
                            motionEx.SetDo("Y011_换盘灯", false);

                            //motionEx.SetGNME601_Output(GNME601IO.Y004_重试灯, false);
                            //motionEx.SetGNME601_Output(GNME601IO.Y005_忽略灯, true);
                            //motionEx.SetGNME601_Output(GNME601IO.Y008_结批灯, false);
                            //motionEx.SetGNME601_Output(GNME601IO.Y011_换TRAY灯, false);
                        }

                        bool fixedstop = io[5] == "1"; //X006_定点停止
                        if (CheckEdgeSingal(fixedstop, ref FixedStopRiseCache, true))
                        {
                            RunLog("按下定点停止", LogType.OperateLog);
                            motionEx.SetDo("Y006_定点停止灯", true);
                            前方定点停止允许 = true;
                        }

                        bool fixedstart = io[6] == "1"; //X007_定点开始
                        if (CheckEdgeSingal(fixedstart, ref FixedStartRiseCache, true))
                        {
                            RunLog("按下定点开始", LogType.OperateLog);
                            motionEx.SetDo("Y007_定点开始灯", true);
                            后方定点停止允许 = true;
                        }

                        bool stopbatch = io[7] == "1"; //X008_结批
                        if (CheckEdgeSingal(stopbatch, ref StopBatchRiseCache, true))
                        {
                            RunLog("按下结批", LogType.OperateLog);
                            if (设备状态 != 0)
                            {
                                if (!强制结批标记)
                                {
                                    if (!结批触发定时器.Enabled)
                                    {
                                        结批触发定时器.Start();
                                        RunLog("结批按钮开始计时", LogType.OperateLog);
                                    }
                                }
                                if (!入料IC吸不起结批)
                                {
                                    入料IC吸不起结批 = true;
                                    按下结批时间 = DateTime.Now;
                                }
                                if (!无待测盘结批)
                                {
                                    无待测盘结批 = true;
                                    按下结批时间 = DateTime.Now;
                                }
                                motionEx.SetDo("Y004_重试灯", false);
                                motionEx.SetDo("Y005_忽略灯", false);
                                motionEx.SetDo("Y008_结批灯", true);
                                motionEx.SetDo("Y011_换盘灯", false);
                            }
                            //motionEx.SetGNME601_Output(GNME601IO.Y004_重试灯, false);
                            //motionEx.SetGNME601_Output(GNME601IO.Y005_忽略灯, false);
                            //motionEx.SetGNME601_Output(GNME601IO.Y008_结批灯, true);
                            //motionEx.SetGNME601_Output(GNME601IO.Y011_换TRAY灯, false);
                        }

                        if (CheckEdgeSingal(stopbatch, ref StopBatchFallCache, false))
                        {
                            结批触发定时器.Stop();
                            RunLog("已松开结批按钮停止计时", LogType.OperateLog);
                        }

                        bool onecycle = io[8] == "1"; //X009_一个循环
                        if (CheckEdgeSingal(onecycle, ref OnecycleRiseCache, true))
                        {
                            RunLog("按下一个循环", LogType.OperateLog);

                            //if (!IsDutAllCLose()) //当初次开始时检测有没有开dut
                            //{
                            //    Listcode.Add("1400442");
                            //    RunLog("[Code]1400442,[Msg](按下一次循环)Dut全部被关闭，请至少开1个Dut进行测试或长按结批后再按开始", LogType.AlarmLog);
                            //}

                            if (IsDutAllCLose()) //当初次开始时检测有没有开dut
                            {
                                if (!结批标记)
                                {
                                    if (!一个循环标记)
                                    {
                                        if (温度模式 == "高温")
                                        {
                                            一个循环选项选择中 = true;
                                            ProgramPause = true;
                                            IsPause = true;
                                            DialogResult dialog = MessageBox.Show("请确认是否清空加热盘?\n是:将加热盘剩余IC取完\n否:直接结批\n取消:不做任何操作", "提示", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                                            if (dialog == DialogResult.Yes)
                                            {
                                                一个循环标记 = true;
                                                一个循环完成 = false;
                                                按下一个循环时间 = DateTime.Now;
                                                OneCycle按钮闪烁的定时器.Start();
                                            }
                                            else if (dialog == DialogResult.No)
                                            {
                                                一个循环标记 = true;
                                                一个循环完成 = false;
                                                按下一个循环时间 = DateTime.Now;
                                                加热盘停止入料标记 = true;
                                                OneCycle按钮闪烁的定时器.Start();
                                            }
                                            一个循环选项选择中 = false;
                                            ProgramStart = true;
                                        }
                                        else
                                        {
                                            一个循环标记 = true;
                                            一个循环完成 = false;
                                            按下一个循环时间 = DateTime.Now;
                                            OneCycle按钮闪烁的定时器.Start();
                                        }
                                    }
                                    else RunLog("无法触发一个循环,正在一个循环中...");
                                }
                                else RunLog("无法触发结批,正在结批中...");
                            }
                        }

                        bool home = io[9] == "1"; //X010_归零
                        if (CheckEdgeSingal(home, ref HomeRiseCache, true))
                        {
                            RunLog("按下归零", LogType.OperateLog);
                            bool ishome = true;
                            if (!H5509正在归零 && 设备状态 == 0)
                            {
                                if (!安全门禁用)
                                {
                                    result = 门禁感应(out string[] iovalue);
                                    if (result == "Success")
                                    {
                                        if (iovalue.Contains("0"))
                                        {
                                            if (iovalue[0] == "0")
                                            {
                                                Listcode.Add("9000000");
                                                RunLog("[Code]9000000,[Msg](按下归零)前门禁被打开", LogType.AlarmLog);
                                            }
                                            if (iovalue[1] == "0")
                                            {
                                                Listcode.Add("9000010");
                                                RunLog("[Code]9000010,[Msg](按下归零)后门禁被打开", LogType.AlarmLog);
                                            }
                                            if (iovalue[2] == "0")
                                            {
                                                Listcode.Add("9000020");
                                                RunLog("[Code]9000020,[Msg](按下归零)左门禁被打开", LogType.AlarmLog);
                                            }
                                            if (iovalue[3] == "0")
                                            {
                                                Listcode.Add("9000040");
                                                RunLog("[Code]9000040,[Msg](按下归零)右门禁被打开", LogType.AlarmLog);
                                            }
                                            ishome = false;
                                        }
                                    }
                                }

                                if (io[27] == 气压报警)
                                {
                                    ishome = false;
                                    Listcode.Add("1400020"); //系统错误--气压不足
                                    RunLog("[Code]1400020,[Msg]整机气压异常", LogType.AlarmLog);
                                }

                                if (ishome)
                                {
                                    参数初始化();

                                    显示Dout颜色();

                                    //if (!Com.IsconnectTester)
                                    //{
                                    //    刷新批次信息();
                                    //}

                                    StationManagers.SetIsconnectTester(false); //Com.IsconnectTester = false;
                                    StationManagers.ConnectAll(); //清空缓存区

                                    刷新批次信息();

                                    H5509正在归零 = true;
                                    H5509归零 = false;
                                    IsPause = false;
                                    结批完成 = false;
                                    motionEx.SetDo("Y001_开始灯", false);
                                    motionEx.SetDo("Y002_暂停灯", false);
                                    motionEx.SetDo("Y010_归零灯", true);
                                    form_alarm.CancelAllAlarm();
                                    if (温控加热 && 温度模式 == "高温") 梭车加热关闭();
                                    //form_alarm.AlarmLight(0, 1, 0, "无音乐"); //按下暂停
                                    Task.Run(() =>
                                    {
                                        result = 整机回零流程();
                                        if (result == "Success")
                                        {
                                            H5509归零 = true;
                                            Listcode.Add("1400444");
                                            RunLog("[Code]1400444,[Msg]归零完成", LogType.AlarmLog);
                                            RunLog("归零完成", LogType.OperateLog);
                                        }
                                        else
                                        {
                                            Listcode.Add("1400420");
                                            RunLog("[Code]1400420,[Msg]归零失败!" + result, LogType.AlarmLog);
                                            全部回零异常Error = result;
                                            RunLog("归零失败", LogType.OperateLog);
                                        }

                                        H5509正在归零 = false;
                                        motionEx.SetDo("Y001_开始灯", false);
                                        motionEx.SetDo("Y002_暂停灯", false);
                                        motionEx.SetDo("Y010_归零灯", false);
                                    });
                                }
                                else
                                {
                                    ProgramPause = true;
                                    IsPause = true;
                                }
                            }
                        }

                        bool exchangetray = io[10] == "1"; //X011_换盘
                        if (CheckEdgeSingal(exchangetray, ref ExchangetrayRiseCache, true))
                        {
                            RunLog("按下换Tray", LogType.OperateLog);

                            if (设备状态 == 1) continue;

                            motionEx.SetDo("Y004_重试灯", false);
                            motionEx.SetDo("Y005_忽略灯", false);
                            motionEx.SetDo("Y008_结批灯", false);
                            motionEx.SetDo("Y011_换盘灯", true);
                        }

                        bool x18 = io[15] == "1";  //X018_空盘门感应
                        if (CheckEdgeSingal(x18, ref EmptyDoorRiseCache, true))
                        {
                            RunLog("Empty仓门已关闭", LogType.OperateLog);
                            if (设备状态 != 0)
                            {
                                if (Empty正在下降回零 && Tray盘模式) IsEmptyPause = true;
                            }
                        }
                        else if (CheckEdgeSingal(x18, ref EmptyDoorFallCache, false))
                        {
                            RunLog("Empty仓门被打开", LogType.OperateLog);
                            if (设备状态 != 0)
                            {
                                if (Empty料仓打开标记 == false) Empty料仓打开标记 = true;

                                if (Empty下降回零开关 && 设备状态 == 2)
                                {
                                    if (!Empty正在下降回零)
                                    {
                                        Empty正在下降回零 = true;
                                        IsEmptyPause = false;
                                        Empty舱门开启标记 = 1;
                                        Empty下降回零标记 = 2;
                                        RunLog("Empty正在执行下降回零...", LogType.OperateLog);
                                        Task task = Task.Run(() =>
                                        {
                                            while (true)
                                            {
                                                if (Empty下降回零标记 == 1)
                                                {
                                                    Empty舱门上顶标记 = true;
                                                    Empty舱门开启标记 = 0;
                                                    break;
                                                }
                                                else Thread.Sleep(10);
                                            }
                                            Empty正在下降回零 = false;
                                            RunLog("Empty下降回零完成", LogType.OperateLog);
                                        });
                                    }
                                    else IsEmptyPause = false;
                                }
                            }
                        }

                        bool x24 = io[16] == "1"; //X024_Load门感应
                        if (CheckEdgeSingal(x24, ref LoaderDoorRiseCache, true))
                        {
                            //Load门开启定时器.Stop();
                            RunLog("Load仓门已关闭", LogType.OperateLog);
                            if (设备状态 != 0)
                            {
                                if (Load正在下降回零 && Tray盘模式) IsLoadPause = true;
                            }
                        }
                        else if (CheckEdgeSingal(x24, ref LoaderDoorFallCache, false))
                        {
                            RunLog("Load仓门被打开", LogType.OperateLog);
                            //if (等待Load门开启)
                            //{
                            //    Load门开启定时器.Start();
                            //}

                            if (设备状态 != 0)
                            {
                                if (Load下降回零开关 && 设备状态 == 2)
                                {
                                    if (!Load正在下降回零)
                                    {
                                        Load正在下降回零 = true;
                                        IsLoadPause = false;
                                        Load舱门开启标记 = 1;
                                        Load下降回零标记 = 2;
                                        RunLog("Load正在执行下降回零...", LogType.OperateLog);
                                        Task task = Task.Run(() =>
                                        {
                                            while (true)
                                            {
                                                if (Load下降回零标记 == 1)
                                                {
                                                    Load舱门上顶标记 = true;
                                                    Load舱门开启标记 = 0;
                                                    break;
                                                }
                                                else Thread.Sleep(10);
                                            }
                                            Load正在下降回零 = false;
                                            RunLog("Load下降回零完成", LogType.OperateLog);
                                        });
                                    }
                                    else IsLoadPause = false;
                                }
                            }
                        }

                        bool traymove = io[17] == "1"; //X038_BIN平移按键
                        if (CheckEdgeSingal(traymove, ref TrayMoveRiseCache, true))
                        {
                            RunLog("按下Tray平移", LogType.OperateLog);

                            if (设备状态 == 1) continue;

                            result = motionEx.GetAxisEnable("M11_Fix3放IC轴", out bool enable);
                            if (result == "Success")
                            {
                                if (!Tray平移运动标记 && enable && 设备状态 != 1)//H5509归零
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
                                            else RunLog("放Tary盘轴移动失败!" + result, LogType.OperateLog);
                                        }

                                        else if (Tray平移左右标记 == 1)
                                        {
                                            result = Fix3放IC盘轴移动(0, 1);
                                            if (result == "Success")
                                            {
                                                Tray平移左右标记 = 0;
                                            }
                                            else RunLog("放Tary盘轴移动失败!" + result, LogType.OperateLog);
                                        }

                                        motionEx.SetDo("Y040_BIN平移按键灯", false);

                                        Tray平移运动标记 = false;
                                    });
                                }
                            }
                            else RunLog("获取M11_Fix3放IC轴状态失败!" + result, LogType.OperateLog);
                        }

                        bool x36 = io[18] == "1";  //X036_Auto1门感应
                        if (CheckEdgeSingal(x36, ref Auto1DoorRiseCache, true))
                        {
                            RunLog("Auto1仓门已关闭", LogType.OperateLog);
                        }
                        else if (CheckEdgeSingal(x36, ref Auto1DoorFallCache, false))
                        {
                            RunLog("Auto1仓门被打开", LogType.OperateLog);
                            if (设备状态 != 0)
                            {
                                if (!Auto1料仓打开标记)
                                {
                                    Auto1料仓打开标记 = true;
                                }
                            }
                        }

                        bool x42 = io[19] == "1"; //X042_Auto2门感应
                        if (CheckEdgeSingal(x42, ref Auto2DoorRiseCache, true))
                        {
                            RunLog("Auto2仓门已关闭", LogType.OperateLog);
                        }
                        else if (CheckEdgeSingal(x42, ref Auto2DoorFallCache, false))
                        {
                            RunLog("Auto2仓门被打开", LogType.OperateLog);

                            if (设备状态 != 0)
                            {
                                if (!Auto2料仓打开标记)
                                {
                                    Auto2料仓打开标记 = true;
                                }
                            }
                        }

                        bool x46 = io[20] == "1"; //X046_FIX1盘感应

                        if (CheckEdgeSingal(x46, ref Fix1TrayRiseCache, true))
                        {
                            if (!Fix1Tray盘放置标记) Fix1Tray盘放置标记 = true;
                            RunLog("Fix1已放置Tray盘", LogType.OperateLog);
                        }
                        else if (CheckEdgeSingal(x46, ref Fix1TrayFallCache, false))
                        {
                            if (!Fix1Tray盘拿走标记) Fix1Tray盘拿走标记 = true;
                            RunLog("Fix1Tray盘被拿走", LogType.OperateLog);
                        }

                        bool x47 = io[21] == "1"; //X047_FIX2盘感应

                        if (CheckEdgeSingal(x47, ref Fix2TrayRiseCache, true))
                        {
                            if (!Fix2Tray盘放置标记) Fix2Tray盘放置标记 = true;
                            RunLog("Fix2已放置Tray盘", LogType.OperateLog);
                        }
                        else if (CheckEdgeSingal(x47, ref Fix2TrayFallCache, false))
                        {
                            if (!Fix2Tray盘拿走标记) Fix2Tray盘拿走标记 = true;
                            RunLog("Fix2Tray盘被拿走", LogType.OperateLog);
                        }

                        bool x45 = io[22] == "0"; //X045_FIX3盘对射

                        if (CheckEdgeSingal(x45, ref Fix3TrayRiseCache, true))
                        {
                            if (!Fix3Tray盘放置标记) Fix3Tray盘放置标记 = true;
                            RunLog("Fix3已放置Tray盘", LogType.OperateLog);
                        }
                        else if (CheckEdgeSingal(x45, ref Fix3TrayFallCache, false))
                        {
                            if (!Fix3Tray盘拿走标记) Fix3Tray盘拿走标记 = true;
                            RunLog("Fix3Tray盘被拿走", LogType.OperateLog);
                        }

                        if (等待Load门开启)
                        {
                            if (io[11] == "0")
                            {
                                if (!Load门开启定时器.Enabled)
                                {
                                    Load门开启定时器.Start();
                                }
                            }
                            else
                            {
                                if (Load门开启定时器.Enabled)
                                {
                                    Load门开启定时器.Stop();
                                }
                            }
                        }

                        if (!IsPause && (设备状态 == 1 || H5509正在归零))
                        {
                            if (!安全门禁用)
                            {
                                if (io[11] == "0" || io[12] == "0" || io[13] == "0" || io[14] == "0")
                                {
                                    if (io[11] == "0")
                                    {
                                        Listcode.Add("9000000");
                                        RunLog("[Code]9000000,[Msg](运行中)前门禁被打开", LogType.AlarmLog);
                                    }
                                    if (io[12] == "0")
                                    {
                                        Listcode.Add("9000010");
                                        RunLog("[Code]9000010,[Msg](运行中)后门禁被打开", LogType.AlarmLog);
                                    }
                                    if (io[13] == "0")
                                    {
                                        Listcode.Add("9000020");
                                        RunLog("[Code]9000020,[Msg](运行中)左门禁被打开", LogType.AlarmLog);
                                    }
                                    if (io[14] == "0")
                                    {
                                        Listcode.Add("9000040");
                                        RunLog("[Code]9000040,[Msg](运行中)右门禁被打开", LogType.AlarmLog);
                                    }
                                    ProgramPause = true;
                                    IsPause = true;
                                }
                            }

                            if (io[27] == 气压报警)
                            {
                                Listcode.Add("1400020"); //系统错误--气压不足
                                RunLog("[Code]1400020,[Msg]整机气压异常", LogType.AlarmLog);
                                ProgramPause = true;
                                IsPause = true;
                            }
                        }
                    }
                    Thread.Sleep(6);
                }
            }
        }

        public string 整机回零流程()
        {
            Task<string> task1 = Task.Run(() => { return 入料手臂Z回零(new bool[] { true, true, true, true }); });
            Task<string> task2 = Task.Run(() => { return 出料手臂Z回零(new bool[] { true, true, true, true }); });
            Task<string> task3 = Task.Run(() => { return 测试手臂Z回零(); });

            if (task1.Result != "Success" || task2.Result != "Success" || task3.Result != "Success")
            {
                string errortext = "";
                if (task1.Result != "Success") errortext += task1.Result + ";";
                if (task2.Result != "Success") errortext += task2.Result + ";";
                if (task3.Result != "Success") errortext += task3.Result + ";";
                return errortext;
            }

            string result = motionEx.GetDiHome(new string[] { "M18_放Tary盘轴" }, out string[] home);
            if (result != "Success")
            {
                return "读取放Tary盘轴原点信号失败！" + result;
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
                string errortext = "";
                if (task1.Result != "Success") errortext += task1.Result + ";";
                if (task2.Result != "Success") errortext += task2.Result + ";";
                if (task3.Result != "Success") errortext += task3.Result + ";";
                if (task4.Result != "Success") errortext += task4.Result + ";";
                if (task5.Result != "Success") errortext += task5.Result + ";";
                if (task6.Result != "Success") errortext += task6.Result + ";";
                return errortext;
            }

            task1 = Task.Run(() => { return 放Tary盘轴回零(); });
            task2 = Task.Run(() => { return 飞梭轴移动(初始点位.梭车2_后_Y); });

            if (task1.Result != "Success" || task2.Result != "Success")
            {
                string errortext = "";
                if (task1.Result != "Success") errortext += task1.Result + ";";
                if (task2.Result != "Success") errortext += task2.Result + ";";
                return errortext;
            }

            if (温控加热 && 温度模式 == "高温")
            {
                result = 梭车加热开启();
                if (result != "Success")
                {
                    return result;
                }
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

        public string 整机强制归零()
        {
            string result = "";
            if (温控加热 && 温度模式 == "高温")
            {
                result = 梭车加热关闭();
                if (result != "Success")
                {
                    return result;
                }
            }

            Task<string> task1 = Task.Run(() => { return 入料手臂Z回零(new bool[] { true, true, true, true }); });
            Task<string> task2 = Task.Run(() => { return 出料手臂Z回零(new bool[] { true, true, true, true }); });
            Task<string> task3 = Task.Run(() => { return 测试手臂Z回零(); });

            if (task1.Result != "Success" || task2.Result != "Success" || task3.Result != "Success")
            {
                string errortext = "";
                if (task1.Result != "Success") errortext += task1.Result + ";";
                if (task2.Result != "Success") errortext += task2.Result + ";";
                if (task3.Result != "Success") errortext += task3.Result + ";";
                return errortext;
            }

            result = motionEx.GetDiHome(new string[] { "M18_放Tary盘轴" }, out string[] home);
            if (result != "Success")
            {
                return "读取放Tary盘轴原点信号失败！" + result;
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

            //Task<string> task4 = Task.Run(() => { return 顶盘轴回零(); });

            Task<string> task5 = Task.Run(() => { return 飞梭回零(); });

            //Task<string> task6 = Task.Run(() => { return Fix3放IC轴回零(); });

            if (task1.Result != "Success" || task2.Result != "Success" || task3.Result != "Success" || task5.Result != "Success")//|| task4.Result != "Success"  || task6.Result != "Success"
            {
                string errortext = "";
                if (task1.Result != "Success") errortext += task1.Result + ";";
                if (task2.Result != "Success") errortext += task2.Result + ";";
                if (task3.Result != "Success") errortext += task3.Result + ";";
                //if (task4.Result != "Success") errortext += task4.Result + ";";
                if (task5.Result != "Success") errortext += task5.Result + ";";
                //if (task6.Result != "Success") errortext += task6.Result + ";";
                return errortext;
            }

            //task1 = Task.Run(() => { return 放Tary盘轴回零(); });
            task2 = Task.Run(() => { return 飞梭轴移动(飞梭2位置 == 0 ? (初始点位.梭车2_后_Y + 点位微调.梭车2_后_Y) : (初始点位.梭车2_前_Y + 初始点位.梭车2_前_Y)); });

            if (task2.Result != "Success")//task1.Result != "Success" || 
            {
                string errortext = "";
                //if (task1.Result != "Success") errortext += task1.Result + ";";
                if (task2.Result != "Success") errortext += task2.Result + ";";
                return errortext;
            }

            if (温控加热 && 温度模式 == "高温")
            {
                result = 梭车加热开启();
                if (result != "Success")
                {
                    return result;
                }
            }

            return "Success";
        }

        public void 初始化流程()
        {
            PS初始化流程.StepID = 500;
            while (true)
            {
                try
                {
                    //Thread.Sleep(10);

                    //if (APPmaster.IsForecePause)
                    //{
                    //    PS初始化流程.State = 1;
                    //    continue;
                    //}

                    if (H5509归零) Thread.Sleep(10);
                    else Thread.Sleep(100);

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
                                PS初始化流程.Processing = true;
                                PS初始化流程.StepID = 505;
                                if (!空跑模式 || Tray盘模式)
                                {
                                    Empty上顶标记 = 2;
                                    Load上顶标记 = 2;
                                }
                            }
                            else
                            {
                                if (IsForecePause)
                                {
                                    bool ispause = PS结批流程.State == 1 && PS入料手臂吸放料流程.State == 1 && PS出料手臂吸放料流程.State == 1 && PS测试手臂取待测料流程.State == 1 && PS测试手臂取已测料流程.State == 1 &&
                                        PS测试区气缸下压动作流程.State == 1 && PS测试区气缸上抬动作流程.State == 1 && PS空盘上顶流程.State == 1 && PS空盘下降流程.State == 1 && PS空盘下降回零流程.State == 1 &&
                                        PS待测盘上顶流程.State == 1 && PS待测盘下降回零流程.State == 1 && PS待测盘下降流程.State == 1 && PS手动盘1上顶流程.State == 1 && PS手动盘1下降回零流程.State == 1 &&
                                        PS手动盘2上顶流程.State == 1 && PS手动盘2下降回零流程.State == 1 && PS入料手臂放料流程.State == 1 && PS抓盘手臂取Empty空盘流程.State == 1 && PS抓盘手臂取Load空盘流程.State == 1 &&
                                        PS抓盘手臂放空盘到Empty流程.State == 1 && PS抓盘手臂放空盘到Auto1流程.State == 1 && PS抓盘手臂放空盘到Auto2流程.State == 1 && PS抓盘手臂给Auto1空盘流程.State == 1 &&
                                        PS抓盘手臂给Auto2空盘流程.State == 1 && PS抓盘手臂换Auto1空盘流程.State == 1 && PS抓盘手臂换Auto2空盘流程.State == 1 && PS入飞梭移动流程.State == 1 && PS整机回零流程.State == 1;
                                    if (ispause)
                                    {
                                        RunLog($"PS初始化流程.StepID={PS初始化流程.StepID}->执行强制归零");
                                        PS初始化流程.StepID = 605;
                                        //执行强制归零
                                    }
                                }
                            }
                            break;

                        case 505:
                            PS初始化流程.StepName = "各轴移动到初始点";
                            RunLog($"PS初始化流程.StepID={PS初始化流程.StepID}->{PS初始化流程.StepName}");
                            Task<string> auto1 = Task.Run(() => { return !空跑模式 || Tray盘模式 ? Auto1顶盘轴移动(初始点位.Auto1_初始位) : "Success"; });
                            Task<string> auto2 = Task.Run(() => { return !空跑模式 || Tray盘模式 ? Auto2顶盘轴移动(初始点位.Auto2_初始位) : "Success"; });//182.715
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
                                if (!空跑模式 || Tray盘模式) PS初始化流程.StepID = 510;
                                else PS初始化流程.StepID = 530;
                            }
                            else
                            {
                                if (auto1.Result != "Success") RunLog("Auto1顶盘轴移动失败!" + auto1.Result);
                                if (auto2.Result != "Success") RunLog("Auto2顶盘轴移动失败!" + auto2.Result);
                                if (testp.Result != "Success") RunLog("测试吸嘴_张开气缸闭合失败!" + testp.Result);
                                if (testx.Result != "Success")
                                {
                                    Listcode.Add("2028007");
                                    RunLog($"[Code]2028007,[Msg]测试手臂X移动到等待位失败!" + testx.Result, LogType.AlarmLog);
                                    M21_测试手臂X轴ErrorMsg = testx.Result;
                                }
                                if (shuttle.Result != "Success")
                                {
                                    Listcode.Add("2027007");
                                    RunLog($"[Code]2027007,[Msg]梭车2移动到后方失败!" + testx.Result, LogType.AlarmLog);
                                    M20_飞梭轴ErrorMsg = testx.Result;
                                }
                                if (inputp.Result != "Success") RunLog("入料手臂P移动失败!" + inputp.Result);
                                if (outputxyp.Result != "Success")
                                {
                                    Listcode.Add("2030007");
                                    RunLog($"[Code]2030007,[Msg]出料手臂走到等待位失败!" + outputxyp.Result, LogType.AlarmLog);
                                    M23_出料手臂X轴ErrorMsg = outputxyp.Result;
                                }
                                if (fix3.Result != "Success") RunLog("Fix3放IC盘轴移动失败!" + fix3.Result);

                                APPmaster.ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 510:
                            PS初始化流程.StepName = "检测Auto1/Auto2是否有盘";
                            RunLog($"PS初始化流程.StepID={PS初始化流程.StepID}->{PS初始化流程.StepName}");
                            string result = Auto1_前Tray存在感应(out string value);
                            string result2 = Auto2_前Tray存在感应(out string value2);
                            if (result == "Success" && result2 == "Success")
                            {
                                if (value == "1" && value2 == "1") //返回0是挡到感应器,1是没有挡到感应器
                                {
                                    PS初始化流程.StepID = 515;
                                    RunLog($"PS初始化流程.StepID={PS初始化流程.StepID}->等待Empty上顶");
                                }
                                else //没有挡到感应器或者放歪感应器被挡到说明没有放盘成功，需重新放盘
                                {
                                    APPmaster.ProgramPause = true;
                                    IsPause = true;

                                    if (value != "0")
                                    {
                                        Listcode.Add("4020001");
                                        RunLog($"[Code]4020001,[Msg]Auto1前有盘感应到有Tray盘({value})!", LogType.AlarmLog);
                                    }
                                    if (value2 != "0")
                                    {
                                        Listcode.Add("4021001");
                                        RunLog($"[Code]4021001,[Msg]Auto2前有盘感应到有Tray盘({value2})!", LogType.AlarmLog);
                                    }
                                }
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                if (result != "Success")
                                {
                                    Listcode.Add("4020001");
                                    RunLog($"[Code]4020001,[Msg]读取Auto1_前Tray存在感应失败!" + result, LogType.AlarmLog);
                                }
                                if (result2 != "Success")
                                {
                                    Listcode.Add("4021001");
                                    RunLog($"[Code]4021001,[Msg]读取Auto2_前Tray存在感应失败!" + result2, LogType.AlarmLog);
                                }
                            }
                            break;

                        case 515:
                            PS初始化流程.StepName = "等待Empty上顶";
                            if (Empty上顶标记 == 1)
                            {
                                Auto等待换盘标记 = true;
                                抓盘手臂放空盘到Auto1标记 = 2;
                                PS初始化流程.StepID = 520;
                                RunLog($"PS初始化流程.StepID={PS初始化流程.StepID}->抓盘手臂放空盘到Auto1");
                            }
                            else Thread.Sleep(10);
                            break;

                        case 520:
                            PS初始化流程.StepName = "抓盘手臂放空盘到Auto1";
                            if (抓盘手臂放空盘到Auto1标记 == 1)
                            {
                                抓盘手臂放空盘到Auto2标记 = 2;
                                PS初始化流程.StepID = 525;
                                RunLog($"PS初始化流程.StepID={PS初始化流程.StepID}->抓盘手臂放空盘到Auto2");
                            }
                            else Thread.Sleep(10);
                            break;

                        case 525:
                            PS初始化流程.StepName = "抓盘手臂放空盘到Auto2";
                            if (抓盘手臂放空盘到Auto2标记 == 1)
                            {
                                PS初始化流程.StepID = 530;
                                RunLog($"PS初始化流程.StepID={PS初始化流程.StepID}->等待Load上顶");
                            }
                            else Thread.Sleep(10);
                            break;

                        case 530:
                            PS初始化流程.StepName = "等待Load上顶";
                            if (Load上顶标记 == 1)
                            {
                                IndexTime = DateTime.Now;
                                Auto等待换盘标记 = false;
                                H5509初始化 = true;
                                H5509正在初始化 = false;
                                PS初始化流程.StepID = 599;
                            }
                            else Thread.Sleep(10);
                            break;

                        case 599:
                            PS初始化流程.StepName = $"初始化流程完成";
                            RunLog($"PS初始化流程.StepID={PS初始化流程.StepID}->{PS初始化流程.StepName}");
                            PS初始化流程.Processing = false;
                            PS初始化流程.StepID = 500;
                            break;

                        case 605:
                            PS初始化流程.StepName = $"执行强制归零";
                            result = 整机强制归零();
                            if (result == "Success")
                            {

                            }
                            break;

                        default:
                            RunLog(("PS初始化流程.StepID =" + PS初始化流程.StepID.ToString() + "对应流程不存在"));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    string errormsg = $"PS初始化流程({PS初始化流程.StepID})运行异常,详细信息:" + ex.Message;
                    RunLog(errormsg, LogType.AlarmLog);
                    系统未定义错误ErrorMsg = errormsg;
                    ProgramPause = true;
                    IsPause = true;
                    Listcode.Add("1400070");
                }
            }
        }

        public void 结批流程()
        {
            bool auto1isding = false;
            bool auto2isding = false;
            string result = "";
            PS结批流程.StepID = 500;
            while (true)
            {
                try
                {
                    Thread.Sleep(10);

                    if (APPmaster.IsForecePause)
                    {
                        PS结批流程.State = 1;
                        continue;
                    }

                    if (结批标记 || 一个循环标记) Thread.Sleep(10);
                    else Thread.Sleep(100);

                    if (!是否可以初始化())
                    {
                        continue;
                    }

                    switch (PS结批流程.StepID)
                    {
                        case 500:
                            PS结批流程.StepName = "路径规划";
                            bool process = !PS出料手臂吸放料流程.Processing
                                        && !PS入料手臂吸放料流程.Processing
                                        && !PS测试手臂取待测料流程.Processing
                                        && !PS测试手臂取已测料流程.Processing
                                        && !PS入飞梭移动流程.Processing
                                        && !PS空盘上顶流程.Processing
                                        && !PS空盘下降流程.Processing
                                        && !PS待测盘上顶流程.Processing
                                        && !PS手动盘1上顶流程.Processing
                                        && !PS手动盘2上顶流程.Processing
                                        && !PS抓盘手臂取Empty空盘流程.Processing
                                        && !PS抓盘手臂取Load空盘流程.Processing
                                        && !PS抓盘手臂放空盘到Empty流程.Processing
                                        && !PS抓盘手臂放空盘到Auto1流程.Processing
                                        && !PS抓盘手臂放空盘到Auto2流程.Processing;

                            PS结批流程.Condition = 结批标记 && H5509归零 && H5509初始化 && !正在结批 && !结批完成 && 设备状态 == 1 && 飞梭停止工作;

                            PS结批流程.Permit = !PS结批流程.Processing && process;

                            if (PS结批流程.Condition && PS结批流程.Permit)
                            {
                                正在结批 = true;
                                RunLog("正在结批", LogType.OperateLog);
                                RunLog($"PS结批流程.StepID={PS结批流程.StepID}->正在结批...");
                                PS结批流程.Processing = true;
                                PS结批流程.StepID = 505;
                            }
                            else
                            {
                                bool condition = 一个循环标记 && H5509归零 && H5509初始化 && !一个循环完成 && 设备状态 == 1 && 飞梭停止工作;
                                if (process && condition)
                                {
                                    ProgramPause = true;
                                    IsPause = true;
                                    Thread.Sleep(50); //等待程序停止

                                    一个循环标记 = false;
                                    一个循环完成 = true;

                                    飞梭停止工作 = false;
                                    入料手臂工作完成标记 = false;
                                    入料手臂停止工作标记 = false;
                                    测试手臂入料停止工作标记 = false;
                                    测试手臂出料停止工作标记 = false;
                                    出料手臂出料停止工作标记 = false;
                                    加热盘停止入料标记 = false;
                                    飞梭后入料状态 = 0;

                                    检测梭车是否有IC遗留();
                                    OneCycle按钮闪烁的定时器.Stop();
                                    Thread.Sleep(10); //等待输出完成
                                    motionEx.SetDo("Y009_一个循环灯", false);

                                    Listcode.Add("1400100");
                                    RunLog($"[Code]1400100,[Msg]一次循环完成", LogType.AlarmLog);
                                }
                                Thread.Sleep(10);
                            }
                            break;

                        case 505:
                            PS结批流程.StepName = $"出入料手臂走到避让位置";
                            RunLog($"PS结批流程.StepID={PS结批流程.StepID}->{PS结批流程.StepName}");
                            Task<string> input = Task.Run(() => { return 入料手臂移动(10, 10, 7.5); });
                            Task<string> output = Task.Run(() => { return 出料手臂移动(10, 10, 0); });
                            if (input.Result == "Success" && output.Result == "Success")
                            {
                                if (!空跑模式 || Tray盘模式)
                                {
                                    Load下降回零标记 = 2;
                                    Auto1上顶标记 = 2;
                                    Auto2上顶标记 = 2;
                                    Auto等待换盘标记 = true;
                                    bool isIC1 = 获取Auto1盘坐标(false, out int auto1xPoint, out int auto1yPoint);
                                    if (!isIC1 || auto1xPoint != 0 || auto1yPoint != 0) //如果有放料才需要盖空盘
                                    {
                                        auto1isding = true;
                                        抓盘手臂放空盘到Auto1标记 = 2;
                                    }
                                    PS结批流程.StepID = 510;
                                    RunLog($"PS结批流程.StepID={PS结批流程.StepID}->抓盘手臂抓空盘到Auto1");
                                }
                                else PS结批流程.StepID = 599;
                            }
                            else
                            {
                                if (input.Result != "Success")
                                {
                                    Listcode.Add("2029007");
                                    RunLog($"[Code]2029007,[Msg]入料手臂移到避让位置失败!" + input.Result, LogType.AlarmLog);
                                    M22_入料手臂X轴ErrorMsg = input.Result;
                                }
                                if (output.Result != "Success")
                                {
                                    Listcode.Add("2030007");
                                    RunLog($"[Code]2030007,[Msg]出料手臂移到避让位置失败!" + output.Result, LogType.AlarmLog);
                                    M23_出料手臂X轴ErrorMsg = output.Result;
                                }
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 510:
                            PS结批流程.StepName = $"抓盘手臂抓空盘到Auto1";
                            if (抓盘手臂放空盘到Auto1标记 == 1 && Auto1上顶标记 == 1) //给Auto2盖空盘时要等Auto1上顶完成
                            {
                                bool isIC2 = 获取Auto2盘坐标(false, out int auto2xPoint, out int auto2yPoint);
                                if (!isIC2 || auto2xPoint != 0 || auto2yPoint != 0) //如果有放料才需要盖空盘
                                {
                                    auto2isding = true;
                                    抓盘手臂放空盘到Auto2标记 = 2;
                                }
                                PS结批流程.StepID = 515;
                                RunLog($"PS结批流程.StepID={PS结批流程.StepID}->抓盘手臂抓空盘到Auto2");
                            }
                            else Thread.Sleep(10);
                            break;

                        case 515:
                            PS结批流程.StepName = $"抓盘手臂抓空盘到Auto2";
                            if (抓盘手臂放空盘到Auto2标记 == 1)
                            {
                                if (auto1isding) Auto1上顶标记 = 2;
                                if (auto2isding) Auto2上顶标记 = 2;
                                PS结批流程.StepID = 520;
                                RunLog($"PS结批流程.StepID={PS结批流程.StepID}->等待Auto1和Auto2上顶");
                            }
                            else Thread.Sleep(10);
                            break;

                        case 520:
                            PS结批流程.StepName = $"等待Auto1和Auto2上顶";
                            if (Auto1上顶标记 == 1 && Auto2上顶标记 == 1)
                            {
                                Empty下降回零标记 = 2;
                                Auto1下降回零标记 = 2;
                                Auto2下降回零标记 = 2;
                                PS结批流程.StepID = 525;
                                RunLog($"PS结批流程.StepID={PS结批流程.StepID}->等待Load,Empty,Auto1和Auto2下降回零");
                            }
                            else Thread.Sleep(10);
                            break;

                        case 525:
                            PS结批流程.StepName = $"出料手臂移动到梭车上避让";
                            RunLog($"PS结批流程.StepID={PS结批流程.StepID}->{PS结批流程.StepName}");
                            double[] xpos = 获取梭车xyp吸料点位(1);
                            result = 出料手臂移动(xpos[0], xpos[1], xpos[2]);
                            if (result == "Success")
                            {
                                PS结批流程.StepID = 530;
                            }
                            else
                            {
                                Listcode.Add("2030007");
                                RunLog($"[Code]2030007,[Msg]出料手臂移动到梭车上避让失败!" + result, LogType.AlarmLog);
                                M23_出料手臂X轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 530:
                            PS结批流程.StepName = $"等待Load,Empty,Auto1和Auto2下降回零";
                            if (Load下降回零标记 == 1 && Empty下降回零标记 == 1 && Auto1下降回零标记 == 1 && Auto2下降回零标记 == 1)
                            {
                                PS结批流程.StepID = 599;
                            }
                            else Thread.Sleep(10);
                            break;

                        case 599:
                            PS结批流程.StepName = $"数据处理";
                            RunLog($"PS结批流程.StepID={PS结批流程.StepID}->数据处理");
                            设备状态 = 0;
                            正在结批 = false;
                            结批标记 = false;
                            结批完成 = true;
                            H5509归零 = false;
                            H5509初始化 = false;
                            Auto等待换盘标记 = false;
                            StationManagers.SetIsconnectTester(false); //Com.IsconnectTester = false;
                            结束时间 = DateTime.Now;

                            LoaderTrayClear();
                            HeatTrayClear();
                            Auto1TrayClear();
                            Auto2TrayClear();
                            Fix1TrayClear();
                            Fix2TrayClear();
                            Fix3TrayClear();

                            RunLog($"吸取模式:{(空跑模式 ? "空跑模式" : "正常模式")}", LogType.OperateLog);
                            RunLog($"测试模式:{(无测试机模式 ? "离线" : "在线")}", LogType.OperateLog);
                            RunLog($"开始时间:{开始时间:yyyy-MM-dd HH:mm:ss.fff}", LogType.OperateLog);
                            RunLog($"按下结批时间:{按下结批时间:yyyy-MM-dd HH:mm:ss.fff}", LogType.OperateLog);
                            RunLog($"结批时间:" + 结批时间.ToString(@"hh\:mm\:ss"), LogType.OperateLog);
                            RunLog($"结束时间:{结束时间:yyyy-MM-dd HH:mm:ss.fff}", LogType.OperateLog);
                            if (批次运行时间.TotalDays >= 1) RunLog($"运行时间:" + 批次运行时间.ToString(@"dd\.hh\:mm\:ss"), LogType.OperateLog);
                            else RunLog($"运行时间:" + 批次运行时间.ToString(@"hh\:mm\:ss"), LogType.OperateLog);
                            RunLog(($"UPH:" + UPHNum.ToString()), LogType.OperateLog);
                            RunLog($"载入数:{载入数}", LogType.OperateLog);
                            RunLog($"总计:{出料数}", LogType.OperateLog);
                            RunLog($"Auto1:{Auto1数量},良率:{Auto1良率:0.00}%", LogType.OperateLog);
                            RunLog($"Auto2:{Auto2数量},良率:{Auto2良率: 0.00}%", LogType.OperateLog);
                            RunLog($"Fix1:{Fix1数量},良率:{Fix1良率: 0.00}%", LogType.OperateLog);
                            RunLog($"Fix2:{Fix2数量},良率:{Fix2良率: 0.00}%", LogType.OperateLog);
                            RunLog($"Fix3:{Fix3数量},良率:{Fix3良率: 0.00}%", LogType.OperateLog);

                            SaveBatchData();

                            Summary(3, null, GetEndLotData());

                            结批按钮闪烁的定时器.Stop();
                            motionEx.SetDo("Y008_结批灯", false);
                            Listcode.Add("1400130");
                            RunLog($"[Code]1400130,[Msg]结批完成", LogType.AlarmLog);

                            bool isok = int.TryParse(LotNo[8], out int lotCout);
                            if (!isok) lotCout = 载入数;

                            if (载入数 != 出料数 || 载入数 != lotCout)
                            {
                                Listcode.Add("1400443");
                                RunLog($"[Code]1400443,[Msg]载入数、总数和批次数量不一致", LogType.AlarmLog);
                            }

                            bool isIC3 = 获取Fix1盘坐标(false, 0, Fix1YPoint.Length, out int fix1xPoint, out int fix1yPoint);
                            if (!isIC3 || fix1xPoint != 0 || fix1yPoint != 0) //如果有放料才需要盖空盘
                            {
                                Listcode.Add("1400469");
                                RunLog($"[Code]1400469,[Msg]请取走Fix1Tray盘", LogType.AlarmLog);
                            }

                            bool isIC4 = 获取Fix2盘坐标(false, 0, Fix2YPoint.Length, out int fix2xPoint, out int fix2yPoint);
                            if (!isIC4 || fix2xPoint != 0 || fix2yPoint != 0) //如果有放料才需要盖空盘
                            {
                                Listcode.Add("1400470");
                                RunLog($"[Code]1400470,[Msg]请取走Fix2Tray盘", LogType.AlarmLog);
                            }

                            bool isIC5 = 获取Fix3盘坐标(false, 0, Fix3YPoint.Length, out int fix3xPoint, out int fix3yPoint);
                            if (!isIC5 || fix3xPoint != 0 || fix3yPoint != 0) //如果有放料才需要盖空盘
                            {
                                Listcode.Add("1400471");
                                RunLog($"[Code]1400471,[Msg]请取走Fix3Tray盘", LogType.AlarmLog);
                            }

                            检测梭车是否有IC遗留();

                            //保存工单dut信息
                            result = Sqlite.runSql(SQLiteRunningString, "Select * From DutCurrent", out DataTable dataTable);
                            if (result == "Success")
                            {
                                string tableName = $"dut_{LotNo[0].Replace("-", "_")}" + "_" + 开始时间.ToString("yyyyMMdd_HH_mm_ss");
                                int line = SqlServer.runSqlNonQuery(SQLServerConnString, $"create table {tableName}(id nchar(10) not NULL,pass int not NULL,total int not NULL,usedTime varchar(50) not NULL)");
                                if (line >= -1)
                                {
                                    SqlServer.runSql(SQLServerConnString, dataTable, tableName);
                                }
                            }
                            //SQLiteHelper.runSqlNonQuery(SQLiteRunningString, $"select * into dut_{LotNo[0] + "_" + 开始时间.ToString("yyyyMMdd_HH_mm_ss")} from DutCurrent");

                            //合并dut信息
                            Sqlite.runSqlNonQuery(SQLiteRunningString, $"UPDATE DutAll SET pass = pass + (SELECT dc.pass FROM DutCurrent dc WHERE dc.id = DutAll.id), total = total + (SELECT dc.total FROM DutCurrent dc WHERE dc.id = DutAll.id), usedTime = CAST(usedTime AS INTEGER) + (SELECT dc.usedTime FROM DutCurrent dc WHERE dc.id = DutAll.id) WHERE EXISTS ( SELECT 1 FROM DutCurrent dc WHERE dc.id = DutAll.id)");

                            RunLog($"结批完成", LogType.OperateLog);
                            RunLog($"PS结批流程.StepID={PS结批流程.StepID}->结批流程完成");

                            PS结批流程.StepID = 500;
                            PS结批流程.Processing = false;
                            ProgramPause = true;
                            IsPause = true;
                            break;

                        default:
                            RunLog(("PS结批流程.StepID =" + PS结批流程.StepID.ToString() + "对应流程不存在"));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    string errormsg = $"PS结批流程({PS结批流程.StepID})运行异常,详细信息:" + ex.Message;
                    RunLog(errormsg, LogType.AlarmLog);
                    系统未定义错误ErrorMsg = errormsg;
                    ProgramPause = true;
                    IsPause = true;
                    Listcode.Add("1400070");
                }
            }
            RunLog(("PS结批流程循环退出!"));
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
            int num2 = 0;

            bool firstdraw = true; //是否第一次重吸
            bool firstdzhome = true; //是否第一次Z回原
            Stopwatch stopwatch = new Stopwatch();
            bool[] homeopen = new bool[4];
            bool ismovewait = false;
            bool iswait = false;

            bool[] countnum = new bool[4];

            PS入料手臂吸放料流程.StepID = 500;
            while (true)
            {
                try
                {
                    Thread.Sleep(10);

                    if (APPmaster.IsForecePause)
                    {
                        PS入料手臂吸放料流程.State = 1;
                        continue;
                    }

                    if (!是否可以开始运行())
                    {
                        continue;
                    }

                    switch (PS入料手臂吸放料流程.StepID)
                    {
                        case 500:
                            PS入料手臂吸放料流程.StepName = "路径规划";

                            bool ispass = true;

                            if (温度模式 == "常温") //常温
                            {
                                if (结批标记 || 一个循环标记)
                                {
                                    ispass = false;
                                }
                            }
                            else
                            {
                                if (结批标记 || 一个循环标记)
                                {
                                    if (加热盘停止入料标记)
                                    {
                                        ispass = false;
                                    }
                                }
                            }

                            PS入料手臂吸放料流程.Condition = Load上顶标记 == 1 && !Auto1换盘标记 && !Auto2换盘标记 && ispass
                                                            //&& ((!结批标记 && 温度模式 == 0) || (!结批标记 && Tray停止入料 && 加热盘停止入料 && 温度模式 == 1)) 
                                                            && !结批触发定时器.Enabled;

                            PS入料手臂吸放料流程.Permit = !PS入料手臂放料流程.Processing;
                            if (PS入料手臂吸放料流程.Condition && PS入料手臂吸放料流程.Permit)
                            {
                                PS入料手臂吸放料流程.Processing = true;
                                PS入料手臂吸放料流程.StepID = 505;
                            }
                            else
                            {
                                if (结批标记 || 一个循环标记)
                                {
                                    if (温度模式 == "常温")
                                    {
                                        入料手臂工作完成标记 = true;
                                        入料手臂停止工作标记 = true;
                                        正在放置前入料[0] = new bool[] { false, false, false, false };
                                        正在放置前入料[1] = new bool[] { false, false, false, false };
                                    }
                                    else
                                    {
                                        if (加热盘停止入料标记)
                                        {
                                            入料手臂工作完成标记 = true;
                                            入料手臂停止工作标记 = true;
                                            正在放置前入料[0] = new bool[] { false, false, false, false };
                                            正在放置前入料[1] = new bool[] { false, false, false, false };
                                        }
                                    }
                                }
                                if (Auto1换盘标记 || Auto2换盘标记)
                                {
                                    PS入料手臂吸放料流程.StepID = 905;
                                    PS入料手臂吸放料流程StepID = 500;
                                    PS入料手臂吸放料流程Processing = false;
                                }
                                Thread.Sleep(10);
                            }
                            break;

                        #region 规划流程

                        case 505:
                            PS入料手臂吸放料流程.StepName = "计算放料IC颗数";

                            if (入料手臂抓料等待标记)
                            {
                                if (Auto1换盘标记 || Auto2换盘标记) //2025-07-19 10:39 新bug
                                {
                                    RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->避让Auto1换盘({Auto1换盘标记})或Auto2换盘({Auto2换盘标记})");
                                    PS入料手臂吸放料流程.StepID = 905;
                                    PS入料手臂吸放料流程StepID = 505;
                                    PS入料手臂吸放料流程Processing = true;
                                    break;
                                }
                            }

                            if (飞梭前入料放料参数(out colums, out open))
                            {
                                if (!入料手臂抓料等待标记)
                                {
                                    if (温度模式 == "高温")
                                    {
                                        int count = open.Count(b => b); //获取true的个数
                                        if (获取Heat盘加热好的指定IC个数(2, true, count, out num2)) //如果已经加热好需要的IC个数
                                        {
                                            opens = 吸嘴是否吸料(open);  //到加热盘吸取IC -> 放到梭车(直接放)
                                            正在放置前入料[colums] = new bool[] { open[0], open[1], open[2], open[3] };
                                            入料手臂抓料等待标记 = false;
                                            PS入料手臂吸放料流程.StepID = 615;
                                        }
                                        else //如果没有加热好
                                        {
                                            if (!结批标记 && !一个循环标记)
                                            {
                                                PS入料手臂吸放料流程.StepID = 511;
                                            }
                                            else
                                            {
                                                if (!获取Heat盘加热好的指定IC个数(1, true, 1, out num2)) //如果所有IC都已加热完成
                                                {
                                                    if (获取Heat盘指定状态IC个数(2, out num2)) //如果至少有一个加热好的IC
                                                    {
                                                        int num3 = 0;
                                                        opens = new int[] { 2, 2, 2, 2 };
                                                        for (int i = 0; i < open.Length; i++)
                                                        {
                                                            if (open[i])
                                                            {
                                                                opens[i] = 0;
                                                                if (++num3 >= num2) break;
                                                            }
                                                        }
                                                        正在放置前入料[colums] = new bool[] { opens[0] == 0, opens[1] == 0, opens[2] == 0, opens[3] == 0 };
                                                        入料手臂抓料等待标记 = false;
                                                        PS入料手臂吸放料流程.StepID = 615;
                                                    }
                                                    else
                                                    {
                                                        加热盘停止入料标记 = true;
                                                        PS入料手臂吸放料流程.StepID = 500;
                                                        PS入料手臂吸放料流程.Processing = false;
                                                    }
                                                }
                                                else //所有放在加热盘的IC已经加热好
                                                {
                                                    Thread.Sleep(10);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        opens = 吸嘴是否吸料(open);
                                        正在放置前入料[colums] = new bool[] { open[0], open[1], open[2], open[3] };
                                        入料手臂抓料等待标记 = false;
                                        PS入料手臂吸放料流程.StepID = 515;
                                    }
                                }
                                else
                                {
                                    入料手臂抓料等待标记 = false; //验证结果
                                    PS入料手臂吸放料流程.StepID = 705;
                                }
                            }
                            else
                            {
                                if (!入料手臂抓料等待标记)
                                {
                                    if (入料手臂工作完成标记 == false)
                                    {
                                        入料手臂工作完成标记 = true; //入料手臂目标达成
                                        PS入料手臂吸放料流程.StepID = 500;
                                        PS入料手臂吸放料流程.Processing = false;
                                    }
                                    else PS入料手臂吸放料流程.StepID = 510;
                                }
                                else Thread.Sleep(10);
                            }
                            break;

                        case 510:
                            PS入料手臂吸放料流程.StepName = "检测后入料IC是否被拿走";
                            if (飞梭后入料放料参数(out colums, out open))
                            {
                                if (温度模式 == "高温")
                                {
                                    int count = open.Count(b => b); //获取true的个数
                                    if (获取Heat盘加热好的指定IC个数(2, true, count, out num2)) //如果已经加热好需要的IC个数
                                    {
                                        opens = 吸嘴是否吸料(open);  //到加热盘吸取IC -> 放到梭车(直接放)
                                        正在放置前入料[colums] = new bool[] { open[0], open[1], open[2], open[3] };
                                        入料手臂抓料等待标记 = true;
                                        PS入料手臂吸放料流程.StepID = 615;
                                    }
                                    else //如果没有加热好
                                    {
                                        if (!结批标记 && !一个循环标记)
                                        {
                                            PS入料手臂吸放料流程.StepID = 511;
                                        }
                                        else
                                        {
                                            if (!获取Heat盘加热好的指定IC个数(1, true, 1, out num2)) //如果所有IC都已加热完成
                                            {
                                                if (获取Heat盘指定状态IC个数(2, out num2)) //如果至少有一个加热好的IC
                                                {
                                                    int num3 = 0;
                                                    opens = new int[] { 2, 2, 2, 2 };
                                                    for (int i = 0; i < open.Length; i++)
                                                    {
                                                        if (open[i])
                                                        {
                                                            opens[i] = 0;
                                                            if (++num3 >= num2) break;
                                                        }
                                                    }
                                                    正在放置前入料[colums] = new bool[] { opens[0] == 0, opens[1] == 0, opens[2] == 0, opens[3] == 0 };
                                                    入料手臂抓料等待标记 = true;
                                                    PS入料手臂吸放料流程.StepID = 615;
                                                }
                                                else
                                                {
                                                    加热盘停止入料标记 = true;
                                                    PS入料手臂吸放料流程.StepID = 500;
                                                    PS入料手臂吸放料流程.Processing = false;
                                                }
                                            }
                                            else //所有放在加热盘的IC已经加热好
                                            {
                                                Thread.Sleep(10);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    opens = 吸嘴是否吸料(open);
                                    正在放置前入料[colums] = new bool[] { open[0], open[1], open[2], open[3] };
                                    入料手臂抓料等待标记 = true;
                                    PS入料手臂吸放料流程.StepID = 515;
                                }
                            }
                            else
                            {
                                if (温度模式 == "常温")
                                {
                                    if (ismovewait)
                                    {
                                        PS入料手臂吸放料流程.StepID = 500;
                                        PS入料手臂吸放料流程.Processing = false;
                                    }
                                    else PS入料手臂吸放料流程.StepID = 1515;
                                }
                                else PS入料手臂吸放料流程.StepID = 511;
                            }
                            break;

                        case 511:
                            if (!结批标记 && !一个循环标记 && 获取Heat盘加热好的指定IC个数(0, false, 1, out num2)) //如果State：0的状态数量 >= 1 说明加热盘还没有放满
                            {
                                if (获取Heat盘未放IC的指定IC个数(0, out int num)) //如果需要放置的IC个数 >=1 && <=4
                                {
                                    opens = new int[] { 2, 2, 2, 2 }; //到Load吸取IC -> 加热盘
                                    for (int i = 0; i < num; i++)
                                    {
                                        opens[i] = 0;
                                    }
                                    //正在放置前入料[colums] = new bool[] { opens[0] == 0, opens[1] == 0, opens[2] == 0, opens[3] == 0 };
                                }
                                PS入料手臂吸放料流程.StepID = 515;
                            }
                            else //加热盘已经放满
                            {
                                PS入料手臂吸放料流程.StepID = 500;
                                PS入料手臂吸放料流程.Processing = false;
                            }

                            break;

                        case 1515:
                            PS入料手臂吸放料流程.StepName = "获取Load盘坐标";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                            if (获取Load盘坐标(true, out xPoint, out yPoint))
                            {
                                PS入料手臂吸放料流程.StepID = 1520;
                            }
                            else //获取不到坐标说明Tray盘吸完了, 要避让并换盘
                            {
                                if (!结批标记)
                                {
                                    if (!空跑模式 || Tray盘模式)
                                    {
                                        PS入料手臂吸放料流程.StepID = 1516;
                                    }
                                    else LoaderTrayClear();
                                }
                                else
                                {
                                    PS入料手臂吸放料流程.StepID = 500;
                                    PS入料手臂吸放料流程.Processing = false;
                                }
                            }
                            break;

                        case 1516:
                            if (!Auto1换盘标记 && !Auto2换盘标记)
                            {
                                iswait = true;
                                Load换盘标记 = true;
                                PS入料手臂吸放料流程.StepID = 805;
                            }
                            else
                            {
                                PS入料手臂吸放料流程.StepID = 905;
                                PS入料手臂吸放料流程StepID = 1516;
                                PS入料手臂吸放料流程Processing = true;
                            }
                            break;

                        case 1520:
                            PS入料手臂吸放料流程.StepName = "XYP轴移动到Load吸料位置等待";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                            result = 入料手臂移动(LoaderXPoint[xPoint], LoaderYPoint[yPoint], 入料吸料间距); //50, 300, 10 写去哪里吸料，至于去哪个位置吸，走多少由APPmaster决定
                            if (result == "Success")
                            {
                                ismovewait = true; //已经在等待位标记
                                PS入料手臂吸放料流程.StepID = 500;
                                PS入料手臂吸放料流程.Processing = false;
                            }
                            else
                            {
                                Listcode.Add("2029007");
                                RunLog($"[Code]2029007,[Msg]XYP轴移动到Load吸料位置等待失败!" + result, LogType.AlarmLog);
                                M22_入料手臂X轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        #endregion

                        #region 从加热盘吸取IC

                        case 615:
                            PS入料手臂吸放料流程.StepName = "获取Heat盘坐标";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                            if (获取Heat盘坐标(2, true, out xPoint, out yPoint))
                            {
                                PS入料手臂吸放料流程.StepID = 620;
                            }
                            else
                            {
                                Thread.Sleep(10);
                                //后续如果跳过了，可能会出现没有2的情况
                            }
                            break;

                        case 620:
                            PS入料手臂吸放料流程.StepName = "获取Heat盘吸料信息";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                            result = APPmaster.获取Heat盘Open(opens, xPoint, yPoint, 0, true, 2, out lsopen, out index);
                            if (result == "Success")
                            {
                                countnum = new bool[] { lsopen[0], lsopen[1], lsopen[2], lsopen[3] };
                                xPos = HeatXPoint[xPoint] + 点位微调.入料_Heat吸_X;
                                yPos = (HeatYPoint[yPoint] + index * HeatInterval * Heat_Y_Pitch) + 点位微调.入料_Heat吸_Y;
                                PS入料手臂吸放料流程.StepID = 625;
                            }
                            break;

                        case 625:
                            PS入料手臂吸放料流程.StepName = "XYP轴移动到Heat盘吸料位置";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                            result = 入料手臂移动(xPos, yPos, 入料吸放料Heat间距 + 点位微调.入料_Heat_变距); //50, 300, 10 写去哪里吸料，至于去哪个位置吸，走多少由APPmaster决定
                            if (result == "Success")
                            {
                                firstdraw = true;
                                ismovewait = false;
                                PS入料手臂吸放料流程.StepID = 630;
                            }
                            else
                            {
                                Listcode.Add("2029007");
                                RunLog($"[Code]2029007,[Msg]XYP轴移动到Heat盘吸料位置失败!" + result, LogType.AlarmLog);
                                M22_入料手臂X轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 630:
                            PS入料手臂吸放料流程.StepName = "入料手臂Z向下移动";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");

                            if (设置信息.测试机信息.入料吸提前开真空 && !空跑模式)
                            {
                                入料吸嘴_吸放料(lsopen, true, true);
                            }

                            result = 入料手臂Z移动(lsopen, 初始点位.入料_Heat吸_Z + 点位微调.入料_Heat吸_Z, true);
                            if (result == "Success")
                            {
                                firstdzhome = true;
                                PS入料手臂吸放料流程.StepID = 635;
                            }
                            break;

                        case 635:
                            PS入料手臂吸放料流程.StepName = "入料吸嘴吸料";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                            result = 空跑模式 ? "Success" : 入料吸嘴_吸放料(lsopen, true, true);
                            if (result == "Success")
                            {
                                PS入料手臂吸放料流程.StepID = 640;
                            }
                            break;

                        case 640:
                            PS入料手臂吸放料流程.StepName = "入料手臂Z真空检测";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                            if (!空跑模式)
                            {
                                stopwatch.Start();
                                while (JudgeNozzleSts(lsopen, opens, 0))
                                {
                                    if (stopwatch.ElapsedMilliseconds > 设置信息.测试机信息.入料手臂吸料超时时间)
                                    {
                                        RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->入料吸料超时");
                                        break;
                                    }

                                    result = 入料吸嘴_真空检测(out ioval);
                                    if (result != "Success") break;

                                    RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{ioval[0]},{ioval[1]},{ioval[2]},{ioval[3]}:{stopwatch.ElapsedMilliseconds}ms");

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

                                bool[] bools = new bool[4] { lsopen[0] && opens[0] == 0, lsopen[1] && opens[1] == 0, lsopen[2] && opens[2] == 0, lsopen[3] && opens[3] == 0 };
                                入料吸嘴_吸放料(bools, true, false);
                            }
                            else
                            {
                                result = "Success";

                                for (int i = 0; i < lsopen.Length; i++)
                                {
                                    if (lsopen[i]) opens[i] = 1;
                                }
                                Thread.Sleep(设置信息.测试机信息.入料手臂吹料时间);
                            }

                            if (result == "Success")
                            {
                                PS入料手臂吸放料流程.StepID = 645;
                            }
                            else
                            {
                                ProgramPause = true;
                                IsPause = true;
                                RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->入料真空检测失败" + result);
                            }
                            break;

                        case 645:
                            PS入料手臂吸放料流程.StepName = "入料手臂Z向上移动";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                            result = 入料手臂Z移动(lsopen, -0.3, false);
                            if (result == "Success")
                            {
                                if (!空跑模式)
                                {
                                    //上来再侦测一次
                                    result = 入料吸嘴_真空检测(out ioval);
                                    if (result != "Success") break;

                                    RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{ioval[0]},{ioval[1]},{ioval[2]},{ioval[3]}");

                                    for (int i = 0; i < ioval.Length; i++)
                                    {
                                        if (lsopen[i])
                                        {
                                            if (opens[i] == 1 && ioval[i] == "0")
                                            {
                                                opens[i] = 0;
                                            }
                                        }
                                    }

                                    bool[] bools = new bool[4] { lsopen[0] && opens[0] == 0, lsopen[1] && opens[1] == 0, lsopen[2] && opens[2] == 0, lsopen[3] && opens[3] == 0 };
                                    入料吸嘴_吸放料(bools, true, false);
                                }

                                for (int i = 0; i < lsopen.Length; i++)
                                {
                                    if (i + index == lsopen.Length) break;
                                    if (lsopen[i + index])
                                    {
                                        if (opens[i + index] == 1)
                                        {
                                            lsopen[i + index] = false;
                                            HeatDicPoint[$"{xPoint},{yPoint + i * HeatInterval}"] = false;
                                            HeatDicTimer[$"{xPoint},{yPoint + i * HeatInterval}"].Status = 0;
                                            //RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->入料Z{i + index + 1}吸料成功");
                                        }
                                        else
                                        {
                                            if (countnum[i + index])
                                            {
                                                countnum[i + index] = false;
                                                入料手臂吸料不起数量++;
                                            }
                                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->入料Z{i + index + 1}吸料失败");
                                        }
                                    }
                                }
                                PS入料手臂吸放料流程.StepID = 650;
                            }
                            break;

                        case 650:
                            PS入料手臂吸放料流程.StepName = "入料手臂Z回原感应";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                            result = 入料吸嘴_回原检测(out home);
                            if (result == "Success")
                            {
                                if (!home.Contains("1") && !lsopen.Contains(true)) //如果有1说明有吸嘴没有挡住原点
                                {
                                    PS入料手臂吸放料流程.StepID = 652;
                                }
                                else
                                {
                                    if (home.Contains("1"))
                                    {
                                        for (int i = 0; i < home.Length; i++)
                                        {
                                            if (home[i] == "1") { RunLog($"入料Z{i + 1}没有挡住原点"); }
                                        }
                                        if (firstdzhome)
                                        {
                                            firstdzhome = false;
                                        }
                                        else
                                        {
                                            for (int i = 0; i < home.Length; i++)
                                            {
                                                if (home[i] == "1")
                                                {
                                                    Listcode.Add($"140046{i}");
                                                    RunLog($"[Code]140046{i},[Msg]入料Z{i + 1}没有挡住原点", LogType.AlarmLog);
                                                }
                                            }
                                            ProgramPause = true;
                                            APPmaster.IsPause = true;
                                        }
                                    }
                                    PS入料手臂吸放料流程.StepID = 651;
                                }
                            }
                            break;

                        case 651:
                            PS入料手臂吸放料流程.StepName = "入料手臂Z回原";
                            homeopen = new bool[] { home[0] == "1", home[1] == "1", home[2] == "1", home[3] == "1" };
                            result = 入料手臂Z回零(homeopen);
                            if (result == "Success")
                            {
                                if (home.Contains("1"))
                                {
                                    PS入料手臂吸放料流程.StepID = 650;
                                }
                                else
                                {
                                    PS入料手臂吸放料流程.StepID = 652;
                                }
                            }
                            break;

                        case 652:
                            PS入料手臂吸放料流程.StepName = "更新LoadMap";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                            if (lsopen.Contains(true)) //有ture说明还有吸嘴没吸上来
                            {
                                if (firstdraw)
                                {
                                    firstdraw = false;
                                    PS入料手臂吸放料流程.StepID = 630; //重吸
                                }
                                else
                                {
                                    for (int i = 0; i < lsopen.Length; i++)
                                    {
                                        if (lsopen[i] && opens[i] == 0)
                                        {
                                            Listcode.Add($"30{i + 1}0000");
                                            RunLog($"[Code]30{i + 1}0000,[Msg]入料Z{i + 1}吸料失败!", LogType.AlarmLog);
                                        }
                                    }

                                    入料IC忽略 = false;
                                    ProgramPause = true;
                                    IsPause = true;
                                    motionEx.SetDo("Y004_重试灯", true);
                                    PS入料手臂吸放料流程.StepID = 653;
                                }
                            }
                            else
                            {
                                PS入料手臂吸放料流程.StepID = 655;
                            }
                            break;

                        case 653:
                            if (入料IC忽略)
                            {
                                for (int i = 0; i < lsopen.Length; i++)
                                {
                                    if (i + index == lsopen.Length) break;
                                    if (lsopen[i + index])
                                    {
                                        lsopen[i + index] = false;
                                        opens[i + index] = 2;
                                        HeatDicPoint[$"{xPoint},{yPoint + i * HeatInterval}"] = false;
                                        HeatDicTimer[$"{xPoint},{yPoint + i * HeatInterval}"].Timer.Stop();
                                        HeatDicTimer[$"{xPoint},{yPoint + i * HeatInterval}"].Status = 0;
                                    }
                                }
                                PS入料手臂吸放料流程.StepID = 655;
                            }
                            else //直接点开始默认重吸
                            {
                                PS入料手臂吸放料流程.StepID = 630;
                            }
                            break;

                        case 655:
                            PS入料手臂吸放料流程.StepName = "更新opens";
                            if (opens.Contains(0)) ////如果有0说明还有吸嘴需要吸料
                            {
                                RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{opens[0]} , {opens[1]} , {opens[2]} , {opens[3]}");
                                PS入料手臂吸放料流程.StepID = 615;
                            }
                            else
                            {
                                RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{opens[0]} , {opens[1]} , {opens[2]} , {opens[3]}");
                                for (int i = 0; i < opens.Length; i++)
                                {
                                    if (opens[i] == 1)
                                    {
                                        lsopen[i] = true;
                                    }
                                    else lsopen[i] = false;
                                }
                                InNozzleICOpen = new bool[] { lsopen[0], lsopen[1], lsopen[2], lsopen[3] }; //更新吸嘴状态
                                InNozzleIC = true;
                                PS入料手臂吸放料流程.StepID = 699;
                            }
                            break;

                        case 699:
                            PS入料手臂吸放料流程.StepName = "入料手臂_Heat盘吸料流程完成";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                            PS入料手臂吸放料流程.StepID = 705;
                            break;

                        #endregion

                        #region 从Tray盘吸取IC

                        case 515:
                            PS入料手臂吸放料流程.StepName = "获取Load盘坐标";
                            if (获取Load盘坐标(true, out xPoint, out yPoint))
                            {
                                RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->获取到Load盘坐标");
                                PS入料手臂吸放料流程.StepID = 520;
                            }
                            else
                            {
                                if (!空跑模式 || Tray盘模式)
                                {
                                    if (!结批标记) //在这里判断为True，说明无待测盘时点了结批
                                    {
                                        if (!Auto1换盘标记 && !Auto2换盘标记) //如果没有换盘标记
                                        {
                                            iswait = false;
                                            Load换盘标记 = true;
                                            PS入料手臂吸放料流程.StepID = 805;
                                        }
                                        else
                                        {
                                            PS入料手臂吸放料流程.StepID = 905;
                                            PS入料手臂吸放料流程StepID = 515;
                                            PS入料手臂吸放料流程Processing = true;
                                        }
                                    }
                                    else //判断吸嘴上有没有料
                                    {
                                        结批按钮闪烁的定时器.Start();
                                        if (opens.Contains(1)) //如果吸嘴上有料
                                        {
                                            for (int i = 0; i < opens.Length; i++)
                                            {
                                                if (opens[i] != 1)
                                                {
                                                    opens[i] = 2;
                                                    lsopen[i] = false;
                                                }
                                            }
                                            入料吸嘴_吸放料(new bool[] { opens[0] == 2, opens[1] == 2, opens[2] == 2, opens[3] == 2 }, true, false); //防止一直吸真空
                                            PS入料手臂吸放料流程.StepID = 555;
                                        }
                                        else //如果吸嘴上没有料,则直接停止放料
                                        {
                                            for (int i = 0; i < opens.Length; i++)
                                            {
                                                opens[i] = 2;
                                                lsopen[i] = false;
                                            }
                                            入料吸嘴_吸放料(new bool[] { true, true, true, true }, true, false); //防止一直吸真空
                                            PS入料手臂吸放料流程.StepID = 555;
                                        }
                                    }
                                }
                                else LoaderTrayClear();
                            }
                            break;

                        case 520:
                            PS入料手臂吸放料流程.StepName = "获取吸料信息";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                            result = APPmaster.获取Load盘Open(opens, xPoint, yPoint, out lsopen, out index);
                            if (result == "Success")
                            {
                                countnum = new bool[] { lsopen[0], lsopen[1], lsopen[2], lsopen[3] };
                                xPos = LoaderXPoint[xPoint] + 点位微调.入料_Load吸_X;
                                yPos = (LoaderYPoint[yPoint] + index * TrayInterval * Tray_Y_Pitch) + 点位微调.入料_Load吸_Y;
                                PS入料手臂吸放料流程.StepID = 525;
                            }
                            else
                            {
                                RunLog($"获取吸料信息失败!" + result);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 525:
                            PS入料手臂吸放料流程.StepName = "XYP轴移动到Load吸料位置";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                            result = 入料手臂移动(xPos, yPos, 入料吸料间距 + 点位微调.入料_Load_变距); //50, 300, 10 写去哪里吸料，至于去哪个位置吸，走多少由APPmaster决定
                            if (result == "Success")
                            {
                                ismovewait = false;
                                firstdraw = true;
                                PS入料手臂吸放料流程.StepID = 530;
                            }
                            else
                            {
                                Listcode.Add("2029007");
                                RunLog($"[Code]2029007,[Msg]XYP轴移动到Load吸料位置失败!" + result, LogType.AlarmLog);
                                M22_入料手臂X轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 530:
                            PS入料手臂吸放料流程.StepName = "入料手臂Z向下移动";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");

                            if (设置信息.测试机信息.入料吸提前开真空 && !空跑模式)
                            {
                                入料吸嘴_吸放料(lsopen, true, true);
                            }

                            result = 入料手臂Z移动(lsopen, 初始点位.入料_Load_Z + 点位微调.入料_Load吸_Z, true);
                            if (result == "Success")
                            {
                                firstdzhome = true;
                                PS入料手臂吸放料流程.StepID = 535;
                            }
                            else
                            {
                                RunLog($"入料手臂Z向下移动失败!" + result);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 535:
                            PS入料手臂吸放料流程.StepName = "入料吸嘴吸料";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                            result = 空跑模式 ? "Success" : 入料吸嘴_吸放料(lsopen, true, true);
                            if (result == "Success")
                            {
                                PS入料手臂吸放料流程.StepID = 540;
                            }
                            else
                            {
                                RunLog($"入料吸嘴吸料失败!" + result);
                                ProgramPause = true;
                                IsPause = true;
                            }

                            break;

                        case 540:
                            PS入料手臂吸放料流程.StepName = "入料手臂Z真空检测";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                            if (!空跑模式)
                            {
                                stopwatch.Start();
                                while (JudgeNozzleSts(lsopen, opens, 0))
                                {
                                    if (stopwatch.ElapsedMilliseconds > 设置信息.测试机信息.入料手臂吸料超时时间)
                                    {
                                        RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->入料吸料超时");
                                        break;
                                    }

                                    result = 入料吸嘴_真空检测(out ioval);
                                    if (result != "Success") break;

                                    RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{ioval[0]},{ioval[1]},{ioval[2]},{ioval[3]}:{stopwatch.ElapsedMilliseconds}ms");

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

                                bool[] bools = new bool[4] { lsopen[0] && opens[0] == 0, lsopen[1] && opens[1] == 0, lsopen[2] && opens[2] == 0, lsopen[3] && opens[3] == 0 };
                                入料吸嘴_吸放料(bools, true, false);
                            }
                            else
                            {
                                result = "Success";

                                for (int i = 0; i < lsopen.Length; i++)
                                {
                                    if (lsopen[i]) opens[i] = 1;
                                }
                                Thread.Sleep(设置信息.测试机信息.入料手臂吹料时间);
                            }

                            if (result == "Success")
                            {
                                PS入料手臂吸放料流程.StepID = 545;
                            }
                            else
                            {
                                ProgramPause = true;
                                IsPause = true;
                                RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->入料真空检测失败" + result);
                            }
                            break;

                        case 545:
                            PS入料手臂吸放料流程.StepName = "入料手臂Z向上移动";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                            result = 入料手臂Z移动(lsopen, -0.3, false);
                            if (result == "Success")
                            {
                                if (!空跑模式)
                                {
                                    //上来再侦测一次
                                    result = 入料吸嘴_真空检测(out ioval);
                                    if (result != "Success") break;

                                    RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{ioval[0]},{ioval[1]},{ioval[2]},{ioval[3]}");

                                    for (int i = 0; i < ioval.Length; i++)
                                    {
                                        if (lsopen[i])
                                        {
                                            if (opens[i] == 1 && ioval[i] == "0")
                                            {
                                                opens[i] = 0;
                                            }
                                        }
                                    }

                                    bool[] bools = new bool[4] { lsopen[0] && opens[0] == 0, lsopen[1] && opens[1] == 0, lsopen[2] && opens[2] == 0, lsopen[3] && opens[3] == 0 };
                                    入料吸嘴_吸放料(bools, true, false);
                                }

                                for (int i = 0; i < lsopen.Length; i++)
                                {
                                    if (i + index == lsopen.Length) break;
                                    if (lsopen[i + index])
                                    {
                                        if (opens[i + index] == 1)
                                        {
                                            lsopen[i + index] = false;
                                            LoaderDicPoint[$"{xPoint},{yPoint + i * TrayInterval}"] = false;
                                            //RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->入料Z{i + index + 1}吸料成功");
                                        }
                                        else
                                        {
                                            if (countnum[i + index])
                                            {
                                                countnum[i + index] = false;
                                                入料手臂吸料不起数量++;
                                            }
                                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->入料Z{i + index + 1}吸料失败");
                                        }
                                    }
                                }
                                PS入料手臂吸放料流程.StepID = 550;
                            }
                            break;

                        case 550:
                            PS入料手臂吸放料流程.StepName = "入料手臂Z回原感应";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
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
                                            if (home[i] == "1") { RunLog($"入料Z{i + 1}没有挡住原点"); }
                                        }
                                        if (firstdzhome)
                                        {
                                            firstdzhome = false;
                                        }
                                        else
                                        {
                                            for (int i = 0; i < home.Length; i++)
                                            {
                                                if (home[i] == "1")
                                                {
                                                    Listcode.Add($"140046{i}");
                                                    RunLog($"[Code]140046{i},[Msg]入料Z{i + 1}没有挡住原点", LogType.AlarmLog);
                                                }
                                            }
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
                            homeopen = new bool[] { home[0] == "1", home[1] == "1", home[2] == "1", home[3] == "1" };
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
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
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
                                            Listcode.Add($"30{i + 1}0000");
                                            RunLog($"[Code]30{i + 1}0000,[Msg]入料Z{i + 1}吸料失败!", LogType.AlarmLog);
                                        }
                                    }
                                    入料IC吸不起结批 = false;
                                    入料IC忽略 = false;
                                    ProgramPause = true;
                                    IsPause = true;
                                    motionEx.SetDo("Y004_重试灯", true);
                                    motionEx.SetDo("Y005_忽略灯", true);
                                    motionEx.SetDo("Y008_结批灯", true);
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
                                        LoaderDicPoint[$"{xPoint},{yPoint + i * TrayInterval}"] = false;
                                    }
                                }
                                if (按了跳过或编辑报警使能) 按了跳过或编辑标记 = true;
                                PS入料手臂吸放料流程.StepID = 555;
                            }
                            else if (入料IC吸不起结批)
                            {
                                结批标记 = true;
                                结批按钮闪烁的定时器.Start();
                                if (opens.Contains(1)) //如果吸嘴上有料
                                {
                                    for (int i = 0; i < lsopen.Length; i++)
                                    {
                                        if (lsopen[i])
                                        {
                                            opens[i] = 2;
                                            lsopen[i] = false;
                                        }
                                    }
                                    入料吸嘴_吸放料(new bool[] { opens[0] == 2, opens[1] == 2, opens[2] == 2, opens[3] == 2 }, true, false); //防止一直吸真空
                                    PS入料手臂吸放料流程.StepID = 555;
                                }
                                else //如果吸嘴上没有料,则直接停止放料
                                {
                                    for (int i = 0; i < opens.Length; i++)
                                    {
                                        opens[i] = 2;
                                        lsopen[i] = false;
                                    }
                                    入料吸嘴_吸放料(new bool[] { true, true, true, true }, true, false); //防止一直吸真空
                                    PS入料手臂吸放料流程.StepID = 555;
                                }

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
                                RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{opens[0]} , {opens[1]} , {opens[2]} , {opens[3]}");
                                PS入料手臂吸放料流程.StepID = 515;
                            }
                            else
                            {
                                RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{opens[0]} , {opens[1]} , {opens[2]} , {opens[3]}");
                                for (int i = 0; i < opens.Length; i++)
                                {
                                    if (opens[i] == 1)
                                    {
                                        载入数++;
                                    }
                                }
                                InNozzleICOpen = new bool[] { opens[0] == 1, opens[1] == 1, opens[2] == 1, opens[3] == 1 }; //更新吸嘴状态
                                InNozzleIC = true;
                                PS入料手臂吸放料流程.StepID = 599;
                            }
                            break;

                        case 599:
                            PS入料手臂吸放料流程.StepName = "入料手臂_吸料流程完成";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                            if (opens.Contains(1))
                            {
                                if (温度模式 == "常温") PS入料手臂吸放料流程.StepID = 705;
                                else PS入料手臂吸放料流程.StepID = 1005;
                            }
                            else
                            {
                                if (结批标记 || 一个循环标记 || 结批触发定时器.Enabled)
                                {
                                    PS入料手臂吸放料流程.StepID = 500;
                                    PS入料手臂吸放料流程.Processing = false;
                                }
                                else PS入料手臂吸放料流程.StepID = 505;
                            }

                            break;

                        #endregion

                        #region 将IC放入梭车

                        case 705: ////放料流程////
                            PS入料手臂吸放料流程.StepName = "XYP轴移动到飞梭放料位置";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");

                            if (Auto1换盘标记 || Auto2换盘标记) //2025-07-19 10:39 新bug
                            {
                                RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->避让Auto1换盘({Auto1换盘标记})或Auto2换盘({Auto2换盘标记})");
                                PS入料手臂吸放料流程.StepID = 905;
                                PS入料手臂吸放料流程StepID = 705;
                                PS入料手臂吸放料流程Processing = true;
                                break;
                            }

                            double[] xypPos = 入料手臂抓料等待标记 ? 获取梭车xyp放料点位2(colums) : 获取梭车xyp放料点位(colums); //抓着料在等待就取得要过来的梭车位置
                            result = 入料手臂移动(xypPos[0], xypPos[1], xypPos[2]); //入料手臂移动(xPos, yPos, 10); //写去哪里吸料，至于去哪个位置吸，走多少由APPmaster决定
                            if (result == "Success")
                            {
                                if (入料手臂抓料等待标记) PS入料手臂吸放料流程.StepID = 505;
                                else
                                {
                                    PS入料手臂吸放料流程.StepID = 710;
                                }
                            }
                            else
                            {
                                Listcode.Add("2029007");
                                RunLog($"[Code]2029007,[Msg]XYP轴移动到飞梭放料位置失败!" + result, LogType.AlarmLog);
                                M22_入料手臂X轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 710:
                            PS入料手臂吸放料流程.StepName = "入料手臂Z向下移动";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                            if (飞梭移动状态 == 0) //梭车停止状态才能放
                            {
                                InNozzleIC = false;
                                lsopen = InNozzleICOpen;
                                double zpos = 获取梭车Z放料点位(colums);
                                result = 入料手臂Z移动(lsopen, zpos, true);
                                if (result == "Success")
                                {
                                    firstdraw = true;
                                    PS入料手臂吸放料流程.StepID = 715;
                                    if (前方定点停止允许)
                                    {
                                        前方定点停止允许 = false;
                                        ProgramPause = true;
                                        IsPause = true;
                                        RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->入料手臂已定点停止完成");
                                    }
                                }
                                else
                                {
                                    RunLog($"入料手臂Z向下移动失败!" + result);
                                    ProgramPause = true;
                                    IsPause = true;
                                }
                            }
                            else Thread.Sleep(10);
                            break;

                        case 715:
                            PS入料手臂吸放料流程.StepName = "入料吸嘴放料";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                            result = 空跑模式 ? "Success" : 入料吸嘴_吸放料(lsopen, true, false); //关吸真空
                            if (result == "Success")
                            {
                                result = 空跑模式 ? "Success" : 入料吸嘴_吸放料(lsopen, false, true); //开破真空
                                if (result == "Success")
                                {
                                    Thread.Sleep(设置信息.测试机信息.入料手臂吹料时间);
                                    result = 空跑模式 ? "Success" : 入料吸嘴_吸放料(lsopen, false, false); //关破真空
                                    if (result == "Success")
                                    {
                                        PS入料手臂吸放料流程.StepID = 725;
                                    }
                                    else
                                    {
                                        RunLog($"入料吸嘴_关破真空失败!" + result);
                                        ProgramPause = true;
                                        IsPause = true;
                                    }
                                }
                                else
                                {
                                    RunLog($"入料吸嘴_开破真空失败!" + result);
                                    ProgramPause = true;
                                    IsPause = true;
                                }
                            }
                            else
                            {
                                RunLog($"入料吸嘴_关吸真空失败!" + result);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 720:
                            PS入料手臂吸放料流程.StepName = "入料手臂Z真空检测";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                            result = 入料吸嘴_真空检测(out ioval);
                            if (result == "Success")
                            {
                                PS入料手臂吸放料流程.StepID = 725;
                            }
                            else
                            {
                                RunLog($"入料手臂Z真空检测失败!" + result);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 725:
                            PS入料手臂吸放料流程.StepName = "入料手臂Z向上移动";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                            result = 入料手臂Z移动(lsopen, -0.3, false);
                            if (result == "Success")
                            {
                                PS入料手臂吸放料流程.StepID = 730;
                            }
                            else
                            {
                                RunLog($"入料手臂Z向上移动失败!" + result);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 730:
                            PS入料手臂吸放料流程.StepName = "入料手臂Z回原感应";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
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
                                    正在放置前入料[colums] = new bool[] { false, false, false, false };
                                    InNozzleICOpen = new bool[] { false, false, false, false };
                                    入料手臂抓料等待标记 = false;
                                    PS入料手臂吸放料流程.StepID = 799;
                                }
                                else
                                {
                                    for (int i = 0; i < home.Length; i++)
                                    {
                                        if (home[i] == "1") { RunLog($"入料Z{i + 1}没有挡住原点"); }
                                    }
                                    if (firstdzhome)
                                    {
                                        firstdzhome = false;
                                    }
                                    else
                                    {
                                        for (int i = 0; i < home.Length; i++)
                                        {
                                            if (home[i] == "1")
                                            {
                                                Listcode.Add($"140046{i}");
                                                RunLog($"[Code]140046{i},[Msg]入料Z{i + 1}没有挡住原点", LogType.AlarmLog);
                                            }
                                        }
                                        ProgramPause = true;
                                        APPmaster.IsPause = true;
                                    }
                                    PS入料手臂吸放料流程.StepID = 731;
                                }
                            }
                            else
                            {
                                RunLog($"入料吸嘴_回原检测失败!" + result);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 731:
                            PS入料手臂吸放料流程.StepName = "入料手臂Z回原";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                            homeopen = new bool[] { home[0] == "1", home[1] == "1", home[2] == "1", home[3] == "1" };
                            result = 入料手臂Z回零(homeopen);
                            if (result == "Success")
                            {
                                PS入料手臂吸放料流程.StepID = 730;
                            }
                            else
                            {
                                RunLog($"入料手臂Z回原失败!" + result);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 799:
                            PS入料手臂吸放料流程.StepName = $"入料手臂_放料第{colums + 1}列流程完成";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                            if (结批标记 || 一个循环标记 || 结批触发定时器.Enabled)
                            {
                                PS入料手臂吸放料流程.StepID = 500;
                                PS入料手臂吸放料流程.Processing = false;
                            }
                            else PS入料手臂吸放料流程.StepID = 505;
                            break;

                        #endregion

                        #region 将IC放入加热盘

                        case 1005:
                            PS入料手臂吸放料流程.StepName = $"获取Heat盘坐标位置";
                            opens = new int[] { InNozzleICOpen[0] ? 1 : 2, InNozzleICOpen[1] ? 1 : 2, InNozzleICOpen[2] ? 1 : 2, InNozzleICOpen[3] ? 1 : 2 };
                            if (获取Heat盘坐标(0, false, out xPoint, out yPoint))
                            {
                                RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->获取Heat盘坐标位置");
                                APPmaster.获取Heat盘Open(opens, xPoint, yPoint, 1, false, 0, out lsopen, out index);
                                xPos = HeatXPoint[xPoint] + 点位微调.入料_Heat放_X;
                                yPos = (HeatYPoint[yPoint] + index * HeatInterval * Heat_Y_Pitch) + 点位微调.入料_Heat放_Y; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                PS入料手臂吸放料流程.StepID = 1010;
                            }
                            else
                            {
                                //按道理不会出现无法获取坐标位置，因为前面已经根据加热盘获取到了数量
                            }
                            break;

                        case 1010:
                            PS入料手臂吸放料流程.StepName = "XYP轴移动到加热盘放料位置";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                            result = 入料手臂移动(xPos, yPos, 入料吸放料Heat间距 + 点位微调.入料_Heat_变距); //入料手臂移动(xPos, yPos, 10); //写去哪里吸料，至于去哪个位置吸，走多少由APPmaster决定
                            if (result == "Success")
                            {
                                //lsopen = InNozzleICOpen;
                                for (int i = 0; i < lsopen.Length; i++)
                                {
                                    if (lsopen[i])
                                    {
                                        InNozzleICOpen[i] = false;
                                    }
                                }
                                PS入料手臂吸放料流程.StepID = 1015;
                            }
                            else
                            {
                                Listcode.Add("2029007");
                                RunLog($"[Code]2029007,[Msg]XYP轴移动到加热盘放料位置失败!" + result, LogType.AlarmLog);
                                M22_入料手臂X轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 1015:
                            PS入料手臂吸放料流程.StepName = "入料手臂Z向下移动";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                            result = 入料手臂Z移动(lsopen, 初始点位.入料_Heat放_Z + 点位微调.入料_Heat放_Z, true);
                            if (result == "Success")
                            {
                                firstdraw = true;
                                PS入料手臂吸放料流程.StepID = 1020;

                                if (前方定点停止允许)
                                {
                                    前方定点停止允许 = false;
                                    ProgramPause = true;
                                    IsPause = true;
                                    RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->入料手臂已定点停止完成");
                                }
                            }
                            else
                            {
                                RunLog($"入料手臂Z向下移动失败!" + result);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 1020:
                            PS入料手臂吸放料流程.StepName = "入料吸嘴放料";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                            result = 空跑模式 ? "Success" : 入料吸嘴_吸放料(lsopen, true, false); //关吸真空
                            if (result == "Success")
                            {
                                result = 空跑模式 ? "Success" : 入料吸嘴_吸放料(lsopen, false, true); //开破真空
                                if (result == "Success")
                                {
                                    Thread.Sleep(设置信息.测试机信息.入料手臂吹料时间);
                                    result = 空跑模式 ? "Success" : 入料吸嘴_吸放料(lsopen, false, false); //关破真空
                                    if (result == "Success")
                                    {
                                        PS入料手臂吸放料流程.StepID = 1025;
                                    }
                                    else
                                    {
                                        RunLog($"入料吸嘴_关破真空失败!" + result);
                                        ProgramPause = true;
                                        IsPause = true;
                                    }
                                }
                                else
                                {
                                    RunLog($"入料吸嘴_开破真空失败!" + result);
                                    ProgramPause = true;
                                    IsPause = true;
                                }
                            }
                            else
                            {
                                RunLog($"入料吸嘴_关吸真空失败!" + result);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 1025:
                            PS入料手臂吸放料流程.StepName = "入料手臂Z真空检测";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                            if (!空跑模式)
                            {
                                stopwatch.Start();
                                while (JudgeNozzleSts(lsopen, opens, 1))
                                {
                                    if (stopwatch.ElapsedMilliseconds > 设置信息.测试机信息.入料手臂吸料超时时间)
                                    {
                                        RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->入料放料超时");
                                        break;
                                    }

                                    result = 入料吸嘴_真空检测(out ioval);
                                    if (result != "Success") break;

                                    RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{ioval[0]},{ioval[1]},{ioval[2]},{ioval[3]}:{stopwatch.ElapsedMilliseconds}ms");

                                    for (int i = 0; i < ioval.Length; i++)
                                    {
                                        if (lsopen[i])
                                        {
                                            if (ioval[i] == "0") opens[i] = 0;
                                        }
                                    }

                                    Thread.Sleep(20);
                                }

                                stopwatch.Stop();
                                stopwatch.Reset();
                            }
                            else
                            {
                                result = "Success";
                                for (int i = 0; i < lsopen.Length; i++)
                                {
                                    if (lsopen[i]) opens[i] = 0;
                                }
                                Thread.Sleep(设置信息.测试机信息.入料手臂吹料时间);
                            }

                            if (result == "Success")
                            {
                                PS入料手臂吸放料流程.StepID = 1030;
                            }
                            else
                            {
                                ProgramPause = true;
                                IsPause = true;
                                RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->入料真空检测失败" + result);
                            }
                            break;

                        case 1030:
                            PS入料手臂吸放料流程.StepName = "入料手臂Z向上移动";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                            result = 入料手臂Z移动(lsopen, -0.3, false);
                            if (result == "Success")
                            {

                                PS入料手臂吸放料流程.StepID = 1031;
                            }
                            else
                            {
                                RunLog($"入料手臂Z向上移动失败!" + result);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 1031:
                            PS入料手臂吸放料流程.StepName = "入料手臂Z真空检测";
                            if (!空跑模式)
                            {
                                result = 入料吸嘴_真空检测(out ioval);
                                if (result == "Success")
                                {
                                    for (int i = 0; i < ioval.Length; i++)
                                    {
                                        if (lsopen[i])
                                        {
                                            if (ioval[i] == "0") opens[i] = 0;
                                            else opens[i] = 1;
                                        }
                                    }
                                    PS入料手臂吸放料流程.StepID = 1032;
                                }
                                else
                                {
                                    ProgramPause = true;
                                    IsPause = true;
                                    RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->入料真空检测失败" + result);
                                }
                            }
                            else PS入料手臂吸放料流程.StepID = 1032;
                            break;

                        case 1032:
                            PS入料手臂吸放料流程.StepName = "heatMap更新";
                            for (int i = 0; i < lsopen.Length; i++)
                            {
                                if (i + index == lsopen.Length) break;
                                if (lsopen[i + index])
                                {
                                    if (opens[i + index] == 0)
                                    {
                                        lsopen[i + index] = false;
                                        HeatDicPoint[$"{xPoint},{yPoint + i * HeatInterval}"] = true;
                                        HeatDicTimer[$"{xPoint},{yPoint + i * HeatInterval}"].Status = 1;
                                        RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->入料Z{i + index + 1}放料成功");
                                    }
                                    else RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->入料Z{i + index + 1}放料失败");
                                }
                            }
                            PS入料手臂吸放料流程.StepID = 1035;
                            break;

                        case 1035:
                            PS入料手臂吸放料流程.StepName = "入料手臂Z回原感应";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                            result = 入料吸嘴_回原检测(out home);
                            if (result == "Success")
                            {
                                if (!home.Contains("1") && !lsopen.Contains(true)) //如果有1说明有吸嘴没有挡住原点
                                {
                                    PS入料手臂吸放料流程.StepID = 1040;
                                }
                                else
                                {
                                    if (home.Contains("1"))
                                    {
                                        for (int i = 0; i < home.Length; i++)
                                        {
                                            if (home[i] == "1") { RunLog($"入料Z{i + 1}没有挡住原点"); }
                                        }
                                        if (firstdzhome)
                                        {
                                            firstdzhome = false;
                                        }
                                        else
                                        {
                                            for (int i = 0; i < home.Length; i++)
                                            {
                                                if (home[i] == "1")
                                                {
                                                    Listcode.Add($"140046{i}");
                                                    RunLog($"[Code]140046{i},[Msg]入料Z{i + 1}没有挡住原点", LogType.AlarmLog);
                                                }
                                            }
                                            ProgramPause = true;
                                            APPmaster.IsPause = true;
                                        }
                                    }
                                    PS入料手臂吸放料流程.StepID = 1036;
                                }
                            }
                            else
                            {
                                RunLog($"入料吸嘴_回原检测失败!" + result);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 1036:
                            PS入料手臂吸放料流程.StepName = "入料手臂Z回原";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                            homeopen = new bool[] { home[0] == "1", home[1] == "1", home[2] == "1", home[3] == "1" };
                            result = 入料手臂Z回零(homeopen);
                            if (result == "Success")
                            {
                                if (home.Contains("1"))
                                {
                                    PS入料手臂吸放料流程.StepID = 1035;
                                }
                                else
                                {
                                    PS入料手臂吸放料流程.StepID = 1040;
                                }
                            }
                            else
                            {
                                RunLog($"入料手臂Z回原失败!" + result);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 1040:
                            PS入料手臂吸放料流程.StepName = "更新LoadMap";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                            if (lsopen.Contains(true)) //有ture说明还有吸嘴没放下去
                            {
                                if (firstdraw)
                                {
                                    firstdraw = false;
                                    PS入料手臂吸放料流程.StepID = 1015; //重放
                                }
                                else
                                {
                                    for (int i = 0; i < lsopen.Length; i++)
                                    {
                                        if (lsopen[i] && opens[i] == 1)
                                        {
                                            Listcode.Add($"30{i + 1}0002");
                                            RunLog($"[Code]30{i + 1}0002,[Msg]入料Z{i + 1}放料失败", LogType.AlarmLog);
                                        }
                                    }
                                    入料IC忽略 = false;
                                    motionEx.SetDo("Y004_重试灯", true);
                                    motionEx.SetDo("Y005_忽略灯", true);
                                    ProgramPause = true;
                                    IsPause = true;
                                    PS入料手臂吸放料流程.StepID = 1045;
                                }
                            }
                            else
                            {
                                PS入料手臂吸放料流程.StepID = 1050;
                            }
                            break;

                        case 1045:
                            if (入料IC忽略)
                            {
                                for (int i = 0; i < lsopen.Length; i++)
                                {
                                    if (i + index == lsopen.Length) break;
                                    if (lsopen[i + index])
                                    {
                                        lsopen[i + index] = false;
                                        opens[i + index] = 0;
                                    }
                                }
                                PS入料手臂吸放料流程.StepID = 1050;
                            }
                            else //直接点开始默认重放
                            {
                                PS入料手臂吸放料流程.StepID = 1015;
                            }
                            break;

                        case 1050:
                            PS入料手臂吸放料流程.StepName = "更新opens";
                            if (opens.Contains(1)) ////如果有1说明还有吸嘴需要放料
                            {
                                RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{opens[0]} , {opens[1]} , {opens[2]} , {opens[3]}");
                                PS入料手臂吸放料流程.StepID = 1005;
                            }
                            else
                            {
                                InNozzleIC = false;
                                InNozzleICOpen = new bool[] { false, false, false, false };
                                正在放置前入料[colums] = new bool[] { false, false, false, false };
                                RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{opens[0]} , {opens[1]} , {opens[2]} , {opens[3]}");
                                PS入料手臂吸放料流程.StepID = 1099;
                            }
                            break;

                        case 1099:
                            PS入料手臂吸放料流程.StepName = $"入料手臂_加热盘放料流程完成";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                            PS入料手臂吸放料流程.StepID = 500;
                            PS入料手臂吸放料流程.Processing = false;
                            break;

                        #endregion

                        #region load盘换空盘

                        case 805:
                            PS入料手臂吸放料流程.StepName = "入料手臂移到避让位";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                            result = 入料手臂移动(初始点位.入料_Heat_X, 0, 7.5);
                            if (result == "Success")
                            {
                                PS入料手臂吸放料流程.StepID = 810;
                                RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->判断抓盘手臂放空盘到Empty标记");
                            }
                            else
                            {
                                Listcode.Add("2029007");
                                RunLog($"[Code]2029007,[Msg]入料手臂移到避让位置失败!" + result, LogType.AlarmLog);
                                M22_入料手臂X轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 810:
                            PS入料手臂吸放料流程.StepName = "判断抓盘手臂放空盘到Empty标记";
                            if (抓盘手臂取Load空盘标记 == 1 && 抓盘手臂放空盘到Empty标记 == 1)
                            {
                                抓盘手臂取Load空盘标记 = 2;
                                PS入料手臂吸放料流程.StepID = 815;
                                RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->等待抓盘手臂取Load空盘完成");
                            }
                            else Thread.Sleep(10);
                            break;

                        case 815:
                            PS入料手臂吸放料流程.StepName = "等待抓盘手臂取Load空盘完成";
                            if (抓盘手臂取Load空盘标记 == 1) //当抓盘手臂移动到Empty上方才会变为1，所以不用担心入料手臂会撞抓盘手臂
                            {
                                PS入料手臂吸放料流程.StepID = 820;
                                RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->等待Load上顶完成");
                            }
                            else Thread.Sleep(10);
                            break;

                        case 820:
                            PS入料手臂吸放料流程.StepName = "等待Load上顶完成";
                            if (Load上顶标记 == 1)
                            {
                                Load换盘标记 = false;
                                if (结批标记)
                                {
                                    RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->无待测盘结批");
                                }
                                else
                                {
                                    if (!获取Load盘坐标(true, out xPoint, out yPoint)) //如果获取坐标成功说明Load盘被框选了，则不用清空
                                    {
                                        LoaderTrayClear();
                                    }
                                    RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->Load上顶完成Map更新");
                                }

                                if (iswait) PS入料手臂吸放料流程.StepID = 1515;
                                else PS入料手臂吸放料流程.StepID = 515;

                                iswait = false;
                            }
                            else Thread.Sleep(10);
                            break;

                        #endregion

                        #region Auto1和Auto2换空盘

                        case 905:
                            PS入料手臂吸放料流程.StepName = "入料手臂移到避让位";
                            RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->{PS入料手臂吸放料流程.StepName}");
                            result = 入料手臂移动(初始点位.入料_Heat_X, 0, 7.5);
                            if (result == "Success")
                            {
                                if (!入料手臂工作完成标记) ismovewait = false;
                                Auto等待换盘标记 = true;
                                PS入料手臂吸放料流程.StepID = 910;
                                RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->等待Auto1和Auto2换盘");
                            }
                            else
                            {
                                Listcode.Add("2029007");
                                RunLog($"[Code]2029007,[Msg]入料手臂移到避让位置失败!" + result, LogType.AlarmLog);
                                M22_入料手臂X轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 910:
                            PS入料手臂吸放料流程.StepName = "等待Auto1和Auto2换盘";
                            if (Auto1换盘标记 == false && Auto2换盘标记 == false)
                            {
                                Auto等待换盘标记 = false;
                                if (!Auto1换盘标记) RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->Auto1换盘完成");
                                if (!Auto2换盘标记) RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->Auto2换盘完成");
                                PS入料手臂吸放料流程.StepID = PS入料手臂吸放料流程StepID;
                                PS入料手臂吸放料流程.Processing = PS入料手臂吸放料流程Processing;
                            }
                            break;

                        #endregion

                        default:
                            RunLog(("PS入料手臂吸放料流程.StepID =" + PS入料手臂吸放料流程.StepID.ToString() + "对应流程不存在"));
                            break;

                    }

                }
                catch (Exception ex)
                {
                    string errormsg = $"PS入料手臂吸放料流程({PS入料手臂吸放料流程.StepID})运行异常,详细信息:" + ex.Message;
                    RunLog(errormsg, LogType.AlarmLog);
                    系统未定义错误ErrorMsg = errormsg;
                    ProgramPause = true;
                    IsPause = true;
                    Listcode.Add("1400070");
                }
            }
        }

        public void 出料手臂吸放料流程()
        {
            string result = "";
            int[] opens = new int[4];
            bool[][] open = new bool[2][];
            bool[] lsopen = new bool[4];
            string[][] bins = new string[2][]; //用于接收保存并用于放置
            string[][] bins2 = new string[2][]; //用于原始记录BIN项结果
            string[] bin = new string[4]; //临时保存用于放置
            string[] bin2 = new string[4]; //临时保存用于记录原始BIN项结果
            TimeSpan testtime = new TimeSpan(0); //记录测试测试时间
            int testsite = 0; //记录测试位置
            short colums = 0;
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
            bool[] countnum = new bool[4];

            Stopwatch stopwatch = new Stopwatch();
            PS出料手臂吸放料流程.StepID = 500;
            while (true)
            {
                try
                {
                    Thread.Sleep(10);

                    if (APPmaster.IsForecePause)
                    {
                        PS出料手臂吸放料流程.State = 1;
                        continue;
                    }

                    if (!是否可以开始运行())
                    {
                        continue;
                    }

                    switch (PS出料手臂吸放料流程.StepID)
                    {
                        case 500:
                            PS出料手臂吸放料流程.StepName = "路径规划";
                            PS出料手臂吸放料流程.Condition = 飞梭前出料状态 == 1 && !出料手臂出料停止工作标记;
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
                                bins2 = new string[2][]
                                {
                                    new string[]{ 飞梭前出料BIN项[0][0], 飞梭前出料BIN项[0][1], 飞梭前出料BIN项[0][2], 飞梭前出料BIN项[0][3]},
                                    new string[]{ 飞梭前出料BIN项[1][0], 飞梭前出料BIN项[1][1], 飞梭前出料BIN项[1][2], 飞梭前出料BIN项[1][3]},
                                };
                                testtime = 飞梭前出料TestTime;
                                testsite = 飞梭前出料TestSite;

                                bool isoth = false;
                                for (int i = 0; i < bins.Length; i++)
                                {
                                    for (int j = 0; j < bins[i].Length; j++)
                                    {
                                        if (!open[i][j]) continue;

                                        oth = bins[i][j];

                                        if (oth == 设置信息.通讯信息.超时Bin字符串)
                                        {
                                            oth = 设置信息.测试机信息.超时Bin字符串显示;
                                            bins[i][j] = oth;
                                        }

                                        if (Auto1 != "" && Auto1.Split(",").Contains(oth)) continue;
                                        else if (Auto2 != "" && Auto2.Split(",").Contains(oth)) continue;
                                        else if (Fix1 != "" && Fix1.Split(",").Contains(oth)) continue;
                                        else if (Fix2 != "" && Fix2.Split(",").Contains(oth)) continue;
                                        else if (Fix3 != "" && Fix3.Split(",").Contains(oth)) continue;
                                        else
                                        {
                                            if (!othAll.Contains(oth))
                                            {
                                                if (首次出现未知Bin项报警)
                                                {
                                                    isoth = true;
                                                    Listcode.Add("1400500");
                                                    RunLog($"[Code]1400500,[Msg]出现未知Bin项目：{oth}，本批次测试所有出现过的未知Bin项：{othAll}", LogType.AlarmLog);
                                                }
                                                else RunLog($"出现未知Bin项目：{oth}，本批次测试所有出现过的未知Bin项：{othAll}", LogType.RunLog);
                                                othAll += oth + ",";
                                            }
                                            bins[i][j] = "OTH";
                                        }
                                    }
                                }

                                if (isoth)
                                {
                                    ProgramPause = true;
                                    IsPause = true;
                                }

                                PS出料手臂吸放料流程.Processing = true;
                                PS出料手臂吸放料流程.StepID = 505;
                            }
                            else
                            {
                                if (!飞梭后出料IC[0].Contains(true) //飞梭后出料IC第1列无料
                                    && !飞梭后出料IC[1].Contains(true) //飞梭后出料IC第2列无料
                                    && !飞梭前出料IC[0].Contains(true) //飞梭前出料IC第1列无料
                                    && !飞梭前出料IC[1].Contains(true) //飞梭前出料IC第2列无料
                                    && 入料手臂停止工作标记
                                    && 测试手臂入料停止工作标记
                                    && 测试手臂出料停止工作标记
                                    && (结批标记 || 一个循环标记))
                                {
                                    出料手臂出料停止工作标记 = true;
                                }
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
                                bin2 = new string[] { bins2[0][0], bins2[0][1], bins2[0][2], bins2[0][3] };
                                PS出料手臂吸放料流程.StepID = 510;
                            }
                            else if (open[1].Contains(true))
                            {
                                colums = 1;
                                opens = new int[] { open[1][0] ? 0 : 2, open[1][1] ? 0 : 2, open[1][2] ? 0 : 2, open[1][3] ? 0 : 2 };
                                lsopen = new bool[] { open[1][0], open[1][1], open[1][2], open[1][3] };
                                bin = new string[] { bins[1][0], bins[1][1], bins[1][2], bins[1][3] };
                                bin2 = new string[] { bins2[1][0], bins2[1][1], bins2[1][2], bins2[1][3] };


                                PS出料手臂吸放料流程.StepID = 515;
                            }
                            else //放料完成
                            {
                                if (!ispass) //如果第二列没有料就会是为false
                                {
                                    飞梭前出料状态 = 0;
                                    PS出料手臂吸放料流程.StepID = 798;
                                }
                                else
                                {
                                    if (飞梭前出料状态 == 0 && !测试手臂出料停止工作标记)
                                    {
                                        PS出料手臂吸放料流程.StepID = 798;
                                    }
                                    else PS出料手臂吸放料流程.StepID = 799;
                                }
                            }
                            break;

                        case 510:
                            PS出料手臂吸放料流程.StepName = "出料手臂移动到飞梭第1列";
                            RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");
                            double[] xpos = 获取梭车xyp吸料点位(colums);
                            result = 出料手臂移动(xpos[0], xpos[1], xpos[2]);
                            if (result == "Success")
                            {
                                firstdraw = true;
                                firstdzhome = true;
                                countnum = new bool[] { lsopen[0], lsopen[1], lsopen[2], lsopen[3] };
                                PS出料手臂吸放料流程.StepID = 520;
                            }
                            else
                            {
                                Listcode.Add("2030007");
                                RunLog($"[Code]2030007,[Msg]出料手臂移动到梭车第1列失败!" + result, LogType.AlarmLog);
                                M23_出料手臂X轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 515:
                            PS出料手臂吸放料流程.StepName = "出料手臂移动到飞梭第2列";
                            RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");
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
                                Listcode.Add("2030007");
                                RunLog($"[Code]2030007,[Msg]出料手臂移动到梭车第2列失败!" + result, LogType.AlarmLog);
                                M23_出料手臂X轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 520:
                            PS出料手臂吸放料流程.StepName = "出料手臂Z向下移动";
                            RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");

                            if (设置信息.测试机信息.出料吸提前开真空 && !空跑模式)
                            {
                                出料吸嘴_吸放料(lsopen, true, true);
                            }

                            double zpos = 获取梭车Z吸料点位(colums);
                            result = 出料手臂Z移动(lsopen, zpos, true);
                            if (result == "Success")
                            {
                                PS出料手臂吸放料流程.StepID = 525;
                            }
                            else
                            {
                                RunLog($"出料手臂Z移动失败!" + result);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 525:
                            PS出料手臂吸放料流程.StepName = "出料吸嘴_吸料";
                            RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");
                            result = 空跑模式 ? "Success" : 出料吸嘴_吸放料(lsopen, true, true);
                            if (result == "Success")
                            {
                                PS出料手臂吸放料流程.StepID = 530;
                            }
                            else
                            {
                                RunLog($"出料吸嘴_吸料失败!" + result);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 530:
                            PS出料手臂吸放料流程.StepName = "出料吸嘴_真空检测";
                            RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");

                            if (!空跑模式)
                            {
                                stopwatch.Start();
                                while (JudgeNozzleSts(lsopen, opens, 0))
                                {
                                    if (stopwatch.ElapsedMilliseconds > 设置信息.测试机信息.出料手臂吸料超时时间)
                                    {
                                        RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->出料吸料超时");
                                        break;
                                    }

                                    result = 出料吸嘴_真空检测(out string[] ioval);
                                    if (result != "Success") break;

                                    RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{ioval[0]},{ioval[1]},{ioval[2]},{ioval[3]}:{stopwatch.ElapsedMilliseconds}ms");

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

                                bool[] bools = new bool[4] { lsopen[0] && opens[0] == 0, lsopen[1] && opens[1] == 0, lsopen[2] && opens[2] == 0, lsopen[3] && opens[3] == 0 };
                                出料吸嘴_吸放料(bools, true, false);
                            }
                            else
                            {
                                result = "Success";
                                for (int i = 0; i < lsopen.Length; i++)
                                {
                                    if (lsopen[i]) opens[i] = 1;
                                }
                                Thread.Sleep(设置信息.测试机信息.出料手臂吹料时间);
                            }

                            if (result == "Success")
                            {
                                PS出料手臂吸放料流程.StepID = 535;
                            }
                            else
                            {
                                ProgramPause = true;
                                IsPause = true;
                                RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->出料吸嘴_真空检测失败" + result);
                            }
                            break;

                        case 535:
                            PS出料手臂吸放料流程.StepName = "出料手臂Z向上移动";
                            RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");
                            result = 出料手臂Z移动(lsopen, -0.3, false);
                            if (result == "Success")
                            {
                                if (!空跑模式)
                                {
                                    //上来再侦测一次
                                    result = 出料吸嘴_真空检测(out string[] ioval);
                                    if (result != "Success") break;

                                    RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{ioval[0]},{ioval[1]},{ioval[2]},{ioval[3]}");

                                    for (int i = 0; i < ioval.Length; i++)
                                    {
                                        if (lsopen[i])
                                        {
                                            if (opens[i] == 1 && ioval[i] == "0")
                                            {
                                                opens[i] = 0;
                                            }
                                        }
                                    }

                                    bool[] bools = new bool[4] { lsopen[0] && opens[0] == 0, lsopen[1] && opens[1] == 0, lsopen[2] && opens[2] == 0, lsopen[3] && opens[3] == 0 };
                                    出料吸嘴_吸放料(bools, true, false);
                                }

                                for (int i = 0; i < lsopen.Length; i++)
                                {
                                    if (lsopen[i])
                                    {
                                        if (opens[i] == 1)
                                        {
                                            lsopen[i] = false;
                                            //RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->出料Z{i + index + 1}吸料成功");
                                        }
                                        else
                                        {
                                            if (countnum[i])
                                            {
                                                countnum[i] = false;
                                                出料手臂吸料不起数量++;
                                            }
                                            RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->出料Z{i + index + 1}吸料失败");
                                        }
                                    }
                                }
                                PS出料手臂吸放料流程.StepID = 540;
                            }
                            else
                            {
                                RunLog($"出料手臂Z向上移动失败!" + result);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 540:
                            PS出料手臂吸放料流程.StepName = "出料Z是否在原点";
                            RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");
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
                                            if (home[i] == "1") { RunLog($"出料吸嘴Z{i + 1}没有挡住原点"); }
                                        }
                                        if (firstdzhome)
                                        {
                                            firstdzhome = false;
                                        }
                                        else
                                        {
                                            for (int i = 0; i < home.Length; i++)
                                            {
                                                if (home[i] == "1")
                                                {
                                                    Listcode.Add($"140046{i + 4}");
                                                    RunLog($"[Code]140046{i + 4},[Msg]出料Z{i + 1}没有挡住原点", LogType.AlarmLog);
                                                }
                                            }
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
                                RunLog($"出料手臂Z回零失败!" + result);
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
                                            Listcode.Add($"30{i + 5}0000");
                                            RunLog($"[Code]30{i + 5}0000,[Msg]出料Z{i + 1}吸料失败!", LogType.AlarmLog);
                                        }
                                    }
                                    出料IC忽略 = false;
                                    ProgramPause = true;
                                    IsPause = true;
                                    motionEx.SetDo("Y004_重试灯", true);
                                    motionEx.SetDo("Y005_忽略灯", true);
                                    PS出料手臂吸放料流程.StepID = 546;
                                }
                            }
                            else
                            {
                                飞梭前出料IC[colums] = new bool[] { false, false, false, false };
                                if (colums == 1)
                                {
                                    ispass = true;
                                    飞梭前出料状态 = 0;
                                }
                                OutNozzleICOpen = new bool[] { opens[0] == 1, opens[1] == 1, opens[2] == 1, opens[3] == 1 };
                                OutNozzleIC = true;
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
                                    飞梭前出料IC[colums] = new bool[] { false, false, false, false }; //梭车清空
                                    open[colums] = new bool[] { false, false, false, false }; //吸嘴清空
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
                            RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");
                            PS出料手臂吸放料流程.StepID = 705;
                            break;

                        case 705: //放料
                            PS出料手臂吸放料流程.StepName = "计算放料盘位置";
                            opens = new int[] { OutNozzleICOpen[0] ? 1 : 2, OutNozzleICOpen[1] ? 1 : 2, OutNozzleICOpen[2] ? 1 : 2, OutNozzleICOpen[3] ? 1 : 2 };
                            if (判断BIN项(opens, Auto1, bin, out index))
                            {
                                trayid = 0;
                                if (获取Auto1盘坐标(false, out xPoint, out yPoint))
                                {
                                    RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->获取Auto1坐标位置");
                                    APPmaster.获取Auto1盘Open(opens, xPoint, yPoint, out lsopen, index, bin);
                                    xPos = Auto1XPoint[xPoint] + 点位微调.出料_Auto1放_X;
                                    yPos = (Auto1YPoint[yPoint] + index * TrayInterval * Tray_Y_Pitch) + 点位微调.出料_Auto1放_Y; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                    出料正在Auto1放料 = true;
                                    PS出料手臂吸放料流程.StepID = 710;
                                    break;

                                }
                                else
                                {
                                    if (!空跑模式 || Tray盘模式)
                                    {
                                        Auto等待换盘标记 = false;
                                        Auto1换盘标记 = true;
                                        RunLog("进入Auto1换盘程序");
                                        PS出料手臂吸放料流程.StepID = 805;
                                    }
                                    else Auto1TrayClear();
                                    break;
                                }
                            }
                            else if (判断BIN项(opens, Auto2, bin, out index))
                            {
                                trayid = 1;
                                if (获取Auto2盘坐标(false, out xPoint, out yPoint))
                                {
                                    RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->获取Auto2坐标位置");
                                    APPmaster.获取Auto2盘Open(opens, xPoint, yPoint, out lsopen, index, bin);
                                    xPos = Auto2XPoint[xPoint] + 点位微调.出料_Auto2放_X;
                                    yPos = (Auto2YPoint[yPoint] + index * TrayInterval * Tray_Y_Pitch) + 点位微调.出料_Auto2放_Y; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                    出料正在Auto2放料 = true;
                                    PS出料手臂吸放料流程.StepID = 710;
                                    break;

                                }
                                else
                                {
                                    if (!空跑模式 || Tray盘模式)
                                    {
                                        Auto等待换盘标记 = false;
                                        Auto2换盘标记 = true;
                                        RunLog("进入Auto2换盘程序");
                                        PS出料手臂吸放料流程.StepID = 905;
                                    }
                                    else Auto2TrayClear();
                                    break;
                                }
                            }
                            else if (判断Fix1BIN项(opens, bin) == "Success")
                            {
                                trayid = 2;
                                if (Fix1Num == 1)
                                {
                                    if (判断BIN项(opens, Fix1, bin, out index))
                                    {
                                        FixMinMax(1, 1, Fix1YPoint.Length, out int min, out int max);
                                        if (获取Fix1盘坐标(false, min, max, out xPoint, out yPoint))
                                        {
                                            获取Fix1盘Open(opens, xPoint, yPoint, Fix1, bin, max, out lsopen, index);
                                            xPos = Fix1XPoint[xPoint] + 点位微调.出料_Fix1放_X;
                                            yPos = (Fix1YPoint[yPoint] + index * TrayInterval * Tray_Y_Pitch) + 点位微调.出料_Fix1放_Y; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                            PS出料手臂吸放料流程.StepID = 710;
                                        }
                                        else
                                        {
                                            RunLog("进入Fix1满盘报警程序");
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
                                            xPos = Fix1XPoint[xPoint] + 点位微调.出料_Fix1放_X;
                                            yPos = (Fix1YPoint[yPoint] + index * TrayInterval * Tray_Y_Pitch) + 点位微调.出料_Fix1放_Y; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                            PS出料手臂吸放料流程.StepID = 710;
                                        }
                                        else
                                        {
                                            RunLog("进入Fix1满盘报警程序");
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
                                            xPos = Fix1XPoint[xPoint] + 点位微调.出料_Fix1放_X;
                                            yPos = (Fix1YPoint[yPoint] + index * TrayInterval * Tray_Y_Pitch) + 点位微调.出料_Fix1放_Y; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                            PS出料手臂吸放料流程.StepID = 710;
                                        }
                                        else
                                        {
                                            RunLog("进入Fix1满盘报警程序");
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
                                            xPos = Fix1XPoint[xPoint] + 点位微调.出料_Fix1放_X;
                                            yPos = (Fix1YPoint[yPoint] + index * TrayInterval * Tray_Y_Pitch) + 点位微调.出料_Fix1放_Y; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                            PS出料手臂吸放料流程.StepID = 710;
                                        }
                                        else
                                        {
                                            RunLog("进入Fix1满盘报警程序");
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
                                            xPos = Fix1XPoint[xPoint] + 点位微调.出料_Fix1放_X;
                                            yPos = (Fix1YPoint[yPoint] + index * TrayInterval * Tray_Y_Pitch) + 点位微调.出料_Fix1放_Y; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                            PS出料手臂吸放料流程.StepID = 710;
                                        }
                                        else
                                        {
                                            RunLog("进入Fix1满盘报警程序");
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
                                            xPos = Fix1XPoint[xPoint] + 点位微调.出料_Fix1放_X;
                                            yPos = (Fix1YPoint[yPoint] + index * TrayInterval * Tray_Y_Pitch) + 点位微调.出料_Fix1放_Y; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                            PS出料手臂吸放料流程.StepID = 710;
                                        }
                                        else
                                        {
                                            RunLog("进入Fix1满盘报警程序");
                                            PS出料手臂吸放料流程.StepID = 1005;
                                            break;
                                        }
                                    }
                                }
                            }
                            else if (判断Fix2BIN项(opens, bin) == "Success")
                            {
                                trayid = 3;
                                if (Fix2Num == 1)
                                {
                                    if (判断BIN项(opens, Fix2, bin, out index))
                                    {
                                        FixMinMax(1, 1, Fix2YPoint.Length, out int min, out int max);
                                        if (获取Fix2盘坐标(false, min, max, out xPoint, out yPoint))
                                        {
                                            获取Fix2盘Open(opens, xPoint, yPoint, Fix2, bin, max, out lsopen, index);
                                            xPos = Fix2XPoint[xPoint] + 点位微调.出料_Fix2放_X;
                                            yPos = (Fix2YPoint[yPoint] + index * TrayInterval * Tray_Y_Pitch) + 点位微调.出料_Fix2放_Y; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                            PS出料手臂吸放料流程.StepID = 710;
                                        }
                                        else
                                        {
                                            RunLog("进入Fix2满盘报警程序");
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
                                            xPos = Fix2XPoint[xPoint] + 点位微调.出料_Fix2放_X;
                                            yPos = (Fix2YPoint[yPoint] + index * TrayInterval * Tray_Y_Pitch) + 点位微调.出料_Fix2放_Y; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                            PS出料手臂吸放料流程.StepID = 710;
                                        }
                                        else
                                        {
                                            RunLog("进入Fix2满盘报警程序");
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
                                            xPos = Fix2XPoint[xPoint] + 点位微调.出料_Fix2放_X;
                                            yPos = (Fix2YPoint[yPoint] + index * TrayInterval * Tray_Y_Pitch) + 点位微调.出料_Fix2放_Y; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                            PS出料手臂吸放料流程.StepID = 710;
                                        }
                                        else
                                        {
                                            RunLog("进入Fix2满盘报警程序");
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
                                            xPos = Fix2XPoint[xPoint] + 点位微调.出料_Fix2放_X;
                                            yPos = (Fix2YPoint[yPoint] + index * TrayInterval * Tray_Y_Pitch) + 点位微调.出料_Fix2放_Y; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                            PS出料手臂吸放料流程.StepID = 710;
                                        }
                                        else
                                        {
                                            RunLog("进入Fix2满盘报警程序");
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
                                            xPos = Fix2XPoint[xPoint] + 点位微调.出料_Fix2放_X;
                                            yPos = (Fix2YPoint[yPoint] + index * TrayInterval * Tray_Y_Pitch) + 点位微调.出料_Fix2放_Y; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                            PS出料手臂吸放料流程.StepID = 710;
                                        }
                                        else
                                        {
                                            RunLog("进入Fix2满盘报警程序");
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
                                            xPos = Fix2XPoint[xPoint] + 点位微调.出料_Fix2放_X;
                                            yPos = (Fix2YPoint[yPoint] + index * TrayInterval * Tray_Y_Pitch) + 点位微调.出料_Fix2放_Y; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                            PS出料手臂吸放料流程.StepID = 710;
                                        }
                                        else
                                        {
                                            RunLog("进入Fix2满盘报警程序");
                                            PS出料手臂吸放料流程.StepID = 1105;
                                            break;
                                        }
                                    }
                                }
                            }
                            else if (判断Fix3BIN项(opens, bin) == "Success")
                            {
                                trayid = 4;
                                if (Fix3Num == 1)
                                {
                                    if (判断BIN项(opens, Fix3, bin, out index))
                                    {
                                        FixMinMax(1, 1, Fix3YPoint.Length, out int min, out int max);
                                        if (获取Fix3盘坐标(false, min, max, out xPoint, out yPoint))
                                        {
                                            获取Fix3盘Open(opens, xPoint, yPoint, Fix3, bin, max, out lsopen, index);
                                            xPos = Fix3XPoint[xPoint] + 点位微调.出料_Fix3放_X;
                                            yPos = (Fix3YPoint[yPoint] + index * TrayInterval * Tray_Y_Pitch) + 点位微调.出料_Fix3放_Y; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                            PS出料手臂吸放料流程.StepID = 710;
                                        }
                                        else
                                        {
                                            RunLog("进入Fix3满盘报警程序");
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
                                            xPos = Fix3XPoint[xPoint] + 点位微调.出料_Fix3放_X;
                                            yPos = (Fix3YPoint[yPoint] + index * TrayInterval * Tray_Y_Pitch) + 点位微调.出料_Fix3放_Y; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                            PS出料手臂吸放料流程.StepID = 710;
                                        }
                                        else
                                        {
                                            RunLog("进入Fix3满盘报警程序");
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
                                            xPos = Fix3XPoint[xPoint] + 点位微调.出料_Fix3放_X;
                                            yPos = (Fix3YPoint[yPoint] + index * TrayInterval * Tray_Y_Pitch) + 点位微调.出料_Fix3放_Y; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                            PS出料手臂吸放料流程.StepID = 710;
                                        }
                                        else
                                        {
                                            RunLog("进入Fix3满盘报警程序");
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
                                            xPos = Fix3XPoint[xPoint] + 点位微调.出料_Fix3放_X;
                                            yPos = (Fix3YPoint[yPoint] + index * TrayInterval * Tray_Y_Pitch) + 点位微调.出料_Fix3放_Y; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                            PS出料手臂吸放料流程.StepID = 710;
                                        }
                                        else
                                        {
                                            RunLog("进入Fix3满盘报警程序");
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
                                            xPos = Fix3XPoint[xPoint] + 点位微调.出料_Fix3放_X;
                                            yPos = (Fix3YPoint[yPoint] + index * TrayInterval * Tray_Y_Pitch) + 点位微调.出料_Fix3放_Y; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                            PS出料手臂吸放料流程.StepID = 710;
                                        }
                                        else
                                        {
                                            RunLog("进入Fix3满盘报警程序");
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
                                            xPos = Fix3XPoint[xPoint] + 点位微调.出料_Fix3放_X;
                                            yPos = (Fix3YPoint[yPoint] + index * TrayInterval * Tray_Y_Pitch) + 点位微调.出料_Fix3放_Y; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                                            PS出料手臂吸放料流程.StepID = 710;
                                        }
                                        else
                                        {
                                            RunLog("进入Fix3满盘报警程序");
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
                            RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");
                            Tray平移运动标记 = true;
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
                            Task<string> outputx = Task.Run(() => { return 出料手臂移动(xPos, yPos, 出料放料间距 + 点位微调.出料_AF_变距); });
                            if (outputx.Result == "Success" && fix3move.Result == "Success")
                            {
                                firstdzhome = true;
                                Tray平移运动标记 = false;
                                for (int i = 0; i < lsopen.Length; i++)
                                {
                                    if (lsopen[i]) OutNozzleICOpen[i] = false;
                                }
                                PS出料手臂吸放料流程.StepID = 715;
                            }
                            else
                            {
                                if (outputx.Result != "Success")
                                {
                                    Listcode.Add("2030007");
                                    RunLog($"[Code]2030007,[Msg]出料手臂走到放料位失败!" + outputx.Result, LogType.AlarmLog);
                                    M23_出料手臂X轴ErrorMsg = outputx.Result;
                                }
                                if (fix3move.Result != "Success")
                                {
                                    RunLog("Fix3放IC盘轴移动失败!" + fix3move.Result);
                                }
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 715:
                            PS出料手臂吸放料流程.StepName = "出料手臂Z向下";
                            RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");
                            double zPos = 获取出料Z放料点位(trayid);
                            result = 出料手臂Z移动(lsopen, zPos, true);
                            if (result == "Success")
                            {
                                PS出料手臂吸放料流程.StepID = 720;

                                if (前方定点停止允许)
                                {
                                    前方定点停止允许 = false;
                                    ProgramPause = true;
                                    IsPause = true;
                                    RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->出料手臂已定点停止完成");
                                }
                            }
                            else
                            {
                                RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->出料手臂Z向下移动失败!" + result);
                                ProgramPause = true;
                                IsPause = true;
                            }

                            break;

                        case 720:
                            PS出料手臂吸放料流程.StepName = "出料吸嘴放料";
                            RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");
                            bool outzenable1 = 设置信息.测试机信息.侧敲时允许放料 || !Auto1正在侧敲;
                            bool outzenable2 = 设置信息.测试机信息.侧敲时允许放料 || !Auto2正在侧敲;
                            if (trayid > 1 || ((trayid == 0 && outzenable1) || (trayid == 1 && outzenable2)))
                            {
                                result = 空跑模式 ? "Success" : 出料吸嘴_吸放料(lsopen, true, false);
                                if (result == "Success")
                                {
                                    result = 空跑模式 ? "Success" : 出料吸嘴_吸放料(lsopen, false, true);
                                    if (result == "Success")
                                    {
                                        Thread.Sleep(设置信息.测试机信息.出料手臂吹料时间);
                                        result = 空跑模式 ? "Success" : 出料吸嘴_吸放料(lsopen, false, false);
                                        if (result == "Success")
                                        {
                                            出料正在Auto1放料 = false;
                                            出料正在Auto2放料 = false;
                                            PS出料手臂吸放料流程.StepID = 725;
                                        }
                                    }
                                }
                            }
                            break;

                        case 725:
                            PS出料手臂吸放料流程.StepName = "出料手臂Z向上";
                            RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");
                            result = 出料手臂Z移动(lsopen, -0.3, false); //result = 出料手臂Z移动(lsopen, false);
                            if (result == "Success")
                            {
                                PS出料手臂吸放料流程.StepID = 730;
                            }
                            else
                            {
                                RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->出料手臂Z向上移动失败!" + result);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 730:
                            PS出料手臂吸放料流程.StepName = "出料Z是否在原点";
                            RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");
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
                                            if (trayid == 0)
                                            {
                                                Auto1数量++;
                                                Auto1DicPoint[$"{xPoint},{yPoint + i * TrayInterval}"] = true;
                                                Auto1DicBin[$"{xPoint},{yPoint + i * TrayInterval}"] = bin2[i + index];
                                                Auto1DicTestTime[$"{xPoint},{yPoint + i * TrayInterval}"] = testtime.TotalSeconds.ToString("0.0");
                                                Auto1DicLocation[$"{xPoint},{yPoint + i * TrayInterval}"] = $"{testsite + 1}-{(colums * 4 + (index + i)) + 1}"; //site-dut
                                            }
                                            else if (trayid == 1)
                                            {
                                                Auto2数量++;
                                                Auto2DicPoint[$"{xPoint},{yPoint + i * TrayInterval}"] = true;
                                                Auto2DicBin[$"{xPoint},{yPoint + i * TrayInterval}"] = bin2[i + index];
                                                Auto2DicTestTime[$"{xPoint},{yPoint + i * TrayInterval}"] = testtime.TotalSeconds.ToString("0.0");
                                                Auto2DicLocation[$"{xPoint},{yPoint + i * TrayInterval}"] = $"{testsite + 1}-{(colums * 4 + (index + i)) + 1}"; //site-dut
                                            }
                                            else if (trayid == 2)
                                            {
                                                Fix1数量++;
                                                Fix1DicPoint[$"{xPoint},{yPoint + i * TrayInterval}"] = true;
                                                Fix1DicBin[$"{xPoint},{yPoint + i * TrayInterval}"] = bin2[i + index];
                                                Fix1DicTestTime[$"{xPoint},{yPoint + i * TrayInterval}"] = testtime.TotalSeconds.ToString("0.0");
                                                Fix1DicLocation[$"{xPoint},{yPoint + i * TrayInterval}"] = $"{testsite + 1}-{(colums * 4 + (index + i)) + 1}"; //site-dut
                                            }
                                            else if (trayid == 3)
                                            {
                                                Fix2数量++;
                                                Fix2DicPoint[$"{xPoint},{yPoint + i * TrayInterval}"] = true;
                                                Fix2DicBin[$"{xPoint},{yPoint + i * TrayInterval}"] = bin2[i + index];
                                                Fix2DicTestTime[$"{xPoint},{yPoint + i * TrayInterval}"] = testtime.TotalSeconds.ToString("0.0");
                                                Fix2DicLocation[$"{xPoint},{yPoint + i * TrayInterval}"] = $"{testsite + 1}-{(colums * 4 + (index + i)) + 1}"; //site-dut
                                            }
                                            else
                                            {
                                                Fix3数量++;
                                                Fix3DicPoint[$"{xPoint},{yPoint + i * TrayInterval}"] = true;
                                                Fix3DicBin[$"{xPoint},{yPoint + i * TrayInterval}"] = bin2[i + index];
                                                Fix3DicTestTime[$"{xPoint},{yPoint + i * TrayInterval}"] = testtime.TotalSeconds.ToString("0.0");
                                                Fix3DicLocation[$"{xPoint},{yPoint + i * TrayInterval}"] = $"{testsite + 1}-{(colums * 4 + (index + i)) + 1}"; //site-dut
                                            }
                                        }
                                    }
                                    OutNozzleIC = false;
                                    PS出料手臂吸放料流程.StepID = 705;
                                }
                                else
                                {
                                    if (home.Contains("1"))
                                    {
                                        for (int i = 0; i < home.Length; i++)
                                        {
                                            if (home[i] == "1") { RunLog($"出料吸嘴{i + 1}没有挡住原点"); }
                                        }
                                        if (firstdzhome)
                                        {
                                            firstdzhome = false;
                                        }
                                        else
                                        {
                                            for (int i = 0; i < home.Length; i++)
                                            {
                                                if (home[i] == "1")
                                                {
                                                    Listcode.Add($"140046{i + 4}");
                                                    RunLog($"[Code]140046{i + 4},[Msg]出料Z{i + 1}没有挡住原点", LogType.AlarmLog);
                                                }
                                            }
                                            ProgramPause = true;
                                            APPmaster.IsPause = true;
                                        }
                                    }
                                    PS出料手臂吸放料流程.StepID = 735;
                                }
                            }
                            break;

                        case 735:
                            PS出料手臂吸放料流程.StepName = "出料手臂Z回原";
                            homeopen = new bool[] { home[0] == "1", home[1] == "1", home[2] == "1", home[3] == "1" };
                            result = 出料手臂Z回零(homeopen);
                            if (result == "Success")
                            {
                                PS出料手臂吸放料流程.StepID = 730;
                            }
                            else
                            {
                                RunLog($"出料手臂Z回零失败!" + result);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 798:
                            PS出料手臂吸放料流程.StepName = "出料手臂移到梭车第1列上等待";
                            RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");
                            xpos = 获取梭车xyp吸料点位2(0);
                            result = 出料手臂移动(xpos[0], xpos[1], xpos[2]);
                            if (result == "Success")
                            {
                                PS出料手臂吸放料流程.StepID = 799;
                            }
                            else
                            {
                                Listcode.Add("2030007");
                                RunLog($"[Code]2030007,[Msg]出料手臂移动到梭车第1列失败!" + result, LogType.AlarmLog);
                                M23_出料手臂X轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 799:
                            PS出料手臂吸放料流程.StepName = "出料手臂_吸放料流程完成";
                            RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");
                            PS出料手臂吸放料流程.Processing = false;
                            PS出料手臂吸放料流程.StepID = 500;
                            break;

                        case 805:
                            PS出料手臂吸放料流程.StepName = "出料手臂移到避让位";
                            RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");
                            result = 出料手臂移动(256.05, 初始点位.出料_Auto1_Y, 出料放料间距);
                            if (result == "Success")
                            {
                                Auto1上顶标记 = 2;
                                if (设置信息.测试机信息.Auto换盘时出料Z回零)
                                {
                                    PS出料手臂吸放料流程.StepID = 806;
                                    PS出料手臂放盘回零流程StepID = 810;
                                }
                                else
                                {
                                    PS出料手臂吸放料流程.StepID = 810;
                                    RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->判断Load未在换盘");
                                }
                            }
                            else
                            {
                                Listcode.Add("2030007");
                                RunLog($"[Code]2030007,[Msg]出料手臂移到避让位置失败!" + result, LogType.AlarmLog);
                                M23_出料手臂X轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 806:
                            PS出料手臂吸放料流程.StepName = "出料手臂Z执行换盘回零前下降2mm";
                            RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");
                            result = 出料手臂Z移动(new bool[] { true, true, true, true }, 2, true);
                            if (result == "Success")
                            {
                                firstdzhome = true;
                                PS出料手臂吸放料流程.StepID = 807;
                            }
                            else
                            {
                                RunLog($"出料手臂Z移动失败!" + result);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 807:
                            PS出料手臂吸放料流程.StepName = "出料手臂Z执行换盘回零";
                            RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");
                            result = 出料手臂Z回零(new bool[] { true, true, true, true });
                            if (result == "Success")
                            {
                                PS出料手臂吸放料流程.StepID = 808;
                            }
                            else
                            {
                                RunLog($"出料手臂Z回零失败!" + result);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 808:
                            PS出料手臂吸放料流程.StepName = "出料Z是否在原点";
                            RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");
                            result = 出料吸嘴_回原检测(out home);
                            if (result == "Success")
                            {
                                if (!home.Contains("1")) //如果有1说明有吸嘴没有挡住原点
                                {
                                    PS出料手臂吸放料流程.StepID = PS出料手臂放盘回零流程StepID;
                                }
                                else
                                {
                                    for (int i = 0; i < home.Length; i++)
                                    {
                                        if (home[i] == "1") { RunLog($"出料吸嘴Z{i + 1}没有挡住原点"); }
                                    }

                                    if (firstdzhome)
                                    {
                                        firstdzhome = false;
                                        PS出料手臂吸放料流程.StepID = 807;
                                    }
                                    else
                                    {
                                        for (int i = 0; i < home.Length; i++)
                                        {
                                            if (home[i] == "1")
                                            {
                                                Listcode.Add($"140046{i + 4}");
                                                RunLog($"[Code]140046{i + 4},[Msg]出料Z{i + 1}没有挡住原点", LogType.AlarmLog);
                                            }
                                        }
                                        ProgramPause = true;
                                        APPmaster.IsPause = true;
                                        PS出料手臂吸放料流程.StepID = 806;
                                    }
                                }
                            }
                            break;

                        case 810:
                            PS出料手臂吸放料流程.StepName = "判断Load未在换盘";
                            if (Load换盘标记 == false && 抓盘手臂放空盘到Empty标记 == 1)
                            {
                                抓盘手臂放空盘到Auto1标记 = 2;
                                PS出料手臂吸放料流程.StepID = 815;
                                RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->抓盘手臂放空盘到Auto1");
                            }
                            else Thread.Sleep(10);
                            break;

                        case 815:
                            PS出料手臂吸放料流程.StepName = "抓盘手臂放空盘到Auto1";
                            if (抓盘手臂放空盘到Auto1标记 == 1)
                            {
                                Auto1TrayClear();
                                Auto1换盘标记 = false;
                                RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->Auto1换盘完成Map更新");
                                PS出料手臂吸放料流程.StepID = 705;
                            }
                            else Thread.Sleep(10);
                            break;

                        case 905:
                            PS出料手臂吸放料流程.StepName = "出料手臂移到避让位";
                            result = 出料手臂移动(256.05, 初始点位.出料_Auto2_Y, 出料放料间距);
                            RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");
                            if (result == "Success")
                            {
                                Auto2上顶标记 = 2;

                                if (设置信息.测试机信息.Auto换盘时出料Z回零)
                                {
                                    PS出料手臂吸放料流程.StepID = 806;
                                    PS出料手臂放盘回零流程StepID = 910;
                                }
                                else
                                {
                                    PS出料手臂吸放料流程.StepID = 910;
                                    RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->判断Load是否未在换盘");
                                }

                            }
                            else
                            {
                                Listcode.Add("2030007");
                                RunLog($"[Code]2030007,[Msg]出料手臂移到避让位置失败!" + result, LogType.AlarmLog);
                                M23_出料手臂X轴ErrorMsg = result;
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
                                RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->判断Load是否未在换盘");
                            }
                            else Thread.Sleep(10);
                            break;

                        case 915:
                            PS出料手臂吸放料流程.StepName = "抓盘手臂放空盘到Auto2";
                            if (抓盘手臂放空盘到Auto2标记 == 1)
                            {
                                Auto2TrayClear();
                                Auto2换盘标记 = false;
                                RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->Auto2换盘完成Map更新");
                                PS出料手臂吸放料流程.StepID = 705;
                            }
                            else Thread.Sleep(10);
                            break;

                        case 1005:
                            if (!空跑模式 || Tray盘模式)
                            {
                                PS出料手臂吸放料流程.StepName = "出料手臂移到避让位(Fix1放满)";
                                RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");

                                Tray平移运动标记 = true;
                                Task<string> fix1traymove = Task.Run(() => { return "Success"; });
                                if (trayid == 3)
                                {
                                    fix1traymove = Task.Run(() => { return Fix3放IC盘轴移动(0); });
                                    Tray平移左右标记 = 0;
                                }
                                else
                                {
                                    if (trayid == 2 || trayid == 4) //如果不是往Fix放料则不用移动
                                    {
                                        fix1traymove = Task.Run(() => { return Fix3放IC盘轴移动(初始点位.出料_Fix3_右移动); });
                                        Tray平移左右标记 = 1;
                                    }
                                }
                                Task<string> fix1xypmove = Task.Run(() => { return 出料手臂移动(0.5, 初始点位.出料_Fix1_Y, 出料放料间距); });
                                if (fix1xypmove.Result == "Success" && fix1traymove.Result == "Success")
                                {
                                    Tray平移运动标记 = false;
                                    PS出料手臂吸放料流程.StepID = 1010;
                                }
                                else
                                {
                                    if (fix1xypmove.Result != "Success")
                                    {
                                        Listcode.Add("2030007");
                                        RunLog($"[Code]2030007,[Msg]出料手臂移到避让位(Fix1放满)失败!" + fix1xypmove.Result, LogType.AlarmLog);
                                        M23_出料手臂X轴ErrorMsg = fix1xypmove.Result;
                                    }
                                    if (fix1traymove.Result != "Success")
                                    {
                                        RunLog("Fix1放IC盘轴移动失败!" + fix1traymove.Result);
                                    }
                                    ProgramPause = true;
                                    IsPause = true;
                                }
                            }
                            else PS出料手臂吸放料流程.StepID = 1020;
                            break;

                        case 1010:
                            PS出料手臂吸放料流程.StepName = "Fix1满盘报警";
                            ProgramPause = true;
                            IsPause = true;
                            Fix1Tray盘放置标记 = false;
                            Fix1Tray盘拿走标记 = false;
                            Listcode.Add("4030013");
                            RunLog($"[Code]4030013,[Msg]Fix1满盘", LogType.AlarmLog);

                            PS出料手臂吸放料流程.StepID = 1015;
                            break;

                        case 1015:
                            if (Fix1Tray盘拿走标记)
                            {
                                result = motionEx.GetDi("X046_FIX1盘感应", out string fix1io);
                                if (result == "Success")
                                {
                                    if (Fix1Tray盘放置标记 && fix1io == "1")
                                    {
                                        PS出料手臂吸放料流程.StepID = 1020;
                                    }
                                    else
                                    {
                                        ProgramPause = true;
                                        IsPause = true;
                                        Listcode.Add("4030009");
                                        RunLog($"[Code]4030009,[Msg]Fix1无盘", LogType.AlarmLog);
                                    }
                                }
                                else
                                {
                                    ProgramPause = true;
                                    IsPause = true;
                                    RunLog("获取Fix1的Tray盘感应失败!" + result);
                                }
                            }
                            else
                            {
                                ProgramPause = true;
                                IsPause = true;
                                Listcode.Add("4030013");
                                RunLog($"[Code]4030013,[Msg]Fix1满盘", LogType.AlarmLog);
                            }
                            break;

                        case 1020:
                            Fix1TrayClear();
                            PS出料手臂吸放料流程.StepID = 705;
                            break;

                        case 1105:
                            if (!空跑模式 || Tray盘模式)
                            {
                                PS出料手臂吸放料流程.StepName = "出料手臂移到避让位(Fix2放满)";
                                RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");

                                Tray平移运动标记 = true;
                                Task<string> fix2traymove = Task.Run(() => { return "Success"; });
                                if (trayid == 3)
                                {
                                    fix2traymove = Task.Run(() => { return Fix3放IC盘轴移动(0); });
                                    Tray平移左右标记 = 0;
                                }
                                else
                                {
                                    if (trayid == 2 || trayid == 4) //如果不是往Fix放料则不用移动
                                    {
                                        fix2traymove = Task.Run(() => { return Fix3放IC盘轴移动(初始点位.出料_Fix3_右移动); });
                                        Tray平移左右标记 = 1;
                                    }
                                }
                                Task<string> fix2xypmove = Task.Run(() => { return 出料手臂移动(0.5, 初始点位.出料_Fix2_Y, 出料放料间距); });
                                if (fix2traymove.Result == "Success" && fix2xypmove.Result == "Success")
                                {
                                    Tray平移运动标记 = false;
                                    PS出料手臂吸放料流程.StepID = 1110;
                                }
                                else
                                {
                                    if (fix2xypmove.Result != "Success")
                                    {
                                        Listcode.Add("2030007");
                                        RunLog($"[Code]2030007,[Msg]出料手臂移到避让位(Fix2放满)失败!" + fix2xypmove.Result, LogType.AlarmLog);
                                        M23_出料手臂X轴ErrorMsg = fix2xypmove.Result;
                                    }
                                    if (fix2traymove.Result != "Success")
                                    {
                                        RunLog("Fix2放IC盘轴移动失败!" + fix2traymove.Result);
                                    }
                                    ProgramPause = true;
                                    IsPause = true;
                                }
                            }
                            else PS出料手臂吸放料流程.StepID = 1120;
                            break;

                        case 1110:
                            PS出料手臂吸放料流程.StepName = "Fix2满盘报警";
                            ProgramPause = true;
                            IsPause = true;
                            Fix2Tray盘放置标记 = false;
                            Fix2Tray盘拿走标记 = false;
                            Listcode.Add("4031013");
                            RunLog($"[Code]4031013,[Msg]Fix2满盘", LogType.AlarmLog);
                            PS出料手臂吸放料流程.StepID = 1115;
                            break;

                        case 1115:
                            if (Fix2Tray盘拿走标记)
                            {
                                result = motionEx.GetDi("X047_FIX2盘感应", out string fix2io);
                                if (result == "Success")
                                {
                                    if (Fix2Tray盘放置标记 && fix2io == "1")
                                    {
                                        PS出料手臂吸放料流程.StepID = 1120;
                                    }
                                    else
                                    {
                                        ProgramPause = true;
                                        IsPause = true;
                                        Listcode.Add("4031009");
                                        RunLog($"[Code]4031009,[Msg]Fix2无盘", LogType.AlarmLog);
                                    }
                                }
                                else
                                {
                                    ProgramPause = true;
                                    IsPause = true;
                                    RunLog("获取Fix2的Tray盘感应失败!" + result);
                                }
                            }
                            else
                            {
                                ProgramPause = true;
                                IsPause = true;
                                Listcode.Add("4031013");
                                RunLog($"[Code]4031013,[Msg]Fix2满盘", LogType.AlarmLog);
                            }
                            break;

                        case 1120:
                            Fix2TrayClear();
                            PS出料手臂吸放料流程.StepID = 705;
                            break;

                        case 1205:
                            if (!空跑模式 || Tray盘模式)
                            {
                                PS出料手臂吸放料流程.StepName = "出料手臂移到避让位(Fix3放满)";
                                RunLog($"PS出料手臂吸放料流程.StepID={PS出料手臂吸放料流程.StepID}->{PS出料手臂吸放料流程.StepName}");

                                Tray平移运动标记 = true;
                                Task<string> fix3traymove = Task.Run(() => { return "Success"; });
                                if (trayid == 3)
                                {
                                    fix3traymove = Task.Run(() => { return Fix3放IC盘轴移动(0); });
                                    Tray平移左右标记 = 0;
                                }
                                else
                                {
                                    if (trayid == 2 || trayid == 4) //如果不是往Fix放料则不用移动
                                    {
                                        fix3traymove = Task.Run(() => { return Fix3放IC盘轴移动(初始点位.出料_Fix3_右移动); });
                                        Tray平移左右标记 = 1;
                                    }
                                }
                                Task<string> fix3xypmove = Task.Run(() => { return 出料手臂移动(0.5, 初始点位.出料_Fix3_Y, 出料放料间距); });
                                if (fix3traymove.Result == "Success" && fix3xypmove.Result == "Success")
                                {
                                    PS出料手臂吸放料流程.StepID = 1210;
                                }
                                else
                                {
                                    if (fix3xypmove.Result != "Success")
                                    {
                                        Listcode.Add("2030007");
                                        RunLog($"[Code]2030007,[Msg]出料手臂移到避让位(Fix3放满)失败!" + fix3xypmove.Result, LogType.AlarmLog);
                                        M23_出料手臂X轴ErrorMsg = fix3xypmove.Result;
                                    }
                                    if (fix3traymove.Result != "Success")
                                    {
                                        RunLog("Fix3放IC盘轴移动失败!" + fix3traymove.Result);
                                    }
                                    ProgramPause = true;
                                    IsPause = true;
                                }
                            }
                            else PS出料手臂吸放料流程.StepID = 1220;
                            break;

                        case 1210:
                            PS出料手臂吸放料流程.StepName = "Fix3满盘报警";
                            ProgramPause = true;
                            IsPause = true;
                            Fix3Tray盘放置标记 = false;
                            Fix3Tray盘拿走标记 = false;
                            Listcode.Add("4032013");
                            RunLog($"[Code]4032013,[Msg]Fix3满盘", LogType.AlarmLog);
                            PS出料手臂吸放料流程.StepID = 1215;
                            break;

                        case 1215:
                            if (Fix3Tray盘拿走标记)
                            {
                                result = motionEx.GetDi("X045_FIX3盘对射", out string fix3io);
                                if (result == "Success")
                                {
                                    if (Fix3Tray盘拿走标记 && fix3io == "0")
                                    {
                                        PS出料手臂吸放料流程.StepID = 1220;
                                    }
                                    else
                                    {
                                        ProgramPause = true;
                                        IsPause = true;
                                        Listcode.Add("4032009");
                                        RunLog($"[Code]4032009,[Msg]Fix3无盘", LogType.AlarmLog);
                                    }
                                }
                                else
                                {
                                    ProgramPause = true;
                                    IsPause = true;
                                    RunLog("获取Fix3的Tray盘对射感应失败!" + result);
                                }
                            }
                            else
                            {
                                ProgramPause = true;
                                IsPause = true;
                                Listcode.Add("4032013");
                                RunLog($"[Code]4032013,[Msg]Fix3满盘", LogType.AlarmLog);
                            }
                            break;

                        case 1220:
                            Fix3TrayClear();
                            PS出料手臂吸放料流程.StepID = 705;
                            break;

                        default:
                            RunLog(("PS出料手臂吸放料流程.StepID =" + PS出料手臂吸放料流程.StepID.ToString() + "对应流程不存在"));
                            break;
                    }

                }
                catch (Exception ex)
                {
                    string errormsg = $"PS出料手臂吸放料流程({PS出料手臂吸放料流程.StepID})运行异常,详细信息:" + ex.Message;
                    RunLog(errormsg, LogType.AlarmLog);
                    系统未定义错误ErrorMsg = errormsg;
                    ProgramPause = true;
                    IsPause = true;
                    Listcode.Add("1400070");
                }
            }
        }

        public void 飞梭移动流程()
        {
            string result;
            PS入飞梭移动流程.StepID = 500;
            while (true)
            {
                try
                {
                    Thread.Sleep(10);

                    if (APPmaster.IsForecePause)
                    {
                        PS入飞梭移动流程.State = 1;
                        continue;
                    }

                    if (!是否可以开始运行())
                    {
                        continue;
                    }

                    switch (PS入飞梭移动流程.StepID)
                    {
                        case 500:
                            PS入飞梭移动流程.StepName = "路径规划";

                            //bool ispass = false;
                            //if (!结批标记)
                            //{
                            //    ispass = 入料手臂工作完成标记;
                            //}
                            //else
                            //{
                            //    if (飞梭后出料IC[0].Contains(true) || 飞梭后出料IC[1].Contains(true))
                            //    {
                            //        ispass = 入料手臂工作完成标记;
                            //    }
                            //}

                            bool[] isstop = { 入料手臂停止工作标记, 测试手臂入料停止工作标记, 测试手臂出料停止工作标记, 出料手臂出料停止工作标记 };
                            if (!isstop.Contains(false)) //有false，说明都还没有停止工作
                            {
                                飞梭停止工作 = true;
                            }

                            PS入飞梭移动流程.Condition = 入料手臂工作完成标记 //ispass// && 入料手臂停止工作标记  // || 结批标记) 
                                                        && 飞梭后入料状态 == 0
                                                        && 飞梭前出料状态 == 0;
                            //&& !正在结批;

                            PS入飞梭移动流程.Permit = !飞梭停止工作;

                            if (PS入飞梭移动流程.Condition && PS入飞梭移动流程.Permit)
                            {
                                飞梭移动状态 = 1;
                                PS入飞梭移动流程.Processing = true;
                                PS入飞梭移动流程.StepID = 501;
                            }
                            break;

                        case 501:
                            PS入飞梭移动流程.StepName = $"关闭梭车加热防止火花";
                            if (温控加热 && 温度模式 == "高温")
                            {
                                RunLog($"PS飞梭移动流程.StepID={PS入飞梭移动流程.StepID}->{PS入飞梭移动流程.StepName}");
                                if (梭车加热关闭() != "Success")
                                {
                                    RunLog("梭车加热关闭失败");
                                    ProgramPause = true;
                                    IsPause = true;
                                    break;
                                }
                                Thread.Sleep(100);
                            }
                            PS入飞梭移动流程.StepID = 505;
                            break;


                        case 505:
                            PS入飞梭移动流程.StepName = $"飞梭_前后检测产品感应";
                            RunLog($"PS飞梭移动流程.StepID={PS入飞梭移动流程.StepID}->{PS入飞梭移动流程.StepName}");
                            result = 飞梭_前后检测产品感应(out string[] iovalues);
                            if (result == "Success")
                            {
                                if (!iovalues.Contains("0"))
                                {
                                    PS入飞梭移动流程.StepID = 515;
                                }
                                else
                                {
                                    //if (iovalues[0] == "0")
                                    //{
                                    //    RunLog("飞梭后检测产品感应4 IC放歪"); //X079_
                                    //}
                                    //if (iovalues[1] == "0")
                                    //{
                                    //    RunLog("飞梭后检测产品感应3 IC放歪"); //X078_
                                    //}
                                    //if (iovalues[2] == "0")
                                    //{
                                    //    RunLog("飞梭后检测产品感应2 IC放歪"); //X077_
                                    //}
                                    //if (iovalues[3] == "0")
                                    //{
                                    //    RunLog("飞梭后检测产品感应1 IC放歪"); //X076_
                                    //}
                                    //if (iovalues[4] == "0")
                                    //{
                                    //    RunLog("飞梭前检测产品感应4 IC放歪"); //X083_
                                    //}
                                    //if (iovalues[5] == "0")
                                    //{
                                    //    RunLog("飞梭前检测产品感应3 IC放歪"); //X082_
                                    //}
                                    //if (iovalues[6] == "0")
                                    //{
                                    //    RunLog("飞梭前检测产品感应2 IC放歪"); //X081_
                                    //}
                                    //if (iovalues[7] == "0")
                                    //{
                                    //    RunLog("飞梭前检测产品感应1 IC放歪"); //X080_
                                    //}

                                    飞梭_前后放歪报警(iovalues);

                                    ProgramPause = true;
                                    IsPause = true;
                                }
                            }
                            break;

                        case 515:
                            PS入飞梭移动流程.StepName = $"飞梭轴移动";
                            RunLog($"PS飞梭移动流程.StepID={PS入飞梭移动流程.StepID}->{PS入飞梭移动流程.StepName}");
                            if (飞梭2位置 == 0)
                            {
                                result = 飞梭轴移动(初始点位.梭车2_前_Y + 点位微调.梭车2_前_Y);
                                if (result == "Success")
                                {
                                    飞梭2位置 = 1;
                                    PS入飞梭移动流程.StepID = 520;
                                }
                                else
                                {
                                    Listcode.Add("2027007");
                                    RunLog($"[Code]2027007,[Msg]梭车2移动到前方失败!" + result, LogType.AlarmLog);
                                    M20_飞梭轴ErrorMsg = result;
                                    ProgramPause = true;
                                    IsPause = true;
                                }
                            }
                            else
                            {
                                result = 飞梭轴移动(初始点位.梭车2_后_Y + 点位微调.梭车2_后_Y);
                                if (result == "Success")
                                {
                                    飞梭2位置 = 0;
                                    PS入飞梭移动流程.StepID = 520;
                                }
                                else
                                {
                                    Listcode.Add("2027007");
                                    RunLog($"[Code]2027007,[Msg]梭车2移动到后方失败!" + result, LogType.AlarmLog);
                                    M20_飞梭轴ErrorMsg = result;
                                    ProgramPause = true;
                                    IsPause = true;
                                }
                            }
                            break;

                        case 520:
                            PS入飞梭移动流程.StepName = $"开启梭车加热";
                            if (温控加热 && 温度模式 == "高温")
                            {
                                RunLog($"PS飞梭移动流程.StepID={PS入飞梭移动流程.StepID}->{PS入飞梭移动流程.StepName}");
                                Thread.Sleep(100);
                                if (梭车加热开启() != "Success")
                                {
                                    RunLog("梭车加热开启失败");
                                    ProgramPause = true;
                                    IsPause = true;
                                    break;
                                }
                            }
                            PS入飞梭移动流程.StepID = 525;
                            break;

                        case 525:
                            PS入飞梭移动流程.StepName = $"数据交换";
                            RunLog($"PS飞梭移动流程.StepID={PS入飞梭移动流程.StepID}->{PS入飞梭移动流程.StepName}");

                            交换飞梭前后入料IC数据();

                            飞梭后入料状态 = 1;
                            //飞梭前入料状态 = 0;
                            入料手臂工作完成标记 = false;

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
                            RunLog($"PS飞梭移动流程.StepID={PS入飞梭移动流程.StepID}->{PS入飞梭移动流程.StepName}");
                            PS入飞梭移动流程.Processing = false;
                            PS入飞梭移动流程.StepID = 500;
                            break;

                        default:
                            RunLog(("PS飞梭移动流程.StepID =" + PS入飞梭移动流程.StepID.ToString() + "对应流程不存在"));
                            break;
                    }

                }
                catch (Exception ex)
                {
                    string errormsg = $"PS飞梭移动流程({PS入飞梭移动流程.StepID})运行异常,详细信息:" + ex.Message;
                    RunLog(errormsg, LogType.AlarmLog);
                    系统未定义错误ErrorMsg = errormsg;
                    ProgramPause = true;
                    IsPause = true;
                    Listcode.Add("1400070");
                }
            }
        }

        public void 测试手臂取待测料流程()
        {
            string result = "";
            string result2 = "";
            string[] home = new string[1];
            int[] opens = new int[8];
            bool[] lsopen = new bool[8];
            Stopwatch stopwatch = new Stopwatch();
            bool firstdraw = true;
            bool firstdzhome = true;
            int site = 0;
            int movesite = 0;
            string[] iovalues = new string[4];
            string[] iovalues2 = new string[1];

            bool[] count = new bool[8];
            PS测试手臂取待测料流程.StepID = 500;
            while (true)
            {
                try
                {
                    Thread.Sleep(10);

                    if (APPmaster.IsForecePause)
                    {
                        PS测试手臂取待测料流程.State = 1;
                        continue;
                    }

                    if (!是否可以开始运行())
                    {
                        continue;
                    }

                    switch (PS测试手臂取待测料流程.StepID)
                    {
                        case 500:
                            PS测试手臂取待测料流程.StepName = "路径规划";
                            PS测试手臂取待测料流程.Condition = (飞梭后入料状态 == 1 && APPmaster.InSiteQueue.Count > 0)
                                                            && (飞梭后出料状态 == 1 || APPmaster.OutSiteQueue.Count == 0)
                                                            && !测试手臂入料停止工作标记;

                            PS测试手臂取待测料流程.Permit = !PS测试手臂取已测料流程.Processing;
                            if (PS测试手臂取待测料流程.Condition && PS测试手臂取待测料流程.Permit)
                            {
                                PS测试手臂取待测料流程.Processing = true;
                                PS测试手臂取待测料流程.StepID = 501;
                            }
                            else
                            {
                                if (测试手臂入料停止工作标记)
                                {
                                    if (飞梭后出料状态 == 1)
                                    {
                                        飞梭后入料状态 = 0;
                                    }
                                }
                                Thread.Sleep(10);
                            }
                            break;

                        case 501:
                            PS测试手臂取待测料流程.StepName = "获取开Site信息";
                            RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");

                            if (!飞梭后入料IC[0].Contains(true) //飞梭后入料IC第1列无料
                                && !飞梭后入料IC[1].Contains(true) //飞梭后入料IC第2列无料
                                && !飞梭前入料IC[0].Contains(true) //飞梭前入料IC第1列无料
                                && !飞梭前入料IC[1].Contains(true) //飞梭前入料IC第2列无料
                                && 入料手臂停止工作标记
                                && (结批标记 || 一个循环标记))
                            {
                                测试手臂入料停止工作标记 = true;
                                PS测试手臂取待测料流程.Processing = false;
                                PS测试手臂取待测料流程.StepID = 500;
                                break;
                            }
                            else
                            {
                                if (入料手臂停止工作标记 && (结批标记 || 一个循环标记))
                                {
                                    bool[] open = new bool[]
                                    {
                                    飞梭后入料IC[0][0], 飞梭后入料IC[0][1], 飞梭后入料IC[0][2], 飞梭后入料IC[0][3],
                                    飞梭后入料IC[1][0], 飞梭后入料IC[1][1], 飞梭后入料IC[1][2], 飞梭后入料IC[1][3]
                                    };

                                    bool[] open2 = new bool[]
                                    {
                                    飞梭前入料IC[0][0], 飞梭前入料IC[0][1], 飞梭前入料IC[0][2], 飞梭前入料IC[0][3],
                                    飞梭前入料IC[1][0], 飞梭前入料IC[1][1], 飞梭前入料IC[1][2], 飞梭前入料IC[1][3]
                                    };

                                    bool[] open3 = new bool[]
                                    {
                                    放料IC目标值[0][0], 放料IC目标值[0][1], 放料IC目标值[0][2], 放料IC目标值[0][3],
                                    放料IC目标值[1][0], 放料IC目标值[1][1], 放料IC目标值[1][2], 放料IC目标值[1][3]
                                    };

                                    bool[] open6 = new bool[]
                                    {
                                    正在吸取后入料[0], 正在吸取后入料[1], 正在吸取后入料[2], 正在吸取后入料[3],
                                    正在吸取后入料[4], 正在吸取后入料[5], 正在吸取后入料[6], 正在吸取后入料[7]
                                    };

                                    bool[] open7 = new bool[]
                                    {
                                    正在放置前入料[0][0], 正在放置前入料[0][1], 正在放置前入料[0][2], 正在放置前入料[0][3],
                                    正在放置前入料[1][0], 正在放置前入料[1][1], 正在放置前入料[1][2], 正在放置前入料[1][3]
                                    };

                                    结批时判断是否关site(open, open2, open3, out bool[] open4, out bool[] open5);

                                    if (open4.Contains(true))
                                    {
                                        for (int i = 0; i < open3.Length; i++)
                                        {
                                            if (!open3[i] && open[i] && !open6[i])
                                            {
                                                open[i] = false;
                                            }
                                        }
                                    }

                                    if (open5.Contains(true))
                                    {
                                        for (int i = 0; i < open3.Length; i++)
                                        {
                                            if (!open3[i] && open2[i] && !open7[i])
                                            {
                                                open2[i] = false;
                                            }
                                        }
                                    }

                                    if (!open.Contains(true) && !open2.Contains(true))
                                    {
                                        测试手臂入料停止工作标记 = true;
                                        PS测试手臂取待测料流程.Processing = false;
                                        PS测试手臂取待测料流程.StepID = 500;
                                        break;
                                    }
                                }
                            }

                            if (InSiteQueue.Count > 0)
                            {
                                site = InSiteQueue.First();
                                bool[] open = GetSiteOpen(site);
                                if (open.Contains(true)) //如果有开Site
                                {
                                    bool temppass = true;
                                    double temp = SiteTemp[site];
                                    if (温度模式 == "高温")
                                    {
                                        if (temp > 加热盘.波动温度上限 || temp <= 加热盘.波动温度下限)
                                        {
                                            temppass = false;
                                        }
                                    }
                                    if (temppass)
                                    {
                                        if (飞梭后入料IC[0].Contains(true) || 飞梭后入料IC[1].Contains(true)) //如果梭车上有料
                                        {
                                            opens = new int[8]
                                            {
                                            open[0] && 飞梭后入料IC[0][0] ? 0 : 2, open[1] && 飞梭后入料IC[0][1] ? 0 : 2, open[2] && 飞梭后入料IC[0][2] ? 0 : 2, open[3] && 飞梭后入料IC[0][3] ? 0 : 2,
                                            open[4] && 飞梭后入料IC[1][0] ? 0 : 2, open[5] && 飞梭后入料IC[1][1] ? 0 : 2, open[6] && 飞梭后入料IC[1][2] ? 0 : 2, open[7] && 飞梭后入料IC[1][3] ? 0 : 2
                                            };

                                            lsopen = new bool[8]
                                            {
                                            opens[0] == 0, opens[1] == 0, opens[2] == 0, opens[3] == 0,
                                            opens[4] == 0, opens[5] == 0, opens[6] == 0, opens[7] == 0,
                                            };

                                            正在吸取后入料 = new bool[] { lsopen[0], lsopen[1], lsopen[2], lsopen[3], lsopen[4], lsopen[5], lsopen[6], lsopen[7], };

                                            if (lsopen.Contains(true)) //梭车上只要有一颗就拿走
                                            {
                                                //InSiteQueue.TryDequeue(out movesite);
                                                正在入料的Site = site;
                                                //RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->移除Site{site + 1}");
                                                RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->拿到取Site{site + 1}的IC信息:{opens[0]},{opens[1]},{opens[2]},{opens[3]},{opens[4]},{opens[5]},{opens[6]},{opens[7]}");
                                                PS测试手臂取待测料流程.StepID = 505;
                                            }
                                            else //没有要拿的料就给下一个,并将site移除并添加到末尾
                                            {
                                                RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->Site{site + 1}：在梭车上没有要取的IC");

                                                if (飞梭后出料状态 == 1)// || (飞梭前入料IC[0].Contains(true) || 飞梭前入料IC[1].Contains(true))
                                                {
                                                    飞梭后入料状态 = 0;
                                                    PS测试手臂取待测料流程.Processing = false;
                                                    PS测试手臂取待测料流程.StepID = 500;
                                                }
                                                else //没有出料则循环判断每个site
                                                {
                                                    InSiteQueue.TryDequeue(out movesite);
                                                    InSiteQueue.Enqueue(movesite);
                                                    Thread.Sleep(10);

                                                    if (InSiteQueue.Count == 1) //如果只有一个Site在等,则返回出去等下一个
                                                    {
                                                        if (!StationManagers.CheckTestState(2)) //如何没有Site正在测试，则交叉
                                                        {
                                                            if (飞梭前入料IC[0].Contains(true) || 飞梭前入料IC[1].Contains(true) && 入料手臂停止工作标记) //如果前梭车入料区有料才交叉
                                                            {
                                                                飞梭后入料状态 = 0;
                                                            }
                                                        }
                                                        PS测试手臂取待测料流程.Processing = false;
                                                        PS测试手臂取待测料流程.StepID = 500;
                                                    }
                                                    //for (int i = 0; i < InSiteQueue.Count; i++)
                                                    //{

                                                    //}
                                                    //InSiteQueue.Contains(site);
                                                }
                                            }
                                        }
                                        else //如果梭车没有料,则等梭车交换后到有料
                                        {
                                            RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->Site{site + 1}取料：梭车上没有IC");
                                            if ((飞梭前入料IC[0].Contains(true) || 飞梭前入料IC[0].Contains(true) || 飞梭后出料状态 == 1) && 入料手臂停止工作标记)
                                            {
                                                飞梭后入料状态 = 0;
                                            }
                                            //SiteTestIC[site] = new bool[][] { new bool[] { false, false, false, false }, new bool[] { false, false, false, false } };
                                            StationManagers.SetStationIC(site, new bool[][] { new bool[] { false, false, false, false }, new bool[] { false, false, false, false } });
                                            StationManagers.SetStationState(site, 0); //SiteTestState[site] = 0; //无料
                                            PS测试手臂取待测料流程.Processing = false;
                                            PS测试手臂取待测料流程.StepID = 500;
                                            Thread.Sleep(10);
                                        }
                                    }
                                    else
                                    {
                                        if (temp > 加热盘.波动温度上限)
                                        {
                                            Listcode.Add($"8{site:D2}319");
                                            RunLog($"[Code]8{site:D2}319,[Msg]Site{site + 1}:温度[{temp}]℃高于上限[{加热盘.波动温度上限}]℃", LogType.AlarmLog);
                                        }
                                        else if (temp <= 加热盘.波动温度下限)
                                        {
                                            Listcode.Add($"8{site:D2}319");
                                            RunLog($"[Code]8{site:D2}319,[Msg]Site{site + 1}:温度[{temp}]℃低于下限[{加热盘.波动温度下限}]℃", LogType.AlarmLog);
                                        }
                                        ProgramPause = true;
                                        IsPause = true;
                                    }
                                }
                                else //如果Site全部被关闭则拿下一个Site的料
                                {
                                    InSiteQueue.TryDequeue(out movesite);
                                    RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->Site{site + 1}：Dut被全部关闭");
                                    //SiteTestIC[site] = new bool[][] { new bool[] { false, false, false, false }, new bool[] { false, false, false, false } };
                                    StationManagers.SetStationIC(site, new bool[][] { new bool[] { false, false, false, false }, new bool[] { false, false, false, false } });
                                    StationManagers.SetStationState(site, 0); //SiteTestState[site] = 0; //无料
                                    Thread.Sleep(10);
                                }
                            }
                            else
                            {
                                RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->测试入料队列为空：{InSiteQueue.Count}");
                                飞梭后入料状态 = 0;
                                PS测试手臂取待测料流程.Processing = false;
                                PS测试手臂取待测料流程.StepID = 500;
                            }
                            break;

                        case 505:
                            PS测试手臂取待测料流程.StepName = "测试吸嘴_张卡气缸闭合";
                            RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");
                            result = 测试吸嘴_张开气缸输出(true);
                            if (result == "Success")
                            {
                                PS测试手臂取待测料流程.StepID = 510;
                            }
                            else
                            {
                                Listcode.Add($"1011001");
                                RunLog($"[Code]1011001,[Msg]测试吸嘴_张卡气缸闭合失败!" + result, LogType.AlarmLog);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 510:
                            PS测试手臂取待测料流程.StepName = "测试手臂X轴到飞梭后入料吸料";
                            RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");
                            double xpos = 飞梭2位置 == 0 ? (初始点位.测试_梭车2吸_X + 点位微调.测试_梭车2左吸_X) : (初始点位.测试_梭车1吸_X + 点位微调.测试_梭车1左吸_X);
                            result = 测试手臂X移动(xpos);
                            if (result == "Success")
                            {
                                firstdraw = true;
                                firstdzhome = true;
                                count = new bool[8] { lsopen[0], lsopen[1], lsopen[2], lsopen[3], lsopen[4], lsopen[5], lsopen[6], lsopen[7] };
                                PS测试手臂取待测料流程.StepID = 515;
                            }
                            else
                            {
                                Listcode.Add("2028008");
                                RunLog($"[Code]2028008,[Msg]测试手臂X移动到梭车失败!" + result);
                                M21_测试手臂X轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 515:
                            PS测试手臂取待测料流程.StepName = "测试吸嘴张开气缸到位输入读取";
                            RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");
                            result = 测试吸嘴张开气缸到位输入读取();
                            if (result == "Success")
                            {
                                PS测试手臂取待测料流程.StepID = 520;
                            }
                            else
                            {
                                Listcode.Add($"1011001");
                                RunLog($"[Code]1011001,[Msg]测试吸嘴张开气缸到位输入读取失败!" + result, LogType.AlarmLog);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 520:
                            PS测试手臂取待测料流程.StepName = "测试Z向下移动"; //飞梭吸
                            RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");

                            if (设置信息.测试机信息.测试吸提前开真空 && !空跑模式)
                            {
                                测试吸嘴_吸放料(lsopen, true, true);
                            }

                            double zpos = 飞梭2位置 == 0 ? (初始点位.测试_梭车2吸_Z + 点位微调.测试_梭车2左吸_Z) : (初始点位.测试_梭车1吸_Z + 点位微调.测试_梭车1左吸_Z);
                            result = 测试手臂Z移动(zpos, true);
                            if (result == "Success")
                            {
                                PS测试手臂取待测料流程.StepID = 525;
                            }
                            else
                            {
                                Listcode.Add("2026007");
                                RunLog($"[Code]2026007,[Msg]测试Z向下移动失败!" + result, LogType.AlarmLog);
                                M19_测试手臂Z轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 525:
                            PS测试手臂取待测料流程.StepName = "测试吸嘴吸料";
                            RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");
                            result = 空跑模式 ? "Success" : 测试吸嘴_吸放料(lsopen, true, true);
                            if (result == "Success")
                            {
                                PS测试手臂取待测料流程.StepID = 530;
                            }
                            else
                            {
                                RunLog($"测试吸嘴开吸真空失败!" + result);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 530:
                            PS测试手臂取待测料流程.StepName = "测试吸嘴真空检测";
                            RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");
                            if (!空跑模式)
                            {
                                stopwatch.Start();
                                while (JudgeNozzleSts(lsopen, opens, 0))
                                {
                                    if (stopwatch.ElapsedMilliseconds > 设置信息.测试机信息.测试手臂吸料超时时间)
                                    {
                                        RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->测试吸料超时");
                                        break;
                                    }

                                    result = 测试吸嘴_真空检测(out string[] ioval);
                                    if (result != "Success") break;

                                    RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{ioval[0]},{ioval[1]},{ioval[2]},{ioval[3]},{ioval[4]},{ioval[5]},{ioval[6]},{ioval[7]}:{stopwatch.ElapsedMilliseconds}ms");

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

                                bool[] bools = new bool[8] { lsopen[0] && opens[0] == 0, lsopen[1] && opens[1] == 0, lsopen[2] && opens[2] == 0, lsopen[3] && opens[3] == 0, lsopen[4] && opens[4] == 0, lsopen[5] && opens[5] == 0, lsopen[6] && opens[6] == 0, lsopen[7] && opens[7] == 0 };
                                测试吸嘴_吸放料(bools, true, false);
                            }
                            else
                            {
                                result = "Success";
                                for (int i = 0; i < lsopen.Length; i++)
                                {
                                    if (lsopen[i]) opens[i] = 1;
                                }
                                Thread.Sleep(设置信息.测试机信息.测试手臂吹料时间);
                            }

                            if (result == "Success")
                            {
                                PS测试手臂取待测料流程.StepID = 535;
                            }
                            else
                            {
                                ProgramPause = true;
                                IsPause = true;
                                RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->测试吸嘴_真空检测失败" + result);
                            }
                            break;

                        case 535:
                            PS测试手臂取待测料流程.StepName = "测试Z向上移动";
                            RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");
                            result = 测试手臂Z移动(-0.3, false);
                            if (result == "Success")
                            {
                                if (!空跑模式)
                                {
                                    //上来再侦测一次
                                    result = 测试吸嘴_真空检测(out string[] ioval);
                                    if (result != "Success") break;

                                    RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{ioval[0]},{ioval[1]},{ioval[2]},{ioval[3]},{ioval[4]},{ioval[5]},{ioval[6]},{ioval[7]}");

                                    for (int i = 0; i < ioval.Length; i++)
                                    {
                                        if (lsopen[i])
                                        {
                                            if (opens[i] == 1 && ioval[i] == "0")
                                            {
                                                opens[i] = 0;
                                            }
                                        }
                                    }
                                    bool[] bools = new bool[8] { lsopen[0] && opens[0] == 0, lsopen[1] && opens[1] == 0, lsopen[2] && opens[2] == 0, lsopen[3] && opens[3] == 0, lsopen[4] && opens[4] == 0, lsopen[5] && opens[5] == 0, lsopen[6] && opens[6] == 0, lsopen[7] && opens[7] == 0 };
                                    测试吸嘴_吸放料(bools, true, false);
                                }

                                for (int i = 0; i < lsopen.Length; i++)
                                {
                                    if (lsopen[i])
                                    {
                                        if (opens[i] == 1)
                                        {
                                            lsopen[i] = false;
                                            //RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->测试Z{i + 1}吸料成功");
                                        }
                                        else
                                        {
                                            if (count[i])
                                            {
                                                count[i] = false;
                                                测试手臂吸不起数量++;
                                            }
                                            RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->测试Z{i + 1}吸料失败");
                                        }
                                    }
                                }
                                PS测试手臂取待测料流程.StepID = 540;
                            }
                            else
                            {
                                Listcode.Add("2026007");
                                RunLog($"[Code]2026007,[Msg]测试Z向上移动失败!" + result, LogType.AlarmLog);
                                M19_测试手臂Z轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;


                        case 540:
                            PS测试手臂取待测料流程.StepName = "测试Z是否在原点";
                            RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");
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
                                        RunLog($"测试Z没有挡住原点");

                                        if (firstdzhome)
                                        {
                                            firstdzhome = false;
                                        }
                                        else
                                        {
                                            Listcode.Add($"1400468");
                                            RunLog($"[Code]1400468,[Msg]测试Z没有挡住原点", LogType.AlarmLog);
                                            ProgramPause = true;
                                            APPmaster.IsPause = true;
                                        }
                                    }
                                    PS测试手臂取待测料流程.StepID = 541;
                                }
                            }
                            else
                            {
                                RunLog($"读取测试Z原点信号失败!" + result);
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
                                RunLog($"测试手臂Z回零失败!" + result);
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
                                            Listcode.Add($"3{i + 9:D2}9900");
                                            RunLog($"[Code]3{i + 9:D2}9900,[Msg]梭车{(飞梭2位置 == 0 ? "2" : "1")}位置,测试Z{i + 1}吸料失败!", LogType.AlarmLog);
                                            //RunLog($"[Code]3{i + 9:D2}9900,[Msg]测试Z{i + 1}吸料失败!", LogType.AlarmLog);
                                        }
                                    }
                                    测试IC忽略 = false;
                                    测试区IC忽略 = true;
                                    ProgramPause = true;
                                    IsPause = true;
                                    motionEx.SetDo("Y004_重试灯", true);
                                    motionEx.SetDo("Y005_忽略灯", true);
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

                                正在吸取后入料 = new bool[] { false, false, false, false, false, false, false, false };

                                飞梭后入料状态 = 0;

                                if (opens.Contains(1)) //如果有吸嘴吸了至少一颗IC则去测试区放置
                                {
                                    InSiteQueue.TryDequeue(out movesite);
                                    RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->移除Site{movesite + 1}");
                                    PS测试手臂取待测料流程.StepID = 599;
                                }
                                else //全部吸料失败则等梭车交叉过来重新吸取
                                {
                                    RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->在梭车{(飞梭2位置 == 0 ? "2" : "1")}未吸起一颗IC(放入Site{site + 1})!");
                                    PS测试手臂取待测料流程.Processing = false;
                                    PS测试手臂取待测料流程.StepID = 500;
                                }
                            }
                            break;

                        case 543:
                            if (测试IC忽略)
                            {
                                //if (opens.Contains(1)) //如果有吸嘴吸了IC则放置
                                //{

                                for (int i = 0; i < lsopen.Length; i++)
                                {
                                    if (lsopen[i] && opens[i] == 0)
                                    {
                                        lsopen[i] = false;
                                        opens[i] = 2;
                                        RunLog($"梭车{(飞梭2位置 == 0 ? "2" : "1")}—{i + 1}位置的IC因测试Z{i + 1}吸料失败被跳过,请手动吸取出IC!", LogType.OperateLog);
                                        飞梭后入料IC[i / 4][i % 4] = false;
                                    }
                                }
                                PS测试手臂取待测料流程.StepID = 542;

                                //}
                                //else //吸嘴上没有IC,则直接返回
                                //{
                                //    测试IC忽略 = false;
                                //    ProgramPause = true;
                                //    IsPause = true;
                                //    RunLog("测试Z吸嘴至少吸上1颗IC");
                                //    //opens = new bool[] { false, false, false, false };
                                //    //PS测试手臂取待测料流程.StepID = 505;
                                //}
                            }
                            else //直接点开始默认重吸
                            {
                                PS测试手臂取待测料流程.StepID = 520;
                            }
                            break;

                        case 599:
                            TestNozzleICOpen = new bool[] { opens[0] == 1, opens[1] == 1, opens[2] == 1, opens[3] == 1, opens[4] == 1, opens[5] == 1, opens[6] == 1, opens[7] == 1 };
                            //TestNozzleICOpen = new bool[] { lsopen[0], lsopen[1], lsopen[2], lsopen[3], lsopen[4], lsopen[5], lsopen[6], lsopen[7] };
                            TestNozzleIC = true;
                            PS测试手臂取待测料流程.StepName = "测试手臂取待测料流程吸料完成";
                            RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");
                            PS测试手臂取待测料流程.StepID = 705;
                            break;

                        case 705:
                            PS测试手臂取待测料流程.StepName = "测试吸嘴_张开";
                            RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");
                            result = 测试吸嘴_张开气缸输出(false);
                            if (result == "Success")
                            {
                                PS测试手臂取待测料流程.StepID = 710;
                            }
                            else
                            {
                                Listcode.Add($"1011000");
                                RunLog($"[Code]1011000,[Msg]测试吸嘴_张开失败!" + result, LogType.AlarmLog);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 710:
                            PS测试手臂取待测料流程.StepName = "测试手臂X移动到测试区放料";
                            RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");
                            result = 获取测试X放料补偿(site, out double sitex);
                            if (result == "Success")
                            {
                                result = 测试手臂X移动(TestXPoint[site] + sitex);
                                if (result == "Success")
                                {
                                    firstdraw = true;
                                    firstdzhome = true;
                                    lsopen = new bool[]
                                    {
                                        TestNozzleICOpen[0], TestNozzleICOpen[1], TestNozzleICOpen[2], TestNozzleICOpen[3],
                                        TestNozzleICOpen[4], TestNozzleICOpen[5], TestNozzleICOpen[6], TestNozzleICOpen[7]
                                    };
                                    TestNozzleIC = false;
                                    PS测试手臂取待测料流程.StepID = 715;
                                }
                                else
                                {
                                    Listcode.Add("2028007");
                                    RunLog($"[Code]2028007,[Msg]测试手臂X轴移动到Site{site + 1}上方失败!" + result, LogType.AlarmLog);
                                    M21_测试手臂X轴ErrorMsg = result;
                                    ProgramPause = true;
                                    IsPause = true;
                                }
                            }
                            break;

                        case 715:
                            PS测试手臂取待测料流程.StepName = "测试吸嘴张开气缸到位输入读取";
                            RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");
                            result = 测试吸嘴张开气缸原位输入读取();
                            string result3 = 测试区_位移气缸原位信号读取(site);
                            string result4 = 测试区_升降气缸原位信号读取(site);
                            if (result == "Success" && result3 == "Success" && result4 == "Success")
                            {
                                PS测试手臂取待测料流程.StepID = 720;
                                RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->测试Z等待气缸动作完成");
                            }
                            else
                            {
                                if (result != "Success")
                                {
                                    Listcode.Add($"1011000");
                                    RunLog($"[Code]1011000,[Msg]测试吸嘴张开气缸原位输入读取失败!" + result, LogType.AlarmLog);
                                }
                                if (result3 != "Success")
                                {
                                    Listcode.Add($"10{site + 30:D2}001");
                                    RunLog($"[Code]10{site + 30:D2}001,[Msg]测试区Site{site + 1}位移气缸未在原位!" + result, LogType.AlarmLog);
                                }
                                if (result4 != "Success")
                                {
                                    Listcode.Add($"10{site + 14:D2}001");
                                    RunLog($"[Code]10{site + 14:D2}001,[Msg]测试区Site{site + 1}升降气缸未在原位!" + result, LogType.AlarmLog);
                                }
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 720:
                            PS测试手臂取待测料流程.StepName = "测试Z向下移动";
                            if (StationManagers.GetStationState(site) == 0)
                            {
                                RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");
                                result = 获取测试Z放料补偿(site, out double sitez);
                                if (result == "Success")
                                {
                                    result = 测试手臂Z移动(初始点位.测试_Test区放_Z + sitez, true); //测试区放
                                    if (result == "Success")
                                    {
                                        PS测试手臂取待测料流程.StepID = 725;

                                        if (后方定点停止允许)
                                        {
                                            后方定点停止允许 = false;
                                            ProgramPause = true;
                                            IsPause = true;
                                            RunLog($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->测试手臂已定点停止完成");
                                        }
                                    }
                                    else
                                    {
                                        Listcode.Add("2026007");
                                        RunLog($"[Code]2026007,[Msg]测试Z向下移动失败!" + result, LogType.AlarmLog);
                                        M19_测试手臂Z轴ErrorMsg = result;
                                        ProgramPause = true;
                                        IsPause = true;
                                    }
                                }
                            }
                            else Thread.Sleep(10);
                            break;

                        case 725:
                            PS测试手臂取待测料流程.StepName = "测试吸嘴放料";
                            RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");
                            result = 空跑模式 ? "Success" : 测试吸嘴_吸料(lsopen, false);
                            if (result == "Success")
                            {
                                result = 空跑模式 ? "Success" : 测试吸嘴_放料(lsopen, true);
                                if (result == "Success")
                                {
                                    Thread.Sleep(设置信息.测试机信息.测试手臂吹料时间);
                                    result = 空跑模式 ? "Success" : 测试吸嘴_放料(lsopen, false);
                                    if (result == "Success")
                                    {
                                        PS测试手臂取待测料流程.StepID = 730;
                                    }
                                    else
                                    {
                                        RunLog($"测试吸嘴_关破真空失败!" + result);
                                        ProgramPause = true;
                                        IsPause = true;
                                    }
                                }
                                else
                                {
                                    RunLog($"测试吸嘴_开破真空失败!" + result);
                                    ProgramPause = true;
                                    IsPause = true;
                                }
                            }
                            else
                            {
                                RunLog($"测试吸嘴_关吸真空失败!" + result);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 730:
                            PS测试手臂取待测料流程.StepName = "测试Z向上移动";
                            RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");
                            result = 测试手臂Z移动(-0.3, false);
                            if (result == "Success")
                            {
                                PS测试手臂取待测料流程.StepID = 735;
                            }
                            else
                            {
                                Listcode.Add("2026007");
                                RunLog($"[Code]2026007,[Msg]测试Z向上移动失败!" + result, LogType.AlarmLog);
                                M19_测试手臂Z轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;


                        case 735:
                            PS测试手臂取待测料流程.StepName = "测试Z是否在原点";
                            RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");
                            result = motionEx.GetDiHome(new string[] { "M19_测试手臂Z轴" }, out home);
                            if (result == "Success")
                            {
                                if (home[0] == "0") //如果有1说明有吸嘴没有挡住原点
                                {
                                    Thread.Sleep(10);//回原后延时一下再读取IO
                                    PS测试手臂取待测料流程.StepID = 740;
                                }
                                else
                                {

                                    RunLog($"测试Z没有挡住原点");

                                    if (firstdzhome)
                                    {
                                        firstdzhome = false;
                                    }
                                    else
                                    {
                                        Listcode.Add($"1400468");
                                        RunLog($"[Code]1400468,[Msg]测试Z没有挡住原点", LogType.AlarmLog);
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
                                RunLog($"测试Z回原失败!" + result);
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
                                RunLog($"测试手臂Z回零失败!" + result);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 740:
                            PS测试手臂取待测料流程.StepName = $"检测是否IC放歪";
                            RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");
                            result = 测试区_左右对射感应(site, out iovalues);
                            result2 = 测试区_上下对射感应(site, out iovalues2);
                            if (result == "Success" && result2 == "Success")
                            {
                                if (!iovalues.Contains("0") && !iovalues2.Contains("0"))
                                {
                                    StationManagers.SetStationIC(site, new bool[][]
                                    {
                                        new bool[] { lsopen[0], lsopen[1], lsopen[2], lsopen[3] },
                                        new bool[] { lsopen[4], lsopen[5], lsopen[6], lsopen[7] }
                                    });

                                    StationManagers.SetStationState(site, 1);// SiteTestState[site] = 1; //有料

                                    if (飞梭后出料状态 == 1)
                                    {
                                        DIndexTime = DateTime.Now - IndexTime;
                                    }

                                    PS测试手臂取待测料流程.StepID = 799;
                                }
                                else
                                {
                                    PS测试手臂取待测料流程.StepID = 745;
                                }
                            }
                            else
                            {
                                if (result != "Success")
                                {
                                    RunLog("测试区_左右对射感应读取失败!" + result);
                                }
                                if (result2 != "Success")
                                {
                                    RunLog("测试区_上下对射感应读取失败!" + result2);
                                }
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 745:
                            PS测试手臂取待测料流程.StepName = $"测试手臂X走到梭车避让";
                            RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");
                            result = 测试手臂X移动(初始点位.测试_梭车1吸_X);
                            if (result == "Success")
                            {
                                if (iovalues.Contains("0"))
                                {
                                    for (int i = 0; i < iovalues.Length; i++)
                                    {
                                        if (iovalues[i] == "0")
                                        {
                                            Listcode.Add($"7005{i + 1}{site:D2}");
                                            RunLog($"[Code]7005{i + 1}{site:D2},[Msg]Site{site + 1} Test X Sensor {i + 1} IC放歪", LogType.AlarmLog);
                                        }
                                    }
                                }
                                if (iovalues2.Contains("0"))
                                {
                                    for (int i = 0; i < iovalues2.Length; i++)
                                    {
                                        if (iovalues2[i] == "0")
                                        {
                                            Listcode.Add($"7007{i + 8}{site:D2}");
                                            RunLog($"[Code]7007{i + 8}{site:D2},[Msg]Site{site + 1} Test Site Y{i + 1} Sensor IC放歪", LogType.AlarmLog);
                                        }
                                    }
                                }
                                ProgramPause = true;
                                IsPause = true;
                                PS测试手臂取待测料流程.StepID = 740;
                            }
                            else
                            {
                                Listcode.Add("2028007");
                                RunLog($"[Code]2028007,[Msg]测试手臂X走到梭车避让位失败!" + result, LogType.AlarmLog);
                                M21_测试手臂X轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 799:
                            PS测试手臂取待测料流程.StepName = $"测试手臂取待测料流程完成";
                            RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{PS测试手臂取待测料流程.StepName}");
                            PS测试手臂取待测料流程.Processing = false;
                            PS测试手臂取待测料流程.StepID = 500;
                            break;

                        default:
                            RunLog(("PS测试手臂取待测料流程.StepID =" + PS测试手臂取待测料流程.StepID.ToString() + "对应流程不存在"));
                            break;
                    }

                }
                catch (Exception ex)
                {
                    string errormsg = $"PS测试手臂取待测料流程({PS测试手臂取待测料流程.StepID})运行异常,详细信息:" + ex.Message;
                    RunLog(errormsg, LogType.AlarmLog);
                    系统未定义错误ErrorMsg = errormsg;
                    ProgramPause = true;
                    IsPause = true;
                    Listcode.Add("1400070");
                }
            }
        }

        public void 测试手臂取已测料流程()
        {
            string result = "";
            int[] opens = new int[8];
            bool[] lsopen = new bool[8];
            int site = 0;
            string[] bin = new string[8];
            bool firstdraw = true;
            bool firstdzhome = true;

            bool[] count = new bool[8];
            TimeSpan testtime = new TimeSpan(0);
            Stopwatch stopwatch = new Stopwatch();
            PS测试手臂取已测料流程.StepID = 500;
            while (true)
            {
                try
                {
                    Thread.Sleep(10);

                    if (APPmaster.IsForecePause)
                    {
                        PS测试手臂取已测料流程.State = 1;
                        continue;
                    }

                    if (!是否可以开始运行())
                    {
                        continue;
                    }

                    switch (PS测试手臂取已测料流程.StepID)
                    {
                        case 500:
                            PS测试手臂取已测料流程.StepName = "路径规划";
                            PS测试手臂取已测料流程.Condition = ((飞梭后入料状态 == 0 && 飞梭后出料状态 == 1) || (飞梭后入料状态 == 1 && 飞梭后出料状态 == 0)) && APPmaster.OutSiteQueue.Count > 0 && !测试手臂出料停止工作标记;
                            PS测试手臂取已测料流程.Permit = !PS测试手臂取待测料流程.Processing;
                            if (PS测试手臂取已测料流程.Condition && PS测试手臂取已测料流程.Permit)
                            {
                                PS测试手臂取已测料流程.Processing = true;
                                PS测试手臂取已测料流程.StepID = 501;
                            }
                            else
                            {
                                if (!StationManagers.CheckTestState(1) //测试区有料
                                    && !StationManagers.CheckTestState(10) //测试区有料
                                    && !StationManagers.CheckTestState(2) //测试区有料测试中
                                    && !StationManagers.CheckTestState(3) //测试区有料已测试
                                    && !StationManagers.CheckTestState(4) //测试区有料待取走
                                    && 测试手臂入料停止工作标记
                                    && 入料手臂停止工作标记
                                    && (结批标记 || 一个循环标记))
                                {
                                    测试手臂出料停止工作标记 = true;
                                }
                            }
                            break;

                        case 501:
                            PS测试手臂取已测料流程.StepName = "获取Site信息";
                            (int SiteId, TestDataInfo Data) sites;
                            if (OutSiteQueue.TryDequeue(out sites))
                            {
                                site = sites.SiteId;
                                bin = sites.Data.SiteTestBin;
                                testtime = sites.Data.TestTime;
                                //sites.Data.TestTime = DateTime.Now;
                                RunLog($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->获取Site{site + 1}信息");
                                IndexTime = DateTime.Now;
                                bool[][] open = StationManagers.GetStationIC(site); //SiteTestIC[site]; 
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
                                RunLog($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->获取Site信息失败!");
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 505:
                            PS测试手臂取已测料流程.StepName = "测试吸嘴_张卡气缸张开";
                            RunLog($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
                            result = 测试吸嘴_张开气缸输出(false);
                            if (result == "Success")
                            {
                                PS测试手臂取已测料流程.StepID = 510;
                            }
                            else
                            {
                                if (result != "Success")
                                {
                                    Listcode.Add($"1011001");
                                    RunLog($"[Code]1011001,[Msg]测试吸嘴_张卡气缸张开失败!" + result, LogType.AlarmLog);
                                }
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 510:
                            PS测试手臂取已测料流程.StepName = $"测试手臂X轴移动到Site{site + 1}上方";
                            RunLog($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
                            result = 获取测试X吸料补偿(site, out double sitex);
                            if (result == "Success")
                            {
                                result = 测试手臂X移动(TestXPoint[site] + sitex);
                                if (result == "Success")
                                {
                                    firstdraw = true;
                                    firstdzhome = true;
                                    count = new bool[] { lsopen[0], lsopen[1], lsopen[2], lsopen[3], lsopen[4], lsopen[5], lsopen[6], lsopen[7] };
                                    PS测试手臂取已测料流程.StepID = 515;
                                }
                                else
                                {
                                    Listcode.Add("2028007");
                                    RunLog($"[Code]2028007,[Msg]测试手臂X轴移动到Site{site + 1}上方失败!" + result, LogType.AlarmLog);
                                    M21_测试手臂X轴ErrorMsg = result;
                                    ProgramPause = true;
                                    IsPause = true;
                                }
                            }
                            break;

                        case 515:
                            PS测试手臂取已测料流程.StepName = "测试吸嘴_张开气缸到位信号读取";
                            RunLog($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
                            result = 测试吸嘴张开气缸原位输入读取();
                            if (result == "Success")
                            {
                                PS测试手臂取已测料流程.StepID = 520;
                                RunLog($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->测试Z等待气缸动作完成");
                            }
                            else
                            {
                                Listcode.Add($"1011001");
                                RunLog($"[Code]1011001,[Msg]测试吸嘴_张开气缸到位信号读取失败!" + result, LogType.AlarmLog);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 520:
                            PS测试手臂取已测料流程.StepName = "测试Z向下移动";
                            if (StationManagers.GetStationState(site) == 4) //待取出
                            {
                                string result3 = 测试区_位移气缸原位信号读取(site);
                                string result4 = 测试区_升降气缸原位信号读取(site);
                                if (result3 == "Success" && result4 == "Success")
                                {
                                    RunLog($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
                                    result = 获取测试Z吸料补偿(site, out double sitez);
                                    if (result == "Success")
                                    {
                                        if (设置信息.测试机信息.测试吸提前开真空 && !空跑模式)
                                        {
                                            测试吸嘴_吸放料(lsopen, true, true);
                                        }

                                        result = 测试手臂Z移动(初始点位.测试_Test区吸_Z + sitez, true);
                                        if (result == "Success")
                                        {
                                            PS测试手臂取已测料流程.StepID = 525;
                                        }
                                        else
                                        {
                                            Listcode.Add("2026007");
                                            RunLog($"[Code]2026007,[Msg]测试Z向下移动失败!" + result, LogType.AlarmLog);
                                            M19_测试手臂Z轴ErrorMsg = result;
                                            ProgramPause = true;
                                            IsPause = true;
                                        }
                                    }
                                }
                                else
                                {
                                    if (result3 != "Success")
                                    {
                                        Listcode.Add($"10{site + 30:D2}001");
                                        RunLog($"[Code]10{site + 30:D2}001,[Msg]测试区Site{site + 1}位移气缸未在原位!" + result, LogType.AlarmLog);
                                    }
                                    if (result4 != "Success")
                                    {
                                        Listcode.Add($"10{site + 14:D2}001");
                                        RunLog($"[Code]10{site + 14:D2}001,[Msg]测试区Site{site + 1}升降气缸未在原位!" + result, LogType.AlarmLog);
                                    }
                                    ProgramPause = true;
                                    IsPause = true;
                                }
                            }
                            else Thread.Sleep(10);
                            break;

                        case 525:
                            PS测试手臂取已测料流程.StepName = "测试吸嘴吸料";
                            RunLog($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
                            result = 空跑模式 ? "Success" : 测试吸嘴_吸放料(lsopen, true, true);
                            if (result == "Success")
                            {
                                PS测试手臂取已测料流程.StepID = 530;
                            }
                            else
                            {
                                RunLog($"测试吸嘴开吸真空失败!" + result);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 530:
                            PS测试手臂取已测料流程.StepName = "测试吸嘴真空检测";
                            RunLog($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");

                            if (!空跑模式)
                            {

                                stopwatch.Start();

                                while (JudgeNozzleSts(lsopen, opens, 0))
                                {
                                    if (stopwatch.ElapsedMilliseconds > 设置信息.测试机信息.测试手臂吸料超时时间)
                                    {
                                        RunLog($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->测试吸料超时");
                                        break;
                                    }

                                    result = 测试吸嘴_真空检测(out string[] ioval);
                                    if (result != "Success") break;

                                    RunLog($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{ioval[0]},{ioval[1]},{ioval[2]},{ioval[3]},{ioval[4]},{ioval[5]},{ioval[6]},{ioval[7]}:{stopwatch.ElapsedMilliseconds}ms");

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

                                bool[] bools = new bool[8] { lsopen[0] && opens[0] == 0, lsopen[1] && opens[1] == 0, lsopen[2] && opens[2] == 0, lsopen[3] && opens[3] == 0, lsopen[4] && opens[4] == 0, lsopen[5] && opens[5] == 0, lsopen[6] && opens[6] == 0, lsopen[7] && opens[7] == 0 };
                                测试吸嘴_吸放料(bools, true, false);
                            }
                            else
                            {
                                result = "Success";
                                for (int i = 0; i < lsopen.Length; i++)
                                {
                                    if (lsopen[i]) opens[i] = 1;
                                }
                                Thread.Sleep(设置信息.测试机信息.测试手臂吹料时间);
                            }
                            if (result == "Success")
                            {
                                PS测试手臂取已测料流程.StepID = 535;
                            }
                            else
                            {
                                ProgramPause = true;
                                IsPause = true;
                                RunLog($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->测试吸嘴_真空检测失败" + result);
                            }
                            break;

                        case 535:
                            PS测试手臂取已测料流程.StepName = "测试Z向上移动";
                            RunLog($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
                            result = 测试手臂Z移动(-0.3, false);
                            if (result == "Success")
                            {
                                if (!空跑模式)
                                {
                                    //上来再侦测一次
                                    result = 测试吸嘴_真空检测(out string[] ioval);
                                    if (result != "Success") break;

                                    RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->{ioval[0]},{ioval[1]},{ioval[2]},{ioval[3]},{ioval[4]},{ioval[5]},{ioval[6]},{ioval[7]}");

                                    for (int i = 0; i < ioval.Length; i++)
                                    {
                                        if (lsopen[i])
                                        {
                                            if (opens[i] == 1 && ioval[i] == "0")
                                            {
                                                opens[i] = 0;
                                            }
                                        }
                                    }
                                    bool[] bools = new bool[8] { lsopen[0] && opens[0] == 0, lsopen[1] && opens[1] == 0, lsopen[2] && opens[2] == 0, lsopen[3] && opens[3] == 0, lsopen[4] && opens[4] == 0, lsopen[5] && opens[5] == 0, lsopen[6] && opens[6] == 0, lsopen[7] && opens[7] == 0 };
                                    测试吸嘴_吸放料(bools, true, false);
                                }

                                for (int i = 0; i < lsopen.Length; i++)
                                {
                                    if (lsopen[i])
                                    {
                                        if (opens[i] == 1)
                                        {
                                            lsopen[i] = false;
                                            //RunLog($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->测试Z{i + 1}吸料成功");
                                        }
                                        else
                                        {
                                            if (count[i])
                                            {
                                                count[i] = false;
                                                测试手臂吸不起数量++;
                                            }
                                            RunLog($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->测试Z{i + 1}吸料失败");
                                        }
                                    }
                                }
                                PS测试手臂取已测料流程.StepID = 540;
                            }
                            else
                            {
                                Listcode.Add("2026007");
                                RunLog($"[Code]2026007,[Msg]测试Z向上移动失败!" + result, LogType.AlarmLog);
                                M19_测试手臂Z轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 540:
                            PS测试手臂取已测料流程.StepName = "测试Z是否在原点";
                            RunLog($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
                            result = motionEx.GetDiHome(new string[] { "M19_测试手臂Z轴" }, out string[] home);
                            if (result == "Success")
                            {
                                RunLog($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->测试Z回原：{home[0]}");
                                if (home[0] == "0") //如果有1说明有吸嘴没有挡住原点
                                {
                                    PS测试手臂取已测料流程.StepID = 542;
                                }
                                else
                                {
                                    if (home.Contains("1"))
                                    {
                                        RunLog($"测试Z没有挡住原点");

                                        if (firstdzhome)
                                        {
                                            firstdzhome = false;
                                        }
                                        else
                                        {
                                            Listcode.Add($"1400468");
                                            RunLog($"[Code]1400468,[Msg]测试Z没有挡住原点", LogType.AlarmLog);
                                            ProgramPause = true;
                                            APPmaster.IsPause = true;
                                        }
                                    }
                                    PS测试手臂取已测料流程.StepID = 541;
                                }
                            }
                            else
                            {
                                RunLog($"测试Z回原失败!" + result);
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
                                RunLog($"测试手臂Z回零失败!" + result);
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
                                    PS测试手臂取已测料流程.StepID = 545;
                                }
                            }
                            else
                            {
                                if (opens.Contains(1)) //如果在测试区至少吸取了至少一个IC
                                {
                                    StationManagers.SetStationIC(site, new bool[][] { new bool[] { false, false, false, false }, new bool[] { false, false, false, false } });
                                    //SiteTestIC[site] = new bool[][] { new bool[] { false, false, false, false }, new bool[] { false, false, false, false } };
                                    StationManagers.SetStationState(site, 0);// SiteTestState[site] = 0; //无料
                                    PS测试手臂取已测料流程.StepID = 599;
                                }
                                else //如何一颗料都没有吸起来,说明该吸料的吸嘴一个都没有吸上来，则不用再放料到梭车出料上了, 会发生什么呢？出料不会至为1，不会
                                {
                                    InSiteQueue.Enqueue(site);
                                    RunLog($"PS测试手臂取待测料流程.StepID={PS测试手臂取待测料流程.StepID}->在测试区Site{site + 1}未吸起一颗IC(放入梭车)!");
                                    PS测试手臂取已测料流程.Processing = false;
                                    PS测试手臂取已测料流程.StepID = 500;
                                }
                            }
                            break;

                        case 543:
                            if (测试IC忽略)
                            {
                                //if (opens.Contains(1)) //如果有吸嘴吸了IC则放置
                                //{

                                for (int i = 0; i < lsopen.Length; i++)
                                {
                                    if (lsopen[i] && opens[i] == 0)
                                    {
                                        lsopen[i] = false;
                                        opens[i] = 2;
                                        RunLog($"测试区位置:Site{site + 1:D2}-{i + 1}的IC因测试Z{i + 1}吸料失败被跳过,请手动吸取出IC!", LogType.OperateLog);
                                    }
                                }
                                PS测试手臂取已测料流程.StepID = 542;

                                //}
                                //else //吸嘴上没有IC,则直接返回
                                //{
                                //    测试IC忽略 = false;
                                //    ProgramPause = true;
                                //    IsPause = true;
                                //    RunLog("测试Z吸嘴至少吸上1颗IC");
                                //    //opens = new bool[] { false, false, false, false };
                                //    //PS测试手臂取已测料流程.StepID = 505;
                                //}
                            }
                            else //直接点开始默认重吸
                            {
                                PS测试手臂取已测料流程.StepID = 505;
                            }
                            break;

                        case 545:
                            PS测试手臂取已测料流程.StepName = "测试Z吸料不起移到梭车上避让";
                            RunLog($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
                            result = 测试手臂X移动(初始点位.测试_梭车1放_X);
                            if (result == "Success")
                            {
                                for (int i = 0; i < lsopen.Length; i++)
                                {
                                    if (lsopen[i] && opens[i] == 0)
                                    {
                                        Listcode.Add($"3{i + 9:D2}{site:D2}00");
                                        RunLog($"[Code]3{i + 9:D2}{site:D2}00,[Msg]测试区位置:Site{site + 1:D2}-{i + 1},测试Z{i + 1}吸料失败!", LogType.AlarmLog);
                                        //Listcode.Add($"3{i + 9:D2}9900");
                                        //RunLog($"[Code]3{i + 9:D2}9900,[Msg]位置:Site{site + 1:D2}-{i + 1},测试Z{i + 1}吸料失败!", LogType.AlarmLog);
                                    }
                                }
                                测试IC忽略 = false;
                                测试区IC忽略 = true;
                                ProgramPause = true;
                                IsPause = true;
                                motionEx.SetDo("Y004_重试灯", true);
                                motionEx.SetDo("Y005_忽略灯", true);
                                PS测试手臂取已测料流程.StepID = 543;
                            }
                            else
                            {
                                Listcode.Add("2028007");
                                RunLog($"[Code]2028007,[Msg]测试Z吸料不起移到梭车上避让失败!" + result, LogType.AlarmLog);
                                M21_测试手臂X轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 599:
                            TestNozzleICOpen = new bool[] { opens[0] == 1, opens[1] == 1, opens[2] == 1, opens[3] == 1, opens[4] == 1, opens[5] == 1, opens[6] == 1, opens[7] == 1 };
                            //TestNozzleICOpen = new bool[] { lsopen[0], lsopen[1], lsopen[2], lsopen[3], lsopen[4], lsopen[5], lsopen[6], lsopen[7] };
                            TestNozzleIC = true;
                            PS测试手臂取已测料流程.StepName = "测试手臂取已测料流程吸料完成";
                            RunLog($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
                            PS测试手臂取已测料流程.StepID = 705;
                            break;

                        case 705:
                            PS测试手臂取已测料流程.StepName = "测试吸嘴_闭合";
                            RunLog($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
                            result = 测试吸嘴_张开气缸输出(true);
                            if (result == "Success")
                            {
                                PS测试手臂取已测料流程.StepID = 710;
                            }
                            else
                            {
                                Listcode.Add($"1011000");
                                RunLog($"[Code]1011000,[Msg]测试吸嘴_闭合失败!" + result, LogType.AlarmLog);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 710:
                            PS测试手臂取已测料流程.StepName = "测试手臂X移动到梭车";
                            RunLog($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
                            int state = 飞梭后出料状态 == 0 ? 0 : 1;
                            double xpos = 飞梭2位置 == state ? (初始点位.测试_梭车2放_X + 点位微调.测试_梭车2右放_X) : (初始点位.测试_梭车1放_X + 点位微调.测试_梭车1右放_X);
                            result = 测试手臂X移动(xpos);
                            if (result == "Success")
                            {
                                firstdraw = true;
                                firstdzhome = true;
                                TestNozzleIC = false;
                                lsopen = TestNozzleICOpen;
                                PS测试手臂取已测料流程.StepID = 715;
                            }
                            else
                            {
                                Listcode.Add("2028007");
                                RunLog($"[Code]2028007,[Msg]测试手臂X移动到梭车失败!" + result, LogType.AlarmLog);
                                M21_测试手臂X轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 715:
                            PS测试手臂取已测料流程.StepName = "测试吸嘴张开气缸到位输入读取";
                            RunLog($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
                            result = 测试吸嘴张开气缸到位输入读取();
                            if (result == "Success")
                            {
                                PS测试手臂取已测料流程.StepID = 720;
                                RunLog($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->等待梭车到达");
                            }
                            else
                            {
                                Listcode.Add($"1011000");
                                RunLog($"[Code]1011000,[Msg]测试吸嘴张开气缸到位输入读取失败!" + result, LogType.AlarmLog);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 720:
                            PS测试手臂取已测料流程.StepName = "测试Z向下移动";
                            if (飞梭后出料状态 == 0) //等梭车过来再放
                            {
                                RunLog($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
                                double zpos = 飞梭2位置 == 0 ? (初始点位.测试_梭车2放_Z + 点位微调.测试_梭车2右放_Z) : (初始点位.测试_梭车1放_Z + 点位微调.测试_梭车1右放_Z);
                                result = 测试手臂Z移动(zpos, true);
                                if (result == "Success")
                                {
                                    PS测试手臂取已测料流程.StepID = 725;

                                    if (后方定点停止允许)
                                    {
                                        后方定点停止允许 = false;
                                        ProgramPause = true;
                                        IsPause = true;
                                        RunLog($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->测试手臂已定点停止完成");
                                    }
                                }
                                else
                                {
                                    Listcode.Add("2026007");
                                    RunLog($"[Code]2026007,[Msg]测试Z向下移动失败!" + result, LogType.AlarmLog);
                                    M19_测试手臂Z轴ErrorMsg = result;
                                    ProgramPause = true;
                                    IsPause = true;
                                }
                            }
                            else Thread.Sleep(10);
                            break;

                        case 725:
                            PS测试手臂取已测料流程.StepName = "测试吸嘴放料";
                            RunLog($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
                            result = 空跑模式 ? "Success" : 测试吸嘴_吸放料(lsopen, true, false);
                            if (result == "Success")
                            {
                                result = 空跑模式 ? "Success" : 测试吸嘴_吸放料(lsopen, false, true);
                                if (result == "Success")
                                {
                                    Thread.Sleep(设置信息.测试机信息.测试手臂吹料时间);
                                    result = 空跑模式 ? "Success" : 测试吸嘴_吸放料(lsopen, false, false);
                                    if (result == "Success")
                                    {
                                        PS测试手臂取已测料流程.StepID = 730;
                                    }
                                    else
                                    {
                                        RunLog($"测试吸嘴_关破真空失败!" + result);
                                        ProgramPause = true;
                                        IsPause = true;
                                    }
                                }
                                else
                                {
                                    RunLog($"测试吸嘴_开破真空失败!" + result);
                                    ProgramPause = true;
                                    IsPause = true;
                                }
                            }
                            else
                            {
                                RunLog($"测试吸嘴_关吸真空失败!" + result);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 730:
                            PS测试手臂取已测料流程.StepName = "测试Z向上移动";
                            RunLog($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
                            result = 测试手臂Z移动(-0.3, false);
                            if (result == "Success")
                            {
                                PS测试手臂取已测料流程.StepID = 735;
                            }
                            else
                            {
                                Listcode.Add("2026007");
                                RunLog($"[Code]2026007,[Msg]测试Z向上移动失败!" + result, LogType.AlarmLog);
                                M19_测试手臂Z轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;


                        case 735:
                            PS测试手臂取已测料流程.StepName = "测试Z是否在原点";
                            RunLog($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
                            result = motionEx.GetDiHome(new string[] { "M19_测试手臂Z轴" }, out home);
                            if (result == "Success")
                            {
                                RunLog($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->测试Z回原：{home[0]}");
                                if (home[0] == "0") //如果有1说明有吸嘴没有挡住原点
                                {
                                    PS测试手臂取已测料流程.StepID = 740;
                                }
                                else
                                {
                                    RunLog($"测试Z没有挡住原点");

                                    if (firstdzhome)
                                    {
                                        firstdzhome = false;
                                    }
                                    else
                                    {
                                        Listcode.Add($"1400468");
                                        RunLog($"[Code]1400468,[Msg]测试Z没有挡住原点", LogType.AlarmLog);
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
                                RunLog($"测试Z回原失败!" + result);
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
                                RunLog($"测试手臂Z回零失败!" + result);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 740:
                            PS测试手臂取已测料流程.StepName = "BIN项保存";
                            RunLog($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
                            //if (BinSaveQueue.TryDequeue(out string[] bin))
                            //{
                            飞梭后出料状态 = 1;
                            飞梭后出料IC = new bool[][] { new bool[] { lsopen[0], lsopen[1], lsopen[2], lsopen[3] }, new bool[] { lsopen[4], lsopen[5], lsopen[6], lsopen[7] } }; //SitesTestIC
                            飞梭后出料BIN项 = new string[][] { new string[] { bin[0], bin[1], bin[2], bin[3] }, new string[] { bin[4], bin[5], bin[6], bin[7] } };
                            飞梭后出料TestTime = testtime;
                            飞梭后出料TestSite = site;
                            InSiteQueue.Enqueue(site);

                            RunLog($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->添加Site:{site + 1}");
                            PS测试手臂取已测料流程.StepID = 799;
                            //}
                            //else
                            //{
                            //    RunLog($"Site{site + 1}BIN项保存失败!");
                            //    ProgramPause = true;
                            //    IsPause = true;
                            //}
                            break;

                        case 799:
                            PS测试手臂取已测料流程.StepName = $"测试手臂取已测料流程完成";
                            RunLog($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->{PS测试手臂取已测料流程.StepName}");
                            PS测试手臂取已测料流程.Processing = false;
                            PS测试手臂取已测料流程.StepID = 500;
                            break;

                        default:
                            RunLog(("PS测试手臂取已测料流程.StepID =" + PS测试手臂取已测料流程.StepID.ToString() + "对应流程不存在"));
                            break;
                    }

                }
                catch (Exception ex)
                {
                    string errormsg = $"PS测试手臂取已测料流程({PS测试手臂取已测料流程.StepID})运行异常,详细信息:" + ex.Message;
                    RunLog(errormsg, LogType.AlarmLog);
                    系统未定义错误ErrorMsg = errormsg;
                    ProgramPause = true;
                    IsPause = true;
                    Listcode.Add("1400070");
                }
            }
        }

        public void 测试区气缸下压动作流程()
        {
            string result;
            short site = 0;
            List<bool[]> open = new List<bool[]>();
            List<List<bool[]>> opens = new List<List<bool[]>>();
            PS测试区气缸下压动作流程.StepID = 500;
            while (true)
            {
                try
                {
                    Thread.Sleep(10);

                    if (APPmaster.IsForecePause)
                    {
                        PS测试区气缸下压动作流程.State = 1;
                        continue;
                    }

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

                            site++;
                            if (site >= 16)
                            {
                                site = 0;
                            }

                            PS测试区气缸下压动作流程.StepName = $"循环检测Site状态";
                            if (StationManagers.GetStationState(site) == 1 || StationManagers.GetStationState(site) == 10) //有料
                            {
                                if (StationManagers.GetStationState(site) == 1)
                                {
                                    RunLog($"PS测试区气缸下压动作流程.StepID={PS测试区气缸下压动作流程.StepID}->循环检测Site状态:{site + 1}有料");
                                    bool[][] siteopne = StationManagers.GetStationIC(site); //APPmaster.SiteTestIC[site];
                                    open = new List<bool[]>()
                                    {
                                        new bool[] { siteopne[0][0],siteopne[0][1],siteopne[0][2] ,siteopne[0][3]  },
                                        new bool[] { siteopne[1][0],siteopne[1][1],siteopne[1][2] ,siteopne[1][3]  },
                                    };
                                    PS测试区气缸下压动作流程.StepID = 510;
                                }
                                else if (StationManagers.GetStationState(site) == 10)
                                {
                                    PS测试区气缸下压动作流程.StepID = 520;
                                }
                            }
                            break;

                        case 510:
                            PS测试区气缸下压动作流程.StepName = $"测试区气缸前移";
                            RunLog($"PS测试区气缸下压动作流程.StepID={PS测试区气缸下压动作流程.StepID}->{PS测试区气缸下压动作流程.StepName}");
                            result = 测试区_位移气缸到位输出(site);
                            if (result == "Success")
                            {
                                Thread.Sleep(100);
                                PS测试区气缸下压动作流程.StepID = 515;
                            }
                            else
                            {
                                Listcode.Add($"10{site + 30:D2}000");
                                RunLog($"[Code]10{site + 30:D2}000,[Msg]测试区Site{site + 1}气缸前移失败!" + result, LogType.AlarmLog);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 515:
                            PS测试区气缸下压动作流程.StepName = $"测试区气缸下压";
                            RunLog($"PS测试区气缸下压动作流程.StepID={PS测试区气缸下压动作流程.StepID}->{PS测试区气缸下压动作流程.StepName}");
                            result = 测试区_下压气缸到位输出(site);
                            if (result == "Success")
                            {
                                if (设置信息.通讯信息.同时测试 == 1)
                                {
                                    Thread.Sleep(设置信息.测试机信息.发送开始信号延时);
                                }
                                StationManagers.SetStationState(site, 10);
                                PS测试区气缸下压动作流程.StepID = 520;
                            }
                            else
                            {
                                Listcode.Add($"10{site + 14:D2}000");
                                RunLog($"[Code]10{site + 14:D2}000,[Msg]测试区Site{site + 1}气缸下压失败!" + result, LogType.AlarmLog);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 520:
                            PS测试区气缸下压动作流程.StepName = $"发送测试信号";
                            if (设置信息.通讯信息.同时测试 == 2)
                            {
                                result = "Success";
                                int[] sites = GetSites(site);
                                bool[][] siteic1 = StationManagers.GetStationIC((int)sites[0]);
                                bool[][] siteic2 = StationManagers.GetStationIC((int)sites[1]);
                                bool[] site1 = new bool[] { siteic1[0][0], siteic1[0][1], siteic1[0][2], siteic1[0][3], siteic1[1][0], siteic1[1][1], siteic1[1][2], siteic1[1][3] };
                                bool[] site2 = new bool[] { siteic2[0][0], siteic2[0][1], siteic2[0][2], siteic2[0][3], siteic2[1][0], siteic2[1][1], siteic2[1][2], siteic2[1][3] };
                                int[] states = new int[] { StationManagers.GetStationState(sites[0]), StationManagers.GetStationState(sites[1]) };

                                bool[] sitew1 = GetSiteOpen(sites[0]);
                                bool[] sitew2 = GetSiteOpen(sites[1]);

                                if (((states[0] == 10 || !sitew1.Contains(true)) && (states[1] == 10 || !sitew2.Contains(true))) || (测试手臂入料停止工作标记 && (结批标记 || 一个循环标记) && (states[0] == 10 || states[0] == 0) && (states[1] == 10 || states[1] == 0)))
                                {
                                    opens = new List<List<bool[]>>()
                                    {
                                        states[0] != 0 ? GetOpens(site1) : new List<bool[]>() { new bool[] { false, false, false, false }, new bool[] { false, false, false, false } },
                                        states[1] != 0 ? GetOpens(site2) : new List<bool[]>() { new bool[] { false, false, false, false }, new bool[] { false, false, false, false } }
                                    };
                                    //result = APPmaster.Com.Send(sites, opens, 无测试机模式);
                                    Thread.Sleep(设置信息.测试机信息.发送开始信号延时);
                                    RunLog($"PS测试区气缸下压动作流程.StepID={PS测试区气缸下压动作流程.StepID}->{PS测试区气缸下压动作流程.StepName}");
                                    result = StationManagers.Send(site, states, opens, 无测试机模式);
                                    if (result == "Success")
                                    {
                                        if (states[0] == 10) StationManagers.SetStationState(sites[0], 2);
                                        if (states[1] == 10) StationManagers.SetStationState(sites[1], 2);
                                        PS测试区气缸下压动作流程.StepID = 599;
                                    }
                                    else
                                    {
                                        Listcode.Add("800007");
                                        RunLog($"[Code]800007,[Msg]发送Site{site + 1}测试信号失败!" + result, LogType.AlarmLog);
                                        ProgramPause = true;
                                        IsPause = true;
                                    }
                                }
                                else PS测试区气缸下压动作流程.StepID = 599;
                            }
                            else if (设置信息.通讯信息.同时测试 == 4)
                            {
                                result = "Success";
                                int[] sites = GetSites2(site); //获取site所属的主站
                                bool[][] siteic1 = StationManagers.GetStationIC((int)sites[0]);
                                bool[][] siteic2 = StationManagers.GetStationIC((int)sites[1]);
                                bool[][] siteic3 = StationManagers.GetStationIC((int)sites[2]);
                                bool[][] siteic4 = StationManagers.GetStationIC((int)sites[3]);
                                bool[] site1 = new bool[] { siteic1[0][0], siteic1[0][1], siteic1[0][2], siteic1[0][3], siteic1[1][0], siteic1[1][1], siteic1[1][2], siteic1[1][3] };
                                bool[] site2 = new bool[] { siteic2[0][0], siteic2[0][1], siteic2[0][2], siteic2[0][3], siteic2[1][0], siteic2[1][1], siteic2[1][2], siteic2[1][3] };
                                bool[] site3 = new bool[] { siteic3[0][0], siteic3[0][1], siteic3[0][2], siteic3[0][3], siteic3[1][0], siteic3[1][1], siteic3[1][2], siteic3[1][3] };
                                bool[] site4 = new bool[] { siteic4[0][0], siteic4[0][1], siteic4[0][2], siteic4[0][3], siteic4[1][0], siteic4[1][1], siteic4[1][2], siteic4[1][3] };
                                int[] states = new int[] { StationManagers.GetStationState(sites[0]), StationManagers.GetStationState(sites[1]), StationManagers.GetStationState(sites[2]), StationManagers.GetStationState(sites[3]) };

                                bool[] sitew1 = GetSiteOpen(sites[0]);
                                bool[] sitew2 = GetSiteOpen(sites[1]);
                                bool[] sitew3 = GetSiteOpen(sites[2]);
                                bool[] sitew4 = GetSiteOpen(sites[3]);

                                if (((states[0] == 10 || !sitew1.Contains(true)) && (states[1] == 10 || !sitew2.Contains(true)) && (states[2] == 10 || !sitew3.Contains(true)) && (states[3] == 10 || !sitew4.Contains(true))) || (测试手臂入料停止工作标记 && (结批标记 || 一个循环标记) && (states[0] == 10 || states[0] == 0) && (states[1] == 10 || states[1] == 0) && (states[2] == 10 || states[2] == 0) && (states[3] == 10 || states[3] == 0)))
                                {
                                    opens = new List<List<bool[]>>() //如果状态==0则全部为False
                                    {
                                        states[0] != 0 ? GetOpens(site1) : new List<bool[]>() { new bool[] { false, false, false, false }, new bool[] { false, false, false, false } },
                                        states[1] != 0 ? GetOpens(site2) : new List<bool[]>() { new bool[] { false, false, false, false }, new bool[] { false, false, false, false } },
                                        states[2] != 0 ? GetOpens(site3) : new List<bool[]>() { new bool[] { false, false, false, false }, new bool[] { false, false, false, false } },
                                        states[3] != 0 ? GetOpens(site4) : new List<bool[]>() { new bool[] { false, false, false, false }, new bool[] { false, false, false, false } },
                                    };
                                    //result = APPmaster.Com.Send(sites, opens, 无测试机模式);
                                    Thread.Sleep(设置信息.测试机信息.发送开始信号延时);
                                    RunLog($"PS测试区气缸下压动作流程.StepID={PS测试区气缸下压动作流程.StepID}->{PS测试区气缸下压动作流程.StepName}");
                                    result = StationManagers.Send(site, states, opens, 无测试机模式);
                                    if (result == "Success")
                                    {
                                        if (states[0] == 10) StationManagers.SetStationState(sites[0], 2);
                                        if (states[1] == 10) StationManagers.SetStationState(sites[1], 2);
                                        if (states[2] == 10) StationManagers.SetStationState(sites[2], 2);
                                        if (states[3] == 10) StationManagers.SetStationState(sites[3], 2);
                                        PS测试区气缸下压动作流程.StepID = 599;
                                    }
                                    else
                                    {
                                        Listcode.Add("800007");
                                        RunLog($"[Code]800007,[Msg]发送Site{site + 1}测试信号失败!" + result, LogType.AlarmLog);
                                        ProgramPause = true;
                                        IsPause = true;
                                    }
                                }
                                else PS测试区气缸下压动作流程.StepID = 599;
                            }
                            else
                            {
                                result = StationManagers.Send(site, open, 无测试机模式);
                                if (result == "Success")
                                {
                                    StationManagers.SetStationState(site, 2);
                                    PS测试区气缸下压动作流程.StepID = 599;
                                }
                                else
                                {
                                    Listcode.Add("800007");
                                    RunLog($"[Code]800007,[Msg]发送Site{site + 1}测试信号失败!" + result, LogType.AlarmLog);
                                    ProgramPause = true;
                                    IsPause = true;
                                }
                            }
                            break;

                        case 599:
                            PS测试区气缸下压动作流程.StepName = $"测试区{site + 1}气缸动作流程完成";
                            //RunLog($"PS测试区气缸下压动作流程.StepID={PS测试区气缸下压动作流程.StepID}->{PS测试区气缸下压动作流程.StepName}");
                            PS测试区气缸下压动作流程.Processing = false;
                            PS测试区气缸下压动作流程.StepID = 500;
                            break;

                        default:
                            RunLog(("PS测试区气缸动作流程.StepID =" + PS测试区气缸下压动作流程.StepID.ToString() + "对应流程不存在"));
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
                catch (Exception ex)
                {
                    string errormsg = $"PS测试区气缸下压动作流程({PS测试区气缸下压动作流程.StepID})运行异常,详细信息:" + ex.Message;
                    RunLog(errormsg, LogType.AlarmLog);
                    系统未定义错误ErrorMsg = errormsg;
                    ProgramPause = true;
                    IsPause = true;
                    Listcode.Add("1400070");
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
                try
                {
                    Thread.Sleep(10);

                    if (APPmaster.IsForecePause)
                    {
                        PS测试区气缸上抬动作流程.State = 1;
                        continue;
                    }

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
                            if (StationManagers.testDataInfos[site].SiteTestState == 3) //已测试 SiteTestState[site] == 3
                            {
                                RunLog($"PS测试区气缸上抬动作流程.StepID={PS测试区气缸上抬动作流程.StepID}->循环检测Site状态:{site + 1}测试完成");
                                Thread.Sleep(设置信息.测试机信息.接收测试信号上抬延时);
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
                            RunLog($"PS测试区气缸上抬动作流程.StepID={PS测试区气缸上抬动作流程.StepID}->{PS测试区气缸上抬动作流程.StepName}");
                            result = 测试区_下压气缸原位输出(site);
                            if (result == "Success")
                            {
                                Thread.Sleep(100);
                                PS测试区气缸上抬动作流程.StepID = 515;
                            }
                            else
                            {
                                Listcode.Add($"10{site + 14:D2}001");
                                RunLog($"[Code]10{site + 14:D2}001,[Msg]测试区Site{site + 1}气缸抬升失败!" + result, LogType.AlarmLog);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 515:
                            PS测试区气缸上抬动作流程.StepName = $"测试区气缸后移";
                            RunLog($"PS测试区气缸上抬动作流程.StepID={PS测试区气缸上抬动作流程.StepID}->{PS测试区气缸上抬动作流程.StepName}");
                            result = 测试区_位移气缸原位输出(site);
                            if (result == "Success")
                            {
                                Thread.Sleep(100);
                                StationManagers.SetStationState(site, 4);// SiteTestState[site] = 4;
                                PS测试区气缸上抬动作流程.StepID = 599;
                            }
                            else
                            {
                                Listcode.Add($"10{site + 30:D2}001");
                                RunLog($"[Code]10{site + 30:D2}001,[Msg]测试区Site{site + 1}气缸后移失败!" + result, LogType.AlarmLog);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 599:
                            PS测试区气缸上抬动作流程.StepName = $"测试区Site{site + 1}气缸上抬动作流程完成";
                            RunLog($"PS测试区气缸上抬动作流程.StepID={PS测试区气缸上抬动作流程.StepID}->{PS测试区气缸上抬动作流程.StepName}");
                            PS测试区气缸上抬动作流程.Processing = false;
                            PS测试区气缸上抬动作流程.StepID = 500;
                            break;

                        default:
                            RunLog(("PS测试区气缸上抬动作流程.StepID =" + PS测试区气缸上抬动作流程.StepID.ToString() + "对应流程不存在"));
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
                catch (Exception ex)
                {
                    string errormsg = $"PS测试区气缸上抬动作流程({PS测试区气缸上抬动作流程.StepID})运行异常,详细信息:" + ex.Message;
                    RunLog(errormsg, LogType.AlarmLog);
                    系统未定义错误ErrorMsg = errormsg;
                    ProgramPause = true;
                    IsPause = true;
                    Listcode.Add("1400070");
                }
            }
        }

        public void Empty顶盘流程()
        {
            string result = "";
            PS空盘上顶流程.StepID = 500;
            while (true)
            {
                try
                {
                    Thread.Sleep(10);

                    if (APPmaster.IsForecePause)
                    {
                        PS空盘上顶流程.State = 1;
                        continue;
                    }

                    if (APPmaster.IsPause && Empty舱门开启标记 == 0)
                    {
                        continue;
                    }

                    switch (PS空盘上顶流程.StepID)
                    {
                        case 999:
                            PS空盘上顶流程.StepName = "空盘上顶流程已停止";
                            break;

                        case 500:
                            PS空盘上顶流程.StepName = "路径规划";
                            PS空盘上顶流程.Condition = Empty上顶标记 == 2;// && Empty下降标记 == 1(下降时，会要上顶流程)
                            PS空盘上顶流程.Permit = !PS空盘上顶流程.Processing; //&& !PS空盘下降流程.Processing && !PS空盘下降回零流程.Processing;
                            if (PS空盘上顶流程.Condition && PS空盘上顶流程.Permit)
                            {
                                PS空盘上顶流程.Processing = true;
                                PS空盘上顶流程.StepID = 505;
                            }
                            else Thread.Sleep(10);
                            break;

                        case 505:
                            PS空盘上顶流程.StepName = "Empty向上移动";
                            RunLog($"PS空盘上顶流程.StepID={PS空盘上顶流程.StepID}->{PS空盘上顶流程.StepName}");
                            result = Empty向上移动(Empty舱门开启标记);
                            if (result == "Success")
                            {
                                PS空盘上顶流程.StepID = 510;
                            }
                            else
                            {
                                PS空盘上顶流程.StepID = 600;
                                RunLog("Empty向上移动失败!" + result);
                            }
                            break;

                        case 510:
                            PS空盘上顶流程.StepName = "Empty向下移动";
                            RunLog($"PS空盘上顶流程.StepID={PS空盘上顶流程.StepID}->{PS空盘上顶流程.StepName}");
                            result = Empty向下移动(Empty舱门开启标记);
                            if (result == "Success")
                            {
                                PS空盘上顶流程.StepID = 515;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog("Empty向下移动失败!" + result);
                            }
                            break;

                        case 515:
                            PS空盘上顶流程.StepName = "Empty向上移动固定距离";
                            RunLog($"PS空盘上顶流程.StepID={PS空盘上顶流程.StepID}->{PS空盘上顶流程.StepName}");
                            result = Empty顶盘轴移动2(Empty原点偏移, Empty舱门开启标记);//3175 1.5875
                            if (result == "Success")
                            {
                                Empty上顶标记 = 1;
                                PS空盘上顶流程.StepID = 599;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog("Empty向上移动固定距离失败!" + result);
                            }
                            break;

                        case 599:
                            PS空盘上顶流程.StepName = $"空盘上顶流程完成";
                            RunLog($"PS空盘上顶流程.StepID={PS空盘上顶流程.StepID}->{PS空盘上顶流程.StepName}");
                            PS空盘上顶流程.Processing = false;
                            PS空盘上顶流程.StepID = 500;
                            break;

                        case 600:
                            PS空盘上顶流程.StepName = "Empty顶盘轴移动到原点";
                            RunLog($"PS空盘上顶流程.StepID={PS空盘上顶流程.StepID}->{PS空盘上顶流程.StepName}");
                            result = Empty顶盘轴移动(1, Empty舱门开启标记);
                            if (result == "Success")
                            {
                                PS空盘上顶流程.StepID = 605;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Empty顶盘轴移动到原点失败！" + result);
                            }
                            break;

                        case 605:
                            PS空盘上顶流程.StepName = "Empty顶盘轴回零";
                            RunLog($"PS空盘上顶流程.StepID={PS空盘上顶流程.StepID}->{PS空盘上顶流程.StepName}");
                            result = Empty顶盘轴回零(Empty舱门开启标记);
                            if (result == "Success")
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add("4018004"); //添加报警弹窗
                                RunLog($"[Code]4018004,[Msg]空盘区没有空盘", LogType.AlarmLog);
                                PS空盘上顶流程.StepID = 505;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Empty顶盘轴回零失败！" + result);
                            }
                            break;

                        default:
                            RunLog(("PS空盘上顶流程.StepID =" + PS空盘上顶流程.StepID.ToString() + "对应流程不存在"));
                            break;
                    }

                }
                catch (Exception ex)
                {
                    string errormsg = $"PS空盘上顶流程({PS空盘上顶流程.StepID})运行异常,详细信息:" + ex.Message;
                    RunLog(errormsg, LogType.AlarmLog);
                    系统未定义错误ErrorMsg = errormsg;
                    ProgramPause = true;
                    IsPause = true;
                    Listcode.Add("1400070");
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
                try
                {
                    Thread.Sleep(10);

                    if (APPmaster.IsForecePause)
                    {
                        PS空盘下降流程.State = 1;
                        continue;
                    }

                    if (APPmaster.IsPause && Empty舱门开启标记 == 0)
                    {
                        continue;
                    }

                    switch (PS空盘下降流程.StepID)
                    {
                        case 500:
                            PS空盘下降流程.StepName = "路径规划";
                            PS空盘下降流程.Condition = Empty下降标记 == 2 && Empty上顶标记 == 1;
                            PS空盘下降流程.Permit = !PS空盘下降流程.Processing && !PS空盘上顶流程.Processing && !PS空盘下降回零流程.Processing;
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
                                result = GetAxisRunInfo("M12_Empty顶盘轴", out AxisRunInfo axisRunInfo2);
                                if (result == "Success")
                                {
                                    movepos = Convert.ToDouble(prfpos1 * axisRunInfo2.步长mm) - Tray_厚度; //6.35
                                    PS空盘下降流程.StepID = 505;
                                }
                                else
                                {
                                    APPmaster.ProgramPause = true;
                                    IsPause = true;
                                    RunLog("M12_Empty顶盘轴信息获取失败!" + result);
                                }
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog("M12_Empty顶盘轴实时位置失败!" + result);
                            }
                            break;

                        case 505:
                            PS空盘下降流程.StepName = $"Empty向下移动{Tray_厚度}mm";
                            RunLog($"PS空盘下降流程.StepID={PS空盘下降流程.StepID}->{PS空盘下降流程.StepName}");
                            result = Empty顶盘轴移动(movepos, Empty舱门开启标记);
                            if (result == "Success")
                            {
                                PS空盘下降流程.StepID = 510;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog("Empty向下移动固定距离失败!" + result);
                            }
                            break;

                        case 510:
                            PS空盘下降流程.StepName = "判断Empty顶盘轴是否走到最小值";
                            RunLog($"PS空盘下降流程.StepID={PS空盘下降流程.StepID}->{PS空盘下降流程.StepName}");
                            result = motionEx.GetCmdEncPos("M12_Empty顶盘轴", out double prfpos);
                            string result3 = GetAxisRunInfo("M12_Empty顶盘轴", out AxisRunInfo axisRunInfo);
                            if (result == "Success" && result3 == "Success")
                            {
                                double minpoint = 初始点位.Empty_满盘位 / axisRunInfo.步长mm;
                                if (prfpos < minpoint) //空盘满盘判断值
                                {
                                    RunLog($"M12_Empty顶盘轴当前行程{prfpos}<{minpoint}");
                                    Listcode.Add("4018006");
                                    RunLog($"[Code]4018006,[Msg]空盘区空盘放满", LogType.AlarmLog);
                                    APPmaster.ProgramPause = true;
                                    IsPause = true;
                                    Empty料仓打开标记 = false;
                                    PS空盘下降流程.StepID = 600; //空盘满盘报警
                                }
                                else PS空盘下降流程.StepID = 599;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                if (result != "Success") RunLog($"读取Empty顶盘轴编码位置失败！" + result);
                                if (result3 != "Success") RunLog($"读取Empty顶盘轴运行信息失败！" + result3);
                            }
                            break;

                        case 599:
                            Empty下降标记 = 1;
                            PS空盘下降流程.StepName = $"空盘下降流程完成";
                            RunLog($"PS空盘下降流程.StepID={PS空盘下降流程.StepID}->{PS空盘下降流程.StepName}");
                            PS空盘下降流程.Processing = false;
                            PS空盘下降流程.StepID = 500;
                            break;

                        case 600:
                            PS空盘下降流程.StepName = $"检测空盘感应门有没有被打开";
                            RunLog($"PS空盘下降流程.StepID={PS空盘下降流程.StepID}->{PS空盘下降流程.StepName}");
                            if (Empty料仓打开标记)
                            {
                                PS空盘下降流程.StepID = 605;
                            }
                            else
                            {
                                Listcode.Add("4018006");
                                RunLog($"[Code]4018006,[Msg]空盘区空盘放满", LogType.AlarmLog);
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
                                    PS空盘下降流程.StepID = 610;
                                    RunLog($"PS空盘下降流程.StepID={PS空盘下降流程.StepID}->等待空盘上顶完成");
                                }
                                else //没有挡到感应器或者放歪感应器被挡到说明没有放盘成功，需重新放盘
                                {
                                    APPmaster.ProgramPause = true;
                                    IsPause = true;

                                    if (value != "0")
                                    {
                                        RunLog($"抓盘手臂放盘失败！Empty前有盘感应到有Tray盘,value:" + value);
                                    }
                                    if (value2 != "0")
                                    {
                                        RunLog($"抓盘手臂放盘失败！Empty后放歪感应到有Tray盘,value:" + value2);
                                    }
                                    RunLog("空盘区空盘放满");
                                }
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                if (result != "Success") RunLog($"读取Empty前Tray存在感应失败！" + result);
                                if (result2 != "Success") RunLog($"读取Empty后Tray放歪感应失败！" + result2);
                            }
                            break;

                        case 610:
                            PS空盘下降流程.StepName = "等待空盘上顶完成";
                            if (Empty上顶标记 == 1)
                            {
                                PS空盘下降流程.StepID = 501;
                            }
                            else Thread.Sleep(10);
                            break;

                        default:
                            RunLog(("PS空盘下降流程.StepID =" + PS空盘下降流程.StepID.ToString() + "对应流程不存在"));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    string errormsg = $"PS空盘下降流程({PS空盘下降流程.StepID})运行异常,详细信息:" + ex.Message;
                    RunLog(errormsg, LogType.AlarmLog);
                    系统未定义错误ErrorMsg = errormsg;
                    ProgramPause = true;
                    IsPause = true;
                    Listcode.Add("1400070");
                }
            }
        }

        public void Empty下降回零流程()
        {
            string result = "";
            PS空盘下降回零流程.StepID = 500;
            while (true)
            {
                try
                {
                    Thread.Sleep(10);

                    if (APPmaster.IsForecePause)
                    {
                        PS空盘下降回零流程.State = 1;
                        continue;
                    }

                    if (APPmaster.IsPause && Empty舱门开启标记 == 0)
                    {
                        continue;
                    }

                    switch (PS空盘下降回零流程.StepID)
                    {
                        case 500:
                            PS空盘下降回零流程.StepName = "路径规划";
                            PS空盘下降回零流程.Condition = Empty下降回零标记 == 2;
                            PS空盘下降回零流程.Permit = !PS空盘下降回零流程.Processing && !PS空盘下降流程.Processing && !PS空盘上顶流程.Processing;
                            if (PS空盘下降回零流程.Condition && PS空盘下降回零流程.Permit)
                            {
                                PS空盘下降回零流程.Processing = true;
                                PS空盘下降回零流程.StepID = 505;
                            }
                            else Thread.Sleep(10);
                            break;

                        case 505:
                            PS空盘下降回零流程.StepName = "Empty顶盘轴移动到原点";
                            RunLog($"PS空盘下降回零流程.StepID={PS空盘下降回零流程.StepID}->{PS空盘下降回零流程.StepName}");
                            result = Empty顶盘轴移动(1, Empty舱门开启标记);
                            if (result == "Success")
                            {
                                PS空盘下降回零流程.StepID = 510;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Empty顶盘轴移动到原点失败！" + result);
                            }
                            break;

                        case 510:
                            PS空盘下降回零流程.StepName = "Empty顶盘轴回零";
                            RunLog($"PS空盘下降回零流程.StepID={PS空盘下降回零流程.StepID}->{PS空盘下降回零流程.StepName}");
                            result = Empty顶盘轴回零(Empty舱门开启标记);
                            if (result == "Success")
                            {
                                Empty下降回零标记 = 1;
                                PS空盘下降回零流程.StepID = 500;
                                PS空盘下降回零流程.Processing = false;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Empty顶盘轴回零失败！" + result);
                            }
                            break;

                        default:
                            RunLog(("PS空盘下降回零流程.StepID =" + PS空盘下降回零流程.StepID.ToString() + "对应流程不存在"));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    string errormsg = $"PS空盘下降回零流程({PS空盘下降回零流程.StepID})运行异常,详细信息:" + ex.Message;
                    RunLog(errormsg, LogType.AlarmLog);
                    系统未定义错误ErrorMsg = errormsg;
                    ProgramPause = true;
                    IsPause = true;
                    Listcode.Add("1400070");
                }
            }
        }

        public void Load顶盘流程()
        {
            string result = "";
            PS待测盘上顶流程.StepID = 500;
            while (true)
            {
                try
                {
                    Thread.Sleep(10);

                    if (APPmaster.IsForecePause)
                    {
                        PS待测盘上顶流程.State = 1;
                        continue;
                    }

                    if (APPmaster.IsPause && Load舱门开启标记 == 0)
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
                            RunLog($"PS待测盘上顶流程.StepID={PS待测盘上顶流程.StepID}->{PS待测盘上顶流程.StepName}");
                            result = Load_Tray前侧定位气缸原位输出();
                            if (result == "Success")
                            {
                                PS待测盘上顶流程.StepID = 510;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Load_Tray前侧定位气缸原位输出失败！" + result);
                            }
                            break;

                        case 510:
                            PS待测盘上顶流程.StepName = "Load向上移动";
                            RunLog($"PS待测盘上顶流程.StepID={PS待测盘上顶流程.StepID}->{PS待测盘上顶流程.StepName}");
                            result = Load向上移动(Load舱门开启标记);
                            if (result == "Success")
                            {
                                PS待测盘上顶流程.StepID = 515;
                            }
                            else
                            {
                                PS待测盘上顶流程.StepID = 600;
                                RunLog($"Load向上移动失败！" + result);
                            }
                            break;

                        case 515:
                            PS待测盘上顶流程.StepName = "Load向下移动";
                            RunLog($"PS待测盘上顶流程.StepID={PS待测盘上顶流程.StepID}->{PS待测盘上顶流程.StepName}");
                            result = Load向下移动(Load舱门开启标记);
                            if (result == "Success")
                            {
                                PS待测盘上顶流程.StepID = 520;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Load向下移动失败！" + result);
                            }
                            break;

                        case 520:
                            PS待测盘上顶流程.StepName = "Load向上移动固定距离";
                            RunLog($"PS待测盘上顶流程.StepID={PS待测盘上顶流程.StepID}->{PS待测盘上顶流程.StepName}");
                            result = Load顶盘轴移动2(Load原点偏移, Load舱门开启标记); //3175 1.5875
                            if (result == "Success")
                            {
                                PS待测盘上顶流程.StepID = 525;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Load向上移动固定距离失败！" + result);
                            }
                            break;

                        case 525:
                            PS待测盘上顶流程.StepName = "Load_Tray前侧定位气缸到位输出";
                            RunLog($"PS待测盘上顶流程.StepID={PS待测盘上顶流程.StepID}->{PS待测盘上顶流程.StepName}");
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
                                RunLog($"Load_Tray前侧定位气缸到位输出失败！" + result);
                            }
                            break;

                        case 599:
                            PS待测盘上顶流程.StepName = $"待测盘上顶流程完成";
                            RunLog($"PS待测盘上顶流程.StepID={PS待测盘上顶流程.StepID}->{PS待测盘上顶流程.StepName}");
                            PS待测盘上顶流程.Processing = false;
                            PS待测盘上顶流程.StepID = 500;
                            break;


                        case 600:
                            PS待测盘上顶流程.StepName = "Load顶盘轴移动到原点";
                            RunLog($"PS待测盘上顶流程.StepID={PS待测盘上顶流程.StepID}->{PS待测盘上顶流程.StepName}");
                            result = Load顶盘轴移动(1, Load舱门开启标记);
                            if (result == "Success")
                            {
                                PS待测盘上顶流程.StepID = 605;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Load顶盘轴移动到原点失败！" + result);
                            }
                            break;

                        case 605:
                            PS待测盘上顶流程.StepName = "Load顶盘轴回零";
                            RunLog($"PS待测盘上顶流程.StepID={PS待测盘上顶流程.StepID}->{PS待测盘上顶流程.StepName}");
                            result = Load顶盘轴回零(Load舱门开启标记);
                            if (result == "Success")
                            {
                                无待测盘结批 = false;
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add("4019003"); //添加报警弹窗
                                RunLog($"[Code]4019003,[Msg]待测区没有盘", LogType.AlarmLog);
                                motionEx.SetDo("Y004_重试灯", true);
                                motionEx.SetDo("Y008_结批灯", true);
                                PS待测盘上顶流程.StepID = 610;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Load顶盘轴回零失败！" + result);
                            }
                            break;

                        case 610:
                            PS待测盘上顶流程.StepName = "待测区没有盘";
                            if (无待测盘结批)
                            {
                                结批标记 = true;
                                Load上顶标记 = 1;
                                PS待测盘上顶流程.StepID = 599;
                            }
                            else
                            {
                                PS待测盘上顶流程.StepID = 505;
                            }
                            break;

                        default:
                            RunLog(("PS待测盘上顶流程.StepID =" + PS待测盘上顶流程.StepID.ToString() + "对应流程不存在"));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    string errormsg = $"PS待测盘上顶流程({PS待测盘上顶流程.StepID})运行异常,详细信息:" + ex.Message;
                    RunLog(errormsg, LogType.AlarmLog);
                    系统未定义错误ErrorMsg = errormsg;
                    ProgramPause = true;
                    IsPause = true;
                    Listcode.Add("1400070");
                }
            }
        }

        public void Load下降回零流程()
        {
            string result = "";
            PS待测盘下降回零流程.StepID = 500;
            while (true)
            {
                try
                {
                    Thread.Sleep(10);

                    if (APPmaster.IsForecePause)
                    {
                        PS待测盘下降回零流程.State = 1;
                        continue;
                    }

                    if (APPmaster.IsPause && Load舱门开启标记 == 0)
                    {
                        continue;
                    }

                    switch (PS待测盘下降回零流程.StepID)
                    {
                        case 500:
                            PS待测盘下降回零流程.StepName = "路径规划";
                            PS待测盘下降回零流程.Condition = Load下降回零标记 == 2;
                            PS待测盘下降回零流程.Permit = !PS待测盘下降回零流程.Processing && !PS待测盘上顶流程.Processing;
                            if (PS待测盘下降回零流程.Condition && PS待测盘下降回零流程.Permit)
                            {
                                PS待测盘下降回零流程.Processing = true;
                                PS待测盘下降回零流程.StepID = 505;
                            }
                            else Thread.Sleep(10);
                            break;

                        case 505:
                            PS待测盘下降回零流程.StepName = "Load_Tray前侧定位气缸原位输出";
                            RunLog($"PS待测盘上顶回零流程.StepID={PS待测盘下降回零流程.StepID}->{PS待测盘下降回零流程.StepName}");
                            result = Load_Tray前侧定位气缸原位输出();
                            if (result == "Success")
                            {
                                PS待测盘下降回零流程.StepID = 510;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Load_Tray前侧定位气缸原位输出失败！" + result);
                            }
                            break;

                        case 510:
                            PS待测盘下降回零流程.StepName = "Load顶盘轴移动到原点";
                            RunLog($"PS待测盘上顶回零流程.StepID={PS待测盘下降回零流程.StepID}->{PS待测盘下降回零流程.StepName}");
                            result = Load顶盘轴移动(1, Load舱门开启标记);
                            if (result == "Success")
                            {
                                PS待测盘下降回零流程.StepID = 515;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Load顶盘轴移动到原点失败！" + result);
                            }
                            break;

                        case 515:
                            PS待测盘下降回零流程.StepName = "Load顶盘轴回零";
                            RunLog($"PS待测盘上顶回零流程.StepID={PS待测盘下降回零流程.StepID}->{PS待测盘下降回零流程.StepName}");
                            result = Load顶盘轴回零(Load舱门开启标记);
                            if (result == "Success")
                            {
                                Load下降回零标记 = 1;
                                PS待测盘下降回零流程.StepID = 500;
                                PS待测盘下降回零流程.Processing = false;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Load顶盘轴回零失败！" + result);
                            }
                            break;

                        default:
                            RunLog(("PS待测盘上顶回零流程.StepID =" + PS待测盘下降回零流程.StepID.ToString() + "对应流程不存在"));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    string errormsg = $"PS待测盘下降回零流程({PS待测盘下降回零流程.StepID})运行异常,详细信息:" + ex.Message;
                    RunLog(errormsg, LogType.AlarmLog);
                    系统未定义错误ErrorMsg = errormsg;
                    ProgramPause = true;
                    IsPause = true;
                    Listcode.Add("1400070");
                }
            }
        }

        public void Auto1顶盘流程() // 动态上顶
        {
            string result = "";
            double movepos = 0;
            PS手动盘1上顶流程.StepID = 500;
            while (true)
            {
                try
                {

                    Thread.Sleep(10);

                    if (APPmaster.IsForecePause)
                    {
                        PS手动盘1上顶流程.State = 1;
                        continue;
                    }

                    if (APPmaster.IsPause && Auto1舱门开启标记 == 0)
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
                            else
                            {
                                bool isenable = 设置信息.测试机信息.放料时允许侧敲 || !出料正在Auto1放料;
                                if (Tray盘模式 && !Auto1换盘标记 && Auto1上顶标记 == 1 && 设置信息.测试机信息.Auto侧敲使能 && isenable)
                                {
                                    if (Auto1数量 - 保存Auto1放料个数 >= 设置信息.测试机信息.放置多少颗敲击)
                                    {
                                        bool isIC1 = 获取Auto1盘坐标(false, out int auto1xPoint, out int auto1yPoint);
                                        if (!isIC1 || auto1xPoint != 0 || auto1yPoint != 0) //如果有放料才需要侧敲，加!isIC1是因为满盘时返回False,auto1xy返回是0，所以满盘的时候也要侧敲，和满盘会放空盘一样，如果不加则不会放空盘自然也不会触发侧敲
                                        {
                                            Auto1正在侧敲 = true;
                                            保存Auto1放料个数 = Auto1数量;
                                            PS手动盘1上顶流程.StepID = 605;
                                        }
                                    }
                                }
                                Thread.Sleep(10);
                            }
                            break;

                        case 505:
                            PS手动盘1上顶流程.StepName = "Auto1_Tray前侧定位气缸原位输出";
                            RunLog($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
                            result = Auto1_Tray前侧定位气缸原位输出();
                            if (result == "Success")
                            {
                                PS手动盘1上顶流程.StepID = 510;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Auto1_Tray前侧定位气缸原位输出失败!" + result);
                            }
                            break;

                        case 510:
                            PS手动盘1上顶流程.StepName = "Auto1向上移动";
                            RunLog($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
                            result = Auto1向上移动(Auto1舱门开启标记);
                            if (result == "Success")
                            {
                                PS手动盘1上顶流程.StepID = 514;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Auto1向上移动失败!" + result);
                            }
                            break;

                        case 514:
                            PS手动盘1上顶流程.StepName = "获取Auto1上移绝对位置";
                            result = motionEx.GetCmdEncPos("M14_Auto1顶盘轴", out double prfpos1);
                            if (result == "Success")
                            {
                                result = GetAxisRunInfo("M14_Auto1顶盘轴", out AxisRunInfo axisRunInfo);
                                if (result == "Success")
                                {
                                    movepos = Convert.ToDouble(prfpos1 * axisRunInfo.步长mm) + 初始点位.Auto1上顶固定高度; //6.35  Auto1上移固定距离
                                    PS手动盘1上顶流程.StepID = 515;
                                }
                                else
                                {
                                    APPmaster.ProgramPause = true;
                                    IsPause = true;
                                    RunLog("M14_Auto1顶盘轴信息获取失败!" + result);
                                }
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog("M14_Auto1顶盘轴实时位置失败!" + result);
                            }
                            break;

                        case 515:
                            PS手动盘1上顶流程.StepName = $"Auto1向上移动绝对位置{movepos}mm";
                            RunLog($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
                            result = Auto1顶盘轴移动(movepos, Auto1舱门开启标记);  //result = motionEx.AxisRelativeMove("M14_Auto1顶盘轴", 21);//3175 1.5875
                            if (result == "Success")
                            {
                                PS手动盘1上顶流程.StepID = 520;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Auto1向上移动固定距离失败!" + result);
                            }
                            break;

                        case 520:
                            PS手动盘1上顶流程.StepName = "Auto1_Tray上顶气缸到位输出";
                            RunLog($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
                            result = Auto1_Tray上顶气缸到位输出();//3175 1.5875
                            if (result == "Success")
                            {
                                PS手动盘1上顶流程.StepID = 525;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Auto1_Tray上顶气缸到位输出失败!" + result);
                            }
                            break;

                        case 525:
                            PS手动盘1上顶流程.StepName = "Auto1向下移动";
                            RunLog($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
                            result = Auto1向下移动(Auto1舱门开启标记);
                            if (result == "Success")
                            {
                                PS手动盘1上顶流程.StepID = 526;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Auto1向下移动失败!" + result);
                            }
                            break;

                        case 526:
                            PS手动盘1上顶流程.StepName = "获取Auto1下移绝对位置";
                            RunLog($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
                            result = motionEx.GetCmdEncPos("M14_Auto1顶盘轴", out double prfpos2);
                            if (result == "Success")
                            {
                                result = GetAxisRunInfo("M14_Auto1顶盘轴", out AxisRunInfo axisRunInfo);
                                if (result == "Success")
                                {
                                    movepos = Convert.ToDouble(prfpos2 * axisRunInfo.步长mm) - 初始点位.Auto1下降固定高度; //6.35  Auto1上移固定距离 - Tray_厚度
                                    PS手动盘1上顶流程.StepID = 530;
                                }
                                else
                                {
                                    APPmaster.ProgramPause = true;
                                    IsPause = true;
                                    RunLog("M14_Auto1顶盘轴信息获取失败!" + result);
                                }
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog("M14_Auto1顶盘轴实时位置失败!" + result);
                            }
                            break;

                        case 530:
                            PS手动盘1上顶流程.StepName = $"Auto1向下移动绝对位置{movepos}mm";
                            RunLog($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
                            result = Auto1顶盘轴移动(movepos, Auto1舱门开启标记); //result = motionEx.AxisRelativeMove("M14_Auto1顶盘轴", -15);//3175 1.5875
                            if (result == "Success")
                            {
                                PS手动盘1上顶流程.StepID = 535;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Auto1向下移动固定距离失败!" + result);
                            }
                            break;

                        case 535:
                            PS手动盘1上顶流程.StepName = "Auto1_Tray上顶气缸原位输出";
                            RunLog($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
                            result = Auto1_Tray上顶气缸原位输出();
                            if (result == "Success")
                            {
                                PS手动盘1上顶流程.StepID = 540;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Auto1_Tray上顶气缸原位输出失败!" + result);
                            }
                            break;

                        case 540:
                            PS手动盘1上顶流程.StepName = "判断Auto1料仓是否满盘";
                            RunLog($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
                            result = motionEx.GetCmdEncPos("M14_Auto1顶盘轴", out double prfpos);
                            string result3 = GetAxisRunInfo("M14_Auto1顶盘轴", out AxisRunInfo axisRunInfo2);
                            if (result == "Success" && result3 == "Success")
                            {
                                if (!正在结批)
                                {
                                    Auto1Tray盘数量++;
                                    double minpoint = 初始点位.Auto1_满盘位 / axisRunInfo2.步长mm;
                                    if (prfpos < minpoint || Auto1Tray盘数量 >= 设置信息.Auto仓提示换盘数量) //Auto1满盘判断值  && !正在结批
                                    {
                                        Auto1料仓满盘标记 = true;
                                        if (prfpos < minpoint) RunLog($"M14_Auto1顶盘轴当前行程{prfpos}<Auto1满盘判断值{minpoint}"); //Auto1满盘判断值
                                        if (Auto1Tray盘数量 >= 设置信息.Auto仓提示换盘数量) RunLog($"Auto1Tray盘数量{Auto1Tray盘数量}>=Auto仓提示换盘数量{设置信息.Auto仓提示换盘数量}"); //Auto1满盘判断值
                                        Auto1Tray盘数量 = 0;
                                    }
                                    else Auto1料仓满盘标记 = false;
                                }
                                Auto1上顶标记 = 1;
                                PS手动盘1上顶流程.StepID = 599;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                if (result != "Success") RunLog($"读取Auto1顶盘轴编码位置失败！" + result);
                                if (result3 != "Success") RunLog($"读取Auto1顶盘轴运行信息失败！" + result3);
                            }
                            break;

                        case 599:
                            PS手动盘1上顶流程.StepName = $"手动盘1上顶流程完成";
                            RunLog($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
                            PS手动盘1上顶流程.Processing = false;
                            PS手动盘1上顶流程.StepID = 500;
                            break;

                        case 605:
                            PS手动盘1上顶流程.StepName = $"手动盘1执行侧敲";
                            RunLog($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
                            bool qenable = 设置信息.测试机信息.前侧同时敲击;
                            int count = 设置信息.测试机信息.Auto侧敲次数;
                            int delay = 设置信息.测试机信息.侧敲间隔;
                            for (int i = 0; i < count; i++)
                            {
                                if (qenable) //前侧同时敲击
                                {
                                    if (i > 0) Thread.Sleep(delay);
                                    Auto1_Tray前侧定位气缸原位输出(10);
                                    Thread.Sleep(delay);
                                    Auto1_Tray前侧定位气缸到位输出(10);
                                }
                                else
                                {
                                    if (i > 0) Thread.Sleep(delay);
                                    Auto1_Tray侧定位气缸原位输出(10);
                                    Thread.Sleep(delay);
                                    Auto1_Tray侧定位气缸到位输出(10);
                                }
                            }
                            Auto1正在侧敲 = false;
                            PS手动盘1上顶流程.StepID = 500;
                            break;

                        default:
                            RunLog(("PS手动盘1上顶流程.StepID =" + PS手动盘1上顶流程.StepID.ToString() + "对应流程不存在"));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    string errormsg = $"PS手动盘1上顶流程({PS手动盘1上顶流程.StepID})运行异常,详细信息:" + ex.Message;
                    RunLog(errormsg, LogType.AlarmLog);
                    系统未定义错误ErrorMsg = errormsg;
                    ProgramPause = true;
                    IsPause = true;
                    Listcode.Add("1400070");
                }
            }
        }

        public void Auto1顶盘流程2() //固定上顶
        {
            string result = "";
            double movepos = 0;

            PS手动盘1上顶流程.StepID = 500;
            while (true)
            {
                try
                {
                    Thread.Sleep(10);

                    if (APPmaster.IsForecePause)
                    {
                        PS手动盘1上顶流程.State = 1;
                        continue;
                    }

                    if (APPmaster.IsPause && Auto1舱门开启标记 == 0)
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
                            RunLog($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
                            result = Auto1_Tray前侧定位气缸原位输出();
                            if (result == "Success")
                            {
                                PS手动盘1上顶流程.StepID = 506;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Auto1_Tray前侧定位气缸原位输出失败!" + result);
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
                                    RunLog("M14_Auto1顶盘轴信息获取失败!" + result);
                                }
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog("M14_Auto1顶盘轴实时位置失败!" + result);
                            }
                            break;

                        case 510:
                            PS手动盘1上顶流程.StepName = $"Auto1向上移动绝对位置{movepos}mm";
                            RunLog($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
                            result = Auto1顶盘轴移动(movepos, Auto1舱门开启标记);
                            if (result == "Success")
                            {
                                PS手动盘1上顶流程.StepID = 515;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Auto1向上移动失败!" + result);
                            }
                            break;

                        case 515:
                            PS手动盘1上顶流程.StepName = "Auto1_Tray上顶气缸到位输出";
                            RunLog($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
                            result = Auto1_Tray上顶气缸到位输出();//3175 1.5875
                            if (result == "Success")
                            {
                                PS手动盘1上顶流程.StepID = 520;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Auto1_Tray上顶气缸到位输出失败!" + result);
                            }
                            break;

                        case 520:
                            PS手动盘1上顶流程.StepName = "获取Auto1下移绝对位置";
                            RunLog($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
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
                                    RunLog("M14_Auto1顶盘轴信息获取失败!" + result);
                                }
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog("M14_Auto1顶盘轴实时位置失败!" + result);
                            }
                            break;

                        case 525:
                            PS手动盘1上顶流程.StepName = $"Auto1向下移动绝对位置{movepos}mm";
                            RunLog($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
                            result = Auto1顶盘轴移动(movepos, Auto1舱门开启标记);
                            if (result == "Success")
                            {
                                PS手动盘1上顶流程.StepID = 530;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Auto1向下移动失败!" + result);
                            }
                            break;

                        case 530:
                            PS手动盘1上顶流程.StepName = "Auto1_Tray上顶气缸原位输出";
                            RunLog($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
                            result = Auto1_Tray上顶气缸原位输出();
                            if (result == "Success")
                            {
                                PS手动盘1上顶流程.StepID = 535;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Auto1_Tray上顶气缸原位输出失败!" + result);
                            }
                            break;

                        case 535:
                            PS手动盘1上顶流程.StepName = "判断Auto1料仓是否满盘";
                            RunLog($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
                            result = motionEx.GetCmdEncPos("M14_Auto1顶盘轴", out double prfpos);
                            string result3 = GetAxisRunInfo("M14_Auto1顶盘轴", out AxisRunInfo axisRunInfo2);
                            if (result == "Success" && result3 == "Success")
                            {
                                double minpoint = 初始点位.Auto1_满盘位 / axisRunInfo2.步长mm;
                                if (prfpos < minpoint) //Auto1满盘判断值
                                {
                                    Auto1料仓满盘标记 = true;
                                    RunLog($"M14_Auto1顶盘轴当前行程{prfpos}<Auto1满盘判断值{minpoint}");
                                }
                                else Auto1料仓满盘标记 = false;

                                Auto1上顶标记 = 1;
                                PS手动盘1上顶流程.StepID = 599;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                if (result != "Success") RunLog($"读取Auto1顶盘轴编码位置失败！" + result);
                                if (result3 != "Success") RunLog($"读取Auto1顶盘轴运行信息失败！" + result3);
                            }
                            break;

                        case 599:
                            PS手动盘1上顶流程.StepName = $"手动盘1上顶流程完成";
                            RunLog($"PS手动盘1上顶流程.StepID={PS手动盘1上顶流程.StepID}->{PS手动盘1上顶流程.StepName}");
                            PS手动盘1上顶流程.Processing = false;
                            PS手动盘1上顶流程.StepID = 500;
                            break;

                        default:
                            RunLog(("PS手动盘1上顶流程.StepID =" + PS手动盘1上顶流程.StepID.ToString() + "对应流程不存在"));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    string errormsg = $"PS手动盘1上顶流程({PS手动盘1上顶流程.StepID})运行异常,详细信息:" + ex.Message;
                    RunLog(errormsg, LogType.AlarmLog);
                    系统未定义错误ErrorMsg = errormsg;
                    ProgramPause = true;
                    IsPause = true;
                    Listcode.Add("1400070");
                }
            }
        }

        public void Auto1下降回零流程()
        {
            string result = "";
            PS手动盘1下降回零流程.StepID = 500;
            while (true)
            {
                try
                {
                    Thread.Sleep(10);

                    if (APPmaster.IsForecePause)
                    {
                        PS手动盘1下降回零流程.State = 1;
                        continue;
                    }

                    if (APPmaster.IsPause && Auto1舱门开启标记 == 0)
                    {
                        continue;
                    }

                    switch (PS手动盘1下降回零流程.StepID)
                    {
                        case 500:
                            PS手动盘1下降回零流程.StepName = "路径规划";
                            PS手动盘1下降回零流程.Condition = Auto1下降回零标记 == 2;
                            PS手动盘1下降回零流程.Permit = !PS手动盘1下降回零流程.Processing && !PS手动盘1上顶流程.Processing;
                            if (PS手动盘1下降回零流程.Condition && PS手动盘1下降回零流程.Permit)
                            {
                                PS手动盘1下降回零流程.Processing = true;
                                PS手动盘1下降回零流程.StepID = 505;
                            }
                            else Thread.Sleep(10);
                            break;

                        case 505:
                            PS手动盘1下降回零流程.StepName = "Auto1顶盘轴移动到原点";
                            RunLog($"PS手动盘1下降回零流程.StepID={PS手动盘1下降回零流程.StepID}->{PS手动盘1下降回零流程.StepName}");
                            result = Auto1顶盘轴移动(1, Auto1舱门开启标记);
                            if (result == "Success")
                            {
                                PS手动盘1下降回零流程.StepID = 510;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Auto1顶盘轴移动到原点失败！" + result);
                            }
                            break;

                        case 510:
                            PS手动盘1下降回零流程.StepName = "Auto1顶盘轴回零";
                            RunLog($"PS手动盘1下降回零流程.StepID={PS手动盘1下降回零流程.StepID}->{PS手动盘1下降回零流程.StepName}");
                            result = Auto1顶盘轴回零(Auto1舱门开启标记);
                            if (result == "Success")
                            {
                                Auto1下降回零标记 = 1;
                                PS手动盘1下降回零流程.StepID = 500;
                                PS手动盘1下降回零流程.Processing = false;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Auto1顶盘轴回零失败！" + result);
                            }
                            break;

                        default:
                            RunLog(("PS手动盘1下降回零流程.StepID =" + PS手动盘1下降回零流程.StepID.ToString() + "对应流程不存在"));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    string errormsg = $"PS手动盘1下降回零流程({PS手动盘1下降回零流程.StepID})运行异常,详细信息:" + ex.Message;
                    RunLog(errormsg, LogType.AlarmLog);
                    系统未定义错误ErrorMsg = errormsg;
                    ProgramPause = true;
                    IsPause = true;
                    Listcode.Add("1400070");
                }
            }
        }

        public void Auto2顶盘流程()// 动态上顶
        {
            string result = "";
            double movepos = 0;
            PS手动盘2上顶流程.StepID = 500;
            while (true)
            {
                try
                {
                    Thread.Sleep(10);

                    if (APPmaster.IsForecePause)
                    {
                        PS手动盘2上顶流程.State = 1;
                        continue;
                    }

                    if (APPmaster.IsPause && Auto2舱门开启标记 == 0)
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
                            else
                            {
                                bool isenable = 设置信息.测试机信息.放料时允许侧敲 || !出料正在Auto2放料;
                                if (Tray盘模式 && !Auto2换盘标记 && Auto2上顶标记 == 1 && 设置信息.测试机信息.Auto侧敲使能 && isenable)
                                {
                                    if (Auto2数量 - 保存Auto2放料个数 >= 设置信息.测试机信息.放置多少颗敲击)
                                    {
                                        bool isIC1 = 获取Auto2盘坐标(false, out int auto2xPoint, out int auto2yPoint);
                                        if (!isIC1 || auto2xPoint != 0 || auto2yPoint != 0) //如果有放料才需要侧敲，加!isIC1是因为满盘时返回False,auto2xy返回是0
                                        {
                                            Auto2正在侧敲 = true;
                                            保存Auto2放料个数 = Auto2数量;
                                            PS手动盘2上顶流程.StepID = 605;
                                        }
                                    }
                                }
                                Thread.Sleep(10);
                            }
                            break;

                        case 505:
                            PS手动盘2上顶流程.StepName = "Auto2_Tray前侧定位气缸原位输出";
                            RunLog($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
                            result = Auto2_Tray前侧定位气缸原位输出();
                            if (result == "Success")
                            {
                                PS手动盘2上顶流程.StepID = 510;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Auto2_Tray前侧定位气缸原位输出失败!" + result);
                            }
                            break;

                        case 510:
                            PS手动盘2上顶流程.StepName = "Auto2向上移动";
                            RunLog($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
                            result = Auto2向上移动(Auto2舱门开启标记);
                            if (result == "Success")
                            {
                                PS手动盘2上顶流程.StepID = 511;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Auto2向上移动失败!" + result);
                            }
                            break;

                        case 511:
                            PS手动盘2上顶流程.StepName = "获取Auto2上移绝对位置";
                            result = motionEx.GetCmdEncPos("M15_Auto2顶盘轴", out double prfpos1);
                            if (result == "Success")
                            {
                                result = GetAxisRunInfo("M15_Auto2顶盘轴", out AxisRunInfo axisRunInfo);
                                if (result == "Success")
                                {
                                    movepos = Convert.ToDouble(prfpos1 * axisRunInfo.步长mm) + 初始点位.Auto2上顶固定高度; //6.35 Auto2上移固定距离
                                    PS手动盘2上顶流程.StepID = 515;
                                }
                                else
                                {
                                    APPmaster.ProgramPause = true;
                                    IsPause = true;
                                    RunLog("M15_Auto2顶盘轴信息获取失败!" + result);
                                }
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog("M15_Auto2顶盘轴实时位置失败!" + result);
                            }
                            break;

                        case 515:
                            PS手动盘2上顶流程.StepName = $"Auto2向上移动绝对位置{movepos}mm";
                            RunLog($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
                            result = Auto2顶盘轴移动(movepos, Auto2舱门开启标记); //result = motionEx.AxisRelativeMove("M15_Auto2顶盘轴", 21);//3175 1.5875
                            if (result == "Success")
                            {
                                PS手动盘2上顶流程.StepID = 520;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Auto2向上移动固定距离失败!" + result);
                            }
                            break;

                        case 520:
                            PS手动盘2上顶流程.StepName = "Auto2_Tray上顶气缸到位输出";
                            RunLog($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
                            result = Auto2_Tray上顶气缸到位输出();//3175 1.5875
                            if (result == "Success")
                            {
                                PS手动盘2上顶流程.StepID = 525;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Auto2_Tray上顶气缸到位输出失败!" + result);
                            }
                            break;

                        case 525:
                            PS手动盘2上顶流程.StepName = "Auto2向下移动";
                            RunLog($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
                            result = Auto2向下移动(Auto2舱门开启标记);
                            if (result == "Success")
                            {
                                PS手动盘2上顶流程.StepID = 526;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Auto2向下移动失败!" + result);
                            }
                            break;

                        case 526:
                            PS手动盘2上顶流程.StepName = "获取Auto2下移绝对位置";
                            RunLog($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
                            result = motionEx.GetCmdEncPos("M15_Auto2顶盘轴", out double prfpos2);
                            if (result == "Success")
                            {
                                result = GetAxisRunInfo("M15_Auto2顶盘轴", out AxisRunInfo axisRunInfo);
                                if (result == "Success")
                                {
                                    movepos = Convert.ToDouble(prfpos2 * axisRunInfo.步长mm) - 初始点位.Auto2下降固定高度; //15 6.35 Auto2上移固定距离 - Tray_厚度
                                    PS手动盘2上顶流程.StepID = 530;
                                }
                                else
                                {
                                    APPmaster.ProgramPause = true;
                                    IsPause = true;
                                    RunLog("M15_Auto2顶盘轴信息获取失败!" + result);
                                }
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog("M15_Auto2顶盘轴实时位置失败!" + result);
                            }
                            break;

                        case 530:
                            PS手动盘2上顶流程.StepName = $"Auto2向下移动绝对位置{movepos}mm";
                            RunLog($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
                            result = Auto2顶盘轴移动(movepos, Auto2舱门开启标记);  //result = motionEx.AxisRelativeMove("M15_Auto2顶盘轴", -15);//3175 1.5875
                            if (result == "Success")
                            {
                                PS手动盘2上顶流程.StepID = 535;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Auto2向下移动固定距离失败!" + result);
                            }
                            break;

                        case 535:
                            PS手动盘2上顶流程.StepName = "Auto2_Tray上顶气缸原位输出";
                            RunLog($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
                            result = Auto2_Tray上顶气缸原位输出();
                            if (result == "Success")
                            {
                                PS手动盘2上顶流程.StepID = 540;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Auto2_Tray上顶气缸原位输出失败!" + result);
                            }
                            break;


                        case 540:
                            PS手动盘2上顶流程.StepName = "判断Auto2料仓是否满盘";
                            RunLog($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
                            result = motionEx.GetCmdEncPos("M15_Auto2顶盘轴", out double prfpos);
                            string result3 = GetAxisRunInfo("M15_Auto2顶盘轴", out AxisRunInfo axisRunInfo2);
                            if (result == "Success" && result3 == "Success")
                            {
                                if (!正在结批)
                                {
                                    Auto2Tray盘数量++;
                                    double minpoint = 初始点位.Auto2_满盘位 / axisRunInfo2.步长mm;
                                    if (prfpos < minpoint || Auto2Tray盘数量 >= 设置信息.Auto仓提示换盘数量)// && !正在结批 Auto2满盘判断值
                                    {
                                        Auto2料仓满盘标记 = true;
                                        if (prfpos < minpoint) RunLog($"M15_Auto2顶盘轴当前行程{prfpos}<Auto2满盘判断值{minpoint}"); //Auto2满盘判断值
                                        if (Auto2Tray盘数量 >= 设置信息.Auto仓提示换盘数量) RunLog($"Auto2Tray盘数量{Auto2Tray盘数量}>=Auto仓提示换盘数量{设置信息.Auto仓提示换盘数量}"); //Auto2满盘判断值
                                        Auto2Tray盘数量 = 0;
                                    }
                                    else Auto2料仓满盘标记 = false;
                                }
                                Auto2上顶标记 = 1;
                                PS手动盘2上顶流程.StepID = 599;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                if (result != "Success") RunLog($"读取Auto2顶盘轴编码位置失败！" + result);
                                if (result3 != "Success") RunLog($"读取Auto2顶盘轴运行信息失败！" + result3);
                            }
                            break;


                        case 599:
                            PS手动盘2上顶流程.StepName = $"手动盘2上顶流程完成";
                            RunLog($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
                            PS手动盘2上顶流程.Processing = false;
                            PS手动盘2上顶流程.StepID = 500;
                            break;

                        case 605:
                            PS手动盘2上顶流程.StepName = $"手动盘2执行侧敲";
                            RunLog($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
                            bool qenable = 设置信息.测试机信息.前侧同时敲击;
                            int count = 设置信息.测试机信息.Auto侧敲次数;
                            int delay = 设置信息.测试机信息.侧敲间隔;
                            for (int i = 0; i < count; i++)
                            {
                                if (qenable) //前侧同时敲击
                                {
                                    if (i > 0) Thread.Sleep(delay);
                                    Auto2_Tray前侧定位气缸原位输出(10);
                                    Thread.Sleep(delay);
                                    Auto2_Tray前侧定位气缸到位输出(10);
                                }
                                else
                                {
                                    if (i > 0) Thread.Sleep(delay);
                                    Auto2_Tray侧定位气缸原位输出(10);
                                    Thread.Sleep(delay);
                                    Auto2_Tray侧定位气缸到位输出(10);
                                }
                            }
                            Auto2正在侧敲 = false;
                            PS手动盘2上顶流程.StepID = 500;
                            break;

                        default:
                            RunLog(("PS手动盘2上顶流程.StepID =" + PS手动盘2上顶流程.StepID.ToString() + "对应流程不存在"));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    string errormsg = $"PS手动盘2上顶流程({PS手动盘2上顶流程.StepID})运行异常,详细信息:" + ex.Message;
                    RunLog(errormsg, LogType.AlarmLog);
                    系统未定义错误ErrorMsg = errormsg;
                    ProgramPause = true;
                    IsPause = true;
                    Listcode.Add("1400070");
                }
            }
        }

        public void Auto2顶盘流程2() //固定上顶
        {
            string result = "";
            double movepos = 0;
            PS手动盘2上顶流程.StepID = 500;
            while (true)
            {
                try
                {
                    Thread.Sleep(10);

                    if (APPmaster.IsForecePause)
                    {
                        PS手动盘2上顶流程.State = 1;
                        continue;
                    }

                    if (APPmaster.IsPause && Auto2舱门开启标记 == 0)
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
                            RunLog($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
                            result = Auto2_Tray前侧定位气缸原位输出();
                            if (result == "Success")
                            {
                                PS手动盘2上顶流程.StepID = 506;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Auto2_Tray前侧定位气缸原位输出失败!" + result);
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
                                    RunLog("M15_Auto2顶盘轴信息获取失败!" + result);
                                }
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog("M15_Auto2顶盘轴实时位置失败!" + result);
                            }
                            break;

                        case 510:
                            PS手动盘2上顶流程.StepName = $"Auto2向上移动绝对位置{movepos}mm";
                            RunLog($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
                            result = Auto2顶盘轴移动(movepos, Auto2舱门开启标记);
                            if (result == "Success")
                            {
                                PS手动盘2上顶流程.StepID = 515;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Auto2向上移动失败!" + result);
                            }
                            break;

                        case 515:
                            PS手动盘2上顶流程.StepName = "Auto2_Tray上顶气缸到位输出";
                            RunLog($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
                            result = Auto2_Tray上顶气缸到位输出();//3175 1.5875
                            if (result == "Success")
                            {
                                PS手动盘2上顶流程.StepID = 520;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Auto2_Tray上顶气缸到位输出失败!" + result);
                            }
                            break;

                        case 520:
                            PS手动盘2上顶流程.StepName = "获取Auto2下移绝对位置";
                            RunLog($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
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
                                    RunLog("M15_Auto2顶盘轴信息获取失败!" + result);
                                }
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog("M15_Auto2顶盘轴实时位置失败!" + result);
                            }
                            break;

                        case 525:
                            PS手动盘2上顶流程.StepName = $"Auto2向下移动绝对位置{movepos}mm";
                            RunLog($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
                            result = Auto2顶盘轴移动(movepos, Auto2舱门开启标记);
                            if (result == "Success")
                            {
                                PS手动盘2上顶流程.StepID = 530;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Auto2向下移动失败!" + result);
                            }
                            break;

                        case 530:
                            PS手动盘2上顶流程.StepName = "Auto2_Tray上顶气缸原位输出";
                            RunLog($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
                            result = Auto2_Tray上顶气缸原位输出();
                            if (result == "Success")
                            {
                                PS手动盘2上顶流程.StepID = 535;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Auto2_Tray上顶气缸原位输出失败!" + result);
                            }
                            break;

                        case 535:
                            PS手动盘2上顶流程.StepName = "判断Auto2料仓是否满盘";
                            RunLog($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
                            result = motionEx.GetCmdEncPos("M15_Auto2顶盘轴", out double prfpos);
                            string result3 = GetAxisRunInfo("M15_Auto2顶盘轴", out AxisRunInfo axisRunInfo2);
                            if (result == "Success" && result3 == "Success")
                            {
                                double minpoint = 初始点位.Auto2_满盘位 / axisRunInfo2.步长mm;
                                if (prfpos < minpoint) //Auto2满盘判断值
                                {
                                    Auto2料仓满盘标记 = true;
                                    RunLog($"M15_Auto2顶盘轴当前行程{prfpos}<Auto2满盘判断值{Auto2满盘判断值}");
                                }
                                else Auto2料仓满盘标记 = false;

                                Auto2上顶标记 = 1;
                                PS手动盘2上顶流程.StepID = 599;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                if (result != "Success") RunLog($"读取Auto2顶盘轴编码位置失败！" + result);
                                if (result3 != "Success") RunLog($"读取Auto2顶盘轴运行信息失败！" + result3);
                            }
                            break;


                        case 599:
                            PS手动盘2上顶流程.StepName = $"手动盘2上顶流程完成";
                            RunLog($"PS手动盘2上顶流程.StepID={PS手动盘2上顶流程.StepID}->{PS手动盘2上顶流程.StepName}");
                            PS手动盘2上顶流程.Processing = false;
                            PS手动盘2上顶流程.StepID = 500;
                            break;


                        default:
                            RunLog(("PS手动盘2上顶流程.StepID =" + PS手动盘2上顶流程.StepID.ToString() + "对应流程不存在"));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    string errormsg = $"PS手动盘2上顶流程({PS手动盘2上顶流程.StepID})运行异常,详细信息:" + ex.Message;
                    RunLog(errormsg, LogType.AlarmLog);
                    系统未定义错误ErrorMsg = errormsg;
                    ProgramPause = true;
                    IsPause = true;
                    Listcode.Add("1400070");
                }
            }
        }

        public void Auto2下降回零流程()
        {
            string result = "";
            PS手动盘2下降回零流程.StepID = 500;
            while (true)
            {
                try
                {
                    Thread.Sleep(10);

                    if (APPmaster.IsForecePause)
                    {
                        PS手动盘2下降回零流程.State = 1;
                        continue;
                    }

                    if (APPmaster.IsPause && Auto2舱门开启标记 == 0)
                    {
                        continue;
                    }

                    switch (PS手动盘2下降回零流程.StepID)
                    {
                        case 500:
                            PS手动盘2下降回零流程.StepName = "路径规划";
                            PS手动盘2下降回零流程.Condition = Auto2下降回零标记 == 2;
                            PS手动盘2下降回零流程.Permit = !PS手动盘2下降回零流程.Processing && !PS手动盘2上顶流程.Processing;
                            if (PS手动盘2下降回零流程.Condition && PS手动盘2下降回零流程.Permit)
                            {
                                PS手动盘2下降回零流程.Processing = true;
                                PS手动盘2下降回零流程.StepID = 505;
                            }
                            else Thread.Sleep(10);
                            break;

                        case 505:
                            PS手动盘2下降回零流程.StepName = "Auto2顶盘轴移动到原点";
                            RunLog($"PS手动盘2下降回零流程.StepID={PS手动盘2下降回零流程.StepID}->{PS手动盘2下降回零流程.StepName}");
                            result = Auto2顶盘轴移动(1, Auto2舱门开启标记);
                            if (result == "Success")
                            {
                                PS手动盘2下降回零流程.StepID = 510;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Auto2顶盘轴移动到原点失败！" + result);
                            }
                            break;

                        case 510:
                            PS手动盘2下降回零流程.StepName = "Auto2顶盘轴回零";
                            RunLog($"PS手动盘2下降回零流程.StepID={PS手动盘2下降回零流程.StepID}->{PS手动盘2下降回零流程.StepName}");
                            result = Auto2顶盘轴回零(Auto2舱门开启标记);
                            if (result == "Success")
                            {
                                Auto2下降回零标记 = 1;
                                PS手动盘2下降回零流程.StepID = 500;
                                PS手动盘2下降回零流程.Processing = false;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Auto2顶盘轴回零失败！" + result);
                            }
                            break;

                        default:
                            RunLog(("PS手动盘2下降回零流程.StepID =" + PS手动盘2下降回零流程.StepID.ToString() + "对应流程不存在"));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    string errormsg = $"PS手动盘2下降回零流程({PS手动盘2下降回零流程.StepID})运行异常,详细信息:" + ex.Message;
                    RunLog(errormsg, LogType.AlarmLog);
                    系统未定义错误ErrorMsg = errormsg;
                    ProgramPause = true;
                    IsPause = true;
                    Listcode.Add("1400070");
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
                try
                {
                    Thread.Sleep(10);

                    if (APPmaster.IsForecePause)
                    {
                        PS抓盘手臂取Empty空盘流程.State = 1;
                        continue;
                    }

                    if (APPmaster.IsPause)
                    {
                        continue;
                    }

                    switch (PS抓盘手臂取Empty空盘流程.StepID)
                    {
                        case 500:
                            PS抓盘手臂取Empty空盘流程.StepName = "路径规划";
                            PS抓盘手臂取Empty空盘流程.Condition = 抓盘手臂取Empty空盘标记 == 2
                                                    && Empty下降标记 == 1
                                                    && Empty上顶标记 == 1
                                                    && 抓盘手臂放空盘到Empty标记 == 1
                                                    && 抓盘手臂取Load空盘标记 == 1;

                            PS抓盘手臂取Empty空盘流程.Permit = !PS抓盘手臂取Empty空盘流程.Processing
                                                    && !PS空盘下降流程.Processing
                                                    && !PS空盘上顶流程.Processing
                                                    && !PS抓盘手臂放空盘到Empty流程.Processing
                                                    && !PS抓盘手臂取Load空盘流程.Processing;

                            if (PS抓盘手臂取Empty空盘流程.Condition && PS抓盘手臂取Empty空盘流程.Permit)
                            {
                                PS抓盘手臂取Empty空盘流程.Processing = true;
                                PS抓盘手臂取Empty空盘流程.StepID = 505;
                            }
                            else Thread.Sleep(10);
                            break;

                        case 505:
                            PS抓盘手臂取Empty空盘流程.StepName = "移Tray上下气缸原位输入读取";
                            RunLog($"PS抓盘手臂取Empty空盘流程.StepID={PS抓盘手臂取Empty空盘流程.StepID}->{PS抓盘手臂取Empty空盘流程.StepName}");
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
                                    Listcode.Add($"1010001"); //C_空盘手臂_升降--气缸传感器没有检测到OFF
                                    RunLog($"[Code]1010001,[Msg]移Tray上下气缸未在原位", LogType.AlarmLog);
                                }
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add($"1010001"); //C_空盘手臂_升降--气缸传感器没有检测到OFF
                                RunLog($"[Code]1010001,[Msg]移Tray上下气缸原位输入读取失败！" + result, LogType.AlarmLog);
                            }
                            break;

                        case 510:
                            PS抓盘手臂取Empty空盘流程.StepName = "移Tray夹爪气缸原位输入读取";
                            RunLog($"PS抓盘手臂取Empty空盘流程.StepID={PS抓盘手臂取Empty空盘流程.StepID}->{PS抓盘手臂取Empty空盘流程.StepName}");
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
                                    Listcode.Add($"1010003"); //C_空盘手臂_松开--气缸传感器没有检测到ON
                                    RunLog($"[Code]1010003,[Msg]移Tray夹爪气缸未在原位", LogType.AlarmLog);
                                }
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add($"1010003"); //C_空盘手臂_松开--气缸传感器没有检测到ON
                                RunLog($"[Code]1010003,[Msg]移Tray夹爪气缸原位输入读取失败！" + result, LogType.AlarmLog);
                            }
                            break;

                        case 515:
                            PS抓盘手臂取Empty空盘流程.StepName = "放Tary盘轴移动到Empty";
                            RunLog($"PS抓盘手臂取Empty空盘流程.StepID={PS抓盘手臂取Empty空盘流程.StepID}->{PS抓盘手臂取Empty空盘流程.StepName}");
                            result = 放Tary盘轴移动(初始点位.抓盘_Empty_X + 点位微调.抓盘_Empty_X);
                            if (result == "Success")
                            {
                                PS抓盘手臂取Empty空盘流程.StepID = 520;
                            }
                            else
                            {
                                Listcode.Add("2025007");
                                RunLog($"[Code]2025007,[Msg]放Tary盘轴移动到Empty失败!" + result, LogType.AlarmLog);
                                M18_放Tary盘轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 520:
                            PS抓盘手臂取Empty空盘流程.StepName = "移Tray上下气缸到位输出";
                            RunLog($"PS抓盘手臂取Empty空盘流程.StepID={PS抓盘手臂取Empty空盘流程.StepID}->{PS抓盘手臂取Empty空盘流程.StepName}");
                            result = 移Tray上下气缸到位输出();
                            if (result == "Success")
                            {
                                PS抓盘手臂取Empty空盘流程.StepID = 525;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add("1010000"); //C_空盘手臂_升降--气缸传感器没有检测到ON
                                RunLog($"[Code]1010000,[Msg]移Tray上下气缸到位输出失败！" + result, LogType.AlarmLog);
                            }
                            break;

                        case 525:
                            PS抓盘手臂取Empty空盘流程.StepName = "移Tray夹爪气缸到位输出";
                            RunLog($"PS抓盘手臂取Empty空盘流程.StepID={PS抓盘手臂取Empty空盘流程.StepID}->{PS抓盘手臂取Empty空盘流程.StepName}");
                            result = 移Tray夹爪气缸到位输出();
                            if (result == "Success")
                            {
                                PS抓盘手臂取Empty空盘流程.StepID = 530;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add("1010002"); //C_空盘手臂_夹紧--气缸传感器没有检测到ON
                                RunLog($"[Code]1010002,[Msg]移Tray夹爪气缸到位输出失败！" + result, LogType.AlarmLog);
                            }
                            break;

                        case 530:
                            PS抓盘手臂取Empty空盘流程.StepName = "移Tray上下气缸原位输出";
                            RunLog($"PS抓盘手臂取Empty空盘流程.StepID={PS抓盘手臂取Empty空盘流程.StepID}->{PS抓盘手臂取Empty空盘流程.StepName}");
                            result = 移Tray上下气缸原位输出();
                            if (result == "Success")
                            {
                                PS抓盘手臂取Empty空盘流程.StepID = 535;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add("1010001"); //C_空盘手臂_升降--气缸传感器没有检测到OFF
                                RunLog($"[Code]1010001,[Msg]移Tray上下气缸原位输出失败！" + result, LogType.AlarmLog);
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
                                    Listcode.Add("3000000");
                                    RunLog($"[Code]3000000,[Msg]空盘手臂取空盘错误，检查取料盘是否为空！" + result, LogType.AlarmLog);
                                }
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"检测Empty是否无空盘失败！" + result);
                            }
                            break;

                        case 536:
                            PS抓盘手臂取Empty空盘流程.StepName = "移Tray夹爪气缸原位输出";
                            RunLog($"PS抓盘手臂取Empty空盘流程.StepID={PS抓盘手臂取Empty空盘流程.StepID}->{PS抓盘手臂取Empty空盘流程.StepName}");
                            result = 移Tray夹爪气缸原位输出();
                            if (result == "Success")
                            {
                                PS抓盘手臂取Empty空盘流程.StepID = 520;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add("1010003"); //C_空盘手臂_松开--气缸传感器没有检测到ON
                                RunLog($"[Code]1010003,[Msg]移Tray夹爪气缸原位输出失败！" + result, LogType.AlarmLog);
                            }
                            break;

                        //case 540:
                        //    PS抓盘手臂取Empty空盘流程.StepName = "判断Empty顶盘轴是否走到最大值";
                        //    RunLog($"PS抓盘手臂取Empty空盘流程.StepID={PS抓盘手臂取Empty空盘流程.StepID}->{PS抓盘手臂取Empty空盘流程.StepName}");
                        //    result = motionEx.GetCmdEncPos("M12_Empty顶盘轴", out double prfpos);
                        //    if (result == "Success")
                        //    {
                        //        if (prfpos + 12700 > 空盘上顶最大值)
                        //        {
                        //            RunLog($"M12_Empty顶盘轴当前上升行程{prfpos + 12700}>{空盘上顶最大值}");
                        //            PS抓盘手臂取Empty空盘流程.StepID = 700; //执行回零再触发上顶
                        //        }
                        //        else PS抓盘手臂取Empty空盘流程.StepID = 545; //上顶一个Tray盘高度
                        //    }
                        //    else
                        //    {
                        //        APPmaster.ProgramPause = true;
                        //        IsPause = true;
                        //        RunLog($"判断Empty顶盘轴是否走到最大值失败！" + result);
                        //    }
                        //    break;

                        //case 545:
                        //    PS抓盘手臂取Empty空盘流程.StepName = "Empty盘轴上顶一个Tray盘";
                        //    RunLog($"PS抓盘手臂取Empty空盘流程.StepID={PS抓盘手臂取Empty空盘流程.StepID}->{PS抓盘手臂取Empty空盘流程.StepName}");
                        //    result = Empty顶盘轴相对移动(6.35);
                        //    if (result == "Success")
                        //    {
                        //        PS抓盘手臂取Empty空盘流程.StepID = 550; //后续需要检测传感器是否灭了
                        //    }
                        //    else
                        //    {
                        //        APPmaster.ProgramPause = true;
                        //        IsPause = true;
                        //        RunLog($"Empty盘轴上顶一个Tray盘失败！" + result);
                        //    }
                        //    break;

                        //case 550:
                        //    PS抓盘手臂取Empty空盘流程.StepName = "检测Empty是否有空盘";
                        //    RunLog($"PS抓盘手臂取Empty空盘流程.StepID={PS抓盘手臂取Empty空盘流程.StepID}->{PS抓盘手臂取Empty空盘流程.StepName}");
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
                        //        RunLog($"检测Empty是否有空盘失败！" + result);
                        //    }
                        //    break;

                        //case 565:
                        //    PS抓盘手臂取Empty空盘流程.StepName = "Empty盘轴下降一个Tray盘";
                        //    RunLog($"PS抓盘手臂取Empty空盘流程.StepID={PS抓盘手臂取Empty空盘流程.StepID}->{PS抓盘手臂取Empty空盘流程.StepName}");
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
                        //        RunLog($"Empty盘轴下降一个Tray盘失败！" + result);
                        //    }
                        //    break;

                        case 599:
                            PS抓盘手臂取Empty空盘流程.StepName = $"抓盘手臂取Empty空盘流程完成";
                            RunLog($"PS待测盘上顶流程.StepID={PS抓盘手臂取Empty空盘流程.StepID}->{PS抓盘手臂取Empty空盘流程.StepName}");
                            PS抓盘手臂取Empty空盘流程.Processing = false;
                            PS抓盘手臂取Empty空盘流程.StepID = 500;
                            break;

                        //case 700:
                        //    PS抓盘手臂取Empty空盘流程.StepName = "Empty顶盘轴回零";
                        //    RunLog($"PS抓盘手臂取Empty空盘流程.StepID={PS抓盘手臂取Empty空盘流程.StepID}->{PS抓盘手臂取Empty空盘流程.StepName}");
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
                        //        RunLog($"Empty顶盘轴回零失败！" + result);
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
                            RunLog(("PS抓盘手臂取Empty空盘流程.StepID =" + PS抓盘手臂取Empty空盘流程.StepID.ToString() + "对应流程不存在"));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    string errormsg = $"PS抓盘手臂取Empty空盘流程({PS抓盘手臂取Empty空盘流程.StepID})运行异常,详细信息:" + ex.Message;
                    RunLog(errormsg, LogType.AlarmLog);
                    系统未定义错误ErrorMsg = errormsg;
                    ProgramPause = true;
                    IsPause = true;
                    Listcode.Add("1400070");
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
                try
                {
                    Thread.Sleep(10);

                    if (APPmaster.IsForecePause)
                    {
                        PS抓盘手臂取Load空盘流程.State = 1;
                        continue;
                    }

                    if (APPmaster.IsPause)
                    {
                        continue;
                    }

                    switch (PS抓盘手臂取Load空盘流程.StepID)
                    {
                        case 500:
                            PS抓盘手臂取Load空盘流程.StepName = "路径规划";
                            PS抓盘手臂取Load空盘流程.Condition = 抓盘手臂取Load空盘标记 == 2
                                                        && 抓盘手臂放空盘到Empty标记 == 1
                                                        && Load上顶标记 == 1
                                                        && Empty下降标记 == 1
                                                        && Empty上顶标记 == 1;
                            //&& !Auto1换盘标记
                            //&& !Auto2换盘标记;

                            PS抓盘手臂取Load空盘流程.Permit = !PS抓盘手臂取Load空盘流程.Processing
                                                        && !PS待测盘上顶流程.Processing
                                                        && !PS待测盘下降流程.Processing
                                                        && !PS待测盘下降回零流程.Processing
                                                        && !PS空盘下降流程.Processing
                                                        && !PS空盘上顶流程.Processing
                                                        && !PS空盘下降回零流程.Processing
                                                        && !PS抓盘手臂放空盘到Empty流程.Processing;

                            if (PS抓盘手臂取Load空盘流程.Condition && PS抓盘手臂取Load空盘流程.Permit)
                            {
                                Empty下降标记 = 2;
                                PS抓盘手臂取Load空盘流程.Processing = true;
                                PS抓盘手臂取Load空盘流程.StepID = 501;
                                RunLog($"PS抓盘手臂取Load空盘流程.StepID={PS抓盘手臂取Load空盘流程.StepID}->判断是否按了跳过或编辑标记");
                            }
                            else Thread.Sleep(10);
                            break;

                        case 501:
                            PS抓盘手臂取Load空盘流程.StepName = "判断是否按了跳过或编辑标记";
                            if (按了跳过或编辑标记)
                            {
                                Load_Tray前侧定位气缸原位输出();
                                门已打开超设定时间 = false;
                                等待Load门开启 = true;
                                Listcode.Add("1400472"); //跳过抓取Load空盘
                                RunLog($"[Code]1400472,[Msg]请打开前安全门{Load门开门时间}秒并确认是否有料剩余在Tray盘！", LogType.AlarmLog);
                                ProgramPause = true;
                                IsPause = true;
                                RunLog($"PS抓盘手臂取Load空盘流程.StepID={PS抓盘手臂取Load空盘流程.StepID}->等待Load仓门开启");
                                PS抓盘手臂取Load空盘流程.StepID = 502;
                            }
                            else
                            {
                                PS抓盘手臂取Load空盘流程.StepID = 505; //没有按下过直接走正常流程
                            }
                            break;

                        case 502:
                            PS抓盘手臂取Load空盘流程.StepName = "等待Load仓门开启";
                            if (门已打开超设定时间)
                            {
                                等待Load门开启 = false;
                                按了跳过或编辑标记 = false;
                                PS抓盘手臂取Load空盘流程.StepID = 505;
                            }
                            else
                            {
                                Listcode.Add("1400472"); //跳过抓取Load空盘
                                RunLog($"[Code]1400472,[Msg]请打开前安全门{Load门开门时间}秒并确认是否有料剩余在Tray盘！" + result, LogType.AlarmLog);
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 505:
                            PS抓盘手臂取Load空盘流程.StepName = "移Tray上下气缸原位输入读取";
                            RunLog($"PS抓盘手臂取Load空盘流程.StepID={PS抓盘手臂取Load空盘流程.StepID}->{PS抓盘手臂取Load空盘流程.StepName}");
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
                                    Listcode.Add($"1010001"); //C_空盘手臂_升降--气缸传感器没有检测到OFF
                                    RunLog($"[Code]1010001,[Msg]移Tray上下气缸未在原位", LogType.AlarmLog);
                                }
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add($"1010001"); //C_空盘手臂_升降--气缸传感器没有检测到OFF
                                RunLog($"[Code]1010001,[Msg]移Tray上下气缸原位输入读取失败！" + result, LogType.AlarmLog);
                            }
                            break;

                        case 510:
                            PS抓盘手臂取Load空盘流程.StepName = "移Tray夹爪气缸原位输入读取";
                            RunLog($"PS抓盘手臂取Load空盘流程.StepID={PS抓盘手臂取Load空盘流程.StepID}->{PS抓盘手臂取Load空盘流程.StepName}");
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
                                    Listcode.Add($"1010003"); //C_空盘手臂_松开--气缸传感器没有检测到ON
                                    RunLog($"[Code]1010003,[Msg]移Tray夹爪气缸未在原位", LogType.AlarmLog);
                                }
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add($"1010003"); //C_空盘手臂_松开--气缸传感器没有检测到ON
                                RunLog($"[Code]1010003,[Msg]移Tray夹爪气缸原位输入读取失败！" + result, LogType.AlarmLog);
                            }
                            break;

                        case 515:
                            PS抓盘手臂取Load空盘流程.StepName = "放Tary盘轴移动到Load";
                            RunLog($"PS抓盘手臂取Load空盘流程.StepID={PS抓盘手臂取Load空盘流程.StepID}->{PS抓盘手臂取Load空盘流程.StepName}");
                            result = 放Tary盘轴移动(初始点位.抓盘_Load_X + 点位微调.抓盘_Load_X);
                            if (result == "Success")
                            {
                                PS抓盘手臂取Load空盘流程.StepID = 520;
                            }
                            else
                            {
                                Listcode.Add("2025007");
                                RunLog($"[Code]2025007,[Msg]放Tary盘轴移动到Load失败!" + result, LogType.AlarmLog);
                                M18_放Tary盘轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 520:
                            PS抓盘手臂取Load空盘流程.StepName = "Load_Tray前侧定位气缸原位输出";
                            RunLog($"PS抓盘手臂取Load空盘流程.StepID={PS抓盘手臂取Load空盘流程.StepID}->{PS抓盘手臂取Load空盘流程.StepName}");
                            result = Load_Tray前侧定位气缸原位输出();
                            if (result == "Success")
                            {
                                PS抓盘手臂取Load空盘流程.StepID = 521;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Load_Tray前侧定位气缸原位输出失败！" + result);
                            }
                            break;

                        case 521:
                            PS抓盘手臂取Load空盘流程.StepName = "移Tray上下气缸到位输出";
                            RunLog($"PS抓盘手臂取Load空盘流程.StepID={PS抓盘手臂取Load空盘流程.StepID}->{PS抓盘手臂取Load空盘流程.StepName}");
                            result = 移Tray上下气缸到位输出();
                            if (result == "Success")
                            {
                                PS抓盘手臂取Load空盘流程.StepID = 525;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add("1010000"); //C_空盘手臂_升降--气缸传感器没有检测到ON
                                RunLog($"[Code]1010000,[Msg]移Tray上下气缸到位输出失败！" + result, LogType.AlarmLog);
                            }
                            break;

                        case 525:
                            PS抓盘手臂取Load空盘流程.StepName = "移Tray夹爪气缸到位输出";
                            RunLog($"PS抓盘手臂取Load空盘流程.StepID={PS抓盘手臂取Load空盘流程.StepID}->{PS抓盘手臂取Load空盘流程.StepName}");
                            result = 移Tray夹爪气缸到位输出();
                            if (result == "Success")
                            {
                                PS抓盘手臂取Load空盘流程.StepID = 530;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add("1010002"); //C_空盘手臂_夹紧--气缸传感器没有检测到ON
                                RunLog($"[Code]1010002,[Msg]移Tray夹爪气缸到位输出失败！" + result, LogType.AlarmLog);
                            }
                            break;

                        case 530:
                            PS抓盘手臂取Load空盘流程.StepName = "移Tray上下气缸原位输出";
                            RunLog($"PS抓盘手臂取Load空盘流程.StepID={PS抓盘手臂取Load空盘流程.StepID}->{PS抓盘手臂取Load空盘流程.StepName}");
                            result = 移Tray上下气缸原位输出();
                            if (result == "Success")
                            {
                                PS抓盘手臂取Load空盘流程.StepID = 535;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add("1010001"); //C_空盘手臂_升降--气缸传感器没有检测到OFF
                                RunLog($"[Code]1010001,[Msg]移Tray上下气缸原位输出失败！" + result, LogType.AlarmLog);
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
                                    RunLog($"抓盘手臂抓盘失败！" + result);
                                }
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"检测Empty是否无空盘失败！" + result);
                            }
                            break;

                        case 536:
                            PS抓盘手臂取Load空盘流程.StepName = "移Tray夹爪气缸原位输出";
                            RunLog($"PS抓盘手臂取Load空盘流程.StepID={PS抓盘手臂取Load空盘流程.StepID}->{PS抓盘手臂取Load空盘流程.StepName}");
                            result = 移Tray夹爪气缸原位输出();
                            if (result == "Success")
                            {
                                PS抓盘手臂取Load空盘流程.StepID = 515;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add("1010003"); //C_空盘手臂_松开--气缸传感器没有检测到ON
                                RunLog($"[Code]1010003,[Msg]移Tray夹爪气缸原位输出失败！" + result, LogType.AlarmLog);
                            }
                            break;

                        case 540:
                            PS抓盘手臂取Load空盘流程.StepName = "放Tary盘轴移动到Empty";
                            RunLog($"PS抓盘手臂取Load空盘流程.StepID={PS抓盘手臂取Load空盘流程.StepID}->{PS抓盘手臂取Load空盘流程.StepName}");
                            result = 放Tary盘轴移动(初始点位.抓盘_Empty_X + 点位微调.抓盘_Empty_X);
                            if (result == "Success")
                            {
                                抓盘手臂取Load空盘标记 = 1;
                                抓盘手臂放空盘到Empty标记 = 2;
                                PS抓盘手臂取Load空盘流程.StepID = 599;
                            }
                            else
                            {
                                Listcode.Add("2025007");
                                RunLog($"[Code]2025007,[Msg]放Tary盘轴移动到Empty失败!" + result, LogType.AlarmLog);
                                M18_放Tary盘轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        //case 540:
                        //    PS抓盘手臂取Load空盘流程.StepName = "判断Load顶盘轴是否走到最大值";
                        //    RunLog($"PS抓盘手臂取Load空盘流程.StepID={PS抓盘手臂取Load空盘流程.StepID}->{PS抓盘手臂取Load空盘流程.StepName}");
                        //    result = motionEx.GetCmdEncPos("M13_Loader顶盘轴", out double prfpos);
                        //    if (result == "Success")
                        //    {
                        //        if (prfpos + 12700 > 待测盘上顶最大值)
                        //        {
                        //            RunLog($"M13_Loader顶盘轴当前上升行程{prfpos + 12700}>{待测盘上顶最大值}");
                        //            PS抓盘手臂取Load空盘流程.StepID = 700; //执行回零再触发上顶
                        //        }
                        //        else PS抓盘手臂取Load空盘流程.StepID = 545; //上顶一个Tray盘高度
                        //    }
                        //    else
                        //    {
                        //        APPmaster.ProgramPause = true;
                        //        IsPause = true;
                        //        RunLog($"判断Load顶盘轴是否走到最大值失败！" + result);
                        //    }
                        //    break;

                        //case 545:
                        //    PS抓盘手臂取Load空盘流程.StepName = "Load盘轴上顶一个Tray盘";
                        //    RunLog($"PS抓盘手臂取Load空盘流程.StepID={PS抓盘手臂取Load空盘流程.StepID}->{PS抓盘手臂取Load空盘流程.StepName}");
                        //    result = Load顶盘轴相对移动(6.35);
                        //    if (result == "Success")
                        //    {
                        //        PS抓盘手臂取Load空盘流程.StepID = 550; //后续需要检测传感器是否灭了
                        //    }
                        //    else
                        //    {
                        //        APPmaster.ProgramPause = true;
                        //        IsPause = true;
                        //        RunLog($"Load盘轴上顶一个Tray盘失败！" + result);
                        //    }
                        //    break;

                        //case 550:
                        //    PS抓盘手臂取Load空盘流程.StepName = "检测Load是否有空盘";
                        //    RunLog($"PS抓盘手臂取Load空盘流程.StepID={PS抓盘手臂取Load空盘流程.StepID}->{PS抓盘手臂取Load空盘流程.StepName}");
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
                        //        RunLog($"检测Load是否有空盘失败！" + result);
                        //    }
                        //    break;

                        //case 555:
                        //    PS抓盘手臂取Load空盘流程.StepName = "Load_Tray前侧定位气缸到位输出";
                        //    RunLog($"PS抓盘手臂取Load空盘流程.StepID={PS抓盘手臂取Load空盘流程.StepID}->{PS抓盘手臂取Load空盘流程.StepName}");
                        //    result = Load_Tray前侧定位气缸到位输出();
                        //    if (result == "Success")
                        //    {
                        //        PS抓盘手臂取Load空盘流程.StepID = 599;
                        //    }
                        //    else
                        //    {
                        //        APPmaster.ProgramPause = true;
                        //        IsPause = true;
                        //        RunLog($"Load_Tray前侧定位气缸到位输出失败！" + result);
                        //    }
                        //    break;

                        //case 565:
                        //    PS抓盘手臂取Load空盘流程.StepName = "Load盘轴下降一个Tray盘";
                        //    RunLog($"PS抓盘手臂取Load空盘流程.StepID={PS抓盘手臂取Load空盘流程.StepID}->{PS抓盘手臂取Load空盘流程.StepName}");
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
                        //        RunLog($"Load盘轴下降一个Tray盘失败！" + result);
                        //    }
                        //    break;

                        case 599:
                            PS抓盘手臂取Load空盘流程.StepName = $"抓盘手臂取Load空盘流程完成";
                            RunLog($"PS抓盘手臂取Load空盘流程.StepID={PS抓盘手臂取Load空盘流程.StepID}->{PS抓盘手臂取Load空盘流程.StepName}");
                            PS抓盘手臂取Load空盘流程.Processing = false;
                            PS抓盘手臂取Load空盘流程.StepID = 500;
                            break;

                        //case 700:
                        //    PS抓盘手臂取Load空盘流程.StepName = "Load顶盘轴回零";
                        //    RunLog($"PS抓盘手臂取Load空盘流程.StepID={PS抓盘手臂取Load空盘流程.StepID}->{PS抓盘手臂取Load空盘流程.StepName}");
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
                        //        RunLog($"Load顶盘轴回零失败！" + result);
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
                            RunLog(("PS待测盘上顶流程.StepID =" + PS抓盘手臂取Load空盘流程.StepID.ToString() + "对应流程不存在"));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    string errormsg = $"PS抓盘手臂取Load空盘流程({PS抓盘手臂取Load空盘流程.StepID})运行异常,详细信息:" + ex.Message;
                    RunLog(errormsg, LogType.AlarmLog);
                    系统未定义错误ErrorMsg = errormsg;
                    ProgramPause = true;
                    IsPause = true;
                    Listcode.Add("1400070");
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
                try
                {
                    Thread.Sleep(10);

                    if (APPmaster.IsForecePause)
                    {
                        PS抓盘手臂放空盘到Empty流程.State = 1;
                        continue;
                    }

                    if (APPmaster.IsPause)
                    {
                        continue;
                    }

                    switch (PS抓盘手臂放空盘到Empty流程.StepID)
                    {
                        case 500:
                            PS抓盘手臂放空盘到Empty流程.StepName = "路径规划";
                            PS抓盘手臂放空盘到Empty流程.Condition = 抓盘手臂放空盘到Empty标记 == 2 && Empty上顶标记 == 1 && Empty下降标记 == 1;
                            PS抓盘手臂放空盘到Empty流程.Permit = !PS抓盘手臂放空盘到Empty流程.Processing && !PS空盘上顶流程.Processing && !PS空盘下降流程.Processing;
                            if (PS抓盘手臂放空盘到Empty流程.Condition && PS抓盘手臂放空盘到Empty流程.Permit)
                            {
                                PS抓盘手臂放空盘到Empty流程.Processing = true;
                                PS抓盘手臂放空盘到Empty流程.StepID = 505;
                            }
                            break;

                        case 505:
                            PS抓盘手臂放空盘到Empty流程.StepName = "移Tray上下气缸原位输入读取";
                            RunLog($"PS抓盘手臂放空盘到Empty流程.StepID={PS抓盘手臂放空盘到Empty流程.StepID}->{PS抓盘手臂放空盘到Empty流程.StepName}");
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
                                    Listcode.Add($"1010001"); //C_空盘手臂_升降--气缸传感器没有检测到OFF
                                    RunLog($"[Code]1010001,[Msg]移Tray上下气缸未在原位", LogType.AlarmLog);
                                }
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add($"1010001"); //C_空盘手臂_升降--气缸传感器没有检测到OFF
                                RunLog($"[Code]1010001,[Msg]移Tray上下气缸原位输入读取失败！" + result, LogType.AlarmLog);
                            }
                            break;

                        case 510:
                            PS抓盘手臂放空盘到Empty流程.StepName = "移Tray夹爪气缸到位输入读取";
                            RunLog($"PS抓盘手臂放空盘到Empty流程.StepID={PS抓盘手臂放空盘到Empty流程.StepID}->{PS抓盘手臂放空盘到Empty流程.StepName}");
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
                                    Listcode.Add($"1010002"); //C_空盘手臂_夹紧--气缸传感器没有检测到ON
                                    RunLog($"[Code]1010002,[Msg]移Tray夹爪气缸未夹紧", LogType.AlarmLog);
                                }
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add($"1010002"); //C_空盘手臂_夹紧--气缸传感器没有检测到ON
                                RunLog($"[Code]1010002,[Msg]移Tray夹爪气缸原位输入读取失败！" + result, LogType.AlarmLog);
                            }
                            break;

                        case 515:
                            PS抓盘手臂放空盘到Empty流程.StepName = "放Tary盘轴移动到Empty";
                            RunLog($"PS抓盘手臂放空盘到Empty流程.StepID={PS抓盘手臂放空盘到Empty流程.StepID}->{PS抓盘手臂放空盘到Empty流程.StepName}");
                            result = 放Tary盘轴移动(初始点位.抓盘_Empty_X + 点位微调.抓盘_Empty_X);
                            if (result == "Success")
                            {
                                PS抓盘手臂放空盘到Empty流程.StepID = 516;
                            }
                            else
                            {
                                Listcode.Add("2025007");
                                RunLog($"[Code]2025007,[Msg]放Tary盘轴移动到Empty失败!" + result, LogType.AlarmLog);
                                M18_放Tary盘轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 516:
                            PS抓盘手臂放空盘到Empty流程.StepName = "判断Empty下降是否完成";
                            if (Empty下降标记 == 1 && Empty上顶标记 == 1)
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
                            RunLog($"PS抓盘手臂放空盘到Empty流程.StepID={PS抓盘手臂放空盘到Empty流程.StepID}->{PS抓盘手臂放空盘到Empty流程.StepName}");
                            result = 移Tray上下气缸到位输出();
                            if (result == "Success")
                            {
                                PS抓盘手臂放空盘到Empty流程.StepID = 525;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add("1010000"); //C_空盘手臂_升降--气缸传感器没有检测到ON
                                RunLog($"[Code]1010000,[Msg]移Tray上下气缸到位输出失败！" + result, LogType.AlarmLog);
                            }
                            break;

                        case 525:
                            PS抓盘手臂放空盘到Empty流程.StepName = "移Tray夹爪气缸原位输出";
                            RunLog($"PS抓盘手臂放空盘到Empty流程.StepID={PS抓盘手臂放空盘到Empty流程.StepID}->{PS抓盘手臂放空盘到Empty流程.StepName}");
                            result = 移Tray夹爪气缸原位输出();
                            if (result == "Success")
                            {
                                PS抓盘手臂放空盘到Empty流程.StepID = 530;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add("1010003"); //C_空盘手臂_松开--气缸传感器没有检测到ON
                                RunLog($"[Code]1010003,[Msg]移Tray夹爪气缸原位输出失败！" + result, LogType.AlarmLog);
                            }
                            break;

                        case 530:
                            PS抓盘手臂放空盘到Empty流程.StepName = "移Tray上下气缸原位输出";
                            RunLog($"PS抓盘手臂放空盘到Empty流程.StepID={PS抓盘手臂放空盘到Empty流程.StepID}->{PS抓盘手臂放空盘到Empty流程.StepName}");
                            result = 移Tray上下气缸原位输出();
                            if (result == "Success")
                            {
                                PS抓盘手臂放空盘到Empty流程.StepID = 535;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add("1010001"); //C_空盘手臂_升降--气缸传感器没有检测到OFF
                                RunLog($"[Code]1010001,[Msg]移Tray上下气缸原位输出失败！" + result, LogType.AlarmLog);
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
                                        Listcode.Add("4018004");
                                        RunLog("[Code]4018004,[Msg]Empty前对射感应到无Tray盘,请打开Empty仓门等待轴降到最低,再按开始重新上顶", LogType.AlarmLog);
                                        RunLog($"抓盘手臂放盘失败！Empty没有感应到Tray盘,value:" + value);
                                    }
                                    if (value2 != "1")
                                    {
                                        Listcode.Add("4018006");
                                        RunLog("[Code]4018006,[Msg]Empty后对射感应到有Tray盘,请打开Empty仓门等待轴降到最低,再按开始重新上顶", LogType.AlarmLog);
                                        RunLog($"抓盘手臂放盘失败！Empty感应到Tray盘放歪,value:" + value2);
                                    }
                                }
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                if (result != "Success") RunLog($"读取Empty前Tray存在感应失败！" + result);
                                if (result2 != "Success") RunLog($"读取Empty后Tray放歪感应失败！" + result2);
                            }
                            break;

                        case 540:
                            PS抓盘手臂放空盘到Empty流程.StepName = "放Tary盘轴回到原点";
                            RunLog($"PS抓盘手臂放空盘到Empty流程.StepID={PS抓盘手臂放空盘到Empty流程.StepID}->{PS抓盘手臂放空盘到Empty流程.StepName}");
                            result = 放Tary盘轴移动(0);
                            if (result == "Success")
                            {
                                抓盘手臂放空盘到Empty标记 = 1;
                                PS抓盘手臂放空盘到Empty流程.StepID = 599;
                            }
                            else
                            {
                                Listcode.Add("2025007");
                                RunLog($"[Code]2025007,[Msg]放Tary盘轴回到原点失败!" + result, LogType.AlarmLog);
                                M18_放Tary盘轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 599:
                            PS抓盘手臂放空盘到Empty流程.StepName = $"抓盘手臂放空盘到Empty流程完成";
                            RunLog($"PS抓盘手臂放空盘到Empty流程.StepID={PS抓盘手臂放空盘到Empty流程.StepID}->{PS抓盘手臂放空盘到Empty流程.StepName}");
                            PS抓盘手臂放空盘到Empty流程.Processing = false;
                            PS抓盘手臂放空盘到Empty流程.StepID = 500;
                            break;

                        default:
                            RunLog(("PS抓盘手臂放空盘到Empty流程.StepID =" + PS抓盘手臂放空盘到Empty流程.StepID.ToString() + "对应流程不存在"));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    string errormsg = $"PS抓盘手臂放空盘到Empty流程({PS抓盘手臂放空盘到Empty流程.StepID})运行异常,详细信息:" + ex.Message;
                    RunLog(errormsg, LogType.AlarmLog);
                    系统未定义错误ErrorMsg = errormsg;
                    ProgramPause = true;
                    IsPause = true;
                    Listcode.Add("1400070");
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
                try
                {
                    Thread.Sleep(10);

                    if (APPmaster.IsForecePause)
                    {
                        PS抓盘手臂放空盘到Auto1流程.State = 1;
                        continue;
                    }

                    if (APPmaster.IsPause)
                    {
                        continue;
                    }

                    switch (PS抓盘手臂放空盘到Auto1流程.StepID)
                    {
                        case 500:
                            PS抓盘手臂放空盘到Auto1流程.StepName = "路径规划";
                            PS抓盘手臂放空盘到Auto1流程.Condition = 抓盘手臂放空盘到Auto1标记 == 2
                                                        && 抓盘手臂取Empty空盘标记 == 1;

                            PS抓盘手臂放空盘到Auto1流程.Permit = !PS抓盘手臂放空盘到Auto1流程.Processing
                                                        && !PS抓盘手臂取Empty空盘流程.Processing;

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
                            RunLog($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
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
                                    Listcode.Add($"1010001"); //C_空盘手臂_升降--气缸传感器没有检测到OFF
                                    RunLog($"[Code]1010001,[Msg]移Tray上下气缸未在原位", LogType.AlarmLog);
                                }
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add($"1010001"); //C_空盘手臂_升降--气缸传感器没有检测到OFF
                                RunLog($"[Code]1010001,[Msg]移Tray上下气缸原位输入读取失败！" + result, LogType.AlarmLog);
                            }
                            break;

                        case 510:
                            PS抓盘手臂放空盘到Auto1流程.StepName = "移Tray夹爪气缸到位输入读取";
                            RunLog($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
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
                                    Listcode.Add($"1010002"); //C_空盘手臂_夹紧--气缸传感器没有检测到ON
                                    RunLog($"[Code]1010002,[Msg]移Tray夹爪气缸未夹紧", LogType.AlarmLog);
                                }
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add($"1010002"); //C_空盘手臂_夹紧--气缸传感器没有检测到ON
                                RunLog($"[Code]1010002,[Msg]移Tray夹爪气缸原位输入读取失败！" + result, LogType.AlarmLog);
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
                            RunLog($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
                            result = 放Tary盘轴移动(初始点位.抓盘_Auto1_X - 200);//656.78
                            if (result == "Success")
                            {
                                PS抓盘手臂放空盘到Auto1流程.StepID = 513;
                            }
                            else
                            {
                                Listcode.Add("2025007");
                                RunLog($"[Code]2025007,[Msg]放Tary盘轴移动到Auto1等待位失败!" + result, LogType.AlarmLog);
                                M18_放Tary盘轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 513:
                            if (Auto1上顶标记 == 1)
                            {
                                PS抓盘手臂放空盘到Auto1流程.StepID = 514;
                            }
                            else
                            {
                                Thread.Sleep(10);
                            }
                            break;

                        case 514:
                            PS抓盘手臂放空盘到Auto1流程.StepName = "检测Auto1是否无空盘";
                            RunLog($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
                            result = Auto1_前Tray存在感应(out value);
                            result2 = Auto1_后Tray放歪感应(out string value3);
                            if (result == "Success" && result2 == "Success")
                            {
                                if (value == "1" && value3 == "1") //返回0是挡到感应器,1是没有挡到感应器
                                {
                                    PS抓盘手臂放空盘到Auto1流程.StepID = 515;
                                }
                                else //没有挡到感应器或者放歪感应器被挡到说明没有放盘成功，需重新放盘
                                {
                                    APPmaster.ProgramPause = true;
                                    IsPause = true;

                                    if (value != "1")
                                    {
                                        Listcode.Add("4020001");
                                        RunLog("[Code]4020001,[Msg]Auto1前对射感应到有Tray盘", LogType.AlarmLog);
                                    }
                                    if (value3 != "1")
                                    {
                                        Listcode.Add("4020001");
                                        RunLog("[Code]4020001,[Msg]Auto1后对射感应到有Tray盘", LogType.AlarmLog);
                                    }
                                }
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;

                                if (result != "Success")
                                {
                                    Listcode.Add("4020001");
                                    RunLog("[Code]4020001,[Msg]读取Auto1前Tray存在感应失败！" + result, LogType.AlarmLog);
                                }
                                if (result2 != "Success")
                                {
                                    Listcode.Add("4020001");
                                    RunLog("[Code]4020001,[Msg]读取Auto1后Tray放歪感应失败!" + result2, LogType.AlarmLog);
                                }
                            }
                            break;

                        case 515:
                            PS抓盘手臂放空盘到Auto1流程.StepName = "放Tary盘轴移动到Auto1";
                            RunLog($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
                            result = 放Tary盘轴移动(初始点位.抓盘_Auto1_X + 点位微调.抓盘_Auto1_X);
                            if (result == "Success")
                            {
                                PS抓盘手臂放空盘到Auto1流程.StepID = 520;
                            }
                            else
                            {
                                Listcode.Add("2025007");
                                RunLog($"[Code]2025007,[Msg]放Tary盘轴移动到Auto1失败!" + result, LogType.AlarmLog);
                                M18_放Tary盘轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 520:
                            PS抓盘手臂放空盘到Auto1流程.StepName = "移Tray上下气缸到位输出";
                            RunLog($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
                            result = 移Tray上下气缸到位输出();
                            if (result == "Success")
                            {
                                PS抓盘手臂放空盘到Auto1流程.StepID = 525;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add("1010000"); //C_空盘手臂_升降--气缸传感器没有检测到ON
                                RunLog($"[Code]1010000,[Msg]移Tray上下气缸到位输出失败！" + result, LogType.AlarmLog);
                            }
                            break;

                        case 525:
                            PS抓盘手臂放空盘到Auto1流程.StepName = "移Tray夹爪气缸原位输出";
                            RunLog($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
                            result = 移Tray夹爪气缸原位输出();
                            if (result == "Success")
                            {
                                PS抓盘手臂放空盘到Auto1流程.StepID = 530;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add("1010003"); //C_空盘手臂_松开--气缸传感器没有检测到ON
                                RunLog($"[Code]1010003,[Msg]移Tray夹爪气缸原位输出失败！" + result, LogType.AlarmLog);
                            }
                            break;

                        case 530:
                            PS抓盘手臂放空盘到Auto1流程.StepName = "移Tray上下气缸原位输出";
                            RunLog($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
                            result = 移Tray上下气缸原位输出();
                            if (result == "Success")
                            {
                                PS抓盘手臂放空盘到Auto1流程.StepID = 535;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add("1010001"); //C_空盘手臂_升降--气缸传感器没有检测到OFF
                                RunLog($"[Code]1010001,[Msg]移Tray上下气缸原位输出失败！" + result, LogType.AlarmLog);
                            }
                            break;

                        case 535:
                            PS抓盘手臂放空盘到Auto1流程.StepName = "检测Auto1是否有空盘";
                            RunLog($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
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
                                        Listcode.Add("4020009");
                                        RunLog("[Code]4020009,[Msg]Auto1前对射感应到无Tray盘", LogType.AlarmLog);
                                    }
                                    if (value2 != "1")
                                    {
                                        Listcode.Add("4020017");
                                        RunLog("[Code]4020017,[Msg]Auto1后对射感应到Tray盘放歪", LogType.AlarmLog);
                                    }
                                }
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;

                                if (result != "Success")
                                {
                                    Listcode.Add("4020009");
                                    RunLog("[Code]4020009,[Msg]读取Auto1前Tray存在感应失败!" + result, LogType.AlarmLog);
                                }
                                if (result2 != "Success")
                                {
                                    Listcode.Add("4020017");
                                    RunLog("[Code]4020017,[Msg]读取Auto1后Tray放歪感应失败!" + result2, LogType.AlarmLog);
                                }
                            }
                            break;

                        case 540:
                            PS抓盘手臂放空盘到Auto1流程.StepName = $"Auto1_Tray前侧定位气缸到位输出";
                            RunLog($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
                            result = Auto1_Tray前侧定位气缸到位输出();
                            if (result == "Success")
                            {
                                PS抓盘手臂放空盘到Auto1流程.StepID = 545;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Auto1_Tray前侧定位气缸到位输出失败！" + result);
                            }
                            break;

                        case 545:
                            PS抓盘手臂放空盘到Auto1流程.StepName = "放Tary盘轴回到原点";
                            RunLog($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
                            result = 放Tary盘轴移动(0);
                            if (result == "Success")
                            {
                                PS抓盘手臂放空盘到Auto1流程.StepID = 550;
                            }
                            else
                            {
                                Listcode.Add("2025007");
                                RunLog($"[Code]2025007,[Msg]放Tary盘轴回到原点失败!" + result, LogType.AlarmLog);
                                M18_放Tary盘轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 550:
                            PS抓盘手臂放空盘到Auto1流程.StepName = "判断Auto1料仓是否满盘";
                            RunLog($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
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
                                Auto1料仓打开标记 = false;
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add("4020001");//自动盘1区--盘放满(满料未拿走)
                                RunLog($"[Code]4020001,[Msg]Auto1料仓满料！", LogType.AlarmLog);
                                PS抓盘手臂放空盘到Auto1流程.StepID = 555;
                            }
                            else Thread.Sleep(10);
                            break;

                        case 555:
                            PS抓盘手臂放空盘到Auto1流程.StepName = "等待料仓料拿走";
                            if (Auto1料仓打开标记) //料仓被打开
                            {
                                抓盘手臂取Empty空盘标记 = 2;
                                PS抓盘手臂放空盘到Auto1流程.StepID = 560;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add("4020001");//自动盘1区--盘放满(满料未拿走)
                                RunLog($"[Code]4020001,[Msg]Auto1料仓满料！", LogType.AlarmLog);
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
                            Task<string> auto1u = Task.Run(() => { return Auto1顶盘轴移动(初始点位.Auto1_初始位); });
                            if (trayx.Result == "Success" && auto1u.Result == "Success")
                            {
                                PS抓盘手臂放空盘到Auto1流程.StepID = 570;
                            }
                            else
                            {
                                if (trayx.Result != "Success")
                                {
                                    Listcode.Add("2025007");
                                    RunLog($"[Code]2025007,[Msg]放Tary盘轴移动到Auto1等待位失败!" + result, LogType.AlarmLog);
                                    M18_放Tary盘轴ErrorMsg = result;
                                }
                                if (auto1u.Result != "Success")
                                {
                                    RunLog($"Auto1顶盘轴移动到等待位失败！" + trayx.Result);
                                }
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 570:
                            PS抓盘手臂放空盘到Auto1流程.StepName = "检测Auto1是否无空盘";
                            RunLog($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
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
                                        Listcode.Add("4020001");
                                        RunLog("[Code]4020001,[Msg]Auto1前对射感应到有Tray盘", LogType.AlarmLog);
                                    }
                                    if (value2 != "1")
                                    {
                                        Listcode.Add("4020001");
                                        RunLog("[Code]4020001,[Msg]Auto1后对射感应到有Tray盘", LogType.AlarmLog);
                                    }
                                }
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;

                                if (result != "Success")
                                {
                                    Listcode.Add("4020001");
                                    RunLog("[Code]4020001,[Msg]读取Auto1前Tray有盘感应失败！" + result, LogType.AlarmLog);
                                }
                                if (result2 != "Success")
                                {
                                    Listcode.Add("4020001");
                                    RunLog("[Code]4020001,[Msg]读取Auto1后Tray放歪感应失败!" + result2, LogType.AlarmLog);
                                }
                            }
                            break;


                        case 575:
                            PS抓盘手臂放空盘到Auto1流程.StepName = "放Tary盘轴移动到Auto1";
                            RunLog($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
                            result = 放Tary盘轴移动(初始点位.抓盘_Auto1_X + 点位微调.抓盘_Auto1_X);
                            if (result == "Success")
                            {
                                PS抓盘手臂放空盘到Auto1流程.StepID = 580;
                            }
                            else
                            {
                                Listcode.Add("2025007");
                                RunLog($"[Code]2025007,[Msg]放Tary盘轴移动到Auto1失败!" + result, LogType.AlarmLog);
                                M18_放Tary盘轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 580:
                            PS抓盘手臂放空盘到Auto1流程.StepName = "移Tray上下气缸到位输出";
                            RunLog($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
                            result = 移Tray上下气缸到位输出();
                            if (result == "Success")
                            {
                                PS抓盘手臂放空盘到Auto1流程.StepID = 585;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add("1010000"); //C_空盘手臂_升降--气缸传感器没有检测到ON
                                RunLog($"[Code]1010000,[Msg]移Tray上下气缸到位输出失败！" + result, LogType.AlarmLog);
                            }
                            break;

                        case 585:
                            PS抓盘手臂放空盘到Auto1流程.StepName = "移Tray夹爪气缸原位输出";
                            RunLog($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
                            result = 移Tray夹爪气缸原位输出();
                            if (result == "Success")
                            {
                                PS抓盘手臂放空盘到Auto1流程.StepID = 590;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add("1010003"); //C_空盘手臂_松开--气缸传感器没有检测到ON
                                RunLog($"[Code]1010003,[Msg]移Tray夹爪气缸原位输出失败！" + result, LogType.AlarmLog);
                            }
                            break;

                        case 590:
                            PS抓盘手臂放空盘到Auto1流程.StepName = "移Tray上下气缸原位输出";
                            RunLog($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
                            result = 移Tray上下气缸原位输出();
                            if (result == "Success")
                            {
                                PS抓盘手臂放空盘到Auto1流程.StepID = 591;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add("1010001"); //C_空盘手臂_升降--气缸传感器没有检测到OFF
                                RunLog($"[Code]1010001,[Msg]移Tray上下气缸原位输出失败！" + result, LogType.AlarmLog);
                            }
                            break;

                        case 591:
                            PS抓盘手臂放空盘到Auto1流程.StepName = "检测Auto1是否有空盘";
                            RunLog($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
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
                                        Listcode.Add("4020009");
                                        RunLog("[Code]4020009,[Msg]Auto1前对射感应到无Tray盘", LogType.AlarmLog);
                                    }
                                    if (value2 != "1")
                                    {
                                        Listcode.Add("4020017");
                                        RunLog("[Code]4020017,[Msg]Auto1后对射感应到Tray盘放歪", LogType.AlarmLog);
                                    }
                                }
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                if (result != "Success")
                                {
                                    Listcode.Add("4020009");
                                    RunLog("[Code]4020009,[Msg]读取Auto1前Tray存在感应失败!" + result, LogType.AlarmLog);
                                }
                                if (result2 != "Success")
                                {
                                    Listcode.Add("4020017");
                                    RunLog("[Code]4020017,[Msg]读取Auto1后Tray放歪感应失败!" + result2, LogType.AlarmLog);
                                }
                            }
                            break;

                        case 592:
                            PS抓盘手臂放空盘到Auto1流程.StepName = $"Auto1_Tray前侧定位气缸到位输出";
                            RunLog($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
                            result = Auto1_Tray前侧定位气缸到位输出();
                            if (result == "Success")
                            {
                                PS抓盘手臂放空盘到Auto1流程.StepID = 595;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Auto1_Tray前侧定位气缸到位输出失败！" + result);
                            }
                            break;

                        case 595:
                            PS抓盘手臂放空盘到Auto1流程.StepName = "放Tary盘轴回到原点";
                            RunLog($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
                            result = 放Tary盘轴移动(0);
                            if (result == "Success")
                            {
                                PS抓盘手臂放空盘到Auto1流程.StepID = 599;
                            }
                            else
                            {
                                Listcode.Add("2025007");
                                RunLog($"[Code]2025007,[Msg]放Tary盘轴回到原点失败!" + result, LogType.AlarmLog);
                                M18_放Tary盘轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 599:
                            抓盘手臂放空盘到Auto1标记 = 1;
                            PS抓盘手臂放空盘到Auto1流程.StepName = $"抓盘手臂放空盘到Auto1流程完成";
                            RunLog($"PS抓盘手臂放空盘到Auto1流程.StepID={PS抓盘手臂放空盘到Auto1流程.StepID}->{PS抓盘手臂放空盘到Auto1流程.StepName}");
                            PS抓盘手臂放空盘到Auto1流程.Processing = false;
                            PS抓盘手臂放空盘到Auto1流程.StepID = 500;
                            break;

                        default:
                            RunLog(("PS抓盘手臂放空盘到Auto1流程.StepID =" + PS抓盘手臂放空盘到Auto1流程.StepID.ToString() + "对应流程不存在"));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    string errormsg = $"PS抓盘手臂放空盘到Auto1流程({PS抓盘手臂放空盘到Auto1流程.StepID})运行异常,详细信息:" + ex.Message;
                    RunLog(errormsg, LogType.AlarmLog);
                    系统未定义错误ErrorMsg = errormsg;
                    ProgramPause = true;
                    IsPause = true;
                    Listcode.Add("1400070");
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
                try
                {
                    Thread.Sleep(10);

                    if (APPmaster.IsForecePause)
                    {
                        PS抓盘手臂放空盘到Auto2流程.State = 1;
                        continue;
                    }

                    if (APPmaster.IsPause)
                    {
                        continue;
                    }
                    switch (PS抓盘手臂放空盘到Auto2流程.StepID)
                    {
                        case 500:
                            PS抓盘手臂放空盘到Auto2流程.StepName = "路径规划";
                            PS抓盘手臂放空盘到Auto2流程.Condition = 抓盘手臂放空盘到Auto2标记 == 2
                                                        && 抓盘手臂取Empty空盘标记 == 1;

                            PS抓盘手臂放空盘到Auto2流程.Permit = !PS抓盘手臂放空盘到Auto2流程.Processing
                                                        && !PS抓盘手臂取Empty空盘流程.Processing;

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
                            RunLog($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
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
                                    Listcode.Add($"1010001"); //C_空盘手臂_升降--气缸传感器没有检测到OFF
                                    RunLog($"[Code]1010001,[Msg]移Tray上下气缸未在原位", LogType.AlarmLog);
                                }
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add($"1010001"); //C_空盘手臂_升降--气缸传感器没有检测到OFF
                                RunLog($"[Code]1010001,[Msg]移Tray上下气缸原位输入读取失败！" + result, LogType.AlarmLog);
                            }
                            break;

                        case 510:
                            PS抓盘手臂放空盘到Auto2流程.StepName = "移Tray夹爪气缸到位输入读取";
                            RunLog($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
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
                                    Listcode.Add($"1010002"); //C_空盘手臂_夹紧--气缸传感器没有检测到ON
                                    RunLog($"[Code]1010002,[Msg]移Tray夹爪气缸未夹紧", LogType.AlarmLog);
                                }
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add($"1010002"); //C_空盘手臂_夹紧--气缸传感器没有检测到ON
                                RunLog($"[Code]1010002,[Msg]移Tray夹爪气缸原位输入读取失败！" + result, LogType.AlarmLog);
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
                            RunLog($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
                            result = 放Tary盘轴移动(初始点位.抓盘_Auto2_X - 200);//859.35
                            if (result == "Success")
                            {
                                PS抓盘手臂放空盘到Auto2流程.StepID = 513;
                            }
                            else
                            {
                                Listcode.Add("2025007");
                                RunLog($"[Code]2025007,[Msg]放Tary盘轴移动到Auto2等待位失败!" + result, LogType.AlarmLog);
                                M18_放Tary盘轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 513:
                            if (Auto2上顶标记 == 1)
                            {
                                PS抓盘手臂放空盘到Auto2流程.StepID = 514;
                            }
                            else
                            {
                                Thread.Sleep(10);
                            }
                            break;

                        case 514:
                            PS抓盘手臂放空盘到Auto2流程.StepName = "检测Auto2是否有空盘";
                            RunLog($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
                            result = Auto2_前Tray存在感应(out value);
                            result2 = Auto2_后Tray放歪感应(out string value3);
                            if (result == "Success" && result2 == "Success")
                            {
                                if (value == "1" && value3 == "1") //返回0是挡到感应器,1是没有挡到感应器
                                {
                                    PS抓盘手臂放空盘到Auto2流程.StepID = 515;
                                }
                                else //没有挡到感应器或者放歪感应器被挡到说明没有放盘成功，需重新放盘
                                {
                                    APPmaster.ProgramPause = true;
                                    IsPause = true;

                                    if (value != "1")
                                    {
                                        Listcode.Add("4021001");
                                        RunLog("[Code]4021001,[Msg]Auto2前对射感应到有Tray盘", LogType.AlarmLog);
                                    }
                                    if (value3 != "1")
                                    {
                                        Listcode.Add("4021001");
                                        RunLog("[Code]4021001,[Msg]Auto2后对射感应到有Tray盘", LogType.AlarmLog);
                                    }
                                }
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                if (result != "Success")
                                {
                                    Listcode.Add("4021001");
                                    RunLog("[Code]4021001,[Msg]读取Auto2前Tray存在感应失败!" + result, LogType.AlarmLog);
                                }
                                if (result2 != "Success")
                                {
                                    Listcode.Add("4021001");
                                    RunLog("[Code]4021001,[Msg]读取Auto2后Tray放歪感应失败!" + result2, LogType.AlarmLog);
                                }
                            }
                            break;

                        case 515:
                            PS抓盘手臂放空盘到Auto2流程.StepName = "放Tary盘轴移动到Auto2";
                            RunLog($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
                            result = 放Tary盘轴移动(初始点位.抓盘_Auto2_X + 点位微调.抓盘_Auto2_X);
                            if (result == "Success")
                            {
                                PS抓盘手臂放空盘到Auto2流程.StepID = 520;
                            }
                            else
                            {
                                Listcode.Add("2025007");
                                RunLog($"[Code]2025007,[Msg]放Tary盘轴移动到Auto2失败!" + result, LogType.AlarmLog);
                                M18_放Tary盘轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 520:
                            PS抓盘手臂放空盘到Auto2流程.StepName = "移Tray上下气缸到位输出";
                            RunLog($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
                            result = 移Tray上下气缸到位输出();
                            if (result == "Success")
                            {
                                PS抓盘手臂放空盘到Auto2流程.StepID = 525;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add("1010000"); //C_空盘手臂_升降--气缸传感器没有检测到ON
                                RunLog($"[Code]1010000,[Msg]移移Tray上下气缸到位输出失败！" + result, LogType.AlarmLog);
                            }
                            break;

                        case 525:
                            PS抓盘手臂放空盘到Auto2流程.StepName = "移Tray夹爪气缸原位输出";
                            RunLog($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
                            result = 移Tray夹爪气缸原位输出();
                            if (result == "Success")
                            {
                                PS抓盘手臂放空盘到Auto2流程.StepID = 530;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add("1010003"); //C_空盘手臂_松开--气缸传感器没有检测到ON
                                RunLog($"[Code]1010003,[Msg]移Tray夹爪气缸原位输出失败！" + result, LogType.AlarmLog);
                            }
                            break;

                        case 530:
                            PS抓盘手臂放空盘到Auto2流程.StepName = "移Tray上下气缸原位输出";
                            RunLog($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
                            result = 移Tray上下气缸原位输出();
                            if (result == "Success")
                            {
                                PS抓盘手臂放空盘到Auto2流程.StepID = 535;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add("1010001"); //C_空盘手臂_升降--气缸传感器没有检测到OFF
                                RunLog($"[Code]1010001,[Msg]移Tray上下气缸原位输出失败！" + result, LogType.AlarmLog);
                            }
                            break;

                        case 535:
                            PS抓盘手臂放空盘到Auto2流程.StepName = "检测Auto2是否有空盘";
                            RunLog($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
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
                                        Listcode.Add("4021009");
                                        RunLog("[Code]4021009,[Msg]Auto2前对射感应到无Tray盘", LogType.AlarmLog);
                                    }
                                    if (value2 != "1")
                                    {
                                        Listcode.Add("4021017");
                                        RunLog("[Code]4021017,[Msg]Auto2后感应到Tray盘放歪", LogType.AlarmLog);
                                    }
                                }
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;

                                if (result != "Success")
                                {
                                    Listcode.Add("4021009");
                                    RunLog("[Code]4021009,[Msg]读取Auto2前Tray存在感应失败!" + result, LogType.AlarmLog);
                                }
                                if (result2 != "Success")
                                {
                                    Listcode.Add("4021017");
                                    RunLog("[Code]4021017,[Msg]读取Auto2后Tray放歪感应失败!" + result2, LogType.AlarmLog);
                                }
                            }
                            break;

                        case 540:
                            PS抓盘手臂放空盘到Auto2流程.StepName = $"Auto2_Tray前侧定位气缸到位输出";
                            RunLog($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
                            result = Auto2_Tray前侧定位气缸到位输出();
                            if (result == "Success")
                            {
                                PS抓盘手臂放空盘到Auto2流程.StepID = 545;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Auto2_Tray前侧定位气缸到位输出失败！" + result);
                            }
                            break;

                        case 545:
                            PS抓盘手臂放空盘到Auto2流程.StepName = "放Tary盘轴回到原点";
                            RunLog($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
                            result = 放Tary盘轴移动(0);
                            if (result == "Success")
                            {
                                PS抓盘手臂放空盘到Auto2流程.StepID = 550;
                            }
                            else
                            {
                                Listcode.Add("2025007");
                                RunLog($"[Code]2025007,[Msg]放Tary盘轴回到原点失败!" + result, LogType.AlarmLog);
                                M18_放Tary盘轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 550:
                            PS抓盘手臂放空盘到Auto2流程.StepName = "判断Auto2料仓是否满盘";
                            RunLog($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
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
                                Auto2料仓打开标记 = false;
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add("4021001");//自动盘2区--盘放满(满料未拿走)
                                RunLog($"[Code]4021001,[Msg]Auto2料仓满料", LogType.AlarmLog);
                                PS抓盘手臂放空盘到Auto2流程.StepID = 555;
                            }
                            else Thread.Sleep(10);
                            break;

                        case 555:
                            PS抓盘手臂放空盘到Auto2流程.StepName = "等待料仓料拿走";
                            if (Auto2料仓打开标记) //料仓被打开
                            {
                                抓盘手臂取Empty空盘标记 = 2;
                                PS抓盘手臂放空盘到Auto2流程.StepID = 560;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add("4021001");//自动盘2区--盘放满(满料未拿走)
                                RunLog($"[Code]4021001,[Msg]Auto1料仓满料", LogType.AlarmLog);
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
                            Task<string> auto2u = Task.Run(() => { return Auto2顶盘轴移动(初始点位.Auto2_初始位); });
                            if (trayx.Result == "Success" && auto2u.Result == "Success")
                            {
                                PS抓盘手臂放空盘到Auto2流程.StepID = 570;
                            }
                            else
                            {
                                if (trayx.Result != "Success")
                                {
                                    Listcode.Add("2025007");
                                    RunLog($"[Code]2025007,[Msg]放Tary盘轴移动到Auto2等待位失败!" + trayx.Result, LogType.AlarmLog);
                                    M18_放Tary盘轴ErrorMsg = trayx.Result;
                                    ProgramPause = true;
                                    IsPause = true;
                                }
                                if (auto2u.Result != "Success")
                                {
                                    RunLog($"Auto2顶盘轴移动到等待位失败！" + trayx.Result);
                                }
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 570:
                            PS抓盘手臂放空盘到Auto2流程.StepName = "检测Auto2是否无空盘";
                            RunLog($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
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
                                        Listcode.Add("4021001");
                                        RunLog("[Code]4021001,[Msg]Auto2前对射感应到有Tray盘", LogType.AlarmLog);
                                    }
                                    if (value2 != "1")
                                    {
                                        Listcode.Add("4021001");
                                        RunLog("[Code]4021001,[Msg]Auto2后对射感应到有Tray盘", LogType.AlarmLog);
                                    }
                                }
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;

                                if (result != "Success")
                                {
                                    Listcode.Add("4021001");
                                    RunLog("[Code]4021001,[Msg]读取Auto2前Tray存在感应失败!" + result, LogType.AlarmLog);
                                }
                                if (result2 != "Success")
                                {
                                    Listcode.Add("4021001");
                                    RunLog("[Code]4021001,[Msg]读取Auto2后Tray放歪感应失败!" + result2, LogType.AlarmLog);
                                }
                            }
                            break;

                        case 575:
                            PS抓盘手臂放空盘到Auto2流程.StepName = "放Tary盘轴移动到Auto2";
                            RunLog($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
                            result = 放Tary盘轴移动(初始点位.抓盘_Auto2_X + 点位微调.抓盘_Auto2_X);
                            if (result == "Success")
                            {
                                PS抓盘手臂放空盘到Auto2流程.StepID = 580;
                            }
                            else
                            {
                                Listcode.Add("2025007");
                                RunLog($"[Code]2025007,[Msg]放Tary盘轴移动到Auto2失败!" + result, LogType.AlarmLog);
                                M18_放Tary盘轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 580:
                            PS抓盘手臂放空盘到Auto2流程.StepName = "移Tray上下气缸到位输出";
                            RunLog($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
                            result = 移Tray上下气缸到位输出();
                            if (result == "Success")
                            {
                                PS抓盘手臂放空盘到Auto2流程.StepID = 585;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add("1010000"); //C_空盘手臂_升降--气缸传感器没有检测到ON
                                RunLog($"[Code]1010000,[Msg]移Tray上下气缸到位输出失败！" + result, LogType.AlarmLog);
                            }
                            break;

                        case 585:
                            PS抓盘手臂放空盘到Auto2流程.StepName = "移Tray夹爪气缸原位输出";
                            RunLog($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
                            result = 移Tray夹爪气缸原位输出();
                            if (result == "Success")
                            {
                                PS抓盘手臂放空盘到Auto2流程.StepID = 590;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add("1010003"); //C_空盘手臂_松开--气缸传感器没有检测到ON
                                RunLog($"[Code]1010003,[Msg]移Tray夹爪气缸原位输出失败！" + result, LogType.AlarmLog);
                            }
                            break;

                        case 590:
                            PS抓盘手臂放空盘到Auto2流程.StepName = "移Tray上下气缸原位输出";
                            RunLog($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
                            result = 移Tray上下气缸原位输出();
                            if (result == "Success")
                            {
                                PS抓盘手臂放空盘到Auto2流程.StepID = 591;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                Listcode.Add("1010001"); //C_空盘手臂_升降--气缸传感器没有检测到OFF
                                RunLog($"[Code]1010001,[Msg]移Tray上下气缸原位输出失败！" + result, LogType.AlarmLog);
                            }
                            break;

                        case 591:
                            PS抓盘手臂放空盘到Auto2流程.StepName = "检测Auto2是否有空盘";
                            RunLog($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
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
                                        Listcode.Add("4021009");
                                        RunLog("[Code]4021009,[Msg]Auto2前对射感应到无Tray盘", LogType.AlarmLog);
                                    }
                                    if (value2 != "1")
                                    {
                                        Listcode.Add("4021017");
                                        RunLog("[Code]4021017,[Msg]Auto2后感应到Tray盘放歪", LogType.AlarmLog);
                                    }
                                }
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;

                                if (result != "Success")
                                {
                                    Listcode.Add("4021009");
                                    RunLog("[Code]4021009,[Msg]读取Auto2前Tray存在感应失败!" + result, LogType.AlarmLog);
                                }
                                if (result2 != "Success")
                                {
                                    Listcode.Add("4021017");
                                    RunLog("[Code]4021017,[Msg]读取Auto2后Tray放歪感应失败!" + result2, LogType.AlarmLog);
                                }
                            }
                            break;

                        case 592:
                            PS抓盘手臂放空盘到Auto2流程.StepName = $"Auto2_Tray前侧定位气缸到位输出";
                            RunLog($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
                            result = Auto2_Tray前侧定位气缸到位输出();
                            if (result == "Success")
                            {
                                PS抓盘手臂放空盘到Auto2流程.StepID = 595;
                            }
                            else
                            {
                                APPmaster.ProgramPause = true;
                                IsPause = true;
                                RunLog($"Auto2_Tray前侧定位气缸到位输出失败！" + result);
                            }
                            break;

                        case 595:
                            PS抓盘手臂放空盘到Auto2流程.StepName = "放Tary盘轴回到原点";
                            RunLog($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
                            result = 放Tary盘轴移动(0);
                            if (result == "Success")
                            {
                                PS抓盘手臂放空盘到Auto2流程.StepID = 599;
                            }
                            else
                            {
                                Listcode.Add("2025007");
                                RunLog($"[Code]2025007,[Msg]放Tary盘轴回到原点失败!" + result, LogType.AlarmLog);
                                M18_放Tary盘轴ErrorMsg = result;
                                ProgramPause = true;
                                IsPause = true;
                            }
                            break;

                        case 599:
                            抓盘手臂放空盘到Auto2标记 = 1;
                            PS抓盘手臂放空盘到Auto2流程.StepName = $"抓盘手臂放空盘到Auto2流程完成";
                            RunLog($"PS抓盘手臂放空盘到Auto2流程.StepID={PS抓盘手臂放空盘到Auto2流程.StepID}->{PS抓盘手臂放空盘到Auto2流程.StepName}");
                            PS抓盘手臂放空盘到Auto2流程.Processing = false;
                            PS抓盘手臂放空盘到Auto2流程.StepID = 500;
                            break;

                        default:
                            RunLog(("PS抓盘手臂放空盘到Auto2流程.StepID =" + PS抓盘手臂放空盘到Auto2流程.StepID.ToString() + "对应流程不存在"));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    string errormsg = $"PS抓盘手臂放空盘到Auto2流程({PS抓盘手臂放空盘到Auto2流程.StepID})运行异常,详细信息:" + ex.Message;
                    RunLog(errormsg, LogType.AlarmLog);
                    系统未定义错误ErrorMsg = errormsg;
                    ProgramPause = true;
                    IsPause = true;
                    Listcode.Add("1400070");
                }
            }
        }

        #endregion
    }
}
