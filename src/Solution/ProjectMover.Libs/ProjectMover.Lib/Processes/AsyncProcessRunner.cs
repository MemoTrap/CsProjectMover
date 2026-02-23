using System.Diagnostics;

namespace ProjectMover.Lib.Processes;

public static class AsyncProcessRunner {

  public static async Task<ProcessExecutionResult> RunAsync (
      string executable,
      string arguments,
      string? workingDirectory = null,
      CancellationToken ct = default
  ) {
    var psi = new ProcessStartInfo {
      FileName = executable,
      Arguments = arguments,
      UseShellExecute = false,
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      CreateNoWindow = true,
      WorkingDirectory = workingDirectory ?? string.Empty
    };

    using var process = new Process { StartInfo = psi };

    process.Start ();

    var stdoutTask = process.StandardOutput.ReadToEndAsync (ct);
    var stderrTask = process.StandardError.ReadToEndAsync (ct);

    await process.WaitForExitAsync (ct).ConfigureAwait (false);

    var stdout = await stdoutTask.ConfigureAwait (false);
    var stderr = await stderrTask.ConfigureAwait (false);

    return new ProcessExecutionResult (
        process.ExitCode,
        stdout,
        stderr
    );
  }

  public static async Task<ProcessExecutionResult> RunAndAssertSuccessAsync (
      string executable,
      string arguments,
      string? workingDirectory = null,
      CancellationToken ct = default
  ) {
    var result = await RunAsync (executable, arguments, workingDirectory, ct);

    if (!result.IsSuccess) {
      throw new InvalidOperationException (
          $"{executable} failed ({result.ExitCode})\n{result.StdErr}");
    }

    return result;
  }
}
