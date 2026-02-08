#pragma warning disable IDE0079
#pragma warning disable CA1860
#pragma warning disable IDE0305

namespace ProjectMover.Lib.Steps {
  internal class Step2DependentProjectsAndSolutions (
    IProgressSink progress,
    ICallbackSink callback,
    IParameters parameters,
    Func<string, CancellationToken, Task<ProjectFile>> getProjectAsync,
    Func<string, CancellationToken, Task<SolutionFile>> getSolutionAsync,
    IEnumerable<SolutionFile> cachedSolutions
  ) {

    sealed record ProjectReference {
      public required SolutionFile Solution { get; init; }
      public required ProjectFile Project { get; init; }

      public Guid SolutionGuid { get; init; }
      public Guid ProjectFileGuid { get; init; }

      public bool RequiresStrictGuidMatch { get; init; }
      public bool IsShared { get; init; }
    }

    public enum EGuidMismatchReason {
      SolutionGuidMismatch,
      MissingProjectGuid,
      ProjectGuidMismatch
    }

    public sealed record SolutionGuidIssue {
      public required SolutionFile Solution { get; init; }
      public required string ProjectPath { get; init; }
      public EGuidMismatchReason Reason { get; init; }
    }

    public sealed record ProjectGuidIssue {
      public required ProjectFile FromProject { get; init; }
      public required ProjectFile ToProject { get; init; }
      public EGuidMismatchReason Reason { get; init; }
    }

    sealed record ProjectReferenceEdge {
      public required ProjectFile FromProject { get; init; }
      public required ProjectFile ToProject { get; init; }

      public Guid FromProjectGuid { get; init; }
      public Guid ToProjectGuid { get; init; }

      // referencing a shared project
      public bool IsSharedReference { get; init; }
    }

    sealed class ProjectLookup (IProgressSink progress) { 
      record PathProj (string Path, ProjectFile Proj);

      private Dictionary<string, ProjectFile> _projects = [];

      public async Task LoadAsync (
        IEnumerable<string> solutionProjectPaths,
        IEnumerable<ProjectFile> candidateProjects,
        Func<string, CancellationToken, Task<ProjectFile>> getProjectAsync,
        CancellationToken ct
      ) {
        progress.SetMax (solutionProjectPaths.Count ());

        // 1️ Preload all solution projects asynchronously
        var preloadedProjects = await Task.WhenAll (
            solutionProjectPaths.Select (path => {
              progress.ReportRel ();
              return getProjectAsync (path, ct);
            })
        );

        // 2️ Combine with candidate projects
        var allProjects = preloadedProjects.Union (candidateProjects).ToList ();

        // 3️ Build dictionary mapping both main paths and shared item paths to the project
        _projects = allProjects
            .SelectMany (p => {
              IEnumerable<PathProj> pp = p is SharedProjectFile shp
                   ? [new PathProj (p.AbsolutePath, Proj: p), new PathProj (shp.ProjItemsAbsPath, Proj: p)]
                   : [new PathProj (Path: p.AbsolutePath, Proj: p)];
              return pp;
            })
             .ToDictionary (
                x => x.Path.ToLowerInvariant (),
                x => x.Proj
            );
      }

      public ProjectFile GetProject (string path) {
        ArgumentNullException.ThrowIfNull (path);

        var normalizedPath = Path.GetFullPath (path).ToLowerInvariant ();

        ProjectFile? proj = _projects.GetValueOrDefault (normalizedPath) ?? 
          throw new KeyNotFoundException ($"Project not found for path '{path}'");
        return proj;
      }
    }

    const string SLN = CsProjectMover.SLN;

    public async Task<DependentSolutionsAndProjects> FindAsync (
      List<ProjectFile> candidateProjects,
      CancellationToken ct
    ) {

      var solutions = await step2a_FindAffectedSolutionsAsync (candidateProjects, ct);
      
      var involvedProjects = await step2b_FindInvolvedProjectsAndGuidIssuesAsync  (
        solutions, candidateProjects, ct);
      
      var dependentProjects = step2c_FindDependentProjects (
        candidateProjects, involvedProjects);

      return new DependentSolutionsAndProjects (solutions, dependentProjects);
    }


    private async Task<List<SolutionFile>> step2a_FindAffectedSolutionsAsync (
      List<ProjectFile> projects,
      CancellationToken ct
     ) {
      var result = new List<SolutionFile> ();

      using var guard = new ResourceGuard (
          () => progress.BeginStep ("Step 2a of 5: Scanning solutions ..."),
          () => progress.EndStep (endStep ())
      );
      progress.SetMax (-1); // indeterminate


      //IEnumerable<string> solutionFiles;
      int total = 0;
      int referenced = 0;

      if (!parameters.MultiMode.HasFlag (EMultiMode.Solutions)) {
        string? slnPath = parameters.SolutionFolderOrFile;
        if (slnPath is null || !File.Exists (parameters.SolutionFolderOrFile!))
          throw new InvalidOperationException (
              $"Specified solution file does not exist: {parameters.SolutionFolderOrFile}");

        await loadAndCheckSolutionAsync (slnPath);
      } else {
        if (cachedSolutions.Any ()) {
          foreach (var slnFile in cachedSolutions) {
            checkSolutionAsync (slnFile);
          }
        } else {
          foreach (string slnPath in Directory.EnumerateFiles (
              parameters.SolutionFolderOrFile ?? parameters.RootFolder!,
              SLN,
              SearchOption.AllDirectories)) {
            await loadAndCheckSolutionAsync (slnPath);
          }
        }
      }

      if (!result.Any ())
        callback.ShowMessage (ECallback.Warning, "No dependent solutions found.");

      string endStep () => $"{result.Count} solutions(s) found";
      return result;

      async Task loadAndCheckSolutionAsync (string slnPath) {
        var slnFile = await getSolutionAsync (slnPath, ct);
        checkSolutionAsync (slnFile);
      }

      void checkSolutionAsync (SolutionFile slnFile) {
        total++;
        if (projects.Select (p => p.AbsolutePath).Any (slnFile.ContainsProject)) {
          // we check later, not in step 2a
          referenced++;
          result.Add (slnFile);
          progress.Report ($"Found {referenced} in {total}: {slnFile.AbsolutePath.ToParaPath (parameters)}");
        } else
          progress.Report ($"Found {referenced} in {total}");
      }
    }


    private async Task<IEnumerable<ProjectFile>> step2b_FindInvolvedProjectsAndGuidIssuesAsync (
        List<SolutionFile> solutions,
        List<ProjectFile> candidateProjects,
        CancellationToken ct
    ) {
      // This is a really long one!
      // It has been split-up into some local functions for readability

      IEnumerable<ProjectFile>? result = [];
      string endStep () => $"{result.Count ()} dependent project(s) found";

      using var guard = new ResourceGuard (
          () => progress.BeginStep ("Step 2b of 5 Resolving project dependencies ..."),
          () => progress.EndStep (endStep ())
      );

      // All projects in all solutions define the scope
      var solutionProjectPaths =
          solutions
              .SelectMany (sln => sln.ProjectsAbsPath)
              .Distinct (StringComparer.OrdinalIgnoreCase)
              .ToList ();

      // Build the project lookup cache
      // LINQ expressions further down the line can't do async,
      // so preload and obtain a sync delegate
      Func<string,ProjectFile> getProject = await buildProjectLookupCache (solutionProjectPaths, ct);

      // Check solutions for GUID issues and get all project references on the way 
      var (solutionsWithGuidIssues, projectRefsInSolution) =
        evaluateSolutions ();

      // Check projects for inter-project GUID issues, which is rather complex
      var (guidValidProjects, candProjectsWithGuidMismatch) =
        evaluateProjects (solutionsWithGuidIssues, projectRefsInSolution);

      // Report the GUID issues, if any
      reportDiscarded (candProjectsWithGuidMismatch, solutionsWithGuidIssues);

      removeDiscardedSolutionsAndProjects (solutionsWithGuidIssues, candProjectsWithGuidMismatch);

      result = guidValidProjects;

      return result;

      #region local functions

      // Build the project lookup cache
      // LINQ expressions further down the line can't do async,
      // so preload and obtain a sync delegate
      async Task<Func<string, ProjectFile>> buildProjectLookupCache (
        IEnumerable<string> solutionProjectPaths, 
        CancellationToken ct
      ) {
        // Within a Linq expression we can't load asynchronously,
        // so do this first, and fill a cache.
        // this is the only operation here that consumes I/O time, so report its progress
        ProjectLookup projectLookupCache = new (progress);
        await projectLookupCache.LoadAsync (solutionProjectPaths, candidateProjects, getProjectAsync, ct);

        // Local function to get the project from the cache by path
        ProjectFile getProject (string absPath) => projectLookupCache.GetProject (absPath);
        return getProject;
      }


      // Check solutions for GUID issues and get all project references on the way 
      (IEnumerable<SolutionFile>, IEnumerable<ProjectReference>) evaluateSolutions () {

        // Gather the data for GUID evaluation between solutions and projects 
        projectRefsInSolution = solutions
          .SelectMany (solution =>
            solution.ProjectsAbsPath.Select (projectPath => {
              var project = getProject (projectPath);
              return new ProjectReference {
                Solution = solution,
                Project = project,
                SolutionGuid = solution.ProjectGuid (projectPath),
                ProjectFileGuid = project.ProjectGuid,
                RequiresStrictGuidMatch = project.RequiresGuid,
                IsShared = project is SharedProjectFile
              };
            })
          )
          .ToList ();

        // Find any GUID issues between solutions and projects
        var solutionGuidIssuesWithProjects = projectRefsInSolution
          .GroupBy (p => p.Project)
          .SelectMany (group => {

            var strictRefs =
                group.Where (p => p.RequiresStrictGuidMatch).ToList ();

            if (strictRefs.Count == 0)
              return [];

            var baselineSolutionGuid = strictRefs[0].SolutionGuid;

            // 1️ Solution GUID mismatch across solutions
            if (strictRefs.Any (p => p.SolutionGuid != baselineSolutionGuid)) {
              return strictRefs.Select (p => new SolutionGuidIssue {
                Solution = p.Solution,
                ProjectPath = group.Key.AbsolutePath,
                Reason = EGuidMismatchReason.SolutionGuidMismatch
              });
            }

            // 2️ Missing project GUID
            var projectGuid = strictRefs[0].ProjectFileGuid;

            if (projectGuid == default) {
              return strictRefs.Select (p => new SolutionGuidIssue {
                Solution = p.Solution,
                ProjectPath = group.Key.AbsolutePath,
                Reason = EGuidMismatchReason.MissingProjectGuid
              });
            }

            // 3️ Project GUID mismatch
            if (strictRefs.Any (p =>
                p.ProjectFileGuid != projectGuid ||
                p.SolutionGuid != projectGuid)) {

              return strictRefs.Select (p => new SolutionGuidIssue {
                Solution = p.Solution,
                ProjectPath = group.Key.AbsolutePath,
                Reason = EGuidMismatchReason.ProjectGuidMismatch
              });
            }

            return [];
          })
          .ToList ();

        solutionsWithGuidIssues = solutionGuidIssuesWithProjects
          .Select (i => i.Solution)
          .Distinct ()
          .ToList ();

        return (solutionsWithGuidIssues, projectRefsInSolution);
      }

      // Check projects for inter-project GUID issues, which is rather complex
      (IEnumerable<ProjectFile>, IEnumerable<ProjectFile>) evaluateProjects (
        IEnumerable<SolutionFile> solutionsWithGuidIssues,
        IEnumerable<ProjectReference> projectRefsInSolution
      ) {

        List<IGrouping<ProjectFile, ProjectReference>> validProjects = projectRefsInSolution
          .Where (p => !solutionsWithGuidIssues.Contains (p.Solution))
          .GroupBy (p => p.Project)
          .Where (group => {

            var strictRefs =
                group.Where (p => p.RequiresStrictGuidMatch).ToList ();

            if (strictRefs.Count == 0)
              return true;

            var solutionGuid = strictRefs[0].SolutionGuid;
            var projectGuid = strictRefs[0].ProjectFileGuid;

            return
                projectGuid != default &&
                strictRefs.All (p =>
                    p.SolutionGuid == solutionGuid &&
                    p.ProjectFileGuid == projectGuid
                );
          })
          .ToList ();

        // Compute all edges, but do not check them here yet.
        // "from" is the project that references the "to" project, the target
        var projectReferenceEdges = validProjects
          .Select (p => p.Key)
          .SelectMany (p => p.AllProjectReferencesAbsPath.Select (a => {
            var r = getProject (a);
            return new ProjectReferenceEdge {
              FromProject = p,
              ToProject = r,
              FromProjectGuid = p.ProjectRefGuid (r),
              ToProjectGuid = r.ProjectGuid,
              IsSharedReference = r is SharedProjectFile
            };
          }))
          .ToList ();

        List<ProjectGuidIssue> projectGuidIssues = [];
        foreach (var edge in projectReferenceEdges) {
          // they could be both 0 which is fine
          // as we checked guid requirements in the previous step
          if (edge.IsSharedReference || edge.FromProjectGuid == edge.ToProjectGuid)
            continue;
          projectGuidIssues.Add (new ProjectGuidIssue {
            FromProject = edge.FromProject,
            ToProject = edge.ToProject,
            Reason = EGuidMismatchReason.ProjectGuidMismatch,
          });
        }

        List<ProjectFile> projectsWithGuidMismatch = projectGuidIssues
          .SelectMany (i => new[] { i.FromProject, i.ToProject })
          .Distinct ()
          .ToList ();


        guidValidProjects = validProjects
          .Select (p => p.Key)
          .Except (projectsWithGuidMismatch)
          .ToList ();


        // double check:
        // if any candidate project is involved in a guid mismatch 
        // we discard it
        candProjectsWithGuidMismatch = candidateProjects
          .Intersect (projectsWithGuidMismatch)
          .ToList ();
        IEnumerable<ProjectFile> sharedProjectsWithInternalGuidMismatch = candidateProjects
          .Except (candProjectsWithGuidMismatch)
          .OfType<SharedProjectFile> ()
          .Where (p => p.OrigProjectGuid != p.OrigProjItemsGuid)
          .ToList ();
        candProjectsWithGuidMismatch = candProjectsWithGuidMismatch
          .Union (sharedProjectsWithInternalGuidMismatch)
          .ToList ();

        return (guidValidProjects, candProjectsWithGuidMismatch);
      }

      void removeDiscardedSolutionsAndProjects (
        IEnumerable<SolutionFile> solutionsWithGuidIssues, IEnumerable<ProjectFile> candProjectsWithGuidMismatch
      ) {
        // we were passed candidate projects and solutions as list parameters,
        // so we can discard the tainted right here
        foreach (var candProject in candProjectsWithGuidMismatch)
          candidateProjects.Remove (candProject);
        foreach (var sln in solutionsWithGuidIssues)
          solutions.Remove (sln);

        var candProjsAbsPaths = candidateProjects
          .Select (p => p.AbsolutePath)
          .ToList ();

        // There may be more solutions no longer relevant
        // because they do not reference any surviving candidate project
        var irrelSolutions = solutions
          .Where (s => !s.ProjectsAbsPath
            .Intersect (candProjsAbsPaths, StringComparer.OrdinalIgnoreCase).Any ())
          .ToList ();
        foreach (var sln in irrelSolutions)
          solutions.Remove (sln);
      }



      #endregion local functions
    }


    private List<ProjectFile> step2c_FindDependentProjects (
      IEnumerable<ProjectFile> candidateProjects,
      IEnumerable<ProjectFile> involvedProjects
    ) {

      var result = new List<ProjectFile> ();
      string endStep () => $"{result.Count} dependent project(s) found";

      using var guard = new ResourceGuard (
          () => progress.BeginStep ("Step 2c of 5 Resolving project dependencies ..."),
          () => progress.EndStep (endStep ())
      );

      int total = 0;
      int referenced = 0;

      var candidateProjectPaths = candidateProjects
        .Select (p => p.AbsolutePath)
        .ToList ();

      // Prepare candidate shared project paths
      var candidateSharedProjPaths = candidateProjects
          .OfType<SharedProjectFile> ()
          .Select (p => p.AbsolutePath)
          .ToList ();

      // Prepare mapping: .shproj -> .projitems
      var candidateShprojToProjitems = candidateProjects
          .OfType<SharedProjectFile> ()
          .ToDictionary (
              sh => sh.AbsolutePath,        // .shproj
              sh => sh.ProjItemsAbsPath,    // .projitems
              StringComparer.OrdinalIgnoreCase
          );

      // Check each project in solution
      foreach (var proj in involvedProjects) {
        total++;

        bool referencesCandidateProject =
            proj.ProjectReferencesAbsPath.Intersect (candidateProjectPaths, StringComparer.OrdinalIgnoreCase).Any ();

        bool importsCandidateSharedProject =
            proj.SharedProjectImportsAbsPath.Intersect (candidateShprojToProjitems.Values, StringComparer.OrdinalIgnoreCase).Any ();

        bool isReferencedByCandidateProject = candidateProjectPaths.
          Contains (proj.AbsolutePath, StringComparer.OrdinalIgnoreCase);

        if (referencesCandidateProject || importsCandidateSharedProject || isReferencedByCandidateProject) {
          referenced++;
          progress.Report (
            $"Found {referenced} in {total}: {proj.AbsolutePath.ToParaPath (parameters)}"
          );
          result.Add (proj);
        } else
          progress.Report ($"Found {referenced} in {total}");
      }

      if (!result.Any ())
        callback.ShowMessage (ECallback.Information, "No dependent projects found.");

      return result;
    }


    private void reportDiscarded (
      IEnumerable<ProjectFile> discardedCandidateProjects,
      IEnumerable<SolutionFile> discardedSolutions
    ) {
      if (!(discardedCandidateProjects.Any () || discardedSolutions.Any ()))
        return;

      StringBuilder sb = new ();
      const int MAX_DISPLAY = 20;


      if (discardedCandidateProjects.Any ()) {
        sb.AppendLine ("Discarded candidate projects because of GUID mismatch:");

        int count = 0;
        foreach (var proj in discardedCandidateProjects) {
          count++;
          if (count > MAX_DISPLAY)
            break;
          sb.AppendLine ($"- {proj.AbsolutePath.ToParaPath (parameters)}");
        }
        if (count > MAX_DISPLAY)
          sb.AppendLine ($"... and {count - MAX_DISPLAY} more projects.");
      }

      if (discardedSolutions.Any ()) {
        if (sb.Length > 0)
          sb.AppendLine ();

        sb.AppendLine ("Discarded solutions because of GUID mismatch:");

        int count = 0;
        foreach (var sln in discardedSolutions) {
          count++;
          if (count > MAX_DISPLAY)
            break;
          sb.AppendLine ($"- {sln.AbsolutePath.ToParaPath (parameters)}");
        }
        if (count > MAX_DISPLAY)
          sb.AppendLine ($"... and {count - MAX_DISPLAY} more solutions.");
      }


      callback.ShowMessage (
        ECallback.Warning, 
        sb.ToString (), 
        "Project/solution GUID mismatch", 
        true);
    }

  }
}
