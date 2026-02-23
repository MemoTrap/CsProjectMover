// Modified file from project https://github.com/audiamus/BookLibConnect
// Original copyright (C) 2021 - 2023 by audiamus
// Originally licensed by audiamus under the GNU General Public License v3.0 (GPL-3.0)
// Slighly modernized and adapted to ProjectMover by MemoTrap
// Still licensed under GPL-3.0

#pragma warning disable IDE0130

using System.Diagnostics;

namespace core.audiamus.aux {
  public static class ShellExecute {
    public static void Url (Uri uri) => File (uri.OriginalString);

    public static void File (string url) {
      Process.Start (new ProcessStartInfo () {
        UseShellExecute = true,
        CreateNoWindow = true,
        FileName = url,
      });

    }
  }
}
