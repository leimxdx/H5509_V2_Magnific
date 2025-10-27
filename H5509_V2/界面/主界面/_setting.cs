using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Resources;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Security.RightsManagement;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ComMaster.Interfaces;
using IHardwareGXN.GXNCard;
using MotionCommon;
using SQLiteHelper;
using SQLServerHelper;
using Sunny.UI;
using Sunny.UI.Win32;
using TextFileLogger;
using static H5509_V2.APPmaster;
using static SiCommunication.Profinet.Knx.KnxCode;
using Color = System.Drawing.Color;
using Control = System.Windows.Forms.Control;

namespace H5509_V2
{
    public partial class _setting : UIUserControl
    {
        private IGXNInterface motion;

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
        public Action 刷新按钮状态;
        public Action 刷新开Site状态;
        public Action 刷新档案选项;
        public Action 刷新自定义界面_委托方法;

        private UIComboBox[] UIComboBoxes;
        public _setting()
        {
            InitializeComponent();

            motion = MotionEx.IMotion;

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

            //uiComboBox11.DataSource = SerialPort.GetPortNames();
            //uiComboBox12.DataSource = new string[] { "9600", "19200", "38400", "57600", "115200" };
            //uiComboBox14.DataSource = new string[] { "None", "Odd", "Even" };
            //uiComboBox13.DataSource = new string[] { "7", "8" };
            //uiComboBox15.DataSource = new string[] { "1", "2" };
            uiComboBox50.DataSource = new string[] { "9600", "19200", "38400", "57600", "115200" };
            uiComboBox51.DataSource = new string[] { "None", "Odd", "Even" };
            uiComboBox49.DataSource = new string[] { "7", "8" };
            uiComboBox48.DataSource = new string[] { "1", "2" };

            UIComboBoxes = new UIComboBox[]
            {
                uiComboBox39, uiComboBox38, uiComboBox37, uiComboBox36,
                uiComboBox35, uiComboBox34, uiComboBox33, uiComboBox32,
                uiComboBox47, uiComboBox46, uiComboBox45, uiComboBox44,
                uiComboBox43, uiComboBox42, uiComboBox41, uiComboBox40,
            };

            foreach (UIComboBox uIComboBox in UIComboBoxes)
            {
                uIComboBox.MouseClick += 刷新串口;
            }



            richTextBox1.Text = Directory.GetCurrentDirectory() + "\\Log";

            刷新串口(SerialPort.GetPortNames());

            清空通讯方式();
            //清空通讯协议();
            刷新通讯方式();
            //刷新通讯协议();
            刷新测试机数量显示();

            TabPage[] initPointtabPages = { tabPage7, tabPage8, tabPage9, tabPage16, tabPage17, tabPage4, tabPage27, tabPage28, tabPage34, tabPage35, tabPage36, tabPage37, tabPage24, tabPage39, tabPage40 };
            foreach (TabPage tabPage in initPointtabPages)
            {
                foreach (Control control in tabPage.Controls)
                {
                    if (control is UITitlePanel uITitle)
                    {
                        foreach (Control control2 in uITitle.Controls)
                        {
                            if (control2 is NumericUpDown numericUpDown)
                            {
                                numericUpDown.MouseWheel += Ctl_MouseWheel;
                            }
                            else if (control2 is GroupBox groupBox)
                            {
                                foreach (Control control3 in control2.Controls)
                                {
                                    if (control3 is NumericUpDown numericUpDown2)
                                    {
                                        numericUpDown2.MouseWheel += Ctl_MouseWheel;
                                    }
                                }
                            }
                        }
                    }
                    else if (control is NumericUpDown numericUpDown3)
                    {
                        numericUpDown3.MouseWheel += Ctl_MouseWheel;
                    }
                }
            }
        }

        public void 刷新串口(object sender, MouseEventArgs e)
        {
            UIComboBox uICombox = sender as UIComboBox;
            string Text = uICombox.Text;
            if (e.Button == MouseButtons.Left)
            {
                uICombox.DataSource = SerialPort.GetPortNames();
                uICombox.DisplayMember = "String";
                //uiComboBox11.Text = 设置信息.测试机信息.串口号;
                uICombox.Text = Text;
            }
        }

        public void 刷新串口(string[] serialPort)
        {
            for (int i = 0; i < UIComboBoxes.Length; i++)
            {
                UIComboBoxes[i].DataSource = serialPort;
                UIComboBoxes[i].SelectedIndex = -1;
            }
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
            string result = motion.GNM601_GetDiBit(core, 3, axis, out short value);
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
                motion.GNM601_GetDi(1, 4, out string value);
                for (int i = 0; i + j < value.Length && i < GNM601DI.Length; i++)
                {
                    if (value[i + j] == '0') GNM601DI[i].BackColor = Color.LimeGreen;
                    else GNM601DI[i].BackColor = Color.Silver;
                }

                j = uiChk_GNM1.Checked ? 0 : 6;
                motion.GNM601_GetDi(1, 3, out value);
                for (int i = 0; i + j < value.Length && i < GNM601Home.Length; i++)
                {
                    if (value[i + j] == '0') GNM601Home[i].BackColor = Color.LimeGreen;
                    else GNM601Home[i].BackColor = Color.Silver;
                }

                motion.GNM601_GetDi(1, 1, out value);
                for (int i = 0; i + j < value.Length && i < GNM601LimitN.Length; i++)
                {
                    if (value[i + j] == '1') GNM601LimitN[i].BackColor = Color.LimeGreen;
                    else GNM601LimitN[i].BackColor = Color.Silver;
                }

                motion.GNM601_GetDi(1, 0, out value);
                for (int i = 0; i + j < value.Length && i < GNM601LimitP.Length; i++)
                {
                    if (value[i + j] == '1') GNM601LimitP[i].BackColor = Color.LimeGreen;
                    else GNM601LimitP[i].BackColor = Color.Silver;
                }

                j = uiChk_GNM1.Checked ? 0 : 10;
                motion.GNM601_GetDo(1, 12, out value);
                for (int i = 0; i + j < value.Length && i < GNM601DO.Length; i++)
                {
                    if (value[i + j] == '1') GNM601DO[i].BackColor = Color.LimeGreen;
                    else GNM601DO[i].BackColor = Color.Silver;
                }
            }
            else if (uiChk_GNM3.Checked || uiChk_GNM4.Checked)
            {
                int j = uiChk_GNM3.Checked ? 0 : 16;
                motion.GNM601_GetDi(2, 4, out string value);
                for (int i = 0; i + j < value.Length && i < GNM601DI.Length; i++)
                {
                    if (value[i + j] == '0') GNM601DI[i].BackColor = Color.LimeGreen;
                    else GNM601DI[i].BackColor = Color.Silver;
                }

                j = uiChk_GNM3.Checked ? 0 : 6;
                motion.GNM601_GetDi(2, 3, out value);
                for (int i = 0; i + j < value.Length && i < GNM601Home.Length; i++)
                {
                    if (value[i + j] == '0') GNM601Home[i].BackColor = Color.LimeGreen;
                    else GNM601Home[i].BackColor = Color.Silver;
                }

                motion.GNM601_GetDi(2, 1, out value);
                for (int i = 0; i + j < value.Length && i < GNM601LimitN.Length; i++)
                {
                    if (value[i + j] == '1') GNM601LimitN[i].BackColor = Color.LimeGreen;
                    else GNM601LimitN[i].BackColor = Color.Silver;
                }

                motion.GNM601_GetDi(2, 0, out value);
                for (int i = 0; i + j < value.Length && i < GNM601LimitP.Length; i++)
                {
                    if (value[i + j] == '1') GNM601LimitP[i].BackColor = Color.LimeGreen;
                    else GNM601LimitP[i].BackColor = Color.Silver;
                }

                j = uiChk_GNM3.Checked ? 0 : 10;
                motion.GNM601_GetDo(2, 12, out value);
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

                if (设备状态 != 1 && 当前权限 != "Operator")
                {
                    button146.Enabled = true;
                    button152.Enabled = true;
                    button2.Enabled = true;
                    button90.Enabled = true; //GlinkIO按钮
                    button91.Enabled = true; //GlinkIO按钮 
                    button92.Enabled = true; //GNM601IO按钮
                    button93.Enabled = true; //GNM601IO按钮
                    button156.Enabled = true;//回零按钮
                    button155.Enabled = true;//停止回零按钮

                    //2025-06-20 17:17
                    //button1.Enabled = true;//正向运动按钮
                    //button39.Enabled = true;//反向运动按钮
                    //button4.Enabled = true; //绝对运动按钮
                    //button145.Enabled = true;//停止运动按钮
                    //button15.Enabled = true; //Jog+
                    //button3.Enabled = true; //Jog-
                }
                else
                {
                    //2025-06-20 17:17
                    button146.Enabled = false;
                    button152.Enabled = false;
                    button2.Enabled = false;
                    button90.Enabled = false; //GlinkIO按钮
                    button91.Enabled = false; //GlinkIO按钮
                    button92.Enabled = false; //GNM601IO按钮
                    button93.Enabled = false; //GNM601IO按钮
                    button156.Enabled = false;//回零按钮
                    button155.Enabled = false;//停止回零按钮

                    button1.Enabled = false;//正向运动按钮
                    button39.Enabled = false;//反向运动按钮
                    button4.Enabled = false; //绝对运动按钮
                    button145.Enabled = false;//停止运动按钮
                    button15.Enabled = false; //Jog+
                    button3.Enabled = false; //Jog-
                }
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
            if (设备状态 == 1) return;
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
            if (设备状态 == 1) return;
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
            if (设备状态 == 1) return;
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

            //OutSiteQueue.TryPeek(out (int SiteId, TestDataInfo Data) site);
            //int siteid = site.SiteId;
            //int site = 6;
            //string result = "";
            //List<bool[]> open = new List<bool[]>();
            //List<List<bool[]>> opens = new List<List<bool[]>>();
            //if (设置信息.通讯信息.同时测试 == 2)
            //{
            //    result = "Success";
            //    int[] sites = GetSites(site);
            //    bool bool1 = SiteTestState[sites[0]] == 1;
            //    bool bool2 = !GetSiteOpen((int)sites[0]).Contains(true);
            //    bool bool3 = SiteTestState[sites[1]] == 1;
            //    bool bool4 = !GetSiteOpen((int)sites[1]).Contains(true);
            //    if ((bool1 || bool2) && (bool3 || bool4))
            //    {
            //        opens = new List<List<bool[]>>() { GetOpens(SiteTestIC[sites[0]]), GetOpens(SiteTestIC[sites[1]]) };
            //        //result = APPmaster.Com.Send(sites, opens, 无测试机模式);
            //        result = StationManagers.Send(sites, opens, 无测试机模式);
            //    }
            //}
            //else result = StationManagers.Send(site, open, 无测试机模式);
            ////else result = APPmaster.Com.Send(site, open, 无测试机模式);

            //if (result == "Success")
            //{
            //    SiteTestState[site] = 2;
            //    PS测试区气缸下压动作流程.StepID = 599;
            //}

            //int site = 0;
            //string[] bin = new string[8];


            //OutSiteQueue.Enqueue((0, new TestDataInfo() { SiteTestBin = new string[] { "01", "02", "20A", "20A", "20A", "20A", "20A", "20A" } }));
            //OutSiteQueue.Enqueue((1, new TestDataInfo() { SiteTestBin = new string[] { "03", "04", "20A", "20A", "20A", "20A", "20A", "20A" } }));
            //OutSiteQueue.Enqueue((2, new TestDataInfo() { SiteTestBin = new string[] { "05", "06", "20A", "20A", "20A", "20A", "20A", "20A" } }));
            //OutSiteQueue.Enqueue((3, new TestDataInfo() { SiteTestBin = new string[] { "07", "08", "20A", "20A", "20A", "20A", "20A", "20A" } }));
            //(int SiteId, TestDataInfo Data) sites;
            //if (OutSiteQueue.TryDequeue(out sites))
            //{
            //    site = sites.SiteId;
            //    bin = sites.Data.SiteTestBin;
            //    RunLog($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->获取Site{site + 1}信息");
            //    IndexTime = DateTime.Now;
            //    bool[][] open = SiteTestIC[site];
            //    int[] opens = new int[8]
            //    {
            //                    open[0][0] ? 0 : 2, open[0][1] ? 0 : 2, open[0][2] ? 0 : 2, open[0][3] ? 0 : 2,
            //                    open[1][0] ? 0 : 2, open[1][1] ? 0 : 2, open[1][2] ? 0 : 2, open[1][3] ? 0 : 2,
            //    };
            //    bool[] lsopen = new bool[] { open[0][0], open[0][1], open[0][2], open[0][3], open[1][0], open[1][1], open[1][2], open[1][3] };
            //    PS测试手臂取已测料流程.StepID = 505;
            //}
            //else
            //{
            //    RunLog($"PS测试手臂取已测料流程.StepID={PS测试手臂取已测料流程.StepID}->获取Site信息失败!");
            //    ProgramPause = true;
            //    IsPause = true;
            //}
            //string bins = string.Join(",", Auto1);
            //bins += "," + string.Join(",", Auto2);
            //if (Fix1.Length != 0)
            //    bins += "," + string.Join(",", Fix1);
            //if (Fix2.Length != 0)
            //    bins += "," + string.Join(",", Fix2);
            //if (Fix3.Length != 0)
            //    bins += "," + string.Join(",", Fix3);

            //string[] bins2 = bins.Split(',');

            //string[] bin = GetRandomBin(0, bins2.Length, bins2, 8);
            //string[] bin = { "20A", "20A", "20A", "20A", "20A", "20A", "20A", "20A" };

            //RefreshInfoLog($"@TEST_DONE {site:D2} {bin[0]} {bin[1]} {bin[2]} {bin[3]} {bin[4]} {bin[5]} {bin[6]} {bin[7]}+", true);
            //RunLog($"@TEST_DONE 00 {bin[0]} {bin[1]} {bin[2]} {bin[3]} {bin[4]} {bin[5]} {bin[6]} {bin[7]}+", LogType.ComLog, true);
            //string test = "@TEST_DONE A1A1F0F0F1F1C1C1F0F0F0F0F0F0F0F0+";

            //int[] site = { 0, 0 + 1 };

            //string bin = test.Substring(11, test.Length - 12);

            //// 2. 将长字符串按每2个字符一组分割
            //string[] allParts = new string[bin.Length / 2];
            //for (int i = 0; i < allParts.Length; i++)
            //{
            //    allParts[i] = bin.Substring(i * 2, 2);
            //}

            //string[][] bins = new string[2][];
            //// 前8个元素组成 bin1
            //bins[0] = allParts.Take(8).ToArray();
            //// 后8个元素组成 bin2
            //bins[1] = allParts.Skip(8).Take(8).ToArray();

            //for (int i = 0; i < site.Length; i++)
            //{
            //    TestDataInfo[] testDataInfos = new TestDataInfo[];
            //    if (testDataInfos[i].SiteTestState == 2 && (testDataInfos[i].Listtimer.Enabled || testDataInfos[i].timeOutTimer.Enabled)) //if (listsitetest[site].TestStatus == true && Iswaitout[site] == false && Isreceive[site] == false)
            //    {
            //        testDataInfos[i].Listtimer.Stop();
            //        testDataInfos[i].timeOutTimer.Stop();

            //        testDataInfos[i].SiteTestBin = bins[i];
            //        testDataInfos[i].listtesttimespan = DateTime.Now - testDataInfos[i].listtestdatetime;
            //        testDataInfos[i].TestTime = DateTime.Now - testDataInfos[i].listtestdatetime;

            //        testDataInfos[i].SiteTestState = 3;

            //        DataUpdate(i, testDataInfos[i]); //必须要传给StationManager,因为数据都是从StationManager取的,单个TestStation有可能负责16个site,也有可能只负责1个site，所以当查看station2时里面会无数据，因为TestStation1负责16个site的数据发送接受(1台测试机)
            //                                         ////用来统计数据

            //        RunLog($"接收Site:{site:D2},Bin:{bins[i][0]} {bins[i][1]} {bins[i][2]} {bins[i][3]} {bins[i][4]} {bins[i][5]} {bins[i][6]} {bins[i][7]}");
            //    }
            //    else RunLog($"site:{site:D2}未在测试!", LogType.ComLog);
            //}

            //bool[] check = StationManagers.CheckTesterConnect();
            //if (check.Contains(false))
            //{
            //    for (int i = 0; i < check.Length; i++)
            //    {
            //        if (!check[i])
            //        {
            //            Debug.WriteLine($"未发送开批信息给测试机{i + 1}");
            //        }
            //    }
            //}
            //温控达温 = true;
            //string point = textBox26.Text;
            //HeatDicPoint[point] = true;
            //HeatDicTimer[point].Status = 1;
            //bool bool1 = !飞梭后入料IC[0].Contains(true); //飞梭后入料IC第1列无料
            //RunLog("!飞梭后入料IC[0]:" + bool1);
            //bool1 = !飞梭后入料IC[1].Contains(true);//飞梭后入料IC第2列无料
            //RunLog("!飞梭后入料IC[1]:" + bool1);
            //bool1 = !飞梭前入料IC[0].Contains(true); //飞梭前入料IC第1列无料
            //RunLog("!飞梭前入料IC[0]:" + bool1);
            //bool1 = !飞梭前入料IC[1].Contains(true); //飞梭前入料IC第2列无料
            //RunLog("!飞梭前入料IC[1]:" + bool1);
            //bool1 = !飞梭后出料IC[0].Contains(true);//飞梭后出料IC第1列无料
            //RunLog("!飞梭后出料IC[0]:" + bool1);
            //bool1 = !飞梭后出料IC[1].Contains(true); //飞梭后出料IC第2列无料
            //RunLog("!飞梭后出料IC[1]:" + bool1);
            //bool1 = !飞梭前出料IC[0].Contains(true); //飞梭前出料IC第1列无料
            //RunLog("!飞梭前出料IC[0]:" + bool1);
            //bool1 = !飞梭前出料IC[1].Contains(true); //飞梭前出料IC第2列无料
            //RunLog("!飞梭前出料IC[1]:" + bool1);
            //bool1 = SiteTestState.Contains(0); //测试区无料
            //RunLog("SiteTestState.Contains(0):" + bool1);
            //bool1 = !SiteTestState.Contains(1); //测试区有料
            //RunLog("!SiteTestState.Contains(1):" + bool1);
            //bool1 = !SiteTestState.Contains(2); //测试区有料测试中
            //RunLog("!SiteTestState.Contains(2):" + bool1);
            //bool1 = !SiteTestState.Contains(3); //测试区有料已测试
            //RunLog("!SiteTestState.Contains(3):" + bool1);
            //bool1 = !SiteTestState.Contains(4); //测试区有料待取走
            //RunLog("!SiteTestState.Contains(4):" + bool1);
            //bool1 = !PS结批流程.Processing;
            //RunLog("!PS结批流程:" + bool1);
            //bool1 = !PS出料手臂吸放料流程.Processing;
            //RunLog("!PS出料手臂吸放料流程:" + bool1);
            //bool1 = !PS入料手臂吸放料流程.Processing;
            //RunLog("!PS入料手臂吸放料流程:" + bool1);
            //bool1 = !PS测试手臂取待测料流程.Processing;
            //RunLog("!PS测试手臂取待测料流程:" + bool1);
            //bool1 = !PS测试手臂取已测料流程.Processing;
            //RunLog("!PS测试手臂取已测料流程:" + bool1);
            //bool1 = !PS入飞梭移动流程.Processing;
            //RunLog("!PS入飞梭移动流程:" + bool1);
            //bool1 = !PS空盘上顶流程.Processing;
            //RunLog("!PS空盘上顶流程:" + bool1);
            //bool1 = !PS空盘下降流程.Processing;
            //RunLog("!PS空盘下降流程:" + bool1);
            //bool1 = !PS待测盘上顶流程.Processing;
            //RunLog("!PS待测盘上顶流程:" + bool1);
            //bool1 = !PS手动盘1上顶流程.Processing;
            //RunLog("!PS手动盘1上顶流程:" + bool1);
            //bool1 = !PS手动盘2上顶流程.Processing;
            //RunLog("!PS手动盘2上顶流程:" + bool1);
            //bool1 = !PS抓盘手臂取Empty空盘流程.Processing;
            //RunLog("!PS抓盘手臂取Empty空盘流程:" + bool1);
            //bool1 = !PS抓盘手臂取Load空盘流程.Processing;
            //RunLog("!PS抓盘手臂取Load空盘流程:" + bool1);
            //bool1 = !PS抓盘手臂放空盘到Empty流程.Processing;
            //RunLog("!PS抓盘手臂放空盘到Empty流程:" + bool1);
            //bool1 = !PS抓盘手臂放空盘到Auto1流程.Processing;
            //RunLog("!PS抓盘手臂放空盘到Auto1流程:" + bool1);
            //bool1 = !PS抓盘手臂放空盘到Auto2流程.Processing;
            //RunLog("!PS抓盘手臂放空盘到Auto2流程:" + bool1);
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
            if (设备状态 == 1) return;
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
            if (设备状态 == 1) return;
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
            if (设备状态 == 1) return;
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
            if (设备状态 == 1) return;
            AxisStop();
        }

