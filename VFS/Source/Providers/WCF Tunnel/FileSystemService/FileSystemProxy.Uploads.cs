using System;
using System.IO;
using System.ServiceModel.Web;
using Vfs.Auditing;
using Vfs.FileSystemService.Faults;
using Vfs.Transfer;

namespace Vfs.FileSystemService
{
  public partial class FileSystemProxy
  {
    /// <summary>
    /// Indicates how restrictively data blocks may be transmitted.
    /// </summary>
    TransmissionCapabilities IFSWriterService.GetTransmissionCapabilities()
    {
      return FaultUtil.SecureFunc(FileSystemTask.ProviderMetaDataRequest,
                                  () => Decorated.UploadTransfers.TransmissionCapabilities);
    }

    /// <summary>
    /// Gets the maximum block size that can be transferred, if such
    /// a limit exists. Returns null in case of arbitrary block sizes.
    /// </summary>
    int? IFSWriterService.GetMaxBlockSize()
    {
      return FaultUtil.SecureFunc(FileSystemTask.ProviderMetaDataRequest,
                                  () => Decorated.UploadTransfers.MaxBlockSize);
    }


    /// <summary>
    /// Requests an upload token for a given file resource.
    /// </summary>
    /// <param name="virtualFilePath">Identifies the resource to be downloaded.</param>
    /// <param name="overwriteExistingResource">Whether an already existing file should be overwritten
    /// or not. If this parameter is false and the file already exists, a <see cref="ResourceOverwriteException"/>
    /// is thrown.</param>
    /// <param name="fileLength">The length of the resource to be uploaded in bytes.</param>
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
    public UploadToken RequestUploadToken(string virtualFilePath, bool overwriteExistingResource, long fileLength,
                                      string contentType)
    {
      Func<UploadToken> func = () => Decorated.UploadTransfers.RequestUploadToken(virtualFilePath,
                                                                                  overwriteExistingResource,
                                                                                  fileLength,
                                                                                  contentType);

      return FaultUtil.SecureFunc(FileSystemTask.UploadTokenRequest, func);
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
    UploadToken IFSWriterService.ReloadToken(string transferId)
    {
      return FaultUtil.SecureFunc(FileSystemTask.UploadTokenRequery,
                                  () => Decorated.UploadTransfers.ReloadToken(transferId));
    }


    /// <summary>
    /// Gets the server-side status of the transfer.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <returns>The status of the requested transfer. If the transfer is unknown,
    /// this does not cause an exception, but merely results in a return value
    /// of <see cref="TransferStatus.UnknownTransfer"/>.</returns>
    TransferStatus IFSWriterService.GetTransferStatus(string transferId)
    {
      return FaultUtil.SecureFunc(FileSystemTask.UploadTransferStatusRequest,
                                  () => Decorated.UploadTransfers.GetTransferStatus(transferId));
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
    void IFSWriterService.PauseTransfer(string transferId)
    {
      FaultUtil.SecureAction(FileSystemTask.UploadTransferPauseRequest,
                                  () => Decorated.UploadTransfers.PauseTransfer(transferId));
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
    TransferStatus IFSWriterService.CompleteTransfer(string transferId)
    {
      return FaultUtil.SecureFunc(FileSystemTask.UploadTransferCompletion,
                                  () => Decorated.UploadTransfers.CompleteTransfer(transferId));
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
    TransferStatus IFSWriterService.CancelTransfer(string transferId, AbortReason reason)
    {
      return FaultUtil.SecureFunc(FileSystemTask.UploadTransferCanceling,
                                  () => Decorated.UploadTransfers.CancelTransfer(transferId, reason));
    }

    /// <summary>
    /// Gets the maximum size of an uploaded file. A value of
    /// null indicates no limit is in place.
    /// </summary>
    long? IFSWriterService.GetMaxFileUploadSize()
    {
      return FaultUtil.SecureFunc(FileSystemTask.ProviderMetaDataRequest,
                                  () => Decorated.UploadTransfers.GetMaxFileUploadSize());
    }


    /// <summary>
    /// Gets the token that represents a running transfer for a given resource,
    /// e.g. for aborting running transfers before deleting or modifying a resource.
    /// </summary>
    /// <param name="virtualFilePath">The identifier of the resource.</param>
    /// <returns>The token that is being maintained for the resource.</returns>
    /// <remarks>This method needs to be secured in remote scenarios (or not be made available
    /// at all). Otherwise, if allows for the hijacking a running upload.</remarks>
    UploadToken IFSWriterService.GetTransferForResource(string virtualFilePath)
    {
      return FaultUtil.SecureFunc(FileSystemTask.UploadTransferByResourceQuery,
                                  () => Decorated.UploadTransfers.GetTransferForResource(virtualFilePath));
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
    TransferStatus IFSWriterService.CompleteTransfer(string transferId, string md5FileHash)
    {
      return FaultUtil.SecureFunc(FileSystemTask.UploadTransferCompletion,
                                  () => Decorated.UploadTransfers.CompleteTransfer(transferId, md5FileHash));
    }




    /// <summary>
    /// Creates or updates a given file resource in the file system.
    /// </summary>
    /// <param name="writeContract">Provides required information about the
    /// file to be created along with the data to be written.</param>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the parent folder
    /// of the submitted file path does not exist.</exception>
    /// <exception cref="ResourceOverwriteException">If a file already exists at the
    /// specified location, and the <see cref="WriteFileDataContract.Overwrite"/> flag was not set.</exception>
    /// <exception cref="ArgumentNullException">If any of the parameters is a null reference.</exception>
    public FileInfoDataContract WriteFile(WriteFileDataContract writeContract)
    {
      var fileInfo = FaultUtil.SecureFunc(FileSystemTask.StreamedFileUploadRequest, () => Decorated.WriteFile(
                                                                                            writeContract.FilePath,
                                                                                            writeContract.Data,
                                                                                            writeContract.Overwrite,
                                                                                            writeContract.ResourceLength,
                                                                                            writeContract.ContentType));
      return new FileInfoDataContract { FileInfo = fileInfo };
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
    void IFSDataUploadService.WriteDataBlock(BufferedDataBlock block)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Uploads a given data block that provides a chunk of data for an uploaded file as a stream.
    /// </summary>
    /// <param name="block">The block to be written.</param>
    /// <exception cref="DataBlockException">If the data block's contents cannot be stored,
    /// either because it's an invalid number, or because only sequential downloads
    /// are possible, and the block does not refer to the current download
    /// position.
    /// </exception>
    /// <exception cref="TransferStatusException">If the transfer has already expired.</exception>
    void IFSDataUploadService.WriteDataBlockStreamed(StreamedDataBlock block)
    {
      throw new NotImplementedException();
    }

    void IFSDataUploadService.WriteDataBlock(string transferId, int blockNumber, long offset, byte[] data)
    {
      throw new NotImplementedException();
    }

    void IFSDataUploadService.WriteDataBlockStreamed(string transferId, int blockNumber, long offset, long blockLength, Stream data)
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
    void IFSDataUploadService.WriteFile(string virtualFilePath, bool overwrite, long resourceLength, string contentType, Stream input)
    {
      throw new NotImplementedException();
    }

    public string WriteFile(string virtualFilePath, bool overwrite, long resourceLength, string contentType, Stream data)
    {
      //resolve content type for web operations
      var context = WebOperationContext.Current;
      if (context == null)
      {
        throw new VirtualResourceNotFoundException("Missing virtual resource name in header.");
      }

      var fileItem = Decorated.WriteFile(virtualFilePath, data, overwrite, resourceLength, contentType);
      //TODO what's with the return value?
      return fileItem.FullName;
    }


    public void WriteDataBlock(string transferId, int blockNumber, int blockLength, long offset, byte[] data)
    {
      Action action = () =>
      {
        BufferedDataBlock block = new BufferedDataBlock
        {
          TransferTokenId = transferId,
          BlockNumber = blockNumber,
          BlockLength = blockLength,
          Offset = offset,
          Data = data
        };

        Decorated.UploadTransfers.WriteBlock(block);
      };

      FaultUtil.SecureAction(FileSystemTask.DataBlockUploadRequest, action);
    }
  }
}