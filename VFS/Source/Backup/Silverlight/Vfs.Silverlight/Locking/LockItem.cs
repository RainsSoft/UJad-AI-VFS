using System;
using Vfs.Util;

namespace Vfs.Locking
{
  /// <summary>
  /// A lock on a given resource.
  /// </summary>
  public class LockItem
  {
    /// <summary>
    /// Identifies the locked resource.
    /// </summary>
    public string ResourceId { get; set; }

    /// <summary>
    /// A unique identifier of the lock within the scope
    /// of the locked resource.
    /// </summary>
    public string LockId { get; set; }

    /// <summary>
    /// Indicates the granted lock.
    /// </summary>
    public ResourceLockType LockType { get; set; }

    /// <summary>
    /// Defines the expiration timestamp of the lock. If the
    /// property is null, the lock is valid indefinitely.
    /// </summary>
    public DateTimeOffset? Expiration { get; set; }

    /// <summary>
    /// Whether the lock was granted or not. This convenience property
    /// just checks the <see cref="LockType"/> property and returns false
    /// if the property is set to <see cref="ResourceLockType.Denied"/>.
    /// </summary>
    public bool IsEnabled
    {
      get { return LockType != ResourceLockType.Denied; }
    }



    /// <summary>
    /// Creates a <see cref="LockItem"/> instance for a given resource
    /// which represents a read lock (<see cref="LockType"/> property set
    /// to <see cref="ResourceLockType.Read"/>).
    /// </summary>
    /// <param name="resourceId">Identifies the locked resource.</param>
    /// <param name="timeout">The timeout until the lock expires in
    /// seconds.</param>
    /// <returns>Read lock item.</returns>
    /// <exception cref="ArgumentOutOfRangeException">In case of a negative
    /// timeout.</exception>
    public static LockItem CreateRead(string resourceId, TimeSpan? timeout)
    {
      Ensure.ArgumentNotNegative(timeout, "timeout");

      return new LockItem
               {
                 ResourceId = resourceId,
                 LockId = Guid.NewGuid().ToString(),
                 LockType = ResourceLockType.Read,
                 Expiration = timeout.HasValue
                                ? SystemTime.Now().Add(timeout.Value)
                                : (DateTimeOffset?) null
               };
    }



    /// <summary>
    /// Creates a <see cref="LockItem"/> instance for a given resource
    /// which represents a write lock (<see cref="LockType"/> property set
    /// to <see cref="ResourceLockType.Write"/>).
    /// </summary>
    /// <param name="resourceId">Identifies the locked resource.</param>
    /// <param name="timeout">The timeout until the lock expires.</param>
    /// <returns>Write lock item.</returns>
    /// <exception cref="ArgumentOutOfRangeException">In case of a negative
    /// timeout.</exception>
    public static LockItem CreateWrite(string resourceId, TimeSpan? timeout)
    {
      Ensure.ArgumentNotNegative(timeout, "timeout");

      return new LockItem
               {
                 ResourceId = resourceId,
                 LockId = Guid.NewGuid().ToString(),
                 LockType = ResourceLockType.Write,
                 Expiration = timeout.HasValue
                                ? SystemTime.Now().Add(timeout.Value)
                                : (DateTimeOffset?) null
               };
    }


    /// <summary>
    /// Creates a <see cref="LockItem"/> instance for a given resource
    /// which represents a denied lock (<see cref="LockType"/> property set
    /// to <see cref="ResourceLockType.Denied"/>).
    /// </summary>
    /// <param name="resourceId">Identifies the locked resource.</param>
    /// <returns>Denied lock item.</returns>
    public static LockItem CreateDenied(string resourceId)
    {
      return new LockItem
               {
                 ResourceId = resourceId,
                 LockId = Guid.NewGuid().ToString(),
                 LockType = ResourceLockType.Denied
               };
    }
  }
}