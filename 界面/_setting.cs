using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sunny.UI;
using Sunny.UI.Win32;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static GTN.mc;
using static H5509_V2.APPmaster;

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

            textBox17.Text = motion.GetCmdPrfPos(core, axis).ToString();//X轴规划位置
            textBox16.Text = motion.GetCmdPrfVel(core, axis).ToString();//X轴规划速度
            textBox15.Text = motion.GetCmdEncPos(core, axis).ToString();//X轴实时位置
            textBox14.Text = motion.GetCmdEncVel(core, axis).ToString();//X轴实时速度

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
            //List<AxisRun> listAxisRun = new List<AxisRun>();
            //listAxisRun.Add(new AxisRun() { 核号 = 1, 轴号 = 10 });
            //listAxisRun.Add(new AxisRun() { 核号 = 1, 轴号 = 11 });
            //listAxisRun.Add(new AxisRun() { 核号 = 1, 轴号 = 12 });
            //listAxisRun.Add(new AxisRun() { 核号 = 1, 轴号 = 1 });

            //List<short[]> intmask = new List<short[]> { new short[32], new short[32] };
            //for (short i = 0; i < listAxisRun.Count ; i++)
            //{
            //    intmask[listAxisRun[i].核号 - 1][listAxisRun[i].轴号 - 1] = 1;
            //}

            //string mask1Str = new string(string.Join("", intmask[0]).Reverse().ToArray());
            //int maskcore1 = Convert.ToInt32(mask1Str, 2); // 标识了输入字符串的进制类型,是实现二进制到十进制转换的关键设置
            //if (maskcore1 != 0) //如果有轴要运动
            //{
            //    Console.WriteLine($"十进制结果：{maskcore1}");
            //}

            //string mask2Str = new string(string.Join("", intmask[1]).Reverse().ToArray());
            //int maskcore2 = Convert.ToInt32(mask2Str, 2); // 标识了输入字符串的进制类型,是实现二进制到十进制转换的关键设置
            //if (maskcore2 != 0) //如果有轴要运动
            //{
            //    Console.WriteLine($"十进制结果：{maskcore2}");
            //}
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

                string result = motion.UniaxialRelativeMove(axisRun);
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

                string result = motion.UniaxialRelativeMove(axisRun);
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
            APPmaster.GetAxisCore(cobox_axisname.Text, out short core, out short axis);

            AxisHomeInfo axisHome = GetHomeValue();
            if (axisHome != null)
            {
                string result = motion.UniaxialHome(axisHome);
                if (result != "Success")
                {
                    MessageBox.Show("执行失败!详细信息:" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
                    Debug.WriteLine(axisHomes[i].轴名称 + "回零失败!");
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
                    motion.ErrorHandler($"GTN_GetHomeStatus({axisHome.核号}, {axisHome.轴号}, out tHomeSts)", error);
                    Thread.Sleep(1); //减少轴卡和CPU压力
                }
                while (tHomeSts.run == 1);

                if (tHomeSts.error != HOME_ERROR_NONE) //未发生错误
                {
                    if (tHomeSts.error == HOME_ERROR_NOT_TRAP_MODE) return "执行Smart Home回原点的轴不是处于点位运动模式";
                    else if (tHomeSts.error == HOME_ERROR_DISABLE) return "执行Smart Home回原点的轴未使能";
                    else if (tHomeSts.error == HOME_ERROR_ALARM) return "执行Smart Home回原点的轴驱动报警";
                    else if (tHomeSts.error == HOME_ERROR_STOP) return "未完成回原点，轴停止运动";
                    else if (tHomeSts.error == HOME_ERROR_STAGE) return "回原点阶段错误";
                    else if (tHomeSts.error == HOME_ERROR_HOME_MODE) return "模式错误，如已经启动Smart Home，勿重复调用回原点指令";
                    else if (tHomeSts.error == HOME_ERROR_SET_CAPTURE_HOME) return "设置Home捕获模式失败";
                    else if (tHomeSts.error == HOME_ERROR_NO_HOME) return "未找到Home";
                    else if (tHomeSts.error == HOME_ERROR_SET_CAPTURE_INDEX) return "设置Index捕获模式失败";
                    else if (tHomeSts.error == HOME_ERROR_NO_INDEX) return "未找到Index";
                    else return "回零异常";
                }

                result = motion.AxisZeroPos(axisHome.核号, axisHome.轴号);

            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return result;
        }

        private string WaitAxisMove(AxisRun axisRun)
        {
            string result = motion.CommonInitedValidate();

            if (result != "Success") return result;

            try
            {
                int sts;
                do
                {
                    short error = GTN_GetSts(axisRun.核号, axisRun.轴号, out sts, 1, out uint pClock);
                    motion.ErrorHandler($"GTN_GetSts({axisRun.核号}, {axisRun.轴号}, out {sts}, 1, out uint pClock)", error);
                    Thread.Sleep(1); //减少轴卡和CPU压力
                }
                while ((sts & 0x400) != 0);

                double encPos = motion.GetCmdEncPos(axisRun.核号, axisRun.轴号);
                double prfPos = motion.GetCmdPrfPos(axisRun.核号, axisRun.轴号);
                double posvalue = Math.Abs(prfPos - encPos);
                Debug.WriteLine($"核心{axisRun.核号},轴号{axisRun.轴号}的规划位置({prfPos})和实际位置({encPos})的跟随误差={posvalue}");
                if (posvalue > 跟随误差)
                {
                    return $"核心{axisRun.核号},轴号{axisRun.轴号}的规划位置({prfPos})和实际位置({encPos})的跟随误差={posvalue}>{跟随误差}";
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
        private string WaitAxisMove(List<AxisRun> axisRuns)
        {
            string result = motion.CommonInitedValidate();

            if (result != "Success") return result;

            try
            {
                bool value = true;
                int[] sts = new int[axisRuns.Count];
                do
                {
                    for (int i = 0; i < axisRuns.Count; i++)
                    {
                        short error = GTN_GetSts(axisRuns[i].核号, axisRuns[i].轴号, out sts[i], 1, out uint pClock);
                        motion.ErrorHandler($"GTN_GetSts({axisRuns[i].核号}, {axisRuns[i].轴号}, out {sts[i]}, 1, out uint pClock)", error);
                    }

                    value = false;

                    for (int i = 0; i < sts.Length; i++)
                    {
                        if ((sts[i] & 0x400) != 0)
                        {
                            value = true;
                            break;
                        }
                    }

                    Thread.Sleep(1); //减少轴卡和CPU压力
                }
                while (value);

                for (int i = 0; i < axisRuns.Count; i++)
                {
                    double encPos = motion.GetCmdEncPos(axisRuns[i].核号, axisRuns[i].轴号);
                    double prfPos = motion.GetCmdPrfPos(axisRuns[i].核号, axisRuns[i].轴号);
                    double posvalue = Math.Abs(prfPos - encPos);
                    Debug.WriteLine($"核心{axisRuns[i].核号},轴号{axisRuns[i].轴号}的规划位置({prfPos})和实际位置({encPos})的跟随误差={posvalue}");
                    if (posvalue > 跟随误差)
                    {
                        return $"核心{axisRuns[i].核号},轴号{axisRuns[i].轴号}的规划位置({prfPos})和实际位置({encPos})的跟随误差={posvalue}>{跟随误差}";
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "Success";
        }

        private string 单轴绝对运动(string axisName, double pos)
        {
            string result = APPmaster.GetAxisRunInfo(axisName, out AxisRunInfo axisRunInfo);
            if (result == "Success")
            {
                AxisRun axisRun = new AxisRun()
                {
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
                        return WaitAxisMove(axisRun);
                    });
                    result = task.Result;
                }
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

        public string 多轴绝对运动(string[] axisNames, double[] poss)
        {
            string result;
            List<AxisRun> axisRuns = new List<AxisRun>();
            for (int i = 0; i < axisNames.Length; i++)
            {
                result = APPmaster.GetAxisRunInfo(axisNames[i], out AxisRunInfo axisRunInfo);
                if (result == "Success")
                {
                    axisRuns.Add(new AxisRun()
                    {
                        核号 = axisRunInfo.核号,
                        轴号 = axisRunInfo.轴号,
                        目标位置 = Convert.ToInt32(Math.Round(poss[i] / axisRunInfo.步长mm)),
                        目标速度 = axisRunInfo.运行速度,
                        目标加速度 = axisRunInfo.加速度,
                        目标减速度 = axisRunInfo.减速度,
                        平滑时间 = axisRunInfo.平滑时间,
                    });
                }
            }
            result =  motion.MultiaxisAbsoluteMoveAbs(axisRuns);
            if (result == "Success")
            {
                Task<string> task = Task.Run(() =>
                {
                    return WaitAxisMove(axisRuns);
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
                if (result != "Success") Debug.WriteLine("入料吸嘴_真空检测IO读取失败!" + result);

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
                Debug.WriteLine($"入料Z回原：{home[0]} {home[1]} {home[2]} {home[3]}");
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
            if (result != "Success") Debug.WriteLine("入料吸嘴_吸真空IO输出失败!" + result);
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
                        if (result != "Success") Debug.WriteLine("入料吸嘴_真空检测IO读取失败!" + result);

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
                Debug.WriteLine($"入料Z回原：{home[0]} {home[1]} {home[2]} {home[3]}");
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
                    MessageBox.Show("M22_入料手臂X轴回零失败!\n详细信息:" + result);
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
                    MessageBox.Show("M16_入料手臂Y轴回零失败!\n详细信息:" + result);
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
                    MessageBox.Show("M5_入料手臂P轴回零失败!\n详细信息:" + result);
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
                    MessageBox.Show("入料手臂所有Z回零失败!\n详细信息:" + result);
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
                    MessageBox.Show("M4_入料手臂Z4轴回零失败!\n详细信息:" + result);
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
                    MessageBox.Show("M3_入料手臂Z3轴回零失败!\n详细信息:" + result);
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
                    MessageBox.Show("M2_入料手臂Z2轴回零失败!\n详细信息:" + result);
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
                    MessageBox.Show("M1_入料手臂Z1轴回零失败!\n详细信息:" + result);
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
                    MessageBox.Show("入料手臂XYP回零失败!\n详细信息:" + result);
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
            for (int i = 0; i < AxisInfoList.Count; i++)
            {
                string result = motion.AxisStop(AxisInfoList[i].核号, AxisInfoList[i].轴号, 1);
                if (result != "Success")
                {
                    Debug.WriteLine($"核号{AxisInfoList[i].核号},轴{AxisInfoList[i].轴号}停止失败!详细信息:" + result);
                }
                else MessageBox.Show("所有轴已紧急停止");
            }
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
    }
}
