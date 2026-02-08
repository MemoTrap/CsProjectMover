using System;

using ClassLibrary1;

namespace ConsoleApp1 {
  internal class Program {
    static void Main (string[] args) {
      Class1 c1 = new Class1 ();
      Console.WriteLine ($"{c1.GetType ().Name} : {c1}");
    }
  }
}
