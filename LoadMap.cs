using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DocumentFormat.OpenXml.Drawing;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
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
            //dGV.EnableHeadersVisualStyles = false;

            dGV.RowsDefaultCellStyle.WrapMode = DataGridViewTriState.False; // 禁止换行
            dGV.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None; // 关闭自动调整行高
            dGV.RowTemplate.Height = (dGV.Size.Height - 23) / y; // 设置所有新行的默认高度

            dGV.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing; // 禁止列标题行高度调整
            dGV.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing; // 禁止行标题列宽度调整

            dGV.Rows.Clear();
            dGV.Columns.Clear();

            for (int i = 0; i < x; i++)
            {
                dGV.Columns.Add("Column" + i.ToString(), (i + 1).ToString("d2"));
                dGV.Columns[i].Width = (dGV.Width - 50) / x; // 设置所有列的宽度
                dGV.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            dGV.Rows.Add(y);

            //if (x == 1)
            //{
            for (int i = 0; i < dGV.Rows.Count; i++)
            {
                dGV.Rows[i].HeaderCell.Value = (i + 1).ToString("d2");
                //dGV.Rows[i].HeaderCell.Style.f
                //dGV.Rows[i].Cells[0].Value = (dGV.Rows.Count - i).ToString("d2");
            }
            //}
            

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
                        aa.Value = 1; // 设置单元格值为1
                        //dGV_TrayMap.Rows[j].Cells[i].Value = 1;
                    }
                    else
                    {
                        aa.Style.BackColor = Color.White;
                        aa.Value = 0; // 设置单元格值为1
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
                        dGV_LoadTrayMap.Rows[x].Cells[y].Value = 0;
                        LsLoadDicPoint[$"{y},{x}"] = false;
                    }
                    else
                    {
                        dGV_LoadTrayMap.Rows[x].Cells[y].Style.BackColor = Color.LimeGreen;
                        dGV_LoadTrayMap.Rows[x].Cells[y].Value = 1;
                        LsLoadDicPoint[$"{y},{x}"] = true;
                    }
                }
                //dGV_LoadTrayMap.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
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
                    dGV_LoadTrayMap.Rows[j].Cells[i].Value = 1;
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
                    dGV_LoadTrayMap.Rows[j].Cells[i].Value = 0;
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
                        dGV_LoadTrayMap.Rows[j].Cells[i].Value = 0;
                        LsLoadDicPoint[$"{i},{j}"] = false;
                    }
                    else
                    {
                        dGV_LoadTrayMap.Rows[j].Cells[i].Style.BackColor = Color.LimeGreen;
                        dGV_LoadTrayMap.Rows[j].Cells[i].Value = 1;
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
                        aa.Value = 1;
                    }
                    else
                    {
                        aa.Style.BackColor = Color.White;
                        aa.Value = 0;
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
                }
            }
            if (!e.Cancel)
            {
                Timer.Stop();
                this.Dispose();
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

        private void dGV_LoadTrayMap_MouseMove(object sender, MouseEventArgs e)
        {
            // 使用 HitTest 定位鼠标位置
            var hitTest = dGV_LoadTrayMap.HitTest(e.X, e.Y);

            // 根据命中结果切换选择模式
            switch (hitTest.Type)
            {
                case DataGridViewHitTestType.ColumnHeader: // 列标题行
                    dGV_LoadTrayMap.ClearSelection();
                    dGV_LoadTrayMap.SelectionMode = DataGridViewSelectionMode.FullColumnSelect;
                    break;
                case DataGridViewHitTestType.RowHeader: // 行标题列
                    dGV_LoadTrayMap.ClearSelection();
                    dGV_LoadTrayMap.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    break;
                case DataGridViewHitTestType.Cell: // 普通单元格
                    dGV_LoadTrayMap.ClearSelection();
                    dGV_LoadTrayMap.SelectionMode = DataGridViewSelectionMode.CellSelect;
                    break;
                // 其他区域（如空白处）可按需处理
                default:
                    dGV_LoadTrayMap.ClearSelection();
                    dGV_LoadTrayMap.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // 恢复原始模式
                    break;
            }
        }

        private void dGV_LoadTrayMap_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && dGV_LoadTrayMap.HitTest(e.X, e.Y).Type != DataGridViewHitTestType.ColumnHeader)
            {
                dGV_LoadTrayMap_CellMouseUp(null, null); //改回前先刷新一下
                dGV_LoadTrayMap.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect; // 恢复行选择
            }
        }

        private void dGV_LoadTrayMap_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex == -1 && e.ColumnIndex >= 0) // 确保点击的是列头
            {
                // 1. 临时切换为 ColumnHeaderSelect 模式（选中整列）
                dGV_LoadTrayMap.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;

                //// 2. 取消之前的选择（可选）
                //dGV_LoadTrayMap.ClearSelection();

                // 3. 选中当前列
                dGV_LoadTrayMap.Columns[e.ColumnIndex].Selected = true;

                // 4. （可选）恢复原选择模式（例如 1 秒后自动恢复）
                // 可以用 Timer 或手动恢复
                // 这里演示手动恢复（例如用户点击其他地方时恢复）
            }
        }
    }
}
