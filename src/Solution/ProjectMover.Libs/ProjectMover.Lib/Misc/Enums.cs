namespace ProjectMover.Lib.Misc {
  [Flags]
  public enum EMultiMode {
    None = 0,
    Projects = 1,
    Solutions = 2,
  }

  public enum EFileOperations {
    Direct,
    Svn,
    Git
  }

  public enum EProjectType {
    Unknown,
    CSharp,
    Shared,
    Folder
  }
}
