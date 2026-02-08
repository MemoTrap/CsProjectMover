using ALibCore;

using BLibUtil;

namespace CLibApp {
  internal class Program {
    static void Main (string[] args) {
      AClass1 a1 = new ();
      BClass1 b1 = new ();
      Console.WriteLine ($"{a1.GetType().Name} : {a1}");
      Console.WriteLine ($"{b1.GetType().Name} : {b1}");
    }
  }
}
