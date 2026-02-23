namespace ProjectMover.Gui.App {
  using static core.audiamus.aux.ApplEnv;


  internal static class Program {
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main (string[] args) {
      // To customize application configuration such as set high DPI settings or default font,
      // see https://aka.ms/applicationconfiguration.
      ApplicationConfiguration.Initialize ();

      AppSettings appSettings = SettingsManager.GetAppSettings<AppSettings> (true);

      Logging.Level = appSettings.LogLevel;
      Logging.InstantFlush = true;

      StringExtensions.DefaultWidth = appSettings.MsgBoxTextWidth;

      Log (1, () => $"{ApplName} {AssemblyVersion}" +
        $" on Windows {OSVersion}");


      ArgParser ap = new (args);
      if (ap.Exists ("dry-run"))
        CsProjectMover.DryRun = true;

      _ = LogTmpFileMaintenance.Instance.CleanupAsync ();

      Application.Run (new MainForm ());
    }
  }
}