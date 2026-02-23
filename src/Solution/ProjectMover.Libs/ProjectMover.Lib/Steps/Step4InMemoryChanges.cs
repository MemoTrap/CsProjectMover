namespace ProjectMover.Lib.Steps {
  internal class Step4InMemoryChanges (
    IParameters parameters,
    IProgressSink progress
  ) {
    public void Apply (
      IReadOnlyDictionary<string, ProjectOperationPlan>? plans
    ) {

      if (plans is null)
        throw new InvalidOperationException ("Proevt plas are null");
      
      using var guard = new ResourceGuard (
        () => progress.BeginStep ("Step 4 of 5: Applying in-memory changes, " +
          $"{plans.Values.Count()} projects ..."),
        () => progress.EndStep ("In-memory updates completed")
      );
      progress.SetMax (plans.Values.Count());

      int total = plans.Count;
      int index = 0;
      foreach (var plan in plans.Values) {
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
