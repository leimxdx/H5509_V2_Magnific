using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
