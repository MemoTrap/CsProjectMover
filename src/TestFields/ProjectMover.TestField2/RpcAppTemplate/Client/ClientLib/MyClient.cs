using ClientProxy;

namespace ClientLib {
  public class MyClient {
    public async Task RunAsync () {
      MyProxy proxy = new ();
      await proxy.SendAsync (new CommonBcl.Dto1 ());
    }
  }
}
