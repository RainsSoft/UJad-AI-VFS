using OpenRasta.Web;

namespace Vfs.Restful.Server.Handlers.Operations
{
  public class MoveFolderHandler : VfsHandlerBase
  {
    public MoveFolderHandler(IFileSystemProvider fileSystem) : base(fileSystem)
    {
    }


    [HttpOperation(HttpMethod.POST, ForUriName = "MoveFolder")]
    public OperationResult<VirtualFolderInfo> Post(string folderPath, string destinationPath)
    {
      return SecureFunc(() => FileSystem.MoveFolder(folderPath, destinationPath));
    }
  }
}