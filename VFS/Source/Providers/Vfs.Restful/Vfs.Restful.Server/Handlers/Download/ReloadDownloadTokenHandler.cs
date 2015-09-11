using Vfs.Transfer;

namespace Vfs.Restful.Server.Handlers.Download
{
  public class ReloadDownloadTokenHandler : DownloadHandler
  {
    public ReloadDownloadTokenHandler(IFileSystemProvider fileSystem) : base(fileSystem)
    {
    }


    public OperationResult<DownloadToken> Get(string transferId)
    {
      return SecureFunc(() => Downloads.ReloadToken(transferId));
    }
  }
}