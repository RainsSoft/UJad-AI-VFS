using System;
using System.Collections.Generic;
using System.IO;
using Vfs.Util;

namespace Vfs.Transfer
{
  /// <summary>
  /// Manages downloads from the local file system.
  /// </summary>
  public class FileDownloadService : IDownloadTransferService
  {
    public Func<string, FileInfo> FileResolveFunc { get; private set; }

    internal Dictionary<string, DownloadTransfer> Transfers { get; set; }

    /// <summary>
    /// Indicates how data blocks need to be transmitted.
    /// </summary>
    public TransmissionCapabilities TransmissionCapabilities
    {
      get { return TransmissionCapabilities.Random; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public FileDownloadService(Func<string, FileInfo> fileResolveFunc)
    {
      FileResolveFunc = fileResolveFunc;
      Transfers = new Dictionary<string, DownloadTransfer>();
    }


    private DownloadTransfer GetCachedTransfer(string transferId, bool throwExceptionIfNotFound)
    {
      DownloadTransfer transfer;
      var status = Transfers.TryGetValue(transferId, out transfer);
      if (!status && throwExceptionIfNotFound)
      {
        throw new UnknownTransferException(String.Format("Unknown transfer ID: {0}.", transferId));
      }

      return transfer;
    }


    /// <summary>
    /// Gets the server-side status of the transfer.
    /// </summary>
    /// <param name="transferId"></param>
    /// <returns></returns>
    public TransferStatus GetTransferStatus(string transferId)
    {
      DownloadTransfer transfer;
      var status = Transfers.TryGetValue(transferId, out transfer);
      if (!status) return TransferStatus.UnknownTransfer;

      lock (transfer.SyncRoot)
      {
        return transfer.Status;
      }
    }

    /// <summary>
    /// Tells the transfer service that transmission is being
    /// paused for an unknown period of time. This should keep
    /// the transfer enabled, but gives the service time to
    /// free or unlock resources.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    public void PauseTransfer(string transferId)
    {
      DownloadTransfer transfer = GetCachedTransfer(transferId, true);
      lock(transfer.SyncRoot)
      {
        if(!transfer.Status.Is(TransferStatus.Starting, TransferStatus.Running))
        {
          string msg = "Only active transfers can be paused. Current status is: [{0}].";
          msg = String.Format(msg, transfer.Status);
          throw new TransferStatusException(msg);
        }

        if (transfer.Stream != null)
        {
          //close stream
          transfer.Stream.Dispose();
          transfer.Stream = null;
        }

        transfer.Token.Status = TransferStatus.Paused;
      }
    }


    #region close / cancel transfer

    /// <summary>
    /// Closes a given transfer - this is either regarded
    /// a commit or a rollback, which clears any currently
    /// written file data.
    /// </summary>
    /// <param name="transferId"></param>
    /// <returns></returns>
    public void CompleteTransfer(string transferId)
    {
      CloseTransferInternal(transferId, TransferStatus.Completed, null);
    }


    /// <summary>
    /// Aborts a currently managed resource transfer.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <param name="reason">The reason to cancel the transfer.</param>
    public void CancelTransfer(string transferId, AbortReason reason)
    {
      CloseTransferInternal(transferId, TransferStatus.Aborted, reason);
    }


    /// <summary>
    /// Closes a given transfer, and discards the <see cref="DownloadToken"/> that
    /// was maintained.
    /// </summary>
    private void CloseTransferInternal(string transferId, TransferStatus status, AbortReason? abortReason)
    {
      DownloadTransfer transfer = GetCachedTransfer(transferId, true);

      lock (transfer.SyncRoot)
      {
        if (transfer.Stream != null && transfer.Status == TransferStatus.Running)
        {
          //dispose stream
          transfer.Stream.Dispose();
          transfer.Stream = null;
        }

        transfer.Token.Status = status;
        transfer.Token.AbortReason = abortReason;
      }

      Transfers.Remove(transferId);
    }

    #endregion


    #region request / renew token

    /// <summary>
    /// Requests a download token for a given resource.
    /// </summary>
    /// <param name="resourceIdentifier"></param>
    /// <param name="includeFileHash">Whether a file hash for the
    /// requested resource should be calculated and assigned to the
    /// <see cref="DownloadToken.Md5FileHash"/> property of the returned
    /// <see cref="DownloadToken"/>.</param>
    /// <returns></returns>
    public DownloadToken RequestDownloadToken(string resourceIdentifier, bool includeFileHash)
    {
      FileInfo fileInfo = FileResolveFunc(resourceIdentifier);

      if(!fileInfo.Exists)
      {
        string msg = String.Format("Resource [{0}] not found.", resourceIdentifier);
        throw new VirtualResourceNotFoundException(msg);
      }

      string transferId = Guid.NewGuid().ToString();

      DownloadToken dt = new DownloadToken
                           {
                             TransferId = transferId,
                             ResourceIdentifier = resourceIdentifier,
                             CreationTime = SystemTime.Now(),
                             ContentType = ContentUtil.ResolveContentType(fileInfo.Extension),
                             DownloadBlockSize = 512*1024, //TODO configure block size and expiration
                             ResourceName = fileInfo.Name,
                             ResourceLength = fileInfo.Length,
                             ExpirationTime = SystemTime.Now().AddHours(24),
                             Status = TransferStatus.Starting
                           };

      //calculate number of blocks
      long count = dt.ResourceLength/dt.DownloadBlockSize.Value;
      if (dt.ResourceLength % dt.DownloadBlockSize != null) count++;
      dt.TotalBlockCount = count;

      if(includeFileHash)
      {
        dt.Md5FileHash = fileInfo.CalculateMd5Hash();
      }

      var transfer = new DownloadTransfer(dt)
                       {
                         File = fileInfo
                       };

      Transfers.Add(transferId, transfer);
      return dt;
    }

    /// <summary>
    /// Requests a download token based on an older one. This allows to resume a paused
    /// or expired download if the service already discarded the token.
    /// </summary>
    /// <param name="oldToken">The previously used token.</param>
    /// <param name="includeFileHash">Whether a file hash for the
    /// requested resource should be calculated and assigned to the
    /// <see cref="DownloadToken.Md5FileHash"/> property of the returned
    /// <see cref="DownloadToken"/>. Can be used to verify file integrity, and also
    /// indicates whether the file was changed if the old token contained a file hash.</param>
    /// <returns>An updated token with a new expiration time, which reflects the
    /// status updates of the submitted token, including the <see cref="TransferToken.LastTransmittedBlockInfo"/></returns>
    public DownloadToken RenewToken(DownloadToken oldToken, bool includeFileHash)
    {    
      //just create a new token - no adjustments needed with this service implementation
      var token = RequestDownloadToken(oldToken.ResourceIdentifier, includeFileHash);
      token.LastTransmittedBlockInfo = oldToken.LastTransmittedBlockInfo;
      token.LastBlockTransmissionTime = oldToken.LastBlockTransmissionTime;
      return token;
    }

    #endregion



    /// <summary>
    /// Gets a given <see cref="BufferedDataBlock"/> from the currently downloaded
    /// resource.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <param name="blockNumber">The number of the downloaded block.</param>
    /// <returns></returns>
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
      //this func creates the returned DataBlock by reading a chunk of data
      //from the underlying stream.
      Func<DownloadTransfer, long, BufferedDataBlock> func = (dt, position) =>
      {
        DownloadToken token = dt.Token;

        //read data
        bool isLastBlock = false;
        byte[] data = new byte[token.DownloadBlockSize.Value];
        int read = dt.Stream.Read(data, 0, data.Length);
        if (read < token.DownloadBlockSize)
        {
          isLastBlock = true;
          Array.Resize(ref data, read);
        }

        return new BufferedDataBlock
        {
          TransferTokenId = transferId,
          BlockNumber = blockNumber,
          BlockLength = data.Length,
          Offset = position,
          Data = data,
          IsLastBlock = isLastBlock
        };
      };

      return PrepareBlockReading(transferId, blockNumber, func);
    }


    /// <summary>
    /// Handles locking, validation and stream preparation of a given transfer in order to read
    /// a given block of data. The actual reading (either into a buffer, or a returned
    /// stream) is being delegated via the <paramref name="dataReaderFunc"/>.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="IDataBlockInfo"/> that is being returned.</typeparam>
    /// <param name="transferId">The transfer token ID.</param>
    /// <param name="blockNumber">The block number to be read.</param>
    /// <param name="dataReaderFunc">A function that receives the <see cref="DownloadTransfer"/> and
    /// the designated stream offset, and returns the required <see cref="IDataBlockInfo"/> that
    /// provides the block's data.</param>
    /// <returns>The <see cref="IDataBlockInfo"/> that is being created by the <paramref name="dataReaderFunc"/>.</returns>
    private T PrepareBlockReading<T>(string transferId, long blockNumber, Func<DownloadTransfer, long, T> dataReaderFunc) where T:IDataBlockInfo
    {
      DownloadTransfer transfer = GetCachedTransfer(transferId, true);
      DownloadToken token = transfer.Token;

      if (!File.Exists(transfer.File.FullName))
      {
        string msg = "Resource [{0}] of transfer [{1}] was not found.";
        msg = String.Format(msg, transfer.Token.ResourceName, transferId);
        throw new VirtualResourceNotFoundException(msg);
      }

      lock (transfer.SyncRoot)
      {
        //make sure the transfer is active
        if (!transfer.Status.Is(TransferStatus.Starting, TransferStatus.Running, TransferStatus.Paused))
        {
          string msg = String.Format("Transfer is not active anymore - status is [{0}].", transfer.Status);
          throw new TransferStatusException(msg);
        }

        long position = blockNumber * token.DownloadBlockSize.Value;

        //in case of an invalid position, throw error
        if (position > token.ResourceLength)
        {
          string msg = "Cannot deliver block {0} - invalid block number.";
          msg = String.Format(msg, blockNumber);
          throw new DataBlockException(msg);
        }


        if (transfer.Stream == null)
        {
          //open stream, share read access
          transfer.Stream = transfer.File.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        //reposition stream if necessary
        if (position != transfer.Stream.Position)
        {
          transfer.Stream.Seek(position, SeekOrigin.Begin);
        }

        T dataBlock = dataReaderFunc(transfer, position);

        if (dataBlock.IsLastBlock)
        {
          //assume the last block will be processed successfully and
          //already close the stream (don't wait for the transfer to be
          //close by client)
          transfer.Stream.Close();
          transfer.Stream.Dispose();
          transfer.Stream = null;
        }

        //update status
        transfer.Token.Status = TransferStatus.Running;

        //maintain local (unshared) copy of the block info without the data
        transfer.Token.LastTransmittedBlockInfo = DataBlockInfo.FromDataBlock(dataBlock);
        return dataBlock;
      }
    }



    /// <summary>
    /// Reads a block via a streaming channel, which enables a more resource friendly
    /// data transmission (compared to sending the whole data of the block at once).
    /// </summary>
    /// <param name="transferId"></param>
    /// <param name="blockNumber"></param>
    /// <returns></returns>
    public StreamedDataBlock ReadBlockStreamed(string transferId, long blockNumber)
    {
      //this func creates the returned DataBlock by reading a chunk of data
      //from the underlying stream.
      Func<DownloadTransfer, long, StreamedDataBlock> func = (dt, position) =>
      {
        DownloadToken token = dt.Token;

        //check if we can use the max block size
        long streamLength = dt.Stream.Length;
        long blockLength = Math.Min(token.DownloadBlockSize.Value, streamLength - position);
        if (blockLength < 0) blockLength = 0;

        ChunkStream stream = new ChunkStream(dt.Stream, blockLength, position, false);
        
        //if we're reading to the end of the stream, we're done
        bool isLastBlock = position + blockLength == streamLength;

        return new StreamedDataBlock
        {
          TransferTokenId = transferId,
          BlockNumber = blockNumber,
          BlockLength = blockLength,
          Offset = position,
          Data = stream,
          IsLastBlock = isLastBlock
        };
      };

      return PrepareBlockReading(transferId, blockNumber, func);
    }

    /// <summary>
    /// Aborts all running transfers for a given resource, e.g. for aborting running
    /// transfers before deleting or modifying a resource.
    /// </summary>
    /// <param name="resourceId"></param>
    public IEnumerable<DownloadToken> GetTransfersForResource(string resourceId)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Gets one stream that allows reading the whole resource.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <returns>A stream that provides read access to the requested resource's data.</returns>
    /// <remarks>This implementation piggy-backs on the capabilities to read file in blocks.
    /// This causes overhead (we could just return the stream), but has the advantage that
    /// if the transfer is aborted (e.g. because a file lock expires), the underlying resource
    /// is immediately unlocked, because the returned stream does not directly access the
    /// resource data, but merely reads block after block via <see cref="ReadBlock"/>.</remarks>
    public Stream ReadResourceStreamed(string transferId)
    {
//      //TODO in providers stream getting method - just forward here
//      IDownloadTransferService srv = this...;
//      var token = srv.RequestDownloadToken(resourceId, false);
//      return srv.ReadResourceStreamed(token.TransferId);

      DownloadTransfer transfer = GetCachedTransfer(transferId, true);
      var stream = new StreamedBlockInputStream(blockNumber => ReadBlockStreamed(transferId, blockNumber), transfer.File.Length);
      return stream;
    }

  }
}
