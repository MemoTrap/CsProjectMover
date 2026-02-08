namespace ProjectMover.Tests.Integration {
  using static ProjectMover.Tests.Const;
  using static ProjectMover.Tests.TestFieldFixture;

  [DoNotParallelize]
  [TestClass]
  public sealed class Tests03_MoveProjectsSvn {

    private TestFieldFixture _field = null!;
    private const bool USE_SVN = true;

    [TestInitialize]
    public async Task SetupAsync () {
      _field = await CreateTestFieldFixtureAsync (TEST_FIELD_1, USE_SVN);
    }

    [TestCleanup]
    public async Task CleanupAsync () {
      if (_field is not null)
        await _field.DisposeAsync ();
    }

    [TestMethod]
    public async Task MoveSvn01_ALibCore () {
      await MoveProjectTestsCommon.Move01_ALibCore (_field, USE_SVN);
    }


    [TestMethod]
    public async Task MoveSvn02_ELibWithLinks () {
      await MoveProjectTestsCommon.Move02_ELibWithLinks (_field, USE_SVN);
    }

    [TestMethod]
    public async Task MoveSvn03_SSharedStuff () {
      await MoveProjectTestsCommon.Move03_SSharedStuff (_field, USE_SVN);
    }

    [TestMethod]
    public async Task MoveSvn04_ALibCore_BLibUtil () {
      await MoveProjectTestsCommon.Move04_ALibCore_BLibUtil (_field, USE_SVN);
    }

    [TestMethod]
    public async Task MoveSvn05_Rename_ALibCore () {
      await MoveProjectTestsCommon.Move05_08_Rename_ALibCore (_field, true, false, USE_SVN);
    }

    [TestMethod]
    public async Task MoveSvn06_Rename_ALibCoreFolderOnly () {
      await MoveProjectTestsCommon.Move05_08_Rename_ALibCore (_field, false, true, USE_SVN);
    }

    [TestMethod]
    public async Task MoveSvn07_Rename_ALibCoreAndFolder () {
      await MoveProjectTestsCommon.Move05_08_Rename_ALibCore (_field, true, true, USE_SVN);
    }

    [TestMethod]
    public async Task MoveSvn08_Rename_ALibCoreNothing () {
      await MoveProjectTestsCommon.Move05_08_Rename_ALibCore (_field, false, false, USE_SVN);
    }


  }
}
