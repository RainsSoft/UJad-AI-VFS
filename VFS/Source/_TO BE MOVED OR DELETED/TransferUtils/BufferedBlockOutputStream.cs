using System;
using System.Collections.Generic;
using System.IO;


namespace Vfs.Transfer
{
  /// <summary>
  /// An output stream that creates <see cref="BufferedDataBlock"/>
  /// instances based on data that is written to the stream.
  /// </summary>
  public class BufferedBlockOutputStream : Stream
  {
    private readonly object syncRoot = new object();

    /// <summary>
    /// Internal buffer.
    /// </summary>
    private readonly List<byte> transmissionBuffer = new List<byte>();

    /// <summary>
    /// The <see cref="BufferedDataBlock.BlockNumber"/> of the last
    /// transmitted block. Initialized with the
    /// <see cref="UploadToken.TransmittedBlockCount"/> - 1
    /// property value of the submitted <see cref="Token"/>.
    /// </summary>
    public long? LastTransmittedBlockNumber { get; private set; }

    /// <summary>
    /// The initial offset that used to calculate the
    /// <see cref="BufferedDataBlock.Offset"/> property. This value was taken
    /// from the <see cref="TransferToken.NextBlockOffset"/>
    /// property during initialization.
    /// </summary>
    public long InitialOffset { get; private set; }

    /// <summary>
    /// Indicates how many bytes have been submitted as
    /// <see cref="BufferedDataBlock"/> instances so far.
    /// </summary>
    public long WrittenBytes { get; private set; }

    /// <summary>
    /// The token that provides the information to create the
    /// <see cref="BufferedDataBlock"/> instances that are being
    /// written to the 
    /// </summary>
    public UploadToken Token { get; private set; }

    /// <summary>
    /// An action that is being invoked with created
    /// <see cref="BufferedDataBlock"/> instances when flushing
    /// the internal buffer.
    /// </summary>
    /// <remarks>Decided to use an action rather than a function with
    /// a status value because I wanted exceptions to hit through.
    /// Silently swallowing errors might lead to massive memory
    /// leaks due to indefinitely growing internal buffer.</remarks>
    public Action<BufferedDataBlock> OutputAction { get; private set; }

    /// <summary>
    /// A threshold that causes the stream to aumatically flush
    /// its internal buffer once it reaches the defined size.<br/>
    /// If the threshold is 0, every write is immediately flushed.
    /// </summary>
    public int AutoFlushThreshold { get; private set; }

    public int InternalBufferSize
    {
      get
      {
        lock(syncRoot)
        {
          return transmissionBuffer.Count;
        }
      }
    }




    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.IO.Stream"/> class. 
    /// </summary>
    /// <param name="autoFlushThreshold">A threshold that causes the stream to aumatically flush
    /// its internal buffer once it reaches the defined size.<br/>
    /// If the threshold is 0, every write is immediately flushed.</param>
    /// <param name="outputAction">An action that is being invoked with created
    /// <see cref="BufferedDataBlock"/> instances when flushing the internal buffer.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="token"/> or <paramref name="outputAction"
    /// is a null reference.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Inf case the <paramref name="autoFlushThreshold"/>
    /// is negative.</exception>
    public BufferedBlockOutputStream(UploadToken token, int autoFlushThreshold, Action<BufferedDataBlock> outputAction)
    {
      if (token == null) throw new ArgumentNullException("token");
      if (outputAction == null) throw new ArgumentNullException("outputAction");
      if(autoFlushThreshold < 0)
      {
        throw new ArgumentOutOfRangeException("autoFlushThreshold",
                                              "Threshold for automatic flushing cannot be negative");
      }

      Token = token;
      AutoFlushThreshold = autoFlushThreshold;
      OutputAction = outputAction;
      InitialOffset = token.NextBlockOffset;
    }
    

