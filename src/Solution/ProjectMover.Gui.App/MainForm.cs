using System.ComponentModel;

using core.audiamus.util;

namespace ProjectMover.Gui.App {

  public partial class MainForm : Form {
    [Flags]
    enum EOpenFlags {
      None = 0,
      Dir = 1,
      Root = 2,
      Create = 4,
    }


    private readonly SystemMenu _systemMenu;
    private bool _updateAvailableFlag;


    private bool _hasRun;
    private bool _fInit;
    private Parameters _origParameters;
    private AppSettings AppSettings { get; }
    private UserSettings UserSettings { get; }
    private AppParameters Parameters {
      get {
        if (UserSettings.Parameters is null)
          UserSettings.Parameters = new AppParameters ();
        return UserSettings.Parameters;
      }
    }

    private CancellationTokenSource CancellationTokenSource { get; set; } = new ();

    private Progress Progress { get; }
    private Callback Callback { get; }
    private ProjectDecisionProvider DecisionProvider { get; }
    private CsProjectMover ProjectMover { get; }

    public MainForm () {
      InitializeComponent ();
      Text = ApplEnv.ApplName;
      if (CsProjectMover.DryRun)
        Text += " - Dry Run";

      _systemMenu = new SystemMenu (this);
      _systemMenu.AddCommand ($"About {ApplEnv.ApplName}", onSysMenuAbout, true);
      _systemMenu.AddCommand ($"Help\tF1", onSysMenuHelp, false);


      Progress = new Progress (labelProg1, labelProg2, progressBar);
      Callback = new Callback (this);
      DecisionProvider = new ProjectDecisionProvider (this, () => CancellationTokenSource);

      ProjectMover = new CsProjectMover (Progress, Callback, DecisionProvider);

      AppSettings = SettingsManager.GetAppSettings<AppSettings> ();
      UserSettings = SettingsManager.GetUserSettings<UserSettings> ();
      _origParameters = (Parameters)Parameters;

      init ();
    }

    protected override void OnShown (EventArgs e) {
      base.OnShown (e);

      if (!UserSettings.NoDisclaimer) {
        using ResourceGuard rg = new (x => Enabled = !x);
        DisclaimerForm disclaimer = new ();
        if (disclaimer.ShowDialog (this) == DialogResult.OK) {
          UserSettings.NoDisclaimer = disclaimer.DontShowAgain;
          UserSettings.Save ();
        }
      }

      checkOnlineUpdate ();

    }

    protected override void OnClosing (CancelEventArgs e) {
      using var _ = new LogGuard (3, this);

      base.OnClosing (e);

      if (_updateAvailableFlag) {
        _updateAvailableFlag = false;
        e.Cancel = true;
        handleDeferredUpdateAsync ();
      }

    }

    protected override void WndProc (ref Message m) {
      base.WndProc (ref m);
      _systemMenu.HandleMessage (ref m);
    }


    protected override void OnKeyDown (KeyEventArgs e) {
      if (e.Modifiers == Keys.Control) {
        //if (e.KeyCode == Keys.A) {
        //  e.SuppressKeyPress = true;
        //  selectAll ();
        //} else
        base.OnKeyDown (e);
      } else {
        if (e.KeyCode == Keys.F1) {
          e.SuppressKeyPress = true;
          onSysMenuHelp ();
        } else
          base.OnKeyDown (e);
      }
    }

    private void onSysMenuAbout () => new AboutForm () { Owner = this }.ShowDialog ();
    private void onSysMenuHelp () {
      var original = new Uri (newOnlineUpdate ().Uri);

      var builder = new UriBuilder (original);
      var segments = builder.Path.Split ('/');
      segments[^1] = "UserGuide.md";
      builder.Path = string.Join ("/", segments);

      var result = rawToBlob (builder.Uri);
      ShellExecute.Url (result);


      static Uri rawToBlob (Uri rawUri, string branch = "main") {
        // raw.githubusercontent.com/{owner}/{repo}/{branch}/{path}
        var segments = rawUri.AbsolutePath.Split ('/', StringSplitOptions.RemoveEmptyEntries);

        var owner = segments[0];
        var repo = segments[1];

        // everything after {branch}
        var path = string.Join ("/", segments.Skip (3));

        return new Uri ($"https://github.com/{owner}/{repo}/blob/{branch}/{path}");
      }
    }


