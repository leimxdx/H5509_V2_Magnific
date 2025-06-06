using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml.Office;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Sunny.UI;
using static GTN.mc;
using static H5509_V2.APPmaster;
using Color = System.Drawing.Color;
using Control = System.Windows.Forms.Control;

namespace H5509_V2
{
    public partial class _setting : UIUserControl
    {
        private MotionEx motionEx = MotionEx.GetInstance();
        private Motion motion;

        public Handler handler = new Handler();

        private System.Timers.Timer RefreshTimer;

        public int 相对运动步长 = 1000;
        public int 绝对运动步长 = 1000;

        public Control[] GlinkDI = new Control[16];
        public Control[] GlinkDO = new Control[16];
        public Control[] GNM601DI = new Control[16];
        public Control[] GNM601DO = new Control[10];
        public Control[] GNM601Home = new Control[6];
        public Control[] GNM601LimitN = new Control[6];
        public Control[] GNM601LimitP = new Control[6];

        public Action 刷新分BIN信息;
        public Action 刷新温度信息;

        public _setting()
        {
            InitializeComponent();

            motion = motionEx.Motion;

            RefreshTimer = new System.Timers.Timer() { Interval = 500 };
            RefreshTimer.Elapsed += RefreshAxisStatusElapsedTick;

            txt_home_mode.Items.Add("限位+Home回零");
            txt_home_mode.Items.Add("Home回零");
            txt_home_mode.SelectedIndex = 0;

            cobox_glink200.SelectedIndex = 0;
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;

            for (int i = 0; i < APPmaster.IOList.Count; i++)
            {
                string name = APPmaster.IOList[i].IO编号 + "_" + APPmaster.IOList[i].IO名称;
                if (APPmaster.IOList[i].IO编号.Contains("X")) comboBox3.Items.Add(name);
                else comboBox4.Items.Add(name);
            }
            if (comboBox3.Items.Count > 0) comboBox3.SelectedIndex = 0;
            if (comboBox4.Items.Count > 0) comboBox4.SelectedIndex = 0;

            for (int i = 0; i < APPmaster.AxisInfoList.Count; i++)
            {
                cobox_axisname.Items.Add(APPmaster.AxisInfoList[i].轴名称);
            }
            cobox_axisname.SelectedIndex = 0;

            GlinkDI = new Control[16] { label154, label153, label155, label152, label150, label149, label151, label148, label144, label143, label145, label142, label140, label139, label141, label138 };
            GlinkDO = new Control[16] { label135, label134, label136, label133, label131, label130, label132, label129, label126, label125, label127, label124, label122, label121, label123, label120 };
            GNM601DI = new Control[16] { label24, label23, label25, label22, label28, label27, label29, label26, label39, label38, label40, label37, label35, label34, label36, label160 };
            GNM601DO = new Control[10] { label115, label114, label116, label113, label111, label110, label112, label109, label106, label105 };

            GNM601Home = new Control[6] { label158, label162, label165, label174, label171, label168 };
            GNM601LimitN = new Control[6] { label159, label163, label166, label175, label172, label169 };
            GNM601LimitP = new Control[6] { label161, label164, label167, label176, label173, label170 };

            uiComboBox11.DataSource = SerialPort.GetPortNames();
            uiComboBox12.DataSource = new string[] { "9600", "19200", "38400", "57600", "115200" };
            uiComboBox14.DataSource = new string[] { "None", "Odd", "Even" };
            uiComboBox13.DataSource = new string[] { "7", "8" };
            uiComboBox15.DataSource = new string[] { "1", "2" };
        }

        public string GetAxisStatus(short core, short axis, ref bool[] gNME601AxisStatus)
        {
            string result = motion.GetAxisStatus(core, axis, out int sts);
            if (result == "Success")
            {
                //驱动器报警标志 控制轴连接的驱动器报警时置 1
                if ((sts & (1 << 1)) == 0) gNME601AxisStatus[1] = false;
                else gNME601AxisStatus[1] = true;

                //跟随误差越限标志 控制轴规划位置和实际位置的误差大于设定极限时置 1
                if ((sts & (1 << 4)) == 0) gNME601AxisStatus[4] = false;
                else gNME601AxisStatus[4] = true;

                //正限位触发标志 正限位开关电平状态为限位触发电平时置 1 规划位置大于正向软限位时置 1
                if ((sts & (1 << 5)) == 0) gNME601AxisStatus[5] = false;
                else gNME601AxisStatus[5] = true;

                //负限位触发标志 负限位开关电平状态为限位触发电平时置 1 规划位置小于负向软限位时置 1
                if ((sts & (1 << 6)) == 0) gNME601AxisStatus[6] = false;
                else gNME601AxisStatus[6] = true;

                //IO 平滑停止触发标志 如果轴设置了平滑停止 IO，当其输入为触发电平时置 1，并自动平滑停止该轴
                if ((sts & (1 << 7)) == 0) gNME601AxisStatus[7] = false;
                else gNME601AxisStatus[7] = true;

                //IO 急停触发标志
                //如果轴设置了急停 IO，当其输入为触发电平时置 1，并自动急停该轴
                if ((sts & (1 << 8)) == 0) gNME601AxisStatus[8] = false;
                else gNME601AxisStatus[8] = true;

                //电机使能标志 电机使能时置 1
                if ((sts & (1 << 9)) == 0) gNME601AxisStatus[9] = false;
                else gNME601AxisStatus[9] = true;

                //规划运动标志 规划器运动时置 1
                if ((sts & (1 << 10)) == 0) gNME601AxisStatus[10] = false;
                else gNME601AxisStatus[10] = true;

                //电机到位标志
                //规划器静止，规划位置和实际位置的误差小于设定误差带，并且在误差带内保持设定
                //时间后，置1起到位标志
                if ((sts & (1 << 11)) == 0) gNME601AxisStatus[11] = false;
                else gNME601AxisStatus[11] = true;
            }
            return result;
        }

        /// <summary>
        /// 刷新轴状态
        /// </summary>
        private void RefreshAxistatus(bool[] axistatus)
        {
            //轴报警
            if (axistatus[1]) label1333.BackColor = Color.Red;
            else label1333.BackColor = Color.LimeGreen;
            //正极限
            if (axistatus[5]) label1330.BackColor = Color.Red;
            else label1330.BackColor = Color.LimeGreen;
            //负极限
            if (axistatus[6]) label1324.BackColor = Color.Red;
            else label1324.BackColor = Color.LimeGreen;
            //使能状态
            if (axistatus[9])
            {
                if (!axistatus[10]) //轴正在运动则运动按钮处于不可点击状态
                {
                    if (uiChk_relative.Checked)
                    {
                        button1.Enabled = true;//正向运动按钮
                        button39.Enabled = true;//反向运动按钮
                        button4.Enabled = false; //绝对运动按钮
                    }
                    else
                    {
                        button1.Enabled = false;//正向运动按钮
                        button39.Enabled = false;//反向运动按钮
                        button4.Enabled = true; //绝对运动按钮
                    }
                }
                else
                {
                    button1.Enabled = false;//正向运动按钮
                    button39.Enabled = false;//反向运动按钮
                    button4.Enabled = false; //绝对运动按钮
                }
                button145.Enabled = true;//停止运动按钮
                //button156.Enabled = true;//回零按钮
                button15.Enabled = true; //Jog+
                button3.Enabled = true; //Jog-
                //button17.Enabled = true;//全部回零按钮
                //button155.Enabled = true;//停止回零按钮
                button146.Text = "使能关闭";
                label1582.BackColor = Color.LimeGreen; //使能状态
                uiChk_relative.Enabled = true;
                uiChk_absolute.Enabled = true;
            }
            else
            {
                button1.Enabled = false;//正向运动按钮
                button39.Enabled = false;//反向运动按钮
                button4.Enabled = false; //绝对运动按钮
                button145.Enabled = false;//停止运动按钮
                //button156.Enabled = false;//回零按钮
                button15.Enabled = false; //Jog+
                button3.Enabled = false; //Jog-
                //button17.Enabled = false;//全部回零按钮
                //button155.Enabled = false;//停止回零按钮
                button146.Text = "使能开启";
                label1582.BackColor = Color.Silver; //使能状态
                uiChk_relative.Enabled = false;
                uiChk_absolute.Enabled = false;
            }
            //运动状态
            if (axistatus[10])
            {
                label1401.BackColor = Color.LimeGreen;

            }
            else
            {
                label1401.BackColor = Color.Silver;
            }
        }

        private void RefreshAxisHomeStatus(short core, short axis)
        {
            string result = motion.GNM601_GetDiBit(core, GTN.mc.MC_HOME, axis, out short value);
            if (result == "Success")
            {
                label1327.BackColor = value == 0 ? Color.Red : Color.LimeGreen; //0处于触发状态(原点常闭), 1处于未触发状态,挡住了原点
            }
        }

        private void RefreshGlink200IO()
        {
            short glink = Convert.ToInt16(cobox_glink200.Text);
            motion.GLink_GetExtDi(1, (short)(((glink - 1) * 16) + 1), out string value1);
            for (int i = 0; i < GlinkDI.Length; i++)
            {
                if (value1[i] == '1') GlinkDI[i].BackColor = Color.LimeGreen;
                else GlinkDI[i].BackColor = Color.Silver;
            }

            motion.GLink_GetExtDO(1, (short)(((glink - 1) * 16) + 1), out string value2);
            for (int i = 0; i < GlinkDO.Length; i++)
            {
                if (value2[i] == '1') GlinkDO[i].BackColor = Color.LimeGreen;
                else GlinkDO[i].BackColor = Color.Silver;
            }
        }

        private void RefreshGNM601IO()
        {
            if (uiChk_GNM1.Checked || uiChk_GNM2.Checked)
            {
                int j = uiChk_GNM1.Checked ? 0 : 16;
                motion.GNM601_GetDi(1, GTN.mc.MC_GPI, out string value);
                for (int i = 0; i + j < value.Length && i < GNM601DI.Length; i++)
                {
                    if (value[i + j] == '0') GNM601DI[i].BackColor = Color.LimeGreen;
                    else GNM601DI[i].BackColor = Color.Silver;
                }

                j = uiChk_GNM1.Checked ? 0 : 6;
                motion.GNM601_GetDi(1, GTN.mc.MC_HOME, out value);
                for (int i = 0; i + j < value.Length && i < GNM601Home.Length; i++)
                {
                    if (value[i + j] == '0') GNM601Home[i].BackColor = Color.LimeGreen;
                    else GNM601Home[i].BackColor = Color.Silver;
                }

                motion.GNM601_GetDi(1, GTN.mc.MC_LIMIT_NEGATIVE, out value);
                for (int i = 0; i + j < value.Length && i < GNM601LimitN.Length; i++)
                {
                    if (value[i + j] == '1') GNM601LimitN[i].BackColor = Color.LimeGreen;
                    else GNM601LimitN[i].BackColor = Color.Silver;
                }

                motion.GNM601_GetDi(1, GTN.mc.MC_LIMIT_POSITIVE, out value);
                for (int i = 0; i + j < value.Length && i < GNM601LimitP.Length; i++)
                {
                    if (value[i + j] == '1') GNM601LimitP[i].BackColor = Color.LimeGreen;
                    else GNM601LimitP[i].BackColor = Color.Silver;
                }

                j = uiChk_GNM1.Checked ? 0 : 10;
                motion.GNM601_GetDo(1, GTN.mc.MC_GPO, out value);
                for (int i = 0; i + j < value.Length && i < GNM601DO.Length; i++)
                {
                    if (value[i + j] == '1') GNM601DO[i].BackColor = Color.LimeGreen;
                    else GNM601DO[i].BackColor = Color.Silver;
                }
            }
            else if (uiChk_GNM3.Checked || uiChk_GNM4.Checked)
            {
                int j = uiChk_GNM3.Checked ? 0 : 16;
                motion.GNM601_GetDi(2, GTN.mc.MC_GPI, out string value);
                for (int i = 0; i + j < value.Length && i < GNM601DI.Length; i++)
                {
                    if (value[i + j] == '0') GNM601DI[i].BackColor = Color.LimeGreen;
                    else GNM601DI[i].BackColor = Color.Silver;
                }

                j = uiChk_GNM3.Checked ? 0 : 6;
                motion.GNM601_GetDi(2, GTN.mc.MC_HOME, out value);
                for (int i = 0; i + j < value.Length && i < GNM601Home.Length; i++)
                {
                    if (value[i + j] == '0') GNM601Home[i].BackColor = Color.LimeGreen;
                    else GNM601Home[i].BackColor = Color.Silver;
                }

                motion.GNM601_GetDi(2, GTN.mc.MC_LIMIT_NEGATIVE, out value);
                for (int i = 0; i + j < value.Length && i < GNM601LimitN.Length; i++)
                {
                    if (value[i + j] == '1') GNM601LimitN[i].BackColor = Color.LimeGreen;
                    else GNM601LimitN[i].BackColor = Color.Silver;
                }

                motion.GNM601_GetDi(2, GTN.mc.MC_LIMIT_POSITIVE, out value);
                for (int i = 0; i + j < value.Length && i < GNM601LimitP.Length; i++)
                {
                    if (value[i + j] == '1') GNM601LimitP[i].BackColor = Color.LimeGreen;
                    else GNM601LimitP[i].BackColor = Color.Silver;
                }

                j = uiChk_GNM3.Checked ? 0 : 10;
                motion.GNM601_GetDo(2, GTN.mc.MC_GPO, out value);
                for (int i = 0; i + j < value.Length && i < GNM601DO.Length; i++)
                {
                    if (value[i + j] == '1') GNM601DO[i].BackColor = Color.LimeGreen;
                    else GNM601DO[i].BackColor = Color.Silver;
                }
            }
        }

        private void UpdateState()
        {
            APPmaster.GetAxisCore(cobox_axisname.Text, out short core, out short axis);

            if (motion.GetCmdPrfPos(core, axis, out int pPos) == "Success") textBox17.Text = pPos.ToString();
            else textBox17.Text = "获取失败"; //X轴规划位置
            if (motion.GetCmdPrfVel(core, axis, out double pPos2) == "Success")
            {
                if (pPos2 == 0) textBox16.Text = "0";
                else textBox16.Text = pPos2.ToString("F2");
            }
            else textBox16.Text = "获取失败"; //X轴规划速度
            if (motion.GetCmdEncPos(core, axis, out double pPos3) == "Success") textBox15.Text = pPos3.ToString();
            else textBox15.Text = "获取失败"; //X轴实时位置
            if (motion.GetCmdEncVel(core, axis, out double pPos4) == "Success")
            {
                if (pPos4 == 0) textBox14.Text = "0";
                else textBox14.Text = pPos4.ToString("F2");
            }
            else textBox14.Text = "获取失败"; //X轴实时速度

            bool[] axistatus = new bool[32];
            GetAxisStatus(core, axis, ref axistatus); //获取指定轴状态
            RefreshAxistatus(axistatus); //刷新轴状态

            RefreshAxisHomeStatus(core, axis);
            RefreshGlink200IO();
            RefreshGNM601IO();
        }

