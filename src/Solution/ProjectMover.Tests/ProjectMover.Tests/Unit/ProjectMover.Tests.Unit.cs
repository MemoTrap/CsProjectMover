#pragma warning disable IDE0305

namespace ProjectMover.Tests.Unit {
  using static Const;
  using ProjectMover.Lib.Misc;
  using ProjectMover.Lib.Models;

  [TestClass]
  public sealed class Tests01_ProjectDependencyGraphBuilder {

    private TestFieldFixture _field = null!;
    private DependentSolutionsAndProjects _dependencies = null!;

    [TestInitialize]
    public async Task SetupAsync () {
      _field = await TestFieldFixture.CreateTestFieldFixtureAsync (TEST_FIELD_1);
      _dependencies = await loadProjectsAndSolutions ();
    }

    [TestCleanup]
    public async Task CleanupAsync () {
      if (_field is not null)
        await _field.DisposeAsync ();
    }


    [TestMethod]
    public void DepGraph01_ALibCore () {
      // --- Arrange

      var aLibCore = _dependencies.Projects
          .First (p => p.Name == A_LIB_CORE);

      var referencingProjects = getReferences (aLibCore);


      // --- Act: build graph targeting ALibCore ---
      var graph = new ProjectDependencyGraphBuilder ();
      graph.Build (aLibCore, referencingProjects);

      var nodes = graph.Nodes.ToDictionary (n => n.Project.Name);

      string[] expectedDependentProjects =
          [
            A_LIB_CORE, B_LIB_UTIL, C_LIB_APP, D_LIB_PLUGIN,
            E_LIB_WITH_LINKS, F_SHARED_APP, G_MIXED_APP
          ];

    // --- Assert nodes exist ---
    CollectionAssert.AreEquivalent (
          expectedDependentProjects,
          nodes.Keys.ToList ()
      );

      // --- Assert: ALibCore ReferencedBy ---
      var alibNode = nodes[A_LIB_CORE];
      string[] expectedRefBy =
      [
        B_LIB_UTIL, C_LIB_APP, D_LIB_PLUGIN, E_LIB_WITH_LINKS, F_SHARED_APP, G_MIXED_APP
      ];
      CollectionAssert.AreEquivalent (expectedRefBy, alibNode.ReferencedBy.Select (n => n.Project.Name).ToList ());

      //// --- Assert: Shared project edges ---
      // Since a shared project can never reference any other project, nothing to assert here

      // --- Assert: selection groups from ALibCore ---
      var selectionGroups = graph.BuildSelectionGroups (aLibCore);
      var groupRoots = selectionGroups.Select (g => g.Root.Name).ToList ();
      CollectionAssert.AreEquivalent (expectedRefBy, groupRoots);

      var allProjectsInGraph = nodes.Values.Select (n => n.Project).ToHashSet ();

      foreach (var group in selectionGroups) {
        Assert.IsTrue (allProjectsInGraph.Contains (group.Root));
        foreach (var member in group.Members) {
          Assert.IsTrue (allProjectsInGraph.Contains (member));
        }
      }

      // --- Assert: GMixedApp references ---
      var gNode = nodes[G_MIXED_APP];
      string[] gExpectedRefs = [A_LIB_CORE, B_LIB_UTIL, D_LIB_PLUGIN, E_LIB_WITH_LINKS];
      CollectionAssert.AreEquivalent (gExpectedRefs, gNode.References.Select (n => n.Project.Name).ToList ());
    }

