using Vfs.Transfer;

namespace Vfs
{
  /// <summary>
  /// Common fault types, that match the core's VFS exceptions. Used in order
  /// to transfer fault information in disconnected scenarios.
  /// </summary>
  public enum VfsFaultType
  {
    /// <summary>
    /// The fault is not defined.
    /// </summary>
    Undefined = 0,
    /// <summary>
    /// A resource wasn't found. This fault matches the
    /// <see cref="VirtualResourceNotFoundException"/>.
    /// </summary>
    ResourceNotFound,
    /// <summary>
    /// Common resource access error. This fault matches
    /// the <see cref="ResourceAccessException"/>.
    /// </summary>
    ResourceAccess,
    /// <summary>
    /// An issue occurred when attempting to overwrite a resource,
    /// or because a resource cannot or should not be overwritten.
    /// This fault matches the <see cref="ResourceOverwriteException"/>.
    /// </summary>
    ResourceOverwrite,
    /// <summary>
    /// A resource is locked and cannot be accessed as requested.
    /// This fault matches the <see cref="ResourceLockedException"/>.
    /// </summary>
    ResourceLocked,
    /// <summary>
    /// An invalid resource path was processed. This fault matches
    /// the <see cref="InvalidResourcePathException"/>.
    /// </summary>
    ResourcePathInvalid,
    /// <summary>
    /// Common transfer error. This fault matches
    /// the <see cref="TransferException"/>.
    /// </summary>
    TransferError,
    /// <summary>
    /// A request for an unknown transfer was processed. This fault matches
    /// the <see cref="UnknownTransferException"/>.
    /// </summary>
    TransferUnknown,
    /// <summary>
    /// A transfer was processed that does no longer have a valid status.
    /// This fault matches the <see cref="TransferStatusException"/>.
    /// </summary>
    TransferStatusError,
    /// <summary>
    /// A fault occurred while processing or creating a block of data during
    /// a running transfer.
    /// </summary>
    DataBlockError
  }
}
