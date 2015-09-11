using System;


namespace Vfs.Auditing
{
  /// <summary>
  /// Auditing interface, which allows to plug in a custom
  /// auditor into a given <see cref="IFileSystemProvider"/>.
  /// </summary>
  public interface IAuditor
  {
    /// <summary>
    /// Audits a given incident.
    /// </summary>
    /// <param name="item">Encapsulates information to be
    /// audited.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="item"/>
    /// is a null reference.</exception>
    void Audit(AuditItem item);


    /// <summary>
    /// Audits a given incident.
    /// </summary>
    /// <param name="level">Indicates the severity of an audited incident.</param>
    /// <param name="context">Defines the context of the audited operation on the file system.</param>
    /// <param name="eventId">An identifier that indicates the incident.</param>
    /// <param name="message">An optional message that provides background information.</param>
    void Audit(AuditLevel level, FileSystemTask context, AuditEvent eventId, string message);


    /// <summary>
    /// Whether auditing is being performed for incidents of
    /// a given <see cref="AuditLevel"/> and context.
    /// </summary>
    /// <param name="level">The severity of the audited incident.</param>
    /// <param name="context">The currently performed file system operation
    /// that delivers the context of the audited incident.</param>
    /// <returns>True if messages for the level and area are being actively
    /// audited. If this method returns false, <see cref="Audit"/> is
    /// not supposed to be invoked with an <see cref="AuditItem"/>
    /// that matches this level and area.</returns>
    bool IsAuditEnabled(AuditLevel level, FileSystemTask context);
  }
}
