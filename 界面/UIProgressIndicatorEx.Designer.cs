namespace H5509_V2.界面
{
    partial class UIProgressIndicatorEx
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
            this.lblSelect = new Sunny.UI.UILabel();
            this.uiProgressIndicator1 = new Sunny.UI.UIProgressIndicator();
            this.SuspendLayout();
            // 
            // lblSelect
            // 
            this.lblSelect.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblSelect.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.lblSelect.Location = new System.Drawing.Point(-3, 39);
            this.lblSelect.Name = "lblSelect";
            this.lblSelect.Size = new System.Drawing.Size(62, 21);
            this.lblSelect.TabIndex = 1;
            this.lblSelect.Text = "查询中";
            this.lblSelect.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiProgressIndicator1
            // 
            this.uiProgressIndicator1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiProgressIndicator1.Location = new System.Drawing.Point(0, 0);
            this.uiProgressIndicator1.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiProgressIndicator1.Name = "uiProgressIndicator1";
            this.uiProgressIndicator1.Size = new System.Drawing.Size(40, 39);
            this.uiProgressIndicator1.TabIndex = 0;
            this.uiProgressIndicator1.Text = "uiProgressIndicator1";
            // 
            // UIProgressIndicatorEx
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.uiProgressIndicator1);
            this.Controls.Add(this.lblSelect);
            this.Name = "UIProgressIndicatorEx";
            this.Size = new System.Drawing.Size(51, 60);
            this.ResumeLayout(false);

        }

        #endregion
        private Sunny.UI.UILabel lblSelect;
        private Sunny.UI.UIProgressIndicator uiProgressIndicator1;
    }
}
