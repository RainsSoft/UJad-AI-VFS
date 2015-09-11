using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Vfs.Locking;
using Vfs.Scheduling;

namespace Vfs.Transfer
{
  /// <summary>
  /// Base class for managed upload and download transfers.
  /// </summary>
  public abstract class TransferBase<TFile, TToken> : ITransfer where TFile:IVirtualFileItem where TToken:TransferToken
  {
    /// <summary>
    /// The currently transferred item.
    /// </summary>
    public TFile FileItem { get; set; }

    /// <summary>
    /// Identifies the owner of this transfer. This optional property
    /// can be used in order to secure transfer-related requests.
    /// </summary>
    public IIdentity Owner { get; set; }

    /// <summary>
    /// Gets the currently transferred item.
    /// </summary>
    IVirtualFileItem ITransfer.TransferredItem
    {
      get { return FileItem; }
    }

    /// <summary>
    /// The underlying download or upload token, which is shared
    /// with the transferring party.
    /// </summary>
    public TToken Token { get; set; }


    /// <summary>
    /// The underlying download or upload token, which is shared
    /// with the transferring party.
    /// </summary>
    TransferToken ITransfer.Token
    {
      get { return Token; }
    }


    /// <summary>
    /// Contains the resource locks which is used to ensure access to
    /// the resource. Also used to abort a transfer if the
    /// lock expired.
    /// </summary>
    public ResourceLockGuard ResourceLock { get; set; }

    /// <summary>
    /// The public transfer status.
    /// </summary>
    public TransferStatus Status { get; set; }

    /// <summary>
    /// Indicates why a transfer was aborted. This property
    /// is only set if the <see cref="Status"/> property
    /// is <see cref="TransferStatus.Aborted"/>.
    /// </summary>
    public AbortReason? AbortReason { get; set; }


    private readonly Dictionary<long, DataBlockInfo> transferredBlocks = new Dictionary<long, DataBlockInfo>();

    /// <summary>
    /// Gets the blocks that have been tranferred so far.
    /// </summary>
    public IEnumerable<DataBlockInfo> TransferredBlocks
    {
      get 
      {
        return transferredBlocks.Values.ToArray();
      }
    }



    /// <summary>
    /// The public transfer ID. This is a convenience
    /// property which just returns the <see cref="TransferToken.TransferId"/>
    /// of the underlying <see cref="Token"/>.
    /// </summary>
    public string TransferId
    {
      get { return Token.TransferId; }
    }


    /// <summary>
    /// A job that will be invoked as soon as the
    /// transfer expires. Used in order to abort a
    /// transfer in time if the transmitting party
    /// does not complete properly.
    /// </summary>
    /// <remarks>The job reference is kept in order
    /// not to have to cancel via the scheduler, which
    /// saves a bit of performance.</remarks>
    public Job ExpirationNotificationJob { get; set; }


    /// <summary>
    /// Synchronization token.
    /// </summary>
    public object SyncRoot { get; private set; }


    protected TransferBase(TToken token, TFile fileItem)
    {
      Token = token;
      FileItem = fileItem;
      SyncRoot = new object();
    }


    /// <summary>
    /// Tries to get an already transferred data block from the
    /// internal cache.
    /// </summary>
    /// <param name="blockNumber">The number of the requested block.</param>
    /// <returns>The requested data block, or null if no such block has been
    /// transferred so far.</returns>
    public DataBlockInfo TryGetTransferredBlock(long blockNumber)
    {
      DataBlockInfo dataBlock;
      transferredBlocks.TryGetValue(blockNumber, out dataBlock);
      return dataBlock;
    }

    /// <summary>
    /// Adds a <see cref="DataBlockInfo"/> instance to the internal
    /// list of registered data blocks.
    /// </summary>
    /// <param name="dataBlock"></param>
    public void RegisterBlock(DataBlockInfo dataBlock)
    {
      Ensure.ArgumentNotNull(dataBlock, "dataBlock");
      transferredBlocks[dataBlock.BlockNumber] = dataBlock;
    }

    /// <summary>
    /// Attempts to remove a given data block.
    /// </summary>
    /// <param name="blockNumber">The number of the block to be
    /// removed.</param>
    /// <returns>True if a corresponding block was found and removed.</returns>
    public bool RemoveRegisteredBlock(long blockNumber)
    {
      return transferredBlocks.Remove(blockNumber);
    }

    /// <summary>
    /// Clear the cached <see cref="TransferredBlocks"/>, e.g. because
    /// tracking is no longer required after a completed transfer.
    /// </summary>
    public void ClearTransferredBlocks()
    {
      transferredBlocks.Clear();
    }
  }
}