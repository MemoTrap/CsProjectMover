#pragma warning disable IDE0079
#pragma warning disable IDE0057
#pragma warning disable IDE1006
#pragma warning disable SYSLIB1045

using System.Text.RegularExpressions;

namespace ProjectMover.Gui.Lib {
  public static class StringExtensions {
    public static int DefaultWidth { get; set; } = 330;


    private const char ELLIPS = '…';

    static readonly Regex __bulletLineRegex = new (
      @"^(?<lead>\s*[-•]?\s*)(?<path>.*?)(?<trail>(?:\s+\(.*\))?)$",
          RegexOptions.Compiled
      );

    static readonly Regex __colonLineRegex = new (
      @"^(?<lead>.*?: )(?<path>.+?)(?<trail> \([^)]*\))?$",
          RegexOptions.Compiled
      );

    public static string ShortenMultiLineBulletDecoratedPathString (this string text, int maxWidth = -1, Font? font = null) {
      if (maxWidth < 0)
        maxWidth = DefaultWidth;

      font ??= SystemFonts.MessageBoxFont;

      var lines = text.Split ([ "\r\n", "\n" ], StringSplitOptions.None);

      for (int i = 0; i < lines.Length; i++)
        lines[i] = shortenLine (lines[i]);

      return string.Join (Environment.NewLine, lines);

      string shortenLine (string line) {
        if (TextRenderer.MeasureText (line, font).Width <= maxWidth)
          return line;
        var m = __bulletLineRegex.Match (line);
        if (!m.Success)
          return line;

        string lead = m.Groups["lead"].Value;
        string path = m.Groups["path"].Value;
        string trail = m.Groups["trail"].Value;

        string? newPath = ShortenPathString (path, maxWidth, font, lead, trail);

        path = newPath ?? ELLIPS.ToString ();

        line = lead + path + trail;

        return line;
      }
    }

    public static string ShortenColonDecoratedPathString (this string line, int maxWidth, Font font) {
      if (TextRenderer.MeasureText (line, font).Width <= maxWidth)
        return line;
      var m = __colonLineRegex.Match (line);
      if (!m.Success)
        return line;

      string lead = m.Groups["lead"].Value;
      string path = m.Groups["path"].Value;
      string trail = m.Groups["trail"].Value;

      string? newPath = ShortenPathString (path, maxWidth, font, lead, trail);

      path = newPath ?? ELLIPS.ToString ();

      line = lead + path + trail;

      return line;
    }


    public static string ShortenPathString (
      this string path, 
      int maxWidth, 
      Font? font = null, 
      string? lead = null, 
      string? trail = null
    ) {
      string text = lead + path + trail;
      int wid = TextRenderer.MeasureText (text, font).Width;
      bool fit = wid <= maxWidth;
      if (fit)
        return path;

      int pos = path.LastIndexOfAny (['/', '\\']);
      string? newPath;
      if (pos >= 0) {
        string left = path.Substring (0, pos);
        string right = path.Substring (pos);

        newPath = shrinkLoop (left, right, false, maxWidth, font, lead, trail);
        if (newPath is null) {
          pos = right.Length / 2;
          left = $"{ELLIPS}{right.Substring (0, pos)}";
          right = right.Substring (pos);

          newPath = shrinkLoop (left, right, true, maxWidth, font, lead, trail);
        }
      } else {
        pos = path.Length / 2;
        string left = path.Substring (0, pos);
        string right = path.Substring (pos);

        newPath = shrinkLoop (left, right, true, maxWidth, font, lead, trail);
      }

      return newPath ?? ELLIPS.ToString();
    }

    private static string? shrinkLoop (
      string left, 
      string right, 
      bool both, 
      int maxWidth, 
      Font? font, 
      string? lead = null, 
      string? trail = null
    ) {
      bool fit = false;
      string? newPath = null;
      int i = 0;
      while (left.Length >= 4 && right.Length >= 4 && !fit) {
        if (!both || i % 2 == 0)
          left = left.Substring (0, left.Length - 1);
        else
          right = right.Substring (1);
        i++;
        newPath = $"{left}{ELLIPS}{right}";

        string newLine = lead + newPath + trail;

        int wid = TextRenderer.MeasureText (newLine, font).Width;
        fit = wid <= maxWidth;
      }
      if (fit)
        return newPath;
      else
        return null;
    }

  }
}
