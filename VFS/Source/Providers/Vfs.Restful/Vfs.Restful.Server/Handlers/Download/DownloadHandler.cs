using System.Web;
using OpenRasta.Web;
using Vfs.Restful.Server.Resources;
using Vfs.Transfer;


namespace Vfs.Restful.Server.Handlers.Download
{
  /// <summary>
  /// Provides download capabilities from the underlying
  /// <see cref="VfsHandlerBase.FileSystem"/>.
  /// </summary>
  public class DownloadHandler : VfsHandlerBase
  {
    /// <summary>
    /// Convenience property that gets the
    /// <see cref="IFileSystemProvider.DownloadTransfers"/>
    /// of the current <see cref="VfsHandlerBase.FileSystem"/>.
    /// </summary>
    protected IDownloadTransferHandler Downloads
    {
      get { return FileSystem.DownloadTransfers; } 
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public DownloadHandler(IFileSystemProvider fileSystem) : base(fileSystem)
    {
    }

//
//    /// <summary>
//    /// Requests a download token for a given resource, and specifies the
//    /// maximum block size that is supported by the client.
//    /// </summary>
//    [HttpOperation(HttpMethod.GET, ForUriName = "GetDownloadTokenWithMaxBlockSize")]
//    public DownloadToken GetToken(string filePath, bool includeFileHash, int maxBlockSize)
//    {
//      return Downloads.RequestDownloadToken(filePath, includeFileHash, maxBlockSize);
//    }
//
//
//    /// <summary>
//    /// Requests a download token for a given resource.
//    /// </summary>
//    [HttpOperation(HttpMethod.GET, ForUriName = "GetDownloadToken")]
//    public DownloadToken GetToken(string filePath, bool includeFileHash)
//    {
//      return Downloads.RequestDownloadToken(filePath, includeFileHash);
//    }
//
//
//
//
//
//    /// <summary>
//    /// Requeries a previously issued token. Can be used if the client only stores
//    /// <see cref="TransferToken.TransferId"/> values rather than the tokens and
//    /// needs to get ahold of them again.
//    /// </summary>
//    /// <param name="transferId">The <see cref="TransferToken.TransferId"/>
//    /// of the requested token.</param>
//    /// <exception cref="UnknownTransferException">In case the <paramref name="transferId"/>
//    /// does not refer to an active transfer.</exception>
//    [HttpOperation(HttpMethod.GET, ForUriName = "ReloadDownloadToken")]
//    public DownloadToken ReloadToken(string transferId)
//    {
//      return Downloads.ReloadToken(transferId);
//    }
//
//
//    /// <summary>
//    /// Returns access to a given file as a whole stream.
//    /// </summary>
//    /// <param name="filePath">Path of the requested file resource.</param>
//    /// <returns>OpenRasta file.</returns>
//    [HttpOperation(HttpMethod.GET, ForUriName = "ReadFileByPath")]
//    public FileDataResource ReadFile(string filePath)
//    {
//      var token = FileSystem.DownloadTransfers.RequestDownloadToken(filePath, false);
//
//      return new FileDataResource(token, () =>
//                                           {
//                                             SetResponseHeader(token);
//                                             return Downloads.ReadFile(token.ResourceIdentifier);
//                                           });
//    }
//
//
//    /// <summary>
//    /// Returns access to a given file as a whole stream.
//    /// </summary>
//    /// <param name="transferId">The <see cref="TransferToken.TransferId"/>
//    /// of an issued download token.</param>
//    [HttpOperation(HttpMethod.GET, ForUriName = "ReadFileByToken")]
//    public FileDataResource ReadFileByToken(string transferId)
//    {
//      var token = FileSystem.DownloadTransfers.ReloadToken(transferId);
//      return new FileDataResource(token, () =>
//                                           {
//                                             SetResponseHeader(token);
//                                             return Downloads.DownloadFile(transferId);
//                                           });
//    }
//
//
//    private static void SetResponseHeader(TransferToken token)
//    {
//      if (HttpContext.Current == null) return;
//
//      //we need to disable buffering and set headers before streaming
//      var resp = HttpContext.Current.Response;
//      resp.BufferOutput = false;
//      resp.AppendHeader("Content-Type", token.ContentType);
//      resp.AppendHeader("Content-Disposition", "inline; filename=" + token.ResourceName);
//      resp.AppendHeader("Content-Length", token.ResourceLength.ToString());
//    }
//
//
//    /// <summary>
//    /// Reads a block via a streaming channel, which enables a more resource friendly
//    /// data transmission (compared to sending the whole data of the block at once).
//    /// </summary>
//    /// <param name="transferId">Identifies the transfer and resource.</param>
//    /// <param name="blockNumber">The number of the requested block.</param>
//    /// <returns>A data block which contains the data as an in-memory buffer
//    /// (<see cref="BufferedDataBlock.Data"/>).</returns>
//    [HttpOperation(HttpMethod.GET, ForUriName = "ReadDataBlock")]
//    public StreamedDataBlock ReadDataBlock(string transferId, long blockNumber)
//    {
//      return Downloads.ReadBlockStreamed(transferId, blockNumber);
//    }

  }
}