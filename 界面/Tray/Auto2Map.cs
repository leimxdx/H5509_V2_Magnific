using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ttimer = System.Timers.Timer;
using static H5509_V2.APPmaster;

namespace H5509_V2
{
    public partial class Auto2Map : Form
    {
        public Ttimer Timer { get; set; }
        public Auto2Map()
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
            double[] trayXPoint = Auto2XPoint;
            double[] trayYPoint = Auto2YPoint;
            Dictionary<string, bool> tryDicPoint = Auto2DicPoint;
            Dictionary<string, string> tryDicBin = Auto2DicBin;

            for (int i = 0; i < trayXPoint.Length; i++)
            {
                for (int j = 0; j < trayYPoint.Length; j++)
                {
                    DataGridViewCell aa = dGV_Auto2TrayMap.Rows[j].Cells[i];
                    if (tryDicPoint[$"{i},{j}"])
                    {
                        aa.Style.BackColor = System.Drawing.Color.FromArgb(设置信息.分BIN信息.color2);
                        aa.Value = tryDicBin[$"{i},{j}"];
                    }
                    else
                    {
                        aa.Style.BackColor = Color.White;
                        aa.Value = "";
                    }
                }
            }
        }

        private void Auto2Map_Load(object sender, EventArgs e)
        {
            dGV_init(dGV_Auto2TrayMap, Tray_X_Num, Tray_Y_Num);

            刷新数据();

            Timer = new Ttimer();
            Timer.Interval = 500;
            Timer.Elapsed += TimerHeatTick;
            Timer.Start();

            dGV_Auto2TrayMap.ClearSelection();
        }
        private void TimerHeatTick(object sender, EventArgs e)
        {
            刷新数据();
        }
    }
}
