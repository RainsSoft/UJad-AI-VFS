using OpenRasta.Web;
using Vfs.Transfer;

namespace Vfs.Restful.Server.Handlers
{
  public class UploadStatusHandler : TransferStatusHandlerBase<UploadToken>
  {
    /*
     * THIS CLASS OVERRIDES THE BASE CLASS METHODS IN ORDER TO ADD CUSTOM
     * ATTRIBUTES -> THE URI NAMES ARE DIFFERENT.
     */


    /// <summary>
    /// Initializes the handler class with the file system
    /// provider that receives incoming requests.
    /// </summary>
    public UploadStatusHandler(IFileSystemProvider fileSystem) : base(fileSystem)
    {
    }


    /// <summary>
    /// Gets the <see cref="IFileSystemProvider.UploadTransfers"/>
    /// of the current <see cref="VfsHandlerBase.FileSystem"/>.
    /// </summary>
    public override ITransferHandler<UploadToken> TransferHandler
    {
      get { return FileSystem.UploadTransfers; }
    }

    /// <summary>
    /// Gets the maximum block size for this kind of transfers.
    /// </summary>
    protected override int? GetSettingsMaxBlockSize(VfsServiceSettings settings)
    {
      return settings.MaxUploadBlockSize;
    }


    /// <summary>
    /// Indicates how restrictively data blocks may be transmitted.
    /// </summary>
    [HttpOperation(HttpMethod.GET, ForUriName = "GetUploadTransmissionCapabilities")]
    public override TransmissionCapabilities GetTransmissionCapabilities()
    {
      return base.GetTransmissionCapabilities();
    }


    /// <summary>
    /// Gets the maximum block size that can be transferred, if such
    /// a limit exists. Returns null in case of arbitrary block sizes.
    /// </summary>
    [HttpOperation(HttpMethod.GET, ForUriName = "GetMaxUploadBlockSize")]
    public override Wrapped<int?> GetMaxBlockSize()
    {
      return base.GetMaxBlockSize();
    }




    /// <summary>
    /// Gets the server-side status of the transfer.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <returns>The status of the requested transfer. If the transfer is unknown,
    /// this does not cause an exception, but merely results in a return value
    /// of <see cref="TransferStatus.UnknownTransfer"/>.</returns>
    [HttpOperation(HttpMethod.GET, ForUriName = "GetUploadTransferStatus")]
    public override TransferStatus GetTransferStatus(string transferId)
    {
      return base.GetTransferStatus(transferId);
    }

    /// <summary>
    /// Requeries a previously issued token. Can be used if the client only stores
    /// <see cref="TransferToken.TransferId"/> values rather than the tokens and
    /// needs to get ahold of them again.
    /// </summary>
    /// <param name="transferId"></param>
    /// <returns></returns>
    /// <exception cref="UnknownTransferException">In case the <paramref name="transferId"/>
    /// does not refer to an active transfer.</exception>
    [HttpOperation(HttpMethod.GET, ForUriName = "ReloadUploadToken")]
    public override UploadToken ReloadToken(string transferId)
    {
      return base.ReloadToken(transferId);
    } 


    /// <summary>
    /// Tells the transfer service that transmission is being
    /// paused for an unknown period of time. This should keep
    /// the transfer enabled (including lock to protect the resource),
    /// but gives the service time to free or unlock resources.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <exception cref="UnknownTransferException">In case the <paramref name="transferId"/>
    /// does not refer to an active transfer.</exception>
    [HttpOperation(HttpMethod.POST, ForUriName = "PauseUploadTransfer")]
    public override OperationResult PauseTransfer(string transferId)
    {
      return base.PauseTransfer(transferId);
    }



    /// <summary>
    /// Completes a given file transfer including a verification of the uploaded
    /// data. Invoking this operation closes and removes the transfer from the service. It is highly
    /// recommended to invoke this method after finishing a transfer
    /// in order to free used/locked resources as soon as possible. As an alternative,
    /// the uploading party my set the <see cref="DataBlockInfo.IsLastBlock"/> property of the
    /// last transmitted block to true in order to have the transfer implicitly closed.
    /// </summary>
    /// <param name="transferId">Identifies the current transfer according to the
    /// <see cref="TransferToken.TransferId"/> that was issued.</param>
    /// <param name="fileHash">An MD5 file hash that should match the uploaded file.</param>
    /// <returns>The new status of the transfer, which is <see cref="TransferStatus.Completed"/>
    /// in case of a known transfer. If the transfer is not known (maybe because it was aborted
    /// by the system), this method returns <see cref="TransferStatus.UnknownTransfer"/>.</returns>
    /// <exception cref="UnknownTransferException">If the submitted <paramref name="transferId"/>
    /// cannot be mapped to a running transfer.</exception>
    /// <exception cref="IntegrityCheckException">If the integrity check based on the submitted
    /// file hash failed.</exception>
    [HttpOperation(HttpMethod.POST, ForUriName = "CompleteTransferWithVerification")]
    public OperationResult CompleteTransfer(string transferId, string fileHash)
    {
      FileSystem.UploadTransfers.CompleteTransfer(transferId, fileHash);
      return new OperationResult.OK();
    }


    /// <summary>
    /// Completes a given file transfer - invoking this operation
    /// closes and removes the transfer from the service. It is highly
    /// recommended to invoke this method after finishing a transfer
    /// in order to free used/locked resources as soon as possible.<br/>
    /// In case of an upload, setting the <see cref="DataBlockInfo.IsLastBlock"/>
    /// property of the last submitted data block implicitly calls this method.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <returns>The new status of the transfer, which is <see cref="TransferStatus.Completed"/>
    /// in case of a known transfer. If the transfer is not known (maybe because it was aborted
    /// by the system), this method returns <see cref="TransferStatus.UnknownTransfer"/>.</returns>
    /// <exception cref="UnknownTransferException">In case the <paramref name="transferId"/>
    /// does not refer to an active transfer.</exception>
    [HttpOperation(HttpMethod.POST, ForUriName = "CompleteUploadTransfer")]
    public override TransferStatus CompleteTransfer(string transferId)
    {
      return base.CompleteTransfer(transferId);
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
    public override TransferStatus CancelTransfer(string transferId, AbortReason reason)
    {
      return base.CancelTransfer(transferId, reason);
    }
  }
}
