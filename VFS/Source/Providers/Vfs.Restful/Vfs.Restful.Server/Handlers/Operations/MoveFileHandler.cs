using OpenRasta.Web;

namespace Vfs.Restful.Server.Handlers.Operations
{
  public class MoveFileHandler : VfsHandlerBase
  {
    public MoveFileHandler(IFileSystemProvider fileSystem)
      : base(fileSystem)
    {
    }

    [HttpOperation(HttpMethod.POST, ForUriName = "MoveFile")]
    public OperationResult<VirtualFileInfo> Post(string filePath, string destinationPath)
    {
      return SecureFunc(() => FileSystem.MoveFile(filePath, destinationPath));
    }
  }
}