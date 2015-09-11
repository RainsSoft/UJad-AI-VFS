using OpenRasta.Web;
using Vfs.Transfer;

namespace Vfs.Restful.Server.Handlers.Upload
{
  public class GetMaxUploadBlockSizeHandler : MaxBlockSizeHandler<UploadToken>
  {
    public GetMaxUploadBlockSizeHandler(IFileSystemProvider fileSystem) : base(fileSystem)
    {
    }

    /// <summary>
    /// Gets the maximum block size that can be transferred, if such
    /// a limit exists. Returns null in case of arbitrary block sizes.
    /// </summary>
    [HttpOperation(HttpMethod.GET, ForUriName = "GetMaxUploadBlockSize")]
    public OperationResult<Wrapped<int?>> Get()
    {
      return SecureFunc(() => GetMaxBlockSize(FileSystem.UploadTransfers));
    }


    /// <summary>
    /// Gets the maximum block size for this kind of transfers.
    /// </summary>
    protected override int? GetSettingsMaxBlockSize(VfsServiceSettings settings)
    {
      return settings.MaxUploadBlockSize;
    }

  }
}
