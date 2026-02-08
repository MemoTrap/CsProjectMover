namespace ProjectMover.Lib.Models {
  internal abstract class FileBase {
    public string? DryRunSave { get; protected set; }

    public string OrigAbsPath { get; private set;}
    public string OrigDirectory { get; private set;}
    public string OrigName { get; private set;}
    public string AbsolutePath { get; private set; }
    public string Name { get; private set;}
    public string Directory { get; private set;}
    public DateTime LastWriteTimeUtc { get; private set;} 

    protected FileBase (string filePath, string extension) {
      string ext = Path.GetExtension (filePath);
      if (!string.Equals (ext, extension, StringComparison.OrdinalIgnoreCase))
        throw new ArgumentException ($"File must have '{extension}' extension.", nameof (filePath));

      bool rooted = Path.IsPathRooted (filePath);
      if (!rooted)
        throw new ArgumentException ($"Path must be rooted", nameof (filePath));

      filePath = Path.GetFullPath (filePath);

      if (!File.Exists (filePath))
        throw new FileNotFoundException ("File not found.", filePath);  

      AbsolutePath = filePath;
      OrigAbsPath = AbsolutePath;
      LastWriteTimeUtc = File.GetLastWriteTimeUtc (AbsolutePath);
      Directory = Path.GetDirectoryName (AbsolutePath)!;
      OrigDirectory = Directory;
      Name = Path.GetFileNameWithoutExtension (AbsolutePath);
      OrigName = Name;
    }

    public abstract Task LoadAsync (CancellationToken ct = default);

    public Task<string> SaveAsync (CancellationToken ct = default) => SaveExAsync (false, ct);
    public abstract Task<string> SaveExAsync (bool dryRun, CancellationToken ct = default);


    public override string ToString () => AbsolutePath;

    public void CommitUpdatedFilePath () {
      OrigAbsPath = AbsolutePath;
      OrigDirectory = Directory;
      OrigName = Name;

      if (File.Exists (AbsolutePath))
        LastWriteTimeUtc = File.GetLastWriteTimeUtc (AbsolutePath);
      else
        LastWriteTimeUtc = DateTime.MinValue;
    }

    protected void updateFilePath (string newFilePath) {
      AbsolutePath = newFilePath;
      Directory = Path.GetDirectoryName (AbsolutePath)!;
      Name = Path.GetFileNameWithoutExtension (AbsolutePath);

      if (File.Exists (AbsolutePath))
        LastWriteTimeUtc = File.GetLastWriteTimeUtc (AbsolutePath);
      else
        LastWriteTimeUtc = DateTime.MinValue;
    }

    protected void updateLastWriteTime () {
      LastWriteTimeUtc = File.GetLastWriteTimeUtc (AbsolutePath);
    }
  }

}
