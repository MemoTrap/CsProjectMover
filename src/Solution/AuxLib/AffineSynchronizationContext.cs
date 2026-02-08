// Modified file from project https://github.com/audiamus/BookLibConnect
// Original copyright (C) 2021 - 2023 by audiamus
// Originally licensed by audiamus under the GNU General Public License v3.0 (GPL-3.0)
// Slighly modernized and adapted to ProjectMover by MemoTrap
// Still licensed under GPL-3.0

// Modified version of original AffineSynchronizationContext by audiamus,
// now actually implementing a SynchronizationContext

#pragma warning disable IDE0130


namespace core.audiamus.aux {
  /// <summary>
  /// Wraps a <see cref="System.Threading.SynchronizationContext"/> with thread affinity check.
  /// Constructor determines synchronization context.
  /// Post and Send methods will invoke delegate directly if same synchronization context, 
  /// otherwise invoke via initial synchronization context.
  /// <para>Closest to former ISynchronizeInvoke.InvokeRequired</para>
  /// </summary>
  public class AffineSynchronizationContext (SynchronizationContext? other = null) :
    SynchronizationContext,
    IEquatable<AffineSynchronizationContext> {

    private readonly SynchronizationContext? _sync = other ?? Current;
    private readonly int _managedThreadId = Environment.CurrentManagedThreadId;

    /// <summary>
    /// Affinity check for current thread.
    /// Always true if no synchronization context is set.
    /// Otherwise true if current synchronization context is the ctor synchronization context and
    /// current thread ID is ctor ID.
    /// </summary>
    public bool Affine => _sync is null ||
      (_sync == Current &&
       _managedThreadId == Environment.CurrentManagedThreadId);

    public SynchronizationContext? SynchronizationContext => _sync;

    #region overrides

    public override void Post (SendOrPostCallback sendOrPostCallback, object? state) {
      if (Affine)
        sendOrPostCallback (state);
      else
        _sync?.Post (sendOrPostCallback, state);
    }

    public override void Send (SendOrPostCallback sendOrPostCallback, object? state) {
      if (Affine)
        sendOrPostCallback (state);
      else
        _sync?.Send (sendOrPostCallback, state);
    }

    public override int GetHashCode () {
      return _sync?.GetHashCode () ?? 0;
    }

    public override bool Equals (object? obj) {
      if (obj is AffineSynchronizationContext sync)
        return sync.Equals (this);
      else
        return false;
    }

    #endregion overrides
    #region IEquatable<AffineSynchronizationContext>

    public bool Equals (AffineSynchronizationContext? other) {
      return (Equals (this.SynchronizationContext, other?.SynchronizationContext));
    }

    #endregion IEquatable<AffineSynchronizationContext>
  }
}
