using System;
using System.ServiceModel;
using Vfs.FileSystemService.Faults;
using Vfs.Transfer;

namespace Vfs.FileSystemService
{
  /// <summary>
  /// Exposes service operations to upload binary
  /// data to the service.
  /// </summary>
  [ServiceContract(Namespace = Namespace.Main)]
  public interface IFSWriterService
  {
    #region common transfer operations

        /// <summary>
    /// Indicates how restrictively data blocks may be transmitted.
    /// </summary>
    [OperationContract]
    [FaultContract(typeof (ResourceFault))]
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
    [System.ServiceModel.Web.WebGet(UriTemplate = "/maxblocksize}", BodyStyle = System.ServiceModel.Web.WebMessageBodyStyle.Bare)]
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
    /// Gets the maximum size of an uploaded file. A value of
    /// null indicates no limit is in place.
    /// </summary>
    [OperationContract]
    long? GetMaxFileUploadSize();


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
    UploadToken RequestUploadToken(string virtualFilePath, bool overwriteExistingResource, long fileLength, string contentType);


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
    UploadToken ReloadToken(string transferId);


    /// <summary>
    /// Gets the token that represents a running transfer for a given resource,
    /// e.g. for aborting running transfers before deleting or modifying a resource.
    /// </summary>
    /// <param name="virtualFilePath">The identifier of the resource.</param>
    /// <returns>The token that is being maintained for the resource.</returns>
    /// <remarks>This method needs to be secured in remote scenarios (or not be made available
    /// at all). Otherwise, if allows for the hijacking a running upload.</remarks>
    [OperationContract]
    [FaultContract(typeof(ResourceFault))]
    UploadToken GetTransferForResource(string virtualFilePath);


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
    [OperationContract]
    [FaultContract(typeof(ResourceFault))]
    TransferStatus CompleteTransfer(string transferId, string md5FileHash);


    /// <summary>
    /// Creates or updates a given file resource in the file system.
    /// </summary>
    /// <param name="writeContract">Provides required information about the
    /// file to be created along with the data to be written.</param>
    /// <returns>Updated file information.</returns>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the parent folder
    /// of the submitted file path does not exist.</exception>
    /// <exception cref="ResourceOverwriteException">If a file already exists at the
    /// specified location, and the <see cref="WriteFileDataContract.Overwrite"/> flag was not set.</exception>
    /// <exception cref="ArgumentNullException">If any of the parameters is a null reference.</exception>
    [OperationContract]
    [FaultContract(typeof(ResourceFault))]
    FileInfoDataContract WriteFile(WriteFileDataContract writeContract);
  }
}