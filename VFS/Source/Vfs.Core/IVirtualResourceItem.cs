namespace Vfs
{
  /// <summary>
  /// A simple wrapper for a virtual resource which can be used
  /// to pass along a currently processed resource along with
  /// some additional data.
  /// </summary>
  /// <typeparam name="T">The type of the encapsulated resource.</typeparam>
  public interface IVirtualResourceItem<T> where T : VirtualResourceInfo
  {
    /// <summary>
    /// Gets the underlying resource.
    /// </summary>
    T ResourceInfo { get; }

    /// <summary>
    /// Indicates whether the resource physically exists on the file system
    /// or not.
    /// </summary>
    bool Exists { get; }

    /// <summary>
    /// Gets a string that provides the fully qualified string of the resource (as opposite to the
    /// <see cref="VirtualResourceInfo.FullName"/>, which is publicly exposed to
    /// clients, e.g. in exception messages).<br/>
    /// It should be ensured that this identifier always looks the same for different requests,
    /// as it is being used for internal processes such as resource locking or auditing.
    /// </summary>
    string QualifiedIdentifier { get; }
  }
}