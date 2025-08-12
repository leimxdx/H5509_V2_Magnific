using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Sunny.UI;
using Sunny.UI.Win32;
using static H5509_V2.APPmaster;
using static H5509_V2.ForMain;
using static H5509_V2.WriteData;
using Color = System.Drawing.Color;
using Control = System.Windows.Forms.Control;

namespace H5509_V2
{
    public partial class _main : UIUserControl
    {
        public Task RefreshData;

        public MotionEx motionEx = MotionEx.GetInstance();

        public Handler handler = new Handler();

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

        private DateTime mousemovetime = DateTime.Now;

        private System.Drawing.Point windowsPoint = Control.MousePosition;

        private System.Timers.Timer windowstimer = new System.Timers.Timer();

        private System.Timers.Timer uphtimer = new System.Timers.Timer(); //uph定时器

        private System.Windows.Forms.Timer currenttime = new System.Windows.Forms.Timer();

        private System.Windows.Forms.Timer[] Chk_timer;

        private Label[] Lbl_Dout;

        private UICheckBox[] Chk_SW;

        private UIButton[] Btn_Open;

        private UIButton[] Btn_Close;

        private UIDigitalLabel[] Lbl_Temp;

        public Action 刷新界面权限;

        public _main()
        {
            InitializeComponent();

            Lbl_Dout = new Label[]
            {
                lbl_dout1, lbl_dout2, lbl_dout3, lbl_dout4, lbl_dout5, lbl_dout6, lbl_dout7, lbl_dout8, lbl_dout9, lbl_dout10,
                lbl_dout11, lbl_dout12, lbl_dout13, lbl_dout14, lbl_dout15, lbl_dout16, lbl_dout17, lbl_dout18, lbl_dout19, lbl_dout20,
                lbl_dout21, lbl_dout22, lbl_dout23, lbl_dout24, lbl_dout25, lbl_dout26, lbl_dout27, lbl_dout28, lbl_dout29, lbl_dout30,
                lbl_dout31, lbl_dout32, lbl_dout33, lbl_dout34, lbl_dout35, lbl_dout36, lbl_dout37, lbl_dout38, lbl_dout39, lbl_dout40,
                lbl_dout41, lbl_dout42, lbl_dout43, lbl_dout44, lbl_dout45, lbl_dout46, lbl_dout47, lbl_dout48, lbl_dout49, lbl_dout50,
                lbl_dout51, lbl_dout52, lbl_dout53, lbl_dout54, lbl_dout55, lbl_dout56, lbl_dout57, lbl_dout58, lbl_dout59, lbl_dout60,
                lbl_dout61, lbl_dout62, lbl_dout63, lbl_dout64, lbl_dout65, lbl_dout66, lbl_dout67, lbl_dout68, lbl_dout69, lbl_dout70,
                lbl_dout71, lbl_dout72, lbl_dout73, lbl_dout74, lbl_dout75, lbl_dout76, lbl_dout77, lbl_dout78, lbl_dout79, lbl_dout80,
                lbl_dout81, lbl_dout82, lbl_dout83, lbl_dout84, lbl_dout85, lbl_dout86, lbl_dout87, lbl_dout88, lbl_dout89, lbl_dout90,
                lbl_dout91, lbl_dout92, lbl_dout93, lbl_dout94, lbl_dout95, lbl_dout96, lbl_dout97, lbl_dout98, lbl_dout99, lbl_dout100,
                lbl_dout101, lbl_dout102, lbl_dout103, lbl_dout104, lbl_dout105, lbl_dout106, lbl_dout107, lbl_dout108, lbl_dout109, lbl_dout110,
                lbl_dout111, lbl_dout112, lbl_dout113, lbl_dout114, lbl_dout115, lbl_dout116, lbl_dout117, lbl_dout118, lbl_dout119, lbl_dout120,
                lbl_dout121, lbl_dout122, lbl_dout123, lbl_dout124, lbl_dout125, lbl_dout126, lbl_dout127, lbl_dout128,
            };

            Chk_SW = new UICheckBox[]
            {
                Chk_switch1, Chk_switch2, Chk_switch3, Chk_switch4,
                Chk_switch5, Chk_switch6, Chk_switch7, Chk_switch8,
                Chk_switch9, Chk_switch10, Chk_switch11, Chk_switch12,
                Chk_switch13, Chk_switch14, Chk_switch15, Chk_switch16,
            };

            Btn_Open = new UIButton[]
            {
                Btn_open1, Btn_open2, Btn_open3, Btn_open4,
                Btn_open5, Btn_open6, Btn_open7, Btn_open8,
                Btn_open9, Btn_open10, Btn_open11, Btn_open12,
                Btn_open13, Btn_open14, Btn_open15, Btn_open16,
            };

            Btn_Close = new UIButton[]
            {
                Btn_close1, Btn_close2, Btn_close3, Btn_close4,
                Btn_close5, Btn_close6, Btn_close7, Btn_close8,
                Btn_close9, Btn_close10, Btn_close11, Btn_close12,
                Btn_close13, Btn_close14, Btn_close15, Btn_close16,
            };

            Lbl_Temp = new UIDigitalLabel[]
            {
                uiDigitalLabel1,
                uiDigitalLabel2,
                uiDigitalLabel3,
                uiDigitalLabel4,
                uiDigitalLabel5,
                uiDigitalLabel6,
                uiDigitalLabel7,
                uiDigitalLabel8,
                uiDigitalLabel9,
                uiDigitalLabel10,
                uiDigitalLabel11,
                uiDigitalLabel12,
                uiDigitalLabel13,
                uiDigitalLabel14,
                uiDigitalLabel15,
                uiDigitalLabel16,
                uiDigitalLabel17,
            };
        }

