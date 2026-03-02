#pragma warning disable IDE0079
#pragma warning disable CA1822
#pragma warning disable CA1859
#pragma warning disable CA1860
#pragma warning disable IDE0305

using static ProjectMover.Lib.Helpers.ProjectDependencyGraph;

namespace ProjectMover.Lib.Steps {
  internal class Step3UserInteraction {

    private readonly IProgressSink _progress;
    private readonly ICallbackSink _callback;
    private readonly IParameters _parameters;
    private readonly IProjectDecisionProvider _decisionProvider;
    private readonly IReadOnlyList<ProjectFile> _candidateProjects;
    private readonly DependentSolutionsAndProjects _dependencies;

    private readonly Dictionary<ProjectFile, ProjectOperationPlan> _projectPlans =
      [];
    private readonly Dictionary<SolutionFile, SolutionOperationPlan> _solutionPlans =
      [];

    private int[]? _preselectedIndexesCache = null;
    IEnumerable<string>? _preselectedAbsProjPaths = null;
    private readonly Dictionary<ProjectFile, ProjectUserDecision> _projectDecisonCache =
      [];
    private readonly HashSet<SolutionFile> _selectedSolutionsCache = [];

    private readonly DependencyPropagation _dependencyPropagation;
    private readonly DestinationPathRule _destinationRule;
    
    private readonly IReadOnlyList<ProjectFile> _allProjects;
    private readonly IReadOnlyList<SolutionFile> _allSolutions;

    public Step3UserInteraction (
      IProgressSink progress,
      ICallbackSink callback,
      IParameters parameters,
      IProjectDecisionProvider decisionProvider,
      IReadOnlyList<ProjectFile> candidateProjects,
      DependentSolutionsAndProjects dependencies
  ) {
      _progress = progress;
      _callback = callback;
      _parameters = parameters;
      _decisionProvider = decisionProvider;
      _candidateProjects = candidateProjects;
      _dependencies = dependencies;

      _allProjects = _candidateProjects.Union (_dependencies.Projects).ToList ();
      _allSolutions = _dependencies.Solutions.ToList ();

      _destinationRule = new DestinationPathRule (_parameters.DestinationFolder!);
      _dependencyPropagation = new DependencyPropagation (candidateProjects, dependencies);
    }


    public ProjectAndSolutionPlans? UserInteraction (
      CancellationToken ct
    ) {
      using var guard = new ResourceGuard (
        () => _progress.BeginStep ("Step 3 of 5: User interaction started, for " +
            $"{_candidateProjects.Count} cand. projs, " +
            $"{_dependencies.Projects.Count} dep. projs, " +
            $"{_dependencies.Solutions.Count} dep. slns ..."),
        () => _progress.EndStep ("User interaction completed")
      );

      bool withPreselect = true;

      while (true) {
        ct.ThrowIfCancellationRequested ();

        if (withPreselect) 
          fetchPreselectionsFromUser ();

        withPreselect = _preselectedAbsProjPaths?.Any() ?? false;

        ct.ThrowIfCancellationRequested ();

        bool proceed = fetchProjectDecisonsFromUserAndApply (ct);
        if (!proceed)
          continue;

        if (!_projectPlans.Any () && !_solutionPlans.Any ()) {
          _callback.ShowMessage (ECallback.Information, "No projects selected. Done.");
          return null;
        }

        reviewPlansToRemoveAllegedlyAffectedSolutions ();
        
        var plans = new ProjectAndSolutionPlans (
          _projectPlans.Values.ToList(), 
          _solutionPlans.Values.ToList());
        proceed = callbackFinalConfirm (plans);
        if (!proceed)
          return null;

        return plans;
      }
    }


    private void fetchPreselectionsFromUser () {
      var projectPaths = _candidateProjects
        .Select (p => p.AbsolutePath.ToParaPath (_parameters))
        .ToList ();

      // preselect not needed if only one project
      if (projectPaths.Count == 1)
        return;

      // preselect option, particularly for GUI frontends
      var preselected = _decisionProvider.PreSelect (projectPaths, _preselectedIndexesCache);
      if (preselected is null)
        return;

      // which have been preselected in this iteration, for potential use in the next iteration
      _preselectedIndexesCache = findIndexes (projectPaths, preselected);

      _preselectedAbsProjPaths = preselected
        .Select (p => p.ToAbsolutePath (_parameters.RootFolder!))
        .ToList ();

    }

