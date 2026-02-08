using CommonBcl;

using RpcCommon;

namespace CommonContract {
  public interface IMyContract : IContractBase {
    public ValueTask<bool> SendAsync (Dto1 dto);
  }
}