        /// <summary>
        /// 回零
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button156_Click(object sender, EventArgs e)
        {
            if (设备状态 == 1) return;
            AxisHomeInfo axisHome = GetHomeValue();
            if (axisHome != null)
            {
                Task.Run(() =>
                {
                    string result = motionEx.AxisHome(axisHome, 1);
                    if (result != "Success")
                    {
                        MessageBox.Show("回零失败!\n详细信息:" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            if (设备状态 == 1) return;
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
            if (设备状态 == 1) return;
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
            if (设备状态 == 1) return;
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
                string result = motion.GNM601_SetDoBit(1, 12, (short)(i + j), value);
                if (result != "Success")
                {
                    MessageBox.Show("执行失败!详细信息:" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else if (uiChk_GNM3.Checked || uiChk_GNM4.Checked)
            {
                short i = (short)(uiChk_GNM3.Checked ? 0 : 10);
                short j = (short)(Convert.ToInt16(comboBox2.Text) + 1);
                string result = motion.GNM601_SetDoBit(2, 12, (short)(i + j), value);
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

        private string 入料Z移动(bool[] isopen, double zpos)
        {
            List<string> axisNames = new List<string>();
            if (isopen[0]) axisNames.Add("M1_入料手臂Z1轴");
            if (isopen[1]) axisNames.Add("M2_入料手臂Z2轴");
            if (isopen[2]) axisNames.Add("M3_入料手臂Z3轴");
            if (isopen[3]) axisNames.Add("M4_入料手臂Z4轴");

            return motionEx.AxisMove(axisNames.ToArray(), new double[] { zpos, zpos, zpos, zpos });

        }

        #region 入料吸料

        private void button113_Click(object sender, EventArgs e)
        {
            if (double.TryParse(comboBox8.Text, out double pos))
            {
                string result = motionEx.AxisAbsoluteMove("M22_入料手臂X轴", pos);
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
                string result = motionEx.AxisAbsoluteMove("M16_入料手臂Y轴", pos);
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
                string result = motionEx.AxisAbsoluteMove("M5_入料手臂P轴", pos);
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

            string result = motionEx.AxisMove(axisNames, axisPoss);
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
                string result = motionEx.AxisAbsoluteMove("M22_入料手臂X轴", pos);
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
                string result = motionEx.AxisAbsoluteMove("M16_入料手臂Y轴", pos);
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
                string result = motionEx.AxisAbsoluteMove("M5_入料手臂P轴", pos);
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

            string result = motionEx.AxisMove(axisNames, axisPoss);
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
                string result = motionEx.AxisHome("M22_入料手臂X轴", 1);
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
                string result = motionEx.AxisHome("M16_入料手臂Y轴");
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
                string result = motionEx.AxisHome("M5_入料手臂P轴");
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
                string result = motionEx.AxisHome(axisNames);
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
                string result = motionEx.AxisHome("M4_入料手臂Z4轴");
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
                string result = motionEx.AxisHome("M3_入料手臂Z3轴");
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
                string result = motionEx.AxisHome("M2_入料手臂Z2轴");
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
                string result = motionEx.AxisHome("M1_入料手臂Z1轴");
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
                string result = motionEx.AxisHome(axisNames);
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

            result = 读取气缸输入信号(xIoName, "0", out long ms);
            if (result != "Success") RunLog($"{yIoName}原位输出超时({ms}ms)");
            //else RunLog($"{yIoName}原位输出成功({ms}ms)");
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
            //else RunLog($"{yIoName}到位输出成功({ms}ms)");
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

            if (result != "Success") RunLog($"{yIoName}原位输出超时({ms}ms)");
            //else RunLog($"{yIoName}原位输出成功({ms}ms)");
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

        #region 调试

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
            Auto等待换盘标记 = true;
            抓盘手臂放空盘到Auto1标记 = 2;
            //Task.Run(() => { handler.抓盘手臂放空盘到Auto1(); });
        }

        private void button218_Click(object sender, EventArgs e)
        {
            Auto2上顶标记 = 1;
            Auto等待换盘标记 = true;
            抓盘手臂放空盘到Auto2标记 = 2;
            //Task.Run(() => { handler.抓盘手臂放空盘到Auto2(); });
        }

        private void button219_Click(object sender, EventArgs e)
        {
            Auto1上顶标记 = 2;
            Auto等待换盘标记 = true;
            抓盘手臂放空盘到Auto1标记 = 2;
            //Task.Run(() => { handler.抓盘手臂换Auto1空盘(); });
        }

        private void button220_Click(object sender, EventArgs e)
        {
            Auto2上顶标记 = 2;
            Auto等待换盘标记 = true;
            抓盘手臂放空盘到Auto2标记 = 2;
            //Task.Run(() => { handler.抓盘手臂换Auto2空盘(); });
        }

        #endregion


        private void uiTabControl2_Selecting(object sender, TabControlCancelEventArgs e)
        {
            刷新初始点位(e.TabPage.Text);
        }

        private void 刷新设置与维护界面(string tabpagetext)
        {
            if (tabpagetext == "产品")
            {
                刷新产品(uiTabControl5.SelectedTab.Text);
            }
            else if (tabpagetext == "系统设置")
            {
                uiCheckBox205.Checked = 安全门禁用;



                if (空跑模式 == false && Tray盘模式 && 无测试机模式 == false)
                {
                    uiCheckBox190.Checked = true;
                    uiCheckBox204.Checked = false;
                    uiCheckBox187.Checked = false;
                    uiCheckBox188.Checked = false;
                    uiCheckBox206.Checked = false;
                }
                else if (空跑模式 == false && Tray盘模式)
                {
                    uiCheckBox204.Checked = true;
                    uiCheckBox187.Checked = false;
                    uiCheckBox188.Checked = false;
                }
                else if (空跑模式 && Tray盘模式)
                {
                    uiCheckBox204.Checked = false;
                    uiCheckBox187.Checked = true;
                    uiCheckBox188.Checked = false;
                }
                else if (空跑模式 && Tray盘模式 == false)
                {
                    uiCheckBox204.Checked = false;
                    uiCheckBox187.Checked = false;
                    uiCheckBox188.Checked = true;
                }

                //uiCheckBox203.Checked = 空跑模式;
                //if (!空跑模式 || 设备状态 != 0)
                //{
                //    uiCheckBox204.Enabled = false;
                //}

                uiCheckBox206.Checked = 无测试机模式;

                刷新灯光状态();
                uiCheckBox181.Checked = 按了跳过或编辑报警使能;
                numericUpDown196.Text = Load门开门时间.ToString();
                uiCheckBox182.Checked = 按批号保存使能;
                richTextBox1.Text = LotNoLog保存路径;

            }
            else if (tabpagetext == "调试")
            {
                刷新调试(uiTabControl3.SelectedTab.Text);
            }
            else if (tabpagetext == "权限")
            {
                刷新按钮权限();
            }
            else if (tabpagetext == "自定义界面")
            {
                uiRadioButton1.Checked = 自定义界面 == "1";
                uiRadioButton2.Checked = 自定义界面 == "2";
                uiRadioButton3.Checked = 自定义界面 == "3";
            }
            //else if (tabpagetext == "产品")
            //{
            //    刷新产品(uiTabControl5.SelectedTab.Text);
            //}
        }

        private void uiTabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            刷新设置与维护界面(e.TabPage.Text);
        }

        public void 刷新设置与维护界面()
        {
            刷新设置与维护界面(uiTabControl1.SelectedTab.Text);
        }

        private void 刷新调试(string tabpagetext)
        {
            if (tabpagetext == "初始点位")
            {
                刷新初始点位(uiTabControl2.SelectedTab.Text);
            }
            else if (tabpagetext == "基础调试")
            {
                cobox_axisname_SelectedIndexChanged(null, null);
            }
        }

        private void 刷新按钮权限()
        {
            for (int i = 0; i < ListConPerm.Count; i++)
            {
                if (ListConPerm[i].控件名 == "button_setting")
                {
                    uiComboBox3.Text = ListConPerm[i].权限;
                }
                else if (ListConPerm[i].控件名 == "button_offset")
                {
                    uiComboBox4.Text = ListConPerm[i].权限;
                }
                else if (ListConPerm[i].控件名 == "button_speed")
                {
                    uiComboBox5.Text = ListConPerm[i].权限;
                }
                else if (ListConPerm[i].控件名 == "button_io")
                {
                    uiComboBox6.Text = ListConPerm[i].权限;
                }
                else if (ListConPerm[i].控件名 == "button_log")
                {
                    uiComboBox18.Text = ListConPerm[i].权限;
                }
                else if (ListConPerm[i].控件名 == "button_exit")
                {
                    uiComboBox7.Text = ListConPerm[i].权限;
                }
            }
        }

        public bool UpdateInitPointData(string pointname, double newvalue, double oldvalue)
        {
            if (newvalue != oldvalue)
            {
                int line = Sqlite.runSqlNonQuery(SQLiteConnString, $"update InitPoint set value = '{newvalue}' where ptname = '{pointname}'");
                if (line >= 1)
                {
                    RunLog($"初始点位:[{pointname}]{oldvalue}->{newvalue}:更新成功", LogType.ParamLog);
                    return true;
                }
                else RunLog($"初始点位:[{pointname}]{oldvalue}->{newvalue}:更新失败", LogType.ParamLog);
            }
            return false;
        }

        public bool UpdateRunInfoData(string pointname, string valuename, double newvalue, double oldvalue)
        {
            if (newvalue != oldvalue)
            {
                int line = Sqlite.runSqlNonQuery(SQLiteConnString, $"update AxisRunInfo set {valuename} = '{newvalue}' where 轴名称 = '{pointname}'");
                if (line >= 1)
                {
                    RunLog($"运行参数:[{pointname}:{valuename}]{oldvalue}->{newvalue}:更新成功", LogType.ParamLog);
                    return true;
                }
                else RunLog($"运行参数:[{pointname}:{valuename}]{oldvalue}->{newvalue}:更新失败", LogType.ParamLog);
            }
            return false;
        }

        #region 初始点位

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

        private void 刷新顶盘手臂初始点位()
        {
            numericUpDown163.Text = 初始点位.Empty_最高上顶.ToString();
            numericUpDown162.Text = 初始点位.Empty_满盘位.ToString();
            numericUpDown161.Text = 初始点位.Load_最高上顶.ToString();

            numericUpDown174.Text = 初始点位.Auto1_最高上顶.ToString();
            numericUpDown179.Text = 初始点位.Auto1_分盘位.ToString();
            numericUpDown159.Text = 初始点位.Auto1_初始位.ToString();
            numericUpDown173.Text = 初始点位.Auto1_满盘位.ToString();

            numericUpDown175.Text = 初始点位.Auto2_最高上顶.ToString();
            numericUpDown178.Text = 初始点位.Auto2_分盘位.ToString();
            numericUpDown177.Text = 初始点位.Auto2_初始位.ToString();
            numericUpDown176.Text = 初始点位.Auto2_满盘位.ToString();

            numericUpDown181.Text = 初始点位.Auto1上顶固定高度.ToString();
            numericUpDown182.Text = 初始点位.Auto1下降固定高度.ToString();
            numericUpDown180.Text = 初始点位.Auto2上顶固定高度.ToString();
            numericUpDown183.Text = 初始点位.Auto2下降固定高度.ToString();
        }

        private void 刷新顶盘手臂速度参数()
        {
            GetAxisRunInfo("M12_Empty顶盘轴", out AxisRunInfo axisRun);
            numericUpDown3.Text = axisRun.最大速度.ToString();
            numericUpDown4.Text = axisRun.加速度.ToString();
            numericUpDown2.Text = axisRun.减速度.ToString();

            GetAxisRunInfo("M13_Loader顶盘轴", out axisRun);
            numericUpDown165.Text = axisRun.最大速度.ToString();
            numericUpDown166.Text = axisRun.加速度.ToString();
            numericUpDown164.Text = axisRun.减速度.ToString();

            GetAxisRunInfo("M14_Auto1顶盘轴", out axisRun);
            numericUpDown171.Text = axisRun.最大速度.ToString();
            numericUpDown172.Text = axisRun.加速度.ToString();
            numericUpDown170.Text = axisRun.减速度.ToString();

            GetAxisRunInfo("M15_Auto2顶盘轴", out axisRun);
            numericUpDown168.Text = axisRun.最大速度.ToString();
            numericUpDown169.Text = axisRun.加速度.ToString();
            numericUpDown167.Text = axisRun.减速度.ToString();
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
            else if (tabpagetext == "顶盘手臂")
            {
                刷新顶盘手臂初始点位();
                刷新顶盘手臂速度参数();
            }
        }

        private void 入料手臂X移动初始点位(double mm)
        {
            button29.Enabled = false;
            button224.Enabled = false;
            button32.Enabled = false;
            button20.Enabled = false;
            button268.Enabled = false;
            Task.Run(() =>
            {
                string result = motionEx.AxisAbsoluteMove("M22_入料手臂X轴", mm, 1);
                //string result = 单轴绝对运动("M22_入料手臂X轴", mm);
                if (result != "Success") RunLog("移动失败!" + result);

                this.Invoke(new Action(() =>
                {
                    button29.Enabled = true;
                    button224.Enabled = true;
                    button32.Enabled = true;
                    button20.Enabled = true;
                    button268.Enabled = true;
                }));
            });
        }

        private void 入料手臂Y移动初始点位(double mm)
        {
            button28.Enabled = false;
            button223.Enabled = false;
            button31.Enabled = false;
            button19.Enabled = false;
            button269.Enabled = false;
            Task.Run(() =>
            {
                string result = motionEx.AxisAbsoluteMove("M16_入料手臂Y轴", mm, 1);
                if (result != "Success") RunLog("移动失败!" + result);

                this.Invoke(new Action(() =>
                {
                    button28.Enabled = true;
                    button223.Enabled = true;
                    button31.Enabled = true;
                    button19.Enabled = true;
                    button269.Enabled = true;
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
            button270.Enabled = false;
            Task.Run(() =>
            {
                double z1 = Convert.ToDouble(uiDoubleUpDown18.Text);
                double z2 = Convert.ToDouble(uiDoubleUpDown17.Text);
                double z3 = Convert.ToDouble(uiDoubleUpDown16.Text);
                double z4 = Convert.ToDouble(uiDoubleUpDown19.Text);
                string[] axis = { "M1_入料手臂Z1轴", "M2_入料手臂Z2轴", "M3_入料手臂Z3轴", "M4_入料手臂Z4轴" };
                double[] pos = { mm + z1, mm + z2, mm + z3, mm + z4 };
                string result = motionEx.AxisMove(axis, pos, 1, inoldpos < mm ? 1 : 0, true);//, 入料Z第二段距离
                //string result = 多轴绝对运动(axis, pos, 1, inoldpos < mm ? 1 : 0, 出入料Z第二段距离);
                if (result != "Success") RunLog("移动失败!" + result);
                inoldpos = mm;
                this.Invoke(new Action(() =>
                {
                    button30.Enabled = true;
                    button18.Enabled = true;
                    button27.Enabled = true;
                    button222.Enabled = true;
                    button226.Enabled = true;
                    button270.Enabled = true;
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
                string result = motionEx.AxisAbsoluteMove("M5_入料手臂P轴", mm, 1);
                if (result != "Success") RunLog("移动失败!" + result);

                this.Invoke(new Action(() =>
                {
                    button240.Enabled = true;
                    button241.Enabled = true;
                    button242.Enabled = true;
                }));
            });
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
            button271.Enabled = false;
            Task.Run(() =>
            {
                string result = motionEx.AxisAbsoluteMove("M23_出料手臂X轴", mm, 1);
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
                    button271.Enabled = true;
                }));
            });
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
            button272.Enabled = false;
            Task.Run(() =>
            {
                string result = motionEx.AxisAbsoluteMove("M17_出料手臂Y轴", mm, 1);
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
                    button272.Enabled = true;
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
            button273.Enabled = false;
            Task.Run(() =>
            {
                double z1 = Convert.ToDouble(uiDoubleUpDown122.Text);
                double z2 = Convert.ToDouble(uiDoubleUpDown121.Text);
                double z3 = Convert.ToDouble(uiDoubleUpDown21.Text);
                double z4 = Convert.ToDouble(uiDoubleUpDown20.Text);
                string[] axis = { "M6_出料手臂Z1轴", "M7_出料手臂Z2轴", "M8_出料手臂Z3轴", "M9_出料手臂Z4轴" };
                double[] pos = { mm + z1, mm + z2, mm + z3, mm + z4 };

                string result = motionEx.AxisMove(axis, pos, 1, outoldpos < mm ? 1 : 0, true);
                //string result = 多轴绝对运动(axis, pos, 1, outoldpos < mm ? 1 : 0, 出入料Z第二段距离);
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
                    button273.Enabled = true;
                }));
            });
        }

        private void 出料手臂变距轴移动初始点位(double mm)
        {
            button245.Enabled = false;
            button244.Enabled = false;
            Task.Run(() =>
            {
                string result = motionEx.AxisAbsoluteMove("M10_出料手臂P轴", mm, 1);
                if (result != "Success") RunLog("移动失败!" + result);

                this.Invoke(new Action(() =>
                {
                    button245.Enabled = true;
                    button244.Enabled = true;
                }));
            });
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
                    button275.Enabled = false;
                    Task.Run(() =>
                    {
                        result = motionEx.AxisAbsoluteMove("M18_放Tary盘轴", mm, 1);
                        if (result != "Success") RunLog("移动失败!" + result);
                        this.Invoke(new Action(() =>
                        {
                            button76.Enabled = true;
                            button75.Enabled = true;
                            button74.Enabled = true;
                            button73.Enabled = true;
                            button275.Enabled = true;
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
                string result = motionEx.AxisAbsoluteMove("M21_测试手臂X轴", mm, 1);
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
            button274.Enabled = false;
            Task.Run(() =>
            {
                string result = motionEx.AxisAbsoluteMove("M19_测试手臂Z轴", mm, 1);
                if (result != "Success") RunLog("移动失败!" + result);

                this.Invoke(new Action(() =>
                {
                    button60.Enabled = true;
                    button68.Enabled = true;
                    button61.Enabled = true;
                    button64.Enabled = true;
                    button47.Enabled = true;
                    button41.Enabled = true;
                    button274.Enabled = true;
                }));
            });
        }

        private void Empty顶盘手臂移动初始点位(double mm)
        {
            button248.Enabled = false;
            button247.Enabled = false;
            Task.Run(() =>
            {
                string result = motionEx.AxisAbsoluteMove("M12_Empty顶盘轴", mm, 1);
                if (result != "Success") RunLog("移动失败!" + result);

                this.Invoke(new Action(() =>
                {
                    button248.Enabled = true;
                    button247.Enabled = true;
                }));
            });
        }

        private void Load顶盘手臂移动初始点位(double mm)
        {
            button246.Enabled = false;
            Task.Run(() =>
            {
                string result = motionEx.AxisAbsoluteMove("M13_Loader顶盘轴", mm, 1);
                if (result != "Success") RunLog("移动失败!" + result);

                this.Invoke(new Action(() =>
                {
                    button246.Enabled = true;
                }));
            });
        }

        private void Auto1顶盘手臂移动初始点位(double mm)
        {
            button251.Enabled = false;
            button256.Enabled = false;
            button249.Enabled = false;
            button250.Enabled = false;
            Task.Run(() =>
            {
                string result = motionEx.AxisAbsoluteMove("M14_Auto1顶盘轴", mm, 1);
                if (result != "Success") RunLog("移动失败!" + result);

                this.Invoke(new Action(() =>
                {
                    button251.Enabled = true;
                    button256.Enabled = true;
                    button249.Enabled = true;
                    button250.Enabled = true;
                }));
            });
        }

        private void Auto2顶盘手臂移动初始点位(double mm)
        {
            button252.Enabled = false;
            button255.Enabled = false;
            button254.Enabled = false;
            button253.Enabled = false;
            Task.Run(() =>
            {
                string result = motionEx.AxisAbsoluteMove("M15_Auto2顶盘轴", mm, 1);
                if (result != "Success") RunLog("移动失败!" + result);

                this.Invoke(new Action(() =>
                {
                    button252.Enabled = true;
                    button255.Enabled = true;
                    button254.Enabled = true;
                    button253.Enabled = true;
                }));
            });
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
                if (UpdateRunInfoData("M2_入料手臂Z2轴", "最大速度", Convert.ToDouble(uiDoubleUpDown15.Text), axisRun.最大速度)) axisRun.最大速度 = Convert.ToDouble(uiDoubleUpDown15.Value); //uiDoubleUpDown81.Value
                if (UpdateRunInfoData("M2_入料手臂Z2轴", "加速度", Convert.ToDouble(uiDoubleUpDown14.Text), axisRun.加速度)) axisRun.加速度 = Convert.ToDouble(uiDoubleUpDown14.Value);//uiDoubleUpDown80.Value
                if (UpdateRunInfoData("M2_入料手臂Z2轴", "减速度", Convert.ToDouble(uiDoubleUpDown13.Text), axisRun.减速度)) axisRun.减速度 = Convert.ToDouble(uiDoubleUpDown13.Value);//uiDoubleUpDown79.Value


                GetAxisRunInfo("M3_入料手臂Z3轴", out axisRun);
                if (UpdateRunInfoData("M3_入料手臂Z3轴", "最大速度", Convert.ToDouble(uiDoubleUpDown15.Text), axisRun.最大速度)) axisRun.最大速度 = Convert.ToDouble(uiDoubleUpDown15.Value); //uiDoubleUpDown6.Value
                if (UpdateRunInfoData("M3_入料手臂Z3轴", "加速度", Convert.ToDouble(uiDoubleUpDown14.Text), axisRun.加速度)) axisRun.加速度 = Convert.ToDouble(uiDoubleUpDown14.Value);//uiDoubleUpDown5.Value
                if (UpdateRunInfoData("M3_入料手臂Z3轴", "减速度", Convert.ToDouble(uiDoubleUpDown13.Text), axisRun.减速度)) axisRun.减速度 = Convert.ToDouble(uiDoubleUpDown13.Value);//uiDoubleUpDown4.Value


                GetAxisRunInfo("M4_入料手臂Z4轴", out axisRun);
                if (UpdateRunInfoData("M4_入料手臂Z4轴", "最大速度", Convert.ToDouble(uiDoubleUpDown15.Text), axisRun.最大速度)) axisRun.最大速度 = Convert.ToDouble(uiDoubleUpDown15.Value); //uiDoubleUpDown3.Value
                if (UpdateRunInfoData("M4_入料手臂Z4轴", "加速度", Convert.ToDouble(uiDoubleUpDown14.Text), axisRun.加速度)) axisRun.加速度 = Convert.ToDouble(uiDoubleUpDown14.Value);//uiDoubleUpDown2.Value
                if (UpdateRunInfoData("M4_入料手臂Z4轴", "减速度", Convert.ToDouble(uiDoubleUpDown13.Text), axisRun.减速度)) axisRun.减速度 = Convert.ToDouble(uiDoubleUpDown13.Value);//uiDoubleUpDown1.Value
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
                if (UpdateRunInfoData("M7_出料手臂Z2轴", "最大速度", Convert.ToDouble(uiDoubleUpDown138.Text), axisRun.最大速度)) axisRun.最大速度 = Convert.ToDouble(uiDoubleUpDown138.Text);//uiDoubleUpDown141.Text
                if (UpdateRunInfoData("M7_出料手臂Z2轴", "加速度", Convert.ToDouble(uiDoubleUpDown137.Text), axisRun.加速度)) axisRun.加速度 = Convert.ToDouble(uiDoubleUpDown137.Text);//uiDoubleUpDown140.Text
                if (UpdateRunInfoData("M7_出料手臂Z2轴", "减速度", Convert.ToDouble(uiDoubleUpDown136.Text), axisRun.减速度)) axisRun.减速度 = Convert.ToDouble(uiDoubleUpDown136.Text);//uiDoubleUpDown139.Text


                GetAxisRunInfo("M8_出料手臂Z3轴", out axisRun);
                if (UpdateRunInfoData("M8_出料手臂Z3轴", "最大速度", Convert.ToDouble(uiDoubleUpDown138.Text), axisRun.最大速度)) axisRun.最大速度 = Convert.ToDouble(uiDoubleUpDown138.Text);//uiDoubleUpDown129.Text
                if (UpdateRunInfoData("M8_出料手臂Z3轴", "加速度", Convert.ToDouble(uiDoubleUpDown137.Text), axisRun.加速度)) axisRun.加速度 = Convert.ToDouble(uiDoubleUpDown137.Text);//uiDoubleUpDown128.Text
                if (UpdateRunInfoData("M8_出料手臂Z3轴", "减速度", Convert.ToDouble(uiDoubleUpDown136.Text), axisRun.减速度)) axisRun.减速度 = Convert.ToDouble(uiDoubleUpDown136.Text);//uiDoubleUpDown127.Text


                GetAxisRunInfo("M9_出料手臂Z4轴", out axisRun);
                if (UpdateRunInfoData("M9_出料手臂Z4轴", "最大速度", Convert.ToDouble(uiDoubleUpDown138.Text), axisRun.最大速度)) axisRun.最大速度 = Convert.ToDouble(uiDoubleUpDown138.Text);//uiDoubleUpDown120.Text
                if (UpdateRunInfoData("M9_出料手臂Z4轴", "加速度", Convert.ToDouble(uiDoubleUpDown137.Text), axisRun.加速度)) axisRun.加速度 = Convert.ToDouble(uiDoubleUpDown137.Text);//uiDoubleUpDown119.Text
                if (UpdateRunInfoData("M9_出料手臂Z4轴", "减速度", Convert.ToDouble(uiDoubleUpDown136.Text), axisRun.减速度)) axisRun.减速度 = Convert.ToDouble(uiDoubleUpDown136.Text);//uiDoubleUpDown118.Text
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

                if (UpdateInitPointData("测试_Test1区_X", Convert.ToDouble(uiDoubleUpDown66.Text), 初始点位.测试_Test1区_X))
                {
                    初始点位.测试_Test1区_X = Convert.ToDouble(uiDoubleUpDown66.Text);
                    TestPointInit();
                }
                if (UpdateInitPointData("测试_Test2区_X", Convert.ToDouble(uiDoubleUpDown65.Text), 初始点位.测试_Test2区_X))
                {
                    初始点位.测试_Test2区_X = Convert.ToDouble(uiDoubleUpDown65.Text);
                    TestPointInit();
                }
                if (UpdateInitPointData("测试_Test3区_X", Convert.ToDouble(uiDoubleUpDown68.Text), 初始点位.测试_Test3区_X))
                {
                    初始点位.测试_Test3区_X = Convert.ToDouble(uiDoubleUpDown68.Text);
                    TestPointInit();
                }
                if (UpdateInitPointData("测试_Test4区_X", Convert.ToDouble(uiDoubleUpDown67.Text), 初始点位.测试_Test4区_X))
                {
                    初始点位.测试_Test4区_X = Convert.ToDouble(uiDoubleUpDown67.Text);
                    TestPointInit();
                }

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
        private void uiButton23_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("请确认是否保存!", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                if (UpdateInitPointData("Empty_最高上顶", Convert.ToDouble(numericUpDown163.Text), 初始点位.Empty_最高上顶)) 初始点位.Empty_最高上顶 = Convert.ToDouble(numericUpDown163.Text);
                if (UpdateInitPointData("Empty_满盘位", Convert.ToDouble(numericUpDown162.Text), 初始点位.Empty_满盘位)) 初始点位.Empty_满盘位 = Convert.ToDouble(numericUpDown162.Text);
                if (UpdateInitPointData("Load_最高上顶", Convert.ToDouble(numericUpDown161.Text), 初始点位.Load_最高上顶)) 初始点位.Load_最高上顶 = Convert.ToDouble(numericUpDown161.Text);

                if (UpdateInitPointData("Auto1_最高上顶", Convert.ToDouble(numericUpDown174.Text), 初始点位.Auto1_最高上顶)) 初始点位.Auto1_最高上顶 = Convert.ToDouble(numericUpDown174.Text);
                if (UpdateInitPointData("Auto1_分盘位", Convert.ToDouble(numericUpDown179.Text), 初始点位.Auto1_分盘位)) 初始点位.Auto1_分盘位 = Convert.ToDouble(numericUpDown179.Text);
                if (UpdateInitPointData("Auto1_初始位", Convert.ToDouble(numericUpDown159.Text), 初始点位.Auto1_初始位)) 初始点位.Auto1_初始位 = Convert.ToDouble(numericUpDown159.Text);
                if (UpdateInitPointData("Auto1_满盘位", Convert.ToDouble(numericUpDown173.Text), 初始点位.Auto1_满盘位)) 初始点位.Auto1_满盘位 = Convert.ToDouble(numericUpDown173.Text);

                if (UpdateInitPointData("Auto2_最高上顶", Convert.ToDouble(numericUpDown175.Text), 初始点位.Auto2_最高上顶)) 初始点位.Auto2_最高上顶 = Convert.ToDouble(numericUpDown175.Text);
                if (UpdateInitPointData("Auto2_分盘位", Convert.ToDouble(numericUpDown178.Text), 初始点位.Auto2_分盘位)) 初始点位.Auto2_分盘位 = Convert.ToDouble(numericUpDown178.Text);
                if (UpdateInitPointData("Auto2_初始位", Convert.ToDouble(numericUpDown177.Text), 初始点位.Auto2_初始位)) 初始点位.Auto2_初始位 = Convert.ToDouble(numericUpDown177.Text);
                if (UpdateInitPointData("Auto2_满盘位", Convert.ToDouble(numericUpDown176.Text), 初始点位.Auto2_满盘位)) 初始点位.Auto2_满盘位 = Convert.ToDouble(numericUpDown176.Text);

                if (UpdateInitPointData("Auto1上顶固定高度", Convert.ToDouble(numericUpDown181.Text), 初始点位.Auto1上顶固定高度)) 初始点位.Auto1上顶固定高度 = Convert.ToDouble(numericUpDown181.Text);
                if (UpdateInitPointData("Auto1下降固定高度", Convert.ToDouble(numericUpDown182.Text), 初始点位.Auto1下降固定高度)) 初始点位.Auto1下降固定高度 = Convert.ToDouble(numericUpDown182.Text);
                if (UpdateInitPointData("Auto2上顶固定高度", Convert.ToDouble(numericUpDown180.Text), 初始点位.Auto2上顶固定高度)) 初始点位.Auto2上顶固定高度 = Convert.ToDouble(numericUpDown180.Text);
                if (UpdateInitPointData("Auto2下降固定高度", Convert.ToDouble(numericUpDown183.Text), 初始点位.Auto2下降固定高度)) 初始点位.Auto2下降固定高度 = Convert.ToDouble(numericUpDown183.Text);


                GetAxisRunInfo("M12_Empty顶盘轴", out AxisRunInfo axisRun);
                if (UpdateRunInfoData("M12_Empty顶盘轴", "最大速度", Convert.ToDouble(numericUpDown3.Text), axisRun.最大速度)) axisRun.最大速度 = Convert.ToDouble(numericUpDown3.Text);
                if (UpdateRunInfoData("M12_Empty顶盘轴", "加速度", Convert.ToDouble(numericUpDown4.Text), axisRun.加速度)) axisRun.加速度 = Convert.ToDouble(numericUpDown4.Text);
                if (UpdateRunInfoData("M12_Empty顶盘轴", "减速度", Convert.ToDouble(numericUpDown2.Text), axisRun.减速度)) axisRun.减速度 = Convert.ToDouble(numericUpDown2.Text);

                GetAxisRunInfo("M13_Loader顶盘轴", out axisRun);
                if (UpdateRunInfoData("M13_Loader顶盘轴", "最大速度", Convert.ToDouble(numericUpDown165.Text), axisRun.最大速度)) axisRun.最大速度 = Convert.ToDouble(numericUpDown165.Text);
                if (UpdateRunInfoData("M13_Loader顶盘轴", "加速度", Convert.ToDouble(numericUpDown166.Text), axisRun.加速度)) axisRun.加速度 = Convert.ToDouble(numericUpDown166.Text);
                if (UpdateRunInfoData("M13_Loader顶盘轴", "减速度", Convert.ToDouble(numericUpDown164.Text), axisRun.减速度)) axisRun.减速度 = Convert.ToDouble(numericUpDown164.Text);

                GetAxisRunInfo("M14_Auto1顶盘轴", out axisRun);
                if (UpdateRunInfoData("M14_Auto1顶盘轴", "最大速度", Convert.ToDouble(numericUpDown171.Text), axisRun.最大速度)) axisRun.最大速度 = Convert.ToDouble(numericUpDown171.Text);
                if (UpdateRunInfoData("M14_Auto1顶盘轴", "加速度", Convert.ToDouble(numericUpDown172.Text), axisRun.加速度)) axisRun.加速度 = Convert.ToDouble(numericUpDown172.Text);
                if (UpdateRunInfoData("M14_Auto1顶盘轴", "减速度", Convert.ToDouble(numericUpDown170.Text), axisRun.减速度)) axisRun.减速度 = Convert.ToDouble(numericUpDown170.Text);

                GetAxisRunInfo("M15_Auto2顶盘轴", out axisRun);
                if (UpdateRunInfoData("M15_Auto2顶盘轴", "最大速度", Convert.ToDouble(numericUpDown168.Text), axisRun.最大速度)) axisRun.最大速度 = Convert.ToDouble(numericUpDown168.Text);
                if (UpdateRunInfoData("M15_Auto2顶盘轴", "加速度", Convert.ToDouble(numericUpDown169.Text), axisRun.加速度)) axisRun.加速度 = Convert.ToDouble(numericUpDown169.Text);
                if (UpdateRunInfoData("M15_Auto2顶盘轴", "减速度", Convert.ToDouble(numericUpDown167.Text), axisRun.减速度)) axisRun.减速度 = Convert.ToDouble(numericUpDown167.Text);
            }
        }

        private void uiButton21_Click(object sender, EventArgs e)
        {
            刷新抓盘手臂初始点位();
            刷新抓盘手臂速度参数();
        }

        private void uiButton20_Click(object sender, EventArgs e)
        {
            刷新顶盘手臂初始点位();
            刷新顶盘手臂速度参数();
        }

        private void button72_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown57.Text);
            button71.Enabled = false;
            button72.Enabled = false;
            button276.Enabled = false;
            Task.Run(() =>
            {
                string result = motionEx.AxisAbsoluteMove("M20_飞梭轴", pos, 1);//梭车第二段距离
                if (result != "Success") RunLog("移动失败!" + result);
                this.Invoke(new Action(() =>
                {
                    button71.Enabled = true;
                    button72.Enabled = true;
                    button276.Enabled = true;
                }));
            });
        }

        private void button71_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown56.Text);
            button71.Enabled = false;
            button72.Enabled = false;
            button276.Enabled = false;
            Task.Run(() =>
            {
                string result = motionEx.AxisAbsoluteMove("M20_飞梭轴", pos, 1);//, 梭车第二段距离
                if (result != "Success") RunLog("移动失败!" + result);
                this.Invoke(new Action(() =>
                {
                    button71.Enabled = true;
                    button72.Enabled = true;
                    button276.Enabled = true;
                }));
            });
        }

        private void button276_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(numericUpDown195.Text);
            button71.Enabled = false;
            button72.Enabled = false;
            button276.Enabled = false;
            Task.Run(() =>
            {
                string result = motionEx.AxisAbsoluteMove("M20_飞梭轴", pos, 1);//, 梭车第二段距离
                if (result != "Success") RunLog("移动失败!" + result);
                this.Invoke(new Action(() =>
                {
                    button71.Enabled = true;
                    button72.Enabled = true;
                    button276.Enabled = true;
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

        private void button268_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(numericUpDown187.Text);
            入料手臂X移动初始点位(pos);
        }

        private void button269_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(numericUpDown189.Text);
            入料手臂Y移动初始点位(pos);
        }

        private void button270_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(numericUpDown188.Text);
            入料手臂Z移动初始点位(pos);
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
        private void button275_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(numericUpDown194.Text);
            抓盘手臂X移动初始点位(pos);
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

        private void button271_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(numericUpDown190.Text);
            出料手臂X移动初始点位(pos);
        }

        private void button272_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(numericUpDown192.Text);
            出料手臂Y移动初始点位(pos);
        }

        private void button273_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(numericUpDown191.Text);
            出料手臂Z移动初始点位(pos);
        }

        private void button267_Click(object sender, EventArgs e)
        {
            TCPConnect = false;
            温控器异常 = false;
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

        private void button274_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(numericUpDown193.Text);
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

        private void button147_Click(object sender, EventArgs e)
        {
            string[] axisNames = { "M6_出料手臂Z1轴", "M7_出料手臂Z2轴", "M8_出料手臂Z3轴", "M9_出料手臂Z4轴" };
            Task.Run(() =>
            {
                string result = motionEx.AxisHome(axisNames, 1);
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
                string result = motionEx.AxisHome(axisNames);
                if (result != "Success")
                {
                    MessageBox.Show("出料手臂XYP回零失败!");
                }
                else MessageBox.Show("出料手臂XYP回零完成");
            });
        }

        #endregion


        #region 测试机

        private void 刷新设定(string name)
        {
            string result = Sqlite.runSql(SQLiteConnString, $"select name from TrayInfo", out DataTable traydt);
            if (result != "Success") return;
            uiComboBox1.DataSource = traydt;
            uiComboBox1.DisplayMember = "name";

            result = Sqlite.runSql(SQLiteConnString, $"select name from HeatInfo", out DataTable heatdt);
            if (result != "Success") return;
            uiComboBox2.DataSource = heatdt;
            uiComboBox2.DisplayMember = "name";

            result = Sqlite.runSql(SQLiteConnString, $"select * from SetupInfo where 档案名称 = '{name}'", out DataTable dt);
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
            //label355.Visible = uiCheckBox193.Checked;
            uiComboBox2.Enabled = uiCheckBox193.Checked;

            numericUpDown198.Text = dt.Rows[0]["Auto仓提示换盘数量"].ToString();
        }

        private void 刷新测试机(string name)
        {
            string result = GetSetupValue(name, "测试机名称", out string value);
            if (result != "Success") return;

            result = GetTesterIFParam(value, out TesterIFParam tester);
            if (result != "Success") return;

            刷新测试机(tester);
        }

        private void 刷新通讯(string name)
        {
            //string result = GetSetupValue(name, "通讯名称", out string value);
            //if (result != "Success") return;

            string result = GetComInfoSet(name, out ComInfoSet tester);
            if (result != "Success") return;

            刷新通讯(tester);
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

            numericUpDown6.Text = tester.入料手臂吸料超时时间.ToString();
            numericUpDown9.Text = tester.出料手臂吸料超时时间.ToString();
            numericUpDown11.Text = tester.测试手臂吸料超时时间.ToString();
            numericUpDown7.Text = tester.入料手臂吹料时间.ToString();
            numericUpDown8.Text = tester.出料手臂吹料时间.ToString();
            numericUpDown10.Text = tester.测试手臂吹料时间.ToString();
            numericUpDown156.Text = tester.发送开始信号延时.ToString();
            numericUpDown197.Text = tester.接收测试信号上抬延时.ToString();
            numericUpDown160.Text = tester.离线模式测试等待时间.ToString();

            checkBox5.Checked = tester.入料吸提前开真空;
            checkBox6.Checked = tester.出料吸提前开真空;
            checkBox7.Checked = tester.测试吸提前开真空;

            checkBox8.Checked = tester.Auto侧敲使能;
            checkBox9.Checked = tester.前侧同时敲击;
            numericUpDown185.Text = tester.放置多少颗敲击.ToString();
            numericUpDown184.Text = tester.Auto侧敲次数.ToString();
            numericUpDown186.Text = tester.侧敲间隔.ToString();
            checkBox10.Checked = tester.放料时允许侧敲;
            checkBox11.Checked = tester.侧敲时允许放料;

            checkBox12.Checked = tester.Auto换盘时出料Z回零;


            string result = Sqlite.runSql(SQLiteConnString, "select 通讯名称 from ComInfoSet", out DataTable dt);
            if (result == "Success")
            {
                uiComboBox119.DataSource = dt;
                uiComboBox119.DisplayMember = "通讯名称";
            }
            uiComboBox119.Text = tester.通讯名称;

            checkBox13.Checked = tester.测试超时自动关闭Dut;
            checkBox14.Checked = tester.接收超时Bin自动关闭Dut;


            uiCheckBox184.Checked = tester.离线模式IC放置位置 == "AutoFix";

            if (tester.离线模式IC放置位置 == "AutoFix")
            {
                uiCheckBox184.Checked = true;
            }
            else
            {
                uiCheckBox184.Checked = false;
                uiCheckBox185.Checked = true;
                uiComboBox11.Text = tester.离线模式IC放置位置;
            }

            string timeoutbin = tester.超时Bin字符串显示;

            string[] items = { timeoutbin, "OTH" };
            comboBox10.Items.Clear();
            comboBox10.Items.AddRange(items);
            comboBox10.Text = timeoutbin;

            textBox39.Text = timeoutbin;
            label418.Text = timeoutbin + "：超时Bin";
            //textBox38.Enabled = false;
            //textBox38.Text = timeoutbin;
        }

        private void 刷新通讯(ComInfoSet comInfoSet)
        {
            textBox37.Text = comInfoSet.通讯名称;

            uiCheckBox173.Checked = comInfoSet.通讯协议 == "默认";
            uiCheckBox162.Checked = comInfoSet.通讯协议 == "晶存";
            uiCheckBox163.Checked = comInfoSet.通讯协议 == "得一微";
            uiCheckBox164.Checked = comInfoSet.通讯协议 == "慧荣";
            uiCheckBox191.Checked = comInfoSet.通讯协议 == "衡宇";
            uiCheckBox216.Checked = comInfoSet.通讯协议 == "鹏武";
            uiCheckBox165.Checked = comInfoSet.通讯协议 == "江波龙";

            uiCheckBox166.Checked = comInfoSet.通讯方式 == "COM";
            uiCheckBox167.Checked = comInfoSet.通讯方式 == "TCP";
            uiCheckBox168.Checked = comInfoSet.通讯方式 == "LAN";
            uiCheckBox169.Checked = comInfoSet.通讯方式 == "GPIB";

            uiCheckBox158.Checked = comInfoSet.测试机数量 == 1;
            uiCheckBox159.Checked = comInfoSet.测试机数量 == 4;
            uiCheckBox161.Checked = comInfoSet.测试机数量 == 8;
            uiCheckBox160.Checked = comInfoSet.测试机数量 == 16;

            uiCheckBox170.Checked = comInfoSet.同时测试 == 1;
            uiCheckBox171.Checked = comInfoSet.同时测试 == 2;
            uiCheckBox183.Checked = comInfoSet.同时测试 == 4;

            uiCheckBox172.Checked = comInfoSet.测试结果中有对应Site;

            uiCheckBox179.Checked = comInfoSet.发送开批信息;

            uiTextBox7.Text = comInfoSet.超时Bin字符串;

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNameCaseInsensitive = true
            };

            string[] sites = Enumerable.Range(1, 16).Select(i =>
            {
                var property = typeof(ComInfoSet).GetProperty($"Site{i}");
                string json = (string)property.GetValue(comInfoSet);
                return JsonSerializer.Deserialize<SerialPortConfig>(json, options).PortName;
            }).ToArray();

            SerialPortConfig serial = JsonSerializer.Deserialize<SerialPortConfig>(comInfoSet.Site1, options);
            uiComboBox50.Text = serial.BaudRate.ToString();
            uiComboBox51.Text = serial.Parity.ToString();
            uiComboBox49.Text = serial.DataBits.ToString();
            uiComboBox48.Text = serial.StopBits.ToString();

            //string site1 = JsonSerializer.Deserialize<SerialPortConfig>(comInfoSet.Site1, options).PortName;
            //string site2 = JsonSerializer.Deserialize<SerialPortConfig>(comInfoSet.Site2, options).PortName;
            //string site3 = JsonSerializer.Deserialize<SerialPortConfig>(comInfoSet.Site3, options).PortName;
            //string site4 = JsonSerializer.Deserialize<SerialPortConfig>(comInfoSet.Site4, options).PortName;
            //string site5 = JsonSerializer.Deserialize<SerialPortConfig>(comInfoSet.Site5, options).PortName;
            //string site6 = JsonSerializer.Deserialize<SerialPortConfig>(comInfoSet.Site6, options).PortName;
            //string site7 = JsonSerializer.Deserialize<SerialPortConfig>(comInfoSet.Site7, options).PortName;
            //string site8 = JsonSerializer.Deserialize<SerialPortConfig>(comInfoSet.Site8, options).PortName;
            //string site9 = JsonSerializer.Deserialize<SerialPortConfig>(comInfoSet.Site9, options).PortName;
            //string site10 = JsonSerializer.Deserialize<SerialPortConfig>(comInfoSet.Site10, options).PortName;
            //string site11 = JsonSerializer.Deserialize<SerialPortConfig>(comInfoSet.Site11, options).PortName;
            //string site12 = JsonSerializer.Deserialize<SerialPortConfig>(comInfoSet.Site12, options).PortName;
            //string site13 = JsonSerializer.Deserialize<SerialPortConfig>(comInfoSet.Site13, options).PortName;
            //string site14 = JsonSerializer.Deserialize<SerialPortConfig>(comInfoSet.Site14, options).PortName;
            //string site15 = JsonSerializer.Deserialize<SerialPortConfig>(comInfoSet.Site15, options).PortName;
            //string site16 = JsonSerializer.Deserialize<SerialPortConfig>(comInfoSet.Site16, options).PortName;

            if (uiCheckBox158.Checked)
            {
                uiComboBox39.Text = sites[0];
            }
            else if (uiCheckBox159.Checked)
            {
                uiComboBox39.Text = sites[0];
                uiComboBox35.Text = sites[4];
                uiComboBox47.Text = sites[8];
                uiComboBox43.Text = sites[12];
            }
            else if (uiCheckBox161.Checked)
            {
                uiComboBox39.Text = sites[0];
                uiComboBox37.Text = sites[2];
                uiComboBox35.Text = sites[4];
                uiComboBox33.Text = sites[6];
                uiComboBox47.Text = sites[8];
                uiComboBox45.Text = sites[10];
                uiComboBox43.Text = sites[12];
                uiComboBox41.Text = sites[14];
            }
            else if (uiCheckBox160.Checked)
            {
                uiComboBox39.Text = sites[0];
                uiComboBox38.Text = sites[1];
                uiComboBox37.Text = sites[2];
                uiComboBox36.Text = sites[3];
                uiComboBox35.Text = sites[4];
                uiComboBox34.Text = sites[5];
                uiComboBox33.Text = sites[6];
                uiComboBox32.Text = sites[7];
                uiComboBox47.Text = sites[8];
                uiComboBox46.Text = sites[9];
                uiComboBox45.Text = sites[10];
                uiComboBox44.Text = sites[11];
                uiComboBox43.Text = sites[12];
                uiComboBox42.Text = sites[13];
                uiComboBox41.Text = sites[14];
                uiComboBox40.Text = sites[15];
            }
            刷新测试机数量显示();
            //numericUpDown12.Text = comInfoSet.Dout连续不良个数.ToString();
            //checkBox1.Checked = comInfoSet.Dout连续不良自动关闭;
            //checkBox2.Checked = comInfoSet.Dout连续不良报警;

            //numericUpDown5.Text = comInfoSet.Dut累计不良报警个数.ToString();
            //checkBox3.Checked = comInfoSet.Dut累计不良报警使能;
            //numericUpDown13.Text = comInfoSet.总良率报警百分比.ToString();
            //numericUpDown14.Text = comInfoSet.总良率报警基数.ToString();
            //numericUpDown15.Text = comInfoSet.总良率报警间隔.ToString();
            //checkBox4.Checked = comInfoSet.总良率报警使能;
            //numericUpDown16.Text = comInfoSet.socket使用次数.ToString();
            //numericUpDown_测试超时时间.Text = comInfoSet.测试超时时间.ToString();
            //numericUpDown_timeout超时时间.Text = comInfoSet.timeout超时时间.ToString();
            //checkBox_timeout.Checked = comInfoSet.超时发送timeout;

            //uiComboBox11.Text = comInfoSet.串口号;
            //uiComboBox12.Text = comInfoSet.波特率.ToString();
            //uiComboBox14.Text = comInfoSet.校验位.ToString();
            //uiComboBox13.Text = comInfoSet.数据位.ToString();
            //uiComboBox15.Text = comInfoSet.停止位.ToString();

            //numericUpDown6.Text = comInfoSet.入料手臂吸料超时时间.ToString();
            //numericUpDown9.Text = comInfoSet.出料手臂吸料超时时间.ToString();
            //numericUpDown11.Text = comInfoSet.测试手臂吸料超时时间.ToString();
            //numericUpDown7.Text = comInfoSet.入料手臂吹料时间.ToString();
            //numericUpDown8.Text = comInfoSet.出料手臂吹料时间.ToString();
            //numericUpDown10.Text = comInfoSet.测试手臂吹料时间.ToString();
            //numericUpDown156.Text = comInfoSet.发送开始信号延时.ToString();
            //numericUpDown160.Text = comInfoSet.离线模式测试等待时间.ToString();

            //checkBox5.Checked = comInfoSet.入料吸提前开真空;
            //checkBox6.Checked = comInfoSet.出料吸提前开真空;
            //checkBox7.Checked = comInfoSet.测试吸提前开真空;

            //checkBox8.Checked = comInfoSet.Auto侧敲使能;
            //checkBox9.Checked = comInfoSet.前侧同时敲击;
            //numericUpDown185.Text = comInfoSet.放置多少颗敲击.ToString();
            //numericUpDown184.Text = comInfoSet.Auto侧敲次数.ToString();
            //numericUpDown186.Text = comInfoSet.侧敲间隔.ToString();
            //checkBox10.Checked = comInfoSet.放料时允许侧敲;
            //checkBox11.Checked = comInfoSet.侧敲时允许放料;

            //checkBox12.Checked = comInfoSet.Auto换盘时出料Z回零;
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

                numericUpDown21.Text = offset.入料_梭车1第2列放_X.ToString();
                numericUpDown20.Text = offset.入料_梭车1第2列放_Y.ToString();
                numericUpDown19.Text = offset.入料_梭车1第2列放_Z.ToString();

                numericUpDown24.Text = offset.入料_Load_变距.ToString();
                numericUpDown23.Text = offset.入料_Heat_变距.ToString();
                numericUpDown22.Text = offset.入料_梭车_变距.ToString();
                
                numericUpDown27.Text = offset.入料_梭车2第1列放_X.ToString();
                numericUpDown26.Text = offset.入料_梭车2第1列放_Y.ToString();
                numericUpDown25.Text = offset.入料_梭车2第1列放_Z.ToString();

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

        private void 刷新加热盘(string name)
        {
            string result = GetHeatInfo(name, out HeatInfo heatInfo);
            if (result == "Success")
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
                numericUpDown134.Text = heatInfo.IC加热时间.ToString();
                numericUpDown154.Text = heatInfo.达温后延时.ToString();
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
        }


        private void 刷新设定信息(string name)
        {
            int rows = uiDataGridView1.SelectedIndex;
            if (rows >= 0)
            {
                刷新设定(name);
            }
        }

        private void 刷新档案信息(string tabpagetext)
        {
            int rows = uiDataGridView1.SelectedIndex;
            if (rows >= 0)
            {
                //uiCheckBox194.Visible = false;
                //uiCheckBox195.Visible = false;
                //uiCheckBox196.Visible = false;

                string name = uiDataGridView1.Rows[rows].Cells[1].Value.ToString();
                if (tabpagetext == "设定")
                {
                    刷新设定(name);
                    //uiCheckBox194.Visible = true;
                    //uiCheckBox195.Visible = true;
                    //uiCheckBox196.Visible = true;
                }
                else if (tabpagetext == "测试机")
                {
                    刷新测试机(name);
                }
                else if (tabpagetext == "分BIN")
                {
                    刷新测试机(name);
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
            }
        }

        private void 刷新Tray盘(string name)
        {
            string result = Sqlite.runSql(SQLiteConnString, $"select * from TrayInfo where name = '{name}'", out DataTable dt);
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

        private void 刷新灯光状态()
        {
            LightSet lightSet = new LightSet();

            if (uiComboBox16.Text == "报警中") lightSet = 报警状态灯设置;
            else if (uiComboBox16.Text == "待料中") lightSet = 待料状态灯设置;
            else if (uiComboBox16.Text == "归零中") lightSet = 归零状态灯设置;
            else if (uiComboBox16.Text == "暂停中") lightSet = 暂停状态灯设置;
            else if (uiComboBox16.Text == "结批中") lightSet = 结批状态灯设置;

            uiCheckBox207.Checked = lightSet.绿灯颜色显示;
            uiCheckBox208.Checked = lightSet.黄灯颜色显示;
            uiCheckBox209.Checked = lightSet.红灯颜色显示;

            uiCheckBox210.Checked = false;
            uiCheckBox211.Checked = false;
            uiCheckBox210.Checked = lightSet.绿灯显示方式 == 1;
            uiCheckBox211.Checked = lightSet.绿灯显示方式 == 2;

            uiCheckBox213.Checked = false;
            uiCheckBox212.Checked = false;
            uiCheckBox213.Checked = lightSet.黄灯显示方式 == 1;
            uiCheckBox212.Checked = lightSet.黄灯显示方式 == 2;

            uiCheckBox215.Checked = false;
            uiCheckBox214.Checked = false;
            uiCheckBox215.Checked = lightSet.红灯显示方式 == 1;
            uiCheckBox214.Checked = lightSet.红灯显示方式 == 2;

            uiComboBox17.Text = lightSet.报警音乐;

            //bool isdis = uiComboBox16.Text == "报警中";

            //label662.Visible = isdis;
            //uiComboBox17.Visible = isdis;
            //uiButton1.Visible = isdis;
        }

        private void 刷新取放设定(string name)
        {
            string result = Sqlite.runSql(SQLiteConnString, $"select * from TrayInfo where name = '{name}'", out DataTable dt);
            if (result == "Success")
            {
                //uiComboBox1.Text = dt.Rows[0][0].ToString();
                //textBox25.Text = dt.Rows[0][1].ToString();
            }
        }

        private void 刷新Tray盘信息(string tabpagetext, string name)
        {
            if (tabpagetext == "Tray盘")
            {
                刷新Tray盘(name);
            }
        }

        private void 刷新Tray盘信息(string tabpagetext)
        {
            int rows = uiDataGridView2.SelectedIndex;
            if (rows >= 0)
            {
                //uiCheckBox197.Visible = false;
                //uiCheckBox198.Visible = false;
                //uiCheckBox199.Visible = false;
                string name = uiDataGridView2.Rows[rows].Cells[1].Value.ToString();
                if (tabpagetext == "Tray盘")
                {
                    //uiCheckBox197.Visible = true;
                    //uiCheckBox198.Visible = true;
                    //uiCheckBox199.Visible = true;
                    刷新Tray盘(name);
                }
                else if (tabpagetext == "取放设定")
                {
                    //刷新取放设定(name);
                }
            }
        }

        private void 刷新通讯信息(string tabpagetext)
        {
            int rows = uiDataGridView4.SelectedIndex;
            if (rows >= 0)
            {
                string name = uiDataGridView4.Rows[rows].Cells[1].Value.ToString();
                //if (tabpagetext == "通讯信息")
                //{
                刷新通讯(name);
                //}
            }
        }

        private void 刷新通讯2(string name)
        {

        }

        private void 刷新通讯2(ComInfoSet comInfoSet)
        {

        }

        private void 刷新加热盘信息(string tabpagetext, string name)
        {
            //if (tabpagetext == "加热盘")
            //{
            刷新加热盘(name);
            //}
        }

        private void 刷新加热盘信息(string tabpagetext)
        {
            int rows = uiDataGridView3.SelectedIndex;
            if (rows >= 0)
            {
                string name = uiDataGridView3.Rows[rows].Cells[1].Value.ToString();

                //uiCheckBox200.Visible = false;
                //uiCheckBox201.Visible = false;
                //uiCheckBox202.Visible = false;

                //if (tabpagetext == "加热盘")
                //{
                //    //uiCheckBox200.Visible = true;
                //    //uiCheckBox201.Visible = true;
                //    //uiCheckBox202.Visible = true;
                //    刷新加热盘(name);
                //}
                //else if (tabpagetext == "温度设定")
                //{
                刷新加热盘(name);
                //}

            }
        }

        private int uiDataColumsWidth1 = 90;
        private int uiDataColumsWidth2 = 420;
        //private int uiDataColumsWidth = 405;

        private string 刷新档案列表()
        {
            string result = Sqlite.runSql(SQLiteConnString, "select '' as ' ', 档案名称 from SetupInfo", out DataTable dt);
            if (result == "Success")
            {
                uiDataGridView1.DataSource = dt;
                uiDataGridView1.Columns[0].Width = 80;
                uiDataGridView1.Columns[1].Width = 425;

                uiDataGridView1.Columns.Cast<DataGridViewColumn>().ToList().ForEach(column => column.SortMode = DataGridViewColumnSortMode.NotSortable);
                uiDataGridView1.ClearSelection();

                result = Sqlite.runSql(SQLiteConnString, "select value from config where name='当前档案'", out string value).ToString();

                int index = 0;
                //bool isCurrentTray = false;
                if (result == "Success")
                {
                    foreach (DataGridViewRow dr in uiDataGridView1.Rows)
                    {
                        if (dr.Cells[1].Value.Equals(value))
                        {
                            dr.Cells[0].Value = "当前档案";
                            //dr.Cells[0].Selected = true;
                            index = dr.Cells[0].RowIndex;
                            //isCurrentTray = true;
                            break;
                        }
                    }

                    //if (uiDataGridView1.Rows.Count > 0)
                    //{
                    //    uiDataGridView1.CurrentCell = uiDataGridView1.Rows[0].Cells[1];
                    //}
                    //刷新档案信息(uiTabControl6.SelectedTab.Text);

                    if (uiDataGridView1.Rows.Count > 0)
                    {
                        uiDataGridView1.CurrentCell = uiDataGridView1.Rows[index].Cells[1];
                    }
                    刷新档案信息(uiTabControl6.SelectedTab.Text);

                    //if (uiDataGridView1.Rows.Count > 0)
                    //{
                    //    string name = uiDataGridView1.Rows[index].Cells[1].Value.ToString();
                    //    if (isCurrentTray) 刷新设定信息(name); //value uiDataGridView1.Rows[0].Cells[]
                    //    if (uiTabControl6.SelectedTab.Text != "设定")
                    //    {
                    //        刷新档案信息(uiTabControl6.SelectedTab.Text);
                    //    }
                    //    //uiDataGridView1.SelectedIndex = index;
                    //}

                    //uiDataGridView1.CurrentCell = uiDataGridView1.Rows[index].Cells[1];
                    //uiDataGridView1.SelectedIndex = index;
                    //uiDataGridView1.ClearSelection();

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
            string reuslt = Sqlite.runSql(SQLiteConnString, "select '' as ' ', name as Tray盘名称 from TrayInfo", out DataTable dt);
            if (reuslt == "Success")
            {
                uiDataGridView2.DataSource = dt;
                uiDataGridView2.Columns[0].Width = 110;
                uiDataGridView2.Columns[1].Width = 395;

                uiDataGridView2.Columns.Cast<DataGridViewColumn>().ToList().ForEach(column => column.SortMode = DataGridViewColumnSortMode.NotSortable);
                uiDataGridView2.ClearSelection();
                string result = Sqlite.runSql(SQLiteConnString, $"select tray盘名称 from SetupInfo where 档案名称='{设置信息.档案名称}'", out string value).ToString();

                int index = 0;
                //bool isCurrentTray = false;
                if (result == "Success")
                {
                    foreach (DataGridViewRow dr in uiDataGridView2.Rows)
                    {
                        if (dr.Cells[1].Value.Equals(value))
                        {
                            dr.Cells[0].Value = "当前Tray盘";
                            //dr.Cells[0].Selected = true;
                            index = dr.Cells[0].RowIndex;
                            //isCurrentTray = true;
                            break;
                        }
                    }
                }
                if (uiDataGridView2.Rows.Count > 0)
                {
                    uiDataGridView2.CurrentCell = uiDataGridView2.Rows[index].Cells[1];
                }
                刷新Tray盘信息(uiTabControl7.SelectedTab.Text);

                //if (uiDataGridView2.Rows.Count > 0)
                //{
                //    if (isCurrentTray) 刷新Tray盘信息(uiTabControl7.SelectedTab.Text, value);
                //    else 刷新Tray盘信息(uiTabControl7.SelectedTab.Text);
                //}

                //uiDataGridView2.SelectedIndex = index;
                //uiDataGridView2.ClearSelection();
            }
        }

        private void 刷新加热盘列表()
        {
            string reuslt = Sqlite.runSql(SQLiteConnString, "select '' as ' ', name as 加热盘名称 from HeatInfo", out DataTable dt);
            if (reuslt == "Success")
            {
                uiDataGridView3.DataSource = dt;
                uiDataGridView3.Columns[0].Width = 100;
                uiDataGridView3.Columns[1].Width = 405;

                uiDataGridView3.Columns.Cast<DataGridViewColumn>().ToList().ForEach(column => column.SortMode = DataGridViewColumnSortMode.NotSortable);

                uiDataGridView3.ClearSelection();
                string result = Sqlite.runSql(SQLiteConnString, $"select 加热盘名称 from SetupInfo where 档案名称='{设置信息.档案名称}'", out string value).ToString();

                int index = 0;
                //bool isCurrentTray = false;
                if (result == "Success")
                {
                    foreach (DataGridViewRow dr in uiDataGridView3.Rows)
                    {
                        if (dr.Cells[1].Value.Equals(value))
                        {
                            dr.Cells[0].Value = "当前加热盘";
                            //dr.Cells[0].Selected = true;
                            index = dr.Cells[0].RowIndex;
                            //isCurrentTray = true;
                            break;
                        }
                    }
                }
                if (uiDataGridView3.Rows.Count > 0)
                {
                    uiDataGridView3.CurrentCell = uiDataGridView3.Rows[index].Cells[1];
                }
                刷新加热盘信息(uiTabControl9.SelectedTab.Text);

                //if (uiDataGridView3.Rows.Count > 0)
                //{
                //    if (isCurrentTray) 刷新加热盘信息(uiTabControl9.SelectedTab.Text, value);
                //    else 刷新加热盘信息(uiTabControl9.SelectedTab.Text);
                //}
                //uiDataGridView3.SelectedIndex = index;
                //uiDataGridView3.ClearSelection();
                //uiDataGridView3.ClearSelection();
            }
        }

        private void 刷新通讯列表()
        {
            string reuslt = Sqlite.runSql(SQLiteConnString, "select '' as ' ', 通讯名称 from ComInfoSet", out DataTable dt);
            if (reuslt == "Success")
            {
                uiDataGridView4.DataSource = dt;
                uiDataGridView4.Columns[0].Width = 105;
                uiDataGridView4.Columns[1].Width = 400;

                uiDataGridView4.Columns.Cast<DataGridViewColumn>().ToList().ForEach(column => column.SortMode = DataGridViewColumnSortMode.NotSortable);

                uiDataGridView4.ClearSelection();
                string result = Sqlite.runSql(SQLiteConnString, $"select 通讯名称 from TesterIFParam where 测试机名称='{设置信息.测试机信息.测试机名称}'", out string value).ToString();

                int index = 0;
                //bool isCurrentTray = false;
                if (result == "Success")
                {
                    foreach (DataGridViewRow dr in uiDataGridView4.Rows)
                    {
                        if (dr.Cells[1].Value.Equals(value))
                        {
                            dr.Cells[0].Value = "当前通讯";
                            //dr.Cells[0].Selected = true;
                            index = dr.Cells[0].RowIndex;
                            //isCurrentTray = true;
                            break;
                        }
                    }
                }
                if (uiDataGridView4.Rows.Count > 0)
                {
                    uiDataGridView4.CurrentCell = uiDataGridView4.Rows[index].Cells[1];
                }
                刷新通讯信息(uiTabControl11.SelectedTab.Text);

                //if (uiDataGridView3.Rows.Count > 0)
                //{
                //    if (isCurrentTray) 刷新加热盘信息(uiTabControl9.SelectedTab.Text, value);
                //    else 刷新加热盘信息(uiTabControl9.SelectedTab.Text);
                //}
                //uiDataGridView3.SelectedIndex = index;
                //uiDataGridView3.ClearSelection();
                //uiDataGridView3.ClearSelection();
            }
        }

        public void 刷新产品(string tabpagetext)
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
            else if (tabpagetext == "通讯列表")
            {
                刷新通讯列表();
            }
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

                        int auto1num = Convert.ToInt32(numericUpDown198.Value);

                        if (trayname != setupInfo.Tray盘信息.TrayName)
                        {
                            int line = Sqlite.runSqlNonQuery(SQLiteConnString, $"update SetupInfo set tray盘名称 = '{trayname}' where 档案名称 = '{name}'");
                            if (line >= 1)
                            {
                                RunLog($"档案名称:[{name}]:Tray名称:{setupInfo.Tray盘信息.TrayName}->{trayname}:更新成功", LogType.ParamLog);
                                if (name == 设置信息.档案名称)
                                {
                                    if (trayname == 设置信息.Tray盘信息.TrayName)
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
                            }
                            else
                            {
                                MessageBox.Show("tray盘名称保存失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }

                        if (heatname != setupInfo.Heat盘信息.HeatName)
                        {
                            int line = Sqlite.runSqlNonQuery(SQLiteConnString, $"update SetupInfo set 加热盘名称 = '{heatname}' where 档案名称 = '{name}'");
                            if (line >= 1)
                            {
                                RunLog($"档案名称:[{name}]:加热盘名称:{setupInfo.Heat盘信息.HeatName}->{heatname}:更新成功", LogType.ParamLog);
                                if (name == 设置信息.档案名称)
                                {
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
                            }
                            else
                            {
                                MessageBox.Show("Heat盘名称保存失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }

                        if (tempmode != setupInfo.温度模式)
                        {
                            int line = Sqlite.runSqlNonQuery(SQLiteConnString, $"update SetupInfo set 温度模式 = '{tempmode}' where 档案名称 = '{name}'");
                            if (line >= 1)
                            {
                                RunLog($"档案名称:[{name}]:温度模式:{setupInfo.温度模式}->{tempmode}:更新成功", LogType.ParamLog);
                                if (name == 设置信息.档案名称)
                                {
                                    if (tempmode != 设置信息.温度模式)
                                    {
                                        设置信息.温度模式 = tempmode;
                                        刷新温度信息();
                                    }
                                }
                            }
                            else MessageBox.Show("温度模式保存失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                        if (auto1num != setupInfo.Auto仓提示换盘数量)
                        {
                            int line = Sqlite.runSqlNonQuery(SQLiteConnString, $"update SetupInfo set Auto仓提示换盘数量 = '{auto1num}' where 档案名称 = '{name}'");
                            if (line >= 1)
                            {
                                RunLog($"档案名称:[{name}]:Auto仓提示换盘数量:{setupInfo.Auto仓提示换盘数量}->{auto1num}:更新成功", LogType.ParamLog);
                                if (name == 设置信息.档案名称)
                                {
                                    设置信息.Auto仓提示换盘数量 = auto1num;
                                }
                            }
                            else MessageBox.Show("Auto仓提示换盘数量保存失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                        newtester.入料手臂吸料超时时间 = Convert.ToInt32(numericUpDown6.Text);
                        newtester.出料手臂吸料超时时间 = Convert.ToInt32(numericUpDown9.Text);
                        newtester.测试手臂吸料超时时间 = Convert.ToInt32(numericUpDown11.Text);
                        newtester.入料手臂吹料时间 = Convert.ToInt32(numericUpDown7.Text);
                        newtester.出料手臂吹料时间 = Convert.ToInt32(numericUpDown8.Text);
                        newtester.测试手臂吹料时间 = Convert.ToInt32(numericUpDown10.Text);
                        newtester.发送开始信号延时 = Convert.ToInt32(numericUpDown156.Text);
                        newtester.接收测试信号上抬延时 = Convert.ToInt32(numericUpDown197.Text);
                        newtester.离线模式测试等待时间 = Convert.ToInt32(numericUpDown160.Text);

                        newtester.入料吸提前开真空 = checkBox5.Checked;
                        newtester.出料吸提前开真空 = checkBox6.Checked;
                        newtester.测试吸提前开真空 = checkBox7.Checked;

                        newtester.Auto侧敲使能 = checkBox8.Checked;
                        newtester.前侧同时敲击 = checkBox9.Checked;
                        newtester.放置多少颗敲击 = Convert.ToInt32(numericUpDown185.Value);
                        newtester.Auto侧敲次数 = Convert.ToInt32(numericUpDown184.Value);
                        newtester.侧敲间隔 = Convert.ToInt32(numericUpDown186.Value);
                        newtester.放料时允许侧敲 = checkBox10.Checked;
                        newtester.侧敲时允许放料 = checkBox11.Checked;

                        newtester.Auto换盘时出料Z回零 = checkBox12.Checked;

                        newtester.通讯名称 = uiComboBox119.Text;

                        newtester.测试超时自动关闭Dut = checkBox13.Checked;
                        newtester.接收超时Bin自动关闭Dut = checkBox14.Checked;

                        newtester.超时Bin字符串显示 = textBox39.Text;

                        newtester.离线模式IC放置位置 = uiCheckBox185.Checked ? uiComboBox11.Text : "AutoFix";

                        result = UpdateTesterIFParam(value, newtester);
                        if (result != "Success")
                        {
                            MessageBox.Show("保存失败!" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            刷新测试机(oldtester);
                        }
                        else
                        {
                            bool comischange = false;
                            bool timeischange = false;
                            bool timeoutbinchange = false;
                            if (oldtester.Dout连续不良个数 != newtester.Dout连续不良个数) RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[Dout连续不良个数]:{oldtester.Dout连续不良个数}->{newtester.Dout连续不良个数}:更新成功", LogType.ParamLog);
                            if (oldtester.Dout连续不良自动关闭 != newtester.Dout连续不良自动关闭) RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[Dout连续不良自动关闭]:{oldtester.Dout连续不良自动关闭}->{newtester.Dout连续不良自动关闭}:更新成功", LogType.ParamLog);
                            if (oldtester.Dout连续不良报警 != newtester.Dout连续不良报警) RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[Dout连续不良报警]:{oldtester.Dout连续不良报警}->{newtester.Dout连续不良报警}:更新成功", LogType.ParamLog);
                            if (oldtester.Dut累计不良报警个数 != newtester.Dut累计不良报警个数) RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[Dut累计不良报警个数]:{oldtester.Dut累计不良报警个数}->{newtester.Dut累计不良报警个数}:更新成功", LogType.ParamLog);
                            if (oldtester.Dut累计不良报警使能 != newtester.Dut累计不良报警使能) RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[Dut累计不良报警使能]:{oldtester.Dut累计不良报警使能}->{newtester.Dut累计不良报警使能}:更新成功", LogType.ParamLog);
                            if (oldtester.总良率报警百分比 != newtester.总良率报警百分比) RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[总良率报警百分比]:{oldtester.总良率报警百分比}->{newtester.总良率报警百分比}:更新成功", LogType.ParamLog);
                            if (oldtester.总良率报警基数 != newtester.总良率报警基数) RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[总良率报警基数]:{oldtester.总良率报警基数}->{newtester.总良率报警基数}:更新成功", LogType.ParamLog);
                            if (oldtester.总良率报警间隔 != newtester.总良率报警间隔) RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[总良率报警间隔]:{oldtester.总良率报警间隔}->{newtester.总良率报警间隔}:更新成功", LogType.ParamLog);
                            if (oldtester.总良率报警使能 != newtester.总良率报警使能) RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[总良率报警使能]:{oldtester.总良率报警使能}->{newtester.总良率报警使能}:更新成功", LogType.ParamLog);
                            if (oldtester.socket使用次数 != newtester.socket使用次数) RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[socket使用次数]:{oldtester.socket使用次数}->{newtester.socket使用次数}:更新成功", LogType.ParamLog);
                            if (oldtester.测试超时时间 != newtester.测试超时时间) { timeischange = true; RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[测试超时时间]:{oldtester.测试超时时间}->{newtester.测试超时时间}:更新成功", LogType.ParamLog); }
                            if (oldtester.timeout超时时间 != newtester.timeout超时时间) { timeischange = true; RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[timeout超时时间]:{oldtester.timeout超时时间}->{newtester.timeout超时时间}:更新成功", LogType.ParamLog); }
                            if (oldtester.超时发送timeout != newtester.超时发送timeout) RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[超时发送timeout]:{oldtester.超时发送timeout}->{newtester.超时发送timeout}:更新成功", LogType.ParamLog);

                            if (oldtester.入料手臂吸料超时时间 != newtester.入料手臂吸料超时时间) { RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[入料手臂吸料超时时间]:{oldtester.入料手臂吸料超时时间}->{newtester.入料手臂吸料超时时间}:更新成功", LogType.ParamLog); }
                            if (oldtester.出料手臂吸料超时时间 != newtester.出料手臂吸料超时时间) { RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[出料手臂吸料超时时间]:{oldtester.出料手臂吸料超时时间}->{newtester.出料手臂吸料超时时间}:更新成功", LogType.ParamLog); }
                            if (oldtester.测试手臂吸料超时时间 != newtester.测试手臂吸料超时时间) { RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[测试手臂吸料超时时间]:{oldtester.测试手臂吸料超时时间}->{newtester.测试手臂吸料超时时间}:更新成功", LogType.ParamLog); }
                            if (oldtester.入料手臂吹料时间 != newtester.入料手臂吹料时间) { RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[入料手臂吹料时间]:{oldtester.入料手臂吹料时间}->{newtester.入料手臂吹料时间}:更新成功", LogType.ParamLog); }
                            if (oldtester.出料手臂吹料时间 != newtester.出料手臂吹料时间) { RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[出料手臂吹料时间]:{oldtester.出料手臂吹料时间}->{newtester.出料手臂吹料时间}:更新成功", LogType.ParamLog); }
                            if (oldtester.测试手臂吹料时间 != newtester.测试手臂吹料时间) { RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[测试手臂吹料时间]:{oldtester.测试手臂吹料时间}->{newtester.测试手臂吹料时间}:更新成功", LogType.ParamLog); }
                            if (oldtester.发送开始信号延时 != newtester.发送开始信号延时) { RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[发送开始信号延时]:{oldtester.发送开始信号延时}->{newtester.发送开始信号延时}:更新成功", LogType.ParamLog); }
                            if (oldtester.接收测试信号上抬延时 != newtester.接收测试信号上抬延时) { RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[接收测试信号上抬延时]:{oldtester.接收测试信号上抬延时}->{newtester.接收测试信号上抬延时}:更新成功", LogType.ParamLog); }
                            if (oldtester.离线模式测试等待时间 != newtester.离线模式测试等待时间) { timeischange = true; RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[离线模式测试等待时间]:{oldtester.离线模式测试等待时间}->{newtester.离线模式测试等待时间}:更新成功", LogType.ParamLog); }

                            if (oldtester.入料吸提前开真空 != newtester.入料吸提前开真空) { RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[入料吸提前开真空]:{oldtester.入料吸提前开真空}->{newtester.入料吸提前开真空}:更新成功", LogType.ParamLog); }
                            if (oldtester.出料吸提前开真空 != newtester.出料吸提前开真空) { RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[出料吸提前开真空]:{oldtester.出料吸提前开真空}->{newtester.出料吸提前开真空}:更新成功", LogType.ParamLog); }
                            if (oldtester.测试吸提前开真空 != newtester.测试吸提前开真空) { RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[测试吸提前开真空]:{oldtester.测试吸提前开真空}->{newtester.测试吸提前开真空}:更新成功", LogType.ParamLog); }

                            if (oldtester.Auto侧敲使能 != newtester.Auto侧敲使能) { RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[Auto侧敲使能]:{oldtester.Auto侧敲使能}->{newtester.Auto侧敲使能}:更新成功", LogType.ParamLog); }
                            if (oldtester.前侧同时敲击 != newtester.前侧同时敲击) { RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[前侧同时敲击]:{oldtester.前侧同时敲击}->{newtester.前侧同时敲击}:更新成功", LogType.ParamLog); }
                            if (oldtester.放置多少颗敲击 != newtester.放置多少颗敲击) { RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[放置多少颗敲击]:{oldtester.放置多少颗敲击}->{newtester.放置多少颗敲击}:更新成功", LogType.ParamLog); }
                            if (oldtester.Auto侧敲次数 != newtester.Auto侧敲次数) { RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[Auto侧敲次数]:{oldtester.Auto侧敲次数}->{newtester.Auto侧敲次数}:更新成功", LogType.ParamLog); }
                            if (oldtester.侧敲间隔 != newtester.侧敲间隔) { RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[侧敲间隔]:{oldtester.侧敲间隔}->{newtester.侧敲间隔}:更新成功", LogType.ParamLog); }
                            if (oldtester.放料时允许侧敲 != newtester.放料时允许侧敲) { RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[放料时允许侧敲]:{oldtester.放料时允许侧敲}->{newtester.放料时允许侧敲}:更新成功", LogType.ParamLog); }
                            if (oldtester.侧敲时允许放料 != newtester.侧敲时允许放料) { RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[侧敲时允许放料]:{oldtester.侧敲时允许放料}->{newtester.侧敲时允许放料}:更新成功", LogType.ParamLog); }

                            if (oldtester.Auto换盘时出料Z回零 != newtester.Auto换盘时出料Z回零) { RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[Auto换盘时出料Z回零]:{oldtester.Auto换盘时出料Z回零}->{newtester.Auto换盘时出料Z回零}:更新成功", LogType.ParamLog); }

                            if (oldtester.通讯名称 != newtester.通讯名称) { comischange = true; RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[通讯名称]:{oldtester.通讯名称}->{newtester.通讯名称}:更新成功", LogType.ParamLog); }

                            if (oldtester.测试超时自动关闭Dut != newtester.测试超时自动关闭Dut) { RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[测试超时自动关闭Dut]:{oldtester.测试超时自动关闭Dut}->{newtester.测试超时自动关闭Dut}:更新成功", LogType.ParamLog); }
                            if (oldtester.接收超时Bin自动关闭Dut != newtester.接收超时Bin自动关闭Dut) { RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[接收超时Bin自动关闭Dut]:{oldtester.接收超时Bin自动关闭Dut}->{newtester.接收超时Bin自动关闭Dut}:更新成功", LogType.ParamLog); }
                            if (oldtester.超时Bin字符串显示 != newtester.超时Bin字符串显示) { timeoutbinchange = true; RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[超时Bin显示字符串]:{oldtester.超时Bin字符串显示}->{newtester.超时Bin字符串显示}:更新成功", LogType.ParamLog); }

                            if (oldtester.离线模式IC放置位置 != newtester.离线模式IC放置位置) { RunLog($"档案名称:[{name}]:测试机名称:[{value}]:[离线模式IC放置位置]:{oldtester.离线模式IC放置位置}->{newtester.离线模式IC放置位置}:更新成功", LogType.ParamLog); }


                            if (value == 设置信息.测试机信息.测试机名称)
                            {
                                result = GetTesterIFParam(value, out newtester);
                                if (result == "Success")
                                {
                                    设置信息.测试机信息 = newtester;
                                }

                                if (timeischange)
                                {
                                    StationManagers.UpdateTime();// Com.刷新测试超时时间();
                                }

                                if (comischange)
                                {
                                    result = GetComInfoSet(newtester.通讯名称, out ComInfoSet comInfoSet);
                                    if (result == "Success")
                                    {
                                        设置信息.通讯信息 = comInfoSet;
                                        刷新通讯配置();
                                    }
                                    //Com.刷新串口信息();
                                    //Com.CloseCom();
                                    //Com.OpenCom();
                                }
                            }
                            if (timeoutbinchange)
                            {
                                MessageBox.Show($"请到分BIN界面保存新的超时BIN({textBox39.Text})!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private void 获取串口信息(out string[] jsons)
        {
            string 通讯方式 = uiCheckBox166.Checked ? "COM" : uiCheckBox167.Checked ? "TCP" : uiCheckBox168.Checked ? "LAN" : uiCheckBox169.Checked ? "GPIB" : "COM";
            int 测试机数量 = uiCheckBox158.Checked ? 1 : uiCheckBox159.Checked ? 4 : uiCheckBox161.Checked ? 8 : uiCheckBox160.Checked ? 16 : 1;

            string[] coms = new string[16];

            if (测试机数量 == 1)
            {
                for (int i = 0; i < coms.Length; i++)
                {
                    coms[i] = uiComboBox39.Text;
                }

            }
            else if (测试机数量 == 4)
            {
                string[] coms2 = { uiComboBox39.Text, uiComboBox35.Text, uiComboBox47.Text, uiComboBox43.Text };
                for (int i = 0; i < coms2.Length; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        coms[i * 4 + j] = coms2[i];
                    }
                }
            }
            else if (测试机数量 == 8)
            {
                string[] coms3 = { uiComboBox39.Text, uiComboBox37.Text, uiComboBox35.Text, uiComboBox33.Text, uiComboBox47.Text, uiComboBox45.Text, uiComboBox43.Text, uiComboBox41.Text };
                for (int i = 0; i < coms3.Length; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        coms[i * 2 + j] = coms3[i];
                    }
                }
            }
            else if (测试机数量 == 16)
            {
                string[] coms4 = { uiComboBox39.Text, uiComboBox38.Text, uiComboBox37.Text, uiComboBox36.Text, uiComboBox35.Text, uiComboBox34.Text, uiComboBox33.Text, uiComboBox32.Text, uiComboBox47.Text, uiComboBox46.Text, uiComboBox45.Text, uiComboBox44.Text, uiComboBox43.Text, uiComboBox42.Text, uiComboBox41.Text, uiComboBox40.Text };
                for (int i = 0; i < coms4.Length; i++)
                {
                    coms[i] = coms4[i];
                }
            }

            int baudRate = Convert.ToInt32(uiComboBox50.Text);
            int dataBits = Convert.ToInt32(uiComboBox49.Text);
            StopBits stopBits = (StopBits)Enum.Parse(typeof(StopBits), uiComboBox48.Text);
            Parity parity = (Parity)Enum.Parse(typeof(Parity), uiComboBox51.Text);
            jsons = new string[coms.Length];
            for (int i = 0; i < coms.Length; i++)
            {
                var config = new SerialPortConfig
                {
                    Protocol = "COM",
                    PortName = coms[i],
                    BaudRate = baudRate,
                    Parity = parity,
                    DataBits = dataBits,
                    StopBits = stopBits,
                };
                // 序列化为JSON
                jsons[i] = System.Text.Json.JsonSerializer.Serialize(config);
                //sites.Add(new Dictionary<string, object>() { { "Protocol", "COM" }, { "PortName", coms[i] }, { "BaudRate", baudRate }, { "DataBits", dataBits }, { "StopBits", stopBits }, { "Parity", parity } } );
            }

            //var site1 = new Dictionary<string, object>() { { "Protocol", "COM" }, { "PortName", coms[0] }, { "BaudRate", baudRate }, { "DataBits", dataBits }, { "StopBits", stopBits }, { "Parity", parity } };
            //var site2 = new Dictionary<string, object>() { { "Protocol", "COM" }, { "PortName", "COM11" }, { "BaudRate", baudRate }, { "DataBits", dataBits }, { "StopBits", stopBits }, { "Parity", parity } };
            //var site3 = new Dictionary<string, object>() { { "Protocol", "COM" }, { "PortName", "COM12" }, { "BaudRate", baudRate }, { "DataBits", dataBits }, { "StopBits", stopBits }, { "Parity", parity } };
            //var site4 = new Dictionary<string, object>() { { "Protocol", "COM" }, { "PortName", "COM13" }, { "BaudRate", baudRate }, { "DataBits", dataBits }, { "StopBits", stopBits }, { "Parity", parity } };
            //var site5 = new Dictionary<string, object>() { { "Protocol", "COM" }, { "PortName", "COM14" }, { "BaudRate", baudRate }, { "DataBits", dataBits }, { "StopBits", stopBits }, { "Parity", parity } };
            //var site6 = new Dictionary<string, object>() { { "Protocol", "COM" }, { "PortName", "COM15" }, { "BaudRate", baudRate }, { "DataBits", dataBits }, { "StopBits", stopBits }, { "Parity", parity } };
            //var site7 = new Dictionary<string, object>() { { "Protocol", "COM" }, { "PortName", "COM16" }, { "BaudRate", baudRate }, { "DataBits", dataBits }, { "StopBits", stopBits }, { "Parity", parity } };
            //var site8 = new Dictionary<string, object>() { { "Protocol", "COM" }, { "PortName", "COM17" }, { "BaudRate", baudRate }, { "DataBits", dataBits }, { "StopBits", stopBits }, { "Parity", parity } };
            //var site9 = new Dictionary<string, object>() { { "Protocol", "COM" }, { "PortName", "COM18" }, { "BaudRate", baudRate }, { "DataBits", dataBits }, { "StopBits", stopBits }, { "Parity", parity } };
            //var site10 = new Dictionary<string, object>() { { "Protocol", "COM" }, { "PortName", "COM19" }, { "BaudRate", baudRate }, { "DataBits", dataBits }, { "StopBits", stopBits }, { "Parity", parity } };
            //var site11 = new Dictionary<string, object>() { { "Protocol", "COM" }, { "PortName", "COM20" }, { "BaudRate", baudRate }, { "DataBits", dataBits }, { "StopBits", stopBits }, { "Parity", parity } };
            //var site12 = new Dictionary<string, object>() { { "Protocol", "COM" }, { "PortName", "COM21" }, { "BaudRate", baudRate }, { "DataBits", dataBits }, { "StopBits", stopBits }, { "Parity", parity } };
            //var site13 = new Dictionary<string, object>() { { "Protocol", "COM" }, { "PortName", "COM22" }, { "BaudRate", baudRate }, { "DataBits", dataBits }, { "StopBits", stopBits }, { "Parity", parity } };
            //var site14 = new Dictionary<string, object>() { { "Protocol", "COM" }, { "PortName", "COM23" }, { "BaudRate", baudRate }, { "DataBits", dataBits }, { "StopBits", stopBits }, { "Parity", parity } };
            //var site15 = new Dictionary<string, object>() { { "Protocol", "COM" }, { "PortName", "COM24" }, { "BaudRate", baudRate }, { "DataBits", dataBits }, { "StopBits", stopBits }, { "Parity", parity } };
            //var site16 = new Dictionary<string, object>() { { "Protocol", "COM" }, { "PortName", "COM25" }, { "BaudRate", baudRate }, { "DataBits", dataBits }, { "StopBits", stopBits }, { "Parity", parity } };
        }

        private void 保存通讯信息()
        {
            int rows = uiDataGridView4.SelectedIndex;
            if (rows >= 0)
            {
                string name = uiDataGridView4.Rows[rows].Cells[1].Value.ToString();
                try
                {
                    DialogResult issave = MessageBox.Show("请确认是否保存！", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                    if (issave == DialogResult.OK)
                    {
                        string result = GetComInfoSet(name, out ComInfoSet oldComset);
                        if (result != "Success")
                        {
                            MessageBox.Show("保存失败!" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        获取串口信息(out string[] jsons);


                        ComInfoSet newComset = new ComInfoSet();
                        newComset.通讯名称 = name;
                        newComset.通讯协议 = uiCheckBox173.Checked ? "默认" : uiCheckBox162.Checked ? "晶存" : uiCheckBox163.Checked ? "得一微" : uiCheckBox164.Checked ? "慧荣" : uiCheckBox191.Checked ? "衡宇" : uiCheckBox216.Checked ? "鹏武" : uiCheckBox165.Checked ? "江波龙" : "默认";
                        newComset.通讯方式 = uiCheckBox166.Checked ? "COM" : uiCheckBox167.Checked ? "TCP" : uiCheckBox168.Checked ? "LAN" : uiCheckBox169.Checked ? "GPIB" : "COM";
                        newComset.测试机数量 = uiCheckBox158.Checked ? 1 : uiCheckBox159.Checked ? 4 : uiCheckBox161.Checked ? 8 : uiCheckBox160.Checked ? 16 : 1;
                        newComset.同时测试 = uiCheckBox170.Checked ? 1 : uiCheckBox171.Checked ? 2 : 4;
                        newComset.测试结果中有对应Site = uiCheckBox172.Checked;
                        newComset.超时Bin字符串 = uiTextBox7.Text;
                        newComset.发送开批信息 = uiCheckBox179.Checked;
                        newComset.Site1 = jsons[0];
                        newComset.Site2 = jsons[1];
                        newComset.Site3 = jsons[2];
                        newComset.Site4 = jsons[3];
                        newComset.Site5 = jsons[4];
                        newComset.Site6 = jsons[5];
                        newComset.Site7 = jsons[6];
                        newComset.Site8 = jsons[7];
                        newComset.Site9 = jsons[8];
                        newComset.Site10 = jsons[9];
                        newComset.Site11 = jsons[10];
                        newComset.Site12 = jsons[11];
                        newComset.Site13 = jsons[12];
                        newComset.Site14 = jsons[13];
                        newComset.Site15 = jsons[14];
                        newComset.Site16 = jsons[15];

                        result = UpdateComInfoSet(name, newComset);
                        if (result != "Success")
                        {
                            MessageBox.Show("保存失败!" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            刷新通讯(oldComset);
                        }
                        else
                        {
                            if (oldComset.通讯协议 != newComset.通讯协议) RunLog($"通讯名称:[{name}]:[通讯协议]:{oldComset.通讯协议}->{newComset.通讯协议}:更新成功", LogType.ParamLog);
                            if (oldComset.通讯方式 != newComset.通讯方式) RunLog($"通讯名称:[{name}]:[通讯方式]:{oldComset.通讯方式}->{newComset.通讯方式}:更新成功", LogType.ParamLog);
                            if (oldComset.测试机数量 != newComset.测试机数量) RunLog($"通讯名称:[{name}]:[测试机数量]:{oldComset.测试机数量}->{newComset.测试机数量}:更新成功", LogType.ParamLog);
                            if (oldComset.同时测试 != newComset.同时测试) RunLog($"通讯名称:[{name}]:[同时测试]:{oldComset.同时测试}->{newComset.同时测试}:更新成功", LogType.ParamLog);
                            if (oldComset.测试结果中有对应Site != newComset.测试结果中有对应Site) RunLog($"通讯名称:[{name}]:[测试结果中有对应Site]:{oldComset.测试结果中有对应Site}->{newComset.测试结果中有对应Site}:更新成功", LogType.ParamLog);
                            if (oldComset.超时Bin字符串 != newComset.超时Bin字符串) RunLog($"通讯名称:[{name}]:[超时Bin字符串]:{oldComset.超时Bin字符串}->{newComset.超时Bin字符串}:更新成功", LogType.ParamLog);
                            if (oldComset.发送开批信息 != newComset.发送开批信息) RunLog($"通讯名称:[{name}]:[发送开批信息]:{oldComset.发送开批信息}->{newComset.发送开批信息}:更新成功", LogType.ParamLog);
                            if (oldComset.Site1 != newComset.Site1) RunLog($"通讯名称:[{name}]:[Site1]:{oldComset.Site1}->{newComset.Site1}:更新成功", LogType.ParamLog);
                            if (oldComset.Site2 != newComset.Site2) RunLog($"通讯名称:[{name}]:[Site2]:{oldComset.Site2}->{newComset.Site2}:更新成功", LogType.ParamLog);
                            if (oldComset.Site3 != newComset.Site3) RunLog($"通讯名称:[{name}]:[Site3]:{oldComset.Site3}->{newComset.Site3}:更新成功", LogType.ParamLog);
                            if (oldComset.Site4 != newComset.Site4) RunLog($"通讯名称:[{name}]:[Site4]:{oldComset.Site4}->{newComset.Site4}:更新成功", LogType.ParamLog);
                            if (oldComset.Site5 != newComset.Site5) RunLog($"通讯名称:[{name}]:[Site5]:{oldComset.Site5}->{newComset.Site5}:更新成功", LogType.ParamLog);
                            if (oldComset.Site6 != newComset.Site6) RunLog($"通讯名称:[{name}]:[Site6]:{oldComset.Site6}->{newComset.Site6}:更新成功", LogType.ParamLog);
                            if (oldComset.Site7 != newComset.Site7) RunLog($"通讯名称:[{name}]:[Site7]:{oldComset.Site7}->{newComset.Site7}:更新成功", LogType.ParamLog);
                            if (oldComset.Site8 != newComset.Site8) RunLog($"通讯名称:[{name}]:[Site8]:{oldComset.Site8}->{newComset.Site8}:更新成功", LogType.ParamLog);
                            if (oldComset.Site9 != newComset.Site9) RunLog($"通讯名称:[{name}]:[Site9]:{oldComset.Site9}->{newComset.Site9}:更新成功", LogType.ParamLog);
                            if (oldComset.Site10 != newComset.Site10) RunLog($"通讯名称:[{name}]:[Site10]:{oldComset.Site10}->{newComset.Site10}:更新成功", LogType.ParamLog);
                            if (oldComset.Site11 != newComset.Site11) RunLog($"通讯名称:[{name}]:[Site11]:{oldComset.Site11}->{newComset.Site11}:更新成功", LogType.ParamLog);
                            if (oldComset.Site12 != newComset.Site12) RunLog($"通讯名称:[{name}]:[Site12]:{oldComset.Site12}->{newComset.Site12}:更新成功", LogType.ParamLog);
                            if (oldComset.Site13 != newComset.Site13) RunLog($"通讯名称:[{name}]:[Site13]:{oldComset.Site13}->{newComset.Site13}:更新成功", LogType.ParamLog);
                            if (oldComset.Site14 != newComset.Site14) RunLog($"通讯名称:[{name}]:[Site14]:{oldComset.Site14}->{newComset.Site14}:更新成功", LogType.ParamLog);
                            if (oldComset.Site15 != newComset.Site15) RunLog($"通讯名称:[{name}]:[Site15]:{oldComset.Site15}->{newComset.Site15}:更新成功", LogType.ParamLog);
                            if (oldComset.Site16 != newComset.Site16) RunLog($"通讯名称:[{name}]:[Site16]:{oldComset.Site16}->{newComset.Site16}:更新成功", LogType.ParamLog);

                            if (name == 设置信息.通讯信息.通讯名称)
                            {
                                result = GetComInfoSet(name, out newComset);
                                if (result == "Success")
                                {
                                    设置信息.通讯信息 = newComset;
                                }

                                刷新通讯配置();

                                //int testNum = 设置信息.通讯信息.测试机数量;

                                //var configDict = new Dictionary<int, Dictionary<string, object>>();

                                //// 直接创建并填充数组
                                //Dictionary<string, object>[] sites = Enumerable.Range(0, 16)
                                //    .Select(i => ConvertToDictionary(GetSiteConfig(i)))
                                //    .ToArray();

                                //int step = 16 / testNum;
                                //for (int i = 0; i < testNum; i++)
                                //{
                                //    int siteIndex = i * step;
                                //    configDict.Add(siteIndex, sites[siteIndex]);
                                //}
                                //StationManagers.UpdateStationsConfig(configDict);
                                //for (int i = 0; i < 16; i++)
                                //{
                                //    StationManagers.Connect(i); Thread.Sleep(5);
                                //    //StationManagers.Send(i, "@START_INFO ZRS2409290dlt:BS_TEST:HM:RS70B64G4M08G:E9002B:AE35:FT3_RS70B64G4M08G_032003003_032003003::3040:::FT::R80544:FT3:FT:25+");
                                //    //.SendAsync("@START_INFO ZRS240929001A:BS_TEST:HM:RS70B64G4M08G:E9002B:AE35:FT3_RS70B64G4M08G_032003003_032003003::3040:::FT::R80544:FT3:FT:25+");//我为宇宙，woweiyuzhou, I am Yuzhou

                                //    //StationManagers.GetStation(i).SendAsync("@START_TEST 00 11111111+");//我为宇宙，woweiyuzhou, I am Yuzhou
                                //}

                            }
                            刷新通讯(newComset);
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

            if (listBox1.Items.Count == 0)
            {
                MessageBox.Show($"{textBox_name1.Text}中BIN项不能为空!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (listBox2.Items.Count == 0)
            {
                MessageBox.Show($"{textBox_name2.Text}中BIN项不能为空!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (radioButton3_1.Checked)
            {
                if (listBox3_1.Items.Count == 0)
                {
                    //issave = MessageBox.Show($"{textBox_name3.Text}分BIN—1区BIN项为空,请确认是否保存!", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    //if (issave == DialogResult.Cancel) return;

                    MessageBox.Show($"{textBox_name3.Text}分BIN—1区BIN项不能为空!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
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
                    //issave = MessageBox.Show($"{textBox_name4.Text}分BIN—1区BIN项为空,请确认是否保存!", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    //if (issave == DialogResult.Cancel) return;

                    MessageBox.Show($"{textBox_name4.Text}分BIN—1区BIN项不能为空!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
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
                    //issave = MessageBox.Show($"{textBox_name5.Text}分BIN—1区BIN项为空,请确认是否保存!", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    //if (issave == DialogResult.Cancel) return;

                    MessageBox.Show($"{textBox_name5.Text}分BIN—1区BIN项不能为空!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
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
                        if (oldbinParam.auto1 != newbinParam.auto1) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[auto1]:{oldbinParam.auto1}->{newbinParam.auto1}:更新成功", LogType.ParamLog);
                        if (oldbinParam.auto2 != newbinParam.auto2) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[auto2]:{oldbinParam.auto2}->{newbinParam.auto2}:更新成功", LogType.ParamLog);
                        if (oldbinParam.fix1 != newbinParam.fix1) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[fix1]:{oldbinParam.fix1}->{newbinParam.fix1}:更新成功", LogType.ParamLog);
                        if (oldbinParam.fix2 != newbinParam.fix2) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[fix2]:{oldbinParam.fix2}->{newbinParam.fix2}:更新成功", LogType.ParamLog);
                        if (oldbinParam.fix3 != newbinParam.fix3) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[fix3]:{oldbinParam.fix3}->{newbinParam.fix3}:更新成功", LogType.ParamLog);
                        if (oldbinParam.fix1num != newbinParam.fix1num) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[fix1num]:{oldbinParam.fix1num}->{newbinParam.fix1num}:更新成功", LogType.ParamLog);
                        if (oldbinParam.fix2num != newbinParam.fix2num) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[fix2num]:{oldbinParam.fix2num}->{newbinParam.fix2num}:更新成功", LogType.ParamLog);
                        if (oldbinParam.fix3num != newbinParam.fix3num) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[fix3num]:{oldbinParam.fix3num}->{newbinParam.fix3num}:更新成功", LogType.ParamLog);
                        if (oldbinParam.color1 != newbinParam.color1) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[color1]:{oldbinParam.color1}->{newbinParam.color1}:更新成功", LogType.ParamLog);
                        if (oldbinParam.color2 != newbinParam.color2) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[color2]:{oldbinParam.color2}->{newbinParam.color2}:更新成功", LogType.ParamLog);
                        if (oldbinParam.color3 != newbinParam.color3) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[color3]:{oldbinParam.color3}->{newbinParam.color3}:更新成功", LogType.ParamLog);
                        if (oldbinParam.color4 != newbinParam.color4) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[color4]:{oldbinParam.color4}->{newbinParam.color4}:更新成功", LogType.ParamLog);
                        if (oldbinParam.color5 != newbinParam.color5) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[color5]:{oldbinParam.color5}->{newbinParam.color5}:更新成功", LogType.ParamLog);
                        if (oldbinParam.name1 != newbinParam.name1) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[name1]:{oldbinParam.name1}->{newbinParam.name1}:更新成功", LogType.ParamLog);
                        if (oldbinParam.name2 != newbinParam.name2) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[name2]:{oldbinParam.name2}->{newbinParam.name2}:更新成功", LogType.ParamLog);
                        if (oldbinParam.name3 != newbinParam.name3) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[name3]:{oldbinParam.name3}->{newbinParam.name3}:更新成功", LogType.ParamLog);
                        if (oldbinParam.name4 != newbinParam.name4) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[name4]:{oldbinParam.name4}->{newbinParam.name4}:更新成功", LogType.ParamLog);
                        if (oldbinParam.name5 != newbinParam.name5) RunLog($"档案名称:[{name}]:分BIN名称:[{value}]:[name5]:{oldbinParam.name5}->{newbinParam.name5}:更新成功", LogType.ParamLog);
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

                    UpdateOffsetPointData(name, page, "入料_梭车1第2列放_X", Convert.ToDouble(numericUpDown21.Text), offset.入料_梭车1第2列放_X);
                    UpdateOffsetPointData(name, page, "入料_梭车1第2列放_Y", Convert.ToDouble(numericUpDown20.Text), offset.入料_梭车1第2列放_Y);
                    UpdateOffsetPointData(name, page, "入料_梭车1第2列放_Z", Convert.ToDouble(numericUpDown19.Text), offset.入料_梭车1第2列放_Z);

                    UpdateOffsetPointData(name, page, "入料_Load_变距", Convert.ToDouble(numericUpDown24.Text), offset.入料_Load_变距);
                    UpdateOffsetPointData(name, page, "入料_Heat_变距", Convert.ToDouble(numericUpDown23.Text), offset.入料_Heat_变距);
                    UpdateOffsetPointData(name, page, "入料_梭车_变距", Convert.ToDouble(numericUpDown22.Text), offset.入料_梭车_变距);

                    UpdateOffsetPointData(name, page, "入料_梭车2第1列放_X", Convert.ToDouble(numericUpDown27.Text), offset.入料_梭车2第1列放_X);
                    UpdateOffsetPointData(name, page, "入料_梭车2第1列放_Y", Convert.ToDouble(numericUpDown26.Text), offset.入料_梭车2第1列放_Y);
                    UpdateOffsetPointData(name, page, "入料_梭车2第1列放_Z", Convert.ToDouble(numericUpDown25.Text), offset.入料_梭车2第1列放_Z);

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
                //if (int.TryParse(textBox40.Text, out int res)) textBox40.Text = res.ToString("D2");

                //if (textBox40.Text == "00")
                //{
                //    MessageBox.Show("测试机中\"00\"代表未放料，禁止添加该Bin项!");
                //    return;
                //}

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
                            MessageBox.Show("特殊Bin项目重复");
                            return;
                        }
                    }
                }
                //if (textBox38.Text == comboBox10.Text) textBox38.Enabled = false;
                (tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox).Items.Add(comboBox10.Text);
            }
            textBox40.Text = "";
        }

        private void button_del1_Click(object sender, EventArgs e)
        {
            ListBox l = tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox;

            //if (textBox38.Text == l.Items[l.SelectedIndex].ToString()) textBox38.Enabled = true;

            l.Items.Remove(l.SelectedItem);
        }

        private void button_add2_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                if (textBox40.Text == "") return;
                //if (int.TryParse(textBox40.Text, out int res)) textBox40.Text = res.ToString("D2");

                //if (textBox40.Text == "00")
                //{
                //    MessageBox.Show("测试机中\"00\"代表未放料，禁止添加该Bin项!");
                //    return;
                //}

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
                            MessageBox.Show("特殊Bin项目重复");
                            return;
                        }
                    }
                }
                //if (textBox38.Text == comboBox10.Text) textBox38.Enabled = false;
                (tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox).Items.Add(comboBox10.Text);
            }
            textBox40.Text = "";
        }

        private void button_del2_Click(object sender, EventArgs e)
        {
            ListBox l = tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox;
            //if (textBox38.Text == l.Items[l.SelectedIndex].ToString()) textBox38.Enabled = true;
            l.Items.Remove(l.SelectedItem);
        }

        private void button_add3_1_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                if (textBox40.Text == "") return;

                //if (int.TryParse(textBox40.Text, out int res)) textBox40.Text = res.ToString("D2");

                //if (textBox40.Text == "00")
                //{
                //    MessageBox.Show("测试机中\"00\"代表未放料，禁止添加该Bin项!");
                //    return;
                //}

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
                            MessageBox.Show("特殊Bin项目重复");
                            return;
                        }
                    }
                }
                //if (textBox38.Text == comboBox10.Text) textBox38.Enabled = false;
                (tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox).Items.Add(comboBox10.Text);
            }
            textBox40.Text = "";
        }

        private void button_del3_1_Click(object sender, EventArgs e)
        {
            ListBox l = tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox;
            //if (textBox38.Text == l.Items[l.SelectedIndex].ToString()) textBox38.Enabled = true;
            l.Items.Remove(l.SelectedItem);
        }

        private void button_add3_2_Click(object sender, EventArgs e)
        {

            if (radioButton1.Checked)
            {
                if (textBox40.Text == "") return;
                //if (int.TryParse(textBox40.Text, out int res)) textBox40.Text = res.ToString("D2");

                //if (textBox40.Text == "00")
                //{
                //    MessageBox.Show("测试机中\"00\"代表未放料，禁止添加该Bin项!");
                //    return;
                //}

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
                            MessageBox.Show("特殊Bin项目重复");
                            return;
                        }
                    }
                }
                //if (textBox38.Text == comboBox10.Text) textBox38.Enabled = false;
                (tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox).Items.Add(comboBox10.Text);
            }
            textBox40.Text = "";
        }

        private void button_del3_2_Click(object sender, EventArgs e)
        {
            ListBox l = tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox;
            //if (textBox38.Text == l.Items[l.SelectedIndex].ToString()) textBox38.Enabled = true;
            l.Items.Remove(l.SelectedItem);
        }

        private void button_add3_3_Click(object sender, EventArgs e)
        {

            if (radioButton1.Checked)
            {
                if (textBox40.Text == "") return;
                //if (int.TryParse(textBox40.Text, out int res)) textBox40.Text = res.ToString("D2");

                //if (textBox40.Text == "00")
                //{
                //    MessageBox.Show("测试机中\"00\"代表未放料，禁止添加该Bin项!");
                //    return;
                //}

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
                            MessageBox.Show("特殊Bin项目重复");
                            return;
                        }
                    }
                }
                //if (textBox38.Text == comboBox10.Text) textBox38.Enabled = false;
                (tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox).Items.Add(comboBox10.Text);
            }
            textBox40.Text = "";
        }

        private void button_del3_3_Click(object sender, EventArgs e)
        {
            ListBox l = tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox;
            //if (textBox38.Text == l.Items[l.SelectedIndex].ToString()) textBox38.Enabled = true;
            l.Items.Remove(l.SelectedItem);
        }

        private void button_add4_1_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                if (textBox40.Text == "") return;
                //if (int.TryParse(textBox40.Text, out int res)) textBox40.Text = res.ToString("D2");

                //if (textBox40.Text == "00")
                //{
                //    MessageBox.Show("测试机中\"00\"代表未放料，禁止添加该Bin项!");
                //    return;
                //}

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
                            MessageBox.Show("特殊Bin项目重复");
                            return;
                        }
                    }
                }
                //if (textBox38.Text == comboBox10.Text) textBox38.Enabled = false;
                (tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox).Items.Add(comboBox10.Text);
            }
            textBox40.Text = "";
        }

        private void button_del4_1_Click(object sender, EventArgs e)
        {
            ListBox l = tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox;
            //if (textBox38.Text == l.Items[l.SelectedIndex].ToString()) textBox38.Enabled = true;
            l.Items.Remove(l.SelectedItem);
        }

        private void button_add4_2_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                if (textBox40.Text == "") return;
                //if (int.TryParse(textBox40.Text, out int res)) textBox40.Text = res.ToString("D2");

                //if (textBox40.Text == "00")
                //{
                //    MessageBox.Show("测试机中\"00\"代表未放料，禁止添加该Bin项!");
                //    return;
                //}

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
                            MessageBox.Show("特殊Bin项目重复");
                            return;
                        }
                    }
                }
                //if (textBox38.Text == comboBox10.Text) textBox38.Enabled = false;
                (tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox).Items.Add(comboBox10.Text);
            }
            textBox40.Text = "";
        }

        private void button_del4_2_Click(object sender, EventArgs e)
        {
            ListBox l = tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox;
            //if (textBox38.Text == l.Items[l.SelectedIndex].ToString()) textBox38.Enabled = true;
            l.Items.Remove(l.SelectedItem);
        }

        private void button_add4_3_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                if (textBox40.Text == "") return;
                //if (int.TryParse(textBox40.Text, out int res)) textBox40.Text = res.ToString("D2");

                //if (textBox40.Text == "00")
                //{
                //    MessageBox.Show("测试机中\"00\"代表未放料，禁止添加该Bin项!");
                //    return;
                //}

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
                            MessageBox.Show("特殊Bin项目重复");
                            return;
                        }
                    }
                }
                //if (textBox38.Text == comboBox10.Text) textBox38.Enabled = false;
                (tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox).Items.Add(comboBox10.Text);
            }
            textBox40.Text = "";
        }

        private void button_del4_3_Click(object sender, EventArgs e)
        {
            ListBox l = tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox;
            //if (textBox38.Text == l.Items[l.SelectedIndex].ToString()) textBox38.Enabled = true;
            l.Items.Remove(l.SelectedItem);
        }

        private void button_add5_1_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                if (textBox40.Text == "") return;
                //if (int.TryParse(textBox40.Text, out int res)) textBox40.Text = res.ToString("D2");

                //if (textBox40.Text == "00")
                //{
                //    MessageBox.Show("测试机中\"00\"代表未放料，禁止添加该Bin项!");
                //    return;
                //}

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
                            MessageBox.Show("特殊Bin项目重复");
                            return;
                        }
                    }
                }
                //if (textBox38.Text == comboBox10.Text) textBox38.Enabled = false;
                (tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox).Items.Add(comboBox10.Text);
            }
            textBox40.Text = "";
        }

        private void button_del5_1_Click(object sender, EventArgs e)
        {
            ListBox l = tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox;
            //if (textBox38.Text == l.Items[l.SelectedIndex].ToString()) textBox38.Enabled = true;
            l.Items.Remove(l.SelectedItem);
        }

        private void button_add5_2_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                if (textBox40.Text == "") return;
                //if (int.TryParse(textBox40.Text, out int res)) textBox40.Text = res.ToString("D2");

                //if (textBox40.Text == "00")
                //{
                //    MessageBox.Show("测试机中\"00\"代表未放料，禁止添加该Bin项!");
                //    return;
                //}

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
                            MessageBox.Show("特殊Bin项目重复");
                            return;
                        }
                    }
                }
                //if (textBox38.Text == comboBox10.Text) textBox38.Enabled = false;
                (tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox).Items.Add(comboBox10.Text);
            }
            textBox40.Text = "";
        }

        private void button_del5_2_Click(object sender, EventArgs e)
        {
            ListBox l = tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox;
            //if (textBox38.Text == l.Items[l.SelectedIndex].ToString()) textBox38.Enabled = true;
            l.Items.Remove(l.SelectedItem);
        }

        private void button_add5_3_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                if (textBox40.Text == "") return;

                //if (int.TryParse(textBox40.Text, out int res)) textBox40.Text = res.ToString("D2");

                //if (textBox40.Text == "00")
                //{
                //    MessageBox.Show("测试机中\"00\"代表未放料，禁止添加该Bin项!");
                //    return;
                //}

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
                            MessageBox.Show("特殊Bin项目重复");
                            return;
                        }
                    }
                }
                //if (textBox38.Text == comboBox10.Text) textBox38.Enabled = false;
                (tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox).Items.Add(comboBox10.Text);
            }
            textBox40.Text = "";
        }

        private void button_del5_3_Click(object sender, EventArgs e)
        {
            ListBox l = tabPage_bin.Controls["listBox" + (sender as UIButton).Name.Substring(10)] as ListBox;
            //if (textBox38.Text == l.Items[l.SelectedIndex].ToString()) textBox38.Enabled = true;
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
                    刷新测试机(uiDataGridView1.Rows[rows].Cells[1].Value.ToString());
                }
                else if (tabtext == "测试机")
                {
                    刷新测试机(uiDataGridView1.Rows[rows].Cells[1].Value.ToString());
                }
                else if (tabtext == "点位偏移")
                {
                    刷新点位偏移(uiDataGridView1.Rows[rows].Cells[1].Value.ToString());
                }
                else if (tabtext == "设定")
                {
                    刷新设定(uiDataGridView1.Rows[rows].Cells[1].Value.ToString());
                }
            }
        }

        private void label_color1_Click(object sender, EventArgs e)
        {
            DialogResult result = colorDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                (sender as Label).BackColor = colorDialog1.Color;
            }
        }

        private void label_color2_Click(object sender, EventArgs e)
        {

            DialogResult result = colorDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                (sender as Label).BackColor = colorDialog1.Color;
            }
        }

        private void label_color3_Click(object sender, EventArgs e)
        {
            DialogResult result = colorDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                (sender as Label).BackColor = colorDialog1.Color;
            }
        }

        private void label_color4_Click(object sender, EventArgs e)
        {

            DialogResult result = colorDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                (sender as Label).BackColor = colorDialog1.Color;
            }
        }

        private void label_color5_Click(object sender, EventArgs e)
        {

            DialogResult result = colorDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                (sender as Label).BackColor = colorDialog1.Color;
            }
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
                    IC加热时间 = Convert.ToInt32(numericUpDown134.Text),
                    达温后延时 = Convert.ToInt32(numericUpDown154.Text),
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
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "IC加热时间", newheatInfo.IC加热时间, oldheatInfo.IC加热时间);
                        UpdateHeatInfo(name, oldheatInfo.HeatName, "达温后延时", newheatInfo.达温后延时, oldheatInfo.达温后延时);
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

        private void uiButton33_Click(object sender, EventArgs e)
        {
            int rows = uiDataGridView4.SelectedIndex;
            if (rows >= 0)
            {
                //string tabtext = uiTabControl11.SelectedTab.Text;

                //if (tabtext == "通讯信息")
                //{
                保存通讯信息();
                //}
                //else if (tabtext == "通讯设置")
                //{
                //    保存Heat盘();
                //}
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
                int line = Sqlite.runSqlNonQuery(SQLiteConnString, $"update TrayInfo set {fiedname} = '{newvalue}' where name = '{trayname}'");
                if (line >= 1)
                {
                    RunLog($"tray盘名称-[{trayname}]-[{fiedname}]{oldvalue}->{newvalue}:更新成功", LogType.ParamLog);
                    return true;
                }
                else RunLog($"tray盘名称-[{trayname}]-[{fiedname}]{oldvalue}->{newvalue}:更新失败", LogType.ParamLog);
            }
            return false;
        }

        public bool UpdateHeatInfo(string name, string trayname, string fiedname, double newvalue, double oldvalue)
        {
            if (newvalue != oldvalue)
            {
                int line = Sqlite.runSqlNonQuery(SQLiteConnString, $"update HeatInfo set {fiedname} = '{newvalue}' where name = '{trayname}'");
                if (line >= 1)
                {
                    RunLog($"加热盘名称-[{trayname}]-[{fiedname}]{oldvalue}->{newvalue}:更新成功", LogType.ParamLog);
                    return true;
                }
                else RunLog($"加热盘名称-[{trayname}]-[{fiedname}]{oldvalue}->{newvalue}:更新失败", LogType.ParamLog);
            }
            return false;
        }

        public bool UpdateHeatInfo(string name, string trayname, string fiedname, string newvalue, string oldvalue)
        {
            if (newvalue != oldvalue)
            {
                int line = Sqlite.runSqlNonQuery(SQLiteConnString, $"update HeatInfo set {fiedname} = '{newvalue}' where name = '{trayname}'");
                if (line >= 1)
                {
                    RunLog($"加热盘名称-[{trayname}]-[{fiedname}]{oldvalue}->{newvalue}:更新成功", LogType.ParamLog);
                    return true;
                }
                else RunLog($"加热盘名称-[{trayname}]-[{fiedname}]{oldvalue}->{newvalue}:更新失败", LogType.ParamLog);
            }
            return false;
        }

        public bool UpdateOffsetPointData(string icname, string otname, string ptname, double newvalue, double oldvalue)
        {
            if (newvalue != oldvalue)
            {
                int line = Sqlite.runSqlNonQuery(SQLiteConnString, $"update OffsetPoint set {ptname} = '{newvalue}' where 微调点位名称 = '{otname}'");
                if (line >= 1)
                {
                    RunLog($"档案名称-[{icname}]-偏移点位名称-[{otname}]-点位-[{ptname}]{oldvalue}->{newvalue}:更新成功", LogType.ParamLog);
                    return true;
                }
                else RunLog($"档案名称-[{icname}]-偏移点位名称-[{otname}]-点位-[{ptname}]{oldvalue}->{newvalue}:更新失败", LogType.ParamLog);
            }
            return false;
        }

        public bool UpdateLight(string lightstate, string fiedname, bool newvalue, bool oldvalue)
        {
            if (newvalue != oldvalue)
            {
                int line = Sqlite.runSqlNonQuery(SQLiteConnString, $"update LightSet set {fiedname} = '{newvalue}' where 设备状态 = '{lightstate}'");
                if (line >= 1)
                {
                    RunLog($"状态灯设置-设备状态-[{lightstate}]-[{fiedname}]{oldvalue}->{newvalue}:更新成功", LogType.ParamLog);
                    return true;
                }
                else RunLog($"状态灯设置-设备状态-[{lightstate}]-[{fiedname}]{oldvalue}->{newvalue}:更新失败", LogType.ParamLog);
            }
            return false;
        }

        public bool UpdateLight(string lightstate, string fiedname, int newvalue, int oldvalue)
        {
            if (newvalue != oldvalue)
            {
                int line = Sqlite.runSqlNonQuery(SQLiteConnString, $"update LightSet set {fiedname} = {newvalue} where 设备状态 = '{lightstate}'");
                if (line >= 1)
                {
                    RunLog($"状态灯设置-设备状态-[{lightstate}]-[{fiedname}]{oldvalue}->{newvalue}:更新成功", LogType.ParamLog);
                    return true;
                }
                else RunLog($"状态灯设置-设备状态-[{lightstate}]-[{fiedname}]{oldvalue}->{newvalue}:更新失败", LogType.ParamLog);
            }
            return false;
        }

        public bool UpdateLight(string lightstate, string fiedname, string newvalue, string oldvalue)
        {
            if (newvalue != oldvalue)
            {
                int line = Sqlite.runSqlNonQuery(SQLiteConnString, $"update LightSet set {fiedname} = '{newvalue}' where 设备状态 = '{lightstate}'");
                if (line >= 1)
                {
                    RunLog($"状态灯设置-设备状态-[{lightstate}]-[{fiedname}]{oldvalue}->{newvalue}:更新成功", LogType.ParamLog);
                    return true;
                }
                else RunLog($"状态灯设置-设备状态-[{lightstate}]-[{fiedname}]{oldvalue}->{newvalue}:更新失败", LogType.ParamLog);
            }
            return false;
        }

        private void Ctl_MouseWheel(object sender, MouseEventArgs e)
        {
            // 将滚轮事件标记为已处理，阻止默认行为
            if (e is HandledMouseEventArgs handledArgs)
            {
                handledArgs.Handled = true;
            }
        }

        private void button227_Click(object sender, EventArgs e)
        {
            string point = textBox25.Text;
            HeatDicPoint[point] = false;
            HeatDicTimer[point].Status = 0;

            //List<bool[]> isopen = new List<bool[]>() { new bool[] { true, true, true, true } , new bool[] { true, true, true, true } };
            //string result = APPmaster.Com.Send(0, isopen, 无测试机模式);
            //Com.ReceiveNoTestTimer(null, null);
            //for (int i = 0; i < 10000; i++)
            //{
            //    RunLog($"吸取模式:{(空跑模式 ? "空跑模式" : "正常模式")}", LogType.RunLog);
            //}
            //RunLog($"测试模式:{(无测试机模式 ? "离线" : "在线")}", LogType.RunLog);
            //RunLog($"开始时间:{开始时间:yyyy-MM-dd HH:mm:ss.fff}", LogType.RunLog);
            //RunLog($"按下结批时间:{按下结批时间:yyyy-MM-dd HH:mm:ss.fff}", LogType.ComLog);
            //RunLog($"结批时间:" + 结批时间.ToString(@"hh\:mm\:ss"), LogType.ComLog);
            //RunLog($"结束时间:{结束时间:yyyy-MM-dd HH:mm:ss.fff}", LogType.ComLog);
            //if (批次运行时间.TotalDays >= 1) RunLog($"运行时间:" + 批次运行时间.ToString(@"dd\.hh\:mm\:ss"), LogType.OperateLog);
            //else RunLog($"运行时间:" + 批次运行时间.ToString(@"hh\:mm\:ss"), LogType.OperateLog);
            //RunLog(($"UPH:" + UPHNum.ToString()), LogType.OperateLog);
            //RunLog($"载入数:{载入数}", LogType.OperateLog);
            //RunLog($"总计:{出料数}", LogType.ParamLog);
            //RunLog($"Auto1:{Auto1数量},良率:{Auto1良率:0.0}%", LogType.ParamLog);
            //RunLog($"Auto2:{Auto2数量},良率:{Auto2良率: 0.0}%", LogType.ParamLog);
            //RunLog($"Fix1:{Fix1数量},良率:{Fix1良率: 0.0}%", LogType.AlarmLog);
            //RunLog($"Fix2:{Fix2数量},良率:{Fix2良率: 0.0}%", LogType.AlarmLog);
            //RunLog($"Fix3:{Fix3数量},良率:{Fix3良率: 0.0}%", LogType.AlarmLog);
            //int trayid = 1;
            //Auto1正在侧敲 = false;
            //Auto2正在侧敲 = true;
            //bool outzenable1 = false || !Auto1正在侧敲;
            //bool outzenable2 = false || !Auto2正在侧敲;
            //if (trayid > 1 || ((trayid == 0 && outzenable1) || (trayid == 1 && outzenable2)))
            //{
            //    Debug.WriteLine("执行");
            //}
            //string sql = $"create table f_{"ZRS202506200955_TE_LIN" + "_" + 开始时间.ToString("yyyyMMdd_HH_mm_ss")}(批次开始日期 nvarchar(50) NULL,获取MAP时间 nvarchar(50) NULL,料号 nvarchar(50) NULL,批号 nvarchar(20) NULL,MAP信息 nvarchar(50) NULL )";
            //string sql2 = $"create table o_{"ZRS202506200955_TE_LIN" + "_" + 开始时间.ToString("yyyyMMdd_HH_mm_ss")}(时间 nvarchar(50) NULL,名称 nvarchar(50) NULL,大小 nvarchar(50) NULL,数据 nvarchar(MAX) NULL)";
            //int line = SqlServer.runSqlNonQuery(SQLServerConnString, sql);
            //int line1 = SqlServer.runSqlNonQuery(SQLServerConnString, sql2);
            //string sql = $"create table f_{LotNo[0] + "_" + 开始时间.ToString("yyyyMMdd_HH_mm_ss")}(批次开始日期 nvarchar(50) NULL,获取MAP时间 nvarchar(50) NULL,料号 nvarchar(50) NULL,批号 nvarchar(20) NULL,MAP信息 nvarchar(50) NULL )";
            //string sql2 = $"create table o_{LotNo[0] + "_" + 开始时间.ToString("yyyyMMdd_HH_mm_ss")}(时间 nvarchar(50) NULL,名称 nvarchar(50) NULL,大小 nvarchar(50) NULL,数据 nvarchar(MAX) NULL)";
            //int line = SqlServer.runSqlNonQuery(SQLServerConnString, sql);
            //int line1 = SqlServer.runSqlNonQuery(SQLServerConnString, sql2);

            //if (line != -1)
            //{
            //    //isstart = false;
            //    Listcode.Add("1400330");
            //    RunLog("[Code]1400330,[Msg]创建MAP信息数据库失败!", LogType.AlarmLog);
            //}

            //if (line1 != -1)
            //{
            //    //isstart = false;
            //    Listcode.Add("1400330");
            //    RunLog("[Code]1400330,[Msg]创建BIN信息数据库失败!", LogType.AlarmLog);
            //}

            //string sql3 = $"insert into f_{LotNo[0] + "_" + 开始时间.ToString("yyyyMMdd_HH_mm_ss")}(批次开始日期,获取MAP时间,料号,批号,MAP信息)" +
            //                    $"values('{开始时间.ToString("D")}','{DateTime.Now.ToString()}','{LotNo[3]}','{LotNo[4]}','123456')";
            //SqlServer.runSqlNonQueryAsync(SQLServerConnString, sql3);
            //doutContinueFail[7][5] = 10;
            //bool isyieldex = false;
            //bool isalarm = 设置信息.测试机信息.Dout连续不良报警;
            //bool isautoclose = 设置信息.测试机信息.Dout连续不良自动关闭;
            //int doutfailnum = 设置信息.测试机信息.Dout连续不良个数;

            //for (int site = 0; site < doutContinueFail.Count; site++) //连续不良率报警
            //{
            //    for (int dut = 0; dut < doutContinueFail[site].Length; dut++)
            //    {
            //        if (doutContinueFail[site][dut] >= doutfailnum)
            //        {
            //            string code = $"6{site + 1:D2}0{dut + 1}14";
            //            if (isalarm) //dout连续不良报警
            //            {
            //                RunLog($"[Code]{code},[Msg]Site{site + 1}-Dut{dut + 1}:连续不良数量:{doutContinueFail[site][dut]}>={doutfailnum}", LogType.AlarmLog);
            //                doutContinueFail[site][dut] = 0; //如果已报警良率异常则清空重新计算
            //                isyieldex = true;
            //                Listcode.Add(code);
            //            }
            //            if (isautoclose) //dout连续不良自动关闭该dout，
            //            {
            //                int dout = site * 8 + dut + 1;
            //                string labelname = $"lbl_dout{dout}";
            //                设置信息.开site信息.SetSiteValue(site, dut, "0");
            //                string result = UpdateSiteStatus(设置信息.开site信息.Site名称, site + 1, 设置信息.开site信息.GetSiteValue(site));
            //                if (result != "Success")
            //                {
            //                    RunLog("Site保存失败!" + result);
            //                }
            //                //result = GetSiteStatus(设置信息.开site信息.Site名称, out SiteStatus siteStatus);
            //                //if (result != "Success")
            //                //{
            //                //    RunLog("Site读取失败!" + result);
            //                //}
            //                //设置信息.开site信息 = siteStatus;
            //                刷新开Site状态();
            //            }
            //        }
            //    }
            //}


            //double 测试_梭车2放_X = 初始点位.测试_梭车2放_X;
            //double 测试_梭车2右放_X = 点位微调.测试_梭车2右放_X;
            //double 测试_梭车1放_X = 初始点位.测试_梭车1放_X;
            //double 测试_梭车1右放_X = 点位微调.测试_梭车1右放_X;
            //double xpos = 飞梭2位置 == 0 ? (初始点位.测试_梭车2放_X + 点位微调.测试_梭车2右放_X) : (初始点位.测试_梭车1放_X + 点位微调.测试_梭车1右放_X);
            //double xpos2 = 飞梭2位置 == 0 ? (初始点位.测试_梭车2放_X + 点位微调.测试_梭车2右放_X) : (初始点位.测试_梭车1放_X + 点位微调.测试_梭车1右放_X);
            //H5509归零 = true;
            //设备状态 = 1;
            //if (!IsPause && (设备状态 == 1 || H5509正在归零))
            //{
            //    if (!安全门禁用)
            //    {
            //        string[] io = new string[4] { "0", "1", "1", "1" };

            //        if (io[0] == "0" || io[1] == "0" || io[2] == "0" || io[3] == "0")
            //        {
            //            if (io[11] == "0")
            //            {
            //                Listcode.Add("9000000");
            //                RunLog("[Code]9000000,[Msg](运行中)前门禁被打开", LogType.AlarmLog);
            //            }
            //            if (io[12] == "0")
            //            {
            //                Listcode.Add("9000010");
            //                RunLog("[Code]9000010,[Msg](运行中)后门禁被打开", LogType.AlarmLog);
            //            }
            //            if (io[13] == "0")
            //            {
            //                Listcode.Add("9000020");
            //                RunLog("[Code]9000020,[Msg](运行中)左门禁被打开", LogType.AlarmLog);
            //            }
            //            if (io[14] == "0")
            //            {
            //                Listcode.Add("9000040");
            //                RunLog("[Code]9000040,[Msg](运行中)右门禁被打开", LogType.AlarmLog);
            //            }
            //            ProgramPause = true;
            //            IsPause = true;
            //        }
            //    }

            //if (!IsPause && 设备状态 == 1)
            //{
            //    if (io[27] == "0")
            //    {
            //        Listcode.Add("1400020"); //系统错误--气压不足
            //        ProgramPause = true;
            //        IsPause = true;
            //    }
            //}
            //}
            //int interval = TrayInterval;
            //string[] logs = new string[1000];
            //for (int i = 0; i < logs.Length; i++)
            //{
            //    Log.AddLog("debug", "run","调试log:" + i);
            //}
            //Log.LogTimer_Tick(null,null);
            ////飞梭前入料IC = new bool[][] { new bool[] { true, true, true, true }, new bool[] { true, true, true, true } };
            //飞梭后入料IC = new bool[][] { new bool[] { false, false, false, false }, new bool[] { true, false, false, false } };
            ////if (入料手臂停止工作标记 && 结批标记)
            //{
            //    bool[] open = new bool[]
            //                    {
            //                        飞梭后入料IC[0][0], 飞梭后入料IC[0][1], 飞梭后入料IC[0][2], 飞梭后入料IC[0][3],
            //                        飞梭后入料IC[1][0], 飞梭后入料IC[1][1], 飞梭后入料IC[1][2], 飞梭后入料IC[1][3]
            //                    };

            //    bool[] open2 = new bool[]
            //    {
            //                        飞梭前入料IC[0][0], 飞梭前入料IC[0][1], 飞梭前入料IC[0][2], 飞梭前入料IC[0][3],
            //                        飞梭前入料IC[1][0], 飞梭前入料IC[1][1], 飞梭前入料IC[1][2], 飞梭前入料IC[1][3]
            //    };

            //    bool[] open3 = new bool[]
            //    {
            //                        放料IC目标值[0][0], 放料IC目标值[0][1], 放料IC目标值[0][2], 放料IC目标值[0][3],
            //                        放料IC目标值[1][0], 放料IC目标值[1][1], 放料IC目标值[1][2], 放料IC目标值[1][3]
            //    };

            //    bool[] open6 = new bool[]
            //    {
            //                        正在吸取后入料[0], 正在吸取后入料[1], 正在吸取后入料[2], 正在吸取后入料[3],
            //                        正在吸取后入料[4], 正在吸取后入料[5], 正在吸取后入料[6], 正在吸取后入料[7]
            //    };

            //    bool[] open7 = new bool[]
            //    {
            //                        正在放置前入料[0][0], 正在放置前入料[0][1], 正在放置前入料[0][2], 正在放置前入料[0][3],
            //                        正在放置前入料[1][0], 正在放置前入料[1][1], 正在放置前入料[1][2], 正在放置前入料[1][3]
            //    };

            //    结批时判断是否关site(open, open2, open3, out bool[] open4, out bool[] open5);

            //    if (open4.Contains(true))
            //    {
            //        for (int i = 0; i < open3.Length; i++)
            //        {
            //            if (!open3[i] && open[i] && !open6[i])
            //            {
            //                open[i] = false;
            //            }
            //        }
            //    }

            //    if (open5.Contains(true))
            //    {
            //        for (int i = 0; i < open3.Length; i++)
            //        {
            //            if (!open3[i] && open2[i] && !open7[i])
            //            {
            //                open2[i] = false;
            //            }
            //        }
            //    }

            //    if (!open.Contains(true) && !open2.Contains(true))
            //    {
            //        RunLog("aaa");
            //        //测试手臂入料停止工作标记 = true;
            //        //PS测试手臂取待测料流程.Processing = false;
            //        //PS测试手臂取待测料流程.StepID = 500;
            //        //break;
            //    }
            //}

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
            //for (int i = 0; i < 1000; i++)
            //{
            //    Listcode.Add(i.ToString());
            //}
        }

        private void button228_Click(object sender, EventArgs e)
        {
            DialogResult dialog = MessageBox.Show("请确认是否写入?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (dialog != DialogResult.OK) return;

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
            DialogResult dialog = MessageBox.Show("请确认是否写入?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (dialog != DialogResult.OK) return;

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
            DialogResult dialog = MessageBox.Show("请确认是否写入?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (dialog != DialogResult.OK) return;

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
            //label355.Visible = uiCheckBox193.Checked;
            uiComboBox2.Enabled = uiCheckBox193.Checked;
        }

        private void uiCheckBox193_Click(object sender, EventArgs e)
        {
            uiCheckBox192.Checked = !uiCheckBox193.Checked;
            //label355.Enabled = uiCheckBox193.Checked;
            uiComboBox2.Enabled = uiCheckBox193.Checked;
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
            //空跑模式 = uiCheckBox203.Checked;
            //if (空跑模式)
            //{
            //    uiCheckBox204.Enabled = true;
            //    uiCheckBox206.Enabled = true;
            //}
            //else
            //{
            //    Tray盘模式 = true; //不是空跑模式时，Tray盘模式默认开启
            //    uiCheckBox204.Checked = true;

            //    无测试机模式 = false; //不是空跑模式时，默认关闭无测试机模式
            //    uiCheckBox206.Checked = false;

            //    uiCheckBox204.Enabled = false;
            //    uiCheckBox206.Enabled = false;
            //}
        }


        private void uiCheckBox190_Click(object sender, EventArgs e) //正常模式 在线模式 + 抓盘 + 吸料
        {
            if (uiCheckBox190.Checked) //正常
            {
                空跑模式 = false; //吸料
                Tray盘模式 = true; //抓盘
                无测试机模式 = false; //在线
                uiCheckBox206.Checked = false;
                uiCheckBox206.Enabled = false;

                uiCheckBox187.Checked = false;
                uiCheckBox188.Checked = false;
                uiCheckBox204.Checked = false;

            }
            else
            {
                uiCheckBox190.Checked = true;
            }
        }

        private void uiCheckBox204_Click(object sender, EventArgs e) //有盘有料模式 正常模式 + 抓盘 + 吸料
        {
            if (uiCheckBox204.Checked) //正常
            {
                空跑模式 = false; //吸料
                Tray盘模式 = true; //抓盘

                无测试机模式 = true; //离线模式
                uiCheckBox206.Checked = true;
                uiCheckBox206.Enabled = true;

                uiCheckBox187.Checked = false;
                uiCheckBox188.Checked = false;
                uiCheckBox190.Checked = false;
            }
            else
            {
                uiCheckBox204.Checked = true;
            }
            //if (设备状态 != 0)
            //{
            //    uiCheckBox204.Checked = Tray盘模式;
            //    return;
            //}
            //Tray盘模式 = uiCheckBox204.Checked;
        }

        private void uiCheckBox187_Click(object sender, EventArgs e) //有盘无料模式 空跑模式 + 抓盘 + 不吸料
        {
            if (uiCheckBox187.Checked) //有盘无料
            {
                空跑模式 = true; //不吸料
                Tray盘模式 = true; //抓盘

                无测试机模式 = true; //离线模式
                uiCheckBox206.Checked = true;
                uiCheckBox206.Enabled = true;

                uiCheckBox188.Checked = false;
                uiCheckBox204.Checked = false;
                uiCheckBox190.Checked = false;
            }
            else uiCheckBox187.Checked = true;
        }

        private void uiCheckBox188_Click(object sender, EventArgs e) //有盘无料模式 空跑模式 + 不抓盘 + 不吸料
        {
            if (uiCheckBox188.Checked) //无盘无料
            {
                空跑模式 = true; //不吸料
                Tray盘模式 = false; //不抓盘

                无测试机模式 = true; //离线模式
                uiCheckBox206.Checked = true;
                uiCheckBox206.Enabled = true;

                uiCheckBox187.Checked = false;
                uiCheckBox204.Checked = false;
                uiCheckBox190.Checked = false;
            }
            else uiCheckBox188.Checked = true;
        }


        private void uiCheckBox205_Click(object sender, EventArgs e)
        {
            安全门禁用 = uiCheckBox205.Checked;
        }

        private void uiCheckBox206_Click(object sender, EventArgs e)
        {
            if (!uiCheckBox190.Checked)
            {
                if (uiCheckBox204.Checked) //有盘有料模式 + 有测试 = 正常模式
                {
                    if (!uiCheckBox206.Checked) //在线测试
                    {
                        无测试机模式 = false;
                        空跑模式 = false;
                        Tray盘模式 = true;
                        uiCheckBox206.Checked = false;
                        uiCheckBox204.Checked = false;
                        uiCheckBox190.Checked = true;
                    }
                }
                无测试机模式 = uiCheckBox206.Checked;
            }
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


        private void 复制档案代码()
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
                    int line = Sqlite.runSqlNonQuery(SQLiteConnString, sql);

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

        //新增档案
        private void uiButton8_Click(object sender, EventArgs e)
        {
            NewSetupName newSetupName = new NewSetupName();
            DialogResult dialog = newSetupName.ShowDialog();
            if (dialog == DialogResult.OK)
            {
                string sql = $"select count(*) from SetupInfo where 档案名称 = '{newSetupName.Name}'";
                string result = Sqlite.runSql(SQLiteConnString, sql, out string value);
                if (result == "Success")
                {
                    if (Convert.ToInt32(value) >= 1) MessageBox.Show("档案已存在!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    else
                    {
                        dialog = MessageBox.Show($"请确认是否新增档案:{newSetupName.Name}", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                        if (dialog == DialogResult.OK)
                        {
                            sql = $"insert into SetupInfo(档案名称,tray盘名称,site名称,加热盘名称,偏移点位名称,测试机名称,通讯名称,分BIN名称,梭车名称,温度模式)" +
                            $"values('{newSetupName.Name}', '默认','{newSetupName.Name}', '默认','{newSetupName.Name}','{newSetupName.Name}','{newSetupName.Name}','{newSetupName.Name}','{newSetupName.Name}', '常温')";
                            int line = Sqlite.runSqlNonQuery(SQLiteConnString, sql);

                            if (line >= 1)
                            {
                                //string site = "{\"Protocol\":\"COM\",\"PortName\":\"COM10\",\"BaudRate\":9600,\"Parity\":0,\"DataBits\":8,\"StopBits\":1}";
                                int line1 = Sqlite.runSqlNonQuery(SQLiteConnString, $"insert into SiteStatus(Site名称)values('{newSetupName.Name}')");
                                int line2 = Sqlite.runSqlNonQuery(SQLiteConnString, $"insert into OffsetPoint(微调点位名称)values('{newSetupName.Name}')");
                                //int line3 = Sqlite.runSqlNonQuery(SQLiteConnString, $"insert into TesterIFParam(测试机名称)values('{newSetupName.Name}')");
                                int line3 = Sqlite.runSqlNonQuery(SQLiteConnString, $"insert into TesterIFParam(测试机名称,Dout连续不良个数,Dout连续不良自动关闭,Dout连续不良报警,Dut累计不良报警个数,Dut累计不良报警使能,总良率报警百分比,总良率报警基数,总良率报警间隔,总良率报警使能,socket使用次数,测试超时时间,timeout超时时间,超时发送timeout,入料手臂吸料超时时间,出料手臂吸料超时时间,测试手臂吸料超时时间,入料手臂吹料时间,出料手臂吹料时间,测试手臂吹料时间,发送开始信号延时,离线模式测试等待时间,入料吸提前开真空,出料吸提前开真空,测试吸提前开真空,Auto侧敲使能,前侧同时敲击,放置多少颗敲击,Auto侧敲次数,侧敲间隔,放料时允许侧敲,侧敲时允许放料,Auto换盘时出料Z回零,测试超时自动关闭Dut,接收超时Bin自动关闭Dut,超时Bin字符串显示)values('{newSetupName.Name}','{20}','false','false','{30}','false','{90}','{100}','{100}','false','{20000}','{600}','{600}','false','{2000}','{2000}','{2000}','{200}','{200}','{200}','{100}','{5}','false','false','false','false','false','{8}','{2}','{100}','true','true','false','false','false','20A')");
                                //int line4 = Sqlite.runSqlNonQuery(SQLiteConnString, $"insert into ComInfoSet(通讯名称,客户,通讯协议,通讯方式,测试机数量,同时测试,测试结果中有对应Site,超时Bin字符串,发送开批信息,Site1,Site2,Site3,Site4,Site5,Site6,Site7,Site8,Site9,Site10,Site11,Site12,Site13,Site14,Site15,Site16)values('{newSetupName.Name}','默认','晶存','COM','{1}','{1}','true','AB','true','{site}','{site}','{site}','{site}','{site}','{site}','{site}','{site}','{site}','{site}','{site}','{site}','{site}','{site}','{site}','{site}')");
                                int line5 = Sqlite.runSqlNonQuery(SQLiteConnString, $"insert into BinParam(分BIN名称,auto1,auto2,fix1,fix2,fix3)values('{newSetupName.Name}','01','02,20A,OTH','03;;','04;;','05;;')");
                                if (line1 >= 1 && line2 >= 1 && line3 >= 1 && line5 >= 1)
                                {
                                    刷新档案列表();
                                    刷新档案选项();
                                    MessageBox.Show("新建成功!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                            else
                            {
                                MessageBox.Show("新建失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
            }
        }

        private void uiButton42_Click(object sender, EventArgs e)
        {
            //是否选中
            int rows = uiDataGridView1.SelectedIndex;
            if (rows >= 0)
            {
                //获取档案信息
                string name = uiDataGridView1.Rows[rows].Cells[1].Value.ToString();
                string result = GetSetupInfo(name, out SetupInfo setupInfo);
                if (result == "Success")
                {
                    //获取复制档案名称
                    CopySetupName copySetupName = new CopySetupName();
                    copySetupName.jName = name;
                    DialogResult dialog = copySetupName.ShowDialog();
                    if (dialog == DialogResult.OK)
                    {
                        //获取档案数量
                        string sql = $"select count(*) from SetupInfo where 档案名称 = '{copySetupName.Name}'";
                        result = Sqlite.runSql(SQLiteConnString, sql, out string value);
                        if (result == "Success")
                        {
                            //检查档案是否不存在
                            if (Convert.ToInt32(value) == 0)
                            {
                                //新增复制档案
                                sql = $"insert into SetupInfo(档案名称,tray盘名称,site名称,加热盘名称,偏移点位名称,测试机名称,通讯名称,分BIN名称,梭车名称,温度模式,Auto仓提示换盘数量)" +
                                $"values('{copySetupName.Name}', '{setupInfo.Tray盘信息.TrayName}','{copySetupName.Name}', '{setupInfo.Heat盘信息.HeatName}'" +
                                $",'{copySetupName.Name}','{copySetupName.Name}','{copySetupName.Name}','{copySetupName.Name}','{copySetupName.Name}', '{setupInfo.温度模式}', '{setupInfo.Auto仓提示换盘数量}')";
                                int line = Sqlite.runSqlNonQuery(SQLiteConnString, sql);
                                if (line >= 1)
                                {
                                    ///点位微调
                                    result = GetOffsetPoint(setupInfo.OffsetPoint.微调点位名称, out OffsetPoint offsetPoint);
                                    if (result != "Success") { MessageBox.Show("获取微调点位信息失败!" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
                                    ///开Site信息
                                    result = GetSiteStatus(setupInfo.开site信息.Site名称, out SiteStatus siteStatus);
                                    if (result != "Success") { MessageBox.Show("获取开site信息失败!" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
                                    //分BIN参数
                                    result = GetBINParam(setupInfo.分BIN信息.分BIN名称, out BinParam binparam);
                                    if (result != "Success") { MessageBox.Show("获取分BIN信息失败!" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
                                    //测试机参数
                                    result = GetTesterIFParam(setupInfo.测试机信息.测试机名称, out TesterIFParam tester);
                                    if (result != "Success") { MessageBox.Show("获取测试机信息失败!" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

                                    //string site = "{\"Protocol\":\"COM\",\"PortName\":\"COM10\",\"BaudRate\":9600,\"Parity\":0,\"DataBits\":8,\"StopBits\":1}";
                                    int line1 = Sqlite.runSqlNonQuery(SQLiteConnString, $"insert into SiteStatus(Site名称,Site1,Site2,Site3,Site4,Site5,Site6,Site7,Site8,Site9,Site10,Site11,Site12,Site13,Site14,Site15,Site16)" +
                                                                    $"values('{copySetupName.Name}','{string.Join("", siteStatus.Site1)}','{string.Join("", siteStatus.Site2)}','{string.Join("", siteStatus.Site3)}','{string.Join("", siteStatus.Site4)}','{string.Join("", siteStatus.Site5)}','{string.Join("", siteStatus.Site6)}','{string.Join("", siteStatus.Site7)}','{string.Join("", siteStatus.Site8)}','{string.Join("", siteStatus.Site9)}','{string.Join("", siteStatus.Site10)}','{string.Join("", siteStatus.Site11)}','{string.Join("", siteStatus.Site12)}','{string.Join("", siteStatus.Site13)}','{string.Join("", siteStatus.Site14)}','{string.Join("", siteStatus.Site15)}','{string.Join("", siteStatus.Site16)}')");
                                    int line2 = Sqlite.runSqlNonQuery(SQLiteConnString, $"insert into OffsetPoint(微调点位名称,入料_Load吸_X,入料_Load吸_Y,入料_Load吸_Z,入料_Load_变距,入料_Heat吸_X,入料_Heat吸_Y,入料_Heat吸_Z,入料_Heat放_X,入料_Heat放_Y,入料_Heat放_Z,入料_Heat_变距,入料_梭车1第1列放_X,入料_梭车1第1列放_Y,入料_梭车1第1列放_Z,入料_梭车1第2列放_X,入料_梭车1第2列放_Y,入料_梭车1第2列放_Z,入料_梭车2第1列放_X,入料_梭车2第1列放_Y,入料_梭车2第1列放_Z,入料_梭车2第2列放_X,入料_梭车2第2列放_Y,入料_梭车2第2列放_Z,入料_梭车_变距,出料_梭车1第1列吸_X,出料_梭车1第1列吸_Y,出料_梭车1第1列吸_Z,出料_梭车1第2列吸_X,出料_梭车1第2列吸_Y,出料_梭车1第2列吸_Z,出料_梭车2第1列吸_X,出料_梭车2第1列吸_Y,出料_梭车2第1列吸_Z,出料_梭车2第2列吸_X,出料_梭车2第2列吸_Y,出料_梭车2第2列吸_Z,出料_梭车_变距,出料_Auto1放_X,出料_Auto1放_Y,出料_Auto1放_Z,出料_Auto2放_X,出料_Auto2放_Y,出料_Auto2放_Z,出料_Fix1放_X,出料_Fix1放_Y,出料_Fix1放_Z,出料_Fix2放_X,出料_Fix2放_Y,出料_Fix2放_Z,出料_Fix3放_X,出料_Fix3放_Y,出料_Fix3放_Z,出料_AF_变距,测试_梭车1左吸_X,测试_梭车1左吸_Z,测试_梭车2左吸_X,测试_梭车2左吸_Z,测试_梭车1右放_X,测试_梭车1右放_Z,测试_梭车2右放_X,测试_梭车2右放_Z,测试_Site1吸_X,测试_Site2吸_X,测试_Site3吸_X,测试_Site4吸_X,测试_Site5吸_X,测试_Site6吸_X,测试_Site7吸_X,测试_Site8吸_X,测试_Site9吸_X,测试_Site10吸_X,测试_Site11吸_X,测试_Site12吸_X,测试_Site13吸_X,测试_Site14吸_X,测试_Site15吸_X,测试_Site16吸_X,测试_Site1吸_Z,测试_Site2吸_Z,测试_Site3吸_Z,测试_Site4吸_Z,测试_Site5吸_Z,测试_Site6吸_Z,测试_Site7吸_Z,测试_Site8吸_Z,测试_Site9吸_Z,测试_Site10吸_Z,测试_Site11吸_Z,测试_Site12吸_Z,测试_Site13吸_Z,测试_Site14吸_Z,测试_Site15吸_Z,测试_Site16吸_Z,测试_Site1放_X,测试_Site2放_X,测试_Site3放_X,测试_Site4放_X,测试_Site5放_X,测试_Site6放_X,测试_Site7放_X,测试_Site8放_X,测试_Site9放_X,测试_Site10放_X,测试_Site11放_X,测试_Site12放_X,测试_Site13放_X,测试_Site14放_X,测试_Site15放_X,测试_Site16放_X,测试_Site1放_Z,测试_Site2放_Z,测试_Site3放_Z,测试_Site4放_Z,测试_Site5放_Z,测试_Site6放_Z,测试_Site7放_Z,测试_Site8放_Z,测试_Site9放_Z,测试_Site10放_Z,测试_Site11放_Z,测试_Site12放_Z,测试_Site13放_Z,测试_Site14放_Z,测试_Site15放_Z,测试_Site16放_Z,梭车2_前_Y,梭车2_后_Y,抓盘_Empty_X,抓盘_Load_X,抓盘_Auto1_X,抓盘_Auto2_X)" +
                                                                    $"values('{copySetupName.Name}','{offsetPoint.入料_Load吸_X}','{offsetPoint.入料_Load吸_Y}','{offsetPoint.入料_Load吸_Z}','{offsetPoint.入料_Load_变距}','{offsetPoint.入料_Heat吸_X}','{offsetPoint.入料_Heat吸_Y}','{offsetPoint.入料_Heat吸_Z}','{offsetPoint.入料_Heat放_X}','{offsetPoint.入料_Heat放_Y}','{offsetPoint.入料_Heat放_Z}','{offsetPoint.入料_Heat_变距}','{offsetPoint.入料_梭车1第1列放_X}','{offsetPoint.入料_梭车1第1列放_Y}','{offsetPoint.入料_梭车1第1列放_Z}','{offsetPoint.入料_梭车1第2列放_X}','{offsetPoint.入料_梭车1第2列放_Y}','{offsetPoint.入料_梭车1第2列放_Z}','{offsetPoint.入料_梭车2第1列放_X}','{offsetPoint.入料_梭车2第1列放_Y}','{offsetPoint.入料_梭车2第1列放_Z}','{offsetPoint.入料_梭车2第2列放_X}','{offsetPoint.入料_梭车2第2列放_Y}','{offsetPoint.入料_梭车2第2列放_Z}','{offsetPoint.入料_梭车_变距}','{offsetPoint.出料_梭车1第1列吸_X}','{offsetPoint.出料_梭车1第1列吸_Y}','{offsetPoint.出料_梭车1第1列吸_Z}','{offsetPoint.出料_梭车1第2列吸_X}','{offsetPoint.出料_梭车1第2列吸_Y}','{offsetPoint.出料_梭车1第2列吸_Z}','{offsetPoint.出料_梭车2第1列吸_X}','{offsetPoint.出料_梭车2第1列吸_Y}','{offsetPoint.出料_梭车2第1列吸_Z}','{offsetPoint.出料_梭车2第2列吸_X}','{offsetPoint.出料_梭车2第2列吸_Y}','{offsetPoint.出料_梭车2第2列吸_Z}','{offsetPoint.出料_梭车_变距}','{offsetPoint.出料_Auto1放_X}','{offsetPoint.出料_Auto1放_Y}','{offsetPoint.出料_Auto1放_Z}','{offsetPoint.出料_Auto2放_X}','{offsetPoint.出料_Auto2放_Y}','{offsetPoint.出料_Auto2放_Z}','{offsetPoint.出料_Fix1放_X}','{offsetPoint.出料_Fix1放_Y}','{offsetPoint.出料_Fix1放_Z}','{offsetPoint.出料_Fix2放_X}','{offsetPoint.出料_Fix2放_Y}','{offsetPoint.出料_Fix2放_Z}','{offsetPoint.出料_Fix3放_X}','{offsetPoint.出料_Fix3放_Y}','{offsetPoint.出料_Fix3放_Z}','{offsetPoint.出料_AF_变距}','{offsetPoint.测试_梭车1左吸_X}','{offsetPoint.测试_梭车1左吸_Z}','{offsetPoint.测试_梭车2左吸_X}','{offsetPoint.测试_梭车2左吸_Z}','{offsetPoint.测试_梭车1右放_X}','{offsetPoint.测试_梭车1右放_Z}','{offsetPoint.测试_梭车2右放_X}','{offsetPoint.测试_梭车2右放_Z}','{offsetPoint.测试_Site1吸_X}','{offsetPoint.测试_Site2吸_X}','{offsetPoint.测试_Site3吸_X}','{offsetPoint.测试_Site4吸_X}','{offsetPoint.测试_Site5吸_X}','{offsetPoint.测试_Site6吸_X}','{offsetPoint.测试_Site7吸_X}','{offsetPoint.测试_Site8吸_X}','{offsetPoint.测试_Site9吸_X}','{offsetPoint.测试_Site10吸_X}','{offsetPoint.测试_Site11吸_X}','{offsetPoint.测试_Site12吸_X}','{offsetPoint.测试_Site13吸_X}','{offsetPoint.测试_Site14吸_X}','{offsetPoint.测试_Site15吸_X}','{offsetPoint.测试_Site16吸_X}','{offsetPoint.测试_Site1吸_Z}','{offsetPoint.测试_Site2吸_Z}','{offsetPoint.测试_Site3吸_Z}','{offsetPoint.测试_Site4吸_Z}','{offsetPoint.测试_Site5吸_Z}','{offsetPoint.测试_Site6吸_Z}','{offsetPoint.测试_Site7吸_Z}','{offsetPoint.测试_Site8吸_Z}','{offsetPoint.测试_Site9吸_Z}','{offsetPoint.测试_Site10吸_Z}','{offsetPoint.测试_Site11吸_Z}','{offsetPoint.测试_Site12吸_Z}','{offsetPoint.测试_Site13吸_Z}','{offsetPoint.测试_Site14吸_Z}','{offsetPoint.测试_Site15吸_Z}','{offsetPoint.测试_Site16吸_Z}','{offsetPoint.测试_Site1放_X}','{offsetPoint.测试_Site2放_X}','{offsetPoint.测试_Site3放_X}','{offsetPoint.测试_Site4放_X}','{offsetPoint.测试_Site5放_X}','{offsetPoint.测试_Site6放_X}','{offsetPoint.测试_Site7放_X}','{offsetPoint.测试_Site8放_X}','{offsetPoint.测试_Site9放_X}','{offsetPoint.测试_Site10放_X}','{offsetPoint.测试_Site11放_X}','{offsetPoint.测试_Site12放_X}','{offsetPoint.测试_Site13放_X}','{offsetPoint.测试_Site14放_X}','{offsetPoint.测试_Site15放_X}','{offsetPoint.测试_Site16放_X}','{offsetPoint.测试_Site1放_Z}','{offsetPoint.测试_Site2放_Z}','{offsetPoint.测试_Site3放_Z}','{offsetPoint.测试_Site4放_Z}','{offsetPoint.测试_Site5放_Z}','{offsetPoint.测试_Site6放_Z}','{offsetPoint.测试_Site7放_Z}','{offsetPoint.测试_Site8放_Z}','{offsetPoint.测试_Site9放_Z}','{offsetPoint.测试_Site10放_Z}','{offsetPoint.测试_Site11放_Z}','{offsetPoint.测试_Site12放_Z}','{offsetPoint.测试_Site13放_Z}','{offsetPoint.测试_Site14放_Z}','{offsetPoint.测试_Site15放_Z}','{offsetPoint.测试_Site16放_Z}','{offsetPoint.梭车2_前_Y}','{offsetPoint.梭车2_后_Y}','{offsetPoint.抓盘_Empty_X}','{offsetPoint.抓盘_Load_X}','{offsetPoint.抓盘_Auto1_X}','{offsetPoint.抓盘_Auto2_X}')");
                                    int line3 = Sqlite.runSqlNonQuery(SQLiteConnString, $"insert into TesterIFParam(测试机名称,Dout连续不良个数,Dout连续不良自动关闭,Dout连续不良报警,Dut累计不良报警个数,Dut累计不良报警使能,总良率报警百分比,总良率报警基数,总良率报警间隔,总良率报警使能,socket使用次数,测试超时时间,timeout超时时间,超时发送timeout,入料手臂吸料超时时间,出料手臂吸料超时时间,测试手臂吸料超时时间,入料手臂吹料时间,出料手臂吹料时间,测试手臂吹料时间,发送开始信号延时,离线模式测试等待时间,入料吸提前开真空,出料吸提前开真空,测试吸提前开真空,Auto侧敲使能,前侧同时敲击,放置多少颗敲击,Auto侧敲次数,侧敲间隔,放料时允许侧敲,侧敲时允许放料,Auto换盘时出料Z回零,通讯名称,测试超时自动关闭Dut,接收超时Bin自动关闭Dut,超时Bin字符串显示,接收测试信号上抬延时,离线模式IC放置位置)" +
                                                                    $"values('{copySetupName.Name}','{tester.Dout连续不良个数}','{tester.Dout连续不良自动关闭}','{tester.Dout连续不良报警}','{tester.Dut累计不良报警个数}','{tester.Dut累计不良报警使能}','{tester.总良率报警百分比}','{tester.总良率报警基数}','{tester.总良率报警间隔}','{tester.总良率报警使能}','{tester.socket使用次数}','{tester.测试超时时间}','{tester.timeout超时时间}','{tester.超时发送timeout}','{tester.入料手臂吸料超时时间}','{tester.出料手臂吸料超时时间}','{tester.测试手臂吸料超时时间}','{tester.入料手臂吹料时间}','{tester.出料手臂吹料时间}','{tester.测试手臂吹料时间}','{tester.发送开始信号延时}','{tester.离线模式测试等待时间}','{tester.入料吸提前开真空}','{tester.出料吸提前开真空}','{tester.测试吸提前开真空}','{tester.Auto侧敲使能}','{tester.前侧同时敲击}','{tester.放置多少颗敲击}','{tester.Auto侧敲次数}','{tester.侧敲间隔}','{tester.放料时允许侧敲}','{tester.侧敲时允许放料}','{tester.Auto换盘时出料Z回零}','{tester.通讯名称}','{tester.测试超时自动关闭Dut}','{tester.接收超时Bin自动关闭Dut}','{tester.超时Bin字符串显示}','{tester.接收测试信号上抬延时}','{tester.离线模式IC放置位置}')");
                                    int line5 = Sqlite.runSqlNonQuery(SQLiteConnString, $"insert into BinParam(分BIN名称,auto1,auto2,fix1,fix2,fix3,fix1num,fix2num,fix3num,color1,color2,color3,color4,color5,name1,name2,name3,name4,name5)" +
                                                                    $"values('{copySetupName.Name}','{binparam.auto1}','{binparam.auto2}','{binparam.fix1}','{binparam.fix2}','{binparam.fix3}','{binparam.fix1num}','{binparam.fix2num}','{binparam.fix3num}','{binparam.color1}','{binparam.color2}','{binparam.color3}','{binparam.color4}','{binparam.color5}','{binparam.name1}','{binparam.name2}','{binparam.name3}','{binparam.name4}','{binparam.name5}')");
                                    if (line1 >= 1 && line2 >= 1 && line3 >= 1 && line5 >= 1)
                                    {
                                        刷新档案列表();
                                        刷新档案选项();
                                    }
                                }
                                else MessageBox.Show("新建档案失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            } 
                            else MessageBox.Show($"档案{copySetupName.Name}已存在!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error); return;
                        }
                        else MessageBox.Show("查询档案是否存在失败!" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    //取消复制
                }
                else MessageBox.Show("获取需复制的档案信息失败!" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else MessageBox.Show("请选择要复制档案!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void uiButton11_Click(object sender, EventArgs e)
        {
            int rows = uiDataGridView1.SelectedIndex;
            if (rows >= 0)
            {
                string name = uiDataGridView1.Rows[rows].Cells[1].Value.ToString();

                if (name == "默认")
                {
                    MessageBox.Show("默认档案不能删除!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (name == 设置信息.档案名称)
                {
                    MessageBox.Show("档案使用中不能删除!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                DialogResult dialog = MessageBox.Show($"请确认是否删除档案:{name}", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (dialog == DialogResult.OK)
                {
                    int line = Sqlite.runSqlNonQuery(SQLiteConnString, $"delete From SetupInfo Where 档案名称 = '{name}'");
                    if (line >= 1)
                    {
                        int line1 = Sqlite.runSqlNonQuery(SQLiteConnString, $"delete From SiteStatus Where Site名称 = '{name}'");
                        int line2 = Sqlite.runSqlNonQuery(SQLiteConnString, $"delete From OffsetPoint Where 微调点位名称 = '{name}'");
                        int line3 = Sqlite.runSqlNonQuery(SQLiteConnString, $"delete From TesterIFParam Where 测试机名称 = '{name}'");
                        int line4 = Sqlite.runSqlNonQuery(SQLiteConnString, $"delete From BinParam Where 分BIN名称 = '{name}'");

                        if (line1 >= 1 && line2 >= 1 && line3 >= 1 && line4 >= 1)
                        {
                            刷新档案列表();
                            刷新档案选项();
                            MessageBox.Show("删除成功!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("删除档案失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else MessageBox.Show("删除档案失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else MessageBox.Show("请选择要删除的档案!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// 新增Tray盘信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param> 
        private void uiButton7_Click_1(object sender, EventArgs e)
        {
            NewTrayInfo newTrayInfo = new NewTrayInfo();
            DialogResult dialog = newTrayInfo.ShowDialog();
            if (dialog == DialogResult.OK)
            {
                TrayInfo trayInfo = newTrayInfo.trayInfo;
                string sql = $"insert into TrayInfo(name,xnum,ynum,xstart,ystart,xpitch,ypitch,width,length,height)" +
                    $"values('{trayInfo.TrayName}', {trayInfo.X_Num}, {trayInfo.Y_Num}, {trayInfo.X_Start}, {trayInfo.Y_Start}, {trayInfo.X_Pitch}, {trayInfo.Y_Pitch}, {trayInfo.Width}, {trayInfo.Length}, {trayInfo.Height})";
                int line = Sqlite.runSqlNonQuery(SQLiteConnString, sql);
                if (line <= 0)
                {
                    RunLog("执行Sql失败:" + sql);
                    MessageBox.Show("新增失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else 刷新Tray盘列表();
            }
        }

        /// <summary>
        /// 新增加热盘信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton24_Click(object sender, EventArgs e)
        {
            NewHeatTrayInfo newHeatTrayInfo = new NewHeatTrayInfo();
            DialogResult dialog = newHeatTrayInfo.ShowDialog();
            if (dialog == DialogResult.OK)
            {
                HeatInfo heatInfo = newHeatTrayInfo.heatInfo;
                string sql = $"insert into HeatInfo(name,xnum,ynum,xstart,ystart,xpitch,ypitch,width,length,height)" +
                    $"values('{heatInfo.HeatName}', {heatInfo.X_Num}, {heatInfo.Y_Num}, {heatInfo.X_Start}, {heatInfo.Y_Start}, {heatInfo.X_Pitch}, {heatInfo.Y_Pitch}, {heatInfo.Width}, {heatInfo.Length}, {heatInfo.Height})";
                int line = Sqlite.runSqlNonQuery(SQLiteConnString, sql);
                if (line <= 0)
                {
                    RunLog("执行Sql失败:" + sql);
                    MessageBox.Show("新增失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else 刷新加热盘列表();
            }
        }

        /// <summary>
        /// 删除Tray盘信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton19_Click(object sender, EventArgs e)
        {
            int rows = uiDataGridView2.SelectedIndex;
            if (rows >= 0)
            {
                string name = uiDataGridView2.Rows[rows].Cells[1].Value.ToString();
                if (name == "默认")
                {
                    MessageBox.Show("默认Tray盘不能删除!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (name == 设置信息.Tray盘信息.TrayName)
                {
                    MessageBox.Show("Tray盘使用中不能删除!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                DialogResult dialog = MessageBox.Show($"请确认是否删除Tray盘:{name}", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (dialog == DialogResult.OK)
                {
                    int line = Sqlite.runSqlNonQuery(SQLiteConnString, $"delete From TrayInfo Where name = '{name}'");
                    if (line >= 1)
                    {
                        刷新Tray盘列表();
                    }
                    else MessageBox.Show("删除Tray盘失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else MessageBox.Show("请选择要删除的Tray盘!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// 删除Heat盘信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton25_Click(object sender, EventArgs e)
        {
            int rows = uiDataGridView3.SelectedIndex;
            if (rows >= 0)
            {
                string name = uiDataGridView3.Rows[rows].Cells[1].Value.ToString();
                if (name == "默认")
                {
                    MessageBox.Show("默认加热盘不能删除!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (name == 设置信息.Heat盘信息.HeatName)
                {
                    MessageBox.Show("加热盘使用中不能删除!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                DialogResult dialog = MessageBox.Show($"请确认是否删除加热盘:{name}", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (dialog == DialogResult.OK)
                {
                    int line = Sqlite.runSqlNonQuery(SQLiteConnString, $"delete From HeatInfo Where name = '{name}'");
                    if (line >= 1)
                    {
                        刷新加热盘列表();
                    }
                    else MessageBox.Show("删除加热盘失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else MessageBox.Show("请选择要删除的加热盘!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void uiCheckBox210_Click(object sender, EventArgs e)
        {
            if (uiCheckBox210.Checked)
            {
                uiCheckBox211.Checked = false;
                uiCheckBox207.Checked = true;
            }
            else
            {
                if (!uiCheckBox211.Checked && !uiCheckBox210.Checked)
                {
                    uiCheckBox207.Checked = false;
                }
            }
            //uiCheckBox211.Checked = !uiCheckBox210.Checked;
        }

        private void uiCheckBox211_Click(object sender, EventArgs e)
        {
            if (uiCheckBox211.Checked)
            {
                uiCheckBox210.Checked = false;
                uiCheckBox207.Checked = true;
            }
            else
            {
                if (!uiCheckBox211.Checked && !uiCheckBox210.Checked)
                {
                    uiCheckBox207.Checked = false;
                }
            }
            //uiCheckBox210.Checked = !uiCheckBox211.Checked;
        }

        private void uiCheckBox213_Click(object sender, EventArgs e)
        {
            if (uiCheckBox213.Checked)
            {
                uiCheckBox212.Checked = false;
                uiCheckBox208.Checked = true;
            }
            else
            {
                if (!uiCheckBox212.Checked && !uiCheckBox213.Checked)
                {
                    uiCheckBox208.Checked = false;
                }
            }
            //uiCheckBox212.Checked = !uiCheckBox213.Checked;
        }

        private void uiCheckBox212_Click(object sender, EventArgs e)
        {
            if (uiCheckBox212.Checked)
            {
                uiCheckBox213.Checked = false;
                uiCheckBox208.Checked = true;
            }
            else
            {
                if (!uiCheckBox212.Checked && !uiCheckBox213.Checked)
                {
                    uiCheckBox208.Checked = false;
                }
            }
            //uiCheckBox213.Checked = !uiCheckBox212.Checked;
        }

        private void uiCheckBox215_Click(object sender, EventArgs e)
        {
            if (uiCheckBox215.Checked)
            {
                uiCheckBox214.Checked = false;
                uiCheckBox209.Checked = true;
            }
            else
            {
                if (!uiCheckBox214.Checked && !uiCheckBox215.Checked)
                {
                    uiCheckBox209.Checked = false;
                }
            }
            //uiCheckBox214.Checked = !uiCheckBox215.Checked;
        }

        private void uiCheckBox214_Click(object sender, EventArgs e)
        {
            if (uiCheckBox214.Checked)
            {
                uiCheckBox215.Checked = false;
                uiCheckBox209.Checked = true;
            }
            else
            {
                if (!uiCheckBox214.Checked && !uiCheckBox215.Checked)
                {
                    uiCheckBox209.Checked = false;
                }
            }
            //uiCheckBox215.Checked = !uiCheckBox214.Checked;
        }

        private void uiComboBox16_SelectedIndexChanged(object sender, EventArgs e)
        {
            刷新灯光状态();
        }

        private void uiButton29_Click(object sender, EventArgs e)
        {
            刷新灯光状态();
        }

        private void uiButton30_Click(object sender, EventArgs e)
        {
            DialogResult dialog = MessageBox.Show("请确认是否保存!", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (dialog == DialogResult.OK)
            {
                int green = 0;
                if (uiCheckBox210.Checked) green = 1;
                else if (uiCheckBox211.Checked) green = 2;

                int yellow = 0;
                if (uiCheckBox213.Checked) yellow = 1;
                else if (uiCheckBox212.Checked) yellow = 2;

                int red = 0;
                if (uiCheckBox215.Checked) red = 1;
                else if (uiCheckBox214.Checked) red = 2;

                LightSet newlightset = new LightSet()
                {
                    状态 = uiComboBox16.Text,
                    绿灯颜色显示 = uiCheckBox207.Checked,
                    绿灯显示方式 = green,
                    黄灯颜色显示 = uiCheckBox208.Checked,
                    黄灯显示方式 = yellow,
                    红灯颜色显示 = uiCheckBox209.Checked,
                    红灯显示方式 = red,
                    音乐提示 = uiComboBox17.Text != "无音乐",
                    报警音乐 = uiComboBox17.Text,
                };

                string result = GetLightSet(newlightset.状态, out LightSet oldLightSet);
                if (result == "Success")
                {
                    UpdateLight(newlightset.状态, "绿灯颜色显示", newlightset.绿灯颜色显示, oldLightSet.绿灯颜色显示);
                    UpdateLight(newlightset.状态, "绿灯显示方式", newlightset.绿灯显示方式, oldLightSet.绿灯显示方式);
                    UpdateLight(newlightset.状态, "黄灯颜色显示", newlightset.黄灯颜色显示, oldLightSet.黄灯颜色显示);
                    UpdateLight(newlightset.状态, "黄灯显示方式", newlightset.黄灯显示方式, oldLightSet.黄灯显示方式);
                    UpdateLight(newlightset.状态, "红灯颜色显示", newlightset.红灯颜色显示, oldLightSet.红灯颜色显示);
                    UpdateLight(newlightset.状态, "红灯显示方式", newlightset.红灯显示方式, oldLightSet.红灯显示方式);
                    UpdateLight(newlightset.状态, "音乐提示", newlightset.音乐提示, oldLightSet.音乐提示);
                    UpdateLight(newlightset.状态, "报警音乐", newlightset.报警音乐, oldLightSet.报警音乐);

                    GetLightSet();
                }
            }
        }

        private void uiCheckBox207_Click(object sender, EventArgs e)
        {
            if (uiCheckBox207.Checked)
            {
                uiCheckBox210.Checked = true;
                uiCheckBox211.Checked = false;
            }
            else
            {
                uiCheckBox210.Checked = false;
                uiCheckBox211.Checked = false;
            }
        }

        private void uiCheckBox208_Click(object sender, EventArgs e)
        {
            if (uiCheckBox208.Checked)
            {
                uiCheckBox213.Checked = true;
                uiCheckBox212.Checked = false;
            }
            else
            {
                uiCheckBox213.Checked = false;
                uiCheckBox212.Checked = false;
            }
        }

        private void uiCheckBox209_Click(object sender, EventArgs e)
        {
            if (uiCheckBox209.Checked)
            {
                uiCheckBox215.Checked = true;
                uiCheckBox214.Checked = false;
            }
            else
            {
                uiCheckBox215.Checked = false;
                uiCheckBox214.Checked = false;
            }
        }

        private void button22_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(uiDoubleUpDown126.Text);
            Task.Run(() =>
            {
                string result = motionEx.AxisAbsoluteMove("M11_Fix3放IC轴", pos, 1);
                if (result != "Success") RunLog("移动失败!" + result);
            });
        }

        private void button243_Click(object sender, EventArgs e)
        {
            所有轴紧急停止();
        }

        private void uiTabControl3_Selecting(object sender, TabControlCancelEventArgs e)
        {
            刷新调试(e.TabPage.Text);
        }

        private void uiButton29_Click_1(object sender, EventArgs e)
        {
            string 设置与维护 = uiComboBox3.Text; //设置与维护
            string offset = uiComboBox4.Text; //offset
            string 速度 = uiComboBox5.Text; //速度
            string IO = uiComboBox6.Text; //IO
            string 日志 = uiComboBox18.Text; //日志
            string 退出 = button_exit.Text; //退出

            string[] controls = new string[] { 设置与维护, offset, 速度, IO, 日志, 退出 };

            //ButtonName = { "button_setting", "button_offset", "button_speed", "button_exit", "button_io", "button_log" };
            try
            {
                DialogResult dialog = MessageBox.Show("请确认是否保存?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (dialog == DialogResult.OK)
                {
                    for (int i = 0; i < ListConPerm.Count; i++)
                    {
                        if (ListConPerm[i].控件名 == "button_setting")
                        {
                            if (ListConPerm[i].权限 != uiComboBox3.Text)
                            {
                                string result = UpateControlPerm("button_setting", uiComboBox3.Text);
                                if (result != "Success")
                                {
                                    MessageBox.Show("修改权限失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                        else if (ListConPerm[i].控件名 == "button_offset")
                        {
                            if (ListConPerm[i].权限 != uiComboBox4.Text)
                            {
                                string result = UpateControlPerm("button_offset", uiComboBox4.Text);
                                if (result != "Success")
                                {
                                    MessageBox.Show("修改权限失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                        else if (ListConPerm[i].控件名 == "button_speed")
                        {
                            if (ListConPerm[i].权限 != uiComboBox5.Text)
                            {
                                string result = UpateControlPerm("button_speed", uiComboBox5.Text);
                                if (result != "Success")
                                {
                                    MessageBox.Show("修改权限失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                        else if (ListConPerm[i].控件名 == "button_io")
                        {
                            if (ListConPerm[i].权限 != uiComboBox6.Text)
                            {
                                string result = UpateControlPerm("button_io", uiComboBox6.Text);
                                if (result != "Success")
                                {
                                    MessageBox.Show("修改权限失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                        else if (ListConPerm[i].控件名 == "button_log")
                        {
                            if (ListConPerm[i].权限 != uiComboBox18.Text)
                            {
                                string result = UpateControlPerm("button_log", uiComboBox18.Text);
                                if (result != "Success")
                                {
                                    MessageBox.Show("修改权限失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                        else if (ListConPerm[i].控件名 == "button_exit")
                        {
                            if (ListConPerm[i].权限 != uiComboBox7.Text)
                            {
                                string result = UpateControlPerm("button_exit", uiComboBox7.Text);
                                if (result != "Success")
                                {
                                    MessageBox.Show("修改权限失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                    GetContorlPerm();
                    刷新按钮状态();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("修改权限异常!" + ex.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void uiButton26_Click(object sender, EventArgs e)
        {
            刷新按钮权限();
        }

        private void button248_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(numericUpDown163.Text);
            Empty顶盘手臂移动初始点位(pos);
        }

        private void button247_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(numericUpDown162.Text);
            Empty顶盘手臂移动初始点位(pos);
        }

        private void button246_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(numericUpDown161.Text);
            Load顶盘手臂移动初始点位(pos);
        }

        private void button251_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(numericUpDown174.Text);
            Auto1顶盘手臂移动初始点位(pos);
        }

        private void button256_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(numericUpDown179.Text);
            Auto1顶盘手臂移动初始点位(pos);
        }

        private void button249_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(numericUpDown159.Text);
            Auto1顶盘手臂移动初始点位(pos);
        }

        private void button250_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(numericUpDown173.Text);
            Auto1顶盘手臂移动初始点位(pos);
        }

        private void button252_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(numericUpDown175.Text);
            Auto2顶盘手臂移动初始点位(pos);
        }

        private void button255_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(numericUpDown178.Text);
            Auto2顶盘手臂移动初始点位(pos);
        }

        private void button254_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(numericUpDown177.Text);
            Auto2顶盘手臂移动初始点位(pos);
        }

        private void button253_Click(object sender, EventArgs e)
        {
            double pos = Convert.ToDouble(numericUpDown176.Text);
            Auto2顶盘手臂移动初始点位(pos);
        }

        private void ChangePwd(string realName)
        {
            //string realName = "Engineer";
            string result = GetPassWord(realName, out string password);
            string result2 = GetPermTime(realName, out string permtime);
            if (result == "Success" && result2 == "Success")
            {
                ChangePwd changePwd = new ChangePwd();
                changePwd.RealName = realName;
                changePwd.OldPassword = password;
                changePwd.PermissoionTime = permtime;
                DialogResult dialog = changePwd.ShowDialog();
                if (dialog == DialogResult.OK)
                {
                    result = UpateJurisdiction(realName, changePwd.NewPassword, changePwd.PermissoionTime);
                    if (result != "Success")
                    {
                        MessageBox.Show($"修改{realName}密码失败!" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        RunLog($"修改{realName}密码失败!" + result);
                    }
                    else
                    {
                        if (当前权限 == realName)
                        {
                            权限时间 = Convert.ToInt32(changePwd.PermissoionTime) * 60;
                        }
                    }
                }
            }
            else
            {
                if (result != "Success") RunLog($"修改{realName}密码异常!" + result);
                if (result != "Success") RunLog($"修改{realName}权限时间异常!" + result);
            }
        }

        private void button257_Click(object sender, EventArgs e)
        {
            ChangePwd("Engineer");
        }

        private void button258_Click(object sender, EventArgs e)
        {
            ChangePwd("Administrator");
        }

        private void button259_Click(object sender, EventArgs e)
        {
            ChangePwd("Supplier");
        }

        private void button260_Click(object sender, EventArgs e)
        {
            string sql = $"create table f_{LotNo[0].Replace("-", "_") + "_" + 开始时间.ToString("yyyyMMdd_HH_mm_ss")}(批次开始日期 nvarchar(50) NULL,获取MAP时间 nvarchar(50) NULL,料号 nvarchar(50) NULL,批号 nvarchar(20) NULL,MAP信息 nvarchar(50) NULL )";
            string sql2 = $"create table o_{LotNo[0].Replace("-", "_") + "_" + 开始时间.ToString("yyyyMMdd_HH_mm_ss")}(时间 nvarchar(50) NULL,名称 nvarchar(50) NULL,大小 nvarchar(50) NULL,数据 nvarchar(MAX) NULL)";
            int line = SqlServer.runSqlNonQuery(SQLServerConnString, sql);
            int line1 = SqlServer.runSqlNonQuery(SQLServerConnString, sql2);

            if (line <= -2)
            {
                RunLog("执行SQl异常：" + sql);
                RunLog("[Code]1400330,[Msg]创建MAP信息数据库失败!", TextFileLogger.LogType.AlarmLog);
            }

            if (line1 <= -2)
            {
                Listcode.Add("1400330");
                RunLog("执行SQl异常：" + sql2);
                RunLog("[Code]1400330,[Msg]创建BIN信息数据库失败!", LogType.AlarmLog);
            }
        }

        private void uiButton30_Click_1(object sender, EventArgs e)
        {
            uiButton30.Enabled = false;
            Task.Run(() =>
            {
                int count = Convert.ToInt32(numericUpDown184.Value); //敲击次数
                int delay = Convert.ToInt32(numericUpDown186.Value); //敲击间隔
                for (int i = 0; i < count; i++)
                {
                    if (checkBox9.Checked) //前侧同时敲击
                    {
                        handler.Auto1_Tray前侧定位气缸原位输出(10);
                        Thread.Sleep(delay);
                        handler.Auto1_Tray前侧定位气缸到位输出(10);
                        Thread.Sleep(delay);
                    }
                    else
                    {
                        handler.Auto1_Tray侧定位气缸原位输出(10);
                        Thread.Sleep(delay);
                        handler.Auto1_Tray侧定位气缸到位输出(10);
                        Thread.Sleep(delay);
                    }
                }
                this.Invoke(new Action(() => { uiButton30.Enabled = true; }));

            });
        }

        private void uiButton31_Click(object sender, EventArgs e)
        {
            uiButton31.Enabled = false;
            Task.Run(() =>
            {
                int count = Convert.ToInt32(numericUpDown184.Value); //敲击次数
                int delay = Convert.ToInt32(numericUpDown186.Value); //敲击间隔     
                for (int i = 0; i < count; i++)
                {
                    if (checkBox9.Checked) //前侧同时敲击
                    {
                        handler.Auto2_Tray前侧定位气缸原位输出(10);
                        Thread.Sleep(delay);
                        handler.Auto2_Tray前侧定位气缸到位输出(10);
                        Thread.Sleep(delay);
                    }
                    else
                    {
                        handler.Auto2_Tray侧定位气缸原位输出(10);
                        Thread.Sleep(delay);
                        handler.Auto2_Tray侧定位气缸到位输出(10);
                        Thread.Sleep(delay);
                    }
                }
                this.Invoke(new Action(() => { uiButton31.Enabled = true; }));
            });
        }

        private void button262_Click(object sender, EventArgs e)
        {
            TimeSpan timeSpan = 获取加热倒数时间();
            Debug.WriteLine(timeSpan.ToString());
            uiTextBox8.Text = timeSpan.ToString();
        }


        public void PrintState()
        {
            try
            {
                RunLog($"设备状态 = {设备状态};");
                RunLog($"空跑模式 = {空跑模式};");
                RunLog($"Tray盘模式 = {Tray盘模式};");
                RunLog($"无测试机模式 = {无测试机模式};");
                RunLog($"温控达温 = {温控达温};");
                RunLog($"初次达温 = {初次达温};");
                RunLog($"H5509归零 = {H5509归零};");
                RunLog($"H5509正在归零 = {H5509正在归零};");
                RunLog($"H5509初始化 = {H5509初始化};");
                RunLog($"H5509正在初始化 = {H5509正在初始化};");
                RunLog($"结批标记 = {结批标记};");
                RunLog($"正在结批 = {正在结批};");
                RunLog($"结批完成 = {结批完成};");
                RunLog($"强制结批标记 = {强制结批标记};");
                RunLog($"一个循环标记 = {一个循环标记};");
                RunLog($"一个循环中 = {一个循环中};");
                RunLog($"一个循环完成 = {一个循环完成};");
                RunLog($"结批选项选择中 = {结批选项选择中};");
                RunLog($"一个循环选项选择中 = {一个循环选项选择中};");
                RunLog($"后方定点停止允许 = {后方定点停止允许};");
                RunLog($"前方定点停止允许 = {前方定点停止允许};");
                RunLog($"开舱门上顶标记 = {开舱门上顶标记};");
                RunLog($"Load舱门上顶标记 = {Load舱门上顶标记};");
                RunLog($"Empty舱门上顶标记 = {Empty舱门上顶标记};");
                RunLog($"Auto1舱门上顶标记 = {Auto1舱门上顶标记};");
                RunLog($"Auto2舱门上顶标记 = {Auto2舱门上顶标记};");
                RunLog($"Load舱门开启标记 = {Load舱门开启标记};");
                RunLog($"Empty舱门开启标记 = {Empty舱门开启标记};");
                RunLog($"Auto1舱门开启标记 = {Auto1舱门开启标记};");
                RunLog($"Auto2舱门开启标记 = {Auto2舱门开启标记};");
                RunLog($"Empty下降回零开关 = {Empty下降回零开关};");
                RunLog($"Load下降回零开关 = {Load下降回零开关};");
                RunLog($"Auto1下降回零开关 = {Auto1下降回零开关};");
                RunLog($"Auto2下降回零开关 = {Auto2下降回零开关};");
                RunLog($"Empty正在下降回零 = {Empty正在下降回零};");
                RunLog($"Load正在下降回零 = {Load正在下降回零};");
                RunLog($"Auto1正在下降回零 = {Auto1正在下降回零};");
                RunLog($"Auto2正在下降回零 = {Auto2正在下降回零};");
                RunLog($"Empty舱门正在上顶 = {Empty舱门正在上顶};");
                RunLog($"Load舱门正在上顶 = {Load舱门正在上顶};");
                RunLog($"Auto1舱门正在上顶 = {Auto1舱门正在上顶};");
                RunLog($"Auto2舱门正在上顶 = {Auto2舱门正在上顶};");
                RunLog($"Load换盘标记 = {Load换盘标记};");
                RunLog($"Auto1换盘标记 = {Auto1换盘标记};");
                RunLog($"Auto2换盘标记 = {Auto2换盘标记};");
                RunLog($"Auto等待换盘标记 = {Auto等待换盘标记};");

                RunLog($"保存Auto1放料个数 = {保存Auto1放料个数};");
                RunLog($"保存Auto2放料个数 = {保存Auto2放料个数};");
                RunLog($"出料正在Auto1放料 = {出料正在Auto1放料};");
                RunLog($"出料正在Auto2放料 = {出料正在Auto2放料};");
                RunLog($"Auto1正在侧敲 = {Auto1正在侧敲};");
                RunLog($"Auto2正在侧敲 = {Auto2正在侧敲};");

                RunLog($"Fix1Tray盘放置标记 = {Fix1Tray盘放置标记};");
                RunLog($"Fix1Tray盘拿走标记 = {Fix1Tray盘拿走标记};");
                RunLog($"Fix2Tray盘放置标记 = {Fix2Tray盘放置标记};");
                RunLog($"Fix2Tray盘拿走标记 = {Fix2Tray盘拿走标记};");
                RunLog($"Fix3Tray盘放置标记 = {Fix3Tray盘放置标记};");
                RunLog($"Fix3Tray盘拿走标记 = {Fix3Tray盘拿走标记};");
                RunLog($"总良率报警间隔计数 = {总良率报警间隔计数};");
                RunLog($"Socket使用次数超限数 = {Socket使用次数超限数};");

                RunLog($"温控器读取Site1_Site8探测温度异常 = {温控器读取Site1_Site8探测温度异常};");
                RunLog($"温控器读取Site9_Site16探测温度异常 = {温控器读取Site9_Site16探测温度异常};");
                RunLog($"温控器读取加热盘探测温度异常 = {温控器读取加热盘探测温度异常};");
                RunLog($"温控器读取Site1_Site8设定温度异常 = {温控器读取Site1_Site8设定温度异常};");
                RunLog($"温控器读取Site9_Site16设定温度异常 = {温控器读取Site9_Site16设定温度异常};");
                RunLog($"温控器读取加热盘设定温度异常 = {温控器读取加热盘设定温度异常};");
                RunLog($"温控器设定Site1_Site8温度异常 = {温控器设定Site1_Site8温度异常};");
                RunLog($"温控器设定Site9_Site16温度异常 = {温控器设定Site9_Site16温度异常};");
                RunLog($"温控器设定加热盘温度异常 = {温控器设定加热盘温度异常};");
                RunLog($"温控器读取_设定温度报警 = {温控器读取_设定温度报警};");

                RunLog($"报警次数 = {报警次数};");
                RunLog($"正在入料的Site = {正在入料的Site};");

                string trayTemp = "";
                for (int i = 0; i < Auto1XPoint.Length; i++)
                {
                    for (int j = 0; j < Auto1YPoint.Length; j++)
                    {
                        trayTemp += Auto1DicBin[$"{i},{j}"] + ",";
                    }
                }
                trayTemp = trayTemp.TrimEnd(',');
                if (trayTemp != "")
                {
                    RunLog($"Auto1TrayMap={trayTemp};");
                }

                trayTemp = "";
                for (int i = 0; i < Auto2XPoint.Length; i++)
                {
                    for (int j = 0; j < Auto2YPoint.Length; j++)
                    {
                        trayTemp += Auto2DicBin[$"{i},{j}"] + ",";
                    }
                }
                trayTemp = trayTemp.TrimEnd(',');
                if (trayTemp != "")
                {
                    RunLog($"Auto2TrayMap={trayTemp};");
                }

                trayTemp = "";
                for (int i = 0; i < Fix1XPoint.Length; i++)
                {
                    for (int j = 0; j < Fix1YPoint.Length; j++)
                    {
                        trayTemp += Fix1DicBin[$"{i},{j}"] + ",";
                    }
                }
                trayTemp = trayTemp.TrimEnd(',');
                if (trayTemp != "")
                {
                    RunLog($"Fix1TrayMap={trayTemp};");
                }

                trayTemp = "";
                for (int i = 0; i < Fix2XPoint.Length; i++)
                {
                    for (int j = 0; j < Fix2YPoint.Length; j++)
                    {
                        trayTemp += Fix2DicBin[$"{i},{j}"] + ",";
                    }
                }
                trayTemp = trayTemp.TrimEnd(',');
                if (trayTemp != "")
                {
                    RunLog($"Fix2TrayMap={trayTemp};");
                }

                trayTemp = "";
                for (int i = 0; i < Fix3XPoint.Length; i++)
                {
                    for (int j = 0; j < Fix3YPoint.Length; j++)
                    {
                        trayTemp += Fix3DicBin[$"{i},{j}"] + ",";
                    }
                }
                trayTemp = trayTemp.TrimEnd(',');
                if (trayTemp != "")
                {
                    RunLog($"Fix3TrayMap={trayTemp};");
                }

                string dout128pass = $"{Dout128PassNumber[0]}";
                for (int i = 1; i < Dout128PassNumber.Length; i++)
                {
                    dout128pass += $",{Dout128PassNumber[i]}";
                }
                RunLog($"Dout128PassNumber={dout128pass};");

                string dout128fail = $"{Dout128FailNumber[0]}";
                for (int i = 1; i < Dout128FailNumber.Length; i++)
                {
                    dout128fail += $",{Dout128FailNumber[i]}";
                }
                RunLog($"Dout128FailNumber={dout128fail};");


                int[] SiteTestState = StationManagers.GetStationStates();// SiteTestState[t] = 3;
                RunLog($"SiteTestState = {SiteTestState[0]},{SiteTestState[1]},{SiteTestState[2]},{SiteTestState[3]},{SiteTestState[4]},{SiteTestState[5]},{SiteTestState[6]},{SiteTestState[7]},{SiteTestState[8]},{SiteTestState[9]},{SiteTestState[10]},{SiteTestState[11]},{SiteTestState[12]},{SiteTestState[13]},{SiteTestState[14]},{SiteTestState[15]};");
                bool[][][] siteTestIC = StationManagers.GetStationICs();
                string[][] siteTestBin = StationManagers.GetStationBins();
                for (int i = 0; i < siteTestIC.Length; i++)
                {
                    //RunLog($"Com.Iswaitout[{i}] = {Com.Iswaitout[i]};");
                    RunLog($"SiteTestIC[{i}] = {siteTestIC[i][0][0]},{siteTestIC[i][0][1]},{siteTestIC[i][0][2]},{siteTestIC[i][0][3]},{siteTestIC[i][1][0]},{siteTestIC[i][1][1]},{siteTestIC[i][1][2]},{siteTestIC[i][1][3]};");
                    RunLog($"doutContinueFail[{i}] = {doutContinueFail[i][0]},{doutContinueFail[i][1]},{doutContinueFail[i][2]},{doutContinueFail[i][3]},{doutContinueFail[i][4]},{doutContinueFail[i][5]},{doutContinueFail[i][6]},{doutContinueFail[i][7]};");
                    RunLog($"单dut总不良计数[{i}] = {单dut总不良计数[i][0]},{单dut总不良计数[i][1]},{单dut总不良计数[i][2]},{单dut总不良计数[i][3]},{单dut总不良计数[i][4]},{单dut总不良计数[i][5]},{单dut总不良计数[i][6]},{单dut总不良计数[i][7]};");
                    RunLog($"单dut总不良累计计数[{i}] = {单dut总不良累计计数[i][0]},{单dut总不良累计计数[i][1]},{单dut总不良累计计数[i][2]},{单dut总不良累计计数[i][3]},{单dut总不良累计计数[i][4]},{单dut总不良累计计数[i][5]},{单dut总不良累计计数[i][6]},{单dut总不良累计计数[i][7]};");
                    RunLog($"SiteTestBin[{i}] = {siteTestBin[i][0]},{siteTestBin[i][1]},{siteTestBin[i][2]},{siteTestBin[i][3]},{siteTestBin[i][4]},{siteTestBin[i][5]},{siteTestBin[i][6]},{siteTestBin[i][7]};");
                }
                RunLog($"入料IC忽略 = {入料IC忽略};");
                RunLog($"入料IC吸不起结批 = {入料IC吸不起结批};");
                RunLog($"无待测盘结批 = {无待测盘结批};");
                RunLog($"出料IC忽略 = {出料IC忽略};");
                RunLog($"测试IC忽略 = {测试IC忽略};");

                //OutSiteQueue.Enqueue((0, new TestDataInfo() { SiteTestBin = new string[] { "01", "02", "20A", "20A", "20A", "20A", "20A", "20A" } }));
                //OutSiteQueue.Enqueue((1, new TestDataInfo() { SiteTestBin = new string[] { "03", "04", "20A", "20A", "20A", "20A", "20A", "20A" } }));
                //OutSiteQueue.Enqueue((2, new TestDataInfo() { SiteTestBin = new string[] { "05", "06", "20A", "20A", "20A", "20A", "20A", "20A" } }));
                //OutSiteQueue.Enqueue((3, new TestDataInfo() { SiteTestBin = new string[] { "07", "08", "20A", "20A", "20A", "20A", "20A", "20A" } }));

                string binsaves = "";
                (int SiteId, TestDataInfo Data)[] copiedArray = OutSiteQueue.ToArray();
                int[] siteids = new int[copiedArray.Length];
                string[][] binSaveQueue = new string[copiedArray.Length][];
                for (int i = 0; i < binSaveQueue.Length; i++)
                {
                    siteids[i] = copiedArray[i].SiteId;
                    binSaveQueue[i] = copiedArray[i].Data.SiteTestBin;
                }
                //string[][] binSaveQueue = DeepCopyQueue(BinSaveQueue);
                if (binSaveQueue.Length > 0)
                {
                    int siteid = siteids[0];
                    string[] binSave = binSaveQueue[0];
                    binsaves += $"Site{siteid + 1}:{binSave[0]},{binSave[1]},{binSave[2]},{binSave[3]},{binSave[4]},{binSave[5]},{binSave[6]},{binSave[7]}";

                    if (binSaveQueue.Length > 1)
                    {
                        for (int i = 1; i < binSaveQueue.Length; i++)
                        {
                            siteid = siteids[i];
                            binSave = binSaveQueue[i];
                            binsaves += $"--Site{siteid + 1}:{binSave[0]},{binSave[1]},{binSave[2]},{binSave[3]},{binSave[4]},{binSave[5]},{binSave[6]},{binSave[7]}";
                        }
                    }
                    RunLog("BinSaveQueue=" + binsaves);
                }
                else RunLog("BinSaveQueue=null");

                binsaves = "";
                var insitequeue = InSiteQueue.ToArray();
                if (insitequeue.Length > 0)
                {
                    binsaves += $"{insitequeue[0]}";
                    if (insitequeue.Length > 1)
                    {
                        for (int i = 1; i < insitequeue.Length; i++)
                        {
                            binsaves += $",{insitequeue[i]}";
                        }
                        RunLog("InSiteQueue=" + binsaves);
                    }
                }
                else RunLog("InSiteQueue=null");

                binsaves = "";
                var outsitequeue = OutSiteQueue.ToArray();
                if (outsitequeue.Length > 0)
                {
                    binsaves += $"{outsitequeue[0]}";
                    if (outsitequeue.Length > 1)
                    {
                        for (int i = 1; i < outsitequeue.Length; i++)
                        {
                            binsaves += $",{outsitequeue[i]}";
                        }
                        RunLog("OutSiteQueue=" + binsaves);
                    }
                }
                else RunLog("OutSiteQueue=null");

                RunLog($"载入数 = {载入数};");
                RunLog($"UphNumber = {UphNumber};");
                RunLog($"Auto1数量 = {Auto1数量};");
                RunLog($"Auto2数量 = {Auto2数量};");
                RunLog($"Fix1数量 = {Fix1数量};");
                RunLog($"Fix2数量 = {Fix2数量};");
                RunLog($"Fix3数量 = {Fix3数量};");
                RunLog($"Other数量 = {OtherNum};");
                RunLog($"空盘上顶到最大值 = {空盘上顶到最大值};");
                RunLog($"空盘下降到最小值 = {空盘下降到最小值};");
                RunLog($"Auto1Tray盘数量 = {Auto1Tray盘数量};");
                RunLog($"Auto2Tray盘数量 = {Auto2Tray盘数量};");
                RunLog($"测试手臂吸不起数量 = {测试手臂吸不起数量};");
                //RunLog($"测试手臂Dut吸不起数量 = {测试区吸不起数量};");
                RunLog($"入料手臂吸料不起数量 = {入料手臂吸料不起数量};");
                RunLog($"出料手臂吸料不起数量  = {出料手臂吸料不起数量};");
                RunLog($"Auto1料仓满盘标记 = {Auto1料仓满盘标记};");
                RunLog($"Auto2料仓满盘标记 = {Auto2料仓满盘标记};");
                RunLog($"Auto1放盘等待标记 = {Auto1放盘等待标记};");
                RunLog($"Auto2放盘等待标记 = {Auto2放盘等待标记};");
                RunLog($"Empty料仓打开标记 = {Empty料仓打开标记};");
                RunLog($"Load料仓打开标记 = {Load料仓打开标记};");
                RunLog($"Auto1料仓打开标记 = {Auto1料仓打开标记};");
                RunLog($"Auto2料仓打开标记 = {Auto2料仓打开标记};");
                RunLog($"入料手臂工作完成标记 = {入料手臂工作完成标记};");
                RunLog($"加热盘停止入料标记 = {加热盘停止入料标记};");
                RunLog($"入料手臂停止工作标记 = {入料手臂停止工作标记};");
                RunLog($"测试手臂入料停止工作标记 = {测试手臂入料停止工作标记};");
                RunLog($"测试手臂出料停止工作标记 = {测试手臂出料停止工作标记};");
                RunLog($"出料手臂出料停止工作标记 = {出料手臂出料停止工作标记};");
                RunLog($"飞梭停止工作 = {飞梭停止工作};");
                RunLog($"出料手臂工作完成标记 = {出料手臂工作完成标记};");
                RunLog($"测试手臂工作完成标记 = {测试手臂工作完成标记};");
                RunLog($"入料手臂抓料等待标记 = {入料手臂抓料等待标记};");
                RunLog($"飞梭移动状态 = {飞梭移动状态};");
                RunLog($"飞梭前入料状态 = {飞梭前入料状态};");
                RunLog($"飞梭前出料状态 = {飞梭前出料状态};");
                RunLog($"飞梭后入料状态 = {飞梭后入料状态};");
                RunLog($"飞梭后出料状态 = {飞梭后出料状态};");
                RunLog($"Empty上顶标记 = {Empty上顶标记};");
                RunLog($"Load上顶标记 = {Load上顶标记};");
                RunLog($"Auto1上顶标记 = {Auto1上顶标记};");
                RunLog($"Auto2上顶标记 = {Auto2上顶标记};");
                RunLog($"Empty下降标记 = {Empty下降标记};");
                RunLog($"Load下降标记 = {Load下降标记};");
                RunLog($"Empty下降回零标记 = {Empty下降回零标记};");
                RunLog($"Load下降回零标记 = {Load下降回零标记};");
                RunLog($"Auto1下降回零标记 = {Auto1下降回零标记};");
                RunLog($"Auto2下降回零标记 = {Auto2下降回零标记};");
                RunLog($"抓盘手臂取Empty空盘标记 = {抓盘手臂取Empty空盘标记};");
                RunLog($"抓盘手臂取Load空盘标记 = {抓盘手臂取Load空盘标记};");
                RunLog($"抓盘手臂给Auto1空盘标记 = {抓盘手臂给Auto1空盘标记};");
                RunLog($"抓盘手臂给Auto2空盘标记 = {抓盘手臂给Auto2空盘标记};");
                RunLog($"抓盘手臂放空盘到Empty标记 = {抓盘手臂放空盘到Empty标记};");
                RunLog($"抓盘手臂放空盘到Auto1标记 = {抓盘手臂放空盘到Auto1标记};");
                RunLog($"抓盘手臂放空盘到Auto2标记 = {抓盘手臂放空盘到Auto2标记};");

                if (飞梭前出料BIN项 != null)
                {
                    if (飞梭前出料BIN项[0] != null)
                    {
                        RunLog($"飞梭前出料BIN项 = {飞梭前出料BIN项[0][0]},{飞梭前出料BIN项[0][1]},{飞梭前出料BIN项[0][2]},{飞梭前出料BIN项[0][3]};");
                    }
                    if (飞梭前出料BIN项[1] != null)
                    {
                        RunLog($"飞梭前出料BIN项 = {飞梭前出料BIN项[1][0]},{飞梭前出料BIN项[1][1]},{飞梭前出料BIN项[1][2]},{飞梭前出料BIN项[1][3]};");
                    }
                }
                if (飞梭后出料BIN项 != null)
                {
                    if (飞梭后出料BIN项[0] != null)
                    {
                        RunLog($"飞梭后出料BIN项 = {飞梭后出料BIN项[0][0]},{飞梭后出料BIN项[0][1]},{飞梭后出料BIN项[0][2]},{飞梭后出料BIN项[0][3]};");
                    }
                    if (飞梭后出料BIN项[1] != null)
                    {
                        RunLog($"飞梭后出料BIN项 = {飞梭后出料BIN项[1][0]},{飞梭后出料BIN项[1][1]},{飞梭后出料BIN项[1][2]},{飞梭后出料BIN项[1][3]};");
                    }
                }

                RunLog($"InNozzleIC = {InNozzleIC};");
                RunLog($"InNozzleICOpen = {InNozzleICOpen[0]},{InNozzleICOpen[1]},{InNozzleICOpen[2]},{InNozzleICOpen[3]};");
                RunLog($"OutNozzleIC = {OutNozzleIC};");
                RunLog($"OutNozzleICOpen = {OutNozzleICOpen[0]},{OutNozzleICOpen[1]},{OutNozzleICOpen[2]},{OutNozzleICOpen[3]};");
                RunLog($"TestNozzleIC = {TestNozzleIC};");
                RunLog($"TestNozzleICOpen = {TestNozzleICOpen[0]},{TestNozzleICOpen[1]},{TestNozzleICOpen[2]},{TestNozzleICOpen[3]},{TestNozzleICOpen[4]},{TestNozzleICOpen[5]},{TestNozzleICOpen[6]},{TestNozzleICOpen[7]};");

                RunLog($"正在吸取后入料 = {正在吸取后入料[0]},{正在吸取后入料[1]},{正在吸取后入料[2]},{正在吸取后入料[3]},{正在吸取后入料[4]},{正在吸取后入料[5]},{正在吸取后入料[6]},{正在吸取后入料[7]}");
                RunLog($"正在放置前入料 = {正在放置前入料[0][0]},{正在放置前入料[0][1]},{正在放置前入料[0][2]},{正在放置前入料[0][3]},{正在放置前入料[1][0]},{正在放置前入料[1][1]},{正在放置前入料[1][2]},{正在放置前入料[1][3]};");
                RunLog($"飞梭前入料IC = {飞梭前入料IC[0][0]},{飞梭前入料IC[0][1]},{飞梭前入料IC[0][2]},{飞梭前入料IC[0][3]},{飞梭前入料IC[1][0]},{飞梭前入料IC[1][1]},{飞梭前入料IC[1][2]},{飞梭前入料IC[1][3]};");
                RunLog($"飞梭前出料IC = {飞梭前出料IC[0][0]},{飞梭前出料IC[0][1]},{飞梭前出料IC[0][2]},{飞梭前出料IC[0][3]},{飞梭前出料IC[1][0]},{飞梭前出料IC[1][1]},{飞梭前出料IC[1][2]},{飞梭前出料IC[1][3]};");
                RunLog($"飞梭后入料IC = {飞梭后入料IC[0][0]},{飞梭后入料IC[0][1]},{飞梭后入料IC[0][2]},{飞梭后入料IC[0][3]},{飞梭后入料IC[1][0]},{飞梭后入料IC[1][1]},{飞梭后入料IC[1][2]},{飞梭后入料IC[1][3]};");
                RunLog($"飞梭后出料IC = {飞梭后出料IC[0][0]},{飞梭后出料IC[0][1]},{飞梭后出料IC[0][2]},{飞梭后出料IC[0][3]},{飞梭后出料IC[1][0]},{飞梭后出料IC[1][1]},{飞梭后出料IC[1][2]},{飞梭后出料IC[1][3]};");
                RunLog($"放料IC目标值 = {放料IC目标值[0][0]},{放料IC目标值[0][1]},{放料IC目标值[0][2]},{放料IC目标值[0][3]},{放料IC目标值[1][0]},{放料IC目标值[1][1]},{放料IC目标值[1][2]},{放料IC目标值[1][3]};");

                RunLog($"PS初始化流程.StepID = {PS初始化流程.StepID};");
                RunLog($"PS初始化流程.Processing = {PS初始化流程.Processing};");
                RunLog($"PS结批流程.StepID = {PS结批流程.StepID};");
                RunLog($"PS结批流程.Processing = {PS结批流程.Processing};");
                RunLog($"PS入料手臂吸放料流程.StepID = {PS入料手臂吸放料流程.StepID};");
                RunLog($"PS入料手臂吸放料流程.Processing = {PS入料手臂吸放料流程.Processing};");
                RunLog($"PS出料手臂吸放料流程.StepID = {PS出料手臂吸放料流程.StepID};");
                RunLog($"PS出料手臂吸放料流程.Processing = {PS出料手臂吸放料流程.Processing};");
                RunLog($"PS入飞梭移动流程.StepID = {PS入飞梭移动流程.StepID};");
                RunLog($"PS入飞梭移动流程.Processing = {PS入飞梭移动流程.Processing};");
                RunLog($"PS测试手臂取待测料流程.StepID = {PS测试手臂取待测料流程.StepID};");
                RunLog($"PS测试手臂取待测料流程.Processing = {PS测试手臂取待测料流程.Processing};");
                RunLog($"PS测试手臂取已测料流程.StepID = {PS测试手臂取已测料流程.StepID};");
                RunLog($"PS测试手臂取已测料流程.Processing = {PS测试手臂取已测料流程.Processing};");
                RunLog($"PS测试区气缸下压动作流程.StepID = {PS测试区气缸下压动作流程.StepID};");
                RunLog($"PS测试区气缸下压动作流程.Processing = {PS测试区气缸下压动作流程.Processing};");
                RunLog($"PS测试区气缸上抬动作流程.StepID = {PS测试区气缸上抬动作流程.StepID};");
                RunLog($"PS测试区气缸上抬动作流程.Processing = {PS测试区气缸上抬动作流程.Processing};");
                RunLog($"PS空盘上顶流程.StepID = {PS空盘上顶流程.StepID};");
                RunLog($"PS空盘上顶流程.Processing = {PS空盘上顶流程.Processing};");
                RunLog($"PS空盘下降流程.StepID = {PS空盘下降流程.StepID};");
                RunLog($"PS空盘下降流程.Processing = {PS空盘下降流程.Processing};");
                RunLog($"PS空盘下降回零流程.StepID = {PS空盘下降回零流程.StepID};");
                RunLog($"PS空盘下降回零流程.Processing = {PS空盘下降回零流程.Processing};");
                RunLog($"PS待测盘上顶流程.StepID = {PS待测盘上顶流程.StepID};");
                RunLog($"PS待测盘上顶流程.Processing = {PS待测盘上顶流程.Processing};");
                RunLog($"PS待测盘下降回零流程.StepID = {PS待测盘下降回零流程.StepID};");
                RunLog($"PS待测盘下降回零流程.Processing = {PS待测盘下降回零流程.Processing};");
                RunLog($"PS手动盘1上顶流程.StepID = {PS手动盘1上顶流程.StepID};");
                RunLog($"PS手动盘1上顶流程.Processing = {PS手动盘1上顶流程.Processing};");
                RunLog($"PS手动盘1下降回零流程.StepID = {PS手动盘1下降回零流程.StepID};");
                RunLog($"PS手动盘1下降回零流程.Processing = {PS手动盘1下降回零流程.Processing};");
                RunLog($"PS手动盘2上顶流程.StepID = {PS手动盘2上顶流程.StepID};");
                RunLog($"PS手动盘2上顶流程.Processing = {PS手动盘2上顶流程.Processing};");
                RunLog($"PS手动盘2下降回零流程.StepID = {PS手动盘2下降回零流程.StepID};");
                RunLog($"PS手动盘2下降回零流程.Processing = {PS手动盘2下降回零流程.Processing};");
                RunLog($"PS抓盘手臂取Empty空盘流程.StepID = {PS抓盘手臂取Empty空盘流程.StepID};");
                RunLog($"PS抓盘手臂取Empty空盘流程.Processing = {PS抓盘手臂取Empty空盘流程.Processing};");
                RunLog($"PS抓盘手臂取Load空盘流程.StepID = {PS抓盘手臂取Load空盘流程.StepID};");
                RunLog($"PS抓盘手臂取Load空盘流程.Processing = {PS抓盘手臂取Load空盘流程.Processing};");
                RunLog($"PS抓盘手臂放空盘到Empty流程.StepID = {PS抓盘手臂放空盘到Empty流程.StepID};");
                RunLog($"PS抓盘手臂放空盘到Empty流程.Processing = {PS抓盘手臂放空盘到Empty流程.Processing};");
                RunLog($"PS抓盘手臂放空盘到Auto1流程.StepID = {PS抓盘手臂放空盘到Auto1流程.StepID};");
                RunLog($"PS抓盘手臂放空盘到Auto1流程.Processing ={PS抓盘手臂放空盘到Auto1流程.Processing};");
                RunLog($"PS抓盘手臂放空盘到Auto2流程.StepID = {PS抓盘手臂放空盘到Auto2流程.StepID};");
                RunLog($"PS抓盘手臂放空盘到Auto2流程.Processing={PS抓盘手臂放空盘到Auto2流程.Processing};");
            }
            catch (Exception ex)
            {
                MessageBox.Show("打印失败!" + ex.Message);
            }
        }
        public string[][] DeepCopyQueue(ConcurrentQueue<string[]> queue)
        {
            return queue
                .Select(originalArray =>
                {
                    // 创建新数组
                    string[] newArray = new string[originalArray.Length];

                    // 复制元素
                    Array.Copy(originalArray, newArray, originalArray.Length);

                    return newArray;
                })
                .ToArray();
        }
        private void button263_Click(object sender, EventArgs e)
        {
            PrintState();
        }

        private void button264_Click(object sender, EventArgs e)
        {
            bool[] bools = StationManagers.CheckComConnect();

            //string result = "Success";

            //for (int i = 0; i < 16; i++)
            //{
            //    int site = i;
            //    List<bool[]> open = new List<bool[]>();
            //    StationManagers.SetStationState(i, 1); //  SiteTestState[6] = 1;
            //    //StationManagers.SetStationState(7, 1); // SiteTestState[7] = 1;
            //    List<List<bool[]>> opens = new List<List<bool[]>>();
            //    if (设置信息.通讯信息.同时测试 == 2)
            //    {
            //        int[] sites = GetSites(site);
            //        bool[] site1 = GetSiteOpen((int)sites[0]);
            //        bool[] site2 = GetSiteOpen((int)sites[1]);
            //        int[] states = new int[] { StationManagers.GetStationState(sites[0]), StationManagers.GetStationState(sites[1]) };
            //        if ((states[0] == 1 || !site1.Contains(true))
            //            && (states[1] == 1 || !site2.Contains(true)))
            //        {
            //            opens = new List<List<bool[]>>() { GetOpens(site1) , GetOpens(site2) };
            //            //result = APPmaster.Com.Send(sites, opens, 无测试机模式);
            //            result = StationManagers.Send(sites, states, opens, 无测试机模式);
            //        }
            //    }
            //    else result = StationManagers.Send(site, open, 无测试机模式);
            //}

            //for (int i = 0; i < 16; i++)
            //{
            //    //StationManagers.Connect(i);
            //    //StationManagers.GetStation(i).SendAsync("ZRS240929001A:BS_TEST:HM:RS70B64G4M08G:E9002B:AE35:FT3_RS70B64G4M08G_032003003_032003003::3040:::FT::R80544:FT3:FT:25");//我为宇宙，woweiyuzhou, I am Yuzhou
            //    //Thread.Sleep(5);

            //    List<bool[]> open = new List<bool[]>()
            //    {
            //        new bool[] { true, true, true, true },
            //        new bool[] { true, true, true, true },
            //    };
            //    StationManagers.SetStationState(i, 2);
            //    StationManagers.Send(i, open, false);//.SendAsync("@START_TEST 00 11111111+");//我为宇宙，woweiyuzhou, I am Yuzhou
            //}
            //short site = 0;
            //string result = ""; //SiteTestState[6] = 1; SiteTestState[7] = 1;
            //List<bool[]> open = new List<bool[]>() { new bool[] { true, true, true, true }, new bool[] { true, true, true, true } };
            //List<List<bool[]>> opens = new List<List<bool[]>>();
            //PS测试区气缸下压动作流程.StepName = $"发送测试信号";
            //RunLog($"PS测试区气缸下压动作流程.StepID={PS测试区气缸下压动作流程.StepID}->{PS测试区气缸下压动作流程.StepName}");
            //if (true)
            //{
            //    result = "Success";
            //    short[] sites = GetSites(site);
            //    if ((SiteTestState[sites[0]] == 1 || !GetSiteOpen((int)sites[0]).Contains(true))
            //        && SiteTestState[sites[1]] == 1 || !GetSiteOpen((int)sites[1]).Contains(true))
            //    {
            //        opens = new List<List<bool[]>>() { GetOpens(SiteTestIC[sites[0]]), GetOpens(SiteTestIC[sites[1]]) };
            //        result = APPmaster.Com.Send(sites, opens, 无测试机模式);
            //    }
            //}
            //else result = APPmaster.Com.Send(site, open, 无测试机模式);

            //if (result == "Success")
            //{
            //    SiteTestState[site] = 2;
            //    PS测试区气缸下压动作流程.StepID = 599;
            //}
            //else
            //{
            //    Listcode.Add("800007");
            //    RunLog($"[Code]800007,[Msg]发送Site{site + 1}测试信号失败!" + result, LogType.AlarmLog);
            //    ProgramPause = true;
            //    IsPause = true;
            //}

            //short[] site = new short[] { 1, 2 };
            //List<List<bool[]>> boosls = new List<List<bool[]>>()
            //{
            //    new List<bool[]>()
            //    {
            //        new bool[] { true, true, true, true },
            //        new bool[] { true, true, true, true }
            //    } ,
            //    new List<bool[]>()
            //    {
            //        new bool[] { true, true, true, true },
            //        new bool[] { true, true, true, true }
            //    }
            //};
            //Com.Send(site, boosls, true);

            //string sendstr = Com.getstartest(site, boosls);
            //Debug.WriteLine(sendstr);

            //PS入料手臂吸放料流程.StepName = "计算放料IC颗数";

            //short colums = 0;
            //bool[] open = { true, true, true, true };
            //入料手臂抓料等待标记 = true;
            //Auto1换盘标记 = false;
            //Auto2换盘标记 = false;

            //if (入料手臂抓料等待标记)
            //{
            //    if (Auto1换盘标记 || Auto2换盘标记) //2025-07-19 10:39 新bug
            //    {
            //        RunLog($"PS入料手臂吸放料流程.StepID={PS入料手臂吸放料流程.StepID}->避让Auto1换盘({Auto1换盘标记})或Auto2换盘({Auto2换盘标记})");
            //        PS入料手臂吸放料流程.StepID = 905;
            //        PS入料手臂吸放料流程StepID = 505;
            //        PS入料手臂吸放料流程Processing = true;
            //       // break;
            //    }
            //}

            //if (飞梭前入料放料参数(out colums, out open))
            //{
            //    if (!入料手臂抓料等待标记)
            //    {
            //        if (温度模式 == "高温")
            //        {
            //        }
            //        else
            //        {
            //            //opens = 吸嘴是否吸料(open);
            //            正在放置前入料[colums] = new bool[] { open[0], open[1], open[2], open[3] };
            //            入料手臂抓料等待标记 = false;
            //            PS入料手臂吸放料流程.StepID = 515;
            //        }
            //    }
            //    else
            //    {
            //        入料手臂抓料等待标记 = false; //验证结果
            //        PS入料手臂吸放料流程.StepID = 705;
            //    }
            //}
            //else
            //{
            //    if (!入料手臂抓料等待标记)
            //    {
            //        if (入料手臂工作完成标记 == false)
            //        {
            //            入料手臂工作完成标记 = true; //入料手臂目标达成
            //            PS入料手臂吸放料流程.StepID = 500;
            //            PS入料手臂吸放料流程.Processing = false;
            //        }
            //        else PS入料手臂吸放料流程.StepID = 510;
            //    }
            //    else Thread.Sleep(10);
            //}
            // break;
            //TimeSpan timeSpan = DateTime.Now - 开始时间;
            //if (timeSpan.TotalSeconds > 加热盘.升温超时报警)
            //{
            //    Listcode.Add($"1400446");
            //    RunLog($"[Code]1400446,[Msg]升温超时!", LogType.AlarmLog);
            //    ProgramPause = true; //如果升温超时则触发停止
            //    IsPause = true;
            //}
            //List<bool[]> open = new List<bool[]>()
            //                {
            //                    new bool[] { true, true, true, true },
            //                    new bool[] { true , true, true, true  },
            //                };
            //string result = APPmaster.Com.Send(0, open, 无测试机模式);
            //if (result == "Success")
            //{
            //    SiteTestState[0] = 2;
            //    PS测试区气缸下压动作流程.StepID = 599;
            //}
            //else
            //{
            //    Listcode.Add("800007");
            //    RunLog($"[Code]800007,[Msg]发送Site{0 + 1}测试信号失败!" + result, LogType.AlarmLog);
            //    ProgramPause = true;
            //    IsPause = true;
            //}
            //string result = handler.整机回零流程();
            //if (result == "Success")
            //{
            //    H5509归零 = true;
            //    Listcode.Add("1400444");
            //    RunLog("[Code]1400444,[Msg]归零完成", LogType.AlarmLog);
            //    RunLog("归零完成", LogType.OperateLog);
            //}
            //else
            //{
            //    Listcode.Add("1400420");
            //    RunLog("[Code]1400420,[Msg]归零失败!" + result, LogType.AlarmLog);
            //    全部回零异常Error = result;
            //    RunLog("归零失败", LogType.OperateLog);
            //}
            //Auto1数量 = 8162;
            //Fix2数量 = 4;
        }

        private void button265_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("是否确认取消暂停？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                IsPause = false;
            }
        }

        private void button266_Click(object sender, EventArgs e)
        {
            string result = GetconfigData();
            if (result != "Success")
            {
                MessageBox.Show("刷新失败!" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ///IO信息
            result = GetIOInfo();
            if (result != "Success")
            {
                MessageBox.Show("刷新失败!" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ///轴信息
            result = GetAxisInfo();
            if (result != "Success")
            {
                MessageBox.Show("刷新失败!" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ///轴运行信息
            result = GetAxisRunInfo();
            if (result != "Success")
            {
                MessageBox.Show("刷新失败!" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ///轴回零信息
            result = GetAxisHomeInfo();
            if (result != "Success")
            {
                MessageBox.Show("刷新失败!" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        #endregion

        #region 通讯协议

        private void 刷新通讯协议()
        {
            //tabPage26.Parent = uiCheckBox162.Checked ? uiTabControl10 : null;
            //tabPage30.Parent = uiCheckBox163.Checked ? uiTabControl10 : null;
            //tabPage31.Parent = uiCheckBox164.Checked ? uiTabControl10 : null;
            //tabPage41.Parent = uiCheckBox165.Checked ? uiTabControl10 : null;

            //uiTabControl10.Refresh();
        }
        private void 清空通讯协议()
        {
            //tabPage19.Parent = null;
            //tabPage20.Parent = null;
            //tabPage21.Parent = null;
            //tabPage22.Parent = null;

            //uiTabControl10.Refresh();
        }

        private void 显示通讯协议()
        {
            //if (uiCheckBox162.Checked) tabPage26.Text = uiCheckBox162.Text;
            //else if (uiCheckBox163.Checked) tabPage26.Text = uiCheckBox163.Text;
            //else if (uiCheckBox164.Checked) tabPage26.Text = uiCheckBox164.Text;
            //else if (uiCheckBox165.Checked) tabPage26.Text = uiCheckBox165.Text;
        }

        private void uiCheckBox173_Click(object sender, EventArgs e)
        {
            if (uiCheckBox173.Checked)
            {
                uiCheckBox170.Checked = true;
                uiCheckBox170_Click(null, null);
                uiCheckBox172.Checked = true;
                uiCheckBox179.Checked = true;

                uiCheckBox191.Checked = false;
                uiCheckBox216.Checked = false;
                uiCheckBox162.Checked = false;
                uiCheckBox163.Checked = false;
                uiCheckBox164.Checked = false;
                uiCheckBox165.Checked = false;
                //显示通讯协议();
            }
            else uiCheckBox173.Checked = true;
        }

        private void uiCheckBox162_Click(object sender, EventArgs e)
        {
            if (uiCheckBox162.Checked)
            {
                uiCheckBox170.Checked = true;
                uiCheckBox170_Click(null, null);
                uiCheckBox172.Checked = true;
                uiCheckBox179.Checked = true;

                uiCheckBox191.Checked = false;
                uiCheckBox216.Checked = false;
                uiCheckBox173.Checked = false;
                uiCheckBox163.Checked = false;
                uiCheckBox164.Checked = false;
                uiCheckBox165.Checked = false;
                //显示通讯协议();
            }
            else uiCheckBox162.Checked = true;
            //清空通讯方式();
            //刷新通讯协议();
            //刷新通讯方式();
        }

        private void uiCheckBox163_Click(object sender, EventArgs e)
        {
            if (uiCheckBox163.Checked)
            {
                uiCheckBox171.Checked = true;
                uiCheckBox171_Click(null, null);
                uiCheckBox172.Checked = false;
                uiCheckBox179.Checked = false;

                uiCheckBox191.Checked = false;
                uiCheckBox216.Checked = false;
                uiCheckBox173.Checked = false;
                uiCheckBox162.Checked = false;
                uiCheckBox164.Checked = false;
                uiCheckBox165.Checked = false;
                //    显示通讯协议();
            }
            else uiCheckBox163.Checked = true;
            //清空通讯方式();
            //刷新通讯协议();
            //刷新通讯方式();
        }

        private void uiCheckBox164_Click(object sender, EventArgs e)
        {
            if (uiCheckBox164.Checked)
            {
                uiCheckBox183.Checked = true;
                uiCheckBox183_Click(null, null);
                uiCheckBox172.Checked = false;
                uiCheckBox179.Checked = false;

                uiCheckBox191.Checked = false;
                uiCheckBox216.Checked = false;
                uiCheckBox173.Checked = false;
                uiCheckBox162.Checked = false;
                uiCheckBox163.Checked = false;
                uiCheckBox165.Checked = false;
                //显示通讯协议();
            }
            else uiCheckBox164.Checked = true;
            //清空通讯方式();
            //刷新通讯协议();
            //刷新通讯方式();
        }

        private void uiCheckBox165_Click(object sender, EventArgs e)
        {
            if (uiCheckBox165.Checked)
            {
                uiCheckBox191.Checked = false;
                uiCheckBox216.Checked = false;
                uiCheckBox173.Checked = false;
                uiCheckBox162.Checked = false;
                uiCheckBox163.Checked = false;
                uiCheckBox164.Checked = false;
                //显示通讯协议();
            }
            else uiCheckBox165.Checked = true;
            //清空通讯方式();
            //刷新通讯协议();
            //刷新通讯方式();
        }

        private void uiCheckBox191_Click(object sender, EventArgs e)
        {
            if (uiCheckBox191.Checked)
            {
                uiCheckBox170.Checked = true;
                uiCheckBox170_Click(null, null);
                uiCheckBox172.Checked = true;
                uiCheckBox179.Checked = false;

                uiCheckBox216.Checked = false;
                uiCheckBox165.Checked = false;
                uiCheckBox173.Checked = false;
                uiCheckBox162.Checked = false;
                uiCheckBox163.Checked = false;
                uiCheckBox164.Checked = false;
                //显示通讯协议();
            }
            else uiCheckBox191.Checked = true;
        }

        private void uiCheckBox216_Click(object sender, EventArgs e)
        {
            if (uiCheckBox216.Checked)
            {
                uiCheckBox191.Checked = false;
                uiCheckBox165.Checked = false;
                uiCheckBox173.Checked = false;
                uiCheckBox162.Checked = false;
                uiCheckBox163.Checked = false;
                uiCheckBox164.Checked = false;
                //显示通讯协议();
            }
            else uiCheckBox216.Checked = true;
        }

        #endregion

        #region 通讯方式

        private void 刷新通讯方式()
        {
            //tabPage19.Parent = uiCheckBox166.Checked ? uiTabControl10 : null;
            //tabPage20.Parent = uiCheckBox167.Checked ? uiTabControl10 : null;
            //tabPage21.Parent = uiCheckBox168.Checked ? uiTabControl10 : null;
            //tabPage22.Parent = uiCheckBox169.Checked ? uiTabControl10 : null;
        }

        private void 刷新通讯方式(TabPage tabPage)
        {
            刷新通讯方式();

            //uiTabControl10.SelectedTab = tabPage;

            //uiTabControl10.Refresh();
        }

        private void 清空通讯方式()
        {
            //tabPage19.Parent = null;
            //tabPage20.Parent = null;
            //tabPage21.Parent = null;
            //tabPage22.Parent = null;

            //uiTabControl10.Refresh();
        }

        private void uiCheckBox166_Click(object sender, EventArgs e)
        {
            if (uiCheckBox166.Checked)
            {
                uiCheckBox167.Checked = false;
                uiCheckBox168.Checked = false;
                uiCheckBox169.Checked = false;
            }
            else uiCheckBox166.Checked = true;
            //清空通讯协议();
            //刷新通讯方式(tabPage19);
            //刷新通讯协议();
            刷新测试机数量显示();
        }

        private void uiCheckBox167_Click(object sender, EventArgs e)
        {
            if (uiCheckBox167.Checked)
            {
                uiCheckBox166.Checked = false;
                uiCheckBox168.Checked = false;
                uiCheckBox169.Checked = false;
            }
            else uiCheckBox167.Checked = true;
            //清空通讯协议();
            //刷新通讯方式(tabPage20);
            //刷新通讯协议();
            刷新测试机数量显示();
        }

        private void uiCheckBox168_Click(object sender, EventArgs e)
        {
            if (uiCheckBox168.Checked)
            {
                uiCheckBox166.Checked = false;
                uiCheckBox167.Checked = false;
                uiCheckBox169.Checked = false;
            }
            else uiCheckBox168.Checked = true;
            //清空通讯协议();
            //刷新通讯方式(tabPage21);
            //刷新通讯协议();
            刷新测试机数量显示();
        }

        private void uiCheckBox169_Click(object sender, EventArgs e)
        {
            if (uiCheckBox169.Checked)
            {
                uiCheckBox166.Checked = false;
                uiCheckBox167.Checked = false;
                uiCheckBox168.Checked = false;
            }
            else uiCheckBox169.Checked = true;
            //清空通讯协议();
            //刷新通讯方式(tabPage22);
            //刷新通讯协议();
            刷新测试机数量显示();
        }

        #endregion

        #region 测试机数量

        private void 刷新测试机数量显示()
        {
            string testType = uiCheckBox166.Checked ? "COM" : uiCheckBox167.Checked ? "TCP" : uiCheckBox168.Checked ? "LAN" : uiCheckBox169.Checked ? "GPIB" : "COM";
            int testNum = uiCheckBox158.Checked ? 1 : uiCheckBox159.Checked ? 4 : uiCheckBox161.Checked ? 8 : uiCheckBox160.Checked ? 16 : 1;

            bool showport = testType == "TCP" || testType == "LAN";

            uiComboBox39.Enabled = true;
            uiComboBox38.Enabled = testNum == 16;
            uiComboBox37.Enabled = testNum == 8 || testNum == 16;
            uiComboBox36.Enabled = testNum == 16;
            uiComboBox35.Enabled = testNum != 1;
            uiComboBox34.Enabled = testNum == 16;
            uiComboBox33.Enabled = testNum == 8 || testNum == 16;
            uiComboBox32.Enabled = testNum == 16;
            uiComboBox47.Enabled = testNum != 1;
            uiComboBox46.Enabled = testNum == 16;
            uiComboBox45.Enabled = testNum == 8 || testNum == 16;
            uiComboBox44.Enabled = testNum == 16;
            uiComboBox43.Enabled = testNum != 1;
            uiComboBox42.Enabled = testNum == 16;
            uiComboBox41.Enabled = testNum == 8 || testNum == 16;
            uiComboBox40.Enabled = testNum == 16;

            uiTextBox9.Enabled = showport;
            uiTextBox10.Enabled = showport && testNum == 16;
            uiTextBox12.Enabled = showport && (testNum == 8 || testNum == 16);
            uiTextBox11.Enabled = showport && testNum == 16;
            uiTextBox16.Enabled = showport && testNum != 1;
            uiTextBox14.Enabled = showport && testNum == 16;
            uiTextBox15.Enabled = showport && (testNum == 8 || testNum == 16);
            uiTextBox13.Enabled = showport && testNum == 16;
            uiTextBox24.Enabled = showport && testNum != 1;
            uiTextBox20.Enabled = showport && testNum == 16;
            uiTextBox22.Enabled = showport && (testNum == 8 || testNum == 16);
            uiTextBox18.Enabled = showport && testNum == 16;
            uiTextBox23.Enabled = showport && testNum != 1;
            uiTextBox19.Enabled = showport && testNum == 16;
            uiTextBox21.Enabled = showport && (testNum == 8 || testNum == 16);
            uiTextBox17.Enabled = showport && testNum == 16;

            uiComboBox50.Enabled = testType == "COM";
            uiComboBox51.Enabled = testType == "COM";
            uiComboBox49.Enabled = testType == "COM";
            uiComboBox48.Enabled = testType == "COM";

            //if (testNum == 1)
            //{
            //uiComboBox39.Enabled = testNum == 1 ||testNum == 4 || testNum == 8 || testNum == 16 ; uiComboBox38.Enabled = testNum == 16; uiComboBox37.Enabled = testNum == 8 || testNum == 16; uiComboBox36.Enabled = testNum == 16;
            //    uiComboBox35.Enabled = testNum == 4 || testNum == 8 || testNum == 16; uiComboBox34.Enabled = testNum == 16; uiComboBox33.Enabled = testNum == 8 || testNum == 16; uiComboBox32.Enabled = testNum == 16;
            //    uiComboBox47.Enabled = testNum == 4 || testNum == 8 || testNum == 16; uiComboBox46.Enabled = testNum == 16; uiComboBox45.Enabled = testNum == 8 || testNum == 16; uiComboBox44.Enabled = testNum == 16;
            //    uiComboBox43.Enabled = testNum == 4 || testNum == 8 || testNum == 16; uiComboBox42.Enabled = testNum == 16; uiComboBox41.Enabled = testNum == 8 || testNum == 16; uiComboBox40.Enabled = testNum == 16;
            //}
            //else if (testNum == 4)
            //{
            //    uiComboBox39.Enabled = true; uiComboBox38.Enabled = false; uiComboBox37.Enabled = false; uiComboBox36.Enabled = false;
            //    uiComboBox35.Enabled = true; uiComboBox34.Enabled = false; uiComboBox33.Enabled = false; uiComboBox32.Enabled = false;
            //    uiComboBox47.Enabled = true; uiComboBox46.Enabled = false; uiComboBox45.Enabled = false; uiComboBox44.Enabled = false;
            //    uiComboBox43.Enabled = true; uiComboBox42.Enabled = false; uiComboBox41.Enabled = false; uiComboBox40.Enabled = false;
            //}
            //else if (testNum == 8)
            //{
            //    uiComboBox39.Enabled = true; uiComboBox38.Enabled = false; uiComboBox37.Enabled = true; uiComboBox36.Enabled = false;
            //    uiComboBox35.Enabled = true; uiComboBox34.Enabled = false; uiComboBox33.Enabled = true; uiComboBox32.Enabled = false;
            //    uiComboBox47.Enabled = true; uiComboBox46.Enabled = false; uiComboBox45.Enabled = true; uiComboBox44.Enabled = false;
            //    uiComboBox43.Enabled = true; uiComboBox42.Enabled = false; uiComboBox41.Enabled = true; uiComboBox40.Enabled = false;
            //}
            //else if (testNum == 16)
            //{
            //    uiComboBox39.Enabled = true; uiComboBox38.Enabled = true; uiComboBox37.Enabled = true; uiComboBox36.Enabled = true;
            //    uiComboBox35.Enabled = true; uiComboBox34.Enabled = true; uiComboBox33.Enabled = true; uiComboBox32.Enabled = true;
            //    uiComboBox47.Enabled = true; uiComboBox46.Enabled = true; uiComboBox45.Enabled = true; uiComboBox44.Enabled = true;
            //    uiComboBox43.Enabled = true; uiComboBox42.Enabled = true; uiComboBox41.Enabled = true; uiComboBox40.Enabled = true;
            //}
            //for (int i = 0; i < UIComboBoxes.Length; i++)
            //{
            //    UIComboBoxes[i].Enabled = 16 / testNum / 
            //}
            //UIComboBoxes[0].
            //panel_1station.Enabled = uiCheckBox158.Checked;
            //panel_2station.Enabled = uiCheckBox161.Checked;
            //panel_3station.Enabled = uiCheckBox161.Checked;
            //panel_4station.Enabled = uiCheckBox160.Checked;
        }

        private void uiCheckBox158_Click(object sender, EventArgs e)
        {
            if (uiCheckBox158.Checked)
            {
                uiCheckBox159.Checked = false;
                uiCheckBox161.Checked = false;
                uiCheckBox160.Checked = false;
            }
            else uiCheckBox158.Checked = true;
            刷新测试机数量显示();
        }

        private void uiCheckBox159_Click(object sender, EventArgs e)
        {
            if (uiCheckBox159.Checked)
            {
                uiCheckBox158.Checked = false;
                uiCheckBox161.Checked = false;
                uiCheckBox160.Checked = false;
            }
            else uiCheckBox159.Checked = true;
            刷新测试机数量显示();
        }

        private void uiCheckBox161_Click(object sender, EventArgs e)
        {
            if (uiCheckBox161.Checked)
            {
                uiCheckBox158.Checked = false;
                uiCheckBox159.Checked = false;
                uiCheckBox160.Checked = false;
            }
            else uiCheckBox161.Checked = true;
            刷新测试机数量显示();
        }

        private void uiCheckBox160_Click(object sender, EventArgs e)
        {
            if (uiCheckBox160.Checked)
            {
                uiCheckBox158.Checked = false;
                uiCheckBox159.Checked = false;
                uiCheckBox161.Checked = false;
            }
            else uiCheckBox160.Checked = true;
            刷新测试机数量显示();
        }

        #endregion

        private void uiCheckBox170_Click(object sender, EventArgs e)
        {
            if (uiCheckBox170.Checked)
            {
                uiCheckBox171.Checked = false;
                uiCheckBox183.Checked = false;
            }
            else uiCheckBox170.Checked = true;
        }

        private void uiCheckBox171_Click(object sender, EventArgs e)
        {
            if (uiCheckBox171.Checked)
            {
                uiCheckBox170.Checked = false;
                uiCheckBox183.Checked = false;
            }
            else uiCheckBox171.Checked = true;
        }
        private void uiCheckBox183_Click(object sender, EventArgs e)
        {
            if (uiCheckBox183.Checked)
            {
                uiCheckBox170.Checked = false;
                uiCheckBox171.Checked = false;
            }
            else uiCheckBox183.Checked = true;
        }

        private void uiDataGridView4_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            int index = e.RowIndex;
            if (index >= 0)
            {
                刷新通讯信息(uiTabControl11.SelectedTab.Text);
            }
        }

        private void uiTabControl11_Selecting(object sender, TabControlCancelEventArgs e)
        {
            刷新通讯信息(uiTabControl11.SelectedTab.Text);
        }

        private void uiButton32_Click(object sender, EventArgs e)
        {
            刷新通讯信息(uiTabControl11.SelectedTab.Text);
        }

        private void uiButton37_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                // 配置对话框属性（可选）
                folderDialog.Description = "请选择一个文件夹"; // 对话框标题
                folderDialog.ShowNewFolderButton = true; // 显示“新建文件夹”按钮
                folderDialog.SelectedPath = Directory.GetCurrentDirectory() + "\\Log";

                // 显示对话框并获取结果
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    string folderPath = folderDialog.SelectedPath; // 选中的文件夹完整路径
                    richTextBox1.Text = folderPath;
                }
            }
        }

        private void uiButton36_Click(object sender, EventArgs e)
        {
            try
            {
                if (richTextBox1.Text.Length == 0)
                {
                    MessageBox.Show("请选择保存路径!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!new DirectoryInfo(richTextBox1.Text).Exists)
                {
                    MessageBox.Show("路径不存在!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                System.Diagnostics.Process.Start("explorer.exe", richTextBox1.Text);
            }
            catch (Exception ex)
            {
                string errortext = "打开文件夹失败!详细信息:" + ex.Message;
                MessageBox.Show(errortext, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                RunLog(errortext);
            }
        }

        private void uiButton39_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("请确认是否保存？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.OK)
            {
                if (uiCheckBox181.Checked != 按了跳过或编辑报警使能)
                {
                    string sql = $"update config set value = '{uiCheckBox181.Checked}' where name = '按了跳过或编辑报警使能'";
                    int line = Sqlite.runSqlNonQuery(SQLiteConnString, sql);
                    if (line >= 1)
                    {
                        RunLog($"按了跳过或编辑报警使能:{(按了跳过或编辑报警使能 ? "开" : "关")}->{(uiCheckBox181.Checked ? "开" : "关")}", LogType.OperateLog);
                        按了跳过或编辑报警使能 = uiCheckBox181.Checked;
                    }
                    else
                    {
                        MessageBox.Show("保存失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }


                if (numericUpDown196.Value != Load门开门时间)
                {
                    string sql = $"update config set value = '{numericUpDown196.Value}' where name = 'Load门开门时间'";
                    int line2 = Sqlite.runSqlNonQuery(SQLiteConnString, sql);

                    if (line2 >= 1)
                    {
                        RunLog($"Load门开门时间:{Load门开门时间}->{numericUpDown196.Value}", LogType.OperateLog);
                        Load门开门时间 = (int)numericUpDown196.Value;
                        handler.Load门开启定时器.Interval = Load门开门时间 * 1000;
                    }
                    else
                    {
                        MessageBox.Show("保存失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

            }
        }

        private void uiButton41_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("请确认是否保存？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.OK)
            {
                if (uiCheckBox182.Checked)
                {
                    if (richTextBox1.Text.Trim().Length == 0)
                    {
                        MessageBox.Show("请输入日志保存路径!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                if (uiCheckBox182.Checked != 按批号保存使能)
                {
                    string sql = $"update config set value = '{uiCheckBox182.Checked}' where name = '按批号保存使能'";
                    int line = Sqlite.runSqlNonQuery(SQLiteConnString, sql);
                    if (line >= 1)
                    {
                        RunLog($"按批号保存使能:{(按批号保存使能 ? "开" : "关")}->{(uiCheckBox182.Checked ? "开" : "关")}", LogType.OperateLog);
                        按批号保存使能 = uiCheckBox182.Checked;
                    }
                    else
                    {
                        MessageBox.Show("保存失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                if (richTextBox1.Text.Trim() != LotNoLog保存路径)
                {
                    string sql = $"update config set value = '{richTextBox1.Text.Trim()}' where name = 'LotNoLog保存路径'";
                    int line2 = Sqlite.runSqlNonQuery(SQLiteConnString, sql);

                    if (line2 >= 1)
                    {
                        RunLog($"批次Log保存路径:{LotNoLog保存路径}->{richTextBox1.Text.Trim()}", LogType.OperateLog);
                        LotNoLog保存路径 = richTextBox1.Text.Trim();
                    }
                    else
                    {
                        MessageBox.Show("保存失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void uiButton40_Click(object sender, EventArgs e)
        {
            //uiCheckBox181.Checked = 按了跳过或编辑报警使能;
            //numericUpDown196.Text = Load门开门时间.ToString();
            uiCheckBox182.Checked = 按批号保存使能;
            richTextBox1.Text = LotNoLog保存路径;
        }

        private void uiButton35_Click(object sender, EventArgs e)
        {
            int rows = uiDataGridView4.SelectedIndex;
            if (rows >= 0)
            {
                string name = uiDataGridView4.Rows[rows].Cells[1].Value.ToString();
                if (name == "默认")
                {
                    MessageBox.Show("默认通讯不能删除!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (name == 设置信息.通讯信息.通讯名称)
                {
                    MessageBox.Show("通讯使用中不能删除!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                DialogResult dialog = MessageBox.Show($"请确认是否删除通讯:{name}", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (dialog == DialogResult.OK)
                {
                    int line = Sqlite.runSqlNonQuery(SQLiteConnString, $"delete From ComInfoSet Where 通讯名称 = '{name}'");
                    if (line >= 1)
                    {
                        刷新通讯列表();
                    }
                    else MessageBox.Show("删除通讯失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else MessageBox.Show("请选择要删除的通讯!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void uiButton34_Click(object sender, EventArgs e)
        {
            NewComName newSetupName = new NewComName();
            DialogResult dialog = newSetupName.ShowDialog();
            if (dialog == DialogResult.OK)
            {
                string sql = $"select count(*) from ComInfoSet where 通讯名称 = '{newSetupName.Name}'";
                string result = Sqlite.runSql(SQLiteConnString, sql, out string value);
                if (result == "Success")
                {
                    if (Convert.ToInt32(value) >= 1) MessageBox.Show("通讯已存在!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    else
                    {
                        dialog = MessageBox.Show($"请确认是否通讯:{newSetupName.Name}", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                        if (dialog == DialogResult.OK)
                        {
                            string site = "{\"Protocol\":\"COM\",\"PortName\":\"COM10\",\"BaudRate\":9600,\"Parity\":0,\"DataBits\":8,\"StopBits\":1}";
                            sql = $"insert into ComInfoSet(通讯名称,客户,通讯协议,通讯方式,测试机数量,同时测试,测试结果中有对应Site,超时Bin字符串,发送开批信息,Site1,Site2,Site3,Site4,Site5,Site6,Site7,Site8,Site9,Site10,Site11,Site12,Site13,Site14,Site15,Site16)values('{newSetupName.Name}','默认','默认','COM','{1}','{1}','true','AB','true','{site}','{site}','{site}','{site}','{site}','{site}','{site}','{site}','{site}','{site}','{site}','{site}','{site}','{site}','{site}','{site}')";
                            int line = Sqlite.runSqlNonQuery(SQLiteConnString, sql);
                            if (line >= 1)
                            {
                                刷新通讯列表();
                                MessageBox.Show("新建成功!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("新建失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
            }
        }

        private void uiRadioButton3_Click(object sender, EventArgs e)
        {
            if (uiRadioButton1.Checked)
            {
                uiRadioButton2.Checked = false;
                uiRadioButton3.Checked = false;
                DialogResult result = MessageBox.Show("请确认是否更改默认界面?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (result == DialogResult.OK)
                {
                    string sql = $"update config set value = '1' where name = '自定义界面'";
                    int line = Sqlite.runSqlNonQuery(SQLiteConnString, sql);
                    if (line >= 1) 自定义界面 = "1";
                    else MessageBox.Show("更改失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    uiRadioButton1.Checked = 自定义界面 == "1";
                    uiRadioButton2.Checked = 自定义界面 == "2";
                    uiRadioButton3.Checked = 自定义界面 == "3";
                }
            }
            else uiRadioButton1.Checked = true;

            刷新自定义界面_委托方法();
        }

        private void uiRadioButton2_Click(object sender, EventArgs e)
        {
            if (uiRadioButton2.Checked)
            {
                uiRadioButton1.Checked = false;
                uiRadioButton3.Checked = false;
                DialogResult result = MessageBox.Show("请确认是否更改自定义1界面?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (result == DialogResult.OK)
                {
                    string sql = $"update config set value = '2' where name = '自定义界面'";

                    int line = Sqlite.runSqlNonQuery(SQLiteConnString, sql);
                    if (line >= 1) 自定义界面 = "2";
                    else MessageBox.Show("更改失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    uiRadioButton1.Checked = 自定义界面 == "1";
                    uiRadioButton2.Checked = 自定义界面 == "2";
                    uiRadioButton3.Checked = 自定义界面 == "3";
                }
            }
            else uiRadioButton2.Checked = true;

            刷新自定义界面_委托方法();
        }

        private void uiRadioButton3_Click_1(object sender, EventArgs e)
        {
            if (uiRadioButton3.Checked)
            {
                uiRadioButton1.Checked = false;
                uiRadioButton2.Checked = false;
                DialogResult result = MessageBox.Show("请确认是否更改自定义2界面?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (result == DialogResult.OK)
                {
                    string sql = $"update config set value = '3' where name = '自定义界面'";
                    int line = Sqlite.runSqlNonQuery(SQLiteConnString, sql);
                    if (line >= 1) 自定义界面 = "3";
                    else MessageBox.Show("更改失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    uiRadioButton1.Checked = 自定义界面 == "1";
                    uiRadioButton2.Checked = 自定义界面 == "2";
                    uiRadioButton3.Checked = 自定义界面 == "3";
                }
            }
            else uiRadioButton3.Checked = true;

            刷新自定义界面_委托方法();
        }

        private void uiCheckBox185_Click(object sender, EventArgs e)
        {
            if (uiCheckBox185.Checked)
            {
                uiCheckBox184.Checked = false;
            }
            else uiCheckBox185.Checked = true;
        }
        private void uiCheckBox184_Click(object sender, EventArgs e)
        {
            if (uiCheckBox184.Checked)
            {
                uiCheckBox185.Checked = false;
            }
            else uiCheckBox184.Checked = true;
        }

        private void uiCheckBox174_Click(object sender, EventArgs e)
        {
            if (uiCheckBox174.Checked)
            {
                uiCheckBox175.Checked = false;
                uiCheckBox186.Checked = false;
            }
            else uiCheckBox174.Checked = true;
        }

        private void uiCheckBox175_Click(object sender, EventArgs e)
        {
            if (uiCheckBox175.Checked)
            {
                uiCheckBox174.Checked = false;
                uiCheckBox186.Checked = false;
            }
            else uiCheckBox175.Checked = true;
        }

        private void uiCheckBox186_Click(object sender, EventArgs e)
        {
            if (uiCheckBox186.Checked)
            {
                uiCheckBox174.Checked = false;
                uiCheckBox175.Checked = false;
            }
            else uiCheckBox186.Checked = true;
        }


        private void uiCheckBox189_Click(object sender, EventArgs e)
        {
        }

        private void uiButton38_Click(object sender, EventArgs e)
        {
            numericUpDown196.Text = Load门开门时间.ToString();
            uiCheckBox181.Checked = 按了跳过或编辑报警使能;
        }
    }
}
