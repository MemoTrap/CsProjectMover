using SSharedStuff;

namespace FSharedApp {
  internal class Program {
    static void Main (string[] args) {
      SPartialClass1 s1 = new ();
      Console.WriteLine ($"{s1.GetType ().Name} : {s1}");
      Console.WriteLine ($"{s1.GetType ().Name} : {s1.GetPartial()}");
    }
  }
}
