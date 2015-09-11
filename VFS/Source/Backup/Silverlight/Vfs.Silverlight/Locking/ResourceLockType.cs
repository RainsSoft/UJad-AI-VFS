
namespace Vfs.Locking
{
  /// <summary>
  /// Resource lock flags.
  /// </summary>
  public enum ResourceLockType
  {
    /// <summary>
    /// The lock was denied.
    /// </summary>
    Denied,
    /// <summary>
    /// A read lock is being acquired.
    /// </summary>
    Read,
    /// <summary>
    /// A write lock is being acquired.
    /// </summary>
    Write
  }
}
