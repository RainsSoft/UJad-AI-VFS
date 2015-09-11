using OpenRasta.Web;

namespace Vfs.Restful.Server.Handlers.Operations
{
  public class CopyFileHandler : VfsHandlerBase
  {
    public CopyFileHandler(IFileSystemProvider fileSystem)
      : base(fileSystem)
    {
    }

    
    [HttpOperation(HttpMethod.POST, ForUriName = "CopyFile")]
    public OperationResult<VirtualFileInfo> Post(string filePath, string destinationPath)
    {
      return SecureFunc(() => FileSystem.CopyFile(filePath, destinationPath));
    }
  }
}