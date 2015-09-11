using System;
using System.Runtime.Serialization;

namespace Vfs
{
  /// <summary>
  /// A generic exception which is thrown in case of an invalid
  /// resource request.
  /// </summary>
#if !SILVERLIGHT
  [Serializable]
#endif
  public class ResourceAccessException : VfsException
  {
    /// <summary>
    /// The resource in question, if any.
    /// </summary>
    public VirtualResourceInfo Resource { get; set; }


    public ResourceAccessException()
    {
    }

    public ResourceAccessException(string message) : base(message)
    {
    }

    public ResourceAccessException(string message, Exception inner) : base(message, inner)
    {
    }

#if !SILVERLIGHT
    protected ResourceAccessException(
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
      get { return VfsFaultType.ResourceAccess; }
    }
  }
}