using System;

namespace Vfs.Transfer
{
  /// <summary>
  /// Encapsulates a partial block of data of a given transfer.
  /// </summary>
  public class DataBlockInfo : IDataBlock
  {
    /// <summary>
    /// Identifies the <see cref="TransferToken"/> to which
    /// this file block belongs to.
    /// </summary>
    public virtual string TransferTokenId { get; set; }

    /// <summary>
    /// The transferred block number.
    /// </summary>
    public virtual long BlockNumber { get; set; }

    /// <summary>
    /// The submitted block length. This is the length
    /// of the block's data (stream length or the length of the
    /// <see cref="BufferedDataBlock.Data"/> buffer). In case of an upload
    /// scenario, it should not be bigger than the
    /// <see cref="UploadToken.MaxBlockSize"/>.<br/>
    /// Returns null if the block length is unknown (streamed).
    /// </summary>
    public virtual int? BlockLength { get; set; }

    /// <summary>
    /// The resource's offset that defines the starting
    /// point of the block.
    /// </summary>
    public virtual long Offset { get; set; }

    /// <summary>
    /// Whether this is the last block that completes
    /// the transmission. This is not necessarily the
    /// block with the highest <see cref="IDataBlock.BlockNumber"/>,
    /// but just the last transmitted block (e.g. in case of
    /// random transmission order).
    /// </summary>
    public virtual bool IsLastBlock { get; set; }


    /// <summary>
    /// Creates an independent <see cref="DataBlockInfo"/> instance
    /// base on another one.
    /// </summary>
    /// <param name="other">The object to copy.</param>
    /// <returns>Independent <see cref="DataBlockInfo"/> whose
    /// properties match the submitted instance.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="other"/>
    /// is a null reference.</exception>
    public static DataBlockInfo FromDataBlock(IDataBlock other)
    {
      Ensure.ArgumentNotNull(other, "other");

      return new DataBlockInfo
               {
                 TransferTokenId = other.TransferTokenId,
                 BlockNumber = other.BlockNumber,
                 BlockLength = other.BlockLength,
                 Offset = other.Offset,
                 IsLastBlock = other.IsLastBlock
               };
    }

  }
}