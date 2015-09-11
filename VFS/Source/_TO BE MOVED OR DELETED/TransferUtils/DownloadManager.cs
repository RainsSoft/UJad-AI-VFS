using System;
using System.IO;
using Vfs.Util;

namespace Vfs.Transfer
{
  /// <summary>
  /// Manages download from a given <see cref="IDownloadTransferService"/>
  /// and writes the data of all received blocks to a given <see cref="OutputStream"/>.
  /// </summary>
  public class DownloadManager
  {
    public IDownloadTransferService TransferService { get; private set; }
    public string ResourceId { get; private set; }
    public DownloadToken Token { get; private set; }
    public Stream OutputStream { get; set; }

    /// <summary>
    /// Whether to use the streaming API of the download service
    /// (<see cref="IDownloadTransferService.ReadBlockStreamed"/> or
    /// transfer the data as a whole (<see cref="IDownloadTransferService.ReadBlock"/>).<br/>
    /// Defaults to false.
    /// </summary>
    public bool UseStreamingTransfer { get; set; }

    
    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public DownloadManager(IDownloadTransferService transferService, string resourceId)
    {
      TransferService = transferService;
      ResourceId = resourceId;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public DownloadManager(IDownloadTransferService transferService, string resourceId, DownloadToken token)
    {
      TransferService = transferService;
      ResourceId = resourceId;
      Token = token;
    }

    public void StartDownload()
    {
      if (Token == null)
      {
        Token = TransferService.RequestDownloadToken(ResourceId, false);
      }

      while(true)
      {
        IDataBlockInfo blockInfo;
        string transferId = Token.TransferId;

        long nextBlock = 0;
        if (Token.LastTransmittedBlockInfo != null)
        {
          nextBlock = Token.LastTransmittedBlockInfo.BlockNumber + 1;
        }

        if (UseStreamingTransfer)
        {
          StreamedDataBlock block = TransferService.ReadBlockStreamed(transferId, nextBlock);
          block.Data.WriteTo(OutputStream);
          blockInfo = block;
        }
        else
        {
          BufferedDataBlock block = TransferService.ReadBlock(transferId, nextBlock);
          OutputStream.Write(block.Data, 0, block.Data.Length);
          blockInfo = block;
        }

        //update the token, get independent file info without data or stream
        Token.LastTransmittedBlockInfo = DataBlockInfo.FromDataBlock(blockInfo);
        Token.LastBlockTransmissionTime = SystemTime.Now();

        if(blockInfo.IsLastBlock)
        {
          //we're done - update token data
          Token.CompletionTime = Token.LastBlockTransmissionTime;
          TransferService.CompleteTransfer(transferId);
          Token.Status = TransferStatus.Completed;

          break;
        }
      }
    }

  }
}
