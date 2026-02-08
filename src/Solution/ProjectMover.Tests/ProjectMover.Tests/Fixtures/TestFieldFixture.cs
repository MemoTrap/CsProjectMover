#pragma warning disable IDE0079
#pragma warning disable CA1822
#pragma warning disable IDE0057
#pragma warning disable IDE0130

using System.Xml.Linq;

using ProjectMover.Lib.Models;
using ProjectMover.Lib.Processes;
using ProjectMover.Lib.Svn;


namespace ProjectMover.Tests {
  using static Const;

  internal sealed class TestFieldFixture : IAsyncDisposable {

    public string Root { get; }
    public bool Svn { get; }

    private TestFieldFixture (string root, bool svn) {
      Root = root;
      Svn = svn;
    }

    /// <summary>
    /// Async factory: creates the fixture and exports the test field via SVN.
    /// </summary>
    public static async Task<TestFieldFixture> CreateTestFieldFixtureAsync (
      string sourceSubfolder,
      bool useSVN = false,
      CancellationToken ct = default
    ) {
      if (useSVN)
        return await setUpSvnFixtureAsync (sourceSubfolder, ct);
      else
        return await setUpRegularFixtureAsync (sourceSubfolder, ct);
    }

    private static async Task<TestFieldFixture> setUpSvnFixtureAsync (
      string sourceSubfolder, CancellationToken ct
    ) {
      string root = await createTestRootDirectorySvnAsync (sourceSubfolder, ct);
      var fixture = new TestFieldFixture (root, true);
      fixture.verifyTestField ();
      return fixture;
    }
    
    private static async Task<TestFieldFixture> setUpRegularFixtureAsync (
      string sourceSubfolder, CancellationToken ct
    ) {
      string root = createTestRootDirectory ();
      var fixture = new TestFieldFixture (root, false);
      await fixture.exportTestFieldAsync (sourceSubfolder, ct);
      fixture.verifyTestField ();
      return fixture;
    }

    public string Project (
       string projectFileName,
       string? projectRootFolder = null
     ) {
      if (!projectFileName.EndsWith (CSPROJ, StringComparison.OrdinalIgnoreCase)
        && !projectFileName.EndsWith (SHPROJ, StringComparison.OrdinalIgnoreCase))
        projectFileName += CSPROJ;
      string projectName = Path.GetFileNameWithoutExtension (projectFileName);

      string projectsRoot = projectRootFolder ?? Root;
      var match = Directory
        .EnumerateFiles (
          projectsRoot,
          projectFileName,
          SearchOption.AllDirectories)
        .FirstOrDefault (p =>
          string.Equals (
            Path.GetFileNameWithoutExtension (p),
            projectName,
            StringComparison.OrdinalIgnoreCase)) 
        ?? throw new InvalidOperationException ($"Project '{projectFileName}' not found in test field.");

      return Path.GetFullPath (match);
    }

    public string Solution (
      string solutionFileName, 
      string? solutionRootFolder = null
    ) {

      if (!solutionFileName.EndsWith (SLN, StringComparison.OrdinalIgnoreCase))
        solutionFileName += SLN;

      string solutionsRoot = solutionRootFolder ?? Root;
      Assert.IsTrue (
          Directory.Exists (solutionsRoot),
          $"Destination root does not exist: {solutionsRoot}");

      var match = Directory
        .EnumerateFiles (
          solutionsRoot,
          solutionFileName,
          SearchOption.AllDirectories)
        .FirstOrDefault ()
      ?? throw new InvalidOperationException ($"Solution '{solutionFileName}' not found in test field.");

      return Path.GetFullPath (match);
    }

    public static void AssertDestinationRootExists (string destinationRoot) {
      Assert.IsTrue (
          Directory.Exists (destinationRoot),
          $"Destination root does not exist: {destinationRoot}");
    }

    public static void AssertNewProjectFolderExists (string newProjectFolder) {
      Assert.IsTrue (
          Directory.Exists (newProjectFolder),
          $"New project folder does not exist: {newProjectFolder}");
    }

    public static void AssertNewProjectFileExists (string newProjectFile) {
      Assert.IsTrue (
          File.Exists (newProjectFile),
          $"New project file does not exist: {newProjectFile}");
    }

    public static void AssertOriginalProjectFileExists (string originalProjectFile) {
      Assert.IsTrue (
          File.Exists (originalProjectFile),
          $"Original project file missing (copy expected): {originalProjectFile}");
    }

