#pragma warning disable IDE0079
#pragma warning disable IDE0057
#pragma warning disable CA1860
#pragma warning disable IDE0305
#pragma warning disable IDE1006
#pragma warning disable SYSLIB1045

using System.Text.RegularExpressions;

namespace ProjectMover.Lib.Models {

  internal class SolutionFile (string filePath) : FileBase (filePath, EXT) {
    enum EParseState {
      Normal,

      // Global sections
      Global_ProjectConfigurationPlatforms,
      Global_NestedProjects,
      Global_SolutionConfigurationPlatforms,
      Global_SharedMSBuildProjectFiles,

      Global_Unknown
    }

    public const string EXT = ".sln";


    static class SolutionProjectTypes {
      public static readonly Guid SolutionFolder =
          Guid.Parse ("2150E333-8FDC-42A3-9474-1A3956D46DE8");

      public static readonly Guid CSharpProject =
          Guid.Parse ("9A19103F-16F7-4668-BE54-9A1E7A4F7556");
      public static readonly Guid CSharpProject2 =
          Guid.Parse ("22FDA992-EB8B-FF70-0906-5B831E6F14DC");
      public static readonly Guid CSharpProject3 =
          Guid.Parse ("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC");

      public static readonly Guid SharedProject =
          Guid.Parse ("D954291E-2A0B-460D-934E-DC6B0785DB48");
    }



    abstract record LineBase {
      public abstract Task WriteLineAsync (TextWriter writer);
      protected static string gs (Guid guid) => guid.ToString ("B").ToUpper ();
    }

    record PlainTextLine (string Text) : LineBase {
      public static PlainTextLine Parse (string textLine) => new (textLine);
      public override Task WriteLineAsync (TextWriter writer) => 
        writer.WriteLineAsync (Text);
    }

    record EndGlobalSectionLine (string Text) : PlainTextLine (Text) {
      private static readonly Regex EndSectionLineRegex = new (
        @"^\s+EndGlobalSection\b",
        RegexOptions.Compiled);

      public static PlainTextLine? TryParse (string textLine) {
        if (!EndSectionLineRegex.IsMatch (textLine))
          return null;
        return new EndGlobalSectionLine (textLine);
      }
    }

    record ProjectLine (
      EProjectType ProjectType,
      Guid TypeGuid,
      string Name,
      string RelativePath,
      string? AbsolutePath,
      Guid ProjectGuid,
      Guid OrigProjectGuid
    ) : LineBase {
      private static readonly Regex regex = new (
        @"^Project\(""\{(?<type>[^}]+)\}""\)\s*=\s*""(?<name>[^""]+)"",\s*""(?<path>[^""]+)"",\s*""\{(?<guid>[^}]+)\}""",
        RegexOptions.Compiled);

      public static ProjectLine? TryParse (string textLine, SolutionFile sln) {
        Match m = regex.Match (textLine);
        if (!m.Success)
          return null;

        var typeGuid = Guid.Parse (m.Groups["type"].Value);

        // The GUID check may not be exhaustive!
        EProjectType projectType = typeGuid switch {
          var g when g == SolutionProjectTypes.CSharpProject => EProjectType.CSharp,
          var g when g == SolutionProjectTypes.CSharpProject2 => EProjectType.CSharp,
          var g when g == SolutionProjectTypes.CSharpProject3 => EProjectType.CSharp,
          var g when g == SolutionProjectTypes.SharedProject => EProjectType.Shared,
          var g when g == SolutionProjectTypes.SolutionFolder => EProjectType.Folder,
          _ => EProjectType.Unknown
        };

        var name = m.Groups["name"].Value;
        var relPath = m.Groups["path"].Value;
        var guid = Guid.Parse (m.Groups["guid"].Value);

        if (projectType == EProjectType.Unknown) {
          // go by file type
          string ext = Path.GetExtension (relPath);
          projectType = ext.ToLowerInvariant () switch {
            CsProjectFile.EXT => EProjectType.CSharp,
            SharedProjectFile.EXT => EProjectType.Shared,
            _ => EProjectType.Unknown
          };
        }

        string? absPath = null;
        if (!(projectType is EProjectType.Folder or EProjectType.Unknown))
          absPath = sln.toAbsolute (relPath);

        return new ProjectLine(projectType, typeGuid, name, relPath, absPath, guid, guid);
      }

      public override Task WriteLineAsync (TextWriter writer) =>
        writer.WriteLineAsync (
          $@"Project(""{gs (TypeGuid)}"") = ""{Name}"", ""{RelativePath}"", ""{gs (ProjectGuid)}"""
        );
    }

