using Vfs.Transfer;

namespace Vfs.Restful.Server.Handlers.Upload
{
  public class ReloadUploadTokenHandler : UploadHandler
  {
    public ReloadUploadTokenHandler(IFileSystemProvider fileSystem) : base(fileSystem)
    {
    }

    /// <summary>
    /// Requeries a previously issued token. Can be used if the client only stores
    /// <see cref="TransferToken.TransferId"/> values rather than the tokens and
    /// needs to get ahold of them again.
    /// </summary>
    /// <param name="transferId">The <see cref="TransferToken.TransferId"/>
    /// of the requested token.</param>
    /// <exception cref="UnknownTransferException">In case the <paramref name="transferId"/>
    /// does not refer to an active transfer.</exception>
    public OperationResult<UploadToken> Get(string transferId)
    {
      return SecureFunc(() => FileSystem.UploadTransfers.ReloadToken(transferId));
    }
  }
}