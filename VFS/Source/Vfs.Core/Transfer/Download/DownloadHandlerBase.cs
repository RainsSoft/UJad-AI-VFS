using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vfs.Auditing;
using Vfs.Locking;
using Vfs.Scheduling;
using Vfs.Security;
using Vfs.Transfer.Util;
using Vfs.Util;

namespace Vfs.Transfer
{
  /// <summary>
  /// A base class for <see cref="IDownloadTransferHandler"/> implementations, which
  /// provides plumbing code such as validation, synchronization, exception handling
  /// etc.
  /// </summary>
  /// <typeparam name="TFile">The underlying file meta data item which is kept within
  /// the cached <see cref="DownloadTransfer{TFile}"/> instances.</typeparam>
  /// <typeparam name="TTransfer">The <see cref="DownloadTransfer{TFile}"/> implementation
  /// that is cached internally.</typeparam>
  public abstract class DownloadHandlerBase<TFile, TTransfer> : TransferHandlerBase<TFile, DownloadToken, TTransfer>,
                                                                IDownloadTransferHandler
    where TFile : IVirtualFileItem
    where TTransfer : DownloadTransfer<TFile>
  {


    /// <summary>
    /// Tells the service whether its used for uploading or downloading.
    /// </summary>
    protected override bool IsUploadService
    {
      get { return false; }
    }

    /// <summary>
    /// Inits the service, and uses a simple <see cref="InMemoryTransferStore{TTransfer}"/>
    /// in order to cache currently running transfers.
    /// </summary>
    protected DownloadHandlerBase()
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
    protected DownloadHandlerBase(ITransferStore<TTransfer> transferStore) : base(transferStore)
    {
    }

    /// <summary>
    /// Requests a download token for a given resource.
    /// </summary>
    /// <param name="virtualFilePath">An identifier for the resource.</param>
    /// <param name="includeFileHash">Whether a file hash for the
    /// requested resource should be calculated and assigned to the
    /// <see cref="DownloadToken.Md5FileHash"/> property of the returned
    /// <see cref="DownloadToken"/>.</param>
    /// <returns>A token that represents a granted resource download, optinally
    /// limited to a given time frame (<see cref="TransferToken.ExpirationTime"/>).</returns>
    /// <exception cref="VirtualResourceNotFoundException">In case the submitted
    /// <paramref name="virtualFilePath"/> does not match a known resource.</exception>
    /// <exception cref="ResourceAccessException">In case the requesting party is not
    /// authorized to access the resource.</exception>
    /// <exception cref="ResourceLockedException">If a lock to access the
    /// resource was not granted.</exception>
    public virtual DownloadToken RequestDownloadToken(string virtualFilePath, bool includeFileHash)
    {
      DownloadTransfer<TFile> transfer = InitTransfer(virtualFilePath, null, includeFileHash);
      return transfer.Token;
    }


    /// <summary>
    /// Requests a download token for a given resource.
    /// </summary>
    /// <param name="virtualFilePath">Identifies the resource to be downloaded.</param>
    /// <param name="maxBlockSize">The maximum size of a read block. This property must be
    /// equal or lower to the <see cref="ITransferHandler.MaxBlockSize"/>, if there is an
    /// upper limit for blocks.</param>
    /// <param name="includeFileHash">Whether a file hash for the
    /// requested resource should be calculated and assigned to the
    /// <see cref="DownloadToken.Md5FileHash"/> property of the returned
    /// <see cref="DownloadToken"/>.</param>
    /// <returns>A token that represents a granted resource download, optionally
    /// limited to a given time frame (<see cref="TransferToken.ExpirationTime"/>).</returns>
    /// <exception cref="VirtualResourceNotFoundException">If the submitted <paramref name="virtualFilePath"/>
    /// does not match an existing resource.</exception>
    /// <exception cref="ResourceAccessException">If the request was not authorized.</exception>
    /// <exception cref="ResourceLockedException">If a lock to access the
    /// resource was not granted.</exception>
    public virtual DownloadToken RequestDownloadToken(string virtualFilePath, bool includeFileHash, int maxBlockSize)
    {
      DownloadTransfer<TFile> transfer = InitTransfer(virtualFilePath, maxBlockSize, includeFileHash);
      return transfer.Token;
    }

    /// <summary>
    /// Gets all running transfers for a given resource, e.g. for aborting running
    /// transfers before deleting or modifying a resource.
    /// </summary>
    /// <param name="virtualFilePath">The identifier of the resource.</param>
    public IEnumerable<DownloadToken> GetTransfersForResource(string virtualFilePath)
    {
      const FileSystemTask context = FileSystemTask.DownloadTransfersByResourceQuery;
      return SecureFunc(context,
        () => GetTransfersForResourceImpl(virtualFilePath),
        () => String.Format("Could not determine download transfers for resource [{0}", virtualFilePath));
    }


    private IEnumerable<DownloadToken> GetTransfersForResourceImpl(string virtualFilePath)
    {

      var context = FileSystemTask.DownloadTransfersByResourceQuery;
      var eventId = AuditEvent.ResourceDownloadsQuery;

      var tokens = TransferStore.GetRunningTransfersForResource(virtualFilePath).Select(t => t.Token);

      string msg = "Queried transfers for resource [{0}] and returned [{1}] download tokens.";
      msg = String.Format(msg, virtualFilePath, tokens.Count());

      Auditor.Audit(AuditLevel.Info, context, eventId, msg);
      return tokens;
    }


    /// <summary>
    /// Peforms the intialization steps for a download transfer, including
    /// authorization checks, resource locking, and expiration scheduling,
    /// and returns a <see cref="TTransfer"/> instance in case
    /// of a valid and granted download request.
    /// </summary>
    /// <param name="submittedResourceFilePath">The resource path that was submitted by
    /// the requesting party. Only used for logging, auditing, exception creation
    /// purpose.</param>
    /// <param name="clientMaxBlockSize">The maximum size of a read block. If set, this property must be
    /// equal or lower to the <see cref="ITransferHandler.MaxBlockSize"/>, if there is an
    /// upper limit for blocks.</param>
    /// <param name="includeFileHash"></param>
    /// <returns>A transfer object that represents the granted transfer.</returns>
    protected virtual TTransfer InitTransfer(string submittedResourceFilePath, int? clientMaxBlockSize,
                                             bool includeFileHash)
    {
      const FileSystemTask context = FileSystemTask.DownloadTokenRequest;
      return SecureFunc(context,
                        () => InitTransferImpl(submittedResourceFilePath, clientMaxBlockSize, includeFileHash),
                        () => String.Format("Could not init transfer for resource [{0}]", submittedResourceFilePath));
    }


    private TTransfer InitTransferImpl(string submittedResourceFilePath, int? clientMaxBlockSize, bool includeFileHash)
    {
      const FileSystemTask context = FileSystemTask.DownloadTokenRequest;
      TFile fileItem = CreateFileItemImpl(submittedResourceFilePath, false, context);

      if (!fileItem.Exists)
      {
        AuditHelper.AuditRequestedFileNotFound(Auditor,fileItem, context);

        string msg = String.Format("Resource [{0}] not found.", submittedResourceFilePath);
        throw new VirtualResourceNotFoundException(msg) {IsAudited = true};
      }

      //get authorization
      FileClaims claims = GetFileClaims(fileItem);
      if (!claims.AllowReadData)
      {
        AuditHelper.AuditDeniedOperation(Auditor,context, AuditEvent.FileDataDownloadDenied, fileItem);

        string msg = "Read request for file [{0}] was denied - you are not authorized to read the resource.";
        msg = String.Format(msg, submittedResourceFilePath);
        throw new ResourceAccessException(msg) {IsAudited = true};
      }

      //try to get lock
      ResourceLockGuard readLock = LockResourceForDownload(fileItem, claims);
      if (readLock != null && !readLock.IsLockEnabled)
      {
        AuditHelper.AuditDeniedOperation(Auditor,context, AuditEvent.FileReadLockDenied, fileItem);

        string msg = "The requested file [{0}] is currently locked and thus cannot be accessed.";
        msg = String.Format(msg, submittedResourceFilePath);
        throw new ResourceLockedException(msg) {IsAudited = true};
      }

      //create download token
      DownloadToken token = CreateDownloadToken(submittedResourceFilePath, fileItem, clientMaxBlockSize, includeFileHash);

      //create expiration job if we have an expiration time
      Job<DownloadToken> job = null;
      if (token.ExpirationTime.HasValue)
      {
        job = ScheduleExpiration(token);
      }

      //create transfer instance
      TTransfer transfer = CreateTransfer(submittedResourceFilePath, fileItem, token, claims, readLock, job);

      //cache transfer
      TransferStore.AddTransfer(transfer);

      //audit issued token
      AuditHelper.AuditResourceOperation(Auditor,context, AuditEvent.DownloadTokenIssued, fileItem);

      return transfer;
    }


    /// <summary>
    /// Creates a token for the currently processed resource.
    /// </summary>
    /// <param name="submittedResourceFilePath">The identifier (file path)
    /// for the requested resource, as submitted by the requesting party.</param>
    /// <param name="fileItem">Represents the requested file.</param>
    /// <param name="clientMaxBlockSize">The maximum size of a downloadable block, as requested by
    /// the client. It is up to the implementing method to correct this value (e.g. in order
    /// to comply to the <see cref="TransferHandlerBase{TFile,TToken,TTransfer}.MaxBlockSize"/>
    /// of the service itself.</param>
    /// <param name="includeFileHash">Whether a file hash for the
    /// requested resource should be calculated and assigned to the
    /// <see cref="DownloadToken.Md5FileHash"/> property of the returned
    /// <see cref="DownloadToken"/>.</param>
    /// <returns>A token for the request.</returns>
    protected abstract DownloadToken CreateDownloadToken(string submittedResourceFilePath, TFile fileItem,
                                                         int? clientMaxBlockSize, bool includeFileHash);


    /// <summary>
    /// Locks the resource if necessary, and returns a <see cref="ResourceLockGuard"/>
    /// that contains all the required locks. The invoking method will check whether
    /// the lock was granted through the <see cref="ResourceLockGuard.IsLockEnabled"/>
    /// property and throw an exception if the lock was not available.<br/>
    /// If no locking is necessary, this method should just return a null reference.
    /// </summary>
    /// <param name="fileItem">Represents the requested file resource.</param>
    /// <param name="claims">The access rights for the resource as returned by
    /// <see cref="TransferHandlerBase{TFile,TToken,TTransfer}.GetFileClaims"/>.</param>
    /// <returns>Locks for the file, or <c>null</c> if no locking is necessary.<br/>
    /// Failed locking is indicated by a returned <see cref="ResourceLockGuard"/> whose
    /// <see cref="ResourceLockGuard.IsLockEnabled"/> property is set to false.</returns></returns>
    protected abstract ResourceLockGuard LockResourceForDownload(TFile fileItem, FileClaims claims);


    /// <summary>
    /// Creates a transfer object for a given requested resource.
    /// </summary>
    /// <param name="submittedResourceFilePath">The resource identifier as submitted.</param>
    /// <param name="fileItem">Represents the requested file resource.</param>
    /// <param name="token">The token that is being issued for the transfer.</param>
    /// <param name="claims">The access rights for the resource.</param>
    /// <param name="lockGuard">File locks, if necessary. Can be a null reference
    /// if no locking takes place.</param>
    /// <param name="expirationJob">A scheduled job that invokes the 
    /// <see cref="TransferHandlerBase{TFile,TToken,TTransfer}.OnTransferExpiration"/>
    /// method once the transfer expires. May be null if the token does not expire.</param>
    /// <returns>A transfer object which encapsulates the information required to perform
    /// the transfer.</returns>
    protected abstract TTransfer CreateTransfer(string submittedResourceFilePath, TFile fileItem, DownloadToken token,
                                                FileClaims claims, ResourceLockGuard lockGuard,
                                                Job<DownloadToken> expirationJob);


    /// <summary>
    /// Gets a given <see cref="BufferedDataBlock"/> from the currently downloaded
    /// resource.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <param name="blockNumber">The number of the requested block.</param>
    /// <returns>A data block which contains the data as an in-memory buffer
    /// (<see cref="BufferedDataBlock.Data"/>).</returns>
    /// <exception cref="DataBlockException">If the data block cannot be delivered,
    /// either because it's an invalid number, or because only sequential downloads
    /// are possible, and the block does not refer to the current download
    /// position. Check the <see cref="TransmissionCapabilities"/> flag in order
    /// to get the service's capabilities.
    /// </exception>
    /// <exception cref="VirtualResourceNotFoundException">If the resource does
    /// not exist.</exception>
    /// <remarks>It's up to the service to resolve a block number to the
    /// corect piece of data. Simplest case for services that operate on one
    /// resource or stream is to just make all served
    /// blocks the same size (apart from the last one, of course), which
    /// allows to easily calculate the offset of the requested block.</remarks>
    public virtual BufferedDataBlock ReadBlock(string transferId, long blockNumber)
    {
      return SecureFunc(FileSystemTask.DataBlockDownloadRequest,
                        () => ReadBlockImpl(transferId, blockNumber),
                        () => String.Format("Error while trying to read buffered data block [{0}] for transfer [{1}]",
                                            blockNumber, transferId));
    }

    private BufferedDataBlock ReadBlockImpl(string transferId, long blockNumber)
    {
      TTransfer transfer = GetCachedTransfer(transferId, true, FileSystemTask.DataBlockDownloadRequest);
      var dataBlock = OrchestrateBlockReading<BufferedDataBlock>(transfer, blockNumber, CreateBufferedDataBlockImpl);

      //if we just read the last block and should close automatically, do so now
      if (dataBlock.IsLastBlock && transfer.AutoCloseAfterLastBlockDelivery)
      {
        CompleteTransfer(transfer.TransferId);
      }

      return dataBlock;
    }

    /// <summary>
    /// Handles the creation of an actual data block based on the underlying resource.
    /// </summary>
    /// <param name="transfer">The processed transfer.</param>
    /// <param name="blockNumber">The number of the downloaded block.</param>
    /// <param name="previouslyTransferredBlock">If this data block was already transferred, this parameter
    /// contains the information about the block. Can be used in order to ensure proper retransmission in case
    /// of variable block sizes.</param>
    /// <returns>A data block which contains the data as an in-memory buffer
    /// (<see cref="BufferedDataBlock.Data"/>).</returns>
    protected abstract BufferedDataBlock CreateBufferedDataBlockImpl(TTransfer transfer, long blockNumber,
                                                                     DataBlockInfo previouslyTransferredBlock);


    /// <summary>
    /// Reads a block via a streaming channel, which enables a more resource friendly
    /// data transmission (compared to sending the whole data of the block at once).
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <param name="blockNumber">The number of the requested block.</param>
    /// <returns>A data block which contains the data as an in-memory buffer
    /// (<see cref="BufferedDataBlock.Data"/>).</returns>
    public virtual StreamedDataBlock ReadBlockStreamed(string transferId, long blockNumber)
    {
      return SecureFunc(FileSystemTask.DataBlockDownloadRequest,
                        () => ReadBlockStreamedImpl(transferId, blockNumber),
                        () => String.Format("Error while trying to read streamed data block [{0}] for transfer [{1}]",
                                            blockNumber, transferId));
    }

    private StreamedDataBlock ReadBlockStreamedImpl(string transferId, long blockNumber)
    {
      TTransfer transfer = GetCachedTransfer(transferId, true, FileSystemTask.DataBlockDownloadRequest);
      var dataBlock = OrchestrateBlockReading<StreamedDataBlock>(transfer, blockNumber, CreateStreamedDataBlockImpl);

      //if we just read the last block and should close automatically, we have the problem that there's
      //still data to be read from the underlying stream. Accordingly, encapsulate the stream with
      //another stream that closes the transfer once it is being closed itself
      if (dataBlock.IsLastBlock && transfer.AutoCloseAfterLastBlockDelivery)
      {
        string id = transfer.TransferId;
        dataBlock.Data = new ClosingActionStream(dataBlock.Data, () => CompleteTransfer(id));
      }

      return dataBlock;
    }

    /// <summary>
    /// Handles the creation of an actual data block based on the underlying resource.
    /// </summary>
    /// <param name="transfer">The processed transfer.</param>
    /// <param name="blockNumber">The number of the downloaded block.</param>
    /// <param name="previouslyTransferredBlock">If this data block was already transferred, this parameter
    /// contains the information about the block. Can be used in order to ensure proper retransmission in case
    /// of variable block sizes.</param>
    /// <returns>A data block which exposes the data as a resource-friendly stream
    /// (<see cref="BufferedDataBlock.Data"/>).</returns>
    protected abstract StreamedDataBlock CreateStreamedDataBlockImpl(TTransfer transfer, long blockNumber,
                                                                     DataBlockInfo previouslyTransferredBlock);


    /// <summary>
    /// Orchestrates common block reading procedures for the <see cref="ReadBlock"/> and
    /// <see cref="ReadBlockStreamed"/> methods.
    /// </summary>
    protected virtual T OrchestrateBlockReading<T>(TTransfer transfer, long blockNumber,
                                         Func<TTransfer, long, DataBlockInfo, T> readerImplFunc) where T : IDataBlock
    {
      const FileSystemTask context = FileSystemTask.DataBlockDownloadRequest;

      if (blockNumber < 0)
      {
        string msg = "Invalid block number [{0}] requested - block numbers cannot be negative.";
        msg = String.Format(msg, blockNumber);
        throw new DataBlockException(msg);
      }

      if (blockNumber >= transfer.Token.TotalBlockCount)
      {
        string msg = "Invalid block number [{0}] requested - the total number of blocks is [{1}].";
        msg = String.Format(msg, blockNumber, transfer.Token.TotalBlockCount);
        throw new DataBlockException(msg);
      }

      //make sure the file still exists
      if (!transfer.FileItem.Exists)
      {
        AuditHelper.AuditRequestedFileNotFound(Auditor,transfer.FileItem, context);

        string msg = "Resource [{0}] of transfer [{1}] was not found.";
        msg = String.Format(msg, transfer.Token.ResourceName, transfer.TransferId);
        throw new VirtualResourceNotFoundException(msg) {IsAudited = true};
      }

      lock (transfer.SyncRoot)
      {
        //make sure the transfer is active
        if (!transfer.Status.Is(TransferStatus.Starting, TransferStatus.Running, TransferStatus.Paused))
        {
          string msg = String.Format("Transfer [{0}] is not active anymore - status is [{1}].", transfer.TransferId,
                                     transfer.Status);

          if (transfer.AbortReason.HasValue)
          {
            msg += String.Format(" Transfer abort reason: [{0}].", transfer.AbortReason);
          }

          AuditHelper.AuditDeniedOperation(Auditor,context, AuditEvent.DownloadNoLongerActive, transfer.FileItem, msg);
          throw new TransferStatusException(msg) {IsAudited = true, EventId = (int) AuditEvent.DownloadNoLongerActive};
        }

        //check whether we already transferred this block once
        DataBlockInfo transferredBlock = transfer.TryGetTransferredBlock(blockNumber);

        T dataBlock = readerImplFunc(transfer, blockNumber, transferredBlock);

        //update status
        transfer.Status = TransferStatus.Running;

        //store copy of the downloaded block that doesn't contain any data
        transfer.RegisterBlock(DataBlockInfo.FromDataBlock(dataBlock));

        //audit download
        AuditHelper.AuditTransferOperation(Auditor,context, AuditEvent.FileBlockDownloaded, transfer.TransferId, transfer.FileItem);

        return dataBlock;
      }
    }


    /// <summary>
    /// Gets the binary contents of a resource as a stream in a blocking operation.
    /// Use the methods in <see cref="ContentUtil"/> class for simplified stream
    /// handling. Unlike the <see cref="IDownloadTransferHandler.DownloadFile"/> method, this method takes
    /// the path of a given resource, and works transparently without download tokens.
    /// </summary>
    /// <param name="virtualFilePath">The path of the resource to be read.</param>
    /// <returns>A stream that allows the contents of the file to be read.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="virtualFilePath"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the resource that is represented
    /// by <paramref name="virtualFilePath"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    /// <exception cref="ResourceLockedException">In case the resource is currently locked
    /// and thus not accessible for reading.</exception>
    /// <remarks>This implementation gets a download token and forwards the token to the
    /// <see cref="DownloadFile"/> method. The download piggy-backs on the capabilities to
    /// read files in blocks. This causes overhead (we could just return the stream), but has
    /// the advantage that if the transfer is aborted (e.g. because a file lock expires),
    /// the underlying resource is immediately unlocked, because the returned stream does
    /// not directly access the resource data, but merely reads block after block via
    /// <see cref="ReadBlock"/>.</remarks>
    public virtual Stream ReadFile(string virtualFilePath)
    {
      const FileSystemTask context = FileSystemTask.StreamedFileDownloadRequest;
      return SecureFunc(context,
                        () => ReadFileImpl(virtualFilePath),
                        () => String.Format("An error occurred when trying to return a stream for file [{0}]", virtualFilePath));
    }


    private Stream ReadFileImpl(string virtualFilePath)
    {
      //get a token - this creates a new transfer and validates the request
      DownloadToken token = RequestDownloadToken(virtualFilePath, false);

      //forward the token
      return DownloadFileImpl(token.TransferId);
    }

    /// <summary>
    /// Gets the binary contents of a resource as a stream in a blocking operation.
    /// Unlike the <see cref="IDownloadTransferHandler.ReadFile"/> method, this method expects the
    /// <see cref="TransferToken.TransferId"/> of a previously issued download token.
    /// Use the methods in <see cref="ContentUtil"/> class for simplified stream
    /// handling.
    /// </summary>
    /// <param name="transferId">The <see cref="TransferToken.TransferId"/> of a previously
    /// issued download token.</param>
    /// <returns>A stream that allows the contents of the file to be read.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="transferId"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the resource that is represented
    /// by the token is no longer available.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    /// <exception cref="TransferStatusException">In case the token is not or no longer
    /// valid.</exception>
    /// <remarks>This implementation piggy-backs on the capabilities to read file in blocks.
    /// This causes overhead (we could just return the stream), but has the advantage that
    /// if the transfer is aborted (e.g. because a file lock expires), the underlying resource
    /// is immediately unlocked, because the returned stream does not directly access the
    /// resource data, but merely reads block after block via <see cref="ReadBlock"/>.</remarks>
    public virtual Stream DownloadFile(string transferId)
    {
      const FileSystemTask context = FileSystemTask.StreamedFileDownloadRequest;
      return SecureFunc(context,
                        () => DownloadFileImpl(transferId),
                        () => String.Format("An error occurred when trying to return a continuous stream for transfer [{0}]", transferId));
    }


    private Stream DownloadFileImpl(string transferId)
    {
      const FileSystemTask context = FileSystemTask.StreamedFileDownloadRequest;

      //get the cached transfer
      TTransfer transfer = GetCachedTransfer(transferId, true, context);


      if (transfer.Token.TotalBlockCount == 0)
      {
        //if we don't have any blocks, return a null stream
        AuditHelper.AuditTransferOperation(Auditor,context, AuditEvent.FileDataDownloaded, transfer.TransferId, transfer.FileItem);
        return Stream.Null;
      }

      //we don't expose the underlying stream directly, but rather use blocks again,
      //which disconnects the exposed stream from the underlying resource and allows for simple
      //aborting
      long resourceLength = transfer.FileItem.ResourceInfo.Length;

      // ReSharper disable UseObjectOrCollectionInitializer
      var stream = new StreamedBlockInputStream(blockNumber => ReadBlockStreamed(transferId, blockNumber),
                                                resourceLength);
      // ReSharper restore UseObjectOrCollectionInitializer

      //assign the stream the transfer object in order so it can query the status during reading
      stream.Transfer = transfer;

      //reading the last block closes the transfer automatically..
      transfer.AutoCloseAfterLastBlockDelivery = true;

      //...and so does disposing the stream at any time
      var closingStream = new ClosingActionStream(stream, () => CompleteTransfer(transferId));

      AuditHelper.AuditTransferOperation(Auditor,context, AuditEvent.FileDataDownloaded, transfer.TransferId, transfer.FileItem);
      return closingStream;
    }


    /// <summary>
    /// Requeries a previously issued token. Can be used if the client only stores
    /// <see cref="TransferToken.TransferId"/> values rather than the tokens and
    /// needs to get ahold of them again.
    /// </summary>
    /// <param name="transferId"></param>
    /// <returns></returns>
    /// <exception cref="UnknownTransferException">In case the <paramref name="transferId"/>
    /// does not refer to an active transfer.</exception>
    public DownloadToken ReloadToken(string transferId)
    {
      return GetCachedTransfer(transferId, true, FileSystemTask.DownloadTokenRequery).Token;
    }

  }
}