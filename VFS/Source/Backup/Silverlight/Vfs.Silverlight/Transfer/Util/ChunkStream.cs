using System;
using System.IO;


namespace Vfs.Transfer.Util
{
  /// <summary>
  /// A stream that provides streamed access to a given range within
  /// another stream, but does not allow reading or writing outside
  /// this block's boundaries.
  /// </summary>
  public class ChunkStream : Stream
  {
    /// <summary>
    /// The stream from which the data is read.
    /// </summary>
    public Stream DecoratedStream { get; private set; }

    /// <summary>
    /// The offset (starting point) of the chunk in the
    /// decorated stream.
    /// </summary>
    public long Offset { get; private set; }

    /// <summary>
    /// The size of the chunk. This is this stream's length.
    /// </summary>
    public long ChunkSize { get; set; }

    private long position;

    /// <summary>
    /// The virtual position within the chunk. The current position
    /// within the <see cref="DecoratedStream"/> is
    /// <see cref="Offset"/> + <see cref="Position"/>. 
    /// </summary>
    public override long Position
    {
      get { return position; }
      set
      {
        if(position < 0 || position > ChunkSize)
        {
          string msg = "Cannot set Position property to '{0}': Position cannot be negative or bigger than the chunk size of [{1}] bytes.";
          msg = String.Format(msg, value, ChunkSize);
          throw new ArgumentOutOfRangeException("value", msg);
        }
        position = value;

        //not a problem if we set the position above the stream length
        //common implementations just correct it
        DecoratedStream.Position = Offset + value;
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.IO.Stream"/> class. 
    /// </summary>
    public ChunkStream(Stream decoratedStream, long chunkSize, long offset, bool initStreamPosition)
    {
      DecoratedStream = decoratedStream;
      ChunkSize = chunkSize;
      Offset = offset;

      if (initStreamPosition)
      {
        decoratedStream.Position = Offset;
      }
    }

    /// <summary>
    /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
    /// </summary>
    /// <returns>
    /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
    /// </returns>
    /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes read from the current source. 
    /// </param><param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin storing the data read from the current stream. 
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
      ValidateReadWriteParams(buffer, offset, count);

      //do not read further than the chunk
      int bytesToRead = (int)Math.Min(count, ChunkSize - Position);

      //if we do not have anything to read, don't read at all - this closes underlying stream
      //implementations!!!
      if(bytesToRead == 0) return 0;
      
      int receivedBytes = DecoratedStream.Read(buffer, offset, bytesToRead);
      
      //update received bytes
      position += receivedBytes;

      return receivedBytes;
    }


    /// <summary>
    /// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
    /// </summary>
    /// <param name="buffer">An array of bytes. This method copies <paramref name="count"/> bytes from <paramref name="buffer"/> to the current stream. 
    /// </param><param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin copying bytes to the current stream. 
    /// </param><param name="count">The number of bytes to be written to the current stream. 
    /// </param><exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is greater than the buffer length. 
    /// </exception><exception cref="T:System.ArgumentNullException"><paramref name="buffer"/> is null. 
    /// </exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative. 
    /// </exception><exception cref="T:System.IO.IOException">An I/O error occurs. Also thrown if an attempt to write
    /// beyond the block size is made.
    /// </exception><exception cref="T:System.NotSupportedException">The stream does not support writing. 
    /// </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. 
    /// </exception><filterpriority>1</filterpriority>
    public override void Write(byte[] buffer, int offset, int count)
    {
      ValidateReadWriteParams(buffer, offset, count);

      long remaining = ChunkSize - Position;
      if(count > remaining)
      {
        string msg = "Blocked attempt to write block of [{0}] bytes - write goes beyond the current chunk. Chunk size is [{1}], stream position is [{2}].";
        msg = String.Format(msg, count, ChunkSize, Position);
        throw new IOException(msg);
      }

      //do not read further than the chunk
      int bytesToWrite = (int)Math.Min(count, ChunkSize - Position);

      //if there is nothing to write, really, don't invoke the stream
      if (bytesToWrite == 0) return;

      //write data and advance position
      DecoratedStream.Write(buffer, offset, bytesToWrite);
      position += bytesToWrite;
    }


    private static void ValidateReadWriteParams(byte[] buffer, int offset, int count)
    {
      if (buffer == null) throw new ArgumentNullException("buffer");

      if (offset < 0)
      {
        throw new ArgumentOutOfRangeException("offset", "Offset cannot be negative.");
      }
      if (count < 0)
      {
        throw new ArgumentOutOfRangeException("count", "Number of bytes to read cannot be negative.");
      }
      if ((buffer.Length - offset) < count)
      {
        throw new ArgumentException("Invalid offset length.");
      }
    }


    public override void Flush()
    {
      DecoratedStream.Flush();
    }

    /// <summary>
    /// When overridden in a derived class, sets the position within the current stream.
    /// </summary>
    /// <returns>
    /// The new position within the current stream.
    /// </returns>
    /// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter. 
    /// </param><param name="origin">A value of type <see cref="T:System.IO.SeekOrigin"/> indicating the reference point used to obtain the new position. 
    /// </param><exception cref="T:System.IO.IOException">An I/O error occurs. 
    /// </exception><exception cref="T:System.NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output. 
    /// </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. 
    /// </exception><filterpriority>1</filterpriority>
    public override long Seek(long offset, SeekOrigin origin)
    {
      long proposedPosition;

      switch(origin)
      {
        case SeekOrigin.Begin:
          proposedPosition = offset;
          break;
        case SeekOrigin.Current:
          proposedPosition = Position + offset;
          break;
        case SeekOrigin.End:
          proposedPosition = ChunkSize + offset;
          break;
        default:
          throw new ArgumentOutOfRangeException("origin");
      }

      if(proposedPosition < 0 || proposedPosition > ChunkSize)
      {
        throw new IOException("Invalid offset with regards to origin");
      }

      Position = proposedPosition;
      return proposedPosition;
    }

    public override void SetLength(long value)
    {
      throw new NotSupportedException();
    }


    public override bool CanRead
    {
      get { return DecoratedStream.CanRead; }
    }

    public override bool CanSeek
    {
      get { return DecoratedStream.CanSeek; }
    }

    public override bool CanWrite
    {
      get { return DecoratedStream.CanWrite; }
    }

    public override long Length
    {
      get { return ChunkSize; }
    }


  }
}