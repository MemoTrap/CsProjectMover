#pragma warning disable IDE0079
#pragma warning disable CA1822
#pragma warning disable CA1859
#pragma warning disable CA1860
#pragma warning disable IDE0305

namespace ProjectMover.Lib.Steps {
  internal class Step3UserInteraction (
    IProgressSink progress,
    ICallbackSink callback,
    IParameters parameters,
    IProjectDecisionProvider decisionProvider,
    IReadOnlyList<ProjectFile> candidateProjects,
    DependentSolutionsAndProjects dependencies
  ) {

    private readonly Dictionary<string, ProjectOperationPlan> _projectPlans =
      new (StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, SolutionOperationPlan> _solutionPlans =
      new (StringComparer.OrdinalIgnoreCase);

    public ProjectAndSolutionPlans? UserInteraction (
      CancellationToken ct
    ) {
      using var guard = new ResourceGuard (
        () => progress.BeginStep ("Step 3 of 5: User interaction started, for " + 
            $"{candidateProjects.Count} cand. projs, " +
            $"{dependencies.Projects.Count} dep. projs, " +
            $"{dependencies.Solutions.Count} dep. slns ..."),
        () => progress.EndStep ("User interaction completed")
      );

      var preselectedProjPaths = userPreselect ();
        
      ct.ThrowIfCancellationRequested ();

      userSelect (preselectedProjPaths, ct);

      if (!_projectPlans.Any () && !_solutionPlans.Any ()) {
        callback.ShowMessage (ECallback.Information, "No projects selected. Done.");
        return null;
      }

      var plans = new ProjectAndSolutionPlans (_projectPlans, _solutionPlans);
      bool proceed = callbackConfirm (plans);
      if (!proceed)
        return null;

      return plans;
    }


    private IEnumerable<string>? userPreselect () {
      var projectPaths = candidateProjects
        .Select (p => p.AbsolutePath.ToParaPath(parameters))
        .ToList ();

      // preselect not needed if only one project
      if (projectPaths.Count == 1)
        return null;

      // preselect option, particularly for GUI frontends
      var preselected = decisionProvider.PreSelect (projectPaths);
      if (preselected is null)
        return null;
      
      preselected = preselected
        .Select (p => p.ToAbsolutePath(parameters.RootFolder!))
        .ToList ();
      
      return preselected;
    }

    private void userSelect (IEnumerable<string>? preselectedProjPaths, CancellationToken ct) {
      HashSet<string> designatedFolderNames = new (StringComparer.OrdinalIgnoreCase);

      bool preselected = preselectedProjPaths is not null && preselectedProjPaths.Any ();

      var projects = candidateProjects;
      if (preselected)
        projects = candidateProjects
          .Where (p => preselectedProjPaths!.Contains (p.AbsolutePath))
          .ToList ();

      int index = 0;
      int total = projects.Count;
      foreach (var project in projects) {

        ct.ThrowIfCancellationRequested ();

        index++;
        var (context, extraContext) = buildContext (
          index,
          total,
          project,
          preselected
        );

        ProjectUserDecision decision;
        while (true) {
          decision = decisionProvider.Decide (context);
          if (!decision.Include)
            break;

          string? errorMsg = validateDecision (context, extraContext, decision, designatedFolderNames);
          if (errorMsg is null) {
            if (isNullOp (context, decision)) {
              // treat as skip
              decision = decision with {
                Include = false
              };
            }
            break;
          }
          context = context with {
            RetryReason = errorMsg
          };
        }

        if (!decision.Include)
          continue;

        applyDecisionToCreateOperationPlan (context, extraContext, decision);
      }
    }

    private (ProjectDecisionContext ctx, ProjectContextExtra) buildContext (
      int indexInBatch,
      int totalInBatch,
      ProjectFile project,
      bool preselected
    ) {

      // Find referencing projects
      var referencingProjects = dependencies.Projects
          .Where (p =>
              p.ProjectReferencesAbsPath.Contains (project.AbsolutePath, StringComparer.OrdinalIgnoreCase)
              || (project is SharedProjectFile sh &&
                  p.SharedProjectImportsAbsPath.Contains (sh.ProjItemsAbsPath, StringComparer.OrdinalIgnoreCase))
          )
          .ToList ();

      ProjectDependencyGraphBuilder? graph = null;
      IReadOnlyList<ProjectSelectionGroup>? groups = null;

      if (parameters.Copy) {
        graph = new ProjectDependencyGraphBuilder ();
        graph.Build (project, referencingProjects);

        groups = graph.BuildSelectionGroups (project);
      }

      var selectableDependentRoots =
          groups?
            .Select (g => g.Root.AbsolutePath)
            .ToList ()
          ?? [];


      // Find solutions using this project
      var usingSolutions = dependencies.Solutions
        .Where (s => s.ContainsProject (project.AbsolutePath))
        .ToList ();

      var groupMembers =
          groups?
              .SelectMany (g => g.Members)
              .Select (p => p.AbsolutePath)
              .ToHashSet (StringComparer.OrdinalIgnoreCase)
          ?? [];

      var solutionsWithTargetOnly = usingSolutions
        .Where (s =>
            s.ContainsProject (project.AbsolutePath) &&
              !s.ProjectsAbsPath.Any (p => groupMembers.Contains (p)))
        .ToList ();

      var solutionPathsWithTargetOnly = solutionsWithTargetOnly
        .Select (s => s.AbsolutePath)
        .ToList ();

      string designatedNewFolder;
      if (parameters.MultiMode.HasFlag (EMultiMode.Projects))
        designatedNewFolder = project.Directory.ToNewDestinationPath (parameters.ProjectFolderOrFile!, parameters.DestinationFolder!);
      else
        designatedNewFolder = Path.Combine (parameters.DestinationFolder!, project.Directory.GetLastDirectoryName ()!);

      var (curAssName, newAssName) = getAssemblyName (project);

      var context = new ProjectDecisionContext (
        Preselected: preselected,
        ProjectType: project.ProjectType,
        CopyModeHint: parameters.Copy,
        IndexInBatch: indexInBatch,
        TotalInBatch: totalInBatch,
        RetryReason: null,
        ProjectName: project.Name,
        CurrentProjectFolder: project.Directory.ToParaPath(parameters),
        CurrentAssemblyName: curAssName,
        SuggestedAssemblyName: newAssName,
        DesignatedNewProjectFolder: designatedNewFolder.ToParaPath(parameters),
        RootFolder: parameters.RootFolder!,
        SelectableSolutions: solutionPathsWithTargetOnly
          .Select (p => p.ToParaPath(parameters))
          .ToArray(),
        SelectableDependentProjectRoots: selectableDependentRoots
          .Select (p => p.ToParaPath(parameters))
          .ToArray()
      );

      var extraContext = new ProjectContextExtra (
        Project: project,
        DependentProjects: referencingProjects,
        DependentProjectGroups: groups,
        DependentSolutions: usingSolutions,
        DependentIsolatedSolutions: solutionsWithTargetOnly);
      return (context, extraContext);
    }

    private (string? curAssName, string? newAssName) getAssemblyName (ProjectFile project) {
      if (project is not CsProjectFile csProj)
        return (null, null);
      string? curAssName =
        csProj.AssemblyName;

      if (!parameters.Copy)
        return (curAssName, null);

      curAssName ??= csProj.Name;
      string newAssName = curAssName + "." + DateTime.Now.ToAssemblySuffix ();
      return (curAssName, newAssName);
    }

    private bool isNullOp (
      ProjectDecisionContext context,
      ProjectUserDecision decision
    ) {
      bool sameProjectName =
        string.Equals (
          decision.NewProjectName ?? context.ProjectName,
          context.ProjectName,
          StringComparison.OrdinalIgnoreCase
        );
      bool sameProjectFolder = 
        string.Equals (
          decision.NewProjectFolder ?? context.DesignatedNewProjectFolder,
          context.CurrentProjectFolder,
          StringComparison.OrdinalIgnoreCase
        );
      return sameProjectName && sameProjectFolder;
    }


    private string? validateDecision (
      ProjectDecisionContext context,
      ProjectContextExtra extraContext,
      ProjectUserDecision decision,
      ISet<string> designatedFolderNames
    ) {
      string? errorMsg = null;

      context = context.ToAbsolutePath (parameters);
      decision = decision.ToAbsolutePath (parameters);

      if (decision.NewProjectName is not null) {
        if (!decision.NewProjectName.Trim().IsValidFileOrFolderName ())
          errorMsg = $"Designated project name {decision.NewProjectName} is not a valid file or folder name.";
      }
      if (errorMsg != null)
        return errorMsg;

      if (decision.NewProjectFolder is not null) {
        if (!decision.NewProjectFolder.Trim().IsValidPath ())
          errorMsg = $"Designated folder {decision.NewProjectFolder.ToParaPath(parameters)} is not a valid path.";
      }
      if (errorMsg != null)
        return errorMsg;

      string newProjectFolder = decision.NewProjectFolder?.Trim() ?? context.DesignatedNewProjectFolder;

      // NOT CURRENTLY SUPPORTED : Moving/Copying into an existing folder

      // designated folder must not exist yet, unless it's the old folder
      if (!context.CurrentProjectFolder.Equals (newProjectFolder, StringComparison.OrdinalIgnoreCase) &&
      Directory.Exists (newProjectFolder))
        errorMsg = $"Designated folder {newProjectFolder.ToParaPath (parameters)} already exists.";
      if (errorMsg != null)
        return errorMsg;

      // and must not conflict with other designated folders
      bool ok = !designatedFolderNames.Contains (newProjectFolder);
      if (!ok)
        errorMsg = $"Designated folder {newProjectFolder.ToParaPath (parameters)} conflicts with another project folder.";
      if (errorMsg != null)
        return errorMsg;


      // designated folder must be a sub-folder of root folder
      if (!newProjectFolder.IsSubPathOf (parameters.RootFolder!))
        errorMsg =
          $"Designated folder {newProjectFolder.ToParaPath (parameters)} " +
          $"must be a sub-folder of root folder {parameters.RootFolder}";
      if (errorMsg != null)
        return errorMsg;

      // copy mode and assembly
      if (context.CopyModeHint && decision.NewAssemblyName is not null) {
        string newProjName = decision.NewProjectName ?? context.ProjectName;
        if (string.Equals (newProjName, context.ProjectName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals (newProjName, decision.NewAssemblyName, StringComparison.OrdinalIgnoreCase))
          errorMsg = $"Designated new assembly name {decision.NewAssemblyName} must be different from unchanged project name.";
        if (errorMsg != null)
          return errorMsg;
      }

      if (errorMsg is null)
        designatedFolderNames.Add (newProjectFolder);

      return errorMsg;
    }

    private void applyDecisionToCreateOperationPlan (
      ProjectDecisionContext context,
      ProjectContextExtra extraContext,
      ProjectUserDecision decision
    ) {
      context = context.ToAbsolutePath (parameters);
      decision = decision.ToAbsolutePath (parameters);

      var project = extraContext.Project;
      ProjectOperationPlan plan = getOrCreateProjectPlan (project);

      plan.Included = decision.Include;
      plan.IsUserSelected = true;
      plan.Copy = parameters.Copy;

      plan.NewProjectName = decision.NewProjectName ?? context.ProjectName;
      plan.NewProjectFolder = decision.NewProjectFolder ?? context.DesignatedNewProjectFolder;
      plan.NewAssemblyName = decision.NewAssemblyName ?? context.SuggestedAssemblyName;
      if (!parameters.Copy && context.CurrentAssemblyName is null &&
          string.Equals (plan.NewAssemblyName, plan.NewProjectName)) {
        plan.NewAssemblyName = null;
      }

      if (parameters.Copy) {
        //if (!(project is CsProjectFile csp && csp.IsSdkStyle))
        // Always new GUID in copy mode
        // C# SDK projects shall ignore it
        // Solutions shall apply the new GUID
        plan.NewProjectGuid = Guid.NewGuid ();

        if (extraContext.DependentProjectGroups is null)
          throw new InvalidOperationException ("Dependent project groups have not been built");
        
        // the actually dependent projects are the group roots plus all their members
        // if selected by the user
        var groups = extraContext.DependentProjectGroups
          .Where (g => decision.SelectedDependentProjectRoots?.Contains(g.Root.AbsolutePath) ?? false)
          .ToList ();

        HashSet<ProjectFile> affectedProjects = [];
        foreach (var group in groups) {
          foreach (var proj in group.Members)
            affectedProjects.Add (proj);
        }
        addAffectedProjectsToPlan (plan, affectedProjects, true);

        var affectedProjectPaths = affectedProjects.Select(p => p.AbsolutePath).ToList ();

        // the selected dependent projects define the affected solutions
        var affectedSolutionsBySelectedDepProjects = extraContext.DependentSolutions
          .Where (s => s.ProjectsAbsPath.Any (p => affectedProjectPaths.Contains (p)))
          .ToList ();
        addAffectedSolutionsToPlan (project, plan, affectedSolutionsBySelectedDepProjects);

        // plus any isolated solutions selected by the user
        var affectedSolutionsByExplicSelection = extraContext.DependentSolutions
          .Where (s => decision.SelectedSolutions?.Contains(s.AbsolutePath) ?? false)
          .ToList ();
        addAffectedSolutionsToPlan (project, plan, affectedSolutionsByExplicSelection);



      } else {
        // all dependent solutions will be affected
        addAffectedSolutionsToPlan (project, plan, extraContext.DependentSolutions);

        // all dependent projects will be affected
        addAffectedProjectsToPlan (plan, extraContext.DependentProjects);
      }
    }

    private void addAffectedSolutionsToPlan (
      ProjectFile project, 
      ProjectOperationPlan plan,
      IEnumerable<SolutionFile> solutions
    ) {
      foreach (var sol in solutions) {
        plan.AffectedSolutions.Add (sol);
        getOrCreateSolutionPlan (sol)
          .AffectedProjects.Add (project);
      }
    }

    private void addAffectedProjectsToPlan (
      ProjectOperationPlan plan, 
      IEnumerable<ProjectFile> projects,
      bool isCopyGroup = false
    ) {
      foreach (var depProj in projects) {
        plan.AffectedDependentProjects.Add (depProj);
        var depPlan = getOrCreateProjectPlan (depProj);
        depPlan.Included = true;
        depPlan.IsDependency = true;
        depPlan.IsCopyGroupDependency = isCopyGroup;
      }
    }


    private SolutionOperationPlan getOrCreateSolutionPlan (SolutionFile solution) =>
      _solutionPlans.GetOrAdd (solution.AbsolutePath, _ => new SolutionOperationPlan (solution));

    private ProjectOperationPlan getOrCreateProjectPlan (ProjectFile project) =>
      _projectPlans.GetOrAdd (project.AbsolutePath, _ => new ProjectOperationPlan (project));

    private bool callbackConfirm (ProjectAndSolutionPlans plans) {
      const int MAX_DISPLAY = 20;

      StringBuilder sb = new ();

      string action = parameters.Copy ? "Copy" : "Move/rename";

      sb.AppendLine ($"{action} operation summary");
      sb.AppendLine ();
      sb.AppendLine (affectedProjects());
      //sb.AppendLine ();
      sb.AppendLine (affectedSolutions ());
      //sb.AppendLine ();
      sb.Append ("Proceed?");

      var result = callback.ShowMessage (
        ECallback.Question,
        sb.ToString (),
        "Confirmation",
        EMsgBtns.YesNo,
        EDefaultMsgBtn.Button1,
        true
      );

      bool proceed = result is EDialogResult.Yes or EDialogResult.OK;
      return proceed;

      string affectedProjects () {
        StringBuilder sbProj = new ();

        var projPlans = plans.ProjectPlans.Values
          .Where (p => p.Included)
          .ToList ();
        
        sbProj.AppendLine ($"{projPlans.Count} projects affected:");

        int count = 0;
        foreach (var plan in projPlans) {
          count++;
          if (count > MAX_DISPLAY)
            break;
          
          sbProj.AppendLine (
            $"- {plan.Project.OrigAbsPath.ToParaPath(parameters)} " +
            $"{plan.AffectionKindToString()}"
          );
        }
        if (count > MAX_DISPLAY)
          sbProj.AppendLine ($"... and {count - MAX_DISPLAY} more projects.");

        return sbProj.ToString();
      }

      string affectedSolutions () {
        StringBuilder sbSol = new ();
        var solPlans = plans.SolutionPlans.Values;
        int nSolPlans = solPlans.Count ();
        sbSol.AppendLine ($"{nSolPlans} solutions affected{(nSolPlans > 0 ? ":" : null)}");
        int count = 0;
        foreach (var plan in solPlans) {
          count++;
          if (count > MAX_DISPLAY)
            break;
          
          sbSol.AppendLine (
            $"- {plan.Solution.AbsolutePath.ToParaPath (parameters)}");
        }
        if (count > MAX_DISPLAY)
          sbSol.AppendLine ($"... and {count - MAX_DISPLAY} more solutions.");

        return sbSol.ToString();
      }
    }
  }

    
}
