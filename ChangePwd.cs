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
    public partial class ChangePwd : Form
    {
        public string RealName { get; set; }

        public string OldPassword { get; set; }

        public string NewPassword { get; set; }

        public string PermissoionTime { get; set; } //S

        public ChangePwd()
        {
            InitializeComponent();
        }

        private void button208_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void button207_Click(object sender, EventArgs e)
        {
            if (textBox84.Text.Length == 0)
            {
                MessageBox.Show("请输入旧密码!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox84.Focus();
                return;
            }

            if (textBox84.Text != OldPassword)
            {
                MessageBox.Show("旧密码错误!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox84.Text = "";
                textBox84.Focus();
                return;
            }

            if (textBox1.Text.Length == 0)
            {
                if (numericUpDown85.Text != PermissoionTime)
                {
                    DialogResult dialog2 = MessageBox.Show("请确认是否修改权限时间?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                    if (dialog2 == DialogResult.OK)
                    {
                        NewPassword = OldPassword;
                        PermissoionTime = numericUpDown85.Text;
                        DialogResult = DialogResult.OK;
                    }
                }
                else
                {
                    MessageBox.Show("请输入新密码!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBox1.Focus();
                    return;
                }
            }
            else
            {
                DialogResult dialog = MessageBox.Show("请确认是否保存?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (dialog == DialogResult.OK)
                {
                    NewPassword = textBox1.Text;
                    PermissoionTime = numericUpDown85.Text;
                    DialogResult = DialogResult.OK;
                }
            }
        }

        private void ChangePwd_Load(object sender, EventArgs e)
        {
            //textBox84.Text = OldPassword;
            this.Text = RealName;
            numericUpDown85.Text = PermissoionTime;
        }
    }
}