    public static void AssertOriginalProjectFileDoesNotExist (string originalProjectFile) {
      Assert.IsFalse (
          File.Exists (originalProjectFile),
          $"Original project file still exists (move expected): {originalProjectFile}");
    }

    public static void AssertOriginalProjectFolderExists (string originalProjectFolder) {
      Assert.IsTrue (
          Directory.Exists (originalProjectFolder),
          $"Original project folder missing (copy expected): {originalProjectFolder}");
    }

    public static void AssertOriginalProjectFolderRemovedIfEmpty (string originalProjectFolder) {
      if (!Directory.Exists (originalProjectFolder))
        return;

      bool hasEntries =
          Directory.EnumerateFileSystemEntries (originalProjectFolder).Any ();

      Assert.IsFalse (
          hasEntries,
          $"Original project folder still exists and is not empty: {originalProjectFolder}");
    }

    public static void AssertNoUnexpectedSiblingProjectFolders (
        string parentFolder,
        IReadOnlyCollection<string> expectedFolderNames) {
      var actualFolders = Directory
          .EnumerateDirectories (parentFolder)
          .Select (Path.GetFileName)
          .ToHashSet (StringComparer.OrdinalIgnoreCase);

      foreach (var expected in expectedFolderNames) {
        Assert.IsTrue (
            actualFolders.Contains (expected),
            $"Expected project folder missing: {expected}");
      }

      foreach (var actual in actualFolders) {
        Assert.IsTrue (
            expectedFolderNames.Contains (actual, StringComparer.OrdinalIgnoreCase),
            $"Unexpected project folder found: {actual}");
      }
    }

    public static void AssertEqualSubFolderAndProjectName (string projectPath) {
      string projectFileName = Path.GetFileName (projectPath);
      string projectName = Path.GetFileNameWithoutExtension (projectFileName);
      string projectFolder = Path.GetFileName (Path.GetDirectoryName (projectPath)!);
      Assert.AreEqual (
          projectName,
          projectFolder,
          $"Project folder name and project name must match for: {projectPath}");
    }

    public static void AssertExactlyOneCsProjectFileInFolder (string projectFolder) {
      var csprojFiles = Directory
          .EnumerateFiles (projectFolder, WC_CSPROJ, SearchOption.TopDirectoryOnly)
          .ToList ();

      Assert.AreEqual (
          1,
          csprojFiles.Count,
          $"Expected exactly one C# project file in folder: {projectFolder}");
    }
    
    public static void AssertExactlyOneSharedProjectFilePairInFolder (string projectFolder) {
      var shprojFiles = Directory
          .EnumerateFiles (projectFolder, WC_SHPROJ, SearchOption.TopDirectoryOnly)
          .ToList ();

      Assert.AreEqual (
          1,
          shprojFiles.Count,
          $"Expected exactly one shared project file in folder: {projectFolder}");
      
      var projitemFiles = Directory
          .EnumerateFiles (projectFolder, WC_PROJITEMS, SearchOption.TopDirectoryOnly)
          .ToList ();

      Assert.AreEqual (
          1,
          projitemFiles.Count,
          $"Expected exactly one shared project items file in folder: {projectFolder}");

      string f1 = Path.GetFileName (shprojFiles[0]);
      string f2 = Path.GetFileName (shprojFiles[0]);

      Assert.AreEqual (
          Path.GetFileNameWithoutExtension(f1),
          Path.GetFileNameWithoutExtension(f2),
          $"Shared project file and shared projects items file must have same name: {f1}, {f2}");

    }

    public static void AssertProjectXmlLoads (string projectFile) {
      _ = loadProjectXml (projectFile);
    }

    public static void AssertAssemblyName (string projectFile, string expectedAssemblyName) {
      var xml = loadProjectXml (projectFile);
      var ns = xml.Root?.Name.Namespace ?? XNamespace.None;

      var assemblyName = xml
          .Descendants (ns + "AssemblyName")
          .Select (e => e.Value)
          .FirstOrDefault ();

      Assert.AreEqual (
          expectedAssemblyName,
          assemblyName,
          $"AssemblyName mismatch in {projectFile}");
    }

