namespace ProjectMover.Gui.App {
  internal class Callback (Form owner) : ICallbackSink {

    private readonly AffineSynchronizationContext _syncCtx = new ();

    public EDialogResult ShowMessage (
      ECallback callbackType,
      string message,
      string? title = null,
      EMsgBtns buttons = EMsgBtns.OK,
      EDefaultMsgBtn defaultButton = EDefaultMsgBtn.Button1,
      bool shortenLongLines = false
    ) {
      var cbResult = _syncCtx.Send (() => {
        string msg = message;
        if (shortenLongLines)
          msg = message.ShortenMultiLineBulletDecoratedPathString ();

        var result = MsgBox.Show (
          owner, 
          msg, 
          title ?? ApplEnv.ApplName,
          (MessageBoxButtons)buttons,
          (MessageBoxIcon)callbackType,
          (MessageBoxDefaultButton)defaultButton
        );


        return (EDialogResult)result;
      });

      Log (3, this, () => $"'{callbackType}' with '{cbResult}' reply for:{Environment.NewLine}{message}");
      return cbResult;
    }
  }
}
