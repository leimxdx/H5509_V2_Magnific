using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DocumentFormat.OpenXml.Drawing;
using static H5509_V2.APPmaster;
using Ttimer = System.Timers.Timer;
namespace H5509_V2
{
    public partial class LoadMap : Form
    {
        public int CurrentSC = 0;
        public double[] LoadXPoint;
        public double[] LoadYPoint;
        public Dictionary<string, bool> LoadDicPoint = new Dictionary<string, bool>();
        public Dictionary<string, bool> LsLoadDicPoint = new Dictionary<string, bool>();
        public Ttimer Timer { get; set; }
        public LoadMap()
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


        public void TrayMapInit()
        {
            LsLoadDicPoint = new Dictionary<string, bool>();
            for (int i = 0; i < LoadXPoint.Length; i++)
            {
                for (int j = 0; j < LoadYPoint.Length; j++)
                {
                    LsLoadDicPoint.Add($"{i},{j}", LoadDicPoint[$"{i},{j}"]);
                }
            }
        }

        private void TimerHeatTick(object sender, EventArgs e)
        {
            if (设备状态 == 1)
            {
                LsLoadDicPoint = null;
                刷新数据();
            }
            else
            {
                if (LsLoadDicPoint == null)
                {
                    TrayMapInit();
                }
                try
                {
                    this.Invoke(new Action(() =>
                    {
                        button6.Enabled = true; //全选按钮不可用
                        button2.Enabled = true; //全选按钮不可用
                        button5.Enabled = true; //全选按钮不可用
                        button4.Enabled = true; //全选按钮不可用
                    }));
                }
                catch { }
            }
        }

        private void 刷新数据()
        {
            for (int i = 0; i < LoadXPoint.Length; i++)
            {
                for (int j = 0; j < LoadYPoint.Length; j++)
                {
                    DataGridViewCell aa = dGV_LoadTrayMap.Rows[j].Cells[i];
                    if (LoadDicPoint[$"{i},{j}"])
                    {
                        aa.Style.BackColor = Color.LimeGreen;
                        //dGV_TrayMap.Rows[j].Cells[i].Value = 1;
                    }
                    else
                    {
                        aa.Style.BackColor = Color.White;
                        //dGV_TrayMap.Rows[j].Cells[i].Value = 0;
                    }
                }
            }
            try
            {
                this.Invoke(new Action(() =>
                {
                    if (设备状态 == 1)
                    {
                        button6.Enabled = false; //全选按钮不可用
                        button2.Enabled = false; //全选按钮不可用
                        button5.Enabled = false; //全选按钮不可用
                        button4.Enabled = false; //全选按钮不可用
                    }
                    else
                    {
                        button6.Enabled = true; //全选按钮不可用
                        button2.Enabled = true; //全选按钮不可用
                        button5.Enabled = true; //全选按钮不可用
                        button4.Enabled = true; //全选按钮不可用
                    }
                }));
            }
            catch { }
        }

        private void LoadMap_Load(object sender, EventArgs e)
        {
            TrayMapInit(); //临时TrayMap初始化

            dGV_init(dGV_LoadTrayMap, Tray_X_Num, Tray_Y_Num);


            刷新数据();

            Timer = new Ttimer();
            Timer.Interval = 200;
            Timer.Elapsed += TimerHeatTick;
            Timer.Start();

            dGV_LoadTrayMap.ClearSelection();
        }

        private void dGV_LoadTrayMap_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (设备状态 == 1 && LsLoadDicPoint == null) return;

