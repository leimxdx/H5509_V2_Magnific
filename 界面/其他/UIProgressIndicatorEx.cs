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
    public partial class UIProgressIndicatorEx : UserControl
    {
        public UIProgressIndicatorEx()
        {
            InitializeComponent();

            // 确保控件已初始化
            if (uiProgressIndicator1 != null)
            {
                uiProgressIndicator1.Active = false; // 初始状态设为非激活
                uiProgressIndicator1.ForeColor = UIStyles.Blue.ProgressIndicatorColor; // 设置默认颜色
            }
        }

        [Description("显示的文本内容")]
        [Category("SunnyUI")]  // 添加分类
        [DefaultValue("")]     // 添加默认值
        [Browsable(true)]      // 确保属性在属性窗口中可见
        public string IndicateText
        {
            get => lblSelect.Text;
            set
            {
                lblSelect.Text = value;
                Invalidate();
            }
        }

        // 添加Active属性代理
        [Description("是否激活动态显示")]
        [Category("SunnyUI")]
        [DefaultValue(false)]
        public bool Active
        {
            get => uiProgressIndicator1.Active;
            set => uiProgressIndicator1.Active = value;
        }

        // 添加ForeColor属性代理
        [Description("进度指示器颜色")]
        [Category("SunnyUI")]
        [DefaultValue(typeof(Color), "80, 160, 255")]
        public new Color ForeColor
        {
            get => uiProgressIndicator1.ForeColor;
            set
            {
                uiProgressIndicator1.ForeColor = value;
                base.ForeColor = value;
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
            uiProgressIndicator1.Active = true; // 确保Active属性被设置
        }

        public void Stop()
        {
            uiProgressIndicator1.Stop();
            uiProgressIndicator1.Active = false; // 确保Active属性被设置
        }

        // 确保在控件大小改变时重绘
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            uiProgressIndicator1?.Invalidate();
        }
    }
}