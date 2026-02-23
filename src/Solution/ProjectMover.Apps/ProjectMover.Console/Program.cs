using ProjectMover.Lib.Api;
using ProjectMover.Lib.Misc;

namespace ProjectMover.ConsApp {
  internal class Program {
    /// <summary>
    /// Entry point for the application that initializes progress, callback, and decision providers, 
    /// configures parameters based on command-line argument, and runs the project mover asynchronously.
    /// </summary>
    /// <param name="args">Command-line argument used to determine parameters configuration,
    /// with "svn" as the only non-default option.</param>
    /// <remarks>
    /// Test field environment setup is required for this to work properly.
    /// Create with testfield1.cmd or testfield1-svn.cmd in the TestFields folder.
    /// </remarks> 
    static async Task Main (string[] args) {
      ConsoleProgressSink progress = new ();
      ConsoleCallbackSink callback = new ();
      ConsoleProjectDecisionProvider decisionProvider = new ();

      bool useSvn = args.Length > 0 && args[0] == "svn";
      bool useTestField2 = args.Length > 0 && args[0] == "2";

      Parameters parameters = useSvn ? 
        makeSvnParameters () :
        (useTestField2 ? makeRegularParameters2 () : makeRegularParameters ());
      consoleWriteParameters (parameters);

      Lib.CsProjectMover mover = new (progress, callback, decisionProvider);
      await mover.RunAsync (parameters);
    }

    private static Parameters makeRegularParameters () {
      string topDir = findTestFieldRootDir ();
      string rootDir = Path.Combine (topDir, @"Temp\Playground");
      Parameters parameters = new () {
        MultiMode = EMultiMode.Projects | EMultiMode.Solutions,
        RootFolder = rootDir,
        ProjectFolderOrFile = @"ProjectMover.TestField1", // Path.Combine (rootDir, @"ProjectMover.TestField1"),
        DestinationFolder = @"scratch", // Path.Combine (rootDir, @"scratch"),
        SolutionFolderOrFile = @"ProjectMover.TestField1", // Path.Combine (rootDir, @"ProjectMover.TestField1"),
        ProjectRootRecursionDepth = 5,
        Copy = true,
        Rescan = false,
        FileOperations = EFileOperations.Direct,
        AbsPathsInUserCommunication = false
      };

      return parameters;
    }

    private static Parameters makeSvnParameters () {
      string topDir = findTestFieldRootDir ();
      string rootDir = Path.Combine (topDir, @"Temp-svn\Playground");
      Parameters parameters = new () {
        MultiMode = EMultiMode.Projects | EMultiMode.Solutions,
        RootFolder = rootDir,
        ProjectFolderOrFile = Path.Combine (rootDir, @"ProjectMover.TestField1"),
        DestinationFolder = Path.Combine (rootDir, @"scratch"),
        SolutionFolderOrFile = Path.Combine (rootDir, @"ProjectMover.TestField1"),
        ProjectRootRecursionDepth = 5,
        Copy = false,
        Rescan = false,
        FileOperations = EFileOperations.Svn,
        AbsPathsInUserCommunication = true
      };

      return parameters;
    }

    private static Parameters makeRegularParameters2 () {
      string topDir = findTestFieldRootDir ();
      string rootDir = Path.Combine (topDir, @"Temp\Playground");
      Parameters parameters = new () {
        MultiMode = EMultiMode.Projects | EMultiMode.Solutions,
        RootFolder = Path.Combine (rootDir, @"ProjectMover.TestField2"),
        ProjectFolderOrFile = Path.Combine (rootDir, @"ProjectMover.TestField2\RpcAppTemplate"),
        DestinationFolder = Path.Combine (rootDir, @"ProjectMover.TestField2\Application"),
        SolutionFolderOrFile = Path.Combine (rootDir, @"ProjectMover.TestField2\Application"),
        ProjectRootRecursionDepth = 5,
        Copy = true,
        Rescan = false,
        FileOperations = EFileOperations.Direct,
        AbsPathsInUserCommunication = true
      };

      return parameters;
    }

    private static void consoleWriteParameters (Parameters parameters) {
      Console.WriteLine ("Parameters:");
      Console.WriteLine ($"  {nameof (Parameters.MultiMode)}: {parameters.MultiMode}");
      Console.WriteLine ($"  {nameof (Parameters.RootFolder)}: {parameters.RootFolder}");
      Console.WriteLine ($"  {nameof (Parameters.ProjectFolderOrFile)}: {parameters.ProjectFolderOrFile}");
      Console.WriteLine ($"  {nameof (Parameters.DestinationFolder)}: {parameters.DestinationFolder}");
      Console.WriteLine ($"  {nameof (Parameters.SolutionFolderOrFile)}: {parameters.SolutionFolderOrFile}");
      Console.WriteLine ($"  {nameof (Parameters.ProjectRootRecursionDepth)}: {parameters.ProjectRootRecursionDepth}");
      Console.WriteLine ($"  {nameof (Parameters.Copy)}: {parameters.Copy}");
      Console.WriteLine ($"  {nameof (Parameters.Rescan)}: {parameters.Rescan}");
      Console.WriteLine ($"  {nameof (Parameters.FileOperations)}: {parameters.FileOperations}");
      Console.WriteLine ();
    }

    private static string findTestFieldRootDir () {
      var dir = new DirectoryInfo (AppContext.BaseDirectory);

      while (dir != null) {

        // pick one marker that exists in repo root
        if (File.Exists (Path.Combine (dir.FullName, "ProjectMover.sln"))) {
          dir = dir.Parent;
          if (dir is null)
            break;
          string testFieldsRootDir = Path.Combine (dir.FullName, "TestFields");
          if (!Directory.Exists (testFieldsRootDir))
            throw new InvalidOperationException ("Test field root not found.");
          return testFieldsRootDir;
        }

        dir = dir.Parent;
      }

      throw new InvalidOperationException ("Test field root not found.");
    }
  }
}
