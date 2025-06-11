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
    public partial class NewSetupName : Form
    {
        public string Name;
        public NewSetupName()
        {
            InitializeComponent();
        }

        private void uiButton1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim().Length == 0) return;
            Name = textBox1.Text;
            DialogResult = DialogResult.OK;
        }

        private void uiButton2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
