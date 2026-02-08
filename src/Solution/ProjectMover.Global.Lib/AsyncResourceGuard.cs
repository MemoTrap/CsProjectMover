namespace ProjectMover.Global.Lib {

  public sealed class AsyncResourceGuard : IAsyncDisposable {

    private readonly Func<ValueTask>? _disposeAsync;
    private bool _disposed;

    /// <summary>
    /// Ctor 1:
    /// Single async action called on DisposeAsync().
    /// </summary>
    public AsyncResourceGuard (Func<ValueTask> onDisposeAsync) {
      _disposeAsync = onDisposeAsync ?? throw new ArgumentNullException (nameof (onDisposeAsync));
    }

    /// <summary>
    /// Ctor 2:
    /// One async action for allocation, one for de-allocation.
    /// Allocation is executed immediately.
    /// </summary>
    /// <remarks>Important note:
    /// Because constructors cannot be async, the allocation step is started synchronously 
    /// and awaited via.AsTask() fire-and-forget.That’s usually fine for:
    ///   progress scopes,
    ///   logging scopes,
    ///   SVN revert guards
    /// </remarks>
    public AsyncResourceGuard (Func<ValueTask> onAllocateAsync, Func<ValueTask> onDisposeAsync) {
      ArgumentNullException.ThrowIfNull (onAllocateAsync);
      ArgumentNullException.ThrowIfNull (onDisposeAsync);

      _ = onAllocateAsync ().AsTask (); // fire & await synchronously below
      _disposeAsync = onDisposeAsync;
    }

    /// <summary>
    /// Asynchronously creates an AsyncResourceGuard by invoking the specified allocation delegate.
    /// </summary>
    /// <param name="onAllocateAsync">A delegate that performs asynchronous resource allocation.</param>
    /// <param name="onDisposeAsync">A delegate that performs asynchronous resource cleanup.</param>
    /// <returns>A task that represents the asynchronous operation, containing the created AsyncResourceGuard.</returns>
    /// <example>
    /// <code>
    /// await using var guard = await AsyncResourceGuard.CreateAsync(
    ///     allocAsync: () => OnAllocateAsync(),
    ///     freeAsync:  () => OnDisposeAsync());
    ///
    /// await DoWorkAsync();
    /// </code>
    /// </example>
    public static async Task<AsyncResourceGuard> CreateAsync (
        Func<ValueTask> onAllocateAsync,
        Func<ValueTask> onDisposeAsync
    ) {
      await onAllocateAsync ().ConfigureAwait (false);
      return new AsyncResourceGuard (onDisposeAsync);
    }


    /// <summary>
    /// Ctor 3:
    /// Single async action with bool argument:
    /// false = allocate
    /// true  = de-allocate
    /// Allocation is executed immediately.
    /// </summary>
    /// <remarks>Important note:
    /// Because constructors cannot be async, the allocation step is started synchronously 
    /// and awaited via.AsTask() fire-and-forget.That’s usually fine for:
    ///   progress scopes,
    ///   logging scopes,
    ///   SVN revert guards
    /// </remarks>
    public AsyncResourceGuard (Func<bool, ValueTask> allocateOrDisposeAsync) {
      ArgumentNullException.ThrowIfNull (allocateOrDisposeAsync);

      _ = allocateOrDisposeAsync (false).AsTask ();
      _disposeAsync = () => allocateOrDisposeAsync (true);
    }

    public async ValueTask DisposeAsync () {
      if (_disposed)
        return;

      _disposed = true;

      if (_disposeAsync is not null)
        await _disposeAsync ().ConfigureAwait (false);
    }
  }
}
