namespace Vfs.Transfer
{
  /// <summary>
  /// Meta data about a given block of data which is part of a resource transfer.
  /// </summary>
  public interface IDataBlock
  {
    /// <summary>
    /// Identifies the <see cref="TransferToken"/> to which
    /// this file block belongs to.
    /// </summary>
    string TransferTokenId { get; set; }

    /// <summary>
    /// The transferred block number.
    /// </summary>
    long BlockNumber { get; set; }

    /// <summary>
    /// The resource's offset that defines the starting
    /// point of the block.
    /// </summary>
    long Offset { get; set; }

    /// <summary>
    /// Whether this is the last block that completes
    /// the transmission. This is not necessarily the
    /// block with the highest <see cref="IDataBlock.BlockNumber"/>,
    /// but just the last block of the transfer (e.g. in case of
    /// random transmission order).<br/>
    /// In case of upload scenarios, a transferred block that has this flag
    /// set to true should automatically complete a transfer.
    /// </summary>
    bool IsLastBlock { get; set; }

    /// <summary>
    /// The submitted block length. This is the length
    /// of the block data (stream length or the length of the
    /// <see cref="BufferedDataBlock.Data"/> buffer). In case of an upload
    /// scenario, it should not be bigger than the
    /// <see cref="UploadToken.MaxBlockSize"/>.<br/>
    /// Returns null if the block length is unknown (streamed).
    /// </summary>
    int? BlockLength { get; set; }
  }
}