using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static H5509_V2.APPmaster;
using Ttimer = System.Timers.Timer;

namespace H5509_V2
{
    public partial class HeatMap : Form
    {
        //public double[] HeatXPoint;
        //public double[] HeatYPoint;
        //public Dictionary<string, bool> HeatDicPoint = new Dictionary<string, bool>();
        //public Dictionary<string, HeatMapTimer> HeatDicTimer = new Dictionary<string, HeatMapTimer>(); //加热盘状态图

        public Ttimer Timer { get; set; }

        public HeatMap()
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
            for (int i = 0; i < HeatXPoint.Length; i++)
            {
                for (int j = 0; j < HeatYPoint.Length; j++)
                {
                    DataGridViewCell aa = dGV_HeatTrayMap.Rows[j].Cells[i];
                    if (HeatDicPoint[$"{i},{j}"])
                    {
                        if (!温控达温)
                        {
                            aa.Value = HeatDicTimer[$"{i},{j}"].Status.ToString() + "(升温)";
                            aa.Style.BackColor = Color.White;
                        }
                        else
                        {
                            if (HeatDicTimer[$"{i},{j}"].Status == 1) //显示倒计时
                            {
                                aa.Value = HeatDicTimer[$"{i},{j}"].CountDownTime.TotalSeconds.ToString("0.0") + "s";
                                aa.Style.BackColor = Color.Gold;
                            }
                            else
                            {
                                aa.Value = HeatDicTimer[$"{i},{j}"].Status;
                                aa.Style.BackColor = Color.LimeGreen;
                            }
                        }
                    }
                    else
                    {
                        aa.Style.BackColor = Color.White;
                        aa.Value = 0;
                    }
                }
            }
        }

        private void HeatMap_Load(object sender, EventArgs e)
        {
            dGV_init(dGV_HeatTrayMap, Heat_X_Num, Heat_Y_Num);

            刷新数据();

            Timer = new Ttimer();
            Timer.Interval = 500;
            Timer.Elapsed += TimerHeatTick;
            Timer.Start();

            dGV_HeatTrayMap.ClearSelection();
        }

        private void TimerHeatTick(object sender, EventArgs e)
        {
            刷新数据();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            Timer.Stop();
            this.Close();
        }
    }
}
