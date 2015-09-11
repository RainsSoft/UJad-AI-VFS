using System;
using System.Collections.Generic;
using System.IO;
using Vfs.Auditing;
using Vfs.FileSystemService;
using Vfs.Transfer;

namespace Vfs.FileSystemServiceClient
{
  public class UploadTransferFacade : TransferFacade, IUploadTransferHandler
  {
    public IFSWriterService WriterService { get; set; }


    #region common transfer methods

    /// <summary>
    /// Indicates how restrictively data blocks may be transmitted.
    /// </summary>
    protected override TransmissionCapabilities GetTransmissionCapabilitiesImpl()
    {
      return WriterService.GetTransmissionCapabilities();
    }

    /// <summary>
    /// Gets the maximum block size that can be transferred, if such
    /// a limit exists. Returns null in case of arbitrary block sizes.
    /// </summary>
    protected override int? GetMaxBlockSizeImpl()
    {
      return WriterService.GetMaxBlockSize();
    }

    /// <summary>
    /// Gets the server-side status of the transfer.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <returns>The status of the requested transfer. If the transfer is unknown,
    /// this does not cause an exception, but merely results in a return value
    /// of <see cref="TransferStatus.UnknownTransfer"/>.</returns>
    protected override TransferStatus GetTransferStatusImpl(string transferId)
    {
      return WriterService.GetTransferStatus(transferId);
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
    public override void PauseTransferImpl(string transferId)
    {
      WriterService.PauseTransfer(transferId);
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
    protected override TransferStatus CompleteTransferImpl(string transferId)
    {
      return WriterService.CompleteTransfer(transferId);
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
    protected override TransferStatus CancelTransferImpl(string transferId, AbortReason reason)
    {
      return WriterService.CancelTransfer(transferId, reason);
    }

    #endregion

    /// <summary>
    /// Gets the maximum size of an uploaded file. A value of
    /// null indicates no limit is in place.
    /// </summary>
    public long? GetMaxFileUploadSize()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Requests an upload token for a given file resource.
    /// </summary>
    /// <param name="virtualFilePath">Identifies the resource to be downloaded.</param>
    /// <param name="overwriteExistingResource">Whether an already existing file should be overwritten
    /// or not. If this parameter is false and the file already exists, a <see cref="ResourceOverwriteException"/>
    /// is thrown.</param>
    /// <param name="resourceLength">The length of the resource to be uploaded in bytes.</param>
    /// <param name="contentType">The content type of the uploaded resource.</param>
    /// <returns>A token that represents a granted resource download, optionally
    /// limited to a given time frame (<see cref="TransferToken.ExpirationTime"/>).</returns>
    /// <exception cref="ResourceOverwriteException">If such a file already exists, and the
    /// <paramref name="overwriteExistingResource"/> parameter was false.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the submitted <paramref name="virtualFilePath"/>
    /// does not match an existing resource.</exception>
    /// <exception cref="ResourceAccessException">If the request was not authorized.</exception>
    /// <exception cref="ResourceLockedException">If a lock to access the
    /// resource was not granted.</exception>
    public UploadToken RequestUploadToken(string virtualFilePath, bool overwriteExistingResource, long resourceLength, string contentType)
    {
      throw new NotImplementedException();
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
    public UploadToken ReloadToken(string transferId)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Gets all running transfers for a given resource, e.g. for aborting running
    /// transfers before deleting or modifying a resource.
    /// </summary>
    /// <param name="virtualFilePath">The identifier of the resource.</param>
    /// <returns>The token that is being maintained for the resource.</returns>
    /// <remarks>This method needs to be secured in remote scenarios (or not be made available
    /// at all). Otherwise, if allows for the hijacking a running upload.</remarks>
    public UploadToken GetTransferForResource(string virtualFilePath)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Uploads a given data block that contains a chunk of data for an uploaded file.
    /// </summary>
    /// <param name="block">The block to be written.</param>
    /// <exception cref="DataBlockException">If the data block's contents cannot be stored,
    /// either because it's an invalid number, or because only sequential downloads
    /// are possible, and the block does not refer to the current download
    /// position.
    /// </exception>
    /// <exception cref="TransferStatusException">If the transfer has already expired.</exception>
    public void WriteBlock(BufferedDataBlock block)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    public void WriteBlockStreamed(StreamedDataBlock block)
    {
      throw new NotImplementedException();
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
    /// <param name="md5FileHash">An MD5 file hash that should match the uploaded file.</param>
    /// <returns>The new status of the transfer, which is <see cref="TransferStatus.Completed"/>
    /// in case of a known transfer. If the transfer is not known (maybe because it was aborted
    /// by the system), this method returns <see cref="TransferStatus.UnknownTransfer"/>.</returns>
    /// <exception cref="UnknownTransferException">If the submitted <paramref name="transferId"/>
    /// cannot be mapped to a running transfer.</exception>
    /// <exception cref="IntegrityCheckException">If the integrity check based on the submitted
    /// file hash failed.</exception>
    public TransferStatus CompleteTransfer(string transferId, string md5FileHash)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Creates or updates a given file resource in the file system in one blocking operation.
    /// </summary>
    /// <param name="virtualFilePath">The qualified path of the file to be created.</param>
    /// <param name="input">A stream that provides the file's contents.</param>
    /// <param name="overwrite">Whether an existing file should be overwritten
    /// or not. If this parameter is false and the file already exists, a
    /// <see cref="ResourceOverwriteException"/> is thrown.</param>
    /// <param name="resourceLength">The length of the resource to be uploaded in bytes.</param>
    /// <param name="contentType">The content type of the uploaded resource.</param>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    /// <exception cref="ResourceOverwriteException">If a file already exists at the
    /// specified location, and the <paramref name="overwrite"/> flag was not set.</exception>
    /// <exception cref="ArgumentNullException">If any of the parameters is a null reference.</exception>
    public void WriteFile(string virtualFilePath, Stream input, bool overwrite, long resourceLength, string contentType)
    {
      throw new NotImplementedException();
    }


  }
}