using System.Collections.Generic;
using System.Linq;

namespace Vfs.Transfer
{
  /// <summary>
  /// A simple but volatile transfer store that keeps managed transfers in memory.
  /// </summary>
  public class InMemoryTransferStore<TTransfer> : ITransferStore<TTransfer> where TTransfer : ITransfer
  {
    /// <summary>
    /// The internal cache that maintains all running transfers.
    /// </summary>
    protected Dictionary<string, TTransfer> Transfers { get; private set; }

    /// <summary>
    /// Synchronization token.
    /// </summary>
    protected object SyncRoot
    {
      get { return this; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public InMemoryTransferStore()
    {
      Transfers = new Dictionary<string, TTransfer>();
    }


    /// <summary>
    /// Adds a newly created transfer object to the store.
    /// </summary>
    /// <param name="transfer">The transfer to be stored.</param>
    public virtual void AddTransfer(TTransfer transfer)
    {
      lock(SyncRoot)
      {
        Transfers.Add(transfer.Token.TransferId, transfer);
      }
    }

    /// <summary>
    /// Tries to resolve a given transfer from the store.
    /// </summary>
    /// <param name="transferId">The <see cref="TransferBase{TFile,TToken}.TransferId"/>
    /// of the requested transfer.</param>
    /// <returns>The transfer, if it was found in the cache. Otherwise
    /// null.</returns>
    public virtual TTransfer TryGetTransfer(string transferId)
    {
      lock (SyncRoot)
      {
        TTransfer transfer;
        Transfers.TryGetValue(transferId, out transfer);
        return transfer;
      }
    }

    /// <summary>
    /// Allows the repository to persist a changed status
    /// of a given transfer.
    /// </summary>
    /// <param name="transfer">The updated transfer instance.</param>
    public virtual void UpdateTransferState(TTransfer transfer)
    {
      //replace transfer instance
      lock(SyncRoot)
      {
        Transfers[transfer.Token.TransferId] = transfer;
      }
    }

    /// <summary>
    /// Indicates that a transfer is no longer active. It is
    /// up to the repository to keep the transfer (or move it)
    /// or discard it completely.
    /// </summary>
    /// <param name="transfer">The transfer that is no longer
    /// active.</param>
    /// <remarks>Depending on the chosen strategy, transfer might be
    /// kept for a while in order to have <see cref="ITransferStore{ITransfer}.TryGetTransfer"/>
    /// return the (now inactive) transfer rather than a null
    /// reference.</remarks>
    public virtual void SetInactive(TTransfer transfer)
    {
      lock(SyncRoot)
      {
        //just remove the transfer
        Transfers.Remove(transfer.Token.TransferId);
      }
    }

    /// <summary>
    /// Gets all running (active) transfers for a given resource, e.g. for aborting running
    /// transfers before deleting or modifying a resource.
    /// </summary>
    /// <param name="resourceId">The identifier of the resource.</param>
    public virtual IEnumerable<TTransfer> GetRunningTransfersForResource(string resourceId)
    {
      TTransfer[] transferList;
      lock(SyncRoot)
      {
        transferList = new TTransfer[Transfers.Count];
        Transfers.Values.CopyTo(transferList, 0);
      }

      return transferList.Where(t => t.Token.ResourceIdentifier == resourceId);
    }

  }
}