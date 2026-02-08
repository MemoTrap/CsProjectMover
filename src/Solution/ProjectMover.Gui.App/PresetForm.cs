namespace ProjectMover.Gui.App {
  public partial class PresetForm : Form {
    const string HDR = "Candidate projects found:";

    public IEnumerable<string>? SelectedPaths { get; private set; }

    private readonly Func<CancellationTokenSource> _getCancelTokenSource;

    public PresetForm (
      IEnumerable<string> projectPaths,
      Func<CancellationTokenSource> getCancelTokenSource
    ) {
      InitializeComponent ();
      _getCancelTokenSource = getCancelTokenSource;

      pathCheckList.ColumnHeader = HDR;
      pathCheckList.CheckedItemsChanged += checkedItemsChanged;
      pathCheckList.SetPaths (projectPaths);

      enable ();
    }



    private void btnOK_Click (object sender, EventArgs e) {
      SelectedPaths = pathCheckList.CheckedPaths;
      DialogResult = DialogResult.OK;
      Close ();
    }

    private void btnLater_Click (object sender, EventArgs e) {
      DialogResult = DialogResult.None;
      Close ();
    }

    private void btnCancel_Click (object sender, EventArgs e) {
      _getCancelTokenSource ().Cancel ();
      Close ();
    }

    private void checkedItemsChanged (object? sender, EventArgs e) => 
      enable ();


    private void enable () => 
      btnOK.Enabled = pathCheckList.CheckedPaths?.Count > 0;
  }
}
