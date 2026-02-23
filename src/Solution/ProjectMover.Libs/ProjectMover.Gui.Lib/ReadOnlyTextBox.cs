using System.ComponentModel;

namespace ProjectMover.Gui.Lib {

  /// <summary>
  /// Better visual appearance of read-only mode than standard TextBox.
  /// </summary>
  /// <seealso cref="System.Windows.Forms.TextBox" />
  public class ReadOnlyTextBox : TextBox {
    private const int WM_COPY = 0x0301;

    private Color? _customReadOnlyBackColor;
    private bool _useSystemGrayForReadOnly = true;
    private readonly BorderStyle _userBorderStyle; // store developer's original border

    [Category ("Appearance")]
    [Description (""" 
      Optional: force a custom readonly background color. 
      If null (default), it auto-adjusts based on light/dark mode. 
      Only used when UseSystemGrayForReadOnly is false.
      """)]     
    [DefaultValue (null)]
    public Color? ReadOnlyBackColorOverride {
      get => _customReadOnlyBackColor;
      set { _customReadOnlyBackColor = value; updateColorsAndBorder (); }
    }

    [Category ("Appearance")]
    [Description ("""
      If true, read-only uses SystemColors.Control (gray) instead of a tinted background. 
      Default = true.
      """)]     
    [DefaultValue (true)]
    public bool UseSystemGrayForReadOnly {
      get => _useSystemGrayForReadOnly;
      set { _useSystemGrayForReadOnly = value; updateColorsAndBorder (); }
    }

    public ReadOnlyTextBox () {
      _userBorderStyle = base.BorderStyle; // remember initial border
    }

    protected override void OnCreateControl () {
      base.OnCreateControl ();
      updateColorsAndBorder ();
    }

    protected override void OnReadOnlyChanged (EventArgs e) {
      base.OnReadOnlyChanged (e);
      updateColorsAndBorder ();
    }

    protected override void OnEnabledChanged (EventArgs e) {
      base.OnEnabledChanged (e);
      updateColorsAndBorder ();
    }

    protected override void WndProc (ref Message m) {
      if (m.Msg == WM_COPY) {
        if (SelectionLength == TextLength &&
            Tag is string tagText) {
          Clipboard.SetText (tagText);
          return; // suppress default copy
        }
      }

      base.WndProc (ref m);
    }

    private void updateColorsAndBorder () {
      // Border
      base.BorderStyle = ReadOnly ? BorderStyle.FixedSingle : _userBorderStyle;

      // Colors
      if (!Enabled) {
        BackColor = SystemColors.Control;
        ForeColor = SystemColors.GrayText;
      } else if (ReadOnly) {
        if (UseSystemGrayForReadOnly) {
          BackColor = SystemColors.Control;
          ForeColor = SystemColors.WindowText;
        } else {
          BackColor = _customReadOnlyBackColor ?? getDefaultReadOnlyBackColor ();
          ForeColor = SystemColors.WindowText;
        }
      } else {
        BackColor = SystemColors.Window;
        ForeColor = SystemColors.WindowText;
      }
    }

    private static Color getDefaultReadOnlyBackColor () {
      bool dark = isDarkMode ();
      if (dark) {
        var baseColor = SystemColors.Window;
        return ControlPaint.Dark (baseColor, 0.1f); // slightly darker
      } else {
        return Color.FromArgb (240, 248, 255); // AliceBlue
      }
    }

    private static bool isDarkMode () {
      // crude but effective: dark mode if window background is darker than text
      int windowBrightness = brightness (SystemColors.Window);
      int textBrightness = brightness (SystemColors.WindowText);
      return windowBrightness < textBrightness;
    }

    private static int brightness (Color c) =>
        (int)Math.Sqrt (
            c.R * c.R * .241 +
            c.G * c.G * .691 +
            c.B * c.B * .068);
  }

}
