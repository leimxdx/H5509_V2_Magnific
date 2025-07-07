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