    /// <summary>
    /// Creates <see cref="BufferedDataBlock"/> packages for the internal buffer
    /// which can be submitted.
    /// </summary>
    public override void Flush()
    {
      FlushInternal(false);
    }


    private void FlushInternal(bool isAutoFlush)
    {
      //on autoflush, do not send blocks smaller than the threshold
      long minBufferSize = 0;
      if (isAutoFlush)
      {
        //if auto-flushing, flush as long as we can create full-sized blocks,
        //OR are above the flushing threshold
        minBufferSize = Math.Min(AutoFlushThreshold, Token.MaxBlockSize ?? 0);
        
        //make sure we don't have a negative value
        minBufferSize = Math.Max(minBufferSize, 0);
      }
      
      lock (syncRoot)
      {
        if (transmissionBuffer.Count == 0) return;


        while (transmissionBuffer.Count > 0 && transmissionBuffer.Count >= minBufferSize)
        {
          //the blockSize is an int - the buffer can't be bigger anyway
          int blockSize = (int)(Math.Min(transmissionBuffer.Count, Token.MaxBlockSize ?? transmissionBuffer.Count));
          byte[] data = new byte[blockSize];
          transmissionBuffer.CopyTo(0, data, 0, blockSize);

          long blockNumber = LastTransmittedBlockNumber.HasValue
                               ? LastTransmittedBlockNumber.Value + 1
                               : Token.TransmittedBlockCount;

          BufferedDataBlock block = new BufferedDataBlock
                              {
                                TransferTokenId = Token.TransferId,
                                BlockNumber = blockNumber,
                                BlockLength = blockSize,
                                Data = data,
                                Offset = InitialOffset + WrittenBytes
                              };

          //write block
          OutputAction(block);

          //if not exception occurred, update state, remove data from buffer and continue
          transmissionBuffer.RemoveRange(0, blockSize);
          WrittenBytes += blockSize;
          LastTransmittedBlockNumber = blockNumber;
        }
      }
    }


    /// <summary>
    /// When overridden in a derived class, sets the position within the current stream.
    /// </summary>
    /// <returns>
    /// The new position within the current stream.
    /// </returns>
    /// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter. 
    ///                 </param><param name="origin">A value of type <see cref="T:System.IO.SeekOrigin"/> indicating the reference point used to obtain the new position. 
    ///                 </param><exception cref="T:System.IO.IOException">An I/O error occurs. 
    ///                 </exception><exception cref="T:System.NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output. 
    ///                 </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. 
    ///                 </exception><filterpriority>1</filterpriority>
    public override long Seek(long offset, SeekOrigin origin)
    {
      throw new NotSupportedException();
    }

    /// <summary>
    /// When overridden in a derived class, sets the length of the current stream.
    /// </summary>
    /// <param name="value">The desired length of the current stream in bytes. 
    ///                 </param><exception cref="T:System.IO.IOException">An I/O error occurs. 
    ///                 </exception><exception cref="T:System.NotSupportedException">The stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output. 
    ///                 </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. 
    ///                 </exception><filterpriority>2</filterpriority>
    public override void SetLength(long value)
    {
      throw new NotSupportedException();
    }

    /// <summary>
    /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
    /// </summary>
    /// <returns>
    /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
    /// </returns>
    /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes read from the current source. 
    ///                 </param><param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin storing the data read from the current stream. 
    ///                 </param><param name="count">The maximum number of bytes to be read from the current stream. 
    ///                 </param><exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length. 
    ///                 </exception><exception cref="T:System.ArgumentNullException"><paramref name="buffer"/> is null. 
    ///                 </exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative. 
    ///                 </exception><exception cref="T:System.IO.IOException">An I/O error occurs. 
    ///                 </exception><exception cref="T:System.NotSupportedException">The stream does not support reading. 
    ///                 </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. 
    ///                 </exception><filterpriority>1</filterpriority>
    public override int Read(byte[] buffer, int offset, int count)
    {
      throw new NotSupportedException();
    }


