using System;
using System.Threading;
using Vfs.Util;

namespace Vfs.Transfer
{
  /// <summary>
  /// Encapsulates the current locking state of a given
  /// resource.
  /// </summary>
  /// <remarks>This class encapsulates a simple <see cref="ReaderWriterLockSlim"/>
  /// class in order to manage locks. Accordingly, a thread must immediately release
  /// its lock before re-acquiring a new lock of the instance. Otherwise, an
  /// exception will be thrown which indicates bad lock management.</remarks>
  public class ResourceLock
  {
    private readonly ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

    /// <summary>
    /// Identifies the managed resource.
    /// </summary>
    public string ResourceId { get; private set; }

    /// <summary>
    /// Gets the current lock state. This method is not synchronized.
    /// </summary>
    public ResourceLockState LockState
    {
      get
      {
        if (rwLock.IsWriteLockHeld) return ResourceLockState.Locked;
        if (rwLock.IsReadLockHeld) return ResourceLockState.ReadOnly;
        return ResourceLockState.Unlocked;
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public ResourceLock(string resourceId)
    {
      ResourceId = resourceId;
    }



    /// <summary>
    /// Gets a read lock of the resource. This method will
    /// </summary>
    public bool TryGetReadLock()
    {
      return rwLock.TryEnterReadLock(0);
    }

    public void ReleaseReadLock()
    {
      rwLock.ExitReadLock();
    }


    /// <summary>
    /// Tries to acquire an exclusive write
    /// lock for the resource. If this operation
    /// succeeds, the lock must be released as soon
    /// as possible through the <see cref="ReleaseWriteLock"/>
    /// method.
    /// </summary>
    /// <returns>True if the lock was granted.</returns>
    public bool TryGetWriteLock()
    {
      return rwLock.TryEnterWriteLock(0);
    }

    /// <summary>
    /// Releases a previously acquired write lock.
    /// Only invoke if <see cref="GetWriteLock"/>
    /// returned <c>true</c>.
    /// </summary>
    public void ReleaseWriteLock()
    {
      rwLock.ExitWriteLock();
    }


    public ResourceLockGuard GetReadGuard()
    {
      var status = TryGetReadLock();
      return new ResourceLockGuard(status, ReleaseReadLock);
    }

    public ResourceLockGuard GetWriteGuard()
    {
      var status = TryGetWriteLock();
      return new ResourceLockGuard(status, ReleaseWriteLock);
    }

  }

  public static class Ext
  {
    public static void ReadLocked(this ResourceLock resLock, 
                                  Action readAction,
                                  Action lockFailAction)
    {
      using(var guard = resLock.GetReadGuard())
      {
        if (guard.IsLockEnabled)
        {
          readAction();
        }
        else
        {
          lockFailAction();
        }
      }
    }
  }



  /// <summary>
  /// A guard that can be wrapped into a <c>using</c> statement
  /// in order to ensure proper releasing of read or write
  /// locks on a given <see cref="ResourceLock"/> instance.
  /// </summary>
  public class ResourceLockGuard : Guard
  {
    /// <summary>
    /// Indicates whether the lock was granted or not.
    /// </summary>
    public bool IsLockEnabled { get; private set; }

    /// <summary>
    /// Inits the guard with the action to be performed once
    /// disposal takes place.
    /// </summary>
    /// <param name="isLockEnabled"></param>
    /// <param name="disposeAction">The action that is being executed as soon
    /// as <see cref="Guard.Dispose"/> is being invoked.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="disposeAction"/>
    /// is a null reference.</exception>
    internal ResourceLockGuard(bool isLockEnabled, Action disposeAction) : base(disposeAction)
    {
      IsLockEnabled = isLockEnabled;
    }

    public override void Dispose()
    {
      if (IsLockEnabled) base.Dispose();
    }
  }


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
    Locked = 2
  }
}
