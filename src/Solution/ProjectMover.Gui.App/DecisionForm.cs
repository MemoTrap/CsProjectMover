using System.Text;

namespace ProjectMover.Gui.App {
  public partial class DecisionForm : Form {

    private const string HDR_PROJ =
      "Dependent project roots that shall reference the copied project (includes dependent solutions)";
    private const string HDR_SLN =
     "Other related solutions that shall reference the copied project";

    private bool _fInit;

    private readonly ProjectDecisionContext _context;
    private readonly Func<CancellationTokenSource> _getCancelTokenSource;
    private readonly ProjectUserDecision _origDecision;
    public ProjectUserDecision Decision { get; private set; } = new ();

    public DecisionForm (
      ProjectDecisionContext context,
      Func<CancellationTokenSource> getCancelTokenSource
    ) {
      InitializeComponent ();
      _context = context;
      _getCancelTokenSource = getCancelTokenSource;

      _origDecision = Decision with { Include = true };

      setTitle ();
      init ();

      btnReset.Enabled = false;

      pathCheckListProj.ColumnHeader = HDR_PROJ;
      pathCheckListProj.CheckedItemsChanged += checkedItemsChanged;
      pathCheckListSln.ColumnHeader = HDR_SLN;
      pathCheckListSln.CheckedItemsChanged += checkedItemsChanged;



    }

    private void init () {
      using var rg = new ResourceGuard (x => _fInit = x);

      if (_context.CopyModeHint)
        toolTip.SetToolTip (txtBxNewAss, "New unique assembly name for copied project");
      else
        toolTip.SetToolTip (txtBxNewAss, "");

      if (_context.RetryReason is not null) {
        panelRetry.Visible = true;
        labelReason.Text = _context.RetryReason;
      }

      txtBxProjName.Text = _context.ProjectName;
      roTxtBoxCurProjFolder.SetTextAsPathWithEllipsis (_context.CurrentProjectFolder);
      txtBxNewProjFolder.Text = _context.DesignatedNewProjectFolder;
      roTxtBxCurAss.Text = _context.CurrentAssemblyName;
      txtBxNewAss.Text = _context.SuggestedAssemblyName;

      if (_context.CopyModeHint) {
        pathCheckListProj.SetPaths (_context.SelectableDependentProjectRoots);
        pathCheckListSln.SetPaths (_context.SelectableSolutions);
      } else {
        pathCheckListProj.Enabled = false;
        pathCheckListSln.Enabled = false;
      }

      setDecision ();
      enable ();
    }

    private void setDecision () {
      Decision = new ProjectUserDecision {
        Include = true,
        NewProjectName = getNullable (txtBxProjName, _context.ProjectName),
        NewProjectFolder = getNullable (txtBxNewProjFolder, _context.DesignatedNewProjectFolder),
        NewAssemblyName = getNullable (txtBxNewAss, _context.SuggestedAssemblyName),
        SelectedSolutions = pathCheckListSln.CheckedPaths,
        SelectedDependentProjectRoots = pathCheckListProj.CheckedPaths,
      };
    }


    private static string? getNullable (Control control, string? reference) {
      string? result = string.IsNullOrWhiteSpace (control.Text) ? null : control.Text;
      if (string.Equals (result, reference, StringComparison.OrdinalIgnoreCase))
        return null;
      return result;
    }

    private void setTitle () {
      StringBuilder sb = new ();
      if (_context.Preselected)
        sb.Append ("Preselected");
      sb.Append ($" Project {_context.IndexInBatch} of {_context.TotalInBatch}: ");
      if (_context.CopyModeHint)
        sb.Append ("Copy");
      else
        sb.Append ("Move / Rename");
      sb.Append ($" Details for \"{_context.ProjectName}\"");

      if (_context.RetryReason is not null)
        sb.Append (", RETRY");

      Text = sb.ToString ();
    }

    private void btnOK_Click (object sender, EventArgs e) {
      DialogResult = DialogResult.OK;
      Close ();
    }

    private void btnSkip_Click (object sender, EventArgs e) {
      DialogResult = DialogResult.None;
      Close ();
    }

    private void btnCancel_Click (object sender, EventArgs e) {
      Decision = new ();
      _getCancelTokenSource ().Cancel ();
      Close ();
    }

    private void btnReset_Click (object sender, EventArgs e) {
      init ();
    }

    private void btnFolderUp_Click (object sender, EventArgs e) {
      string? newProjFolderRoot = Path.GetDirectoryName (Decision.NewProjectFolder ?? _context.DesignatedNewProjectFolder);
      string newProjectName = Decision.NewProjectName ?? _context.ProjectName;
      if (newProjFolderRoot is null)
        txtBxNewProjFolder.Text = newProjectName;
      else
        txtBxNewProjFolder.Text = Path.Combine (newProjFolderRoot, newProjectName);
    }

    private void btnFolderDn_Click (object sender, EventArgs e) {
      string newProjSubFolderName = Path.GetFileName (Decision.NewProjectFolder ?? _context.DesignatedNewProjectFolder);
      txtBxProjName.Text = newProjSubFolderName;
    }

