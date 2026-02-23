using ProjectMover.Lib.Api;

namespace ProjectMover.ConsApp {
  internal class ConsoleProgressSink : IProgressSink {
    public void BeginStep (string title) => Console.WriteLine (title);
    public void EndStep (string finalMessage) => Console.WriteLine (finalMessage);
    public void Report (string message) => Console.WriteLine (message);
    public void SetMax (int max) { }
    public void ReportAbs (int total) { }
    public void ReportRel (int increment = 1) {}
  }
}
