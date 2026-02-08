namespace ProjectMover.Tests.Integration {
  using static ProjectMover.Tests.Const;
  using static ProjectMover.Tests.TestFieldFixture;

  [DoNotParallelize]
  [TestClass]
  public sealed class Tests04_CopyProjectsSvn {

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
    public async Task CopySvn01_ALibCore_updates_BLibUtil_reference () {
      await CopyProjectTestsCommon.Copy01_ALibCore (_field, USE_SVN);
    }

    [TestMethod]
    public async Task CopySvn02_SSharedStuff_updates_FSharedApp_reference () {
      await CopyProjectTestsCommon.Copy02_SSharedStuff (_field, USE_SVN);
    }
  }
}
