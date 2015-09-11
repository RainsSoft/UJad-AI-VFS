using System;
using System.IO;
using System.Runtime.InteropServices;
using OpenRasta.Web;
using Vfs.Restful.Server.Util;
using Vfs.Transfer;

namespace Vfs.Restful.Server.Handlers.Upload
{
  public class WriteDataBlockHandler : UploadHandler
  {
    public WriteDataBlockHandler(IFileSystemProvider fileSystem) : base(fileSystem)
    {
    }


    /// <summary>
    /// Uploads a given data block that provides a chunk of data for an uploaded file as a stream.
    /// </summary>
    /// <exception cref="DataBlockException">If the data block's contents cannot be stored,
    /// either because it's an invalid number, or because only sequential downloads
    /// are possible, and the block does not refer to the current download
    /// position.
    /// </exception>
    /// <exception cref="TransferStatusException">If the transfer has already expired.</exception>
    [HttpOperation(HttpMethod.POST, ForUriName = "WriteDataBlock")]
    public OperationResult Post([Optional]Stream input, string transferId, long blockNumber)
    {
      //in case of an empty data block, there is no stream - just use a null stream instead
      if(input == null) input = Stream.Null;  
      
      return SecureAction(() => WriteBlock(transferId, blockNumber, input));
    }


    private void WriteBlock(string transferId, long blockNumber, Stream input)
    {
      //get meta information from headers
      VfsHttpHeaders headerNames = VfsHttpHeaders.Default;
      HttpHeaderDictionary headers = Request.Headers;

      var blockLength = Convert.ToInt32(headers[headerNames.BlockLength]);
      var isLastBlock = headers.ContainsKey(headerNames.IsLastBlock) ? Convert.ToBoolean(headers[headerNames.IsLastBlock]) : false;
      var offset = Convert.ToInt64(headers[headerNames.BlockOffset]);

      StreamedDataBlock block = new StreamedDataBlock
                                  {
                                    TransferTokenId = transferId,
                                    BlockNumber = blockNumber,
                                    IsLastBlock = isLastBlock,
                                    BlockLength = blockLength,
                                    Offset = offset,
                                    Data = new NonSeekableStream(input)
                                  };

      FileSystem.UploadTransfers.WriteBlockStreamed(block);
    }
  }
}