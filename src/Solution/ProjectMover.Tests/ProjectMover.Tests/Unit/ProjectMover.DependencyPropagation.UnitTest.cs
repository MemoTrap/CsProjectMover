namespace ProjectMover.Tests.Unit {
  using ProjectMover.Lib.Helpers;
  using ProjectMover.Lib.Models;

  using static Const;
  using static Tests01_ProjectDependencyGraphBuilder;

  [TestClass]
  public sealed class Tests02_DependencyPropagation {
    private TestFieldFixture _field = null!;
    private DependentSolutionsAndProjects _dependencies = null!;

    [TestInitialize]
    public async Task SetupAsync () {
      _field = await TestFieldFixture.CreateTestFieldFixtureAsync (TEST_FIELD_1);
      _dependencies = await LoadProjectsAndSolutionsAsync (_field);
    }

    [TestCleanup]
    public async Task CleanupAsync () {
      if (_field is not null)
        await _field.DisposeAsync ();
    }


    [TestMethod]
    public void Propagation01_Field1 () {
      var candidateProjects = _dependencies.Projects.ToList ();
      DependencyPropagation depProp = new (candidateProjects, _dependencies);

      var selectedProjA = candidateProjects.First (p => p.Name.Equals (A_LIB_CORE));
      var affectedProjC = _dependencies.Projects.First (p => p.Name.Equals (C_LIB_APP));

      var (affectedProjects, affectedSolutions) =
        depProp.ComputeCopyPropagation (
          selectedProjA,
         [affectedProjC],
         []
        );

      assert (
        [
          B_LIB_UTIL, 
          C_LIB_APP, 
          D_LIB_PLUGIN,
          E_LIB_WITH_LINKS,
          G_MIXED_APP
        ], 
        affectedProjects);
      assert (
       [
          SOL_APP,
          SOL_LINKS,
          SOL_MIXED,
          SOL_PLUGIN,
        ], 
        affectedSolutions);
    }

    private static void assert (IEnumerable<string> expectedNames, IEnumerable<FileBase> actualProjsOrSlns) {
      List<string> actualNames = [.. actualProjsOrSlns.Select (p => p.Name)];
      foreach (var expected in expectedNames) 
        Assert.IsTrue (actualNames.Contains (expected), $"expected '{expected}' not in actual names");
      foreach (var actual in actualNames) 
        Assert.IsTrue (expectedNames.Contains (actual), $"actual '{actual}' not in expected names");
    }
  }
}
