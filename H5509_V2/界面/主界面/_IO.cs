using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sunny.UI;
using Sunny.UI.Win32;
using static H5509_V2.APPmaster;
using MotionCommon;
using SQLiteHelper;

namespace H5509_V2
{
    public partial class _IO : UIUserControl
    {
        string currentIOPage;
        List<string> currentX;
        List<string> currentY;
        IOButton btn;

        public string[] GNME601_核1_输入 = new string[32];
        public string[] GNME601_核2_输入 = new string[32];
        public string[][] GNME601_核输入 = new string[2][];

        public string[] GNME601_核1_输出 = new string[32];
        public string[] GNME601_核2_输出 = new string[32];
        public string[][] GNME601_核输出 = new string[2][];

        public string[] Glink200_核1_输入 = new string[200];
        public string[] Glink200_核1_输出 = new string[200];

        public Control[][] lbl_梭车前入 = new Control[2][];
        public Control[][] lbl_梭车前出 = new Control[2][];
        public Control[][] lbl_梭车后入 = new Control[2][];
        public Control[][] lbl_梭车后出 = new Control[2][];

        List<UILight> ListUiLight = new List<UILight>();
        List<UISwitch> ListUiSwitch = new List<UISwitch>();

        public _IO()
        {
            InitializeComponent();

            //string sql = "select 区域 from IOInfo group by 区域";
            string sql = "SELECT '系统' UNION ALL SELECT '入料区'  UNION ALL SELECT '出料区'  UNION ALL SELECT '测试区'  UNION ALL SELECT '梭车' UNION ALL SELECT '抓盘手臂'";

            //DataRowCollection drs =
            string result = Sqlite.runSql(SQLiteConnString, sql, out DataTable dataTable);
            if (result == "Success")
            {
                DataRowCollection drs = dataTable.Rows;
                for (int i = 0; i < drs.Count; i++)
                {
                    TabPage tab = new TabPage();
                    tab.Name = "IOPage_" + drs[i][0].ToString();
                    tab.Text = drs[i][0].ToString();
                    tab.BackColor = Color.FromArgb(243, 249, 255);
                    tab.BorderStyle = BorderStyle.Fixed3D;
                    uiTabControl2.TabPages.Add(tab);

                    UIFlowLayoutPanel flowLayoutPanel = new UIFlowLayoutPanel();
                    flowLayoutPanel.Name = "IOflp_" + drs[i][0].ToString();
                    flowLayoutPanel.Height = 883;
                    flowLayoutPanel.Width = tab.Width / 2;
                    flowLayoutPanel.Location = new Point(4, 4);
                    flowLayoutPanel.Padding = new Padding(2, 2, 2, 2);
                    tab.Controls.Add(flowLayoutPanel);
                }


                foreach (IOInfo iOInfo in APPmaster.IOList)
                {
                    //if(iOInfo.IO名称=="备用")
                    //{
                    //    continue;
                    //}
                    btn = new IOButton(motionEx);
                    btn.SetDPIScale();
                    btn.Name = "IOButton_" + iOInfo.IO编号;
                    btn.Text = iOInfo.IO名称.Replace("上料", "").Replace("下料", "").Replace("测试", "").Replace("抓盘手臂", "");
                    btn.ioInfo = iOInfo;
                    btn.uiLabel1.Text = iOInfo.IO编号;

                    uiToolTip1.SetToolTip(btn, iOInfo.IO编号 + ":" + iOInfo.IO名称, iOInfo.模块类型 + "-" + iOInfo.模块号 + "-" + iOInfo.硬件索引);

                    uiTabControl2.Controls["IOPage_" + iOInfo.IO区域].Controls["IOflp_" + iOInfo.IO区域].Controls.Add(btn);

                    //switch (iOInfo.IO区域)
                    //{
                    //    //建议用封装的方法Add
                    //    case "系统": uiFlowLayoutPanel1.Add(btn); break;
                    //    case "其他": uiFlowLayoutPanel2.Add(btn); break;

                    //        //或者Panel.Controls.Add
                    //        //uiFlowLayoutPanel1.Panel.Controls.Add(btn);

                    //        //不能用原生方法Controls.Add
                    //        //----uiFlowLayoutPanel1.Controls.Add(btn);----
                    //}

                }

                for (int i = 0; i < drs.Count; i++)
                {
                    UIFlowLayoutPanel uIFlow = uiTabControl2.Controls["IOPage_" + drs[i][0].ToString()].Controls["IOflp_" + drs[i][0].ToString()] as UIFlowLayoutPanel;
                    if ((int)Math.Ceiling(uIFlow.ControlCount / 21.0) >= 9)
                    {
                        uIFlow.Width = (btn.Width + 7) * 9;
                    }
                    else if (uIFlow.ControlCount > 84)
                    {
                        uIFlow.Width = (btn.Width + 7) * (int)Math.Ceiling(uIFlow.ControlCount / 21.0) + 15;
                    }
                    else
                    {
                        uIFlow.Width = (btn.Width + 7) * 4;
                    }

                }

                uiTabControl2_SelectedIndexChanged(this.uiTabControl2.SelectedTab, null);
            }

            TabPage[] tabpages = { tabPage7, tabPage8, tabPage9, tabPage16 };
            for (int i = 0; i < tabpages.Length; i++)
            {
                foreach (var item in tabpages[i].Controls)
                {
                    if (item is UIGroupBox uigroupos)
                    {
                        foreach (var item1 in uigroupos.Controls)
                        {
                            if (item1 is UILight uilight)
                            {
                                if (uilight.TagString == null) continue;
                                if (uilight.TagString.ToString().Length == 0) continue;
                                ListUiLight.Add(uilight);

                            }
                            else if (item1 is UISwitch uiswitch)
                            {
                                if (uiswitch.TagString == null) continue;
                                if (uiswitch.TagString.ToString().Length == 0) continue;
                                uiswitch.Click += SetDoValue;
                                ListUiSwitch.Add(uiswitch);
                            }
                        }
                    }
                }
            }

            lbl_梭车前入 = new Control[2][] { new Control[] { lbl_梭车前入1, lbl_梭车前入2, lbl_梭车前入3, lbl_梭车前入4 }, new Control[] { lbl_梭车前入5, lbl_梭车前入6, lbl_梭车前入7, lbl_梭车前入8 } };
            lbl_梭车前出 = new Control[2][] { new Control[] { lbl_梭车前出1, lbl_梭车前出2, lbl_梭车前出3, lbl_梭车前出4 }, new Control[] { lbl_梭车前出5, lbl_梭车前出6, lbl_梭车前出7, lbl_梭车前出8 } };
            lbl_梭车后入 = new Control[2][] { new Control[] { lbl_梭车后入1, lbl_梭车后入2, lbl_梭车后入3, lbl_梭车后入4 }, new Control[] { lbl_梭车后入5, lbl_梭车后入6, lbl_梭车后入7, lbl_梭车后入8 } };
            lbl_梭车后出 = new Control[2][] { new Control[] { lbl_梭车后出1 ,lbl_梭车后出2, lbl_梭车后出3, lbl_梭车后出4 }, new Control[] { lbl_梭车后出5, lbl_梭车后出6, lbl_梭车后出7, lbl_梭车后出8 } };
        }

