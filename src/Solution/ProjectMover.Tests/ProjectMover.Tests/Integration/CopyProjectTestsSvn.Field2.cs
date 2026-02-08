#pragma warning disable IDE0305

namespace ProjectMover.Tests.Integration {
  using ProjectMover.Lib;
  using ProjectMover.Lib.Api;

  using static ProjectMover.Tests.ConfirmationAssertions;
  using static ProjectMover.Tests.Const;
  using static ProjectMover.Tests.TestFieldFixture;

  [DoNotParallelize]
  [TestClass]
  public sealed class Tests05_CopyProjectsSvn_Field2 {

    private TestFieldFixture _field = null!;
    private const bool USE_SVN = true;

    [TestInitialize]
    public async Task SetupAsync () {
      _field = await CreateTestFieldFixtureAsync (TEST_FIELD_2, USE_SVN);
    }

    [TestCleanup]
    public async Task CleanupAsync () {
      if (_field is not null)
        await _field.DisposeAsync ();
    }

    [TestMethod]
    public async Task CopySvn01_RpcAppTemplate () {
      // Arrange

      string projectFolder = Path.Combine (_field.Root, RPC_APP_TEMPLATE);
      string destinationFolder = Path.Combine (_field.Root, APPLICATION);
      string solutionFolder = destinationFolder;

      // Get them before the act, as thew will exist twice after the copy 
      string[] origProjectPaths =
      [
          _field.Project (PROJ_CLIENT_CONSOLE_APP),
          _field.Project (PROJ_CLIENT_GUI_APP),
          _field.Project (PROJ_CLIENT_LIB),
          _field.Project (PROJ_CLIENT_PROXY),
          _field.Project (PROJ_COMMON_BCL),
          _field.Project (PROJ_COMMON_CONTRACT),
          _field.Project (PROJ_SERVER_APP),
          _field.Project (PROJ_SERVER_BIZ),
          _field.Project (PROJ_SERVER_HOST),
          _field.Project (PROJ_SERVER_SERVICE)
      ];

      string[] expectedConstProjectPaths =
      [
          _field.Project (PROJ_RPC_CLIENT),
          _field.Project (PROJ_RPC_CLIENT_PROXY_GENERATOR),
          _field.Project (PROJ_RPC_CLIENT_WINFORMS_CONTROLS),
          _field.Project (PROJ_RPC_COMMON),
          _field.Project (PROJ_RPC_SERVER),
      ];


      string affectedSolutionPath = _field.Solution (SLN_APPLICATION);

      var parameters = TestParametersFactory.CopyMultiProjects (
             _field,
             projectFolder,
             destinationFolder,
             solutionFolder,
             true);

      var decisionProvider = new ScriptedDecisionProvider (
              new Dictionary<string, ProjectUserDecision> {
                [WILDCARD] = new ProjectUserDecision (
                    Include: true,
                    NewProjectName: null,
                    NewProjectFolder: null,
                    NewAssemblyName: null,
                    SelectedSolutions: [affectedSolutionPath],
                    SelectedDependentProjectRoots: null
                  )
              });

      CapturingCallbackSink callbackSink = new ();

      // Act

      var mover = new CsProjectMover (
        new NullProgressSink (),
        callbackSink,
        decisionProvider
      );

      await mover.RunAsync (parameters, CancellationToken.None);


      // Assert

      // Callback confirmation messages

      AssertConfirmationMessage (
        callbackSink.Messages,
        expectedProjectPaths:
          origProjectPaths,
        expectedSolutionPaths:
        [
            affectedSolutionPath
        ]);

      //// Layer 1 - file system

      AssertDestinationRootExists (destinationFolder);

      string[] newProjectPaths =
        [
            _field.Project (PROJ_CLIENT_CONSOLE_APP, destinationFolder),
            _field.Project (PROJ_CLIENT_GUI_APP, destinationFolder),
            _field.Project (PROJ_CLIENT_LIB, destinationFolder),
            _field.Project (PROJ_CLIENT_PROXY, destinationFolder),
            _field.Project (PROJ_COMMON_BCL, destinationFolder),
            _field.Project (PROJ_COMMON_CONTRACT, destinationFolder),
            _field.Project (PROJ_SERVER_APP, destinationFolder),
            _field.Project (PROJ_SERVER_BIZ, destinationFolder),
            _field.Project (PROJ_SERVER_HOST, destinationFolder),
            _field.Project (PROJ_SERVER_SERVICE, destinationFolder)
        ];

      // we only get here if the paths exist
      foreach (string newProjPath in newProjectPaths) {
        AssertEqualSubFolderAndProjectName (newProjPath);
        AssertExactlyOneCsProjectFileInFolder (Path.GetDirectoryName(newProjPath)!);
      }

      foreach (string origProjPath in origProjectPaths) {
        AssertOriginalProjectFileExists (origProjPath);
        AssertExactlyOneCsProjectFileInFolder (Path.GetDirectoryName(origProjPath)!);
      }



      // Layer 2 – projects
      foreach (string newProjPath in newProjectPaths) {

        AssertProjectXmlLoads (newProjPath);
        AssertAllIncludesAreRelative (newProjPath);

        decisionProvider.LoggedContextDecisionsByProjectName.TryGetValue (
            Path.GetFileNameWithoutExtension (newProjPath),
            out var contextWithDecision);
        Assert.IsNotNull (contextWithDecision, $"No decision logged for project '{newProjPath}'");
        Assert.IsNotNull (contextWithDecision.Context.SuggestedAssemblyName, 
          $"No new assembly name was suggested for project '{newProjPath}'");
        AssertAssemblyName (newProjPath, contextWithDecision.Context.SuggestedAssemblyName);
      }


      // Layer 3 – solution

      List<string> expectedProjectPathsInSolution = newProjectPaths.ToList (); 
      expectedProjectPathsInSolution.AddRange (expectedConstProjectPaths);

      await AssertSolutionProjectPathsAsync (affectedSolutionPath, expectedProjectPathsInSolution);

      // Layer 4 – build

      await BuildSolutionAsync (affectedSolutionPath);
    }

  }
}
