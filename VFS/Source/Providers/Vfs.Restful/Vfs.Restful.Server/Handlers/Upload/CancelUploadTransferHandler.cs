using OpenRasta.Web;
using Vfs.Transfer;

namespace Vfs.Restful.Server.Handlers.Upload
{
  public class CancelUploadTransferHandler : UploadHandler
  {
    public CancelUploadTransferHandler(IFileSystemProvider fileSystem)
      : base(fileSystem)
    {
    }

    /// <summary>
    /// Aborts a currently managed resource transfer.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <param name="reason">The reason to cancel the transfer.</param>
    /// <returns>The new status of the transfer, which is <see cref="TransferStatus.Aborted"/>
    /// in case of a known transfer. If the transfer is not known (maybe because it was aborted
    /// by the system), this method returns <see cref="TransferStatus.UnknownTransfer"/>.</returns>
    /// <exception cref="UnknownTransferException">In case the <paramref name="transferId"/>
    /// does not refer to an active transfer.</exception>
    [HttpOperation(HttpMethod.POST, ForUriName = "CancelUploadTransfer")]
    public virtual OperationResult<Wrapped<TransferStatus>> Post(string transferId, AbortReason reason)
    {
      return SecureFunc(() => new Wrapped<TransferStatus>(Uploads.CancelTransfer(transferId, reason)));
    }
  }
}