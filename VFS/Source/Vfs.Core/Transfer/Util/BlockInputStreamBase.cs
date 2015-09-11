using System;
using System.IO;

namespace Vfs.Transfer.Util
{
  /// <summary>
  /// A base class for the <see cref="StreamedBlockInputStream"/> and
  /// <see cref="BufferedBlockInputStream"/> classes that provides plumbing
  /// and verification code.
  /// </summary>
  /// <typeparam name="T">The underlying block type.</typeparam>
  public abstract class BlockInputStreamBase<T> : Stream where T:IDataBlock
  {
    protected long? TotalLength { get; set; }

    /// <summary>
    /// The resource posi
    /// </summary>
    public long TotalBytesDelivered { get; set; }

    /// <summary>
    /// The currently read data block.
    /// </summary>
    public T CurrentBlock { get; set; }

    /// <summary>
    /// The current position within the <see cref="CurrentBlock"/>.
    /// </summary>
    public int CurrentBlockPosition { get; set; }

    /// <summary>
    /// A func that is used to retrieve the next block to be read.
    /// </summary>
    public Func<long, T> ReceiverFunc { get; private set; }

    /// <summary>
    /// Sets the transfer object, which is being queried for its status before performing
    /// stream access.
    /// </summary>
    public ITransfer Transfer { get; set; }

    public override bool CanRead
    {
      get { return true; }
    }

    public override bool CanSeek
    {
      get { return false; }
    }

    public override bool CanWrite
    {
      get { return false; }
    }

    public override long Length
    {
      get
      {
        if(TotalLength == null) throw new NotSupportedException();
        return TotalLength.Value;
      }
    }

    public override long Position
    {
      get { return TotalBytesDelivered; }
      set { throw new NotSupportedException(); }
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.IO.Stream"/> class. 
    /// </summary>
    protected BlockInputStreamBase(Func<long, T> receiverFunc, long? totalLength)
    {
      TotalLength = totalLength;
      ReceiverFunc = receiverFunc;
    }


    /// <summary>
    /// Validates the status of the attached <see cref="Transfer"/>, and throws
    /// a <see cref="TransferStatusException"/> if the transfer is no longer
    /// active (either <see cref="TransferStatus.Running"/> or
    /// <see cref="TransferStatus.Starting"/>).
    /// </summary>
    /// <remarks>It is expected that paused transfers are being automatically
    /// reset to <see cref="TransferStatus.Running"/>. If the status is still
    /// <see cref="TransferStatus.Paused"/>, there's a status update missing.</remarks>
    protected void VerifyTransferIsActive()
    {
      if (Transfer == null) return;

      if (!Transfer.Status.Is(TransferStatus.Running, TransferStatus.Starting))
      {
        //do not lock the transfer here - locking the transfer for the whole reading period
        //might take a long time, as we don't know where the blocks come from
        string msg = "Could not read data for resource [{0}] - transfer is no longer active (status is [{1}]).";
        msg = String.Format(msg, Transfer.Token.ResourceName, Transfer.Status);

        if(Transfer.AbortReason.HasValue)
        {
          msg += String.Format(" Transfer abort reason: [{0}]", Transfer.AbortReason);
        }

        throw new TransferStatusException(msg);
      }
    }


    public override void Flush()
    {
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
      throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      throw new NotSupportedException();
    }

  }
}