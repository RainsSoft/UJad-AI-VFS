using System.Collections.Generic;
using System.IO;
using Vfs.Locking;


namespace Vfs.Transfer
{
  public interface ITransferService
  {
    /// <summary>
    /// Indicates how data blocks need to be transmitted.
    /// </summary>
    TransmissionCapabilities TransmissionCapabilities { get; }

    /// <summary>
    /// Gets the server-side status of the transfer.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <returns></returns>
    TransferStatus GetTransferStatus(string transferId);

    /// <summary>
    /// Tells the transfer service that transmission is being
    /// paused for an unknown period of time. This should keep
    /// the transfer enabled, but gives the service time to
    /// free or unlock resources.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    void PauseTransfer(string transferId);

    /// <summary>
    /// Completes a given file transfer - invoking this operation
    /// closes and removes the transfer from the service.
    /// </summary>
    void CompleteTransfer(string transferId);

    /// <summary>
    /// Aborts a currently managed resource transfer.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <param name="reason">The reason to cancel the transfer.</param>
    void CancelTransfer(string transferId, AbortReason reason);
  }


  /// <summary>
  /// A service that provides downloading capabilities
  /// in order to read resource data from the file system.
  /// </summary>
  public interface IDownloadTransferService : ITransferService
  {
    /// <summary>
    /// Requests a download token for a given resource.
    /// </summary>
    /// <param name="resourceIdentifier"></param>
    /// <param name="includeFileHash">Whether a file hash for the
    /// requested resource should be calculated and assigned to the
    /// <see cref="DownloadToken.Md5FileHash"/> property of the returned
    /// <see cref="DownloadToken"/>.</param>
    /// <returns>A token that represents a granted resource download, optinally
    /// limited to a given time frame (<see cref="TransferToken.ExpirationTime"/>).</returns>
    /// <exception cref="ResourceLockedException">If a lock to access the
    /// resource was not granted.</exception>
    DownloadToken RequestDownloadToken(string resourceIdentifier, bool includeFileHash);


    /// <summary>
    /// Requests a download token based on an older one. This allows to resume a paused
    /// or expired download if the service already discarded the token.
    /// </summary>
    /// <param name="oldToken">The previously used token.</param>
    /// <param name="includeFileHash">Whether a file hash for the
    /// requested resource should be calculated and assigned to the
    /// <see cref="DownloadToken.Md5FileHash"/> property of the returned
    /// <see cref="DownloadToken"/>.</param>
    /// <returns>An updated token with a new expiration time, which reflects the
    /// status updates of the submitted token.</returns>
    DownloadToken RenewToken(DownloadToken oldToken, bool includeFileHash);



    /// <summary>
    /// Gets a given <see cref="BufferedDataBlock"/> from the currently downloaded
    /// resource.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <param name="blockNumber">The number of the downloaded block.</param>
    /// <returns></returns>
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
    BufferedDataBlock ReadBlock(string transferId, long blockNumber);


    /// <summary>
    /// Reads a block via a streaming channel, which enables a more resource friendly
    /// data transmission (compared to sending the whole data of the block at once).
    /// </summary>
    /// <param name="transferId"></param>
    /// <param name="blockNumber"></param>
    /// <returns></returns>
    StreamedDataBlock ReadBlockStreamed(string transferId, long blockNumber);

    /// <summary>
    /// Gets all running transfers for a given resource, e.g. for aborting running
    /// transfers before deleting or modifying a resource.
    /// </summary>
    /// <param name="resourceId">The identifier of the resource.</param>
    IEnumerable<DownloadToken> GetTransfersForResource(string resourceId);

    /// <summary>
    /// Gets one stream that allows reading the whole resource.
    /// </summary>
    /// <param name="transferId"></param>
    /// <returns></returns>
    Stream ReadResourceStreamed(string transferId);
  }



  /// <summary>
  /// A service that provides an interface to upload (write)
  /// resource data to the file system.
  /// </summary>
  public interface IUploadTransferService : ITransferService
  {
    /// <summary>
    /// Requests and upload token for a given resource.
    /// </summary>
    /// <param name="resourceIdentifier"></param>
    /// <param name="overwriteExistingResource"></param>
    /// <param name="resourceLength"></param>
    /// <param name="contentType"></param>
    /// <returns></returns>
    /// <exception cref="ResourceLockedException">If a lock to access the
    /// resource was not granted.</exception>
    UploadToken RequestUploadToken(string resourceIdentifier, bool overwriteExistingResource, long resourceLength, string contentType);


    /// <summary>
    /// Uploads a given data block.
    /// </summary>
    /// <param name="block">The block to be written.</param>
    /// <returns></returns>
    /// <exception cref="DataBlockException">If the data block's contents cannot be stored,
    /// either because it's an invalid number, or because only sequential downloads
    /// are possible, and the block does not refer to the current download
    /// position.
    /// </exception>
    bool WriteBlock(BufferedDataBlock block);


    /// <summary>
    /// 
    /// </summary>
    void WriteBlockStreamed(DataBlockInfo blockInfo, Stream data);


    /// <summary>
    /// Allows to verify the integrity of an uploaded file
    /// before closing (committing or cancelling) a
    /// given transfer.
    /// </summary>
    /// <param name="transferId"></param>
    /// <param name="md5FileHash">An MD5 file hash that should
    /// match the uploaded file.</param>
    /// <returns>True in case the uploaded file has the same
    /// hash.</returns>
    /// <exception cref="UnknownTransferException">In case
    /// the transfer is unknown (e.g. because it was already closed
    /// via <see cref="CloseTransfer"/> and removed).</exception>
    /// <exception cref="InvalidTransferStatusException">If the
    /// transfer is not <see cref="TransferStatus.Completed"/>.</exception>
    bool VerifyUploadIntegrity(string transferId, string md5FileHash);

  }
}
