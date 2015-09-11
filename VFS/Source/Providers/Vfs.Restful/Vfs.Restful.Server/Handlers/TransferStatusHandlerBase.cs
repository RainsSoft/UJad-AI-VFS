using System;
using OpenRasta.Web;
using Vfs.Transfer;

namespace Vfs.Restful.Server.Handlers
{
  /// <summary>
  /// Implements functionality to implement both upload 
  /// and download transfer services.
  /// </summary>
  public abstract class TransferStatusHandlerBase<T> : VfsHandlerBase
  {
    /// <summary>
    /// Gets the actual <see cref="ITransferHandler{TToken}"/> that
    /// is being used to send status requests.
    /// </summary>
    public abstract ITransferHandler<T> TransferHandler { get; }

    /// <summary>
    /// The service settings, which are being injected automatically
    /// via dependency injection, if available.
    /// </summary>
    public VfsServiceSettings Settings { get; set; }


    /// <summary>
    /// Initializes the handler class with the file system
    /// provider that receives incoming requests.
    /// </summary>
    protected TransferStatusHandlerBase(IFileSystemProvider fileSystem) : base(fileSystem)
    {
     
    }


    /// <summary>
    /// Indicates how restrictively data blocks may be transmitted.
    /// </summary>
    [HttpOperation(HttpMethod.GET, ForUriName = "GetTransmissionCapabilities")]
    public virtual TransmissionCapabilities GetTransmissionCapabilities()
    {
      return TransferHandler.TransmissionCapabilities;
    }


    /// <summary>
    /// Gets the maximum block size that can be transferred, if such
    /// a limit exists. Returns null in case of arbitrary block sizes.
    /// </summary>
    [HttpOperation(HttpMethod.GET, ForUriName = "GetMaxBlockSize")]
    public virtual Wrapped<int?> GetMaxBlockSize()
    {
      //get the minimum of the configured block size and the file system block size
      //(which are both optional)
      int? settingsValue = Settings == null ? null : GetSettingsMaxBlockSize(Settings);
      if (settingsValue.HasValue)
      {
        int blockSize = Math.Min(settingsValue.Value, TransferHandler.MaxBlockSize ?? int.MaxValue);
        return new Wrapped<int?>(blockSize);
      }

      return new Wrapped<int?>(TransferHandler.MaxBlockSize);
    }


    /// <summary>
    /// Gets the maximum block size for this kind of transfers.
    /// </summary>
    protected abstract int? GetSettingsMaxBlockSize(VfsServiceSettings settings);

    

    /// <summary>
    /// Gets the server-side status of the transfer.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <returns>The status of the requested transfer. If the transfer is unknown,
    /// this does not cause an exception, but merely results in a return value
    /// of <see cref="TransferStatus.UnknownTransfer"/>.</returns>
    [HttpOperation(HttpMethod.GET, ForUriName = "GetTransferStatus")]
    public virtual TransferStatus GetTransferStatus(string transferId)
    {
      return TransferHandler.GetTransferStatus(transferId);
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
    [HttpOperation(HttpMethod.GET, ForUriName = "ReloadToken")]
    public virtual T ReloadToken(string transferId)
    {
      return TransferHandler.ReloadToken(transferId);
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
    [HttpOperation(HttpMethod.POST, ForUriName = "PauseTransfer")]
    public virtual OperationResult PauseTransfer(string transferId)
    {
      TransferHandler.PauseTransfer(transferId);
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
    [HttpOperation(HttpMethod.POST, ForUriName = "CompleteTransfer")]
    public virtual TransferStatus CompleteTransfer(string transferId)
    {
      return TransferHandler.CompleteTransfer(transferId);
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
    [HttpOperation(HttpMethod.POST, ForUriName = "CancelTransfer")]
    public virtual TransferStatus CancelTransfer(string transferId, AbortReason reason)
    {
      return TransferHandler.CancelTransfer(transferId, reason);
    }

  }
}