    private int[]? findIndexesSln (IReadOnlyList<string> allPaths, IEnumerable<string>? selectedPaths) {
      List<string> combinedPreselect = [];
      
      // from the previous decision
      if (selectedPaths is not null)
        combinedPreselect = combinedPreselect.Union (selectedPaths, StringComparer.OrdinalIgnoreCase).ToList();

      // from all previous decisions
      var selectedSlnPaths = _selectedSolutionsCache
        .Select (s => s.AbsolutePath.ToParaPath (_parameters))
        .ToList ();
      combinedPreselect = combinedPreselect.Union (selectedSlnPaths, StringComparer.OrdinalIgnoreCase).ToList();

      return findIndexes (allPaths, combinedPreselect);
    }

    private int[]? findIndexesProj (IReadOnlyList<string> allPaths, IEnumerable<string>? selectedPaths) {
      List<string> combinedPreselect = [];
      
      // from the previous decision
      if (selectedPaths is not null)
        combinedPreselect = combinedPreselect.Union (selectedPaths, StringComparer.OrdinalIgnoreCase).ToList();

      // from preselect
      if (_preselectedAbsProjPaths is not null) {
        var preselectedPaths = _preselectedAbsProjPaths
          .Select (p => p.ToParaPath (_parameters))
          .ToList ();
        combinedPreselect = combinedPreselect.Union (preselectedPaths, StringComparer.OrdinalIgnoreCase).ToList();
      }

      // from all previous decisions = created project plans 
      combinedPreselect = combinedPreselect
        .Union (getParaPaths(_projectPlans.Keys), StringComparer.OrdinalIgnoreCase)
        .ToList ();

      return findIndexes (allPaths, combinedPreselect);
    }

    private static int[]? findIndexes (IReadOnlyList<string> allPaths, IEnumerable<string>? selectedPaths) {
      if (selectedPaths is null)
        return null;
      int[] indexes = selectedPaths
        .Select (p => {
          for (int i = 0; i < allPaths.Count; i++) {
            if (string.Equals (
                    allPaths[i],
                    p,
                    StringComparison.OrdinalIgnoreCase))
              return i;
          }

          return -1;
        })
        .ToArray ();
      return indexes;
    }

    private bool fetchProjectDecisonsFromUserAndApply (CancellationToken ct) {
      HashSet<string> designatedFolderNames = new (StringComparer.OrdinalIgnoreCase);

      bool preselected = _preselectedAbsProjPaths is not null && _preselectedAbsProjPaths.Any ();

      var projects = _candidateProjects;
      if (preselected)
        projects = _candidateProjects
          .Where (p => _preselectedAbsProjPaths!.Contains (p.AbsolutePath))
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
          decision = _decisionProvider.Decide (context);
          if (!decision.Include)
            break;

          _projectDecisonCache[project] = decision;

          string? errorMsg = validateDecision (context, decision, designatedFolderNames);
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

        bool proceed = applyDecisionToCreateOperationPlan (context, extraContext, decision, preselected);
        if (!proceed)
          return false;
      }

      return true;
    }

