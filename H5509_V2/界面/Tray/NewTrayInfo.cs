using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MotionCommon;

namespace H5509_V2
{
    public partial class NewTrayInfo : Form
    {
        public TrayInfo trayInfo = new TrayInfo();

        public NewTrayInfo()
        {
            InitializeComponent();
        }

        private void uiButton1_Click(object sender, EventArgs e)
        {
            if (textBox30.Text.Trim().Length == 0)
            {
                MessageBox.Show("请输入Tary盘名称", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox30.Focus();
                return;
            }

            int xnum = Convert.ToInt32(textBox29.Text);
            if (xnum == 0)
            {
                MessageBox.Show("X数量不能为0", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox29.Focus();
                return;
            }

            double ynum = Convert.ToInt32(textBox28.Text);
            if (ynum == 0)
            {
                MessageBox.Show("Y数量不能为0", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox28.Focus();
                return;
            }

            double x_start = Convert.ToDouble(textBox27.Text);
            if (x_start == 0)
            {
                MessageBox.Show("X_Start不能为0", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox27.Focus();
                return;
            }

            double y_start = Convert.ToDouble(textBox31.Text);
            if (y_start == 0)
            {
                MessageBox.Show("Y_Start不能为0", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox31.Focus();
                return;
            }

            double x_pitch = Convert.ToDouble(textBox33.Text);
            if (x_pitch == 0)
            {
                MessageBox.Show("X_Pitch不能为0", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox33.Focus();
                return;
            }

            double y_pitch = Convert.ToDouble(textBox32.Text);
            if (y_pitch == 0)
            {
                MessageBox.Show("Y_Pitch不能为0", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox32.Focus();
                return;
            }

            double width = Convert.ToDouble(textBox36.Text);
            if (width == 0)
            {
                MessageBox.Show("Width不能为0", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox36.Focus();
                return;
            }

            double length = Convert.ToDouble(textBox35.Text);
            if (length == 0)
            {
                MessageBox.Show("Length不能为0", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox35.Focus();
                return;
            }

            double height = Convert.ToDouble(textBox34.Text);
            if (height == 0)
            {
                MessageBox.Show("Height不能为0", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox34.Focus();
                return;
            }

            trayInfo = new TrayInfo()
            {
                TrayName = textBox30.Text,
                X_Num = Convert.ToInt32(textBox29.Text),
                Y_Num = Convert.ToInt32(textBox28.Text),
                X_Start = Convert.ToDouble(textBox27.Text),
                Y_Start = Convert.ToDouble(textBox31.Text),
                X_Pitch = Convert.ToDouble(textBox33.Text),
                Y_Pitch = Convert.ToDouble(textBox32.Text),
                Width = Convert.ToDouble(textBox36.Text),
                Length = Convert.ToDouble(textBox35.Text),
                Height = Convert.ToDouble(textBox34.Text),
            };
            DialogResult diag = MessageBox.Show($"请确认是否新增Tray盘信息({trayInfo.TrayName})?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (diag == DialogResult.OK)
            { 
                DialogResult = DialogResult.OK;
            }
        }

        private void uiButton2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
