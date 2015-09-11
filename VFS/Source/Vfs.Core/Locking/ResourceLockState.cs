using System;

namespace Vfs.Locking
{
  /// <summary>
  /// Lock state flags, which indicate the level of accessiblity of a given
  /// resource.
  /// </summary>
  public enum ResourceLockState
  {
    /// <summary>
    /// The file is currently not accessed - both read
    /// and write locks can be acquired.
    /// </summary>
    Unlocked = 0,
    /// <summary>
    /// The file is currently been read, not write access
    /// is granted.
    /// </summary>
    ReadOnly = 1,
    /// <summary>
    /// The file is modified or deleted and thus locked
    /// exclusively.
    /// </summary>
    Locked = 2,
    /// <summary>
    /// A write lock request has been enqueued, no read locks
    /// are being granted until the lock has been granted
    /// and subsequently released.
    /// </summary>
    /// TODO remove
    [Obsolete("not used", true)]
    WriteLockPending = 3
  }
}