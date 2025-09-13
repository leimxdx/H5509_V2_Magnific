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
    public partial class ComUC : UserControl
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

       [Description("串口通信-连接按钮事件"),Category("自定义事件")]
        public event Func<string, bool> ConnectClick;

        [Description("串口通信-断开按钮事件"), Category("自定义事件")]
        public event Func<string, bool> DisconnectClick;

        public ComUC()
        {
            SetControlStyle();
            InitializeComponent();
            ControlDoubleBufferUtil.EnableDoubleBuffering(this, typeof(TableLayoutPanel));
        }

        /// <summary>
        /// 刷新UI
        /// </summary>
        /// <param name="isOpen">串口打开状态</param>
        /// <param name="com">串口号</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="parity">奇偶校验</param>
        /// <param name="databits">数据位</param>
        /// <param name="stopbits">停止位</param>
        public void RefreshCOM(bool isOpen,string com, string baudRate,string parity,string databits,string stopbits)
        {
            this.Invoke(new Action(() =>
            {
                cbx_COM.SelectedItem = com;
                cbx_Baudrate.SelectedItem = baudRate;
                cbx_Parity.SelectedItem = parity;
                cbx_DataBits.SelectedItem = databits;
                cbx_StopBits.SelectedItem = stopbits;
            }));

            SafeUpdateUI((bool status) => ControlEnable(status), isOpen);
        }

        private void SafeUpdateUI(Action<bool> action,bool arg)
        {
            if (InvokeRequired)
                Invoke(action, arg);
            else
                action(arg);
        }

        private void ControlEnable(bool isOpen)
        {
            cbx_COM.Enabled = !isOpen;
            cbx_Baudrate.Enabled = !isOpen;
            cbx_Parity.Enabled = !isOpen;
            cbx_DataBits.Enabled = !isOpen;
            cbx_StopBits.Enabled = !isOpen;
            btn_Connect.Enabled = !isOpen;
            btn_DisConnect.Enabled = isOpen;

            lbl_LinkStatus.Text = isOpen ? "[已连接]" : "[未连接]";
            lbl_LinkStatus.ForeColor = isOpen ? Color.Green : Color.Red;
        }

        private void btn_Connect_Click(object sender, EventArgs e)
        {
            if (cbx_COM.SelectedIndex == -1)
            {
                UIMessageBox.ShowError("请选择串口后才能连接端口");
                return;
            }

            string comStr = $"{cbx_COM.SelectedItem?.ToString()},"
                          + $"{cbx_Baudrate.SelectedItem.ToString()},"
                          + $"{cbx_Parity.SelectedItem.ToString()},"
                          + $"{cbx_DataBits.SelectedItem.ToString()},"
                          + $"{cbx_StopBits.SelectedItem.ToString()},";

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
