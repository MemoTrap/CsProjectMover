using System.Runtime.CompilerServices;


namespace ProjectMover.Gui.App {
  internal class Progress (Label label1, Label label2, ProgressBar progBar): IProgressSink {
    private readonly AffineSynchronizationContext _syncCtx = new ();

    public void BeginStep (string title) {
      Log (4, this, () => title);
      _syncCtx.Post (() => {
        Log (3, this, () => title);
        setMax (0);
        logProgBar ();

        label1.Text = title;
        label2.Text = string.Empty;
      });
    }
    
    public void EndStep (string finalMessage) {
      Log (4, this, () => finalMessage);
      _syncCtx.Post (() => {
        Log (3, this, () => finalMessage);
        label1.Text = finalMessage;
      });
    }

    public void Report (string message) {
      Log (4, this, () => message);
      _syncCtx.Post (() => {
        message = message.ShortenColonDecoratedPathString (label2.Width, label2.Font);
        Log (3, this, () => message);
        label2.Text = message;
      });
    }
    
    public void ReportAbs (int total) {
      Log (4, this, total.ToString);
      _syncCtx.Post (() => {
        Log (3, this, total.ToString);
        progBar.Value = total;
      });
    }

    public void ReportRel (int increment = 1) {
      Log (4, this, increment.ToString);
      _syncCtx.Post (() => {
        Log (4, this, increment.ToString);
        progBar.Value += increment;
      });
    }

    public void SetMax (int max) {
      Log (4, this, max.ToString);
      _syncCtx.Post (() => { 
        Log (3, this, max.ToString);
        setMax (max); 
      });
    }

    public void Reset () {
      Log (3, this);
      setMax (0);
      label1.Text = string.Empty;
      label2.Text = string.Empty;
    }

    private void setMax (int max) {
      progBar.Style = max < 0 ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;
      progBar.Value = 0;
      progBar.Maximum = Math.Max(0, max);
      if (progBar.Style == ProgressBarStyle.Marquee) {
        progBar.Maximum = 100;
        progBar.MarqueeAnimationSpeed = 100;
      }
      logProgBar ();
    }

    private void logProgBar ([CallerMemberName] string? method = null) {
      Log (3, this, 
        () => $"progbar style={progBar.Style}, max={progBar.Maximum}, val={progBar.Value}, spd={progBar.MarqueeAnimationSpeed}",
        method);
    }
  }
}
