using System;

namespace Vfs.Auditing
{
  /// <summary>
  /// A null implementation which discards all received
  /// audit messages. The <see cref="IsAuditEnabled"/>
  /// method always returns <c>false</c>.
  /// </summary>
  public class NullAuditor : IAuditor
  {
    /// <summary>
    /// The <see cref="NullAuditor"/> discards all submitted messages.
    /// </summary>
    /// <param name="item">Encapsulates information to be
    /// audited.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="item"/>
    /// is a null reference.</exception>
    public void Audit(AuditItem item)
    {
      Ensure.ArgumentNotNull(item, "item");
    }

    /// <summary>
    /// Audits a given incident.
    /// </summary>
    /// <param name="level">Indicates the severity of an audited incident.</param>
    /// <param name="context">Defines the context of the audited operation on the file system.</param>
    /// <param name="eventId">An identifier that indicates the incident.</param>
    /// <param name="message">An optional message that provides background information.</param>
    public void Audit(AuditLevel level, FileSystemTask context, AuditEvent eventId, string message)
    {
    }

    /// <summary>
    /// Whether auditing is being performed for incidents of
    /// a given <see cref="AuditLevel"/> and context.
    /// </summary>
    /// <param name="level">The severity of the audited incident.</param>
    /// <param name="context">The currently performed file system operation
    /// that delivers the context of the audited incident.</param>
    /// <returns>This method always return false, as no incidents
    /// are being logged at all.</returns>
    public bool IsAuditEnabled(AuditLevel level, FileSystemTask context)
    {
      return false;
    }

  }
}