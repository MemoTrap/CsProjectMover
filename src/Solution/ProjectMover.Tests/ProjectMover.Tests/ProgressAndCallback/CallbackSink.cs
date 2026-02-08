#pragma warning disable IDE0130

using ProjectMover.Global.Lib;

namespace ProjectMover.Tests {
  internal class SimpleCallbackSink : ICallbackSink {
    public virtual EDialogResult ShowMessage (
      ECallback callbackType,
      string message,
      string? title = null,
      EMsgBtns buttons = default,
      EDefaultMsgBtn defaultButton = default,
      bool shortenLongLines = false
    ) {

      return callbackType switch {
        ECallback.Error => EDialogResult.OK,
        ECallback.Warning => EDialogResult.OK,
        ECallback.Information => EDialogResult.OK,
        ECallback.Question => question (),
        _ => EDialogResult.None
      };

      EDialogResult question () {
        bool yes = true;

        return buttons switch {
          EMsgBtns.OKCancel => yes ? EDialogResult.OK : EDialogResult.Cancel,
          EMsgBtns.YesNoCancel => yes ? EDialogResult.Yes : EDialogResult.No,
          EMsgBtns.YesNo => yes ? EDialogResult.Yes : EDialogResult.No,
          EMsgBtns.AbortRetryIgnore => yes ? EDialogResult.Retry : EDialogResult.Abort,
          _ => EDialogResult.None
        };
      }
    }
  }

  internal class CapturingCallbackSink : SimpleCallbackSink {
    public List<string> Messages { get; } = [];

    public override EDialogResult ShowMessage (
      ECallback callbackType,
      string message,
      string? title,
      EMsgBtns buttons,
      EDefaultMsgBtn defaultButton,
      bool shortenLongLines
    ) {     
      Messages.Add ($"{callbackType.ToString().ToUpper()}: {message}");

      return base.ShowMessage (callbackType, message, title, buttons, defaultButton);
    }
  }
}
