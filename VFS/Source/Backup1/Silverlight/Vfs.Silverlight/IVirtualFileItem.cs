namespace Vfs
{
  /// <summary>
  /// Common interface for wrapper classes that encasulates information
  /// about a given file resource. Used by file system providers in order
  /// to handle requests that refer to that given file.
  /// </summary>
  public interface IVirtualFileItem : IVirtualResourceItem<VirtualFileInfo>
  {
  }
}