using RpcServer;

using ServerBiz;

using ServerService;

namespace ServerHost {
  public class MyHost : RpcHost {
    public void Startup () {
      RegisterSingleton<MyBizClass>();
      RegisterService<MyService>();
    }
  }
}
