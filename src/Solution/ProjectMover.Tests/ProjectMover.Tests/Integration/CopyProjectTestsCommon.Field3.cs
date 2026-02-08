using ProjectMover.Lib.Api;

namespace ProjectMover.Tests.Integration {
  using ProjectMover.Lib;

  using static ProjectMover.Tests.Const;
  using static ProjectMover.Tests.ConfirmationAssertions;
  using static ProjectMover.Tests.SharedProjectFixture;
  using static ProjectMover.Tests.TestFieldFixture;

  internal static class CopyProjectTestsCommon_Field3 {

    public static async Task Copy03_ClassLibrary2 (TestFieldFixture field, bool svn = false) {
      
      // Arrange

      string oldProjNameCL2 = CLASS_LIBRARY_2;
      string oldProjFileCL2 = oldProjNameCL2 + CSPROJ;
      string oldProjPathCL2 = field.Project (oldProjFileCL2);
      
      string destFolderRoot = Path.Combine (field.Root, RELOCATED);
      
      string newProjNameCL2 = CLASS_LIBRARY_2 + COPY;
      string newProjFileCL2 = newProjNameCL2 + CSPROJ;
      string newProjFolderCL2 = Path.Combine (destFolderRoot, newProjNameCL2);
      string newProjPathCL2 = Path.Combine (newProjFolderCL2, newProjFileCL2);
      
      string affectedProjPathCA2 = field.Project (CONSOLE_APP_2); 

      string affectedSolutionPath = field.Solution (SLN_NET_FRAMEWORK_1);

      var parameters = TestParametersFactory.CopySingleProject (
             field,
             oldProjFileCL2,
             destFolderRoot,
             svn);

      var decisionProvider = new ScriptedDecisionProvider (
              new Dictionary<string, ProjectUserDecision> {
                [oldProjNameCL2] = new ProjectUserDecision (
                    Include: true,
                    NewProjectName: newProjNameCL2,
                    NewProjectFolder: newProjFolderCL2,
                    NewAssemblyName: newProjNameCL2,
                    SelectedSolutions: null,
                    SelectedDependentProjectRoots: [affectedProjPathCA2]
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
        ],
        expectedSolutionPaths:
        [
            affectedSolutionPath,
        ]);

      // Layer 1 - file system

      AssertDestinationRootExists (destFolderRoot);

      AssertNewProjectFolderExists (newProjFolderCL2);

      AssertNewProjectFileExists (newProjPathCL2);

      AssertOriginalProjectFileExists (oldProjPathCL2);

      AssertExactlyOneCsProjectFileInFolder (newProjFolderCL2);


      // Layer 2 – projects

      AssertProjectXmlLoads (newProjPathCL2);
      AssertAssemblyName (newProjPathCL2, newProjNameCL2);
      AssertAllIncludesAreRelative (newProjPathCL2);

      AssertProjectReference (
          affectedProjPathCA2,
          expectedReferencedProjectAbsPath: newProjPathCL2,
          forbiddenReferencedProjectAbsPath: oldProjPathCL2);

      AssertCopiedProjectReferenceGuids (
        newProjPathCL2,
        [affectedProjPathCA2]
      );

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

      // Layer 2/3 – shared project GUIDs and projitems

      await AssertProjectGuidUpdatedAsync (oldProjPathCL2, newProjPathCL2, affectedSolutionPath);


      // Layer 4 – build
      // Cannot run "dotnet" for legacy .Net Framework projects which require MSBuild

    }

    public static async Task Copy04_SharedProject1 (TestFieldFixture field, bool svn = false) {
      
      // Arrange

      string oldProjNameS = SHARED_PROJECT_1;
      string oldProjFileS = oldProjNameS + SHPROJ;
      string oldProjPathS = field.Project (oldProjFileS);
      
      string destFolderRoot = Path.Combine (field.Root, RELOCATED);

      string newProjNameS = oldProjNameS + COPY;
      string newProjFileS = newProjNameS + SHPROJ;
      string newProjFolderS = Path.Combine (destFolderRoot, newProjNameS);
      string newProjPathS = Path.Combine (newProjFolderS, newProjFileS);

      string affectedProjPathCA3 = field.Project (PROJ_CONSOLE_APP_3);

      string affectedSlnPathShared = field.Solution (SLN_NET_FRAMEWORK_2);

      var parameters = TestParametersFactory.CopySingleProject (
             field,
             oldProjFileS,
             destFolderRoot,
             svn);

      var decisionProvider = new ScriptedDecisionProvider (
              new Dictionary<string, ProjectUserDecision> {
                [oldProjNameS] = new ProjectUserDecision (
                    Include: true,
                    NewProjectName: newProjNameS,
                    NewProjectFolder: newProjFolderS,
                    NewAssemblyName: null,
                    SelectedSolutions: [ ],
                    SelectedDependentProjectRoots: [affectedProjPathCA3]
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
        ],
        expectedSolutionPaths:
        [
            affectedSlnPathShared,
        ]);

      // Layer 1 - file system

      AssertDestinationRootExists (destFolderRoot);

      AssertNewProjectFolderExists (newProjFolderS);

      AssertNewProjectFileExists (newProjPathS);

      AssertOriginalProjectFileExists (oldProjPathS);

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
      await AssertSolutionProjectPathsAsync (affectedSlnPathShared, expectedProjectPathsInSolution);

      // Layer 2/3 – shared project GUIDs and projitems
      
      await AssertProjectGuidUpdatedAsync (oldProjPathS, newProjPathS, affectedSlnPathShared);
      await AssertSharedProjitemsUpdatedAsync (oldProjPathS, newProjPathS, affectedSlnPathShared);

      // Layer 4 – build
      // Cannot run "dotnet" for legacy .Net Framework projects which require MSBuild

    }
  }
}