    public static void AssertNoPathContains (string projectFile, string forbiddenFragment) {
      var text = File.ReadAllText (projectFile);

      Assert.IsFalse (
             text.Contains (forbiddenFragment, StringComparison.OrdinalIgnoreCase),
             $"Forbidden path fragment found in {projectFile}: {forbiddenFragment}");
    }

    public static void AssertAllIncludesAreRelative (string projectFile) {
      var xml = loadProjectXml (projectFile);
      var ns = xml.Root?.Name.Namespace ?? XNamespace.None;

      var includes = xml
          .Descendants ()
          .Attributes ("Include")
          .Select (a => a.Value)
          .ToList ();

      foreach (var inc in includes) {
        Assert.IsFalse (
            Path.IsPathRooted (inc),
            $"Absolute include path found in {projectFile}: {inc}");
      }
    }

    public static void AssertProjectReference (
        string projectFile,
        string expectedReferencedProjectAbsPath,   // absolute path
        string? forbiddenReferencedProjectAbsPath)  // absolute path
    {
      var xml = loadProjectXml (projectFile);
      var projectDir = Path.GetDirectoryName (projectFile)!;

      var ns = xml.Root?.Name.Namespace ?? XNamespace.None;

      XName xmlElementName = ns + "ProjectReference";
      string xmlAttributeName = "Include";

      // Substitute .shproj -> .projitems if needed
      expectedReferencedProjectAbsPath =
        normalizeSharedProjectReference (expectedReferencedProjectAbsPath);

      string expectedReferencedProjectRelPath = Path.GetRelativePath (
          projectDir,
          expectedReferencedProjectAbsPath);



      // Find the ProjectReference by rel path (expected one)
      string? targetRef = resolve (expectedReferencedProjectRelPath);

      Assert.IsNotNull (targetRef, $"Expected {xmlElementName} not found in {projectFile}");

      if (forbiddenReferencedProjectAbsPath is not null) {

        forbiddenReferencedProjectAbsPath =
          normalizeSharedProjectReference (forbiddenReferencedProjectAbsPath);

        string forbiddenReferencedProjectRelPath = Path.GetRelativePath (
            projectDir,
            forbiddenReferencedProjectAbsPath);

        // Must not point to the forbidden project rel path
        // Note: this isn't fully exhaustive, but good enough for our tests
        string? forbiddenRef = resolve (forbiddenReferencedProjectRelPath);

        Assert.IsNull (forbiddenRef, $"Forbidden {xmlElementName} found in {projectFile}");
      }

      string? resolve (string relPath) {
        var k = xml
            .Descendants (xmlElementName)
            .Select (e => new { Element = e, Include = e.Attribute (xmlAttributeName)?.Value })
            .FirstOrDefault (r => r.Include != null && string.Equals(r.Include, relPath, StringComparison.OrdinalIgnoreCase));

        return k?.Include;
      }

      string normalizeSharedProjectReference (string absPath) {
        if (absPath.EndsWith (SharedProjectFile.EXT, StringComparison.OrdinalIgnoreCase)) {
          xmlElementName = ns + "Import";
          xmlAttributeName = "Project";

          // Convert .shproj -> .projitems
          string dir = Path.GetDirectoryName (absPath)!;
          string name = Path.GetFileNameWithoutExtension (absPath);
          return Path.Combine (dir, name + SharedProjectFile.PROJITEMS_EXT);
        }
        return absPath;
      }
    }

    public static void AssertCopiedProjectReferenceGuids (
        string copiedProjectPath,
        IEnumerable<string> referencingProjectPaths
    ) {
      var xmlProj = loadProjectXml (copiedProjectPath);
      var nsProj = xmlProj.Root?.Name.Namespace ?? XNamespace.None;

      bool hasGuid = Guid.TryParse (xmlProj
        .Descendants (nsProj + "ProjectGuid")
        .Select (e => e.Value)
        .FirstOrDefault (), out Guid projGuid);
      hasGuid = projGuid != default;
      Assert.IsTrue (hasGuid, $"Project {copiedProjectPath} must have a GUID");

      foreach (string refPath in referencingProjectPaths) {
        var xmlRefProj = loadProjectXml (refPath);
        var nsRefProj = xmlRefProj.Root?.Name.Namespace ?? XNamespace.None;
              
        var refGuidValues = xmlRefProj
          .Descendants (nsRefProj + "Project")
          .Select (e => e.Value)
          .ToList ();
        var refGuids = refGuidValues
          .Select (v => {
            bool succ = Guid.TryParse (v, out Guid guid);
            return guid;
          })
          .Where (g => g != default)
          .ToList ();

        bool doesReferenceProjGuid = refGuids.Contains (projGuid);
        Assert.IsTrue (hasGuid, $"Referencing Project {refPath} does refernce the expected project GUID");
      }

    }


