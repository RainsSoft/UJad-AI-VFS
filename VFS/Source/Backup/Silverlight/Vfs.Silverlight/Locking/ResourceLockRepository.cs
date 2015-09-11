using System;
using System.Collections.Generic;


namespace Vfs.Locking
{
  /// <summary>
  /// A repository that maintains <see cref="LockTracker"/>
  /// instances for actively locked resources in a synchronized dictionary.
  /// </summary>
  public class ResourceLockRepository : IResourceLockRepository
  {
    /// <summary>
    /// The internal dictionary that manages the currently active resource locks.
    /// </summary>
    readonly Dictionary<string, LockTracker> lockCache = new Dictionary<string, LockTracker>();
    

    /// <summary>
    /// Gets the resource locks for a given resource from the internal
    /// cache, or creates a new instance if no cached <see cref="LockTracker"/>
    /// instance was found.
    /// </summary>
    /// <param name="resourceId">Identifies the locked resource.</param>
    /// <param name="createIfNotFound">Whether to create and cache a new
    /// <see cref="LockTracker"/> instance if no item is found in the
    /// cache, or just return null.</param>
    /// <returns>A <see cref="LockTracker"/> instance for the requested
    /// resource identifier.</returns>
    private LockTracker GetLocks(string resourceId, bool createIfNotFound)
    {
      LockTracker tracker;
      bool status = lockCache.TryGetValue(resourceId, out tracker);
      if(!status && createIfNotFound)
      {
        tracker = new LockTracker(resourceId);
        lockCache.Add(resourceId, tracker);
      }

      return tracker;
    }


    /// <summary>
    /// Tries to acquire a shared read lock for a given resource
    /// which never expires.
    /// If this operation succeeds, the lock must be released as soon
    /// as possible through the <see cref="IResourceLockRepository.ReleaseReadLock"/>
    /// method.
    /// </summary>
    /// <returns>A <see cref="LockItem"/> instance which represents
    /// the acquired lock, if any. If no lock was granted, the
    /// returned item's <see cref="LockItem.LockType"/> property
    /// returns <see cref="ResourceLockType.Denied"/>.</returns>
    public LockItem TryGetReadLock(string resourceId)
    {
      return TryGetReadLock(resourceId, null);
    }


    /// <summary>
    /// Tries to acquire an exclusive write lock for the resource
    /// which expires after a given time span in order not to block
    /// the resource indefinitely.
    /// If this operation succeeds, the lock must be released as soon
    /// as possible through the <see cref="IResourceLockRepository.ReleaseReadLock"/>
    /// method.
    /// </summary>
    /// <param name="resourceId">Identifies the locked resource.</param>
    /// <param name="timeout">The specified expiration timeout from now.
    /// Allowed values are null for indefinite locking, or any positive value.</param>
    /// <returns>True if the lock was granted.</returns>
    /// <exception cref="ArgumentOutOfRangeException">In case of a negative
    /// timeout.</exception>
    public LockItem TryGetReadLock(string resourceId, TimeSpan? timeout)
    {
      lock (this)
      {
        var resLock = GetLocks(resourceId, true);
        return resLock.TryGetReadLock(timeout);
      }
    }


    /// <summary>
    /// Tries to acquire an exclusive write lock for a given resource
    /// which never expires.
    /// If this operation succeeds, the lock must be released as soon
    /// as possible through the <see cref="IResourceLockRepository.ReleaseWriteLock"/>
    /// method.
    /// </summary>
    /// <returns>A <see cref="LockItem"/> instance which represents
    /// the acquired lock, if any. If no lock was granted, the
    /// returned item's <see cref="LockItem.LockType"/> property
    /// returns <see cref="ResourceLockType.Denied"/>.</returns>
    public LockItem TryGetWriteLock(string resourceId)
    {
      return TryGetWriteLock(resourceId, null);
    }


