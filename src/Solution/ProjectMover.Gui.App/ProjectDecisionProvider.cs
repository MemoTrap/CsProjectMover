namespace ProjectMover.Gui.App {
  internal class ProjectDecisionProvider (
    IWin32Window owner, 
    Func<CancellationTokenSource> getCancelTokenSource
  ) : IProjectDecisionProvider {

    private readonly AffineSynchronizationContext _syncCtx = new();

    public IEnumerable<string>? PreSelect (IEnumerable<string> projectPaths) {
      return _syncCtx.Send (() => {

        var dlg = new PresetForm (projectPaths, getCancelTokenSource) {
          StartPosition = FormStartPosition.CenterParent
        };
        var dlgResult = dlg.ShowDialog (owner);

        if (dlgResult == DialogResult.OK)
          return dlg.SelectedPaths;      

        return null;
      });

    }

    public ProjectUserDecision Decide (ProjectDecisionContext context) {
      return _syncCtx.Send (() => {
        var dlg = new DecisionForm (context, getCancelTokenSource) {
          StartPosition = FormStartPosition.CenterParent
        };
        dlg.ShowDialog (owner);
        return dlg.Decision;
      });
    }

  }
}