    private void btnAssDn_Click (object sender, EventArgs e) {
      string newProjectName = Decision.NewProjectName ?? _context.ProjectName;
      txtBxNewAss.Text = newProjectName;
    }

    private void btnAssUp_Click (object sender, EventArgs e) {
      string? newAssemblyName = Decision.NewAssemblyName ?? _context.SuggestedAssemblyName ?? _context.CurrentAssemblyName;
      if (newAssemblyName is not null)
        txtBxProjName.Text = newAssemblyName;
    }

    private void txtBxNewProjFolder_TextChanged (object sender, EventArgs e) {
      if (_fInit)
        return;
      Decision = Decision with {
        NewProjectFolder = getNullable (txtBxNewProjFolder, _context.DesignatedNewProjectFolder)
      };
      enable ();
    }

    private void txtBxProjName_TextChanged (object sender, EventArgs e) {
      if (_fInit)
        return;

      Decision = Decision with {
        NewProjectName = getNullable (txtBxProjName, _context.ProjectName)
      };
      enable ();
    }

    private void txtBxNewAss_TextChanged (object sender, EventArgs e) {
      if (_fInit)
        return;

      Decision = Decision with {
        NewAssemblyName = getNullable (txtBxNewAss, _context.SuggestedAssemblyName)
      };
      enable ();
    }

    private void enable () {
      btnReset.Enabled = _origDecision != Decision;

      string newProjSubFolderName = Path.GetFileName (Decision.NewProjectFolder ?? _context.DesignatedNewProjectFolder);
      string newProjectName = Decision.NewProjectName ?? _context.ProjectName;
      bool canSyncFolder = !string.Equals (newProjSubFolderName, newProjectName, StringComparison.OrdinalIgnoreCase);

      btnFolderDn.Enabled = canSyncFolder;
      btnFolderUp.Enabled = canSyncFolder;

      bool hasNewProjectName = Decision.NewProjectName is not null && !string.Equals (_context.ProjectName, Decision.NewProjectName);
      bool equalAssAndProjName = string.Equals (Decision.NewAssemblyName, Decision.NewProjectName ?? _context.ProjectName);

      // It only makes sense to sync between project and assembly name, if in copy mode or current assembly is not null
      // and - in copy mode - new assembly name unequal suggested assembly name 

      bool canSyncAssDn;
      bool canSyncAssUp;

      if (_context.CopyModeHint) {
        bool hasNewAssemblyName = !string.Equals (_context.SuggestedAssemblyName, Decision.NewAssemblyName);
        // In copy mode we shall be able to override assembly name if new project name and different from current project name.
        canSyncAssDn = hasNewProjectName && hasNewAssemblyName && !equalAssAndProjName;
        // In copy mode we shall be able to override project name if new project name, different from current project name
        canSyncAssUp = canSyncAssDn;
      } else {
        // In move mode we shall be able to override assembly name if new project name and different from current project name.
        bool hasNewAssemblyName = !string.Equals (_context.CurrentAssemblyName, Decision.NewAssemblyName);
        canSyncAssDn = hasNewProjectName && hasNewAssemblyName && !equalAssAndProjName;
        // In move mode we shall be able to override project name if we have an assembly name
        string? assName = Decision.NewAssemblyName ?? _context.CurrentAssemblyName;
        canSyncAssUp = assName is not null && !equalAssAndProjName;
      }

      btnAssDn.Enabled = canSyncAssDn;
      btnAssUp.Enabled = canSyncAssUp;
    }


    private void checkedItemsChanged (object? sender, EventArgs e) {
      if (sender is not PathCheckListUC pcl)
        return;

      Decision = pcl switch {
        var _ when pcl == pathCheckListProj =>
            Decision with { SelectedDependentProjectRoots = pcl.CheckedPaths },

        var _ when pcl == pathCheckListSln =>
            Decision with { SelectedSolutions = pcl.CheckedPaths },

        _ => Decision
      };

      enable ();
    }

    private void btnFolder_Click (object sender, EventArgs e) {
      string initialDirectory = Decision.NewProjectFolder ?? _context.DesignatedNewProjectFolder;
      bool isRelPath = !Path.IsPathRooted (initialDirectory);
      if (isRelPath)
        initialDirectory = Path.Combine (_context.RootFolder, initialDirectory);

      // Walk up the path until we find an existing folder
      string? pathToUse = initialDirectory;
      while (!string.IsNullOrEmpty (pathToUse) && !Directory.Exists (pathToUse)) {
        pathToUse = Path.GetDirectoryName (pathToUse);
      }

      // If nothing exists at all, default to MyDocuments
      if (string.IsNullOrEmpty (pathToUse))
        pathToUse = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);

      var dlg = new FolderBrowserDialog {
        Description = "New project folder",
        UseDescriptionForTitle = true,
        InitialDirectory = pathToUse,
        ShowNewFolderButton = true
      };
      if (dlg.ShowDialog () != DialogResult.OK)
        return;

      if (isRelPath) {
        // Make the selected path relative to root folder if initial path was relative
        string relPath = Path.GetRelativePath (_context.RootFolder, dlg.SelectedPath);
        txtBxNewProjFolder.Text = relPath;
      } else
        txtBxNewProjFolder.Text = dlg.SelectedPath;
      
    }
  }
}
