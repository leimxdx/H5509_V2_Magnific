using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HardwareGXN;
using IHardwareGXN.GXNCard;
using MotionCommon;

namespace HardwareGXN.GSNAxisCard
{
    public partial class FrmGSN : Form
    {
        IGXNInterface iGxnInterface = null;

        private System.Timers.Timer RefreshTimer;

        public List<AxisInfo> AxisInfoList = new List<AxisInfo>();

        public List<AxisHomeInfo> AxisHomeInfoList = new List<AxisHomeInfo>();

        public int 相对运动步长 = 1000;

        public int 绝对运动步长 = 1000;

        public FrmGSN()
        {
            InitializeComponent();

            RefreshTimer = new System.Timers.Timer() { Interval = 500 };
            RefreshTimer.Elapsed += RefreshAxisStatusElapsedTick;

            txt_home_mode.Items.Add("限位+Home回零");
            txt_home_mode.Items.Add("Home回零");
            txt_home_mode.SelectedIndex = 0;
        }

        public FrmGSN(IGXNInterface gxnInterface) : this()
        {
            iGxnInterface = gxnInterface;
            //iGxnInterface = new GSNAxisController();
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

        public bool GetAxisCore(string axisname, out short core, out short axis)
        {
            core = axis = 0;
            for (int i = 0; i < AxisInfoList.Count; i++)
            {
                if (axisname == AxisInfoList[i].轴名称)
                {
                    core = AxisInfoList[i].核号;
                    axis = AxisInfoList[i].轴号;
                    return true;
                }
            }
            return false;
        }

        public bool GetAxisHomeInfo(string axisInfo, out AxisHomeInfo axisRun)
        {
            axisRun = new AxisHomeInfo();
            for (int i = 0; i < AxisHomeInfoList.Count; i++)
            {
                if (axisInfo == AxisHomeInfoList[i].轴名称)
                {
                    axisRun = AxisHomeInfoList[i];
                    return true;
                }
            }
            return false;
        }

        private void UpdateState()
        {
            if (!GetAxisCore(cobox_axisname.Text, out short core, out short axis)) return;

            if (iGxnInterface.GetCmdPrfPos(core, axis, out int pPos) == "Success") textBox17.Text = pPos.ToString();
            else textBox17.Text = "获取失败"; //X轴规划位置
            if (iGxnInterface.GetCmdPrfVel(core, axis, out double pPos2) == "Success")
            {
                if (pPos2 == 0) textBox16.Text = "0";
                else textBox16.Text = pPos2.ToString("F2");
            }
            else textBox16.Text = "获取失败"; //X轴规划速度
            if (iGxnInterface.GetCmdEncPos(core, axis, out double pPos3) == "Success") textBox15.Text = pPos3.ToString();
            else textBox15.Text = "获取失败"; //X轴实时位置
            if (iGxnInterface.GetCmdEncVel(core, axis, out double pPos4) == "Success")
            {
                if (pPos4 == 0) textBox14.Text = "0";
                else textBox14.Text = pPos4.ToString("F2");
            }
            else textBox14.Text = "获取失败"; //X轴实时速度

            bool[] axistatus = new bool[32];
            GetAxisStatus(core, axis, ref axistatus); //获取指定轴状态
            RefreshAxistatus(axistatus); //刷新轴状态

            RefreshAxisHomeStatus(core, axis); //刷新原点状态
            //RefreshGlink200IO();
            //RefreshGNM601IO();
        }

        public string GetAxisStatus(short core, short axis, ref bool[] gNME601AxisStatus)
        {
            string result = iGxnInterface.GetAxisStatus(core, axis, out int sts);
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
            string result = iGxnInterface.GNM601_GetDiBit(core, 3, axis, out short value);
            if (result == "Success")
            {
                label1327.BackColor = value == 0 ? Color.Red : Color.LimeGreen; //0处于触发状态(原点常闭), 1处于未触发状态,挡住了原点
            }
        }

        /// <summary>
        /// 开启读取轴信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button52_Click(object sender, EventArgs e)
        {
            if (iGxnInterface == null)
            {
                iGxnInterface = new GSNAxisController();
            }

            if (!iGxnInterface.InitedOK)
            {
                iGxnInterface.InitCard();

                if (!iGxnInterface.InitedOK)
                {
                    MessageBox.Show("轴卡未连接！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            if (button52.Text == "开启读取轴信息")
            {
                RefreshTimer.Start();
                button52.Text = "取消读取轴信息";


                button146.Enabled = true;
                button152.Enabled = true;
                button2.Enabled = true;
                //button90.Enabled = true; //GlinkIO按钮
                //button91.Enabled = true; //GlinkIO按钮 
                //button92.Enabled = true; //GNM601IO按钮
                //button93.Enabled = true; //GNM601IO按钮
                button156.Enabled = true;//回零按钮
                button155.Enabled = true;//停止回零按钮

                button1.Enabled = false;//正向运动按钮
                button39.Enabled = false;//反向运动按钮
                button4.Enabled = false; //绝对运动按钮
                button145.Enabled = false;//停止运动按钮
                button15.Enabled = false; //Jog+
                button3.Enabled = false; //Jog-

            }
            else
            {
                RefreshTimer.Stop();
                button146.Enabled = false;
                button152.Enabled = false;
                button2.Enabled = false;
                //button90.Enabled = false; //GlinkIO按钮
                //button91.Enabled = false; //GlinkIO按钮
                //button92.Enabled = false; //GNM601IO按钮
                //button93.Enabled = false; //GNM601IO按钮
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

        private void FrmGSN_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < AxisInfoList.Count; i++)
            {
                cobox_axisname.Items.Add(AxisInfoList[i].轴名称);
            }
            if (cobox_axisname.Items.Count > 0)
            {
                cobox_axisname.SelectedIndex = 0;
            }
        }
        private void cobox_axisname_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (GetAxisHomeInfo(cobox_axisname.Text, out AxisHomeInfo axisHomeInfo))
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

        private void button146_Click(object sender, EventArgs e)
        {
            //if (设备状态 == 1) return;
            GetAxisCore(cobox_axisname.Text, out short core, out short axis);

            if (button146.Text == "使能开启")
            {
                string result = iGxnInterface.AxisOn(core, axis, true);
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
                string result = iGxnInterface.AxisOn(core, axis, false);
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
            //if (设备状态 == 1) return;
            GetAxisCore(cobox_axisname.Text, out short core, out short axis);

            string result = iGxnInterface.AxisClrSts(core, axis);
            if (result != "Success")
            {
                MessageBox.Show("清除状态失败！详细信息：" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 位置清零
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            //if (设备状态 == 1) return;
            GetAxisCore(cobox_axisname.Text, out short core, out short axis);

            string result = iGxnInterface.AxisZeroPos(core, axis);
            if (result != "Success")
            {
                MessageBox.Show("清除状态失败！详细信息：" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //else writeDataHelper.RunLog("位置清零成功，详细信息：" + result.ErrorMsg);
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

        private void uiCheckBox5_Click(object sender, EventArgs e)
        {
            chk_home_pmovedir.Checked = !chk_home_nmovedir.Checked;
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

            GetAxisCore(cobox_axisname.Text, out short core, out short axis);

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

            GetAxisCore(cobox_axisname.Text, out short core, out short axis);

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
            GetAxisCore(cobox_axisname.Text, out short core, out short axis);

            string result = iGxnInterface.AxisStop(core, axis, 1);

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
            //if (设备状态 == 1) return;
            AxisRun axisRun = GetRunValue();
            if (axisRun != null)
            {
                if (uiChk_relative.Checked) 相对运动步长 = axisRun.目标位置;
                else 绝对运动步长 = axisRun.目标位置;

                string result = iGxnInterface.UniaxialRelativeMove(axisRun, out int prfPos);
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
            //if (设备状态 == 1) return;
            AxisRun axisRun = GetRunValue();
            if (axisRun != null)
            {
                if (uiChk_relative.Checked) 相对运动步长 = axisRun.目标位置;
                else 绝对运动步长 = axisRun.目标位置;

                axisRun.目标位置 = -axisRun.目标位置;

                string result = iGxnInterface.UniaxialRelativeMove(axisRun, out int prfPos);
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
            //if (设备状态 == 1) return;
            AxisRun axisRun = GetRunValue();
            if (axisRun != null)
            {
                if (uiChk_relative.Checked) 相对运动步长 = axisRun.目标位置;
                else 绝对运动步长 = axisRun.目标位置;

                string result = iGxnInterface.UniaxialAbsoluteMoveAbs(axisRun);
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
            //if (设备状态 == 1) return;
            AxisStop();
        }
        private void button156_Click(object sender, EventArgs e)
        {
            //if (设备状态 == 1) return;
            AxisHomeInfo axisHome = GetHomeValue();
            if (axisHome != null)
            {
                string result = iGxnInterface.UniaxialHome(axisHome);
                if (result == "Success")
                {
                    Task.Run(() =>
                    {
                        result = iGxnInterface.WaitAxisHome(axisHome, 1);//等待回零完成
                        if (result != "Success")
                        {
                            MessageBox.Show("回零失败!\n详细信息:" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    });
                }
                else
                {
                    MessageBox.Show("执行回零失败!\n详细信息:" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            //if (设备状态 == 1) return;
            AxisStop();
        }

        /// <summary>
        /// Jog+开始运动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button15_MouseDown(object sender, MouseEventArgs e)
        {
            //if (设备状态 == 1) return;
            AxisRun axisRun = GetRunValue();
            if (axisRun != null)
            {
                string result = iGxnInterface.JogMove(axisRun);
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
            //if (设备状态 == 1) return;
            AxisRun axisRun = GetRunValue();
            if (axisRun != null)
            {
                axisRun.目标速度 = -axisRun.目标速度;
                string result = iGxnInterface.JogMove(axisRun);
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
    }
}
