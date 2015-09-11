using System;
using System.Collections.Generic;

namespace Vfs.Locking
{
  /// <summary>
  /// A guard that can be wrapped into a <c>using</c> statement
  /// in order to ensure proper releasing of read or write
  /// locks on a given <see cref="LockItem"/> instance.
  /// </summary>
  public class ResourceLockGuard : IDisposable
  {
    /// <summary>
    /// Indicates whether the lock was granted or not.
    /// </summary>
    public bool IsLockEnabled
    {
      get { return Item.LockType != ResourceLockType.Denied; }
    }

    /// <summary>
    /// A list of secondary locks that are being released
    /// after releasing the <see cref="LockItem"/>.
    /// </summary>
    public IEnumerable<LockItem> SecondaryLocks { get; set; }

    /// <summary>
    /// Gets the underlying lock item.
    /// </summary>
    public LockItem Item { get; private set; }


    /// <summary>
    /// The used resource lock repository.
    /// </summary>
    public IResourceLockRepository Repository { get; private set; }


    /// <summary>
    /// Inits the guard with the action to be performed once
    /// disposal takes place.
    /// </summary>
    /// <param name="item">Encapsulates the locking information.</param>
    /// <param name="repository">The repository that issued the lock.</param>
    /// <exception cref="ArgumentNullException">If any of the parameters
    /// is a null reference.</exception>
    public ResourceLockGuard(LockItem item, IResourceLockRepository repository)
    {
      Ensure.ArgumentNotNull(item, "item");
      Ensure.ArgumentNotNull(repository, "repository");

      Item = item;
      Repository = repository;
    }


    /// <summary>
    /// Releases the lock on the underlying <see cref="Item"/>
    /// if it's not a denied one anyway.
    /// </summary>
    public void Dispose()
    {
      switch(Item.LockType)
      {
        case ResourceLockType.Denied:
          //nothing to do
          break;
        case ResourceLockType.Read:
          Repository.ReleaseReadLock(Item.ResourceId, Item.LockId);
          break;
        case ResourceLockType.Write:
          Repository.ReleaseWriteLock(Item.ResourceId, Item.LockId);
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }

      if(SecondaryLocks != null)
      {
        SecondaryLocks.Do(li => Repository.ReleaseLock(li));
      }
    }
  }
}