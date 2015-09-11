using System;
using System.Collections;
using System.IO;
using System.Threading;


namespace Vfs.Transfer
{
  public abstract class OutboundTransferManager
  {
    public DownloadToken Token { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    protected OutboundTransferManager(DownloadToken token)
    {
      Token = token;
    }

  }




  /// <summary>
  /// Manages the transfer of a single file.
  /// </summary>
  public abstract class InboundTransferManager
  {
    public UploadToken Token { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public long? LastProcessedBlockNumber { get; set; }

    /// <summary>
    /// Whether resuming stopped file transfers
    /// can be resumed or not.
    /// </summary>
    public bool SupportsResume { get; set; }


    public void WriteBlock(BufferedDataBlock block)
    {
      lock(this)
      {
        if (block.BlockNumber != LastProcessedBlockNumber + 1)
        {
          throw new NotImplementedException(""); //TODO provide implementation
        }

        WriteDataImpl(block);

        if (block.IsLastBlock)
        {
          FinalizeTransfer();
        }
      }
    }


    public void AbortTransfer()
    {
    }



    /// <summary>
    /// Finalizes a transfer after having written the last file block.
    /// </summary>
    protected abstract void FinalizeTransfer();


    /// <summary>
    /// 
    /// </summary>
    /// <param name="block"></param>
    protected abstract void WriteDataImpl(BufferedDataBlock block);



    protected abstract void Cleanup();
  }


  /// <summary>
  /// Writes data into an underlying stream.
  /// </summary>
  internal class StreamingTransferManager : InboundTransferManager
  {
    public Stream DestinationStream { get; set; }

    /// <summary>
    /// Finalizes a transfer after having written the last file block.
    /// </summary>
    protected override void FinalizeTransfer()
    {
      DestinationStream.Close();
    }

    protected override void WriteDataImpl(BufferedDataBlock block)
    {
      DestinationStream.Seek(block.Offset, SeekOrigin.Begin);
      DestinationStream.Write(block.Data, 0, block.Data.Length);
      DestinationStream.Flush();
    }

    protected override void Cleanup()
    {
      DestinationStream.Dispose();
    }
  }

  public class UploadManager
  {
    public TransferToken Token { get; set; }

    public Queue Queue { get; set; }
  }
}
