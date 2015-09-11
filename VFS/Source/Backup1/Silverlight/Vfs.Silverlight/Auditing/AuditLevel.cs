namespace Vfs.Auditing
{
  /// <summary>
  /// Indicates the severity of an audited incident.
  /// </summary>
  public enum AuditLevel
  {
    /// <summary>
    /// An information is audited.
    /// </summary>
    Info,
    /// <summary>
    /// Warnings include all incidents that do not affect VFS, but
    /// might indicate a problem. This includes requests for resources
    /// that do not exist, blocked attempts due to missing access rights
    /// etc.
    /// </summary>
    Warning,
    /// <summary>
    /// Indicates that an internal error happened, or an incident
    /// should be reviewed asap.
    /// </summary>
    Critical
  }
}