using System;
using System.IO;
using System.ServiceModel;
using Vfs.FileSystemService.Faults;
using Vfs.Transfer;
using Vfs.Util;

namespace Vfs.FileSystemService
{
  /// <summary>
  /// Complements the <see cref="IFSReaderService"/> with
  /// functionality to actually transfer data from the service
  /// to the client.
  /// </summary>
  public interface IFSDataDownloadService
  {

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
    [OperationContract]
    [FaultContract(typeof(ResourceFault))]
#if !SILVERLIGHT
    [System.ServiceModel.Web.WebGet(UriTemplate = "/datablock?transfer={transferId}&blockNumber={blockNumber}", BodyStyle = System.ServiceModel.Web.WebMessageBodyStyle.Bare)]
#endif
    BufferedDataBlock ReadDataBlock(string transferId, long blockNumber);


    /// <summary>
    /// Reads a block via a streaming channel, which enables a more resource friendly
    /// data transmission (compared to sending the whole data of the block at once).
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <param name="blockNumber">The number of the requested block.</param>
    /// <returns>A data block which contains the data as an in-memory buffer
    /// (<see cref="BufferedDataBlock.Data"/>).</returns>
    [OperationContract]
    [FaultContract(typeof(ResourceFault))]
#if !SILVERLIGHT
    [System.ServiceModel.Web.WebGet(UriTemplate = "/streameddatablock?transfer={transferId}&blockNumber={blockNumber}", BodyStyle = System.ServiceModel.Web.WebMessageBodyStyle.Bare)]
#endif
    StreamedDataBlock ReadDataBlockStreamed(string transferId, long blockNumber);



    /// <summary>
    /// Gets the binary contents of a file as a stream in one blocking operation.
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
    [OperationContract]
    [FaultContract(typeof(ResourceFault))]
#if !SILVERLIGHT
    [System.ServiceModel.Web.WebGet(UriTemplate = "/getcontents/{virtualFilePath}", BodyStyle = System.ServiceModel.Web.WebMessageBodyStyle.Bare)]
#endif
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
    [OperationContract]
    [FaultContract(typeof(ResourceFault))]
    Stream DownloadFile(string transferId);
  }
}
