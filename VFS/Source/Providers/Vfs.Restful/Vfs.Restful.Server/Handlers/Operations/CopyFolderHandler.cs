using OpenRasta.Web;

namespace Vfs.Restful.Server.Handlers.Operations
{
  public class CopyFolderHandler : VfsHandlerBase
  {
    public CopyFolderHandler(IFileSystemProvider fileSystem)
      : base(fileSystem)
    {
    }

    [HttpOperation(HttpMethod.POST, ForUriName = "CopyFolder")]
    public OperationResult<VirtualFolderInfo> Post(string folderPath, string destinationPath)
    {
      return SecureFunc(() => FileSystem.CopyFolder(folderPath, destinationPath));
    }
  }
}