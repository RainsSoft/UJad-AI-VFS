using System;
using System.Runtime.Serialization;

namespace Vfs
{
  /// <summary>
  /// An exception that is thrown is a resource request contains
  /// an invalid path string that cannot be interpreted as a valid
  /// resource identifier.<br/>
  /// </summary>
#if !SILVERLIGHT
  [Serializable]
#endif
  public class InvalidResourcePathException : VfsException
  {
    /// <summary>
    /// The resource in question, if any.
    /// </summary>
    public VirtualResourceInfo Resource { get; set; }


    public InvalidResourcePathException()
    {
    }

    public InvalidResourcePathException(string message) : base(message)
    {
    }

    public InvalidResourcePathException(string message, Exception inner) : base(message, inner)
    {
    }

#if !SILVERLIGHT
    protected InvalidResourcePathException(
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
      get { return VfsFaultType.ResourcePathInvalid; }
    }
  }
}