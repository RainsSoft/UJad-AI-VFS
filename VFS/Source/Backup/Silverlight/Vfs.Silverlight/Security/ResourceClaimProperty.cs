using System.Runtime.Serialization;

namespace Vfs.Security
{
  /// <summary>
  /// A named binary flag that depicts a custom
  /// property associated with a resource's
  /// permissions.
  /// </summary>
  public class ResourceClaimProperty
  {
    /// <summary>
    /// The property name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The boolean property value, which indicates
    /// whether the claim is granted or not.
    /// </summary>
    public bool Value { get; set; }
  }
}