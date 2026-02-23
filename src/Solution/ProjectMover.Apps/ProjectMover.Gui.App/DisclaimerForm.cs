namespace ProjectMover.Gui.App {
  internal class DisclaimerForm : Form {
    public bool DontShowAgain { get; private set; }

    public DisclaimerForm () {
      this.FormBorderStyle = FormBorderStyle.None;
      this.StartPosition = FormStartPosition.CenterParent;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.ShowInTaskbar = false;
      this.ControlBox = false;
      this.AutoScaleMode = AutoScaleMode.Font;
      this.Padding = new Padding (12);

      this.TopMost = true;   // for first-run disclaimers
      this.DoubleBuffered = true;

      var layout = new TableLayoutPanel {
        Dock = DockStyle.Fill,
        ColumnCount = 1,
        RowCount = 3,
        AutoSize = true,
      };

      layout.RowStyles.Add (new RowStyle (SizeType.Percent, 100));
      layout.RowStyles.Add (new RowStyle (SizeType.AutoSize));
      layout.RowStyles.Add (new RowStyle (SizeType.AutoSize));

      var disclaimerText = new Label {
        BorderStyle = BorderStyle.None,
        Dock = DockStyle.Fill,
        BackColor = SystemColors.Control,
        Text =
          """
          NO WARRANTY

          This software is provided "as is", without warranty of any kind, express or implied,
          including but not limited to the warranties of merchantability, fitness for a
          particular purpose and noninfringement.

          In no event shall the authors be liable for any claim, damages or other liability.

          Use at your own risk.
          """
      };


      var dontShowAgainCk = new CheckBox {
        Text = "Don’t show again",
        AutoSize = true,
        Dock = DockStyle.Left,
      };
      dontShowAgainCk.CheckedChanged += (s, e) => DontShowAgain = dontShowAgainCk.Checked;

      var okButton = new Button {
        Text = "OK",
        DialogResult = DialogResult.OK,
        Anchor = AnchorStyles.Right,
      };

      this.AcceptButton = okButton;

      layout.Controls.Add (disclaimerText, 0, 0);
      layout.Controls.Add (dontShowAgainCk, 0, 1);
      layout.Controls.Add (okButton, 0, 2);

      this.Controls.Add (layout);
      this.ClientSize = new Size (480, 260);

      this.Paint += (_, e) =>
        e.Graphics.DrawRectangle (Pens.Gray, this.ClientRectangle.X,
          this.ClientRectangle.Y,
          this.ClientRectangle.Width - 1,
          this.ClientRectangle.Height - 1);

    }

  }


}
