namespace ProjectMover.Lib.Helpers {
  internal sealed record DependentSolutionsAndProjects (
    IReadOnlyList<SolutionFile> Solutions,
    IReadOnlyList<ProjectFile> Projects
  );

  internal sealed record ProjectSelectionGroup (
    ProjectFile Root,
    IReadOnlySet<ProjectFile> Members
  );
  internal sealed record ProjectContextExtra (
    ProjectFile Project,
    //string DesignatedNewProjectFolder,
    IReadOnlyList<ProjectFile> DependentProjects,
    IReadOnlyList<ProjectSelectionGroup>? DependentProjectGroups,
    IReadOnlyList<SolutionFile> DependentSolutions,
    IReadOnlyList<SolutionFile> DependentIsolatedSolutions
  );

  internal sealed record ProjectAndSolutionPlans (
    IReadOnlyList<ProjectOperationPlan> ProjectPlans,
    IReadOnlyList<SolutionOperationPlan> SolutionPlans
  );
  
}
