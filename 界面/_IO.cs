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
using System.Reflection;
using Sunny.UI.Win32;


namespace H5509_V2
{
    public partial class _IO : UIUserControl
    {
        string currentIOPage;
        List<string> currentX;
        List<string> currentY;
        IOButton btn;
        MotionEx motionEx = MotionEx.GetInstance();

        public _IO()
        {
            InitializeComponent();

            //string sql = "select 区域 from IOInfo group by 区域";
            string sql = "SELECT '系统' UNION ALL SELECT '入料区'  UNION ALL SELECT '出料区'  UNION ALL SELECT '测试区'  UNION ALL SELECT '梭车' UNION ALL SELECT '抓盘手臂'";

            DataRowCollection drs = SQLiteHelper.GetDataSet(sql).Tables[0].Rows;
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
                btn = new IOButton();
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

        private void IOUpdateTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (uiTabControl1.SelectedIndex == 0)
                {
                    //UIMessageTip.Show(currentIOPage, null, 1000, true);
                    UIFlowLayoutPanel uIFlow = uiTabControl2.SelectedTab.Controls["IOflp_" + currentIOPage.Substring(7)] as UIFlowLayoutPanel;

                    motionEx.GetDi(currentX.ToArray(), out string[] diX);
                    motionEx.GetDO(currentY.ToArray(), out string[] doY);

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
                }
                else
                {
                    UIMessageTip.Show("IO界面", null, 1000, true);

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
    }
}
