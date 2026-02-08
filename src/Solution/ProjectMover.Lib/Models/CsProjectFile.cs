#pragma warning disable IDE0305

namespace ProjectMover.Lib.Models {
  internal class CsProjectFile (string projectFilePath) : ProjectFile(projectFilePath, EXT) {
    public const string EXT = ".csproj";

    private XDocument? _xml;
    private XElement? _projectGuidElement;   

    protected override string Extension => EXT;


    // Collections of nodes with relative paths
    public List<ProjectReference> ProjectReferences { get; private set; } = [];
    public List<ReferenceItem> ReferenceItems { get; private set; } = [];
    public List<CompileItem> CompileItems { get; private set; } = [];
    public List<ContentItem> ContentItems { get; private set; } = [];
    public List<ImportItem> ImportItems { get; private set; } = [];

    public string? AssemblyName { get; private set; } = null;

    public override IReadOnlyList<string> SharedProjectImportsAbsPath =>
      ImportItems
        .Where (i => i.Project.EndsWith (SharedProjectFile.PROJITEMS_EXT, StringComparison.OrdinalIgnoreCase))
        .Select (i => i.AbsolutePath)
        .ToList ();

    public override EProjectType ProjectType => EProjectType.CSharp;
    public override bool RequiresGuid => !IsSdkStyle;

    public bool IsSdkStyle { get; private set; }

    public override async Task LoadAsync (CancellationToken ct = default) {

      using var stream = File.OpenRead (AbsolutePath);
      _xml = await XDocument.LoadAsync (stream, LoadOptions.PreserveWhitespace, ct);

      var root = _xml.Root
        ?? throw new InvalidOperationException ("Invalid project file");

      IsSdkStyle = root.Attribute ("Sdk") != null;


      var ns = _xml.Root?.Name.Namespace ?? XNamespace.None;

      // ProjectGuid (legacy only)
      if (!IsSdkStyle) {
        // Read ProjectGuid
        var guidElement = root
            .Descendants ()
            .FirstOrDefault (e => e.Name.LocalName == nameof (ProjectGuid));
        if (guidElement is null || string.IsNullOrWhiteSpace (guidElement.Value))
          throw new InvalidOperationException ("Legacy C# project has no ProjectGuid.");

        _projectGuidElement = guidElement;
        ProjectGuid = Guid.Parse (guidElement.Value);
        OrigProjectGuid = ProjectGuid;
      }

      // Assembly name
      AssemblyName =
       _xml
         .Descendants (ns + "AssemblyName")
         .Select (e => e.Value)
         .FirstOrDefault ();

      // ProjectReference
      ProjectReferences = _xml.Descendants (ns + "ProjectReference")
          .Select (x => new ProjectReference ((string)x.Attribute (nameof(IIncludeItem.Include))!, x, Directory))
          .ToList ();

      // HintPath inside Reference
      ReferenceItems = _xml.Descendants (ns + "Reference")
          .SelectMany (x => x.Elements (ns + nameof(ReferenceItem.HintPath))
                            .Select (h => new ReferenceItem (h.Value, h, Directory)))
          .ToList ();

      // Compile
      CompileItems = _xml.Descendants (ns + "Compile")
          .Where (x => x.Attribute ("Include") != null)
          .Select (x => new CompileItem ((string)x.Attribute (nameof (IIncludeItem.Include))!, x, Directory))
          .ToList ();

      // Content
      ContentItems = _xml.Descendants (ns + "Content")
          .Where (x => x.Attribute ("Include") != null)
          .Select (x => new ContentItem ((string)x.Attribute (nameof (IIncludeItem.Include))!, x, Directory))
          .ToList ();

      // None
      ContentItems.AddRange (_xml.Descendants (ns + "None")
          .Where (x => x.Attribute ("Include") != null)
          .Select (x => new ContentItem ((string)x.Attribute (nameof (IIncludeItem.Include))!, x, Directory)));

      // Import
      ImportItems = _xml.Descendants (ns + "Import")
          .Where (x => x.Attribute ("Project") != null)
          .Select (x => new ImportItem ((string)x.Attribute (nameof (ImportItem.Project))!, x, Directory))
          .ToList ();


      ProjectReferencesAbsPath = ProjectReferences
        .Select (pr => Path.GetFullPath (Path.Combine (Directory, pr.Include)))
        .ToList ();
    }

    public override async Task<string> SaveExAsync (bool dryRun, CancellationToken ct = default) {
      if (_xml is null)
        throw new InvalidOperationException ("Project not loaded.");

      var xmlSettings = new XmlWriterSettings {
        Async = true,
        Indent = true,
        OmitXmlDeclaration = IsSdkStyle,
      };

      if (dryRun) {
        _xml.AddFirst (new XComment ($" {AbsolutePath} "));
        StringBuilder sb = new ();
        using (var writer = XmlWriter.Create (sb, xmlSettings))
          await _xml.SaveAsync (writer, ct);
        DryRunSave = sb.ToString ();
      } else { 
        using var writer = XmlWriter.Create (AbsolutePath, xmlSettings);
        await _xml.SaveAsync (writer, ct);
      }

      updateLastWriteTime ();
      return AbsolutePath;
    }

