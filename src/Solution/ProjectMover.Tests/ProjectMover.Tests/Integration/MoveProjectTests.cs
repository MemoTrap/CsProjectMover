namespace ProjectMover.Tests.Integration {
  using static ProjectMover.Tests.TestFieldFixture;

  [TestClass]
  public sealed class Tests01_MoveProjects {

    private TestFieldFixture _field = null!;

    [TestInitialize]
    public async Task SetupAsync () {
      _field = await CreateTestFieldFixtureAsync ("ProjectMover.TestField1");
    }

    [TestCleanup]
    public async Task CleanupAsync () {
      if (_field is not null)
        await _field.DisposeAsync ();
    }

    [TestMethod]
    public async Task Move01_ALibCore () {
      await MoveProjectTestsCommon.Move01_ALibCore (_field);
    }


    [TestMethod]
    public async Task Move02_ELibWithLinks () {
      await MoveProjectTestsCommon.Move02_ELibWithLinks (_field);  
    }

    [TestMethod]
    public async Task Move03_SSharedStuff () {
      await MoveProjectTestsCommon.Move03_SSharedStuff (_field);
    }

    [TestMethod]
    public async Task Move04_ALibCore_BLibUtil () {
      await MoveProjectTestsCommon.Move04_ALibCore_BLibUtil (_field);
    }

    [TestMethod]
    public async Task Move05_Rename_ALibCore () {
      await MoveProjectTestsCommon.Move05_08_Rename_ALibCore (_field, true, false);
    }

    [TestMethod]
    public async Task Move06_Rename_ALibCoreFolderOnly () {
      await MoveProjectTestsCommon.Move05_08_Rename_ALibCore (_field, false, true);
    }

    [TestMethod]
    public async Task Move07_Rename_ALibCoreAndFolder () {
      await MoveProjectTestsCommon.Move05_08_Rename_ALibCore (_field, true, true);
    }

    [TestMethod]
    public async Task Move08_Rename_ALibCoreNothing () {
      await MoveProjectTestsCommon.Move05_08_Rename_ALibCore (_field, false, false);
    }

  }
}
