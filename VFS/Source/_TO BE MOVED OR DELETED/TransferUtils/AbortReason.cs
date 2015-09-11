namespace Vfs.Transfer
{
  /// <summary>
  /// Indicates why a transmission was aborted by a <see cref="ITransferService"/>.
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
    ResourceNotAccessible = 3

  }
}