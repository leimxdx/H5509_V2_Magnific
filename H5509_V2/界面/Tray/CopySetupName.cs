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
    public partial class CopySetupName : Form
    {
        public string Name;

        public string jName;
        public CopySetupName()
        {
            InitializeComponent();
        }

        private void uiButton1_Click(object sender, EventArgs e)
        {
            if (uiTextBox1.Text.Trim().Length == 0) return;
            if (textBox1.Text.Trim() == uiTextBox1.Text.Trim())
            {
                MessageBox.Show("档案名称一样,无法复制!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            DialogResult reuslt = MessageBox.Show("请确认是否保存复制的新档案?","提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (reuslt == DialogResult.OK)
            {
                Name = uiTextBox1.Text;
                DialogResult = DialogResult.OK;
            }
        }

        private void uiButton2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void CopySetupName_Load(object sender, EventArgs e)
        {
            textBox1.Text = jName;
            uiTextBox1.Text = jName + "-2";
        }
    }
}
