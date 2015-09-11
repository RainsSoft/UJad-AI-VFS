using System;
using System.IO;
using System.Linq;
using Vfs.Auditing;
using Vfs.Locking;
using Vfs.Scheduling;
using Vfs.Security;


namespace Vfs.Transfer.Upload
{
  public abstract class UploadHandlerBase<TFile, TFolder, TTransfer> : TransferHandlerBase<TFile, UploadToken, TTransfer>
                                                     , IUploadTransferHandler
    where TFile : IVirtualFileItem
    where TFolder : IVirtualFolderItem
    where TTransfer : UploadTransfer<TFile>
  {

    /// <summary>
    /// Tells the service whether its used for uploading or downloading.
    /// </summary>
    protected override bool IsUploadService
    {
      get { return true; }
    }

    /// <summary>
    /// Inits the service, and uses a simple <see cref="InMemoryTransferStore{TTransfer}"/>
    /// in order to cache currently running transfers.
    /// </summary>
    protected UploadHandlerBase()
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
    protected UploadHandlerBase(ITransferStore<TTransfer> transferStore)
      : base(transferStore)
    {
    }


    /// <summary>
    /// Gets the maximum size of an uploaded file. A value of
    /// null indicates no limit is in place.
    /// </summary>
    public abstract long? GetMaxFileUploadSize();


    /// <summary>
    /// Requests an upload token for a given file resource.
    /// </summary>
    /// <param name="virtualFilePath">Identifies the resource to be downloaded.</param>
    /// <param name="overwriteExistingResource">Whether an already existing file should be overwritten
    /// or not. If this parameter is false and the file already exists, a <see cref="ResourceOverwriteException"/>
    /// is thrown.</param>
    /// <param name="resourceLength">The length of the resource to be uploaded in bytes.</param>
    /// <param name="contentType">The content type of the uploaded resource.</param>
    /// <returns>A token that represents a granted resource download, optionally
    /// limited to a given time frame (<see cref="TransferToken.ExpirationTime"/>).</returns>
    /// <exception cref="ResourceOverwriteException">If such a file already exists, and the
    /// <paramref name="overwriteExistingResource"/> parameter was false.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the submitted <paramref name="virtualFilePath"/>
    /// does not match an existing resource.</exception>
    /// <exception cref="ResourceAccessException">If the request was not authorized.</exception>
    /// <exception cref="ResourceLockedException">If a lock to access the
    /// resource was not granted.</exception>
    public UploadToken RequestUploadToken(string virtualFilePath, bool overwriteExistingResource, long resourceLength, string contentType)
    {
      UploadTransfer<TFile> transfer = InitTransfer(virtualFilePath, overwriteExistingResource, resourceLength, contentType);
      return transfer.Token;
    }


    /// <summary>
    /// Checks whether an upload transfer is running for a given file.
    /// </summary>
    /// <param name="virtualFilePath">The path of the file.</param>
    /// <returns></returns>
    public virtual UploadToken GetTransferForResource(string virtualFilePath)
    {
      const FileSystemTask context = FileSystemTask.UploadTransferByResourceQuery;
      return SecureFunc(context,
        () => GetTransferForResourceImpl(virtualFilePath),
        () => String.Format("Could not determine upload transfer for resource [{0}", virtualFilePath));
    }

    private UploadToken GetTransferForResourceImpl(string virtualFilePath)
    {
      const FileSystemTask context = FileSystemTask.UploadTransferByResourceQuery;
      const AuditEvent eventId = AuditEvent.ResourceUploadQuery;

      var token = TransferStore.GetRunningTransfersForResource(virtualFilePath)
        .Select(t => t.Token)
        .SingleOrDefault();

      string msg = "Queried upload transfer for resource [{0}]. Token found: [{1}].";
      msg = String.Format(msg, virtualFilePath, token != null);

      Auditor.Audit(AuditLevel.Info, context, eventId, msg);
      return token;
    }


    /// <summary>
    /// Peforms the intialization steps for an upload transfer, including
    /// authorization checks, resource locking, and expiration scheduling,
    /// and returns a <see cref="TTransfer"/> instance in case
    /// of a valid and granted upload request.
    /// </summary>
    /// <param name="submittedResourceFilePath">The resource path that was submitted by
    /// the requesting party. Only used for logging, auditing, exception creation
    /// purpose.</param>
    /// <param name="overwrite">Whether an existing file can be overwritten.</param>
    /// <param name="resourceLength">The length of the resource to be uploaded in bytes.</param>
    /// <param name="contentType">The content type of the uploaded resource.</param>
    /// <returns>A transfer object that represents the granted transfer.</returns>
    protected virtual TTransfer InitTransfer(string submittedResourceFilePath, bool overwrite, long resourceLength, string contentType)
    {
      const FileSystemTask context = FileSystemTask.UploadTokenRequest;
      return SecureFunc(context,
                        () => InitTransferImpl(submittedResourceFilePath, overwrite, resourceLength, contentType),
                        () => String.Format("Could not init transfer for resource [{0}]", submittedResourceFilePath));

    }

    private TTransfer InitTransferImpl(string submittedResourceFilePath, bool overwrite, long resourceLength, string contentType)
    {
      const FileSystemTask context = FileSystemTask.UploadTokenRequest;

      //validate maximum file size
      var maxFileSize = GetMaxFileUploadSize();
      if(maxFileSize.HasValue && maxFileSize < resourceLength)
      {
        string msg = "Upload for file [{0}] denied: Resource length of [{1}] is above the maximum upload limit of [{2}] bytes.";
        msg = String.Format(msg, submittedResourceFilePath, resourceLength, maxFileSize.Value);
        throw new ResourceAccessException(msg);
      }

      //of course, the length cannot be negative
      if(resourceLength < 0)
      {
        string msg = "Upload for file [{0}] denied: Resource length cannot be negative [{1}].";
        msg = String.Format(msg, submittedResourceFilePath, resourceLength);
        throw new ResourceAccessException(msg);
      }


      TFile fileItem = CreateFileItemImpl(submittedResourceFilePath, false, context);

      if (fileItem.Exists && !overwrite)
      {
        AuditHelper.AuditDeniedOperation(Auditor,context, AuditEvent.FileAlreadyExists, fileItem);

        string msg = String.Format("Cannot upload file [{0}] without overwriting existing data - a file already exists at this location.", submittedResourceFilePath);
        throw new ResourceOverwriteException(msg) { IsAudited = true };
      }

      //get authorization
      TFolder parentFolder = GetParentFolder(fileItem, context);

      //validate file system specific restrictions
      VerifyCanUploadFileToFileSystemLocation(submittedResourceFilePath, parentFolder, fileItem);

      //get parent folder and check whether files can be added
      FolderClaims folderClaims = GetParentFolderClaims(fileItem, parentFolder);
      if(!folderClaims.AllowAddFiles)
      {
        //deny adding a file to that folder
        AuditHelper.AuditDeniedOperation(Auditor,context, AuditEvent.CreateFileDenied, fileItem);

        string msg = "Cannot create file at [{0}] - adding files to the folder is not permitted.";
        msg = String.Format(msg, submittedResourceFilePath);
        throw new ResourceAccessException(msg) { IsAudited = true };
      }


      //only overwrite a file if explicitly requested
      FileClaims claims = GetFileClaims(fileItem);
      if (fileItem.Exists)
      {
        if (!claims.AllowOverwrite)
        {
          AuditHelper.AuditDeniedOperation(Auditor,context, AuditEvent.FileDataOverwriteDenied, fileItem);

          string msg = "Overwriting file [{0}] was denied due to missing permission.";
          msg = String.Format(msg, submittedResourceFilePath);
          throw new ResourceOverwriteException(msg) {IsAudited = true};
        }
      }


      //try to get lock
      ResourceLockGuard writeLock = LockResourceForUpload(fileItem);
      if (writeLock != null && !writeLock.IsLockEnabled)
      {
        AuditHelper.AuditDeniedOperation(Auditor,context, AuditEvent.FileReadLockDenied, fileItem);

        string msg = "The file [{0}] is currently locked and cannot be accessed.";
        msg = String.Format(msg, submittedResourceFilePath);
        throw new ResourceLockedException(msg) { IsAudited = true };
      }

      //create upload token
      UploadToken token = CreateUploadToken(submittedResourceFilePath, fileItem, resourceLength, contentType);

      //create expiration job if we have an expiration time
      Job<UploadToken> job = null;
      if (token.ExpirationTime.HasValue)
      {
        job = ScheduleExpiration(token);
      }

      //create and cache transfer instance
      TTransfer transfer = CreateTransfer(submittedResourceFilePath, fileItem, parentFolder, token, claims, writeLock, job);
      TransferStore.AddTransfer(transfer);

      AuditHelper.AuditResourceOperation(Auditor,context, AuditEvent.UploadTokenIssued, fileItem);
      return transfer;
    }


    /// <summary>
    /// Creates a transfer object for a given file resource.
    /// </summary>
    /// <param name="submittedResourceFilePath">The resource identifier as submitted.</param>
    /// <param name="fileItem">Represents the file resource to be uploaded.</param>
    /// <param name="parentFolder">The file's parent folder.</param>
    /// <param name="token">The token that is being issued for the transfer.</param>
    /// <param name="claims">The access rights for the resource.</param>
    /// <param name="lockGuard">File locks, if necessary. Can be a null reference
    /// if no locking takes place.</param>
    /// <param name="expirationJob">A scheduled job that invokes the 
    /// <see cref="TransferHandlerBase{TFile,TToken,TTransfer}.OnTransferExpiration"/>
    /// method once the transfer expires. May be null if the token does not expire.</param>
    /// <returns>A transfer object which encapsulates the information required to perform
    /// the transfer.</returns>
    protected abstract TTransfer CreateTransfer(string submittedResourceFilePath, TFile fileItem, TFolder parentFolder, UploadToken token, FileClaims claims, ResourceLockGuard lockGuard, Job<UploadToken> expirationJob);


    /// <summary>
    /// Creates a token for the currently processed resource.
    /// </summary>
    /// <param name="submittedResourceFilePath">The identifier (file path)
    /// for the requested resource, as submitted by the requesting party.</param>
    /// <param name="fileItem">Represents the file resource to be uploaded.</param>
    /// <param name="resourceLength">The length of the resource to be uploaded in bytes.</param>
    /// <param name="contentType">The content type of the uploaded resource.</param>
    /// <returns>A token for the request.</returns>
    protected abstract UploadToken CreateUploadToken(string submittedResourceFilePath, TFile fileItem, long resourceLength, string contentType);


    /// <summary>
    /// Locks the resource if necessary, and returns a <see cref="ResourceLockGuard"/>
    /// that contains all the required locks. The invoking method will check whether
    /// the lock was granted through the <see cref="ResourceLockGuard.IsLockEnabled"/>
    /// property and throw an exception if the lock was not available.<br/>
    /// If no locking is necessary, this method should just return a null reference.
    /// </summary>
    /// <param name="fileItem">Represents the file resource to be uploaded.</param>
    /// <returns>Locks for the file, or <c>null</c> if no locking is necessary.<br/>
    /// Failed locking is indicated by a returned <see cref="ResourceLockGuard"/> whose
    /// <see cref="ResourceLockGuard.IsLockEnabled"/> property is set to false.</returns>
    protected abstract ResourceLockGuard LockResourceForUpload(TFile fileItem);


    /// <summary>
    /// Resolves the parent folder of a given file item.
    /// </summary>
    /// <param name="fileItem">Represents the file resource to be uploaded.</param>
    /// <param name="context">The file system operation that is being performed during the invocation of
    /// this method. Used for internal auditing.</param>
    /// <returns>The designated parent folder, if any.</returns>
    protected abstract TFolder GetParentFolder(TFile fileItem, FileSystemTask context);


    /// <summary>
    /// Verifies whether a file can be uploaded to a given parent folder or not.
    /// It is up to this method to ensure the file can be created on the particular
    /// file system (e.g. by checking whether the folder can even contain files if
    /// it's the root folder).
    /// </summary>
    /// <param name="submittedResourceFilePath">The submitted file path, which can be
    /// used for logging and exception message creation.</param>
    /// <param name="parentFolder">The parent folder of the designated file resource.</param>
    /// <param name="fileItem">Represents the file resource to be uploaded.</param>
    protected abstract void VerifyCanUploadFileToFileSystemLocation(string submittedResourceFilePath, TFolder parentFolder, TFile fileItem);


    /// <summary>
    /// Gets the permissions of the file's parent folder.
    /// </summary>
    /// <param name="fileItem">Represents the file resource to be uploaded.</param>
    /// <param name="parentFolder">Represents the file's parent folder.</param>
    /// <returns>Permissions of the file's parent folder.</returns>
    protected abstract FolderClaims GetParentFolderClaims(TFile fileItem, TFolder parentFolder);


    /// <summary>
    /// Uploads a given data block that contains a chunk of data for an uploaded file.
    /// </summary>
    /// <param name="block">The block to be written.</param>
    /// <exception cref="DataBlockException">If the data block's contents cannot be stored,
    /// either because it's an invalid number, or because only sequential downloads
    /// are possible, and the block does not refer to the current download
    /// position.
    /// </exception>
    /// <exception cref="TransferStatusException">If the transfer has already expired.</exception>
    public virtual void WriteBlock(BufferedDataBlock block)
    {
      SecureAction(FileSystemTask.DataBlockUploadRequest,
                   () => OrchestrateBlockWriting(block, WriteBufferedDataBlockImpl),
                   () =>
                   String.Format("Could not write buffered data block [{0}] for transfer [{1}]", block.BlockNumber,
                                 block.TransferTokenId));
    }


    /// <summary>
    /// Handles the actual writing of the submitted data to the file system.
    /// </summary>
    /// <param name="transfer">The processed transfer.</param>
    /// <param name="dataBlock">A data block that contains a chunk of data
    /// which should be written to the file system.</param>
    protected abstract void WriteBufferedDataBlockImpl(TTransfer transfer, BufferedDataBlock dataBlock);


    /// <summary>
    /// Uploads a given data block that provides a chunk of data for an uploaded file as a stream.
    /// </summary>
    /// <param name="block">The block to be written.</param>
    /// <exception cref="DataBlockException">If the data block's contents cannot be stored,
    /// either because it's an invalid number, or because only sequential downloads
    /// are possible, and the block does not refer to the current download
    /// position.
    /// </exception>
    /// <exception cref="TransferStatusException">If the transfer has already expired.</exception>
    public virtual void WriteBlockStreamed(StreamedDataBlock block)
    {
      SecureAction(FileSystemTask.DataBlockUploadRequest,
             () => OrchestrateBlockWriting(block, WriteStreamedDataBlockImpl),
             () =>
             String.Format("Could not write strea,ed data block [{0}] for transfer [{1}]", block.BlockNumber,
                           block.TransferTokenId));
    }



    /// <summary>
    /// Handles the actual writing of the submitted data to the file system.
    /// </summary>
    /// <param name="transfer">The processed transfer.</param>
    /// <param name="dataBlock">A data block that provides a data stream that
    /// is written to the file system.</param>
    protected abstract void WriteStreamedDataBlockImpl(TTransfer transfer, StreamedDataBlock dataBlock);


    /// <summary>
    /// Orchestrates common block reading procedures for the <see cref="WriteBlock"/> and
    /// <see cref="WriteBlockStreamed"/> methods.
    /// </summary>
    protected virtual void OrchestrateBlockWriting<T>(T dataBlock, Action<TTransfer, T> writerImplFunc) where T : class, IDataBlock
    {
      Ensure.ArgumentNotNull(dataBlock, "dataBlock");

      const FileSystemTask context = FileSystemTask.DataBlockUploadRequest;
      TTransfer transfer = GetCachedTransfer(dataBlock.TransferTokenId, true, context);

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

          AuditHelper.AuditDeniedOperation(Auditor,context, AuditEvent.UploadNoLongerActive, transfer.FileItem, msg);
          throw new TransferStatusException(msg) { IsAudited = true, EventId = (int)AuditEvent.UploadNoLongerActive };
        }

        if(!transfer.HasUploadStarted)
        {
          if(transfer.FileItem.Exists)
          {
            //overwrite an existing file
            DeleteExistingFileImpl(transfer); 
          }

          InitializeFileUploadImpl(transfer);
          transfer.HasUploadStarted = true;
        }

        //update status
        transfer.Status = TransferStatus.Running;

        ValidateBlockSizeAndSettings(transfer, dataBlock);

        //write the data to the file system
        writerImplFunc(transfer, dataBlock);

        //store copy of the uploaded block that doesn't contain any data
        transfer.RegisterBlock(DataBlockInfo.FromDataBlock(dataBlock));

        //audit uploaded block
        AuditHelper.AuditResourceOperation(Auditor,context, AuditEvent.FileBlockUploaded, transfer.FileItem);

        //if we just read the last block, close implicitly
        if (dataBlock.IsLastBlock)
        {
          CompleteTransfer(transfer.TransferId);
        }
      }
    }



    /// <summary>
    /// Validates block sizes, buffers and streams of a submitted data block.
    /// </summary>
    /// <typeparam name="T">Data block type.</typeparam>
    /// <param name="transfer">The processed transfer.</param>
    /// <param name="dataBlock">The validated data block.</param>
    /// <exception cref="DataBlockException">In case of invalid or contradictory settings that can
    /// be detected so far.</exception>
    protected virtual void ValidateBlockSizeAndSettings<T>(TTransfer transfer, T dataBlock) where T:IDataBlock
    {
      var bufferedBlock = dataBlock as BufferedDataBlock;
      if (bufferedBlock != null)
      {
        if (bufferedBlock.Data == null)
        {
          string msg = "Block number {0} of transfer [{1}] for resource [{2}] does not contain a buffer.";
          msg = String.Format(msg, dataBlock.BlockNumber, transfer.TransferId, transfer.FileItem.ResourceInfo.FullName);
          throw new DataBlockException(msg);
        }

        if(bufferedBlock.Data.Length != bufferedBlock.BlockLength)
        {
          string msg = "Block number {0} of transfer [{1}] for resource [{2}] has a buffer of size [{3}] that doesn't match the indicated block size of [{4}].";
          msg = String.Format(msg, dataBlock.BlockNumber, transfer.TransferId, transfer.FileItem.ResourceInfo.FullName,
                              bufferedBlock.Data.Length, bufferedBlock.BlockLength);
          throw new DataBlockException(msg);
        }
      }


      var streamedBlock = dataBlock as StreamedDataBlock;
      if(streamedBlock != null)
      {
        if(streamedBlock.Data == null)
        {
          string msg = "Block number {0} of transfer [{1}] for resource [{2}] does not contain a data stream.";
          msg = String.Format(msg, dataBlock.BlockNumber, transfer.TransferId, transfer.FileItem.ResourceInfo.FullName);
          throw new DataBlockException(msg);
        }

        //do not validate the stream length - if the submitted amount of data is too high,
        //skip it
      }


      //validate block length
      if(dataBlock.BlockLength < 0)
      {
        string msg = "Block number {0} of transfer [{1}] for resource [{2}] indicates a negative block length of [{3}] bytes.";
        msg = String.Format(msg, dataBlock.BlockNumber, transfer.TransferId, transfer.FileItem.ResourceInfo.FullName,
                            dataBlock.BlockLength);
        throw new DataBlockException(msg);
      }

      //validate indicated block size is correct
      long? maxBlockSize = transfer.Token.MaxBlockSize;
      if(maxBlockSize.HasValue && dataBlock.BlockLength > maxBlockSize)
      {
        string msg = "The block's length of [{0}] exceeds the maximum block size of [{1}].";
        msg = String.Format(msg, dataBlock.BlockLength, maxBlockSize);
        throw new DataBlockException(msg);
      }


      //make sure offset does not go beyond boundaries of the maximum resource size
      long? maxResourceSize = transfer.Token.MaxResourceSize;
      if (maxResourceSize.HasValue && dataBlock.Offset + dataBlock.BlockLength > maxResourceSize)
      {
        string msg = "Submitted block's offset of [{0}] plus the block length of [{1}] exceeds the maximum resource size of [{2}].";
        msg = String.Format(msg, dataBlock.Offset, dataBlock.BlockLength, maxResourceSize.Value);
        throw new DataBlockException(msg);
      }
    }


    /// <summary>
    /// This method is being invoked right before the first data block
    /// is being written, and may prepare the file system for the write, if
    /// necessary (e.g. by opening a file stream if not done directly
    /// in the write methods).
    /// </summary>
    /// <param name="transfer">The currently running upload transfer.</param>
    protected abstract void InitializeFileUploadImpl(TTransfer transfer);


    /// <summary>
    /// This method is invoked if a file is about to be overwritten, and
    /// should explicitly make room to upload the new file. It's up to the
    /// implementation whether the file should really be physically deleted,
    /// however.
    /// </summary>
    /// <param name="transfer">The currently running upload transfer.</param>
    protected abstract void DeleteExistingFileImpl(TTransfer transfer);


    /// <summary>
    /// Completes a given file transfer including a verification of the uploaded
    /// data. Invoking this operation closes and removes the transfer from the service. It is highly
    /// recommended to invoke this method after finishing a transfer
    /// in order to free used/locked resources as soon as possible. As an alternative,
    /// the uploading party my set the <see cref="DataBlockInfo.IsLastBlock"/> property of the
    /// last transmitted block to true in order to have the transfer implicitly closed.
    /// </summary>
    /// <param name="transferId">Identifies the current transfer according to the
    /// <see cref="TransferToken.TransferId"/> that was issued.</param>
    /// <param name="md5FileHash">An MD5 file hash that should match the uploaded file.</param>
    /// <returns>The new status of the transfer, which is <see cref="TransferStatus.Completed"/>
    /// in case of a known transfer. If the transfer is not known (maybe because it was aborted
    /// by the system), this method returns <see cref="TransferStatus.UnknownTransfer"/>.</returns>
    /// <exception cref="UnknownTransferException">If the submitted <paramref name="transferId"/>
    /// cannot be mapped to a running transfer.</exception>
    /// <exception cref="IntegrityCheckException">If the integrity check based on the submitted
    /// file hash failed.</exception>
    /// <remarks>Implementing classes basically can use the <see cref="TransferHandlerBase{TFile,TToken,TTransfer}.CompleteTransfer"/>
    /// method, and add the missing functionality to calculate the file hash.</remarks>
    public virtual TransferStatus CompleteTransfer(string transferId, string md5FileHash)
    {
      return SecureFunc(FileSystemTask.UploadTransferCompletion,
                        () => CompleteTransferInternal(transferId, md5FileHash),
                        () => String.Format("Could not verify and complete upload transfer [{0}]", transferId));
    }

    

    /// <summary>
    /// Closes a given transfer, and frees resources that were associated with the
    /// transfer (by invoking the <see cref="TransferHandlerBase{TFile,TToken,TTransfer}.FinalizeTransfer"/> method).
    /// </summary>
    /// <returns>The status depending on the result of the hash verification, which is delegated to the
    /// abstract <see cref="VerifyTransfer"/> method.</returns>
    protected virtual TransferStatus CompleteTransferInternal(string transferId, string md5FileHash)
    {
      const FileSystemTask context = FileSystemTask.UploadTransferCompletion;

      //get the transfer
      TTransfer transfer = GetCachedTransfer(transferId, true, context);

      lock (transfer.ResourceLock)
      {
        bool status = VerifyTransfer(transfer, md5FileHash);
        
        if(status)
        {
          //complete transfer
          return CloseTransferInternal(transfer.TransferId, TransferStatus.Completed, null, context,
                                     AuditEvent.TransferCompleted);
        }


        //cancel transfer if verification failed
        return CloseTransferInternal(transfer.TransferId, TransferStatus.Aborted, AbortReason.VerificationFailure, context,
                                     AuditEvent.TransferHashVerificationError);
      }

    }


    /// <summary>
    /// Verifies uploaded data before committing the transfer. This method is invoked while
    /// processing the <see cref="CompleteTransfer"/> method.
    /// </summary>
    /// <param name="transfer">The transfer that is being finalized.</param>
    /// <param name="md5FileHash">An MD5 file hash that should match the uploaded file.</param>
    /// <returns>True if the submitted file hash corresponds to the data that was uploaded through
    /// the transfer, otherwise false.</returns>
    /// <remarks>Returning true results in an invocation of the
    /// <see cref="TransferHandlerBase{TFile,TToken,TTransfer}.CompleteTransfer"/>
    /// method, while a return value of false cancels the transfer by invoking
    /// <see cref="TransferHandlerBase{TFile,TToken,TTransfer}.CancelTransfer"/>.</remarks>
    protected abstract bool VerifyTransfer(TTransfer transfer, string md5FileHash);



    /// <summary>
    /// Creates or updates a given file resource in the file system in one blocking operation.
    /// </summary>
    /// <param name="virtualFilePath">The qualified path of the file to be created.</param>
    /// <param name="input">A stream that provides the file's contents.</param>
    /// <param name="overwrite">Whether an existing file should be overwritten
    /// or not. If this parameter is false and the file already exists, a
    /// <see cref="ResourceOverwriteException"/> is thrown.</param>
    /// <param name="resourceLength">The length of the resource to be uploaded in bytes.</param>
    /// <param name="contentType">The content type of the uploaded resource.</param>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    /// <exception cref="ResourceOverwriteException">If a file already exists at the
    /// specified location, and the <paramref name="overwrite"/> flag was not set.</exception>
    /// <exception cref="ArgumentNullException">If any of the parameters is a null reference.</exception>
    public virtual void WriteFile(string virtualFilePath, Stream input, bool overwrite, long resourceLength, string contentType)
    {
      SecureAction(FileSystemTask.StreamedFileUploadRequest,
        () => WriteFileImpl(virtualFilePath, input, overwrite, resourceLength, contentType),
        () => String.Format("Could not upload streamed file [{0}]", virtualFilePath));
    }

    private void WriteFileImpl(string virtualFilePath, Stream input, bool overwrite, long resourceLength, string contentType)
    {
      //get a token - this creates a new transfer and validates the request
      UploadToken token = RequestUploadToken(virtualFilePath, overwrite, resourceLength, contentType);

      if (resourceLength == 0)
      {
        //create an empty data block and submit it
        var block = new BufferedDataBlock
        {
          TransferTokenId = token.TransferId,
          BlockLength = 0,
          BlockNumber = 0,
          Data = new byte[0],
          IsLastBlock = true
        };

        WriteBlock(block);
      }
      else
      {
        //we don't expose the underlying stream directly, but rather use blocks again,
        //which disconnects the exposed stream from the underlying resource and allows for simple
        //aborting
        try
        {
          input.WriteTo(token, resourceLength, token.MaxBlockSize ?? 32768, WriteBlockStreamed);
        }
        catch (Exception)
        {
          CancelTransfer(token.TransferId, AbortReason.Undefined);
          throw;
        }

        //we don't need to explicitly complete the transfer - the extension method
        //implicitly closed the transfer by marking the last block:
        //CompleteTransfer(token.TransferId);
      }
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
    public UploadToken ReloadToken(string transferId)
    {
      return GetCachedTransfer(transferId, true, FileSystemTask.UploadTokenRequery).Token;
    }

  }
}