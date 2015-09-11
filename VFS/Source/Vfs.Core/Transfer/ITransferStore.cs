using System.Collections.Generic;

namespace Vfs.Transfer
{
  /// <summary>
  /// Provides a (optionally persistent) storage mechanism
  /// for transfers.
  /// </summary>
  public interface ITransferStore<TTransfer> where TTransfer : ITransfer
  {
    /// <summary>
    /// Adds a newly created transfer object to the store.
    /// </summary>
    /// <param name="transfer">The transfer to be stored.</param>
    void AddTransfer(TTransfer transfer);

    /// <summary>
    /// Tries to resolve a given transfer from the store.
    /// </summary>
    /// <param name="transferId">The <see cref="TransferBase{TFile,TToken}.TransferId"/>
    /// of the requested transfer.</param>
    /// <returns>The transfer, if it was found in the cache. Otherwise
    /// null.</returns>
    TTransfer TryGetTransfer(string transferId);

    /// <summary>
    /// Allows the repository to persist a changed status
    /// of a given transfer.
    /// </summary>
    /// <param name="transfer">The updated transfer instance.</param>
    void UpdateTransferState(TTransfer transfer);

    /// <summary>
    /// Indicates that a transfer is no longer active. It is
    /// up to the repository to keep the transfer (or move it)
    /// or discard it completely.
    /// </summary>
    /// <param name="transfer">The transfer that is no longer
    /// active.</param>
    /// <remarks>Depending on the chosen strategy, transfer might be
    /// kept for a while in order to have <see cref="TryGetTransfer"/>
    /// return the (now inactive) transfer rather than a null
    /// reference.</remarks>
    void SetInactive(TTransfer transfer);

    /// <summary>
    /// Gets all running (active) transfers for a given resource, e.g. for aborting running
    /// transfers before deleting or modifying a resource.
    /// </summary>
    /// <param name="resourceId">The identifier of the resource.</param>
    IEnumerable<TTransfer> GetRunningTransfersForResource(string resourceId);
  }
}