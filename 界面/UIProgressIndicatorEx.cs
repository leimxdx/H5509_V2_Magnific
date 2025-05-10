using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace H5509_V2.界面
{
    public partial class UIProgressIndicatorEx :  UserControl
    {
        private readonly UIProgressIndicator progressIndicator;
        public UIProgressIndicatorEx()
        {
            InitializeComponent();
          
        }
        
        [Description("显示的文本内容")]
        private string IndicateText 
        {  get=>lblSelect.Text;
            set {
                lblSelect.Text = value;
                Invalidate();
            } 
        }
        // 添加一个公共方法用于重新计算位置
        public void CenterToParent()
        {
            if (this.Parent != null)
            {
                this.Location = new Point(
                    (Parent.ClientSize.Width - this.Width) / 2,
                    (Parent.ClientSize.Height - this.Height) / 2);
            }
        }
       
        public void Start()
        {
            uiProgressIndicator1.Start();
        }

        public void Stop()
        {
            uiProgressIndicator1.Stop();
        }
    
    }
}
