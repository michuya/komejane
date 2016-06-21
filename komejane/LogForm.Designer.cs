namespace Komejane
{
  partial class LogForm
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
      this.tboxLogView = new System.Windows.Forms.RichTextBox();
      this.btnClose = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // tboxLogView
      // 
      this.tboxLogView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tboxLogView.Location = new System.Drawing.Point(12, 12);
      this.tboxLogView.Name = "tboxLogView";
      this.tboxLogView.ReadOnly = true;
      this.tboxLogView.Size = new System.Drawing.Size(260, 208);
      this.tboxLogView.TabIndex = 0;
      this.tboxLogView.Text = "";
      // 
      // btnClose
      // 
      this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.btnClose.Location = new System.Drawing.Point(12, 226);
      this.btnClose.Name = "btnClose";
      this.btnClose.Size = new System.Drawing.Size(260, 23);
      this.btnClose.TabIndex = 1;
      this.btnClose.Text = "Close";
      this.btnClose.UseVisualStyleBackColor = true;
      this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
      // 
      // LogForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(284, 261);
      this.Controls.Add(this.btnClose);
      this.Controls.Add(this.tboxLogView);
      this.Name = "LogForm";
      this.Text = "LogForm";
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.RichTextBox tboxLogView;
    private System.Windows.Forms.Button btnClose;
  }
}