using OpenRasta.Web;

namespace Vfs.Restful.Server.Handlers.Browse
{
  public class GetCreateOrDeleteFolderHandler : BrowsingHandler
  {
    public GetCreateOrDeleteFolderHandler(IFileSystemProvider fileSystem) : base(fileSystem)
    {
    }

    //[HttpOperation(HttpMethod.GET, ForUriName = "GetFolderInfo")]
    public OperationResult<VirtualFolderInfo> Get(string folderpath)
    {
      return SecureFunc(() => FileSystem.GetFolderInfo(folderpath));
    }

    //create
    public OperationResult<VirtualFolderInfo> Post(string folderpath)
    {
      return SecureFunc(() => FileSystem.CreateFolder(folderpath));
    }

    //delete
    public OperationResult Delete(string folderpath)
    {
      return SecureAction(() => FileSystem.DeleteFolder(folderpath));
    }
  }
}