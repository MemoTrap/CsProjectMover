namespace ProjectMover.Tests.Integration {
  using static ProjectMover.Tests.Const;
  using static ProjectMover.Tests.TestFieldFixture;

  [TestClass]
  public sealed class Tests02_CopyProjects {

    private TestFieldFixture _field = null!;

    [TestInitialize]
    public async Task SetupAsync () {
      _field = await CreateTestFieldFixtureAsync (TEST_FIELD_1);
    }

    [TestCleanup]
    public async Task CleanupAsync () {
      if (_field is not null)
        await _field.DisposeAsync ();
    }

    [TestMethod]
    public async Task Copy01_ALibCore_updates_BLibUtil_reference () {
      await CopyProjectTestsCommon.Copy01_ALibCore (_field);
    }

    [TestMethod]
    public async Task Copy02_SSharedStuff_updates_FSharedApp_reference () {
      await CopyProjectTestsCommon.Copy02_SSharedStuff (_field);
    }
  }
}
