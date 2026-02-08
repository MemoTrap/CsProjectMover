namespace ProjectMover.Global.Lib {
  public enum ECallback {
    None = 0,
    Error = 16,
    Question = 32,
    Warning = 48,
    Information = 64,
  }

  public enum EMsgBtns {
    OK = 0,
    OKCancel = 1,
    AbortRetryIgnore = 2,
    YesNoCancel = 3,
    YesNo = 4,
    RetryCancel = 5,
    CancelTryContinue = 6
  }

  public enum EDefaultMsgBtn {
    Button1 = 0,
    Button2 = 256,
    Button3 = 512,
    Button4 = 768,
  }

  public enum EDialogResult {
    None = 0,
    OK = 1,
    Cancel = 2,
    Abort = 3,
    Retry = 4,
    Ignore = 5,
    Yes = 6,
    No = 7,
    TryAgain = 10,
    Continue = 11,
  }

  public interface ICallbackSink {
    EDialogResult ShowMessage (
      ECallback callbackType,
      string message,
      string? title = null,
      EMsgBtns buttons = EMsgBtns.OK,
      EDefaultMsgBtn defaultButton = EDefaultMsgBtn.Button1,
      bool shortenLongLines = false
    );

    EDialogResult ShowMessage (
      ECallback callbackType,
      string message,
      string? title,
      bool shortenLongLines
    ) => ShowMessage (
        callbackType, 
        message, 
        title, 
        EMsgBtns.OK, 
        EDefaultMsgBtn.Button1, 
        shortenLongLines
      );
    EDialogResult ShowMessage (
      ECallback callbackType,
      string message,
      bool shortenLongLines
    ) => ShowMessage (
        callbackType, 
        message, 
        null, 
        EMsgBtns.OK, 
        EDefaultMsgBtn.Button1, 
        shortenLongLines
      );
  }

}
