using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using Microsoft.Http;
using Microsoft.Http.Headers;
using Vfs.Auditing;
using Vfs.Transfer;
using Vfs.Util;

namespace Vfs.Restful.Client
{
  public class UploadTransferFacade : TransferFacade<UploadToken>, IUploadTransferHandler
  {
    /// <summary>
    /// Initializes a new instance of the façade.
    /// </summary>
    public UploadTransferFacade(string serviceBaseUri)
      : base(serviceBaseUri)
    {
    }

    #region common transfer methods

    protected override string GetTransmissionCapabilitiesRequestUri()
    {
      return VfsUris.Default.GetUploadTransmissionCapabilitiesUri;
    }

    protected override string GetMaxBlockSizeRequestUri()
    {
      return VfsUris.Default.GetMaxUploadBlockSizeUri;
    }

    protected override string GetTransferStatusRequestUri()
    {
      return VfsUris.Default.GetUploadTransferStatusUri;
    }

    protected override FileSystemTask GetTransferStatusRequestContext()
    {
      return FileSystemTask.UploadTransferStatusRequest;
    }

    protected override string GetPauseTransferRequestUri()
    {
      return VfsUris.Default.PauseUploadTransferUri;
    }

    protected override FileSystemTask GetPauseTransferRequestContext()
    {
      return FileSystemTask.UploadTransferPauseRequest;
    }

    protected override string GetCompleteTransferRequestUri()
    {
      return VfsUris.Default.CompleteUploadTransferUri;
    }

    protected override FileSystemTask GetCompleteTransferRequestContext()
    {
      return FileSystemTask.UploadTransferCompletion;
    }

    protected override string GetCancelTransferRequestUri()
    {
      return VfsUris.Default.CancelUploadTransferUri;
    }

    protected override FileSystemTask GetCancelTransferRequestContext()
    {
      return FileSystemTask.UploadTransferCanceling;
    }

    protected override string GetReloadTokenRequestUri()
    {
      return VfsUris.Default.ReloadUploadTokenUri;
    }

    protected override FileSystemTask GetReloadTokenRequestContext()
    {
      return FileSystemTask.UploadTokenRequery;
    }

    #endregion


