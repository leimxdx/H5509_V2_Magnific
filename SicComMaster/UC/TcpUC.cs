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
using SicUIAssist;

namespace SicComMaster
{
    public partial class TcpUC : UserControl
    {
        #region 窗体双缓存设定
        /// <summary>
        /// 防止控件过多产生闪烁现象
        /// </summary>
        private void SetControlStyle()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true); // 双缓冲
            this.UpdateStyles();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }

        #endregion

        [Description("串口通信-连接按钮事件"), Category("自定义事件")]
        public event Func<string, bool> ConnectClick;

        [Description("串口通信-断开按钮事件"), Category("自定义事件")]
        public event Func<string, bool> DisconnectClick;

        public TcpUC()
        {
            SetControlStyle();
            InitializeComponent();
            ControlDoubleBufferUtil.EnableDoubleBuffering(this, typeof(TableLayoutPanel));
        }

        /// <summary>
        /// 刷新UI
        /// </summary>
        /// <param name="isOpen">TCP开启状态</param>
        /// <param name="ip">Ip地址</param>
        /// <param name="port">端口</param>
        public void RefreshTCP(bool isOpen,string ip, int port)
        {
            this.Invoke(new Action(() =>
            {
                tb_IP.Text = ip ?? "127.0.0.1";
                tbI_PortNo.IntValue = port;
            }));

            SafeUpdateUI((bool status) => ControlEnable(status), isOpen);
        }

        private void SafeUpdateUI(Action<bool> action, bool arg)
        {
            if (InvokeRequired)
                Invoke(action, arg);
            else
                action(arg);
        }

        private void ControlEnable(bool isOpen)
        {
            tb_IP.Enabled = !isOpen;
            tbI_PortNo.Enabled = !isOpen;
            btn_Connect.Enabled = !isOpen;
            btn_DisConnect.Enabled = isOpen;

            lbl_LinkStatus.Text = isOpen ? "[已连接]" : "[未连接]";
            lbl_LinkStatus.ForeColor = isOpen ? Color.Green : Color.Red;
        }

        private void btn_Connect_Click(object sender, EventArgs e)
        {
            if (tb_IP.Text.Length == 0)
            {
                UIMessageBox.ShowError("请先输入IP地址再继续！");
                return;
            }

            string comStr = $"{tb_IP.Text},"
                          + $"{tbI_PortNo.IntValue.ToString()}";

            if (ConnectClick?.Invoke(comStr) ?? true)
                SafeUpdateUI((bool isOpen) => ControlEnable(isOpen), true);
        }

        private void btn_DisConnect_Click(object sender, EventArgs e)
        {
            if (DisconnectClick?.Invoke("") ?? true)
                SafeUpdateUI((bool isOpen) => ControlEnable(isOpen), false);
        }

    }
}
