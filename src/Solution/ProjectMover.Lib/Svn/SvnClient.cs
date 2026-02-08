namespace ProjectMover.Lib.Svn;

internal static class SvnClient {

  public static async Task<SvnResult> RunAsync (
      SvnCommandBuilder cmd,
      CancellationToken ct
  ) {
    var result = await AsyncProcessRunner.RunAsync (
        "svn",
        cmd.Build (),
        ct: ct);

    return SvnResult.From (result);
  }

  public static async Task RunAndAssertAsync (
      SvnCommandBuilder cmd,
      CancellationToken ct
  ) {
    var r = await RunAsync (cmd, ct);

    if (!r.IsSuccess) {
      throw new InvalidOperationException (
          $"svn {cmd.Build ()} failed ({r.ExitCode})\n{r.StdErr}");
    }
  }

  public static async Task<string> GetWorkingCopyRootAsync (string anyPathInsideWc, CancellationToken ct) {

    var result = await RunAsync (
      new SvnCommandBuilder ()
        .Add ("info")
        .Add ("--show-item")
        .Add ("wc-root")
        .AddPath (anyPathInsideWc),
      ct
    );

    if (!result.IsSuccess)
      throw new InvalidOperationException ("Failed to determine SVN working copy root.");

    return result.StdOut.Trim ();
  }

}

