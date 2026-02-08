namespace ClientGuiApp {
  partial class Form1 {
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose (bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose ();
      }
      base.Dispose (disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent () {
      rpcClientUserControl11 = new RpcClientWinFormsControls.RpcClientUserControl1 ();
      button1 = new Button ();
      SuspendLayout ();
      // 
      // rpcClientUserControl11
      // 
      rpcClientUserControl11.BackColor = Color.Silver;
      rpcClientUserControl11.Location = new Point (18, 12);
      rpcClientUserControl11.Name = "rpcClientUserControl11";
      rpcClientUserControl11.Size = new Size (161, 89);
      rpcClientUserControl11.TabIndex = 0;
      // 
      // button1
      // 
      button1.Location = new Point (59, 134);
      button1.Name = "button1";
      button1.Size = new Size (78, 33);
      button1.TabIndex = 1;
      button1.Text = "Run";
      button1.UseVisualStyleBackColor = true;
      button1.Click += button1_Click;
      // 
      // Form1
      // 
      AutoScaleDimensions = new SizeF (7F, 15F);
      AutoScaleMode = AutoScaleMode.Font;
      ClientSize = new Size (196, 199);
      Controls.Add (button1);
      Controls.Add (rpcClientUserControl11);
      FormBorderStyle = FormBorderStyle.FixedSingle;
      Name = "Form1";
      Text = "Form1";
      ResumeLayout (false);
    }

    #endregion

    private RpcClientWinFormsControls.RpcClientUserControl1 rpcClientUserControl11;
    private Button button1;
  }
}
