#pragma warning disable IDE0057

namespace ProjectMover.Lib.Models {
  internal interface IIncludeItem {
    /// <summary>
    /// The path as stored in the project file (relative to project folder)
    /// </summary>
    string Include { get; }

    /// <summary>
    /// Absolute path convenience property
    /// </summary>
    string AbsolutePath { get; }

    /// <summary>
    /// Rewrite the relative path given a new project folder
    /// </summary>
    void RewriteRelativePath (string newProjectDir);
  }

  internal abstract class IncludeItem : IIncludeItem {
    protected string _projectDir; // directory of the project file (old folder)
    private string _include;

    public string Include {
      get => _include;
      protected set {
        _include = value;
        // Update XML attribute/value
        updateElementInclude ();
      }
    }
    public XElement Element { get; }

    public string OriginalAbsolutePath { get; }


    protected const string MSBUILD_THIS_FILE_DIRECTORY = "$(MSBuildThisFileDirectory)";

    public IncludeItem (string include, XElement element, string projectDir) {
      _projectDir = projectDir;
      _include = include;
      Element = element;
      OriginalAbsolutePath = AbsolutePath;
    }

    public string AbsolutePath {
      get {
        if (Include.StartsWith (MSBUILD_THIS_FILE_DIRECTORY, StringComparison.Ordinal)) {
          string relative = Include.Substring (MSBUILD_THIS_FILE_DIRECTORY.Length);
          return Path.GetFullPath (Path.Combine (_projectDir, relative));
        }
        return Path.GetFullPath (Path.Combine (_projectDir, Include));
      }
    }

    public virtual void RewriteRelativePath (string newProjectDir) {
      string oldProjectDir = Path.GetFullPath (_projectDir)
          .TrimEnd (Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

      string oldFileAbs = AbsolutePath;

      bool hadMsBuildPrefix =
          Include.StartsWith (MSBUILD_THIS_FILE_DIRECTORY, StringComparison.OrdinalIgnoreCase);

      string newRel;

      if (oldFileAbs.StartsWith (oldProjectDir, StringComparison.OrdinalIgnoreCase)) {
        // File moved with project → preserve relative path
        newRel = oldFileAbs.Substring (oldProjectDir.Length);
      } else {
        // External file → compute relative path to new project folder
        newRel = Path.GetRelativePath (newProjectDir, oldFileAbs);
      }

      newRel = newRel.TrimStart (Path.DirectorySeparatorChar);

      Include = hadMsBuildPrefix
          ? MSBUILD_THIS_FILE_DIRECTORY + newRel
          : newRel;

    }

    internal void UpdateToNewAbsolutePath (string newProjectDir, string newReferencedProjectAbsPath) {
      var newAbs = Path.GetFullPath (newReferencedProjectAbsPath);
      var newRelative = Path.GetRelativePath (newProjectDir, newAbs);
      Include = newRelative;
    }


    /// <summary>
    /// Derived classes override how to update the underlying XML.
    /// </summary>
    protected abstract void updateElementInclude ();
  }

  // --- Concrete items ---

  internal class CompileItem : IncludeItem {
    public string? Link { get; }

    public CompileItem (string include, XElement element, string projectDir)
        : base (include, element, projectDir) {
      var ns = element.Name.Namespace;
      Link = element.Element (ns + "Link")?.Value;
    }

    protected override void updateElementInclude () {
      Element.SetAttributeValue (nameof (Include), Include);
    }
  }

  internal class ContentItem (string include, XElement element, string projectDir) :
    IncludeItem (include, element, projectDir) {
    protected override void updateElementInclude () {
      Element.SetAttributeValue (nameof (Include), Include);
    }
  }

  internal class ImportItem (string project, XElement element, string projectDir) : 
    IncludeItem(project, element, projectDir) 
  {
    public string Project {
      get => Include;
      set => Include = value;
    }

    protected override void updateElementInclude () {
      Element.SetAttributeValue (nameof (Project), Include);
    }
  }

  internal class ProjectReference : IncludeItem {
    private XElement? _projectGuidElement;

    public Guid? ProjectGuid { get; private set; }
    public Guid? OrigProjectGuid { get; }

    public ProjectReference (string include, XElement element, string projectDir) : 
      base (include, element, projectDir) {

      var ns = element.Name.Namespace;

      _projectGuidElement =
        element.Elements (ns + "Project").FirstOrDefault ();

      if (_projectGuidElement != null &&
                  Guid.TryParse (_projectGuidElement.Value.Trim ('{', '}'), out var g)) {
        ProjectGuid = g;
        OrigProjectGuid = g;
      }
    }

    public void UpdateProjectGuid (Guid newGuid) {
      var ns = Element.Name.Namespace;

      if (_projectGuidElement == null) {
        _projectGuidElement = new XElement (ns + "Project");
        Element.Add (_projectGuidElement);
      }

      _projectGuidElement.Value = $"{{{newGuid.ToString ().ToUpperInvariant ()}}}";
      ProjectGuid = newGuid;
    }

    protected override void updateElementInclude () {
      Element.SetAttributeValue (nameof (Include), Include);
    }


  }

  internal class ReferenceItem (string hintPath, XElement element, string projectDir) :
    IncludeItem (hintPath, element, projectDir) {
    public string HintPath {
      get => Include;
      set => Include = value;
    }

    protected override void updateElementInclude () {
      Element.Value = Include;
    }
  }
}

