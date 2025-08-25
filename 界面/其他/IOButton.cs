using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sunny.UI;
using System.Diagnostics.Contracts;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using static H5509_V2.APPmaster;
using MotionCommon;

namespace H5509_V2
{
    public partial class IOButton : UIUserControl
    {
        MotionEx motionEx;

        public Color _color { get; set; }
        public bool value { get; set; }
        public IOInfo ioInfo { get; set; }

        public IOButton(MotionEx motionex)
        {
            InitializeComponent();

            motionEx = motionex;
            //默认颜色
            _color = Color.Gray;
            //默认值
            value = false;

        }

        private void IOButton_Paint(object sender, PaintEventArgs e)
        {

            //IO编号
            //e.Graphics.DrawString(Name.Substring(9), Font, ForeColor, new Rectangle(0, 0, Width, 40), ContentAlignment.MiddleLeft);

            //画点
            e.Graphics.FillEllipse(_color, new Rectangle(44, 10, 20, 20));

            //IO名称
            e.Graphics.DrawString(Text, Font, ForeColor, new Rectangle(62, 0, Width, 40), ContentAlignment.MiddleLeft);

            //动态调整字体大小
            //using (Graphics g = this.CreateGraphics())
            //{

            //while (g.MeasureString(this.Text, this.Font).Width > (this.Width - 62) && this.Font.Size > 6)
            //{
            //    this.Font = new Font(this.Font.Name, this.Font.Size - 1, this.Font.Style);
            //}
            //}
            while (TextRenderer.MeasureText(this.Text, this.Font).Width > (this.Width - 62) && this.Font.Size > 6)
            {
                this.Font = new Font(this.Font.Name, this.Font.Size - 1, this.Font.Style);
            }

        }

        private void IOButton_Click(object sender, EventArgs e)
        {
            if (!APPmaster.InitedOK)
            {
                UIMessageTip.ShowError(this.Parent, "轴卡未连接", 1000, true, true);
                return;
            }

            if (APPmaster.设备状态 == 1)
            {
                UIMessageTip.ShowError(this.Parent, "设备正在运行...", 1000, true, true);
                return;
            }

            if (uiLabel1.Text[0] == 'X')
            {
                return;
            }

            if (_color == Color.Gray)
            {
                UIMessageTip.Show(this.Parent, "未初始化", null, 1000, true, true);
                //UIMessageTip.Show("未初始化", null, 1000, true);
                return;
            }


            if (value == true)
            {

                string result = motionEx.SetDo(uiLabel1.Text + Text, false);

                if (result == "Success")
                {
                    UIMessageTip.ShowOk(this, "关闭" + uiLabel1.Text + ":" + Text, 1000, true, true);
                    //value = false;
                    //_color = Color.Red;
                }
                else
                {
                    UIMessageTip.ShowError(this, "关闭" + uiLabel1.Text + ":" + Text + "\n错误：" + result, 1000, true, true);
                }


            }
            else
            {

                string result = motionEx.SetDo(uiLabel1.Text + Text, true);

                if (result == "Success")
                {
                    UIMessageTip.ShowOk(this, "打开" + uiLabel1.Text + ":" + Text, 1000, true, true);
                    //value = true;
                    //_color = Color.Lime;
                }
                else
                {
                    UIMessageTip.ShowError(this, "打开" + uiLabel1.Text + ":" + Text + "\n错误：" + result, 1000, true, true);
                }
            }

            this.Refresh();

        }

        private void uiLabel1_Click(object sender, EventArgs e)
        {
            IOButton_Click(this.Parent,e);
        }
    }
}
