#pragma warning disable IDE0079 
#pragma warning disable CA1859 
#pragma warning disable IDE0305 

namespace ProjectMover.Lib.Svn;

internal sealed class SvnFileOperationContext (string wcRoot) : IFileOperationContext {
  record FileOrDirPath (string Path, bool IsDir);
  List<FileOrDirPath> TrackedPath { get; } = [];
  
  private readonly string _wcRoot = wcRoot; // possibly an intermediate folder in the working copy
  private bool _completed = true;


  public async ValueTask DisposeAsync () {

    if (_completed)
      return;

    await SvnClient.RunAndAssertAsync (
      new SvnCommandBuilder ()
        .Add ("revert")
        .Add ("--remove-added")
        .Add ("-R")
        .AddPath (_wcRoot),
      CancellationToken.None);

    deleteTrackedFiles ();
    deleteTrackedDirectories ();
  }

  private void deleteTrackedFiles () {
    foreach (var f in TrackedPath.Distinct().Where (i => !i.IsDir)) {
      try {
        if (File.Exists (f.Path))
          File.Delete (f.Path);
      } catch {
        // swallow or log – rollback must be best-effort
      }
    }
  }

  private void deleteTrackedDirectories () {

    var dirs = TrackedPath
      .Distinct ()
      .Where (i => i.IsDir)
      .Select (i => i.Path)
      .OrderByDescending (p => p.Length)   // deepest first
      .ToList ();

    foreach (var dir in dirs) {
      try {
        if (Directory.Exists (dir))
          Directory.Delete (dir, recursive: true);
      } catch {
        // swallow or log
      }
    }
  }


  public void MarkCompleted (bool isCompleted = true) => _completed = isCompleted;

  public async Task CreateDirectoryAsync (string path, CancellationToken ct) {
    if (Directory.Exists (path))
      return;

    var created = getMissingDirectories (path);
    TrackedPath.AddRange (created.Select (d => new FileOrDirPath (d, true)));

    await SvnClient.RunAndAssertAsync (
      new SvnCommandBuilder ()
        .Add ("mkdir")
        .Add ("--parents")
        .AddPath (path),
      ct);
  }

  private static IReadOnlyList<string> getMissingDirectories (string path) {
    var list = new Stack<string> ();
    var current = Path.GetFullPath (path);

    while (!Directory.Exists (current)) {
      list.Push (current);
      current = Path.GetDirectoryName (current)
        ?? throw new InvalidOperationException ($"Reached filesystem root for '{path}'.");
    }

    return list.ToList (); // top-down order
  }


  public async Task MoveDirectoryAsync (string sourceDir, string targetDir, CancellationToken ct) {
    await SvnClient.RunAndAssertAsync (
      new SvnCommandBuilder ()
        .Add ("move")
        .AddPath (sourceDir)
        .AddPath (targetDir),
      ct);
  }

  public async Task CopyDirectoryAsync (string sourceDir, string targetDir, CancellationToken ct) {
    // copies everything on disk, only adds versioned items to svn
    await SvnClient.RunAndAssertAsync (
      new SvnCommandBuilder ()
        .Add ("copy")
        .AddPath (sourceDir)
        .AddPath (targetDir),
      ct);

    // we don't want bin and obj, so in this case, we just delete them in the target afterwards
    await deleteTargetAsync ("obj");
    await deleteTargetAsync ("bin");

    async Task deleteTargetAsync (string subdir) =>
      await deleteUnversionedAsync (Path.Combine (targetDir, subdir), ct);
  }

  private static async Task deleteUnversionedAsync (string path, CancellationToken ct) {
    if (!Directory.Exists (path))
      return;
    bool versioned = await isVersionedAsync (path, ct);
    if (!versioned)
      Directory.Delete (path, true);
  }

  private static async Task<bool> isVersionedAsync (string path, CancellationToken ct) {
    var result = await SvnClient.RunAsync (
      new SvnCommandBuilder ()
        .Add ("status")
        .Add ("--depth")
        .Add ("empty")
        .AddPath (path),
      ct);

    // No output → path not versioned and not even known to svn
    if (string.IsNullOrWhiteSpace (result.StdOut))
      return false;

    // If first non-whitespace char is '?', it's unversioned, if "I", it's ignored
    char first = result.StdOut.TrimStart ()[0];
    return first != '?' && first != 'I';
  }

