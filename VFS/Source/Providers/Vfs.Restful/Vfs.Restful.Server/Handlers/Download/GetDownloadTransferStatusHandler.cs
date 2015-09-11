using OpenRasta.Web;
using Vfs.Transfer;

namespace Vfs.Restful.Server.Handlers.Download
{
  public class GetDownloadTransferStatusHandler : DownloadHandler
  {
    public GetDownloadTransferStatusHandler(IFileSystemProvider fileSystem) : base(fileSystem)
    {
    }

    /// <summary>
    /// Gets the server-side status of the transfer.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <returns>The status of the requested transfer. If the transfer is unknown,
    /// this does not cause an exception, but merely results in a return value
    /// of <see cref="TransferStatus.UnknownTransfer"/>.</returns>
    [HttpOperation(HttpMethod.GET, ForUriName = "GetDownloadTransferStatus")]
    public OperationResult<Wrapped<TransferStatus>> Get(string transferId)
    {
      return SecureFunc(() => new Wrapped<TransferStatus>(Downloads.GetTransferStatus(transferId)));
    }
  }
}