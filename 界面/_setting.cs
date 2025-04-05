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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;
using System.Security.Cryptography;
using Sunny.UI.Win32;

namespace H5509_V2
{
    public partial class _setting : UIUserControl
    {
        private Motion motion = new Motion();

        private System.Timers.Timer RefreshTimer;

        public _setting()
        {
            InitializeComponent();

            RefreshTimer = new System.Timers.Timer() { Interval = 200 };
            RefreshTimer.Elapsed += RefreshAxisStatusElapsedTick;

            for (int i = 1; i <= 24; i++)
            {
                comboBox3.Items.Add(i.ToString());
            }
            comboBox3.Text = "1";
        }

        public OperationResult GetAxisStatus(short core, short axis, ref bool[] gNME601AxisStatus)
        {
            OperationResult result = motion.GetAxisStatus(core, axis, out int sts);
            if (result.IsSuccess)
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
                button1.Enabled = true;//正向运动按钮
                button39.Enabled = true;//反向运动按钮
                button4.Enabled = true; //绝对运动按钮
                button145.Enabled = true;//停止运动按钮
                button156.Enabled = true;//回零按钮
                button15.Enabled = true; //Jog+
                button3.Enabled = true; //Jog-
                //button17.Enabled = true;//全部回零按钮
                button155.Enabled = true;//停止回零按钮
                button146.Text = "使能关闭";
                label1582.BackColor = Color.LimeGreen;
            }
            else
            {
                button1.Enabled = false;//正向运动按钮
                button39.Enabled = false;//反向运动按钮
                button4.Enabled = false; //绝对运动按钮
                button145.Enabled = false;//停止运动按钮
                button156.Enabled = false;//回零按钮
                button15.Enabled = false; //Jog+
                button3.Enabled = false; //Jog-
                //button17.Enabled = false;//全部回零按钮
                button155.Enabled = false;//停止回零按钮
                button146.Text = "使能开启";
                label1582.BackColor = Color.Silver;
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

        private void UpdateState()
        {
            this.Invoke(new Action(() =>
            {
                short axis = Convert.ToInt16(comboBox3.Text.Trim()); //获取软件轴

                short core = 1;
                if (axis > 12) core = 2;

                textBox17.Text = motion.GetCmdPrfPos(core, (short)(axis - (core - 1) * 12)).ToString();//X轴规划位置
                textBox16.Text = motion.GetCmdPrfVel(core, (short)(axis - (core - 1) * 12)).ToString();//X轴规划速度
                textBox15.Text = motion.GetCmdEncPos(core, (short)(axis - (core - 1) * 12)).ToString();//X轴实时位置
                textBox14.Text = motion.GetCmdEncVel(core, (short)(axis - (core - 1) * 12)).ToString();//X轴实时速度

                bool[] axistatus = new bool[32];
                GetAxisStatus(core, axis, ref axistatus); //获取指定轴状态
                RefreshAxistatus(axistatus); //刷新轴状态
            }));
            //bool[] axistatus = motion.GetAxisStatusInfo(axis); //获取指定轴状态
            //RefreshAxistatus(axistatus); //刷新轴状态

            //int x = (axis - 1) / 6; //轴模块0-3
            //int y = (axis - 1) % 6; //轴0-5
            //RefreshAxisPNlimitsAndOriginstatus(x, y); //刷新轴原点状态
        }

        private void RefreshAxisStatusElapsedTick(object sender, EventArgs eventArgs)
        {
            UpdateState();
        }

        /// <summary>
        /// 开启读取轴信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button52_Click(object sender, EventArgs e)
        {
            if (button52.Text == "开启读取轴信息")
            {
                RefreshTimer.Start();
                button52.Text = "取消读取轴信息";
                button146.Enabled = true;
                button152.Enabled = true;
                button2.Enabled = true;
            }
            else
            {
                RefreshTimer.Stop();
                button146.Enabled = false;
                button152.Enabled = false;
                button2.Enabled = false;
                button52.Text = "开启读取轴信息";
            }
        }

        private void button146_Click(object sender, EventArgs e)
        {
            short axis = Convert.ToInt16(comboBox3.Text.Trim()); //获取软件轴

            short core = 1;
            if (axis > 12) core = 2;

            if (button146.Text == "使能开启")
            {
                OperationResult result = motion.AxisOn(core, axis, true);
                if (!result.IsSuccess)
                {
                    //writeDataHelper.RunLog("使能开启失败，详细信息：" + result.ErrorMsg);
                }
                else
                {
                    button146.Text = "使能关闭";
                }
            }
            else
            {
                OperationResult result = motion.AxisOn(core, axis, false);
                if (!result.IsSuccess)
                {
                    //writeDataHelper.RunLog("使能关闭失败，详细信息：" + result.ErrorMsg);
                }
                else
                {
                    button146.Text = "使能开启";
                }
            }
        }

        private void button152_Click(object sender, EventArgs e)
        {
            short axis = Convert.ToInt16(comboBox3.Text.Trim()); //获取软件轴

            short core = 1;
            if (axis > 12) core = 2;

            OperationResult result = motion.AxisClrSts(core, axis);
            if (!result.IsSuccess)
            {
                // writeDataHelper.RunLog("清除标志失败，详细信息：" + result.ErrorMsg);
            }
            //else writeDataHelper.RunLog("清除标志成功，详细信息：" + result.ErrorMsg);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            short axis = Convert.ToInt16(comboBox3.Text.Trim()); //获取软件轴

            short core = 1;
            if (axis > 12) core = 2;

            OperationResult result = motion.AxisZeroPos(core, axis);
            if (!result.IsSuccess)
            {
                //writeDataHelper.RunLog("位置清零失败，详细信息：" + result.ErrorMsg);
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
    }
}
