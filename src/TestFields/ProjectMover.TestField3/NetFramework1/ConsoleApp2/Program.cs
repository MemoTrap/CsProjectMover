using System;

using ClassLibrary1;

using ClassLibrary2;

using ClassLibrary3;

namespace ConsoleApp2 {
  internal class Program {
    static void Main (string[] args) {
      Class1 c1 = new Class1 ();
      Class2 c2 = new Class2 ();
      Class3 c3 = new Class3 ();      
      
      Console.WriteLine ($"{c1.GetType ().Name} : {c1}");
      Console.WriteLine ($"{c2.GetType ().Name} : {c2}");
      Console.WriteLine ($"{c3.GetType ().Name} : {c3}");
    }
  }
}