        private void _main_Load(object sender, EventArgs e)
        {
            //1s刷新一次当前时间
            currenttime = new System.Windows.Forms.Timer() { Interval = 1000 };
            currenttime.Tick += Currenttime_Tick;
            currenttime.Start(); //开始实时时间刷新

            //鼠标实时位置捕捉定时器
            windowstimer.Interval = 200; //200ms
            windowstimer.Elapsed += WindowstimerTick;

            uphtimer.Interval = 10 * 1000; //10s
            uphtimer.Elapsed += uphtimeTick;
            uphtimer.Start();

            Chk_timer = new System.Windows.Forms.Timer[Chk_SW.Length];
            for (int i = 0; i < Chk_SW.Length; i++)
            {
                System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer() { Interval = 10000 }; //10s
                timer.Tick += TimerCloseSelcet;
                Chk_timer[i] = timer;
            }


            textBoxmachine.Text = Environment.MachineName;

            放料IC目标值 = new bool[][] { new bool[] { false, false, false, false }, new bool[] { false, false, false, false } }; //防止报错

            刷新开Site状态();

            StartSite数量 = 开Site数量;

            WindowsButtonBind(); //Open/Close按钮绑定

            刷新分BIN信息();

            刷新档案选项();

            刷新权限选项();

            RefreshData = Task.Run(() =>
            {
                屏幕刷新();
            });
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
                    if (uiCheckBox1.Checked && textBoxmqty.Text.Trim().Length == 0)
                    {
                        MessageBox.Show("请输入批次数量!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    string[] getstr = textBoxFlowcaedno.Text.Split(':');
                    LotNo = getstr;
                    if (getstr.Length == 17)
                    {
                        textBoxFlowcaedno.Text = getstr[0].Replace("-", "_"); //ZRS240929001A:
                        getstr[0] = textBoxFlowcaedno.Text;
                        getstr[1] = textBoxmachine.Text;
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
                        else
                        {
                            getstr[8] = textBoxmqty.Text; //如果是手动输入就不修改扫码出来的数量
                            uiCheckBox1.Checked = false;
                        }
                        textBoxcode.Text = getstr[9];
                        textBoxcodecrc.Text = getstr[10];
                        textBoxstation.Text = getstr[11];
                        textBoxmark.Text = getstr[12];
                        if (textBoxuser.Text.Trim().Length == 0)
                        {
                            textBoxuser.Text = getstr[13]; //如果用户没有输入就不修改扫码出来的用户 
                        }
                        else getstr[13] = textBoxuser.Text.Trim();
                        textBoxtestflow.Text = getstr[14];
                        textBoxstate.Text = getstr[15];
                        textBoxtemprature.Text = getstr[16];

                        SendLotNo();
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

        public void SendLotNo()
        {
            if (!Com.IsOpenCom)
            {
                MessageBox.Show("串口未连接!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        private void uphtimeTick(object sender, EventArgs eventArgs)
        {
            int uph = 0;

            if (设备状态 != 0 && UPHType == 0)
            {
                if (出料数 != UphNumber)
                {
                    uph = 3600 / 10 * (出料数 - UphNumber);

                    UphNumber = 出料数;
                }

                if (uph >= 0)
                {
                    lab_uph.Invoke(new Action(() =>
                    {
                        lab_uph.Text = $"UPH={uph}";
                    }));
                }
            }

            //RunLog($"UPH={uph}");
        }

        private void RefreshCurrentTime()
        {
            lbl_currenttime.Text = DateTime.Now.ToString("HH:mm:ss");
            if (设备状态 != 0) //显示运行时间标记
            {
                TimeSpan timeSpan = DateTime.Now - 开始时间;
                if (timeSpan.TotalDays >= 1) lbl_runtime.Text = "RunTime " + timeSpan.ToString(@"dd\.hh\:mm\:ss");
                else lbl_runtime.Text = "RunTime " + timeSpan.ToString(@"hh\:mm\:ss");
            }
        }

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

        private void Currenttime_Tick(object sender, EventArgs e)
        {
            RefreshCurrentTime();
            RefreshLightState(flicker);
            flicker = !flicker;
            if (设备状态 != 0 && UPHType == 1)
            {
                lab_uph.Invoke(new Action(() =>
                {
                    lab_uph.Text = $"UPH={RealUPHNum}";
                }));
                UphNumber = 出料数;
            }
        }

        private void TimerCloseSelcet(object sender, EventArgs e)
        {
            if (sender is System.Windows.Forms.Timer timer)
            {
                for (int i = 0; i < Chk_timer.Length; i++)
                {
                    if (timer.Equals(Chk_timer[i]))
                    {
                        Chk_timer[i].Stop();
                        Chk_SW[i].Checked = false;
                        Btn_Close[i].Enabled = false;
                        Btn_Open[i].Enabled = false;
                        break;
                    }
                }
            }
        }

        private void ClicKCloseButton(object sender, EventArgs e)
        {
            if (sender is UIButton button)
            {
                for (int i = 0; i < Btn_Close.Length; i++)
                {
                    if (button.Equals(Btn_Close[i]))
                    {
                        Chk_timer[i].Stop();
                        Chk_timer[i].Start();
                        设置信息.开site信息.SetSiteValue(i, "0");
                        string result = UpdateSiteStatus(设置信息.开site信息.Site名称, i + 1, 设置信息.开site信息.GetSiteValue(i));
                        if (result != "Success") { MessageBox.Show("保存失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error); break; }
                        result = GetSiteStatus(设置信息.开site信息.Site名称, out SiteStatus siteStatus);
                        if (result != "Success") { MessageBox.Show("读取失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error); break; }
                        设置信息.开site信息 = siteStatus;
                    }
                }
                刷新开Site状态();
            }
        }

        private void ClicKOpenButton(object sender, EventArgs e)
        {
            if (sender is UIButton button)
            {
                for (int i = 0; i < Btn_Open.Length; i++)
                {
                    if (button.Equals(Btn_Open[i]))
                    {
                        Chk_timer[i].Stop();
                        Chk_timer[i].Start();
                        设置信息.开site信息.SetSiteValue(i, "1");
                        string result = UpdateSiteStatus(设置信息.开site信息.Site名称, i + 1, 设置信息.开site信息.GetSiteValue(i));
                        if (result != "Success") { MessageBox.Show("保存失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error); break; }
                        result = GetSiteStatus(设置信息.开site信息.Site名称, out SiteStatus siteStatus);
                        if (result != "Success") { MessageBox.Show("读取失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error); break; }
                        设置信息.开site信息 = siteStatus;
                    }
                }
                刷新开Site状态();
            }
        }

        private void WindowsButtonBind()
        {
            for (int i = 0; i < Btn_Open.Length; i++)
            {
                Btn_Open[i].MouseClick += ClicKOpenButton;
            }
            for (int i = 0; i < Btn_Close.Length; i++)
            {
                Btn_Close[i].MouseClick += ClicKCloseButton;
            }
            for (int i = 0; i < Chk_SW.Length; i++)
            {
                Chk_SW[i].MouseClick += CheckChanged;
            }
            for (int i = 0; i < Lbl_Dout.Length; i++)
            {
                Lbl_Dout[i].MouseClick += DoutChanged;
            }
        }

        private void DoutChanged(Label label)
        {
            bool ischange = false;
            for (int i = 0; i < Lbl_Dout.Length; i++)
            {
                if (label.Equals(Lbl_Dout[i]))
                {
                    int site = i / 8;
                    if (!Chk_SW[site].Checked) break;
                    if (Chk_timer[site].Enabled)
                    {
                        Chk_timer[site].Stop();
                        Chk_timer[site].Start();
                    }
                    ischange = true;
                    int dut = i % 8;
                    设置信息.开site信息.SetSiteValue(site, dut, label.Text == "" ? "1" : "0");
                    string result = UpdateSiteStatus(设置信息.开site信息.Site名称, site + 1, 设置信息.开site信息.GetSiteValue(site));
                    if (result != "Success") { MessageBox.Show("保存失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error); break; }
                    result = GetSiteStatus(设置信息.开site信息.Site名称, out SiteStatus siteStatus);
                    if (result != "Success") { MessageBox.Show("读取失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error); break; }
                    设置信息.开site信息 = siteStatus;
                    break;
                }
            }
            if (ischange) 刷新开Site状态();
        }

        private void DoutChanged(object sender, EventArgs e)
        {
            if (sender is Label label)
            {
                DoutChanged(label);
            }
        }

        private void CheckChanged(object sender, EventArgs e)
        {
            if (sender is UICheckBox checkBox)
            {
                for (int i = 0; i < Chk_SW.Length; i++)
                {
                    if (checkBox.Equals(Chk_SW[i]))
                    {
                        if (checkBox.Checked)
                        {
                            Chk_timer[i].Start();
                            Btn_Open[i].Enabled = true;
                            Btn_Close[i].Enabled = true;
                        }
                        else
                        {
                            Chk_timer[i].Start();
                            Btn_Open[i].Enabled = false;
                            Btn_Close[i].Enabled = false;
                        }
                        break;
                    }
                }
            }
        }

        public void 刷新界面按钮权限()
        {
            canVisable(this);
            刷新界面权限();
        }

        public void canVisable(Control cons)
        {
            foreach (Control con in cons.Controls)
            {
                if (con.Tag != null)
                {
                    //根据操作权限控制visable与enable
                    if (con.Tag.ToString().Contains("#"))
                    {
                        string t = con.Tag.ToString();
                        t = t.Substring(t.IndexOf("#") + 1);
                        t = t.Substring(0, t.IndexOf("#"));

                        if (t.Contains("0"))
                        {
                            if (t.Contains(操作权限))
                            {
                                con.Visible = true;
                            }
                            else
                            {
                                con.Visible = false;
                                continue;
                            }
                        }
                        else //if (t.Contains("1"))没有设置默认都是1
                        {
                            if (t.Contains(操作权限))
                            {
                                con.Enabled = true;
                                if (ButtonName.Contains(con.Name))
                                {
                                    con.BackColor = Color.White;
                                }
                            }
                            else
                            {
                                con.Enabled = false;
                                if (ButtonName.Contains(con.Name))
                                {
                                    con.BackColor = Color.FromArgb(226, 226, 226);
                                }
                                continue;
                            }
                        }
                    }

                    //根据运行状态控制enable
                    if (con.Tag.ToString().Contains("@"))
                    {
                        string t = con.Tag.ToString();
                        t = t.Substring(t.IndexOf("@") + 1);
                        t = t.Substring(0, t.IndexOf("@"));
                        if (t.Contains(设备状态.ToString()))
                        {
                            if (ButtonName.Contains(con.Name))
                            {
                                con.BackColor = Color.White;
                            }
                            con.Enabled = true;
                        }
                        else
                        {
                            if (ButtonName.Contains(con.Name))
                            {
                                con.BackColor = Color.FromArgb(226, 226, 226);
                            }
                            con.Enabled = false;
                        }
                    }

                }
                if (con.Controls.Count > 0)
                {
                    canVisable(con);
                }
            }
        }

        public void 切换为操作员权限()
        {
            windowstimer.Stop();
            当前权限 = "Operator";
            操作权限 = "A";
            安全门禁用 = false;
            this.Invoke(new Action(() =>
            {
                //checkBox_doorbear.Checked = true;
                this.combox_juris.SelectedIndexChanged -= new System.EventHandler(this.combox_juris_SelectedIndexChanged);
                combox_juris.Text = 当前权限;
                this.combox_juris.SelectedIndexChanged += new System.EventHandler(this.combox_juris_SelectedIndexChanged);
                刷新界面按钮权限();
            }));
        }

        public void TempFirst(object sender, EventArgs e)
        {
            初次达温 = true;
            达温延时.Stop(); //初次达温延时
            RunLog("初次达温延时到");
        }

        private void WindowstimerTick(object sender, EventArgs eventArgs)
        {
            System.Drawing.Point mousePosition = Control.MousePosition;
            if (windowsPoint.X != mousePosition.X || windowsPoint.Y != mousePosition.Y)
            {
                mousemovetime = DateTime.Now;
                windowsPoint = mousePosition;
            }
            else //如果长时间没有动过鼠标
            {
                TimeSpan remainingTime = TimeSpan.FromMilliseconds(权限时间 * 1000) - (DateTime.Now - mousemovetime);
                if (remainingTime.Seconds < 0)
                {
                    切换为操作员权限();
                }
            }
            //this.Invoke(new Action(() =>
            //{
            //    label153.Text = $"鼠标坐标: X={mousePosition.X}, Y={mousePosition.Y}";
            //}));
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            //H5509归零 = false;
            bool canchange = true;
            bool ischange = false;
            if (设备状态 != 0)
            {
                canchange = false;
                MessageBox.Show($"请结批后更改档案!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            if (当前档案 == comboBox1.Text.Trim()) return;

            if (当前权限 == "Operator")
            {
                canchange = false;
                MessageBox.Show($"权限不足!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            if (canchange)
            {
                DialogResult dialogResult = MessageBox.Show($"请确认是否将工作档案更改为【{comboBox1.Text}】", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.OK)
                {
                    string result = UpdateRecord(comboBox1.Text); //(1)更新config当前档案
                    if (result == "Success")
                    {
                        result = GetconfigValue("当前档案", out string value); //(2)读取config当前档案
                        if (result == "Success")
                        {
                            result = GetSetupInfoData(value); //(3)读取当前档案信息
                            if (result == "Success") //(4)更新当前档案信息
                            {
                                result = Init_TrayPoint();
                                if (result == "Success") //(5)更新tray点位
                                {
                                    刷新分BIN信息();

                                    刷新开Site状态();
                                    刷新温度显示();
                                    Com.刷新串口信息();
                                    ischange = true;
                                    RunLog($"档案切换[{当前档案}]->[{comboBox1.Text.Trim()}]");
                                    当前档案 = value;
                                }
                                else
                                {
                                    RunLog($"更新Tray盘点位失败!" + result);
                                    MessageBox.Show($"更新Tray盘点位失败!" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                            else
                            {
                                RunLog($"读取当前档案信息失败!" + result);
                                MessageBox.Show($"读取当前档案信息失败!" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            RunLog($"读取当前档案失败!" + result);
                            MessageBox.Show($"读取当前档案失败!" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        RunLog($"更新档案失败!" + result);
                        MessageBox.Show($"更新档案失败!" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            if (!ischange)
            {
                this.comboBox1.SelectedValueChanged -= new System.EventHandler(this.comboBox1_SelectedValueChanged);
                comboBox1.Text = 当前档案;
                this.comboBox1.SelectedValueChanged += new System.EventHandler(this.comboBox1_SelectedValueChanged);
            }
        }

        private void combox_juris_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool ischange = false;

            if (当前权限 != combox_juris.Text)
            {
                DialogResult result = MessageBox.Show($"是否修改权限为({combox_juris.Text})", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    if (combox_juris.Text == "Operator")
                    {
                        当前权限 = combox_juris.Text;
                        操作权限 = "A";
                        windowstimer.Stop();
                        安全门禁用 = false; //checkBox_doorbear.Checked = true;
                    }
                    else
                    {
                        FrmInputPwd frmInputPwd = new FrmInputPwd();
                        if (frmInputPwd.ShowDialog() == DialogResult.OK)
                        {
                            string getpwd = GetPassWord(combox_juris.Text, out string 密码);
                            string gettime = GetPermTime(combox_juris.Text, out string 时间);
                            if (getpwd == "Success")
                            {
                                if (frmInputPwd.Password == 密码)
                                {
                                    当前权限 = combox_juris.Text;
                                    权限时间 = Convert.ToInt32(时间) * 60;
                                    ischange = true;

                                    switch (当前权限)
                                    {
                                        //case "operator": operation = "A"; break;
                                        case "Engineer":
                                            操作权限 = "B";
                                            windowstimer.Start();
                                            mousemovetime = DateTime.Now;
                                            break;
                                        case "Administrator":
                                            操作权限 = "C";
                                            windowstimer.Start();
                                            mousemovetime = DateTime.Now;
                                            break;
                                        case "Supplier":
                                            操作权限 = "D";
                                            windowstimer.Start();
                                            mousemovetime = DateTime.Now;
                                            break;
                                    }
                                }
                                else
                                {
                                    RunLog($"密码错误！");//{frmInputPwd.Password}
                                    MessageBox.Show($"密码错误！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);//{frmInputPwd.Password}
                                }
                            }
                            else
                            {
                                RunLog($"获取{combox_juris.Text}密码失败！" + getpwd);
                                MessageBox.Show($"获取{combox_juris.Text}密码失败！" + getpwd, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }

                }
            }

            if (ischange == false)
            {
                this.combox_juris.SelectedIndexChanged -= new System.EventHandler(this.combox_juris_SelectedIndexChanged);
                combox_juris.Text = 当前权限;
                this.combox_juris.SelectedIndexChanged += new System.EventHandler(this.combox_juris_SelectedIndexChanged);
            }

            刷新界面按钮权限();
        }

        public void 刷新档案选项()
        {
            string result = GetSetupInfoName(out DataTable dt);
            if (result != "Success")
            {
                RunLog("刷新档案选项失败!" + result);
                return;
            }
            comboBox1.DataSource = dt;
            comboBox1.DisplayMember = "档案名称";

            comboBox1.SelectedIndexChanged -= comboBox1_SelectedValueChanged;
            comboBox1.Text = 当前档案;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedValueChanged;
        }

        public void 刷新权限选项()
        {
            string result = GetJurisdictionName(out DataTable dt);
            if (result != "Success")
            {
                RunLog("刷新权限选项失败!" + result);
                return;
            }

            combox_juris.DataSource = dt;
            combox_juris.DisplayMember = "realName";
            当前权限 = "Operator";

            combox_juris.SelectedIndexChanged -= combox_juris_SelectedIndexChanged;
            comboBox1.Text = 当前权限;
            combox_juris.SelectedIndexChanged += combox_juris_SelectedIndexChanged;
        }

        public void 刷新开Site状态()
        {
            int open = 0;
            int close = 0;
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    string ison = 设置信息.开site信息.GetSiteValue(i, j);
                    if (ison == "1")
                    {
                        open++;
                        Lbl_Dout[i * 8 + j].Text = (j + 1).ToString();
                    }
                    else
                    {
                        close++;
                        Lbl_Dout[i * 8 + j].Text = "";
                    }
                }
            }

            开Site数量 = open;
            关Site数量 = close;

            bool[][] 修改前入料目标值 = new bool[2][]
            {
                new bool[] { 放料IC目标值[0][0], 放料IC目标值[0][1], 放料IC目标值[0][2], 放料IC目标值[0][3] },
                new bool[] { 放料IC目标值[1][0], 放料IC目标值[1][1], 放料IC目标值[1][2], 放料IC目标值[1][3] }
            };

            if (保存开Site更改 == null) 保存开Site更改 = new bool[16];
            bool[] 修改前开Site更改 = new bool[保存开Site更改.Length];

            for (int i = 0; i < 保存开Site更改.Length; i++)
            {
                修改前开Site更改[i] = 保存开Site更改[i];
            }

            更新入料目标值();

            更新关site更改(修改前入料目标值);

            更新开Site更改(修改前开Site更改);

            //飞梭前入料IC = new bool[][] { new bool[] { true, true, true, true }, new bool[] { true, true, true, true } };
            //飞梭后入料IC = new bool[][] { new bool[] { true, true, true, true }, new bool[] { true, true, true, true } };

            //正在放置前入料 = new bool[][] { new bool[] { true, true, true, true }, new bool[] { true, true, true, true } };

            if (设备状态 != 0)
            {
                bool[][] 关site更改 = 保存关Site更改;

                if (关site更改[0].Contains(true) || 关site更改[1].Contains(true))// 
                {
                    for (int cloum = 0; cloum < 关site更改.Length; cloum++)
                    {
                        if (!关site更改[cloum].Contains(true)) continue;

                        for (int dut = 0; dut < 关site更改[cloum].Length; dut++)
                        {
                            if (关site更改[cloum][dut])
                            {
                                if (飞梭后入料IC[cloum][dut] && !正在吸取后入料[cloum * 4 + dut])
                                {
                                    if (设备状态 == 1)
                                    {
                                        Listcode.Add($"5{cloum * 4 + dut + 43}500");
                                    }
                                    else MessageBox.Show($"后梭车入料:所有site的Dut{cloum * 4 + dut + 1}全部被关闭，IC无法被取出", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    RunLog($"后梭车入料:所有site的Dut{cloum * 4 + dut + 1}全部被关闭，IC无法被取出");
                                }
                                if (飞梭前入料IC[cloum][dut] || 正在放置前入料[cloum][dut]) //入料4个吸嘴
                                {
                                    if (设备状态 == 1)
                                    {
                                        Listcode.Add($"5{cloum * 4 + dut + 51}500");
                                    }
                                    else MessageBox.Show($"前梭车入料:所有site的Dut{cloum * 4 + dut + 1}全部被关闭，IC无法被取出{(正在放置前入料[cloum][dut] ? "(入料吸嘴上)" : "")}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    RunLog($"前梭车入料:所有site的Dut{cloum * 4 + dut + 1}全部被关闭，IC无法被取出{(正在放置前入料[cloum][dut] ? "(入料吸嘴上)" : "")}");
                                }
                            }
                        }
                    }
                }

                for (int i = 0; i < 修改前开Site更改.Length; i++)
                {
                    if (!修改前开Site更改[i] && 保存开Site更改[i])
                    {
                        RunLog($"新开Site{i}");

                        if (!InSiteQueue.Contains(i) //如果不在入料队列中
                            && !OutSiteQueue.Contains(i) //不在出料队列中
                            && (SiteTestState[i] == 0)
                            && (正在入料的Site != i))
                        //&& !正在吸取后入料.Contains(true)) //测试区有料待取走
                        {
                            InSiteQueue.Enqueue(i);
                            RunLog($"添加Site{i}");
                        }
                        else
                        {
                            RunLog("!InSiteQueue:" + !InSiteQueue.Contains(i));
                            RunLog("!OutSiteQueue:" + !OutSiteQueue.Contains(i));
                            RunLog($"SiteTestState[{i}]:" + SiteTestState[i]);
                            RunLog("site:" + 正在入料的Site);
                        }

                    }
                    else if (修改前开Site更改[i] && !保存开Site更改[i])
                    {
                        RunLog($"新全关Site{i}");
                    }
                }
            }
        }

        public void 刷新分BIN信息()
        {
            BinParam binParam = 设置信息.分BIN信息;

            textBox1.Text = Auto1 = binParam.auto1;
            textBox9.Text = Auto2 = binParam.auto2;
            //textBox3.Text = fix1 = motionEx.binParam.fix1;
            //textBox2.Text = fix2 = motionEx.binParam.fix2;
            //textBox4.Text = fix3 = motionEx.binParam.fix3;

            Fix1 = Fix1_1 = binParam.fix1.Split(';')[0];
            Fix1_2 = binParam.fix1.Split(';')[1];
            Fix1_3 = binParam.fix1.Split(';')[2];
            Fix2 = Fix2_1 = binParam.fix2.Split(';')[0];
            Fix2_2 = binParam.fix2.Split(';')[1];
            Fix2_3 = binParam.fix2.Split(';')[2];
            Fix3 = Fix3_1 = binParam.fix3.Split(';')[0];
            Fix3_2 = binParam.fix3.Split(';')[1];
            Fix3_3 = binParam.fix3.Split(';')[2];

            Fix1Num = binParam.fix1num;
            Fix2Num = binParam.fix2num;
            Fix3Num = binParam.fix3num;

            switch (binParam.fix1num)
            {
                case 1: textBox3.Text = Fix1_1; break;
                case 2:
                    {
                        textBox3.Text = "分区一：" + Fix1_1 + ",分区二：" + Fix1_2;
                        Fix1 += "," + Fix1_2;
                    }
                    break;
                case 3:
                    {
                        textBox3.Text = "分区一：" + Fix1_1 + ",分区二：" + Fix1_2 + ",分区三：" + Fix1_3;
                        Fix1 += "," + Fix1_2 + "," + Fix1_3;
                    }
                    break;
            }
            switch (binParam.fix2num)
            {
                case 1: textBox2.Text = Fix2_1; break;
                case 2:
                    {
                        textBox2.Text = "分区一：" + Fix2_1 + ",分区二：" + Fix2_2;
                        Fix2 += "," + Fix2_2;
                    }
                    break;
                case 3:
                    {
                        textBox2.Text = "分区一：" + Fix2_1 + ",分区二：" + Fix2_2 + ",分区三：" + Fix2_3;
                        Fix2 += "," + Fix2_2 + "," + Fix2_3;
                    }
                    break;
            }
            switch (binParam.fix3num)
            {
                case 1: textBox4.Text = Fix3_1; break;
                case 2:
                    {
                        textBox4.Text = "分区一：" + Fix3_1 + ",分区二：" + Fix3_2;
                        Fix3 += "," + Fix3_2;
                    }
                    break;
                case 3:
                    {
                        textBox4.Text = "分区一：" + Fix3_1 + ",分区二：" + Fix3_2 + ",分区三：" + Fix3_3;
                        Fix3 += "," + Fix3_2 + "," + Fix3_3;
                    }
                    break;
            }

            分bin.Clear();

            if (Auto1.Length > 0)
            {
                foreach (string s in Auto1.Split(','))
                {
                    if (s == "") continue;
                    分bin.Add(new 分Bin(s, "auto1", 0));
                }
            }
            if (Auto2.Length > 0)
            {
                foreach (string s in Auto2.Split(','))
                {
                    if (s == "") continue;
                    分bin.Add(new 分Bin(s, "auto2", 0));
                }
            }
            if (Fix1.Length > 0)
            {
                foreach (string s in Fix1.Split(','))
                {
                    if (s == "") continue;
                    分bin.Add(new 分Bin(s, "fix1", 0));
                }
            }
            if (Fix2.Length > 0)
            {
                foreach (string s in Fix2.Split(','))
                {
                    if (s == "") continue;
                    分bin.Add(new 分Bin(s, "fix2", 0));
                }
            }
            if (Fix3.Length > 0)
            {
                foreach (string s in Fix3.Split(','))
                {
                    if (s == "") continue;
                    分bin.Add(new 分Bin(s, "fix3", 0));
                }
            }
            int[] 分bin_fix = { Fix1Num, Fix2Num, Fix3Num };
            string[] 分bin_fix1 = { Fix1_1, Fix1_2, Fix1_3 };
            string[] 分bin_fix2 = { Fix2_1, Fix2_2, Fix2_3 };
            string[] 分bin_fix3 = { Fix3_1, Fix3_2, Fix3_3 };
            string[][] 分bin_fix_ = { 分bin_fix1, 分bin_fix2, 分bin_fix3 };

            for (int j = 0; j < 3; j++)
            {
                for (int i = 0; i < 分bin_fix[j]; i++)
                {
                    if (分bin_fix_[j][i].Length > 0)
                    {
                        foreach (string s in 分bin_fix_[j][i].Split(','))
                        {
                            分bin.Add(new 分Bin(s, "fix" + (j + 1) + "_" + (i + 1), 0));
                        }
                    }
                }
            }


            txt_auto1.BackColor = System.Drawing.Color.FromArgb(binParam.color1);
            txt_auto2.BackColor = System.Drawing.Color.FromArgb(binParam.color2);
            txt_fix1.BackColor = System.Drawing.Color.FromArgb(binParam.color3);
            txt_fix2.BackColor = System.Drawing.Color.FromArgb(binParam.color4);
            txt_fix3.BackColor = System.Drawing.Color.FromArgb(binParam.color5);

            label149.Text = "Auto1(" + binParam.name1 + ")";
            label150.Text = "Auto2(" + binParam.name2 + ")";
            label152.Text = "Fix1(" + binParam.name3 + ")";
            label151.Text = "Fix2(" + binParam.name4 + ")";
            label1590.Text = "Fix3(" + binParam.name5 + ")";
            label141.Text = "Auto1(" + binParam.name1 + ")";
            label143.Text = "Auto2(" + binParam.name2 + ")";
            label142.Text = "Fix1(" + binParam.name3 + ")";
            label144.Text = "Fix2(" + binParam.name4 + ")";
            label1.Text = "Fix3(" + binParam.name5 + ")";


        }

        public void 刷新批次信息()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    foreach (Control c in groupBox26.Controls)
                    {
                        if (c is TextBox)
                        {
                            if (c.Name == "textBoxmachine") (c as TextBox).Text = Environment.MachineName;
                            else (c as TextBox).Text = "";
                        }
                    }
                }));
            }
            else
            {
                foreach (Control c in groupBox26.Controls)
                {
                    if (c is TextBox)
                    {
                        if (c.Name == "textBoxmachine") (c as TextBox).Text = Environment.MachineName; //continue;
                        else (c as TextBox).Text = "";
                    }
                }
            }
        }

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
                            if (结批标记) //正在结批
                            {
                                label_driverStatus.Text = "结批中";
                                label_driverStatus.ForeColor = Color.LimeGreen;
                            }
                            else if (一个循环标记)
                            {
                                label_driverStatus.Text = "一个循环中";
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
                            if (一个循环完成)
                            {
                                label_driverStatus.Text = "一个循环完成";
                                label_driverStatus.ForeColor = Color.Black;
                            }
                            else
                            {
                                label_driverStatus.Text = "STOP";
                                label_driverStatus.ForeColor = Color.Red;
                            }
                            isRedFlicker = true;
                            isYellowFlicker = false;
                            isGreenFlicker = false;
                        }

                        lbl_opennum.Text = "Open=" + 开Site数量.ToString();
                        lbl_closenum.Text = "Close=" + 关Site数量.ToString();
                        label653.Text = $"IndexTime={DIndexTime.TotalSeconds:0.0}s";
                        label652.Text = $"TestTime={TestTime.TotalSeconds:0.0}s";

                        if (空跑模式)
                        {
                            Lbl_IClistenmode.Text = "空跑模式";
                            Lbl_IClistenmode.ForeColor = Color.Red;
                        }
                        else
                        {
                            Lbl_IClistenmode.Text = "正常模式";
                            Lbl_IClistenmode.ForeColor = Color.LimeGreen;
                        }

                        if (无测试机模式)
                        {
                            Lbl_connectstate.Text = "离线模式";
                            Lbl_connectstate.ForeColor = Color.Red;
                        }
                        else
                        {
                            Lbl_connectstate.Text = "在线模式";
                            Lbl_connectstate.ForeColor = Color.LimeGreen;
                        }

                        if (温度模式 == "高温")
                        {
                            label6.Text = "SokingTime=" + 获取加热倒数时间().TotalSeconds.ToString("0.0") + "s";

                            //label6.Text = "HeatingTime=" + 升温时间.ToString(@"hh\:mm\:ss");

                            //if (升温时间.TotalHours >= 1) label6.Text = "HeatingTime=" + 升温时间.ToString(@"hh\:mm\:ss");
                            //else label6.Text = "HeatingTime=" + 升温时间.ToString(@"mm\:ss");

                            if (温控加热)
                            {
                                if (label10.Text != "停止")
                                {
                                    label10.Text = "停止";
                                }
                            }
                        }

                        Txt_loadnum.Text = 载入数.ToString();
                        float total = 出料数;
                        txt_total.Text = total.ToString();
                        txt_auto1.Text = Auto1数量.ToString();
                        txt_auto2.Text = Auto2数量.ToString();
                        txt_fix1.Text = Fix1数量.ToString();
                        txt_fix2.Text = Fix2数量.ToString();
                        txt_fix3.Text = Fix3数量.ToString();

                        //double auto1 = total != 0 ? Math.Truncate((Auto1数量 / total * 10000)) / 100 : 0;
                        txt_auto1yield.Text = Auto1良率.ToString("0.00") + '%';
                        txt_auto2yield.Text = total != 0 ? (Auto2数量 / total * 100).ToString("0.00") + '%': "0.00%";
                        txt_fix1yield.Text = total != 0 ? (Fix1数量 / total * 100).ToString("0.00") + '%': "0.00%";
                        txt_fix2yield.Text = total != 0 ? (Fix2数量 / total * 100).ToString("0.00") + '%': "0.00%";
                        txt_fix3yield.Text = total != 0 ? (Fix3数量 / total * 100).ToString("0.00") + '%': "0.00%";

                        if (windowstimer.Enabled)
                        {
                            label153.Visible = true;
                            TimeSpan remainingTime = TimeSpan.FromMilliseconds(权限时间 * 1000) - (DateTime.Now - mousemovetime);
                            label153.Text = $"权限剩余时间: {(int)remainingTime.TotalSeconds} 秒";
                        }
                        else label153.Visible = false;

                        for (int i = 0; i < Lbl_Temp.Length; i++)
                        {
                            double value = 0;
                            Color color = Color.Silver;
                            if (TCPConnect && SiteTemp[i] != 32770)
                            {
                                value = SiteTemp[i];
                                if (温度模式 == "高温")
                                {
                                    if (GetSiteOpen(i).Contains(true))
                                    {
                                        if (SiteTemp[i] < 加热盘.波动温度上限 && SiteTemp[i] >= 加热盘.波动温度下限)
                                        {
                                            color = Color.Lime;
                                        }
                                        else
                                        {
                                            if (温控达温) color = Color.Red;
                                            else color = Color.Gold;
                                        }
                                    }
                                }
                            }
                            Lbl_Temp[i].Value = value;
                            Lbl_Temp[i].BackColor = color;
                        }

                        if (设备状态 != 0)
                        {
                            for (int site = 0; site < SiteTestIC.Count; site++)
                            {
                                bool[][] open = SiteTestIC[site];
                                显示Dout颜色(site, open, SiteTestBin[site]);
                            }
                        }
                    }));

                    Thread.Sleep(50);
                }
                catch (Exception ex)
                {
                    RunLog($"屏幕刷新异常：{ex.Message}");
                    Thread.Sleep(100);
                }
            }
        }

        public void 显示Dout颜色()
        {
            for (int site = 0; site < SiteTestIC.Count; site++)
            {
                bool[][] open = SiteTestIC[site];
                显示Dout颜色(site, open, SiteTestBin[site]);
            }
        }

        public void 显示Dout颜色(int site, bool[][] open, string[] bins)
        {
            for (int j = 0; j < open.Length; j++)
            {
                for (int k = 0; k < open[j].Length; k++)
                {
                    if (open[j][k])
                    {
                        int sitestate = SiteTestState[site];
                        if (sitestate == 1)
                        {
                            Lbl_Dout[(site * 8) + (j * 4 + k)].BackColor = Color.Green;
                        }
                        else if (sitestate == 2)
                        {
                            Lbl_Dout[(site * 8) + (j * 4 + k)].BackColor = Color.Yellow;
                        }
                        else if (sitestate == 3 || sitestate == 4)
                        {
                            string bin = bins[j * 4 + k];
                            if (Auto1.Contains(bin)) Lbl_Dout[(site * 8) + (j * 4 + k)].BackColor = Color.Lime;
                            else Lbl_Dout[(site * 8) + (j * 4 + k)].BackColor = Color.Red;
                        }
                        else
                        {
                            //Lbl_Dout[(site * 8) + (j * 4 + k)].BackColor = Color.Cyan;
                        }
                    }
                    else Lbl_Dout[(site * 8) + (j * 4 + k)].BackColor = Color.Cyan;
                }
            }
        }

        public void 刷新温度显示()
        {
            uiDigitalLabel18.Value = 设定温度;
            label116.Text = 温度模式;
            if (温度模式 == "常温")
            {
                if (label10.Text == "停止")
                {
                    string result = handler.加热盘加热关闭();
                    string result2 = handler.梭车加热关闭();
                    string result3 = handler.Site加热关闭();
                    if (result != "Success" || result2 != "Success" || result3 != "Success")
                    {
                        if (result != "Success") RunLog($"加热盘加热关闭失败!" + result);
                        if (result2 != "Success") RunLog($"梭车加热关闭失败!" + result2);
                        if (result3 != "Success") RunLog($"Site加热关闭失败!" + result3);
                        MessageBox.Show("停止加热失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                label10.Text = "关闭";
                label10.Enabled = false;
                label116.BackColor = Color.White;// System.Drawing.SystemColors.Control;
            }
            else
            {
                label10.Text = "开启";
                label10.Enabled = true;
                label116.BackColor = Color.Red;
            }

            if (达温延时.Interval != 达温后延时时间 * 1000)
            {
                达温延时.Interval = 达温后延时时间 * 1000;
            }

            foreach (var item in HeatDicTimer)
            {
                if (item.Value.Timer.Interval != IC加热时间 * 1000)
                {
                    item.Value.Timer.Interval = IC加热时间 * 1000;
                }
            }
            //设定温控器温度();
            //string result4 = 设定温控器温度();
            //if (result4 != "Success")
            //{
            //    Listcode.Add("1400447");
            //    RunLog("[Code]1400447,[Msg]设定温度失败");
            //}

            //RunLog($"开始设定温度({设定温度}):" + DateTime.Now.ToString());
            //string result4 = dTMHelper.WriteSv(2, 0, 设定温度);
            //if (result4 == "Success")
            //{
            //    string result5 = dTMHelper.WriteSv(1, 设定温度);
            //    string result6 = dTMHelper.WriteSv(0, 设定温度);
            //    if (result5 != "Success" || result6 != "Success")
            //    {
            //        if (result5 != "Success") RunLog($"模块2设定温度失败!" + result5);
            //        if (result6 != "Success") RunLog($"模块1设定温度失败!" + result6);
            //        MessageBox.Show("设定温度失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //        return;
            //    }
            //}
            //else
            //{
            //    RunLog($"设定温度失败!" + result4);
            //    MessageBox.Show("模块3设定温度失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return;
            //}

            //this.uiDoubleUpDown14.ValueChanged -= new Sunny.UI.UIDoubleUpDown.OnValueChanged(this.uiDoubleUpDown14_ValueChanged);
            //uiDoubleUpDown14.Value = 设定温度;
            //this.uiDoubleUpDown14.ValueChanged += new Sunny.UI.UIDoubleUpDown.OnValueChanged(this.uiDoubleUpDown14_ValueChanged);
        }


        //private void label116_Click(object sender, EventArgs e)
        //{
        //    if (设备状态 == 0)
        //    {
        //        if (label116.Text == "常温")
        //        {
        //            DialogResult dialog = MessageBox.Show($"请确认是否切换为高温模式", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
        //            if (dialog == DialogResult.OK)
        //            {
        //                温度模式 = "高温";
        //                label10.Text = "开启";
        //                label10.Enabled = true;
        //                label116.Text = 温度模式;
        //                label116.BackColor = Color.Red;
        //            }
        //        }
        //        else
        //        {
        //            DialogResult dialog = MessageBox.Show($"请确认是否切换为常温模式", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
        //            if (dialog == DialogResult.OK)
        //            {
        //                if (label10.Text == "停止")
        //                {
        //                    string result = motionEx.SetDo("Y095_加热盘加热", false);
        //                    if (result != "Success")
        //                    {
        //                        MessageBox.Show("停止加热失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //                        return;
        //                    }
        //                }
        //                温度模式 = "常温";
        //                label10.Text = "开启";
        //                label10.Enabled = false;
        //                label116.Text = 温度模式;
        //                label116.BackColor = System.Drawing.SystemColors.Control;
        //            }
        //        }
        //    }
        //}


        private void label10_Click(object sender, EventArgs e)
        {
            if (设备状态 != 1)
            {
                if (label10.Text == "停止")
                {
                    DialogResult dialog = MessageBox.Show($"请确认是否停止加热", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                    if (dialog == DialogResult.OK)
                    {
                        string result = handler.加热盘加热关闭();
                        string result2 = handler.梭车加热关闭();
                        string result3 = handler.Site加热关闭();
                        if (result == "Success" && result2 == "Success" && result3 == "Success")
                        {
                            label10.Text = "开启";
                            温控加热 = false;
                            温控达温 = false;
                        }
                        else
                        {
                            MessageBox.Show("停止加热失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                else
                {
                    DialogResult dialog = MessageBox.Show($"请确认是否开启加热", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                    if (dialog == DialogResult.OK)
                    {
                        string result = handler.加热盘加热开启();
                        string result2 = H5509归零 ? handler.梭车加热开启() : "Success";
                        string result3 = handler.Site加热开启();
                        if (result == "Success" && result2 == "Success" && result3 == "Success")
                        {
                            label10.Text = "停止";
                            温控加热 = true;
                            温控达温 = false;
                        }
                        else
                        {
                            MessageBox.Show("开启加热失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void label139_Click(object sender, EventArgs e)
        {
            LoadMap loadMap = new LoadMap();

            loadMap.LoadXPoint = LoaderXPoint;
            loadMap.LoadYPoint = LoaderYPoint;
            loadMap.LoadDicPoint = LoaderDicPoint;

            loadMap.ShowDialog();
        }

        private void uiDoubleUpDown14_ValueChanged(object sender, double value)
        {
            if (设定温度 != value)
            {
                DialogResult dialog = MessageBox.Show($"请确认是否将温度设置为【{value}】℃", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (dialog == DialogResult.OK)
                {
                    string result = dTMHelper.WriteSv(2, 0, value);
                    if (result != "Success")
                    {
                        MessageBox.Show($"温度设置为【{value}】℃失败!\n详细信息：" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        //this.uiDoubleUpDown14.ValueChanged -= new Sunny.UI.UIDoubleUpDown.OnValueChanged(this.uiDoubleUpDown14_ValueChanged);
                        //uiDoubleUpDown14.Value = 设定温度;
                        //this.uiDoubleUpDown14.ValueChanged += new Sunny.UI.UIDoubleUpDown.OnValueChanged(this.uiDoubleUpDown14_ValueChanged);
                    }
                    else
                    {
                        设定温度 = value;
                    }
                }
                else
                {
                    //this.uiDoubleUpDown14.ValueChanged -= new Sunny.UI.UIDoubleUpDown.OnValueChanged(this.uiDoubleUpDown14_ValueChanged);
                    //uiDoubleUpDown14.Value = 设定温度;
                    //this.uiDoubleUpDown14.ValueChanged += new Sunny.UI.UIDoubleUpDown.OnValueChanged(this.uiDoubleUpDown14_ValueChanged);
                }
            }
        }

        private void uiDigitalLabel17_Click(object sender, EventArgs e)
        {
            HeatMap heatMap = new HeatMap();

            heatMap.ShowDialog();
        }

        //public void bin(string b)
        //{
        //    for (int i = 0; i < 分bin.Count; i++)
        //    {
        //        if (分bin[i].bin == b) 分bin[i].num++;
        //    }
        //}

        private void Lbl_IClistenmode_Click(object sender, EventArgs e)
        {
            //bool[] tempopen = new bool[17];
            //if (tempopen[16]) //如果加热盘已经达温则开始延时
            //{
            //    if (!初次达温) //初次达温开启延时定时器
            //    {
            //        if (!达温延时.Enabled)
            //        {
            //            RunLog("初次达温延时开始");
            //            达温延时.Start();
            //        }
            //    }
            //}
            //if (!tempopen.Contains(false)) //有false(没有达温:False,达温/探头异常：true)说明还有Site没有达温
            //{
            //    if (初次达温)
            //    {
            //        温控达温 = true;
            //        达温时间 = DateTime.Now;
            //        RunLog("加热盘已达温");
            //        RunLog("达温时间：" + 达温时间.ToString("yyyy-MM-dd HH:mm:ss"));
            //        RunLog("升温时间：" + 升温时间.ToString(@"hh\:mm\:ss"));
            //    }
            //}
            //else
            //{
            //    TimeSpan timeSpan = DateTime.Now - 开始时间;
            //    if (timeSpan.TotalSeconds + 1800 > 加热盘.升温超时报警)
            //    {
            //        Listcode.Add($"1400446");
            //        RunLog($"[Code]1400446,[Msg]升温超时!", LogType.AlarmLog);
            //        ProgramPause = true; //如果升温超时则触发停止
            //        IsPause = true;
            //    }
            //}
            //int iscan = HeatInterval;
            //if (!handler.CheckAircyStatus())
            //{
            //    Debug.WriteLine("气缸异常");
            //}
            //bool[] tempopen; //bool[] 用于存储每个Site的温度是否达温或异常
            //double tempwavemax = 加热盘.波动温度上限;
            //double tempwavemin = 加热盘.波动温度下限;
            //if (!温控达温)
            //{
            //    tempopen = 检测温度是否达温(tempwavemax, tempwavemin);
            //    if (!tempopen.Contains(false)) //有false(没有达温:False,达温/探头异常：true)说明还有Site没有达温
            //    {
            //        if (!初次达温) //达温后开启延时定时器
            //        {
            //            if (!达温延时.Enabled)
            //            {
            //                RunLog("初次达温延时开始");
            //                达温延时.Start();
            //            }
            //        }
            //        else
            //        {
            //            温控达温 = true;
            //            达温时间 = DateTime.Now;
            //            RunLog("加热盘已达温");
            //            RunLog("达温时间：" + 达温时间.ToString("yyyy-MM-dd HH:mm:ss"));
            //            RunLog("升温时间：" + 升温时间.ToString(@"hh\:mm\:ss"));
            //        }
            //    }
            //}
            //else
            //{
            //    tempopen = 检测温度是否异常(tempwavemax, tempwavemin);
            //    if (tempopen.Contains(true)) //有true说明有Site超出波动范围
            //    {
            //        for (int site = 0; site < SiteTemp.Length; site++)
            //        {
            //            if (tempopen[site] && SiteTemp[site] != 32770)
            //            {
            //                double temp = SiteTemp[site];
            //                if (temp > 加热盘.波动温度上限)
            //                {
            //                    Listcode.Add($"8{site:D2}319");
            //                    if (site == 16) RunLog($"[Code]8{site:D2}319,[Msg]温控器:温度[{temp}]℃高于上限[{加热盘.波动温度上限}]℃", LogType.AlarmLog);
            //                    else RunLog($"[Code]8{site:D2}319,[Msg]Site{site + 1}:温度[{temp}]℃高于上限[{加热盘.波动温度上限}]℃", LogType.AlarmLog);
            //                }
            //                else if (temp <= 加热盘.波动温度下限)
            //                {
            //                    Listcode.Add($"8{site:D2}319");
            //                    if (site == 16) RunLog($"[Code]8{site:D2}319,[Msg]温控器:温度[{temp}]℃低于下限[{加热盘.波动温度下限}]℃", LogType.AlarmLog);
            //                    else RunLog($"[Code]8{site:D2}319,[Msg]Site{site + 1}:温度[{temp}]℃低于下限[{加热盘.波动温度下限}]℃", LogType.AlarmLog);
            //                }
            //            }
            //        }
            //        ProgramPause = true;
            //        IsPause = true;
            //    }
            //}

            #region 温度调试代码 保存下
            //bool[] tempopen;
            //double tempwavemax = 加热盘.波动温度上限;
            //double tempwavemin = 加热盘.波动温度下限;

            //if (!温控达温)
            //{
            //    tempopen = 检测温度是否达温(tempwavemax, tempwavemin);
            //    if (!tempopen.Contains(false)) //有false(没有达温:False,达温/探头异常：true)说明还有Site没有达温
            //    {
            //        if (!初次达温) //达温后开启延时定时器
            //        {
            //            if (!达温延时.Enabled)
            //            {
            //                RunLog("初次达温延时开始");
            //                达温延时.Start();
            //            }
            //        }
            //        else
            //        {
            //            温控达温 = true;
            //            达温时间 = DateTime.Now;
            //            RunLog("加热盘已达温");
            //            RunLog("达温时间：" + 达温时间.ToString("yyyy-MM-dd HH:mm:ss"));
            //            RunLog("升温时间：" + 升温时间.ToString(@"hh\:mm\:ss"));
            //        }
            //    }
            //}
            //else
            //{
            //    tempopen = 检测温度是否异常(tempwavemax, tempwavemin);
            //    if (tempopen.Contains(true)) //有true说明有Site超出波动范围
            //    {
            //        for (int site = 0; site < SiteTemp.Length; site++)
            //        {
            //            if (tempopen[site] && SiteTemp[site] != 32770)
            //            {
            //                double temp = SiteTemp[site];
            //                if (temp > 加热盘.波动温度上限)
            //                {
            //                    //Listcode.Add($"8{site:D2}319");
            //                    if (site == 16) RunLog($"[Code]8{site:D2}319,[Msg]温控器:温度[{temp}]℃高于上限[{加热盘.波动温度上限}]℃", LogType.AlarmLog);
            //                    else RunLog($"[Code]8{site:D2}319,[Msg]Site{site + 1}:温度[{temp}]℃高于上限[{加热盘.波动温度上限}]℃", LogType.AlarmLog);
            //                }
            //                else if (temp <= 加热盘.波动温度下限)
            //                {
            //                    //Listcode.Add($"8{site:D2}319");
            //                    if (site == 16) RunLog($"[Code]8{site:D2}319,[Msg]温控器:温度[{temp}]℃低于下限[{加热盘.波动温度下限}]℃", LogType.AlarmLog);
            //                    else RunLog($"[Code]8{site:D2}319,[Msg]Site{site + 1}:温度[{temp}]℃低于下限[{加热盘.波动温度下限}]℃", LogType.AlarmLog);

            //                }
            //            }
            //        }
            //        //ProgramPause = true;
            //        //IsPause = true;
            //    }
            //}
            //飞梭后入料IC = new bool[][] { new bool[] { true, false, false, false }, new bool[] { false, true, false, false } };
            //飞梭前入料IC = new bool[][] { new bool[] { false, false, true, false }, new bool[] { false, false, false, true } };
            ////检测梭车是否有IC遗留
            //if (飞梭后入料IC[0].Contains(true) || 飞梭后入料IC[1].Contains(true)) //飞梭后入料IC第1/2列无料
            //{
            //    for (int i = 0; i < 飞梭后入料IC[0].Length; i++)
            //    {
            //        if (飞梭后入料IC[0][i] || 飞梭后入料IC[1][i])
            //        {
            //            if (飞梭2位置 == 0)
            //            {
            //                if (飞梭后入料IC[0][i])
            //                {
            //                    Listcode.Add($"5{i + 47}0102");
            //                    RunLog($"[Code][Msg]后梭车(梭车2)Dut{i + 1:D2}有IC遗留", LogType.AlarmLog);
            //                }
            //                if (飞梭后入料IC[1][i])
            //                {
            //                    Listcode.Add($"5{i + 47}0103");
            //                    RunLog($"[Code][Msg]后梭车(梭车2)Dut{i + 5:D2}有IC遗留", LogType.AlarmLog);
            //                }
            //            }
            //            else
            //            {
            //                if (飞梭后入料IC[0][i])
            //                {
            //                    Listcode.Add($"5{i + 47}0100");
            //                    RunLog($"[Code][Msg]后梭车(梭车1)Dut{i + 1:D2}有IC遗留", LogType.AlarmLog);
            //                }
            //                if (飞梭后入料IC[1][i])
            //                {
            //                    Listcode.Add($"5{i + 47}0101");
            //                    RunLog($"[Code][Msg]后梭车(梭车1)Dut{i + 5:D2}有IC遗留", LogType.AlarmLog);
            //                }
            //            }
            //        }
            //    }
            //}

            //if (飞梭前入料IC[0].Contains(true) || 飞梭前入料IC[1].Contains(true)) //飞梭前入料IC第1/2列无料
            //{
            //    for (int i = 0; i < 飞梭前入料IC[0].Length; i++)
            //    {
            //        if (飞梭前入料IC[0][i] || 飞梭前入料IC[1][i])
            //        {
            //            if (飞梭2位置 == 0)
            //            {
            //                if (飞梭前入料IC[0][i])
            //                {
            //                    Listcode.Add($"5{i + 43}0100");
            //                    RunLog($"[Code][Msg]前梭车(梭车1)Dut{i + 1:D2}有IC遗留", LogType.AlarmLog);
            //                }
            //                if (飞梭前入料IC[1][i])
            //                {
            //                    Listcode.Add($"5{i + 43}0101");
            //                    RunLog($"[Code][Msg]前梭车(梭车1)Dut{i + 5:D2}有IC遗留", LogType.AlarmLog);
            //                }
            //            }
            //            else
            //            {
            //                if (飞梭前入料IC[0][i])
            //                {
            //                    Listcode.Add($"5{i + 43}0102");
            //                    RunLog($"[Code][Msg]前梭车(梭车2)Dut{i + 1:D2}有IC遗留", LogType.AlarmLog);
            //                }
            //                if (飞梭前入料IC[1][i])
            //                {
            //                    Listcode.Add($"5{i + 43}0103");
            //                    RunLog($"[Code][Msg]前梭车(梭车2)Dut{i + 5:D2}有IC遗留", LogType.AlarmLog);
            //                }
            //            }
            //        }
            //    }
            //}
            //飞梭后入料IC = new bool[][] { new bool[] { true, false, false, false }, new bool[] { false, true, false, false } };
            //飞梭前入料IC = new bool[][] { new bool[] { false, false, true, false }, new bool[] { false, false, false, true } };
            ////检测梭车是否有IC遗留
            //if (飞梭后入料IC[0].Contains(true) || 飞梭后入料IC[1].Contains(true)) //飞梭后入料IC第1/2列无料
            //{
            //    for (int i = 0; i < 飞梭后入料IC[0].Length; i++)
            //    {
            //        if (飞梭后入料IC[0][i] || 飞梭后入料IC[1][i])
            //        {
            //            if (飞梭2位置 == 0)
            //            {
            //                Listcode.Add($"5{i + 47}0102");
            //                Listcode.Add($"5{i + 47}0103");
            //            }
            //            else
            //            {
            //                Listcode.Add($"5{i + 47}0100");
            //                Listcode.Add($"5{i + 47}0101");
            //            }
            //        }
            //    }
            //}

            //if (飞梭前入料IC[0].Contains(true) || 飞梭前入料IC[1].Contains(true)) //飞梭前入料IC第1/2列无料
            //{
            //    for (int i = 0; i < 飞梭前入料IC[0].Length; i++)
            //    {
            //        if (飞梭前入料IC[0][i] || 飞梭前入料IC[1][i])
            //        {
            //            if (飞梭2位置 == 0)
            //            {
            //                Listcode.Add($"5{i + 43}0100");
            //                Listcode.Add($"5{i + 43}0101");
            //            }
            //            else
            //            {
            //                Listcode.Add($"5{i + 43}0102");
            //                Listcode.Add($"5{i + 43}0103");
            //            }
            //        }
            //    }
            //}
            //    double
            //    if (!温控达温)
            //    {
            //        if (!tempopen.Contains(false)) //有false(没有达温:False,达温/探头异常：true)说明还有Site没有达温
            //        {
            //            if (!初次达温) //达温后开启延时定时器
            //            {
            //                if (!达温延时.Enabled)
            //                {
            //                    达温延时.Start();
            //                }
            //            }
            //            else
            //            {
            //                温控达温 = true;
            //                达温时间 = DateTime.Now;
            //                RunLog("加热盘已达温");
            //                RunLog("达温时间：" + 达温时间.ToString("yyyy-MM-dd HH:mm:ss"));
            //                RunLog("升温时间：" + 升温时间.ToString(@"hh\:mm\:ss"));
            //            }
            //        }
            //    }
            //    else
            //    {
            //        if (tempopen.Contains(false)) //有false说明有Site超出波动范围
            //        {
            //            for (int site = 0; site < SiteTemp.Length; site++)
            //            {
            //                if (SiteTemp[site] != 32770)
            //                {
            //                    double temp = SiteTemp[site];
            //                    if (temp > 加热盘.波动温度上限)
            //                    {
            //                        Listcode.Add($"8{site:D2}319");
            //                        if (site == 16) RunLog($"[Code]8{site:D2}319,[Msg]温控器:温度[{temp}]℃高于上限[{加热盘.波动温度上限}]℃", LogType.AlarmLog);
            //                        else RunLog($"[Code]8{site:D2}319,[Msg]Site{site + 1}:温度[{temp}]℃高于上限[{加热盘.波动温度上限}]℃", LogType.AlarmLog);
            //                    }
            //                    else if (temp <= 加热盘.波动温度下限)
            //                    {
            //                        Listcode.Add($"8{site:D2}319");
            //                        if (site == 16) RunLog($"[Code]8{site:D2}319,[Msg]温控器:温度[{temp}]℃低于下限[{加热盘.波动温度下限}]℃", LogType.AlarmLog);
            //                        else RunLog($"[Code]8{site:D2}319,[Msg]Site{site + 1}:温度[{temp}]℃低于下限[{加热盘.波动温度下限}]℃", LogType.AlarmLog);

            //                    }
            //                }
            //            }
            //            ProgramPause = true;
            //            IsPause = true;
            //        }
            //    }
            //}

            //bool[] tempopen = 检测温度探头状态();
            //bool[] tempopen = new bool[] { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true };
            //if (tempopen.Contains(true))
            //{
            //    for (int i = 0; i < tempopen.Length; i++)
            //    {
            //        if (tempopen[i])
            //        {
            //            Listcode.Add($"8{i:D2}219");
            //            if (i == 15) RunLog($"[Code]8{i:D2}219,[Msg]加热盘温度异常!" , LogType.AlarmLog);
            //            else RunLog($"[Code]8{i:D2}219,[Msg]Site{i}温度异常!" , LogType.AlarmLog);
            //        }
            //    }
            //    //ProgramPause = true; //如果有温度异常则触发停止
            //    //IsPause = true;
            //}

            ////保存工单dut信息
            //string result = SQLiteHelper.runSql(SQLiteRunningString, "Select * From DutCurrent", out DataTable dataTable);
            //if (result == "Success")
            //{
            //    string tableName = $"dut_{LotNo[0]}" + "_" + 开始时间.ToString("yyyyMMdd_HH_mm_ss");
            //    int line = SqlServer.runSqlNonQuery(SQLServerConnString, $"create table {tableName}(id nchar(10) not NULL,pass int not NULL,total int not NULL,usedTime varchar(50) not NULL)");
            //    if (line >= -1)
            //    {
            //        SqlServer.runSql(SQLServerConnString, dataTable, tableName);
            //    }
            //}

            //for (int i = 0; i < Auto2XPoint.Length; i++)
            //{
            //    for (int j = 0; j < Auto2YPoint.Length; j++)
            //    {
            //        Auto2DicPoint[$"{i},{j}"] = true;
            //        Auto2DicBin[$"{i},{j}"] = $"{i}{j}";
            //    }
            //}
            ////int line = SQLiteHelper.runSqlNonQuery(SQLiteRunningString, $"UPDATE DutAll SET pass = pass + (SELECT dc.pass FROM DutCurrent dc WHERE dc.id = DutAll.id), total = total + (SELECT dc.total FROM DutCurrent dc WHERE dc.id = DutAll.id), usedTime = CAST(usedTime AS INTEGER) + (SELECT dc.usedTime FROM DutCurrent dc WHERE dc.id = DutAll.id) WHERE EXISTS ( SELECT 1 FROM DutCurrent dc WHERE dc.id = DutAll.id)");
            ////Debug.WriteLine(line);
            //int line = SqlServer.runSqlNonQuery(SQLServerConnString, $"create table f_{LotNo[0] + "_" + 开始时间.ToString("yyyyMMdd_HH_mm_ss")}(批次开始日期 nvarchar(50) NULL,获取MAP时间 nvarchar(50) NULL,料号 nvarchar(50) NULL,批号 nvarchar(20) NULL,MAP信息 nvarchar(50) NULL )");
            //int line1 = SqlServer.runSqlNonQuery(SQLServerConnString, $"create table o_{LotNo[0] + "_" + 开始时间.ToString("yyyyMMdd_HH_mm_ss")}(时间 nvarchar(50) NULL,名称 nvarchar(50) NULL,大小 nvarchar(50) NULL,数据 nvarchar(MAX) NULL)");

            ////SQLiteHelper.runSqlNonQuery(SQLiteRunningString, "update DutCurrent set pass=0,total=0,usedTime=0");
            ////int line = SqlServer.runSqlNonQuery(SQLServerConnString, $"create table f_ZRS240929001A_20250609_10_26_46(批次开始日期 nvarchar(50) NULL,获取MAP时间 nvarchar(50) NULL,料号 nvarchar(50) NULL,批号 nvarchar(20) NULL,MAP信息 nvarchar(50) NULL )");
            ////int line1 = SqlServer.runSqlNonQuery(SQLServerConnString, $"create table o_ZRS240929001A_20250609_10_26_46(时间 nvarchar(50) NULL,名称 nvarchar(50) NULL,大小 nvarchar(50) NULL,数据 nvarchar(MAX) NULL)");

            //if (line <= -2)
            //{
            //    //isstart = false;
            //    Listcode.Add("1400330");
            //    RunLog("[Code]1400330,[Msg]创建MAP信息数据库失败!", LogType.AlarmLog);
            //}

            //if (line1 <= -2)
            //{
            //    //isstart = false;
            //    Listcode.Add("1400330");
            //    RunLog("[Code]1400330,[Msg]创建BIN信息数据库失败!", LogType.AlarmLog);
            //}

            //Auto1TrayClear();
            //Auto2TrayClear();
            //Fix1TrayClear();
            //Fix2TrayClear();
            //Fix3TrayClear();

            // string[] bins = new string[] { "01", "01", "01", "01", "01", "01", "01", "01" };

            //string bb = "@TEST_DONE " + 0.ToString("D2");
            //foreach (string s in bins)
            //{
            //    bb += " " + s;
            //}
            //string sql = $"insert into f_{LotNo[0] + "_" + 开始时间.ToString("yyyyMMdd_HH_mm_ss")}(批次开始日期,获取MAP时间,料号,批号,MAP信息)" +
            //$"values('{开始时间.ToString("D")}','{DateTime.Now.ToString()}','{LotNo[3]}','{LotNo[4]}','{bb}')";
            //Task<int> task = SqlServer.runSqlNonQueryAsync(SQLServerConnString, sql);

            //foreach (string b in bins)
            //{
            //    bin(b);
            //}

            //string result = SQLiteHelper.runSql(SQLiteRunningString, "select id from SocketUse where num>" + 设置信息.测试机信息.socket使用次数, out List<object> res);

            //try
            //{
            //   result = SQLiteHelper.runSql(SQLiteRunningString, "select count(*) from SocketUse where num>" + 设置信息.测试机信息.socket使用次数, out string value);
            //    //object su = await SQLiteHelper.runSql(SQLiteRunningString, "select count(*) from SocketUse where num>" + motionEx.testerIFParam.socket使用次数);
            //    bool isint = int.TryParse(value.ToString(), out int socketnum);
            //    if (isint)
            //    {
            //        if (socketnum > Socket使用次数超限数)
            //        {
            //            Listcode.Add("1400600");
            //            Socket使用次数超限数 = socketnum;
            //        }
            //        else
            //        {
            //            if (socketnum == 0)
            //            {
            //                Socket使用次数超限数 = 0;
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex) { RunLog("读取socket使用次数超限数异常：" + ex.Message); }

            //            刷新分BIN信息();

            //SQLiteHelper.runSqlNonQuery(SQLiteRunningString, "update DutCurrent set pass=0,total=0,usedTime=0");
            //SqlServer.runSqlNonQuery(SQLServerConnString, $"create table f_{LotNo[0] + "_" + 开始时间.ToString("yyyyMMdd_HH_mm_ss")}(批次开始日期 nvarchar(50) NULL,获取MAP时间 nvarchar(50) NULL,料号 nvarchar(50) NULL,批号 nvarchar(20) NULL,MAP信息 nvarchar(50) NULL )");
            //SqlServer.runSqlNonQuery(SQLServerConnString, $"create table o_{LotNo[0] + "_" + 开始时间.ToString("yyyyMMdd_HH_mm_ss")}(时间 nvarchar(50) NULL,名称 nvarchar(50) NULL,大小 nvarchar(50) NULL,数据 nvarchar(MAX) NULL)");

            //RunLog("调试操作log",LogType.OperateLog);
            //RunLog("调试报警log", LogType.AlarmLog);
            //int passnum = 10;
            //int failnum = 19;
            ////RunLog($"update dutCurrent set pass={passnum},total={failnum + passnum},usedTime=CAST(usedTime as bigint)+{Com.listtesttimespan[sitenum].Ticks} where Id='{doutstr}'");
            //string sql = $"update DutCurrent set pass={passnum},total={failnum + passnum},usedTime=CAST(usedTime as INTEGER)+{100} where Id='001'";
            //int line1 = SQLiteHelper.runSqlNonQuery(SQLiteConnString, sql);
            //if (line1 <= 0) RunLog(sql + $"返回:{line1}");
            //RunLog($"Site{0 + 1:D2}-{0 * 4 + 0 + 1}:Pass({passnum}):Fail({failnum}):Yield({(passnum / (float)(passnum + failnum) * 100):0.0}%)");

            //SaveBatchData();

            //string auto1_num = "";
            //string auto2_num = "";
            //string fix1_num = "";
            //string fix2_num = "";
            //string fix3_num = "";

            //foreach (分Bin 分 in 分bin)
            //{
            //    switch (分.pan)
            //    {
            //        case "auto1": auto1_num += 分.bin + ":" + 分.num + "，"; break;
            //        case "auto2": auto2_num += 分.bin + ":" + 分.num + "，"; break;
            //        case "fix1": fix1_num += 分.bin + ":" + 分.num + "，"; break;
            //        case "fix2": fix2_num += 分.bin + ":" + 分.num + "，"; break;
            //        case "fix3": fix3_num += 分.bin + ":" + 分.num + "，"; break;
            //    }
            //}

            //auto1_num = auto1_num.TrimEnd('，');
            //auto2_num = auto2_num.TrimEnd('，');
            //fix1_num = fix1_num.TrimEnd('，');
            //fix2_num = fix2_num.TrimEnd('，');
            //fix3_num = fix3_num.TrimEnd('，');

            //string sql = $"insert into FlowcaedNoInfo(开始时间,工单号,生产模式,产品类型,产品批号,用户名,总数量,良品数量,良率," +
            //       $"投入数量,结束时间,工作时间,档案名称,UPH,Auto1名称,Auto2名称," +
            //       $"Fix1名称,Fix2名称,Fix3名称,Auto1总数,Auto2总数,Fix1总数,Fix2总数,Fix3总数," +
            //       $"Auto1数量,Auto2数量,Fix1数量,Fix2数量,Fix3数量,Socket开启数量,生产中关闭Socket数量,报警次数,加热类型,加热设定值,加热误差范围,生产速度," +
            //       $"IC吸取模式,测试模式)" +
            //        $"values('{DateTime.Now.ToString("F")}','{LotNo[0]}','正常生产','{LotNo[3]}','{LotNo[4]}','{LotNo[13]}','{出料数}','{Auto1数量}','{Auto1良率}%'," +
            //       $"'{载入数}','{DateTime.Now.ToString()}','{批次运行时间.ToString(@"dd\.hh\:mm\:ss")}','{设置信息.档案名称}','{UPHNum}','{Auto1}','{Auto2}'," +  //工作时间.Ticks
            //       $"'{Fix1}','{Fix2}','{Fix3}','{Auto1数量}','{Auto2数量}','{Fix1数量}','{Fix2数量}','{Fix3数量}'," +
            //       $"'{auto1_num}','{auto2_num}','{fix1_num}','{fix2_num}','{fix3_num}','{StartSite数量}','{StartSite数量 - 开Site数量}','{1}','{温度模式}','{设置信息.Heat盘信息.设定温度}','{设置信息.Heat盘信息.波动温度}','{100}'," +
            //       $"'正常','正常')";

            //int line = SQLiteHelper.runSqlNonQuery(SQLiteBatchString, sql);

            //if (line <= 0) Debug.WriteLine("sql保存失败！:" + sql);


            // int uph = (int)(出料数 / (批次运行时间.TotalSeconds + 1) * 3600);
            //RunLog($"PS结批流程.StepID={PS结批流程.StepID}->开始时间:{开始时间:yyyy-MM-dd HH:mm:ss.fff}");
            //RunLog($"PS结批流程.StepID={PS结批流程.StepID}->按下结批时间:{按下结批时间:yyyy-MM-dd HH:mm:ss.fff}");
            //RunLog($"PS结批流程.StepID={PS结批流程.StepID}->结批时间:" + 结批时间.ToString(@"hh\:mm\:ss"));
            //RunLog($"PS结批流程.StepID={PS结批流程.StepID}->结束时间:{结束时间:yyyy-MM-dd HH:mm:ss.fff}");
            //if (批次运行时间.TotalDays >= 1) RunLog($"PS结批流程.StepID={PS结批流程.StepID}->运行时间:" + 批次运行时间.ToString(@"dd\.hh\:mm\:ss"));
            //else RunLog($"PS结批流程.StepID={PS结批流程.StepID}->运行时间:" + 批次运行时间.ToString(@"hh\:mm\:ss"));
            //RunLog(($"PS结批流程.StepID={PS结批流程.StepID}->UPH:" + UPHNum.ToString()));
            //RunLog($"PS结批流程.StepID={PS结批流程.StepID}->载入数:{载入数}");
            //float total = 出料数;
            //RunLog($"PS结批流程.StepID={PS结批流程.StepID}->总计:{total}");
            //RunLog($"PS结批流程.StepID={PS结批流程.StepID}->Auto1:{Auto1数量},良率:{Auto1良率:0.0}%");
            //RunLog($"PS结批流程.StepID={PS结批流程.StepID}->Auto2:{Auto2数量},良率:{Auto2良率: 0.0}%");
            //RunLog($"PS结批流程.StepID={PS结批流程.StepID}->Fix1:{Fix1数量},良率:{Fix1良率: 0.0}%");
            //RunLog($"PS结批流程.StepID={PS结批流程.StepID}->Fix2:{Fix2数量},良率:{Fix2良率: 0.0}%");
            //RunLog($"PS结批流程.StepID={PS结批流程.StepID}->Fix3:{Fix3数量},良率:{Fix3良率: 0.0}%");

            //LogType logType = LogType.OperateLog;
            //Debug.WriteLine((int)logType);
            //int sitenum = 1;
            //int doutnum = sitenum * 8;
            //bool[][] open = new bool[][] { new bool[] { true, true, true, true }, new bool[] { true, true, true, true } };
            //string[] bins = { "01", "01", "01", "01", "01", "01", "01", "01" };
            //for (int m = 0; m < open.Length; m++)
            //{
            //    for (int n = 0; n < open[m].Length; n++)
            //    {
            //        string doutstr = $"{sitenum:D2}{m * 4 + n + 1}";
            //        if (open[m][n])
            //        {
            //            if (Auto1.Contains(bins[m * 4 + n]))
            //            {
            //                Dout128PassNumber[doutnum + (m * 4 + n)]++;
            //                doutContinueFail[sitenum][n + m * 4] = 0;
            //            }
            //            else
            //            {
            //                Dout128FailNumber[doutnum + (m * 4 + n)]++;
            //                doutContinueFail[sitenum][n + m * 4]++;
            //                单dut总不良计数[sitenum][n + m * 4]++;
            //                单dut总不良累计计数[sitenum][n + m * 4]++;
            //                总良率报警间隔计数++;
            //            }

            //            int line = SQLiteHelper.runSqlNonQuery(SQLiteConnString, $"update SocketUse set Num=Num+1 where Id='{doutstr}'");
            //            if (line <= 0) RunLog($"update SocketUse set Num=Num+1 where Id='{doutstr}',返回:{line}");
            //        }

            //        int passnum = Dout128PassNumber[sitenum * 8 + (m * 4 + n)];
            //        int failnum = Dout128FailNumber[sitenum * 8 + (m * 4 + n)];
            //        RunLog($"update dutCurrent set pass={passnum},total={failnum + passnum},usedTime=CAST(usedTime as bigint)+{Com.listtesttimespan[sitenum].Ticks} where Id='{doutstr}'");
            //        RunLog($"Site{sitenum + 1:D2}-{m * 4 + n + 1}:Pass({passnum}):Fail({failnum}):Yield({(passnum / (float)(passnum + failnum) * 100):0.0}%)");
            //    }
            //}
            //int[] opens = new int[4] { 0, 0, 0, 0 };
            //int xPoint = 0;
            //int yPoint = 6;

            //HeatDicPoint["0,6"] = true;
            //HeatDicPoint["0,7"] = true;
            //HeatDicPoint["5,10"] = true;
            //HeatDicPoint["5,11"] = true;
            //HeatDicTimer["0,6"].Status = 2;
            //HeatDicTimer["0,7"].Status = 2;
            //HeatDicTimer["5,10"].Status = 2;
            //HeatDicTimer["5,11"].Status = 2;

            //APPmaster.获取Heat盘Open(opens, xPoint, yPoint, 0, true, 2, out bool[] lsopen, out int index);

            //int i = 1;
            //if (!InSiteQueue.Contains(i) //如果不在入料队列中
            //                && !SiteTestState.Contains(1) //测试区有料
            //                && !SiteTestState.Contains(2) //测试区有料测试中
            //                && !SiteTestState.Contains(3) //测试区有料已测试
            //                && !SiteTestState.Contains(4) //测试区有料待取走
            //                && !正在吸取后入料.Contains(true)) //测试区有料待取走
            //{
            //    InSiteQueue.Enqueue(i);
            //}

            //H5509正在归零 = true;
            //if (!安全门禁用 && !IsPause && 设备状态 == 1 || H5509归零 || H5509正在归零)
            //{
            //    string result = handler.门禁感应(out string[] iovalue);
            //    if (result == "Success")
            //    {
            //        if (iovalue.Contains("0"))
            //        {
            //            if (iovalue[0] == "0")
            //            {
            //                Listcode.Add("9000000");
            //                RunLog("前门禁被打开");
            //            }
            //            else if (iovalue[1] == "0")
            //            {
            //                Listcode.Add("9000010");
            //                RunLog("后门禁被打开");
            //            }
            //            else if (iovalue[2] == "0")
            //            {
            //                Listcode.Add("9000020");
            //                RunLog("左门禁被打开");
            //            }
            //            else if (iovalue[3] == "0")
            //            {
            //                Listcode.Add("9000040");
            //                RunLog("右门禁被打开");
            //            }
            //            ProgramPause = true;
            //            IsPause = true;
            //        }
            //    }
            //}

            //Com.OutBin.Enqueue(new string[] { "02", "02", "02", "02", "02", "02", "02", "02" });
            //Com.Listtestsingal.Add(1);
            //SiteTestIC[1] = new bool[2][] { new bool[] { true, true, true, true }, new bool[] { true, true, true, true } };

            //int site = 6;
            //int dut = 4;

            //设置信息.开site信息.SetSiteValue(site, dut, "0");
            //string result = UpdateSiteStatus(设置信息.开site信息.Site名称, 6 + 1, 设置信息.开site信息.GetSiteValue(site));
            //if (result != "Success") { MessageBox.Show("保存失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            //result = GetSiteStatus(设置信息.开site信息.Site名称, out SiteStatus siteStatus);
            //if (result != "Success") { MessageBox.Show("读取失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            //设置信息.开site信息 = siteStatus;

            //刷新开Site状态();
            #endregion
        }

        private void label_driverStatus_TextChanged(object sender, EventArgs e)
        {
            刷新界面权限();
        }

        private void uiButton1_Click(object sender, EventArgs e)
        {
            设备状态 = Convert.ToInt32(uiComboBox1.Text.ToString());
        }

        private void label139_MouseEnter(object sender, EventArgs e)
        {
            System.Drawing.FontStyle fontsty = ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline)));
            float emsize = label139.Font.Size;
            this.label139.Font = new System.Drawing.Font("微软雅黑", emsize, fontsty, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        }

        private void label139_MouseLeave(object sender, EventArgs e)
        {
            System.Drawing.FontStyle fontsty = System.Drawing.FontStyle.Bold;
            float emsize = label139.Font.Size;
            this.label139.Font = new System.Drawing.Font("微软雅黑", emsize, fontsty, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        }

        private void label141_MouseEnter(object sender, EventArgs e)
        {
            System.Drawing.FontStyle fontsty = ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline)));
            float emsize = label141.Font.Size;
            this.label141.Font = new System.Drawing.Font("微软雅黑", emsize, fontsty, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        }

        private void label141_MouseLeave(object sender, EventArgs e)
        {
            System.Drawing.FontStyle fontsty = System.Drawing.FontStyle.Bold;
            float emsize = label141.Font.Size;
            this.label141.Font = new System.Drawing.Font("微软雅黑", emsize, fontsty, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        }

        private void label143_MouseEnter(object sender, EventArgs e)
        {
            System.Drawing.FontStyle fontsty = ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline)));
            float emsize = label143.Font.Size;
            this.label143.Font = new System.Drawing.Font("微软雅黑", emsize, fontsty, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        }

        private void label143_MouseLeave(object sender, EventArgs e)
        {
            System.Drawing.FontStyle fontsty = System.Drawing.FontStyle.Bold;
            float emsize = label143.Font.Size;
            this.label143.Font = new System.Drawing.Font("微软雅黑", emsize, fontsty, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        }

        private void label142_MouseEnter(object sender, EventArgs e)
        {
            System.Drawing.FontStyle fontsty = ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline)));
            float emsize = label142.Font.Size;
            this.label142.Font = new System.Drawing.Font("微软雅黑", emsize, fontsty, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        }

        private void label142_MouseLeave(object sender, EventArgs e)
        {
            System.Drawing.FontStyle fontsty = System.Drawing.FontStyle.Bold;
            float emsize = label142.Font.Size;
            this.label142.Font = new System.Drawing.Font("微软雅黑", emsize, fontsty, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        }

        private void label144_MouseEnter(object sender, EventArgs e)
        {
            System.Drawing.FontStyle fontsty = ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline)));
            float emsize = label144.Font.Size;
            this.label144.Font = new System.Drawing.Font("微软雅黑", emsize, fontsty, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        }

        private void label144_MouseLeave(object sender, EventArgs e)
        {
            System.Drawing.FontStyle fontsty = System.Drawing.FontStyle.Bold;
            float emsize = label144.Font.Size;
            this.label144.Font = new System.Drawing.Font("微软雅黑", emsize, fontsty, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        }

        private void label1_MouseEnter(object sender, EventArgs e)
        {
            System.Drawing.FontStyle fontsty = ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline)));
            float emsize = label1.Font.Size;
            this.label1.Font = new System.Drawing.Font("微软雅黑", emsize, fontsty, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        }

        private void label1_MouseLeave(object sender, EventArgs e)
        {
            System.Drawing.FontStyle fontsty = System.Drawing.FontStyle.Bold;
            float emsize = label1.Font.Size;
            this.label1.Font = new System.Drawing.Font("微软雅黑", emsize, fontsty, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        }

        private void label141_Click(object sender, EventArgs e)
        {
            Auto1Map auto1Map = new Auto1Map();
            auto1Map.ShowDialog();
        }

        private void label143_Click(object sender, EventArgs e)
        {
            Auto2Map auto2Map = new Auto2Map();
            auto2Map.ShowDialog();
        }

        private void label142_Click(object sender, EventArgs e)
        {
            Fix1Map fix1Map = new Fix1Map();
            fix1Map.ShowDialog();
        }

        private void label144_Click(object sender, EventArgs e)
        {
            Fix2Map fix2Map = new Fix2Map();
            fix2Map.ShowDialog();
        }

        private void label1_Click(object sender, EventArgs e)
        {
            Fix3Map fix3Map = new Fix3Map();
            fix3Map.ShowDialog();
        }

        private void lab_uph_Click(object sender, EventArgs e)
        {
            if (当前权限 != "Operator")
            {
                if (UPHType == 0)
                {
                    UPHType = 1;
                }
                else
                {
                    UPHType = 0;
                }
            }
            else UPHType = 0;
        }

        private void label9_Click(object sender, EventArgs e)
        {
        }
    }
}