    record NestedProjectLine (
      string Indent,
      Guid ChildGuid,
      Guid ParentGuid
    ) : LineBase {

      private static readonly Regex regex = new (
        @"^(?<indent>\s*)\{(?<child>[0-9A-Fa-f\-]+)\}\s*=\s*\{(?<parent>[0-9A-Fa-f\-]+)\}",
        RegexOptions.Compiled);
      
      public static NestedProjectLine? TryParse (string textLine, SolutionFile sln) {

        Match m = regex.Match (textLine);
        if (!m.Success)
          return null;

        var indent = m.Groups["indent"].Value;
        var childGuid = Guid.Parse (m.Groups["child"].Value);
        var parentGuid = Guid.Parse (m.Groups["parent"].Value);

        return new (indent, childGuid, parentGuid);
      }

      public override Task WriteLineAsync (TextWriter writer) =>
        writer.WriteLineAsync (
          $"{Indent}{gs (ChildGuid)} = {gs (ParentGuid)}"
        );
    }

    record ProjectConfigurationPlatformLine (
      string Indent,
      Guid ProjectGuid,
      string Key,
      string After
    ) : LineBase {
      private static readonly Regex regex = new (
        @"^(?<indent>\s+)\{(?<projectGuid>[A-Fa-f0-9\-]+)\}\.(?<key>[^=]+)(?<after>\s*=\s*.+)$",
        RegexOptions.Compiled);

      public static ProjectConfigurationPlatformLine? TryParse (string textLine, SolutionFile sln) {

        Match m = regex.Match (textLine);
        if (!m.Success)
          return null;

        string indent = m.Groups["indent"].Value;
        Guid projectGuid = Guid.Parse (m.Groups["projectGuid"].Value);
        string key = m.Groups["key"].Value;
        string after = m.Groups["after"].Value;

        return new (
          indent,
          projectGuid,
          key,
          after
        );
      }

      public override Task WriteLineAsync (TextWriter writer) => 
        writer.WriteLineAsync (
          $"{Indent}{gs (ProjectGuid)}.{Key}{After}"
         );
    }

    record GlobalSectionLine (
        string Indent,
        string Keyword,
        string Kind,
        string After,
        EParseState State
    ) : LineBase {
      private static readonly Regex regex = new (
        @"^(?<indent>\s+)(?<keyword>GlobalSection)\((?<kind>[^)]+)\)(?<after>\s*=\s*\w+.*)$",
        RegexOptions.Compiled);

      public static GlobalSectionLine? TryParse (string textLine, SolutionFile sln) {

        Match m = regex.Match (textLine);
        if (!m.Success)
          return null;

        EParseState state = getGlobalSectionState (m.Groups["kind"].Value);
        string globalKind = m.Groups["kind"].Value;
        string indent = m.Groups["indent"].Value;
        string keyword = m.Groups["keyword"].Value;
        string after = m.Groups["after"].Value;

        return new (
          indent,
          keyword,
          globalKind,
          after,
          state
        );
      }

      public override Task WriteLineAsync (TextWriter writer) => 
        writer.WriteLineAsync (
          $"{Indent}{Keyword}({Kind}){After}"
        );
    }

    record SharedMsBuildProjectFileLine (
        string Indent,
        string RelativeProjItemsPath,
        string AbsoluteProjItemsPath,
        Guid ProjectGuid,
        string TrailingText   // "*SharedItemsImports = 5"
    ) : LineBase {
      private static readonly Regex regex = new (
        @"^(?<indent>\s*)(?<path>[^*]+)\*\{(?<guid>[0-9a-fA-F\-]+)\}(?<trailing>\*.*)$",
        RegexOptions.Compiled);

      public static SharedMsBuildProjectFileLine? TryParse (string textLine, SolutionFile sln) {

        Match m = regex.Match (textLine);
        if (!m.Success)
          return null;

        var indent = m.Groups["indent"].Value;
        var relPath = m.Groups["path"].Value;
        var absPath = sln.toAbsolute (relPath);
        var guid = Guid.Parse (m.Groups["guid"].Value);
        var trailing = m.Groups["trailing"].Value;

        return new (indent, relPath, absPath, guid, trailing);
      }


      public override Task WriteLineAsync (TextWriter writer) => 
        writer.WriteLineAsync (
          $"{Indent}{RelativeProjItemsPath}*{gs (ProjectGuid)}{TrailingText}"
        );
    }

