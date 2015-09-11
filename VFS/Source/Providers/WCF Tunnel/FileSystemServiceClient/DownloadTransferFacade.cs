using System;
using System.Collections.Generic;
using System.IO;
using Vfs.Auditing;
using Vfs.FileSystemService;
using Vfs.Transfer;
using Vfs.Util;

namespace Vfs.FileSystemServiceClient
{
  public class DownloadTransferFacade : TransferFacade, IDownloadTransferHandler
  {
    /// <summary>
    /// A service proxy that provides access to the file system
    /// service's file download functionality.
    /// </summary>
    public IFSReaderService ReaderService { get; set; }


    /// <summary>
    /// A service proxy that provides access to the file system
    /// service's file download functionality.
    /// </summary>
    public IFSDataDownloadService DownloadService { get; set; }


    #region common transfer methods

    /// <summary>
    /// Indicates how restrictively data blocks may be transmitted.
    /// </summary>
    protected override TransmissionCapabilities GetTransmissionCapabilitiesImpl()
    {
      return ReaderService.GetTransmissionCapabilities();
    }

    /// <summary>
    /// Gets the maximum block size that can be transferred, if such
    /// a limit exists. Returns null in case of arbitrary block sizes.
    /// </summary>
    protected override int? GetMaxBlockSizeImpl()
    {
      return ReaderService.GetMaxBlockSize();
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
      return ReaderService.GetTransferStatus(transferId);
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
      ReaderService.PauseTransfer(transferId);
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
      return ReaderService.CompleteTransfer(transferId);
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
      return ReaderService.CancelTransfer(transferId, reason);
    }

    #endregion



    /// <summary>
    /// Requests a download token for a given resource.
    /// </summary>
    /// <param name="virtualFilePath">Identifies the resource to be downloaded.</param>
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
      return SecureFunc(FileSystemTask.DownloadTokenRequest,
                        () => ReaderService.RequestDownloadToken(virtualFilePath, includeFileHash),
                        () => String.Format("Download token request for file [{0}] failed.", virtualFilePath));
    }

    /// <summary>
    /// Requests a download token for a given resource.
    /// </summary>
    /// <param name="virtualFilePath">Identifies the resource to be downloaded.</param>
    /// <param name="maxBlockSize">The maximum size of a read block. This property must be
    /// equal or lower to the <see cref="ITransferHandler.MaxBlockSize"/>, if there is an
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
    public DownloadToken RequestDownloadToken(string virtualFilePath, bool includeFileHash, int maxBlockSize)
    {
      return SecureFunc(FileSystemTask.DownloadTokenRequest,
                        () => ReaderService.RequestDownloadTokenWithBlockSize(virtualFilePath, maxBlockSize, includeFileHash),
                        () => String.Format("Download token request for file [{0}] failed.", virtualFilePath));
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
    public DownloadToken ReloadToken(string transferId)
    {
      return SecureFunc(FileSystemTask.DownloadTokenRequery,
                        () => ReaderService.ReloadToken(transferId),
                        () => String.Format("Could not requery the download token for transfer [{0}].", transferId));
    }


    /// <summary>
    /// Gets all running transfers for a given resource, e.g. for aborting running
    /// transfers before deleting or modifying a resource.
    /// </summary>
    /// <param name="virtualFilePath">The identifier of the resource.</param>
    public IEnumerable<DownloadToken> GetTransfersForResource(string virtualFilePath)
    {
      return SecureFunc(FileSystemTask.DownloadTransfersByResourceQuery,
                        () => ReaderService.GetTransfersForResource(virtualFilePath),
                        () => String.Format("Could not get running downloads for file [{0}].", virtualFilePath));
    }

    /// <summary>
    /// Gets a given <see cref="BufferedDataBlock"/> from the currently downloaded
    /// resource.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <param name="blockNumber">The number of the requested block.</param>
    /// <returns>A data block which contains the data as an in-memory buffer
    /// (<see cref="BufferedDataBlock.Data"/>).</returns>
    /// <exception cref="DataBlockException">If the data block cannot be delivered,
    /// either because it's an invalid number, or because only sequential downloads
    /// are possible, and the block does not refer to the current download
    /// position. Check the <see cref="TransmissionCapabilities"/> flag in order
    /// to get the service's capabilities.
    /// </exception>
    /// <exception cref="VirtualResourceNotFoundException">If the resource does
    /// not exist.</exception>
    /// <remarks>It's up to the service to resolve a block number to the
    /// corect piece of data. Simplest case for services that operate on one
    /// resource or stream is to just make all served
    /// blocks the same size (apart from the last one, of course), which
    /// allows to easily calculate the offset of the requested block.</remarks>
    public BufferedDataBlock ReadBlock(string transferId, long blockNumber)
    {
      return SecureFunc(FileSystemTask.DataBlockDownloadRequest,
                        () => DownloadService.ReadDataBlock(transferId, blockNumber),
                        () => String.Format("Could not get read buffered block [{0}] on transfer [{1}].", blockNumber, transferId));
    }

    /// <summary>
    /// Reads a block via a streaming channel, which enables a more resource friendly
    /// data transmission (compared to sending the whole data of the block at once).
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <param name="blockNumber">The number of the requested block.</param>
    /// <returns>A data block which contains the data as an in-memory buffer
    /// (<see cref="BufferedDataBlock.Data"/>).</returns>
    public StreamedDataBlock ReadBlockStreamed(string transferId, long blockNumber)
    {
      return SecureFunc(FileSystemTask.DataBlockDownloadRequest,
                        () => DownloadService.ReadDataBlockStreamed(transferId, blockNumber),
                        () => String.Format("Could not get read streamed block [{0}] on transfer [{1}].", blockNumber, transferId));
    }


    /// <summary>
    /// Gets the binary contents of a resource as a stream in a blocking operation.
    /// Use the methods in <see cref="ContentUtil"/> class for simplified stream
    /// handling. Unlike the <see cref="IDownloadTransferHandler.DownloadFile"/> method, this method takes
    /// the path of a given resource, and works transparently without download tokens.
    /// </summary>
    /// <param name="virtualFilePath">The path of the resource to be read.</param>
    /// <returns>A stream that allows the contents of the file to be read.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="virtualFilePath"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the resource that is represented
    /// by <paramref name="virtualFilePath"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    /// <exception cref="ResourceLockedException">In case the resource is currently locked
    /// and thus not accessible for reading.</exception>
    public Stream ReadFile(string virtualFilePath)
    {
      return SecureFunc(FileSystemTask.StreamedFileDownloadRequest,
                        () => DownloadService.ReadFile(virtualFilePath),
                        () => String.Format("Could not get a file stream to read file [{0}].", virtualFilePath));
    }

    /// <summary>
    /// Gets the binary contents of a resource as a stream in a blocking operation.
    /// Unlike the <see cref="IDownloadTransferHandler.ReadFile"/> method, this method expects the
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
      return SecureFunc(FileSystemTask.StreamedFileDownloadRequest,
                        () => DownloadService.DownloadFile(transferId),
                        () => String.Format("Could not get a continuous file stream to download file for transfer [{0}].", transferId));
    }

  }
}
