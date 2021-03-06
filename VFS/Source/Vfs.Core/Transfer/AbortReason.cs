namespace Vfs.Transfer
{
  /// <summary>
  /// Indicates why a transmission was aborted by a <see cref="ITransferHandler"/>.
  /// This property should be set if the <see cref="TransferStatus"/> of a given
  /// transfer is set to <see cref="TransferStatus.Aborted"/>.
  /// </summary>
  public enum AbortReason
  {
    /// <summary>
    /// The reason is undefined.
    /// </summary>
    Undefined = 0,
    /// <summary>
    /// The transfer was aborted because the transmitted resource is no longer available.
    /// This flag may also be used if the resource was just moved.
    /// </summary>
    ResourceNoLongerAvailable = 1,
    /// <summary>
    /// The transfer was aborted because the transmitted resource was modified. A retransmission
    /// might be performed.
    /// </summary>
    ResourceModified = 2,
    /// <summary>
    /// Access to the resource was denied.
    /// </summary>
    ResourceNotAccessible = 3,
    /// <summary>
    /// The transfer was closed because we are past its expiration date.
    /// </summary>
    Expired = 4,
    /// <summary>
    /// The final verification of the transfer (using a file hash) showed that
    /// data was not transmitted correctly.
    /// </summary>
    VerificationFailure = 5,
    /// <summary>
    /// The client that requested the resource aborted the transfer (e.g. due
    /// to user interaction).
    /// </summary>
    ClientAbort = 6
  }
}