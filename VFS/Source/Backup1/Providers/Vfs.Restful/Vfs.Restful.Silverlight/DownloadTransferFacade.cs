using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Http;
using Microsoft.Http.Headers;
using Vfs.Auditing;
using Vfs.Transfer;
using Vfs.Util;

namespace Vfs.Restful.Client
{
  public class DownloadTransferFacade : TransferFacade<DownloadToken>, IDownloadTransferHandler
  {
    /// <summary>
    /// Initializes a new instance of the façade.
    /// </summary>
    public DownloadTransferFacade(string serviceBaseUri)
      : base(serviceBaseUri)
    {
    }


    #region common transfer methods

    protected override string GetTransmissionCapabilitiesRequestUri()
    {
      return VfsUris.Default.GetDownloadTransmissionCapabilitiesUri;
    }


    protected override string GetMaxBlockSizeRequestUri()
    {
      return VfsUris.Default.GetMaxDownloadBlockSizeUri;
    }

    protected override string GetTransferStatusRequestUri()
    {
      return VfsUris.Default.GetDownloadTransferStatusUri;
    }

    protected override FileSystemTask GetTransferStatusRequestContext()
    {
      return FileSystemTask.DownloadTransferStatusRequest;
    }

    protected override string GetPauseTransferRequestUri()
    {
      return VfsUris.Default.PauseDownloadTransferUri;
    }

    protected override FileSystemTask GetPauseTransferRequestContext()
    {
      return FileSystemTask.DownloadTransferPauseRequest;
    }

    protected override string GetCompleteTransferRequestUri()
    {
      return VfsUris.Default.CompleteDownloadTransferUri;
    }

    protected override FileSystemTask GetCompleteTransferRequestContext()
    {
      return FileSystemTask.DownloadTransferCompletion;
    }

    protected override string GetCancelTransferRequestUri()
    {
      return VfsUris.Default.CancelDownloadTransferUri;
    }

    protected override FileSystemTask GetCancelTransferRequestContext()
    {
      return FileSystemTask.DownloadTransferCanceling;
    }

    protected override string GetReloadTokenRequestUri()
    {
      return VfsUris.Default.ReloadDownloadTokenUri;
    }

    protected override FileSystemTask GetReloadTokenRequestContext()
    {
      return FileSystemTask.DownloadTokenRequery;
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
      string actionUri = Uris.GetDownloadTokenUri;
      actionUri = actionUri.ConstructUri(Uris.PatternFilePath, virtualFilePath);
      actionUri = actionUri.ConstructUri(Uris.PatternIncludeFileHash, Convert.ToString(includeFileHash).ToLowerInvariant());
      
      Func<string> errorMessage =  () => String.Format("Download token request for file [{0}] failed.", virtualFilePath);
      const FileSystemTask context = FileSystemTask.DownloadTokenRequest;

      return SecureGet<DownloadToken>(context, actionUri, errorMessage);
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
      string actionUri = Uris.GetDownloadTokenWithMaxBlockSizeUri;
      actionUri = actionUri.ConstructUri(Uris.PatternFilePath, virtualFilePath);
      actionUri = actionUri.ConstructUri(Uris.PatternIncludeFileHash, Convert.ToString(includeFileHash).ToLowerInvariant());
      actionUri = actionUri.ConstructUri(Uris.PatternMaxBlockSize, Convert.ToString(maxBlockSize));

      Func<string> errorMessage = () => String.Format("Download token request for file [{0}] failed.", virtualFilePath);
      const FileSystemTask context = FileSystemTask.DownloadTokenRequest;

      return SecureGet<DownloadToken>(context, actionUri, errorMessage);
    }



    /// <summary>
    /// Gets all running transfers for a given resource, e.g. for aborting running
    /// transfers before deleting or modifying a resource.
    /// </summary>
    /// <param name="virtualFilePath">The identifier of the resource.</param>
    public IEnumerable<DownloadToken> GetTransfersForResource(string virtualFilePath)
    {
      throw new NotImplementedException(""); //TODO provide implementation

//      return SecureFunc(FileSystemTask.DownloadTransfersByResourceQuery,
//                        () => ReaderService.GetTransfersForResource(virtualFilePath),
//                        () => String.Format("Could not get running downloads for file [{0}].", virtualFilePath));
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
      //read stream
      StreamedDataBlock block = ReadBlockStreamed(transferId, blockNumber);
      byte[] buffer = block.Data.ReadIntoBuffer();

      return new BufferedDataBlock
               {
                 TransferTokenId = transferId,
                 BlockNumber = blockNumber,
                 BlockLength = block.BlockLength,
                 Offset = block.Offset,
                 IsLastBlock = block.IsLastBlock,
                 Data = buffer
               };
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
      string actionUri = Uris.GetDataBlockStreamedUri;
      actionUri = actionUri.ConstructUri(Uris.PatternTransferId, transferId);
      actionUri = actionUri.ConstructUri(Uris.PatternBlockNumber, blockNumber.ToString(CultureInfo.InvariantCulture));

      Func<string> errorMessage = () => String.Format("Could not read streamed block [{0}] on transfer [{1}].", blockNumber, transferId);
      const FileSystemTask context = FileSystemTask.DataBlockDownloadRequest;

      return SecureFunc(context, () => GetStreamedBlockImpl(actionUri, transferId, blockNumber), errorMessage);
    }


    private StreamedDataBlock GetStreamedBlockImpl(string actionUri, string transferId, long blockNumber)
    {
      //run request and get response stream
      var response = Util.RunRequest(ServiceBaseUri, c => c.Get(actionUri));
      Stream stream = response.Content.ReadAsStream();

      //get meta data from headers
      ResponseHeaders headers = response.Headers;

      var offset = long.Parse(headers[VfsHttpHeaders.Default.BlockOffset], CultureInfo.InvariantCulture);
      var isLastBlock = bool.Parse(headers[VfsHttpHeaders.Default.IsLastBlock]);

      int? blockLength = null;
      if (headers.ContainsKey(VfsHttpHeaders.Default.BlockLength))
      {
        blockLength = int.Parse(headers[VfsHttpHeaders.Default.BlockLength], CultureInfo.InvariantCulture);
      }

      return new StreamedDataBlock
      {
        BlockNumber = blockNumber,
        TransferTokenId = transferId,
        Data = stream,
        Offset = offset,
        BlockLength = blockLength,
        IsLastBlock = isLastBlock
      };
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
      string actionUri = Uris.ReadFileContentsUri;
      actionUri = actionUri.ConstructUri(Uris.PatternFilePath, virtualFilePath);

      Func<string> errorMessage = () => String.Format("Could not read data of file '{0}.", virtualFilePath);
      Func<Stream> func = () =>
                            {
                              var response = Util.RunRequest(ServiceBaseUri, c => c.Get(actionUri));
                              return response.Content.ReadAsStream();
                            };

      return SecureFunc(FileSystemTask.StreamedFileDownloadRequest, func, errorMessage); 
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
      string actionUri = Uris.ReadFileContentsByTokenUri;
      actionUri = actionUri.ConstructUri(Uris.PatternTransferId, transferId);

      Func<string> errorMessage = () => String.Format("Could not get a file stream to download file for transfer [{0}].", transferId);
      Func<Stream> func = () =>
      {
        var response = Util.RunRequest(ServiceBaseUri, c => c.Get(actionUri));
        return response.Content.ReadAsStream();
      };

      return SecureFunc(FileSystemTask.StreamedFileDownloadRequest, func, errorMessage); 
    }

  }
}