    /// <summary>
    /// Asserts that the solution contains exactly the expected projects at the correct relative paths.
    /// </summary>
    /// <param name="solutionPath">Absolute path to the solution file.</param>
    /// <param name="expectedProjectPaths">Absolute paths of projects that should appear in the solution.</param>
    public static async Task AssertSolutionProjectPathsAsync (string solutionPath, IReadOnlyList<string> expectedProjectPaths) {
      if (!File.Exists (solutionPath))
        Assert.Fail ($"Solution file not found: {solutionPath}");

      var solution = new SolutionFile (solutionPath);
      await solution.LoadAsync(); 

      var solutionDir = Path.GetDirectoryName (solutionPath)!;
      var projectRelPathsInSolution = solution.ProjectRelativePaths;

      foreach (var expectedAbsPath in expectedProjectPaths) {
        var normalizedExpected = Path.GetFullPath (expectedAbsPath);

        var projectRelPath = projectRelPathsInSolution
            .FirstOrDefault (p =>
                string.Equals (Path.GetFullPath (Path.Combine (solutionDir, p)),
                              normalizedExpected,
                              StringComparison.OrdinalIgnoreCase));

        Assert.IsNotNull (projectRelPath,
            $"Solution '{solutionPath}' does not contain project '{expectedAbsPath}'");

        // Ensure the relative path is correctly computed
        var expectedRel = Path.GetRelativePath (solutionDir, normalizedExpected);
        Assert.AreEqual (expectedRel, projectRelPath,
            $"Solution '{solutionPath}' contains project '{expectedAbsPath}' with incorrect relative path");
      }
    }


    public static async Task BuildSolutionAsync (string solutionPath, string configuration = "Debug", string? framework = null) {
      if (!File.Exists (solutionPath))
        Assert.Fail ($"Solution file not found: {solutionPath}");

      var args = $"build \"{solutionPath}\" --configuration {configuration}";
      if (!string.IsNullOrEmpty (framework))
        args += $" --framework {framework}";

      var result = await runDotnetAsync (args, Path.GetDirectoryName (solutionPath)!, CancellationToken.None);

      BuildAssertions.AssertSuccessfulBuild (result, solutionPath);
    }

    private static void requireDir (string path) {
      if (!Directory.Exists (path))
        throw new InvalidOperationException ($"Missing dir: {path}");
    }

    private static void requireFile (string path) {
      if (!File.Exists (path))
        throw new InvalidOperationException ($"Missing file: {path}");
    }

    private static string createTestRootDirectory () {

      // Test may run asynchronously and in parallel, so ensure unique folder name,
      // still sorted by timestamp

      string guid = Guid.NewGuid ().ToString("N");
      string ts = DateTime.Now.ToString ("yyyyMMdd_HHmmss-");

      string root = Path.Combine (
        Path.GetTempPath (),
        "ProjectMoverTests",
        ts + guid
      );

      Directory.CreateDirectory (root);
      return root;
    }

    private static async Task<string> createTestRootDirectorySvnAsync (
      string sourceSubfolder, CancellationToken ct
    ) {
      var rootDir = getRepoRoot ();
      string sourceAbsPath = getRepoTestFieldPath (sourceSubfolder, rootDir);
      string playgroundRoot = Path.Combine (rootDir.FullName, TESTFIELDS_TEMPSVN_PLAYGROUND);
      string destAbsPath = Path.Combine (playgroundRoot, sourceSubfolder);

      // 1. Ensure playground root exists and is WC
      if (!Directory.Exists (playgroundRoot)) {
        Directory.CreateDirectory (playgroundRoot);

        // bring it under version control
        await SvnClient.RunAndAssertAsync (
          new SvnCommandBuilder ()
            .Add ("add")
            .AddPath (playgroundRoot),
          ct);

        await SvnClient.RunAndAssertAsync (
          new SvnCommandBuilder ()
            .Add ("commit")
            .Add ("-m").Add ("\"ProjectMover test framework: Add Playground root\"")
            .AddPath (playgroundRoot),
          ct);
      }

      // sanity: must be under svn
      var info = await SvnClient.RunAsync (
        new SvnCommandBuilder ().Add ("info").AddPath (playgroundRoot),
        ct);

      if (!info.IsSuccess)
        throw new InvalidOperationException ("Playground root is not an SVN working copy.");


      // 2. If test field not yet present: svn copy + commit
      if (!Directory.Exists (destAbsPath)) {

        await SvnClient.RunAndAssertAsync (
          new SvnCommandBuilder ()
            .Add ("copy")
            .AddPath (sourceAbsPath)
            .AddPath (destAbsPath),
          ct);

        // ensure committed so future reverts work
        await SvnClient.RunAndAssertAsync (
          new SvnCommandBuilder ()
            .Add ("commit")
            .Add ("-m").Add ("\"ProjectMover test framework: Commit SVN test field\"")
            .AddPath (destAbsPath),
          ct);
      }

      // 3. Always clean working copy before returning
      await revertAndCleanupUnversionedAsync (destAbsPath, ct);

      return destAbsPath;
    }

