using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DocumentFormat.OpenXml.Spreadsheet;
using MotionCommon;


namespace H5509_V2
{
    public partial class NewHeatTrayInfo : Form
    {
        public HeatInfo heatInfo = new HeatInfo();
        public NewHeatTrayInfo()
        {
            InitializeComponent();
        }

        private void uiButton2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void uiButton1_Click(object sender, EventArgs e)
        {
            if (textBox57.Text.Trim().Length == 0)
            {
                MessageBox.Show("请输入加热盘名称", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox57.Focus();
                return;
            }

            int xnum = Convert.ToInt32(textBox60.Text);
            if (xnum == 0)
            {
                MessageBox.Show("X数量不能为0", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox60.Focus();
                return;
            }

            double ynum = Convert.ToInt32(textBox59.Text);
            if (ynum == 0)
            {
                MessageBox.Show("Y数量不能为0", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox59.Focus();
                return;
            }

            double x_start = Convert.ToDouble(textBox58.Text);
            if (x_start == 0)
            {
                MessageBox.Show("X_Start不能为0", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox58.Focus();
                return;
            }

            double y_start = Convert.ToDouble(textBox56.Text);
            if (y_start == 0)
            {
                MessageBox.Show("Y_Start不能为0", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox56.Focus();
                return;
            }

            double x_pitch = Convert.ToDouble(textBox55.Text);
            if (x_pitch == 0)
            {
                MessageBox.Show("X_Pitch不能为0", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox55.Focus();
                return;
            }

            double y_pitch = Convert.ToDouble(textBox54.Text);
            if (y_pitch == 0)
            {
                MessageBox.Show("Y_Pitch不能为0", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox54.Focus();
                return;
            }

            double width = Convert.ToDouble(textBox53.Text);
            if (width == 0)
            {
                MessageBox.Show("Width不能为0", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox53.Focus();
                return;
            }

            double length = Convert.ToDouble(textBox52.Text);
            if (length == 0)
            {
                MessageBox.Show("Length不能为0", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox52.Focus();
                return;
            }

            double height = Convert.ToDouble(textBox51.Text);
            if (height == 0)
            {
                MessageBox.Show("Height不能为0", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox51.Focus();
                return;
            }
            heatInfo = new HeatInfo()
            {
                HeatName = textBox57.Text,
                X_Num = Convert.ToInt32(textBox60.Text),
                Y_Num = Convert.ToInt32(textBox59.Text),
                X_Start = Convert.ToDouble(textBox58.Text),
                Y_Start = Convert.ToDouble(textBox56.Text),
                X_Pitch = Convert.ToDouble(textBox55.Text),
                Y_Pitch = Convert.ToDouble(textBox54.Text),
                Width = Convert.ToDouble(textBox53.Text),
                Length = Convert.ToDouble(textBox52.Text),
                Height = Convert.ToDouble(textBox51.Text),
            };
            DialogResult diag = MessageBox.Show($"请确认是否新增加热盘信息({heatInfo.HeatName})?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (diag == DialogResult.OK)
            {
                DialogResult = DialogResult.OK;
            }
        }
    }
}
