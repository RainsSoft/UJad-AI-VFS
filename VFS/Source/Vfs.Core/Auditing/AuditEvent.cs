namespace Vfs.Auditing
{
  /// <summary>
  /// Provides a comprehensive set of identifiers for specific incidents that are being
  /// audited. This enum matches the list of event IDs that are part of the VFS documentation.
  /// </summary>
  public enum AuditEvent
  {
    /// <summary>
    /// No further information about the circumstances is available.
    /// </summary>
    Unknown = -1,
    /// <summary>
    /// No event ID was defomed-
    /// </summary>
    Undefined = 0,
    /// <summary>
    /// An unexpected error occurred.
    /// </summary>
    InternalError = 9999,


    /// <summary>
    /// Information about a given file was requested.
    /// </summary>
    FileInfoRequested = 1001,
    /// <summary>
    /// Information about a given folder was requested.
    /// </summary>
    FolderInfoRequested = 1002,
    /// <summary>
    /// Files and/or folders of a given parent folder were requested.
    /// </summary>
    FolderContentsRequested = 1003,

    /// <summary>
    /// A file's data was read from the file system.
    /// </summary>
    FileDataDownloaded = 3001,
    /// <summary>
    /// A download token was issued for a given download request.
    /// </summary>
    DownloadTokenIssued = 3002,
    /// <summary>
    /// A request within the context of a download transfer was denied
    /// because the transfer was already closed or canceled.
    /// </summary>
    DownloadNoLongerActive = 3003,
    /// <summary>
    /// A chunk of data that is part of a download is delivered
    /// to the requesting party.
    /// </summary>
    FileBlockDownloaded = 3004,
    /// <summary>
    /// A download token was renewed.
    /// </summary>
    DownloadTokenRenewed = 3005,
    /// <summary>
    /// Answered a query for running downloads of a given resource.
    /// </summary>
    ResourceDownloadsQuery = 3006,



    /// <summary>
    /// A file was written to the file system.
    /// </summary>
    FileDataUploaded = 3101,
    /// <summary>
    /// An upload token was issued for a given upload request.
    /// </summary>
    UploadTokenIssued = 3102,
    /// <summary>
    /// A request within the context of an upload transfer was denied
    /// because the transfer was already closed or canceled.
    /// </summary>
    UploadNoLongerActive = 3103,
    /// <summary>
    /// A chunk of data that is part of an upload was received.
    /// </summary>
    FileBlockUploaded	= 3104,
    /// <summary>
    /// An upload token was renewed.
    /// </summary>
    UploadTokenRenewed = 3105,
    /// <summary>
    /// Answered a query for a running upload of a given resource.
    /// </summary>
    ResourceUploadQuery	= 3106,



    /// <summary>
    /// A request referred to an unknown transfer or transfer ID.
    /// </summary>
    UnknownTransferRequest = 3202,
    /// <summary>
    /// A request to change the status of a transfer was denied.
    /// </summary>
    InvalidTransferStatusChange = 3203,
    /// <summary>
    /// The status of an active transfer was set to paused.
    /// </summary>
    TransferPaused	= 3204,
    /// <summary>
    /// A transfer was finished.
    /// </summary>
    TransferCompleted = 3205,
    /// <summary>
    /// A transfer was aborted.
    /// </summary>
    TransferCanceled	= 3206,
    /// <summary>
    /// Verification of a transferred resource failed.
    /// </summary>
    TransferHashVerificationError = 3207,

    /// <summary>
    /// A list of data blocks that were transmitted during a given transfer was requested.
    /// </summary>
    TransferredBlockInfoRequest	= 3251,



    /// <summary>
    /// An attempt was made to delete the file system root.
    /// </summary>
    DeleteFileSystemRoot = 4001,
    /// <summary>
    /// Creating a folder was not possible because the folder
    /// already exists on the file system.
    /// </summary>
    FolderAlreadyExists = 4002,
    /// <summary>
    /// A folder was created on the file system.
    /// </summary>
    FolderCreated = 4003,
    /// <summary>
    /// A folder was removed from the file system.
    /// </summary>
    FolderDeleted = 4004,
    /// <summary>
    /// A file was removed from the file system.
    /// </summary>
    FileDeleted = 4005,
    /// <summary>
    /// An operation to move or copy a resource caused
    /// an error, because source and target are the same.
    /// </summary>
    SourceEqualsDestination = 4006,
    /// <summary>
    /// A folder was moved to a new location on the file system.
    /// </summary>
    FolderMoved = 4007,
    /// <summary>
    /// A file was moved to a new location on the file system.
    /// </summary>
    FileMoved = 4008,
    /// <summary>
    /// A folder was copied to a new location on the file system.
    /// </summary>
    FolderCopied = 4009,
    /// <summary>
    /// A file was copied to a new location on the file system.
    /// </summary>
    FileCopied = 4010,
    /// <summary>
    /// Creating a file was not possible because the file
    /// already exists on the file system.
    /// </summary>
    FileAlreadyExists = 4011,
    

    /// <summary>
    /// A request get a read lock for a given file
    /// (and/or related resources) was denied.
    /// </summary>
    FileReadLockDenied = 4101,
    /// <summary>
    /// A request get a write lock for a given file
    /// (and/or related resources) was denied.
    /// </summary>
    FileWriteLockDenied = 4102,
    /// <summary>
    /// A request get a read lock for a given folder
    /// (and/or related resources) was denied.
    /// </summary>
    FolderReadLockDenied = 4103,
    /// <summary>
    /// A request get a write lock for a given folder
    /// (and/or related resources) was denied.
    /// </summary>
    FolderWriteLockDenied = 4104,


    /// <summary>
    /// An attempt to delete a given folder was blocked because the
    /// requesting party does not have permission to delete the folder.
    /// </summary>
    DeleteFolderDenied = 5001,
    /// <summary>
    /// An attempt to delete a given file was blocked because the
    /// requesting party does not have permission to delete the file.
    /// </summary>
    DeleteFileDenied = 5002,
    /// <summary>
    /// Retrieving the contents (files and/or folders) of a given parent
    /// folder was denied.
    /// </summary>
    ListFolderContentsDenied = 5003,
    /// <summary>
    /// A folder was not created because the requesting party did
    /// not have the necessary permission.
    /// </summary>
    CreateFolderDenied = 5004,
    /// <summary>
    /// A file was not created because the requesting party did
    /// not have the necessary permission.
    /// </summary>
    CreateFileDenied = 5005,
    /// <summary>
    /// Access to a given file's data was denied because the requesting party did
    /// not have the necessary permission.
    /// </summary>
    FileDataDownloadDenied = 5006,
    /// <summary>
    /// Overwriting an existing file was requested but denied because the requesting party did
    /// not have the necessary permission.
    /// </summary>
    FileDataOverwriteDenied = 5007,


    /// <summary>
    /// A submitted file path could not be interpreted as a valid resource identifier.
    /// </summary>
    InvalidFilePathFormat = 8001,
    /// <summary>
    /// A submitted folder path could not be interpreted as a valid resource identifier.
    /// </summary>
    InvalidFolderPathFormat = 8002,
    /// <summary>
    /// A file was not found on the file system.
    /// </summary>
    FileNotFound = 8003,
    /// <summary>
    /// A folder was not found on the file system.
    /// </summary>
    FolderNotFound = 8004,
    /// <summary>
    /// Resolving a folder path failed, but the reason is unclear.
    /// Only use this flag if a more detailed <see cref="AuditEvent"/> is not available.
    /// </summary>
    FileResolveFailed = 8005,
    /// <summary>
    /// Resolving a folder path failed, but the reason is unclear.
    /// Only use this flag if a more detailed <see cref="AuditEvent"/> is not available.
    /// </summary>
    FolderResolveFailed = 8006,
    /// <summary>
    /// The submitted resource location is not supposed to be accessed
    /// (e.g. outside of the allowed area on the file system). This could
    /// indicate an attempt to hack the system.
    /// </summary>
    InvalidResourceLocationRequested = 8007
  }
}