using ClassLibrary1;

using SharedProject1;

namespace ClassLibrary3 {
  public class Class3 {
    public Class1 C1 { get; }
    public ClassS CS { get; }

    public Class3 () {
      C1 = new Class1 ();
      CS = new ClassS ();
    }

    public override string ToString () => $"Class3 with Class1: {C1}" +
      $" and ClassS : {CS}";

  }
}
