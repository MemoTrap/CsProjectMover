namespace ProjectMover.Gui.App {
  partial class DecisionForm {
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager (typeof (DecisionForm));
      panelBottom = new Panel ();
      btnReset = new Button ();
      btnCancel = new Button ();
      btnSkip = new Button ();
      btnOK = new Button ();
      panelMain = new Panel ();
      splitContainer = new SplitContainer ();
      pathCheckListProj = new PathCheckListUC ();
      pathCheckListSln = new PathCheckListUC ();
      panelText = new Panel ();
      btnFolder = new Button ();
      txtBxProjName = new TextBox ();
      roTxtBxCurAss = new ReadOnlyTextBox ();
      txtBxNewProjFolder = new TextBox ();
      btnAssDn = new Button ();
      btnAssUp = new Button ();
      labelNewAss = new Label ();
      labelCurAss = new Label ();
      labelProjName = new Label ();
      txtBxNewAss = new TextBox ();
      roTxtBoxCurProjFolder = new ReadOnlyTextBox ();
      labelNewProjFolder = new Label ();
      btnFolderUp = new Button ();
      labelCurProjFolder = new Label ();
      btnFolderDn = new Button ();
      panelRetry = new Panel ();
      labelReason = new Label ();
      labelRetry = new Label ();
      toolTip = new ToolTip (components);
      panelBottom.SuspendLayout ();
      panelMain.SuspendLayout ();
      ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit ();
      splitContainer.Panel1.SuspendLayout ();
      splitContainer.Panel2.SuspendLayout ();
      splitContainer.SuspendLayout ();
      panelText.SuspendLayout ();
      panelRetry.SuspendLayout ();
      SuspendLayout ();
      // 
      // panelBottom
      // 
      panelBottom.Controls.Add (btnReset);
      panelBottom.Controls.Add (btnCancel);
      panelBottom.Controls.Add (btnSkip);
      panelBottom.Controls.Add (btnOK);
      panelBottom.Dock = DockStyle.Bottom;
      panelBottom.Location = new Point (0, 545);
      panelBottom.Name = "panelBottom";
      panelBottom.Size = new Size (541, 106);
      panelBottom.TabIndex = 3;
      // 
      // btnReset
      // 
      btnReset.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      btnReset.Location = new Point (454, 14);
      btnReset.Name = "btnReset";
      btnReset.Size = new Size (75, 23);
      btnReset.TabIndex = 2;
      btnReset.Text = "Reset";
      toolTip.SetToolTip (btnReset, "Reset to suggested values");
      btnReset.UseVisualStyleBackColor = true;
      btnReset.Click += btnReset_Click;
      // 
      // btnCancel
      // 
      btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      btnCancel.Location = new Point (454, 54);
      btnCancel.Name = "btnCancel";
      btnCancel.Size = new Size (75, 39);
      btnCancel.TabIndex = 3;
      btnCancel.Text = "Cancel";
      toolTip.SetToolTip (btnCancel, "Cancel the current run");
      btnCancel.UseVisualStyleBackColor = true;
      btnCancel.Click += btnCancel_Click;
      // 
      // btnSkip
      // 
      btnSkip.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      btnSkip.Location = new Point (97, 54);
      btnSkip.Name = "btnSkip";
      btnSkip.Size = new Size (75, 39);
      btnSkip.TabIndex = 1;
      btnSkip.Text = "Skip";
      toolTip.SetToolTip (btnSkip, "Skip this project");
      btnSkip.UseVisualStyleBackColor = true;
      btnSkip.Click += btnSkip_Click;
      // 
      // btnOK
      // 
      btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      btnOK.Location = new Point (16, 54);
      btnOK.Name = "btnOK";
      btnOK.Size = new Size (75, 39);
      btnOK.TabIndex = 0;
      btnOK.Text = "OK";
      toolTip.SetToolTip (btnOK, "Proceed with the given settings for this project. \r\nThere will still be a final confirmation question.");
      btnOK.UseVisualStyleBackColor = true;
      btnOK.Click += btnOK_Click;
      // 
      // panelMain
      // 
      panelMain.Controls.Add (splitContainer);
      panelMain.Controls.Add (panelText);
      panelMain.Controls.Add (panelRetry);
      panelMain.Dock = DockStyle.Fill;
      panelMain.Location = new Point (0, 0);
      panelMain.Name = "panelMain";
      panelMain.Size = new Size (541, 545);
      panelMain.TabIndex = 3;
      // 
      // splitContainer
      // 
      splitContainer.Dock = DockStyle.Fill;
      splitContainer.Location = new Point (0, 282);
      splitContainer.Name = "splitContainer";
      splitContainer.Orientation = Orientation.Horizontal;
      // 
      // splitContainer.Panel1
      // 
      splitContainer.Panel1.Controls.Add (pathCheckListProj);
      // 
      // splitContainer.Panel2
      // 
      splitContainer.Panel2.Controls.Add (pathCheckListSln);
      splitContainer.Size = new Size (541, 263);
      splitContainer.SplitterDistance = 131;
      splitContainer.TabIndex = 2;
      // 
      // pathCheckListProj
      // 
      pathCheckListProj.ColumnHeader = "Path";
      pathCheckListProj.Dock = DockStyle.Fill;
      pathCheckListProj.Location = new Point (0, 0);
      pathCheckListProj.Name = "pathCheckListProj";
      pathCheckListProj.Size = new Size (541, 131);
      pathCheckListProj.TabIndex = 0;
      // 
      // pathCheckListSln
      // 
      pathCheckListSln.ColumnHeader = "Path";
      pathCheckListSln.Dock = DockStyle.Fill;
      pathCheckListSln.Location = new Point (0, 0);
      pathCheckListSln.Name = "pathCheckListSln";
      pathCheckListSln.Size = new Size (541, 128);
      pathCheckListSln.TabIndex = 0;
      // 
      // panelText
      // 
      panelText.Controls.Add (btnFolder);
      panelText.Controls.Add (txtBxProjName);
      panelText.Controls.Add (roTxtBxCurAss);
      panelText.Controls.Add (txtBxNewProjFolder);
      panelText.Controls.Add (btnAssDn);
      panelText.Controls.Add (btnAssUp);
      panelText.Controls.Add (labelNewAss);
      panelText.Controls.Add (labelCurAss);
      panelText.Controls.Add (labelProjName);
      panelText.Controls.Add (txtBxNewAss);
      panelText.Controls.Add (roTxtBoxCurProjFolder);
      panelText.Controls.Add (labelNewProjFolder);
      panelText.Controls.Add (btnFolderUp);
      panelText.Controls.Add (labelCurProjFolder);
      panelText.Controls.Add (btnFolderDn);
      panelText.Dock = DockStyle.Top;
      panelText.Location = new Point (0, 35);
      panelText.Name = "panelText";
      panelText.Size = new Size (541, 247);
      panelText.TabIndex = 1;
      // 
      // btnFolder
      // 
      btnFolder.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      btnFolder.Location = new Point (488, 44);
      btnFolder.Name = "btnFolder";
      btnFolder.Size = new Size (41, 23);
      btnFolder.TabIndex = 2;
      btnFolder.Text = "...";
      toolTip.SetToolTip (btnFolder, "Select the new project folder");
      btnFolder.UseVisualStyleBackColor = true;
      btnFolder.Click += btnFolder_Click;
      // 
      // txtBxProjName
      // 
      txtBxProjName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      txtBxProjName.Location = new Point (155, 102);
      txtBxProjName.Name = "txtBxProjName";
      txtBxProjName.Size = new Size (247, 23);
      txtBxProjName.TabIndex = 6;
      toolTip.SetToolTip (txtBxProjName, "Project name, in the project folder, without file extension.");
      txtBxProjName.TextChanged += txtBxProjName_TextChanged;
      // 
      // roTxtBxCurAss
      // 
      roTxtBxCurAss.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      roTxtBxCurAss.BackColor = SystemColors.Control;
      roTxtBxCurAss.BorderStyle = BorderStyle.FixedSingle;
      roTxtBxCurAss.ForeColor = SystemColors.WindowText;
      roTxtBxCurAss.Location = new Point (155, 163);
      roTxtBxCurAss.Name = "roTxtBxCurAss";
      roTxtBxCurAss.ReadOnly = true;
      roTxtBxCurAss.Size = new Size (247, 23);
      roTxtBxCurAss.TabIndex = 9;
      // 
      // txtBxNewProjFolder
      // 
      txtBxNewProjFolder.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      txtBxNewProjFolder.Location = new Point (155, 45);
      txtBxNewProjFolder.Name = "txtBxNewProjFolder";
      txtBxNewProjFolder.Size = new Size (327, 23);
      txtBxNewProjFolder.TabIndex = 1;
      toolTip.SetToolTip (txtBxNewProjFolder, "Project folder, without the project file.");
      txtBxNewProjFolder.TextChanged += txtBxNewProjFolder_TextChanged;
      // 
      // btnAssDn
      // 
      btnAssDn.Location = new Point (155, 134);
      btnAssDn.Name = "btnAssDn";
      btnAssDn.Size = new Size (120, 23);
      btnAssDn.TabIndex = 7;
      btnAssDn.Text = "Sync assembly ▼";
      toolTip.SetToolTip (btnAssDn, "Synchronize assembly name with project name");
      btnAssDn.UseVisualStyleBackColor = true;
      btnAssDn.Click += btnAssDn_Click;
      // 
      // btnAssUp
      // 
      btnAssUp.Location = new Point (282, 134);
      btnAssUp.Name = "btnAssUp";
      btnAssUp.Size = new Size (120, 23);
      btnAssUp.TabIndex = 8;
      btnAssUp.Text = "Sync proj name ▲";
      toolTip.SetToolTip (btnAssUp, "Synchronize project name with assembly name");
      btnAssUp.UseVisualStyleBackColor = true;
      btnAssUp.Click += btnAssUp_Click;
      // 
      // labelNewAss
      // 
      labelNewAss.AutoSize = true;
      labelNewAss.Location = new Point (12, 197);
      labelNewAss.Name = "labelNewAss";
      labelNewAss.Size = new Size (137, 30);
      labelNewAss.TabIndex = 9;
      labelNewAss.Text = "New assembly name\r\n(leave empty for default)";
      // 
      // labelCurAss
      // 
      labelCurAss.AutoSize = true;
      labelCurAss.Location = new Point (17, 165);
      labelCurAss.Name = "labelCurAss";
      labelCurAss.Size = new Size (132, 15);
      labelCurAss.TabIndex = 9;
      labelCurAss.Text = "Current assembly name";
      // 
      // labelProjName
      // 
      labelProjName.AutoSize = true;
      labelProjName.Location = new Point (72, 105);
      labelProjName.Name = "labelProjName";
      labelProjName.Size = new Size (77, 15);
      labelProjName.TabIndex = 9;
      labelProjName.Text = "Project name";
      // 
      // txtBxNewAss
      // 
      txtBxNewAss.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      txtBxNewAss.Location = new Point (155, 194);
      txtBxNewAss.Name = "txtBxNewAss";
      txtBxNewAss.Size = new Size (247, 23);
      txtBxNewAss.TabIndex = 10;
      toolTip.SetToolTip (txtBxNewAss, "Assembly name");
      txtBxNewAss.TextChanged += txtBxNewAss_TextChanged;
      // 
      // roTxtBoxCurProjFolder
      // 
      roTxtBoxCurProjFolder.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      roTxtBoxCurProjFolder.BackColor = SystemColors.Control;
      roTxtBoxCurProjFolder.BorderStyle = BorderStyle.FixedSingle;
      roTxtBoxCurProjFolder.ForeColor = SystemColors.WindowText;
      roTxtBoxCurProjFolder.Location = new Point (155, 12);
      roTxtBoxCurProjFolder.Name = "roTxtBoxCurProjFolder";
      roTxtBoxCurProjFolder.ReadOnly = true;
      roTxtBoxCurProjFolder.Size = new Size (327, 23);
      roTxtBoxCurProjFolder.TabIndex = 0;
      // 
      // labelNewProjFolder
      // 
      labelNewProjFolder.AutoSize = true;
      labelNewProjFolder.Location = new Point (44, 48);
      labelNewProjFolder.Name = "labelNewProjFolder";
      labelNewProjFolder.Size = new Size (105, 15);
      labelNewProjFolder.TabIndex = 9;
      labelNewProjFolder.Text = "New project folder";
      // 
      // btnFolderUp
      // 
      btnFolderUp.Location = new Point (155, 74);
      btnFolderUp.Name = "btnFolderUp";
      btnFolderUp.Size = new Size (120, 23);
      btnFolderUp.TabIndex = 4;
      btnFolderUp.Text = "Sync proj folder ▲";
      toolTip.SetToolTip (btnFolderUp, "Synchronize project folder with project name");
      btnFolderUp.UseVisualStyleBackColor = true;
      btnFolderUp.Click += btnFolderUp_Click;
      // 
      // labelCurProjFolder
      // 
      labelCurProjFolder.AutoSize = true;
      labelCurProjFolder.Location = new Point (28, 14);
      labelCurProjFolder.Name = "labelCurProjFolder";
      labelCurProjFolder.Size = new Size (121, 15);
      labelCurProjFolder.TabIndex = 9;
      labelCurProjFolder.Text = "Current project folder";
      // 
      // btnFolderDn
      // 
      btnFolderDn.Location = new Point (282, 73);
      btnFolderDn.Name = "btnFolderDn";
      btnFolderDn.Size = new Size (120, 23);
      btnFolderDn.TabIndex = 5;
      btnFolderDn.Text = "Sync proj name ▼";
      toolTip.SetToolTip (btnFolderDn, "Synchronize project name with project folder");
      btnFolderDn.UseVisualStyleBackColor = true;
      btnFolderDn.Click += btnFolderDn_Click;
      // 
      // panelRetry
      // 
      panelRetry.Controls.Add (labelReason);
      panelRetry.Controls.Add (labelRetry);
      panelRetry.Dock = DockStyle.Top;
      panelRetry.Location = new Point (0, 0);
      panelRetry.Name = "panelRetry";
      panelRetry.Size = new Size (541, 35);
      panelRetry.TabIndex = 0;
      panelRetry.Visible = false;
      // 
      // labelReason
      // 
      labelReason.ForeColor = Color.DarkRed;
      labelReason.Location = new Point (155, 0);
      labelReason.Name = "labelReason";
      labelReason.Size = new Size (374, 35);
      labelReason.TabIndex = 1;
      labelReason.Text = "Reason";
      labelReason.TextAlign = ContentAlignment.MiddleLeft;
      // 
      // labelRetry
      // 
      labelRetry.AutoSize = true;
      labelRetry.Location = new Point (66, 9);
      labelRetry.Name = "labelRetry";
      labelRetry.Size = new Size (83, 15);
      labelRetry.TabIndex = 0;
      labelRetry.Text = "Retry because:";
      // 
      // DecisionForm
      // 
      AcceptButton = btnOK;
      AutoScaleDimensions = new SizeF (7F, 15F);
      AutoScaleMode = AutoScaleMode.Font;
      CancelButton = btnSkip;
      ClientSize = new Size (541, 651);
      ControlBox = false;
      Controls.Add (panelMain);
      Controls.Add (panelBottom);
      Icon = (Icon)resources.GetObject ("$this.Icon");
      MinimumSize = new Size (557, 667);
      Name = "DecisionForm";
      Text = "project details";
      panelBottom.ResumeLayout (false);
      panelMain.ResumeLayout (false);
      splitContainer.Panel1.ResumeLayout (false);
      splitContainer.Panel2.ResumeLayout (false);
      ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit ();
      splitContainer.ResumeLayout (false);
      panelText.ResumeLayout (false);
      panelText.PerformLayout ();
      panelRetry.ResumeLayout (false);
      panelRetry.PerformLayout ();
      ResumeLayout (false);
    }

    #endregion

    private Panel panelBottom;
    private Button btnCancel;
    private Button btnSkip;
    private Button btnOK;
    private Panel panelMain;
    private Panel panelRetry;
    private Label labelReason;
    private Label labelRetry;
    private Button btnReset;
    private Lib.ReadOnlyTextBox roTxtBoxCurProjFolder;
    private TextBox txtBxNewAss;
    private TextBox txtBxNewProjFolder;
    private TextBox txtBxProjName;
    private Button btnAssDn;
    private Button btnAssUp;
    private Button btnFolderDn;
    private Button btnFolderUp;
    private Label labelNewProjFolder;
    private Label labelCurProjFolder;
    private Panel panelText;
    private Lib.ReadOnlyTextBox roTxtBxCurAss;
    private Label labelProjName;
    private SplitContainer splitContainer;
    private Label labelNewAss;
    private Label labelCurAss;
    private Lib.PathCheckListUC pathCheckListProj;
    private Lib.PathCheckListUC pathCheckListSln;
    private ToolTip toolTip;
    private Button btnFolder;
  }
}