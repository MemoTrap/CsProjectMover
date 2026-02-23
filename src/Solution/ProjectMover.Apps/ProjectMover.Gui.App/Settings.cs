using core.audiamus.util;

namespace ProjectMover.Gui.App {
  internal class UserSettings : IUserSettings {
    public UpdateSettings UpdateSettings { get; set; } = new ();
    public AppParameters? Parameters { get; set; }
    public bool NoDisclaimer { get; set; }
  }

  internal class AppSettings {
    public int LogLevel { get; set; } = 3;
    public bool DbgOnlineUpdate { get; set; }
    public int MsgBoxTextWidth { get; set; } = 330;
  }
}
