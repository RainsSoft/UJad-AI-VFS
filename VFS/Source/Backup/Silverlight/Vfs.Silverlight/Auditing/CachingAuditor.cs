using System;
using System.Collections.Generic;

namespace Vfs.Auditing
{
  /// <summary>
  /// An auditor that just maintains a sequential list of audited items.
  /// </summary>
  public class CachingAuditor : IAuditor
  {
    public List<AuditItem> Items { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public CachingAuditor()
    {
      Items = new List<AuditItem>();
    }


    public void Reset()
    {
      lock(this)
      {
        Items.Clear();
      }
    }


    /// <summary>
    /// Audits a given message and sorts the <see cref="Items"/> list
    /// by the submitted timestamps.
    /// </summary>
    /// <param name="item">Encapsulates information to be
    /// audited.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="item"/>
    /// is a null reference.</exception>
    public void Audit(AuditItem item)
    {
      if(!IsAuditEnabled(item.Level, item.Context)) return;

      lock (this)
      {
        Items.Add(item);
        Items.Sort((a1, a2) => a1.Timestamp.CompareTo(a2.Timestamp));
      }
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
      Audit(new AuditItem(level, context, eventId, message));
    }

    /// <summary>
    /// Whether auditing is being performed for incidents of
    /// a given <see cref="AuditLevel"/> and context.
    /// </summary>
    /// <param name="level">The severity of the audited incident.</param>
    /// <param name="context">The currently performed file system operation
    /// that delivers the context of the audited incident.</param>
    /// <returns>True if messages for the level and area are being actively
    /// audited. If this method returns false, <see cref="IAuditor.Audit"/> is
    /// not supposed to be invoked with an <see cref="AuditItem"/>
    /// that matches this level and area.</returns>
    public bool IsAuditEnabled(AuditLevel level, FileSystemTask context)
    {
      return true;
    }

  }
}
