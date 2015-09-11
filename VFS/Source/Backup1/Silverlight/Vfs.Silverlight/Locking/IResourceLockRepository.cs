using System;

namespace Vfs.Locking
{
  /// <summary>
  /// A repository that maintains <see cref="LockTracker"/>
  /// instances for actively locked resources.
  /// </summary>
  public interface IResourceLockRepository
  {
    /// <summary>
    /// Tries to acquire a shared read lock for a given resource
    /// which never expires.
    /// If this operation succeeds, the lock must be released as soon
    /// as possible through the <see cref="ReleaseReadLock"/>
    /// method.
    /// </summary>
    /// <returns>A <see cref="LockItem"/> instance which represents
    /// the acquired lock, if any. If no lock was granted, the
    /// returned item's <see cref="LockItem.LockType"/> property
    /// returns <see cref="ResourceLockType.Denied"/>.</returns>
    LockItem TryGetReadLock(string resourceId);

    /// <summary>
    /// Tries to acquire an exclusive write lock for the resource
    /// which expires after a given time span in order not to block
    /// the resource indefinitely.
    /// If this operation succeeds, the lock must be released as soon
    /// as possible through the <see cref="ReleaseReadLock"/>
    /// method.
    /// </summary>
    /// <param name="resourceId">Identifies the locked resource.</param>
    /// <param name="timeout">The specified expiration timeout from now.
    /// Allowed values are null for indefinite locking, or any positive value.</param>
    /// <returns>True if the lock was granted.</returns>
    /// <exception cref="ArgumentOutOfRangeException">In case of a negative
    /// timeout.</exception>
    LockItem TryGetReadLock(string resourceId, TimeSpan? timeout);

    /// <summary>
    /// Tries to acquire an exclusive write lock for a given resource
    /// which never expires.
    /// If this operation succeeds, the lock must be released as soon
    /// as possible through the <see cref="ReleaseWriteLock"/>
    /// method.
    /// </summary>
    /// <returns>A <see cref="LockItem"/> instance which represents
    /// the acquired lock, if any. If no lock was granted, the
    /// returned item's <see cref="LockItem.LockType"/> property
    /// returns <see cref="ResourceLockType.Denied"/>.</returns>
    LockItem TryGetWriteLock(string resourceId);

    /// <summary>
    /// Tries to acquire an exclusive write lock for the resource
    /// which expires after a given while in order not to block
    /// the resource indefinitely.
    /// If this operation succeeds, the lock must be released as soon
    /// as possible through the <see cref="ReleaseWriteLock"/>
    /// method.
    /// </summary>
    /// <param name="resourceId">Identifies the locked resource.</param>
    /// <param name="timeout">The specified expiration timeout from now.
    /// Allowed values are null for indefinite locking, or any positive value.</param>
    /// <returns>True if the lock was granted.</returns>
    /// <exception cref="ArgumentOutOfRangeException">In case of a negative
    /// timeout.</exception>sitive value.</param>
    /// <returns>True if the lock was granted.</returns>
    LockItem TryGetWriteLock(string resourceId, TimeSpan? timeout);

    /// <summary>
    /// Releases a previously acquired lock for a given
    /// resource.
    /// </summary>
    /// <param name="item">The lock to be released.</param>
    /// <returns>True if the lock was released. False in case
    /// of an unknown (e.g. automatically removed) lock, or if
    /// the lock is ignored because it's <see cref="LockItem.LockType"/>
    /// is <see cref="ResourceLockType.Denied"/>.</returns>
    bool ReleaseLock(LockItem item);


    /// <summary>
    /// Releases a previously acquired read lock for a given
    /// resource.
    /// </summary>
    /// <param name="resourceId">Identifies the locked resource.</param>
    /// <param name="lockId">The lock identifier that was acquired
    /// for the resource.</param>
    /// <returns>True if the lock was released. False in case
    /// of an unknown (e.g. automatically removed) lock.</returns>
    bool ReleaseReadLock(string resourceId, string lockId);


    /// <summary>
    /// Releases a previously acquired write lock for a given
    /// resource.
    /// </summary>
    /// <param name="resourceId">Identifies the locked resource.</param>
    /// <param name="lockId">The lock identifier that was acquired
    /// for the resource.</param>
    /// <returns>True if the lock was released. False in case
    /// of an unknown (e.g. automatically removed) lock.</returns>
    bool ReleaseWriteLock(string resourceId, string lockId);
    

    /// <summary>
    /// Gets a guard that can be used with a <c>using</c>
    /// statement, which tries to get a read lock and
    /// ensures the lock is being released as soon as
    /// the using block is being exited. Check the
    /// guard's <see cref="ResourceLockGuard.IsLockEnabled"/>
    /// property to verify whether the lock was granted
    /// or not.
    /// </summary>
    /// <returns>A guard that handles management of the
    /// acquired read lock.</returns>
    ResourceLockGuard GetReadGuard(string resourceId);

    /// <summary>
    /// Gets a guard that can be used with a <c>using</c>
    /// statement, which tries to get a write lock and
    /// ensures the lock is being released as soon as
    /// the using block is being exited. Check the
    /// guard's <see cref="ResourceLockGuard.IsLockEnabled"/>
    /// property to verify whether the lock was granted
    /// or not.
    /// </summary>
    /// <returns>A guard that handles management of the
    /// acquired write lock.</returns>
    ResourceLockGuard GetWriteGuard(string resourceId);

    /// <summary>
    /// Checks whether a resource lock is active for a given resource,
    /// and returns its locking state.
    /// </summary>
    /// <param name="resourceId">The ID of the resource to be queried.</param>
    /// <returns>The locking state of the resource.</returns>
    ResourceLockState GetLockState(string resourceId);
  }
}