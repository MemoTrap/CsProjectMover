namespace RpcServer {
  public class RpcHost {
    public void RegisterSingleton<T> () { }
    public void RegisterService<T> () where T : RpcServiceBase { }

    public void Run () { 
      Thread.Sleep (TimeSpan.FromMinutes(5)); 
    }
  }
}
