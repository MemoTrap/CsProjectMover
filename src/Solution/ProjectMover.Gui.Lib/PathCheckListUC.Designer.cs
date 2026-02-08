namespace ProjectMover.Gui.Lib {
  partial class PathCheckListUC {
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent () {
      listView = new ListView ();
      SuspendLayout ();
      // 
      // listView
      // 
      listView.CheckBoxes = true;
      listView.Dock = DockStyle.Fill;
      listView.FullRowSelect = true;
      listView.Location = new Point (0, 0);
      listView.Name = "listView";
      listView.Size = new Size (293, 105);
      listView.TabIndex = 0;
      listView.UseCompatibleStateImageBehavior = false;
      listView.View = View.Details;
      listView.ItemChecked += listView_ItemChecked;
      listView.KeyDown += listView_KeyDown;
      // 
      // PathCheckListUC
      // 
      AutoScaleDimensions = new SizeF (7F, 15F);
      AutoScaleMode = AutoScaleMode.Font;
      Controls.Add (listView);
      Name = "PathCheckListUC";
      Size = new Size (293, 105);
      ResumeLayout (false);
    }

    #endregion

    private ListView listView;
  }
}
