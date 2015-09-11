namespace Vfs
{
  /// <summary>
  /// A wrapper class that encasulates information about a given file resource.
  /// Used by file system providers in order to handle requests that refer
  /// to that given file.
  /// </summary>
  public abstract class VirtualFileItem : IVirtualFileItem
  {
    /// <summary>
    /// Gets the underlying resource.
    /// </summary>
    public VirtualFileInfo ResourceInfo { get; set; }

    /// <summary>
    /// Indicates whether the resource physically exists on the file system
    /// or not.
    /// </summary>
    public abstract bool Exists { get; }

    /// <summary>
    /// Gets a string that provides the fully qualified string of the resource (as opposite to the
    /// <see cref="VirtualResourceInfo.FullName"/>, which is publicly exposed to
    /// clients, e.g. in exception messages).<br/>
    /// It should be ensured that this identifier always looks exactly the same for different requests,
    /// as it is being used for internal processes such as resource locking or auditing.
    /// </summary>
    public abstract string QualifiedIdentifier { get; }
  }
}