    public override Guid ProjectRefGuid (ProjectFile projectReference) {
      return ProjectReferences
        .FirstOrDefault (r =>
          r.OriginalAbsolutePath.Equals (projectReference.OrigAbsPath, StringComparison.OrdinalIgnoreCase))?
        .OrigProjectGuid ?? default;
    }

    public override void ApplyProjectPlan (ProjectOperationPlan plan) {
      if (_xml is null)
        throw new InvalidOperationException ("Project not loaded.");

      // The plan could be for our project itslef or for one of our dependent projects
      bool self = plan.Project == this;

      if (self)
        applyToSelf (plan);
      else
        applyFromDependentProject (plan);
    }


    private void applyToSelf (ProjectOperationPlan plan) {
      // only if user-selected
      if (!plan.IsUserSelected)
        return;

      // 1. Update own project file path (in-memory only)
      // which is the new and only one from now on.
      // This is allowed because all include items have the old dir path stored
      if (plan.NewProjectFolder is not null || plan.NewProjectName is not null) {
        string newProjPath = getNewAbsProjectPath (plan);
        updateFilePath (newProjPath);
      }

      // 2. Update AssemblyName if specified
      if (plan.NewAssemblyName is not null) {
        setOrUpdateProperty (nameof (AssemblyName), plan.NewAssemblyName);
        AssemblyName = plan.NewAssemblyName;
      }

      // 3 Copy mode and non-SDK: assign new GUID 
      if (!IsSdkStyle && (plan.Copy ?? false) && plan.NewProjectGuid != default) {
        ProjectGuid = plan.NewProjectGuid;
        if (_projectGuidElement is not null)
          _projectGuidElement.Value = ProjectGuid.ToString ();
      }


      // 4. Rewrite project references, always
      rewriteIncludes (ProjectReferences, Directory);

      // 5. Rewrite reference/compile/content+none/import includes
      rewriteIncludes (ReferenceItems, Directory);
      rewriteIncludes (CompileItems, Directory);
      rewriteIncludes (ContentItems, Directory);
      rewriteIncludes (ImportItems, Directory);
    }


    private void applyFromDependentProject (ProjectOperationPlan plan) {
      if (plan.Project is SharedProjectFile)
        applyFromDependentShProject (plan);
      else
        applyFromDependentCsProject (plan);
    }

    private void applyFromDependentShProject (ProjectOperationPlan plan) {
      if (plan.Project is not SharedProjectFile shProj)
        return;
      // do we have this dependency?
      string origAbsPath = shProj.OrigProjItemsAbsPath;
      var projRef = ImportItems
        .FirstOrDefault (r => string.Equals (origAbsPath, r.OriginalAbsolutePath, StringComparison.OrdinalIgnoreCase));
      if (projRef is null)
        return;

      string newRefProjPath = getNewAbsProjectPath (plan);
      newRefProjPath = newRefProjPath.ReplaceFileExtensionWith (SharedProjectFile.PROJITEMS_EXT);

      projRef.UpdateToNewAbsolutePath (Directory, newRefProjPath);
    }

    private void applyFromDependentCsProject (ProjectOperationPlan plan) {
      // do we have this dependency?
      string origAbsPath = plan.Project.OrigAbsPath;
      var projRef = ProjectReferences
        .FirstOrDefault (r => string.Equals (origAbsPath, r.OriginalAbsolutePath, StringComparison.OrdinalIgnoreCase));
      if (projRef is null)
        return;

      string newRefProjPath = getNewAbsProjectPath (plan);
      projRef.UpdateToNewAbsolutePath (Directory, newRefProjPath);

      if ((plan.Copy ?? false) && plan.Project.RequiresGuid) {
        if (plan.NewProjectGuid == default)
          throw new InvalidOperationException (
            $"{plan.GetType ().Name} in copy mode must have GUID: {plan.Project.AbsolutePath}");
        projRef.UpdateProjectGuid (plan.NewProjectGuid);
      }
    }

    private static string getNewAbsProjectPath (ProjectOperationPlan plan) {
      string ext = Path.GetExtension (plan.Project.OrigAbsPath);
      string newProjectFolder = plan.NewProjectFolder ?? plan.OldProjectFolder;
      string newProjectName = plan.NewProjectName ?? plan.OldProjectName;
      string newProjPath = Path.Combine (
        newProjectFolder,
        newProjectName + ext);

      return newProjPath;
    }


    private static void rewriteIncludes (
      IEnumerable<IIncludeItem> items,
      string newProjectDir
    ) {
      foreach (var item in items)
        item.RewriteRelativePath (newProjectDir);
    }

    private void setOrUpdateProperty (string propertyName, string value) {
      if (_xml is null)
        throw new InvalidOperationException ("Project not loaded.");

      var ns = _xml.Root!.Name.Namespace;

      var element =
          _xml
            .Descendants (ns + propertyName)
            .FirstOrDefault ();

      if (element != null) {
        element.Value = value;
        return;
      }

      // Create it if missing
      var pg =
          _xml
            .Root!
            .Elements (ns + "PropertyGroup")
            .FirstOrDefault (e => !e.Attributes ().Any ());

      if (pg == null) {
        pg = new XElement (ns + "PropertyGroup");
        _xml.Root!.AddFirst (pg);
      }

      pg.Add (new XElement (ns + propertyName, value));
    }

  }

}
