using OpenRasta.Web;
using Vfs.Transfer;

namespace Vfs.Restful.Server.Handlers.Download
{
  public class GetMaxDownloadBlockSizeHandler : MaxBlockSizeHandler<DownloadToken>
  {
    public GetMaxDownloadBlockSizeHandler(IFileSystemProvider fileSystem) : base(fileSystem)
    {
    }

    /// <summary>
    /// Gets the maximum block size that can be transferred, if such
    /// a limit exists. Returns null in case of arbitrary block sizes.
    /// </summary>
    [HttpOperation(HttpMethod.GET, ForUriName = "GetMaxDownloadBlockSize")]
    public virtual OperationResult<Wrapped<int?>> Get()
    {
      return SecureFunc(() => GetMaxBlockSize(FileSystem.DownloadTransfers));
    }

    /// <summary>
    /// Gets the maximum block size for this kind of transfers.
    /// </summary>
    protected override int? GetSettingsMaxBlockSize(VfsServiceSettings settings)
    {
      return settings.MaxDownloadBlockSize;
    }
  }
}
