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
    public partial class Defalut2 : UserControl
    {
        public int Dut连续不良报警个数 = 0;

        public bool Dut连续不良报警使能 = false;

        public double 总良率报警百分比 = 0;

        public bool 总良率报警使能 = false;

        public int testnum = 0;

        public int inputnum = 0;

        public int outputnum = 0;

        public int totalnum = 0;

        public Defalut2()
        {
            InitializeComponent();
        }

        private void UpdateData()
        {
            textBox1.Text = Dut连续不良报警个数.ToString();
            textBox2.Text = 总良率报警百分比.ToString();

            textBox4.Text = testnum.ToString();
            textBox5.Text = inputnum.ToString();
            textBox6.Text = outputnum.ToString();

            textBox3.Text = $"{testnum + inputnum + outputnum}/{totalnum}";
            //uiCheckBox192.Checked = Dut连续不良报警使能;
            //uiCheckBox1.Checked = 总良率报警使能;
        }

        public void RefreshUI()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    UpdateData();
                }));
            }
            else UpdateData();
        }

    }
}
