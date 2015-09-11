using System.Collections.Generic;
using System.ServiceModel;
using Vfs.FileSystemService.Faults;
using Vfs.Transfer;


namespace Vfs.FileSystemService
{
  /// <summary>
  /// A service that provides access to contents of the file
  /// system's file. This service is the complement of the
  /// <see cref="IDownloadTransferHandler"/> of the core library.
  /// </summary>
  [ServiceContract(Namespace = Namespace.Main)]
  public interface IFSReaderService
  {
    #region common transfer operations

    /// <summary>
    /// Indicates how restrictively data blocks may be transmitted.
    /// </summary>
    [OperationContract]
    [FaultContract(typeof(ResourceFault))]
#if !SILVERLIGHT
    [System.ServiceModel.Web.WebGet(UriTemplate = "/transmissioncapabilities",
      BodyStyle = System.ServiceModel.Web.WebMessageBodyStyle.Bare)]
#endif
    TransmissionCapabilities GetTransmissionCapabilities();

    /// <summary>
    /// Gets the maximum block size that can be transferred, if such
    /// a limit exists. Returns null in case of arbitrary block sizes.
    /// </summary>
    [OperationContract]
    [FaultContract(typeof(ResourceFault))]
#if !SILVERLIGHT
    [System.ServiceModel.Web.WebGet(UriTemplate = "/maxblocksize", BodyStyle = System.ServiceModel.Web.WebMessageBodyStyle.Bare)]
#endif
    int? GetMaxBlockSize();

    /// <summary>
    /// Gets the server-side status of the transfer.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <returns>The status of the requested transfer. If the transfer is unknown,
    /// this does not cause an exception, but merely results in a return value
    /// of <see cref="TransferStatus.UnknownTransfer"/>.</returns>
    [OperationContract]
    [FaultContract(typeof(ResourceFault))]
#if !SILVERLIGHT
    [System.ServiceModel.Web.WebGet(UriTemplate = "/transferstatus?transfer={transferId}", BodyStyle = System.ServiceModel.Web.WebMessageBodyStyle.Bare)]
#endif
    TransferStatus GetTransferStatus(string transferId);

    /// <summary>
    /// Tells the transfer service that transmission is being
    /// paused for an unknown period of time. This should keep
    /// the transfer enabled (including lock to protect the resource),
    /// but gives the service time to free or unlock resources.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <exception cref="UnknownTransferException">In case the <paramref name="transferId"/>
    /// does not refer to an active transfer.</exception>
    [OperationContract]
    [FaultContract(typeof(ResourceFault))]
#if !SILVERLIGHT
    [System.ServiceModel.Web.WebGet(UriTemplate = "/pausetransfer?transfer={transferId}", BodyStyle = System.ServiceModel.Web.WebMessageBodyStyle.Bare)]
#endif
    void PauseTransfer(string transferId);

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
    [OperationContract]
    [FaultContract(typeof(ResourceFault))]
#if !SILVERLIGHT
    [System.ServiceModel.Web.WebGet(UriTemplate = "/completetransfer?transfer={transferId}", BodyStyle = System.ServiceModel.Web.WebMessageBodyStyle.Bare)]
#endif
    TransferStatus CompleteTransfer(string transferId);

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
    [OperationContract]
    [FaultContract(typeof(ResourceFault))]
#if !SILVERLIGHT
    [System.ServiceModel.Web.WebGet(UriTemplate = "/canceltransfer?transfer={transferId}&reason={reason}", BodyStyle = System.ServiceModel.Web.WebMessageBodyStyle.Bare)]
#endif
    TransferStatus CancelTransfer(string transferId, AbortReason reason);

    #endregion


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
    [OperationContract]
    [FaultContract(typeof (ResourceFault))]
#if !SILVERLIGHT
    [System.ServiceModel.Web.WebGet(UriTemplate = "/gettoken?file={virtualFilePath}&includeFileHash={includeFileHash}",
      BodyStyle = System.ServiceModel.Web.WebMessageBodyStyle.Bare)]
#endif
    DownloadToken RequestDownloadToken(string virtualFilePath, bool includeFileHash);


    /// <summary>
    /// Requests a download token for a given resource.
    /// </summary>
    /// <param name="virtualFilePath">Identifies the resource to be downloaded.</param>
    /// <param name="maxBlockSize">The maximum size of a read block. This value must be
    /// equal or lower to the <see cref="GetMaxBlockSize"/>, if there is an
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
    [FaultContract(typeof (ResourceFault))]
#if !SILVERLIGHT
    [System.ServiceModel.Web.WebGet(
      UriTemplate = "/gettoken?file={virtualFilePath}&includefilehash={includeFileHash}&maxblocksize={maxBlockSize}",
      BodyStyle = System.ServiceModel.Web.WebMessageBodyStyle.Bare)]
#endif
    DownloadToken RequestDownloadTokenWithBlockSize(string virtualFilePath, int maxBlockSize, bool includeFileHash);


    /// <summary>
    /// Requeries a previously issued token. Can be used if the client only stores
    /// <see cref="TransferToken.TransferId"/> values rather than the tokens and
    /// needs to get ahold of them again.
    /// </summary>
    /// <param name="transferId"></param>
    /// <returns></returns>
    /// <exception cref="UnknownTransferException">In case the <paramref name="transferId"/>
    /// does not refer to an active transfer.</exception>
    [FaultContract(typeof(ResourceFault))]
#if !SILVERLIGHT
    [System.ServiceModel.Web.WebGet(
      UriTemplate = "/reloadtoken?transferid={transferId}",
      BodyStyle = System.ServiceModel.Web.WebMessageBodyStyle.Bare)]
#endif
    DownloadToken ReloadToken(string transferId);

    /// <summary>
    /// Gets all running transfers for a given resource, e.g. for aborting running
    /// transfers before deleting or modifying a resource.
    /// </summary>
    /// <param name="virtualFilePath">The identifier of the resource.</param>
    [OperationContract]
    [FaultContract(typeof(ResourceFault))]
#if !SILVERLIGHT
    [System.ServiceModel.Web.WebGet(
      UriTemplate = "/getdownloads?file={virtualFilePath}",
      BodyStyle = System.ServiceModel.Web.WebMessageBodyStyle.Bare)]
#endif
    IEnumerable<DownloadToken> GetTransfersForResource(string virtualFilePath);



  }
}
