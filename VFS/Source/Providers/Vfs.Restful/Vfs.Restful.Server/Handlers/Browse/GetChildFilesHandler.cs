using System.Collections.Generic;
using System.Linq;

namespace Vfs.Restful.Server.Handlers.Browse
{
  public class GetChildFilesHandler : BrowsingHandler
  {
    public GetChildFilesHandler(IFileSystemProvider fileSystem) : base(fileSystem)
    {
    }

    public OperationResult<IEnumerable<VirtualFileInfo>> Get(string parentFolderPath)
    {
      return SecureFunc(() => (IEnumerable<VirtualFileInfo>)FileSystem.GetChildFiles(parentFolderPath).ToArray());
    }


    public OperationResult<IEnumerable<VirtualFileInfo>> Get(string parentFolderPath, string filter)
    {
      return SecureFunc(() => (IEnumerable<VirtualFileInfo>)FileSystem.GetChildFiles(parentFolderPath, filter).ToArray());
    }
  }
}