using System.IO;

namespace Vfs.Transfer
{
  /// <summary>
  /// A data block that provides streaming capabilities rather
  /// than submitting the whole data as a buffer.
  /// </summary>
  public class StreamedDataBlock :IDataBlock
  {
    /// <summary>
    /// Gets the underlying file data as a stream.
    /// </summary>
    public Stream Data { get; set; }

    /// <summary>
    /// Identifies the <see cref="TransferToken"/> to which
    /// this file block belongs to.
    /// </summary>
    public string TransferTokenId { get; set; }

    /// <summary>
    /// The transferred block number.
    /// </summary>
    public long BlockNumber { get; set; }

    /// <summary>
    /// The resource's offset that defines the starting
    /// point of the block.
    /// </summary>
    public long Offset { get; set; }

    /// <summary>
    /// Whether this is the last block that completes
    /// the transmission.
    /// </summary>
    public bool IsLastBlock { get; set; }

    /// <summary>
    /// The submitted block length. This is the length
    /// of the block data (stream length or the length of the
    /// <see cref="BufferedDataBlock.Data"/> buffer). In case of an upload
    /// scenario, it should not be bigger than the
    /// <see cref="UploadToken.MaxBlockSize"/>.<br/>
    /// Returns null if the block length is unknown (streamed).
    /// </summary>
    public int? BlockLength { get; set; }

  }
}