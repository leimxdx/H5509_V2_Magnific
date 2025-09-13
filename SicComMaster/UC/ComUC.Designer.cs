namespace SicComMaster
{
    partial class ComUC
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lbl_LinkStatus = new Sunny.UI.UILabel();
            this.cbx_StopBits = new Sunny.UI.UIComboBox();
            this.cbx_DataBits = new Sunny.UI.UIComboBox();
            this.cbx_Parity = new Sunny.UI.UIComboBox();
            this.uiLabel1 = new Sunny.UI.UILabel();
            this.uiLabel2 = new Sunny.UI.UILabel();
            this.uiLabel3 = new Sunny.UI.UILabel();
            this.uiLabel5 = new Sunny.UI.UILabel();
            this.uiLabel4 = new Sunny.UI.UILabel();
            this.cbx_COM = new Sunny.UI.UIComboBox();
            this.cbx_Baudrate = new Sunny.UI.UIComboBox();
            this.btn_Connect = new Sunny.UI.UISymbolButton();
            this.btn_DisConnect = new Sunny.UI.UISymbolButton();
            this.uiTitlePanel1 = new Sunny.UI.UITitlePanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.uiTitlePanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(223)))), ((int)(((byte)(240)))));
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel1.Controls.Add(this.lbl_LinkStatus, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.cbx_StopBits, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.cbx_DataBits, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.cbx_Parity, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.uiLabel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.uiLabel2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.uiLabel3, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.uiLabel5, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.uiLabel4, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.cbx_COM, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.cbx_Baudrate, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.btn_Connect, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.btn_DisConnect, 2, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(1, 45);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(378, 251);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // lbl_LinkStatus
            // 
            this.lbl_LinkStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbl_LinkStatus.Font = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Bold);
            this.lbl_LinkStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(87)))), ((int)(((byte)(34)))));
            this.lbl_LinkStatus.Location = new System.Drawing.Point(253, 0);
            this.lbl_LinkStatus.Name = "lbl_LinkStatus";
            this.lbl_LinkStatus.Size = new System.Drawing.Size(122, 50);
            this.lbl_LinkStatus.TabIndex = 9;
            this.lbl_LinkStatus.Text = "[未连接]";
            this.lbl_LinkStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cbx_StopBits
            // 
            this.cbx_StopBits.DataSource = null;
            this.cbx_StopBits.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbx_StopBits.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            this.cbx_StopBits.DropDownWidth = 120;
            this.cbx_StopBits.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(223)))), ((int)(((byte)(240)))));
            this.cbx_StopBits.FillDisableColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(200)))), ((int)(((byte)(225)))));
            this.cbx_StopBits.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cbx_StopBits.ItemHeight = 40;
            this.cbx_StopBits.ItemHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(200)))), ((int)(((byte)(255)))));
            this.cbx_StopBits.Items.AddRange(new object[] {
            "1",
            "1.5",
            "2"});
            this.cbx_StopBits.ItemSelectForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.cbx_StopBits.Location = new System.Drawing.Point(104, 205);
            this.cbx_StopBits.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbx_StopBits.MinimumSize = new System.Drawing.Size(63, 0);
            this.cbx_StopBits.Name = "cbx_StopBits";
            this.cbx_StopBits.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            this.cbx_StopBits.Size = new System.Drawing.Size(142, 41);
            this.cbx_StopBits.SymbolSize = 24;
            this.cbx_StopBits.TabIndex = 6;
            this.cbx_StopBits.Text = "1";
            this.cbx_StopBits.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.cbx_StopBits.Watermark = "";
            // 
            // cbx_DataBits
            // 
            this.cbx_DataBits.DataSource = null;
            this.cbx_DataBits.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbx_DataBits.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            this.cbx_DataBits.DropDownWidth = 120;
            this.cbx_DataBits.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(223)))), ((int)(((byte)(240)))));
            this.cbx_DataBits.FillDisableColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(200)))), ((int)(((byte)(225)))));
            this.cbx_DataBits.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cbx_DataBits.ItemHeight = 40;
            this.cbx_DataBits.ItemHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(200)))), ((int)(((byte)(255)))));
            this.cbx_DataBits.Items.AddRange(new object[] {
            "7",
            "8"});
            this.cbx_DataBits.ItemSelectForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.cbx_DataBits.Location = new System.Drawing.Point(104, 155);
            this.cbx_DataBits.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbx_DataBits.MinimumSize = new System.Drawing.Size(63, 0);
            this.cbx_DataBits.Name = "cbx_DataBits";
            this.cbx_DataBits.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            this.cbx_DataBits.Size = new System.Drawing.Size(142, 40);
            this.cbx_DataBits.SymbolSize = 24;
            this.cbx_DataBits.TabIndex = 6;
            this.cbx_DataBits.Text = "7";
            this.cbx_DataBits.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.cbx_DataBits.Watermark = "";
            // 
            // cbx_Parity
            // 
            this.cbx_Parity.DataSource = null;
            this.cbx_Parity.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbx_Parity.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            this.cbx_Parity.DropDownWidth = 120;
            this.cbx_Parity.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(223)))), ((int)(((byte)(240)))));
            this.cbx_Parity.FillDisableColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(200)))), ((int)(((byte)(225)))));
            this.cbx_Parity.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cbx_Parity.ItemHeight = 40;
            this.cbx_Parity.ItemHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(200)))), ((int)(((byte)(255)))));
            this.cbx_Parity.Items.AddRange(new object[] {
            "None",
            "Odd",
            "Even"});
            this.cbx_Parity.ItemSelectForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.cbx_Parity.Location = new System.Drawing.Point(104, 105);
            this.cbx_Parity.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbx_Parity.MinimumSize = new System.Drawing.Size(63, 0);
            this.cbx_Parity.Name = "cbx_Parity";
            this.cbx_Parity.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            this.cbx_Parity.Size = new System.Drawing.Size(142, 40);
            this.cbx_Parity.SymbolSize = 24;
            this.cbx_Parity.TabIndex = 6;
            this.cbx_Parity.Text = "Odd";
            this.cbx_Parity.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.cbx_Parity.Watermark = "";
            // 
            // uiLabel1
            // 
            this.uiLabel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiLabel1.Font = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Bold);
            this.uiLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(54)))), ((int)(((byte)(81)))));
            this.uiLabel1.Location = new System.Drawing.Point(3, 0);
            this.uiLabel1.Name = "uiLabel1";
            this.uiLabel1.Size = new System.Drawing.Size(94, 50);
            this.uiLabel1.TabIndex = 0;
            this.uiLabel1.Text = "串口号:";
            this.uiLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiLabel2
            // 
            this.uiLabel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiLabel2.Font = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Bold);
            this.uiLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(54)))), ((int)(((byte)(81)))));
            this.uiLabel2.Location = new System.Drawing.Point(3, 50);
            this.uiLabel2.Name = "uiLabel2";
            this.uiLabel2.Size = new System.Drawing.Size(94, 50);
            this.uiLabel2.TabIndex = 1;
            this.uiLabel2.Text = "波特率:";
            this.uiLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiLabel3
            // 
            this.uiLabel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiLabel3.Font = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Bold);
            this.uiLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(54)))), ((int)(((byte)(81)))));
            this.uiLabel3.Location = new System.Drawing.Point(3, 100);
            this.uiLabel3.Name = "uiLabel3";
            this.uiLabel3.Size = new System.Drawing.Size(94, 50);
            this.uiLabel3.TabIndex = 2;
            this.uiLabel3.Text = "奇偶位:";
            this.uiLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiLabel5
            // 
            this.uiLabel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiLabel5.Font = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Bold);
            this.uiLabel5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(54)))), ((int)(((byte)(81)))));
            this.uiLabel5.Location = new System.Drawing.Point(3, 150);
            this.uiLabel5.Name = "uiLabel5";
            this.uiLabel5.Size = new System.Drawing.Size(94, 50);
            this.uiLabel5.TabIndex = 4;
            this.uiLabel5.Text = "数据位:";
            this.uiLabel5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiLabel4
            // 
            this.uiLabel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiLabel4.Font = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Bold);
            this.uiLabel4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(54)))), ((int)(((byte)(81)))));
            this.uiLabel4.Location = new System.Drawing.Point(3, 200);
            this.uiLabel4.Name = "uiLabel4";
            this.uiLabel4.Size = new System.Drawing.Size(94, 51);
            this.uiLabel4.TabIndex = 3;
            this.uiLabel4.Text = "停止位:";
            this.uiLabel4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cbx_COM
            // 
            this.cbx_COM.DataSource = null;
            this.cbx_COM.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbx_COM.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            this.cbx_COM.DropDownWidth = 120;
            this.cbx_COM.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(223)))), ((int)(((byte)(240)))));
            this.cbx_COM.FillDisableColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(200)))), ((int)(((byte)(225)))));
            this.cbx_COM.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cbx_COM.ItemHeight = 40;
            this.cbx_COM.ItemHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(200)))), ((int)(((byte)(255)))));
            this.cbx_COM.Items.AddRange(new object[] {
            "COM1",
            "COM2",
            "COM3",
            "COM4",
            "COM5",
            "COM6",
            "COM7",
            "COM8",
            "COM9",
            "COM10",
            "COM11",
            "COM12"});
            this.cbx_COM.ItemSelectForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.cbx_COM.Location = new System.Drawing.Point(104, 5);
            this.cbx_COM.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbx_COM.MinimumSize = new System.Drawing.Size(63, 0);
            this.cbx_COM.Name = "cbx_COM";
            this.cbx_COM.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            this.cbx_COM.Size = new System.Drawing.Size(142, 40);
            this.cbx_COM.SymbolSize = 24;
            this.cbx_COM.TabIndex = 5;
            this.cbx_COM.Text = "COM1";
            this.cbx_COM.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.cbx_COM.Watermark = "";
            // 
            // cbx_Baudrate
            // 
            this.cbx_Baudrate.DataSource = null;
            this.cbx_Baudrate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbx_Baudrate.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            this.cbx_Baudrate.DropDownWidth = 120;
            this.cbx_Baudrate.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(223)))), ((int)(((byte)(240)))));
            this.cbx_Baudrate.FillDisableColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(200)))), ((int)(((byte)(225)))));
            this.cbx_Baudrate.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cbx_Baudrate.ItemHeight = 40;
            this.cbx_Baudrate.ItemHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(200)))), ((int)(((byte)(255)))));
            this.cbx_Baudrate.Items.AddRange(new object[] {
            "9600",
            "19200",
            "38400",
            "57600",
            "115200"});
            this.cbx_Baudrate.ItemSelectForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.cbx_Baudrate.Location = new System.Drawing.Point(104, 55);
            this.cbx_Baudrate.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbx_Baudrate.MinimumSize = new System.Drawing.Size(63, 0);
            this.cbx_Baudrate.Name = "cbx_Baudrate";
            this.cbx_Baudrate.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            this.cbx_Baudrate.Size = new System.Drawing.Size(142, 40);
            this.cbx_Baudrate.SymbolSize = 24;
            this.cbx_Baudrate.TabIndex = 6;
            this.cbx_Baudrate.Text = "19200";
            this.cbx_Baudrate.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.cbx_Baudrate.Watermark = "";
            // 
            // btn_Connect
            // 
            this.btn_Connect.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_Connect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Connect.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(136)))));
            this.btn_Connect.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(136)))));
            this.btn_Connect.FillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(171)))), ((int)(((byte)(160)))));
            this.btn_Connect.FillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(109)))));
            this.btn_Connect.FillSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(109)))));
            this.btn_Connect.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_Connect.LightColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.btn_Connect.Location = new System.Drawing.Point(253, 53);
            this.btn_Connect.MinimumSize = new System.Drawing.Size(1, 1);
            this.btn_Connect.Name = "btn_Connect";
            this.btn_Connect.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(136)))));
            this.btn_Connect.RectHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(171)))), ((int)(((byte)(160)))));
            this.btn_Connect.RectPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(109)))));
            this.btn_Connect.RectSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(109)))));
            this.tableLayoutPanel1.SetRowSpan(this.btn_Connect, 2);
            this.btn_Connect.Size = new System.Drawing.Size(122, 94);
            this.btn_Connect.Style = Sunny.UI.UIStyle.Custom;
            this.btn_Connect.Symbol = 361633;
            this.btn_Connect.TabIndex = 7;
            this.btn_Connect.Text = "连 接";
            this.btn_Connect.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_Connect.Click += new System.EventHandler(this.btn_Connect_Click);
            // 
            // btn_DisConnect
            // 
            this.btn_DisConnect.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_DisConnect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_DisConnect.Enabled = false;
            this.btn_DisConnect.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(87)))), ((int)(((byte)(34)))));
            this.btn_DisConnect.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(87)))), ((int)(((byte)(34)))));
            this.btn_DisConnect.FillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(121)))), ((int)(((byte)(78)))));
            this.btn_DisConnect.FillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(70)))), ((int)(((byte)(28)))));
            this.btn_DisConnect.FillSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(70)))), ((int)(((byte)(28)))));
            this.btn_DisConnect.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_DisConnect.LightColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(244)))), ((int)(((byte)(240)))));
            this.btn_DisConnect.Location = new System.Drawing.Point(253, 153);
            this.btn_DisConnect.MinimumSize = new System.Drawing.Size(1, 1);
            this.btn_DisConnect.Name = "btn_DisConnect";
            this.btn_DisConnect.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(87)))), ((int)(((byte)(34)))));
            this.btn_DisConnect.RectHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(121)))), ((int)(((byte)(78)))));
            this.btn_DisConnect.RectPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(70)))), ((int)(((byte)(28)))));
            this.btn_DisConnect.RectSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(70)))), ((int)(((byte)(28)))));
            this.tableLayoutPanel1.SetRowSpan(this.btn_DisConnect, 2);
            this.btn_DisConnect.Size = new System.Drawing.Size(122, 95);
            this.btn_DisConnect.Style = Sunny.UI.UIStyle.Custom;
            this.btn_DisConnect.Symbol = 361735;
            this.btn_DisConnect.TabIndex = 8;
            this.btn_DisConnect.Text = "断 开";
            this.btn_DisConnect.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_DisConnect.Click += new System.EventHandler(this.btn_DisConnect_Click);
            // 
            // uiTitlePanel1
            // 
            this.uiTitlePanel1.Controls.Add(this.tableLayoutPanel1);
            this.uiTitlePanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiTitlePanel1.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(223)))), ((int)(((byte)(240)))));
            this.uiTitlePanel1.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(223)))), ((int)(((byte)(240)))));
            this.uiTitlePanel1.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiTitlePanel1.Location = new System.Drawing.Point(0, 0);
            this.uiTitlePanel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiTitlePanel1.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiTitlePanel1.Name = "uiTitlePanel1";
            this.uiTitlePanel1.Padding = new System.Windows.Forms.Padding(1, 45, 1, 1);
            this.uiTitlePanel1.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(76)))), ((int)(((byte)(76)))));
            this.uiTitlePanel1.ShowText = false;
            this.uiTitlePanel1.Size = new System.Drawing.Size(380, 297);
            this.uiTitlePanel1.TabIndex = 1;
            this.uiTitlePanel1.Text = null;
            this.uiTitlePanel1.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.uiTitlePanel1.TitleColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(76)))), ((int)(((byte)(76)))));
            this.uiTitlePanel1.TitleHeight = 45;
            // 
            // ComUC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.uiTitlePanel1);
            this.Name = "ComUC";
            this.Size = new System.Drawing.Size(380, 297);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.uiTitlePanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Sunny.UI.UILabel uiLabel1;
        private Sunny.UI.UILabel uiLabel2;
        private Sunny.UI.UILabel uiLabel3;
        private Sunny.UI.UILabel uiLabel4;
        private Sunny.UI.UILabel uiLabel5;
        private Sunny.UI.UITitlePanel uiTitlePanel1;
        private Sunny.UI.UIComboBox cbx_StopBits;
        private Sunny.UI.UIComboBox cbx_DataBits;
        private Sunny.UI.UIComboBox cbx_Parity;
        private Sunny.UI.UIComboBox cbx_COM;
        private Sunny.UI.UIComboBox cbx_Baudrate;
        private Sunny.UI.UISymbolButton btn_Connect;
        private Sunny.UI.UISymbolButton btn_DisConnect;
        private Sunny.UI.UILabel lbl_LinkStatus;
    }
}
