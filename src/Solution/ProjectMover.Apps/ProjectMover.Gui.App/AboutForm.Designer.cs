namespace ProjectMover.Gui.App {
  partial class AboutForm {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
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
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent () {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager (typeof (AboutForm));
      button1 = new Button ();
      lblIcon = new Label ();
      tableLayoutPanel1 = new TableLayoutPanel ();
      lblProduct = new Label ();
      lblVersion = new Label ();
      lblCopyright = new Label ();
      lblHomePage = new Label ();
      linkLblHomePage = new LinkLabel ();
      tableLayoutPanel1.SuspendLayout ();
      SuspendLayout ();
      // 
      // button1
      // 
      button1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
      button1.Location = new Point (18, 119);
      button1.Name = "button1";
      button1.Size = new Size (75, 23);
      button1.TabIndex = 0;
      button1.Text = "OK";
      button1.UseVisualStyleBackColor = true;
      // 
      // lblIcon
      // 
      lblIcon.Image = (Image)resources.GetObject ("lblIcon.Image");
      lblIcon.Location = new Point (18, 12);
      lblIcon.Name = "lblIcon";
      lblIcon.Size = new Size (72, 72);
      lblIcon.TabIndex = 1;
      // 
      // tableLayoutPanel1
      // 
      tableLayoutPanel1.ColumnCount = 1;
      tableLayoutPanel1.ColumnStyles.Add (new ColumnStyle (SizeType.Percent, 100F));
      tableLayoutPanel1.Controls.Add (lblProduct, 0, 0);
      tableLayoutPanel1.Controls.Add (lblVersion, 0, 1);
      tableLayoutPanel1.Controls.Add (lblCopyright, 0, 2);
      tableLayoutPanel1.Controls.Add (lblHomePage, 0, 3);
      tableLayoutPanel1.Controls.Add (linkLblHomePage, 0, 4);
      tableLayoutPanel1.Location = new Point (117, 14);
      tableLayoutPanel1.Name = "tableLayoutPanel1";
      tableLayoutPanel1.RowCount = 5;
      tableLayoutPanel1.RowStyles.Add (new RowStyle (SizeType.Percent, 20F));
      tableLayoutPanel1.RowStyles.Add (new RowStyle (SizeType.Percent, 20F));
      tableLayoutPanel1.RowStyles.Add (new RowStyle (SizeType.Percent, 20F));
      tableLayoutPanel1.RowStyles.Add (new RowStyle (SizeType.Percent, 20F));
      tableLayoutPanel1.RowStyles.Add (new RowStyle (SizeType.Percent, 20F));
      tableLayoutPanel1.Size = new Size (316, 142);
      tableLayoutPanel1.TabIndex = 2;
      // 
      // lblProduct
      // 
      lblProduct.AutoSize = true;
      lblProduct.Location = new Point (3, 0);
      lblProduct.Name = "lblProduct";
      lblProduct.Size = new Size (0, 15);
      lblProduct.TabIndex = 0;
      // 
      // lblVersion
      // 
      lblVersion.AutoSize = true;
      lblVersion.Location = new Point (3, 28);
      lblVersion.Name = "lblVersion";
      lblVersion.Size = new Size (48, 15);
      lblVersion.TabIndex = 1;
      lblVersion.Text = "Version ";
      // 
      // lblCopyright
      // 
      lblCopyright.AutoSize = true;
      lblCopyright.Location = new Point (3, 56);
      lblCopyright.Name = "lblCopyright";
      lblCopyright.Size = new Size (0, 15);
      lblCopyright.TabIndex = 1;
      // 
      // lblHomePage
      // 
      lblHomePage.AutoSize = true;
      lblHomePage.Location = new Point (3, 84);
      lblHomePage.Name = "lblHomePage";
      lblHomePage.Size = new Size (110, 15);
      lblHomePage.TabIndex = 1;
      lblHomePage.Text = "Project home page:";
      // 
      // linkLblHomePage
      // 
      linkLblHomePage.AutoSize = true;
      linkLblHomePage.Location = new Point (3, 112);
      linkLblHomePage.Name = "linkLblHomePage";
      linkLblHomePage.Size = new Size (260, 15);
      linkLblHomePage.TabIndex = 2;
      linkLblHomePage.TabStop = true;
      linkLblHomePage.Text = "https://github.com/MemoTrap/CsProjectMover";
      linkLblHomePage.LinkClicked += linkLabelHomepage_LinkClicked;
      // 
      // AboutForm
      // 
      AcceptButton = button1;
      AutoScaleDimensions = new SizeF (7F, 15F);
      AutoScaleMode = AutoScaleMode.Font;
      CancelButton = button1;
      ClientSize = new Size (448, 163);
      Controls.Add (tableLayoutPanel1);
      Controls.Add (lblIcon);
      Controls.Add (button1);
      FormBorderStyle = FormBorderStyle.FixedDialog;
      Icon = (Icon)resources.GetObject ("$this.Icon");
      MaximizeBox = false;
      MinimizeBox = false;
      Name = "AboutForm";
      StartPosition = FormStartPosition.CenterParent;
      Text = "About ";
      tableLayoutPanel1.ResumeLayout (false);
      tableLayoutPanel1.PerformLayout ();
      ResumeLayout (false);
    }

    #endregion

    private Button button1;
    private Label lblIcon;
    private TableLayoutPanel tableLayoutPanel1;
    private Label lblProduct;
    private Label lblVersion;
    private Label lblCopyright;
    private Label lblHomePage;
    private LinkLabel linkLblHomePage;
  }
}