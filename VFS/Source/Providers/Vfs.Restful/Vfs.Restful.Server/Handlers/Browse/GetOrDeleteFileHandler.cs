using OpenRasta.Web;

namespace Vfs.Restful.Server.Handlers.Browse
{
  public class GetOrDeleteFileHandler : BrowsingHandler
  {
    public GetOrDeleteFileHandler(IFileSystemProvider fileSystem) : base(fileSystem)
    {
    }


    public OperationResult<VirtualFileInfo> Get(string filePath)
    {
      return SecureFunc(() => FileSystem.GetFileInfo(filePath));
    }


    public OperationResult Delete(string filepath)
    {
      return SecureAction(() => FileSystem.DeleteFile(filepath));
    }
  }
}