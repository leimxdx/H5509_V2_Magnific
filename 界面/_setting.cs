using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sunny.UI;
using Sunny.UI.Win32;

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
                label1327.BackColor = value == 0 ? Color.LimeGreen : Color.Red;
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
                    if (value[i + j] == '0') GNM601LimitN[i].BackColor = Color.LimeGreen;
                    else GNM601LimitN[i].BackColor = Color.Silver;
                }

                motion.GNM601_GetDi(1, GTN.mc.MC_LIMIT_POSITIVE, out value);
                for (int i = 0; i + j < value.Length && i < GNM601LimitP.Length; i++)
                {
                    if (value[i + j] == '0') GNM601LimitP[i].BackColor = Color.LimeGreen;
                    else GNM601LimitP[i].BackColor = Color.Silver;
                }

                j = uiChk_GNM1.Checked ? 0 : 10;
                motion.GNM601_GetDo(1, GTN.mc.MC_GPO, out value);
                for (int i = 0; i + j < value.Length && i < GNM601DO.Length; i++)
                {
                    if (value[i + j] == '0') GNM601DO[i].BackColor = Color.LimeGreen;
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
                    if (value[i + j] == '0') GNM601LimitN[i].BackColor = Color.LimeGreen;
                    else GNM601LimitN[i].BackColor = Color.Silver;
                }

                motion.GNM601_GetDi(2, GTN.mc.MC_LIMIT_POSITIVE, out value);
                for (int i = 0; i + j < value.Length && i < GNM601LimitP.Length; i++)
                {
                    if (value[i + j] == '0') GNM601LimitP[i].BackColor = Color.LimeGreen;
                    else GNM601LimitP[i].BackColor = Color.Silver;
                }

                j = uiChk_GNM3.Checked ? 0 : 10;
                motion.GNM601_GetDo(2, GTN.mc.MC_GPO, out value);
                for (int i = 0; i + j < value.Length && i < GNM601DO.Length; i++)
                {
                    if (value[i + j] == '0') GNM601DO[i].BackColor = Color.LimeGreen;
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

        private void uiCheckBox5_Click(object sender, EventArgs e)
        {
            chk_home_pmovedir.Checked = !chk_home_nmovedir.Checked;
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

                return motion.UniaxialAbsoluteMoveAbs(axisRun);
            }
            return result;
        }

        public string 多轴绝对运动(string[] axisNames, double[] poss)
        {
            List<AxisRun> axisRuns = new List<AxisRun>();
            for (int i = 0; i < axisNames.Length; i++)
            {
                string result = APPmaster.GetAxisRunInfo(axisNames[i], out AxisRunInfo axisRunInfo);
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
            return motion.MultiaxisAbsoluteMoveAbs(axisRuns);

        }

        private void 入料Z移动(bool[] isopen, bool isdown, double zpos)
        { 
            List<string> axisNames = new List<string>();
            if (isopen[0]) axisNames.Add("M1_入料手臂Z1轴");
            if (isopen[1]) axisNames.Add("M2_入料手臂Z2轴");
            if (isopen[2]) axisNames.Add("M3_入料手臂Z3轴");
            if (isopen[3]) axisNames.Add("M4_入料手臂Z4轴");
            if (isdown) 
            {
                string result = 多轴绝对运动(axisNames.ToArray(),new double[] { zpos, zpos, zpos, zpos } );

            }
        }

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
            double[] axisPoss = { xpos , ypos, ppos };

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
                string result = 单轴绝对运动("M5_入料手臂P轴", pos);
                if (result != "Success")
                {
                    MessageBox.Show("入料手臂P移动失败!\n详细信息:" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
