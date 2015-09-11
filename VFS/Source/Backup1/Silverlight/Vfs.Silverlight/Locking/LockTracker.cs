using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Vfs.Locking
{
  /// <summary>
  /// Encapsulates the current locking state of a given
  /// resource.
  /// </summary>
  /// <remarks>This class encapsulates a simple <see cref="ReaderWriterLockSlim"/>
  /// class in order to manage locks. Accordingly, a thread must immediately release
  /// its lock before re-acquiring a new lock of the instance. Otherwise, an
  /// exception will be thrown which indicates bad lock management.</remarks>
  public class LockTracker
  {
    /// <summary>
    /// The currently active read locks, if any.
    /// </summary>
    private readonly List<LockItem> readLocks = new List<LockItem>();

    /// <summary>
    /// A counter which is used to periodically clean up expired
    /// locks. Given we do this rarely and locks are being removed
    /// as soon as there are no active ones, cleanups should be
    /// rarely necessary.
    /// </summary>
    private int cleanUpCounter;

    /// <summary>
    /// Stores a reference to a granted write lock. If this property
    /// is set, and the lock's <see cref="LockItem.Expiration"/> is
    /// still valid, no other locks are granted.
    /// </summary>
    private LockItem activeWriteLock;

    /// <summary>
    /// Identifies the managed resource.
    /// </summary>
    public string ResourceId { get; private set; }
    
    /// <summary>
    /// Indicates whether a write lock was issued for the resource.
    /// Write locks are granted exclusively, which also means that
    /// no read locks can be active.
    /// </summary>
    public bool HasWriteLock
    {
      get
      {
        var item = activeWriteLock;
        return item != null && !item.Expiration.IsExpired();
      }
    }


    /// <summary>
    /// Checks whether any active read locks are held.
    /// </summary>
    public bool HasReadLocks
    {
      get
      {
        lock(this)
        {
          return readLocks.FirstOrDefault(l => l.Expiration.IsExpired() == false) != null;
        }
      }
    }


    /// <summary>
    /// Gets the current lock state. This method is not synchronized.
    /// </summary>
    public ResourceLockState LockState
    {
      get
      {
        lock (this)
        {
          //check for a valid write lock
          if (HasWriteLock) return ResourceLockState.Locked;

          //check for active read locks
          if (HasReadLocks)
          {
            return ResourceLockState.ReadOnly;
          }

          //no active locks means no locks at all
          return ResourceLockState.Unlocked;
        }
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public LockTracker(string resourceId)
    {
      ResourceId = resourceId;
    }


    /// <summary>
    /// Tries to acquire an indefinite shared read lock
    /// for the resource. If this operation
    /// succeeds, the lock must be released as soon
    /// as possible through the <see cref="ReleaseReadLock"/>
    /// method.
    /// </summary>
    /// <returns>A lock that corresponds to the request. If the lock was denied, the
    /// returned item's <see cref="LockItem.LockType"/>
    /// is <see cref="ResourceLockType.Denied"/>.</returns>
    public LockItem TryGetReadLock()
    {
      return TryGetReadLock(null);
    }



    /// <summary>
    /// Tries to acquire a shared read lock
    /// for the resource, and specifies a timeout for the lock. If this operation
    /// succeeds, the lock must be released as soon
    /// as possible through the <see cref="ReleaseReadLock"/>
    /// method.
    /// </summary>
    /// <returns>A lock that corresponds to the request. If the lock was denied, the
    /// returned item's <see cref="LockItem.LockType"/>
    /// is <see cref="ResourceLockType.Denied"/>.</returns>
    /// <param name="timeout">The timeout of the lock.
    /// A null value requests a lock that doesn't time out.</param>
    /// <returns>True if the lock was granted.</returns>
    /// <exception cref="ArgumentOutOfRangeException">In case of a negative
    /// timeout.</exception>
    public LockItem TryGetReadLock(TimeSpan? timeout)
    {
      lock (this)
      {
        if (HasWriteLock) return LockItem.CreateDenied(ResourceId);

        //clean up inactive locks from time to time
        Cleanup();

        var item = LockItem.CreateRead(ResourceId, timeout);
        readLocks.Add(item);
        return item;
      }
    }



    /// <summary>
    /// Releases a previously acquired read lock.
    /// </summary>
    /// <param name="lockId">The <see cref="LockItem.LockId"/>
    /// which identifies the lock to be released.</param>
    /// <returns>True if a matching lock was found and released.
    /// If no lock was found, false.</returns>
    public bool ReleaseReadLock(string lockId)
    {
      lock (this)
      {
        for (int i = 0; i < readLocks.Count; i++)
        {
          LockItem lockItem = readLocks[i];
          if (lockItem.LockId == lockId)
          {
            readLocks.RemoveAt(i);

            //don't check whether lock has expired - invoking party is happy
            return true;
          }
        }

        return false;
      }
    }


    /// <summary>
    /// Tries to acquire an exclusive write
    /// lock for the resource. If this operation
    /// succeeds, the lock must be released as soon
    /// as possible through the <see cref="ReleaseWriteLock"/>
    /// method.
    /// </summary>
    /// <returns>True if the lock was granted.</returns>
    public LockItem TryGetWriteLock()
    {
      return TryGetWriteLock(null);
    }

    /// <summary>
    /// Tries to acquire an exclusive write
    /// for the resource, and specifies a timeout for the lock. If this operation
    /// succeeds, the lock must be released as soon
    /// as possible through the <see cref="ReleaseWriteLock"/>
    /// method.
    /// </summary>
    /// <param name="timeout">The timeout of the lock.
    /// A value of null requests a lock that doesn't time out.</param>
    /// <returns>A lock that corresponds to the request. If the lock was denied, the
    /// returned item's <see cref="LockItem.LockType"/>
    /// is <see cref="ResourceLockType.Denied"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">In case of a negative
    /// timeout.</exception>
    public LockItem TryGetWriteLock(TimeSpan? timeout)
    {
      lock (this)
      {
        if (HasWriteLock || HasReadLocks)
        {
          return LockItem.CreateDenied(ResourceId);
        }

        activeWriteLock = LockItem.CreateWrite(ResourceId, timeout);
        return activeWriteLock;
      }
    }


    /// <summary>
    /// Releases a previously acquired write lock.
    /// </summary>
    /// <param name="lockId">The <see cref="LockItem.LockId"/>
    /// which identifies the lock to be released.</param>
    /// <returns>True if a matching lock was found and released.
    /// If no lock was found, false.</returns>
    public bool ReleaseWriteLock(string lockId)
    {
      lock (this)
      {
        if(activeWriteLock != null && activeWriteLock.LockId == lockId)
        {
          //don't check whether lock has expired - invoking party is happy
          activeWriteLock = null;
          return true;
        }

        return false;
      }
    }


    /// <summary>
    /// Removes expired locks.
    /// </summary>
    private void Cleanup()
    {
      cleanUpCounter++;
      if (cleanUpCounter % 100 == 0)
      {
        //TODO audit/log expired locks?
        readLocks.RemoveAll(li => li.Expiration.IsExpired());
        
        if (activeWriteLock != null && activeWriteLock.Expiration.IsExpired())
        {
          activeWriteLock = null;
        }

        cleanUpCounter = 1;
      }
    }
  }
}