    private List<LineBase> SolutionLines { get; set; } = [];
    private List<ProjectLine> ProjectLines { get; set; } = [];
    internal IEnumerable<string> ProjectRelativePaths =>
      ProjectLines
        .Where (pl => pl.AbsolutePath is not null)
        .Select (pl => pl.RelativePath)
        .ToList ();

    public IEnumerable<string> ProjectsAbsPath { get; private set; } = [];

    public Guid ProjectGuid (string absProjectPath) {
      return ProjectLines
        .FirstOrDefault (pl => absProjectPath.Equals (pl.AbsolutePath, StringComparison.OrdinalIgnoreCase))?
        .ProjectGuid ?? default;
    }

    public override async Task LoadAsync (CancellationToken ct = default) {
      SolutionLines = [];
      
      EParseState state = EParseState.Normal;

      await foreach (var line in File.ReadLinesAsync (AbsolutePath, ct)) {

        switch (state) {
          case EParseState.Normal: 
            parseGlobalSectionBeginOrNormalLine ( line );
            continue;

          case EParseState.Global_ProjectConfigurationPlatforms:
            parseInGlobalSectionLine (line, ProjectConfigurationPlatformLine.TryParse);
            continue;
            
          case EParseState.Global_NestedProjects:
            parseInGlobalSectionLine (line, NestedProjectLine.TryParse);
            continue;

          case EParseState.Global_SharedMSBuildProjectFiles:
            parseInGlobalSectionLine (line, SharedMsBuildProjectFileLine.TryParse);
            continue;
          
            // in a global section not processed or unknown
          default:
            parseInGlobalSectionLine (line, null);
            continue;      
        }
      }


      // convenience collections

      ProjectLines = SolutionLines
        .OfType<ProjectLine> ()
        .ToList ();

      ProjectsAbsPath = ProjectLines
        .Where (pl => pl.AbsolutePath is not null)
        .Select (pl => pl.AbsolutePath!)
        .ToList ();

      #region local functions

      void parseGlobalSectionBeginOrNormalLine (string line) {
        var gsl = GlobalSectionLine.TryParse (line, this);
        if (gsl != null) {
          state = gsl.State;
          SolutionLines.Add (gsl);
          return;
        }

        parseNormalKnownLine (line);
      }


      void parseInGlobalSectionLine (string line, Func<string, SolutionFile, LineBase?>? tryParse) {
        // Check for end of global section
        var esl = EndGlobalSectionLine.TryParse (line);
        if (esl != null) {
          state = EParseState.Normal;
          SolutionLines.Add (esl);
          return;
        }

        // Parse global section content line
        if (tryParse != null) {
          var isl = tryParse (line, this);
          if (isl != null) {
            SolutionLines.Add (isl);
            return;            
          }
        }

        // Anything else is illegal but preserved
        var ptl = PlainTextLine.Parse (line);
        SolutionLines.Add (ptl);
      }


      void parseNormalKnownLine (string line) {
        var pl = ProjectLine.TryParse (line, this);
        if (pl != null) {
          SolutionLines.Add (pl); 
          return;
        }

        var ptl = PlainTextLine.Parse (line);
        SolutionLines.Add (ptl);
      }


      #endregion local functions
    }

    private static EParseState getGlobalSectionState (string kind) =>
      kind switch {
        "ProjectConfigurationPlatforms" => EParseState.Global_ProjectConfigurationPlatforms,
        "NestedProjects" => EParseState.Global_NestedProjects,
        "SolutionConfigurationPlatforms" => EParseState.Global_SolutionConfigurationPlatforms,
        "SharedMSBuildProjectFiles" => EParseState.Global_SharedMSBuildProjectFiles,
        _ => EParseState.Global_Unknown
      };


    /// <summary>
    /// Find if a solution contains the specified absolute project path.
    /// </summary>
    public bool ContainsProject (string absProjPath) {
      if (!ProjectLines.Any ())
        throw new InvalidOperationException ("Solution not loaded.");

      var normalized = Path.GetFullPath (absProjPath);

      return ProjectLines
          .Any (pl =>
              StringComparer.OrdinalIgnoreCase.Equals (
                  pl.AbsolutePath, normalized));

    }


