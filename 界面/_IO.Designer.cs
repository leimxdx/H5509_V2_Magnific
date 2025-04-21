namespace H5509_V2
{
    partial class _IO
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
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.uiTabControl2 = new Sunny.UI.UITabControl();
            this.uiTabControl1 = new Sunny.UI.UITabControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.uiToolTip1 = new Sunny.UI.UIToolTip(this.components);
            this.IOUpdateTimer = new System.Windows.Forms.Timer(this.components);
            this.uiButton1 = new Sunny.UI.UIButton();
            this.tabPage1.SuspendLayout();
            this.uiTabControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            this.tabPage1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tabPage1.Controls.Add(this.uiTabControl2);
            this.tabPage1.Location = new System.Drawing.Point(0, 40);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Size = new System.Drawing.Size(1893, 944);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "IO调试";
            // 
            // uiTabControl2
            // 
            this.uiTabControl2.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.uiTabControl2.Font = new System.Drawing.Font("微软雅黑", 15F);
            this.uiTabControl2.ItemSize = new System.Drawing.Size(150, 40);
            this.uiTabControl2.Location = new System.Drawing.Point(3, 3);
            this.uiTabControl2.MainPage = "";
            this.uiTabControl2.MenuStyle = Sunny.UI.UIMenuStyle.Custom;
            this.uiTabControl2.Name = "uiTabControl2";
            this.uiTabControl2.SelectedIndex = 0;
            this.uiTabControl2.Size = new System.Drawing.Size(1885, 936);
            this.uiTabControl2.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.uiTabControl2.Style = Sunny.UI.UIStyle.Custom;
            this.uiTabControl2.TabBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            this.uiTabControl2.TabIndex = 1;
            this.uiTabControl2.TabSelectedColor = System.Drawing.SystemColors.Menu;
            this.uiTabControl2.TabSelectedForeColor = System.Drawing.Color.Black;
            this.uiTabControl2.TabUnSelectedColor = System.Drawing.SystemColors.Menu;
            this.uiTabControl2.TabUnSelectedForeColor = System.Drawing.Color.Black;
            this.uiTabControl2.TipsFont = new System.Drawing.Font("微软雅黑", 15F);
            this.uiTabControl2.TipsForeColor = System.Drawing.Color.Black;
            this.uiTabControl2.SelectedIndexChanged += new System.EventHandler(this.uiTabControl2_SelectedIndexChanged);
            // 
            // uiTabControl1
            // 
            this.uiTabControl1.Controls.Add(this.tabPage1);
            this.uiTabControl1.Controls.Add(this.tabPage2);
            this.uiTabControl1.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.uiTabControl1.Font = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiTabControl1.ItemSize = new System.Drawing.Size(150, 40);
            this.uiTabControl1.Location = new System.Drawing.Point(2, 3);
            this.uiTabControl1.MainPage = "";
            this.uiTabControl1.MenuStyle = Sunny.UI.UIMenuStyle.Custom;
            this.uiTabControl1.Name = "uiTabControl1";
            this.uiTabControl1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.uiTabControl1.SelectedIndex = 0;
            this.uiTabControl1.Size = new System.Drawing.Size(1893, 984);
            this.uiTabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.uiTabControl1.Style = Sunny.UI.UIStyle.Custom;
            this.uiTabControl1.TabBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            this.uiTabControl1.TabIndex = 3;
            this.uiTabControl1.TabSelectedColor = System.Drawing.SystemColors.Menu;
            this.uiTabControl1.TabSelectedForeColor = System.Drawing.Color.Black;
            this.uiTabControl1.TabUnSelectedColor = System.Drawing.SystemColors.Menu;
            this.uiTabControl1.TabUnSelectedForeColor = System.Drawing.Color.Black;
            this.uiTabControl1.TipsFont = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiTabControl1.TipsForeColor = System.Drawing.Color.Black;
            // 
            // tabPage2
            // 
            this.tabPage2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tabPage2.Location = new System.Drawing.Point(0, 40);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(200, 60);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "IO界面";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // uiToolTip1
            // 
            this.uiToolTip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            this.uiToolTip1.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiToolTip1.ForeColor = System.Drawing.Color.Black;
            this.uiToolTip1.OwnerDraw = true;
            this.uiToolTip1.RectColor = System.Drawing.Color.Black;
            this.uiToolTip1.TitleFont = new System.Drawing.Font("微软雅黑", 14.25F);
            // 
            // IOUpdateTimer
            // 
            this.IOUpdateTimer.Interval = 500;
            this.IOUpdateTimer.Tick += new System.EventHandler(this.IOUpdateTimer_Tick);
            // 
            // uiButton1
            // 
            this.uiButton1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.uiButton1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiButton1.Location = new System.Drawing.Point(674, 6);
            this.uiButton1.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiButton1.Name = "uiButton1";
            this.uiButton1.Size = new System.Drawing.Size(100, 35);
            this.uiButton1.TabIndex = 4;
            this.uiButton1.Text = "uiButton1";
            this.uiButton1.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiButton1.Click += new System.EventHandler(this.uiButton1_Click);
            // 
            // _IO
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.uiButton1);
            this.Controls.Add(this.uiTabControl1);
            this.Name = "_IO";
            this.RectColor = System.Drawing.Color.Transparent;
            this.Size = new System.Drawing.Size(1896, 990);
            this.tabPage1.ResumeLayout(false);
            this.uiTabControl1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabPage tabPage1;
        private Sunny.UI.UITabControl uiTabControl1;
        private System.Windows.Forms.TabPage tabPage2;
        private Sunny.UI.UIToolTip uiToolTip1;
        private Sunny.UI.UITabControl uiTabControl2;
        public System.Windows.Forms.Timer IOUpdateTimer;
        private Sunny.UI.UIButton uiButton1;
    }
}
