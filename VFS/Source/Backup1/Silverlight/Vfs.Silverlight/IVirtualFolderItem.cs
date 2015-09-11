namespace Vfs
{
  /// <summary>
  /// A common interface for wrapper class that encasulates information about
  /// a given folder resource. Used by file system providers in order to handle
  /// requests that refer to that given folder.
  /// </summary>
  public interface IVirtualFolderItem : IVirtualResourceItem<VirtualFolderInfo>
  {
  }
}