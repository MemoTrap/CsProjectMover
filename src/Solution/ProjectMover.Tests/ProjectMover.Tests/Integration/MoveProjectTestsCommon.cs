using ProjectMover.Lib.Api;

namespace ProjectMover.Tests.Integration {
  using ProjectMover.Lib;
  using ProjectMover.Lib.Extensions;

  using static ProjectMover.Tests.Const;
  using static ProjectMover.Tests.ConfirmationAssertions;
  using static ProjectMover.Tests.TestFieldFixture;
  using ProjectMover.Global.Lib;

  internal static class MoveProjectTestsCommon {

    public static async Task Move01_ALibCore (TestFieldFixture field, bool svn = false) {
      // Arrange

      string projNameA = A_LIB_CORE;
      string projFileA = projNameA + CSPROJ;
      string oldProjPathA = field.Project (projFileA);

      string destFolderRoot = Path.Combine (field.Root, RELOCATED);

      string newProjFolderA = Path.Combine (destFolderRoot, projNameA);
      string newProjPathA = Path.Combine (newProjFolderA, projFileA);

      string affectedProjPathB = field.Project (PROJ_B_LIB_UTIL);

      string affectedSolutionPath = field.Solution (SLN_SOL_APP);

      var parameters = TestParametersFactory.MoveSingleProject (
             field,
             projFileA,
             destFolderRoot,
             svn);

      var decisionProvider = new ScriptedDecisionProvider (
              new Dictionary<string, ProjectUserDecision> {
                [projNameA] = new ProjectUserDecision (
                    Include: true,
                    NewProjectName: null,
                    NewProjectFolder: newProjFolderA,
                    NewAssemblyName: null,
                    SelectedSolutions: null,
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
        [
            oldProjPathA,
            affectedProjPathB,
            field.Project(PROJ_C_LIB_APP),
            field.Project(PROJ_D_LIB_PLUGIN),
            field.Project(PROJ_E_LIB_WITH_LINKS),
            field.Project(PROJ_F_SHARED_APP),
            field.Project(PROJ_G_MIXED_APP),
        ],
        expectedSolutionPaths:
        [
            affectedSolutionPath,
            field.Solution(SLN_SOL_CORE),
            field.Solution(SLN_SOL_LINKS),
            field.Solution(SLN_SOL_MIXED),
            field.Solution(SLN_SOL_PLUGIN),
            field.Solution(SLN_SOL_SHARED),
        ]);

      // Layer 1 - file system

      AssertDestinationRootExists (destFolderRoot);

      AssertNewProjectFolderExists (newProjFolderA);

      AssertNewProjectFileExists (newProjPathA);

      AssertOriginalProjectFileDoesNotExist (oldProjPathA);

      AssertExactlyOneCsProjectFileInFolder (newProjFolderA);


      // Layer 2 – projects

      AssertProjectXmlLoads (newProjPathA);
      AssertAllIncludesAreRelative (newProjPathA);

      AssertProjectReference (
          affectedProjPathB,
          expectedReferencedProjectAbsPath: newProjPathA,
          forbiddenReferencedProjectAbsPath: oldProjPathA);

      // Layer 3 – solution

      List<string> expectedProjectPathsInSolution = [
        newProjPathA,
        affectedProjPathB,
        field.Project(PROJ_C_LIB_APP),
        field.Project(PROJ_Z_LIB_STD)
      ];
      await AssertSolutionProjectPathsAsync (affectedSolutionPath, expectedProjectPathsInSolution);

      // Layer 4 – build

      await BuildSolutionAsync (affectedSolutionPath);
    }

    public static async Task Move02_ELibWithLinks (TestFieldFixture field, bool svn = false) {

      // Arrange

      string projNameE = E_LIB_WITH_LINKS;
      string projFileE = projNameE + CSPROJ;
      string oldProjPathE = field.Project (projFileE);

      string destFolderRoot = Path.Combine (field.Root, RELOCATED);

      string newProjFolderE = Path.Combine (destFolderRoot, projNameE);
      string newProjPathE = Path.Combine (newProjFolderE, projFileE);

      string affectedProjPathG = field.Project (PROJ_G_MIXED_APP);

      string affectedSolutionPath = field.Solution (SLN_SOL_MIXED);

      var parameters = TestParametersFactory.MoveSingleProject (
             field,
             projFileE,
             destFolderRoot,
             svn);

      var decisionProvider = new ScriptedDecisionProvider (
              new Dictionary<string, ProjectUserDecision> {
                [projNameE] = new ProjectUserDecision (
                    Include: true,
                    NewProjectName: null,
                    NewProjectFolder: newProjFolderE,
                    NewAssemblyName: null,
                    SelectedSolutions: null,
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
        [
            oldProjPathE,
            affectedProjPathG,
        ],
        expectedSolutionPaths:
        [
            affectedSolutionPath,
            field.Solution(SLN_SOL_LINKS),
        ]);

      // Layer 1 - file system

      AssertDestinationRootExists (destFolderRoot);

      AssertNewProjectFolderExists (newProjFolderE);

      AssertNewProjectFileExists (newProjPathE);

      AssertOriginalProjectFileDoesNotExist (oldProjPathE);

      AssertExactlyOneCsProjectFileInFolder (newProjFolderE);


      // Layer 2 – projects

      AssertProjectXmlLoads (newProjPathE);
      AssertAllIncludesAreRelative (newProjPathE);

      AssertProjectReference (
          affectedProjPathG,
          expectedReferencedProjectAbsPath: newProjPathE,
          forbiddenReferencedProjectAbsPath: oldProjPathE);

      // Layer 3 – solution

      List<string> expectedProjectPathsInSolution = [
        newProjPathE,
        affectedProjPathG,
        field.Project(PROJ_A_LIB_CORE),
        field.Project(PROJ_B_LIB_UTIL),
        field.Project(PROJ_S_SHARED_STUFF),
        field.Project(PROJ_Z_LIB_STD)
      ];
      await AssertSolutionProjectPathsAsync (affectedSolutionPath, expectedProjectPathsInSolution);

      // Layer 4 – build

      await BuildSolutionAsync (affectedSolutionPath);
    }

    public static async Task Move03_SSharedStuff (TestFieldFixture field, bool svn = false) {

      // Arrange

      string projNameS = S_SHARED_STUFF;
      string projFileS = projNameS + SHPROJ;
      string oldProjPathS = field.Project (projFileS);

      string destFolderRoot = Path.Combine (field.Root, RELOCATED);

      string newProjFolderS = Path.Combine (destFolderRoot, projNameS);
      string newProjPathS = Path.Combine (newProjFolderS, projFileS);

      string affectedProjPathD = field.Project (PROJ_D_LIB_PLUGIN);

      string affectedSolutionPath = field.Solution (SLN_SOL_PLUGIN);

      var parameters = TestParametersFactory.MoveSingleProject (
             field,
             projFileS,
             destFolderRoot,
             svn);

      var decisionProvider = new ScriptedDecisionProvider (
              new Dictionary<string, ProjectUserDecision> {
                [projNameS] = new ProjectUserDecision (
                    Include: true,
                    NewProjectName: null,
                    NewProjectFolder: newProjFolderS,
                    NewAssemblyName: null,
                    SelectedSolutions: null,
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
        [
            oldProjPathS,
            affectedProjPathD,
            field.Project(PROJ_F_SHARED_APP),
        ],
        expectedSolutionPaths:
        [
            affectedSolutionPath,
            field.Solution(SLN_SOL_MIXED),
            field.Solution(SLN_SOL_SHARED),
        ]);

      // Layer 1 - file system

      AssertDestinationRootExists (destFolderRoot);

      AssertNewProjectFolderExists (newProjFolderS);

      AssertNewProjectFileExists (newProjPathS);

      AssertOriginalProjectFileDoesNotExist (oldProjPathS);

      AssertExactlyOneSharedProjectFilePairInFolder (newProjFolderS);


      // Layer 2 – projects

      AssertProjectXmlLoads (newProjPathS);

      AssertProjectReference (
          affectedProjPathD,
          expectedReferencedProjectAbsPath: newProjPathS,
          forbiddenReferencedProjectAbsPath: oldProjPathS);

      // Layer 3 – solution

      List<string> expectedProjectPathsInSolution = [
        newProjPathS,
        affectedProjPathD,
        field.Project(PROJ_A_LIB_CORE),
        field.Project(PROJ_B_LIB_UTIL),
        field.Project(PROJ_Z_LIB_STD)
      ];
      await AssertSolutionProjectPathsAsync (affectedSolutionPath, expectedProjectPathsInSolution);

      // Layer 4 – build

      await BuildSolutionAsync (affectedSolutionPath);
    }

    public static async Task Move04_ALibCore_BLibUtil (TestFieldFixture field, bool svn = false) {

      // Arrange

      string projNameA = A_LIB_CORE;
      string projFileA = projNameA + CSPROJ;
      string oldProjPathA = field.Project (projFileA);
      string projNameB = B_LIB_UTIL;
      string projFileB = projNameB + CSPROJ;
      string oldProjPathB = field.Project (projFileB);

      string destFolderRoot = Path.Combine (field.Root, RELOCATED);

      string newProjPathA = oldProjPathA.ToNewDestinationPath (
          field.Root,
          destFolderRoot);
      string newProjFolderA = Path.GetDirectoryName (newProjPathA)!;
      string newProjPathB = oldProjPathB.ToNewDestinationPath (
          field.Root,
          destFolderRoot);
      string newProjFolderB = Path.GetDirectoryName (newProjPathB)!;

      string affectedProjPathC = field.Project (PROJ_C_LIB_APP);

      string affectedSolutionPath = field.Solution (SLN_SOL_APP);

      var parameters = TestParametersFactory.MoveMultiProjects (
             field,
             field.Root,
             destFolderRoot,
             svn);

      var decisionProvider = new ScriptedDecisionProvider (
              new Dictionary<string, ProjectUserDecision> {
                [projNameA] = new ProjectUserDecision (
                    Include: true,
                    NewProjectName: null,
                    NewProjectFolder: null,
                    NewAssemblyName: null,
                    SelectedSolutions: null,
                    SelectedDependentProjectRoots: null
                  ),
                [projNameB] = new ProjectUserDecision (
                    Include: true,
                    NewProjectName: null,
                    NewProjectFolder: null,
                    NewAssemblyName: null,
                    SelectedSolutions: null,
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
        [
            oldProjPathA,
            oldProjPathB,
            affectedProjPathC,
            field.Project(PROJ_D_LIB_PLUGIN),
            field.Project(PROJ_E_LIB_WITH_LINKS),
            field.Project(PROJ_F_SHARED_APP),
            field.Project(PROJ_G_MIXED_APP),
        ],
        expectedSolutionPaths:
        [
            affectedSolutionPath,
            field.Solution(SLN_SOL_CORE),
            field.Solution(SLN_SOL_LINKS),
            field.Solution(SLN_SOL_MIXED),
            field.Solution(SLN_SOL_PLUGIN),
            field.Solution(SLN_SOL_SHARED),
        ]);

      // Layer 1 - file system

      AssertDestinationRootExists (destFolderRoot);

      AssertNewProjectFolderExists (newProjFolderA);
      AssertNewProjectFolderExists (newProjFolderB);

      AssertNewProjectFileExists (newProjPathA);
      AssertNewProjectFileExists (newProjPathB);

      AssertOriginalProjectFileDoesNotExist (oldProjPathA);
      AssertOriginalProjectFileDoesNotExist (oldProjPathB);

      AssertExactlyOneCsProjectFileInFolder (newProjFolderA);
      AssertExactlyOneCsProjectFileInFolder (newProjFolderB);


      // Layer 2 – projects

      AssertProjectXmlLoads (newProjPathA);
      AssertAllIncludesAreRelative (newProjPathA);
      AssertProjectXmlLoads (newProjPathB);
      AssertAllIncludesAreRelative (newProjPathB);

      AssertProjectReference (
          affectedProjPathC,
          expectedReferencedProjectAbsPath: newProjPathA,
          forbiddenReferencedProjectAbsPath: oldProjPathA);
      AssertProjectReference (
          affectedProjPathC,
          expectedReferencedProjectAbsPath: newProjPathB,
          forbiddenReferencedProjectAbsPath: oldProjPathB);

      // Layer 3 – solution

      List<string> expectedProjectPathsInSolution = [
        newProjPathA,
        affectedProjPathC,
        field.Project(PROJ_C_LIB_APP),
        field.Project(PROJ_Z_LIB_STD)
      ];
      await AssertSolutionProjectPathsAsync (affectedSolutionPath, expectedProjectPathsInSolution);

      // Layer 4 – build

      await BuildSolutionAsync (affectedSolutionPath);
    }


    public static async Task Move05_08_Rename_ALibCore (     
      TestFieldFixture field, 
      bool setNewProjectName,
      bool setNewProjectFolder,
      bool svn = false
    ) {
      // Arrange

      string projNameA = A_LIB_CORE;
      string projFileA = projNameA + CSPROJ;
      string oldProjPathA = field.Project (projFileA);

      string? intermediateParentDir = Path.GetDirectoryName (oldProjPathA) ?? 
        throw new InvalidOperationException ($"{nameof(oldProjPathA)} does not have a parent directory.");
      string? destFolderRoot = Path.GetDirectoryName (intermediateParentDir) ?? 
        throw new InvalidOperationException ($"Expected dest folder root {nameof (oldProjPathA)} does not exist");
      
      const string RENAMED = "_renamed";
      
      string newProjNameA = setNewProjectName ? projNameA + RENAMED : projNameA;
      string newProjSubdirA = setNewProjectFolder ? projNameA + RENAMED : projNameA;

      string newProjFolderA = Path.Combine (destFolderRoot, newProjSubdirA);
      string newProjPathA = Path.Combine (newProjFolderA, newProjNameA + CSPROJ);

      string affectedProjPathB = field.Project (PROJ_B_LIB_UTIL);

      string affectedSolutionPath = field.Solution (SLN_SOL_APP);

      var parameters = TestParametersFactory.MoveSingleProject (
             field,
             projFileA,
             destFolderRoot,
             svn);

      var decisionProvider = new ScriptedDecisionProvider (
              new Dictionary<string, ProjectUserDecision> {
                [projNameA] = new ProjectUserDecision (
                    Include: true,
                    NewProjectName: newProjNameA,
                    NewProjectFolder: newProjFolderA,
                    NewAssemblyName: null,
                    SelectedSolutions: null,
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

      if (!(setNewProjectName || setNewProjectFolder)) {
        AssertNoConfirmationMessage (callbackSink.Messages);
        return;
      }

      // Callback confirmation messages

      AssertConfirmationMessage (
        callbackSink.Messages,
        expectedProjectPaths:
        [
            oldProjPathA,
            affectedProjPathB,
            field.Project(PROJ_C_LIB_APP),
            field.Project(PROJ_D_LIB_PLUGIN),
            field.Project(PROJ_E_LIB_WITH_LINKS),
            field.Project(PROJ_F_SHARED_APP),
            field.Project(PROJ_G_MIXED_APP),
        ],
        expectedSolutionPaths:
        [
            affectedSolutionPath,
            field.Solution(SLN_SOL_CORE),
            field.Solution(SLN_SOL_LINKS),
            field.Solution(SLN_SOL_MIXED),
            field.Solution(SLN_SOL_PLUGIN),
            field.Solution(SLN_SOL_SHARED),
        ]);

      // Layer 1 - file system

      AssertDestinationRootExists (destFolderRoot);

      AssertNewProjectFolderExists (newProjFolderA);

      AssertNewProjectFileExists (newProjPathA);

      if (setNewProjectName || setNewProjectFolder)
        AssertOriginalProjectFileDoesNotExist (oldProjPathA);

      AssertExactlyOneCsProjectFileInFolder (newProjFolderA);


      // Layer 2 – projects

      AssertProjectXmlLoads (newProjPathA);
      AssertAllIncludesAreRelative (newProjPathA);

      string? forbiddenOldProjPathA = null;
      if (setNewProjectName || setNewProjectFolder)
        forbiddenOldProjPathA = oldProjPathA;

      AssertProjectReference (
          affectedProjPathB,
          expectedReferencedProjectAbsPath: newProjPathA,
          forbiddenReferencedProjectAbsPath: forbiddenOldProjPathA);

      // Layer 3 – solution

      List<string> expectedProjectPathsInSolution = [
        newProjPathA,
        affectedProjPathB,
        field.Project(PROJ_C_LIB_APP),
        field.Project(PROJ_Z_LIB_STD)
      ];
      await AssertSolutionProjectPathsAsync (affectedSolutionPath, expectedProjectPathsInSolution);

      // Layer 4 – build

      await BuildSolutionAsync (affectedSolutionPath);
    }


  }
}

