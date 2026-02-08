
using CommonBcl;

using CommonContract;

using RpcServer;

using ServerBiz;

namespace ServerService {
  public class MyService (MyBizClass biz): RpcServiceBase, IMyContract {

    private MyBizClass Biz { get; } = biz;

    ValueTask<bool> IMyContract.SendAsync (Dto1 dto) {
      bool succ = Biz.Send (dto);
      return ValueTask.FromResult (succ);
    }
  }
}