    public void ApplyProjectPlan (ProjectOperationPlan plan) {
      if (Directory is null)
        throw new InvalidOperationException ("Solution not loaded.");

      if (!plan.IsUserSelected)
        return;

      bool copy = plan.Copy ?? false;

      string newAbsPath = plan.Project.AbsolutePath;
      string newRelPath = Path.GetRelativePath (Directory, newAbsPath);

      ProjectFile proj = plan.Project;
      SharedProjectFile? shProj = proj as SharedProjectFile;

      for (int i = 0; i < SolutionLines.Count; i++) {
        if (SolutionLines[i] is ProjectLine pl) {
          // old paths must match
          if (!string.Equals (pl.AbsolutePath, plan.Project.OrigAbsPath, StringComparison.OrdinalIgnoreCase))
            continue;

          Guid projectGuid = pl.ProjectGuid;

          if (copy && plan.NewProjectGuid != default)
            projectGuid = plan.NewProjectGuid;

          SolutionLines[i] = pl with {
            RelativePath = newRelPath,
            AbsolutePath = toAbsolute (newRelPath),
            ProjectGuid = projectGuid
          };
        }
      }

      // Project lines may be new instances. Update extract.
      ProjectLines = SolutionLines.OfType<ProjectLine> ().ToList ();


      // Other solution lines may be affected: shared project items
      if (shProj is not null) {
        string oldProjItemsAbs = shProj.OrigProjItemsAbsPath;
        string newProjItemsAbs = shProj.ProjItemsAbsPath;

        string newRelPathSol = Path.GetRelativePath (Directory, newProjItemsAbs);

        for (int i = 0; i < SolutionLines.Count; i++) {
          var line = SolutionLines[i];
          if (line is SharedMsBuildProjectFileLine s) {

            if (s.AbsoluteProjItemsPath.Equals (oldProjItemsAbs, StringComparison.OrdinalIgnoreCase)) {
              s = s with {
                AbsoluteProjItemsPath = newProjItemsAbs,
                RelativeProjItemsPath = newRelPathSol
              };
            }

            if (s.ProjectGuid == shProj.OrigProjectGuid) {
              s = s with {
                ProjectGuid = shProj.ProjectGuid
              };
            }

            SolutionLines[i] = s;
          }

        }
      }

      // Other solution lines may be affected for GUID only:
      // - NestedProjects
      // - ProjectConfigurationPlatforms
      if (copy) {
        Guid origProjGuid = ProjectLines
          .FirstOrDefault (l =>
            l.AbsolutePath?.Equals (proj.AbsolutePath, StringComparison.OrdinalIgnoreCase) ?? false)?
          .OrigProjectGuid ?? default;

        for (int i = 0; i < SolutionLines.Count; i++) {
          SolutionLines[i] = SolutionLines[i] switch {
            NestedProjectLine npl => npl with {
              ChildGuid = npl.ChildGuid == origProjGuid ? plan.NewProjectGuid : npl.ChildGuid,
              ParentGuid = npl.ParentGuid == origProjGuid ? plan.NewProjectGuid : npl.ParentGuid
            },

            ProjectConfigurationPlatformLine cpl
              when cpl.ProjectGuid == origProjGuid
                => cpl with { ProjectGuid = plan.NewProjectGuid },

            var line => line
          };

        }
      }
    }

    public override async Task<string> SaveExAsync (bool dryRun, CancellationToken ct = default) {
      if (!SolutionLines.Any ())
        throw new InvalidOperationException ("Solution not loaded.");

      using TextWriter writer = dryRun ?
        new StringWriter () :
        new StreamWriter (AbsolutePath, false, new UTF8Encoding (true));

      using var rg = new ResourceGuard (onDryRun);

      foreach (var line in SolutionLines)
        await line.WriteLineAsync (writer);

      updateLastWriteTime ();

      return AbsolutePath;

      void onDryRun (bool allocate) {
        if (!dryRun)
          return;
        if (allocate)
          writer.WriteLine ($"# {AbsolutePath}");
        else 
          DryRunSave = writer.ToString ();
      }
    }

    private string toAbsolute (string relativePath) =>
      relativePath.ToAbsolutePath (Directory);


  }
}