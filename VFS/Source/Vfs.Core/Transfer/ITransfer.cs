using System.Collections.Generic;
using System.Security.Principal;
using Vfs.Locking;
using Vfs.Scheduling;

namespace Vfs.Transfer
{
  /// <summary>
  /// Common interface for both upload and download transfers.
  /// </summary>
  public interface ITransfer
  {
    /// <summary>
    /// Identifies the owning user identity. This optional property
    /// can be used in order to secure transfer-related requests.
    /// </summary>
    IIdentity Owner { get; }

    /// <summary>
    /// Gets the currently transferred item.
    /// </summary>
    IVirtualFileItem TransferredItem { get; }

    /// <summary>
    /// The underlying download or upload token, which is shared
    /// with the transferring party.
    /// </summary>
    TransferToken Token { get; }

    /// <summary>
    /// Contains the resource locks which is used to ensure access to
    /// the resource. Also used to abort a transfer if the
    /// lock expired.
    /// </summary>
    ResourceLockGuard ResourceLock { get; set; }

    /// <summary>
    /// The public transfer status.
    /// </summary>
    TransferStatus Status { get; set; }

    /// <summary>
    /// Indicates why a transfer was aborted. This property
    /// is only set if the <see cref="Status"/> property
    /// is <see cref="TransferStatus.Aborted"/>.
    /// </summary>
    AbortReason? AbortReason { get; set; }

    /// <summary>
    /// Gets the blocks that have been tranferred so far.
    /// </summary>
    IEnumerable<DataBlockInfo> TransferredBlocks { get; }
  }
}