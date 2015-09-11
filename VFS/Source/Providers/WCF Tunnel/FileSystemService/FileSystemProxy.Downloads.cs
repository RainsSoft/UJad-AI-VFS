using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel.Web;
using Vfs.Auditing;
using Vfs.FileSystemService.Faults;
using Vfs.Transfer;
using Vfs.Util;

namespace Vfs.FileSystemService
{
  public partial class FileSystemProxy
  {
    /// <summary>
    /// Indicates how restrictively data blocks may be transmitted.
    /// </summary>
    TransmissionCapabilities IFSReaderService.GetTransmissionCapabilities()
    {
      return FaultUtil.SecureFunc(FileSystemTask.ProviderMetaDataRequest,
                                  () => Decorated.DownloadTransfers.TransmissionCapabilities);
    }

    /// <summary>
    /// Gets the maximum block size that can be transferred, if such
    /// a limit exists. Returns null in case of arbitrary block sizes.
    /// </summary>
    int? IFSReaderService.GetMaxBlockSize()
    {
      return FaultUtil.SecureFunc(FileSystemTask.ProviderMetaDataRequest,
                                  () => Decorated.DownloadTransfers.MaxBlockSize);
    }


    /// <summary>
    /// Requests a download token for a given file.
    /// </summary>
    /// <param name="virtualFilePath">Identifies the file resource to be downloaded.</param>
    /// <param name="includeFileHash">Whether a file hash for the
    /// requested resource should be calculated and assigned to the
    /// <see cref="DownloadToken.Md5FileHash"/> property of the returned
    /// <see cref="DownloadToken"/>.</param>
    /// <returns>A token that represents a granted resource download, optionally
    /// limited to a given time frame (<see cref="TransferToken.ExpirationTime"/>).</returns>
    /// <exception cref="VirtualResourceNotFoundException">If the submitted <paramref name="virtualFilePath"/>
    /// does not match an existing resource.</exception>
    /// <exception cref="ResourceAccessException">If the request was not authorized.</exception>
    /// <exception cref="ResourceLockedException">If a lock to access the
    /// resource was not granted.</exception>
    public DownloadToken RequestDownloadToken(string virtualFilePath, bool includeFileHash)
    {
      return FaultUtil.SecureFunc(FileSystemTask.DownloadTokenRequest,
                                  () =>
                                  Decorated.DownloadTransfers.RequestDownloadToken(virtualFilePath, includeFileHash));
    }


