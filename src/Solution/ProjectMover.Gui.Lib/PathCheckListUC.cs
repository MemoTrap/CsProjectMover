namespace ProjectMover.Gui.Lib {
  public partial class PathCheckListUC : UserControl {

    private readonly ColumnHeader _column;
    private bool _isBulkUpdate;

    public IReadOnlyList<string>? CheckedPaths =>
      listView.CheckedItems
        .Cast<ListViewItem> ()
        .Select (i => i.Tag as string)
        .Where (s => s != null)
        .Select (s => s!)
        .ToList () is { Count: > 0 } list ? list : null;

    public event EventHandler? CheckedItemsChanged;

    public string ColumnHeader {
      get => _column.Text;
      set => _column.Text = value;
    }

    public PathCheckListUC () {
      InitializeComponent ();

      _column = new ColumnHeader {
        Text = "Path",
        Width = -2 // auto-size
      };

      listView.Columns.Add (_column);

      listView.Activation = ItemActivation.OneClick;
      listView.HoverSelection = true;
      listView.ShowItemToolTips = true;


      var menu = new ContextMenuStrip ();
      menu.Items.Add ("Select All", null, (_, _) => CheckAll ());
      menu.Items.Add ("Select None", null, (_, _) => UncheckAll ());
      listView.ContextMenuStrip = menu;

    }

    public void SetPaths (IEnumerable<string> paths) {
      using var rg = new ResourceGuard (listView.BeginUpdate, listView.EndUpdate);

      listView.Items.Clear ();

      Enabled = paths.Any ();

      foreach (var path in paths) {
        var item = new ListViewItem (path) {
          Tag = path // store the full original path here
        };
        listView.Items.Add (item);
      }
      autoSizeSingleColumn ();
    }

    public void Clear () {
      listView.Items.Clear ();
    }

    public void CheckAll () {
      setAllChecked (true);
    }

    public void UncheckAll () {
      setAllChecked (false);
    }

    protected override void OnEnabledChanged (EventArgs e) {
      base.OnEnabledChanged (e);
      if (Enabled) {
        listView.BackColor = SystemColors.Window;
        listView.ForeColor = SystemColors.WindowText;
        listView.HeaderStyle = ColumnHeaderStyle.Clickable; // show header
      } else {
        listView.BackColor = SystemColors.Control;          // greyed out background
        listView.ForeColor = SystemColors.GrayText;         // greyed out text
        listView.HeaderStyle = ColumnHeaderStyle.None;      // hide header
      }
    }

    protected override void OnSizeChanged (EventArgs e) {
      base.OnSizeChanged (e);
      autoSizeSingleColumn();
    }

    private void setAllChecked (bool check) {
      using var rg = new ResourceGuard (
        () => {
          _isBulkUpdate = true;
          listView.BeginUpdate ();
        },
        () => {
          _isBulkUpdate = false;
          listView.EndUpdate ();
        }
      );

      foreach (ListViewItem item in listView.Items)
        item.Checked = check;

      CheckedItemsChanged?.Invoke (this, EventArgs.Empty); 
    }


    private void autoSizeSingleColumn () {
      if (listView.Columns.Count == 0)
        return;
      if (listView.Items.Count == 0)
        return;

      // Calculate available width
      int clientWidth = listView.ClientSize.Width;

      if (listView.Items.Count > listView.ClientSize.Height / listView.GetItemRect (0).Height)
        clientWidth -= SystemInformation.VerticalScrollBarWidth;

      // good for the column itself
      listView.Columns[0].Width = clientWidth;

      // subtract checkbox width for the actually allowed text width
      int maxTextWidth = clientWidth - (SystemInformation.SmallIconSize.Width + 6); // 6px padding around checkbox

      // shorten each item
      foreach (ListViewItem item in listView.Items) {
        if (item.Tag is not string fullPath)
          continue;

        string shortened = fullPath.ShortenPathString (maxTextWidth, listView.Font);
        item.Text = shortened;

        // Show tooltip only if shortening actually happened
        item.ToolTipText = shortened != fullPath
            ? fullPath
            : string.Empty;
      }
    }


    private void listView_ItemChecked (object sender, ItemCheckedEventArgs e) {
      if (_isBulkUpdate)
        return;

      CheckedItemsChanged?.Invoke (this, e);
    }

    private void listView_KeyDown (object sender, KeyEventArgs e) {
      if (e.Control && e.KeyCode == Keys.A) {
        if (e.Shift)
          UncheckAll ();
        else
          CheckAll ();
        e.Handled = true;
      }
    }
  }
}
