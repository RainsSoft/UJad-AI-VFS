using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Vfs.Transfer.Util;
using Vfs.Util;

namespace Vfs.Transfer
{
  /// <summary>
  /// A helper class that provides extension methods to write
  /// data blocks to underlying streams.
  /// </summary>
  public static class DataBlockWriterExtensions
  {
    /// <summary>
    /// Reads a data block's stream and writes it to a given <paramref name="targetStream"/>.
    /// </summary>
    /// <param name="dataBlock">The data block that provides a chunk of data that should
    /// be written to the <paramref name="targetStream"/>.</param>
    /// <param name="targetStream">The target stream that receives the block's
    /// <see cref="StreamedDataBlock.Data"/>.</param>
    public static void WriteTo(this StreamedDataBlock dataBlock, Stream targetStream)
    {
      targetStream.Position = dataBlock.Offset;
      dataBlock.Data.WriteTo(targetStream);
    }


    /// <summary>
    /// Reads a data block's stream and writes it to a given <paramref name="targetStream"/>.
    /// </summary>
    /// <param name="dataBlock">The data block that provides a chunk of data that should
    /// be written to the <paramref name="targetStream"/>.</param>
    /// <param name="targetStream">The target stream that receives the block's
    /// <see cref="StreamedDataBlock.Data"/>.</param>
    /// <param name="maxStreamSize">The maximum number of bytes that can be written to the destination
    /// stream. If the read stream exceeds this limit, a <see cref="DataBlockException"/> is thrown.</param>
    /// <exception cref="DataBlockException">If the data block's stream length exceeds the
    /// <paramref name="maxStreamSize"/> threshhold.</exception>
    public static void WriteTo(this StreamedDataBlock dataBlock, Stream targetStream, long maxStreamSize)
    {
      //use default byte sizes
      byte[] buffer = new byte[32768];

      long totalBytesRead = 0;

      while (true)
      {
        int bytesRead = dataBlock.Data.Read(buffer, 0, buffer.Length);
        totalBytesRead += bytesRead;

        if(totalBytesRead > maxStreamSize)
        {
          string msg = "The length of the stream of data block number [{0}] for transfer [{1}] exceeds the size limit of [{2}] bytes.";
          msg = String.Format(msg, dataBlock.BlockNumber, dataBlock.TransferTokenId, maxStreamSize);
          throw new DataBlockException(msg);
        }

        if (bytesRead > 0)
        {
          targetStream.Write(buffer, 0, bytesRead);
        }
        else
        {
          targetStream.Flush();
          break;
        }
      }
    }


    /// <summary>
    /// Reads a data block's buffer and writes it to a given <paramref name="targetStream"/>.
    /// </summary>
    /// <param name="dataBlock">The data block that provides a chunk of data that should
    /// be written to the <paramref name="targetStream"/>.</param>
    /// <param name="targetStream">The target stream that receives the block's
    /// <see cref="BufferedDataBlock.Data"/>.</param>
    public static void WriteTo(this BufferedDataBlock dataBlock, Stream targetStream)
    {
      //TODO support variable block length that differs from buffer length:
      //targetStream.Write(dataBlock.Data, 0, dataBlock.BlockLength ?? dataBlock.Data.Length);
      //-> means we also have to change block validation

      targetStream.Position = dataBlock.Offset;
      targetStream.Write(dataBlock.Data, 0, dataBlock.Data.Length);
    }



    /// <summary>
    /// Splits a stream into chunks using a <see cref="ChunkStream"/>, and wraps them into
    /// <see cref="StreamedDataBlock"/> instances that can be sent to an <see cref="IUploadTransferHandler"/>.
    /// This overload writes the whole stream to the target. In order to resume a transfer and start in the
    /// middle of the stream, use the overload of this method that takes an initial block number and offset.
    /// <br/>This extension method also takes care of implicitly completing the upload by setting the
    /// <see cref="IDataBlock.IsLastBlock"/> property of the last block to true.
    /// </summary>
    /// <param name="sourceStream">The source stream that provides the data to be uploaded. It is assumed that
    /// the reading position within the stream has already been set.</param>
    /// <param name="token">An upload token that defines the resource.</param>
    /// <param name="resourceLength">The total length of the submitted stream.</param>
    /// <param name="blockSize">The block size to be used. All blocks (except the last one) will have
    /// this size.</param>
    /// <param name="writerAction">An action that is being invoked for every created <see cref="StreamedDataBlock"/>
    /// instance.</param>
    public static void WriteTo(this Stream sourceStream, UploadToken token, long resourceLength, int blockSize, Action<StreamedDataBlock> writerAction)
    {
      WriteTo(sourceStream, token, resourceLength, blockSize, 0, 0, writerAction);
    }


    /// <summary>
    /// Splits a stream into chunks using a <see cref="ChunkStream"/>, and wraps them into
    /// <see cref="StreamedDataBlock"/> instances that can be sent to an <see cref="IUploadTransferHandler"/>.
    /// This overload allows to resume a transfer and start with a given offset.
    /// <br/>This extension method also takes care of implicitly completing the upload by setting the
    /// <see cref="IDataBlock.IsLastBlock"/> property of the last block to true.
    /// </summary>
    /// <param name="sourceStream">The source stream that provides the data to be uploaded.</param>
    /// <param name="token">An upload token that defines the resource.</param>
    /// <param name="resourceLength">The total length of the submitted stream.</param>
    /// <param name="blockSize">The block size to be used. All blocks (except the last one) will have
    /// this size.</param>
    /// <param name="initialBlockNumber">The initial block number to be used.</param>
    /// <param name="offset">The offset of the first written block (0 to start at the beginning of the stream).</param>
    /// <param name="writerAction">An action that is being invoked for every created <see cref="StreamedDataBlock"/>
    /// instance.</param>
    /// <remarks>The position within the stream is only set according to the submitted
    /// <paramref name="offset"/> if the underlying <paramref name="sourceStream"/> supports seeking as indicated by
    /// its <see cref="Stream.CanSeek"/> property.</remarks>
    public static void WriteTo(this Stream sourceStream, UploadToken token, long resourceLength, int blockSize, long initialBlockNumber, long offset, Action<StreamedDataBlock> writerAction)
    {
      long remaining = resourceLength;
      long position = offset;
      long blockNumber = initialBlockNumber;

      while(remaining > 0)
      {
        //decorate the stream with a chunk stream that limits access to a block of data
        int chunkSize = (int)Math.Min(remaining, blockSize);
        ChunkStream cs = new ChunkStream(sourceStream, chunkSize, position, sourceStream.CanSeek);

        StreamedDataBlock dataBlock = new StreamedDataBlock
                                 {
                                   TransferTokenId = token.TransferId,
                                   BlockLength = chunkSize,
                                   BlockNumber = blockNumber,
                                   Data = cs,
                                   Offset = position
                                 };

        //update position within stream and remaining bytes
        position += chunkSize;
        remaining -= chunkSize;
        blockNumber++;

        if(remaining == 0)
        {
          //implicitly complete the transfer by marking the last block
          dataBlock.IsLastBlock = true;
        }

        writerAction(dataBlock);
      }
    }

  }
}