    /// <summary>
    /// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
    /// </summary>
    /// <param name="buffer">An array of bytes. This method copies <paramref name="count"/> bytes from <paramref name="buffer"/> to the current stream. 
    ///</param><param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin copying bytes to the current stream. 
    ///</param><param name="count">The number of bytes to be written to the current stream. 
    ///</param><exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is greater than the buffer length. 
    ///</exception><exception cref="T:System.ArgumentNullException"><paramref name="buffer"/> is null. 
    ///</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative. 
    ///</exception><exception cref="T:System.IO.IOException">An I/O error occurs. 
    ///</exception><exception cref="T:System.NotSupportedException">The stream does not support writing. 
    ///</exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. 
    ///</exception><filterpriority>1</filterpriority>
    /// <remarks>This operation autoflushes once the internal buffer has reached the
    /// <see cref="AutoFlushThreshold"/>. which is a blocking method.</remarks>
    public override void Write(byte[] buffer, int offset, int count)
    {
      if (buffer == null) throw new ArgumentNullException("buffer");
      if (offset < 0 || offset > buffer.Length)
      {
        throw new ArgumentOutOfRangeException("offset", "Invalid offset: " + offset);
      }

      if (count < 0 || count > buffer.Length)
      {
        throw new ArgumentOutOfRangeException("count", "Invalid number of bytes to copy: " + offset);
      }

      if(count + offset > buffer.Length)
      {
        throw new ArgumentOutOfRangeException("count", "Offset and count exceed the submitted buffer size.");
      }


      lock(syncRoot)
      {
        if (offset == 0 && count == buffer.Length)
        {
          //copy the whole buffer
          transmissionBuffer.AddRange(buffer);
        }
        else if(count > 0)
        {
          int start = offset;
          int end = offset + count;
          for (int i = start; i < end; i++)
          {
            transmissionBuffer.Add(buffer[i]);
          }
        }

        //flush buffer (makes sense to implement blocking, or our buffer might fill up)
        if(transmissionBuffer.Count >= AutoFlushThreshold)
        {
          FlushInternal(true);
        }
      }
    }

    /// <summary>
    /// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
    /// </summary>
    /// <returns>
    /// true if the stream supports reading; otherwise, false.
    /// </returns>
    /// <filterpriority>1</filterpriority>
    public override bool CanRead
    {
      get { return false; }
    }

    /// <summary>
    /// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
    /// </summary>
    /// <returns>
    /// true if the stream supports seeking; otherwise, false.
    /// </returns>
    /// <filterpriority>1</filterpriority>
    public override bool CanSeek
    {
      get { return false; }
    }

    /// <summary>
    /// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
    /// </summary>
    /// <returns>
    /// true if the stream supports writing; otherwise, false.
    /// </returns>
    /// <filterpriority>1</filterpriority>
    public override bool CanWrite
    {
      get { return true; }
    }

    /// <summary>
    /// When overridden in a derived class, gets the length in bytes of the stream.
    /// </summary>
    /// <returns>
    /// A long value representing the length of the stream in bytes.
    /// </returns>
    /// <exception cref="T:System.NotSupportedException">A class derived from Stream does not support seeking. 
    ///                 </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. 
    ///                 </exception><filterpriority>1</filterpriority>
    public override long Length
    {
      get
      {
        lock(syncRoot)
        {
          return WrittenBytes;
        }
      }
    }

    /// <summary>
    /// When overridden in a derived class, gets or sets the position within the current stream.
    /// </summary>
    /// <returns>
    /// The current position within the stream.
    /// </returns>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs. 
    ///                 </exception><exception cref="T:System.NotSupportedException">The stream does not support seeking. 
    ///                 </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. 
    ///                 </exception><filterpriority>1</filterpriority>
    public override long Position
    {
      get { return WrittenBytes; }
      set { throw new NotSupportedException("Seeking is not supported."); }
    }
  }
}
