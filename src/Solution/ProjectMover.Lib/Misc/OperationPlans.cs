namespace ProjectMover.Lib.Misc {
  internal abstract class OperationPlan (FileBase fileBase) {
    public string AbsolutePath { get; } = fileBase.OrigAbsPath;
  }


  internal sealed class ProjectOperationPlan (ProjectFile project) : OperationPlan(project) {
    public ProjectFile Project { get; } = project;

    public bool Included { get; set; }

    public bool IsUserSelected { get; set; }
    public bool IsDependency { get; set; }
    public bool IsCopyGroupDependency { get; set; }

    public bool? Copy { get; set; }  // null until known

    public string? NewProjectName { get; set; }
    public string? NewProjectFolder { get; set; }
    public string? NewAssemblyName { get; set; }
    public Guid NewProjectGuid { get; set; }
    public string OldProjectFolder => Project.OrigDirectory;
    public string OldProjectName => Project.OrigName;

    public HashSet<SolutionFile> AffectedSolutions { get; } = [];
    public HashSet<ProjectFile> AffectedDependentProjects { get; } = [];

    public string AffectionKindToString () {
      StringBuilder sb = new ();
      if (IsUserSelected)
        sb.Append ("selected");
      if (IsDependency) {
        if (sb.Length > 0)
          sb.Append (", ");
        sb.Append ("dependent");
      }
      if (IsCopyGroupDependency) {
        if (sb.Length > 0)
          sb.Append (", ");
        sb.Append ("group");
      }
      return $"({sb})";
    }
  }

  internal sealed class SolutionOperationPlan (SolutionFile solution) : OperationPlan(solution){
    public SolutionFile Solution { get; } = solution;
    public HashSet<ProjectFile> AffectedProjects { get; } = [];
  }


}
