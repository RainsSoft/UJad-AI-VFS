namespace Vfs.Transfer
{
  /// <summary>
  /// Indicates transmission / retransmission capabilities of a given
  /// <see cref="ITransferHandler"/>.
  /// </summary>
  public enum TransmissionCapabilities
  {
    /// <summary>
    /// No data can being transmitted. This should obviously not be the case and regarded
    /// an invalid state.
    /// </summary>
    Undefined = 0,
    /// <summary>
    /// Transmission has to occur sequentially (starting with block 0, followed by
    /// block 1, 2 etc.). Retransmission of already transmitted
    /// blocks is not possible.
    /// </summary>
    SequentialStrict = 1,
    /// <summary>
    /// Sequential transmission of blocks (starting with block 0, followed by
    /// block 1, 2 etc.). Retransmitting already transmitted blocks is possible.
    /// </summary>
    SequentialWithRetransmission = 2,
    /// <summary>
    /// Blocks can be transmitted completely randomly.
    /// </summary>
    Random = 3
  }
}