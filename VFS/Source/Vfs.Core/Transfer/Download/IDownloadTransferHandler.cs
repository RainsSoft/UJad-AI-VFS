using System;
using System.Collections.Generic;
using System.IO;
using Vfs.Auditing;
using Vfs.Util;

namespace Vfs.Transfer
{
  /// <summary>
  /// A service that provides downloading capabilities
  /// in order to read resource data from the file system.
  /// </summary>
  public interface IDownloadTransferHandler : ITransferHandler<DownloadToken>
  {
    /// <summary>
    /// Adds an auditor to the service, which receives
    /// auditing messages for download requests.<br/>
    /// Assigning a null reference should not set the property to null,
    /// but fall back to a <see cref="NullAuditor"/> instead so
    /// this property never returns null but a
    /// valid <see cref="IAuditor"/> instance.
    /// </summary>
    IAuditor Auditor { get; set; }


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
    DownloadToken RequestDownloadToken(string virtualFilePath, bool includeFileHash);


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
    DownloadToken RequestDownloadToken(string virtualFilePath, bool includeFileHash, int maxBlockSize);


    /// <summary>
    /// Gets all running transfers for a given resource, e.g. for aborting running
    /// transfers before deleting or modifying a resource.
    /// </summary>
    /// <param name="virtualFilePath">The identifier of the resource.</param>
    IEnumerable<DownloadToken> GetTransfersForResource(string virtualFilePath);


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
    BufferedDataBlock ReadBlock(string transferId, long blockNumber);


    /// <summary>
    /// Reads a block via a streaming channel, which enables a more resource friendly
    /// data transmission (compared to sending the whole data of the block at once).
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <param name="blockNumber">The number of the requested block.</param>
    /// <returns>A data block which contains the data as an in-memory buffer
    /// (<see cref="BufferedDataBlock.Data"/>).</returns>
    StreamedDataBlock ReadBlockStreamed(string transferId, long blockNumber);


    /// <summary>
    /// Gets the binary contents of a resource as a stream in a blocking operation.
    /// Use the methods in <see cref="ContentUtil"/> class for simplified stream
    /// handling. Unlike the <see cref="DownloadFile"/> method, this method takes
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
    Stream ReadFile(string virtualFilePath);



    /// <summary>
    /// Gets the binary contents of a resource as a stream in a blocking operation.
    /// Unlike the <see cref="ReadFile"/> method, this method expects the
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
    Stream DownloadFile(string transferId);
  }
}