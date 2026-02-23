namespace ProjectMover.Lib.Processes {

  public sealed record ProcessExecutionResult (
      int ExitCode,
      string StdOut,
      string StdErr
  ) {
    public bool IsSuccess => ExitCode == 0;
  }
}
