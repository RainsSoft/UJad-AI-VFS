using System;
using Vfs.Util;

namespace Vfs.Auditing
{
  /// <summary>
  /// Encapsulates information to be audited.
  /// </summary>
  public class AuditItem
  {
    /// <summary>
    /// The timestamp of the audited message. Creating
    /// an instance initializes this propery with the current
    /// timestamp (<see cref="DateTimeOffset.Now"/>).
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Defines the severity of the audited incident.
    /// </summary>
    public AuditLevel Level { get; set; }

    /// <summary>
    /// The context of the audited incident (e.g. a request to
    /// delete a file: <see cref="FileSystemTask.FileDeleteRequest"/>).
    /// </summary>
    public FileSystemTask Context { get; set; }

    /// <summary>
    /// A predefined <see cref="AuditEvent"/> identifier. Set this value to
    /// <see cref="AuditEvent.Unknown"/> in case of an event that has not been
    /// defined yet.
    /// </summary>
    public AuditEvent EventId { get; set; }

    /// <summary>
    /// The textual information to be audited.
    /// </summary>
    public string Message { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public AuditItem() : this(AuditLevel.Info, FileSystemTask.Undefined, AuditEvent.Undefined)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public AuditItem(AuditLevel level,  FileSystemTask context, AuditEvent eventId)
      : this(level, context, eventId, String.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public AuditItem(AuditLevel level, FileSystemTask context, AuditEvent eventId, string message)
    {
      Timestamp = SystemTime.Now();
      Level = level;
      Context = context;
      EventId = eventId;
      Message = message;
    }


    /// <summary>
    /// Creates a simple string representation of the audited item.
    /// </summary>
    /// <returns></returns>
    public string CreateAuditString()
    {
      const string format = "{0}: Operation '{1}' caused event '{2}' at '{3}':\n{4}";
      return String.Format(format, Level, Context, EventId, Timestamp, Message);
    }
  }
}
