using ProjectMover.Lib.Api;

namespace ProjectMover.Tests.Integration {
  using ProjectMover.Lib;

  using static ProjectMover.Tests.ConfirmationAssertions;
  using static ProjectMover.Tests.Const;
  using static ProjectMover.Tests.TestFieldFixture;

  internal static class MoveProjectTestsCommon_Field3 {

    public static async Task Move01_ClassLibrary2 (TestFieldFixture field, bool svn = false) {
      // Arrange

      string projNameCL2 = CLASS_LIBRARY_2;
      string projFileCL2 = PROJ_CLASS_LIBRARY_2;
      string oldProjPathCL2 = field.Project (projFileCL2);

      string destFolderRoot = Path.Combine (field.Root, RELOCATED);

      string newProjFolderCL2 = Path.Combine (destFolderRoot, projNameCL2);
      string newProjPathCL2 = Path.Combine (newProjFolderCL2, projFileCL2);

      string affectedProjPathCA2 = field.Project (PROJ_CONSOLE_APP_2);
      string affectedProjPathCA3 = field.Project (PROJ_CONSOLE_APP_3);

      string affectedSolutionPath = field.Solution (SLN_NET_FRAMEWORK_1);

      var parameters = TestParametersFactory.MoveSingleProject (
             field,
             projFileCL2,
             destFolderRoot,
             svn);

      var decisionProvider = new ScriptedDecisionProvider (
              new Dictionary<string, ProjectUserDecision> {
                [projNameCL2] = new ProjectUserDecision (
                    Include: true,
                    NewProjectName: null,
                    NewProjectFolder: newProjFolderCL2,
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
            oldProjPathCL2,
            affectedProjPathCA2,
            affectedProjPathCA3,
        ],
        expectedSolutionPaths:
        [
            affectedSolutionPath,
            field.Solution(SLN_NET_FRAMEWORK_2),
        ]);

      // Layer 1 - file system

      AssertDestinationRootExists (destFolderRoot);

      AssertNewProjectFolderExists (newProjFolderCL2);

      AssertNewProjectFileExists (newProjPathCL2);

      AssertOriginalProjectFileDoesNotExist (oldProjPathCL2);

      AssertExactlyOneCsProjectFileInFolder (newProjFolderCL2);


      // Layer 2 – projects

      AssertProjectXmlLoads (newProjPathCL2);
      AssertAllIncludesAreRelative (newProjPathCL2);

      AssertProjectReference (
          affectedProjPathCA2,
          expectedReferencedProjectAbsPath: newProjPathCL2,
          forbiddenReferencedProjectAbsPath: oldProjPathCL2);

      // Layer 3 – solution

      List<string> expectedProjectPathsInSolution = [
        newProjPathCL2,
        affectedProjPathCA2,
        field.Project(PROJ_CONSOLE_APP_1),
        field.Project(PROJ_CLASS_LIBRARY_1),
        field.Project(PROJ_CLASS_LIBRARY_3),
        field.Project(PROJ_SHARED_PROJECT_1),
      ];
      await AssertSolutionProjectPathsAsync (affectedSolutionPath, expectedProjectPathsInSolution);

      // Layer 4 – build
      // Cannot run "dotnet" for legacy .Net Framework projects which require MSBuild
    }


    public static async Task Move02_SharedProject1 (TestFieldFixture field, bool svn = false) {

      // Arrange

      string projNameS = SHARED_PROJECT_1;
      string projFileS = projNameS + SHPROJ;
      string oldProjPathS = field.Project (projFileS);

      string destFolderRoot = Path.Combine (field.Root, RELOCATED);

      string newProjFolderS = Path.Combine (destFolderRoot, projNameS);
      string newProjPathS = Path.Combine (newProjFolderS, projFileS);

      string affectedProjPathCA3 = field.Project (PROJ_CONSOLE_APP_3);

      string affectedSolutionPath = field.Solution (SLN_NET_FRAMEWORK_2);

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
            affectedProjPathCA3,
            field.Project(PROJ_CLASS_LIBRARY_3),
        ],
        expectedSolutionPaths:
        [
            affectedSolutionPath,
            field.Solution(SLN_NET_FRAMEWORK_1),
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
          affectedProjPathCA3,
          expectedReferencedProjectAbsPath: newProjPathS,
          forbiddenReferencedProjectAbsPath: oldProjPathS);

      // Layer 3 – solution

      List<string> expectedProjectPathsInSolution = [
        newProjPathS,
        affectedProjPathCA3,
        field.Project(PROJ_CLASS_LIBRARY_1),
        field.Project(PROJ_CLASS_LIBRARY_2),
      ];
      await AssertSolutionProjectPathsAsync (affectedSolutionPath, expectedProjectPathsInSolution);

      // Layer 4 – build
      // Cannot run "dotnet" for legacy .Net Framework projects which require MSBuild

    }



  }
}