    private void rbProjSol_CheckedChanged (object sender, EventArgs e) {
      if (_fInit)
        return;

      bool multiProjOld = Parameters.MultiMode.HasFlag (EMultiMode.Projects);
      bool multiSlnOld = Parameters.MultiMode.HasFlag (EMultiMode.Solutions);
      switch (sender) {
        case RadioButton { Checked: true } rb when rb == rbSglProjSglSol:
          Parameters.MultiMode = default;
          break;
        case RadioButton { Checked: true } rb when rb == rbSglProjMulSol:
          Parameters.MultiMode = EMultiMode.Solutions;
          break;
        case RadioButton { Checked: true } rb when rb == rbMulProjMulSol:
          Parameters.MultiMode = EMultiMode.Projects | EMultiMode.Solutions;
          break;
      }

      bool multiProjNew = Parameters.MultiMode.HasFlag (EMultiMode.Projects);
      bool multiSlnNew = Parameters.MultiMode.HasFlag (EMultiMode.Solutions);

      if (multiProjOld != multiProjNew) {
        Parameters.ProjectFolderOrFile = null;
        txtBoxProj.Text = string.Empty;
      }
      if (multiSlnOld != multiSlnNew) {
        Parameters.SolutionFolderOrFile = null;
        txtBoxSln.Text = string.Empty;
      }

      enable ();
    }

    private void rbMoveCopy_CheckedChanged (object sender, EventArgs e) {
      if (_fInit)
        return;

      Parameters.Copy = rbCopy.Checked;
      enable ();
    }

    private void rbAbs_CheckedChanged (object sender, EventArgs e) {
      if (_fInit)
        return;

      Parameters.AbsPathsInUserCommunication = rbAbs.Checked;

      Parameters.ProjectFolderOrFile = updateParaPath (txtBoxProj, Parameters.ProjectFolderOrFile);
      Parameters.DestinationFolder = updateParaPath (txtBoxDest, Parameters.DestinationFolder);
      Parameters.SolutionFolderOrFile = updateParaPath (txtBoxSln, Parameters.SolutionFolderOrFile);

      enable ();
    }


    private void ckBoxSVN_CheckedChanged (object sender, EventArgs e) {
      if (_fInit)
        return;

      Parameters.FileOperations = ckBoxSVN.Checked ? EFileOperations.Svn : EFileOperations.Direct;
      enable ();
    }

    private void ckBoxRescan_CheckedChanged (object sender, EventArgs e) {
      if (_fInit)
        return;

      Parameters.Rescan = ckBoxSVN.Checked;
      enable ();
    }

    private void updnDepth_ValueChanged (object sender, EventArgs e) {
      if (_fInit)
        return;

      Parameters.ProjectRootRecursionDepth = (int)updnDepth.Value;
      enable ();
    }


    private void btnOpenFile_Click (object sender, EventArgs e) {
      switch (sender) {
        case Button btn when btn == btnRoot: {
            string? rootFolder = Parameters.RootFolder;
            Parameters.RootFolder = openFileOrDir (
              labelRoot.Text,
              Parameters.RootFolder,
              EOpenFlags.Dir | EOpenFlags.Root,
              null,
              txtBoxRoot
            )
            ?? Parameters.RootFolder;
            checkDependentPaths (rootFolder);
          }
          break;

        case Button btn when btn == btnProj: {
            EOpenFlags flags = default;
            bool isDir = Parameters.MultiMode.HasFlag (EMultiMode.Projects);
            if (isDir)
              flags |= EOpenFlags.Dir;

            string? projDir = !isDir ?
              (Parameters.ProjectFolderOrFile != null ?
                  Path.GetDirectoryName (Parameters.ProjectFolderOrFile) : Parameters.RootFolder) :
               Parameters.RootFolder;

            Parameters.ProjectFolderOrFile = openFileOrDir (
              labelProj.Text,
              projDir, flags,
              "Project files (*.csproj, *.shproj)|*.csproj; *.shproj",
              txtBoxProj
            )
            ?? Parameters.ProjectFolderOrFile;
          }
          break;

        case Button btn when btn == btnDest:
          Parameters.DestinationFolder = openFileOrDir (
            labelDest.Text,
            Parameters.DestinationFolder ?? Parameters.RootFolder,
            EOpenFlags.Dir | EOpenFlags.Create,
            null,
            txtBoxDest
          )
          ?? Parameters.DestinationFolder;
          break;

        case Button btn when btn == btnSln: {
            EOpenFlags flags = default;
            bool isDir = Parameters.MultiMode.HasFlag (EMultiMode.Solutions);
            if (isDir)
              flags |= EOpenFlags.Dir;
            string? slnDir = !isDir ?
              (Parameters.SolutionFolderOrFile != null ?
                  Path.GetDirectoryName (Parameters.SolutionFolderOrFile) : Parameters.RootFolder) :
               Parameters.RootFolder;

            Parameters.SolutionFolderOrFile = openFileOrDir (
              labelSln.Text,
              slnDir,
              flags,
              "Solution files (*.sln)|*.sln",
              txtBoxSln
            )
            ?? Parameters.SolutionFolderOrFile;
          }
          break;
      }
      enable ();
    }