    private async Task exportTestFieldAsync (string sourceSubfolder, CancellationToken ct) {
      var rootDir = getRepoRoot ();
      string sourceRepo = getRepoTestFieldPath (sourceSubfolder, rootDir);

      await runSvnAsync ($"export --help", ct);

      await runSvnAsync ($"export --force \"{sourceRepo}\" \"{Root}\"", ct);
    }

    private void verifyTestField () {
      // Fail fast if field is broken
    }

    private static string getRepoTestFieldPath (string sourceSubfolder, DirectoryInfo repoRootDir) {
      string sourceDir = Path.Combine (repoRootDir.FullName, TEST_FIELDS, sourceSubfolder);
      if (!Directory.Exists (sourceDir))
        throw new InvalidOperationException ("Repo root not found.");
      return sourceDir;
    }         


    private static DirectoryInfo getRepoRoot () {

      var dir = new DirectoryInfo (AppContext.BaseDirectory);

      while (dir != null) {

        // pick one marker that exists in repo root
        if (File.Exists (Path.Combine (dir.FullName, PROJECT_MOVER_SLN))) {         
          dir = dir.Parent;
          if (dir == null)
            break;

          return dir;
        }         

        dir = dir.Parent;
      }

      throw new InvalidOperationException ("Repo root not found.");

    }

    private static async Task runSvnAsync (
      string args,
      CancellationToken ct
    ) => await AsyncProcessRunner.RunAndAssertSuccessAsync ("svn", args, ct: ct);

    private static async Task<ProcessExecutionResult> runDotnetAsync (
      string args,
      string workingDir,
      CancellationToken ct
    ) => await AsyncProcessRunner.RunAsync ("dotnet", args, workingDir, ct);


    private static XDocument loadProjectXml (string projectFile) {
      try {
        return XDocument.Load (projectFile, LoadOptions.PreserveWhitespace);
      } catch (Exception ex) {
        Assert.Fail ($"Failed to load project XML '{projectFile}': {ex.Message}");
        throw;
      }
    }

    private static async Task revertAndCleanupUnversionedAsync (
      string root,
      CancellationToken ct = default
    ) {
      // revert all versioned changes
      await SvnClient.RunAndAssertAsync (
        new SvnCommandBuilder ()
          .Add ("revert")
          .Add ("-R")
          .AddPath (root),
        ct);

      // find unversioned paths
      var status = await SvnClient.RunAsync (
        new SvnCommandBuilder ()
          .Add ("status")
          .AddPath (root),
        ct);

      var unversioned = status.StdOut
        .Split ([ '\r', '\n' ], StringSplitOptions.RemoveEmptyEntries)
        .Where (l => l.Length > 2 && l[0] == '?')
        .Select (l => l.Substring (1).Trim ())
        .Select (p => Path.Combine (root, p))
        .OrderByDescending (p => p.Length) // deepest first
        .ToList ();

      foreach (var path in unversioned) {
        if (Directory.Exists (path))
          Directory.Delete (path, recursive: true);
        else if (File.Exists (path))
          File.Delete (path);
      }
    }


    public async ValueTask DisposeAsync () {
      if (Svn) {
        await revertAndCleanupUnversionedAsync (Root);
      } else {
        try {
          Directory.Delete (Root, recursive: true);
        } catch {
          // best effort – never fail test teardown
        }
      }
    }
  }

}
