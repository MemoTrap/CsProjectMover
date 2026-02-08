namespace ProjectMover.Lib.Models {
  internal class SharedProjectFile (string projectFilePath) : ProjectFile(projectFilePath, EXT) {
    public const string EXT = ".shproj";
    public const string PROJITEMS_EXT = ".projitems";

    // .shproj
    private XDocument? _shprojXml;
    private ImportItem? _projItemsImport;

    // .projitems
    private XDocument? _projItemsXml;
    private string _projItemsAbsPath = null!;
    //private string _projItemsDir = null!;

    private readonly List<IIncludeItem> _items = [];

    private XElement? _projectGuidElement;   // in .shproj
    private XElement? _sharedGuidElement;    // in .projitems

    protected override string Extension => EXT;

    public override EProjectType ProjectType => EProjectType.Shared;

    public override bool RequiresGuid => true;

    public string ProjItemsAbsPath => _projItemsAbsPath;
    public string OrigProjItemsAbsPath { get; private set; } = null!;
    public Guid OrigProjItemsGuid { get; private set; }

    public override async Task LoadAsync (CancellationToken ct = default) {
      ct.ThrowIfCancellationRequested ();

      // Load .shproj XML
      await loadShProjAsync (ct);

      if (!Path.GetDirectoryName (AbsolutePath)!
        .Equals (Path.GetDirectoryName (_projItemsAbsPath),
                StringComparison.OrdinalIgnoreCase)) {
        throw new InvalidOperationException (
            "Shared project .shproj and .projitems must be in the same folder.");
      }

      // Load .projitems XML
      await loadProjItemsAsync (ct);
    }

    private async Task loadShProjAsync (CancellationToken ct) {
      using var stream = File.OpenRead (AbsolutePath);

      _shprojXml = await XDocument.LoadAsync (stream, LoadOptions.PreserveWhitespace, ct);

      var root = _shprojXml.Root ?? throw new InvalidOperationException ("Invalid shproj.");

      // Read ProjectGuid
      var guidElement = root
          .Descendants ()
          .FirstOrDefault (e => e.Name.LocalName == nameof (ProjectGuid));

      if (guidElement is null || string.IsNullOrWhiteSpace (guidElement.Value))
        throw new InvalidOperationException ("Shared project has no ProjectGuid.");

      _projectGuidElement = guidElement;
      ProjectGuid = Guid.Parse (guidElement.Value);
      OrigProjectGuid = ProjectGuid;

      // Find the <Import> that points to the actual .projitems file
      var import = root
          .Elements ()
          .FirstOrDefault (e =>
              e.Name.LocalName == "Import" &&
              e.Attribute ("Project") is { } attr &&
              attr.Value.EndsWith (PROJITEMS_EXT, StringComparison.OrdinalIgnoreCase)) 
        ?? throw new InvalidOperationException ("Shared project has no .projitems import.");

      _projItemsImport = new ImportItem (import.Attribute ("Project")!.Value, import, Directory);
      _projItemsAbsPath = _projItemsImport.AbsolutePath;
      OrigProjItemsAbsPath = _projItemsAbsPath;

      if (!File.Exists (_projItemsAbsPath))
        throw new FileNotFoundException ("projitems not found.", _projItemsAbsPath);
    }


    private async Task loadProjItemsAsync (CancellationToken ct) {
      using var stream = File.OpenRead (_projItemsAbsPath);

      _projItemsXml = await XDocument.LoadAsync (stream, LoadOptions.PreserveWhitespace, ct);
      var ns = _projItemsXml.Root!.Name.Namespace;

      var sharedGuidElement = _projItemsXml
        .Descendants ()
        .FirstOrDefault (e => e.Name.LocalName == "SharedGUID");

      if (sharedGuidElement is null || string.IsNullOrWhiteSpace (sharedGuidElement.Value))
        throw new InvalidOperationException ("projitems has no SharedGUID.");

      _sharedGuidElement = sharedGuidElement;
      OrigProjItemsGuid = Guid.Parse (sharedGuidElement.Value);

      // Compile
      var compileItems = _projItemsXml.Descendants (ns + "Compile")
          .Where (x => x.Attribute ("Include") != null)
          .Select (x => new CompileItem ((string)x.Attribute (nameof (IIncludeItem.Include))!, x, Directory))
          .ToList ();

      _items.AddRange (compileItems);

      // Content
      var contentItems = _projItemsXml.Descendants (ns + "Content")
          .Where (x => x.Attribute ("Include") != null)
          .Select (x => new ContentItem ((string)x.Attribute (nameof (IIncludeItem.Include))!, x, Directory))
          .ToList ();

      // None
      contentItems.AddRange (_projItemsXml.Descendants (ns + "None")
          .Where (x => x.Attribute ("Include") != null)
          .Select (x => new ContentItem ((string)x.Attribute (nameof (IIncludeItem.Include))!, x, Directory)));

      _items.AddRange (contentItems);
    }


    public override async Task<string> SaveExAsync (bool dryRun, CancellationToken ct = default) {
      if (_shprojXml is null || _projItemsXml is null)
        throw new InvalidOperationException ("Project not loaded.");

      ct.ThrowIfCancellationRequested ();

      var xmlSettings = new XmlWriterSettings {
        Async = true,
        Indent = true,
        OmitXmlDeclaration = false,
      };

      if (dryRun) {
        StringBuilder sb = new ();

        _shprojXml.AddFirst (new XComment ($" {AbsolutePath} "));
        using (var writer = XmlWriter.Create (sb, xmlSettings))
          await _shprojXml.SaveAsync (writer, ct);

        sb.AppendLine ();

        _projItemsXml.AddFirst (new XComment ($" {_projItemsAbsPath} "));
        using (var writer = XmlWriter.Create (sb, xmlSettings))
          await _projItemsXml.SaveAsync (writer, ct);

        DryRunSave = sb.ToString ();
      } else {
        using (var writer = XmlWriter.Create (AbsolutePath, xmlSettings))
          await _shprojXml.SaveAsync (writer, ct);

        using (var writer = XmlWriter.Create (_projItemsAbsPath, xmlSettings))
          await _projItemsXml.SaveAsync (writer, ct);
      }

      updateLastWriteTime ();
      return AbsolutePath;
    }

    public override void ApplyProjectPlan (ProjectOperationPlan plan) {
      if (_shprojXml is null || _projItemsXml is null)
        throw new InvalidOperationException ("Project not loaded.");


      if (plan.Project != this || !plan.IsUserSelected)
        return;

      if (plan.NewProjectFolder is null)
        return;

      // --- Step 1: Copy mode → assign new GUIDs ---
      if (plan.Copy ?? false && plan.NewProjectGuid != default) { 
        ProjectGuid = plan.NewProjectGuid;

        string guid = ProjectGuid.ToString ();

        _projectGuidElement!.Value = guid;
        _sharedGuidElement!.Value = guid;
      }

      string newName = plan.NewProjectName ?? Name;

      // --- Step 2: Update project items name in .shproj and project name in .projitems if it changed ---
      if (!Name.Equals (plan.NewProjectName, StringComparison.Ordinal)) {

        // Update any PropertyGroup elements that reference project name
        var ns = _projItemsXml.Root!.Name.Namespace;

        // Update <Import_RootNamespace> only if it originally matched the old project name
        foreach (var el in _projItemsXml.Descendants (ns + "Import_RootNamespace")) {
          if (string.Equals (el.Value, Name, StringComparison.Ordinal)) {
            el.Value = newName;
          }
        }

        // Compute new .projitems filename
        string newProjItemsName = Path.GetFileNameWithoutExtension (_projItemsImport!.AbsolutePath)
                                  .Replace (Name, plan.NewProjectName) + SharedProjectFile.PROJITEMS_EXT;

        // Update Include / Project in the import item
        _projItemsImport.Project = newProjItemsName;

        //// Element attribute update
        //_projItemsImport.Element.SetAttributeValue ("Project", newProjItemsName);

      }


      // --- Step 3: Rewrite Compile / Content / None items relative paths ---
      // For shared projects, these files usually stay inside the folder, so the paths remain unchanged.
      foreach (var item in _items)
        item.RewriteRelativePath (plan.NewProjectFolder);


      // --- Step 4: Update internal paths ---

      _projItemsAbsPath = Path.Combine (plan.NewProjectFolder, newName + PROJITEMS_EXT);

      updateFilePath (Path.Combine (plan.NewProjectFolder, newName + EXT));
    }

  }
}
