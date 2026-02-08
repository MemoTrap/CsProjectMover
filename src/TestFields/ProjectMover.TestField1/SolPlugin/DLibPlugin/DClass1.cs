using ALibCore;

using SSharedStuff;

namespace DLibPlugin {
  public class DClass1 {
    public AClass1 A1 { get; } = new ();
    public SPartialClass1 S1 { get; } = new ();


    public override string ToString() => $"DClass1 with AClass1 containing ZClass1: {A1.Z1}" +
      $" and SClass : {S1.GetPartial()}";
  }
}
