using System;
using System.Runtime.Serialization;

namespace Vfs
{
  /// <summary>
  /// Thrown if a request to access a given resource was blocked
  /// because the resource is locked.
  /// </summary>
#if !SILVERLIGHT
  [Serializable]
#endif
  public class ResourceLockedException : VfsException
  {
    public ResourceLockedException()
    {
    }

    public ResourceLockedException(string message) : base(message)
    {
    }

    public ResourceLockedException(string message, Exception inner) : base(message, inner)
    {
    }

#if !SILVERLIGHT
    protected ResourceLockedException(
      SerializationInfo info,
      StreamingContext context) : base(info, context)
    {
    }
#endif

    /// <summary>
    /// The fault type can be used in order to transfer fault information
    /// in disconnected scenarios.
    /// </summary>
    public override VfsFaultType FaultType
    {
      get { return VfsFaultType.ResourceLocked; }
    }
  }

}