    [TestMethod]
    public void DepGraph02_SSharedStuff () {
      // --- Arrange

      var sharedProj = _dependencies.Projects
          .OfType<SharedProjectFile> ()
          .First (p => p.Name == S_SHARED_STUFF);

      var referencingProjects = getReferences (sharedProj);

      // --- Act: build graph targeting shared project ---
      var graph = new ProjectDependencyGraphBuilder ();
      graph.Build (sharedProj, referencingProjects);

      var nodes = graph.Nodes.ToDictionary (n => n.Project.Name);

      string[] expectedDependentProjects =
      [
        S_SHARED_STUFF, D_LIB_PLUGIN, F_SHARED_APP
      ];


      // --- Assert nodes exist ---
      CollectionAssert.AreEquivalent (
          expectedDependentProjects,
          nodes.Keys.ToList ()
      );

      // --- Assert: shared project ReferencedBy ---
      var sNode = nodes[S_SHARED_STUFF];
      string[] expectedRefBy = [D_LIB_PLUGIN, F_SHARED_APP];
      CollectionAssert.AreEquivalent (
          expectedRefBy,
          sNode.ReferencedBy.Select (n => n.Project.Name).ToList ()
      );

      // --- Assert: References (shared project never references other projects) ---
      CollectionAssert.AreEquivalent (
          Array.Empty<string> (),
          sNode.References.Select (n => n.Project.Name).ToList ()
      );

      // --- Assert: selection groups ---
      var selectionGroups = graph.BuildSelectionGroups (sharedProj);
      var groupRoots = selectionGroups.Select (g => g.Root.Name).ToList ();
      CollectionAssert.AreEquivalent (expectedRefBy, groupRoots);

      var allProjectsInGraph = nodes.Values.Select (n => n.Project).ToHashSet ();
      foreach (var group in selectionGroups) {
        Assert.IsTrue (allProjectsInGraph.Contains (group.Root));
        foreach (var member in group.Members) {
          Assert.IsTrue (allProjectsInGraph.Contains (member));
        }
      }

      // --- Assert: one of the dependent projects references the shared project ---
      var gNode = nodes[D_LIB_PLUGIN];
      string[] expectedReferencedNodes = [S_SHARED_STUFF];
      CollectionAssert.AreEquivalent (
          expectedReferencedNodes,
          gNode.References.Select (n => n.Project.Name).ToList ()
      );
    }


    private List<ProjectFile> getReferences (ProjectFile project) {
      // Find referencing projects
      return _dependencies.Projects
          .Where (p =>
              p.ProjectReferencesAbsPath.Contains (project.AbsolutePath, StringComparer.OrdinalIgnoreCase)
              || (project is SharedProjectFile sh &&
                  p.SharedProjectImportsAbsPath.Contains (sh.ProjItemsAbsPath, StringComparer.OrdinalIgnoreCase))
          )
          .ToList ();
    }

    private async Task<DependentSolutionsAndProjects> loadProjectsAndSolutions () {
      // load all projects from test field
      var zLibStd = await loadCs (_field.Project (PROJ_Z_LIB_STD));
      var aLibCore = await loadCs (_field.Project (PROJ_A_LIB_CORE));
      var bLibUtil = await loadCs (_field.Project (PROJ_B_LIB_UTIL));
      var cLibApp = await loadCs (_field.Project (PROJ_C_LIB_APP));
      var dLibPlugin = await loadCs (_field.Project (PROJ_D_LIB_PLUGIN));
      var eLibWithLinks = await loadCs (_field.Project (PROJ_E_LIB_WITH_LINKS));
      var fSharedApp = await loadCs (_field.Project (PROJ_F_SHARED_APP));
      var gMixedApp = await loadCs (_field.Project (PROJ_G_MIXED_APP));
      var sSharedStuff = await loadShared (_field.Project (PROJ_S_SHARED_STUFF));

      // load all solutions from the test field
      var solCore = await loadSolution (_field.Solution (SLN_SOL_CORE));
      var solApp = await loadSolution (_field.Solution (SLN_SOL_APP));
      var solPlugin = await loadSolution (_field.Solution (SLN_SOL_PLUGIN));
      var solLinks = await loadSolution (_field.Solution (SLN_SOL_LINKS));
      var solShared = await loadSolution (_field.Solution (SLN_SOL_SHARED));
      var solMixed = await loadSolution (_field.Solution (SLN_SOL_MIXED));


      var allProjects = new List<ProjectFile>
      {
          zLibStd, sSharedStuff, aLibCore, bLibUtil, cLibApp,
          dLibPlugin, eLibWithLinks, fSharedApp, gMixedApp
      };

      var allSolutions = new List<SolutionFile> { solCore, solApp, solPlugin, solLinks, solShared, solMixed };

      DependentSolutionsAndProjects dependencies = new (allSolutions, allProjects);
      return dependencies;
    }

    private static async Task<CsProjectFile> loadCs (string filePath, CancellationToken ct = default) {
      var proj = new CsProjectFile (filePath);
      await proj.LoadAsync (ct);
      return proj;
    }

    private static async Task<SharedProjectFile> loadShared (string filePath, CancellationToken ct = default) {
      var proj = new SharedProjectFile (filePath);
      await proj.LoadAsync (ct);
      return proj;
    }

    private static async Task<SolutionFile> loadSolution (string filePath, CancellationToken ct = default) {
      var sol = new SolutionFile (filePath);
      await sol.LoadAsync (ct);
      return sol;
    }

  }
}
