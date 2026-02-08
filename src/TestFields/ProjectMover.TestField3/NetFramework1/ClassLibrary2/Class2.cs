using ClassLibrary1;

namespace ClassLibrary2 {
  public class Class2 {
    public Class1 C1 { get; } 

    public Class2 () {
      C1 = new Class1 ();
    }

    public override string ToString () => $"Class2 with Class1: {C1}";
  }
}
