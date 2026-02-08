using ALibCore;

namespace BLibUtil {
  public class BClass1 {
    public AClass1 A1 { get; } = new ();

    public override string ToString() => $"BClass1 with AClass1 containing ZClass1: {A1.Z1}";
  }
}
