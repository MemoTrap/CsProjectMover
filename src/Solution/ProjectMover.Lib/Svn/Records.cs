namespace ProjectMover.Lib.Svn;

internal sealed record SvnResult (
    int ExitCode,
    string StdOut,
    string StdErr
) {
  public bool IsSuccess => ExitCode == 0;

  public bool HasErrors => !string.IsNullOrWhiteSpace (StdErr);

  public IEnumerable<string> ErrorLines =>
      StdErr.Split (['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

  public static SvnResult From (ProcessExecutionResult r)
      => new (r.ExitCode, r.StdOut, r.StdErr);
}
