#pragma warning disable IDE0130

using ProjectMover.Lib.Processes;

namespace ProjectMover.Tests {
  internal static class BuildAssertions {
    public static void AssertSuccessfulBuild (
        ProcessExecutionResult result,
        string solutionPath) {
      if (result.ExitCode != 0) {
        Assert.Fail (
            $"dotnet build failed for solution:\n{solutionPath}\n\n" +
            $"EXIT CODE: {result.ExitCode}\n\n" +
            $"STDOUT:\n{result.StdOut}\n\n" +
            $"STDERR:\n{result.StdErr}"
        );
      }

      // sanity checks (non-brittle)
      StringAssert.Contains (
          result.StdOut,
          "Build succeeded",
          "Expected 'Build succeeded' in dotnet build output");

      Assert.IsTrue (
          string.IsNullOrWhiteSpace (result.StdErr),
          $"Expected no stderr output, but got:\n{result.StdErr}"
      );
    }
  }
}
