#pragma warning disable IDE0079
#pragma warning disable CA1860

namespace ProjectMover.Lib.Steps {
  internal class Step1ProjectCandidates (
    IProgressSink progress,
    ICallbackSink callback,
    IParameters parameters,
    Func<string, CancellationToken, Task<ProjectFile>> getProjectAsync
  ) {
    const string CSPROJ = CsProjectMover.CSPROJ;
    const string SHPROJ = CsProjectMover.SHPROJ;

    public async Task<List<ProjectFile>?> FindAsync (
      CancellationToken ct
    ) {
      var foundPaths = new List<string> ();
      var skippedPaths = new List<(string, bool)> ();

      using var gd = new ResourceGuard (
          () => progress.BeginStep ("Step 1 of 5: Finding project candidates ..."),
          () => progress.EndStep (endStep ())
      );
      progress.SetMax (-1); // indeterminate

      if (parameters.MultiMode.HasFlag (EMultiMode.Projects)) {
        var root = Path.GetFullPath (parameters.ProjectFolderOrFile!);
        if (!Directory.Exists (root))
          throw new InvalidOperationException (
              $"Specified project folder does not exist: {root}");
        await scanProjectFoldersAsync (root, parameters.ProjectRootRecursionDepth, foundPaths, skippedPaths, ct);
      } else {
        var single = Path.GetFullPath (parameters.ProjectFolderOrFile!);
        if (!File.Exists (single))
          throw new InvalidOperationException (
              $"Specified project file does not exist: {parameters.ProjectFolderOrFile}");
        progress.Report ($"Found: {single.ToParaPath (parameters)}");
        foundPaths.Add (single);
      }

      var projects = await loadProjectsAsync (foundPaths, ct);

      if (!projects.Any ()) {
        callback.ShowMessage (ECallback.Information, "No projects found. Done.");
        return null;
      }
      return projects;

      string endStep () => $"{foundPaths.Count} project(s) found, {skippedPaths.Count} project(s) skipped";

    }

    private async Task scanProjectFoldersAsync (
        string folder,
        int depth,
        List<string> result,
        List<(string, bool)>? skipped,
        CancellationToken ct
    ) {
      ct.ThrowIfCancellationRequested ();

      // start with gathering all projects in the folder
      List<ProjectFile> projectsInFolder = [];
      foreach (string projPath in folder
        .EnumerateFiles ([CSPROJ, SHPROJ], SearchOption.TopDirectoryOnly)) {

        ct.ThrowIfCancellationRequested ();
        var project = await getProjectAsync (projPath, ct);
        projectsInFolder.Add (project);
      }

      if (projectsInFolder.Any ()) {
        // now we distinguish between SDK and legacy C# projects (and shared projects)
        int sdkProjCount = projectsInFolder
          .Where (p => p is CsProjectFile csp && csp.IsSdkStyle)
          .Count ();

        if (/*sdkProjCount == 1 && */projectsInFolder.Count == 1) {
          // if single C# SDK project no subfolder may have any other project 

          // any projects in sub folders?
          foreach (string sub in Directory.EnumerateDirectories (folder)) {
            ct.ThrowIfCancellationRequested ();
            var subProjects = sub
              .EnumerateFiles ([CSPROJ, SHPROJ], SearchOption.AllDirectories)
              .ToList ();

            // will be skipped if so
            if (subProjects.Any ()) {
              skipped?.Add ((projectsInFolder[0].AbsolutePath, true));

              // and stop searching further
              return;
            }
          }

          string projPath = projectsInFolder[0].AbsolutePath; // we are good
          result.Add (projPath);
          progress.Report ($"Found {result.Count}: {projPath.ToParaPath (parameters)}");
          return;

        } else if (sdkProjCount > 1) {
          // multiple C# SDK projects in same folder – skip all
          skipped?.Add ((projectsInFolder[0].AbsolutePath, false));
          return;
        } else {
          // NOT CURRENTLY SUPPORTED :legacy C# projects or shared projects in same folder
          skipped?.Add ((projectsInFolder[0].AbsolutePath, false));
          return;
          //foreach (var proj in projectsInFolder) {
          //  result.Add (proj.AbsolutePath);
          //  progress.Report ($"Found {result.Count}: {proj.AbsolutePath.ToParaPath (parameters)}");
          //}
          //// do not return, sub folders are legit
        }
      }

      //// search this level only for projects
      //var projectPaths = folder
      //  .EnumerateFiles ([CSPROJ, SHPROJ], SearchOption.TopDirectoryOnly)
      //  .ToList ();

      //// single project this folder?
      //if (projectPaths.Count == 1) {
      //  var project = await getProjectAsync (projectPaths [0], ct);
      //  // SDK style C# project may only have a single project in a folder
      //  if (project is CsProjectFile csProj && csProj.IsSdkStyle) {

      //    // any projects in sub folders?
      //    foreach (string sub in Directory.EnumerateDirectories (folder)) {
      //      ct.ThrowIfCancellationRequested ();
      //      var subProjects = sub
      //        .EnumerateFiles ([CSPROJ, SHPROJ], SearchOption.AllDirectories)
      //        .ToList ();

      //      // will be skipped if so
      //      if (subProjects.Any ()) {
      //        skipped?.Add ((projectPaths[0], true));

      //        // and stop searching further
      //        return;
      //      }
      //    }
      //  }

      //  string projPath = projectPaths[0]; // we are good
      //  result.Add (projPath); 
      //  progress.Report ($"Found {result.Count}: {projPath.ToParaPath (parameters)}");

      //} else if (projectPaths.Count > 1) {
      //  // multiple projects in same folder – skip all
      //  skipped?.Add ((projectPaths[0], false));
      //  return;
      //}

      // no or non-C# SDK projects – stop or decrement depth
      if (depth <= 0)
        return;
      depth -= 1;

      // recurse
      foreach (var sub in Directory.EnumerateDirectories (folder))
        await scanProjectFoldersAsync (sub, depth, result, skipped, ct);
    }

    private async Task<List<ProjectFile>> loadProjectsAsync (
      IEnumerable<string> projectPaths,
      CancellationToken ct
    ) {
      List<ProjectFile> result = [];
      foreach (var path in projectPaths) {
        var projectFile = await getProjectAsync (path, ct);
        result.Add (projectFile);
      }
      return result;
    }
  }
}
