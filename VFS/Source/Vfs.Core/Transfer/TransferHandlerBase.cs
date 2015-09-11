using System;
using System.Collections.Generic;
using System.Linq;
using Vfs.Auditing;
using Vfs.Scheduling;
using Vfs.Security;
using Vfs.Util;

namespace Vfs.Transfer
{
  /// <summary>
  /// Base class for transfer services that implement the
  /// <see cref="ITransferHandler"/> interface.
  /// </summary>
  /// <typeparam name="TFile">Helper class that provides information about
  /// the transferred file resource.</typeparam>
  /// <typeparam name="TToken">The token type. Depends on whether
  /// the implementing class provides upload or download services.</typeparam>
  /// <typeparam name="TTransfer">The <see cref="ITransfer"/> implementation that
  /// is used to keep track of running transfers.</typeparam>
  public abstract class TransferHandlerBase<TFile, TToken, TTransfer>
    where TFile : IVirtualFileItem
    where TToken : TransferToken
    where TTransfer : TransferBase<TFile, TToken>
  {
    /// <summary>
    /// Provides a storage mechanism for maintained transfers.
    /// </summary>
    public ITransferStore<TTransfer> TransferStore { get; private set; }

    /// <summary>
    /// The scheduler that handles expiration times for maintained transfers.
    /// </summary>
    public Scheduler ExpirationScheduler { get; private set; }

    private IAuditor auditor = new NullAuditor();

    /// <summary>
    /// The auditor that receives transfer status messages.
    /// </summary>
    public IAuditor Auditor
    {
      get { return auditor; }
      set { auditor = value ?? new NullAuditor(); }
    }


    /// <summary>
    /// Gets the maximum block size that can be transferred, if such
    /// a limit exists. Returns null in case of arbitrary block sizes.
    /// </summary>
    public abstract int? MaxBlockSize { get; }

    /// <summary>
    /// Tells the service whether its used for uploading or downloading.
    /// </summary>
    protected abstract bool IsUploadService { get; }

    /// <summary>
    /// Indicates how restrictively data blocks may be transmitted.
    /// </summary>
    public abstract TransmissionCapabilities TransmissionCapabilities { get; }


    /// <summary>
    /// Inits the service, and uses a simple <see cref="InMemoryTransferStore{TTransfer}"/>
    /// in order to cache currently running transfers.
    /// </summary>
    protected TransferHandlerBase() : this(new InMemoryTransferStore<TTransfer>())
    {
    }


    /// <summary>
    /// Initializes the service with a specific <see cref="ITransferStore{TTransfer}"/>
    /// that maintains running transfers.
    /// </summary>
    /// <param name="transferStore">Provides a storage mechanism for
    /// managed transfers.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="transferStore"/>
    /// is a null reference.</exception>
    protected TransferHandlerBase(ITransferStore<TTransfer> transferStore)
    {
      Ensure.ArgumentNotNull(transferStore, "transferStore");

      TransferStore = transferStore;
      ExpirationScheduler = new Scheduler();
    }



    /// <summary>
    /// Resolves the submitted resource path and returns a matching
    /// file item. This process should include all steps that a regular
    /// request for a <see cref="VirtualFileInfo"/> includes, e.g. checking
    /// for file availability if requested.
    /// </summary>
    /// <param name="submittedResourceFilePath">Identifies the requested file resource.</param>
    /// <param name="mustExist">Whether the file is expected to exist on the file system. If
    /// this parameter is true, the implementing method should throw a
    /// <see cref="VirtualResourceNotFoundException"/> for the requested resource.
    /// </param>
    /// <param name="context">The file system operation that is being performed during the invocation of
    /// this method. Used for internal auditing.</param>
    /// <returns>A file item that represents the requested file resource.</returns>
    /// <exception cref="VirtualResourceNotFoundException">In case the submitted
    /// <paramref name="submittedResourceFilePath"/> does not match a known resource,
    /// and the <paramref name="mustExist"/> flag is true.</exception>
    /// <exception cref="ResourceAccessException">In case the requesting party is not
    /// authorized to access the resource.</exception>
    protected abstract TFile CreateFileItemImpl(string submittedResourceFilePath, bool mustExist, FileSystemTask context);

    /// <summary>
    /// Gets the access rights for the requested file. This method
    /// is invoked in order to check whether the requesting party
    /// is allowed to read the resource.
    /// </summary>
    /// <param name="file">Represents the requested resource.</param>
    /// <returns>Access rights for the file.</returns>
    protected abstract FileClaims GetFileClaims(TFile file);

    /// <summary>
    /// Gets the server-side status of the transfer.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <returns>The status of the requested transfer. If the transfer is unknown,
    /// this does not cause an exception, but merely results in a return value
    /// of <see cref="TransferStatus.UnknownTransfer"/>.</returns>
    public virtual TransferStatus GetTransferStatus(string transferId)
    {
      var context = IsUploadService
                      ? FileSystemTask.UploadTransferStatusRequest
                      : FileSystemTask.DownloadTransferStatusRequest;

      TTransfer transfer = GetCachedTransfer(transferId, false, context);
      if (transfer == null) return TransferStatus.UnknownTransfer;

      lock (transfer.SyncRoot)
      {
        return transfer.Status;
      }
    }

    /// <summary>
    /// Submits an expiration job that makes sure the transfer is being marked
    /// as expired and eventually disposed of if the client does not complete
    /// the transfer. The expiration time is taken from the submitted
    /// <see cref="token"/>.
    /// </summary>
    /// <param name="token">The token to be issued.</param>
    /// <returns>The job that was submitted to the scheduler. It's
    /// <see cref="Job.JobId"/> matches the <see cref="TransferToken.TransferId"/>
    /// of the submitted <paramref name="token"/>.</returns>
    /// <exception cref="InvalidOperationException">If the submitted <paramref name="token"/>
    /// does not have an <see cref="TransferToken.ExpirationTime"/>.</exception>
    protected virtual Job<TToken> ScheduleExpiration(TToken token)
    {
      if (!token.ExpirationTime.HasValue)
      {
        string msg = "Token '{0}' does not have an expiration time.";
        msg = String.Format(msg, token.TransferId);
        throw new InvalidOperationException(msg);
      }

      //create and submit job
      Job<TToken> job = new Job<TToken>(token.TransferId) { Data = token };
      job.Run.From(token.ExpirationTime.Value).Once();
      ExpirationScheduler.SubmitJob(job, OnTransferExpiration);

      return job;
    }


    /// <summary>
    /// This method is invoked by the internally used scheduler, whenever a transfer is
    /// being expired. If the expired transfer is still active, it is being canceled.
    /// </summary>
    /// <param name="job">The scheduled job that was used to get a notification about the
    /// expiration.</param>
    /// <param name="token">The token of the expired transfer.</param>
    protected virtual void OnTransferExpiration(Job<TToken> job, TToken token)
    {
      var context = IsUploadService ? FileSystemTask.UploadTransferExpiration : FileSystemTask.DownloadTransferExpiration;
      TTransfer transfer = GetCachedTransfer(token.TransferId, false, context);

      //abort if the transfer is no longer active
      if (transfer == null) return;

      if (transfer.Status.Is(TransferStatus.Completed, TransferStatus.Aborted)) return;

      //cancel the transfer
      CancelTransfer(token.TransferId, AbortReason.Expired);
    }


    /// <summary>
    /// Frees resources associated with a now inactive (completed or canceled) transfer and
    /// finally submits the transfer to the <see cref="ITransferStore{TTransfer}.SetInactive"/>
    /// method of the underlying <see cref="TransferStore"/>.<br/>
    /// This method can be overridden in order to do some custom housekeeping
    /// at the end of every transfer, whether successfully completed or aborted.
    /// </summary>
    /// <param name="transfer">The transfer to be finalized.</param>
    protected virtual void FinalizeTransfer(TTransfer transfer)
    {
      //release locks, if any
      lock (transfer.SyncRoot)
      {
        if (transfer.ResourceLock != null)
        {
          //release locked resource
          transfer.ResourceLock.Dispose();
          transfer.ResourceLock = null;
        }

        if (transfer.ExpirationNotificationJob != null)
        {
          //cancel the expired job
          transfer.ExpirationNotificationJob.Cancel();
        }

        //finally set the transfer as inactive
        TransferStore.SetInactive(transfer);
      }
    }

    /// <summary>
    /// Tries to get a given <see cref="TTransfer"/> from the internal
    /// cache.
    /// </summary>
    /// <param name="transferId">The identifier of the transfer, according to the corresponding
    /// <see cref="TransferToken.TransferId"/>.</param>
    /// <param name="throwExceptionIfNotFound">If true, a <see cref="UnknownTransferException"/>
    /// is thrown if no matching transfer was found in the cache.</param>
    /// <param name="context">The file system operation that is being performed during the invocation of
    /// this method. Used for internal auditing.</param>
    /// <returns>Either the matching transfer, or null, if no match was found *and*
    /// <paramref name="throwExceptionIfNotFound"/> is false.</returns>
    protected virtual TTransfer GetCachedTransfer(string transferId, bool throwExceptionIfNotFound, FileSystemTask context)
    {
      TTransfer transfer = TransferStore.TryGetTransfer(transferId);
      if (transfer == null && throwExceptionIfNotFound)
      {
        AuditHelper.AuditUnknownTransferRequest(Auditor,context, transferId);

        string msg = String.Format("Unknown transfer ID: {0}.", transferId);
        throw new UnknownTransferException(msg) {IsAudited = true, EventId = (int)AuditEvent.UnknownTransferRequest};
      }

      return transfer;
    }


    /// <summary>
    /// Tells the transfer service that transmission is being
    /// paused for an unknown period of time. This should keep
    /// the transfer enabled, but gives the service time to
    /// free or unlock resources.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <exception cref="UnknownTransferException">In case no such transfer
    /// is currently maintained.</exception>
    public virtual void PauseTransfer(string transferId)
    {
      var context = IsUploadService
                      ? FileSystemTask.UploadTransferPauseRequest
                      : FileSystemTask.DownloadTransferPauseRequest;

      TTransfer transfer = GetCachedTransfer(transferId, true, context);
      lock (transfer.SyncRoot)
      {
        if (!transfer.Status.Is(TransferStatus.Starting, TransferStatus.Running))
        {
          AuditEvent auditEvent = AuditEvent.InvalidTransferStatusChange;
          AuditHelper.AuditInvalidTransferPauseRequest(Auditor, transfer, context, auditEvent);

          string msg = "Only active transfers can be paused. Current status is: [{0}].";
          msg = String.Format(msg, transfer.Status);
          
          throw new TransferStatusException(msg) {IsAudited = true, EventId = (int)auditEvent};
        }

        PauseTransferImpl(transfer);
        transfer.Status = TransferStatus.Paused;

        AuditHelper.AuditChangedTransferStatus(Auditor,transfer, context, AuditEvent.TransferPaused);
      }
    }

    /// <summary>
    /// Performs housekeeping code once a transfer is paused, e.g.
    /// to close an open stream or free other resources.
    /// </summary>
    /// <param name="transfer">The paused transfer.</param>
    protected abstract void PauseTransferImpl(TTransfer transfer);


    /// <summary>
    /// Completes a given file transfer - invoking this operation
    /// closes and removes the transfer from the service. It is highly
    /// recommended to invoke this method after finishing a transfer
    /// in order to free used/locked resources as soon as possible.
    /// </summary>
    /// <returns>The new status of the transfer, which is <see cref="TransferStatus.Completed"/>
    /// in case of a known transfer. If the transfer is not known (maybe because it was aborted
    /// by the system), this method returns <see cref="TransferStatus.UnknownTransfer"/>.</returns>
    public virtual TransferStatus CompleteTransfer(string transferId)
    {
      var context = IsUploadService
                      ? FileSystemTask.UploadTransferCompletion
                      : FileSystemTask.DownloadTransferCompletion;

      return CloseTransferInternal(transferId, TransferStatus.Completed, null, context, AuditEvent.TransferCompleted);
    }


    /// <summary>
    /// Aborts a currently managed resource transfer.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <param name="reason">The reason to cancel the transfer.</param>
    /// <returns>The new status of the transfer, which is <see cref="TransferStatus.Aborted"/>
    /// in case of a known transfer. If the transfer is not known (maybe because it was aborted
    /// by the system), this method returns <see cref="TransferStatus.UnknownTransfer"/>.</returns>
    public virtual TransferStatus CancelTransfer(string transferId, AbortReason reason)
    {
      var context = IsUploadService
                ? FileSystemTask.UploadTransferCanceling
                : FileSystemTask.DownloadTransferCanceling;

      return CloseTransferInternal(transferId, TransferStatus.Aborted, reason, context, AuditEvent.TransferCanceled);
    }

    /// <summary>
    /// Closes a given transfer, and frees resources that were associated with the
    /// transfer (by invoking the <see cref="FinalizeTransfer"/> method).
    /// </summary>
    /// <returns>The suggested <param name="status" /> if the transfer was found, or
    /// <see cref="TransferStatus.UnknownTransfer"/> if no matching transfer was found.</returns>
    protected virtual TransferStatus CloseTransferInternal(string transferId, TransferStatus status, AbortReason? abortReason, FileSystemTask context, AuditEvent eventId)
    {
      //try to get the transfer
      TTransfer transfer = GetCachedTransfer(transferId, false, context);

      //if we don't have this one (anymore), return corresponding flag
      if (transfer == null) return TransferStatus.UnknownTransfer;

      lock (transfer.SyncRoot)
      {
        CloseTransferImpl(transfer, status, abortReason);

        transfer.Status = status;
        transfer.AbortReason = abortReason;

        FinalizeTransfer(transfer);
      }

      //audit status change
      AuditHelper.AuditChangedTransferStatus(Auditor,transfer, context, eventId);

      return status;
    }

    /// <summary>
    /// Cleans up a transfer and used FS specific resources after the transfer
    /// was closed. This method is being invoked during the execution of
    /// <see cref="CloseTransferInternal"/>.
    /// </summary>
    /// <param name="status">The transfer status.</param>
    /// <param name="abortReason">Indicates why the transfer was aborted, in case the
    /// <paramref name="status"/> is <see cref="TransferStatus.Aborted"/>.</param>
    /// <param name="transfer">The closed transfer.</param>
    protected abstract void CloseTransferImpl(TTransfer transfer, TransferStatus status, AbortReason? abortReason);


    /// <summary>
    /// Gets information about all blocks that have been transferred so far.
    /// This information can be used to resume a transfer that spawns several
    /// sessions on the client side.<br/>
    /// In case of retransmissions of blocks, this method returns only the
    /// <see cref="DataBlockInfo"/> instance per block number that corresponds
    /// to the most recent transfer.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <returns>All transferred blocks (without data).</returns>
    /// <exception cref="UnknownTransferException">In case no such transfer
    /// is currently maintained.</exception>
    /// <remarks>Implementing classes might decide to discard cached block information
    /// once a transfer was completed, and throw exceptions or just return an empty list.</remarks>
    public IEnumerable<DataBlockInfo> GetTransferredBlocks(string transferId)
    {
      var context = IsUploadService
                ? FileSystemTask.UploadedBlockInfosRequest
                : FileSystemTask.DownloadedBlockInfosRequest;

      TTransfer transfer = GetCachedTransfer(transferId, true, context);
      var blocks = transfer.TransferredBlocks;

      AuditHelper.AuditTransferOperation(Auditor,context, AuditEvent.TransferredBlockInfoRequest, transferId, transfer.FileItem);

      return blocks;
    }


    /// <summary>
    /// Provides exception handling and auditing for a given function.
    /// </summary>
    /// <param name="task">The context, used for auditing exceptions that may occur.</param>
    /// <param name="func">The function to be invoked.</param>
    /// <param name="errorMessage">Returns an error message in case of an unhandled exception
    /// that is not derived from <see cref="VfsException"/>.</param>
    /// <returns>The result of the submitted <paramref name="func"/> function.</returns>
    protected virtual T SecureFunc<T>(FileSystemTask task, Func<T> func, Func<string> errorMessage)
    {
      return VfsUtil.SecureFunc(task, func, errorMessage, Auditor);
    }


    /// <summary>
    /// Provides exception handling and auditing for a given function.
    /// </summary>
    /// <param name="task">The context, used for auditing exceptions that may occur.</param>
    /// <param name="action">The action to be invoked.</param>
    /// <param name="errorMessage">Returns an error message in case of an unhandled exception
    /// that is not derived from <see cref="VfsException"/>.</param>
    /// <returns>The result of the submitted <paramref name="action"/> function.</returns>
    protected virtual void SecureAction(FileSystemTask task, Action action, Func<string> errorMessage)
    {
      VfsUtil.SecureAction(task, action, errorMessage, Auditor);
    }
  }
}