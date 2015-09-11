using OpenRasta.Web;

namespace Vfs.Restful.Server.Handlers.Operations
{
  public class DeleteFileHandler : VfsHandlerBase
  {
    public DeleteFileHandler(IFileSystemProvider fileSystem)
      : base(fileSystem)
    {
    }


    [HttpOperation(HttpMethod.DELETE, ForUriName = "DeleteFile")]
    public OperationResult Delete(string filepath)
    {
      return SecureAction(() => FileSystem.DeleteFile(filepath));
    }
  }
}