namespace Vfs.Transfer
{
  /// <summary>
  /// State flags for a given file transfer.
  /// </summary>
  public enum TransferStatus
  {
    Undefined = 0,
    /// <summary>
    /// The transfer has been created but not started.
    /// </summary>
    Starting = 1,
    /// <summary>
    /// Transfer is currently in progress.
    /// </summary>
    Running = 2,
    /// <summary>
    /// The transfer was successfully completed.
    /// </summary>
    Completed = 3,
    /// <summary>
    /// The transfer has been paused and waits to be
    /// resumed.
    /// </summary>
    Paused = 4,
    /// <summary>
    /// The transfer has been aborted.
    /// </summary>
    Aborted = 5,
    /// <summary>
    /// The transfer is unknown.
    /// </summary>
    UnknownTransfer = 99,
  }
}