namespace ProjectMover.Lib.Api {


  public interface IProgressSink {
    void BeginStep (string title);
    void Report (string message);
    void EndStep (string finalMessage);
    void SetMax (int max);
    void ReportAbs (int total);
    void ReportRel (int increment = 1);
  }



}
