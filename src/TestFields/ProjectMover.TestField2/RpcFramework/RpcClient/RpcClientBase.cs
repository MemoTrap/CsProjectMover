namespace RpcClient {
  public class RpcClientBase {
    public static ValueTask<TOut?> InvokeAsync<TContract, TIn, TOut> (string method, TIn arg) {
      return ValueTask.FromResult (default(TOut));
    }  
  }
}
