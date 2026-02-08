namespace ProjectMover.Lib.Steps {
  internal class Step5FileOperations (
    IParameters parameters,
    IProgressSink progress,
    ICallbackSink callback
  ) {

    readonly string _pfx = CsProjectMover.DryRun ? "DRY " : string.Empty;

    public async Task ExecuteAsync (
      ProjectAndSolutionPlans plans,
      CancellationToken ct
    ) {

      using var gd = new ResourceGuard (
        () => progress.BeginStep ("Step 5 of 5: Executing file operations," +
          $" {plans.ProjectPlans.Values.Count()} projects /" +
          $" {plans.SolutionPlans.Values.Count()} solutions ..."),
        () => progress.EndStep ("File operations completed")
      );
      int max = plans.ProjectPlans.Values.Count() * 3 +
                plans.SolutionPlans.Values.Count();
      progress.SetMax (max);

      ct.ThrowIfCancellationRequested ();

      await using IFileOperationContext ctx =
        await FileOperationContextFactory.CreateWithDryRunAwarenessAsync (parameters, ct);
      ctx.MarkCompleted (false);

      await createDirectories (ctx, plans.ProjectPlans.Values, ct);

      if (parameters.Copy)
        await copyProjectsAsync (ctx, plans.ProjectPlans.Values, ct);
      else
        await moveProjectsAsync (ctx, plans.ProjectPlans.Values, ct);

      await writeProjectsAsync (ctx, plans.ProjectPlans.Values, ct);
      await writeSolutionsAsync (ctx, plans.SolutionPlans.Values, ct);

      ctx.MarkCompleted ();
    }

    private async Task createDirectories (
      IFileOperationContext ctx,
      IEnumerable<ProjectOperationPlan> allPlans,
      CancellationToken ct
    ) {
      var plans = allPlans.Where (p => p.NewProjectFolder is not null).ToList ();
      
      int total = plans.Count;
      int index = 0;

      foreach (var plan in plans) {
        ct.ThrowIfCancellationRequested ();

        index++;

        string? dirPath = Path.GetDirectoryName (plan.NewProjectFolder!);
        if (dirPath is null)
          continue;
        progress.Report ($"{_pfx}Create folder {index} of {total}: {dirPath.ToParaPath(parameters)}");
        progress.ReportRel ();
        await ctx.CreateDirectoryAsync (dirPath, ct);
      }

      int delta = allPlans.Count() - plans.Count;
      progress.ReportRel (delta);
    }

    private async Task moveProjectsAsync (
      IFileOperationContext ctx,
      IEnumerable<ProjectOperationPlan> allPlans,
      CancellationToken ct
    ) {
      var plans = allPlans
        .Where (p  => p.NewProjectFolder is not null && 
                !p.OldProjectFolder.Equals (p.NewProjectFolder, StringComparison.OrdinalIgnoreCase))
        .ToList ();

      int index = 0;
      int total = plans.Count;
      foreach (var plan in plans) {
        ct.ThrowIfCancellationRequested ();
        
        index++;     
        progress.Report ($"{_pfx}Move folder {index} of {total} to: {plan.NewProjectFolder?.ToParaPath(parameters)}");
        progress.ReportRel ();

        try {
          await ctx.MoveDirectoryAsync (
            plan.OldProjectFolder,
            plan.NewProjectFolder!,
            ct
          );
        } catch (Exception exc) {
          callback.ShowMessage (ECallback.Error, $"{exc.GetType().Name}: {exc.Message}");
        }
      }
      int delta = allPlans.Count () - plans.Count;
      progress.ReportRel (delta);
    }

    private async Task copyProjectsAsync (
      IFileOperationContext ctx,
      IEnumerable<ProjectOperationPlan> allPlans,
      CancellationToken ct
    ) {
      
      var plans = allPlans.Where (p => p.NewProjectFolder is not null).ToList ();

      int index = 0;
      int total = plans.Count;
      foreach (var plan in plans) {
        ct.ThrowIfCancellationRequested ();

        index++;
        
        progress.Report ($"{_pfx}Copy folder {index} of {total} to: {plan.NewProjectFolder?.ToParaPath (parameters)}");
        progress.ReportRel ();

        await ctx.CopyDirectoryAsync (
          plan.OldProjectFolder,
          plan.NewProjectFolder!,
          ct
        );
      }
      int delta = allPlans.Count () - plans.Count;
      progress.ReportRel (delta);
    }


    private async Task writeProjectsAsync (
      IFileOperationContext ctx,
      IEnumerable<ProjectOperationPlan> plans,
      CancellationToken ct
    ) {
      int index = 0;
      int total = plans.Count ();
      foreach (var plan in plans) {
        ct.ThrowIfCancellationRequested ();

        index++;
        progress.Report ($"{_pfx}Write project {index} of {total}: " + 
          $"{plan.Project.AbsolutePath.ToParaPath (parameters)} " +
          $"{plan.AffectionKindToString ()}");
        progress.ReportRel ();

        await removeOldProjectFileAtNewLocationAsync (ctx, plan, ct);

        await ctx.CreateDirectoryAsync (plan.Project.Directory, ct);

        await ctx.WriteFileAsync (plan.Project.SaveExAsync, ct);
      }
    }

    private static async Task removeOldProjectFileAtNewLocationAsync (
      IFileOperationContext ctx, 
      ProjectOperationPlan plan,
      CancellationToken ct
    ) {
      if (plan.IsUserSelected && !string.Equals (
        plan.NewProjectName,
        plan.OldProjectName,
        StringComparison.OrdinalIgnoreCase)
      ) {
        // old project file at new location
        string oldExt = Path.GetExtension (plan.Project.OrigAbsPath);
        string oldProjName = plan.OldProjectName;
        string oldProjFileAtNewLocation = Path.Combine (plan.Project.Directory, oldProjName + oldExt);
        await ctx.DeleteFileAsync (oldProjFileAtNewLocation, ct);

        if (plan.Project is SharedProjectFile sh) {
          string oldItmsExt = Path.GetExtension (sh.OrigProjItemsAbsPath);
          string oldItmsProjName = Path.GetFileNameWithoutExtension (sh.OrigProjItemsAbsPath);
          string oldItmsProjFileAtNewLocation =
            Path.Combine (plan.Project.Directory, oldItmsProjName + oldItmsExt);
          await ctx.DeleteFileAsync (oldItmsProjFileAtNewLocation, ct);
        }
      }
    }

    private async Task writeSolutionsAsync (
      IFileOperationContext ctx,
      IEnumerable<SolutionOperationPlan> plans,
      CancellationToken ct
    ) {
      int index = 0;
      int total = plans.Count ();
      foreach (var plan in plans) {
        ct.ThrowIfCancellationRequested ();
        
        index++;
        progress.Report ($"{_pfx}Write solution {index} of {total}: {plan.Solution.AbsolutePath.ToParaPath(parameters)}");
        progress.ReportRel ();

        await ctx.WriteFileAsync (plan.Solution.SaveExAsync, ct);
      }
    }
  }

}
