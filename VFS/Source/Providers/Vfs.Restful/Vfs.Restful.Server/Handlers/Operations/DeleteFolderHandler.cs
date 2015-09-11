using OpenRasta.Web;

namespace Vfs.Restful.Server.Handlers.Operations
{
  public class DeleteFolderHandler : VfsHandlerBase
  {
    public DeleteFolderHandler(IFileSystemProvider fileSystem)
      : base(fileSystem)
    {
    }

    [HttpOperation(HttpMethod.DELETE, ForUriName = "DeleteFolder")]
    public OperationResult Delete(string folderpath)
    {
      return SecureAction(() => FileSystem.DeleteFolder(folderpath));
    }
  }
}