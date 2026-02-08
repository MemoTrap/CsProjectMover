using ALibCore;

namespace SSharedStuff {
  public partial class SPartialClass1 {
    public AClass1 A1 { get; } = new ();
    public partial string GetPartial ();  

    public override string ToString() => $"SPartialClass1 with {A1}";
  }
}
