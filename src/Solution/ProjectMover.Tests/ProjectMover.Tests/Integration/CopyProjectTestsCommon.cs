using ProjectMover.Lib.Api;

namespace ProjectMover.Tests.Integration {
  using ProjectMover.Lib;

  using static ProjectMover.Tests.Const;
  using static ProjectMover.Tests.ConfirmationAssertions;
  using static ProjectMover.Tests.SharedProjectFixture;
  using static ProjectMover.Tests.TestFieldFixture;

  internal static class CopyProjectTestsCommon {

    public static async Task Copy01_ALibCore (TestFieldFixture field, bool svn = false) {
      
      // Arrange

      string oldProjNameA = A_LIB_CORE;
      string oldProjFileA = oldProjNameA + CSPROJ;
      string oldProjPathA = field.Project (oldProjFileA);
      
      string destFolderRoot = Path.Combine (field.Root, RELOCATED);
      
      string newProjNameA = A_LIB_CORE + COPY;
      string newProjFileA = newProjNameA + CSPROJ;
      string newProjFolderA = Path.Combine (destFolderRoot, newProjNameA);
      string newProjPathA = Path.Combine (newProjFolderA, newProjFileA);
      
      string affectedProjPathB = field.Project (PROJ_B_LIB_UTIL); 

      string affectedSolutionPath = field.Solution (SLN_SOL_APP);

      var parameters = TestParametersFactory.CopySingleProject (
             field,
             oldProjFileA,
             destFolderRoot,
             svn);

      var decisionProvider = new ScriptedDecisionProvider (
              new Dictionary<string, ProjectUserDecision> {
                [oldProjNameA] = new ProjectUserDecision (
                    Include: true,
                    NewProjectName: newProjNameA,
                    NewProjectFolder: newProjFolderA,
                    NewAssemblyName: newProjNameA,
                    SelectedSolutions: null,
                    SelectedDependentProjectRoots: [affectedProjPathB]
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
            field.Project(PROJ_G_MIXED_APP),
            field.Project(PROJ_C_LIB_APP),
        ],
        expectedSolutionPaths:
        [
            affectedSolutionPath,
            field.Solution(SLN_SOL_MIXED),
            field.Solution(SLN_SOL_PLUGIN),
        ]);

      // Layer 1 - file system

      AssertDestinationRootExists (destFolderRoot);

      AssertNewProjectFolderExists (newProjFolderA);

      AssertNewProjectFileExists (newProjPathA);

      AssertOriginalProjectFileExists (oldProjPathA);

      AssertExactlyOneCsProjectFileInFolder (newProjFolderA);


      // Layer 2 – projects

      AssertProjectXmlLoads (newProjPathA);
      AssertAssemblyName (newProjPathA, newProjNameA);
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

    public static async Task Copy02_SSharedStuff (TestFieldFixture field, bool svn = false) {
      
      // Arrange

      string oldProjNameS = S_SHARED_STUFF;
      string oldProjFileS = oldProjNameS + SHPROJ;
      string oldProjPathS = field.Project (oldProjFileS);
      
      string destFolderRoot = Path.Combine (field.Root, RELOCATED);

      string newProjNameS = S_SHARED_STUFF + COPY;
      string newProjFileS = newProjNameS + SHPROJ;
      string newProjFolderS = Path.Combine (destFolderRoot, newProjNameS);
      string newProjPathS = Path.Combine (newProjFolderS, newProjFileS);
      
      string affectedProjPathF = field.Project (PROJ_F_SHARED_APP); 

      string affectedSlnPathShared = field.Solution (SLN_SOL_SHARED);

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
                    SelectedDependentProjectRoots: [affectedProjPathF]
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
            affectedProjPathF,
            //field.Project(G_MIXED_APP),
        ],
        expectedSolutionPaths:
        [
            affectedSlnPathShared,
            //field.Solution(SLN_SOL_MIXED),
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
          affectedProjPathF,
          expectedReferencedProjectAbsPath: newProjPathS,
          forbiddenReferencedProjectAbsPath: oldProjPathS);

      // Layer 3 – solution

      List<string> expectedProjectPathsInSolution = [ 
        newProjPathS,
        affectedProjPathF,
        field.Project(PROJ_Z_LIB_STD)
      ];
      await AssertSolutionProjectPathsAsync (affectedSlnPathShared, expectedProjectPathsInSolution);

      // Layer 2/3 – shared project GUIDs and projitems
      
      await AssertProjectGuidUpdatedAsync (oldProjPathS, newProjPathS, affectedSlnPathShared);
      await AssertSharedProjitemsUpdatedAsync (oldProjPathS, newProjPathS, affectedSlnPathShared);

      // Layer 4 – build

      await BuildSolutionAsync (affectedSlnPathShared);
    }
  }
}
