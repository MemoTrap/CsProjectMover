namespace ProjectMover.Lib.Steps {
  internal class Step4InMemoryChanges (
    IParameters parameters,
    IProgressSink progress
  ) {
    public void Apply (
      IReadOnlyList<ProjectOperationPlan>? plans
    ) {

      if (plans is null)
        throw new InvalidOperationException ("Project plans are null");
      
      using var guard = new ResourceGuard (
        () => progress.BeginStep ("Step 4 of 5: Applying in-memory changes, " +
          $"{plans.Count} projects ..."),
        () => progress.EndStep ("In-memory updates completed")
      );
      progress.SetMax (plans.Count);

      int total = plans.Count;
      int index = 0;
      foreach (var plan in plans) {
        index++;
        progress.Report ($"Apply to project {index} of {total}: {plan.Project.AbsolutePath.ToParaPath (parameters)}");
        progress.ReportRel ();

        // update own project file first, may alter project path, but not original path 
        plan.Project.ApplyProjectPlan (plan);

        // Update all dependent projects, referencing original project paths 
        foreach (var proj in plan.AffectedDependentProjects) {
          proj.ApplyProjectPlan (plan);
        }
        // Update all affected solutions, referencing original project paths 
        foreach (var sol in plan.AffectedSolutions) {
          sol.ApplyProjectPlan (plan);
        }
      }
    }
  }
}
