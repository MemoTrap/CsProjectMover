#pragma warning disable IDE0130

using ProjectMover.Gui.Lib;

namespace ProjectMover.Tests.Unit;

[TestClass]
public sealed class Tests01_ShortenPathStrings {
  private const int MATCH_LENGTH = 10;

  private const string SAMPLE_TEXT_REL_PATHS =
    """
    Move/rename operation summary

    7 projects affected:
    - ProjectMover.TestField1\SolCore\ALibCore\ALibCore.csproj (selected)
    - ProjectMover.TestField1\SolApp\BLibUtil\BLibUtil.csproj (dependent)
    - ProjectMover.TestField1\SolApp\CLibApp\CLibApp.csproj (dependent)
    - ProjectMover.TestField1\SolLinks\ELibWithLinks\ELibWithLinks.csproj (dependent)
    - ProjectMover.TestField1\SolPlugin\DLibPlugin\DLibPlugin.csproj (dependent)
    - ProjectMover.TestField1\SolMixed\GMixedApp\GMixedApp.csproj (dependent)
    - ProjectMover.TestField1\SolShared\FSharedApp\FSharedApp.csproj (dependent)

    6 solutions affected:
    - ProjectMover.TestField1\SolApp\SolApp.sln
    - ProjectMover.TestField1\SolCore\SolCore.sln
    - ProjectMover.TestField1\SolLinks\SolLinks.sln
    - ProjectMover.TestField1\SolMixed\SolMixed.sln
    - ProjectMover.TestField1\SolPlugin\SolPlugin.sln
    - ProjectMover.TestField1\SolShared\SolShared.sln

    Proceed?
    """;

  private const string SAMPLE_TEXT_ABS_PATHS =
    """
    Move/rename operation summary

    7 projects affected:
    - F:\svn-source\ProjectMover\TestFields\ProjectMover.TestField1\SolCore\ALibCore\ALibCore.csproj (selected)
    - F:\svn-source\ProjectMover\TestFields\ProjectMover.TestField1\SolApp\BLibUtil\BLibUtil.csproj (dependent)
    - F:\svn-source\ProjectMover\TestFields\ProjectMover.TestField1\SolApp\CLibApp\CLibApp.csproj (dependent)
    - F:\svn-source\ProjectMover\TestFields\ProjectMover.TestField1\SolLinks\ELibWithLinks\ELibWithLinks.csproj (dependent)
    - F:\svn-source\ProjectMover\TestFields\ProjectMover.TestField1\SolPlugin\DLibPlugin\DLibPlugin.csproj (dependent)
    - F:\svn-source\ProjectMover\TestFields\ProjectMover.TestField1\SolMixed\GMixedApp\GMixedApp.csproj (dependent)
    - F:\svn-source\ProjectMover\TestFields\ProjectMover.TestField1\SolShared\FSharedApp\FSharedApp.csproj (dependent)

    6 solutions affected:
    - F:\svn-source\ProjectMover\TestFields\ProjectMover.TestField1\SolApp\SolApp.sln
    - F:\svn-source\ProjectMover\TestFields\ProjectMover.TestField1\SolCore\SolCore.sln
    - F:\svn-source\ProjectMover\TestFields\ProjectMover.TestField1\SolLinks\SolLinks.sln
    - F:\svn-source\ProjectMover\TestFields\ProjectMover.TestField1\SolMixed\SolMixed.sln
    - F:\svn-source\ProjectMover\TestFields\ProjectMover.TestField1\SolPlugin\SolPlugin.sln
    - F:\svn-source\ProjectMover\TestFields\ProjectMover.TestField1\SolShared\SolShared.sln

    Proceed?
    """;
 
  private const string SAMPLE_TEXT_REL_PATHS_WITH_SPACES =
    """
    Move/rename operation summary

    7 projects affected:
    - ProjectMover.TestField1\SolCore\A Lib Core\ALibCore.csproj (selected)
    - ProjectMover.TestField1\SolApp\B Lib Util\BLibUtil.csproj (dependent)
    - ProjectMover.TestField1\SolApp\C Lib App\CLibApp.csproj (dependent)
    - ProjectMover.TestField1\SolLinks\E Lib With Links\ELibWithLinks.csproj (dependent)
    - ProjectMover.TestField1\Sol Plugin\D Lib Plugin\DLibPlugin.csproj (dependent)
    - ProjectMover.TestField1\Sol Mixed\G Mixed App\GMixedApp.csproj (dependent)
    - ProjectMover.TestField1\Sol Shared\F Shared App\FSharedApp.csproj (dependent)

    6 solutions affected:
    - ProjectMover.TestField1\SolApp\SolApp.sln
    - ProjectMover.TestField1\SolCore\SolCore.sln
    - ProjectMover.TestField1\SolLinks\SolLinks.sln
    - ProjectMover.TestField1\Sol Mixed\SolMixed.sln
    - ProjectMover.TestField1\Sol Plugin\SolPlugin.sln
    - ProjectMover.TestField1\SolShared\SolShared.sln

    Proceed?
    """;

  private const string FILES =
    """
    ALibCore.csproj (selected)
    BLibUtil.csproj (dependent)
    CLibApp.csproj (dependent)
    ELibWithLinks.csproj (dependent)
    DLibPlugin.csproj (dependent)
    GMixedApp.csproj (dependent)
    FSharedApp.csproj (dependent)
    SolApp.sln
    SolCore.sln
    SolLinks.sln
    SolMixed.sln
    SolPlugin.sln
    SolShared.sln
    """;

  [TestMethod]
  public void Shorten01_RelPaths () {
    shortenPaths (SAMPLE_TEXT_REL_PATHS);
  }

  [TestMethod]
  public void Shorten02_AbsPaths () {
    shortenPaths (SAMPLE_TEXT_ABS_PATHS);
  }

  [TestMethod]
  public void Shorten03_SpacePaths () {
    shortenPaths (SAMPLE_TEXT_REL_PATHS_WITH_SPACES);
  }

  private static void shortenPaths (string sample) {
    var shortened = sample.ShortenMultiLineBulletDecoratedPathString ();
    var extracted = shortened
        .Split ([ "\r\n", "\n" ], StringSplitOptions.None)
        .ToList ();


    var sourceLines = sample
        .Split ([ "\r\n", "\n" ], StringSplitOptions.None)
        .ToList ();

    var files = FILES
        .Split ([ "\r\n", "\n" ], StringSplitOptions.None)
        .ToList ();


    Assert.AreEqual (sourceLines.Count, extracted.Count, "Line count mismatch.");

    int fileCount = 0;
    for (int i = 0; i < extracted.Count; i++) {
      var src = sourceLines[i];
      var dst = extracted[i];

      if (src.Length < MATCH_LENGTH)
        continue;

      Assert.IsTrue (dst.Length >= MATCH_LENGTH, "Extracted line too short.");

      Assert.AreEqual (first (src, MATCH_LENGTH), first (dst, MATCH_LENGTH),
        $"Prefix mismatch at index {i}");

      Assert.AreEqual (last (src, MATCH_LENGTH), last (dst, MATCH_LENGTH),
          $"Suffix mismatch at index {i}");

      string? file = files
        .FirstOrDefault (src.Contains);
      if (file is null)
        continue;
      bool match = dst.Contains (file);
      Assert.IsTrue (match, "File must be in dest line");
      fileCount++;
    }

    Assert.AreEqual (files.Count, fileCount, "Not all files found");
  }

  private static string first (string s, int n) => s[..n];
  private static string last (string s, int n) => s[^n..];
}
