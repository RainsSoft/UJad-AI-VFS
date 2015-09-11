using System.Collections.Generic;
using System.Linq;

namespace Vfs.Restful.Server.Handlers.Browse
{
  public class GetChildFoldersHandler : BrowsingHandler
  {
    public GetChildFoldersHandler(IFileSystemProvider fileSystem) : base(fileSystem)
    {
    }


    public OperationResult<IEnumerable<VirtualFolderInfo>> Get(string parentFolderPath)
    {
      return SecureFunc(() => (IEnumerable<VirtualFolderInfo>)FileSystem.GetChildFolders(parentFolderPath).ToArray());
    }


    public OperationResult<IEnumerable<VirtualFolderInfo>> Get(string parentFolderPath, string filter)
    {
      return SecureFunc(() => (IEnumerable<VirtualFolderInfo>) FileSystem.GetChildFolders(parentFolderPath, filter).ToArray());
    }
  }
}