        private void IOUpdateTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!InitedOK) return;

                char[] ioval;
                string value;

                //GNME601 核1 核2 输入
                for (short i = 1; i <= 2; i++)
                {
                    motionEx.GetDi("GNM601", i, 1, 4, out value);
                    ioval = value.ToCharArray();
                    for (int j = 0; j < ioval.Length; j++)
                    {
                        GNME601_核输入[i][j] = ioval[j].ToString();
                    }
                }

                ////GNME601 核1
                //motionEx.GetDi("GNM601", 1, 1, 4, out value);
                //ioval = value.ToCharArray();
                //for (int i = 0; i < ioval.Length; i++)
                //{
                //    GNME601_核1_输入[i] = ioval[i].ToString();
                //}

                //motionEx.GetDO("GNM601", 1, 1, 12, out value);
                //ioval = value.ToCharArray();
                //for (int i = 0; i < ioval.Length; i++)
                //{
                //    GNME601_核1_输出[i] = ioval[i].ToString();
                //}

                //GNME601 核1 核2 输出
                for (short i = 1; i <= 2; i++)
                {
                    motionEx.GetDO("GNM601", i, 1, 12, out value);
                    ioval = value.ToCharArray();
                    for (int j = 0; j < ioval.Length; j++)
                    {
                        GNME601_核输出[i][j] = ioval[j].ToString();
                    }
                }

