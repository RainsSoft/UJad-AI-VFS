using System;
using System.Collections.Generic;
using Vfs.Auditing;
using Vfs.Transfer;

namespace Vfs.FileSystemServiceClient
{
  /// <summary>
  /// Implements functionality to access both upload and download transfer services.
  /// </summary>
  public abstract class TransferFacade
  {

    private IAuditor auditor = new NullAuditor();

    /// <summary>
    /// Adds an auditor to file system provider, which receives
    /// auditing messages for file system requests and incidents.<br/>
    /// Assigning a null reference does not set the property to null,
    /// but falls back to a <see cref="NullAuditor"/> instead so
    /// this property never returns null but a
    /// valid <see cref="IAuditor"/> instance.
    /// </summary>
    public virtual IAuditor Auditor
    {
      get { return auditor; }
      set { auditor = value ?? new NullAuditor(); }
    }


    /// <summary>
    /// Indicates how restrictively data blocks may be transmitted.
    /// </summary>
    public TransmissionCapabilities TransmissionCapabilities
    {
      get 
      {
        return SecureFunc(FileSystemTask.ProviderMetaDataRequest,
                          () => GetTransmissionCapabilitiesImpl(),
                          () => "Could not get transmission capabilitites.");
      }
    }


    /// <summary>
    /// Indicates how restrictively data blocks may be transmitted.
    /// </summary>
    protected abstract TransmissionCapabilities GetTransmissionCapabilitiesImpl();



    /// <summary>
    /// Gets the maximum block size that can be transferred, if such
    /// a limit exists. Returns null in case of arbitrary block sizes.
    /// </summary>
    public int? MaxBlockSize
    {
      get
      {
        return SecureFunc(FileSystemTask.ProviderMetaDataRequest,
                          () => GetMaxBlockSizeImpl(),
                          () => "Could not get maximum block size.");
      }
    }

    /// <summary>
    /// Gets the maximum block size that can be transferred, if such
    /// a limit exists. Returns null in case of arbitrary block sizes.
    /// </summary>
    protected abstract int? GetMaxBlockSizeImpl();


    /// <summary>
    /// Gets the server-side status of the transfer.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <returns>The status of the requested transfer. If the transfer is unknown,
    /// this does not cause an exception, but merely results in a return value
    /// of <see cref="TransferStatus.UnknownTransfer"/>.</returns>
    public TransferStatus GetTransferStatus(string transferId)
    {
      return SecureFunc(FileSystemTask.ProviderMetaDataRequest,
                        () => GetTransferStatusImpl(transferId),
                        () => String.Format("Could not get transfer status of transfer [{0}].", transferId));
    }



    /// <summary>
    /// Gets the server-side status of the transfer.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <returns>The status of the requested transfer. If the transfer is unknown,
    /// this does not cause an exception, but merely results in a return value
    /// of <see cref="TransferStatus.UnknownTransfer"/>.</returns>
    protected abstract TransferStatus GetTransferStatusImpl(string transferId);


    /// <summary>
    /// Tells the transfer service that transmission is being
    /// paused for an unknown period of time. This should keep
    /// the transfer enabled (including lock to protect the resource),
    /// but gives the service time to free or unlock resources.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <exception cref="UnknownTransferException">In case the <paramref name="transferId"/>
    /// does not refer to an active transfer.</exception>
    public void PauseTransfer(string transferId)
    {
      SecureAction(FileSystemTask.ProviderMetaDataRequest,
                   () => PauseTransferImpl(transferId),
                   () => String.Format("Could not pause transfer [{0}].", transferId));
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
    public abstract void PauseTransferImpl(string transferId);



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
    public TransferStatus CompleteTransfer(string transferId)
    {
      return SecureFunc(FileSystemTask.ProviderMetaDataRequest,
                        () => CompleteTransferImpl(transferId),
                        () => String.Format("Could not get complete transfer [{0}].", transferId));
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
    protected abstract TransferStatus CompleteTransferImpl(string transferId);


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
    public TransferStatus CancelTransfer(string transferId, AbortReason reason)
    {
      return SecureFunc(FileSystemTask.ProviderMetaDataRequest,
                        () => CancelTransferImpl(transferId, reason),
                        () => String.Format("Could not cancel transfer [{0}].", transferId));
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
    protected abstract TransferStatus CancelTransferImpl(string transferId, AbortReason reason);


    /// <summary>
    /// Gets information about all blocks that have been transferred so far.
    /// This information can be used to resume a transfer that spawns several
    /// sessions on the client side.<br/>
    /// In case of retransmissions of blocks, this method returns only the
    /// <see cref="DataBlockInfo"/> instance per block number that corresponds
    /// to the most recent transfer.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <returns>All transferred blocks (without data).</returns>
    /// <exception cref="UnknownTransferException">In case no such transfer
    /// is currently maintained.</exception>
    public IEnumerable<DataBlockInfo> GetTransferredBlocks(string transferId)
    {
      throw new NotImplementedException(""); //TODO provide implementation
    }


    protected virtual T SecureFunc<T>(FileSystemTask task, Func<T> func, Func<string> errorMessage)
    {
      return Util.SecureFunc(Auditor, task, func, errorMessage);
    }


    protected virtual void SecureAction(FileSystemTask task, Action action, Func<string> errorMessage)
    {
      Util.SecureAction(Auditor, task, action, errorMessage);
    }
  }
}