    private (ProjectDecisionContext ctx, ProjectContextExtra) buildContext (
      int indexInBatch,
      int totalInBatch,
      ProjectFile project,
      bool preselected
    ) {

      // Find referencing projects
      var referencingProjects = _dependencies.Projects
          .Where (p =>
              p.ProjectReferencesAbsPath.Contains (project.AbsolutePath, StringComparer.OrdinalIgnoreCase)
              || (project is SharedProjectFile sh &&
                  p.SharedProjectImportsAbsPath.Contains (sh.ProjItemsAbsPath, StringComparer.OrdinalIgnoreCase))
          )
          .ToList ();

      IReadOnlyList<ProjectSelectionGroup>? groups = null;

      if (_parameters.Copy) {
        groups = _dependencyPropagation.BuildSelectionGroups (project, ERefKind.ReferencedBy);
      }

      var selectableDependentRoots =
          groups?
            .Select (g => g.Root.AbsolutePath)
            .ToList ()
          ?? [];


      // Find solutions using this project
      var usingSolutions = _dependencies.Solutions
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

      string source = project.Directory;
      string designatedNewFolder = _destinationRule.Apply (source);


      var (curAssName, newAssName) = getAssemblyName (project);

      var prevDecision = _projectDecisonCache.GetValueOrDefault (project);

      var slnPaths = solutionPathsWithTargetOnly
          .Select (p => p.ToParaPath (_parameters))
          .ToArray ();
      var projPaths = selectableDependentRoots
          .Select (p => p.ToParaPath (_parameters))
          .ToArray ();


      var context = new ProjectDecisionContext (
        Preselected: preselected,
        ProjectType: project.ProjectType,
        CopyModeHint: _parameters.Copy,
        IndexInBatch: indexInBatch,
        TotalInBatch: totalInBatch,
        RetryReason: null,
        ProjectName: prevDecision?.NewProjectName ?? project.Name,
        CurrentProjectFolder: project.Directory.ToParaPath (_parameters),
        CurrentAssemblyName: curAssName,
        SuggestedAssemblyName: prevDecision?.NewAssemblyName ?? newAssName,
        DesignatedNewProjectFolder: designatedNewFolder.ToParaPath (_parameters),
        RootFolder: _parameters.RootFolder!,
        SelectableSolutions: slnPaths,
        SelectableDependentProjectRoots: projPaths,
        SolutionPreset: findIndexesSln (slnPaths, prevDecision?.SelectedSolutions),
        DependentProjectRootsPreset: findIndexesProj (projPaths, prevDecision?.SelectedDependentProjectRoots)
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

      if (!_parameters.Copy)
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
      ProjectUserDecision decision,
      ISet<string> designatedFolderNames
    ) {
      string? errorMsg = null;

      context = context.ToAbsolutePath (_parameters);
      decision = decision.ToAbsolutePath (_parameters);

      if (decision.NewProjectName is not null) {
        if (!decision.NewProjectName.Trim().IsValidFileOrFolderName ())
          errorMsg = $"Designated project name {decision.NewProjectName} is not a valid file or folder name.";
      }
      if (errorMsg != null)
        return errorMsg;

      if (decision.NewProjectFolder is not null) {
        if (!decision.NewProjectFolder.Trim().IsValidPath ())
          errorMsg = $"Designated folder {decision.NewProjectFolder.ToParaPath(_parameters)} is not a valid path.";
      }
      if (errorMsg != null)
        return errorMsg;

      string newProjectFolder = decision.NewProjectFolder?.Trim() ?? context.DesignatedNewProjectFolder;

      // NOT CURRENTLY SUPPORTED : Moving/Copying into an existing folder

      // designated folder must not exist yet, unless it's the old folder
      if (!context.CurrentProjectFolder.Equals (newProjectFolder, StringComparison.OrdinalIgnoreCase) &&
      Directory.Exists (newProjectFolder))
        errorMsg = $"Designated folder {newProjectFolder.ToParaPath (_parameters)} already exists.";
      if (errorMsg != null)
        return errorMsg;

      // and must not conflict with other designated folders
      bool ok = !designatedFolderNames.Contains (newProjectFolder);
      if (!ok)
        errorMsg = $"Designated folder {newProjectFolder.ToParaPath (_parameters)} conflicts with another project folder.";
      if (errorMsg != null)
        return errorMsg;


      // designated folder must be a sub-folder of root folder
      if (!newProjectFolder.IsSubPathOf (_parameters.RootFolder!))
        errorMsg =
          $"Designated folder {newProjectFolder.ToParaPath (_parameters)} " +
          $"must be a sub-folder of root folder {_parameters.RootFolder}";
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

    private bool applyDecisionToCreateOperationPlan (
      ProjectDecisionContext context,
      ProjectContextExtra extraContext,
      ProjectUserDecision decision,
      bool preselected
    ) {
      context = context.ToAbsolutePath (_parameters);
      decision = decision.ToAbsolutePath (_parameters);

      var project = extraContext.Project;

      _destinationRule.Adapt (decision.NewProjectFolder);

      ProjectOperationPlan plan = getOrCreateProjectPlan (project);

      plan.Included = decision.Include;
      plan.IsUserSelected = true;
      plan.Copy = _parameters.Copy;

      plan.NewProjectName = decision.NewProjectName ?? context.ProjectName;
      plan.NewProjectFolder = decision.NewProjectFolder ?? context.DesignatedNewProjectFolder;
      plan.NewAssemblyName = decision.NewAssemblyName ?? context.SuggestedAssemblyName;
      if (!_parameters.Copy && context.CurrentAssemblyName is null &&
          string.Equals (plan.NewAssemblyName, plan.NewProjectName)) {
        plan.NewAssemblyName = null;
      }

      if (_parameters.Copy) {
        // Always new GUID in copy mode
        // C# SDK projects shall ignore it
        // Solutions shall apply the new GUID
        plan.NewProjectGuid = Guid.NewGuid ();


        bool proceed = findAllCopyAffectedProjectsAndSolutions (
          plan,
          extraContext,
          decision,
          preselected
          //out var affectedProjects,
          //out var affectedSolutions
        );

        return proceed;

      } else {
        var affectedSolutions = 
          extraContext.DependentSolutions.ToList();

        // all dependent solutions will be affected
        addAffectedSolutionsToPlan (plan, affectedSolutions);

        // all dependent projects will be affected
        addAffectedProjectsToPlan (plan, extraContext.DependentProjects);
      }

      return true;
    }

    private bool findAllCopyAffectedProjectsAndSolutions (
      ProjectOperationPlan plan,
      ProjectContextExtra extraContext, 
      ProjectUserDecision decision, 
      bool preselected
    ) {

      var decisionProjects = extraContext.DependentProjects
        .Where (dp => decision.SelectedDependentProjectRoots?.Contains(
          dp.AbsolutePath, StringComparer.OrdinalIgnoreCase
         ) ?? false)
        .ToList ();
      var decisionSolutions = extraContext.DependentSolutions
        .Where (ds => decision.SelectedSolutions?.Contains(
          ds.AbsolutePath, StringComparer.OrdinalIgnoreCase
         ) ?? false)
        .ToList ();

      HashSet<ProjectFile> affectedProjects = decisionProjects.ToHashSet (); ;
      HashSet<SolutionFile> affectedSolutions = decisionSolutions.ToHashSet ();
      
      var (propagatedProjects, propagatedSolutions) =
        _dependencyPropagation.ComputeCopyPropagation (
          extraContext.Project,
          affectedProjects,
          affectedSolutions
        );


      var addedProjects = propagatedProjects
        .Except (affectedProjects).ToList ();
      var addedSolutions = propagatedSolutions
        .Except (affectedSolutions).ToList ();

      bool proceed = callbackDependencyPropagationsConfirm (
        extraContext.Project, 
        propagatedProjects, 
        propagatedSolutions, 
        preselected
      );

      if (proceed) {

        affectedProjects.UnionWith (propagatedProjects); 
        affectedSolutions.UnionWith (propagatedSolutions);

        addAffectedProjectsToPlan (
          plan,
          affectedProjects,
          decisionProjects,
          true);
        
        addAffectedSolutionsToPlan (
          plan, 
          affectedSolutions, 
          decisionSolutions);


        // identify user selected solution dependencies for cache
        var selectedSolutions = affectedSolutions
          .Where (s => decisionSolutions?
            .Contains (s) ?? false)
          .ToList ();
        _selectedSolutionsCache.UnionWith (selectedSolutions);


      }
        
      return proceed;

    }

    private void reviewPlansToRemoveAllegedlyAffectedSolutions () {
      // Copy mode only! In move mode all affected solutions are required.
      if (!_parameters.Copy)
        return;


      // Affected solutions in plans that have not been selected
      var unselectedSolutions = _solutionPlans.Values
        .Where (p => !p.IsUserAppliedDependency)
        .Select (p => p.Solution)
        .ToList ();


      foreach (var solution in unselectedSolutions) {
        // all referenced projects in a solution
        var referencedProjects = 
          _dependencyPropagation.GetReferencedProjects (solution);

        // Find the project plans where these solutions are referenced
        var referencedProjectPlans = _projectPlans.Values
          .Where (p => referencedProjects.Contains (p.Project)) 
          .ToList ();

        // If all referencing project plans are selected projects then remove solution from all 
        bool areAllSelected = referencedProjectPlans
          .All (p => p.IsUserSelected /*|| p.IsUserAppliedDependency*/);
        if (areAllSelected) {
          _solutionPlans.Remove (solution);

          foreach (var projPlan in referencedProjectPlans)
            projPlan.AffectedSolutions.Remove (solution);
        }
      }
    }


    private void addAffectedSolutionsToPlan (
      ProjectOperationPlan projPlan,
      IEnumerable<SolutionFile> affectedSolutions,
      IEnumerable<SolutionFile>? decisionSolutions = null
    ) {
      foreach (var sln in affectedSolutions) {
        projPlan.AffectedSolutions.Add (sln);

        bool isUserAppliedDependency = decisionSolutions?.Contains (sln) ?? false;
        var slnPlan = getOrCreateSolutionPlan (sln);
        slnPlan.AffectedProjects.Add (projPlan.Project);
          if (isUserAppliedDependency)
            slnPlan.IsUserAppliedDependency = true;
        
      }
    }

    private void addAffectedProjectsToPlan (
      ProjectOperationPlan plan, 
      IEnumerable<ProjectFile> affectedProjects,
      IEnumerable<ProjectFile>? decisionProjects = null,
      bool isCopyGroup = false
    ) {
      foreach (var depProj in affectedProjects) {
        plan.AffectedDependentProjects.Add (depProj);
        
        bool isUserAppliedDependency = decisionProjects?.Contains (depProj) ?? false;
        var depPlan = getOrCreateProjectPlan (depProj);
        depPlan.Included = true;
        depPlan.IsDependency = true;
        if (isCopyGroup)
          depPlan.IsCopyGroupDependency = true;
        if (isUserAppliedDependency)
          depPlan.IsUserAppliedDependency = true;
      }
    }


    private SolutionOperationPlan getOrCreateSolutionPlan (SolutionFile solution) =>
      _solutionPlans.GetOrAdd (solution, _ => new SolutionOperationPlan (solution));

    private ProjectOperationPlan getOrCreateProjectPlan (ProjectFile project) =>
      _projectPlans.GetOrAdd (project, _ => new ProjectOperationPlan (project));

    private bool callbackFinalConfirm (ProjectAndSolutionPlans plans) {
      const int MAX_DISPLAY = 20;

      StringBuilder sb = new ();

      string action = _parameters.Copy ? "Copy" : "Move/rename";

      sb.AppendLine ($"{action} operation summary");
      sb.AppendLine ();
      sb.AppendLine (affectedProjects());
      //sb.AppendLine ();
      sb.AppendLine (affectedSolutions ());
      //sb.AppendLine ();
      sb.Append ("Proceed?");

      var result = _callback.ShowMessage (
        ECallback.Question,
        sb.ToString (),
        "Final confirmation",
        EMsgBtns.YesNo,
        EDefaultMsgBtn.Button1,
        true
      );

      bool proceed = result is EDialogResult.Yes or EDialogResult.OK;
      return proceed;

      string affectedProjects () {
        StringBuilder sbProj = new ();

        var projPlans = plans.ProjectPlans
          .Where (p => p.Included)
          .ToList ();
        
        sbProj.AppendLine (
          $"{projPlans.Count} project{pluralS (projPlans.Count)} affected:"
        );

        int count = 0;
        foreach (var plan in projPlans) {
          count++;
          if (count > MAX_DISPLAY)
            break;
          
          sbProj.AppendLine (
            $"- {plan.Project.OrigAbsPath.ToParaPath(_parameters)}" +
            $"{plan.AffectionKindToString()}"
          );
        }
        int diff = count - MAX_DISPLAY;
        if (diff > 0)
          sbProj.AppendLine ($"... and {diff} more project{pluralS (diff)}.");

        return sbProj.ToString();
      }

      string affectedSolutions () {
        StringBuilder sbSol = new ();
        var solPlans = plans.SolutionPlans;
        int nSolPlans = solPlans.Count;
        sbSol.AppendLine (
          $"{nSolPlans} solution{pluralS(nSolPlans)} affected:"
        );
        int count = 0;
        foreach (var plan in solPlans) {
          count++;
          if (count > MAX_DISPLAY)
            break;

          sbSol.AppendLine (
            $"- {plan.Solution.AbsolutePath.ToParaPath (_parameters)}" +
            $"{plan.AffectionKindToString ()}"
          );
        }
        int diff = count - MAX_DISPLAY;
        if (diff > 0)
          sbSol.AppendLine ($"... and {diff} more solution{pluralS (diff)}.");

        return sbSol.ToString();
      }
    }

     private bool callbackDependencyPropagationsConfirm (
       ProjectFile projectToCopy,
       IReadOnlyCollection<ProjectFile> projects,
       IReadOnlyCollection<SolutionFile> solutions,
       bool preselected
     ) {
      if (!(solutions.Any () || projects.Any ()))
        return true;

      const int MAX_DISPLAY = 20;

      StringBuilder sb = new ();

      sb.Append ("Additional ");
      if (projects.Any ())
        sb.Append ($"project{pluralS (projects.Count)}");
      if (projects.Any () && solutions.Any ()) 
        sb.Append (" and ");
      if (solutions.Any ())
        sb.Append ($"solution{pluralS (solutions.Count)}");
      sb.AppendLine (" will be affected,");
      sb.AppendLine ("because of interdependencies.");
      sb.AppendLine ();
      if (projects.Any ())
        sb.AppendLine (affectedProjects());
      //sb.AppendLine ();
      if (solutions.Any ())
        sb.AppendLine (affectedSolutions ());
      //sb.AppendLine ();
      sb.AppendLine ("Yes:\tProceed");
      sb.AppendLine ($"No:\tReturn to {(preselected ? "Preselection" : "Project Details")}");
      sb.Append ("Cancel:\tAbort");

      var result = _callback.ShowMessage (
        ECallback.Question,
        sb.ToString (),
        $"Per project confirmation \"{projectToCopy.Name}\"",
        EMsgBtns.YesNoCancel,
        EDefaultMsgBtn.Button1,
        true
      );

      bool proceed = result is EDialogResult.Yes or EDialogResult.OK;
      return proceed;

      string affectedProjects () {
        StringBuilder sbProj = new ();
        
        sbProj.AppendLine ($"{projects.Count} project{pluralS (projects.Count)} affected:");

        int count = 0;
        foreach (var project in projects) {
          count++;
          if (count > MAX_DISPLAY)
            break;
          
          sbProj.AppendLine (
            $"- {project.OrigAbsPath.ToParaPath(_parameters)}"
          );
        }
        int diff = count - MAX_DISPLAY;
        if (diff > 0)
          sbProj.AppendLine ($"... and {diff} more project{pluralS (diff)}.");

        return sbProj.ToString();
      }

      string affectedSolutions () {
        StringBuilder sbSol = new ();

        sbSol.AppendLine ($"{solutions.Count} solution{pluralS(solutions.Count)} affected:");

        int count = 0;
        foreach (var solution in solutions) {
          count++;
          if (count > MAX_DISPLAY)
            break;
          
          sbSol.AppendLine (
            $"- {solution.AbsolutePath.ToParaPath (_parameters)}");
        }
        int diff = count - MAX_DISPLAY;
        if (diff > 0)
          sbSol.AppendLine ($"... and {diff} more solution{pluralS(diff)}.");

        sbSol.AppendLine ();
        sbSol.AppendLine ("Note: Affected solutions not selected by user");
        sbSol.AppendLine ("may be removed again in the internal final review.");

        return sbSol.ToString();
      }
    }

    private ProjectFile? getProject (string projPath) {
      string absPath = projPath.ToAbsolutePath (_parameters.RootFolder!);
      return _allProjects
        .FirstOrDefault (p => p.AbsolutePath.Equals (absPath, StringComparison.OrdinalIgnoreCase));
    }

    private SolutionFile? getSolution (string slnPath) {
      string absPath = slnPath.ToAbsolutePath (_parameters.RootFolder!);
      return _allSolutions
        .FirstOrDefault (p => p.AbsolutePath.Equals (absPath, StringComparison.OrdinalIgnoreCase));
    }

    private string getParaPath (FileBase model) {
      return model.AbsolutePath.ToParaPath (_parameters);
    }

    private IReadOnlyList<string> getParaPaths (IEnumerable<FileBase> models) {
      return models
        .Select (m => m.AbsolutePath.ToParaPath (_parameters))
        .ToList ();
    }

    private static string pluralS (int cnt) => cnt != 1 ? "s" : "";
  }
}


