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
using static H5509_V2.APPmaster;
using System.Threading;
using System.Diagnostics;

namespace H5509_V2
{
    public partial class _main : UIUserControl
    {
        public Task RefreshData;

        /// <summary>
        /// 设备状态灯闪烁
        /// </summary>
        private bool flicker { get; set; } = false;
        /// <summary>
        /// 红灯是否闪烁
        /// </summary>
        private bool isRedFlicker { get; set; } = false;
        /// <summary>
        /// 黄灯是否闪烁
        /// </summary>
        private bool isYellowFlicker { get; set; } = false;
        /// <summary>
        /// 绿灯是否闪烁
        /// </summary>
        private bool isGreenFlicker { get; set; } = true;

        private System.Windows.Forms.Timer currenttime = new System.Windows.Forms.Timer();

        public _main()
        {
            InitializeComponent();
        }

        private void textBoxFlowcaedno_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.Enter)
            {
                try
                {
                    //MF20240815053-1001@125℃@8H
                    //ZRS240929001A:BS-TEST:HM:RS70B64G4M08G:E9002B:AE35:FT3_RS70B64G4M08G_032003003_032003003::3040:::FT::R80544:FT3:FT:25

                    //ZRS240929001A:BS-TEST:HM:RS70B64G4M08G:E9002B:AE35:FT3_RS70B64G4M08G_032003003_032003003::3040:::FT::R80544:FT3:FT:25
                    string[] getstr = textBoxFlowcaedno.Text.Split(':');
                    if (getstr.Length == 17)
                    {
                        textBoxFlowcaedno.Text = getstr[0].Replace("-", "_"); //ZRS240929001A:
                        //textBoxmachine.Text = getstr[1]; //BS-TEST:
                        textBoxcustom.Text = getstr[2]; //HM:
                        textBoxproduct.Text = getstr[3];
                        textBoxLotno.Text = getstr[4];
                        textBoxdatecode.Text = getstr[5];
                        textBoxfwversion.Text = getstr[6];
                        textBoxcheckcode.Text = getstr[7];
                        if (uiCheckBox1.Checked == false) //如果不是手动就显示扫码出来的数量
                        {
                            textBoxmqty.Text = getstr[8];
                        }
                        textBoxcode.Text = getstr[9];
                        textBoxcodecrc.Text = getstr[10];
                        textBoxstation.Text = getstr[11];
                        textBoxmark.Text = getstr[12];
                        if (textBoxuser.Text == "")
                        {
                            textBoxuser.Text = getstr[13];
                        }
                        textBoxtestflow.Text = getstr[14];
                        textBoxstate.Text = getstr[15];
                        textBoxtemprature.Text = getstr[16];

                        Com.Send($"@START_INFO {textBoxFlowcaedno.Text}:" +
                                                    $"{textBoxmachine.Text}:" + //Machine
                                                    $"{textBoxcustom.Text}:" + //Custom
                                                    $"{textBoxproduct.Text}:" + //Product
                                                    $"{textBoxLotno.Text}:" +//LotNO
                                                    $"{textBoxdatecode.Text}:" +//DateCode
                                                    $"{textBoxfwversion.Text}:" +//FW Version
                                                    $"{textBoxcheckcode.Text}:" +//CheckCode
                                                    $"{textBoxmqty.Text}:" +//MQTY
                                                    $"{textBoxcode.Text}:" +//CODE
                                                    $"{textBoxcodecrc.Text}:" +//CODECRC
                                                    $"{textBoxstation.Text}:" +//Station
                                                    $"{textBoxmark.Text}:" +//Mark
                                                    $"{textBoxuser.Text}:" +//User
                                                    $"{textBoxtestflow.Text}:" +//TestFlow
                                                    $"{textBoxstate.Text}:" +//State
                                                    $"{textBoxtemprature.Text}+");  //Temperature
                    }
                    else
                    {
                        textBoxFlowcaedno.Text = "";
                        throw new Exception($"长度不等于17");
                    }
                }
                catch (Exception ex)
                {
                    textBoxFlowcaedno.Text = "";
                    MessageBox.Show($"批次信息{textBoxFlowcaedno.Text}解析异常：详细信息:" + ex.Message, "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                //if (keyflag == false) //防止长按输入
                //{
                //    keyflag = true;
                //    _dt = System.DateTime.Now;
                //    if (e.KeyCode == Keys.Control)
                //    {
                //        ld.Clear();
                //        textBoxFlowcaedno.Text = "";//清空
                //        return;
                //    }
                //    DateTime _tempDt = System.DateTime.Now;
                //    ld.Add(_tempDt);
                //}
            }
        }

        private void uiCheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (uiCheckBox1.Checked)
            {
                textBoxmqty.ReadOnly = false;
            }
            else
            {
                textBoxmqty.ReadOnly = true;
            }
        }

