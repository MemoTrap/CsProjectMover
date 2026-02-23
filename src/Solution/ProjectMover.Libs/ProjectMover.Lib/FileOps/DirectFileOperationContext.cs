namespace ProjectMover.Lib.FileOps;

internal sealed class DirectFileOperationContext : IFileOperationContext {

  public ValueTask DisposeAsync () => ValueTask.CompletedTask;

  public Task CreateDirectoryAsync (string path, CancellationToken ct) {
    Directory.CreateDirectory (path);
    return Task.CompletedTask;
  }

  public Task MoveDirectoryAsync (string sourceDir, string targetDir, CancellationToken ct) {
    Directory.Move (sourceDir, targetDir);
    return Task.CompletedTask;
  }

  public async Task CopyDirectoryAsync (string sourceDir, string targetDir, CancellationToken ct) {
    await copyDirectoryAsync (sourceDir, targetDir, ct);
  }

  public Task DeleteFileAsync (string path, CancellationToken ct) {
    File.Delete (path);
    return Task.CompletedTask;
  }

  public async Task WriteFileAsync (
    Func<CancellationToken, Task<string>> writerFuncAsync, 
    CancellationToken ct
  ) { 
    await writerFuncAsync (ct);
  }

  public async Task WriteFileAsync (
    Func<bool, CancellationToken, Task<string>> writerFuncAsync, 
    CancellationToken ct
  ) { 
    await writerFuncAsync (false, ct);
  }

  public async Task WriteFileAsync (
    string path,
    Func<string, CancellationToken, Task> writerFuncAsync,
    CancellationToken ct
  ) {
    await writerFuncAsync (path, ct);
  }

  private static async Task copyDirectoryAsync (
      string sourceDir,
      string targetDir,
      CancellationToken ct
  ) {
    Directory.CreateDirectory (targetDir);

    foreach (var filePath in Directory.EnumerateFiles (sourceDir)) {
      ct.ThrowIfCancellationRequested ();

      var fileName = Path.GetFileName (filePath);
      var targetFile = Path.Combine (targetDir, fileName);
      File.Copy (filePath, targetFile, overwrite: true);
    }

    foreach (var dirPath in Directory.EnumerateDirectories (sourceDir)) {
      ct.ThrowIfCancellationRequested ();

      var dirName = Path.GetFileName (dirPath);
      if (string.Equals (dirName, "bin", StringComparison.OrdinalIgnoreCase))
        continue;
      if (string.Equals (dirName, "obj", StringComparison.OrdinalIgnoreCase))
        continue;

      var targetSub = Path.Combine (targetDir, dirName);
      await copyDirectoryAsync (dirPath, targetSub, ct);
    }
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
