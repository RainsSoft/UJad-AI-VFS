using OpenRasta.Web;

namespace Vfs.Restful.Server.Handlers.Browse
{
  public class GetFolderParentHandler : BrowsingHandler
  {
    public GetFolderParentHandler(IFileSystemProvider fileSystem) : base(fileSystem)
    {
    }

    [HttpOperation(HttpMethod.GET, ForUriName = "GetFolderParent")]
    public OperationResult<VirtualFolderInfo> Get(string folderPath)
    {
      return SecureFunc(() => FileSystem.GetFolderParent(folderPath));
    }  
  }
}