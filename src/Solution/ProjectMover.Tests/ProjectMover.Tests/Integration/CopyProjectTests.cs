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
    public async Task Copy01_BLibUtil_updates_SolPlugin () {
      await CopyProjectTestsCommon.Copy01_BLibUtil (_field);
    }

    [TestMethod]
    public async Task Copy02_ALibCore_updates_CLibApp_reference () {
      await CopyProjectTestsCommon.Copy02_ALibCore (_field);
    }

    [TestMethod]
    public async Task Copy03_SSharedStuff_updates_FSharedApp_reference () {
      await CopyProjectTestsCommon.Copy03_SSharedStuff (_field);
    }
  }
}