        private void RefreshCurrentTime()
        {
            lbl_currenttime.Text = DateTime.Now.ToString("HH:mm:ss");
            //if (H5509IsRuningTime) //显示运行时间标记
            //{
            //    TimeSpan timeSpan = DateTime.Now - machinestarttime;
            //    if (timeSpan.TotalDays >= 1) lbl_runtime.Text = "RunTime " + timeSpan.ToString(@"dd\.hh\:mm\:ss");
            //    else lbl_runtime.Text = "RunTime " + timeSpan.ToString(@"hh\:mm\:ss");
            //}
        }

        /// <summary>
        /// 三灯闪烁
        /// </summary>
        /// <param name="flicker"></param>
        private void RefreshLightState(bool flicker)
        {
            if (flicker)
            {
                Lbl_redlight.BackColor = Color.LightGray;
                Lbl_yellowlight.BackColor = Color.LightGray;
                Lbl_greenlight.BackColor = Color.LightGray;
            }
            else
            {
                if (isRedFlicker) Lbl_redlight.BackColor = Color.Red;
                if (isYellowFlicker) Lbl_yellowlight.BackColor = Color.Gold;
                if (isGreenFlicker) Lbl_greenlight.BackColor = Color.LimeGreen;
            }
        }

        /// <summary>
        /// 1s定时器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Currenttime_Tick(object sender, EventArgs e)
        {
            RefreshCurrentTime();
            RefreshLightState(flicker);
            flicker = !flicker;
        }

        /// <summary>
        /// 主界面刷新
        /// </summary>
        private void 屏幕刷新()
        {
            while (true)
            {
                try
                {
                    this.Invoke(new Action(() =>
                    {
                        if (设备状态 == 0)
                        {
                            if (H5509归零)
                            {
                                //if (OneCycleFlag) label_driverStatus.Text = "一个循环完成";
                                //else 
                                label_driverStatus.Text = "归零完成";
                                label_driverStatus.ForeColor = Color.Black;
                            }
                            else
                            {
                                if (H5509正在归零)
                                {
                                    label_driverStatus.Text = "归零中";
                                    label_driverStatus.ForeColor = Color.Gold;
                                }
                                else if (结批完成)
                                {
                                    label_driverStatus.Text = "结批完成";
                                    label_driverStatus.ForeColor = Color.Black;
                                }
                                else
                                {
                                    label_driverStatus.Text = "HALT";
                                    label_driverStatus.ForeColor = Color.Black;
                                }
                            }
                            isRedFlicker = false;
                            isYellowFlicker = true;
                            isGreenFlicker = false;
                        }
                        else if (设备状态 == 1)
                        {
                            //if (IsHoming)
                            //{
                            //    label_driverStatus.Text = "归零中";
                            //    label_driverStatus.ForeColor = Color.Gold;
                            //}
                            //else if (IsOneCycle)
                            //{
                            //    label_driverStatus.Text = "一个循环中";
                            //    label_driverStatus.ForeColor = Color.LimeGreen;
                            //}
                            //else 
                            if (正在结批)
                            {
                                label_driverStatus.Text = "结批中";
                                label_driverStatus.ForeColor = Color.LimeGreen;
                            }
                            else
                            {
                                label_driverStatus.Text = "Running";
                                label_driverStatus.ForeColor = Color.LimeGreen;
                            }
                            isRedFlicker = false;
                            isYellowFlicker = false;
                            isGreenFlicker = true;
                        }
                        else if (设备状态 == 2)
                        {
                            label_driverStatus.Text = "STOP";
                            label_driverStatus.ForeColor = Color.Red;
                            isRedFlicker = true;
                            isYellowFlicker = false;
                            isGreenFlicker = false;
                        }
                    }));
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"屏幕刷新异常：{ex.Message}");
                    Thread.Sleep(100);
                }
            }
        }

        private void _main_Load(object sender, EventArgs e)
        {
            //1s刷新一次当前时间
            currenttime = new System.Windows.Forms.Timer() { Interval = 1000 };
            currenttime.Tick += Currenttime_Tick;
            currenttime.Start(); //开始实时时间刷新

            RefreshData = Task.Run(() =>
            {
                屏幕刷新();
            });
        }
    }
}