    private async void btnRun_Click (object sender, EventArgs e) {
      UserSettings.Save ();
      _origParameters = (Parameters)Parameters;
      btnReset.Enabled = false;

      CancellationTokenSource = new ();

      using var rg = new ResourceGuard (x => {
        panelParam.Enabled = !x;
        btnRun.Enabled = !x;
        btnAbort.Enabled = x;
        btnExit.Enabled = !x;
      });

      Log (3, this, Parameters.ToString);

      bool completed = await ProjectMover.RunAsync (Parameters, CancellationTokenSource.Token);

      if (completed)
        MsgBox.Show (this, $"{(CsProjectMover.DryRun ? "Dry run" : "Run")} completed", Text);
      Progress.Reset ();
      _hasRun = true;
      enable ();
    }

    private void btnAbort_Click (object sender, EventArgs e) {
      btnAbort.Enabled = false;
      CancellationTokenSource.Cancel ();
    }

    private void btnReset_Click (object sender, EventArgs e) {
      Parameters.From (_origParameters);
      init ();
    }

    private void btnExit_Click (object sender, EventArgs e) {
      UserSettings.Save ();
      Close ();
    }

    private string? openFileOrDir (
      string caption,
      string? rootDir,
      EOpenFlags flags,
      string? extFilter,
      TextBox tb
    ) {
      string? path;
      bool isDir = flags.HasFlag (EOpenFlags.Dir);
      bool isRootDir = flags.HasFlag (EOpenFlags.Root);
      bool create = flags.HasFlag (EOpenFlags.Create);

      if (isDir) {
        path = openDir (caption, rootDir, create);
      } else {
        path = openFile (caption, rootDir, extFilter);
      }

      if (!isRootDir)
        path = path?.ToParaPath (Parameters);

      tb.SetTextAsPathWithEllipsis (path);
      return path;
    }

    private static string? openFile (string caption, string? rootDir, string? extFilter) {
      string initialDirectory = rootDir ?? Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
      var dlg = new OpenFileDialog {
        Title = $"Select {caption}",
        InitialDirectory = initialDirectory,
        CheckFileExists = true,
        CheckPathExists = true,
        DereferenceLinks = true,
        Multiselect = false,
        Filter = extFilter, // "All Files (*.*)|*.*"       
      };
      if (dlg.ShowDialog () == DialogResult.OK) {
        return dlg.FileName;
      }
      return null;
    }

    private static string? openDir (string caption, string? rootDir, bool create) {
      string initialDirectory = rootDir ?? Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
      var dlg = new FolderBrowserDialog {
        Description = $"Select {caption}",
        UseDescriptionForTitle = true,
        InitialDirectory = initialDirectory,
        ShowNewFolderButton = create
      };
      if (dlg.ShowDialog () == DialogResult.OK) {
        return dlg.SelectedPath;
      }
      return null;
    }

    private void checkDependentPaths (string? prevRootFolder) {
      if (Parameters.RootFolder is null || !Directory.Exists (Parameters.RootFolder)) {
        Parameters.ProjectFolderOrFile = null;
        Parameters.DestinationFolder = null;
        Parameters.SolutionFolderOrFile = null;
      } else {
        Parameters.ProjectFolderOrFile = updateSubPath (Parameters.ProjectFolderOrFile);
        Parameters.DestinationFolder = updateSubPath (Parameters.DestinationFolder);
        Parameters.SolutionFolderOrFile = updateSubPath (Parameters.SolutionFolderOrFile);
      }

      Parameters.ProjectFolderOrFile = updateParaPath (txtBoxProj, Parameters.ProjectFolderOrFile);
      Parameters.DestinationFolder = updateParaPath (txtBoxDest, Parameters.DestinationFolder);
      Parameters.SolutionFolderOrFile = updateParaPath (txtBoxSln, Parameters.SolutionFolderOrFile);

      string? updateSubPath (string? path) {
        string? absPath = path?.ToAbsolutePath (prevRootFolder!);
        if (absPath is not null && !absPath.IsSubPathOf (Parameters.RootFolder!))
          return null;
        return absPath?.ToParaPath (Parameters);
      }
    }