    /// <summary>
    /// Gets the maximum size of an uploaded file. A value of
    /// null indicates no limit is in place.
    /// </summary>
    public long? GetMaxFileUploadSize()
    {
      string actionUri = VfsUris.Default.GetMaxFileUploadSizeUri;
      Func<string> errorMessage = () => "Could not get maximum file upload size.";
      var wrapped = SecureGet<Wrapped<long?>>(FileSystemTask.ProviderMetaDataRequest, actionUri, errorMessage);
      return wrapped.Value;
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
    public UploadToken RequestUploadToken(string virtualFilePath, bool overwriteExistingResource, long resourceLength,
                                          string contentType)
    {
      if (String.IsNullOrEmpty(contentType))
      {
        contentType = ContentUtil.UnknownContentType;
      }

      string actionUri = VfsUris.Default.GetUploadTokenUri;
      var inv = CultureInfo.InvariantCulture;
      actionUri = actionUri.ConstructUri(Uris.PatternFilePath, virtualFilePath);
      actionUri = actionUri.ConstructUri(Uris.PatternOverwrite, overwriteExistingResource.ToString(inv).ToLowerInvariant());
      actionUri = actionUri.ConstructUri(Uris.PatternFileLength, resourceLength.ToString(inv));
      actionUri = actionUri.ConstructUri(Uris.PatternContentType, contentType);

      return SecureGet<UploadToken>(FileSystemTask.UploadTokenRequest,
                    actionUri,
                    () => String.Format("Could not get upload token for file [{0}].", virtualFilePath));
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
      Ensure.ArgumentNotNull(block, "block");

      //switch to streaming
      MemoryStream stream = new MemoryStream(block.Data);

      if(block.BlockLength != block.Data.Length)
      {
        string msg = "Block number {0} of transfer [{1}] has a buffer of size [{2}] that doesn't match the indicated block size of [{3}].";
        msg = String.Format(msg, block.BlockNumber, block.TransferTokenId, block.Data.Length, block.BlockLength);
        throw new DataBlockException(msg);
      }

      var streamingBlock = new StreamedDataBlock
                             {
                               TransferTokenId = block.TransferTokenId,
                               BlockNumber = block.BlockNumber,
                               BlockLength = block.BlockLength,
                               Offset = block.Offset,
                               IsLastBlock = block.IsLastBlock,
                               Data = stream
                             };

      WriteBlockStreamed(streamingBlock);
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
    public void WriteBlockStreamed(StreamedDataBlock block)
    {
      Ensure.ArgumentNotNull(block, "block");

      SecureRequest(FileSystemTask.DataBlockUploadRequest,
                    c => SendDataBlock(c, block),
                    () =>
                    String.Format("Could not upload block [{0}] for transfer [{1}].", block.BlockNumber,
                                  block.TransferTokenId));
    }


    /// <summary>
    /// Posts the data of a given data block to the server.
    /// </summary>
    private HttpResponseMessage SendDataBlock(HttpClient client, StreamedDataBlock dataBlock)
    {
      string actionUri = VfsUris.Default.WriteStreamedDataBlockUri;
      actionUri = actionUri.ConstructUri(Uris.PatternTransferId, dataBlock.TransferTokenId);
      actionUri = actionUri.ConstructUri(Uris.PatternBlockNumber, dataBlock.BlockNumber.ToString(CultureInfo.InvariantCulture));

      RequestHeaders requestHeader = new RequestHeaders();

      //write the HTTP headers
      VfsHttpHeaders headers = VfsHttpHeaders.Default;
      requestHeader.Add(headers.TransferId, dataBlock.TransferTokenId); //duplicate transfer ID, might be useful in some scenarios
      requestHeader.Add(headers.BlockNumber, dataBlock.BlockNumber.ToString());
      requestHeader.Add(headers.IsLastBlock, Convert.ToString(dataBlock.IsLastBlock).ToLowerInvariant());
      requestHeader.Add(headers.BlockOffset, dataBlock.Offset.ToString());

      if (dataBlock.BlockLength.HasValue)
      {
        requestHeader.Add(headers.BlockLength, dataBlock.BlockLength.ToString());
      }

      using (dataBlock.Data)
      {
        return client.Send(HttpMethod.POST, actionUri, requestHeader, HttpContent.Create(() => dataBlock.Data));
      }
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
      string actionUri = VfsUris.Default.CompleteUploadTransferWithVerificationUri;
      actionUri = actionUri.ConstructUri(Uris.PatternTransferId, transferId);
      actionUri = actionUri.ConstructUri(Uris.PatternFileHash, md5FileHash);

      var wrapped = SecurePost<Wrapped<TransferStatus>>(GetCompleteTransferRequestContext(),
                                        actionUri,
                                        HttpContent.CreateEmpty(),
                                        () =>
                                        String.Format("Could not complete transfer [{0}] with verification hash [{1}].",
                                                      transferId,
                                                      md5FileHash)
        );
      return wrapped.Value;
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
      string actionUri = VfsUris.Default.WriteFileContentsUri;
      actionUri = actionUri.ConstructUri(Uris.PatternFilePath, virtualFilePath);

      //set headers
      RequestHeaders requestHeader = new RequestHeaders();
      VfsHttpHeaders headers = VfsHttpHeaders.Default;
      requestHeader.Add(headers.OverwriteExistingResource, overwrite.ToString(CultureInfo.InvariantCulture));
      requestHeader.Add("Content-Length", resourceLength.ToString(CultureInfo.InvariantCulture));
      requestHeader.Add("Content-Type", contentType);


      Func<HttpClient, HttpResponseMessage> func = c =>
                                                     {
                                                       HttpContent content = HttpContent.Create(() => input);
                                                       return c.Send(HttpMethod.POST, actionUri, requestHeader,
                                                              content);
                                                     };

      SecureRequest(FileSystemTask.StreamedFileUploadRequest,
        func,
        () => String.Format("Could not write data for file [{0}] to file system.", virtualFilePath));
    }
  }
}