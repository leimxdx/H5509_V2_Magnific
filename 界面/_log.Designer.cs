namespace H5509_V2
{
    partial class _log
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle13 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle14 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle16 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle17 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle18 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle15 = new System.Windows.Forms.DataGridViewCellStyle();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cbbLogLevel = new Sunny.UI.UIComboBox();
            this.startTime = new Sunny.UI.UIDatetimePicker();
            this.endTime = new Sunny.UI.UIDatetimePicker();
            this.label4 = new System.Windows.Forms.Label();
            this.btnTimeSelect = new Sunny.UI.UIButton();
            this.cbbLogType = new Sunny.UI.UIComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.dgv_data = new System.Windows.Forms.DataGridView();
            this.LogTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LogLevel = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LogType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LogContent = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnWriteLog = new Sunny.UI.UIButton();
            this.lblTotalNum = new Sunny.UI.UILabel();
            this.LogTimer = new System.Windows.Forms.Timer(this.components);
            this.txtLog = new Sunny.UI.UITextBox();
            this.txtkeyWords = new Sunny.UI.UITextBox();
            this.btnSelectKey = new Sunny.UI.UIButton();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_data)).BeginInit();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(27, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 16);
            this.label2.TabIndex = 3;
            this.label2.Text = "日志等级";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(609, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(71, 16);
            this.label3.TabIndex = 4;
            this.label3.Text = "开始时间";
            // 
            // cbbLogLevel
            // 
            this.cbbLogLevel.DataSource = null;
            this.cbbLogLevel.FillColor = System.Drawing.Color.White;
            this.cbbLogLevel.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cbbLogLevel.ItemHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(200)))), ((int)(((byte)(255)))));
            this.cbbLogLevel.Items.AddRange(new object[] {
            "全部输出-all",
            "正常-normal",
            "调试-debug",
            "错误-error",
            "警告-warning"});
            this.cbbLogLevel.ItemSelectForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.cbbLogLevel.Location = new System.Drawing.Point(105, 16);
            this.cbbLogLevel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbbLogLevel.MinimumSize = new System.Drawing.Size(63, 0);
            this.cbbLogLevel.Name = "cbbLogLevel";
            this.cbbLogLevel.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            this.cbbLogLevel.Size = new System.Drawing.Size(132, 29);
            this.cbbLogLevel.SymbolSize = 24;
            this.cbbLogLevel.TabIndex = 5;
            this.cbbLogLevel.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.cbbLogLevel.Watermark = "";
            this.cbbLogLevel.SelectedIndexChanged += new System.EventHandler(this.cbbLogLevel_SelectedIndexChanged);
            // 
            // startTime
            // 
            this.startTime.FillColor = System.Drawing.Color.White;
            this.startTime.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.startTime.Location = new System.Drawing.Point(687, 16);
            this.startTime.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.startTime.MaxLength = 19;
            this.startTime.MinimumSize = new System.Drawing.Size(63, 0);
            this.startTime.Name = "startTime";
            this.startTime.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            this.startTime.Size = new System.Drawing.Size(188, 29);
            this.startTime.SymbolDropDown = 61555;
            this.startTime.SymbolNormal = 61555;
            this.startTime.SymbolSize = 24;
            this.startTime.TabIndex = 7;
            this.startTime.Text = "2025-05-06 21:29:11";
            this.startTime.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.startTime.Value = new System.DateTime(2025, 5, 6, 21, 29, 11, 496);
            this.startTime.Watermark = "";
            this.startTime.ValueChanged += new Sunny.UI.UIDatetimePicker.OnDateTimeChanged(this.startTime_ValueChanged);
            this.startTime.Leave += new System.EventHandler(this.startTime_Leave);
            // 
            // endTime
            // 
            this.endTime.FillColor = System.Drawing.Color.White;
            this.endTime.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.endTime.Location = new System.Drawing.Point(1000, 16);
            this.endTime.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.endTime.MaxLength = 19;
            this.endTime.MinimumSize = new System.Drawing.Size(63, 0);
            this.endTime.Name = "endTime";
            this.endTime.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            this.endTime.Size = new System.Drawing.Size(193, 29);
            this.endTime.SymbolDropDown = 61555;
            this.endTime.SymbolNormal = 61555;
            this.endTime.SymbolSize = 24;
            this.endTime.TabIndex = 9;
            this.endTime.Text = "2025-05-06 21:29:11";
            this.endTime.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.endTime.Value = new System.DateTime(2025, 5, 6, 21, 29, 11, 496);
            this.endTime.Watermark = "";
            this.endTime.ValueChanged += new Sunny.UI.UIDatetimePicker.OnDateTimeChanged(this.endTime_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(922, 23);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 16);
            this.label4.TabIndex = 8;
            this.label4.Text = "结束时间";
            // 
            // btnTimeSelect
            // 
            this.btnTimeSelect.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTimeSelect.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnTimeSelect.Location = new System.Drawing.Point(1426, 10);
            this.btnTimeSelect.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnTimeSelect.Name = "btnTimeSelect";
            this.btnTimeSelect.Size = new System.Drawing.Size(100, 35);
            this.btnTimeSelect.TabIndex = 10;
            this.btnTimeSelect.Text = "查询";
            this.btnTimeSelect.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnTimeSelect.Click += new System.EventHandler(this.btnTimeSelect_Click);
            // 
            // cbbLogType
            // 
            this.cbbLogType.DataSource = null;
            this.cbbLogType.FillColor = System.Drawing.Color.White;
            this.cbbLogType.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cbbLogType.ItemHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(200)))), ((int)(((byte)(255)))));
            this.cbbLogType.Items.AddRange(new object[] {
            "全部类型-all",
            "人工操作-operate",
            "设备运行-run",
            "产能信息-spc"});
            this.cbbLogType.ItemSelectForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.cbbLogType.Location = new System.Drawing.Point(392, 16);
            this.cbbLogType.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbbLogType.MinimumSize = new System.Drawing.Size(63, 0);
            this.cbbLogType.Name = "cbbLogType";
            this.cbbLogType.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            this.cbbLogType.Size = new System.Drawing.Size(151, 29);
            this.cbbLogType.SymbolSize = 24;
            this.cbbLogType.TabIndex = 12;
            this.cbbLogType.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.cbbLogType.Watermark = "";
            this.cbbLogType.SelectedIndexChanged += new System.EventHandler(this.cbbLogType_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(302, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 16);
            this.label1.TabIndex = 11;
            this.label1.Text = "日志类型";
            // 
            // dgv_data
            // 
            this.dgv_data.AllowUserToAddRows = false;
            this.dgv_data.AllowUserToDeleteRows = false;
            this.dgv_data.AllowUserToResizeColumns = false;
            this.dgv_data.AllowUserToResizeRows = false;
            dataGridViewCellStyle13.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle13.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle13.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            dataGridViewCellStyle13.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle13.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle13.SelectionForeColor = System.Drawing.Color.White;
            this.dgv_data.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle13;
            this.dgv_data.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgv_data.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            this.dgv_data.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle14.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle14.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle14.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle14.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle14.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle14.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle14.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgv_data.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle14;
            this.dgv_data.ColumnHeadersHeight = 40;
            this.dgv_data.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgv_data.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.LogTime,
            this.LogLevel,
            this.LogType,
            this.LogContent});
            dataGridViewCellStyle16.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle16.BackColor = System.Drawing.Color.LightGray;
            dataGridViewCellStyle16.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle16.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle16.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle16.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle16.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgv_data.DefaultCellStyle = dataGridViewCellStyle16;
            this.dgv_data.EnableHeadersVisualStyles = false;
            this.dgv_data.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(243)))));
            this.dgv_data.Location = new System.Drawing.Point(30, 79);
            this.dgv_data.Name = "dgv_data";
            this.dgv_data.ReadOnly = true;
            this.dgv_data.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle17.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle17.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle17.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle17.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle17.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle17.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle17.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgv_data.RowHeadersDefaultCellStyle = dataGridViewCellStyle17;
            this.dgv_data.RowHeadersWidth = 99;
            this.dgv_data.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle18.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle18.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle18.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            dataGridViewCellStyle18.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle18.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle18.SelectionForeColor = System.Drawing.Color.White;
            this.dgv_data.RowsDefaultCellStyle = dataGridViewCellStyle18;
            this.dgv_data.RowTemplate.Height = 35;
            this.dgv_data.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv_data.Size = new System.Drawing.Size(1570, 728);
            this.dgv_data.TabIndex = 85;
            this.dgv_data.RowPostPaint += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(this.dgv_data_RowPostPaint);
            // 
            // LogTime
            // 
            this.LogTime.DataPropertyName = "Time";
            dataGridViewCellStyle15.Format = "yyyy-MM-dd HH:mm:ss";
            this.LogTime.DefaultCellStyle = dataGridViewCellStyle15;
            this.LogTime.HeaderText = "日志时间";
            this.LogTime.Name = "LogTime";
            this.LogTime.ReadOnly = true;
            this.LogTime.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.LogTime.Width = 380;
            // 
            // LogLevel
            // 
            this.LogLevel.DataPropertyName = "LogLevel";
            this.LogLevel.HeaderText = "日志等级";
            this.LogLevel.Name = "LogLevel";
            this.LogLevel.ReadOnly = true;
            this.LogLevel.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.LogLevel.Width = 290;
            // 
            // LogType
            // 
            this.LogType.DataPropertyName = "LogType";
            this.LogType.HeaderText = "日志类型";
            this.LogType.Name = "LogType";
            this.LogType.ReadOnly = true;
            this.LogType.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.LogType.Width = 290;
            // 
            // LogContent
            // 
            this.LogContent.DataPropertyName = "LogContent";
            this.LogContent.HeaderText = "日志内容";
            this.LogContent.Name = "LogContent";
            this.LogContent.ReadOnly = true;
            this.LogContent.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.LogContent.Width = 530;
            // 
            // btnWriteLog
            // 
            this.btnWriteLog.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnWriteLog.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnWriteLog.Location = new System.Drawing.Point(1692, 237);
            this.btnWriteLog.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnWriteLog.Name = "btnWriteLog";
            this.btnWriteLog.Size = new System.Drawing.Size(110, 63);
            this.btnWriteLog.TabIndex = 86;
            this.btnWriteLog.Text = "写入日志";
            this.btnWriteLog.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnWriteLog.Click += new System.EventHandler(this.btnWriteLog_Click);
            // 
            // lblTotalNum
            // 
            this.lblTotalNum.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblTotalNum.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblTotalNum.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.lblTotalNum.Location = new System.Drawing.Point(1381, 845);
            this.lblTotalNum.Name = "lblTotalNum";
            this.lblTotalNum.Size = new System.Drawing.Size(224, 51);
            this.lblTotalNum.TabIndex = 87;
            this.lblTotalNum.Text = "4";
            this.lblTotalNum.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LogTimer
            // 
            this.LogTimer.Tick += new System.EventHandler(this.LogTimer_Tick);
            // 
            // txtLog
            // 
            this.txtLog.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtLog.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtLog.Location = new System.Drawing.Point(1237, 15);
            this.txtLog.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtLog.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtLog.Name = "txtLog";
            this.txtLog.Padding = new System.Windows.Forms.Padding(5);
            this.txtLog.ShowText = false;
            this.txtLog.Size = new System.Drawing.Size(162, 29);
            this.txtLog.TabIndex = 88;
            this.txtLog.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtLog.Watermark = "";
            // 
            // txtkeyWords
            // 
            this.txtkeyWords.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtkeyWords.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtkeyWords.Location = new System.Drawing.Point(1565, 16);
            this.txtkeyWords.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtkeyWords.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtkeyWords.Name = "txtkeyWords";
            this.txtkeyWords.Padding = new System.Windows.Forms.Padding(5);
            this.txtkeyWords.ShowText = false;
            this.txtkeyWords.Size = new System.Drawing.Size(145, 29);
            this.txtkeyWords.TabIndex = 89;
            this.txtkeyWords.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtkeyWords.Watermark = "";
            // 
            // btnSelectKey
            // 
            this.btnSelectKey.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSelectKey.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnSelectKey.Location = new System.Drawing.Point(1741, 10);
            this.btnSelectKey.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnSelectKey.Name = "btnSelectKey";
            this.btnSelectKey.Size = new System.Drawing.Size(120, 35);
            this.btnSelectKey.TabIndex = 90;
            this.btnSelectKey.Text = "查找下一个";
            this.btnSelectKey.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnSelectKey.Click += new System.EventHandler(this.btnSelectKey_Click);
            // 
            // _log
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.btnSelectKey);
            this.Controls.Add(this.txtkeyWords);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.lblTotalNum);
            this.Controls.Add(this.btnWriteLog);
            this.Controls.Add(this.dgv_data);
            this.Controls.Add(this.cbbLogType);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnTimeSelect);
            this.Controls.Add(this.endTime);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.startTime);
            this.Controls.Add(this.cbbLogLevel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Name = "_log";
            this.Size = new System.Drawing.Size(1891, 990);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_data)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private Sunny.UI.UIComboBox cbbLogLevel;
        private Sunny.UI.UIDatetimePicker startTime;
        private Sunny.UI.UIDatetimePicker endTime;
        private System.Windows.Forms.Label label4;
        private Sunny.UI.UIButton btnTimeSelect;
        private Sunny.UI.UIComboBox cbbLogType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView dgv_data;
        private Sunny.UI.UIButton btnWriteLog;
        private System.Windows.Forms.DataGridViewTextBoxColumn LogTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn LogLevel;
        private System.Windows.Forms.DataGridViewTextBoxColumn LogType;
        private System.Windows.Forms.DataGridViewTextBoxColumn LogContent;
        private Sunny.UI.UILabel lblTotalNum;
        private System.Windows.Forms.Timer LogTimer;
        private Sunny.UI.UITextBox txtLog;
        private Sunny.UI.UITextBox txtkeyWords;
        private Sunny.UI.UIButton btnSelectKey;
    }
}
