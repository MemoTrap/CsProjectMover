#pragma warning disable IDE1006

namespace ProjectMover.Gui.Lib {
  public static class ControlExtensions {

    private static readonly Dictionary<Form, ToolTip> __tooltips = [];
    private static readonly HashSet<Control> __controls = [];


    public static void SetTextAsPathWithEllipsis (this Control label, string? text = null) {
      // Tag is used for resize event
      if (text is null)
        text = label.Tag as string;
      else
        label.Tag = text;

      if (text is null)
        return;

      label.Text = text;

      var form = label.FindForm ();
      if (form is null)
        return;

      __tooltips.TryGetValue (form, out var tooltip);

      Size size = new (label.Width - 8, label.Height);
      Size reqSize = TextRenderer.MeasureText (text, label.Font);

      if (reqSize.Width > size.Width) {
        if (tooltip is null) {
          tooltip = new ToolTip ();
          __tooltips[form] = tooltip;
        }
        if (__controls.Add (label))
          label.Resize += label_Resize; // once only

        tooltip!.SetToolTip (label, text);

        string newText = text.ShortenPathString (size.Width, label.Font);

        label.Text = newText;
      } else 
        tooltip?.SetToolTip (label, null);
    }

    private static void label_Resize (object? sender, System.EventArgs e) => 
      (sender as Control)?.SetTextAsPathWithEllipsis ();


  }
}
