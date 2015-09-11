using OpenRasta.Web;
using Vfs.Transfer;

namespace Vfs.Restful.Server.Handlers.Upload
{
  public class GetUploadTransferStatusHandler : UploadHandler
  {
    public GetUploadTransferStatusHandler(IFileSystemProvider fileSystem)
      : base(fileSystem)
    {
    }

    /// <summary>
    /// Gets the server-side status of the transfer.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <returns>The status of the requested transfer. If the transfer is unknown,
    /// this does not cause an exception, but merely results in a return value
    /// of <see cref="TransferStatus.UnknownTransfer"/>.</returns>
    [HttpOperation(HttpMethod.GET, ForUriName = "GetUploadTransferStatus")]
    public OperationResult<Wrapped<TransferStatus>> Get(string transferId)
    {
      return SecureFunc(() => new Wrapped<TransferStatus>(Uploads.GetTransferStatus(transferId)));
    }
  }
}