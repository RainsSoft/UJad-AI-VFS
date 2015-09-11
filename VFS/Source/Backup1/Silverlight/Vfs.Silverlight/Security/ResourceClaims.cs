using System.Collections.Generic;

namespace Vfs.Security
{
  /// <summary>
  /// Encapsulates permissions related to a given
  /// resource.
  /// </summary>
  /// <remarks>A claims object does *not* define whether
  /// a resource's information can be retrieved at all, so there's
  /// no <c>IsHidden</c> or <c>IsLocked</c> property. After all, if a
  /// requestor already has the resource it's too late for that.
  /// Accordingly, it's up to a provider implementation not to
  /// deliver information about a locked / forbidden file in the first
  /// place.</remarks>
  public abstract class ResourceClaims
  {
    /// <summary>
    /// Gets a list of custom properties associated with
    /// the resource.
    /// </summary>
    public IEnumerable<ResourceClaimProperty> CustomProperties { get; protected set; }

    /// <summary>
    /// Whether the resource can be deleted.
    /// </summary>
    public bool AllowDelete { get; set; }

    /// <summary>
    /// Whether the resource can be renamed.
    /// A security provider might decide to allow this
    /// despite missing permissions to delete and
    /// recreate new files.
    /// </summary>
    public bool AllowRename { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    protected ResourceClaims()
    {
      CustomProperties = new List<ResourceClaimProperty>();
    }
  }
}