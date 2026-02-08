using ALibCore;
using BLibUtil;
using DLibPlugin;
using ELibWithLinks;
using ZLibStd;

namespace GMixedApp {
  internal class Program {
    static void Main (string[] args) {
      AClass1 a1 = new ();
      BClass1 b1 = new ();
      DClass1 d1 = new ();
      EClass1 e1 = new ();
      ZClass1 z1 = new ();
      
      Console.WriteLine ($"{a1.GetType ().Name} : {a1}");
      Console.WriteLine ($"{b1.GetType ().Name} : {b1}");
      Console.WriteLine ($"{d1.GetType ().Name} : {d1}");
      Console.WriteLine ($"{e1.GetType ().Name} : {e1}");
      Console.WriteLine ($"{z1.GetType ().Name} : {z1}");
    }
  }
}