                ////GNME601 核2
                //motionEx.GetDi("GNM601", 2, 1, 4, out value);
                //ioval = value.ToCharArray();
                //for (int i = 0; i < ioval.Length; i++)
                //{
                //    GNME601_核2_输入[i] = ioval[i].ToString();
                //}

                //motionEx.GetDO("GNM601", 2, 1, 12, out value);
                //ioval = value.ToCharArray();
                //for (int i = 0; i < ioval.Length; i++)
                //{
                //    GNME601_核2_输出[i] = ioval[i].ToString();
                //}

                //gLink200 输入
                for (int i = 0; i < 5; i++)
                {
                    motionEx.GetDi("gLink200", 1, (short)(i * 32 + 1), 4, out value);
                    ioval = value.ToCharArray();
                    for (int j = 0; j < ioval.Length; j++)
                    {
                        Glink200_核1_输入[j + i * 32] = ioval[j].ToString();
                    }
                }

                //motionEx.GetDi("gLink200", 1, 0 * 32 + 1, 4, out value);
                //ioval = value.ToCharArray();
                //for (int i = 0; i < ioval.Length; i++)
                //{
                //    Glink200_核1_输入[i + 0 * 32] = ioval[i].ToString();
                //}
                //motionEx.GetDi("gLink200", 1, 1 * 32 + 1, 4, out value);
                //ioval = value.ToCharArray();
                //for (int i = 0; i < ioval.Length; i++)
                //{
                //    Glink200_核1_输入[i + 1 * 32] = ioval[i].ToString();
                //}
                //motionEx.GetDi("gLink200", 1, 2 * 32 + 1, 4, out value);
                //ioval = value.ToCharArray();
                //for (int i = 0; i < ioval.Length; i++)
                //{
                //    Glink200_核1_输入[i + 2 * 32] = ioval[i].ToString();
                //}
                //motionEx.GetDi("gLink200", 1, 3 * 32 + 1, 4, out value);
                //ioval = value.ToCharArray();
                //for (int i = 0; i < ioval.Length; i++)
                //{
                //    Glink200_核1_输入[i + 3 * 32] = ioval[i].ToString();
                //}
                //motionEx.GetDi("gLink200", 1, 4 * 32 + 1, 4, out value);
                //ioval = value.ToCharArray();
                //for (int i = 0; i < ioval.Length; i++)
                //{
                //    Glink200_核1_输入[i + 4 * 32] = ioval[i].ToString();
                //}

                //gLink200 输出
                for (int i = 0; i < 4; i++)
                {
                    motionEx.GetDO("gLink200", 1, (short)(i * 32 + 1), 12, out value);
                    ioval = value.ToCharArray();
                    for (int j = 0; j < ioval.Length; j++)
                    {
                        Glink200_核1_输出[j + i * 32] = ioval[j].ToString();
                    }
                }

                //motionEx.GetDO("gLink200", 1, 0 * 32 + 1, 12, out value);
                //ioval = value.ToCharArray();
                //for (int i = 0; i < ioval.Length; i++)
                //{
                //    Glink200_核1_输出[i + 0 * 32] = ioval[i].ToString();
                //}
                //motionEx.GetDO("gLink200", 1, 1 * 32 + 1, 12, out value);
                //ioval = value.ToCharArray();
                //for (int i = 0; i < ioval.Length; i++)
                //{
                //    Glink200_核1_输出[i + 1 * 32] = ioval[i].ToString();
                //}
                //motionEx.GetDO("gLink200", 1, 2 * 32 + 1, 12, out value);
                //ioval = value.ToCharArray();
                //for (int i = 0; i < ioval.Length; i++)
                //{
                //    Glink200_核1_输出[i + 2 * 32] = ioval[i].ToString();
                //}
                //motionEx.GetDO("gLink200", 1, 3 * 32 + 1, 12, out value);
                //ioval = value.ToCharArray();
                //for (int i = 0; i < ioval.Length; i++)
                //{
                //    Glink200_核1_输出[i + 3 * 32] = ioval[i].ToString();
                //}

