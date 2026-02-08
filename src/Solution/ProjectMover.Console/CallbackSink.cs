using ProjectMover.Global.Lib;

namespace ProjectMover.ConsApp {
  internal class ConsoleCallbackSink : ICallbackSink {
    public EDialogResult ShowMessage (
      ECallback callbackType,
      string message,
      string? title = null,
      EMsgBtns buttons = EMsgBtns.OK,
      EDefaultMsgBtn defaultButton = EDefaultMsgBtn.Button1,
      bool shortenLongLines = false
    ) {

      Console.Write ($"{callbackType.ToString().ToUpper()}: {message}");

      return callbackType switch {
        ECallback.Error => question(),
        ECallback.Warning => question(),
        ECallback.Information => question (),
        ECallback.Question => question(),
        _ => EDialogResult.None
      };

      EDialogResult question () {
        if (buttons == EMsgBtns.OK) {
          Console.WriteLine ();
          return EDialogResult.OK;
        }

        Console.Write (" [y/N]: ");
        var reply = Console.ReadLine ();
        bool yes = string.Equals (reply, "y", StringComparison.OrdinalIgnoreCase);

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
}
