using System;

using ClassLibrary2;

using SharedProject1;

namespace ConsoleApp3 {
  internal class Program {
    static void Main (string[] args) {
      Class2 c2 = new Class2 ();
      ClassS cs = new ClassS ();

      Console.WriteLine ($"{c2.GetType ().Name} : {c2}");
      Console.WriteLine ($"{cs.GetType ().Name} : {cs}");
    }
  }
}
