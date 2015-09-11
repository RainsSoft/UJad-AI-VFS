namespace Vfs.Transfer
{
  /// <summary>
  /// Indicates transmission / retransmission capabilities of a given
  /// <see cref="ITransferService"/>.
  /// </summary>
  public enum TransmissionCapabilities
  {
    /// <summary>
    /// Transmission has to occur sequentially (starting with block 0, followed by
    /// block 1, 2 etc.). Retransmission of already transmitted
    /// blocks is not possible.
    /// </summary>
    SequentialStrict = 0,
    /// <summary>
    /// Sequential transmission of blocks (starting with block 0, followed by
    /// block 1, 2 etc.). Retransmitting already transmitted blocks is possible.
    /// </summary>
    SequentialWithRetransmission = 1,
    /// <summary>
    /// Blocks can be transmitted completely randomly.
    /// </summary>
    Random = 2
  }
}