                if (uiTabControl1.SelectedIndex == 1)
                {
                    //UIMessageTip.Show(currentIOPage, null, 1000, true);
                    UIFlowLayoutPanel uIFlow = uiTabControl2.SelectedTab.Controls["IOflp_" + currentIOPage.Substring(7)] as UIFlowLayoutPanel;

                    //motionEx.GetDi(currentX.ToArray(), out string[] diX);
                    //motionEx.GetDO(currentY.ToArray(), out string[] doY);

                    string io = "";
                    string result = "";

                    string[] diX = new string[currentX.Count];
                    string[] doY = new string[currentY.Count];

                    for (int i = 0; i < currentX.Count; i++)
                    {
                        UILight uilight = ListUiLight[i];

                        result = motionEx.GetIOInfo(currentX[i], out IOInfo ioinfo);
                        if (ioinfo.模块类型 == "GNM601")
                        {
                            if (ioinfo.核号 == 1)
                            {
                                if (GNME601_核输入[0][ioinfo.软件索引 - 1] == "0")
                                {
                                    diX[i] = "1";
                                }
                                else diX[i] = "0";
                            }
                            else
                            {
                                if (GNME601_核输入[1][ioinfo.软件索引 - 1] == "0")
                                {
                                    diX[i] = "1";
                                }
                                else diX[i] = "0";
                            }
                        }
                        else if (ioinfo.模块类型 == "gLink200")
                        {
                            if (Glink200_核1_输入[ioinfo.软件索引 - 1] == "0")
                            {
                                diX[i] = "0";
                            }
                            else diX[i] = "1";
                        }
                    }

                    for (int i = 0; i < currentY.Count; i++)
                    {
                        result = motionEx.GetIOInfo(currentY[i], out IOInfo ioinfo);

                        if (ioinfo.模块类型 == "GNM601")
                        {

                            if (ioinfo.核号 == 1)
                            {
                                if (GNME601_核输出[0][ioinfo.软件索引 - 1] == "0")
                                {
                                    doY[i] = "0";
                                }
                                else doY[i] = "1";
                            }
                            else
                            {
                                if (GNME601_核输出[1][ioinfo.软件索引 - 1] == "0")
                                {
                                    doY[i] = "0";
                                }
                                else doY[i] = "1";
                            }
                        }
                        else if (ioinfo.模块类型 == "gLink200")
                        {
                            if (Glink200_核1_输出[ioinfo.软件索引 - 1] == "0")
                            {
                                doY[i] = "0";
                            }
                            else doY[i] = "1";

                        }
                    }

                    //Stopwatch stopwatch = new Stopwatch();
                    //stopwatch.Start();

                    for (int i = 0; i < currentX.Count; i++)
                    {
                        IOButton btn = uIFlow.Panel.Controls["IOButton_" + currentX[i]] as IOButton;
                        btn.value = diX[i] == "1" ? true : false;
                        btn._color = diX[i] == "1" ? Color.Lime : Color.Red;
                    }

                    for (int i = 0; i < currentY.Count; i++)
                    {
                        IOButton btn = uIFlow.Panel.Controls["IOButton_" + currentY[i]] as IOButton;
                        btn.value = doY[i] == "1" ? true : false;
                        btn._color = doY[i] == "1" ? Color.Lime : Color.Red;
                    }

                    this.Refresh();
                    //stopwatch.Stop();
                    //UIMessageTip.Show(this, stopwatch.ElapsedMilliseconds.ToString(), null, 1000, true, true);

                }
                else if (uiTabControl1.SelectedIndex == 0)
                {
                    //Stopwatch stopwatch = new Stopwatch();
                    //stopwatch.Start();

                    string io = "";
                    string result = "";
                    for (int i = 0; i < ListUiLight.Count; i++)
                    {
                        UILight uilight = ListUiLight[i];

                        result = motionEx.GetIOInfo(uilight.TagString, out IOInfo ioinfo);
                        if (ioinfo.模块类型 == "GNM601")
                        {
                            if (ioinfo.IO编号.Contains("X"))
                            {
                                if (ioinfo.核号 == 1)
                                {
                                    if (GNME601_核输入[0][ioinfo.软件索引 - 1] == "0")
                                    {
                                        uilight.State = UILightState.On;
                                    }
                                    else uilight.State = UILightState.Off;
                                }
                                else
                                {
                                    if (GNME601_核输入[1][ioinfo.软件索引 - 1] == "0")
                                    {
                                        uilight.State = UILightState.On;
                                    }
                                    else uilight.State = UILightState.Off;
                                }
                            }
                        }
                        else if (ioinfo.模块类型 == "gLink200")
                        {
                            if (ioinfo.IO编号.Contains("X"))
                            {
                                if (Glink200_核1_输入[ioinfo.软件索引 - 1] == "0")
                                {
                                    uilight.State = UILightState.Off;
                                }
                                else uilight.State = UILightState.On;
                            }
                        }
                    }

                    for (int i = 0; i < ListUiSwitch.Count; i++)
                    {
                        UISwitch uiswitch = ListUiSwitch[i];

                        result = motionEx.GetIOInfo(uiswitch.TagString, out IOInfo ioinfo);

                        if (ioinfo.模块类型 == "GNM601")
                        {

                            if (ioinfo.IO编号.Contains("Y"))
                            {
                                if (ioinfo.核号 == 1)
                                {
                                    if (GNME601_核输出[0][ioinfo.软件索引 - 1] == "0")
                                    {
                                        uiswitch.Active = false;
                                    }
                                    else uiswitch.Active = true;
                                }
                                else
                                {
                                    if (GNME601_核输出[1][ioinfo.软件索引 - 1] == "0")
                                    {
                                        uiswitch.Active = false;
                                    }
                                    else uiswitch.Active = true;
                                }
                            }
                        }
                        else if (ioinfo.模块类型 == "gLink200")
                        {
                            if (ioinfo.IO编号.Contains("Y"))
                            {
                                if (Glink200_核1_输出[ioinfo.软件索引 - 1] == "0")
                                {
                                    uiswitch.Active = false;
                                }
                                else uiswitch.Active = true;
                            }
                        }
                    }

                    for (int i = 0; i < lbl_梭车前入.Length; i++)
                    {
                        for (int j = 0; j < lbl_梭车前入[i].Length; j++)
                        {
                            lbl_梭车前入[i][j].BackColor = 飞梭前入料IC[i][j] ? Color.Lime : Color.Cyan;
                        }
                    }

                    for (int i = 0; i < lbl_梭车前出.Length; i++)
                    {
                        for (int j = 0; j < lbl_梭车前出[i].Length; j++)
                        {
                            lbl_梭车前出[i][j].BackColor = 飞梭前出料IC[i][j] ? Color.Lime : Color.Cyan;
                        }
                    }

                    for (int i = 0; i < lbl_梭车后入.Length; i++)
                    {
                        for (int j = 0; j < lbl_梭车后入[i].Length; j++)
                        {
                            lbl_梭车后入[i][j].BackColor = 飞梭后入料IC[i][j] ? Color.Lime : Color.Cyan;
                        }
                    }

                    for (int i = 0; i < lbl_梭车后出.Length; i++)
                    {
                        for (int j = 0; j < lbl_梭车后出[i].Length; j++)
                        {
                            lbl_梭车后出[i][j].BackColor = 飞梭后出料IC[i][j] ? Color.Lime : Color.Cyan;
                        }
                    }

                    bool[] iscan = { uiSwitch19.Active, uiSwitch20.Active, uiSwitch21.Active, uiSwitch22.Active };
                    uiSwitch24.Active = iscan.Contains(true);

                    iscan = new bool[] { uiSwitch26.Active, uiSwitch27.Active, uiSwitch28.Active, uiSwitch29.Active };
                    uiSwitch23.Active = iscan.Contains(true);

                    iscan = new bool[] { uiSwitch45.Active, uiSwitch46.Active, uiSwitch47.Active, uiSwitch48.Active };
                    uiSwitch44.Active = iscan.Contains(true);

                    iscan = new bool[] { uiSwitch40.Active, uiSwitch41.Active, uiSwitch42.Active, uiSwitch43.Active };
                    uiSwitch39.Active = iscan.Contains(true);

                    iscan = new bool[] { uiSwitch88.Active, uiSwitch87.Active, uiSwitch86.Active, uiSwitch85.Active, uiSwitch92.Active, uiSwitch91.Active, uiSwitch90.Active, uiSwitch89.Active };
                    uiSwitch84.Active = iscan.Contains(true);

                    iscan = new bool[] { uiSwitch83.Active, uiSwitch82.Active, uiSwitch81.Active, uiSwitch80.Active, uiSwitch97.Active, uiSwitch96.Active, uiSwitch95.Active, uiSwitch94.Active };
                    uiSwitch78.Active = iscan.Contains(true);

                    //stopwatch.Stop();
                    //UIMessageTip.Show(this, stopwatch.ElapsedMilliseconds.ToString(), null, 1000, true, true);
                }
                else
                {
                    //UIMessageTip.Show("IO界面", null, 1000, true);

                }
            }
            catch (Exception ex)
            {
            }
        }

        private void uiButton1_Click(object sender, EventArgs e)
        {
            UIMessageTip.Show(this, "TEST!TEST!TEST!", null, 1000, true, true);

        }

        private void uiTabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentIOPage = uiTabControl2.SelectedTab.Name;
            UIFlowLayoutPanel uIFlow = uiTabControl2.SelectedTab.Controls["IOflp_" + currentIOPage.Substring(7)] as UIFlowLayoutPanel;

            currentX = new List<string>();
            currentY = new List<string>();
            for (int i = 0; i < uIFlow.ControlCount; i++)
            {
                if (uIFlow.Panel.Controls[i] is IOButton)
                {
                    if ((uIFlow.Panel.Controls[i] as IOButton).uiLabel1.Text.Contains("X"))
                    {
                        currentX.Add((uIFlow.Panel.Controls[i] as IOButton).uiLabel1.Text);
                    }
                    else
                    {
                        currentY.Add((uIFlow.Panel.Controls[i] as IOButton).uiLabel1.Text);
                    }
                }
            }
        }

        private bool StateCheck()
        {
            if (当前权限 == "Operator")
            {
                UIMessageTip.ShowError(this.Parent, "权限不足", 1000, true, true);
                return false;
            }

            if (!APPmaster.InitedOK)
            {
                UIMessageTip.ShowError(this.Parent, "轴卡未连接", 1000, true, true);
                return false;
            }

            if (APPmaster.设备状态 == 1)
            {
                UIMessageTip.ShowError(this.Parent, "设备正在运行...", 1000, true, true);
                return false;
            }

            return true;
        }

        private void SetDoValue(object sender, EventArgs e)
        {
            if (!StateCheck()) return;

            if (sender is UISwitch uISwitch)
            {
                if (uISwitch.TagString == null) return;
                if (uISwitch.TagString.ToString().Length == 0) return;
                string result = motionEx.SetDo(uISwitch.TagString, uISwitch.Active);
                if (result != "Success")
                {
                    MessageBox.Show("输出失败!" + result);
                }
            }
        }

        private void uiSwitch84_ValueChanged(object sender, bool value)
        {
            if (!StateCheck()) return;

            bool[] lsopen = new bool[] { true, true, true, true, true, true, true, true };
            handler.测试吸嘴_吸放料(lsopen, false, false);
            handler.测试吸嘴_吸放料(lsopen, true, value);
            this.uiSwitch78.ValueChanged -= new Sunny.UI.UISwitch.OnValueChanged(this.uiSwitch78_ValueChanged);
            uiSwitch78.Active = false;
            this.uiSwitch78.ValueChanged += new Sunny.UI.UISwitch.OnValueChanged(this.uiSwitch78_ValueChanged);
        }

        private void uiSwitch78_ValueChanged(object sender, bool value)
        {
            if (!StateCheck()) return;

            bool[] lsopen = new bool[] { true, true, true, true, true, true, true, true };
            handler.测试吸嘴_吸放料(lsopen, true, false);
            handler.测试吸嘴_吸放料(lsopen, false, value);
            this.uiSwitch84.ValueChanged -= new Sunny.UI.UISwitch.OnValueChanged(this.uiSwitch84_ValueChanged);
            uiSwitch84.Active = false;
            this.uiSwitch84.ValueChanged += new Sunny.UI.UISwitch.OnValueChanged(this.uiSwitch84_ValueChanged);
        }

        private void uiSwitch23_ValueChanged(object sender, bool value)
        {
            if (!StateCheck()) return;

            bool[] lsopen = new bool[] { true, true, true, true };
            handler.入料吸嘴_吸放料(lsopen, false, false);
            handler.入料吸嘴_吸放料(lsopen, true, value);
            this.uiSwitch24.ValueChanged -= new Sunny.UI.UISwitch.OnValueChanged(this.uiSwitch24_ValueChanged);
            uiSwitch24.Active = false;
            this.uiSwitch24.ValueChanged += new Sunny.UI.UISwitch.OnValueChanged(this.uiSwitch24_ValueChanged);
        }

        private void uiSwitch24_ValueChanged(object sender, bool value)
        {
            if (!StateCheck()) return;

            bool[] lsopen = new bool[] { true, true, true, true };
            handler.入料吸嘴_吸放料(lsopen, true, false);
            handler.入料吸嘴_吸放料(lsopen, false, value);
            this.uiSwitch23.ValueChanged -= new Sunny.UI.UISwitch.OnValueChanged(this.uiSwitch23_ValueChanged);
            uiSwitch23.Active = false;
            this.uiSwitch23.ValueChanged += new Sunny.UI.UISwitch.OnValueChanged(this.uiSwitch23_ValueChanged);
        }

        private void uiSwitch44_ValueChanged(object sender, bool value)
        {
            if (!StateCheck()) return;

            bool[] lsopen = new bool[] { true, true, true, true };
            handler.出料吸嘴_吸放料(lsopen, false, false);
            handler.出料吸嘴_吸放料(lsopen, true, value);
            this.uiSwitch39.ValueChanged -= new Sunny.UI.UISwitch.OnValueChanged(this.uiSwitch39_ValueChanged);
            uiSwitch39.Active = false;
            this.uiSwitch39.ValueChanged += new Sunny.UI.UISwitch.OnValueChanged(this.uiSwitch39_ValueChanged);
        }

        private void uiSwitch39_ValueChanged(object sender, bool value)
        {
            if (!StateCheck()) return;

            bool[] lsopen = new bool[] { true, true, true, true };
            handler.出料吸嘴_吸放料(lsopen, true, false);
            handler.出料吸嘴_吸放料(lsopen, false, value);
            this.uiSwitch44.ValueChanged -= new Sunny.UI.UISwitch.OnValueChanged(this.uiSwitch44_ValueChanged);
            uiSwitch44.Active = false;
            this.uiSwitch44.ValueChanged += new Sunny.UI.UISwitch.OnValueChanged(this.uiSwitch44_ValueChanged);
        }

        private void uiSwitch23_Click(object sender, EventArgs e)
        {
            if (!StateCheck()) return;

            bool[] lsopen = new bool[] { true, true, true, true };
            handler.入料吸嘴_吸放料(lsopen, false, false);
            handler.入料吸嘴_吸放料(lsopen, true, uiSwitch23.Active);
        }

        private void uiSwitch24_Click(object sender, EventArgs e)
        {
            if (!StateCheck()) return;

            bool[] lsopen = new bool[] { true, true, true, true };
            handler.入料吸嘴_吸放料(lsopen, true, false);
            handler.入料吸嘴_吸放料(lsopen, false, uiSwitch24.Active);
        }

        private void uiSwitch44_Click(object sender, EventArgs e)
        {
            if (!StateCheck()) return;

            bool[] lsopen = new bool[] { true, true, true, true };
            handler.出料吸嘴_吸放料(lsopen, false, false);
            handler.出料吸嘴_吸放料(lsopen, true, uiSwitch44.Active);
        }

        private void uiSwitch39_Click(object sender, EventArgs e)
        {
            if (!StateCheck()) return;

            bool[] lsopen = new bool[] { true, true, true, true };
            handler.出料吸嘴_吸放料(lsopen, true, false);
            handler.出料吸嘴_吸放料(lsopen, false, uiSwitch39.Active);
        }

        private void uiSwitch84_Click(object sender, EventArgs e)
        {
            if (!StateCheck()) return;

            bool[] lsopen = new bool[] { true, true, true, true, true, true, true, true };
            handler.测试吸嘴_吸放料(lsopen, false, false);
            handler.测试吸嘴_吸放料(lsopen, true, uiSwitch84.Active);
        }

        private void uiSwitch78_Click(object sender, EventArgs e)
        {
            if (!StateCheck()) return;

            bool[] lsopen = new bool[] { true, true, true, true, true, true, true, true };
            handler.测试吸嘴_吸放料(lsopen, true, false);
            handler.测试吸嘴_吸放料(lsopen, false, uiSwitch78.Active);
        }
    }
}
