using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OpenRasta.IO;
using OpenRasta.Web;
using Vfs.Restful.Server.Resources;
using Vfs.Restful.Server.Util;
using Vfs.Transfer;
using Vfs.Util;

namespace Vfs.Restful.Server.Handlers.Upload
{
  public class UploadHandler : VfsHandlerBase
  {
    /// <summary>
    /// Convenience property that gets the
    /// <see cref="IFileSystemProvider.UploadTransfers"/>
    /// of the current <see cref="VfsHandlerBase.FileSystem"/>.
    /// </summary>
    protected IUploadTransferHandler Uploads
    {
      get { return FileSystem.UploadTransfers; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public UploadHandler(IFileSystemProvider fileSystem) : base(fileSystem)
    {
    }

//
//    /// <summary>
//    /// Requests a download token for a given resource.
//    /// </summary>
//    [HttpOperation(HttpMethod.GET, ForUriName = "GetUploadToken")]
//    public UploadToken GetToken(string filePath, bool overwrite, long fileLength, string contentType)
//    {
//      return FileSystem.UploadTransfers.RequestUploadToken(filePath, overwrite, fileLength, contentType);
//    }
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
//    [HttpOperation(HttpMethod.GET, ForUriName = "ReloadUploadToken")]
//    public UploadToken ReloadToken(string transferId)
//    {
//      return FileSystem.UploadTransfers.ReloadToken(transferId);
//    }
//
//
//    /// <summary>
//    /// Creates or updates a given file resource in the file system.
//    /// </summary>
//    [HttpOperation(HttpMethod.POST, ForUriName = "WriteFileStream")]
//    public VirtualFileInfo WriteFile(Stream input, string filePath)
//    {
//      VfsHttpHeaders headers = VfsHttpHeaders.Default;
//
//      //get custom headers
//      bool overwrite = Convert.ToBoolean(Request.Headers[headers.OverwriteExistingResource]);
//      long resourceLength = Request.Headers.ContentLength ?? 0;
//      
//      var ct = Request.Headers.ContentType;
//      string contentType = ct == null ? ContentUtil.UnknownContentType : ct.Name;
//      if(String.IsNullOrEmpty(contentType)) contentType = ContentUtil.UnknownContentType;
//      
//      //wrap OpenRasta stream into a non-seekable one - the OR streams indicates it's seekable
//      //but does not support setting its position
//      var stream = new NonSeekableStream(input);
//      return FileSystem.WriteFile(filePath, stream, overwrite, resourceLength, contentType);
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
//    [HttpOperation(HttpMethod.POST, ForUriName = "WriteDataBlock")]
//    public void WriteDataBlock(Stream input, string transferId, long blockNumber)
//    {
//      Stream nonBuggyStream = new NonSeekableStream(input);
//      //forward the stream
//
//      //get meta information from headers
//      VfsHttpHeaders headerNames = VfsHttpHeaders.Default;
//      HttpHeaderDictionary headers = Request.Headers;
//
//      var blockLength = Convert.ToInt32(headers[headerNames.BlockLength]);
//      var isLastBlock = headers.ContainsKey(headerNames.IsLastBlock) ? Convert.ToBoolean(headers[headerNames.IsLastBlock]) : false;
//      var offset = Convert.ToInt64(headers[headerNames.BlockOffset]);
//
//      StreamedDataBlock block = new StreamedDataBlock
//                                  {
//                                    TransferTokenId = transferId,
//                                    BlockNumber = blockNumber,
//                                    IsLastBlock = isLastBlock,
//                                    BlockLength = blockLength,
//                                    Offset = offset,
//                                    Data = new NonSeekableStream(input)
//                                  };
//
//      FileSystem.UploadTransfers.WriteBlockStreamed(block);
//    }


  }
}