    /// <summary>
    /// Requests a download token for a given resource.
    /// </summary>
    /// <param name="virtualFilePath">Identifies the resource to be downloaded.</param>
    /// <param name="maxBlockSize">The maximum size of a read block. This value must be
    /// equal or lower to the <see cref="IFSReaderService.GetMaxBlockSize"/>, if there is an
    /// upper limit for blocks.</param>
    /// <param name="includeFileHash">Whether a file hash for the
    /// requested resource should be calculated and assigned to the
    /// <see cref="DownloadToken.Md5FileHash"/> property of the returned
    /// <see cref="DownloadToken"/>.</param>
    /// <returns>A token that represents a granted resource download, optionally
    /// limited to a given time frame (<see cref="TransferToken.ExpirationTime"/>).</returns>
    /// <exception cref="VirtualResourceNotFoundException">If the submitted <paramref name="virtualFilePath"/>
    /// does not match an existing resource.</exception>
    /// <exception cref="ResourceAccessException">If the request was not authorized.</exception>
    /// <exception cref="ResourceLockedException">If a lock to access the
    /// resource was not granted.</exception>
    public DownloadToken RequestDownloadTokenWithBlockSize(string virtualFilePath, int maxBlockSize, bool includeFileHash)
    {
      return FaultUtil.SecureFunc(FileSystemTask.DownloadTokenRequest,
                                  () => Decorated.DownloadTransfers.RequestDownloadToken(virtualFilePath, includeFileHash, maxBlockSize));
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
    DownloadToken IFSReaderService.ReloadToken(string transferId)
    {
      return FaultUtil.SecureFunc(FileSystemTask.DownloadTokenRequery,
                                  () => Decorated.DownloadTransfers.ReloadToken(transferId));
    }


    /// <summary>
    /// Gets the server-side status of the transfer.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <returns>The status of the requested transfer. If the transfer is unknown,
    /// this does not cause an exception, but merely results in a return value
    /// of <see cref="TransferStatus.UnknownTransfer"/>.</returns>
    TransferStatus IFSReaderService.GetTransferStatus(string transferId)
    {
      return FaultUtil.SecureFunc(FileSystemTask.DownloadTransferStatusRequest,
                                  () => Decorated.DownloadTransfers.GetTransferStatus(transferId));
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
    void IFSReaderService.PauseTransfer(string transferId)
    {
      FaultUtil.SecureAction(FileSystemTask.DownloadTransferPauseRequest,
                                  () => Decorated.DownloadTransfers.PauseTransfer(transferId));
    }

    /// <summary>
    /// Completes a given file transfer - invoking this operation
    /// closes and removes the transfer from the service. It is highly
    /// recommended to invoke this method after finishing a transfer
    /// in order to free used/locked resources as soon as possible.<br/>
    /// In case of an Download, setting the <see cref="DataBlockInfo.IsLastBlock"/>
    /// property of the last submitted data block implicitly calls this method.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <returns>The new status of the transfer, which is <see cref="TransferStatus.Completed"/>
    /// in case of a known transfer. If the transfer is not known (maybe because it was aborted
    /// by the system), this method returns <see cref="TransferStatus.UnknownTransfer"/>.</returns>
    /// <exception cref="UnknownTransferException">In case the <paramref name="transferId"/>
    /// does not refer to an active transfer.</exception>
    TransferStatus IFSReaderService.CompleteTransfer(string transferId)
    {
      return FaultUtil.SecureFunc(FileSystemTask.DownloadTransferCompletion,
                                  () => Decorated.DownloadTransfers.CompleteTransfer(transferId));
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
    TransferStatus IFSReaderService.CancelTransfer(string transferId, AbortReason reason)
    {
      return FaultUtil.SecureFunc(FileSystemTask.DownloadTransferCanceling,
                                  () => Decorated.DownloadTransfers.CancelTransfer(transferId, reason));
    }



    /// <summary>
    /// Gets all running transfers for a given resource, e.g. for aborting running
    /// transfers before deleting or modifying a resource.
    /// </summary>
    /// <param name="virtualFilePath">The identifier of the resource.</param>
    public IEnumerable<DownloadToken> GetTransfersForResource(string virtualFilePath)
    {
      return FaultUtil.SecureFunc(FileSystemTask.DownloadTransfersByResourceQuery,
                                  () => Decorated.DownloadTransfers.GetTransfersForResource(virtualFilePath));
    }


    /// <summary>
    /// Gets the binary contents as a stream in a blocking operation.
    /// Use the methods in <see cref="ContentUtil"/> class for simplified stream
    /// handling.
    /// </summary>
    /// <param name="virtualFilePath">The path of the file to be read.</param>
    /// <returns>A stream that allows the contents of the file to be read.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="virtualFilePath"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the file that is represented
    /// by <paramref name="virtualFilePath"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    public Stream ReadFile(string virtualFilePath)
    {
      Func<Stream> func = () =>
      {
        //resolve content type for web operations
        var context = WebOperationContext.Current;
        if (context != null)
        {
          var fileInfo = Decorated.GetFileInfo(virtualFilePath);
          context.OutgoingResponse.ContentType = fileInfo.ContentType;
          var resourceName = String.Format("attachment; filename=\"{0}\"", fileInfo.Name);
          context.OutgoingResponse.Headers.Add("content-disposition", resourceName);
          context.OutgoingResponse.ContentLength = fileInfo.Length;
        }

        return Decorated.ReadFileContents(virtualFilePath);
      };


      return FaultUtil.SecureFunc(FileSystemTask.StreamedFileDownloadRequest, func);
    }





    /// <summary>
    /// Gets the binary contents of a resource as a stream in a blocking operation.
    /// Unlike the <see cref="IFSReaderService.ReadFileContents"/> method, this method expects the
    /// <see cref="TransferToken.TransferId"/> of a previously issued download token.
    /// Use the methods in <see cref="ContentUtil"/> class for simplified stream
    /// handling.
    /// </summary>
    /// <param name="transferId">The <see cref="TransferToken.TransferId"/> of a previously
    /// issued download token.</param>
    /// <returns>A stream that allows the contents of the file to be read.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="transferId"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the resource that is represented
    /// by the token is no longer available.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    /// <exception cref="TransferStatusException">In case the token is not or no longer
    /// valid.</exception>
    public Stream DownloadFile(string transferId)
    {
      Func<Stream> func = () =>
      {
        //resolve content type for web operations
        var context = WebOperationContext.Current;
        if (context != null)
        {
          var token = Decorated.DownloadTransfers.ReloadToken(transferId);
          context.OutgoingResponse.ContentType = token.ContentType;
          context.OutgoingResponse.ContentLength = token.ResourceLength;
          var resourceName = String.Format("attachment; filename=\"{0}\"", token.ResourceName);
          context.OutgoingResponse.Headers.Add("content-disposition", resourceName);
        }

        return Decorated.DownloadTransfers.DownloadFile(transferId);
      };


      return FaultUtil.SecureFunc(FileSystemTask.StreamedFileDownloadRequest, func);
    }


    public BufferedDataBlock ReadDataBlock(string transferId, long blockNumber)
    {
      return FaultUtil.SecureFunc(FileSystemTask.DataBlockDownloadRequest,
                                  () => Decorated.DownloadTransfers.ReadBlock(transferId, blockNumber));
    }

    public StreamedDataBlock ReadDataBlockStreamed(string transferId, long blockNumber)
    {
      return FaultUtil.SecureFunc(FileSystemTask.DataBlockDownloadRequest,
                                  () => Decorated.DownloadTransfers.ReadBlockStreamed(transferId, blockNumber));
    }

  }
}
