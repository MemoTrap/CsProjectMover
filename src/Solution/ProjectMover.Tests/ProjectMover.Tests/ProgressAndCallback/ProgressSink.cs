#pragma warning disable IDE0130

using ProjectMover.Lib.Api;

namespace ProjectMover.Tests {
  internal class NullProgressSink : IProgressSink {
    public virtual void BeginStep (string text) { }
    public virtual void EndStep (string text) { }
    public virtual void Report (string text) { }
    public virtual void ReportAbs (int total) { }
    public virtual void ReportRel (int increment = 1) {}
    public virtual void SetMax (int max) {}
  }

  internal class CapturingProgressSink : NullProgressSink {
    public List<string> Messages { get; } = [];

    public override void BeginStep (string text) => Messages.Add ($"BEGIN: {text}");
    public override void EndStep (string text) => Messages.Add ($"END: {text}");
    public override void Report (string text) => Messages.Add (text);
  }
}