    /// <summary>
    /// Tries to acquire an exclusive write lock for the resource
    /// which expires after a given while in order not to block
    /// the resource indefinitely.
    /// If this operation succeeds, the lock must be released as soon
    /// as possible through the <see cref="IResourceLockRepository.ReleaseWriteLock"/>
    /// method.
    /// </summary>
    /// <param name="resourceId">Identifies the locked resource.</param>
    /// <param name="timeout">The specified expiration timeout from now.
    /// Allowed values are null for indefinite locking, or any positive value.</param>
    /// <returns>True if the lock was granted.</returns>
    /// <exception cref="ArgumentOutOfRangeException">In case of a negative
    /// timeout.</exception>sitive value.</param>
    /// <returns>True if the lock was granted.</returns>
    public LockItem TryGetWriteLock(string resourceId, TimeSpan? timeout)
    {
      lock (this)
      {
        var resLock = GetLocks(resourceId, true);
        return resLock.TryGetWriteLock(timeout);
      }
    }


    /// <summary>
    /// Releases a previously acquired lock for a given
    /// resource.
    /// </summary>
    /// <param name="item">The lock to be released.</param>
    /// <returns>True if the lock was released. False in case
    /// of an unknown (e.g. automatically removed) lock, or if
    /// the lock is ignored because it's <see cref="LockItem.LockType"/>
    /// is <see cref="ResourceLockType.Denied"/>.</returns>
    public bool ReleaseLock(LockItem item)
    {
      switch (item.LockType)
      {
        case ResourceLockType.Read:
          return ReleaseReadLock(item.ResourceId, item.LockId);
        case ResourceLockType.Write:
          return ReleaseWriteLock(item.ResourceId, item.LockId);
        case ResourceLockType.Denied:
          return false;
        default:
          throw new ArgumentOutOfRangeException("Unsupported lock type: " + item.LockType);
      }
    }


    /// <summary>
    /// Releases a previously acquired read lock for a given
    /// resource.
    /// </summary>
    /// <param name="resourceId">Identifies the locked resource.</param>
    /// <param name="lockId">The lock identifier that was acquired
    /// for the resource.</param>
    /// <returns>True if the lock was released. False in case
    /// of an unknown (e.g. automatically removed) lock.</returns>
    public bool ReleaseReadLock(string resourceId, string lockId)
    {
      lock(this)
      {
        var locks = GetLocks(resourceId, false);
        if(locks == null) return false;

        bool status = locks.ReleaseReadLock(lockId);
        if (status && locks.LockState == ResourceLockState.Unlocked)
        {
          //remove unlocked resource from cache
          lockCache.Remove(resourceId);
        }

        return status;
      }
    }


    /// <summary>
    /// Releases a previously acquired write lock for a given
    /// resource.
    /// </summary>
    /// <param name="resourceId">Identifies the locked resource.</param>
    /// <param name="lockId">The lock identifier that was acquired
    /// for the resource.</param>
    /// <returns>True if the lock was released. False in case
    /// of an unknown (e.g. automatically removed) lock.</returns>
    public bool ReleaseWriteLock(string resourceId, string lockId)
    {
      lock (this)
      {
        var locks = GetLocks(resourceId, false);
        if (locks == null) return false;

        bool status = locks.ReleaseWriteLock(lockId);
        if (status && locks.LockState == ResourceLockState.Unlocked)
        {
          //remove unlocked resource from cache
          lockCache.Remove(resourceId);
        }

        return status;
      }
    }


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
    public ResourceLockGuard GetReadGuard(string resourceId)
    {
      var item = TryGetReadLock(resourceId);
      return new ResourceLockGuard(item, this);
    }


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
    public ResourceLockGuard GetWriteGuard(string resourceId)
    {
      var item = TryGetWriteLock(resourceId);
      return new ResourceLockGuard(item, this);
    }


    /// <summary>
    /// Checks whether a resource lock is active for a given resource,
    /// and returns its locking state.
    /// </summary>
    /// <param name="resourceId">The ID of the resource to be queried.</param>
    /// <returns>The locking state of the resource.</returns>
    public ResourceLockState GetLockState(string resourceId)
    {
      lock(this)
      {
        LockTracker tracker;
        bool status = lockCache.TryGetValue(resourceId, out tracker);
        return status ? tracker.LockState : ResourceLockState.Unlocked;
      }
    }
  }
}
