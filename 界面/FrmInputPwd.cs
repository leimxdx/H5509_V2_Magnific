using System;
using System.Windows.Forms;

namespace H5509
{
    public partial class FrmInputPwd : Form
    {
        public string Password { get; set; }
        public FrmInputPwd()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Password = textBox1.Text;
            DialogResult = DialogResult.OK;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Password = textBox1.Text;
                DialogResult = DialogResult.OK;
            }
        }
    }
}
