namespace H5509_V2
{
    partial class ForMain
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

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ForMain));
            this.button_main = new System.Windows.Forms.Button();
            this.button_setting = new System.Windows.Forms.Button();
            this.button_offset = new System.Windows.Forms.Button();
            this.button_speed = new System.Windows.Forms.Button();
            this.button_io = new System.Windows.Forms.Button();
            this.button_log = new System.Windows.Forms.Button();
            this.button_exit = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.MainPanel = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // button_main
            // 
            this.button_main.BackColor = System.Drawing.Color.White;
            this.button_main.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.button_main.Image = global::H5509_V2.Properties.Resources.主界面;
            this.button_main.Location = new System.Drawing.Point(12, 12);
            this.button_main.Name = "button_main";
            this.button_main.Size = new System.Drawing.Size(150, 60);
            this.button_main.TabIndex = 8;
            this.button_main.Text = "主界面";
            this.button_main.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.button_main.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button_main.UseVisualStyleBackColor = false;
            this.button_main.Click += new System.EventHandler(this.button_main_Click);
            // 
            // button_setting
            // 
            this.button_setting.BackColor = System.Drawing.Color.White;
            this.button_setting.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.button_setting.Image = global::H5509_V2.Properties.Resources.设置与维护图标;
            this.button_setting.Location = new System.Drawing.Point(168, 12);
            this.button_setting.Name = "button_setting";
            this.button_setting.Size = new System.Drawing.Size(150, 60);
            this.button_setting.TabIndex = 9;
            this.button_setting.Tag = "";
            this.button_setting.Text = "设置与维护";
            this.button_setting.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.button_setting.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button_setting.UseVisualStyleBackColor = false;
            this.button_setting.Click += new System.EventHandler(this.button_setting_Click);
            // 
            // button_offset
            // 
            this.button_offset.BackColor = System.Drawing.Color.White;
            this.button_offset.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.button_offset.Image = global::H5509_V2.Properties.Resources.Offset图标;
            this.button_offset.Location = new System.Drawing.Point(324, 12);
            this.button_offset.Name = "button_offset";
            this.button_offset.Size = new System.Drawing.Size(150, 60);
            this.button_offset.TabIndex = 10;
            this.button_offset.Tag = "@02@#BCD#";
            this.button_offset.Text = "Offset";
            this.button_offset.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.button_offset.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button_offset.UseVisualStyleBackColor = false;
            this.button_offset.Click += new System.EventHandler(this.button_offset_Click);
            // 
            // button_speed
            // 
            this.button_speed.BackColor = System.Drawing.Color.White;
            this.button_speed.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.button_speed.Image = global::H5509_V2.Properties.Resources.速度图标;
            this.button_speed.Location = new System.Drawing.Point(480, 12);
            this.button_speed.Name = "button_speed";
            this.button_speed.Size = new System.Drawing.Size(150, 60);
            this.button_speed.TabIndex = 11;
            this.button_speed.Tag = "@02@#BCD#";
            this.button_speed.Text = "速度";
            this.button_speed.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.button_speed.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button_speed.UseVisualStyleBackColor = false;
            this.button_speed.Click += new System.EventHandler(this.button_speed_Click);
            // 
            // button_io
            // 
            this.button_io.BackColor = System.Drawing.Color.White;
            this.button_io.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.button_io.Image = global::H5509_V2.Properties.Resources.IO图标;
            this.button_io.Location = new System.Drawing.Point(636, 12);
            this.button_io.Name = "button_io";
            this.button_io.Size = new System.Drawing.Size(150, 60);
            this.button_io.TabIndex = 12;
            this.button_io.Tag = "@012@#ABCD#";
            this.button_io.Text = "IO";
            this.button_io.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.button_io.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button_io.UseVisualStyleBackColor = false;
            this.button_io.Click += new System.EventHandler(this.button_io_Click);
            // 
            // button_log
            // 
            this.button_log.BackColor = System.Drawing.Color.White;
            this.button_log.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.button_log.Image = global::H5509_V2.Properties.Resources.日志图标;
            this.button_log.Location = new System.Drawing.Point(792, 12);
            this.button_log.Name = "button_log";
            this.button_log.Size = new System.Drawing.Size(150, 60);
            this.button_log.TabIndex = 13;
            this.button_log.Tag = "@012@#ABCD#";
            this.button_log.Text = "日志";
            this.button_log.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.button_log.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button_log.UseVisualStyleBackColor = false;
            this.button_log.Click += new System.EventHandler(this.button_log_Click);
            // 
            // button_exit
            // 
            this.button_exit.BackColor = System.Drawing.Color.White;
            this.button_exit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.button_exit.Image = global::H5509_V2.Properties.Resources.退出图标;
            this.button_exit.Location = new System.Drawing.Point(948, 12);
            this.button_exit.Name = "button_exit";
            this.button_exit.Size = new System.Drawing.Size(150, 60);
            this.button_exit.TabIndex = 14;
            this.button_exit.Tag = "@02@#BCD#";
            this.button_exit.Text = "退出";
            this.button_exit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.button_exit.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button_exit.UseVisualStyleBackColor = false;
            this.button_exit.Click += new System.EventHandler(this.button_exit_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(1326, 8);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(582, 64);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 15;
            this.pictureBox1.TabStop = false;
            // 
            // MainPanel
            // 
            this.MainPanel.Location = new System.Drawing.Point(12, 78);
            this.MainPanel.Name = "MainPanel";
            this.MainPanel.Size = new System.Drawing.Size(1896, 990);
            this.MainPanel.TabIndex = 16;
            // 
            // ForMain
            // 
            this.AllowShowTitle = false;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1920, 1080);
            this.ControlBoxFillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(63)))));
            this.Controls.Add(this.MainPanel);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.button_exit);
            this.Controls.Add(this.button_log);
            this.Controls.Add(this.button_io);
            this.Controls.Add(this.button_speed);
            this.Controls.Add(this.button_offset);
            this.Controls.Add(this.button_setting);
            this.Controls.Add(this.button_main);
            this.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(2560, 1440);
            this.Name = "ForMain";
            this.Padding = new System.Windows.Forms.Padding(0);
            this.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(63)))));
            this.ShowFullScreen = true;
            this.ShowTitle = false;
            this.Text = "Form1";
            this.TitleColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(63)))));
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ZoomScaleRect = new System.Drawing.Rectangle(15, 15, 800, 450);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ForMain_FormClosing);
            this.Load += new System.EventHandler(this.ForMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button button_main;
        private System.Windows.Forms.Button button_setting;
        private System.Windows.Forms.Button button_offset;
        private System.Windows.Forms.Button button_speed;
        private System.Windows.Forms.Button button_io;
        private System.Windows.Forms.Button button_log;
        private System.Windows.Forms.Button button_exit;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel MainPanel;
    }
}

