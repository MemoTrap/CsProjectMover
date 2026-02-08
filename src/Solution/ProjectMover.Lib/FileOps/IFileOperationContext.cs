namespace ProjectMover.Lib.FileOps;

internal interface IFileOperationContext : IAsyncDisposable {

  Task CreateDirectoryAsync (string path, CancellationToken ct);

  Task MoveDirectoryAsync (string sourceDir, string targetDir, CancellationToken ct);

  Task CopyDirectoryAsync (string sourceDir, string targetDir, CancellationToken ct);

  Task DeleteFileAsync (string path, CancellationToken ct);

  Task WriteFileAsync (Func<CancellationToken, Task<string>> writerFuncAsync, CancellationToken ct);
  Task WriteFileAsync (Func<bool, CancellationToken, Task<string>> writerFuncAsync, CancellationToken ct);
  Task WriteFileAsync (string path, Func<string, CancellationToken, Task> writerFuncAsync, CancellationToken ct);

  /// <summary>
  /// Perform all pre-flight checks required by this backend.
  /// Throws InvalidOperationException if environment is not valid.
  /// </summary>
  Task<bool> ValidateEnvironmentAsync (
    IProgressSink progress, 
    ICallbackSink callback, 
    IEnumerable<string?> pathsToCheck, 
    CancellationToken ct);

  void MarkCompleted (bool isCompleted = true);

}

