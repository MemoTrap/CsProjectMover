namespace ProjectMover.Lib.FileOps;

internal sealed class NullFileOperationContext : IFileOperationContext {

  public ValueTask DisposeAsync () => ValueTask.CompletedTask;

  public Task CreateDirectoryAsync (string path, CancellationToken ct) {
    return Task.CompletedTask;
  }

  public Task MoveDirectoryAsync (string sourceDir, string targetDir, CancellationToken ct) {
    return Task.CompletedTask;
  }

  public Task CopyDirectoryAsync (string sourceDir, string targetDir, CancellationToken ct) {
    return Task.CompletedTask;
  }

  public Task DeleteFileAsync (string path, CancellationToken ct) {
    return Task.CompletedTask;
  }

  public Task WriteFileAsync (
    Func<CancellationToken, Task<string>> writerFuncAsync, 
    CancellationToken ct
  ) {
    return Task.CompletedTask;
  }

  public async Task WriteFileAsync (
    Func<bool, CancellationToken, Task<string>> writerFuncAsync,
    CancellationToken ct
  ) {
    await writerFuncAsync (true, ct);
  }

  public Task WriteFileAsync (
    string path,
    Func<string, CancellationToken, Task> writerFuncAsync,
    CancellationToken ct
  ) {
    return Task.CompletedTask;
  }


  public Task<bool> ValidateEnvironmentAsync (
    IProgressSink _, 
    ICallbackSink __, 
    IEnumerable<string?> ____, 
    CancellationToken _____) 
    => Task.FromResult (true);

  public void MarkCompleted (bool isCompleted = true) {
    // nothing to do here
  }
}
