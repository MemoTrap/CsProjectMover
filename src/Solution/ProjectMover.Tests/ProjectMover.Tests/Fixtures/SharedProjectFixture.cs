#pragma warning disable IDE0130

using System.Xml.Linq;

using ProjectMover.Global.Lib;
using ProjectMover.Lib.Extensions;
using ProjectMover.Lib.Models;


namespace ProjectMover.Tests {
  using static Const;
  public static class SharedProjectFixture {
    /// <summary>
    /// Asserts that a copied shared project has a new GUID and that any referencing solution points to it.
    /// Also usable for legacy C# projects
    /// </summary>
    /// <param name="originalProjPath">Path to the original project (.shproj or .csProj)</param>
    /// <param name="copiedProjPath">Path to the copied project (.shproj or .csProj)</param>
    /// <param name="dependentSolutionPaths">Paths to all solutions that reference this project</param>
    public static async Task AssertProjectGuidUpdatedAsync (
        string originalProjPath,
        string copiedProjPath,
        params string[] dependentSolutionPaths
    ) {
      
      Guid originalGuid = loadProjGuid (originalProjPath);

      // Load copied project
      bool isSharedProject = SHPROJ.Equals(Path.GetExtension (copiedProjPath), StringComparison.OrdinalIgnoreCase); 
      ProjectFile copiedProject = isSharedProject ? 
        new SharedProjectFile (copiedProjPath) :
        new CsProjectFile (copiedProjPath);
      await copiedProject.LoadAsync ();
      if (!copiedProject.RequiresGuid)
        return;

      // 1️⃣ Copied project GUID must exist and be different from original
      Assert.AreNotEqual (Guid.Empty, copiedProject.ProjectGuid, "Copied project must have a non-empty GUID");
      Assert.AreNotEqual (originalGuid, copiedProject.ProjectGuid, "Copied project must have a new GUID");

      // 2️⃣ <SharedGUID> element in copied .shproj must match ProjectGuid
      Guid copyGuid = loadProjGuid (copiedProjPath);
      
      Assert.AreEqual (
          copiedProject.ProjectGuid,
          copyGuid,
          "Copied project <ProjectGuid> must match the new ProjectGuid"
      );

      if (isSharedProject) {
        string copiedShprojItemsPath = copiedProjPath.ReplaceFileExtensionWith (SharedProjectFile.PROJITEMS_EXT);
        Guid copyItemGuid = loadProjGuid (copiedShprojItemsPath);

        Assert.AreEqual (
            copyItemGuid,
            copyGuid,
            "Copied shared project items <SharedGUID> must match the new ProjectGuid"
        );
      }

      // 3️⃣ All dependent solutions must reference the new GUID
      string oldGuidString = originalGuid.ToString ("B"); // solution files use brace GUIDs uppercase
      string newGuidString = copiedProject.ProjectGuid.ToString ("B"); // solution files use brace GUIDs uppercase
      foreach (var solPath in dependentSolutionPaths) {
        var solText = await File.ReadAllTextAsync (solPath);
        Assert.IsTrue (
            solText.Contains (newGuidString, StringComparison.OrdinalIgnoreCase),
            $"Solution {Path.GetFileName (solPath)} must reference the new shared project GUID"
        );
        Assert.IsFalse (
            solText.Contains (oldGuidString, StringComparison.OrdinalIgnoreCase),
            $"Solution {Path.GetFileName (solPath)} must not reference the original shared project GUID"
        );
      }
    }

    /// <summary>
    /// Asserts that a copied shared project has updated relative paths to shared projitems.
    /// </summary>
    /// <param name="originalShprojPath">Path to the original shared project (.shproj)</param>
    /// <param name="copiedShprojPath">Path to the copied/shared project (.shproj)</param>
    /// <param name="dependentSolutionPaths">Paths to all solutions that reference this shared project</param>
    public static async Task AssertSharedProjitemsUpdatedAsync (
        string originalShprojPath,
        string copiedShprojPath,
        params string[] dependentSolutionPaths
    ) {

      foreach ( var solPath in dependentSolutionPaths ) {
        string? solDir = Path.GetDirectoryName ( solPath );
        Assert.IsNotNull (solDir);
        string oldRelPath = Path.GetRelativePath ( solDir, originalShprojPath );
        string newRelPath = Path.GetRelativePath ( solDir, copiedShprojPath);

        oldRelPath = oldRelPath.ReplaceFileExtensionWith (".projitems");
        newRelPath = newRelPath.ReplaceFileExtensionWith (".projitems");

        var solText = await File.ReadAllTextAsync (solPath);
        Assert.IsTrue (
            solText.Contains (newRelPath, StringComparison.OrdinalIgnoreCase),
            $"Solution {Path.GetFileName (solPath)} must reference the new shared project items relative path"
        );
        Assert.IsFalse (
            solText.Contains (oldRelPath, StringComparison.OrdinalIgnoreCase),
            $"Solution {Path.GetFileName (solPath)} must not reference the original shared project project items relative path"
        );

      }

    }

    private static Guid loadProjGuid (string projPath) {
      var doc = XDocument.Load (projPath);

      var guidElement =
          doc.Descendants ()
             .FirstOrDefault (e =>
                 e.Name.LocalName is "SharedGUID" or "ProjectGuid");

      if (guidElement is null || string.IsNullOrWhiteSpace (guidElement.Value))
        throw new AssertFailedException ($"No ProjectGuid or SharedGUID in {projPath}");

      return Guid.Parse (guidElement.Value);
    }

  }
}
