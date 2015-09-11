using System;
using System.IO;
using Vfs.Auditing;
using Vfs.Locking;
using Vfs.Scheduling;
using Vfs.Security;
using Vfs.Transfer;
using Vfs.Transfer.Upload;
using Vfs.Util;
using Vfs.Util.TemporaryStorage;

namespace Vfs.Zip.Transfer
{
  /// <summary>
  /// Handles uploads into the ZIP file.
  /// </summary>
  public class ZipUploadHandler : TempUploadHandlerBase<ZipFileItem, ZipFolderItem, ZipUploadTransfer>
  {

    /// <summary>
    /// Provides access to file system functionality that is required
    /// to manage transfers.
    /// </summary>
    public ZipTransferConfig Config { get; private set; }


    /// <summary>
    /// Gets the maximum block size that can be transferred, if such
    /// a limit exists. Returns null in case of arbitrary block sizes.
    /// </summary>
    public override int? MaxBlockSize
    {
      get { return Config.FileSystemConfiguration.MaxUploadBlockSize; }
    }

    /// <summary>
    /// Indicates how restrictively data blocks may be transmitted.
    /// </summary>
    public override TransmissionCapabilities TransmissionCapabilities
    {
      get { return TransmissionCapabilities.Random; }
    }


    /// <summary>
    /// Initializes the service.
    /// </summary>
    /// <param name="configuration">Provides access to file system functionality
    /// that is required to manage transfers.</param>
    public ZipUploadHandler(ZipTransferConfig configuration)
    {
      Ensure.ArgumentNotNull(configuration, "configuration");
      Config = configuration;
    }


    /// <summary>
    /// Initializes the service with a specific <see cref="ITransferStore{TTransfer}"/>
    /// that maintains running transfers.
    /// </summary>
    /// <param name="configuration">Provides access to file system functionality
    /// that is required to manage transfers.</param>
   /// <param name="transferStore">Provides a storage mechanism for
    /// managed transfers.</param>
    public ZipUploadHandler(ZipTransferConfig configuration, ITransferStore<ZipUploadTransfer> transferStore)
      : base(transferStore)
    {
      Ensure.ArgumentNotNull(configuration, "configuration");
      Config = configuration;
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
    protected override ZipFileItem CreateFileItemImpl(string submittedResourceFilePath, bool mustExist, FileSystemTask context)
    {
      //if the file request refers to an existing local directory, abort
      if (Config.Provider.IsFolderAvailable(submittedResourceFilePath))
      {
        string msg = "Upload denied - the submitted path [{0}] refers to an existing folder and cannot be used as a file path.";
        msg = String.Format(msg, submittedResourceFilePath);
        throw new ResourceAccessException(msg);
      }

      return Config.FileResolverFunc(submittedResourceFilePath, mustExist, context);
    }

    /// <summary>
    /// Gets the access rights for the requested file. This method
    /// is invoked in order to check whether the requesting party
    /// is allowed to read the resource.
    /// </summary>
    /// <param name="file">Represents the requested resource.</param>
    /// <returns>Access rights for the file.</returns>
    protected override FileClaims GetFileClaims(ZipFileItem file)
    {
      return Config.ClaimsResolverFunc(file);
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
    protected override ZipUploadTransfer CreateTransfer(string submittedResourceFilePath, ZipFileItem fileItem, ZipFolderItem parentFolder, UploadToken token, FileClaims claims, ResourceLockGuard lockGuard, Job<UploadToken> expirationJob)
    {
      var transfer = new ZipUploadTransfer(token, fileItem)
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
    protected override UploadToken CreateUploadToken(string submittedResourceFilePath, ZipFileItem fileItem, long resourceLength, string contentType)
    {
      var fileInfo = fileItem.ResourceInfo;
      string transferId = Guid.NewGuid().ToString();

      var token = new UploadToken
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
      if (expiration.HasValue)
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
    protected override ResourceLockGuard LockResourceForUpload(ZipFileItem fileItem)
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
    protected override ZipFolderItem GetParentFolder(ZipFileItem fileItem, FileSystemTask context)
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
    protected override void VerifyCanUploadFileToFileSystemLocation(string submittedResourceFilePath, ZipFolderItem parentFolder, ZipFileItem fileItem)
    {
      //no additional verification needed on the local FS
    }


    /// <summary>
    /// Gets the permissions of the file's parent folder.
    /// </summary>
    /// <param name="fileItem">Represents the file resource to be uploaded.</param>
    /// <param name="parentFolder">Represents the file's parent folder.</param>
    /// <returns>Permissions of the file's parent folder.</returns>
    protected override FolderClaims GetParentFolderClaims(ZipFileItem fileItem, ZipFolderItem parentFolder)
    {
      return Config.FolderClaimsResolverFunc(parentFolder);
    }


    /// <summary>
    /// This method is invoked if a file is about to be overwritten, and
    /// should explicitly make room to upload the new file. It's up to the
    /// implementation whether the file should really be physically deleted,
    /// however.
    /// </summary>
    /// <param name="transfer">The currently running upload transfer.</param>
    protected override void DeleteExistingFileImpl(ZipUploadTransfer transfer)
    {
      //do nothing - keep the original file, and overwrite if necessary
    }


    /// <summary>
    /// Returns a reference to temporary data that can be used to
    /// store received resource data.
    /// </summary>
    /// <param name="transfer">The currently processed transfer.</param>
    /// <returns>A <see cref="TempStream"/> items which is used to store
    /// received file chunks.</returns>
    protected override TempStream CreateTempDataItem(ZipUploadTransfer transfer)
    {
      return Config.FileSystemConfiguration.TempStreamFactory.CreateTempStream();
    }


    /// <summary>
    /// Writes the temporary file to the ZIP file.
    /// </summary>
    /// <param name="transfer">The completed transfer.</param>
    /// <param name="tempStream">The temporary file that provides the uploaded
    /// resource data.</param>
    protected override void CommitCompletedFile(ZipUploadTransfer transfer, TempStream tempStream)
    {
      var repository = Config.Repository;
      Action action = () =>
                        {
                          //remove existing node, if any
                          var node = repository.TryFindNode(transfer.FileItem.QualifiedIdentifier);
                          if(node != null && node.FileEntry != null) repository.ZipFile.RemoveEntry(node.FileEntry);

                          //add file
                          var filePath = transfer.FileItem.QualifiedIdentifier;
                          tempStream.Position = 0;
                          repository.ZipFile.AddEntry(filePath, tempStream);
                        };

      repository.PerformWriteAction(action);
    }
  }
}