            if (dGV_LoadTrayMap.SelectedCells.Count > 0)
            {
                DataGridViewSelectedCellCollection selectcells = dGV_LoadTrayMap.SelectedCells;
                for (int i = 0; i < dGV_LoadTrayMap.SelectedCells.Count; i++)
                {
                    int x = selectcells[i].RowIndex;
                    int y = selectcells[i].ColumnIndex;
                    //Console.WriteLine($"{x + 1},{y + 1}");

                    if (dGV_LoadTrayMap.Rows[x].Cells[y].Style.BackColor == Color.LimeGreen)
                    {
                        dGV_LoadTrayMap.Rows[x].Cells[y].Style.BackColor = Color.White;
                        //dGV_TrayMap.Rows[x].Cells[y].Value = 0;
                        LsLoadDicPoint[$"{y},{x}"] = false;
                    }
                    else
                    {
                        dGV_LoadTrayMap.Rows[x].Cells[y].Style.BackColor = Color.LimeGreen;
                        //dGV_TrayMap.Rows[x].Cells[y].Value = 1;
                        LsLoadDicPoint[$"{y},{x}"] = true;
                    }
                }
                dGV_LoadTrayMap.ClearSelection();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (设备状态 == 1 || LsLoadDicPoint == null) return;

            DialogResult result = MessageBox.Show("请确定是否保存?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                for (int i = 0; i < LoadXPoint.Length; i++)
                {
                    for (int j = 0; j < LoadYPoint.Length; j++)
                    {
                        //dGV_TrayMap.Rows[j].Cells[i].Value = 0;
                        LoadDicPoint[$"{i},{j}"] = LsLoadDicPoint[$"{i},{j}"];
                    }
                }
                DialogResult = DialogResult.OK;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (设备状态 == 1) return;

            for (int i = 0; i < LoadXPoint.Length; i++)
            {
                for (int j = 0; j < LoadYPoint.Length; j++)
                {
                    dGV_LoadTrayMap.Rows[j].Cells[i].Style.BackColor = Color.LimeGreen;
                    //dGV_TrayMap.Rows[j].Cells[i].Value = 1;
                    LsLoadDicPoint[$"{i},{j}"] = true;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (设备状态 == 1) return;

            for (int i = 0; i < LoadXPoint.Length; i++)
            {
                for (int j = 0; j < LoadYPoint.Length; j++)
                {
                    dGV_LoadTrayMap.Rows[j].Cells[i].Style.BackColor = Color.White;
                    //dGV_TrayMap.Rows[j].Cells[i].Value = 1;
                    LsLoadDicPoint[$"{i},{j}"] = false;
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (设备状态 == 1) return;

            for (int i = 0; i < LoadXPoint.Length; i++)
            {
                for (int j = 0; j < LoadYPoint.Length; j++)
                {
                    if (dGV_LoadTrayMap.Rows[j].Cells[i].Style.BackColor == Color.LimeGreen)
                    {
                        dGV_LoadTrayMap.Rows[j].Cells[i].Style.BackColor = Color.White;
                        //dGV_TrayMap.Rows[j].Cells[i].Value = 0;
                        LsLoadDicPoint[$"{i},{j}"] = false;
                    }
                    else
                    {
                        dGV_LoadTrayMap.Rows[j].Cells[i].Style.BackColor = Color.LimeGreen;
                        //dGV_TrayMap.Rows[j].Cells[i].Value = 1;
                        LsLoadDicPoint[$"{i},{j}"] = true;
                    }
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (设备状态 == 1) return;

            LsLoadDicPoint = new Dictionary<string, bool>();
            for (int i = 0; i < LoadXPoint.Length; i++)
            {
                for (int j = 0; j < LoadYPoint.Length; j++)
                {
                    LsLoadDicPoint.Add($"{i},{j}", LoadDicPoint[$"{i},{j}"]);
                }
            }

            for (int i = 0; i < LoadXPoint.Length; i++)
            {
                for (int j = 0; j < LoadYPoint.Length; j++)
                {
                    DataGridViewCell aa = dGV_LoadTrayMap.Rows[j].Cells[i];
                    if (LoadDicPoint[$"{i},{j}"])
                    {
                        aa.Style.BackColor = Color.LimeGreen;
                        //dGV_TrayMap.Rows[j].Cells[i].Value = 1;
                    }
                    else
                    {
                        aa.Style.BackColor = Color.White;
                        //dGV_TrayMap.Rows[j].Cells[i].Value = 0;
                    }
                }
            }
        }

        private bool CheckChange()
        {
            for (int i = 0; i < LoadXPoint.Length; i++)
            {
                for (int j = 0; j < LoadYPoint.Length; j++)
                {
                    if (LoadDicPoint[$"{i},{j}"] != LsLoadDicPoint[$"{i},{j}"])
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (设备状态 == 1) DialogResult = DialogResult.Cancel;

            if (LsLoadDicPoint != null && CheckChange())
            {
                DialogResult dialog = MessageBox.Show("Map图有被修改,请确认是否退出!", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (dialog == DialogResult.OK)
                {
                    DialogResult = DialogResult.Cancel;
                }
                else Timer.Stop();
            }
            else DialogResult = DialogResult.Cancel;
        }

        private void LoadMap_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult != DialogResult.Cancel)
            {
                if (设备状态 != 1 && LsLoadDicPoint != null && CheckChange())
                {
                    DialogResult dialog = MessageBox.Show("Map图有被修改,请确认是否退出!", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                    if (dialog == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                        DialogResult = DialogResult.Cancel;
                    }
                    else Timer.Stop();
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            设备状态 = 1;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            设备状态 = 0;

        }
    }
}
