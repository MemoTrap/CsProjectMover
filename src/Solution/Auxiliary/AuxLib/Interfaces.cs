// Modified file from project https://github.com/audiamus/BookLibConnect
// Original copyright (C) 2021 - 2023 by audiamus
// Originally licensed by audiamus under the GNU General Public License v3.0 (GPL-3.0)
// Slighly modernized and adapted to ProjectMover by MemoTrap
// Still licensed under GPL-3.0

#pragma warning disable IDE0130


using System.Diagnostics;

namespace core.audiamus.aux {
  public interface IProcessList {
    bool Add (Process process);
    bool Remove (Process process);
  }

  public interface IUserSettings {
  }

  public interface IInitSettings {
    void Init ();
  }
}
