namespace ProjectMover.Gui.App {
  partial class PresetForm {
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
      components = new System.ComponentModel.Container ();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager (typeof (PresetForm));
      panelBottom = new Panel ();
      btnCancel = new Button ();
      btnLater = new Button ();
      btnOK = new Button ();
      pathCheckList = new PathCheckListUC ();
      toolTip = new ToolTip (components);
      panelBottom.SuspendLayout ();
      SuspendLayout ();
      // 
      // panelBottom
      // 
      panelBottom.Controls.Add (btnCancel);
      panelBottom.Controls.Add (btnLater);
      panelBottom.Controls.Add (btnOK);
      panelBottom.Dock = DockStyle.Bottom;
      panelBottom.Location = new Point (0, 264);
      panelBottom.Name = "panelBottom";
      panelBottom.Size = new Size (484, 67);
      panelBottom.TabIndex = 1;
      // 
      // btnCancel
      // 
      btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      btnCancel.Location = new Point (397, 16);
      btnCancel.Name = "btnCancel";
      btnCancel.Size = new Size (75, 39);
      btnCancel.TabIndex = 2;
      btnCancel.Text = "Cancel";
      toolTip.SetToolTip (btnCancel, "Cancel the current run.");
      btnCancel.UseVisualStyleBackColor = true;
      btnCancel.Click += btnCancel_Click;
      // 
      // btnLater
      // 
      btnLater.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      btnLater.Location = new Point (97, 16);
      btnLater.Name = "btnLater";
      btnLater.Size = new Size (75, 39);
      btnLater.TabIndex = 1;
      btnLater.Text = "Later";
      toolTip.SetToolTip (btnLater, "No preselection. \r\nDecide on each candidate project in the next step.");
      btnLater.UseVisualStyleBackColor = true;
      btnLater.Click += btnLater_Click;
      // 
      // btnOK
      // 
      btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      btnOK.Location = new Point (16, 16);
      btnOK.Name = "btnOK";
      btnOK.Size = new Size (75, 39);
      btnOK.TabIndex = 0;
      btnOK.Text = "OK";
      toolTip.SetToolTip (btnOK, "Continue with the preselected projects \r\nand only decide on them.\r\nDisregard any project not selected here.");
      btnOK.UseVisualStyleBackColor = true;
      btnOK.Click += btnOK_Click;
      // 
      // pathCheckList
      // 
      pathCheckList.ColumnHeader = "Path";
      pathCheckList.Dock = DockStyle.Fill;
      pathCheckList.Location = new Point (0, 0);
      pathCheckList.Name = "pathCheckList";
      pathCheckList.Size = new Size (484, 264);
      pathCheckList.TabIndex = 0;
      // 
      // PresetForm
      // 
      AcceptButton = btnOK;
      AutoScaleDimensions = new SizeF (7F, 15F);
      AutoScaleMode = AutoScaleMode.Font;
      CancelButton = btnLater;
      ClientSize = new Size (484, 331);
      ControlBox = false;
      Controls.Add (pathCheckList);
      Controls.Add (panelBottom);
      Icon = (Icon)resources.GetObject ("$this.Icon");
      MinimumSize = new Size (300, 200);
      Name = "PresetForm";
      Text = "Project Preselection";
      panelBottom.ResumeLayout (false);
      ResumeLayout (false);
    }

    #endregion
    private Panel panelBottom;
    private Button btnLater;
    private Button btnOK;
    private Button btnCancel;
    private Lib.PathCheckListUC pathCheckList;
    private ToolTip toolTip;
  }
}