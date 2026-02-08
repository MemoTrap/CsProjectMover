namespace ProjectMover.Lib.Api {

  public record ProjectUserDecision (
    bool Include,                 // false → project is skipped entirely
    string? NewProjectName,       // null → keep original
    string? NewProjectFolder,     // null → keep original
    string? NewAssemblyName,      // null if unchanged (copy only)
    IReadOnlyList<string>? SelectedSolutions,             // copy only: absolute solution paths
    IReadOnlyList<string>? SelectedDependentProjectRoots  // copy only: absolute project paths
  ) {
    public ProjectUserDecision () : this (false, null, null, null, null, null) { }
  }

  public record ProjectDecisionContext (
    bool Preselected,                   // The project has already been preselected
    EProjectType ProjectType,           // type of the project being decided upon
    bool CopyModeHint,                  // true → project subject to being copied, false → moved
    int IndexInBatch,                   // one-based index of the project in the current batch
    int TotalInBatch,                   // total number of projects in the current batch
    string? RetryReason,                // null if first attempt, otherwise a user-readable reason for retry
    string ProjectName,                 // name of the project (without path or extension)
    string CurrentProjectFolder,        // absolute or relative path to current project folder
    string? CurrentAssemblyName,        // set to project name if not specified
    string? SuggestedAssemblyName,      // copy only, suggested unique-enough name for easy handling 
    string DesignatedNewProjectFolder,  // absolute or relative path to designated new project folder
    string RootFolder,                  // absolute path to root folder
    IReadOnlyList<string> SelectableSolutions,             // copy only: absolute solution paths
    IReadOnlyList<string> SelectableDependentProjectRoots  // copy only: absolute project paths
  );

  public interface IProjectDecisionProvider {
    IEnumerable<string>? PreSelect (IEnumerable<string> projectPaths);
    ProjectUserDecision Decide (ProjectDecisionContext context);
  }

}