  public async Task DeleteFileAsync (string path, CancellationToken ct) {

    if (File.Exists (path)) {
      await SvnClient.RunAndAssertAsync (
        new SvnCommandBuilder ()
          .Add ("delete")
          .AddPath (path),
        ct);
    }
  }

  public async Task WriteFileAsync (
    Func<CancellationToken, Task<string>> writerFuncAsync,
    CancellationToken ct
  ) {
    string path = await writerFuncAsync (ct);

    await addToSvn (path, ct);
  }

  public async Task WriteFileAsync (
    Func<bool, CancellationToken, Task<string>> writerFuncAsync,
    CancellationToken ct
  ) {
    string path = await writerFuncAsync (false, ct);

    await addToSvn (path, ct);
  }

  public async Task WriteFileAsync (
    string path,
    Func<string, CancellationToken, Task> writerFuncAsync, 
    CancellationToken ct
  ) {
    
    await writerFuncAsync (path, ct);

    await addToSvn (path, ct);
  }

  private async Task addToSvn (string path, CancellationToken ct) {
    // if new file → svn add
    var status = await SvnClient.RunAsync (
      new SvnCommandBuilder ()
        .Add ("status")
        .AddPath (path),
      ct);

    if (status.StdOut.StartsWith ('?')) {
      await SvnClient.RunAndAssertAsync (
        new SvnCommandBuilder ()
          .Add ("add")
          .AddPath (path),
        ct);
    
      TrackedPath.Add (new FileOrDirPath (path, false));
    }

  }

  public async Task<bool> ValidateEnvironmentAsync (
    IProgressSink progress,
    ICallbackSink callback,
    IEnumerable<string?> pathsToCheck, 
    CancellationToken ct
  ) {
    ct.ThrowIfCancellationRequested ();

    // check for local modifications in the root folder
    var statusResult = await SvnClient.RunAsync (
      new SvnCommandBuilder ()
        .Add ("diff")
        .Add ("--summarize")
        .AddPath (_wcRoot),
      ct);

    string nl = Environment.NewLine;
    if (!string.IsNullOrWhiteSpace (statusResult.StdOut)) {
      string msg =
        "Local SVN changes were detected in the root folder. " + nl +
        "ProjectMover performs automatic rollbacks on failure, which would also revert those changes." + nl +
        "Therefore, all changes should be committed or reverted before running ProjectMover." + nl + nl +
        "Continue anyway?";

      var res = callback.ShowMessage (ECallback.Warning, msg, "SVN Local Changes", EMsgBtns.YesNo, EDefaultMsgBtn.Button2);
      if (res != EDialogResult.Yes && res != EDialogResult.OK)
        return false;
    }


    foreach (var path in pathsToCheck) {
      ct.ThrowIfCancellationRequested ();

      if (path is null)
        continue;

      if (Directory.Exists (path)) {

        // Existing directory must be under version control
        var infoResult = await SvnClient.RunAsync (new SvnCommandBuilder ().Add ("info").AddPath (path), ct);
        if (!infoResult.StdOut.Contains ("Path:"))
          throw new InvalidOperationException ($"Path is not under SVN control: '{path}'");

        // Test writing a file and adding to SVN
        var guid = Guid.NewGuid ();
        string testFile = Path.Combine (path, $".{guid}.tmp");
        async Task asyncWriter (string _,CancellationToken __) {
          await File.WriteAllTextAsync (testFile, "validation", ct);
        }
        await WriteFileAsync (testFile, asyncWriter, ct);

        // revert to leave the working copy clean
        await SvnClient.RunAndAssertAsync (new SvnCommandBuilder ()
          .Add ("revert")
          .Add("--remove-added")
          .AddPath (testFile), 
        ct);

        // play safe and delete the test file if it still exists
        if (File.Exists (testFile))
          File.Delete (testFile);
      } else if (File.Exists (path)) {
        // Existing file must be under version control
        var infoResult = await SvnClient.RunAsync (new SvnCommandBuilder ().Add ("info").AddPath (path), ct);
        if (!infoResult.StdOut.Contains ("Path:"))
          throw new InvalidOperationException ($"Path is not under SVN control: '{path}'");
      } 
    }

    return true;
  }
}
