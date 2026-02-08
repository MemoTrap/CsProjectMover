// Modified file from project https://github.com/audiamus/BookLibConnect
// Original copyright (C) 2021 - 2023 by audiamus
// Originally licensed by audiamus under the GNU General Public License v3.0 (GPL-3.0)
// Slighly modernized and adapted to ProjectMover by MemoTrap
// Still licensed under GPL-3.0


#pragma warning disable IDE0130
#pragma warning disable IDE0305


using System.Diagnostics;
using System.Security.Cryptography;

using core.audiamus.aux;
using core.audiamus.aux.ex;

using ProjectMover.Global.Lib;

using static core.audiamus.aux.Logging;

namespace core.audiamus.util {
  public class OnlineUpdate {

    const string SETUP_REF_URL =
      "https://raw.githubusercontent.com/{0}/{1}/master/res/Setup.json";
    private readonly string _setupRefUrl;
    private readonly string _defaultAppName;
    private readonly bool _dbg;

    private static HttpClient HttpClient { get; } = new HttpClient ();
    private static string DownloadDir { get; }

    private IUpdateSettings Settings { get; }

    IEnumerable<PackageInfoLocal>? _packageInfos;
    PackageInfoLocal? _defaultPackageInfo;

    static OnlineUpdate () {
      string downloads = Environment.ExpandEnvironmentVariables (@"%USERPROFILE%\Downloads");
      if (string.IsNullOrWhiteSpace (downloads))
        downloads = ApplEnv.TempDirectory;
      DownloadDir = downloads;
    }

    public string Uri => _setupRefUrl;

    public OnlineUpdate(
      IUpdateSettings settings, 
      string? gitHubUserName, 
      string? defaultAppName, 
      string? setupRefUrl, 
      bool dbg
    ) {
      if (gitHubUserName is null || defaultAppName is null) {
        Log (1, this, () => "GitHub user name or default app name is null.");
        ArgumentNullException.ThrowIfNull (gitHubUserName, nameof(gitHubUserName));
        ArgumentNullException.ThrowIfNull (defaultAppName, nameof(defaultAppName));
      }

      Settings = settings;
      _dbg = dbg;
      _defaultAppName = defaultAppName;
      if (setupRefUrl is null)
        _setupRefUrl = string.Format (SETUP_REF_URL, gitHubUserName, defaultAppName);
      else
        _setupRefUrl = setupRefUrl;

    }

    public async Task UpdateAsync (
      ICallbackSink interactCallback,
      Action finalCallback,
      Func<bool> busyCallback
  ) {
      if (Settings.OnlineUpdate == EOnlineUpdate.no)
        return;

      await getSetupRefAsync ();

      var pi = _defaultPackageInfo;
      if (pi is null) {
        Log (1, this, () => "no package info");
        return;
      }

      Log (3, this, () => $"server={pi.Version}");
    
      // do we have a new version?
      bool newVersion = pi.Version > ApplEnv.AssemblyVersion;
      if (_dbg)
        newVersion = true;
      if (!newVersion)
        return;
          
      // do we have it downloaded already?
      bool exists = await checkDownloadAsync (pi);
      Log (3, this, () => $"download exists={exists}");

      if (!exists) {
        if (Settings.OnlineUpdate == EOnlineUpdate.promptForDownload) {
          var msg = updateInteractMessage (
            new UpdateInteractionMessage (EUpdateInteract.newVersAvail, pi)
          );

          EDialogResult result = interactCallback.ShowMessage (
            ECallback.Question,
            msg,
            null,
            EMsgBtns.YesNo,
            EDefaultMsgBtn.Button1);
          if (result != EDialogResult.Yes)
            return;
        }

        // yes: download,  verify md5
        await downloadSetupAsync (pi);
      }

      bool isBusy = busyCallback ();
      if (isBusy) {
        Log (3, this, () => "is already busy, cancel.");
        return;
      }

      bool cont = install (pi, interactCallback, EUpdateInteract.installNow);
      if (!cont) {
        Log (3, this, () => "do not install now, cancel.");
        return;
      }

      if (pi.DefaultApp)
        finalCallback?.Invoke ();
    }

    public async Task<bool> InstallAsync (
      ICallbackSink interactCallback, 
      Action finalCallback
    ) {
      if (Settings.OnlineUpdate == EOnlineUpdate.no)
        return false;

      await getSetupRefAsync ();

      var pi = _defaultPackageInfo;
      if (pi is null) {
        Log (1, this, () => "no package info");
        return false;
      }

      // do we have a new version?
      bool newVersion = pi.Version > ApplEnv.AssemblyVersion;
      if (_dbg)
        newVersion = true;
      if (!newVersion)
        return false;

      // do we have it downloaded already?
      bool exists = await checkDownloadAsync (pi);
      if (!exists)
        return false;

      install (pi, interactCallback, EUpdateInteract.installLater);

      if (pi.DefaultApp)
        finalCallback?.Invoke ();

      return true;
    }

