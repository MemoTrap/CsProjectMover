// Modified file from project https://github.com/audiamus/BookLibConnect
// Original copyright (C) 2021 - 2023 by audiamus
// Originally licensed by audiamus under the GNU General Public License v3.0 (GPL-3.0)
// Slighly modernized and adapted to ProjectMover by MemoTrap
// Still licensed under GPL-3.0

#pragma warning disable IDE0079
#pragma warning disable IDE0130
#pragma warning disable IDE1006
#pragma warning disable SYSLIB1045

using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace core.audiamus.aux {
  public static class ApplEnv {

    static readonly char[] INVALID_CHARS = Path.GetInvalidFileNameChars ();

    public static Version OSVersion { get; } = getOSVersion();
    public static bool Is64BitOperatingSystem => Environment.Is64BitOperatingSystem;
    public static bool Is64BitProcess => Environment.Is64BitProcess;
    public static int ProcessorCount => Environment.ProcessorCount;

    public static Assembly? EntryAssembly { get; } = Assembly.GetEntryAssembly ();
    public static Assembly? ExecutingAssembly { get; } = Assembly.GetExecutingAssembly ();

    public static Version? AssemblyVersion { get; } = EntryAssembly?.GetName ().Version;
    public static string AssemblyTitle { get; } = 
      getAttribute<AssemblyTitleAttribute> ()?.Title ?? Path.GetFileNameWithoutExtension (ExecutingAssembly.Location);
    public static string? AssemblyProduct { get; } = getAttribute<AssemblyProductAttribute> ()?.Product;
    public static string? AssemblyCopyright { get; } = getAttribute<AssemblyCopyrightAttribute> ()?.Copyright;
    public static string? AssemblyCompany { get; } = getAttribute<AssemblyCompanyAttribute> ()?.Company;
    public static string? NeutralCultureName { get; } = getAttribute<NeutralResourcesLanguageAttribute> ()?.CultureName;

    public static string? AssemblyGuid { get; } = getAttribute<GuidAttribute> ()?.Value;  

    public static string? ApplName { get; } = EntryAssembly?.GetName ().Name;
    public static string ApplDirectory { get; } = AppContext.BaseDirectory;
    public static string LocalDirectoryRoot { get; } = Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData);
    public static string LocalCompanyDirectory { get; } = Path.Combine (LocalDirectoryRoot, getCompanyFileName());
    public static string LocalApplDirectory { get; } = Path.Combine (LocalCompanyDirectory, ApplName ?? string.Empty);
    public static string SettingsDirectory { get; } = Path.Combine (LocalApplDirectory, "settings");
    public static string TempDirectory { get; } = Path.Combine (LocalApplDirectory, "tmp");
    public static string LogDirectory { get; } = Path.Combine (LocalApplDirectory, "log");
    public static string UserName { get; } = Environment.UserName;
    public static string UserDirectoryRoot { get; } = Environment.GetFolderPath (Environment.SpecialFolder.UserProfile);

    private static T? getAttribute<T> () where T : Attribute {
      object[] attributes = EntryAssembly?.GetCustomAttributes (typeof (T), false) ?? [];
      if (attributes.Length == 0)
        return null;
      return attributes[0] as T;
    }


    private static string getCompanyFileName () {
      string? company = AssemblyCompany;
      if (string.IsNullOrEmpty (company))
        company = "misc";
      if (company.IndexOfAny (INVALID_CHARS) >= 0)
        foreach (char c in INVALID_CHARS)
          company = company.Replace (c, ' ');
      company = company.Replace (' ', '_');
      return company;
    }

    static Regex? _versRegex;

    private static Version getOSVersion () {
      string os = RuntimeInformation.OSDescription;
      _versRegex ??= new (@"\s([0-9.]+)", RegexOptions.Compiled);
      var match = _versRegex.Match (os);
      if (!match.Success)
        return new Version ();
      string osvers = match.Groups[1].Value;
      try {
        return new Version (osvers);
      } catch (Exception) {
        return new Version ();
      }
    }
  }
}
