using System;
using System.IO;
using Vfs.Auditing;
using Vfs.Locking;
using Vfs.Scheduling;
using Vfs.Security;
using Vfs.Transfer;
using Vfs.Transfer.Upload;
using Vfs.Util;

namespace Vfs.LocalFileSystem.Transfer
{
  public class LocalUploadHandler : UploadHandlerBase<FileItem, FolderItem, LocalUploadTransfer>
  {
    /// <summary>
    /// Provides access to file system functionality that is required
    /// to manage transfers.
    /// </summary>
    public LocalTransferConfig Config { get; private set; }


    /// <summary>
    /// Gets the maximum block size that can be transferred, if such
    /// a limit exists. Returns null in case of arbitrary block sizes.
    /// </summary>
    public override int? MaxBlockSize
    {
      get { return Config.FileSystemConfiguration.MaxUploadBlockSize; }
    }

    /// <summary>
    /// Indicates how restrictively data blocks may be transmitted. The local
    /// file system allows random block uploads (meaning that the first uploaded
    /// block does not have to contain the first bytes of the file).
    /// </summary>
    public override TransmissionCapabilities TransmissionCapabilities
    {
      get { return TransmissionCapabilities.Random; }
    }


    /// <summary>
    /// Gets the maximum size of an uploaded file. A value of
    /// null indicates no limit is in place.
    /// </summary>
    public override long? GetMaxFileUploadSize()
    {
      return Config.FileSystemConfiguration.MaxUploadFileSize;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    /// <param name="config">Provides access to file system functionality
    /// that is required to manage transfers.</param>
    public LocalUploadHandler(LocalTransferConfig config)
    {
      Ensure.ArgumentNotNull(config, "config");
      Config = config;
    }


    /// <summary>
    /// Initializes the service with a specific <see cref="ITransferStore{TTransfer}"/>
    /// that maintains running transfers.
    /// </summary>
    /// <param name="config">Provides access to file system functionality
    /// that is required to manage transfers.</param>
    /// <param name="transferStore">Provides a storage mechanism for
    /// managed transfers.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="transferStore"/>
    /// is a null reference.</exception>
    public LocalUploadHandler(ITransferStore<LocalUploadTransfer> transferStore,
                                          LocalTransferConfig config) : base(transferStore)
    {
      Ensure.ArgumentNotNull(config, "config");
      Config = config;
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
    protected override FileItem CreateFileItemImpl(string submittedResourceFilePath, bool mustExist, FileSystemTask context)
    {
      //if the file request refers to an existing local directory, abort
      if(Config.Provider.IsFolderAvailable(submittedResourceFilePath))
      {
        string msg = "Upload denied - the submitted path [{0}] refers to an existing folder and cannot be used as a file path.";
        msg = String.Format(msg, submittedResourceFilePath);
        throw new ResourceAccessException(msg);
      }

      return Config.FileResolverFunc(submittedResourceFilePath, mustExist, context);
    }


    protected override FileClaims GetFileClaims(FileItem file)
    {
      return Config.ClaimsResolverFunc(file);
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
    protected override LocalUploadTransfer CreateTransfer(string submittedResourceFilePath, FileItem fileItem, FolderItem parentFolder, UploadToken token, FileClaims claims, ResourceLockGuard lockGuard, Job<UploadToken> expirationJob)
    {
      LocalUploadTransfer transfer = new LocalUploadTransfer(token, fileItem)
                                       {
                                         ParentFolder = parentFolder,
                                         Status = TransferStatus.Starting,
                                         Owner = Config.Provider.Security.GetIdentity()
                                       };

      if (expirationJob != null) transfer.ExpirationNotificationJob = expirationJob;
      if (lockGuard != null) transfer.ResourceLock = lockGuard;

      return transfer;
    }


    /// <summary>
    /// Creates a token for the currently processed resource.
    /// </summary>
    /// <param name="submittedResourceFilePath">The identifier (file path)
    /// for the requested resource, as submitted by the requesting party.</param>
    /// <param name="fileItem">Represents the file resource to be uploaded.</param>
    /// <param name="resourceLength">The length of the resource to be uploaded in bytes.</param>
    /// <param name="contentType">The content type of the uploaded resource.</param>
    /// <returns>A token for the request.</returns>
    protected override UploadToken CreateUploadToken(string submittedResourceFilePath, FileItem fileItem, long resourceLength, string contentType)
    {
      var fileInfo = fileItem.ResourceInfo;
      string transferId = Guid.NewGuid().ToString();

      UploadToken token = new UploadToken
      {
        TransferId = transferId,
        ResourceIdentifier = submittedResourceFilePath,
        CreationTime = SystemTime.Now(),
        ContentType = fileInfo.ContentType,
        MaxResourceSize = resourceLength,
        MaxBlockSize = MaxBlockSize,
        ResourceName = fileInfo.Name,
        ResourceLength = resourceLength
      };

      TimeSpan? expiration = Config.FileSystemConfiguration.UploadTokenExpirationTime;
      if(expiration.HasValue)
      {
        token.ExpirationTime = SystemTime.Now().Add(expiration.Value);
      }

      return token;
    }


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
    protected override ResourceLockGuard LockResourceForUpload(FileItem fileItem)
    {
      return Config.LockResolverFunc(fileItem, ResourceLockType.Write);
    }

    /// <summary>
    /// Resolves the parent folder of a given file item.
    /// </summary>
    /// <param name="fileItem">Represents the file resource to be uploaded.</param>
    /// <param name="context">The file system operation that is being performed during the invocation of
    /// this method. Used for internal auditing.</param>
    /// <returns>The designated parent folder, if any.</returns>
    protected override FolderItem GetParentFolder(FileItem fileItem, FileSystemTask context)
    {
      return Config.FileParentResolverFunc(fileItem, context);
    }

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
    protected override void VerifyCanUploadFileToFileSystemLocation(string submittedResourceFilePath, FolderItem parentFolder, FileItem fileItem)
    {
      //no additional verification needed on the local FS
    }


    /// <summary>
    /// Gets the permissions of the file's parent folder.
    /// </summary>
    /// <param name="fileItem">Represents the file resource to be uploaded.</param>
    /// <param name="parentFolder">Represents the file's parent folder.</param>
    /// <returns>Permissions of the file's parent folder.</returns>
    protected override FolderClaims GetParentFolderClaims(FileItem fileItem, FolderItem parentFolder)
    {
      return Config.FolderClaimsResolverFunc(parentFolder);
    }


    /// <summary>
    /// This method is being invoked right before the first data block
    /// is being written, and may prepare the file system for the write, if
    /// necessary (e.g. by opening a file stream if not done directly
    /// in the write methods).
    /// </summary>
    /// <param name="transfer">The currently running upload transfer.</param>
    protected override void InitializeFileUploadImpl(LocalUploadTransfer transfer)
    {
      //the stream is being created as soon as we write by invocation of the
      //EnsureTargetStream method
    }


    /// <summary>
    /// This method is invoked if a file is about to be overwritten, and
    /// should explicitly make room to uplod the new file. It's up to the
    /// implementation whether the file should really be physically deleted,
    /// however.
    /// </summary>
    /// <param name="transfer">The currently running upload transfer.</param>
    protected override void DeleteExistingFileImpl(LocalUploadTransfer transfer)
    {
      if (File.Exists(transfer.File.FullName))
      {
        transfer.File.Delete();
        transfer.File.Refresh();
      }
    }


    /// <summary>
    /// Verifies uploaded data before committing the transfer. This method is invoked while
    /// processing the <see cref="UploadHandlerBase{TFile,TFolder,TTransfer}.CompleteTransfer(string,string)"/> method.
    /// </summary>
    /// <param name="transfer">The transfer that is being finalized.</param>
    /// <param name="md5FileHash">An MD5 file hash that should match the uploaded file.</param>
    /// <returns>True if the submitted file hash corresponds to the data that was uploaded through
    /// the transfer, otherwise false.</returns>
    /// <remarks>Returning true results in an invocation of the
    /// <see cref="TransferHandlerBase{TFile,TToken,TTransfer}.CompleteTransfer"/>
    /// method, while a return value of false cancels the transfer by invoking
    /// <see cref="TransferHandlerBase{TFile,TToken,TTransfer}.CancelTransfer"/>.</remarks>
    protected override bool VerifyTransfer(LocalUploadTransfer transfer, string md5FileHash)
    {
      //close stream
      CloseStream(transfer);

      //calculate and compare hashes
      var hash = transfer.File.CalculateMd5Hash();
      return hash.Equals(md5FileHash, StringComparison.InvariantCultureIgnoreCase);
    }


    /// <summary>
    /// Performs housekeeping code once a transfer is paused, e.g.
    /// to close an open stream or free other resources.
    /// </summary>
    /// <param name="transfer">The paused transfer.</param>
    protected override void PauseTransferImpl(LocalUploadTransfer transfer)
    {
      CloseStream(transfer);
    }


    /// <summary>
    /// Cleans up a transfer and used FS specific resources after the transfer
    /// was closed. This method is being invoked during the executino of
    /// <see cref="TransferHandlerBase{TFile,TToken,TTransfer}.CloseTransferInternal"/>.
    /// This implementation also removes an incomplete file part in case a transfer
    /// was aborted after having written data to disk.
    /// </summary>
    /// <param name="status">The transfer status.</param>
    /// <param name="abortReason">Indicates why the transfer was aborted, in case the
    /// <paramref name="status"/> is <see cref="TransferStatus.Aborted"/>.</param>
    /// <param name="transfer">The closed transfer.</param>
    protected override void CloseTransferImpl(LocalUploadTransfer transfer, TransferStatus status, AbortReason? abortReason)
    {
      CloseStream(transfer);
      transfer.ClearTransferredBlocks();

      //delete incomplete files (but keep the ones that are still supposed to be overwritten)
      if(status == TransferStatus.Aborted && transfer.HasUploadStarted)
      {
        transfer.File.Refresh();
        if(transfer.File.Exists)
        {
          transfer.File.Delete();
        }
      }
    }



    /// <summary>
    /// Closes and disposes a transfer's <see cref="LocalUploadTransfer.Stream"/>
    /// and sets the property to null.
    /// </summary>
    /// <param name="transfer">The processed transfer.</param>
    private static void CloseStream(LocalUploadTransfer transfer)
    {
      if (transfer.Stream != null)
      {
        transfer.Stream.Dispose();
        transfer.Stream = null;
      }
    }


    /// <summary>
    /// Handles the actual writing of the submitted data to the file system.
    /// </summary>
    /// <param name="transfer">The processed transfer.</param>
    /// <param name="dataBlock">A data block that contains a chunk of data
    /// which should be written to the file system.</param>
    protected override void WriteBufferedDataBlockImpl(LocalUploadTransfer transfer, BufferedDataBlock dataBlock)
    {
      //we might have to open a stream more than once in case the transfer was paused
      EnsureTargetStream(transfer, dataBlock.Offset);
      dataBlock.WriteTo(transfer.Stream);
    }


    /// <summary>
    /// Handles the actual writing of the submitted data to the file system.
    /// </summary>
    /// <param name="transfer">The processed transfer.</param>
    /// <param name="dataBlock">A data block that provides a data stream that
    /// is written to the file system.</param>
    protected override void WriteStreamedDataBlockImpl(LocalUploadTransfer transfer, StreamedDataBlock dataBlock)
    {
      EnsureTargetStream(transfer, dataBlock.Offset);

      //when it comes to reading from the source stream, make sure no limits (block or whole file size)
      //are exceeded

      if(!transfer.Token.MaxResourceSize.HasValue)
      {
        if(!transfer.Token.MaxBlockSize.HasValue)
        {
          //just write the stream - there is no limit
          dataBlock.WriteTo(transfer.Stream);
        }
        else
        {
          //the limit is the maximum block size
          dataBlock.WriteTo(transfer.Stream, transfer.Token.MaxBlockSize.Value);
        }
      }
      else
      {
        //first we need to make sure we won't go over the max resource size
        long maxSize = transfer.Token.MaxResourceSize.Value - dataBlock.Offset;

        if (transfer.Token.MaxBlockSize.HasValue)
        {
          //the other limit is the block size - take whatever is smaller
          maxSize = Math.Min(maxSize, transfer.Token.MaxBlockSize.Value);
        }

        //the limit is the maximum block size
        dataBlock.WriteTo(transfer.Stream, maxSize);
      }
    }



    /// <summary>
    /// Opens a file stream to the target file as indicated by the
    /// submitted <see cref="transfer"/>.
    /// </summary>
    /// <param name="transfer">A currently running upload transfer.</param>
    private static void EnsureTargetStream(LocalUploadTransfer transfer, long position)
    {
      if (transfer.Stream == null)
      {
        //open stream, share full access (it's up to the application layer to restrict write access, VFS does not block)1
        transfer.Stream = transfer.File.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
      }

      transfer.Stream.Position = position;
    }
  }
}
