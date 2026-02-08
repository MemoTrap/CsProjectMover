namespace ProjectMover.Tests.Integration {
  using static ProjectMover.Tests.Const;
  using static ProjectMover.Tests.TestFieldFixture;

  [TestClass]
  public sealed class Tests06_MoveCopyLegacyProjects {

    private TestFieldFixture _field = null!;
    private const bool USE_SVN = false;

    [TestInitialize]
    public async Task SetupAsync () {
      _field = await CreateTestFieldFixtureAsync (TEST_FIELD_3, USE_SVN);
    }

    [TestCleanup]
    public async Task CleanupAsync () {
      if (_field is not null)
        await _field.DisposeAsync ();
    }

    [TestMethod]
    public async Task MoveCopy01_Move_ClassLibrary2 () {
      await MoveProjectTestsCommon_Field3.Move01_ClassLibrary2 (_field, USE_SVN);
    }

    [TestMethod]
    public async Task MoveCopy02_Move_SharedProject1 () {
      await MoveProjectTestsCommon_Field3.Move02_SharedProject1 (_field, USE_SVN);
    }

    [TestMethod]
    public async Task MoveCopy03_Copy_ClassLibrary2_update_ConsoleApp2 () {
      await CopyProjectTestsCommon_Field3.Copy03_ClassLibrary2 (_field, USE_SVN);
    }

    [TestMethod]
    public async Task MoveCopy04_Copy_SharedProject1_updates_ConsoleApp3 () {
      await CopyProjectTestsCommon_Field3.Copy04_SharedProject1 (_field, USE_SVN);
    }
  }
}
