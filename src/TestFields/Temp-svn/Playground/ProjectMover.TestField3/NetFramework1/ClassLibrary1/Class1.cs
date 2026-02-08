using System;

namespace ClassLibrary1
{
  public class Class1 {
    public void Run () {
      Console.WriteLine ($"Hello from {this.GetType ().FullName}");
    }
  }
}
