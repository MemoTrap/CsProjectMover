using ZLibStd;

namespace ALibCore {
  public class AClass1 {
    public ZClass1 Z1 { get; } = new ();

    public override string ToString() => $"AClass1 with ZClass1: {Z1}";
  }
}
