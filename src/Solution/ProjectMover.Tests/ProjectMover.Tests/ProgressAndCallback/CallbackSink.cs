#pragma warning disable IDE0130

using ProjectMover.Global.Lib;

namespace ProjectMover.Tests {
  internal class StandardCallbackSink : ICallbackSink {

    public Dictionary<ECallback, Func<string?, string, EDialogResult?>> CustomReplies { get; } = [];
      
    public virtual EDialogResult ShowMessage (
      ECallback callbackType,
      string message,
      string? title = null,
      EMsgBtns buttons = default,
      EDefaultMsgBtn defaultButton = default,
      bool shortenLongLines = false
    ) {
      var func = CustomReplies.GetValueOrDefault (callbackType);
      EDialogResult? customReply = null;

      if (func is not null)
        customReply = func (title, message);

      return callbackType switch {
        ECallback.Error => customReply ?? EDialogResult.OK,
        ECallback.Warning => customReply ?? EDialogResult.OK,
        ECallback.Information => customReply ?? EDialogResult.OK,
        ECallback.Question => customReply ?? question (),
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

  internal class CapturingCallbackSink : StandardCallbackSink {
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
