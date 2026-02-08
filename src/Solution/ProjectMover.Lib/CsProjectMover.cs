#pragma warning disable IDE0079
#pragma warning disable CA1859
#pragma warning disable CA1860
#pragma warning disable CA2208
#pragma warning disable IDE0305

namespace ProjectMover.Lib {
  public sealed class CsProjectMover (
    IProgressSink progressSink, 
    ICallbackSink callbackSink,
    IProjectDecisionProvider decisionProvider
  ) {

    public static bool DryRun { get; set; }

    public const string CSPROJ = "*" + CsProjectFile.EXT;
    public const string SHPROJ = "*" + SharedProjectFile.EXT;
    public const string SLN = "*" + SolutionFile.EXT;

    private readonly IProgressSink _progress = progressSink;
    private readonly ICallbackSink _callback = callbackSink;
    private readonly IProjectDecisionProvider _decisionProvider = decisionProvider;

    // Solution cache – valid for lifetime of the instance, until root folder changes
    private readonly Dictionary<string, SolutionFile> _solutionCache =
        new (StringComparer.OrdinalIgnoreCase);

    // Project cache – valid for lifetime of the instance
    private readonly Dictionary<string, ProjectFile> _projectCache =
        new (StringComparer.OrdinalIgnoreCase);

    private string? _cachedSolutionFolder;

    public async Task<bool> RunAsync (
        IParameters parameters,
        CancellationToken cancellationToken = default
    ) {
      // we run it in its own task, as a good deal of the work is CPU-bound.
      return await Task.Run (async () => {

        bool _completed = false;

        try {

          cancellationToken.ThrowIfCancellationRequested ();

          var modifiedParameters = await initialChecksAsync (parameters, cancellationToken);
          if (modifiedParameters is null)
            return _completed;
          parameters = modifiedParameters;

          clearAndUpdateCaches (parameters);

          // STEP 1
          var projectCandidates = await step1_FindProjectCandidatesAsync (parameters, cancellationToken);

          if (projectCandidates is null)
            return _completed;

          // STEP 2
          var dependencies = await step2_FindDependingSolutionsAndProjectsAsync (parameters, projectCandidates, cancellationToken);
          
          if (!projectCandidates.Any())
            return _completed;

          // STEP 3
          var plans = step3_UserInteraction (parameters, projectCandidates, dependencies, cancellationToken);

          if (plans is null)
            return _completed;

          // Step 4
          step4_ApplyInMemoryChanges (parameters, plans);

          // Step 5
          await step5_ExecuteFileOperationsAsync (parameters, plans, cancellationToken);

          _completed = true;
        } catch (OperationCanceledException) {
          _callback.ShowMessage (ECallback.Information, "Operation cancelled.");
        } catch (Exception ex) {
          _callback.ShowMessage (ECallback.Error, ex.Message, ex.GetType ().Name);
        }

        return _completed;
      });
    }

    private async Task<List<ProjectFile>?> step1_FindProjectCandidatesAsync (
      IParameters parameters,
      CancellationToken ct
    ) {
      Step1ProjectCandidates step1 = new (_progress, _callback, parameters, getProjectAsync);
      var projectCandidates = await step1.FindAsync (ct);
      return projectCandidates;
    }


    private async Task<DependentSolutionsAndProjects> step2_FindDependingSolutionsAndProjectsAsync (
      IParameters parameters,
      List<ProjectFile> candidateProjects,
      CancellationToken ct
    ) {
      Step2DependentProjectsAndSolutions step2 = new (
        _progress,
        _callback,
        parameters,
        getProjectAsync,
        getSolutionAsync,
        _solutionCache.Values
      );
      var dependencies = await step2.FindAsync (candidateProjects, ct);
      return dependencies;
    }



    private ProjectAndSolutionPlans? step3_UserInteraction (
      IParameters parameters,
      IReadOnlyList<ProjectFile> candidateProjects,
      DependentSolutionsAndProjects dependencies,
      CancellationToken ct
    ) {
      Step3UserInteraction step3 = new (
        _progress,
        _callback,
        parameters,
        _decisionProvider,
        candidateProjects,
        dependencies
      );

      var plans = step3.UserInteraction (ct);

      return plans;
    }



    private void step4_ApplyInMemoryChanges (IParameters parameters, ProjectAndSolutionPlans plans) {
      Step4InMemoryChanges step4 = new (parameters, _progress);
      step4.Apply (plans.ProjectPlans);
    }

    private async Task step5_ExecuteFileOperationsAsync (
        IParameters parameters,
        ProjectAndSolutionPlans plans,
        CancellationToken ct
    ) {
      Step5FileOperations step5 = new (parameters, _progress, _callback);
      await step5.ExecuteAsync (plans, ct);
    }

    private async Task<IParameters?> initialChecksAsync (IParameters parameters, CancellationToken ct) {
      using var gd = new ResourceGuard (
        () => _progress.BeginStep ("Step 0 of 5: Initial parameters and environment checks..."),
        () => _progress.EndStep ("Initial checks completed.")
      );
      _progress.SetMax (-1);

      checkFileAndFolderParameters (parameters);

      var modifiedParameters = await checkFileOperationsEnvironment (parameters, ct);
      return modifiedParameters;
    }


    private static void checkFileAndFolderParameters (IParameters parameters) {
      // root folder
      if (string.IsNullOrWhiteSpace (parameters.RootFolder))
        throw new ArgumentException (
          "Root folder must be specified.",
          nameof (parameters.RootFolder));
      if (!Directory.Exists (parameters.RootFolder))
        throw new ArgumentException (
          $"Root folder must exist: {parameters.RootFolder}",
          nameof (parameters.RootFolder));
      string root = parameters.RootFolder;

     // destination folder
     string? destFolder = parameters.DestinationFolder?.ToAbsolutePath (root);
      if (string.IsNullOrWhiteSpace (destFolder))
        throw new ArgumentException (
          "Destination folder must be specified.",
          nameof (parameters.DestinationFolder));
      if (!destFolder.IsSubPathOf (parameters.RootFolder))
        throw new ArgumentException (
            $"Destination folder {parameters.DestinationFolder} must be a sub-folder of root folder {parameters.RootFolder}");

      // project folder or file
     string? projFolderOrFile = parameters.ProjectFolderOrFile?.ToAbsolutePath (root);
      if (parameters.MultiMode.HasFlag (EMultiMode.Projects)) {
        if (string.IsNullOrWhiteSpace (projFolderOrFile))
          throw new ArgumentException (
            "Project root folder must be specified in multi-project mode.",
            nameof (parameters.ProjectFolderOrFile));
        if (!Directory.Exists (projFolderOrFile))
          throw new ArgumentException (
            $"Project root folder must exist: {projFolderOrFile}",
            nameof (parameters.ProjectFolderOrFile));
      } else {
        if (string.IsNullOrWhiteSpace (projFolderOrFile))
          throw new ArgumentException (
            "Project file must be specified in single-project mode.",
            nameof (parameters.ProjectFolderOrFile));
        if (!File.Exists (projFolderOrFile))
          throw new ArgumentException (
            $"Project file must exist: {parameters.ProjectFolderOrFile}",
            nameof (parameters.ProjectFolderOrFile));
        string ext = Path.GetExtension (projFolderOrFile);
        if (!string.Equals (ext, CsProjectFile.EXT, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals (ext, SharedProjectFile.EXT, StringComparison.OrdinalIgnoreCase))
          throw new ArgumentException (
            $"Project file is not a recognized file type: {parameters.ProjectFolderOrFile}",
            nameof (parameters.ProjectFolderOrFile));
      }
      if (!projFolderOrFile.IsExistingSubPathOf (parameters.RootFolder))
        throw new ArgumentException (
          $"Project folder or file {parameters.ProjectFolderOrFile} must be a sub-path of root folder {parameters.RootFolder}",
          nameof (parameters.ProjectFolderOrFile));

      // solution folder or file
     string? solFolderOrFile = parameters.SolutionFolderOrFile?.ToAbsolutePath (root);
      if (parameters.MultiMode.HasFlag (EMultiMode.Solutions)) {
        if (solFolderOrFile is not null) {
          if (!Directory.Exists (solFolderOrFile))
            throw new ArgumentException (
              $"Solution root folder must exist: {parameters.SolutionFolderOrFile}",
              nameof (parameters.SolutionFolderOrFile));
          if (!solFolderOrFile.IsExistingSubPathOf (parameters.RootFolder))
            throw new ArgumentException (
              $"Solution root folder {parameters.SolutionFolderOrFile} must be a sub-path of root folder {parameters.RootFolder}",
              nameof (parameters.SolutionFolderOrFile));
        }
      } else {
        if (string.IsNullOrWhiteSpace (solFolderOrFile))
          throw new ArgumentException (
            "Solution file must be specified in single-project mode.",
            nameof (parameters.SolutionFolderOrFile));
        if (!File.Exists (solFolderOrFile))
          throw new ArgumentException (
            $"Solution file must exist: {parameters.SolutionFolderOrFile}",
            nameof (parameters.SolutionFolderOrFile));
        string ext = Path.GetExtension (solFolderOrFile);
        if (!string.Equals (ext, SolutionFile.EXT, StringComparison.OrdinalIgnoreCase))
          throw new ArgumentException (
            $"Solution file is not a recognized file type: {parameters.SolutionFolderOrFile}",
            nameof (parameters.SolutionFolderOrFile));
      }
    }

    private async Task<IParameters?> checkFileOperationsEnvironment (IParameters parameters, CancellationToken ct) {
      IParameters? modifiedParameters = 
        await checkFileOperationsEnvironmentParametersConsistencyAsync (parameters, _callback, ct);
      if (modifiedParameters is null)
        return null;
      else
        parameters = modifiedParameters;

      await using var ctx = await FileOperationContextFactory.CreateAsync (parameters, ct);

      List<string?> pathsToCheck = [
        parameters.RootFolder,
        //parameters.DestinationFolder,
        parameters.ProjectFolderOrFile,
        parameters.SolutionFolderOrFile
      ];

      bool succ = await ctx.ValidateEnvironmentAsync (
        _progress,
        _callback,
        pathsToCheck,
        ct
      );
      return succ ? parameters : null;
    }

    private async static Task<IParameters?> checkFileOperationsEnvironmentParametersConsistencyAsync (
      IParameters paras, 
      ICallbackSink callback, 
      CancellationToken ct
    ) {
      string nl = Environment.NewLine;

      if (paras is not Parameters parameters)
        parameters = new Parameters (paras);
      parameters = parameters.ToAbsolutePath ();

      // Always try and create SVN context to see if we have SVN working copy
      IFileOperationContext? ctx;
      try {
        ctx = await FileOperationContextFactory.CreateAsync (parameters with { FileOperations = EFileOperations.Svn }, ct);
      } catch (Exception) {
        ctx = null;
      }

      ValueTask disposeCtxAsync () => ctx is null ? ValueTask.CompletedTask : ctx.DisposeAsync ();
      await using var rg = new AsyncResourceGuard (disposeCtxAsync);
      

      switch (parameters.FileOperations) {
        // if parameters ask for SVN but we don't have SVN working copy → error
        case EFileOperations.Svn: {
            if (ctx is null) {
              string msg =
                "File operations are set to SVN, " + nl +
                "but the root folder is not an SVN working copy." + nl + nl +
                "Continue without SVN?";
              var res = callback.ShowMessage (ECallback.Error, msg, "SVN Working Copy Required", EMsgBtns.OKCancel, EDefaultMsgBtn.Button2);
              return res switch {
                EDialogResult.OK => parameters with { FileOperations = EFileOperations.Direct },
                _ => null
              };
            } else
              return parameters;
          }

        // if parameters ask for direct file ops but we do have SVN working copy → warning
        case EFileOperations.Direct: {
            if (ctx is null)
              return parameters;
            else {
              string msg = "File operations are set to Direct file operations, " + nl +
              "but the root folder is an SVN working copy." + nl + nl +
              "Continue with SVN file operations?";
              var res = callback.ShowMessage (ECallback.Warning, msg, "SVN Working Copy Detected", EMsgBtns.YesNoCancel, EDefaultMsgBtn.Button3);
              return res switch {
                EDialogResult.Yes => parameters with { FileOperations = EFileOperations.Svn },
                EDialogResult.No => parameters,
                _ => null
              };
            }
          }
        default:
          return parameters;
      }
    }

    private void clearAndUpdateCaches (IParameters parameters) {
      if (parameters.RootFolder is null)
        throw new NullReferenceException ($"{nameof (parameters.RootFolder)} must not be null");

      if (parameters.MultiMode.HasFlag (EMultiMode.Solutions)) {
        string solutionFolder = parameters.SolutionFolderOrFile ?? parameters.RootFolder;

        bool solutionCacheRescan = parameters.Rescan ||
            !solutionFolder.Equals (_cachedSolutionFolder, StringComparison.OrdinalIgnoreCase);

        if (solutionCacheRescan) {
          _solutionCache.Clear ();
          _cachedSolutionFolder = solutionFolder;
        }
      } else {
        _solutionCache.Clear ();
        _cachedSolutionFolder = null;
      }


      // TODO review project cache update logic, currently disabled.
      // Referencedprojects in ProjectFile must be updated on reset, too.
      // Play safe for now and clear entire cache
      _projectCache.Clear ();


      //// we do not clear project cache but we need to update it because keys may no longer exist
      //// projects should update themselves when written, but we must commit the new paths
      //var kvps = _projectCache.ToList ();
      //foreach (var kvp in kvps) {
      //  if (string.Equals (kvp.Key, kvp.Value.AbsolutePath, StringComparison.OrdinalIgnoreCase))
      //    continue;

      //  _projectCache.Remove (kvp.Key);
      //  kvp.Value.CommitUpdatedFilePath ();
      //  _projectCache[kvp.Value.AbsolutePath] = kvp.Value;
      //}

    }


    private async Task<SolutionFile> getSolutionAsync (
      string path,
      CancellationToken ct
    ) {
      path = Path.GetFullPath (path);

      SolutionFile solutionFile = await _solutionCache.GetOrAddAsync (path, createSolutionFile, recreateCriterion);

      return solutionFile;

      async Task<SolutionFile> createSolutionFile (string _) {
        var sln = new SolutionFile (path);
        await sln.LoadAsync (ct);
        return sln;
      }

      bool recreateCriterion (string _, SolutionFile value) =>
        value.LastWriteTimeUtc != File.GetLastWriteTimeUtc (path);
    }


    private async Task<ProjectFile> getProjectAsync (
        string path,
        CancellationToken ct
    ) {
      path = Path.GetFullPath (path);

      ProjectFile proj = await _projectCache.GetOrAddAsync (path, createProjectFile, recreateCriterion);

      return proj;

      async Task<ProjectFile> createProjectFile (string _) {
        ProjectFile proj = Path.GetExtension (path) switch {
          CsProjectFile.EXT => new CsProjectFile (path),
          SharedProjectFile.EXT => new SharedProjectFile (path),
          _ => throw new ArgumentException (
              $"Unsupported project file type: {Path.GetExtension (path)}",
              nameof (path)
              )
        };
        await proj.LoadAsync (ct);
        return proj;
      }

      bool recreateCriterion (string _, ProjectFile value) =>
        value.LastWriteTimeUtc != File.GetLastWriteTimeUtc (path);
    }

  }

}
