
using CommonBcl;

using CommonContract;
using RpcClient;

namespace ClientProxy {
  public class MyProxy : IMyContract {
    public ValueTask<bool> SendAsync (Dto1 dto) {
      return RpcClientBase.InvokeAsync<IMyContract, Dto1, bool> (nameof (IMyContract.SendAsync), dto);
    }
  }
}
