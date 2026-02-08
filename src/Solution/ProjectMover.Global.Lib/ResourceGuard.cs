namespace ProjectMover.Global.Lib {
  public sealed class ResourceGuard : IDisposable {
    private readonly Action? _dispose;
    private bool _disposed;

    /// <summary>
    /// Ctor 1:
    /// Single action called on Dispose().
    /// </summary>
    public ResourceGuard (Action onDispose) {
      _dispose = onDispose ?? throw new ArgumentNullException (nameof (onDispose));
    }

    /// <summary>
    /// Ctor 2:
    /// One action for allocation, one for de-allocation.
    /// Allocation is executed immediately.
    /// </summary>
    public ResourceGuard (Action onAllocate, Action onDispose) {
      ArgumentNullException.ThrowIfNull (onAllocate);
      ArgumentNullException.ThrowIfNull (onDispose);

      onAllocate ();
      _dispose = onDispose;
    }

    /// <summary>
    /// Ctor 3:
    /// Single action with bool argument:
    /// true  = allocate
    /// false = de-allocate
    /// </summary>
    public ResourceGuard (Action<bool> allocateOrDispose) {
      ArgumentNullException.ThrowIfNull (allocateOrDispose);

      allocateOrDispose (true);
      _dispose = () => allocateOrDispose (false);
    }

    public void Dispose () {
      if (_disposed)
        return;

      _disposed = true;
      _dispose?.Invoke ();
    }
  }
}
