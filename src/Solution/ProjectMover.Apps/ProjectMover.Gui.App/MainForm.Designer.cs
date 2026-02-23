namespace ProjectMover.Gui.App
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent () {
      components = new System.ComponentModel.Container ();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager (typeof (MainForm));
      txtBoxRoot = new ReadOnlyTextBox ();
      rbSglProjSglSol = new RadioButton ();
      rbSglProjMulSol = new RadioButton ();
      rbMulProjMulSol = new RadioButton ();
      panelRoot = new Panel ();
      labelRoot = new Label ();
      btnRoot = new Button ();
      panelTop = new Panel ();
      panelMulti = new Panel ();
      panelMode = new Panel ();
      rbCopy = new RadioButton ();
      rbMove = new RadioButton ();
      ckBoxSVN = new CheckBox ();
      panelProject = new Panel ();
      updnDepth = new NumericUpDown ();
      labelDepth = new Label ();
      labelProj = new Label ();
      btnProj = new Button ();
      txtBoxProj = new ReadOnlyTextBox ();
      panelDest = new Panel ();
      labelDest = new Label ();
      btnDest = new Button ();
      txtBoxDest = new ReadOnlyTextBox ();
      panelSolution = new Panel ();
      ckBoxRescan = new CheckBox ();
      labelRescan = new Label ();
      labelSln = new Label ();
      btnSln = new Button ();
      txtBoxSln = new ReadOnlyTextBox ();
      panelRun = new Panel ();
      btnReset = new Button ();
      btnAbort = new Button ();
      btnExit = new Button ();
      btnRun = new Button ();
      panelProgress = new Panel ();
      progressBar = new ProgressBar ();
      labelProg1 = new Label ();
      labelProg2 = new Label ();
      panelParam = new Panel ();
      panelAbsRel = new Panel ();
      rbAbs = new RadioButton ();
      rbRel = new RadioButton ();
      toolTip = new ToolTip (components);
      panelRoot.SuspendLayout ();
      panelTop.SuspendLayout ();
      panelMulti.SuspendLayout ();
      panelMode.SuspendLayout ();
      panelProject.SuspendLayout ();
      ((System.ComponentModel.ISupportInitialize)updnDepth).BeginInit ();
      panelDest.SuspendLayout ();
      panelSolution.SuspendLayout ();
      panelRun.SuspendLayout ();
      panelProgress.SuspendLayout ();
      panelParam.SuspendLayout ();
      panelAbsRel.SuspendLayout ();
      SuspendLayout ();
      // 
      // txtBoxRoot
      // 
      txtBoxRoot.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      txtBoxRoot.BackColor = SystemColors.Control;
      txtBoxRoot.BorderStyle = BorderStyle.FixedSingle;
      txtBoxRoot.ForeColor = SystemColors.WindowText;
      txtBoxRoot.Location = new Point (25, 34);
      txtBoxRoot.Name = "txtBoxRoot";
      txtBoxRoot.ReadOnly = true;
      txtBoxRoot.Size = new Size (397, 23);
      txtBoxRoot.TabIndex = 2;
      toolTip.SetToolTip (txtBoxRoot, "The root folder for all projects, solutions and new project destinations.");
      // 
      // rbSglProjSglSol
      // 
      rbSglProjSglSol.AutoSize = true;
      rbSglProjSglSol.Location = new Point (25, 15);
      rbSglProjSglSol.Name = "rbSglProjSglSol";
      rbSglProjSglSol.Size = new Size (185, 19);
      rbSglProjSglSol.TabIndex = 0;
      rbSglProjSglSol.Text = "Single project / single solution";
      toolTip.SetToolTip (rbSglProjSglSol, "Select a single project for move/rename or copy and a single dependent solution.\r\nNOTE: In move/rename mode this may break other dependent solutions and their projects.");
      rbSglProjSglSol.UseVisualStyleBackColor = true;
      rbSglProjSglSol.CheckedChanged += rbProjSol_CheckedChanged;
      // 
      // rbSglProjMulSol
      // 
      rbSglProjMulSol.AutoSize = true;
      rbSglProjMulSol.Location = new Point (25, 42);
      rbSglProjMulSol.Name = "rbSglProjMulSol";
      rbSglProjMulSol.Size = new Size (203, 19);
      rbSglProjMulSol.TabIndex = 1;
      rbSglProjMulSol.Text = "Single project / multiple solutions";
      rbSglProjMulSol.UseVisualStyleBackColor = true;
      rbSglProjMulSol.Click += rbProjSol_CheckedChanged;
      // 
      // rbMulProjMulSol
      // 
      rbMulProjMulSol.AutoSize = true;
      rbMulProjMulSol.Location = new Point (25, 69);
      rbMulProjMulSol.Name = "rbMulProjMulSol";
      rbMulProjMulSol.Size = new Size (215, 19);
      rbMulProjMulSol.TabIndex = 2;
      rbMulProjMulSol.Text = "multiple projects / multiple solution";
      toolTip.SetToolTip (rbMulProjMulSol, "Select multiple projects for move/rename or copy and multiple dependent solutions.");
      rbMulProjMulSol.UseVisualStyleBackColor = true;
      rbMulProjMulSol.Click += rbProjSol_CheckedChanged;
      // 
      // panelRoot
      // 
      panelRoot.Controls.Add (labelRoot);
      panelRoot.Controls.Add (btnRoot);
      panelRoot.Controls.Add (txtBoxRoot);
      panelRoot.Dock = DockStyle.Top;
      panelRoot.Location = new Point (0, 104);
      panelRoot.Name = "panelRoot";
      panelRoot.Size = new Size (496, 66);
      panelRoot.TabIndex = 0;
      // 
      // labelRoot
      // 
      labelRoot.AutoSize = true;
      labelRoot.Location = new Point (28, 10);
      labelRoot.Name = "labelRoot";
      labelRoot.Size = new Size (66, 15);
      labelRoot.TabIndex = 2;
      labelRoot.Text = "Root folder";
      // 
      // btnRoot
      // 
      btnRoot.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      btnRoot.Location = new Point (434, 34);
      btnRoot.Name = "btnRoot";
      btnRoot.Size = new Size (41, 23);
      btnRoot.TabIndex = 0;
      btnRoot.Text = "...";
      toolTip.SetToolTip (btnRoot, "Select the root folder");
      btnRoot.UseVisualStyleBackColor = true;
      btnRoot.Click += btnOpenFile_Click;
      // 
      // panelTop
      // 
      panelTop.Controls.Add (panelMulti);
      panelTop.Controls.Add (panelMode);
      panelTop.Dock = DockStyle.Top;
      panelTop.Location = new Point (0, 0);
      panelTop.Name = "panelTop";
      panelTop.Size = new Size (496, 104);
      panelTop.TabIndex = 0;
      // 
      // panelMulti
      // 
      panelMulti.Controls.Add (rbMulProjMulSol);
      panelMulti.Controls.Add (rbSglProjMulSol);
      panelMulti.Controls.Add (rbSglProjSglSol);
      panelMulti.Dock = DockStyle.Left;
      panelMulti.Location = new Point (0, 0);
      panelMulti.Name = "panelMulti";
      panelMulti.Size = new Size (287, 104);
      panelMulti.TabIndex = 0;
      // 
      // panelMode
      // 
      panelMode.Controls.Add (rbCopy);
      panelMode.Controls.Add (rbMove);
      panelMode.Controls.Add (ckBoxSVN);
      panelMode.Dock = DockStyle.Right;
      panelMode.Location = new Point (282, 0);
      panelMode.Name = "panelMode";
      panelMode.Size = new Size (214, 104);
      panelMode.TabIndex = 1;
      // 
      // rbCopy
      // 
      rbCopy.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      rbCopy.AutoSize = true;
      rbCopy.Location = new Point (140, 15);
      rbCopy.Name = "rbCopy";
      rbCopy.Size = new Size (53, 19);
      rbCopy.TabIndex = 1;
      rbCopy.Text = "Copy";
      toolTip.SetToolTip (rbCopy, "Create a copy of each selected project and update selected dependent projects and solutions.");
      rbCopy.UseVisualStyleBackColor = true;
      rbCopy.Click += rbMoveCopy_CheckedChanged;
      // 
      // rbMove
      // 
      rbMove.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      rbMove.AutoSize = true;
      rbMove.Location = new Point (21, 15);
      rbMove.Name = "rbMove";
      rbMove.Size = new Size (106, 19);
      rbMove.TabIndex = 0;
      rbMove.Text = "Move / rename";
      toolTip.SetToolTip (rbMove, "Move or rename selected projects and update selected dependent projects and solutions.");
      rbMove.UseVisualStyleBackColor = true;
      rbMove.CheckedChanged += rbMoveCopy_CheckedChanged;
      // 
      // ckBoxSVN
      // 
      ckBoxSVN.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      ckBoxSVN.AutoSize = true;
      ckBoxSVN.Location = new Point (21, 70);
      ckBoxSVN.Name = "ckBoxSVN";
      ckBoxSVN.Size = new Size (48, 19);
      ckBoxSVN.TabIndex = 11;
      ckBoxSVN.Text = "SVN";
      toolTip.SetToolTip (ckBoxSVN, "All file operations via SVN (without Commit)");
      ckBoxSVN.UseVisualStyleBackColor = true;
      ckBoxSVN.CheckedChanged += ckBoxSVN_CheckedChanged;
      // 
      // panelProject
      // 
      panelProject.Controls.Add (updnDepth);
      panelProject.Controls.Add (labelDepth);
      panelProject.Controls.Add (labelProj);
      panelProject.Controls.Add (btnProj);
      panelProject.Controls.Add (txtBoxProj);
      panelProject.Dock = DockStyle.Top;
      panelProject.Location = new Point (0, 200);
      panelProject.Name = "panelProject";
      panelProject.Size = new Size (496, 66);
      panelProject.TabIndex = 2;
      // 
      // updnDepth
      // 
      updnDepth.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      updnDepth.Location = new Point (434, 8);
      updnDepth.Name = "updnDepth";
      updnDepth.Size = new Size (41, 23);
      updnDepth.TabIndex = 1;
      toolTip.SetToolTip (updnDepth, "Recursive scan for candidate projects in the sub-tree will stop \r\nonce the given recursion depth is reached ");
      updnDepth.Value = new decimal (new int[] { 3, 0, 0, 0 });
      updnDepth.ValueChanged += updnDepth_ValueChanged;
      // 
      // labelDepth
      // 
      labelDepth.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      labelDepth.AutoSize = true;
      labelDepth.Location = new Point (303, 10);
      labelDepth.Name = "labelDepth";
      labelDepth.Size = new Size (119, 15);
      labelDepth.TabIndex = 2;
      labelDepth.Text = "Max. recursion depth";
      // 
      // labelProj
      // 
      labelProj.AutoSize = true;
      labelProj.Location = new Point (28, 10);
      labelProj.Name = "labelProj";
      labelProj.Size = new Size (44, 15);
      labelProj.TabIndex = 2;
      labelProj.Text = "Project";
      // 
      // btnProj
      // 
      btnProj.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      btnProj.Location = new Point (434, 34);
      btnProj.Name = "btnProj";
      btnProj.Size = new Size (41, 23);
      btnProj.TabIndex = 0;
      btnProj.Text = "...";
      toolTip.SetToolTip (btnProj, "Select the project file");
      btnProj.UseVisualStyleBackColor = true;
      btnProj.Click += btnOpenFile_Click;
      // 
      // txtBoxProj
      // 
      txtBoxProj.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      txtBoxProj.BackColor = SystemColors.Control;
      txtBoxProj.BorderStyle = BorderStyle.FixedSingle;
      txtBoxProj.ForeColor = SystemColors.WindowText;
      txtBoxProj.Location = new Point (25, 34);
      txtBoxProj.Name = "txtBoxProj";
      txtBoxProj.ReadOnly = true;
      txtBoxProj.Size = new Size (397, 23);
      txtBoxProj.TabIndex = 2;
      // 
      // panelDest
      // 
      panelDest.Controls.Add (labelDest);
      panelDest.Controls.Add (btnDest);
      panelDest.Controls.Add (txtBoxDest);
      panelDest.Dock = DockStyle.Top;
      panelDest.Location = new Point (0, 266);
      panelDest.Name = "panelDest";
      panelDest.Size = new Size (496, 66);
      panelDest.TabIndex = 3;
      // 
      // labelDest
      // 
      labelDest.AutoSize = true;
      labelDest.Location = new Point (28, 10);
      labelDest.Name = "labelDest";
      labelDest.Size = new Size (101, 15);
      labelDest.TabIndex = 2;
      labelDest.Text = "Destination folder";
      // 
      // btnDest
      // 
      btnDest.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      btnDest.Location = new Point (434, 34);
      btnDest.Name = "btnDest";
      btnDest.Size = new Size (41, 23);
      btnDest.TabIndex = 0;
      btnDest.Text = "...";
      toolTip.SetToolTip (btnDest, "Select the destination folder");
      btnDest.UseVisualStyleBackColor = true;
      btnDest.Click += btnOpenFile_Click;
      // 
      // txtBoxDest
      // 
      txtBoxDest.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      txtBoxDest.BackColor = SystemColors.Control;
      txtBoxDest.BorderStyle = BorderStyle.FixedSingle;
      txtBoxDest.ForeColor = SystemColors.WindowText;
      txtBoxDest.Location = new Point (25, 34);
      txtBoxDest.Name = "txtBoxDest";
      txtBoxDest.ReadOnly = true;
      txtBoxDest.Size = new Size (397, 23);
      txtBoxDest.TabIndex = 2;
      toolTip.SetToolTip (txtBoxDest, "The destination root folder");
      // 
      // panelSolution
      // 
      panelSolution.Controls.Add (ckBoxRescan);
      panelSolution.Controls.Add (labelRescan);
      panelSolution.Controls.Add (labelSln);
      panelSolution.Controls.Add (btnSln);
      panelSolution.Controls.Add (txtBoxSln);
      panelSolution.Dock = DockStyle.Top;
      panelSolution.Location = new Point (0, 332);
      panelSolution.Name = "panelSolution";
      panelSolution.Size = new Size (496, 66);
      panelSolution.TabIndex = 4;
      // 
      // ckBoxRescan
      // 
      ckBoxRescan.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      ckBoxRescan.AutoSize = true;
      ckBoxRescan.Location = new Point (434, 11);
      ckBoxRescan.Name = "ckBoxRescan";
      ckBoxRescan.Size = new Size (15, 14);
      ckBoxRescan.TabIndex = 1;
      toolTip.SetToolTip (ckBoxRescan, "Solution folder sub-tree will be scanned again \r\non the next run, as solutions my have changed in between.");
      ckBoxRescan.UseVisualStyleBackColor = true;
      ckBoxRescan.CheckedChanged += ckBoxRescan_CheckedChanged;
      // 
      // labelRescan
      // 
      labelRescan.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      labelRescan.AutoSize = true;
      labelRescan.Location = new Point (378, 10);
      labelRescan.Name = "labelRescan";
      labelRescan.Size = new Size (44, 15);
      labelRescan.TabIndex = 2;
      labelRescan.Text = "Rescan";
      // 
      // labelSln
      // 
      labelSln.AutoSize = true;
      labelSln.Location = new Point (28, 10);
      labelSln.Name = "labelSln";
      labelSln.Size = new Size (51, 15);
      labelSln.TabIndex = 2;
      labelSln.Text = "Solution";
      // 
      // btnSln
      // 
      btnSln.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      btnSln.Location = new Point (434, 34);
      btnSln.Name = "btnSln";
      btnSln.Size = new Size (41, 23);
      btnSln.TabIndex = 0;
      btnSln.Text = "...";
      btnSln.UseVisualStyleBackColor = true;
      btnSln.Click += btnOpenFile_Click;
      // 
      // txtBoxSln
      // 
      txtBoxSln.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      txtBoxSln.BackColor = SystemColors.Control;
      txtBoxSln.BorderStyle = BorderStyle.FixedSingle;
      txtBoxSln.ForeColor = SystemColors.WindowText;
      txtBoxSln.Location = new Point (25, 34);
      txtBoxSln.Name = "txtBoxSln";
      txtBoxSln.ReadOnly = true;
      txtBoxSln.Size = new Size (397, 23);
      txtBoxSln.TabIndex = 2;
      // 
      // panelRun
      // 
      panelRun.Controls.Add (btnReset);
      panelRun.Controls.Add (btnAbort);
      panelRun.Controls.Add (btnExit);
      panelRun.Controls.Add (btnRun);
      panelRun.Dock = DockStyle.Fill;
      panelRun.Location = new Point (0, 398);
      panelRun.Name = "panelRun";
      panelRun.Size = new Size (496, 76);
      panelRun.TabIndex = 1;
      // 
      // btnReset
      // 
      btnReset.Enabled = false;
      btnReset.Location = new Point (120, 11);
      btnReset.Name = "btnReset";
      btnReset.Size = new Size (75, 23);
      btnReset.TabIndex = 1;
      btnReset.Text = "Reset";
      toolTip.SetToolTip (btnReset, "Reset the latest unsaved parameter changes to their previous state");
      btnReset.UseVisualStyleBackColor = true;
      btnReset.Click += btnReset_Click;
      // 
      // btnAbort
      // 
      btnAbort.Enabled = false;
      btnAbort.Location = new Point (120, 41);
      btnAbort.Name = "btnAbort";
      btnAbort.Size = new Size (75, 23);
      btnAbort.TabIndex = 2;
      btnAbort.Text = "Abort";
      toolTip.SetToolTip (btnAbort, "Abort the run in progress");
      btnAbort.UseVisualStyleBackColor = true;
      btnAbort.Click += btnAbort_Click;
      // 
      // btnExit
      // 
      btnExit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      btnExit.Location = new Point (400, 41);
      btnExit.Name = "btnExit";
      btnExit.Size = new Size (75, 23);
      btnExit.TabIndex = 3;
      btnExit.Text = "Exit";
      toolTip.SetToolTip (btnExit, "Exit the app, hereby saving any unsaved parameters");
      btnExit.UseVisualStyleBackColor = true;
      btnExit.Click += btnExit_Click;
      // 
      // btnRun
      // 
      btnRun.Enabled = false;
      btnRun.Location = new Point (25, 11);
      btnRun.Name = "btnRun";
      btnRun.Size = new Size (87, 53);
      btnRun.TabIndex = 0;
      btnRun.Text = "Run";
      toolTip.SetToolTip (btnRun, "Perform a run with the current parameters. \r\nWait for callbacks to select or confirm further actions. \r\n(Run will save current parameters.)");
      btnRun.UseVisualStyleBackColor = true;
      btnRun.Click += btnRun_Click;
      // 
      // panelProgress
      // 
      panelProgress.Controls.Add (progressBar);
      panelProgress.Controls.Add (labelProg1);
      panelProgress.Controls.Add (labelProg2);
      panelProgress.Dock = DockStyle.Bottom;
      panelProgress.Location = new Point (0, 474);
      panelProgress.Name = "panelProgress";
      panelProgress.Size = new Size (496, 67);
      panelProgress.TabIndex = 2;
      // 
      // progressBar
      // 
      progressBar.Dock = DockStyle.Fill;
      progressBar.Location = new Point (0, 0);
      progressBar.Name = "progressBar";
      progressBar.Size = new Size (496, 21);
      progressBar.TabIndex = 1;
      // 
      // labelProg1
      // 
      labelProg1.Dock = DockStyle.Bottom;
      labelProg1.Location = new Point (0, 21);
      labelProg1.Name = "labelProg1";
      labelProg1.Size = new Size (496, 23);
      labelProg1.TabIndex = 0;
      labelProg1.TextAlign = ContentAlignment.MiddleLeft;
      // 
      // labelProg2
      // 
      labelProg2.Dock = DockStyle.Bottom;
      labelProg2.Location = new Point (0, 44);
      labelProg2.Name = "labelProg2";
      labelProg2.Size = new Size (496, 23);
      labelProg2.TabIndex = 0;
      labelProg2.TextAlign = ContentAlignment.MiddleLeft;
      // 
      // panelParam
      // 
      panelParam.Controls.Add (panelSolution);
      panelParam.Controls.Add (panelDest);
      panelParam.Controls.Add (panelProject);
      panelParam.Controls.Add (panelAbsRel);
      panelParam.Controls.Add (panelRoot);
      panelParam.Controls.Add (panelTop);
      panelParam.Dock = DockStyle.Top;
      panelParam.Location = new Point (0, 0);
      panelParam.Name = "panelParam";
      panelParam.Size = new Size (496, 398);
      panelParam.TabIndex = 0;
      // 
      // panelAbsRel
      // 
      panelAbsRel.Controls.Add (rbAbs);
      panelAbsRel.Controls.Add (rbRel);
      panelAbsRel.Dock = DockStyle.Top;
      panelAbsRel.Location = new Point (0, 170);
      panelAbsRel.Name = "panelAbsRel";
      panelAbsRel.Size = new Size (496, 30);
      panelAbsRel.TabIndex = 1;
      // 
      // rbAbs
      // 
      rbAbs.AutoSize = true;
      rbAbs.Location = new Point (201, 5);
      rbAbs.Name = "rbAbs";
      rbAbs.Size = new Size (104, 19);
      rbAbs.TabIndex = 1;
      rbAbs.Text = "Absolute paths";
      rbAbs.UseVisualStyleBackColor = true;
      rbAbs.CheckedChanged += rbAbs_CheckedChanged;
      // 
      // rbRel
      // 
      rbRel.AutoSize = true;
      rbRel.Location = new Point (27, 5);
      rbRel.Name = "rbRel";
      rbRel.Size = new Size (168, 19);
      rbRel.TabIndex = 0;
      rbRel.Text = "Paths relative to root folder";
      rbRel.UseVisualStyleBackColor = true;
      rbRel.CheckedChanged += rbMoveCopy_CheckedChanged;
      // 
      // MainForm
      // 
      AutoScaleDimensions = new SizeF (7F, 15F);
      AutoScaleMode = AutoScaleMode.Font;
      ClientSize = new Size (496, 541);
      Controls.Add (panelRun);
      Controls.Add (panelParam);
      Controls.Add (panelProgress);
      Icon = (Icon)resources.GetObject ("$this.Icon");
      KeyPreview = true;
      MaximumSize = new Size (1024, 580);
      MinimumSize = new Size (512, 580);
      Name = "MainForm";
      Text = "C# Project Mover";
      toolTip.SetToolTip (this, "Select a single project for move/rename or copy and multiple dependent solutions.");
      panelRoot.ResumeLayout (false);
      panelRoot.PerformLayout ();
      panelTop.ResumeLayout (false);
      panelMulti.ResumeLayout (false);
      panelMulti.PerformLayout ();
      panelMode.ResumeLayout (false);
      panelMode.PerformLayout ();
      panelProject.ResumeLayout (false);
      panelProject.PerformLayout ();
      ((System.ComponentModel.ISupportInitialize)updnDepth).EndInit ();
      panelDest.ResumeLayout (false);
      panelDest.PerformLayout ();
      panelSolution.ResumeLayout (false);
      panelSolution.PerformLayout ();
      panelRun.ResumeLayout (false);
      panelProgress.ResumeLayout (false);
      panelParam.ResumeLayout (false);
      panelAbsRel.ResumeLayout (false);
      panelAbsRel.PerformLayout ();
      ResumeLayout (false);
    }

    #endregion

    private Lib.ReadOnlyTextBox txtBoxRoot;
    private RadioButton rbSglProjSglSol;
    private RadioButton rbSglProjMulSol;
    private RadioButton rbMulProjMulSol;
    private Panel panelRoot;
    private Label labelRoot;
    private Button btnRoot;
    private Panel panelTop;
    private Panel panelProject;
    private Label labelProj;
    private Button btnProj;
    private Lib.ReadOnlyTextBox txtBoxProj;
    private Panel panelDest;
    private Label labelDest;
    private Button btnDest;
    private Lib.ReadOnlyTextBox txtBoxDest;
    private Panel panelSolution;
    private Label labelSln;
    private Button btnSln;
    private Lib.ReadOnlyTextBox txtBoxSln;
    private Panel panelRun;
    private Panel panelMode;
    private RadioButton rbCopy;
    private RadioButton rbMove;
    private Panel panelMulti;
    private Button btnRun;
    private CheckBox ckBoxSVN;
    private Panel panelProgress;
    private ProgressBar progressBar;
    private Label labelProg1;
    private Label labelProg2;
    private Button btnAbort;
    private NumericUpDown updnDepth;
    private Label labelDepth;
    private CheckBox ckBoxRescan;
    private Label labelRescan;
    private Panel panelParam;
    private Button btnReset;
    private Panel panelAbsRel;
    private RadioButton rbAbs;
    private RadioButton rbRel;
    private Button btnExit;
    private ToolTip toolTip;
  }
}