    private bool install (
      PackageInfoLocal pi,
      ICallbackSink interactCallback, 
      EUpdateInteract prompt
    ) {
      if (pi.SetupFile is null)
        return false;
      string msg = updateInteractMessage (
        new UpdateInteractionMessage (prompt, pi)
      );
      EDialogResult result = interactCallback.ShowMessage (
        ECallback.Question,
        msg,
        null,
        EMsgBtns.YesNo,
        EDefaultMsgBtn.Button1);

      if (result != EDialogResult.Yes)
        return false;

      // launch installer
      try {
        Log (3, this, () => "launch.");
        Process.Start (pi.SetupFile);
      } catch (Exception exc) {
        Log (1, this, () => $"{exc.Summary ()}");
      }
      return true;
    }

    private async Task getSetupRefAsync () {
      string? result = null;

      try {
        using (HttpResponseMessage response = await HttpClient.GetAsync (_setupRefUrl)) {
          response.EnsureSuccessStatusCode ();
          
          using HttpContent content = response.Content;
          result = await content.ReadAsStringAsync ();
        }
        if (string.IsNullOrWhiteSpace (result))
            return;

        var packageInfos = JsonSerialization.FromJsonString<PackageInfo[]> (result);

        _packageInfos = packageInfos.Select (pi => {
          var pil = new PackageInfoLocal (pi);

          string? file = Path.GetFileName (pi.Url);
          if (file is not null) {
            string setupFile = Path.Combine (DownloadDir, file);
            pil = pil with { SetupFile = setupFile };
          }
          return pil;
        }).
        ToList ();

        var defaultPackageInfo = _packageInfos
          .FirstOrDefault (pi => 
            string.Equals (pi.AppName, _defaultAppName, StringComparison.InvariantCultureIgnoreCase) ||
            string.Equals (pi.AppName, ApplEnv.ApplName, StringComparison.InvariantCultureIgnoreCase));
        if (defaultPackageInfo is null) {
          Log (1, this, () => $"no package info for default app \"{_defaultAppName}\"");
          return;
        }
        _defaultPackageInfo = defaultPackageInfo with { DefaultApp = true };

      } catch (Exception exc) {
        Log (1, this, () => $"{exc.Summary ()}{Environment.NewLine}  \"{result}\"");
      }
    }

    private async Task<bool> checkDownloadAsync (PackageInfoLocal pi) {
      if (!File.Exists (pi.SetupFile))
        return false;

      string md5 = await computeMd5ForFileAsync (pi.SetupFile);
      bool succ = string.Equals (pi.Md5, md5, StringComparison.InvariantCultureIgnoreCase);
      Log (3, this, () => $"succ={succ}, file={md5}, server={pi.Md5}");
      return succ;
    }

    private static async Task<string> computeMd5ForFileAsync (string filePath) {
      return await Task.Run (() => computeMd5HashForFile (filePath));
    }

    private static string computeMd5HashForFile (string filePath) {
      using var md5 = MD5.Create ();
      using var stream = File.OpenRead (filePath);
      var hash = md5.ComputeHash (stream);
      return BitConverter.ToString (hash).Replace ("-", "").ToLowerInvariant ();
    }


    private async Task<bool> downloadSetupAsync (PackageInfoLocal pi) {
      Log (3, this);
      await downloadAsync (pi.Url, pi.SetupFile);
      return await checkDownloadAsync (pi);
    }


    private async Task downloadAsync (string? requestUri, string? filename) {
      if (requestUri is null || filename is null)
        return;
      Log (3, this, $"\"{requestUri}\"");
      try {
        // for .Net framework
        //ServicePointManager.Expect100Continue = true;
        //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        using (var fileStream = File.OpenWrite (filename)) {
          using var networkStream = await HttpClient.GetStreamAsync (requestUri);
          Log (3, this, $"copy to \"{filename.SubstitUser ()}\"");
          await networkStream.CopyToAsync (fileStream);
          Log (3, this, "flush");
          await fileStream.FlushAsync ();
        }
        Log (1, this, () => "complete");
      } catch (Exception exc) {
        Log (1, this, () => $"{exc.Summary ()}");
      }
    }

    private string updateInteractMessage (UpdateInteractionMessage uim) {

      string msg = string.Empty;
      var pi = uim.PckInfo;
      string? appName = pi.AppName;
      if (string.Equals (_defaultAppName, pi.AppName, StringComparison.OrdinalIgnoreCase))
        appName = ApplEnv.ApplName;

      if (!pi.DefaultApp)
        return msg;
      switch (uim.Kind) {
        case EUpdateInteract.newVersAvail:
          msg = string.Format (
            "New version {0}{1} of {2} is available.{3}\r\nDownload now?",
            pi.Version, prev (pi.Preview), appName, desc (null));
          break;
        case EUpdateInteract.installNow:
          msg = string.Format (
            "New version {0}{1} of {2} is ready for installation.\r\n" +
            "Install now? (Will close current {2} window.)",
            pi.Version, prev (pi.Preview), appName);
          break;
        case EUpdateInteract.installLater:
          msg = string.Format (
            "New version {0}{1} of {2} is ready for installation.\r\nInstall now?",
            pi.Version, prev (pi.Preview), appName);
          break;
      }
      return msg;

      static string prev (bool prv) => prv ? $" (Preview)" : string.Empty;
      static string desc (string? dsc) => !dsc.IsNullOrWhiteSpace () ? $"\r\n\"{dsc}\"" : string.Empty;

    }


  }
}