        private void RefreshAxisStatusElapsedTick(object sender, EventArgs eventArgs)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    UpdateState();
                }));
            }
            else UpdateState();
        }

        /// <summary>
        /// 开启读取轴信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button52_Click(object sender, EventArgs e)
        {
            if (!APPmaster.InitedOK)
            {
                MessageBox.Show("轴卡未连接！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (button52.Text == "开启读取轴信息")
            {
                RefreshTimer.Start();
                button52.Text = "取消读取轴信息";
                button146.Enabled = true;
                button152.Enabled = true;
                button2.Enabled = true;
                button90.Enabled = true; //GlinkIO按钮
                button91.Enabled = true; //GlinkIO按钮
                button92.Enabled = true; //GNM601IO按钮
                button93.Enabled = true; //GNM601IO按钮
                button156.Enabled = true;//回零按钮
                button155.Enabled = true;//停止回零按钮
            }
            else
            {
                RefreshTimer.Stop();
                button146.Enabled = false;
                button152.Enabled = false;
                button2.Enabled = false;
                button90.Enabled = false; //GlinkIO按钮
                button91.Enabled = false; //GlinkIO按钮
                button92.Enabled = false; //GNM601IO按钮
                button93.Enabled = false; //GNM601IO按钮
                button156.Enabled = false;//回零按钮
                button155.Enabled = false;//停止回零按钮
                button52.Text = "开启读取轴信息";

                button1.Enabled = false;//正向运动按钮
                button39.Enabled = false;//反向运动按钮
                button4.Enabled = false; //绝对运动按钮
                button145.Enabled = false;//停止运动按钮
                button15.Enabled = false; //Jog+
                button3.Enabled = false; //Jog-
                uiChk_relative.Enabled = false;
                uiChk_absolute.Enabled = false;
            }
        }

        private void button146_Click(object sender, EventArgs e)
        {

            APPmaster.GetAxisCore(cobox_axisname.Text, out short core, out short axis);

            if (button146.Text == "使能开启")
            {
                string result = motion.AxisOn(core, axis, true);
                if (result != "Success")
                {
                    MessageBox.Show("使能开启失败！详细信息：" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    button146.Text = "使能关闭";
                }
            }
            else
            {
                string result = motion.AxisOn(core, axis, false);
                if (result != "Success")
                {
                    MessageBox.Show("使能关闭失败！详细信息：" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    button146.Text = "使能开启";
                }
            }
        }

        /// <summary>
        /// 清除状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button152_Click(object sender, EventArgs e)
        {

            APPmaster.GetAxisCore(cobox_axisname.Text, out short core, out short axis);

            string result = motion.AxisClrSts(core, axis);
            if (result != "Success")
            {
                MessageBox.Show("清除状态失败！详细信息：" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //else writeDataHelper.RunLog("清除标志成功，详细信息：" + result.ErrorMsg);
        }

        /// <summary>
        /// 位置清零
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {

            APPmaster.GetAxisCore(cobox_axisname.Text, out short core, out short axis);

            string result = motion.AxisZeroPos(core, axis);
            if (result != "Success")
            {
                MessageBox.Show("清除状态失败！详细信息：" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //else writeDataHelper.RunLog("位置清零成功，详细信息：" + result.ErrorMsg);
        }

        private void button89_Click(object sender, EventArgs e)
        {
            bool bool1 = !飞梭后入料IC[0].Contains(true); //飞梭后入料IC第1列无料
            RunLog("!飞梭后入料IC[0]:" + bool1);
            bool1 = !飞梭后入料IC[1].Contains(true);//飞梭后入料IC第2列无料
            RunLog("!飞梭后入料IC[1]:" + bool1);
            bool1 = !飞梭前入料IC[0].Contains(true); //飞梭前入料IC第1列无料
            RunLog("!飞梭前入料IC[0]:" + bool1);
            bool1 = !飞梭前入料IC[1].Contains(true); //飞梭前入料IC第2列无料
            RunLog("!飞梭前入料IC[1]:" + bool1);
            bool1 = !飞梭后出料IC[0].Contains(true);//飞梭后出料IC第1列无料
            RunLog("!飞梭后出料IC[0]:" + bool1);
            bool1 = !飞梭后出料IC[1].Contains(true); //飞梭后出料IC第2列无料
            RunLog("!飞梭后出料IC[1]:" + bool1);
            bool1 = !飞梭前出料IC[0].Contains(true); //飞梭前出料IC第1列无料
            RunLog("!飞梭前出料IC[0]:" + bool1);
            bool1 = !飞梭前出料IC[1].Contains(true); //飞梭前出料IC第2列无料
            RunLog("!飞梭前出料IC[1]:" + bool1);
            bool1 = SiteTestState.Contains(0); //测试区无料
            RunLog("SiteTestState.Contains(0):" + bool1);
            bool1 = !SiteTestState.Contains(1); //测试区有料
            RunLog("!SiteTestState.Contains(1):" + bool1);
            bool1 = !SiteTestState.Contains(2); //测试区有料测试中
            RunLog("!SiteTestState.Contains(2):" + bool1);
            bool1 = !SiteTestState.Contains(3); //测试区有料已测试
            RunLog("!SiteTestState.Contains(3):" + bool1);
            bool1 = !SiteTestState.Contains(4); //测试区有料待取走
            RunLog("!SiteTestState.Contains(4):" + bool1);
            bool1 = !PS结批流程.Processing;
            RunLog("!PS结批流程:" + bool1);
            bool1 = !PS出料手臂吸放料流程.Processing;
            RunLog("!PS出料手臂吸放料流程:" + bool1);
            bool1 = !PS入料手臂吸放料流程.Processing;
            RunLog("!PS入料手臂吸放料流程:" + bool1);
            bool1 = !PS测试手臂取待测料流程.Processing;
            RunLog("!PS测试手臂取待测料流程:" + bool1);
            bool1 = !PS测试手臂取已测料流程.Processing;
            RunLog("!PS测试手臂取已测料流程:" + bool1);
            bool1 = !PS入飞梭移动流程.Processing;
            RunLog("!PS入飞梭移动流程:" + bool1);
            bool1 = !PS空盘上顶流程.Processing;
            RunLog("!PS空盘上顶流程:" + bool1);
            bool1 = !PS空盘下降流程.Processing;
            RunLog("!PS空盘下降流程:" + bool1);
            bool1 = !PS待测盘上顶流程.Processing;
            RunLog("!PS待测盘上顶流程:" + bool1);
            bool1 = !PS手动盘1上顶流程.Processing;
            RunLog("!PS手动盘1上顶流程:" + bool1);
            bool1 = !PS手动盘2上顶流程.Processing;
            RunLog("!PS手动盘2上顶流程:" + bool1);
            bool1 = !PS抓盘手臂取Empty空盘流程.Processing;
            RunLog("!PS抓盘手臂取Empty空盘流程:" + bool1);
            bool1 = !PS抓盘手臂取Load空盘流程.Processing;
            RunLog("!PS抓盘手臂取Load空盘流程:" + bool1);
            bool1 = !PS抓盘手臂放空盘到Empty流程.Processing;
            RunLog("!PS抓盘手臂放空盘到Empty流程:" + bool1);
            bool1 = !PS抓盘手臂放空盘到Auto1流程.Processing;
            RunLog("!PS抓盘手臂放空盘到Auto1流程:" + bool1);
            bool1 = !PS抓盘手臂放空盘到Auto2流程.Processing;
            RunLog("!PS抓盘手臂放空盘到Auto2流程:" + bool1);
            //飞梭后出料IC = new bool[][] { new bool[] { false, false, false, false }, new bool[] { false, false, false, false } };
        }

        private void uiCheckBox1_Click(object sender, EventArgs e)
        {
            uiChk_absolute.Checked = !uiChk_relative.Checked;
            if (uiChk_relative.Checked)
            {
                button1.Enabled = true;
                button39.Enabled = true;
                button4.Enabled = false;
                txt_run_pos.Text = 相对运动步长.ToString();
            }
            else
            {
                button1.Enabled = false;
                button39.Enabled = false;
                button4.Enabled = true;
                txt_run_pos.Text = 绝对运动步长.ToString();
            }
        }

        private void Chk_switch1_Click(object sender, EventArgs e)
        {
            uiChk_relative.Checked = !uiChk_absolute.Checked;
            if (uiChk_absolute.Checked)
            {
                button1.Enabled = false;
                button39.Enabled = false;
                button4.Enabled = true;
                txt_run_pos.Text = 绝对运动步长.ToString();
            }
            else
            {
                button1.Enabled = true;
                button39.Enabled = true;
                button4.Enabled = false;
                txt_run_pos.Text = 相对运动步长.ToString();
            }
        }

        private void uiCheckBox4_Click(object sender, EventArgs e)
        {
            chk_home_nmovedir.Checked = !chk_home_pmovedir.Checked;
        }

        private void uiCheckBox2_Click(object sender, EventArgs e)
        {
            chk_home_falling.Checked = !chk_home_rising.Checked;
        }

        private void uiCheckBox3_Click(object sender, EventArgs e)
        {
            chk_home_rising.Checked = !chk_home_falling.Checked;
        }

        private AxisRun GetRunValue()
        {
            if (!double.TryParse(txt_run_vel.Text, out double run_vel))
            {
                txt_run_vel.Focus();
                MessageBox.Show("速度值异常，请输入数字!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            if (!double.TryParse(txt_run_acc.Text, out double run_acc))
            {
                txt_run_acc.Focus();
                MessageBox.Show("加速度值异常，请输入数字!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            if (!double.TryParse(txt_run_dec.Text, out double run_dec))
            {
                txt_run_dec.Focus();
                MessageBox.Show("减速度值异常，请输入数字!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            if (!short.TryParse(txt_run_smoothTime.Text, out short run_smoothTime))
            {
                txt_run_smoothTime.Focus();
                MessageBox.Show("平滑时间值异常，请输入数字!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            else
            {
                if (run_smoothTime > 50 || run_smoothTime < 0)
                {
                    txt_run_smoothTime.Focus();
                    MessageBox.Show("平滑时间值范围[0,50]", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }

            if (!int.TryParse(txt_run_pos.Text, out int run_pos))
            {
                txt_run_pos.Focus();
                MessageBox.Show("步长值异常，请输入整数!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            APPmaster.GetAxisCore(cobox_axisname.Text, out short core, out short axis);

            AxisRun axisRun = new AxisRun()
            {
                轴名称 = cobox_axisname.Text,
                核号 = core,
                轴号 = axis,
                目标位置 = run_pos,
                目标速度 = run_vel,
                目标加速度 = run_acc,
                目标减速度 = run_dec,
                平滑时间 = run_smoothTime,
                Jog平滑时间 = run_smoothTime,
            };

            return axisRun;
        }

        private AxisHomeInfo GetHomeValue()
        {
            short mode = 0;
            if (!int.TryParse(txt_home_offset.Text, out int home_offset))
            {
                txt_home_offset.Focus();
                MessageBox.Show("零点偏置值异常，请输入数字!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            if (!double.TryParse(txt_home_vel.Text, out double home_vel))
            {
                txt_home_vel.Focus();
                MessageBox.Show("回零速度值异常，请输入数字!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            if (!double.TryParse(txt_home_dingwei_vel.Text, out double home_dw_vel))
            {
                txt_home_dingwei_vel.Focus();
                MessageBox.Show("定位速度值异常，请输入数字!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            if (!double.TryParse(txt_home_acc.Text, out double home_acc))
            {
                txt_home_acc.Focus();
                MessageBox.Show("加速度值异常，请输入数字!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            if (!double.TryParse(txt_home_dec.Text, out double home_dec))
            {
                txt_home_dec.Focus();
                MessageBox.Show("减速度值异常，请输入数字!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            if (!int.TryParse(txt_escapeStep.Text, out int home_escapeStep))
            {
                txt_run_acc.Focus();
                MessageBox.Show("反向脱离值异常，请输入数字!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            if (!short.TryParse(txt_home_Smoothtime.Text, out short home_Smoothtime))
            {
                txt_home_Smoothtime.Focus();
                MessageBox.Show("平滑时间值异常，请输入数字!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            else
            {
                if (home_Smoothtime > 50 || home_Smoothtime < 0)
                {
                    txt_home_Smoothtime.Focus();
                    MessageBox.Show("平滑时间值范围[0,50]", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }

            if (txt_home_mode.Text == "限位+Home回零") mode = 11;
            else if (txt_home_mode.Text == "Home回零") mode = 20;

            APPmaster.GetAxisCore(cobox_axisname.Text, out short core, out short axis);

            short moveDir = (short)(chk_home_pmovedir.Checked ? 1 : -1);

            short edge = (short)(chk_home_rising.Checked ? 1 : 0);

            AxisHomeInfo axisHome = new AxisHomeInfo()
            {
                核号 = core,
                轴号 = axis,
                模式 = mode,
                回零启动方向 = moveDir,
                捕获触发沿 = edge,
                搜索速度 = home_vel,
                定位速度 = home_dw_vel,
                加速度 = home_acc,
                减速度 = home_dec,
                平滑时间 = home_Smoothtime,
                零点偏置 = home_offset,
                反向脱离步长 = home_escapeStep,
            };

            return axisHome;
        }

        private void AxisStop()
        {
            APPmaster.GetAxisCore(cobox_axisname.Text, out short core, out short axis);

            string result = motion.AxisStop(core, axis, 1);

            if (result != "Success")
            {
                MessageBox.Show("执行失败!详细信息:" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 正向运动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            AxisRun axisRun = GetRunValue();
            if (axisRun != null)
            {
                if (uiChk_relative.Checked) 相对运动步长 = axisRun.目标位置;
                else 绝对运动步长 = axisRun.目标位置;

                string result = motion.UniaxialRelativeMove(axisRun, out int prfPos);
                if (result != "Success")
                {
                    MessageBox.Show("执行失败!详细信息:" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// 反向运动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button39_Click(object sender, EventArgs e)
        {
            AxisRun axisRun = GetRunValue();
            if (axisRun != null)
            {
                if (uiChk_relative.Checked) 相对运动步长 = axisRun.目标位置;
                else 绝对运动步长 = axisRun.目标位置;

                axisRun.目标位置 = -axisRun.目标位置;

                string result = motion.UniaxialRelativeMove(axisRun, out int prfPos);
                if (result != "Success")
                {
                    MessageBox.Show("执行失败!详细信息:" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// 绝对定位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            AxisRun axisRun = GetRunValue();
            if (axisRun != null)
            {
                if (uiChk_relative.Checked) 相对运动步长 = axisRun.目标位置;
                else 绝对运动步长 = axisRun.目标位置;

                string result = motion.UniaxialAbsoluteMoveAbs(axisRun);
                if (result != "Success")
                {
                    MessageBox.Show("执行失败!详细信息:" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// 轴停止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button145_Click(object sender, EventArgs e)
        {
            AxisStop();
        }

        /// <summary>
        /// 回零
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button156_Click(object sender, EventArgs e)
        {
            AxisHomeInfo axisHome = GetHomeValue();
            if (axisHome != null)
            {
                Task.Run(() =>
                {
                    string result = 单轴回零(axisHome);
                    if (result != "Success")
                    {
                        MessageBox.Show("执行失败!详细信息:" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                });
            }
        }

        /// <summary>
        /// 停止回零
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button155_Click(object sender, EventArgs e)
        {
            AxisStop();
        }

        private void uiChk_GNM1_Click(object sender, EventArgs e)
        {
            if (uiChk_GNM1.Checked)
            {
                uiChk_GNM2.Checked = false;
                uiChk_GNM3.Checked = false;
                uiChk_GNM4.Checked = false;
            }
            else uiChk_GNM1.Checked = true;
        }

        private void uiChk_GNM2_Click(object sender, EventArgs e)
        {
            if (uiChk_GNM2.Checked)
            {
                uiChk_GNM1.Checked = false;
                uiChk_GNM3.Checked = false;
                uiChk_GNM4.Checked = false;
            }
            else uiChk_GNM2.Checked = true;
        }

        private void uiChk_GNM3_Click(object sender, EventArgs e)
        {
            if (uiChk_GNM3.Checked)
            {
                uiChk_GNM1.Checked = false;
                uiChk_GNM2.Checked = false;
                uiChk_GNM4.Checked = false;
            }
            else uiChk_GNM3.Checked = true;
        }

        private void uiChk_GNM4_Click(object sender, EventArgs e)
        {

            if (uiChk_GNM4.Checked)
            {
                uiChk_GNM1.Checked = false;
                uiChk_GNM2.Checked = false;
                uiChk_GNM3.Checked = false;
            }
            else uiChk_GNM4.Checked = true;
        }

        /// <summary>
        /// Jog+开始运动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button15_MouseDown(object sender, MouseEventArgs e)
        {
            AxisRun axisRun = GetRunValue();
            if (axisRun != null)
            {
                string result = motion.JogMove(axisRun);
                if (result != "Success")
                {
                    MessageBox.Show("执行失败!详细信息:" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Jog+停止运动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button15_MouseUp(object sender, MouseEventArgs e)
        {
            AxisStop();
        }

        /// <summary>
        /// Jog-开始运动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_MouseDown(object sender, MouseEventArgs e)
        {
            AxisRun axisRun = GetRunValue();
            if (axisRun != null)
            {
                axisRun.目标速度 = -axisRun.目标速度;
                string result = motion.JogMove(axisRun);
                if (result != "Success")
                {
                    MessageBox.Show("执行失败!详细信息:" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Jog-停止运动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_MouseUp(object sender, MouseEventArgs e)
        {
            AxisStop();
        }

        private void GlinkOutSignal(bool value)
        {
            short mode = (short)(Convert.ToInt16(cobox_glink200.Text) - 1);
            short i = (short)(Convert.ToInt16(comboBox1.Text) + 1);
            string result = motion.GLink_SetExtDoBit(1, (short)(mode * 16 + i), value);
            if (result != "Success")
            {
                MessageBox.Show("执行失败!详细信息:" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button90_Click(object sender, EventArgs e)
        {
            GlinkOutSignal(true);
        }

        private void button91_Click(object sender, EventArgs e)
        {
            GlinkOutSignal(false);
        }

        private void GNM601OutSignal(bool value)
        {
            if (uiChk_GNM1.Checked || uiChk_GNM2.Checked)
            {
                short i = (short)(uiChk_GNM1.Checked ? 0 : 10);
                short j = (short)(Convert.ToInt16(comboBox2.Text) + 1);
                string result = motion.GNM601_SetDoBit(1, GTN.mc.MC_GPO, (short)(i + j), value);
                if (result != "Success")
                {
                    MessageBox.Show("执行失败!详细信息:" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else if (uiChk_GNM3.Checked || uiChk_GNM4.Checked)
            {
                short i = (short)(uiChk_GNM3.Checked ? 0 : 10);
                short j = (short)(Convert.ToInt16(comboBox2.Text) + 1);
                string result = motion.GNM601_SetDoBit(2, GTN.mc.MC_GPO, (short)(i + j), value);
                if (result != "Success")
                {
                    MessageBox.Show("执行失败!详细信息:" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button93_Click(object sender, EventArgs e)
        {
            GNM601OutSignal(true);
        }

        private void button92_Click(object sender, EventArgs e)
        {
            GNM601OutSignal(false);
        }

        private void button96_Click(object sender, EventArgs e)
        {
            string result = motionEx.GetDi(comboBox3.Text, out string iovalue);
            if (result == "Success")
            {
                textBox1.Text = iovalue;
            }
            else
            {
                MessageBox.Show("读取失败！" + result);
            }
        }

        private void button95_Click(object sender, EventArgs e)
        {
            string result = motionEx.SetDo(comboBox4.Text, true);
            if (result == "Success")
            {
                //textBox1.Text = iovalue;
            }
            else
            {
                MessageBox.Show("写入失败！" + result);
            }
        }

        private void button94_Click(object sender, EventArgs e)
        {
            string result = motionEx.SetDo(comboBox4.Text, false);
            if (result == "Success")
            {
                //textBox1.Text = iovalue;
            }
            else
            {
                MessageBox.Show("写入失败！" + result);
            }
        }

        private void button97_Click(object sender, EventArgs e)
        {
            string result = motionEx.GetDO(comboBox4.Text, out string iovalue);
            if (result == "Success")
            {
                textBox2.Text = iovalue;
            }
            else
            {
                MessageBox.Show("读取失败！" + result);
            }
        }


        public string 入料手臂Y回零()
        {
            return motionEx.AxisHome("M2_入料手臂Z2轴");
        }

        private void button78_Click(object sender, EventArgs e)
        {
            Task task = Task.Run(() =>
            {
                string result = 入料手臂Y回零();
                if (result == "Success")
                {
                    MessageBox.Show("回零成功!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else MessageBox.Show("回零失败!" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        }

        /// <summary>
        /// 等待多轴回零
        /// </summary>
        /// <param name="axisHomes"></param>
        /// <returns></returns>
        private string WaitAxisHome(List<AxisHomeInfo> axisHomes)
        {
            bool[] bools = new bool[axisHomes.Count];
            for (int i = 0; i < axisHomes.Count; i++)
            {
                string result = WaitAxisHome(axisHomes[i]);
                if (result != "Success")
                {
                    RunLog(result);
                    bools[i] = false;
                }
                else bools[i] = true;
            }
            if (bools.Contains(false))
            {
                return "Fail";
            }
            else return "Success";
        }

        /// <summary>
        /// 等待单轴回零
        /// </summary>
        /// <param name="axisHome"></param>
        /// <returns></returns>
        private string WaitAxisHome(AxisHomeInfo axisHome)
        {
            string result = motion.CommonInitedValidate();

            if (result != "Success") return result;

            try
            {
                THomeStatus tHomeSts;
                do
                {
                    short error = GTN_GetHomeStatus(axisHome.核号, axisHome.轴号, out tHomeSts);
                    motion.ErrorHandler($"GTN_GetHomeStatus({axisHome.核号}, {axisHome.轴号}, out {tHomeSts})", error);

                    error = GTN_GetSts(axisHome.核号, axisHome.轴号, out int sts, 1, out uint pClock);
                    motion.ErrorHandler($"GTN_GetSts({axisHome.核号}, {axisHome.轴号}, out {sts}, 1, out {pClock})", error);

                    if ((sts & (1 << 1)) == 1)
                    {
                        motion.AxisStop(axisHome.核号, axisHome.轴号, 1);
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

                result = motion.AxisZeroPos(axisHome.核号, axisHome.轴号);

            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return result;
        }

        private string WaitAxisMove(AxisRun axisRun, int prfPos, int type = 0, int move = 0, int pos2 = 0)
        {
            string result = motion.CommonInitedValidate();

            if (result != "Success") return result;

            try
            {
                int sts;
                bool isException = false;
                string errortxt = "";
                bool issecond = false;
                do
                {
                    short error = GTN_GetSts(axisRun.核号, axisRun.轴号, out sts, 1, out uint pClock);
                    motion.ErrorHandler($"GTN_GetSts({axisRun.核号}, {axisRun.轴号}, out {sts}, 1, out uint pClock)", error);

                    if (move == 1 && !issecond)
                    {
                        motionEx.GetCmdPrfPos(axisRun.轴名称, out int prfPos1); //规划位置
                        motionEx.GetCmdEncPos(axisRun.轴名称, out double encPos1); //实时位置

                        double posvalue1 = Math.Abs(prfPos1 - encPos1);

                        if (posvalue1 < pos2)
                        {
                            issecond = true;
                            //RunLog($"{axisRun.轴名称}修改前第一段目标速度:{axisRun.目标速度}");

                            axisRun.目标速度 = axisRun.目标速度 * 第二段速度;

                            //RunLog($"{axisRun.轴名称}修改后第二段目标速度:{axisRun.目标速度}");

                            // 设置AXIS轴的目标速度
                            error = GTN_SetVel(axisRun.核号, axisRun.轴号, axisRun.目标速度);
                            motion.ErrorHandler("GTN_SetVel", error);

                            // 启动AXIS轴的运动
                            error = GTN_Update(axisRun.核号, 1 << (axisRun.轴号 - 1));
                            motion.ErrorHandler("GTN_Update", error);
                        }
                    }

                    if (((sts & (1 << 1)) == 1))
                    {
                        GTN_Stop(axisRun.核号, 1 << (axisRun.轴号 - 1), 1);
                        errortxt = $"{axisRun.轴名称}运动被停止,轴报警";
                        isException = true;
                    }

                    Thread.Sleep(1); //减少轴卡和CPU压力
                }
                while ((sts & 0x400) != 0);

                if (isException) return errortxt;

                motion.GetCmdEncPos(axisRun.核号, axisRun.轴号, out double encPos);
                //motion.GetCmdPrfPos(axisRun.核号, axisRun.轴号, out int prfPos);

                double posvalue = Math.Abs(prfPos - encPos);
                int errorvalue = 跟随误差;

                if (axisRun.轴名称 == "M19_测试手臂Z轴")
                {
                    if (posvalue > errorvalue * 2)
                    {
                        return $"{axisRun.轴名称}规划位置({prfPos})和实际位置({encPos})的跟随误差={posvalue}>{errorvalue * 2}";
                    }
                    else if (posvalue > 300)
                    {
                        RunLog($"{axisRun.轴名称}规划位置({prfPos})和实际位置({encPos})的跟随误差={posvalue}>{300}");
                    }
                }
                else
                {
                    if (posvalue > errorvalue)
                    {
                        return $"{axisRun.轴名称}规划位置({prfPos})和实际位置({encPos})的跟随误差={posvalue}>{errorvalue}";
                    }
                    else if (posvalue > 150)
                    {
                        RunLog($"{axisRun.轴名称}规划位置({prfPos})和实际位置({encPos})的跟随误差={posvalue}>{150}");
                    }
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
        /// <returns></returns>
        private string WaitAxisMove(List<AxisRun> axisRuns, List<int> prfPos, int type = 0, int move = 0, int pos2 = 0)
        {
            string result = motion.CommonInitedValidate();

            if (result != "Success") return result;

            try
            {
                bool value = true;
                bool isException = false;
                string errortxt = "";

                int[] sts = new int[axisRuns.Count];
                bool[] issecond = new bool[axisRuns.Count];
                do
                {
                    value = false;
                    isException = false;
                    errortxt = "";

                    for (int i = 0; i < axisRuns.Count; i++)
                    {
                        short error = GTN_GetSts(axisRuns[i].核号, axisRuns[i].轴号, out sts[i], 1, out uint pClock);
                        motion.ErrorHandler($"GTN_GetSts({axisRuns[i].核号}, {axisRuns[i].轴号}, out {sts[i]}, 1, out uint pClock)", error);
                    }


                    if (move == 1 && issecond.Contains(false))
                    {
                        for (int i = 0; i < axisRuns.Count; i++)
                        {
                            if (!issecond[i]) //只要有一个Z到达减速区间则全部减速，这样才会一致
                            {
                                motionEx.GetCmdPrfPos(axisRuns[i].轴名称, out int prfPos1); //规划位置
                                motionEx.GetCmdEncPos(axisRuns[i].轴名称, out double encPos1); //实时位置
                                double posvalue1 = Math.Abs(prfPos1 - encPos1);

                                if (posvalue1 < pos2)
                                {
                                    if (axisRuns[i].轴名称.Contains("入料手臂Z") || axisRuns[i].轴名称.Contains("出料手臂Z"))
                                    {
                                        for (int se = 0; se < axisRuns.Count; se++)
                                        {
                                            RunLog($"{axisRuns[se].轴名称}修改前目标速度:" + axisRuns[se].目标速度);

                                            issecond[se] = true;
                                            axisRuns[se].目标速度 = axisRuns[se].目标速度 * 出入料Z第二段速度;

                                            RunLog($"{axisRuns[se].轴名称}修改后目标速度:" + axisRuns[se].目标速度);

                                            // 设置AXIS轴的目标速度
                                            short error = GTN_SetVel(axisRuns[se].核号, axisRuns[se].轴号, axisRuns[se].目标速度);
                                            motion.ErrorHandler("GTN_SetVel", error);
                                        }

                                        for (int se = 0; se < issecond.Length; se++)
                                        {
                                            // 启动AXIS轴的运动
                                            short error1 = GTN_Update(axisRuns[se].核号, 1 << (axisRuns[se].轴号 - 1));
                                            motion.ErrorHandler("GTN_Update", error1);
                                        }

                                        break;
                                    }
                                    else
                                    {
                                        RunLog($"{axisRuns[i].轴名称}修改前目标速度:" + axisRuns[i].目标速度);

                                        issecond[i] = true;
                                        axisRuns[i].目标速度 = axisRuns[i].目标速度 * 第二段速度;

                                        RunLog($"{axisRuns[i].轴名称}修改后目标速度:" + axisRuns[i].目标速度);

                                        // 设置AXIS轴的目标速度
                                        short error = GTN_SetVel(axisRuns[i].核号, axisRuns[i].轴号, axisRuns[i].目标速度);
                                        motion.ErrorHandler("GTN_SetVel", error);

                                        // 启动AXIS轴的运动
                                        short error1 = GTN_Update(axisRuns[i].核号, 1 << (axisRuns[i].轴号 - 1));
                                        motion.ErrorHandler("GTN_Update", error1);
                                    }
                                }
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
                    motion.GetCmdEncPos(axisRuns[i].核号, axisRuns[i].轴号, out double encPos);
                    //motion.GetCmdPrfPos(axisRuns[i].核号, axisRuns[i].轴号, out int prfPos);

                    double posvalue = Math.Abs(prfPos[i] - encPos);
                    int errorvalue = 跟随误差;

                    if (axisRuns[i].轴名称 == "M19_测试手臂Z轴")
                    {
                        if (posvalue > errorvalue * 2)
                        {
                            return $"{axisRuns[i].轴名称}规划位置({prfPos[i]})和实际位置({encPos})的跟随误差={posvalue}>{errorvalue * 2}";
                        }
                        else if (posvalue > 300)
                        {
                            RunLog($"{axisRuns[i].轴名称}规划位置({prfPos[i]})和实际位置({encPos})的跟随误差={posvalue}>{300}");
                        }
                    }
                    else
                    {
                        if (posvalue > errorvalue)
                        {
                            return $"{axisRuns[i].轴名称}规划位置({prfPos[i]})和实际位置({encPos})的跟随误差={posvalue}>{errorvalue}";
                        }
                        else if (posvalue > 150)
                        {
                            RunLog($"{axisRuns[i].轴名称}规划位置({prfPos[i]})和实际位置({encPos})的跟随误差={posvalue}>{150}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "Success";
        }

        private string 单轴绝对运动(string axisName, double pos, int type = 0, int move = 0, int pos2 = 0)
        {
            string result = APPmaster.GetAxisRunInfo(axisName, out AxisRunInfo axisRunInfo);
            if (result == "Success")
            {
                AxisRun axisRun = new AxisRun()
                {
                    轴名称 = axisRunInfo.轴名称,
                    核号 = axisRunInfo.核号,
                    轴号 = axisRunInfo.轴号,
                    目标位置 = Convert.ToInt32(Math.Round(pos / axisRunInfo.步长mm)),
                    目标速度 = axisRunInfo.运行速度,
                    目标加速度 = axisRunInfo.加速度,
                    目标减速度 = axisRunInfo.减速度,
                    平滑时间 = axisRunInfo.平滑时间,
                };

                result = motion.UniaxialAbsoluteMoveAbs(axisRun);
                if (result == "Success")
                {
                    Task<string> task = Task.Run(() =>
                    {
                        return WaitAxisMove(axisRun, axisRun.目标位置, type, move, pos2);
                    });
                    result = task.Result;
                }
            }
            return result;
        }

        private string 单轴回零(AxisHomeInfo axisHomeInfo)
        {
            string result = motion.UniaxialHome(axisHomeInfo);
            if (result == "Success")
            {
                Task<string> task = Task.Run(() =>
                {
                    return WaitAxisHome(axisHomeInfo);
                });
                result = task.Result;
            }
            return result;
        }

        private string 单轴回零(string axisName)
        {
            string result = GetAxisHomeInfo(axisName, out AxisHomeInfo axisHomeInfo);
            if (result == "Success")
            {
                result = motion.UniaxialHome(axisHomeInfo);
                if (result == "Success")
                {
                    Task<string> task = Task.Run(() =>
                    {
                        return WaitAxisHome(axisHomeInfo);
                    });
                    result = task.Result;
                }
            }
            return result;
        }

        private string 多轴回零(string[] axisNames)
        {
            List<AxisHomeInfo> axisHomes = new List<AxisHomeInfo>();
            string result = GetAxisHomeInfo(axisNames, out axisHomes);
            if (result == "Success")
            {
                result = motion.MultiaxisHome(axisHomes);
                if (result == "Success")
                {
                    Task<string> task = Task.Run(() =>
                    {
                        return WaitAxisHome(axisHomes);
                    });
                    result = task.Result;
                }
            }
            return result;
        }

        public string 多轴绝对运动(string[] axisNames, double[] poss, int type = 0, int move = 0, int pos2 = 0)
        {
            string result;
            List<int> listpos = new List<int>();
            List<AxisRun> axisRuns = new List<AxisRun>();
            for (int i = 0; i < axisNames.Length; i++)
            {
                result = APPmaster.GetAxisRunInfo(axisNames[i], out AxisRunInfo axisRunInfo);
                if (result == "Success")
                {
                    int movepos = Convert.ToInt32(Math.Round(poss[i] / axisRunInfo.步长mm));
                    axisRuns.Add(new AxisRun()
                    {
                        轴名称 = axisRunInfo.轴名称,
                        核号 = axisRunInfo.核号,
                        轴号 = axisRunInfo.轴号,
                        目标位置 = movepos,
                        目标速度 = axisRunInfo.运行速度,
                        目标加速度 = axisRunInfo.加速度,
                        目标减速度 = axisRunInfo.减速度,
                        平滑时间 = axisRunInfo.平滑时间,
                    });
                    listpos.Add(movepos);
                }
            }
            result = motion.MultiaxisAbsoluteMoveAbs(axisRuns);
            if (result == "Success")
            {
                Task<string> task = Task.Run(() =>
                {
                    return WaitAxisMove(axisRuns, listpos, type, move, pos2);
                });
                result = task.Result;
            }
            return result;
        }

        private string 入料Z移动(bool[] isopen, double zpos)
        {
            List<string> axisNames = new List<string>();
            if (isopen[0]) axisNames.Add("M1_入料手臂Z1轴");
            if (isopen[1]) axisNames.Add("M2_入料手臂Z2轴");
            if (isopen[2]) axisNames.Add("M3_入料手臂Z3轴");
            if (isopen[3]) axisNames.Add("M4_入料手臂Z4轴");

            return 多轴绝对运动(axisNames.ToArray(), new double[] { zpos, zpos, zpos, zpos });

        }

        #region 入料吸料

        private void button113_Click(object sender, EventArgs e)
        {
            if (double.TryParse(comboBox8.Text, out double pos))
            {
                string result = 单轴绝对运动("M22_入料手臂X轴", pos);
                if (result != "Success")
                {
                    MessageBox.Show("入料手臂X移动失败!\n详细信息:" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button114_Click(object sender, EventArgs e)
        {
            if (double.TryParse(comboBox9.Text, out double pos))
            {
                string result = 单轴绝对运动("M16_入料手臂Y轴", pos);
                if (result != "Success")
                {
                    MessageBox.Show("入料手臂Y移动失败!\n详细信息:" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button115_Click(object sender, EventArgs e)
        {
            if (double.TryParse(textBox7.Text, out double pos))
            {
                string result = 单轴绝对运动("M5_入料手臂P轴", pos);
                if (result != "Success")
                {
                    MessageBox.Show("入料手臂P移动失败!\n详细信息:" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button116_Click(object sender, EventArgs e)
        {
            if (!double.TryParse(comboBox8.Text, out double xpos)) return;
            if (!double.TryParse(comboBox9.Text, out double ypos)) return;
            if (!double.TryParse(textBox7.Text, out double ppos)) return;

            string[] axisNames = { "M22_入料手臂X轴", "M16_入料手臂Y轴", "M5_入料手臂P轴" };
            double[] axisPoss = { xpos, ypos, ppos };

            string result = 多轴绝对运动(axisNames, axisPoss);
            if (result != "Success")
            {
                MessageBox.Show("入料手臂XYP移动失败!\n详细信息:" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button117_Click(object sender, EventArgs e)
        {
            if (double.TryParse(textBox3.Text, out double pos))
            {
                bool[] isopen = { uiCheckBox18.Checked, uiCheckBox20.Checked, uiCheckBox19.Checked, uiCheckBox17.Checked };
                string result = 入料Z移动(isopen, pos);
                if (result != "Success")
                {
                    MessageBox.Show("入料手臂Z移动失败!\n详细信息:" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button118_Click(object sender, EventArgs e)
        {
            string result = 入料吸嘴_吸放料(new bool[] { true, true, true, true }, true, true);

            if (result == "Success")
            {
                string[] ioNames =
                {
                    "X068_IC上料_1_吸真空检测", "X069_IC上料_2_吸真空检测",
                    "X070_IC上料_3_吸真空检测", "X071_IC上料_4_吸真空检测"
                };

                result = motionEx.GetDi(ioNames, out string[] iovalues);
                if (result != "Success") RunLog("入料吸嘴_真空检测IO读取失败!" + result);

                uiCheckBox5.Checked = iovalues[0] == "1";
                uiCheckBox8.Checked = iovalues[1] == "1";
                uiCheckBox7.Checked = iovalues[2] == "1";
                uiCheckBox6.Checked = iovalues[3] == "1";
            }
        }

        private void button119_Click(object sender, EventArgs e)
        {
            if (double.TryParse(textBox4.Text, out double pos))
            {
                bool[] isopen = { uiCheckBox26.Checked, uiCheckBox28.Checked, uiCheckBox27.Checked, uiCheckBox25.Checked };
                string result = 入料Z移动(isopen, pos);
                if (result != "Success")
                {
                    MessageBox.Show("入料手臂Z移动失败!\n详细信息:" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button120_Click(object sender, EventArgs e)
        {
            string[] axisNames =
            {
                "M1_入料手臂Z1轴", "M2_入料手臂Z2轴",
                "M3_入料手臂Z3轴", "M4_入料手臂Z4轴"
            };
            string result = motionEx.GetDiHome(axisNames, out string[] home);

            if (result != "Success")
            {
                RunLog($"入料Z回原：{home[0]} {home[1]} {home[2]} {home[3]}");
                MessageBox.Show("读取入料Z原点失败!\n详细信息:" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            uiCheckBox14.Checked = home[0] == "0";
            uiCheckBox16.Checked = home[1] == "0";
            uiCheckBox15.Checked = home[2] == "0";
            uiCheckBox13.Checked = home[3] == "0";
        }

        #endregion

        #region 入料放料



        #endregion

        private void button98_Click(object sender, EventArgs e)
        {
            if (double.TryParse(textBox8.Text, out double pos))
            {
                string result = 单轴绝对运动("M22_入料手臂X轴", pos);
                if (result != "Success")
                {
                    MessageBox.Show("入料手臂X移动失败!\n详细信息:" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button99_Click(object sender, EventArgs e)
        {
            if (double.TryParse(textBox9.Text, out double pos))
            {
                string result = 单轴绝对运动("M16_入料手臂Y轴", pos);
                if (result != "Success")
                {
                    MessageBox.Show("入料手臂Y移动失败!\n详细信息:" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button100_Click(object sender, EventArgs e)
        {
            if (double.TryParse(textBox10.Text, out double pos))
            {
                string result = 单轴绝对运动("M5_入料手臂P轴", pos);
                if (result != "Success")
                {
                    MessageBox.Show("入料手臂P移动失败!\n详细信息:" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button101_Click(object sender, EventArgs e)
        {
            if (!double.TryParse(textBox8.Text, out double xpos)) return;
            if (!double.TryParse(textBox9.Text, out double ypos)) return;
            if (!double.TryParse(textBox10.Text, out double ppos)) return;

            string[] axisNames = { "M22_入料手臂X轴", "M16_入料手臂Y轴", "M5_入料手臂P轴" };
            double[] axisPoss = { xpos, ypos, ppos };

            string result = 多轴绝对运动(axisNames, axisPoss);
            if (result != "Success")
            {
                MessageBox.Show("入料手臂XYP移动失败!\n详细信息:" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button107_Click(object sender, EventArgs e)
        {
            if (double.TryParse(textBox6.Text, out double pos))
            {
                bool[] isopen = { uiCheckBox11.Checked, uiCheckBox2.Checked, uiCheckBox10.Checked, uiCheckBox12.Checked };
                string result = 入料Z移动(isopen, pos);
                if (result != "Success")
                {
                    MessageBox.Show("入料手臂Z移动失败!\n详细信息:" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

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

        private void button109_Click(object sender, EventArgs e)
        {
            string result = 入料吸嘴_吸放料(new bool[] { true, true, true, true }, true, false);

            if (result == "Success")
            {
                result = 入料吸嘴_吸放料(new bool[] { true, true, true, true }, false, true);
                if (result == "Success")
                {
                    Thread.Sleep(200);
                    result = 入料吸嘴_吸放料(new bool[] { true, true, true, true }, false, false);
                    if (result == "Success")
                    {
                        string[] ioNames =
                    {
                        "X068_IC上料_1_吸真空检测", "X069_IC上料_2_吸真空检测",
                        "X070_IC上料_3_吸真空检测", "X071_IC上料_4_吸真空检测"
                    };

                        result = motionEx.GetDi(ioNames, out string[] iovalues);
                        if (result != "Success") RunLog("入料吸嘴_真空检测IO读取失败!" + result);

                        uiCheckBox5.Checked = iovalues[0] == "1";
                        uiCheckBox8.Checked = iovalues[1] == "1";
                        uiCheckBox7.Checked = iovalues[2] == "1";
                        uiCheckBox6.Checked = iovalues[3] == "1";
                    }
                }
            }
        }

        private void button121_Click(object sender, EventArgs e)
        {
            if (double.TryParse(textBox5.Text, out double pos))
            {
                bool[] isopen = { uiCheckBox3.Checked, uiCheckBox4.Checked, uiCheckBox9.Checked, uiCheckBox1.Checked };
                string result = 入料Z移动(isopen, pos);
                if (result != "Success")
                {
                    MessageBox.Show("入料手臂Z移动失败!\n详细信息:" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button108_Click(object sender, EventArgs e)
        {
            string[] axisNames =
            {
                "M1_入料手臂Z1轴", "M2_入料手臂Z2轴",
                "M3_入料手臂Z3轴", "M4_入料手臂Z4轴"
            };
            string result = motionEx.GetDiHome(axisNames, out string[] home);

            if (result != "Success")
            {
                RunLog($"入料Z回原：{home[0]} {home[1]} {home[2]} {home[3]}");
                MessageBox.Show("读取入料Z原点失败!\n详细信息:" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            uiCheckBox29.Checked = home[0] == "0";
            uiCheckBox32.Checked = home[1] == "0";
            uiCheckBox31.Checked = home[2] == "0";
            uiCheckBox23.Checked = home[3] == "0";
        }

        /// <summary>
        /// 入料手臂X回零
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button102_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                string result = 单轴回零("M22_入料手臂X轴");
                if (result != "Success")
                {
                    MessageBox.Show(result);
                }
                else MessageBox.Show("M22_入料手臂X轴回零完成");
            });
        }

        /// <summary>
        /// 入料手臂Y回零
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button111_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                string result = 单轴回零("M16_入料手臂Y轴");
                if (result != "Success")
                {
                    MessageBox.Show(result);
                }
                else MessageBox.Show("M16_入料手臂Y轴回零完成");
            });
        }

        /// <summary>
        /// 入料手臂P回零
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button112_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                string result = 单轴回零("M5_入料手臂P轴");
                if (result != "Success")
                {
                    MessageBox.Show(result);
                }
                else MessageBox.Show("M5_入料手臂P轴回零完成");
            });
        }

        /// <summary>
        /// 入料手臂所有Z回零
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button110_Click(object sender, EventArgs e)
        {
            string[] axisNames = { "M1_入料手臂Z1轴", "M2_入料手臂Z2轴", "M3_入料手臂Z3轴", "M4_入料手臂Z4轴" };
            Task.Run(() =>
            {
                string result = 多轴回零(axisNames);
                if (result != "Success")
                {
                    MessageBox.Show("入料手臂所有Z回零失败!");
                }
                else MessageBox.Show("入料手臂所有Z回零完成");
            });
        }

        /// <summary>
        /// 入料手臂Z4回零
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button103_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                string result = 单轴回零("M4_入料手臂Z4轴");
                if (result != "Success")
                {
                    MessageBox.Show(result);
                }
                else MessageBox.Show("M4_入料手臂Z4轴回零完成");
            });
        }

        /// <summary>
        /// 入料手臂Z3回零
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button105_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                string result = 单轴回零("M3_入料手臂Z3轴");
                if (result != "Success")
                {
                    MessageBox.Show(result);
                }
                else MessageBox.Show("M3_入料手臂Z3轴回零完成");
            });
        }

        /// <summary>
        /// 入料手臂Z2回零
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button104_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                string result = 单轴回零("M2_入料手臂Z2轴");
                if (result != "Success")
                {
                    MessageBox.Show(result);
                }
                else MessageBox.Show("M2_入料手臂Z2轴回零完成");
            });
        }

        /// <summary>
        /// 入料手臂Z1回零
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button106_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                string result = 单轴回零("M1_入料手臂Z1轴");
                if (result != "Success")
                {
                    MessageBox.Show(result);
                }
                else MessageBox.Show("入料手臂Z1回零完成");
            });
        }

        /// <summary>
        /// 入料手臂XYP回零
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button122_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                string[] axisNames = { "M22_入料手臂X轴", "M16_入料手臂Y轴", "M5_入料手臂P轴" };
                string result = 多轴回零(axisNames);
                if (result != "Success")
                {
                    MessageBox.Show("入料手臂XYP回零失败!");
                }
                else MessageBox.Show("入料手臂XYP回零完成");
            });
        }

        /// <summary>
        /// 所有轴紧急停止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button123_Click(object sender, EventArgs e)
        {
            APPmaster.IsPause = true;
            for (int i = 0; i < AxisInfoList.Count; i++)
            {
                string result = motion.AxisStop(AxisInfoList[i].核号, AxisInfoList[i].轴号, 1);
                if (result != "Success")
                {
                    RunLog($"核号{AxisInfoList[i].核号},轴{AxisInfoList[i].轴号}停止失败!详细信息:" + result);
                }
            }
            MessageBox.Show("所有轴已紧急停止");
        }

        private void cobox_axisname_SelectedIndexChanged(object sender, EventArgs e)
        {
            string result = APPmaster.GetAxisHomeInfo(cobox_axisname.Text, out AxisHomeInfo axisHomeInfo);
            if (result == "Success")
            {
                if (axisHomeInfo.模式 == 11) txt_home_mode.Text = "限位+Home回零";
                else if (axisHomeInfo.模式 == 20) txt_home_mode.Text = "Home回零";
                else txt_home_mode.Text = "";

                txt_home_vel.Text = axisHomeInfo.搜索速度.ToString();
                txt_home_dingwei_vel.Text = axisHomeInfo.定位速度.ToString();
                txt_home_acc.Text = axisHomeInfo.加速度.ToString();
                txt_home_dec.Text = axisHomeInfo.减速度.ToString();
                txt_home_Smoothtime.Text = axisHomeInfo.平滑时间.ToString();
                txt_home_offset.Text = axisHomeInfo.零点偏置.ToString();
                txt_escapeStep.Text = axisHomeInfo.反向脱离步长.ToString();

                if (axisHomeInfo.回零启动方向 == 1)
                {
                    chk_home_pmovedir.Checked = true;
                    chk_home_nmovedir.Checked = false;
                }
                else
                {
                    chk_home_pmovedir.Checked = false;
                    chk_home_nmovedir.Checked = true;
                }
                if (axisHomeInfo.捕获触发沿 == 1)
                {
                    chk_home_rising.Checked = true;
                    chk_home_falling.Checked = false;
                }
                else
                {
                    chk_home_rising.Checked = false;
                    chk_home_falling.Checked = true;
                }
            }
        }

        private void uiCheckBox5_Click(object sender, EventArgs e)
        {
            chk_home_pmovedir.Checked = !chk_home_nmovedir.Checked;
        }

        /// <summary>
        /// 飞梭后入料清空
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button129_Click(object sender, EventArgs e)
        {
            飞梭后入料IC = new bool[][] { new bool[] { false, false, false, false }, new bool[] { false, false, false, false } };
        }

        private void button130_Click(object sender, EventArgs e)
        {
            飞梭后入料状态 = 0;
        }

        private void button132_Click(object sender, EventArgs e)
        {
            //int site = Convert.ToInt32(comboBox5.Text);
            //SiteTestState[site] = 3;
            //OutSiteQueue.Enqueue(site);
            //飞梭后出料IC = new bool[][] { new bool[] { true, true, true, true }, new bool[] { true, true, true, true } };
            //飞梭后出料BIN项 = new string[][] { new string[] { "01", "01", "01", "01" }, new string[] { "01", "01", "01", "01" } };
        }

        private void button131_Click(object sender, EventArgs e)
        {
            //飞梭后出料状态 = 1;
        }

        private void button165_Click(object sender, EventArgs e)
        {
            飞梭前出料IC = new bool[][] { new bool[] { true, true, true, true }, new bool[] { true, true, true, true } };
        }

        private void button164_Click(object sender, EventArgs e)
        {
            //飞梭前入料状态 = 1;
        }

        #region 测试区位移和上下位移气缸

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
            else RunLog($"{yIoName}到位输出成功({ms}ms)");
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

            result = 读取气缸输入信号(xIoName, "0", out long ms);
            if (result != "Success") RunLog($"{yIoName}原位输出超时({ms}ms)");
            else RunLog($"{yIoName}原位输出成功({ms}ms)");
            return result;
        }

        public string 测试区_下压气缸到位输出(string yIoName, string xIoName)
        {
            string result = motionEx.SetDo(yIoName, false);
            if (result != "Success")
            {
                RunLog($"{yIoName}到位输出失败！" + result);
                return result;
            }

            result = 读取气缸输入信号(xIoName, "0", out long ms);
            if (result != "Success") RunLog($"{yIoName}到位输出超时({ms}ms)");
            else RunLog($"{yIoName}到位输出成功({ms}ms)");
            return result;
        }

        public string 测试区_下压气缸原位输出(string yIoName, string xIoName)
        {
            string result = motionEx.SetDo(yIoName, true);
            if (result != "Success")
            {
                RunLog($"{yIoName}原位输出失败！" + result);
                return result;
            }
            result = 读取气缸输入信号(xIoName, "1", out long ms);

            if (result == "Success") RunLog($"{yIoName}原位输出超时({ms}ms)");
            else RunLog($"{yIoName}原位输出成功({ms}ms)");
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

        private void button208_Click(object sender, EventArgs e)
        {
            short site = Convert.ToInt16(comboBox6.Text);
            测试区_位移气缸原位输出(site);
        }

        private void button209_Click(object sender, EventArgs e)
        {
            short site = Convert.ToInt16(comboBox6.Text);
            测试区_位移气缸到位输出(site);
        }

        private void button210_Click(object sender, EventArgs e)
        {
            short site = Convert.ToInt16(comboBox7.Text);
            测试区_下压气缸到位输出(site);
        }

        private void button211_Click(object sender, EventArgs e)
        {
            short site = Convert.ToInt16(comboBox7.Text);
            测试区_下压气缸原位输出(site);
        }

        private void button212_Click(object sender, EventArgs e)
        {
            Empty上顶标记 = 2;
        }

        private void button213_Click(object sender, EventArgs e)
        {
            Load上顶标记 = 2;
        }

        private void button215_Click(object sender, EventArgs e)
        {
            Auto1上顶标记 = 2;
            //Task.Run(() => {
            //    handler.Auto1顶盘流程();
            //});
        }

        private void button214_Click(object sender, EventArgs e)
        {
            Auto2上顶标记 = 2;
            //Task.Run(() => {
            //    handler.Auto2顶盘流程();
            //});
        }

        private void button221_Click(object sender, EventArgs e)
        {
            抓盘手臂取Empty空盘标记 = 2;
            //Task.Run(() => { handler.抓盘手臂取Empty盘(); });
        }

        private void button216_Click(object sender, EventArgs e)
        {
            抓盘手臂取Load空盘标记 = 2;
            //抓盘手臂放空盘到Empty标记 = 2;
        }

        private void button217_Click(object sender, EventArgs e)
        {
            Auto1上顶标记 = 1;
            抓盘手臂放空盘到Auto1标记 = 2;
            //Task.Run(() => { handler.抓盘手臂放空盘到Auto1(); });
        }

        private void button218_Click(object sender, EventArgs e)
        {
            Auto2上顶标记 = 1;
            抓盘手臂放空盘到Auto2标记 = 2;
            //Task.Run(() => { handler.抓盘手臂放空盘到Auto2(); });
        }

        private void button219_Click(object sender, EventArgs e)
        {
            Auto1上顶标记 = 2;
            抓盘手臂放空盘到Auto1标记 = 2;
            //Task.Run(() => { handler.抓盘手臂换Auto1空盘(); });
        }

        private void button220_Click(object sender, EventArgs e)
        {
            Auto2上顶标记 = 2;
            抓盘手臂放空盘到Auto2标记 = 2;
            //Task.Run(() => { handler.抓盘手臂换Auto2空盘(); });
        }

        private void 刷新入料手臂初始点位()
        {
            uiDoubleUpDown99.Text = 初始点位.入料_Load_X.ToString();
            uiDoubleUpDown97.Text = 初始点位.入料_Load_Y.ToString();
            uiDoubleUpDown98.Text = 初始点位.入料_Load_Z.ToString();

            uiDoubleUpDown108.Text = 初始点位.入料_Heat_X.ToString();
            uiDoubleUpDown107.Text = 初始点位.入料_Heat_Y.ToString();
            uiDoubleUpDown106.Text = 初始点位.入料_Heat吸_Z.ToString();
            uinumericUpDown126.Text = 初始点位.入料_Heat放_Z.ToString();

            uiDoubleUpDown102.Text = 初始点位.入料_梭车1_X.ToString();
            uiDoubleUpDown101.Text = 初始点位.入料_梭车1_Y.ToString();
            uiDoubleUpDown100.Text = 初始点位.入料_梭车1_Z.ToString();

            uiDoubleUpDown114.Text = 初始点位.入料_梭车2_X.ToString();
            uiDoubleUpDown113.Text = 初始点位.入料_梭车2_Y.ToString();
            uiDoubleUpDown112.Text = 初始点位.入料_梭车2_Z.ToString();
            numericUpDown152.Text = 入料吸料间距.ToString();
            numericUpDown155.Text = 入料吸放料Heat间距.ToString();
            numericUpDown153.Text = 入料放料间距.ToString();

            uiDoubleUpDown105.Text = 初始点位.入料_变距最小值.ToString();
            uiDoubleUpDown104.Text = 初始点位.入料_变距最大值.ToString();
            uiDoubleUpDown103.Text = 初始点位.入料_梭车列间隔.ToString();

            uiDoubleUpDown18.Text = 初始点位.入料_Z1_硬件补偿.ToString();
            uiDoubleUpDown17.Text = 初始点位.入料_Z2_硬件补偿.ToString();
            uiDoubleUpDown16.Text = 初始点位.入料_Z3_硬件补偿.ToString();
            uiDoubleUpDown19.Text = 初始点位.入料_Z4_硬件补偿.ToString();
        }

        private void 刷新入料手臂速度参数()
        {
            GetAxisRunInfo("M22_入料手臂X轴", out AxisRunInfo axisRun).ToString();
            uiDoubleUpDown9.Text = axisRun.最大速度.ToString();
            uiDoubleUpDown8.Text = axisRun.加速度.ToString();
            uiDoubleUpDown7.Text = axisRun.减速度.ToString();

            GetAxisRunInfo("M16_入料手臂Y轴", out axisRun).ToString();
            uiDoubleUpDown117.Text = axisRun.最大速度.ToString();
            uiDoubleUpDown116.Text = axisRun.加速度.ToString();
            uiDoubleUpDown115.Text = axisRun.减速度.ToString();

            GetAxisRunInfo("M5_入料手臂P轴", out axisRun).ToString();
            uiDoubleUpDown12.Text = axisRun.最大速度.ToString();
            uiDoubleUpDown11.Text = axisRun.加速度.ToString();
            uiDoubleUpDown10.Text = axisRun.减速度.ToString();

            GetAxisRunInfo("M1_入料手臂Z1轴", out axisRun).ToString();
            uiDoubleUpDown15.Text = axisRun.最大速度.ToString();
            uiDoubleUpDown14.Text = axisRun.加速度.ToString();
            uiDoubleUpDown13.Text = axisRun.减速度.ToString();

            GetAxisRunInfo("M2_入料手臂Z2轴", out axisRun).ToString();
            uiDoubleUpDown81.Text = axisRun.最大速度.ToString();
            uiDoubleUpDown80.Text = axisRun.加速度.ToString();
            uiDoubleUpDown79.Text = axisRun.减速度.ToString();

            GetAxisRunInfo("M3_入料手臂Z3轴", out axisRun).ToString();
            uiDoubleUpDown6.Text = axisRun.最大速度.ToString();
            uiDoubleUpDown5.Text = axisRun.加速度.ToString();
            uiDoubleUpDown4.Text = axisRun.减速度.ToString();

            GetAxisRunInfo("M4_入料手臂Z4轴", out axisRun).ToString();
            uiDoubleUpDown3.Text = axisRun.最大速度.ToString();
            uiDoubleUpDown2.Text = axisRun.加速度.ToString();
            uiDoubleUpDown1.Text = axisRun.减速度.ToString();
        }

        private void 刷新出料手臂初始点位()
        {
            uiDoubleUpDown75.Text = 初始点位.出料_梭车1_X.ToString();
            uiDoubleUpDown74.Text = 初始点位.出料_梭车1_Y.ToString();
            uiDoubleUpDown73.Text = 初始点位.出料_梭车1_Z.ToString();

            uiDoubleUpDown90.Text = 初始点位.出料_Auto1_X.ToString();
            uiDoubleUpDown89.Text = 初始点位.出料_Auto1_Y.ToString();
            uiDoubleUpDown88.Text = 初始点位.出料_Auto1_Z.ToString();

            uiDoubleUpDown93.Text = 初始点位.出料_Auto2_X.ToString();
            uiDoubleUpDown92.Text = 初始点位.出料_Auto2_Y.ToString();
            uiDoubleUpDown91.Text = 初始点位.出料_Auto2_Z.ToString();
            numericUpDown158.Text = 出料放料间距.ToString();
            numericUpDown157.Text = 出料吸料间距.ToString();

            uiDoubleUpDown78.Text = 初始点位.出料_梭车2_X.ToString();
            uiDoubleUpDown77.Text = 初始点位.出料_梭车2_Y.ToString();
            uiDoubleUpDown76.Text = 初始点位.出料_梭车2_Z.ToString();

            uiDoubleUpDown87.Text = 初始点位.出料_Fix1_X.ToString();
            uiDoubleUpDown86.Text = 初始点位.出料_Fix1_Y.ToString();
            uiDoubleUpDown85.Text = 初始点位.出料_Fix1_Z.ToString();

            uiDoubleUpDown84.Text = 初始点位.出料_Fix3_X.ToString();
            uiDoubleUpDown83.Text = 初始点位.出料_Fix3_Y.ToString();
            uiDoubleUpDown82.Text = 初始点位.出料_Fix3_Z.ToString();
            uiDoubleUpDown126.Text = 初始点位.出料_Fix3_右移动.ToString();

            uiDoubleUpDown96.Text = 初始点位.出料_Fix2_X.ToString();
            uiDoubleUpDown95.Text = 初始点位.出料_Fix2_Y.ToString();
            uiDoubleUpDown94.Text = 初始点位.出料_Fix2_Z.ToString();

            uiDoubleUpDown125.Text = 初始点位.出料_变距最小值.ToString();
            uiDoubleUpDown124.Text = 初始点位.出料_变距最大值.ToString();
            uiDoubleUpDown123.Text = 初始点位.出料_梭车列间隔.ToString();

            uiDoubleUpDown122.Text = 初始点位.出料_Z1_硬件补偿.ToString();
            uiDoubleUpDown121.Text = 初始点位.出料_Z2_硬件补偿.ToString();
            uiDoubleUpDown21.Text = 初始点位.出料_Z3_硬件补偿.ToString();
            uiDoubleUpDown20.Text = 初始点位.出料_Z4_硬件补偿.ToString();
        }

        private void 刷新出料手臂速度参数()
        {
            GetAxisRunInfo("M23_出料手臂X轴", out AxisRunInfo axisRun).ToString();
            uiDoubleUpDown132.Text = axisRun.最大速度.ToString();
            uiDoubleUpDown131.Text = axisRun.加速度.ToString();
            uiDoubleUpDown130.Text = axisRun.减速度.ToString();

            GetAxisRunInfo("M17_出料手臂Y轴", out axisRun).ToString();
            uiDoubleUpDown144.Text = axisRun.最大速度.ToString();
            uiDoubleUpDown143.Text = axisRun.加速度.ToString();
            uiDoubleUpDown142.Text = axisRun.减速度.ToString();

            GetAxisRunInfo("M10_出料手臂P轴", out axisRun).ToString();
            uiDoubleUpDown135.Text = axisRun.最大速度.ToString();
            uiDoubleUpDown134.Text = axisRun.加速度.ToString();
            uiDoubleUpDown133.Text = axisRun.减速度.ToString();

            GetAxisRunInfo("M6_出料手臂Z1轴", out axisRun).ToString();
            uiDoubleUpDown138.Text = axisRun.最大速度.ToString();
            uiDoubleUpDown137.Text = axisRun.加速度.ToString();
            uiDoubleUpDown136.Text = axisRun.减速度.ToString();

            GetAxisRunInfo("M7_出料手臂Z2轴", out axisRun).ToString();
            uiDoubleUpDown141.Text = axisRun.最大速度.ToString();
            uiDoubleUpDown140.Text = axisRun.加速度.ToString();
            uiDoubleUpDown139.Text = axisRun.减速度.ToString();

            GetAxisRunInfo("M8_出料手臂Z3轴", out axisRun).ToString();
            uiDoubleUpDown129.Text = axisRun.最大速度.ToString();
            uiDoubleUpDown128.Text = axisRun.加速度.ToString();
            uiDoubleUpDown127.Text = axisRun.减速度.ToString();

            GetAxisRunInfo("M9_出料手臂Z4轴", out axisRun).ToString();
            uiDoubleUpDown120.Text = axisRun.最大速度.ToString();
            uiDoubleUpDown119.Text = axisRun.加速度.ToString();
            uiDoubleUpDown118.Text = axisRun.减速度.ToString();

        }

        private void 刷新测试手臂初始点位()
        {
            uiDoubleUpDown62.Text = 初始点位.测试_梭车1吸_X.ToString();
            uiDoubleUpDown61.Text = 初始点位.测试_梭车1吸_Z.ToString();

            uiDoubleUpDown64.Text = 初始点位.测试_梭车2吸_X.ToString();
            uiDoubleUpDown63.Text = 初始点位.测试_梭车2吸_Z.ToString();

            uiDoubleUpDown66.Text = 初始点位.测试_Test1区_X.ToString();
            uiDoubleUpDown65.Text = 初始点位.测试_Test2区_X.ToString();
            uiDoubleUpDown68.Text = 初始点位.测试_Test3区_X.ToString();
            uiDoubleUpDown67.Text = 初始点位.测试_Test4区_X.ToString();

            uiDoubleUpDown70.Text = 初始点位.测试_梭车1放_X.ToString();
            uiDoubleUpDown69.Text = 初始点位.测试_梭车1放_Z.ToString();

            uiDoubleUpDown72.Text = 初始点位.测试_梭车2放_X.ToString();
            uiDoubleUpDown71.Text = 初始点位.测试_梭车2放_Z.ToString();

            uiDoubleUpDown23.Text = 初始点位.测试_Test区Site间隔.ToString();
            uiDoubleUpDown25.Text = 初始点位.测试_Test区吸_Z.ToString();
            uiDoubleUpDown27.Text = 初始点位.测试_Test区放_Z.ToString();
        }

        private void 刷新测试手臂速度参数()
        {
            GetAxisRunInfo("M21_测试手臂X轴", out AxisRunInfo axisRun);
            uiDoubleUpDown45.Text = axisRun.最大速度.ToString();
            uiDoubleUpDown44.Text = axisRun.加速度.ToString();
            uiDoubleUpDown43.Text = axisRun.减速度.ToString();

            GetAxisRunInfo("M19_测试手臂Z轴", out axisRun);
            uiDoubleUpDown48.Text = axisRun.最大速度.ToString();
            uiDoubleUpDown47.Text = axisRun.加速度.ToString();
            uiDoubleUpDown46.Text = axisRun.减速度.ToString();
        }

        private void 刷新梭车初始点位()
        {
            uiDoubleUpDown57.Text = 初始点位.梭车2_前_Y.ToString();
            uiDoubleUpDown56.Text = 初始点位.梭车2_后_Y.ToString();
        }

        private void 刷新梭车速度参数()
        {
            GetAxisRunInfo("M20_飞梭轴", out AxisRunInfo axisRun);
            uiDoubleUpDown51.Text = axisRun.最大速度.ToString();
            uiDoubleUpDown50.Text = axisRun.加速度.ToString();
            uiDoubleUpDown49.Text = axisRun.减速度.ToString();
        }

        private void 刷新抓盘手臂初始点位()
        {
            uiDoubleUpDown55.Text = 初始点位.抓盘_Empty_X.ToString();
            uiDoubleUpDown58.Text = 初始点位.抓盘_Load_X.ToString();
            uiDoubleUpDown60.Text = 初始点位.抓盘_Auto1_X.ToString();
            uiDoubleUpDown59.Text = 初始点位.抓盘_Auto2_X.ToString();
        }

        private void 刷新抓盘手臂速度参数()
        {
            GetAxisRunInfo("M18_放Tary盘轴", out AxisRunInfo axisRun);
            uiDoubleUpDown29.Text = axisRun.最大速度.ToString(); ;
            uiDoubleUpDown24.Text = axisRun.加速度.ToString(); ;
            uiDoubleUpDown22.Text = axisRun.减速度.ToString(); ;
        }

        private void 刷新初始点位(string tabpagetext)
        {
            if (tabpagetext == "入料手臂")
            {
                刷新入料手臂初始点位();
                刷新入料手臂速度参数();
            }
            else if (tabpagetext == "出料手臂")
            {
                刷新出料手臂初始点位();
                刷新出料手臂速度参数();
            }
            else if (tabpagetext == "测试手臂")
            {
                刷新测试手臂初始点位();
                刷新测试手臂速度参数();
            }
            else if (tabpagetext == "梭车")
            {
                刷新梭车初始点位();
                刷新梭车速度参数();
            }
            else if (tabpagetext == "抓盘手臂")
            {
                刷新抓盘手臂初始点位();
                刷新抓盘手臂速度参数();
            }
        }

        private void 刷新设定(string name)
        {
            string result = SQLiteHelper.runSql(SQLiteConnString, $"select name from TrayInfo", out DataTable traydt);
            if (result != "Success") return;
            uiComboBox1.DataSource = traydt;
            uiComboBox1.DisplayMember = "name";

            result = SQLiteHelper.runSql(SQLiteConnString, $"select name from HeatInfo", out DataTable heatdt);
            if (result != "Success") return;
            uiComboBox2.DataSource = heatdt;
            uiComboBox2.DisplayMember = "name";

            result = SQLiteHelper.runSql(SQLiteConnString, $"select * from SetupInfo where 档案名称 = '{name}'", out DataTable dt);
            if (result != "Success") return;

            uiTextBox1.Text = dt.Rows[0]["档案名称"].ToString();
            uiComboBox1.Text = dt.Rows[0]["tray盘名称"].ToString();
            uiComboBox2.Text = dt.Rows[0]["加热盘名称"].ToString();
            string tempmode = dt.Rows[0]["温度模式"].ToString();
            if (tempmode == "常温")
            {
                uiCheckBox192.Checked = true;
                uiCheckBox193.Checked = false;
            }
            else
            {
                uiCheckBox192.Checked = false;
                uiCheckBox193.Checked = true;
            }
            label355.Visible = uiCheckBox193.Checked;
            uiComboBox2.Visible = uiCheckBox193.Checked;
        }

        private void 刷新测试机(string name)
        {
            string result = GetSetupValue(name, "测试机名称", out string value);
            if (result != "Success") return;

            result = GetTesterIFParam(value, out TesterIFParam tester);
            if (result != "Success") return;

            刷新测试机(tester);
        }

        private void 刷新测试机(TesterIFParam tester)
        {
            numericUpDown12.Text = tester.Dout连续不良个数.ToString();
            checkBox1.Checked = tester.Dout连续不良自动关闭;
            checkBox2.Checked = tester.Dout连续不良报警;

            numericUpDown5.Text = tester.Dut累计不良报警个数.ToString();
            checkBox3.Checked = tester.Dut累计不良报警使能;
            numericUpDown13.Text = tester.总良率报警百分比.ToString();
            numericUpDown14.Text = tester.总良率报警基数.ToString();
            numericUpDown15.Text = tester.总良率报警间隔.ToString();
            checkBox4.Checked = tester.总良率报警使能;
            numericUpDown16.Text = tester.socket使用次数.ToString();
            numericUpDown_测试超时时间.Text = tester.测试超时时间.ToString();
            numericUpDown_timeout超时时间.Text = tester.timeout超时时间.ToString();
            checkBox_timeout.Checked = tester.超时发送timeout;

            uiComboBox11.Text = tester.串口号;
            uiComboBox12.Text = tester.波特率.ToString();
            uiComboBox14.Text = tester.校验位.ToString();
            uiComboBox13.Text = tester.数据位.ToString();
            uiComboBox15.Text = tester.停止位.ToString();
        }


        private void 刷新分BIN(BinParam binParam)
        {
            (panel3.Controls["radioButton3_" + binParam.fix1num] as UIRadioButton).Checked = true;
            (panel10.Controls["radioButton4_" + binParam.fix2num] as UIRadioButton).Checked = true;
            (panel11.Controls["radioButton5_" + binParam.fix3num] as UIRadioButton).Checked = true;


            foreach (object listbox in tabPage_bin.Controls)
            {
                if (listbox.GetType() != typeof(ListBox))
                {
                    continue;
                }
                (listbox as ListBox).Items.Clear();
            }

            foreach (string s1 in binParam.auto1.Split(','))
            {
                if (s1 == "") break;
                listBox1.Items.Add(s1);
            }
            foreach (string s2 in binParam.auto2.Split(','))
            {
                if (s2 == "") break;
                listBox2.Items.Add(s2);
            }

            string[] s = binParam.fix1.Split(';');
            for (int i = 0; i < 3; i++)
            {
                if (s[i] == "") continue;
                foreach (string ss in s[i].Split(','))
                {
                    (tabPage_bin.Controls["listBox3_" + (i + 1)] as ListBox).Items.Add(ss);
                }

            }
            s = binParam.fix2.Split(';');
            for (int i = 0; i < 3; i++)
            {
                if (s[i] == "") continue;
                foreach (string ss in s[i].Split(','))
                {
                    (tabPage_bin.Controls["listBox4_" + (i + 1)] as ListBox).Items.Add(ss);
                }

            }
            s = binParam.fix3.Split(';');
            for (int i = 0; i < 3; i++)
            {
                if (s[i] == "") continue;
                foreach (string ss in s[i].Split(','))
                {
                    (tabPage_bin.Controls["listBox5_" + (i + 1)] as ListBox).Items.Add(ss);
                }

            }

            label_color1.BackColor = System.Drawing.Color.FromArgb(binParam.color1);
            label_color2.BackColor = System.Drawing.Color.FromArgb(binParam.color2);
            label_color3.BackColor = System.Drawing.Color.FromArgb(binParam.color3);
            label_color4.BackColor = System.Drawing.Color.FromArgb(binParam.color4);
            label_color5.BackColor = System.Drawing.Color.FromArgb(binParam.color5);

            textBox_name1.Text = binParam.name1;
            textBox_name2.Text = binParam.name2;
            textBox_name3.Text = binParam.name3;
            textBox_name4.Text = binParam.name4;
            textBox_name5.Text = binParam.name5;
        }

        private void 刷新分BIN(string name)
        {
            string result = GetSetupValue(name, "分BIN名称", out string value);
            if (result != "Success") return;

            result = GetBINParam(value, out BinParam binParam);
            if (result != "Success") return;

            刷新分BIN(binParam);
        }

        private void 刷新手臂点位(string name, string page)
        {
            string result = GetOffsetPoint(name, out OffsetPoint offset);
            if (result != "Success") return;

            if (page == "入料手臂")
            {
                numericUpDown33.Text = offset.入料_Load吸_X.ToString();
                numericUpDown30.Text = offset.入料_Load吸_Y.ToString();
                numericUpDown28.Text = offset.入料_Load吸_Z.ToString();

                numericUpDown39.Text = offset.入料_Heat吸_X.ToString();
                numericUpDown38.Text = offset.入料_Heat吸_Y.ToString();
                numericUpDown37.Text = offset.入料_Heat吸_Z.ToString();

                numericUpDown36.Text = offset.入料_Heat放_X.ToString();
                numericUpDown35.Text = offset.入料_Heat放_Y.ToString();
                numericUpDown34.Text = offset.入料_Heat放_Z.ToString();

                numericUpDown32.Text = offset.入料_梭车1第1列放_X.ToString();
                numericUpDown31.Text = offset.入料_梭车1第1列放_Y.ToString();
                numericUpDown29.Text = offset.入料_梭车1第1列放_Z.ToString();

                numericUpDown27.Text = offset.入料_梭车1第2列放_X.ToString();
                numericUpDown26.Text = offset.入料_梭车1第2列放_Y.ToString();
                numericUpDown25.Text = offset.入料_梭车1第2列放_Z.ToString();

                numericUpDown24.Text = offset.入料_Load_变距.ToString();
                numericUpDown23.Text = offset.入料_Heat_变距.ToString();
                numericUpDown22.Text = offset.入料_梭车_变距.ToString();

                numericUpDown21.Text = offset.入料_梭车2第1列放_X.ToString();
                numericUpDown20.Text = offset.入料_梭车2第1列放_Y.ToString();
                numericUpDown19.Text = offset.入料_梭车2第1列放_Z.ToString();

                numericUpDown18.Text = offset.入料_梭车2第2列放_X.ToString();
                numericUpDown17.Text = offset.入料_梭车2第2列放_Y.ToString();
                numericUpDown1.Text = offset.入料_梭车2第2列放_Z.ToString();


            }
            else if (page == "出料手臂")
            {
                numericUpDown63.Text = offset.出料_梭车1第1列吸_X.ToString();
                numericUpDown62.Text = offset.出料_梭车1第1列吸_Y.ToString();
                numericUpDown61.Text = offset.出料_梭车1第1列吸_Z.ToString();

                numericUpDown60.Text = offset.出料_梭车1第2列吸_X.ToString();
                numericUpDown59.Text = offset.出料_梭车1第2列吸_Y.ToString();
                numericUpDown58.Text = offset.出料_梭车1第2列吸_Z.ToString();

                numericUpDown57.Text = offset.出料_Fix1放_X.ToString();
                numericUpDown56.Text = offset.出料_Fix1放_Y.ToString();
                numericUpDown55.Text = offset.出料_Fix1放_Z.ToString();

                uiDoubleUpDown33.Text = offset.出料_梭车_变距.ToString();
                uiDoubleUpDown34.Text = offset.出料_AF_变距.ToString();
                uiDoubleUpDown31.Text = offset.出料_梭车2第1列吸_X.ToString();
                uiDoubleUpDown32.Text = offset.出料_梭车2第1列吸_Y.ToString();
                uiDoubleUpDown30.Text = offset.出料_梭车2第1列吸_Z.ToString();

                numericUpDown54.Text = offset.出料_梭车2第2列吸_X.ToString();
                numericUpDown53.Text = offset.出料_梭车2第2列吸_Y.ToString();
                numericUpDown52.Text = offset.出料_梭车2第2列吸_Z.ToString();

                numericUpDown51.Text = offset.出料_Fix2放_X.ToString();
                numericUpDown50.Text = offset.出料_Fix2放_Y.ToString();
                numericUpDown49.Text = offset.出料_Fix2放_Z.ToString();
                numericUpDown48.Text = offset.出料_Auto1放_X.ToString();
                numericUpDown47.Text = offset.出料_Auto1放_Y.ToString();
                numericUpDown46.Text = offset.出料_Auto1放_Z.ToString();

                numericUpDown45.Text = offset.出料_Auto2放_X.ToString();
                numericUpDown44.Text = offset.出料_Auto2放_Y.ToString();
                numericUpDown43.Text = offset.出料_Auto2放_Z.ToString();

                numericUpDown42.Text = offset.出料_Fix3放_X.ToString();
                numericUpDown41.Text = offset.出料_Fix3放_Y.ToString();
                numericUpDown40.Text = offset.出料_Fix3放_Z.ToString();
            }
            else if (page == "测试手臂")
            {
                numericUpDown78.Text = offset.测试_梭车1左吸_X.ToString();
                numericUpDown75.Text = offset.测试_梭车1左吸_Z.ToString();
                numericUpDown73.Text = offset.测试_梭车1右放_X.ToString();
                numericUpDown72.Text = offset.测试_梭车1右放_Z.ToString();
                numericUpDown70.Text = offset.测试_梭车2左吸_X.ToString();
                numericUpDown68.Text = offset.测试_梭车2左吸_Z.ToString();
                numericUpDown66.Text = offset.测试_梭车2右放_X.ToString();
                numericUpDown64.Text = offset.测试_梭车2右放_Z.ToString();
                numericUpDown119.Text = offset.测试_Site1吸_X.ToString();
                numericUpDown118.Text = offset.测试_Site2吸_X.ToString();
                uiDoubleUpDown26.Text = offset.测试_Site3吸_X.ToString();
                numericUpDown117.Text = offset.测试_Site4吸_X.ToString();
                uiDoubleUpDown36.Text = offset.测试_Site5吸_X.ToString();
                uiDoubleUpDown35.Text = offset.测试_Site6吸_X.ToString();
                uiDoubleUpDown28.Text = offset.测试_Site7吸_X.ToString();
                numericUpDown116.Text = offset.测试_Site8吸_X.ToString();
                numericUpDown115.Text = offset.测试_Site9吸_X.ToString();
                numericUpDown114.Text = offset.测试_Site10吸_X.ToString();
                uiDoubleUpDown42.Text = offset.测试_Site11吸_X.ToString();
                uiDoubleUpDown41.Text = offset.测试_Site12吸_X.ToString();
                uiDoubleUpDown40.Text = offset.测试_Site13吸_X.ToString();
                uiDoubleUpDown39.Text = offset.测试_Site14吸_X.ToString();
                uiDoubleUpDown38.Text = offset.测试_Site15吸_X.ToString();
                uiDoubleUpDown37.Text = offset.测试_Site16吸_X.ToString();
                numericUpDown113.Text = offset.测试_Site1吸_Z.ToString();
                numericUpDown109.Text = offset.测试_Site2吸_Z.ToString();
                numericUpDown104.Text = offset.测试_Site3吸_Z.ToString();
                numericUpDown105.Text = offset.测试_Site4吸_Z.ToString();
                numericUpDown112.Text = offset.测试_Site5吸_Z.ToString();
                numericUpDown108.Text = offset.测试_Site6吸_Z.ToString();
                numericUpDown103.Text = offset.测试_Site7吸_Z.ToString();
                numericUpDown100.Text = offset.测试_Site8吸_Z.ToString();
                numericUpDown111.Text = offset.测试_Site9吸_Z.ToString();
                numericUpDown107.Text = offset.测试_Site10吸_Z.ToString();
                numericUpDown102.Text = offset.测试_Site11吸_Z.ToString();
                numericUpDown99.Text = offset.测试_Site12吸_Z.ToString();
                numericUpDown110.Text = offset.测试_Site13吸_Z.ToString();
                numericUpDown106.Text = offset.测试_Site14吸_Z.ToString();
                numericUpDown101.Text = offset.测试_Site15吸_Z.ToString();
                numericUpDown98.Text = offset.测试_Site16吸_Z.ToString();
                numericUpDown97.Text = offset.测试_Site1放_X.ToString();
                numericUpDown95.Text = offset.测试_Site2放_X.ToString();
                numericUpDown93.Text = offset.测试_Site3放_X.ToString();
                numericUpDown91.Text = offset.测试_Site4放_X.ToString();
                numericUpDown89.Text = offset.测试_Site5放_X.ToString();
                numericUpDown87.Text = offset.测试_Site6放_X.ToString();
                numericUpDown84.Text = offset.测试_Site7放_X.ToString();
                numericUpDown81.Text = offset.测试_Site8放_X.ToString();
                numericUpDown85.Text = offset.测试_Site9放_X.ToString();
                uiDoubleUpDown54.Text = offset.测试_Site10放_X.ToString();
                uiDoubleUpDown53.Text = offset.测试_Site11放_X.ToString();
                uiDoubleUpDown52.Text = offset.测试_Site12放_X.ToString();
                numericUpDown76.Text = offset.测试_Site13放_X.ToString();
                numericUpDown71.Text = offset.测试_Site14放_X.ToString();
                numericUpDown77.Text = offset.测试_Site15放_X.ToString();
                numericUpDown67.Text = offset.测试_Site16放_X.ToString();
                numericUpDown96.Text = offset.测试_Site1放_Z.ToString();
                numericUpDown94.Text = offset.测试_Site2放_Z.ToString();
                numericUpDown92.Text = offset.测试_Site3放_Z.ToString();
                numericUpDown90.Text = offset.测试_Site4放_Z.ToString();
                numericUpDown88.Text = offset.测试_Site5放_Z.ToString();
                numericUpDown86.Text = offset.测试_Site6放_Z.ToString();
                numericUpDown82.Text = offset.测试_Site7放_Z.ToString();
                numericUpDown80.Text = offset.测试_Site8放_Z.ToString();
                numericUpDown83.Text = offset.测试_Site9放_Z.ToString();
                numericUpDown79.Text = offset.测试_Site10放_Z.ToString();
                uiDoubleUpDown111.Text = offset.测试_Site11放_Z.ToString();
                uiDoubleUpDown110.Text = offset.测试_Site12放_Z.ToString();
                uiDoubleUpDown109.Text = offset.测试_Site13放_Z.ToString();
                numericUpDown69.Text = offset.测试_Site14放_Z.ToString();
                numericUpDown74.Text = offset.测试_Site15放_Z.ToString();
                numericUpDown65.Text = offset.测试_Site16放_Z.ToString();
            }
            else if (page == "梭车")
            {
                numericUpDown121.Text = offset.梭车2_前_Y.ToString();
                numericUpDown120.Text = offset.梭车2_后_Y.ToString();
            }
            else if (page == "抓盘手臂")
            {
                numericUpDown125.Text = offset.抓盘_Empty_X.ToString();
                numericUpDown124.Text = offset.抓盘_Load_X.ToString();
                numericUpDown123.Text = offset.抓盘_Auto1_X.ToString();
                numericUpDown122.Text = offset.抓盘_Auto2_X.ToString();
            }
        }

        private void 刷新开Site(string name)
        {

        }

        public void 刷新点位偏移(string name)
        {
            string result = GetSetupValue(name, "偏移点位名称", out string value);
            if (result != "Success") return;

            刷新手臂点位(value, uiTabControl8.SelectedTab.Text);
        }

        private void 刷新加热盘(HeatInfo heatInfo)
        {
            uiComboBox8.Items.Clear();
            uiComboBox8.Items.AddRange(new string[] { "平均数", "中位数", "最小值", "最大值", "去极值平均" });
            uiComboBox9.Items.Clear();
            uiComboBox9.Items.AddRange(new string[] { "PT100", "K" });

            textBox57.Text = heatInfo.HeatName;
            textBox60.Text = heatInfo.X_Num.ToString();
            textBox59.Text = heatInfo.Y_Num.ToString();
            textBox58.Text = heatInfo.X_Start.ToString();
            textBox56.Text = heatInfo.Y_Start.ToString();
            textBox55.Text = heatInfo.X_Pitch.ToString();
            textBox54.Text = heatInfo.Y_Pitch.ToString();
            textBox53.Text = heatInfo.Width.ToString();
            textBox52.Text = heatInfo.Length.ToString();
            textBox51.Text = heatInfo.Height.ToString();
            textBox24.Text = heatInfo.HeatName;
            numericUpDown129.Text = heatInfo.设定温度.ToString();
            numericUpDown130.Text = heatInfo.波动温度.ToString();
            numericUpDown134.Text = heatInfo.波动延时报警.ToString();
            numericUpDown154.Text = heatInfo.达温波动报警.ToString();
            numericUpDown131.Text = heatInfo.报警温度.ToString();
            numericUpDown132.Text = heatInfo.超温温度.ToString();
            numericUpDown133.Text = heatInfo.升温超时报警.ToString();
            uiComboBox8.Text = heatInfo.达温标准.ToString();
            uiComboBox9.Text = heatInfo.探头类型.ToString();
            numericUpDown135.Text = heatInfo.温控器温度补偿.ToString();
            numericUpDown136.Text = heatInfo.Site1温度补偿.ToString();
            numericUpDown137.Text = heatInfo.Site2温度补偿.ToString();
            numericUpDown139.Text = heatInfo.Site3温度补偿.ToString();
            numericUpDown138.Text = heatInfo.Site4温度补偿.ToString();
            numericUpDown143.Text = heatInfo.Site5温度补偿.ToString();
            numericUpDown142.Text = heatInfo.Site6温度补偿.ToString();
            numericUpDown141.Text = heatInfo.Site7温度补偿.ToString();
            numericUpDown140.Text = heatInfo.Site8温度补偿.ToString();
            numericUpDown147.Text = heatInfo.Site9温度补偿.ToString();
            numericUpDown146.Text = heatInfo.Site10温度补偿.ToString();
            numericUpDown145.Text = heatInfo.Site11温度补偿.ToString();
            numericUpDown144.Text = heatInfo.Site12温度补偿.ToString();
            numericUpDown151.Text = heatInfo.Site13温度补偿.ToString();
            numericUpDown150.Text = heatInfo.Site14温度补偿.ToString();
            numericUpDown149.Text = heatInfo.Site15温度补偿.ToString();
            numericUpDown148.Text = heatInfo.Site16温度补偿.ToString();
        }

        private void 刷新梭车(string name)
        {

        }

        private void 刷新档案信息(string tabpagetext)
        {
            int rows = uiDataGridView1.SelectedIndex;
            if (rows >= 0)
            {
                uiCheckBox194.Visible = false;
                uiCheckBox195.Visible = false;
                uiCheckBox196.Visible = false;
                string name = uiDataGridView1.Rows[rows].Cells[1].Value.ToString();
                if (tabpagetext == "设定")
                {
                    刷新设定(name);
                    uiCheckBox194.Visible = true;
                    uiCheckBox195.Visible = true;
                    uiCheckBox196.Visible = true;
                }
                else if (tabpagetext == "测试机")
                {
                    刷新测试机(name);
                }
                else if (tabpagetext == "分BIN")
                {
                    刷新分BIN(name);
                }
                else if (tabpagetext == "开Site")
                {
                    刷新开Site(name);
                }
                else if (tabpagetext == "点位偏移")
                {
                    刷新点位偏移(name);
                }
                else if (tabpagetext == "梭车")
                {
                    刷新梭车(name);
                }
            }
        }

        private void 刷新Tray盘(string name)
        {
            string result = SQLiteHelper.runSql(SQLiteConnString, $"select * from TrayInfo where name = '{name}'", out DataTable dt);
            if (result == "Success")
            {
                textBox30.Text = dt.Rows[0]["name"].ToString();
                textBox29.Text = dt.Rows[0]["xnum"].ToString();
                textBox28.Text = dt.Rows[0]["ynum"].ToString();
                textBox27.Text = dt.Rows[0]["xstart"].ToString();
                textBox31.Text = dt.Rows[0]["ystart"].ToString();
                textBox33.Text = dt.Rows[0]["xpitch"].ToString();
                textBox32.Text = dt.Rows[0]["ypitch"].ToString();
                textBox36.Text = dt.Rows[0]["width"].ToString();
                textBox35.Text = dt.Rows[0]["length"].ToString();
                textBox34.Text = dt.Rows[0]["height"].ToString();
            }
        }

        private void 刷新取放设定(string name)
        {
            string result = SQLiteHelper.runSql(SQLiteConnString, $"select * from TrayInfo where name = '{name}'", out DataTable dt);
            if (result == "Success")
            {
                //uiComboBox1.Text = dt.Rows[0][0].ToString();
                //textBox25.Text = dt.Rows[0][1].ToString();
            }
        }

        private void 刷新Tray盘信息(string tabpagetext)
        {
            int rows = uiDataGridView2.SelectedIndex;
            if (rows >= 0)
            {
                uiCheckBox197.Visible = false;
                uiCheckBox198.Visible = false;
                uiCheckBox199.Visible = false;
                string name = uiDataGridView2.Rows[rows].Cells[1].Value.ToString();
                if (tabpagetext == "Tray盘")
                {
                    uiCheckBox197.Visible = true;
                    uiCheckBox198.Visible = true;
                    uiCheckBox199.Visible = true;
                    刷新Tray盘(name);
                }
                else if (tabpagetext == "取放设定")
                {
                    //刷新取放设定(name);
                }
            }
        }

        private void 刷新加热盘信息(string tabpagetext)
        {
            int rows = uiDataGridView3.SelectedIndex;
            if (rows >= 0)
            {
                string name = uiDataGridView3.Rows[rows].Cells[1].Value.ToString();

                string result = GetHeatInfo(name, out HeatInfo heatInfo);

                uiCheckBox200.Visible = false;
                uiCheckBox201.Visible = false;
                uiCheckBox202.Visible = false;

                if (result == "Success")
                {
                    if (tabpagetext == "加热盘")
                    {
                        uiCheckBox200.Visible = true;
                        uiCheckBox201.Visible = true;
                        uiCheckBox202.Visible = true;
                        刷新加热盘(heatInfo);
                    }
                    else if (tabpagetext == "温度设定")
                    {
                        刷新加热盘(heatInfo);
                    }
                }
            }
        }

        private string 刷新档案列表()
        {
            string result = SQLiteHelper.runSql(SQLiteConnString, "select '' as ' ', 档案名称 from SetupInfo", out DataTable dt);
            if (result == "Success")
            {
                uiDataGridView1.DataSource = dt;
                uiDataGridView1.Columns[0].Width = 100;
                uiDataGridView1.Columns[1].Width = 250;

                uiDataGridView1.Columns.Cast<DataGridViewColumn>().ToList().ForEach(column => column.SortMode = DataGridViewColumnSortMode.NotSortable);

                result = SQLiteHelper.runSql(SQLiteConnString, "select value from config where name='当前档案'", out string value).ToString();

                if (result == "Success")
                {
                    foreach (DataGridViewRow dr in uiDataGridView1.Rows)
                    {
                        if (dr.Cells[1].Value.Equals(value))
                        {
                            dr.Cells[0].Value = "当前档案";
                            dr.Cells[0].Selected = true;
                        }
                    }

                    刷新档案信息(uiTabControl6.SelectedTab.Text);

                    return "Success";
                }
                else return result;
            }
            else
            {
                RunLog($"刷新档案列表失败: {result}");
                return result;
            }
        }
        private void 刷新Tray盘列表()
        {
            string reuslt = SQLiteHelper.runSql(SQLiteConnString, "select '' as ' ', name as Tray盘名称 from TrayInfo", out DataTable dt);
            if (reuslt == "Success")
            {
                uiDataGridView2.DataSource = dt;
                uiDataGridView2.Columns[0].Width = 100;
                uiDataGridView2.Columns[1].Width = 250;

                uiDataGridView2.Columns.Cast<DataGridViewColumn>().ToList().ForEach(column => column.SortMode = DataGridViewColumnSortMode.NotSortable);

                string result = SQLiteHelper.runSql(SQLiteConnString, $"select tray盘名称 from SetupInfo where 档案名称='{设置信息.档案名称}'", out string value).ToString();

                if (result == "Success")
                {
                    foreach (DataGridViewRow dr in uiDataGridView2.Rows)
                    {
                        if (dr.Cells[1].Value.Equals(value))
                        {
                            dr.Cells[0].Value = "当前Tray盘";
                            dr.Cells[0].Selected = true;
                        }
                    }
                }
                刷新Tray盘信息(uiTabControl7.SelectedTab.Text);
            }
        }
        private void 刷新加热盘列表()
        {
            string reuslt = SQLiteHelper.runSql(SQLiteConnString, "select '' as ' ', name as 加热盘名称 from HeatInfo", out DataTable dt);
            if (reuslt == "Success")
            {
                uiDataGridView3.DataSource = dt;
                uiDataGridView3.Columns[0].Width = 100;
                uiDataGridView3.Columns[1].Width = 250;

                uiDataGridView3.Columns.Cast<DataGridViewColumn>().ToList().ForEach(column => column.SortMode = DataGridViewColumnSortMode.NotSortable);

                string result = SQLiteHelper.runSql(SQLiteConnString, $"select 加热盘名称 from SetupInfo where 档案名称='{设置信息.档案名称}'", out string value).ToString();

                if (result == "Success")
                {
                    foreach (DataGridViewRow dr in uiDataGridView3.Rows)
                    {
                        if (dr.Cells[1].Value.Equals(value))
                        {
                            dr.Cells[0].Value = "当前加热盘";
                            dr.Cells[0].Selected = true;
                        }
                    }
                }
                刷新加热盘信息(uiTabControl9.SelectedTab.Text);
            }
        }
        private void 刷新产品(string tabpagetext)
        {
            if (tabpagetext == "档案列表")
            {
                刷新档案列表();
            }
            else if (tabpagetext == "Tray盘列表")
            {
                刷新Tray盘列表();
            }
            else if (tabpagetext == "加热盘列表")
            {
                刷新加热盘列表();
            }
        }

        private void uiTabControl2_Selecting(object sender, TabControlCancelEventArgs e)
        {
            刷新初始点位(e.TabPage.Text);
        }

        private void 刷新设置与维护界面(string tabpagetext)
        {
            if (tabpagetext == "初始点位")
            {
                刷新初始点位(uiTabControl2.SelectedTab.Text);
            }
            else if (tabpagetext == "产品")
            {
                刷新产品(uiTabControl5.SelectedTab.Text);
            }
        }

        private void uiTabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            刷新设置与维护界面(e.TabPage.Text);
        }

        public void 刷新设置与维护界面()
        {
            刷新设置与维护界面(uiTabControl1.SelectedTab.Text);
        }

        public bool UpdateInitPointData(string pointname, double newvalue, double oldvalue)
        {
            if (newvalue != oldvalue)
            {
                int line = SQLiteHelper.runSqlNonQuery(SQLiteConnString, $"update InitPoint set value = '{newvalue}' where ptname = '{pointname}'");
                if (line >= 1)
                {
                    RunLog($"初始点位:[{pointname}]{oldvalue}->{newvalue}:更新成功", WriteData.LogType.ParamLog);
                    return true;
                }
                else RunLog($"初始点位:[{pointname}]{oldvalue}->{newvalue}:更新失败", WriteData.LogType.ParamLog);
            }
            return false;
        }

        public bool UpdateRunInfoData(string pointname, string valuename, double newvalue, double oldvalue)
        {
            if (newvalue != oldvalue)
            {
                int line = SQLiteHelper.runSqlNonQuery(SQLiteConnString, $"update AxisRunInfo set {valuename} = '{newvalue}' where 轴名称 = '{pointname}'");
                if (line >= 1)
                {
                    RunLog($"运行参数:[{pointname}:{valuename}]{oldvalue}->{newvalue}:更新成功", WriteData.LogType.ParamLog);
                    return true;
                }
                else RunLog($"运行参数:[{pointname}:{valuename}]{oldvalue}->{newvalue}:更新失败", WriteData.LogType.ParamLog);
            }
            return false;
        }


        #region 初始点位

        /// <summary>
        /// 保存入料手臂速度参数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton10_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("请确认是否保存!", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                if (UpdateInitPointData("入料_Load_X", Convert.ToDouble(uiDoubleUpDown99.Text), 初始点位.入料_Load_X))
                {
                    初始点位.入料_Load_X = Convert.ToDouble(uiDoubleUpDown99.Value);
                    LoaderTrayICPointInit();
                }
                if (UpdateInitPointData("入料_Load_Y", Convert.ToDouble(uiDoubleUpDown97.Text), 初始点位.入料_Load_Y))
                {
                    初始点位.入料_Load_Y = Convert.ToDouble(uiDoubleUpDown97.Value);
                    LoaderTrayICPointInit();
                }
                if (UpdateInitPointData("入料_Load_Z", Convert.ToDouble(uiDoubleUpDown98.Text), 初始点位.入料_Load_Z)) 初始点位.入料_Load_Z = Convert.ToDouble(uiDoubleUpDown98.Value);
                if (UpdateInitPointData("入料_Heat_X", Convert.ToDouble(uiDoubleUpDown108.Text), 初始点位.入料_Heat_X))
                {
                    初始点位.入料_Heat_X = Convert.ToDouble(uiDoubleUpDown108.Value);
                    HeatTrayICPointInit();
                }
                if (UpdateInitPointData("入料_Heat_Y", Convert.ToDouble(uiDoubleUpDown107.Text), 初始点位.入料_Heat_Y))
                {
                    初始点位.入料_Heat_Y = Convert.ToDouble(uiDoubleUpDown107.Value);
                    HeatTrayICPointInit();
                }
                if (UpdateInitPointData("入料_Heat吸_Z", Convert.ToDouble(uiDoubleUpDown106.Text), 初始点位.入料_Heat吸_Z)) 初始点位.入料_Heat吸_Z = Convert.ToDouble(uiDoubleUpDown106.Value);
                if (UpdateInitPointData("入料_Heat放_Z", Convert.ToDouble(uinumericUpDown126.Text), 初始点位.入料_Heat放_Z)) 初始点位.入料_Heat放_Z = Convert.ToDouble(uinumericUpDown126.Value);

                if (UpdateInitPointData("入料_梭车1_X", Convert.ToDouble(uiDoubleUpDown102.Text), 初始点位.入料_梭车1_X)) 初始点位.入料_梭车1_X = Convert.ToDouble(uiDoubleUpDown102.Value);
                if (UpdateInitPointData("入料_梭车1_Y", Convert.ToDouble(uiDoubleUpDown101.Text), 初始点位.入料_梭车1_Y)) 初始点位.入料_梭车1_Y = Convert.ToDouble(uiDoubleUpDown101.Value);
                if (UpdateInitPointData("入料_梭车1_Z", Convert.ToDouble(uiDoubleUpDown100.Text), 初始点位.入料_梭车1_Z)) 初始点位.入料_梭车1_Z = Convert.ToDouble(uiDoubleUpDown100.Value);
                if (UpdateInitPointData("入料_梭车2_X", Convert.ToDouble(uiDoubleUpDown114.Text), 初始点位.入料_梭车2_X)) 初始点位.入料_梭车2_X = Convert.ToDouble(uiDoubleUpDown114.Value);
                if (UpdateInitPointData("入料_梭车2_Y", Convert.ToDouble(uiDoubleUpDown113.Text), 初始点位.入料_梭车2_Y)) 初始点位.入料_梭车2_Y = Convert.ToDouble(uiDoubleUpDown113.Value);
                if (UpdateInitPointData("入料_梭车2_Z", Convert.ToDouble(uiDoubleUpDown112.Text), 初始点位.入料_梭车2_Z)) 初始点位.入料_梭车2_Z = Convert.ToDouble(uiDoubleUpDown112.Value);

                if (UpdateInitPointData("入料_变距最小值", Convert.ToDouble(uiDoubleUpDown105.Text), 初始点位.入料_变距最小值))
                {
                    初始点位.入料_变距最小值 = Convert.ToDouble(uiDoubleUpDown105.Value);
                    LoaderTrayICPointInit();
                    HeatTrayICPointInit();
                }
                if (UpdateInitPointData("入料_变距最大值", Convert.ToDouble(uiDoubleUpDown104.Text), 初始点位.入料_变距最大值))
                {
                    初始点位.入料_变距最大值 = Convert.ToDouble(uiDoubleUpDown104.Value);
                    LoaderTrayICPointInit();
                    HeatTrayICPointInit();
                }
                if (UpdateInitPointData("入料_梭车列间隔", Convert.ToDouble(uiDoubleUpDown103.Text), 初始点位.入料_梭车列间隔)) 初始点位.入料_梭车列间隔 = Convert.ToDouble(uiDoubleUpDown103.Value);
                if (UpdateInitPointData("入料_Z1_硬件补偿", Convert.ToDouble(uiDoubleUpDown18.Text), 初始点位.入料_Z1_硬件补偿)) 初始点位.入料_Z1_硬件补偿 = Convert.ToDouble(uiDoubleUpDown18.Value);
                if (UpdateInitPointData("入料_Z2_硬件补偿", Convert.ToDouble(uiDoubleUpDown17.Text), 初始点位.入料_Z2_硬件补偿)) 初始点位.入料_Z2_硬件补偿 = Convert.ToDouble(uiDoubleUpDown17.Value);
                if (UpdateInitPointData("入料_Z3_硬件补偿", Convert.ToDouble(uiDoubleUpDown16.Text), 初始点位.入料_Z3_硬件补偿)) 初始点位.入料_Z3_硬件补偿 = Convert.ToDouble(uiDoubleUpDown16.Value);
                if (UpdateInitPointData("入料_Z4_硬件补偿", Convert.ToDouble(uiDoubleUpDown19.Text), 初始点位.入料_Z4_硬件补偿)) 初始点位.入料_Z4_硬件补偿 = Convert.ToDouble(uiDoubleUpDown19.Value);

                GetAxisRunInfo("M22_入料手臂X轴", out AxisRunInfo axisRun);
                if (UpdateRunInfoData("M22_入料手臂X轴", "最大速度", Convert.ToDouble(uiDoubleUpDown9.Text), axisRun.最大速度)) axisRun.最大速度 = Convert.ToDouble(uiDoubleUpDown9.Value);
                if (UpdateRunInfoData("M22_入料手臂X轴", "加速度", Convert.ToDouble(uiDoubleUpDown8.Text), axisRun.加速度)) axisRun.加速度 = Convert.ToDouble(uiDoubleUpDown8.Value);
                if (UpdateRunInfoData("M22_入料手臂X轴", "减速度", Convert.ToDouble(uiDoubleUpDown7.Text), axisRun.减速度)) axisRun.减速度 = Convert.ToDouble(uiDoubleUpDown7.Value);


                GetAxisRunInfo("M16_入料手臂Y轴", out axisRun);
                if (UpdateRunInfoData("M16_入料手臂Y轴", "最大速度", Convert.ToDouble(uiDoubleUpDown117.Text), axisRun.最大速度)) axisRun.最大速度 = Convert.ToDouble(uiDoubleUpDown117.Value);
                if (UpdateRunInfoData("M16_入料手臂Y轴", "加速度", Convert.ToDouble(uiDoubleUpDown116.Text), axisRun.加速度)) axisRun.加速度 = Convert.ToDouble(uiDoubleUpDown116.Value);
                if (UpdateRunInfoData("M16_入料手臂Y轴", "减速度", Convert.ToDouble(uiDoubleUpDown115.Text), axisRun.减速度)) axisRun.减速度 = Convert.ToDouble(uiDoubleUpDown115.Value);


                GetAxisRunInfo("M5_入料手臂P轴", out axisRun);
                if (UpdateRunInfoData("M5_入料手臂P轴", "最大速度", Convert.ToDouble(uiDoubleUpDown12.Text), axisRun.最大速度)) axisRun.最大速度 = Convert.ToDouble(uiDoubleUpDown12.Value);
                if (UpdateRunInfoData("M5_入料手臂P轴", "加速度", Convert.ToDouble(uiDoubleUpDown11.Text), axisRun.加速度)) axisRun.加速度 = Convert.ToDouble(uiDoubleUpDown11.Value);
                if (UpdateRunInfoData("M5_入料手臂P轴", "减速度", Convert.ToDouble(uiDoubleUpDown10.Text), axisRun.减速度)) axisRun.减速度 = Convert.ToDouble(uiDoubleUpDown10.Value);


                GetAxisRunInfo("M1_入料手臂Z1轴", out axisRun);
                if (UpdateRunInfoData("M1_入料手臂Z1轴", "最大速度", Convert.ToDouble(uiDoubleUpDown15.Text), axisRun.最大速度)) axisRun.最大速度 = Convert.ToDouble(uiDoubleUpDown15.Value);
                if (UpdateRunInfoData("M1_入料手臂Z1轴", "加速度", Convert.ToDouble(uiDoubleUpDown14.Text), axisRun.加速度)) axisRun.加速度 = Convert.ToDouble(uiDoubleUpDown14.Value);
                if (UpdateRunInfoData("M1_入料手臂Z1轴", "减速度", Convert.ToDouble(uiDoubleUpDown13.Text), axisRun.减速度)) axisRun.减速度 = Convert.ToDouble(uiDoubleUpDown13.Value);


                GetAxisRunInfo("M2_入料手臂Z2轴", out axisRun);
                if (UpdateRunInfoData("M2_入料手臂Z2轴", "最大速度", Convert.ToDouble(uiDoubleUpDown81.Text), axisRun.最大速度)) axisRun.最大速度 = Convert.ToDouble(uiDoubleUpDown81.Value);
                if (UpdateRunInfoData("M2_入料手臂Z2轴", "加速度", Convert.ToDouble(uiDoubleUpDown80.Text), axisRun.加速度)) axisRun.加速度 = Convert.ToDouble(uiDoubleUpDown80.Value);
                if (UpdateRunInfoData("M2_入料手臂Z2轴", "减速度", Convert.ToDouble(uiDoubleUpDown79.Text), axisRun.减速度)) axisRun.减速度 = Convert.ToDouble(uiDoubleUpDown79.Value);


                GetAxisRunInfo("M3_入料手臂Z3轴", out axisRun);
                if (UpdateRunInfoData("M3_入料手臂Z3轴", "最大速度", Convert.ToDouble(uiDoubleUpDown6.Text), axisRun.最大速度)) axisRun.最大速度 = Convert.ToDouble(uiDoubleUpDown6.Value);
                if (UpdateRunInfoData("M3_入料手臂Z3轴", "加速度", Convert.ToDouble(uiDoubleUpDown5.Text), axisRun.加速度)) axisRun.加速度 = Convert.ToDouble(uiDoubleUpDown5.Value);
                if (UpdateRunInfoData("M3_入料手臂Z3轴", "减速度", Convert.ToDouble(uiDoubleUpDown4.Text), axisRun.减速度)) axisRun.减速度 = Convert.ToDouble(uiDoubleUpDown4.Value);


                GetAxisRunInfo("M4_入料手臂Z4轴", out axisRun);
                if (UpdateRunInfoData("M4_入料手臂Z4轴", "最大速度", Convert.ToDouble(uiDoubleUpDown3.Text), axisRun.最大速度)) axisRun.最大速度 = Convert.ToDouble(uiDoubleUpDown3.Value);
                if (UpdateRunInfoData("M4_入料手臂Z4轴", "加速度", Convert.ToDouble(uiDoubleUpDown2.Text), axisRun.加速度)) axisRun.加速度 = Convert.ToDouble(uiDoubleUpDown2.Value);
                if (UpdateRunInfoData("M4_入料手臂Z4轴", "减速度", Convert.ToDouble(uiDoubleUpDown1.Text), axisRun.减速度)) axisRun.减速度 = Convert.ToDouble(uiDoubleUpDown1.Value);
            }
        }

        /// <summary>
        /// 入料点位恢复默认值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton7_Click(object sender, EventArgs e)
        {
            刷新入料手臂初始点位();
        }

        /// <summary>
        /// 入料速度恢复默认值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton9_Click(object sender, EventArgs e)
        {
            刷新入料手臂初始点位();
            刷新入料手臂速度参数();
        }

        private void uiButton6_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("请确认是否保存!", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                if (UpdateInitPointData("出料_梭车1_X", Convert.ToDouble(uiDoubleUpDown75.Text), 初始点位.出料_梭车1_X)) 初始点位.出料_梭车1_X = Convert.ToDouble(uiDoubleUpDown75.Text);
                if (UpdateInitPointData("出料_梭车1_Y", Convert.ToDouble(uiDoubleUpDown74.Text), 初始点位.出料_梭车1_Y)) 初始点位.出料_梭车1_Y = Convert.ToDouble(uiDoubleUpDown74.Text);
                if (UpdateInitPointData("出料_梭车1_Z", Convert.ToDouble(uiDoubleUpDown73.Text), 初始点位.出料_梭车1_Z)) 初始点位.出料_梭车1_Z = Convert.ToDouble(uiDoubleUpDown73.Text);
                if (UpdateInitPointData("出料_Auto1_X", Convert.ToDouble(uiDoubleUpDown90.Text), 初始点位.出料_Auto1_X))
                {
                    初始点位.出料_Auto1_X = Convert.ToDouble(uiDoubleUpDown90.Text);
                    Auto1TrayICPointInit();
                }
                if (UpdateInitPointData("出料_Auto1_Y", Convert.ToDouble(uiDoubleUpDown89.Text), 初始点位.出料_Auto1_Y))
                {
                    初始点位.出料_Auto1_Y = Convert.ToDouble(uiDoubleUpDown89.Text);
                    Auto1TrayICPointInit();
                }
                if (UpdateInitPointData("出料_Auto1_Z", Convert.ToDouble(uiDoubleUpDown88.Text), 初始点位.出料_Auto1_Z)) 初始点位.出料_Auto1_Z = Convert.ToDouble(uiDoubleUpDown88.Text);

                if (UpdateInitPointData("出料_Auto2_X", Convert.ToDouble(uiDoubleUpDown93.Text), 初始点位.出料_Auto2_X))
                {
                    初始点位.出料_Auto2_X = Convert.ToDouble(uiDoubleUpDown93.Text);
                    Auto2TrayICPointInit();
                }
                if (UpdateInitPointData("出料_Auto2_Y", Convert.ToDouble(uiDoubleUpDown92.Text), 初始点位.出料_Auto2_Y))
                {
                    初始点位.出料_Auto2_Y = Convert.ToDouble(uiDoubleUpDown92.Text);
                    Auto2TrayICPointInit();
                }
                if (UpdateInitPointData("出料_Auto2_Z", Convert.ToDouble(uiDoubleUpDown91.Text), 初始点位.出料_Auto2_Z)) 初始点位.出料_Auto2_Z = Convert.ToDouble(uiDoubleUpDown91.Text);


                if (UpdateInitPointData("出料_梭车2_X", Convert.ToDouble(uiDoubleUpDown78.Text), 初始点位.出料_梭车2_X)) 初始点位.出料_梭车2_X = Convert.ToDouble(uiDoubleUpDown78.Text);
                if (UpdateInitPointData("出料_梭车2_Y", Convert.ToDouble(uiDoubleUpDown77.Text), 初始点位.出料_梭车2_Y)) 初始点位.出料_梭车2_Y = Convert.ToDouble(uiDoubleUpDown77.Text);
                if (UpdateInitPointData("出料_梭车2_Z", Convert.ToDouble(uiDoubleUpDown76.Text), 初始点位.出料_梭车2_Z)) 初始点位.出料_梭车2_Z = Convert.ToDouble(uiDoubleUpDown76.Text);


                if (UpdateInitPointData("出料_Fix1_X", Convert.ToDouble(uiDoubleUpDown87.Text), 初始点位.出料_Fix1_X))
                {
                    初始点位.出料_Fix1_X = Convert.ToDouble(uiDoubleUpDown87.Text);
                    Fix1TrayICPointInit();
                }
                if (UpdateInitPointData("出料_Fix1_Y", Convert.ToDouble(uiDoubleUpDown86.Text), 初始点位.出料_Fix1_Y))
                {
                    初始点位.出料_Fix1_Y = Convert.ToDouble(uiDoubleUpDown86.Text);
                    Fix1TrayICPointInit();
                }
                if (UpdateInitPointData("出料_Fix1_Z", Convert.ToDouble(uiDoubleUpDown85.Text), 初始点位.出料_Fix1_Z)) 初始点位.出料_Fix1_Z = Convert.ToDouble(uiDoubleUpDown85.Text);

                if (UpdateInitPointData("出料_Fix3_X", Convert.ToDouble(uiDoubleUpDown84.Text), 初始点位.出料_Fix3_X))
                {
                    初始点位.出料_Fix3_X = Convert.ToDouble(uiDoubleUpDown84.Text);
                    Fix3TrayICPointInit();
                }
                if (UpdateInitPointData("出料_Fix3_Y", Convert.ToDouble(uiDoubleUpDown83.Text), 初始点位.出料_Fix3_Y))
                {
                    初始点位.出料_Fix3_Y = Convert.ToDouble(uiDoubleUpDown83.Text);
                    Fix3TrayICPointInit();
                }
                if (UpdateInitPointData("出料_Fix3_Z", Convert.ToDouble(uiDoubleUpDown82.Text), 初始点位.出料_Fix3_Z)) 初始点位.出料_Fix3_Z = Convert.ToDouble(uiDoubleUpDown82.Text);
                if (UpdateInitPointData("出料_Fix3_右移动", Convert.ToDouble(uiDoubleUpDown126.Text), 初始点位.出料_Fix3_右移动)) 初始点位.出料_Fix3_右移动 = Convert.ToDouble(uiDoubleUpDown126.Text);

                if (UpdateInitPointData("出料_Fix2_X", Convert.ToDouble(uiDoubleUpDown96.Text), 初始点位.出料_Fix2_X))
                {
                    初始点位.出料_Fix2_X = Convert.ToDouble(uiDoubleUpDown96.Text);
                    Fix2TrayICPointInit();
                }
                if (UpdateInitPointData("出料_Fix2_Y", Convert.ToDouble(uiDoubleUpDown95.Text), 初始点位.出料_Fix2_Y))
                {
                    初始点位.出料_Fix2_Y = Convert.ToDouble(uiDoubleUpDown95.Text);
                    Fix2TrayICPointInit();
                }
                if (UpdateInitPointData("出料_Fix2_Z", Convert.ToDouble(uiDoubleUpDown94.Text), 初始点位.出料_Fix2_Z)) 初始点位.出料_Fix2_Z = Convert.ToDouble(uiDoubleUpDown94.Text);

                if (UpdateInitPointData("出料_变距最小值", Convert.ToDouble(uiDoubleUpDown125.Text), 初始点位.出料_变距最小值))
                {
                    初始点位.出料_变距最小值 = Convert.ToDouble(uiDoubleUpDown125.Text);
                    Auto1TrayICPointInit();
                    Auto2TrayICPointInit();
                    Fix1TrayICPointInit();
                    Fix2TrayICPointInit();
                    Fix3TrayICPointInit();
                }
                if (UpdateInitPointData("出料_变距最大值", Convert.ToDouble(uiDoubleUpDown124.Text), 初始点位.出料_变距最大值))
                {
                    初始点位.出料_变距最大值 = Convert.ToDouble(uiDoubleUpDown124.Text);
                    Auto1TrayICPointInit();
                    Auto2TrayICPointInit();
                    Fix1TrayICPointInit();
                    Fix2TrayICPointInit();
                    Fix3TrayICPointInit();
                }
                if (UpdateInitPointData("出料_梭车列间隔", Convert.ToDouble(uiDoubleUpDown123.Text), 初始点位.入料_梭车列间隔)) 初始点位.出料_梭车列间隔 = Convert.ToDouble(uiDoubleUpDown123.Text);
                if (UpdateInitPointData("出料_Z1_硬件补偿", Convert.ToDouble(uiDoubleUpDown122.Text), 初始点位.入料_Z1_硬件补偿)) 初始点位.出料_Z1_硬件补偿 = Convert.ToDouble(uiDoubleUpDown122.Text);
                if (UpdateInitPointData("出料_Z2_硬件补偿", Convert.ToDouble(uiDoubleUpDown121.Text), 初始点位.入料_Z2_硬件补偿)) 初始点位.出料_Z2_硬件补偿 = Convert.ToDouble(uiDoubleUpDown121.Text);
                if (UpdateInitPointData("出料_Z3_硬件补偿", Convert.ToDouble(uiDoubleUpDown21.Text), 初始点位.出料_Z3_硬件补偿)) 初始点位.出料_Z3_硬件补偿 = Convert.ToDouble(uiDoubleUpDown21.Text);
                if (UpdateInitPointData("出料_Z4_硬件补偿", Convert.ToDouble(uiDoubleUpDown20.Text), 初始点位.出料_Z4_硬件补偿)) 初始点位.出料_Z4_硬件补偿 = Convert.ToDouble(uiDoubleUpDown20.Text);

                GetAxisRunInfo("M23_出料手臂X轴", out AxisRunInfo axisRun);
                if (UpdateRunInfoData("M23_出料手臂X轴", "最大速度", Convert.ToDouble(uiDoubleUpDown132.Text), axisRun.最大速度)) axisRun.最大速度 = Convert.ToDouble(uiDoubleUpDown132.Text);
                if (UpdateRunInfoData("M23_出料手臂X轴", "加速度", Convert.ToDouble(uiDoubleUpDown131.Text), axisRun.加速度)) axisRun.加速度 = Convert.ToDouble(uiDoubleUpDown131.Text);
                if (UpdateRunInfoData("M23_出料手臂X轴", "减速度", Convert.ToDouble(uiDoubleUpDown130.Text), axisRun.减速度)) axisRun.减速度 = Convert.ToDouble(uiDoubleUpDown130.Text);


                GetAxisRunInfo("M17_出料手臂Y轴", out axisRun);
                if (UpdateRunInfoData("M17_出料手臂Y轴", "最大速度", Convert.ToDouble(uiDoubleUpDown144.Text), axisRun.最大速度)) axisRun.最大速度 = Convert.ToDouble(uiDoubleUpDown144.Text);
                if (UpdateRunInfoData("M17_出料手臂Y轴", "加速度", Convert.ToDouble(uiDoubleUpDown143.Text), axisRun.加速度)) axisRun.加速度 = Convert.ToDouble(uiDoubleUpDown143.Text);
                if (UpdateRunInfoData("M17_出料手臂Y轴", "减速度", Convert.ToDouble(uiDoubleUpDown142.Text), axisRun.减速度)) axisRun.减速度 = Convert.ToDouble(uiDoubleUpDown142.Text);


                GetAxisRunInfo("M10_出料手臂P轴", out axisRun);
                if (UpdateRunInfoData("M10_出料手臂P轴", "最大速度", Convert.ToDouble(uiDoubleUpDown135.Text), axisRun.最大速度)) axisRun.最大速度 = Convert.ToDouble(uiDoubleUpDown135.Text);
                if (UpdateRunInfoData("M10_出料手臂P轴", "加速度", Convert.ToDouble(uiDoubleUpDown134.Text), axisRun.加速度)) axisRun.加速度 = Convert.ToDouble(uiDoubleUpDown134.Text);
                if (UpdateRunInfoData("M10_出料手臂P轴", "减速度", Convert.ToDouble(uiDoubleUpDown133.Text), axisRun.减速度)) axisRun.减速度 = Convert.ToDouble(uiDoubleUpDown133.Text);


                GetAxisRunInfo("M6_出料手臂Z1轴", out axisRun);
                if (UpdateRunInfoData("M6_出料手臂Z1轴", "最大速度", Convert.ToDouble(uiDoubleUpDown138.Text), axisRun.最大速度)) axisRun.最大速度 = Convert.ToDouble(uiDoubleUpDown138.Text);
                if (UpdateRunInfoData("M6_出料手臂Z1轴", "加速度", Convert.ToDouble(uiDoubleUpDown137.Text), axisRun.加速度)) axisRun.加速度 = Convert.ToDouble(uiDoubleUpDown137.Text);
                if (UpdateRunInfoData("M6_出料手臂Z1轴", "减速度", Convert.ToDouble(uiDoubleUpDown136.Text), axisRun.减速度)) axisRun.减速度 = Convert.ToDouble(uiDoubleUpDown136.Text);


                GetAxisRunInfo("M7_出料手臂Z2轴", out axisRun);
                if (UpdateRunInfoData("M7_出料手臂Z2轴", "最大速度", Convert.ToDouble(uiDoubleUpDown141.Text), axisRun.最大速度)) axisRun.最大速度 = Convert.ToDouble(uiDoubleUpDown141.Text);
                if (UpdateRunInfoData("M7_出料手臂Z2轴", "加速度", Convert.ToDouble(uiDoubleUpDown140.Text), axisRun.加速度)) axisRun.加速度 = Convert.ToDouble(uiDoubleUpDown140.Text);
                if (UpdateRunInfoData("M7_出料手臂Z2轴", "减速度", Convert.ToDouble(uiDoubleUpDown139.Text), axisRun.减速度)) axisRun.减速度 = Convert.ToDouble(uiDoubleUpDown139.Text);


                GetAxisRunInfo("M8_出料手臂Z3轴", out axisRun);
                if (UpdateRunInfoData("M8_出料手臂Z3轴", "最大速度", Convert.ToDouble(uiDoubleUpDown129.Text), axisRun.最大速度)) axisRun.最大速度 = Convert.ToDouble(uiDoubleUpDown129.Text);
                if (UpdateRunInfoData("M8_出料手臂Z3轴", "加速度", Convert.ToDouble(uiDoubleUpDown128.Text), axisRun.加速度)) axisRun.加速度 = Convert.ToDouble(uiDoubleUpDown128.Text);
                if (UpdateRunInfoData("M8_出料手臂Z3轴", "减速度", Convert.ToDouble(uiDoubleUpDown127.Text), axisRun.减速度)) axisRun.减速度 = Convert.ToDouble(uiDoubleUpDown127.Text);


                GetAxisRunInfo("M9_出料手臂Z4轴", out axisRun);
                if (UpdateRunInfoData("M9_出料手臂Z4轴", "最大速度", Convert.ToDouble(uiDoubleUpDown120.Text), axisRun.最大速度)) axisRun.最大速度 = Convert.ToDouble(uiDoubleUpDown120.Text);
                if (UpdateRunInfoData("M9_出料手臂Z4轴", "加速度", Convert.ToDouble(uiDoubleUpDown119.Text), axisRun.加速度)) axisRun.加速度 = Convert.ToDouble(uiDoubleUpDown119.Text);
                if (UpdateRunInfoData("M9_出料手臂Z4轴", "减速度", Convert.ToDouble(uiDoubleUpDown118.Text), axisRun.减速度)) axisRun.减速度 = Convert.ToDouble(uiDoubleUpDown118.Text);
            }
        }

        private void uiButton5_Click(object sender, EventArgs e)
        {
            刷新出料手臂初始点位();
            刷新出料手臂速度参数();
        }

        private void uiButton14_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("请确认是否保存!", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                if (UpdateInitPointData("测试_梭车1吸_X", Convert.ToDouble(uiDoubleUpDown62.Text), 初始点位.测试_梭车1吸_X)) 初始点位.测试_梭车1吸_X = Convert.ToDouble(uiDoubleUpDown62.Text);
                if (UpdateInitPointData("测试_梭车1吸_Z", Convert.ToDouble(uiDoubleUpDown61.Text), 初始点位.测试_梭车1吸_Z)) 初始点位.测试_梭车1吸_Z = Convert.ToDouble(uiDoubleUpDown61.Text);

                if (UpdateInitPointData("测试_梭车2吸_X", Convert.ToDouble(uiDoubleUpDown64.Text), 初始点位.测试_梭车2吸_X)) 初始点位.测试_梭车2吸_X = Convert.ToDouble(uiDoubleUpDown64.Text);
                if (UpdateInitPointData("测试_梭车2吸_Z", Convert.ToDouble(uiDoubleUpDown63.Text), 初始点位.测试_梭车2吸_Z)) 初始点位.测试_梭车2吸_Z = Convert.ToDouble(uiDoubleUpDown63.Text);

                if (UpdateInitPointData("测试_Test1区_X", Convert.ToDouble(uiDoubleUpDown66.Text), 初始点位.测试_Test1区_X)) 初始点位.测试_Test1区_X = Convert.ToDouble(uiDoubleUpDown66.Text);
                if (UpdateInitPointData("测试_Test2区_X", Convert.ToDouble(uiDoubleUpDown65.Text), 初始点位.测试_Test2区_X)) 初始点位.测试_Test2区_X = Convert.ToDouble(uiDoubleUpDown65.Text);
                if (UpdateInitPointData("测试_Test3区_X", Convert.ToDouble(uiDoubleUpDown68.Text), 初始点位.测试_Test3区_X)) 初始点位.测试_Test3区_X = Convert.ToDouble(uiDoubleUpDown68.Text);
                if (UpdateInitPointData("测试_Test4区_X", Convert.ToDouble(uiDoubleUpDown67.Text), 初始点位.测试_Test4区_X)) 初始点位.测试_Test4区_X = Convert.ToDouble(uiDoubleUpDown67.Text);


                if (UpdateInitPointData("测试_梭车1放_X", Convert.ToDouble(uiDoubleUpDown70.Text), 初始点位.测试_梭车1放_X)) 初始点位.测试_梭车1放_X = Convert.ToDouble(uiDoubleUpDown70.Text);
                if (UpdateInitPointData("测试_梭车1放_Z", Convert.ToDouble(uiDoubleUpDown69.Text), 初始点位.测试_梭车1放_Z)) 初始点位.测试_梭车1放_Z = Convert.ToDouble(uiDoubleUpDown69.Text);

                if (UpdateInitPointData("测试_梭车2放_X", Convert.ToDouble(uiDoubleUpDown72.Text), 初始点位.测试_梭车2放_X)) 初始点位.测试_梭车2放_X = Convert.ToDouble(uiDoubleUpDown72.Text);
                if (UpdateInitPointData("测试_梭车2放_Z", Convert.ToDouble(uiDoubleUpDown71.Text), 初始点位.测试_梭车2放_Z)) 初始点位.测试_梭车2放_Z = Convert.ToDouble(uiDoubleUpDown71.Text);

                if (UpdateInitPointData("测试_Test区Site间隔", Convert.ToDouble(uiDoubleUpDown23.Text), 初始点位.测试_Test区Site间隔)) 初始点位.测试_Test区Site间隔 = Convert.ToDouble(uiDoubleUpDown23.Text);
                if (UpdateInitPointData("测试_Test区吸_Z", Convert.ToDouble(uiDoubleUpDown25.Text), 初始点位.测试_Test区吸_Z)) 初始点位.测试_Test区吸_Z = Convert.ToDouble(uiDoubleUpDown25.Text);
                if (UpdateInitPointData("测试_Test区放_Z", Convert.ToDouble(uiDoubleUpDown27.Text), 初始点位.测试_Test区放_Z)) 初始点位.测试_Test区放_Z = Convert.ToDouble(uiDoubleUpDown27.Text);


                GetAxisRunInfo("M21_测试手臂X轴", out AxisRunInfo axisRun);
                if (UpdateRunInfoData("M21_测试手臂X轴", "最大速度", Convert.ToDouble(uiDoubleUpDown45.Text), axisRun.最大速度)) axisRun.最大速度 = Convert.ToDouble(uiDoubleUpDown45.Text);
                if (UpdateRunInfoData("M21_测试手臂X轴", "加速度", Convert.ToDouble(uiDoubleUpDown44.Text), axisRun.加速度)) axisRun.加速度 = Convert.ToDouble(uiDoubleUpDown44.Text);
                if (UpdateRunInfoData("M21_测试手臂X轴", "减速度", Convert.ToDouble(uiDoubleUpDown43.Text), axisRun.减速度)) axisRun.减速度 = Convert.ToDouble(uiDoubleUpDown43.Text);


                GetAxisRunInfo("M19_测试手臂Z轴", out axisRun);
                if (UpdateRunInfoData("M19_测试手臂Z轴", "最大速度", Convert.ToDouble(uiDoubleUpDown48.Text), axisRun.最大速度)) axisRun.最大速度 = Convert.ToDouble(uiDoubleUpDown48.Text);
                if (UpdateRunInfoData("M19_测试手臂Z轴", "加速度", Convert.ToDouble(uiDoubleUpDown47.Text), axisRun.加速度)) axisRun.加速度 = Convert.ToDouble(uiDoubleUpDown47.Text);
                if (UpdateRunInfoData("M19_测试手臂Z轴", "减速度", Convert.ToDouble(uiDoubleUpDown46.Text), axisRun.减速度)) axisRun.减速度 = Convert.ToDouble(uiDoubleUpDown46.Text);
            }
        }

        private void uiButton13_Click(object sender, EventArgs e)
        {
            刷新测试手臂初始点位();
            刷新测试手臂速度参数();
        }

        private void uiButton18_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("请确认是否保存!", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                if (UpdateInitPointData("梭车2_前_Y", Convert.ToDouble(uiDoubleUpDown57.Text), 初始点位.梭车2_前_Y)) 初始点位.梭车2_前_Y = Convert.ToDouble(uiDoubleUpDown57.Text);
                if (UpdateInitPointData("梭车2_后_Y", Convert.ToDouble(uiDoubleUpDown56.Text), 初始点位.梭车2_后_Y)) 初始点位.梭车2_后_Y = Convert.ToDouble(uiDoubleUpDown56.Text);

                GetAxisRunInfo("M20_飞梭轴", out AxisRunInfo axisRun);
                if (UpdateRunInfoData("M20_飞梭轴", "最大速度", Convert.ToDouble(uiDoubleUpDown51.Text), axisRun.最大速度)) axisRun.最大速度 = Convert.ToDouble(uiDoubleUpDown51.Text);
                if (UpdateRunInfoData("M20_飞梭轴", "加速度", Convert.ToDouble(uiDoubleUpDown50.Text), axisRun.加速度)) axisRun.加速度 = Convert.ToDouble(uiDoubleUpDown50.Text);
                if (UpdateRunInfoData("M20_飞梭轴", "减速度", Convert.ToDouble(uiDoubleUpDown49.Text), axisRun.减速度)) axisRun.减速度 = Convert.ToDouble(uiDoubleUpDown49.Text);
            }
        }

        private void uiButton17_Click(object sender, EventArgs e)
        {
            刷新梭车初始点位();
            刷新梭车速度参数();
        }

        private void uiButton22_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("请确认是否保存!", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                if (UpdateInitPointData("抓盘_Empty_X", Convert.ToDouble(uiDoubleUpDown55.Text), 初始点位.抓盘_Empty_X)) 初始点位.抓盘_Empty_X = Convert.ToDouble(uiDoubleUpDown55.Text);
                if (UpdateInitPointData("抓盘_Load_X", Convert.ToDouble(uiDoubleUpDown58.Text), 初始点位.抓盘_Load_X)) 初始点位.抓盘_Load_X = Convert.ToDouble(uiDoubleUpDown58.Text);
                if (UpdateInitPointData("抓盘_Auto1_X", Convert.ToDouble(uiDoubleUpDown60.Text), 初始点位.抓盘_Auto1_X)) 初始点位.抓盘_Auto1_X = Convert.ToDouble(uiDoubleUpDown60.Text);
                if (UpdateInitPointData("抓盘_Auto2_X", Convert.ToDouble(uiDoubleUpDown59.Text), 初始点位.抓盘_Auto2_X)) 初始点位.抓盘_Auto2_X = Convert.ToDouble(uiDoubleUpDown59.Text);

                GetAxisRunInfo("M18_放Tary盘轴", out AxisRunInfo axisRun);
                if (UpdateRunInfoData("M18_放Tary盘轴", "最大速度", Convert.ToDouble(uiDoubleUpDown29.Text), axisRun.最大速度)) axisRun.最大速度 = Convert.ToDouble(uiDoubleUpDown29.Text);
                if (UpdateRunInfoData("M18_放Tary盘轴", "加速度", Convert.ToDouble(uiDoubleUpDown24.Text), axisRun.加速度)) axisRun.加速度 = Convert.ToDouble(uiDoubleUpDown24.Text);
                if (UpdateRunInfoData("M18_放Tary盘轴", "减速度", Convert.ToDouble(uiDoubleUpDown22.Text), axisRun.减速度)) axisRun.减速度 = Convert.ToDouble(uiDoubleUpDown22.Text);
            }
        }

        private void uiButton21_Click(object sender, EventArgs e)
        {
            刷新抓盘手臂初始点位();
            刷新抓盘手臂速度参数();
        }

        private void button72_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown57.Text);
            button71.Enabled = false;
            button72.Enabled = false;
            Task.Run(() =>
            {
                string result = 单轴绝对运动("M20_飞梭轴", pos, 0, 1, 15000);
                if (result != "Success") RunLog("移动失败!" + result);
                this.Invoke(new Action(() =>
                {
                    button71.Enabled = true;
                    button72.Enabled = true;
                }));
            });
        }

        private void button71_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown56.Text);
            button71.Enabled = false;
            button72.Enabled = false;
            Task.Run(() =>
            {
                string result = 单轴绝对运动("M20_飞梭轴", pos, 0, 1, 15000);
                if (result != "Success") RunLog("移动失败!" + result);
                this.Invoke(new Action(() =>
                {
                    button71.Enabled = true;
                    button72.Enabled = true;
                }));
            });
        }

        private void 入料手臂X移动初始点位(double mm)
        {
            button29.Enabled = false;
            button224.Enabled = false;
            button32.Enabled = false;
            button20.Enabled = false;
            Task.Run(() =>
            {
                string result = 单轴绝对运动("M22_入料手臂X轴", mm);
                if (result != "Success") RunLog("移动失败!" + result);

                this.Invoke(new Action(() =>
                {
                    button29.Enabled = true;
                    button224.Enabled = true;
                    button32.Enabled = true;
                    button20.Enabled = true;
                }));
            });
        }

        private void button29_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown102.Text);
            入料手臂X移动初始点位(pos);
        }

        private void button224_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown114.Text);
            入料手臂X移动初始点位(pos);
        }

        private void button32_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown99.Text);
            入料手臂X移动初始点位(pos);
        }

        private void button20_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown108.Text);
            入料手臂X移动初始点位(pos);
        }


        private void 入料手臂Y移动初始点位(double mm)
        {
            button28.Enabled = false;
            button223.Enabled = false;
            button31.Enabled = false;
            button19.Enabled = false;
            Task.Run(() =>
            {
                string result = 单轴绝对运动("M16_入料手臂Y轴", mm);
                if (result != "Success") RunLog("移动失败!" + result);

                this.Invoke(new Action(() =>
                {
                    button28.Enabled = true;
                    button223.Enabled = true;
                    button31.Enabled = true;
                    button19.Enabled = true;
                }));
            });
        }

        double inoldpos = 0;
        private void 入料手臂Z移动初始点位(double mm)
        {
            button30.Enabled = false;
            button18.Enabled = false;
            button27.Enabled = false;
            button222.Enabled = false;
            button226.Enabled = false;
            Task.Run(() =>
            {
                double z1 = Convert.ToDouble(uiDoubleUpDown18.Text);
                double z2 = Convert.ToDouble(uiDoubleUpDown17.Text);
                double z3 = Convert.ToDouble(uiDoubleUpDown16.Text);
                double z4 = Convert.ToDouble(uiDoubleUpDown19.Text);
                string[] axis = { "M1_入料手臂Z1轴", "M2_入料手臂Z2轴", "M3_入料手臂Z3轴", "M4_入料手臂Z4轴" };
                double[] pos = { mm + z1, mm + z2, mm + z3, mm + z4 };
                string result = 多轴绝对运动(axis, pos, 1, inoldpos < mm ? 1 : 0, 出入料Z第二段距离);
                if (result != "Success") RunLog("移动失败!" + result);
                inoldpos = mm;
                this.Invoke(new Action(() =>
                {
                    button30.Enabled = true;
                    button18.Enabled = true;
                    button27.Enabled = true;
                    button222.Enabled = true;
                    button226.Enabled = true;
                }));
            });
        }
        private void 入料手臂变距轴移动初始点位(double mm)
        {
            button240.Enabled = false;
            button241.Enabled = false;
            button242.Enabled = false;
            Task.Run(() =>
            {
                string result = 单轴绝对运动("M5_入料手臂P轴", mm);
                if (result != "Success") RunLog("移动失败!" + result);

                this.Invoke(new Action(() =>
                {
                    button240.Enabled = true;
                    button241.Enabled = true;
                    button242.Enabled = true;
                }));
            });
        }
        private void 出料手臂变距轴移动初始点位(double mm)
        {
            button245.Enabled = false;
            button244.Enabled = false;
            Task.Run(() =>
            {
                string result = 单轴绝对运动("M10_出料手臂P轴", mm);
                if (result != "Success") RunLog("移动失败!" + result);

                this.Invoke(new Action(() =>
                {
                    button245.Enabled = true;
                    button244.Enabled = true;
                }));
            });
        }

        double outoldpos = 0;
        private void 出料手臂Z移动初始点位(double mm)
        {
            button40.Enabled = false;
            button46.Enabled = false;
            button53.Enabled = false;
            button38.Enabled = false;
            button42.Enabled = false;
            button33.Enabled = false;
            button43.Enabled = false;
            Task.Run(() =>
            {
                double z1 = Convert.ToDouble(uiDoubleUpDown122.Text);
                double z2 = Convert.ToDouble(uiDoubleUpDown121.Text);
                double z3 = Convert.ToDouble(uiDoubleUpDown21.Text);
                double z4 = Convert.ToDouble(uiDoubleUpDown20.Text);
                string[] axis = { "M6_出料手臂Z1轴", "M7_出料手臂Z2轴", "M8_出料手臂Z3轴", "M9_出料手臂Z4轴" };
                double[] pos = { mm + z1, mm + z2, mm + z3, mm + z4 };

                string result = 多轴绝对运动(axis, pos, 1, outoldpos < mm ? 1 : 0, 出入料Z第二段距离);
                if (result != "Success") RunLog("移动失败!" + result);
                outoldpos = mm;

                this.Invoke(new Action(() =>
                {
                    button40.Enabled = true;
                    button46.Enabled = true;
                    button53.Enabled = true;
                    button38.Enabled = true;
                    button42.Enabled = true;
                    button33.Enabled = true;
                    button43.Enabled = true;
                }));
            });
        }

        private void button28_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown101.Text);
            入料手臂Y移动初始点位(pos);
        }

        private void button223_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown113.Text);
            入料手臂Y移动初始点位(pos);
        }

        private void button31_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown97.Text);
            入料手臂Y移动初始点位(pos);
        }

        private void button19_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown107.Text);
            入料手臂Y移动初始点位(pos);
        }

        private void 所有轴紧急停止()
        {
            for (int i = 0; i < AxisInfoList.Count; i++)
            {
                string result = motion.AxisStop(AxisInfoList[i].核号, AxisInfoList[i].轴号, 1);
                if (result != "Success")
                {
                    RunLog($"核号{AxisInfoList[i].核号},轴{AxisInfoList[i].轴号}停止失败!详细信息:" + result);
                }
            }
            MessageBox.Show("所有轴已紧急停止");
        }

        private void button24_Click(object sender, EventArgs e)
        {
            所有轴紧急停止();
        }

        private void button23_Click(object sender, EventArgs e)
        {
            所有轴紧急停止();
        }

        private void button25_Click(object sender, EventArgs e)
        {
            所有轴紧急停止();
        }

        private void button26_Click(object sender, EventArgs e)
        {
            所有轴紧急停止();
        }

        private void button35_Click(object sender, EventArgs e)
        {
            所有轴紧急停止();
        }

        private void 抓盘手臂X移动初始点位(double mm)
        {
            string result = handler.移Tray上下气缸原位输入读取(out string value);
            if (result == "Success")
            {
                if (value == "1")
                {
                    button76.Enabled = false;
                    button75.Enabled = false;
                    button74.Enabled = false;
                    button73.Enabled = false;
                    Task.Run(() =>
                    {
                        result = 单轴绝对运动("M18_放Tary盘轴", mm);
                        if (result != "Success") RunLog("移动失败!" + result);
                        this.Invoke(new Action(() =>
                        {
                            button76.Enabled = true;
                            button75.Enabled = true;
                            button74.Enabled = true;
                            button73.Enabled = true;
                        }));
                    });
                }
                else
                {
                    MessageBox.Show($"抓盘手臂上下气缸未在原位");
                }
            }
            else
            {
                APPmaster.ProgramPause = true;
                IsPause = true;
                MessageBox.Show($"移Tray上下气缸原位输入读取失败！" + result);
            }
        }

        private void button76_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown55.Text);
            抓盘手臂X移动初始点位(pos);
        }

        private void button75_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown58.Text);
            抓盘手臂X移动初始点位(pos);
        }

        private void button74_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown60.Text);
            抓盘手臂X移动初始点位(pos);
        }

        private void button73_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown59.Text);
            抓盘手臂X移动初始点位(pos);
        }


        private void 出料手臂X移动初始点位(double mm)
        {
            button56.Enabled = false;
            button58.Enabled = false;
            button55.Enabled = false;
            button36.Enabled = false;
            button51.Enabled = false;
            button21.Enabled = false;
            button45.Enabled = false;
            Task.Run(() =>
            {
                string result = 单轴绝对运动("M23_出料手臂X轴", mm);
                if (result != "Success") RunLog("移动失败!" + result);

                this.Invoke(new Action(() =>
                {
                    button56.Enabled = true;
                    button58.Enabled = true;
                    button55.Enabled = true;
                    button36.Enabled = true;
                    button51.Enabled = true;
                    button21.Enabled = true;
                    button45.Enabled = true;
                }));
            });
        }

        private void button56_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown75.Text);
            出料手臂X移动初始点位(pos);
        }

        private void button36_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown78.Text);
            出料手臂X移动初始点位(pos);
        }

        private void button58_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown90.Text);
            出料手臂X移动初始点位(pos);
        }

        private void button51_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown87.Text);
            出料手臂X移动初始点位(pos);
        }

        private void button55_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown93.Text);
            出料手臂X移动初始点位(pos);
        }

        private void button21_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown84.Text);
            出料手臂X移动初始点位(pos);
        }

        private void button45_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown96.Text);
            出料手臂X移动初始点位(pos);
        }

        private void 出料手臂Y移动初始点位(double mm)
        {
            button48.Enabled = false;
            button57.Enabled = false;
            button54.Enabled = false;
            button37.Enabled = false;
            button49.Enabled = false;
            button34.Enabled = false;
            button44.Enabled = false;
            Task.Run(() =>
            {
                string result = 单轴绝对运动("M17_出料手臂Y轴", mm);
                if (result != "Success") RunLog("移动失败!" + result);

                this.Invoke(new Action(() =>
                {
                    button48.Enabled = true;
                    button57.Enabled = true;
                    button54.Enabled = true;
                    button37.Enabled = true;
                    button49.Enabled = true;
                    button34.Enabled = true;
                    button44.Enabled = true;
                }));
            });
        }

        private void button48_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown74.Text);
            出料手臂Y移动初始点位(pos);
        }

        private void button57_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown89.Text);
            出料手臂Y移动初始点位(pos);
        }

        private void button54_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown92.Text);
            出料手臂Y移动初始点位(pos);
        }

        private void button37_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown77.Text);
            出料手臂Y移动初始点位(pos);
        }

        private void button49_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown86.Text);
            出料手臂Y移动初始点位(pos);
        }

        private void button34_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown83.Text);
            出料手臂Y移动初始点位(pos);
        }

        private void button44_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown95.Text);
            出料手臂Y移动初始点位(pos);
        }

        private void 测试手臂X移动初始点位(double mm)
        {
            button62.Enabled = false;
            button67.Enabled = false;
            button70.Enabled = false;
            button69.Enabled = false;
            button66.Enabled = false;
            button63.Enabled = false;
            button59.Enabled = false;
            button65.Enabled = false;
            Task.Run(() =>
            {
                string result = 单轴绝对运动("M21_测试手臂X轴", mm);
                if (result != "Success") RunLog("移动失败!" + result);

                this.Invoke(new Action(() =>
                {
                    button62.Enabled = true;
                    button67.Enabled = true;
                    button70.Enabled = true;
                    button69.Enabled = true;
                    button66.Enabled = true;
                    button63.Enabled = true;
                    button59.Enabled = true;
                    button65.Enabled = true;
                }));
            });
        }


        private void 测试手臂Z移动初始点位(double mm)
        {
            button60.Enabled = false;
            button68.Enabled = false;
            button61.Enabled = false;
            button64.Enabled = false;
            button47.Enabled = false;
            button41.Enabled = false;
            Task.Run(() =>
            {
                string result = 单轴绝对运动("M19_测试手臂Z轴", mm);
                if (result != "Success") RunLog("移动失败!" + result);

                this.Invoke(new Action(() =>
                {
                    button60.Enabled = true;
                    button68.Enabled = true;
                    button61.Enabled = true;
                    button64.Enabled = true;
                    button47.Enabled = true;
                    button41.Enabled = true;
                }));
            });
        }

        private void button62_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown62.Text);
            测试手臂X移动初始点位(pos);
        }

        private void button67_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown64.Text);
            测试手臂X移动初始点位(pos);
        }

        private void button70_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown66.Text);
            测试手臂X移动初始点位(pos);
        }

        private void button69_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown65.Text);
            测试手臂X移动初始点位(pos);
        }

        private void button66_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown68.Text);
            测试手臂X移动初始点位(pos);
        }

        private void button63_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown67.Text);
            测试手臂X移动初始点位(pos);
        }

        private void button59_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown70.Text);
            测试手臂X移动初始点位(pos);
        }

        private void button65_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown72.Text);
            测试手臂X移动初始点位(pos);
        }

        private void button60_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown61.Text);
            测试手臂Z移动初始点位(pos);
        }

        private void button68_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown63.Text);
            测试手臂Z移动初始点位(pos);
        }

        private void button61_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown69.Text);
            测试手臂Z移动初始点位(pos);
        }

        private void button64_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown71.Text);
            测试手臂Z移动初始点位(pos);
        }

        private void button47_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown25.Text);
            测试手臂Z移动初始点位(pos);
        }

        private void button41_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown27.Text);
            测试手臂Z移动初始点位(pos);
        }

        private void button30_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown98.Text);
            入料手臂Z移动初始点位(pos);
        }

        private void button18_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown106.Text);
            入料手臂Z移动初始点位(pos);
        }

        private void button226_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uinumericUpDown126.Text);
            入料手臂Z移动初始点位(pos);
        }

        private void button27_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown100.Text);
            入料手臂Z移动初始点位(pos);
        }

        private void button222_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown112.Text);
            入料手臂Z移动初始点位(pos);
        }

        private void button40_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown73.Text);
            出料手臂Z移动初始点位(pos);
        }

        private void button46_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown88.Text);
            出料手臂Z移动初始点位(pos);
        }

        private void button53_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown91.Text);
            出料手臂Z移动初始点位(pos);
        }

        private void button38_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown76.Text);
            出料手臂Z移动初始点位(pos);
        }

        private void button42_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown85.Text);
            出料手臂Z移动初始点位(pos);
        }

        private void button33_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown82.Text);
            出料手臂Z移动初始点位(pos);
        }

        private void button43_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown94.Text);
            出料手臂Z移动初始点位(pos);
        }

        private void button50_Click(object sender, EventArgs e)
        {
            Task.Run(() => { return handler.测试吸嘴_张开气缸输出(false); });
        }

        private void button225_Click(object sender, EventArgs e)
        {
            Task.Run(() => { return handler.测试吸嘴_张开气缸输出(true); });
        }

        #endregion

        private void button147_Click(object sender, EventArgs e)
        {
            string[] axisNames = { "M6_出料手臂Z1轴", "M7_出料手臂Z2轴", "M8_出料手臂Z3轴", "M9_出料手臂Z4轴" };
            Task.Run(() =>
            {
                string result = 多轴回零(axisNames);
                if (result != "Success")
                {
                    MessageBox.Show("出料手臂所有Z回零失败!");
                }
                else MessageBox.Show("出料手臂所有Z回零完成");
            });
        }

        private void button144_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                string[] axisNames = { "M23_出料手臂X轴", "M17_出料手臂Y轴", "M10_出料手臂P轴" };
                string result = 多轴回零(axisNames);
                if (result != "Success")
                {
                    MessageBox.Show("出料手臂XYP回零失败!");
                }
                else MessageBox.Show("出料手臂XYP回零完成");
            });
        }

        private void uiTabControl5_Selecting(object sender, TabControlCancelEventArgs e)
        {
            刷新产品(uiTabControl5.SelectedTab.Text);
        }

        private void uiTabControl6_Selecting(object sender, TabControlCancelEventArgs e)
        {
            刷新档案信息(uiTabControl6.SelectedTab.Text);
        }

        private void uiTabControl7_Selecting(object sender, TabControlCancelEventArgs e)
        {
            刷新Tray盘信息(uiTabControl7.SelectedTab.Text);
        }

        private void uiDataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            int index = e.RowIndex;
            if (index >= 0)
            {
                刷新档案信息(uiTabControl6.SelectedTab.Text);
            }
        }

        private void uiDataGridView2_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            int index = e.RowIndex;
            if (index >= 0)
            {
                刷新Tray盘信息(uiTabControl7.SelectedTab.Text);
            }
        }

        private void 保存设定信息()
        {
            int rows = uiDataGridView1.SelectedIndex;
            if (rows >= 0)
            {
                string name = uiDataGridView1.Rows[rows].Cells[1].Value.ToString();
                try
                {
                    DialogResult issave = MessageBox.Show("请确认是否保存！", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                    if (issave == DialogResult.OK)
                    {
                        string result = GetSetupInfo(name, out SetupInfo setupInfo);
                        if (result != "Success")
                        {
                            MessageBox.Show("保存失败!" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        string tempmode = uiCheckBox192.Checked ? "常温" : "高温";

                        string heatname = uiCheckBox192.Checked ? "默认" : uiComboBox2.Text;

                        string trayname = uiComboBox1.Text;


                        if (trayname != setupInfo.Tray盘信息.TrayName)
                        {
                            int line = SQLiteHelper.runSqlNonQuery(SQLiteConnString, $"update SetupInfo set tray盘名称 = '{trayname}' where 档案名称 = '{name}'");
                            if (line >= 1)
                            {
                                RunLog($"档案名称:[{name}]:Tray名称:{setupInfo.Tray盘信息.TrayName}->{trayname}:更新成功", WriteData.LogType.ParamLog);
                                if (trayname != 设置信息.Tray盘信息.TrayName)
                                {
                                    result = GetTrayInfo(trayname, out TrayInfo trayInfo);
                                    if (result == "Success")
                                    {
                                        设置信息.Tray盘信息 = trayInfo;
                                        result = Init_TrayPoint();
                                        if (result != "Success")
                                        {
                                            MessageBox.Show("获取tray盘信息保存失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("tray盘名称保存失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }

                        if (heatname != setupInfo.Heat盘信息.HeatName)
                        {
                            int line = SQLiteHelper.runSqlNonQuery(SQLiteConnString, $"update SetupInfo set 加热盘名称 = '{heatname}' where 档案名称 = '{name}'");
                            if (line >= 1)
                            {
                                RunLog($"档案名称:[{name}]:加热盘名称:{setupInfo.Heat盘信息.HeatName}->{heatname}:更新成功", WriteData.LogType.ParamLog);
                                if (heatname != 设置信息.Heat盘信息.HeatName)
                                {
                                    result = GetHeatInfo(heatname, out HeatInfo heatInfo);
                                    if (result == "Success")
                                    {
                                        设置信息.温度模式 = tempmode;
                                        设置信息.Heat盘信息 = heatInfo;
                                        HeatTrayICPointInit();
                                        刷新温度信息();
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("Heat盘名称保存失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }

                        if (tempmode != setupInfo.温度模式)
                        {
                            int line = SQLiteHelper.runSqlNonQuery(SQLiteConnString, $"update SetupInfo set 温度模式 = '{tempmode}' where 档案名称 = '{name}'");
                            if (line >= 1)
                            {
                                RunLog($"档案名称:[{name}]:温度模式:{setupInfo.温度模式}->{tempmode}:更新成功", WriteData.LogType.ParamLog);
                                if (tempmode != 设置信息.温度模式)
                                {
                                    设置信息.温度模式 = tempmode;
                                    刷新温度信息();
                                }
                            }
                            else
                            {
                                MessageBox.Show("温度模式保存失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("档案保存失败!" + ex.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void 保存测试机信息()
        {
            int rows = uiDataGridView1.SelectedIndex;
            if (rows >= 0)
            {
                string name = uiDataGridView1.Rows[rows].Cells[1].Value.ToString();
                try
                {
                    DialogResult issave = MessageBox.Show("请确认是否保存！", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                    if (issave == DialogResult.OK)
                    {
                        string result = GetSetupValue(name, "测试机名称", out string value);
                        if (result != "Success")
                        {
                            MessageBox.Show("保存失败!" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        result = GetTesterIFParam(value, out TesterIFParam oldtester);
                        if (result != "Success")
                        {
                            MessageBox.Show("保存失败!" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        TesterIFParam newtester = new TesterIFParam();
                        newtester.Dout连续不良个数 = Convert.ToInt32(numericUpDown12.Text);
                        newtester.Dout连续不良自动关闭 = checkBox1.Checked;
                        newtester.Dout连续不良报警 = checkBox2.Checked;
                        newtester.Dut累计不良报警个数 = Convert.ToInt32(numericUpDown5.Text);
                        newtester.Dut累计不良报警使能 = checkBox3.Checked;
                        newtester.总良率报警百分比 = Convert.ToDouble(numericUpDown13.Text);
                        newtester.总良率报警基数 = Convert.ToInt32(numericUpDown14.Text);
                        newtester.总良率报警间隔 = Convert.ToInt32(numericUpDown15.Text);
                        newtester.总良率报警使能 = checkBox4.Checked;
                        newtester.socket使用次数 = Convert.ToInt32(numericUpDown16.Text);
                        newtester.测试超时时间 = Convert.ToInt32(numericUpDown_测试超时时间.Text);
                        newtester.timeout超时时间 = Convert.ToInt32(numericUpDown_timeout超时时间.Text);
                        newtester.超时发送timeout = checkBox_timeout.Checked;
                        newtester.串口号 = uiComboBox11.Text.Trim();
                        newtester.波特率 = Convert.ToInt32(uiComboBox12.Text);
                        newtester.校验位 = (Parity)Enum.Parse(typeof(Parity), uiComboBox14.Text, true);
                        newtester.数据位 = Convert.ToInt32(uiComboBox13.Text);
                        newtester.停止位 = (StopBits)Enum.Parse(typeof(StopBits), uiComboBox15.Text, true);

                        result = UpdateTesterIFParam(value, newtester);
                        if (result != "Success")
                        {
                            MessageBox.Show("保存失败!" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            刷新测试机(oldtester);
                        }
                        else
                        {
                            bool comischange = false;
                            if (oldtester.Dout连续不良个数 != newtester.Dout连续不良个数) RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[Dout连续不良个数]:{oldtester.Dout连续不良个数}->{newtester.Dout连续不良个数}:更新成功", WriteData.LogType.ParamLog);
                            if (oldtester.Dout连续不良自动关闭 != newtester.Dout连续不良自动关闭) RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[Dout连续不良自动关闭]:{oldtester.Dout连续不良自动关闭}->{newtester.Dout连续不良自动关闭}:更新成功", WriteData.LogType.ParamLog);
                            if (oldtester.Dout连续不良报警 != newtester.Dout连续不良报警) RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[Dout连续不良报警]:{oldtester.Dout连续不良报警}->{newtester.Dout连续不良报警}:更新成功", WriteData.LogType.ParamLog);
                            if (oldtester.Dut累计不良报警个数 != newtester.Dut累计不良报警个数) RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[Dut累计不良报警个数]:{oldtester.Dut累计不良报警个数}->{newtester.Dut累计不良报警个数}:更新成功", WriteData.LogType.ParamLog);
                            if (oldtester.Dut累计不良报警使能 != newtester.Dut累计不良报警使能) RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[Dut累计不良报警使能]:{oldtester.Dut累计不良报警使能}->{newtester.Dut累计不良报警使能}:更新成功", WriteData.LogType.ParamLog);
                            if (oldtester.总良率报警百分比 != newtester.总良率报警百分比) RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[总良率报警百分比]:{oldtester.总良率报警百分比}->{newtester.总良率报警百分比}:更新成功", WriteData.LogType.ParamLog);
                            if (oldtester.总良率报警基数 != newtester.总良率报警基数) RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[总良率报警基数]:{oldtester.总良率报警基数}->{newtester.总良率报警基数}:更新成功", WriteData.LogType.ParamLog);
                            if (oldtester.总良率报警间隔 != newtester.总良率报警间隔) RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[总良率报警间隔]:{oldtester.总良率报警间隔}->{newtester.总良率报警间隔}:更新成功", WriteData.LogType.ParamLog);
                            if (oldtester.总良率报警使能 != newtester.总良率报警使能) RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[总良率报警使能]:{oldtester.总良率报警使能}->{newtester.总良率报警使能}:更新成功", WriteData.LogType.ParamLog);
                            if (oldtester.socket使用次数 != newtester.socket使用次数) RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[socket使用次数]:{oldtester.socket使用次数}->{newtester.socket使用次数}:更新成功", WriteData.LogType.ParamLog);
                            if (oldtester.测试超时时间 != newtester.测试超时时间) RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[测试超时时间]:{oldtester.测试超时时间}->{newtester.测试超时时间}:更新成功", WriteData.LogType.ParamLog);
                            if (oldtester.timeout超时时间 != newtester.timeout超时时间) RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[timeout超时时间]:{oldtester.timeout超时时间}->{newtester.timeout超时时间}:更新成功", WriteData.LogType.ParamLog);
                            if (oldtester.超时发送timeout != newtester.超时发送timeout) RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[超时发送timeout]:{oldtester.超时发送timeout}->{newtester.超时发送timeout}:更新成功", WriteData.LogType.ParamLog);
                            if (oldtester.串口号 != newtester.串口号) { comischange = true; RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[串口号]:{oldtester.串口号}->{newtester.串口号}:更新成功", WriteData.LogType.ParamLog); }
                            if (oldtester.波特率 != newtester.波特率) { comischange = true; RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[波特率]:{oldtester.波特率}->{newtester.波特率}:更新成功", WriteData.LogType.ParamLog); }
                            if (oldtester.校验位 != newtester.校验位) { comischange = true; RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[测试超时时间]:{oldtester.校验位}->{newtester.校验位}:更新成功", WriteData.LogType.ParamLog); }
                            if (oldtester.数据位 != newtester.数据位) { comischange = true; RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[timeout超时时间]:{oldtester.数据位}->{newtester.数据位}:更新成功", WriteData.LogType.ParamLog); }
                            if (oldtester.停止位 != newtester.停止位) { comischange = true; RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[超时发送timeout]:{oldtester.停止位}->{newtester.停止位}:更新成功", WriteData.LogType.ParamLog); }

                            if (value == 设置信息.测试机信息.测试机名称)
                            {
                                result = GetTesterIFParam(value, out newtester);
                                if (result == "Success")
                                {
                                    设置信息.测试机信息 = newtester;
                                }

                                if (comischange)
                                {
                                    Com.CloseCom();
                                    Com.OpenCom();
                                }
                            }
                            刷新测试机(newtester);
                        }
                    }
                }
                catch (Exception ex)
                {
                    RunLog("保存测试机信息()异常!" + ex.Message);
                    return;
                }
            }
        }

        private void 保存分BIN信息()
        {
            int rows = uiDataGridView1.SelectedIndex;
            if (rows < 0) return;

            DialogResult issave = DialogResult.Cancel;

            string name = uiDataGridView1.Rows[rows].Cells[1].Value.ToString();

            //检查特殊Bin项是否全部分配
            foreach (object o in comboBox10.Items)
            {
                bool ishere = false;
                foreach (object listbox in tabPage_bin.Controls)
                {
                    if (listbox.GetType() != typeof(ListBox))
                    {
                        continue;
                    }
                    if ((listbox as ListBox).Items.Contains(o))
                    {
                        ishere = true;
                        break;
                    }
                }

                if (!ishere)
                {
                    MessageBox.Show("特殊Bin项\"" + o.ToString() + "\"未分配。请分配全部特殊Bin后再保存。");
                    radioButton2.Checked = true;
                    comboBox10.SelectedItem = o;
                    return;
                }
            }

            if (radioButton3_1.Checked)
            {
                if (listBox3_1.Items.Count == 0)
                {
                    issave = MessageBox.Show($"{textBox_name3.Text}分BIN—1区BIN项为空,请确认是否保存!", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (issave == DialogResult.Cancel) return;
                }
            }
            else if (radioButton3_2.Checked)
            {
                if (listBox3_1.Items.Count == 0)
                {
                    MessageBox.Show($"{textBox_name3.Text}分BIN—1区BIN项不能为空!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (listBox3_2.Items.Count == 0)
                {
                    MessageBox.Show($"{textBox_name3.Text}分BIN—2区BIN项不能为空!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {

                if (listBox3_1.Items.Count == 0)
                {
                    MessageBox.Show($"{textBox_name3.Text}分BIN—1区BIN项不能为空!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (listBox3_2.Items.Count == 0)
                {
                    MessageBox.Show($"{textBox_name3.Text}分BIN—2区BIN项不能为空!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (listBox3_3.Items.Count == 0)
                {
                    MessageBox.Show($"{textBox_name3.Text}分BIN—3区BIN项不能为空!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            if (radioButton4_1.Checked)
            {
                if (listBox4_1.Items.Count == 0)
                {
                    issave = MessageBox.Show($"{textBox_name4.Text}分BIN—1区BIN项为空,请确认是否保存!", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (issave == DialogResult.Cancel) return;
                }
            }
            else if (radioButton4_2.Checked)
            {
                if (listBox4_1.Items.Count == 0)
                {
                    MessageBox.Show($"{textBox_name4.Text}分BIN—1区BIN项不能为空!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (listBox4_2.Items.Count == 0)
                {
                    MessageBox.Show($"{textBox_name4.Text}分BIN—2区BIN项不能为空!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {

                if (listBox4_1.Items.Count == 0)
                {
                    MessageBox.Show($"{textBox_name4.Text}分BIN—1区BIN项不能为空!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (listBox4_2.Items.Count == 0)
                {
                    MessageBox.Show($"{textBox_name4.Text}分BIN—2区BIN项不能为空!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (listBox4_3.Items.Count == 0)
                {
                    MessageBox.Show($"{textBox_name4.Text}分BIN—3区BIN项不能为空!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }


            if (radioButton5_1.Checked)
            {
                if (listBox5_1.Items.Count == 0)
                {
                    issave = MessageBox.Show($"{textBox_name5.Text}分BIN—1区BIN项为空,请确认是否保存!", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (issave == DialogResult.Cancel) return;
                }
            }
            else if (radioButton5_2.Checked)
            {
                if (listBox5_1.Items.Count == 0)
                {
                    MessageBox.Show($"{textBox_name5.Text}分BIN—1区BIN项不能为空!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (listBox5_2.Items.Count == 0)
                {
                    MessageBox.Show($"{textBox_name5.Text}分BIN—2区BIN项不能为空!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {

                if (listBox5_1.Items.Count == 0)
                {
                    MessageBox.Show($"{textBox_name5.Text}分BIN—1区BIN项不能为空!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (listBox5_2.Items.Count == 0)
                {
                    MessageBox.Show($"{textBox_name5.Text}分BIN—2区BIN项不能为空!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (listBox5_3.Items.Count == 0)
                {
                    MessageBox.Show($"{textBox_name5.Text}分BIN—3区BIN项不能为空!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            try
            {
                if (issave == DialogResult.Cancel)
                {
                    issave = MessageBox.Show("请确认是否保存！", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                }
                if (issave == DialogResult.OK)
                {
                    string result = GetSetupValue(name, "分BIN名称", out string value);
                    if (result != "Success")
                    {
                        MessageBox.Show("保存失败!" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    result = GetBINParam(value, out BinParam oldbinParam);
                    if (result != "Success")
                    {
                        MessageBox.Show("保存失败!" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    BinParam newbinParam = new BinParam();

                    newbinParam.auto1 = "";
                    newbinParam.auto2 = "";
                    newbinParam.fix1 = "";
                    newbinParam.fix2 = "";
                    newbinParam.fix3 = "";

                    //auto1
                    foreach (object o in listBox1.Items)
                    {
                        newbinParam.auto1 += o.ToString() + ",";
                    }
                    newbinParam.auto1 = newbinParam.auto1.TrimEnd(',');

                    //auto2
                    foreach (object o in listBox2.Items)
                    {
                        newbinParam.auto2 += o.ToString() + ",";
                    }
                    newbinParam.auto2 = newbinParam.auto2.TrimEnd(',');

                    //fix1
                    foreach (object o in listBox3_1.Items)
                    {
                        newbinParam.fix1 += o.ToString() + ",";
                    }
                    newbinParam.fix1 = newbinParam.fix1.TrimEnd(',') + ";";
                    foreach (object o in listBox3_2.Items)
                    {
                        newbinParam.fix1 += o.ToString() + ",";
                    }
                    newbinParam.fix1 = newbinParam.fix1.TrimEnd(',') + ";";
                    foreach (object o in listBox3_3.Items)
                    {
                        newbinParam.fix1 += o.ToString() + ",";
                    }
                    newbinParam.fix1 = newbinParam.fix1.TrimEnd(',');

                    //fix2
                    foreach (object o in listBox4_1.Items)
                    {
                        newbinParam.fix2 += o.ToString() + ",";
                    }
                    newbinParam.fix2 = newbinParam.fix2.TrimEnd(',') + ";";
                    foreach (object o in listBox4_2.Items)
                    {
                        newbinParam.fix2 += o.ToString() + ",";
                    }
                    newbinParam.fix2 = newbinParam.fix2.TrimEnd(',') + ";";
                    foreach (object o in listBox4_3.Items)
                    {
                        newbinParam.fix2 += o.ToString() + ",";
                    }
                    newbinParam.fix2 = newbinParam.fix2.TrimEnd(',');

                    //fix3
                    foreach (object o in listBox5_1.Items)
                    {
                        newbinParam.fix3 += o.ToString() + ",";
                    }
                    newbinParam.fix3 = newbinParam.fix3.TrimEnd(',') + ";";
                    foreach (object o in listBox5_2.Items)
                    {
                        newbinParam.fix3 += o.ToString() + ",";
                    }
                    newbinParam.fix3 = newbinParam.fix3.TrimEnd(',') + ";";
                    foreach (object o in listBox5_3.Items)
                    {
                        newbinParam.fix3 += o.ToString() + ",";
                    }
                    newbinParam.fix3 = newbinParam.fix3.TrimEnd(',');

                    newbinParam.color1 = label_color1.BackColor.ToArgb();
                    newbinParam.color2 = label_color2.BackColor.ToArgb();
                    newbinParam.color3 = label_color3.BackColor.ToArgb();
                    newbinParam.color4 = label_color4.BackColor.ToArgb();
                    newbinParam.color5 = label_color5.BackColor.ToArgb();

                    newbinParam.name1 = textBox_name1.Text;
                    newbinParam.name2 = textBox_name2.Text;
                    newbinParam.name3 = textBox_name3.Text;
                    newbinParam.name4 = textBox_name4.Text;
                    newbinParam.name5 = textBox_name5.Text;

                    if (radioButton3_1.Checked) newbinParam.fix1num = 1;
                    else if (radioButton3_2.Checked) newbinParam.fix1num = 2;
                    else if (radioButton3_3.Checked) newbinParam.fix1num = 3;

                    if (radioButton4_1.Checked) newbinParam.fix2num = 1;
                    else if (radioButton4_2.Checked) newbinParam.fix2num = 2;
                    else if (radioButton4_3.Checked) newbinParam.fix2num = 3;

                    if (radioButton5_1.Checked) newbinParam.fix3num = 1;
                    else if (radioButton5_2.Checked) newbinParam.fix3num = 2;
                    else if (radioButton5_3.Checked) newbinParam.fix3num = 3;

                    result = UpdateBinParam(value, newbinParam);
                    if (result != "Success")
                    {
                        MessageBox.Show("保存失败!" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        if (oldbinParam.auto1 != newbinParam.auto1) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[auto1]:{oldbinParam.auto1}->{newbinParam.auto1}:更新成功", WriteData.LogType.ParamLog);
                        if (oldbinParam.auto2 != newbinParam.auto2) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[auto2]:{oldbinParam.auto2}->{newbinParam.auto2}:更新成功", WriteData.LogType.ParamLog);
                        if (oldbinParam.fix1 != newbinParam.fix1) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[fix1]:{oldbinParam.fix1}->{newbinParam.fix1}:更新成功", WriteData.LogType.ParamLog);
                        if (oldbinParam.fix2 != newbinParam.fix2) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[fix2]:{oldbinParam.fix2}->{newbinParam.fix2}:更新成功", WriteData.LogType.ParamLog);
                        if (oldbinParam.fix3 != newbinParam.fix3) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[fix3]:{oldbinParam.fix3}->{newbinParam.fix3}:更新成功", WriteData.LogType.ParamLog);
                        if (oldbinParam.fix1num != newbinParam.fix1num) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[fix1num]:{oldbinParam.fix1num}->{newbinParam.fix1num}:更新成功", WriteData.LogType.ParamLog);
                        if (oldbinParam.fix2num != newbinParam.fix2num) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[fix2num]:{oldbinParam.fix2num}->{newbinParam.fix2num}:更新成功", WriteData.LogType.ParamLog);
                        if (oldbinParam.fix3num != newbinParam.fix3num) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[fix3num]:{oldbinParam.fix3num}->{newbinParam.fix3num}:更新成功", WriteData.LogType.ParamLog);
                        if (oldbinParam.color1 != newbinParam.color1) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[color1]:{oldbinParam.color1}->{newbinParam.color1}:更新成功", WriteData.LogType.ParamLog);
                        if (oldbinParam.color2 != newbinParam.color2) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[color2]:{oldbinParam.color2}->{newbinParam.color2}:更新成功", WriteData.LogType.ParamLog);
                        if (oldbinParam.color3 != newbinParam.color3) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[color3]:{oldbinParam.color3}->{newbinParam.color3}:更新成功", WriteData.LogType.ParamLog);
                        if (oldbinParam.color4 != newbinParam.color4) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[color4]:{oldbinParam.color4}->{newbinParam.color4}:更新成功", WriteData.LogType.ParamLog);
                        if (oldbinParam.color5 != newbinParam.color5) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[color5]:{oldbinParam.color5}->{newbinParam.color5}:更新成功", WriteData.LogType.ParamLog);
                        if (oldbinParam.name1 != newbinParam.name1) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[name1]:{oldbinParam.name1}->{newbinParam.name1}:更新成功", WriteData.LogType.ParamLog);
                        if (oldbinParam.name2 != newbinParam.name2) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[name2]:{oldbinParam.name2}->{newbinParam.name2}:更新成功", WriteData.LogType.ParamLog);
                        if (oldbinParam.name3 != newbinParam.name3) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[name3]:{oldbinParam.name3}->{newbinParam.name3}:更新成功", WriteData.LogType.ParamLog);
                        if (oldbinParam.name4 != newbinParam.name4) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[name4]:{oldbinParam.name4}->{newbinParam.name4}:更新成功", WriteData.LogType.ParamLog);
                        if (oldbinParam.name5 != newbinParam.name5) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[name5]:{oldbinParam.name5}->{newbinParam.name5}:更新成功", WriteData.LogType.ParamLog);
                    }

                    if (value == 设置信息.分BIN信息.分BIN名称)
                    {
                        result = GetBINParam(value, out newbinParam);
                        if (result == "Success")
                        {
                            设置信息.分BIN信息 = newbinParam;
                            刷新分BIN信息();
                        }
                    }

                    刷新分BIN(newbinParam);
                }
            }
            catch (Exception ex)
            {
                RunLog($"保存档案名称:[{name}]异常！\n详细信息：" + ex.Message);
                return;
            }

        }

        private void 保存入料手臂Offset(string name, string page, OffsetPoint offset)
        {
            DialogResult dialog = MessageBox.Show("请确认是否保存!", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (dialog == DialogResult.OK)
            {
                try
                {
                    UpdateOffsetPointData(name, page, "入料_Load吸_X", Convert.ToDouble(numericUpDown33.Text), offset.入料_Load吸_X);
                    UpdateOffsetPointData(name, page, "入料_Load吸_Y", Convert.ToDouble(numericUpDown30.Text), offset.入料_Load吸_Y);
                    UpdateOffsetPointData(name, page, "入料_Load吸_Z", Convert.ToDouble(numericUpDown28.Text), offset.入料_Load吸_Z);

                    UpdateOffsetPointData(name, page, "入料_Heat吸_X", Convert.ToDouble(numericUpDown39.Text), offset.入料_Heat吸_X);
                    UpdateOffsetPointData(name, page, "入料_Heat吸_Y", Convert.ToDouble(numericUpDown38.Text), offset.入料_Heat吸_Y);
                    UpdateOffsetPointData(name, page, "入料_Heat吸_Z", Convert.ToDouble(numericUpDown37.Text), offset.入料_Heat吸_Z);

                    UpdateOffsetPointData(name, page, "入料_Heat放_X", Convert.ToDouble(numericUpDown36.Text), offset.入料_Heat放_X);
                    UpdateOffsetPointData(name, page, "入料_Heat放_Y", Convert.ToDouble(numericUpDown35.Text), offset.入料_Heat放_Y);
                    UpdateOffsetPointData(name, page, "入料_Heat放_Z", Convert.ToDouble(numericUpDown34.Text), offset.入料_Heat放_Z);

                    UpdateOffsetPointData(name, page, "入料_梭车1第1列放_X", Convert.ToDouble(numericUpDown32.Text), offset.入料_梭车1第1列放_X);
                    UpdateOffsetPointData(name, page, "入料_梭车1第1列放_Y", Convert.ToDouble(numericUpDown31.Text), offset.入料_梭车1第1列放_Y);
                    UpdateOffsetPointData(name, page, "入料_梭车1第1列放_Z", Convert.ToDouble(numericUpDown29.Text), offset.入料_梭车1第1列放_Z);

                    UpdateOffsetPointData(name, page, "入料_梭车1第2列放_X", Convert.ToDouble(numericUpDown27.Text), offset.入料_梭车1第2列放_X);
                    UpdateOffsetPointData(name, page, "入料_梭车1第2列放_Y", Convert.ToDouble(numericUpDown26.Text), offset.入料_梭车1第2列放_Y);
                    UpdateOffsetPointData(name, page, "入料_梭车1第2列放_Z", Convert.ToDouble(numericUpDown25.Text), offset.入料_梭车1第2列放_Z);

                    UpdateOffsetPointData(name, page, "入料_Load_变距", Convert.ToDouble(numericUpDown24.Text), offset.入料_Load_变距);
                    UpdateOffsetPointData(name, page, "入料_Heat_变距", Convert.ToDouble(numericUpDown23.Text), offset.入料_Heat_变距);
                    UpdateOffsetPointData(name, page, "入料_梭车_变距", Convert.ToDouble(numericUpDown22.Text), offset.入料_梭车_变距);

                    UpdateOffsetPointData(name, page, "入料_梭车2第1列放_X", Convert.ToDouble(numericUpDown21.Text), offset.入料_梭车2第1列放_X);
                    UpdateOffsetPointData(name, page, "入料_梭车2第1列放_Y", Convert.ToDouble(numericUpDown20.Text), offset.入料_梭车2第1列放_Y);
                    UpdateOffsetPointData(name, page, "入料_梭车2第1列放_Z", Convert.ToDouble(numericUpDown19.Text), offset.入料_梭车2第1列放_Z);

                    UpdateOffsetPointData(name, page, "入料_梭车2第2列放_X", Convert.ToDouble(numericUpDown18.Text), offset.入料_梭车2第2列放_X);
                    UpdateOffsetPointData(name, page, "入料_梭车2第2列放_Y", Convert.ToDouble(numericUpDown17.Text), offset.入料_梭车2第2列放_Y);
                    UpdateOffsetPointData(name, page, "入料_梭车2第2列放_Z", Convert.ToDouble(numericUpDown1.Text), offset.入料_梭车2第2列放_Z);

                    if (page == 设置信息.OffsetPoint.微调点位名称)
                    {
                        设置信息.OffsetPoint = offset;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"保存失败,请检查数据格式是否正确!{ex.Message}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
        private void 保存出料手臂Offset(string name, string page, OffsetPoint offset)
        {
            DialogResult dialog = MessageBox.Show("请确认是否保存!", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (dialog == DialogResult.OK)
            {
                try
                {
                    UpdateOffsetPointData(name, page, "出料_梭车1第1列吸_X", Convert.ToDouble(numericUpDown63.Text), offset.出料_梭车1第1列吸_X);
                    UpdateOffsetPointData(name, page, "出料_梭车1第1列吸_Y", Convert.ToDouble(numericUpDown62.Text), offset.出料_梭车1第1列吸_Y);
                    UpdateOffsetPointData(name, page, "出料_梭车1第1列吸_Z", Convert.ToDouble(numericUpDown61.Text), offset.出料_梭车1第1列吸_Z);

                    UpdateOffsetPointData(name, page, "出料_梭车1第2列吸_X", Convert.ToDouble(numericUpDown60.Text), offset.出料_梭车1第2列吸_X);
                    UpdateOffsetPointData(name, page, "出料_梭车1第2列吸_Y", Convert.ToDouble(numericUpDown59.Text), offset.出料_梭车1第2列吸_Y);
                    UpdateOffsetPointData(name, page, "出料_梭车1第2列吸_Z", Convert.ToDouble(numericUpDown58.Text), offset.出料_梭车1第2列吸_Z);

                    UpdateOffsetPointData(name, page, "出料_Fix1放_X", Convert.ToDouble(numericUpDown57.Text), offset.出料_Fix1放_X);
                    UpdateOffsetPointData(name, page, "出料_Fix1放_Y", Convert.ToDouble(numericUpDown56.Text), offset.出料_Fix1放_Y);
                    UpdateOffsetPointData(name, page, "出料_Fix1放_Z", Convert.ToDouble(numericUpDown55.Text), offset.出料_Fix1放_Z);

                    UpdateOffsetPointData(name, page, "出料_梭车_变距", Convert.ToDouble(uiDoubleUpDown33.Text), offset.出料_梭车_变距);
                    UpdateOffsetPointData(name, page, "出料_AF_变距", Convert.ToDouble(uiDoubleUpDown34.Text), offset.出料_AF_变距);
                    UpdateOffsetPointData(name, page, "出料_梭车2第1列吸_X", Convert.ToDouble(uiDoubleUpDown31.Text), offset.出料_梭车2第1列吸_X);
                    UpdateOffsetPointData(name, page, "出料_梭车2第1列吸_Y", Convert.ToDouble(uiDoubleUpDown32.Text), offset.出料_梭车2第1列吸_Y);
                    UpdateOffsetPointData(name, page, "出料_梭车2第1列吸_Z", Convert.ToDouble(uiDoubleUpDown30.Text), offset.出料_梭车2第1列吸_Z);

                    UpdateOffsetPointData(name, page, "出料_梭车2第2列吸_X", Convert.ToDouble(numericUpDown54.Text), offset.出料_梭车2第2列吸_X);
                    UpdateOffsetPointData(name, page, "出料_梭车2第2列吸_Y", Convert.ToDouble(numericUpDown53.Text), offset.出料_梭车2第2列吸_Y);
                    UpdateOffsetPointData(name, page, "出料_梭车2第2列吸_Z", Convert.ToDouble(numericUpDown52.Text), offset.出料_梭车2第2列吸_Z);

                    UpdateOffsetPointData(name, page, "出料_Fix2放_X", Convert.ToDouble(numericUpDown51.Text), offset.出料_Fix2放_X);
                    UpdateOffsetPointData(name, page, "出料_Fix2放_Y", Convert.ToDouble(numericUpDown50.Text), offset.出料_Fix2放_Y);
                    UpdateOffsetPointData(name, page, "出料_Fix2放_Z", Convert.ToDouble(numericUpDown49.Text), offset.出料_Fix2放_Z);
                    UpdateOffsetPointData(name, page, "出料_Auto1放_X", Convert.ToDouble(numericUpDown48.Text), offset.出料_Auto1放_X);
                    UpdateOffsetPointData(name, page, "出料_Auto1放_Y", Convert.ToDouble(numericUpDown47.Text), offset.出料_Auto1放_Y);
                    UpdateOffsetPointData(name, page, "出料_Auto1放_Z", Convert.ToDouble(numericUpDown46.Text), offset.出料_Auto1放_Z);

                    UpdateOffsetPointData(name, page, "出料_Auto2放_X", Convert.ToDouble(numericUpDown45.Text), offset.出料_Auto2放_X);
                    UpdateOffsetPointData(name, page, "出料_Auto2放_Y", Convert.ToDouble(numericUpDown44.Text), offset.出料_Auto2放_Y);
                    UpdateOffsetPointData(name, page, "出料_Auto2放_Z", Convert.ToDouble(numericUpDown43.Text), offset.出料_Auto2放_Z);

                    UpdateOffsetPointData(name, page, "出料_Fix3放_X", Convert.ToDouble(numericUpDown42.Text), offset.出料_Fix3放_X);
                    UpdateOffsetPointData(name, page, "出料_Fix3放_Y", Convert.ToDouble(numericUpDown41.Text), offset.出料_Fix3放_Y);
                    UpdateOffsetPointData(name, page, "出料_Fix3放_Z", Convert.ToDouble(numericUpDown40.Text), offset.出料_Fix3放_Z);


                    if (page == 设置信息.OffsetPoint.微调点位名称)
                    {
                        设置信息.OffsetPoint = offset;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"保存失败,请检查数据格式是否正确!{ex.Message}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
        private void 保存测试手臂Offset(string name, string page, OffsetPoint offset)
        {

            DialogResult dialog = MessageBox.Show("请确认是否保存!", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (dialog == DialogResult.OK)
            {
                try
                {

                    UpdateOffsetPointData(name, page, "测试_梭车1左吸_X", Convert.ToDouble(numericUpDown78.Text), offset.测试_梭车1左吸_X);
                    UpdateOffsetPointData(name, page, "测试_梭车1左吸_Z", Convert.ToDouble(numericUpDown75.Text), offset.测试_梭车1左吸_Z);
                    UpdateOffsetPointData(name, page, "测试_梭车1右放_X", Convert.ToDouble(numericUpDown73.Text), offset.测试_梭车1右放_X);
                    UpdateOffsetPointData(name, page, "测试_梭车1右放_Z", Convert.ToDouble(numericUpDown72.Text), offset.测试_梭车1右放_Z);
                    UpdateOffsetPointData(name, page, "测试_梭车2左吸_X", Convert.ToDouble(numericUpDown70.Text), offset.测试_梭车2左吸_X);
                    UpdateOffsetPointData(name, page, "测试_梭车2左吸_Z", Convert.ToDouble(numericUpDown68.Text), offset.测试_梭车2左吸_Z);
                    UpdateOffsetPointData(name, page, "测试_梭车2右放_X", Convert.ToDouble(numericUpDown66.Text), offset.测试_梭车2右放_X);
                    UpdateOffsetPointData(name, page, "测试_梭车2右放_Z", Convert.ToDouble(numericUpDown64.Text), offset.测试_梭车2右放_Z);
                    UpdateOffsetPointData(name, page, "测试_Site1吸_X", Convert.ToDouble(numericUpDown119.Text), offset.测试_Site1吸_X);
                    UpdateOffsetPointData(name, page, "测试_Site2吸_X", Convert.ToDouble(numericUpDown118.Text), offset.测试_Site2吸_X);
                    UpdateOffsetPointData(name, page, "测试_Site3吸_X", Convert.ToDouble(uiDoubleUpDown26.Text), offset.测试_Site3吸_X);
                    UpdateOffsetPointData(name, page, "测试_Site4吸_X", Convert.ToDouble(numericUpDown117.Text), offset.测试_Site4吸_X);
                    UpdateOffsetPointData(name, page, "测试_Site5吸_X", Convert.ToDouble(uiDoubleUpDown36.Text), offset.测试_Site5吸_X);
                    UpdateOffsetPointData(name, page, "测试_Site6吸_X", Convert.ToDouble(uiDoubleUpDown35.Text), offset.测试_Site6吸_X);
                    UpdateOffsetPointData(name, page, "测试_Site7吸_X", Convert.ToDouble(uiDoubleUpDown28.Text), offset.测试_Site7吸_X);
                    UpdateOffsetPointData(name, page, "测试_Site8吸_X", Convert.ToDouble(numericUpDown116.Text), offset.测试_Site8吸_X);
                    UpdateOffsetPointData(name, page, "测试_Site9吸_X", Convert.ToDouble(numericUpDown115.Text), offset.测试_Site9吸_X);
                    UpdateOffsetPointData(name, page, "测试_Site10吸_X", Convert.ToDouble(numericUpDown114.Text), offset.测试_Site10吸_X);
                    UpdateOffsetPointData(name, page, "测试_Site11吸_X", Convert.ToDouble(uiDoubleUpDown42.Text), offset.测试_Site11吸_X);
                    UpdateOffsetPointData(name, page, "测试_Site12吸_X", Convert.ToDouble(uiDoubleUpDown41.Text), offset.测试_Site12吸_X);
                    UpdateOffsetPointData(name, page, "测试_Site13吸_X", Convert.ToDouble(uiDoubleUpDown40.Text), offset.测试_Site13吸_X);
                    UpdateOffsetPointData(name, page, "测试_Site14吸_X", Convert.ToDouble(uiDoubleUpDown39.Text), offset.测试_Site14吸_X);
                    UpdateOffsetPointData(name, page, "测试_Site15吸_X", Convert.ToDouble(uiDoubleUpDown38.Text), offset.测试_Site15吸_X);
                    UpdateOffsetPointData(name, page, "测试_Site16吸_X", Convert.ToDouble(uiDoubleUpDown37.Text), offset.测试_Site16吸_X);
                    UpdateOffsetPointData(name, page, "测试_Site1吸_Z", Convert.ToDouble(numericUpDown113.Text), offset.测试_Site1吸_Z);
                    UpdateOffsetPointData(name, page, "测试_Site2吸_Z", Convert.ToDouble(numericUpDown109.Text), offset.测试_Site2吸_Z);
                    UpdateOffsetPointData(name, page, "测试_Site3吸_Z", Convert.ToDouble(numericUpDown104.Text), offset.测试_Site3吸_Z);
                    UpdateOffsetPointData(name, page, "测试_Site4吸_Z", Convert.ToDouble(numericUpDown105.Text), offset.测试_Site4吸_Z);
                    UpdateOffsetPointData(name, page, "测试_Site5吸_Z", Convert.ToDouble(numericUpDown112.Text), offset.测试_Site5吸_Z);
                    UpdateOffsetPointData(name, page, "测试_Site6吸_Z", Convert.ToDouble(numericUpDown108.Text), offset.测试_Site6吸_Z);
                    UpdateOffsetPointData(name, page, "测试_Site7吸_Z", Convert.ToDouble(numericUpDown103.Text), offset.测试_Site7吸_Z);
                    UpdateOffsetPointData(name, page, "测试_Site8吸_Z", Convert.ToDouble(numericUpDown100.Text), offset.测试_Site8吸_Z);
                    UpdateOffsetPointData(name, page, "测试_Site9吸_Z", Convert.ToDouble(numericUpDown111.Text), offset.测试_Site9吸_Z);
                    UpdateOffsetPointData(name, page, "测试_Site10吸_Z", Convert.ToDouble(numericUpDown107.Text), offset.测试_Site10吸_Z);
                    UpdateOffsetPointData(name, page, "测试_Site11吸_Z", Convert.ToDouble(numericUpDown102.Text), offset.测试_Site11吸_Z);
                    UpdateOffsetPointData(name, page, "测试_Site12吸_Z", Convert.ToDouble(numericUpDown99.Text), offset.测试_Site12吸_Z);
                    UpdateOffsetPointData(name, page, "测试_Site13吸_Z", Convert.ToDouble(numericUpDown110.Text), offset.测试_Site13吸_Z);
                    UpdateOffsetPointData(name, page, "测试_Site14吸_Z", Convert.ToDouble(numericUpDown106.Text), offset.测试_Site14吸_Z);
                    UpdateOffsetPointData(name, page, "测试_Site15吸_Z", Convert.ToDouble(numericUpDown101.Text), offset.测试_Site15吸_Z);
                    UpdateOffsetPointData(name, page, "测试_Site16吸_Z", Convert.ToDouble(numericUpDown98.Text), offset.测试_Site16吸_Z);
                    UpdateOffsetPointData(name, page, "测试_Site1放_X", Convert.ToDouble(numericUpDown97.Text), offset.测试_Site1放_X);
                    UpdateOffsetPointData(name, page, "测试_Site2放_X", Convert.ToDouble(numericUpDown95.Text), offset.测试_Site2放_X);
                    UpdateOffsetPointData(name, page, "测试_Site3放_X", Convert.ToDouble(numericUpDown93.Text), offset.测试_Site3放_X);
                    UpdateOffsetPointData(name, page, "测试_Site4放_X", Convert.ToDouble(numericUpDown91.Text), offset.测试_Site4放_X);
                    UpdateOffsetPointData(name, page, "测试_Site5放_X", Convert.ToDouble(numericUpDown89.Text), offset.测试_Site5放_X);
                    UpdateOffsetPointData(name, page, "测试_Site6放_X", Convert.ToDouble(numericUpDown87.Text), offset.测试_Site6放_X);
                    UpdateOffsetPointData(name, page, "测试_Site7放_X", Convert.ToDouble(numericUpDown84.Text), offset.测试_Site7放_X);
                    UpdateOffsetPointData(name, page, "测试_Site8放_X", Convert.ToDouble(numericUpDown81.Text), offset.测试_Site8放_X);
                    UpdateOffsetPointData(name, page, "测试_Site9放_X", Convert.ToDouble(numericUpDown85.Text), offset.测试_Site9放_X);
                    UpdateOffsetPointData(name, page, "测试_Site10放_X", Convert.ToDouble(uiDoubleUpDown54.Text), offset.测试_Site10放_X);
                    UpdateOffsetPointData(name, page, "测试_Site11放_X", Convert.ToDouble(uiDoubleUpDown53.Text), offset.测试_Site11放_X);
                    UpdateOffsetPointData(name, page, "测试_Site12放_X", Convert.ToDouble(uiDoubleUpDown52.Text), offset.测试_Site12放_X);
                    UpdateOffsetPointData(name, page, "测试_Site13放_X", Convert.ToDouble(numericUpDown76.Text), offset.测试_Site13放_X);
                    UpdateOffsetPointData(name, page, "测试_Site14放_X", Convert.ToDouble(numericUpDown71.Text), offset.测试_Site14放_X);
                    UpdateOffsetPointData(name, page, "测试_Site15放_X", Convert.ToDouble(numericUpDown77.Text), offset.测试_Site15放_X);
                    UpdateOffsetPointData(name, page, "测试_Site16放_X", Convert.ToDouble(numericUpDown67.Text), offset.测试_Site16放_X);
                    UpdateOffsetPointData(name, page, "测试_Site1放_Z", Convert.ToDouble(numericUpDown96.Text), offset.测试_Site1放_Z);
                    UpdateOffsetPointData(name, page, "测试_Site2放_Z", Convert.ToDouble(numericUpDown94.Text), offset.测试_Site2放_Z);
                    UpdateOffsetPointData(name, page, "测试_Site3放_Z", Convert.ToDouble(numericUpDown92.Text), offset.测试_Site3放_Z);
                    UpdateOffsetPointData(name, page, "测试_Site4放_Z", Convert.ToDouble(numericUpDown90.Text), offset.测试_Site4放_Z);
                    UpdateOffsetPointData(name, page, "测试_Site5放_Z", Convert.ToDouble(numericUpDown88.Text), offset.测试_Site5放_Z);
                    UpdateOffsetPointData(name, page, "测试_Site6放_Z", Convert.ToDouble(numericUpDown86.Text), offset.测试_Site6放_Z);
                    UpdateOffsetPointData(name, page, "测试_Site7放_Z", Convert.ToDouble(numericUpDown82.Text), offset.测试_Site7放_Z);
                    UpdateOffsetPointData(name, page, "测试_Site8放_Z", Convert.ToDouble(numericUpDown80.Text), offset.测试_Site8放_Z);
                    UpdateOffsetPointData(name, page, "测试_Site9放_Z", Convert.ToDouble(numericUpDown83.Text), offset.测试_Site9放_Z);
                    UpdateOffsetPointData(name, page, "测试_Site10放_Z", Convert.ToDouble(numericUpDown79.Text), offset.测试_Site10放_Z);
                    UpdateOffsetPointData(name, page, "测试_Site11放_Z", Convert.ToDouble(uiDoubleUpDown111.Text), offset.测试_Site11放_Z);
                    UpdateOffsetPointData(name, page, "测试_Site12放_Z", Convert.ToDouble(uiDoubleUpDown110.Text), offset.测试_Site12放_Z);
                    UpdateOffsetPointData(name, page, "测试_Site13放_Z", Convert.ToDouble(uiDoubleUpDown109.Text), offset.测试_Site13放_Z);
                    UpdateOffsetPointData(name, page, "测试_Site14放_Z", Convert.ToDouble(numericUpDown69.Text), offset.测试_Site14放_Z);
                    UpdateOffsetPointData(name, page, "测试_Site15放_Z", Convert.ToDouble(numericUpDown74.Text), offset.测试_Site15放_Z);
                    UpdateOffsetPointData(name, page, "测试_Site16放_Z", Convert.ToDouble(numericUpDown65.Text), offset.测试_Site16放_Z);

                    if (page == 设置信息.OffsetPoint.微调点位名称)
                    {
                        设置信息.OffsetPoint = offset;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"保存失败,请检查数据格式是否正确!{ex.Message}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
        private void 保存梭车Offset(string name, string page, OffsetPoint offset)
        {

            DialogResult dialog = MessageBox.Show("请确认是否保存!", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (dialog == DialogResult.OK)
            {
                try
                {
                    UpdateOffsetPointData(name, page, "梭车2_前_Y", Convert.ToDouble(numericUpDown121.Text), offset.梭车2_前_Y);
                    UpdateOffsetPointData(name, page, "梭车2_后_Y", Convert.ToDouble(numericUpDown120.Text), offset.梭车2_后_Y);

                    if (page == 设置信息.OffsetPoint.微调点位名称)
                    {
                        设置信息.OffsetPoint = offset;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"保存失败,请检查数据格式是否正确!{ex.Message}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
        private void 保存抓料手臂Offset(string name, string page, OffsetPoint offset)
        {

            DialogResult dialog = MessageBox.Show("请确认是否保存!", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (dialog == DialogResult.OK)
            {
                try
                {
                    UpdateOffsetPointData(name, page, "抓盘_Empty_X", Convert.ToDouble(numericUpDown125.Text), offset.抓盘_Empty_X);
                    UpdateOffsetPointData(name, page, "抓盘_Load_X", Convert.ToDouble(numericUpDown124.Text), offset.抓盘_Load_X);
                    UpdateOffsetPointData(name, page, "抓盘_Auto1_X", Convert.ToDouble(numericUpDown123.Text), offset.抓盘_Auto1_X);
                    UpdateOffsetPointData(name, page, "抓盘_Auto2_X", Convert.ToDouble(numericUpDown122.Text), offset.抓盘_Auto2_X);

                    if (page == 设置信息.OffsetPoint.微调点位名称)
                    {
                        设置信息.OffsetPoint = offset;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"保存失败,请检查数据格式是否正确!{ex.Message}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void 保存点位信息(string tabpage)
        {
            int rows = uiDataGridView1.SelectedIndex;
            if (rows >= 0)
            {
                string name = uiDataGridView1.Rows[rows].Cells[1].Value.ToString();

                string result = GetSetupValue(name, "偏移点位名称", out string value);
                if (result != "Success") return;

                result = GetOffsetPoint(value, out OffsetPoint oldoffset);
                if (result != "Success") return;

                if (tabpage == "入料手臂")
                {
                    保存入料手臂Offset(name, value, oldoffset);
                }
                else if (tabpage == "出料手臂")
                {
                    保存出料手臂Offset(name, value, oldoffset);
                }
                else if (tabpage == "测试手臂")
                {
                    保存测试手臂Offset(name, value, oldoffset);
                }
                else if (tabpage == "梭车")
                {
                    保存梭车Offset(name, value, oldoffset);
                }
                else if (tabpage == "抓盘手臂")
                {
                    保存抓料手臂Offset(name, value, oldoffset);
                }
            }
        }

        private void uiButton3_Click_1(object sender, EventArgs e)
        {
            int rows = uiDataGridView1.SelectedIndex;
            if (rows >= 0)
            {
                string tabtext = uiTabControl6.SelectedTab.Text;
                if (tabtext == "分BIN")
                {
                    保存分BIN信息();
                }
                else if (tabtext == "测试机")
                {
                    保存测试机信息();
                }
                else if (tabtext == "设定")
                {
                    保存设定信息();
                }
                else if (tabtext == "点位偏移")
                {
                    保存点位信息(uiTabControl8.SelectedTab.Text);
                }
            }
        }

        private void button_add1_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                if (textBox40.Text == "") return;
                if (int.TryParse(textBox40.Text, out int res)) textBox40.Text = res.ToString("D2");

                if (textBox40.Text == "00")
                {
                    MessageBox.Show("测试机中\"00\"代表未放料，禁止添加该Bin项!");
                    return;
                }

                foreach (object listbox in tabPage_bin.Controls)
                {
                    if (listbox.GetType() != typeof(ListBox))
                    {
                        continue;
                    }

                    foreach (object it in (listbox as ListBox).Items)
                    {
                        if (textBox40.Text == it.ToString())
                        {
                            MessageBox.Show("Bin项目重复");
                            return;
                        }
                    }
                }

                (tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox).Items.Add(textBox40.Text);
            }
            else
            {
                foreach (object listbox in tabPage_bin.Controls)
                {
                    if (listbox.GetType() != typeof(ListBox))
                    {
                        continue;
                    }
                    foreach (object it in (listbox as ListBox).Items)
                    {
                        if (comboBox10.Text == it.ToString())
                        {
                            MessageBox.Show("Bin项目重复");
                            return;
                        }
                    }
                }
                (tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox).Items.Add(comboBox10.Text);
            }
            textBox40.Text = "";
        }

        private void button_del1_Click(object sender, EventArgs e)
        {
            ListBox l = tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox;
            l.Items.Remove(l.SelectedItem);
        }

        private void button_add2_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                if (textBox40.Text == "") return;
                if (int.TryParse(textBox40.Text, out int res)) textBox40.Text = res.ToString("D2");

                if (textBox40.Text == "00")
                {
                    MessageBox.Show("测试机中\"00\"代表未放料，禁止添加该Bin项!");
                    return;
                }

                foreach (object listbox in tabPage_bin.Controls)
                {
                    if (listbox.GetType() != typeof(ListBox))
                    {
                        continue;
                    }

                    foreach (object it in (listbox as ListBox).Items)
                    {
                        if (textBox40.Text == it.ToString())
                        {
                            MessageBox.Show("Bin项目重复");
                            return;
                        }
                    }
                }

                (tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox).Items.Add(textBox40.Text);
            }
            else
            {
                foreach (object listbox in tabPage_bin.Controls)
                {
                    if (listbox.GetType() != typeof(ListBox))
                    {
                        continue;
                    }
                    foreach (object it in (listbox as ListBox).Items)
                    {
                        if (comboBox10.Text == it.ToString())
                        {
                            MessageBox.Show("Bin项目重复");
                            return;
                        }
                    }
                }
                (tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox).Items.Add(comboBox10.Text);
            }
            textBox40.Text = "";
        }

        private void button_del2_Click(object sender, EventArgs e)
        {
            ListBox l = tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox;
            l.Items.Remove(l.SelectedItem);
        }

        private void button_add3_1_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                if (textBox40.Text == "") return;
                if (int.TryParse(textBox40.Text, out int res)) textBox40.Text = res.ToString("D2");

                if (textBox40.Text == "00")
                {
                    MessageBox.Show("测试机中\"00\"代表未放料，禁止添加该Bin项!");
                    return;
                }

                foreach (object listbox in tabPage_bin.Controls)
                {
                    if (listbox.GetType() != typeof(ListBox))
                    {
                        continue;
                    }

                    foreach (object it in (listbox as ListBox).Items)
                    {
                        if (textBox40.Text == it.ToString())
                        {
                            MessageBox.Show("Bin项目重复");
                            return;
                        }
                    }
                }

                (tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox).Items.Add(textBox40.Text);
            }
            else
            {
                foreach (object listbox in tabPage_bin.Controls)
                {
                    if (listbox.GetType() != typeof(ListBox))
                    {
                        continue;
                    }
                    foreach (object it in (listbox as ListBox).Items)
                    {
                        if (comboBox10.Text == it.ToString())
                        {
                            MessageBox.Show("Bin项目重复");
                            return;
                        }
                    }
                }
                (tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox).Items.Add(comboBox10.Text);
            }
            textBox40.Text = "";
        }

        private void button_del3_1_Click(object sender, EventArgs e)
        {
            ListBox l = tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox;
            l.Items.Remove(l.SelectedItem);
        }

        private void button_add3_2_Click(object sender, EventArgs e)
        {

            if (radioButton1.Checked)
            {
                if (textBox40.Text == "") return;
                if (int.TryParse(textBox40.Text, out int res)) textBox40.Text = res.ToString("D2");

                if (textBox40.Text == "00")
                {
                    MessageBox.Show("测试机中\"00\"代表未放料，禁止添加该Bin项!");
                    return;
                }

                foreach (object listbox in tabPage_bin.Controls)
                {
                    if (listbox.GetType() != typeof(ListBox))
                    {
                        continue;
                    }

                    foreach (object it in (listbox as ListBox).Items)
                    {
                        if (textBox40.Text == it.ToString())
                        {
                            MessageBox.Show("Bin项目重复");
                            return;
                        }
                    }
                }

                (tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox).Items.Add(textBox40.Text);
            }
            else
            {
                foreach (object listbox in tabPage_bin.Controls)
                {
                    if (listbox.GetType() != typeof(ListBox))
                    {
                        continue;
                    }
                    foreach (object it in (listbox as ListBox).Items)
                    {
                        if (comboBox10.Text == it.ToString())
                        {
                            MessageBox.Show("Bin项目重复");
                            return;
                        }
                    }
                }
                (tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox).Items.Add(comboBox10.Text);
            }
            textBox40.Text = "";
        }

        private void button_del3_2_Click(object sender, EventArgs e)
        {
            ListBox l = tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox;
            l.Items.Remove(l.SelectedItem);
        }

        private void button_add3_3_Click(object sender, EventArgs e)
        {

            if (radioButton1.Checked)
            {
                if (textBox40.Text == "") return;
                if (int.TryParse(textBox40.Text, out int res)) textBox40.Text = res.ToString("D2");

                if (textBox40.Text == "00")
                {
                    MessageBox.Show("测试机中\"00\"代表未放料，禁止添加该Bin项!");
                    return;
                }

                foreach (object listbox in tabPage_bin.Controls)
                {
                    if (listbox.GetType() != typeof(ListBox))
                    {
                        continue;
                    }

                    foreach (object it in (listbox as ListBox).Items)
                    {
                        if (textBox40.Text == it.ToString())
                        {
                            MessageBox.Show("Bin项目重复");
                            return;
                        }
                    }
                }

                (tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox).Items.Add(textBox40.Text);
            }
            else
            {
                foreach (object listbox in tabPage_bin.Controls)
                {
                    if (listbox.GetType() != typeof(ListBox))
                    {
                        continue;
                    }
                    foreach (object it in (listbox as ListBox).Items)
                    {
                        if (comboBox10.Text == it.ToString())
                        {
                            MessageBox.Show("Bin项目重复");
                            return;
                        }
                    }
                }
                (tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox).Items.Add(comboBox10.Text);
            }
            textBox40.Text = "";
        }

        private void button_del3_3_Click(object sender, EventArgs e)
        {
            ListBox l = tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox;
            l.Items.Remove(l.SelectedItem);
        }

        private void button_add4_1_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                if (textBox40.Text == "") return;
                if (int.TryParse(textBox40.Text, out int res)) textBox40.Text = res.ToString("D2");

                if (textBox40.Text == "00")
                {
                    MessageBox.Show("测试机中\"00\"代表未放料，禁止添加该Bin项!");
                    return;
                }

                foreach (object listbox in tabPage_bin.Controls)
                {
                    if (listbox.GetType() != typeof(ListBox))
                    {
                        continue;
                    }

                    foreach (object it in (listbox as ListBox).Items)
                    {
                        if (textBox40.Text == it.ToString())
                        {
                            MessageBox.Show("Bin项目重复");
                            return;
                        }
                    }
                }

                (tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox).Items.Add(textBox40.Text);
            }
            else
            {
                foreach (object listbox in tabPage_bin.Controls)
                {
                    if (listbox.GetType() != typeof(ListBox))
                    {
                        continue;
                    }
                    foreach (object it in (listbox as ListBox).Items)
                    {
                        if (comboBox10.Text == it.ToString())
                        {
                            MessageBox.Show("Bin项目重复");
                            return;
                        }
                    }
                }
                (tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox).Items.Add(comboBox10.Text);
            }
            textBox40.Text = "";
        }

        private void button_del4_1_Click(object sender, EventArgs e)
        {
            ListBox l = tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox;
            l.Items.Remove(l.SelectedItem);
        }

        private void button_add4_2_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                if (textBox40.Text == "") return;
                if (int.TryParse(textBox40.Text, out int res)) textBox40.Text = res.ToString("D2");

                if (textBox40.Text == "00")
                {
                    MessageBox.Show("测试机中\"00\"代表未放料，禁止添加该Bin项!");
                    return;
                }

                foreach (object listbox in tabPage_bin.Controls)
                {
                    if (listbox.GetType() != typeof(ListBox))
                    {
                        continue;
                    }

                    foreach (object it in (listbox as ListBox).Items)
                    {
                        if (textBox40.Text == it.ToString())
                        {
                            MessageBox.Show("Bin项目重复");
                            return;
                        }
                    }
                }

                (tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox).Items.Add(textBox40.Text);
            }
            else
            {
                foreach (object listbox in tabPage_bin.Controls)
                {
                    if (listbox.GetType() != typeof(ListBox))
                    {
                        continue;
                    }
                    foreach (object it in (listbox as ListBox).Items)
                    {
                        if (comboBox10.Text == it.ToString())
                        {
                            MessageBox.Show("Bin项目重复");
                            return;
                        }
                    }
                }
                (tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox).Items.Add(comboBox10.Text);
            }
            textBox40.Text = "";
        }

        private void button_del4_2_Click(object sender, EventArgs e)
        {
            ListBox l = tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox;
            l.Items.Remove(l.SelectedItem);
        }

        private void button_add4_3_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                if (textBox40.Text == "") return;
                if (int.TryParse(textBox40.Text, out int res)) textBox40.Text = res.ToString("D2");

                if (textBox40.Text == "00")
                {
                    MessageBox.Show("测试机中\"00\"代表未放料，禁止添加该Bin项!");
                    return;
                }

                foreach (object listbox in tabPage_bin.Controls)
                {
                    if (listbox.GetType() != typeof(ListBox))
                    {
                        continue;
                    }

                    foreach (object it in (listbox as ListBox).Items)
                    {
                        if (textBox40.Text == it.ToString())
                        {
                            MessageBox.Show("Bin项目重复");
                            return;
                        }
                    }
                }

                (tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox).Items.Add(textBox40.Text);
            }
            else
            {
                foreach (object listbox in tabPage_bin.Controls)
                {
                    if (listbox.GetType() != typeof(ListBox))
                    {
                        continue;
                    }
                    foreach (object it in (listbox as ListBox).Items)
                    {
                        if (comboBox10.Text == it.ToString())
                        {
                            MessageBox.Show("Bin项目重复");
                            return;
                        }
                    }
                }
                (tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox).Items.Add(comboBox10.Text);
            }
            textBox40.Text = "";
        }

        private void button_del4_3_Click(object sender, EventArgs e)
        {
            ListBox l = tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox;
            l.Items.Remove(l.SelectedItem);
        }

        private void button_add5_1_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                if (textBox40.Text == "") return;
                if (int.TryParse(textBox40.Text, out int res)) textBox40.Text = res.ToString("D2");

                if (textBox40.Text == "00")
                {
                    MessageBox.Show("测试机中\"00\"代表未放料，禁止添加该Bin项!");
                    return;
                }

                foreach (object listbox in tabPage_bin.Controls)
                {
                    if (listbox.GetType() != typeof(ListBox))
                    {
                        continue;
                    }

                    foreach (object it in (listbox as ListBox).Items)
                    {
                        if (textBox40.Text == it.ToString())
                        {
                            MessageBox.Show("Bin项目重复");
                            return;
                        }
                    }
                }

                (tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox).Items.Add(textBox40.Text);
            }
            else
            {
                foreach (object listbox in tabPage_bin.Controls)
                {
                    if (listbox.GetType() != typeof(ListBox))
                    {
                        continue;
                    }
                    foreach (object it in (listbox as ListBox).Items)
                    {
                        if (comboBox10.Text == it.ToString())
                        {
                            MessageBox.Show("Bin项目重复");
                            return;
                        }
                    }
                }
                (tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox).Items.Add(comboBox10.Text);
            }
            textBox40.Text = "";
        }

        private void button_del5_1_Click(object sender, EventArgs e)
        {
            ListBox l = tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox;
            l.Items.Remove(l.SelectedItem);
        }

        private void button_add5_2_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                if (textBox40.Text == "") return;
                if (int.TryParse(textBox40.Text, out int res)) textBox40.Text = res.ToString("D2");

                if (textBox40.Text == "00")
                {
                    MessageBox.Show("测试机中\"00\"代表未放料，禁止添加该Bin项!");
                    return;
                }

                foreach (object listbox in tabPage_bin.Controls)
                {
                    if (listbox.GetType() != typeof(ListBox))
                    {
                        continue;
                    }

                    foreach (object it in (listbox as ListBox).Items)
                    {
                        if (textBox40.Text == it.ToString())
                        {
                            MessageBox.Show("Bin项目重复");
                            return;
                        }
                    }
                }

                (tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox).Items.Add(textBox40.Text);
            }
            else
            {
                foreach (object listbox in tabPage_bin.Controls)
                {
                    if (listbox.GetType() != typeof(ListBox))
                    {
                        continue;
                    }
                    foreach (object it in (listbox as ListBox).Items)
                    {
                        if (comboBox10.Text == it.ToString())
                        {
                            MessageBox.Show("Bin项目重复");
                            return;
                        }
                    }
                }
                (tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox).Items.Add(comboBox10.Text);
            }
            textBox40.Text = "";
        }

        private void button_del5_2_Click(object sender, EventArgs e)
        {
            ListBox l = tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox;
            l.Items.Remove(l.SelectedItem);
        }

        private void button_add5_3_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                if (textBox40.Text == "") return;
                if (int.TryParse(textBox40.Text, out int res)) textBox40.Text = res.ToString("D2");

                if (textBox40.Text == "00")
                {
                    MessageBox.Show("测试机中\"00\"代表未放料，禁止添加该Bin项!");
                    return;
                }

                foreach (object listbox in tabPage_bin.Controls)
                {
                    if (listbox.GetType() != typeof(ListBox))
                    {
                        continue;
                    }

                    foreach (object it in (listbox as ListBox).Items)
                    {
                        if (textBox40.Text == it.ToString())
                        {
                            MessageBox.Show("Bin项目重复");
                            return;
                        }
                    }
                }

                (tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox).Items.Add(textBox40.Text);
            }
            else
            {
                foreach (object listbox in tabPage_bin.Controls)
                {
                    if (listbox.GetType() != typeof(ListBox))
                    {
                        continue;
                    }
                    foreach (object it in (listbox as ListBox).Items)
                    {
                        if (comboBox10.Text == it.ToString())
                        {
                            MessageBox.Show("Bin项目重复");
                            return;
                        }
                    }
                }
                (tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox).Items.Add(comboBox10.Text);
            }
            textBox40.Text = "";
        }

        private void button_del5_3_Click(object sender, EventArgs e)
        {
            ListBox l = tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox;
            l.Items.Remove(l.SelectedItem);
        }

        private void textBox40_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 8)
            {
                return;
            }

            if (textBox1.Text.Length > 1)
            {
                e.Handled = true;
                return;
            }

            if (e.KeyChar >= 97 && e.KeyChar <= 122)
            {
                e.KeyChar = (char)(e.KeyChar - 32);
            }

            if (!((e.KeyChar >= 48 && e.KeyChar <= 57) || (e.KeyChar >= 65 && e.KeyChar <= 90)))
            {
                e.Handled = true;
            }
        }

        private void uiButton4_Click(object sender, EventArgs e)
        {
            int rows = uiDataGridView1.SelectedIndex;
            if (rows >= 0)
            {
                string tabtext = uiTabControl6.SelectedTab.Text;
                if (tabtext == "分BIN")
                {
                    刷新分BIN(uiDataGridView1.Rows[rows].Cells[1].Value.ToString());
                }
                else if (tabtext == "测试机")
                {
                    刷新测试机(uiDataGridView1.Rows[rows].Cells[1].Value.ToString());
                }
                else if (tabtext == "点位偏移")
                {
                    刷新点位偏移(uiDataGridView1.Rows[rows].Cells[1].Value.ToString());
                }
            }
        }

        private void label_color1_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();

            (sender as Label).BackColor = colorDialog1.Color;
        }

        private void label_color2_Click(object sender, EventArgs e)
        {

            colorDialog1.ShowDialog();

            (sender as Label).BackColor = colorDialog1.Color;
        }

        private void label_color3_Click(object sender, EventArgs e)
        {

            colorDialog1.ShowDialog();

            (sender as Label).BackColor = colorDialog1.Color;
        }

        private void label_color4_Click(object sender, EventArgs e)
        {

            colorDialog1.ShowDialog();

            (sender as Label).BackColor = colorDialog1.Color;
        }

        private void label_color5_Click(object sender, EventArgs e)
        {

            colorDialog1.ShowDialog();

            (sender as Label).BackColor = colorDialog1.Color;
        }

        private void uiDataGridView3_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            int index = e.RowIndex;
            if (index >= 0)
            {
                刷新加热盘信息(uiTabControl9.SelectedTab.Text);
            }
        }

        private void 保存Tray盘()
        {
            DialogResult dialog = MessageBox.Show("请确认是否保存!", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (dialog == DialogResult.OK)
            {
                TrayInfo newtrayInfo = new TrayInfo()
                {
                    TrayName = textBox30.Text,
                    X_Num = Convert.ToInt32(textBox29.Text),
                    Y_Num = Convert.ToInt32(textBox28.Text),
                    X_Start = Convert.ToDouble(textBox27.Text),
                    Y_Start = Convert.ToDouble(textBox31.Text),
                    X_Pitch = Convert.ToDouble(textBox33.Text),
                    Y_Pitch = Convert.ToDouble(textBox32.Text),
                    Width = Convert.ToDouble(textBox36.Text),
                    Length = Convert.ToDouble(textBox35.Text),
                    Height = Convert.ToDouble(textBox34.Text),
                };

                int rows = uiDataGridView2.SelectedIndex;
                if (rows >= 0)
                {
                    string name = uiDataGridView2.Rows[rows].Cells[1].Value.ToString();

                    string result = GetTrayInfo(name, out TrayInfo oldtrayInfo);
                    if (result == "Success")
                    {
                        UpdateTrayInfo(name, oldtrayInfo.TrayName, "xnum", newtrayInfo.X_Num, oldtrayInfo.X_Num);
                        UpdateTrayInfo(name, oldtrayInfo.TrayName, "ynum", newtrayInfo.Y_Num, oldtrayInfo.Y_Num);
                        UpdateTrayInfo(name, oldtrayInfo.TrayName, "xstart", newtrayInfo.X_Start, oldtrayInfo.X_Start);
                        UpdateTrayInfo(name, oldtrayInfo.TrayName, "ystart", newtrayInfo.Y_Start, oldtrayInfo.Y_Start);
                        UpdateTrayInfo(name, oldtrayInfo.TrayName, "xpitch", newtrayInfo.X_Pitch, oldtrayInfo.X_Pitch);
                        UpdateTrayInfo(name, oldtrayInfo.TrayName, "ypitch", newtrayInfo.Y_Pitch, oldtrayInfo.Y_Pitch);
                        UpdateTrayInfo(name, oldtrayInfo.TrayName, "width", newtrayInfo.Width, oldtrayInfo.Width);
                        UpdateTrayInfo(name, oldtrayInfo.TrayName, "length", newtrayInfo.Length, oldtrayInfo.Length);
                        UpdateTrayInfo(name, oldtrayInfo.TrayName, "height", newtrayInfo.Height, oldtrayInfo.Height);

                        if (oldtrayInfo.TrayName == 设置信息.Tray盘信息.TrayName)
                        {
                            设置信息.Tray盘信息 = newtrayInfo;
                            result = Init_TrayPoint();
                            if (result != "Success")
                            {
                                MessageBox.Show("获取tray盘信息保存失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    else MessageBox.Show("获取Tray盘信息失败!" + result);
                }
            }
        }

        private void 保存Heat盘()
        {
            DialogResult dialog = MessageBox.Show("请确认是否保存!", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (dialog == DialogResult.OK)
            {
                HeatInfo newheatInfo = new HeatInfo()
                {
                    HeatName = textBox57.Text,
                    X_Num = Convert.ToInt32(textBox60.Text),
                    Y_Num = Convert.ToInt32(textBox59.Text),
                    X_Start = Convert.ToDouble(textBox58.Text),
                    Y_Start = Convert.ToDouble(textBox56.Text),
                    X_Pitch = Convert.ToDouble(textBox55.Text),
                    Y_Pitch = Convert.ToDouble(textBox54.Text),
                    Width = Convert.ToDouble(textBox53.Text),
                    Length = Convert.ToDouble(textBox52.Text),
                    Height = Convert.ToDouble(textBox51.Text),
                    设定温度 = Convert.ToDouble(numericUpDown129.Text),
                    波动温度 = Convert.ToDouble(numericUpDown130.Text),
                    波动延时报警 = Convert.ToInt32(numericUpDown134.Text),
                    达温波动报警 = Convert.ToInt32(numericUpDown154.Text),
                    报警温度 = Convert.ToDouble(numericUpDown131.Text),
                    超温温度 = Convert.ToDouble(numericUpDown132.Text),
                    升温超时报警 = Convert.ToInt32(numericUpDown133.Text),
                    达温标准 = uiComboBox8.Text,
                    探头类型 = uiComboBox9.Text,
                    温控器温度补偿 = Convert.ToDouble(numericUpDown135.Text),
                    Site1温度补偿 = Convert.ToDouble(numericUpDown136.Text),
                    Site2温度补偿 = Convert.ToDouble(numericUpDown137.Text),
                    Site3温度补偿 = Convert.ToDouble(numericUpDown139.Text),
                    Site4温度补偿 = Convert.ToDouble(numericUpDown138.Text),
                    Site5温度补偿 = Convert.ToDouble(numericUpDown143.Text),
                    Site6温度补偿 = Convert.ToDouble(numericUpDown142.Text),
                    Site7温度补偿 = Convert.ToDouble(numericUpDown141.Text),
                    Site8温度补偿 = Convert.ToDouble(numericUpDown140.Text),
                    Site9温度补偿 = Convert.ToDouble(numericUpDown147.Text),
                    Site10温度补偿 = Convert.ToDouble(numericUpDown146.Text),
                    Site11温度补偿 = Convert.ToDouble(numericUpDown145.Text),
                    Site12温度补偿 = Convert.ToDouble(numericUpDown144.Text),
                    Site13温度补偿 = Convert.ToDouble(numericUpDown151.Text),
                    Site14温度补偿 = Convert.ToDouble(numericUpDown150.Text),
                    Site15温度补偿 = Convert.ToDouble(numericUpDown149.Text),
                    Site16温度补偿 = Convert.ToDouble(numericUpDown148.Text),

                };

                int rows = uiDataGridView3.SelectedIndex;
                if (rows >= 0)
                {
                    string name = uiDataGridView3.Rows[rows].Cells[1].Value.ToString();

                    string result = GetHeatInfo(name, out HeatInfo oldheatInfo);
                    if (result == "Success")
                    {
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "xnum", newheatInfo.X_Num, oldheatInfo.X_Num);
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "ynum", newheatInfo.Y_Num, oldheatInfo.Y_Num);
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "xstart", newheatInfo.X_Start, oldheatInfo.X_Start);
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "ystart", newheatInfo.Y_Start, oldheatInfo.Y_Start);
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "xpitch", newheatInfo.X_Pitch, oldheatInfo.X_Pitch);
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "ypitch", newheatInfo.Y_Pitch, oldheatInfo.Y_Pitch);
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "width", newheatInfo.Width, oldheatInfo.Width);
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "length", newheatInfo.Length, oldheatInfo.Length);
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "height", newheatInfo.Height, oldheatInfo.Height);
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "设定温度", newheatInfo.设定温度, oldheatInfo.设定温度);
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "波动温度", newheatInfo.波动温度, oldheatInfo.波动温度);
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "波动延时报警", newheatInfo.波动延时报警, oldheatInfo.波动延时报警);
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "达温波动报警", newheatInfo.达温波动报警, oldheatInfo.达温波动报警);
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "报警温度", newheatInfo.报警温度, oldheatInfo.报警温度);
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "超温温度", newheatInfo.超温温度, oldheatInfo.超温温度);
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "升温超时报警", newheatInfo.升温超时报警, oldheatInfo.升温超时报警);
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "达温标准", newheatInfo.达温标准, oldheatInfo.达温标准);
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "探头类型", newheatInfo.探头类型, oldheatInfo.探头类型);
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "温控器温度补偿", newheatInfo.温控器温度补偿, oldheatInfo.温控器温度补偿);
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "Site1温度补偿", newheatInfo.Site1温度补偿, oldheatInfo.Site1温度补偿);
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "Site2温度补偿", newheatInfo.Site2温度补偿, oldheatInfo.Site2温度补偿);
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "Site3温度补偿", newheatInfo.Site3温度补偿, oldheatInfo.Site3温度补偿);
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "Site4温度补偿", newheatInfo.Site4温度补偿, oldheatInfo.Site4温度补偿);
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "Site5温度补偿", newheatInfo.Site5温度补偿, oldheatInfo.Site5温度补偿);
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "Site6温度补偿", newheatInfo.Site6温度补偿, oldheatInfo.Site6温度补偿);
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "Site7温度补偿", newheatInfo.Site7温度补偿, oldheatInfo.Site7温度补偿);
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "Site8温度补偿", newheatInfo.Site8温度补偿, oldheatInfo.Site8温度补偿);
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "Site9温度补偿", newheatInfo.Site9温度补偿, oldheatInfo.Site9温度补偿);
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "Site10温度补偿", newheatInfo.Site10温度补偿, oldheatInfo.Site10温度补偿);
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "Site11温度补偿", newheatInfo.Site11温度补偿, oldheatInfo.Site11温度补偿);
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "Site12温度补偿", newheatInfo.Site12温度补偿, oldheatInfo.Site12温度补偿);
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "Site13温度补偿", newheatInfo.Site13温度补偿, oldheatInfo.Site13温度补偿);
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "Site14温度补偿", newheatInfo.Site14温度补偿, oldheatInfo.Site14温度补偿);
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "Site15温度补偿", newheatInfo.Site15温度补偿, oldheatInfo.Site15温度补偿);
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "Site16温度补偿", newheatInfo.Site16温度补偿, oldheatInfo.Site16温度补偿);
                        if (oldheatInfo.HeatName == 设置信息.Heat盘信息.HeatName)
                        {
                            设置信息.Heat盘信息 = newheatInfo;
                            HeatTrayICPointInit();
                            刷新温度信息();
                        }
                    }
                    else MessageBox.Show("获取Heat盘信息失败!" + result);
                }
            }
        }

        private void uiButton16_保存Tray盘信息Click(object sender, EventArgs e)
        {

            int rows = uiDataGridView2.SelectedIndex;
            if (rows >= 0)
            {
                string tabtext = uiTabControl7.SelectedTab.Text;
                if (tabtext == "Tray盘")
                {
                    保存Tray盘();
                }
                else if (tabtext == "取放设定")
                {

                }
            }

        }

        private void uiButton28_保存加热盘信息Click(object sender, EventArgs e)
        {
            int rows = uiDataGridView3.SelectedIndex;
            if (rows >= 0)
            {
                string tabtext = uiTabControl9.SelectedTab.Text;

                if (tabtext == "加热盘")
                {
                    保存Heat盘();
                }
                else if (tabtext == "温度设定")
                {
                    保存Heat盘();
                }
            }
        }

        private void uiTabControl8_Selecting(object sender, TabControlCancelEventArgs e)
        {
            int rows = uiDataGridView1.SelectedIndex;
            if (rows >= 0)
            {
                刷新点位偏移(uiDataGridView1.Rows[rows].Cells[1].Value.ToString());
            }
        }

        public bool UpdateTrayInfo(string name, string trayname, string fiedname, double newvalue, double oldvalue)
        {
            if (newvalue != oldvalue)
            {
                int line = SQLiteHelper.runSqlNonQuery(SQLiteConnString, $"update TrayInfo set {fiedname} = '{newvalue}' where name = '{trayname}'");
                if (line >= 1)
                {
                    RunLog($"档案名称-[{name}]-tray盘名称-[{trayname}]-[{fiedname}]{oldvalue}->{newvalue}:更新成功", WriteData.LogType.ParamLog);
                    return true;
                }
                else RunLog($"档案名称-[{name}]-tray盘名称-[{trayname}]-[{fiedname}]{oldvalue}->{newvalue}:更新失败", WriteData.LogType.ParamLog);
            }
            return false;
        }
        public bool UpdateHeatInfo(string name, string trayname, string fiedname, double newvalue, double oldvalue)
        {
            if (newvalue != oldvalue)
            {
                int line = SQLiteHelper.runSqlNonQuery(SQLiteConnString, $"update HeatInfo set {fiedname} = '{newvalue}' where name = '{trayname}'");
                if (line >= 1)
                {
                    RunLog($"档案名称-[{name}]-加热盘名称-[{trayname}]-[{fiedname}]{oldvalue}->{newvalue}:更新成功", WriteData.LogType.ParamLog);
                    return true;
                }
                else RunLog($"档案名称-[{name}]-加热盘名称-[{trayname}]-[{fiedname}]{oldvalue}->{newvalue}:更新失败", WriteData.LogType.ParamLog);
            }
            return false;
        }
        public bool UpdateHeatInfo(string name, string trayname, string fiedname, string newvalue, string oldvalue)
        {
            if (newvalue != oldvalue)
            {
                int line = SQLiteHelper.runSqlNonQuery(SQLiteConnString, $"update HeatInfo set {fiedname} = '{newvalue}' where name = '{trayname}'");
                if (line >= 1)
                {
                    RunLog($"档案名称-[{name}]-加热盘名称-[{trayname}]-[{fiedname}]{oldvalue}->{newvalue}:更新成功", WriteData.LogType.ParamLog);
                    return true;
                }
                else RunLog($"档案名称-[{name}]-加热盘名称-[{trayname}]-[{fiedname}]{oldvalue}->{newvalue}:更新失败", WriteData.LogType.ParamLog);
            }
            return false;
        }

        public bool UpdateOffsetPointData(string icname, string otname, string ptname, double newvalue, double oldvalue)
        {
            if (newvalue != oldvalue)
            {
                int line = SQLiteHelper.runSqlNonQuery(SQLiteConnString, $"update OffsetPoint set {ptname} = '{newvalue}' where 微调点位名称 = '{otname}'");
                if (line >= 1)
                {
                    RunLog($"档案名称-[{icname}]-偏移点位名称-[{otname}]-点位-[{ptname}]{oldvalue}->{newvalue}:更新成功", WriteData.LogType.ParamLog);
                    return true;
                }
                else RunLog($"档案名称-[{icname}]-偏移点位名称-[{otname}]-点位-[{ptname}]{oldvalue}->{newvalue}:更新失败", WriteData.LogType.ParamLog);
            }
            return false;
        }

        private void button227_Click(object sender, EventArgs e)
        {
            //飞梭前入料IC = new bool[][] { new bool[] { true, true, true, true }, new bool[] { true, true, true, true } };
            飞梭后入料IC = new bool[][] { new bool[] { false, false, false, false }, new bool[] { true, false, false, false } };
            //if (入料手臂停止工作标记 && 结批标记)
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
                    RunLog("aaa");
                    //测试手臂入料停止工作标记 = true;
                    //PS测试手臂取待测料流程.Processing = false;
                    //PS测试手臂取待测料流程.StepID = 500;
                    //break;
                }
            }

            //结批时判断是否关site(out bool[] open, out bool[] open2);

            //飞梭前入料IC = new bool[][] { new bool[] { true, true, true, true }, new bool[] { true, true, true, true } };
            //飞梭后入料IC = new bool[][] { new bool[] { true, true, true, true }, new bool[] { true, true, true, true } };
            //if (飞梭后入料IC[0].Contains(true) || 飞梭后入料IC[1].Contains(true) || 飞梭前入料IC[0].Contains(true) || 飞梭前入料IC[1].Contains(true))
            //{
            //    if (放料IC目标值[0].Contains(true))
            //    {
            //        if (飞梭后入料IC[0].Contains(true) || 飞梭前入料IC[0].Contains(true))
            //        {
            //            for (int i = 0; i < 放料IC目标值[0].Length; i++)
            //            {
            //                if (!放料IC目标值[0][i])
            //                {
            //                    if (飞梭后入料IC[0][i] || 飞梭前入料IC[0][i])
            //                    {
            //                        RunLog($"梭车后入料：Dut{i + 1}中IC无法被取出");
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    if (放料IC目标值[1].Contains(true))
            //    {
            //        if (飞梭后入料IC[1].Contains(true) || 飞梭前入料IC[1].Contains(true))
            //        {
            //            for (int i = 0; i < 放料IC目标值[1].Length; i++)
            //            {
            //                if (!放料IC目标值[1][i])
            //                {
            //                    if (飞梭后入料IC[1][i] || 飞梭前入料IC[1][i])
            //                    {
            //                        RunLog($"梭车后入料：Dut{i + 4 + 1}中IC无法被取出");
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}

            //for (int i = 0; i < Auto2XPoint.Length; i++)
            //{
            //    for (int j = 0; j < Auto2YPoint.Length; j++)
            //    {
            //        Auto2DicPoint[$"{i},{j}"] = false;
            //        //trayTemp += Auto2DicBin[$"{i},{j}"] + ",";
            //        //Auto2DicBin[$"{i},{j}"] = "";
            //    }
            //}
            //Auto2DicPoint[$"0,0"] = true;
            //bool isIC1 = 获取Auto2盘坐标(false, out int auto2xPoint, out int auto2yPoint);
            //if (!isIC1 || auto2xPoint != 0 || auto2yPoint != 0) //如果有放料才需要盖空盘
            //{
            //    RunLog("食醋胡");
            //    //auto2isding = true;
            //    //抓盘手臂放空盘到Auto2标记 = 2;
            //}
            //string result = handler.飞梭_前后检测产品感应(out string[] iovalues);
            //if (result == "Success")
            //{
            //    if (!iovalues.Contains("0"))
            //    {
            //       //PS入飞梭移动流程.StepID = 515;
            //    }
            //    else
            //    {
            //        RunLog("飞梭前后检测产品感应有IC放歪");
            //        //ProgramPause = true;
            //        //IsPause = true;
            //    }
            //}
            //飞梭前出料BIN项 = new string[][] { new string[] { "01", "02", "03", "04" }, new string[] { "05", "06", "07", "08" } };

            //string[][] bins = new string[2][]
            // {
            //    new string[]{ 飞梭前出料BIN项[0][0], 飞梭前出料BIN项[0][1], 飞梭前出料BIN项[0][2], 飞梭前出料BIN项[0][3]},
            //    new string[]{ 飞梭前出料BIN项[1][0], 飞梭前出料BIN项[1][1], 飞梭前出料BIN项[1][2], 飞梭前出料BIN项[1][3]},
            // };

            //for (int i = 0; i < bins.Length; i++)
            //{
            //    for (int j = 0; j < bins[i].Length; j++)
            //    {
            //        oth = bins[i][j];
            //        if (!Auto1.Split(",").Contains(oth) && !Auto2.Split(",").Contains(oth) && !Fix1.Split(",").Contains(oth) && !Fix2.Split(",").Contains(oth) && !Fix3.Split(",").Contains(oth))
            //        {
            //            if (!othAll.Contains(oth))
            //            {
            //                othAll += oth + ",";
            //            }
            //            bins[i][j] = "OTH";
            //        }
            //    }
            //}
            //handler.debug();
        }

        private void button228_Click(object sender, EventArgs e)
        {
            int chl = Convert.ToInt32(numericUpDown128.Value);
            int staID = Convert.ToInt32(numericUpDown127.Value);
            double vaule = Convert.ToDouble(numericUpDown126.Value);
            string result = dTMHelper.WriteSv(staID, chl, vaule);
            if (result != "Success")
            {
                MessageBox.Show("设定失败!" + result);
            }
        }

        private void button229_Click(object sender, EventArgs e)
        {
            int chl = Convert.ToInt32(numericUpDown128.Value);
            int staID = Convert.ToInt32(numericUpDown127.Value);
            string result = dTMHelper.ReadSv(staID, chl, out short[] value);
            if (result != "Success")
            {
                MessageBox.Show("读取失败!" + result);
            }
            else uiTextBox3.Text = ((double)value[0] / 10).ToString(); //(value / 10).Tostring();
        }

        private void button230_Click(object sender, EventArgs e)
        {
            int staID = Convert.ToInt32(numericUpDown127.Value);
            string result = dTMHelper.ReadSv(staID, out short[] value);
            if (result != "Success")
            {
                MessageBox.Show("读取失败!" + result);
            }
            string strvaue = (value[0] / 10).ToString() + ",";
            for (int i = 1; i < value.Length; i++)
            {
                strvaue += (value[i] / 10).ToString() + ",";
            }
            RunLog(strvaue);
            uiTextBox4.Text = strvaue;
        }

        private void button231_Click(object sender, EventArgs e)
        {
            int staID = Convert.ToInt32(numericUpDown127.Value);
            double vaule = Convert.ToDouble(numericUpDown126.Value);
            for (int i = 0; i < 8; i++)
            {
                string result = dTMHelper.WriteSv(staID, i, vaule);
                if (result != "Success")
                {
                    MessageBox.Show("设定失败!" + result);
                }
            }
        }

        private void button232_Click(object sender, EventArgs e)
        {
            int staID = Convert.ToInt32(numericUpDown127.Value);
            string result = dTMHelper.ReadSvMax(staID, out short[] value);
            if (result != "Success")
            {
                MessageBox.Show("读取失败!" + result);
            }
            string strvaue = (value[0] / 10).ToString() + ",";
            for (int i = 1; i < value.Length; i++)
            {
                strvaue += (value[i] / 10).ToString() + ",";
            }
            RunLog(strvaue);
            uiTextBox5.Text = strvaue;
        }

        private void button233_Click(object sender, EventArgs e)
        {
            int staID = Convert.ToInt32(numericUpDown127.Value);
            string result = dTMHelper.ReadSvMin(staID, out short[] value);
            if (result != "Success")
            {
                MessageBox.Show("读取失败!" + result);
            }
            string strvaue = (value[0] / 10).ToString() + ",";
            for (int i = 1; i < value.Length; i++)
            {
                strvaue += (value[i] / 10).ToString() + ",";
            }
            RunLog(strvaue);
            uiTextBox6.Text = strvaue;
        }

        private void button234_Click(object sender, EventArgs e)
        {
            int staID = Convert.ToInt32(numericUpDown127.Value);
            double vaule = Convert.ToDouble(numericUpDown126.Value);
            for (int i = 0; i < 8; i++)
            {
                string result = dTMHelper.WriteSvMax(staID, i, vaule);
                if (result != "Success")
                {
                    MessageBox.Show("设定失败!" + result);
                }
            }
        }

        private void button235_Click(object sender, EventArgs e)
        {
            int staID = Convert.ToInt32(numericUpDown127.Value);
            double vaule = Convert.ToDouble(numericUpDown126.Value);
            for (int i = 0; i < 8; i++)
            {
                string result = dTMHelper.WriteSvMin(staID, i, vaule);
                if (result != "Success")
                {
                    MessageBox.Show("设定失败!" + result);
                }
            }
        }

        private void uiCheckBox192_Click(object sender, EventArgs e)
        {
            uiCheckBox193.Checked = !uiCheckBox192.Checked;
            label355.Visible = uiCheckBox193.Checked;
            uiComboBox2.Visible = uiCheckBox193.Checked;
        }

        private void uiCheckBox193_Click(object sender, EventArgs e)
        {
            uiCheckBox192.Checked = !uiCheckBox193.Checked;
            label355.Visible = uiCheckBox193.Checked;
            uiComboBox2.Visible = uiCheckBox193.Checked;
        }

        /// <summary>
        /// 新增档案
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton12_Click(object sender, EventArgs e)
        {

        }

        private void uiTabControl9_Selecting(object sender, TabControlCancelEventArgs e)
        {
            刷新加热盘信息(uiTabControl9.SelectedTab.Text);
        }

        private void uiCheckBox196_Click(object sender, EventArgs e)
        {
            if (uiCheckBox196.Checked)
            {
                uiCheckBox194.Checked = false;
                uiCheckBox195.Checked = false;
            }
            else uiCheckBox196.Checked = true;
        }

        private void uiCheckBox194_Click(object sender, EventArgs e)
        {
            if (uiCheckBox194.Checked)
            {
                uiCheckBox195.Checked = false;
                uiCheckBox196.Checked = false;
            }
            else uiCheckBox194.Checked = true;
        }

        private void uiCheckBox195_Click(object sender, EventArgs e)
        {
            if (uiCheckBox195.Checked)
            {
                uiCheckBox194.Checked = false;
                uiCheckBox196.Checked = false;
            }
            else uiCheckBox195.Checked = true;
        }

        private void uiCheckBox197_Click(object sender, EventArgs e)
        {
            if (uiCheckBox197.Checked)
            {
                uiCheckBox198.Checked = false;
                uiCheckBox199.Checked = false;
            }
            else uiCheckBox197.Checked = true;
        }

        private void uiCheckBox199_Click(object sender, EventArgs e)
        {
            if (uiCheckBox199.Checked)
            {
                uiCheckBox198.Checked = false;
                uiCheckBox197.Checked = false;
            }
            else uiCheckBox199.Checked = true;
        }

        private void uiCheckBox198_Click(object sender, EventArgs e)
        {
            if (uiCheckBox198.Checked)
            {
                uiCheckBox199.Checked = false;
                uiCheckBox197.Checked = false;
            }
            else uiCheckBox198.Checked = true;
        }

        private void uiCheckBox200_Click(object sender, EventArgs e)
        {
            if (uiCheckBox200.Checked)
            {
                uiCheckBox201.Checked = false;
                uiCheckBox202.Checked = false;
            }
            else uiCheckBox200.Checked = true;
        }

        private void uiCheckBox202_Click(object sender, EventArgs e)
        {
            if (uiCheckBox202.Checked)
            {
                uiCheckBox200.Checked = false;
                uiCheckBox201.Checked = false;
            }
            else uiCheckBox202.Checked = true;
        }

        private void uiCheckBox201_Click(object sender, EventArgs e)
        {
            if (uiCheckBox201.Checked)
            {
                uiCheckBox200.Checked = false;
                uiCheckBox202.Checked = false;
            }
            else uiCheckBox201.Checked = true;
        }

        private void button239_Click(object sender, EventArgs e)
        {
            short site = Convert.ToInt16(uiComboBox10.Text.Split("_")[1]);
            string result = handler.测试区_位移气缸到位输出((short)(site - 1));
            if (result != "Success")
            {
                MessageBox.Show("动作失败!" + result);
            }
            else
            {
                MessageBox.Show("动作成功!");
            }
        }

        private void button238_Click(object sender, EventArgs e)
        {
            short site = Convert.ToInt16(uiComboBox10.Text.Split("_")[1]);
            string result = handler.测试区_位移气缸原位输出((short)(site - 1));
            if (result != "Success")
            {
                MessageBox.Show("动作失败!" + result);
            }
            else
            {
                MessageBox.Show("动作成功!");
            }
        }

        private void button236_Click(object sender, EventArgs e)
        {
            short site = Convert.ToInt16(uiComboBox10.Text.Split("_")[1]);
            string result = handler.测试区_下压气缸原位输出((short)(site - 1));
            if (result != "Success")
            {
                MessageBox.Show("动作失败!" + result);
            }
            else
            {
                MessageBox.Show("动作成功!");
            }
        }

        private void button237_Click(object sender, EventArgs e)
        {
            short site = Convert.ToInt16(uiComboBox10.Text.Split("_")[1]);
            string result = handler.测试区_下压气缸到位输出((short)(site - 1));
            if (result != "Success")
            {
                MessageBox.Show("动作失败!" + result);
            }
            else
            {
                MessageBox.Show("动作成功!");
            }
        }

        private void uiCheckBox203_Click(object sender, EventArgs e)
        {
            空跑模式 = uiCheckBox203.Checked;
            if (空跑模式)
            {
                uiCheckBox204.Enabled = true;
                uiCheckBox206.Enabled = true;
            }
            else
            {
                Tray盘模式 = true; //不是空跑模式时，Tray盘模式默认开启
                uiCheckBox204.Checked = true;

                无测试机模式 = false; //不是空跑模式时，默认关闭无测试机模式
                uiCheckBox206.Checked = false;

                uiCheckBox204.Enabled = false;
                uiCheckBox206.Enabled = false;
            }
        }

        private void uiCheckBox204_Click(object sender, EventArgs e)
        {
            Tray盘模式 = uiCheckBox204.Checked;
        }

        private void uiCheckBox205_Click(object sender, EventArgs e)
        {
            安全门禁用 = uiCheckBox205.Checked;
        }

        private void uiCheckBox206_Click(object sender, EventArgs e)
        {
            无测试机模式 = uiCheckBox206.Checked;
        }

        private void button240_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(numericUpDown152.Text);
            入料手臂变距轴移动初始点位(pos);
        }

        private void button242_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(numericUpDown155.Text);
            入料手臂变距轴移动初始点位(pos);
        }

        private void button241_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(numericUpDown153.Text);
            入料手臂变距轴移动初始点位(pos);
        }

        private void button245_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(numericUpDown158.Text);
            出料手臂变距轴移动初始点位(pos);
        }

        private void button244_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(numericUpDown157.Text);
            出料手臂变距轴移动初始点位(pos);
        }

        private void uiComboBox11_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                uiComboBox11.DataSource = SerialPort.GetPortNames();
                //uiComboBox11.Text = 设置信息.测试机信息.串口号;
            }
        }

        //复制档案
        private void uiButton8_Click(object sender, EventArgs e)
        {
            int rows = uiDataGridView1.SelectedIndex;
            if (rows >= 0)
            {
                string name = uiDataGridView1.Rows[rows].Cells[1].Value.ToString();
                string result = GetSetupInfo(name, out SetupInfo setupInfo);
                if (result == "Success")
                {
                    string sql = $"insert into SetupInfo(档案名称,tray盘名称,site名称,加热盘名称,偏移点位名称,测试机名称,分BIN名称,梭车名称,温度模式)" +
                        $"values('{name}_副本','{setupInfo.Tray盘信息.TrayName}','{setupInfo.开site信息.Site名称}'," +
                        $"'{setupInfo.Heat盘信息.HeatName}','{setupInfo.OffsetPoint.微调点位名称}','{setupInfo.测试机信息.测试机名称}'," +
                        $"'{setupInfo.分BIN信息.分BIN名称}','{setupInfo.梭车名称}','{setupInfo.温度模式}')";
                    int line = SQLiteHelper.runSqlNonQuery(SQLiteConnString, sql);
                    if (line >= 0)
                    {
                        result = 刷新档案列表();
                        if (result == "Success") MessageBox.Show("复制档案成功!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        else MessageBox.Show("复制档案失败!" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else MessageBox.Show("复制档案失败!返回行数:0", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else MessageBox.Show("复制档案失败!" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else MessageBox.Show("请选择要复制档案!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
