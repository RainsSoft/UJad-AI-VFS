using System;
using System.IO;

namespace Vfs.Transfer.Util
{
  /// <summary>
  /// A stream that reads data blocks, and
  /// outputs the received data as a continuous
  /// stream, allowing transparent reading of data.
  /// </summary>
  public class BufferedBlockInputStream : BlockInputStreamBase<BufferedDataBlock>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.IO.Stream"/> class. 
    /// </summary>
    public BufferedBlockInputStream(Func<long, BufferedDataBlock> receiverFunc, long? totalLength)
      : base(receiverFunc, totalLength)
    {
    }


    /// <summary>
    /// When overridden in a derived class, reads a sequence of bytes from the current
    /// stream and advances the position within the stream by the number of bytes read.
    /// </summary>
    /// <returns>
    /// The total number of bytes read into the buffer.
    /// This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
    /// </returns>
    /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes read from the current source. 
    /// </param><param name="offset">The zero-based byte offset in <paramref name="buffer"/>
    ///  at which to begin storing the data read from the current stream. 
    /// </param><param name="count">The maximum number of bytes to be read from the current stream. 
    /// </param><exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length. 
    /// </exception><exception cref="T:System.ArgumentNullException"><paramref name="buffer"/> is null. 
    /// </exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative. 
    /// </exception><exception cref="T:System.IO.IOException">An I/O error occurs. 
    /// </exception><exception cref="T:System.NotSupportedException">The stream does not support reading. 
    /// </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. 
    /// </exception><filterpriority>1</filterpriority>
    public override int Read(byte[] buffer, int offset, int count)
    {
      VerifyTransferIsActive();

      //get the first block
      if (CurrentBlock == null) CurrentBlock = ReceiverFunc(0);

      //if no block is received, assume an empty file
      if(CurrentBlock == null)
      {
        return 0;
      }

      //create a memory stream in order to write to the buffer
      using (MemoryStream stream = new MemoryStream(buffer))
      {
        //start at the offset position
        stream.Position = offset;

        int readBytes = 0;
        while (count > 0)
        {
          int remaining = CurrentBlock.Data.Length - CurrentBlockPosition;

          if (remaining == 0)
          {
            //there's no more data to be read here
            if (CurrentBlock.IsLastBlock) break;

            //get the next block
            CurrentBlockPosition = 0;
            CurrentBlock = ReceiverFunc(CurrentBlock.BlockNumber + 1);
            remaining = CurrentBlock.Data.Length;
          }

          int readChunk = Math.Min(remaining, count);

          //write all bytes and update total
          stream.Write(CurrentBlock.Data, CurrentBlockPosition, readChunk);
          readBytes += readChunk;

          //update current block posistion
          CurrentBlockPosition += readChunk;

          //reset bytes to read
          count -= readChunk;
        }

        TotalBytesDelivered += readBytes;
        return readBytes;
      }
    }

  }
}