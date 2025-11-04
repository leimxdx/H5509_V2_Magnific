using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.Core.Metadata.Edm;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static H5509_V2.APPmaster;
using Ttimer = System.Timers.Timer;

namespace H5509_V2
{
    public partial class Fix1Map : Form
    {
        public static int ShowType = 0;
        public Ttimer Timer { get; set; }
        public Fix1Map()
        {
            InitializeComponent();
        }
        void dGV_init(DataGridView dGV, int x, int y)
        {
            dGV.RowsDefaultCellStyle.WrapMode = DataGridViewTriState.False; // 禁止换行
            dGV.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None; // 关闭自动调整行高

            dGV.RowTemplate.Height = dGV.Size.Height / y; // 设置所有新行的默认高度

            dGV.Rows.Clear();
            dGV.Columns.Clear();

            for (int i = 0; i < x; i++)
            {
                dGV.Columns.Add("Column" + i.ToString(), "Column" + i.ToString());
                dGV.Columns[i].Width = dGV.Width / x; // 设置所有列的宽度
            }

            dGV.Rows.Add(y);

            if (x == 1)
            {
                for (int i = 0; i < dGV.Rows.Count; i++)
                {
                    dGV.Rows[i].Cells[0].Value = (dGV.Rows.Count - i).ToString("d2");
                }
            }

            //dGV.Enabled = false;
        }

        private void 刷新数据()
        {
            int showtype = ShowType;
            double[] trayXPoint = Fix1XPoint;
            double[] trayYPoint = Fix1YPoint;
            Dictionary<string, bool> tryDicPoint = Fix1DicPoint;
            Dictionary<string, string> tryDicBin = Fix1DicBin;
            Dictionary<string, string> tryDicTestTime = Fix1DicTestTime;
            Dictionary<string, string> tryDicLocation = Fix1DicLocation;

            for (int i = 0; i < trayXPoint.Length; i++)
            {
                for (int j = 0; j < trayYPoint.Length; j++)
                {
                    DataGridViewCell aa = dGV_Fix1TrayMap.Rows[j].Cells[i];
                    if (tryDicPoint[$"{i},{j}"])
                    {
                        aa.Style.BackColor = System.Drawing.Color.FromArgb(设置信息.分BIN信息.color3);
                        if (showtype == 0) aa.Value = tryDicBin[$"{i},{j}"];
                        else if (showtype == 1) aa.Value = tryDicTestTime[$"{i},{j}"];
                        else if (showtype == 2) aa.Value = tryDicLocation[$"{i},{j}"];
                        //aa.Value = tryDicBin[$"{i},{j}"];
                    }
                    else
                    {
                        aa.Style.BackColor = Color.White;
                        aa.Value = "";
                    }
                }
            }
        }


        private void Fix1Map_Load(object sender, EventArgs e)
        {
            dGV_init(dGV_Fix1TrayMap, Tray_X_Num, Tray_Y_Num);

            刷新数据();

            Timer = new Ttimer();
            Timer.Interval = 500;
            Timer.Elapsed += TimerHeatTick;
            Timer.Start();

            dGV_Fix1TrayMap.ClearSelection();

            uiCheckBox204.Checked = ShowType == 0;
            uiCheckBox1.Checked = ShowType == 1;
            uiCheckBox2.Checked = ShowType == 2;
        }
        private void TimerHeatTick(object sender, EventArgs e)
        {
            刷新数据();
        }

        private void Fix1Map_FormClosing(object sender, FormClosingEventArgs e)
        {
            Timer.Stop();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Timer.Stop();
            this.Close();
        }

        private void uiCheckBox2_Click(object sender, EventArgs e)
        {
            if (uiCheckBox2.Checked)
            {
                uiCheckBox1.Checked = false;
                uiCheckBox204.Checked = false;
            }
            else uiCheckBox2.Checked = true;

            ShowType = 2;
        }

        private void uiCheckBox1_Click(object sender, EventArgs e)
        {
            if (uiCheckBox1.Checked)
            {
                uiCheckBox2.Checked = false;
                uiCheckBox204.Checked = false;
            }
            else uiCheckBox1.Checked = true;

            ShowType = 1;
        }

        private void uiCheckBox204_Click(object sender, EventArgs e)
        {
            if (uiCheckBox204.Checked)
            {
                uiCheckBox1.Checked = false;
                uiCheckBox2.Checked = false;
            }
            else uiCheckBox204.Checked = true;

            ShowType = 0;
        }
    }
}
