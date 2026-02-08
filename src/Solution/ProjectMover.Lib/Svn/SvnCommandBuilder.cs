namespace ProjectMover.Lib.Svn;

internal sealed class SvnCommandBuilder {

  private readonly List<string> _args = [];

  public SvnCommandBuilder Add (string arg) {
    _args.Add (arg);
    return this;
  }

  public SvnCommandBuilder AddPath (string path) {
    _args.Add (quoteIfNeeded (path));
    return this;
  }

  public SvnCommandBuilder AddFlag (string flag)
      => Add (flag);

  public SvnCommandBuilder AddRevision (string revision)
      => Add ("-r").Add (revision);

  public string Build ()
      => string.Join (" ", _args);

  private static string quoteIfNeeded (string s)
      => s.Contains (' ') ? $"\"{s}\"" : s;
}
