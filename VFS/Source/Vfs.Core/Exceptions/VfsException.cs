using System;
using System.Runtime.Serialization;

namespace Vfs
{
  /// <summary>
  /// 
  /// </summary>
#if !SILVERLIGHT
  [Serializable]
#endif
  public abstract class VfsException : Exception
  {
    /// <summary>
    /// The VFS event ID. Check the documentation for further information
    /// about the event.
    /// </summary>
    public int EventId { get; set; }


    /// <summary>
    /// Indicates whether the exception has already been audited. This property
    /// is used in order to make sure that bubbling exceptions are
    /// properly audited without creating duplicate entries in the audit trail.
    /// </summary>
    /// <seealso cref="SuppressAuditing"/>
    public bool IsAudited { get; set; }


    /// <summary>
    /// Set this property to true in order to suppress auditing if the exception is being
    /// bubbled.
    /// </summary>
    /// <seealso cref="IsAudited"/>
    public bool SuppressAuditing { get; set; }

    /// <summary>
    /// The fault type can be used in order to transfer fault information
    /// in disconnected scenarios.
    /// </summary>
    public abstract VfsFaultType FaultType { get; }

    /// <summary>
    /// Creates an empty exception. 
    /// </summary>
    protected VfsException()
    {
    }


    /// <summary>
    /// Creates a new exception with an attached exception message. 
    /// </summary>
    /// <param name="message">Exception information.</param>
    protected VfsException(string message)
        : base(message)
    {
    }


    /// <summary>
    /// Creates a new exception based on another exception.
    /// </summary>
    /// <param name="message">Exception information.</param>
    /// <param name="innerException">Inner exception that caused the failure.</param>
    protected VfsException(string message, Exception innerException)
        : base(message, innerException)
    {
    }


#if !SILVERLIGHT
    /// <summary>
    /// Protected constructor that ensures proper exception serialization across
    /// AppDomain boundaries.
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    protected VfsException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
      //no type-specific serialization constructor logic
    }
#endif
  }
}