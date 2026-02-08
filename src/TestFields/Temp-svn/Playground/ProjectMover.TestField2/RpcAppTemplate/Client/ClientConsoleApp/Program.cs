using ClientLib;

namespace ClientConsoleApp {
  internal class Program {
    static async Task Main () {
      MyClient client = new ();
      await client.RunAsync ();
    }
  }
}
