using System;
using System.Text;

namespace Vfs.Restful.Server.Handlers.Browse
{
  public class GetRootFolderHandler : BrowsingHandler
  {
    public GetRootFolderHandler(IFileSystemProvider fileSystem) : base(fileSystem)
    {
    }

    /// <summary>
    /// Gets the file system root folder from the underlying
    /// file system.
    /// </summary>
    public OperationResult<VirtualFolderInfo> Get()
    {
      return SecureFunc(() => FileSystem.GetFileSystemRoot());
    }
  }


}
