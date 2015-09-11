using System;
using System.Runtime.Serialization;

namespace Vfs
{
  /// <summary>
  /// An exception that is thrown for illegal attempts to
  /// overwrite an already existing resource.
  /// </summary>
#if !SILVERLIGHT
  [Serializable]
#endif
  public class ResourceOverwriteException : VfsException
  {
    //
    // For guidelines regarding the creation of new exception types, see
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
    // and
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
    //

    public ResourceOverwriteException()
    {
    }

    public ResourceOverwriteException(string message) : base(message)
    {
    }

    public ResourceOverwriteException(string message, Exception inner) : base(message, inner)
    {
    }

#if !SILVERLIGHT
    protected ResourceOverwriteException(
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
      get { return VfsFaultType.ResourceOverwrite; }
    }
  }
}