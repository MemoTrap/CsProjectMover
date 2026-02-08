namespace ProjectMover.Lib.Api {
  public interface IParameters {
    EMultiMode MultiMode { get; }
    string? RootFolder { get; }
    string? ProjectFolderOrFile { get; }
    string? DestinationFolder { get; }
    string? SolutionFolderOrFile { get; }

    /// <summary>
    /// Maximum recursion depth when searching from ProjectFolderOrFile
    /// in multi-project mode.
    /// Default: 5
    /// </summary>
    int ProjectRootRecursionDepth { get; }

    /// <summary>
    /// If false and RootFolder has not changed, cached scan results
    /// may be reused.
    /// </summary>
    bool Rescan { get; }
    
    /// <summary>
    /// Activate copy mode, default is move/rename
    /// </summary>
    bool Copy { get; }

    /// <summary>
    /// File operations direct or via version control system
    /// </summary>
    EFileOperations FileOperations { get; }

    /// <summary>
    /// Demand full paths. By default, path names in user communication are relative to the root folder 
    /// </summary>
    public bool AbsPathsInUserCommunication { get; }
  }

  public record Parameters : IParameters {
    public EMultiMode MultiMode { get; init; }
    public string? RootFolder { get; init; }
    public string? ProjectFolderOrFile { get; init; }
    public string? DestinationFolder { get; init; }
    public string? SolutionFolderOrFile { get; init; }

    public int ProjectRootRecursionDepth { get; init; } = 5; 

    public bool Copy { get; init; }
    public bool Rescan { get; init; }
    public EFileOperations FileOperations { get; init; }
    public bool AbsPathsInUserCommunication { get; init; }

    public Parameters () { }
    public Parameters (IParameters other) {
      MultiMode = other.MultiMode;
      RootFolder = other.RootFolder;
      ProjectFolderOrFile = other.ProjectFolderOrFile;
      DestinationFolder = other.DestinationFolder;
      SolutionFolderOrFile = other.SolutionFolderOrFile;
      ProjectRootRecursionDepth = other.ProjectRootRecursionDepth;
      Copy = other.Copy;
      Rescan = other.Rescan;
      FileOperations = other.FileOperations;
      AbsPathsInUserCommunication = other.AbsPathsInUserCommunication;
    }

    public static explicit operator Parameters (AppParameters other) => new(other);

  }

  public class AppParameters : IParameters {
    public EMultiMode MultiMode { get; set; }
    public string? RootFolder { get; set; }
    public string? ProjectFolderOrFile { get; set; }
    public string? DestinationFolder { get; set; }
    public string? SolutionFolderOrFile { get; set; }

    public int ProjectRootRecursionDepth { get; set; } = 5;

    public bool Copy { get; set; }
    public bool Rescan { get; set; }
    public EFileOperations FileOperations { get; set; }
    public bool AbsPathsInUserCommunication { get; set; }

    public void From (IParameters other) {
      MultiMode = other.MultiMode;
      RootFolder = other.RootFolder;
      ProjectFolderOrFile = other.ProjectFolderOrFile;
      DestinationFolder = other.DestinationFolder;
      SolutionFolderOrFile = other.SolutionFolderOrFile;
      ProjectRootRecursionDepth = other.ProjectRootRecursionDepth;
      Copy = other.Copy;
      Rescan = other.Rescan;
      FileOperations = other.FileOperations;
      AbsPathsInUserCommunication = other.AbsPathsInUserCommunication;
    }

    public override string ToString () {
      StringBuilder sb = new ();

      sb.AppendLine ($"{nameof (AppParameters)}:");
      sb.AppendLine ($"  {nameof (MultiMode)} = {MultiMode}");
      sb.AppendLine ($"  {nameof (RootFolder)} = {RootFolder}");
      sb.AppendLine ($"  {nameof (ProjectFolderOrFile)} = {ProjectFolderOrFile}");
      sb.AppendLine ($"  {nameof (DestinationFolder)} = {DestinationFolder}");
      sb.AppendLine ($"  {nameof (SolutionFolderOrFile)} = {SolutionFolderOrFile}");
      sb.AppendLine ($"  {nameof (ProjectRootRecursionDepth)} = {ProjectRootRecursionDepth}");
      sb.AppendLine ($"  {nameof (Copy)} = {Copy}");
      sb.AppendLine ($"  {nameof (Rescan)} = {Rescan}");
      sb.AppendLine ($"  {nameof (AbsPathsInUserCommunication)} = {AbsPathsInUserCommunication}");

      return sb.ToString ();
    }

  }
}
