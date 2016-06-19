namespace Komejane
{
    partial class ControlForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
      this.label1 = new System.Windows.Forms.Label();
      this.txtUrlSample = new System.Windows.Forms.TextBox();
      this.btnClose = new System.Windows.Forms.Button();
      this.btnServRun = new System.Windows.Forms.Button();
      this.btnServStop = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(12, 9);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(275, 15);
      this.label1.TabIndex = 0;
      this.label1.Text = "下のURLをOBSのBrowserSourceのURL欄へコピペしてね";
      // 
      // txtUrlSample
      // 
      this.txtUrlSample.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txtUrlSample.Enabled = false;
      this.txtUrlSample.Location = new System.Drawing.Point(12, 27);
      this.txtUrlSample.Name = "txtUrlSample";
      this.txtUrlSample.ReadOnly = true;
      this.txtUrlSample.Size = new System.Drawing.Size(312, 23);
      this.txtUrlSample.TabIndex = 1;
      this.txtUrlSample.Text = "停止中…";
      // 
      // btnClose
      // 
      this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
      this.btnClose.Location = new System.Drawing.Point(138, 89);
      this.btnClose.Name = "btnClose";
      this.btnClose.Size = new System.Drawing.Size(61, 23);
      this.btnClose.TabIndex = 2;
      this.btnClose.Text = "閉じる";
      this.btnClose.UseVisualStyleBackColor = true;
      this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
      // 
      // btnServRun
      // 
      this.btnServRun.Location = new System.Drawing.Point(12, 56);
      this.btnServRun.Name = "btnServRun";
      this.btnServRun.Size = new System.Drawing.Size(88, 23);
      this.btnServRun.TabIndex = 3;
      this.btnServRun.Text = "サーバ起動";
      this.btnServRun.UseVisualStyleBackColor = true;
      this.btnServRun.Click += new System.EventHandler(this.btnServRun_Click);
      // 
      // btnServStop
      // 
      this.btnServStop.Enabled = false;
      this.btnServStop.Location = new System.Drawing.Point(106, 56);
      this.btnServStop.Name = "btnServStop";
      this.btnServStop.Size = new System.Drawing.Size(88, 23);
      this.btnServStop.TabIndex = 4;
      this.btnServStop.Text = "サーバ停止";
      this.btnServStop.UseVisualStyleBackColor = true;
      this.btnServStop.Click += new System.EventHandler(this.btnServStop_Click);
      // 
      // ControlForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(336, 124);
      this.Controls.Add(this.btnServStop);
      this.Controls.Add(this.btnServRun);
      this.Controls.Add(this.btnClose);
      this.Controls.Add(this.txtUrlSample);
      this.Controls.Add(this.label1);
      this.Font = new System.Drawing.Font("Yu Gothic UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ControlForm";
      this.Text = "こめじゃね！";
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtUrlSample;
        private System.Windows.Forms.Button btnClose;
    private System.Windows.Forms.Button btnServRun;
    private System.Windows.Forms.Button btnServStop;
  }
}