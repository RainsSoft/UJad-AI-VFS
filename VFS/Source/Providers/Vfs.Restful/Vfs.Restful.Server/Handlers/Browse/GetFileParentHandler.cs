using OpenRasta.Web;

namespace Vfs.Restful.Server.Handlers.Browse
{
  public class GetFileParentHandler : BrowsingHandler
  {
    public GetFileParentHandler(IFileSystemProvider fileSystem)
      : base(fileSystem)
    {
    }


    [HttpOperation(HttpMethod.POST, ForUriName = "GetFileParent")]
    public OperationResult<VirtualFolderInfo> Get(string filePath)
    {
      return SecureFunc(() => FileSystem.GetFileParent(filePath));
    }
  }
}