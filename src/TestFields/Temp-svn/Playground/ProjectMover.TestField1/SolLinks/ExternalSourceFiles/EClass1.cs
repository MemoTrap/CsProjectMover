using ALibCore;

namespace ELibWithLinks {
  public class EClass1 {
    public AClass1 A1 { get; } = new ();

    public override string ToString () => $"EClass1 with AClass1: {A1}";
  }
}
