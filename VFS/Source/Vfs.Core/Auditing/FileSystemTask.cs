using System;

namespace Vfs.Auditing
{
  /// <summary>
  /// Flags that define the context of an operation
  /// on the file system.
  /// </summary>
  public enum FileSystemTask
  {
    Undefined,
    Unknown,
    All,

    /// <summary>
    /// Common queries about the file system provider's
    /// configuration and/or capabilities.
    /// </summary>
    ProviderMetaDataRequest,

    //single info request
    FileInfoRequest,
    FolderInfoRequest,
    RootFolderInfoRequest,

    //browsing
    FolderContentsRequest,
    ChildFilesRequest,
    ChildFoldersRequest,
    FileParentRequest,
    FolderParentRequest,
    CheckFileAvailability,
    CheckFolderAvailability,
    
    //operations
    FileMoveRequest,
    FileCopyRequest,
    FileDeleteRequest,
    FolderMoveRequest,
    FolderCopyRequest,
    FolderDeleteRequest,
    FolderCreateRequest,

    //file transfers
    DownloadTokenRequest,
    UploadTokenRequest,
    DownloadTokenRequery,
    UploadTokenRequery,
    StreamedFileDownloadRequest,
    StreamedFileUploadRequest,
    DataBlockDownloadRequest,
    DataBlockUploadRequest,
    DownloadTransferCompletion,
    UploadTransferCompletion,
    DownloadTransferExpiration,
    UploadTransferExpiration,
    DownloadTransferCanceling,
    UploadTransferCanceling,
    DownloadTransferPauseRequest,
    UploadTransferPauseRequest,
    DownloadTransferStatusRequest,
    UploadTransferStatusRequest,
    DownloadedBlockInfosRequest,
    UploadedBlockInfosRequest,
    DownloadTransfersByResourceQuery,
    UploadTransferByResourceQuery,
    

    CreateFolderPathRequest,
    CreateFilePathRequest
  }
}