    private void enable () {
      if (Parameters.Copy) {
        toolTip.SetToolTip (rbSglProjSglSol, "Select a single project for copy " +
          "and a single dependent solution.");
        toolTip.SetToolTip (txtBoxDest, "Destination root folder for copied projects.");
      } else {
        toolTip.SetToolTip (rbSglProjSglSol, "Select a single project for move/rename " +
          "and a single dependent solution." + Environment.NewLine +
          "NOTE: This may break other dependent solutions and their projects.");
        toolTip.SetToolTip (txtBoxDest, "Destination root folder for moved/renamed projects.");
      }
      bool multiProjects = Parameters.MultiMode.HasFlag (EMultiMode.Projects);
      bool multiSolutions = Parameters.MultiMode.HasFlag (EMultiMode.Solutions);

      labelProj.Text = multiProjects ?
        "Root folder for projects" : "Project";
      if (multiProjects) {
        toolTip.SetToolTip (txtBoxProj, "The root folder for candidate projects.");
        toolTip.SetToolTip (btnProj, "Select the root folder for candidate projects.");
      } else {
        toolTip.SetToolTip (txtBoxProj, "The single candidate project.");
        toolTip.SetToolTip (btnProj, "Select the candidate project.");
      }
      labelDepth.Enabled = multiProjects;
      updnDepth.Enabled = multiProjects;

      labelSln.Text = multiSolutions ?
        "Root folder for solutions" : "Solution";
      if (multiProjects) {
        toolTip.SetToolTip (txtBoxSln, "The root folder for dependent solutions to be considered, " +
          "including the dependent projects within.");
        toolTip.SetToolTip (btnSln, "Select the root folder for dependent solutions.");
      } else {
        toolTip.SetToolTip (txtBoxSln, "The single dependent solution to be considered.");
        toolTip.SetToolTip (btnSln, "Select the single dependent solution.");
      }
      labelRescan.Enabled = multiSolutions && _hasRun;
      ckBoxRescan.Enabled = multiSolutions && _hasRun;

      bool hasRootFolder = Parameters.RootFolder is not null;

      panelDest.Enabled = hasRootFolder;
      panelProject.Enabled = hasRootFolder;
      panelSolution.Enabled = hasRootFolder;

      btnReset.Enabled = _origParameters != (Parameters)Parameters;


      enableRun ();
    }

    private string? updateParaPath (Control tb, string? path) {
      if (Parameters.RootFolder is not null)
        path = path?.ToParaPath (Parameters);
      if (path is null)
        tb.SetTextAsPathWithEllipsis (string.Empty);
      else
        tb.SetTextAsPathWithEllipsis (path);
      return path;
    }

    private void init () {
      using var rg = new ResourceGuard (x => _fInit = x);

      switch (Parameters.MultiMode) {
        case EMultiMode.Projects:
          rbMulProjMulSol.Checked = true;
          break;
        case EMultiMode.Solutions | EMultiMode.Projects:
          rbMulProjMulSol.Checked = true;
          break;
        default:
          rbSglProjSglSol.Checked = true;
          break;
      }

      switch (Parameters.Copy) {
        case true:
          rbCopy.Checked = true;
          break;
        default:
          rbMove.Checked = true;
          break;
      }

      switch (Parameters.AbsPathsInUserCommunication) {
        case true:
          rbAbs.Checked = true;
          break;
        default:
          rbRel.Checked = true;
          break;
      }

      updnDepth.Value = Parameters.ProjectRootRecursionDepth;

      txtBoxRoot.SetTextAsPathWithEllipsis (Parameters.RootFolder);
      txtBoxProj.SetTextAsPathWithEllipsis (Parameters.ProjectFolderOrFile);
      txtBoxDest.SetTextAsPathWithEllipsis (Parameters.DestinationFolder);
      txtBoxSln.SetTextAsPathWithEllipsis (Parameters.SolutionFolderOrFile);

      ckBoxSVN.Checked = Parameters.FileOperations == EFileOperations.Svn;
      ckBoxRescan.Checked = Parameters.Rescan;

      enable ();
    }


    private void enableRun () {
      bool enable = !string.IsNullOrWhiteSpace (Parameters.RootFolder) &&
        !string.IsNullOrWhiteSpace (Parameters.DestinationFolder) &&
        !string.IsNullOrWhiteSpace (Parameters.ProjectFolderOrFile);

      if (!Parameters.MultiMode.HasFlag (EMultiMode.Solutions)) {
        enable &= !string.IsNullOrWhiteSpace (Parameters.SolutionFolderOrFile);
      }

      btnRun.Enabled = enable;
    }

    private OnlineUpdate newOnlineUpdate () =>
      new(
        UserSettings.UpdateSettings, 
        ApplEnv.AssemblyCompany, 
        "CsProjectMover", 
        null, 
        AppSettings.DbgOnlineUpdate
      );

    private async void checkOnlineUpdate () {
      var update = newOnlineUpdate ();

      await update.UpdateAsync (Callback, Application.Exit, isBusyForUpdate);
    }

    private async void handleDeferredUpdateAsync () {
      var update = newOnlineUpdate ();

      await update.InstallAsync (Callback, Application.Exit);
    }

    private bool isBusyForUpdate () {
      bool busy = !btnExit.Enabled;
      if (busy)
        _updateAvailableFlag = true;
      return busy;
    }

  }
}
