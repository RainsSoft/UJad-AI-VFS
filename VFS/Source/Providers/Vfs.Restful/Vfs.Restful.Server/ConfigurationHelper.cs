using System;
using System.Collections.Generic;
using OpenRasta.Codecs;
using OpenRasta.Configuration;
using OpenRasta.DI;
using OpenRasta.OperationModel.Interceptors;
using OpenRasta.Web;
using Vfs.Restful.Server.Codecs;
using Vfs.Restful.Server.Handlers;
using Vfs.Restful.Server.Handlers.Browse;
using Vfs.Restful.Server.Handlers.Download;
using Vfs.Restful.Server.Handlers.Operations;
using Vfs.Restful.Server.Handlers.Upload;
using Vfs.Restful.Server.Resources;
using Vfs.Transfer;

namespace Vfs.Restful.Server
{
  /// <summary>
  /// Provides code to perform default configuration operations for the VFS service.
  /// </summary>
  public static class ConfigurationHelper
  {
    /// <summary>
    /// Registers a file system provider instance with Open Rasta's dependency
    /// injection mechanism.
    /// </summary>
    /// <param name="provider">The file system provider to be used by the handlers that process
    /// incoming VFS requests.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="provider"/>
    /// is a null reference.</exception>
    public static void RegisterFileSystemProvider(IFileSystemProvider provider)
    {
      Ensure.ArgumentNotNull(provider, "provider");
      ResourceSpace.Uses.Resolver.AddDependencyInstance(typeof(IFileSystemProvider), provider);
    }


    /// <summary>
    /// Registers service settings with Open Rasta's dependency injection mechanism.
    /// </summary>
    /// <param name="settings">Common settings that can be retrieved by the handler classes
    /// if required.</param>
    public static void RegisterSettings(VfsServiceSettings settings)
    {
      Ensure.ArgumentNotNull(settings, "settings");
      ResourceSpace.Uses.Resolver.AddDependencyInstance<VfsServiceSettings>(settings);
    }

    /// <summary>
    /// Registers the <see cref="VfsExceptionInterceptor"/>, which processes exceptions and
    /// adjusts the returned HTTP status and response data accordingly.
    /// </summary>
    public static void RegisterExceptionInterceptor()
    {
      ResourceSpace.Uses.CustomDependency<IOperationInterceptor, VfsExceptionInterceptor>(DependencyLifetime.Transient);
    }




    /// <summary>
    /// Registers handlers for purely administrative use, such
    /// as to get running transfers for a resource.
    /// </summary>
    public static void RegisterAdministrativeHandlers()
    {
      throw new NotImplementedException(""); //TODO provide implementation  
    }


    /// <summary>
    /// Registers default handler methods for the restful source.
    /// The action URIs are taken from the Settings class, which allows
    /// customization, should it be needed.
    /// </summary>
    public static void RegisterDefaultHandlers()
    {
      //register fault handler
      ResourceSpace.Has.ResourcesOfType<VfsFault>()
        .WithoutUri
        .TranscodedBy<VfsFaultCodec>();

      RegisterBrowsingHandlers();
      RegisterPathCreationHandlers();
      RegisterOperationHandlers();

      RegisterDownloadHandlers();
      RegisterDownloadStatusHandlers();
      
      RegisterUploadStatusHandlers();
      RegisterUploadHandlers();
    }


    private static void RegisterUploadHandlers()
    {
      var uris = VfsUris.Default;

      //request upload token
      ResourceSpace.Has.ResourcesOfType<UploadToken>()
        .AtUri(uris.GetUploadTokenUri)
        .HandledBy<GetUploadTokenHandler>()
        .AsXmlDataContract();


      ResourceSpace.Has.ResourcesOfType<UploadToken>()
        .AtUri(uris.ReloadUploadTokenUri)
        .HandledBy<ReloadUploadTokenHandler>()
        .AsXmlDataContract();


      //write file stream
      ResourceSpace.Has.ResourcesOfType<VirtualFileInfo>()
        .AtUri(uris.WriteFileContentsUri)
        .HandledBy<WriteFileHandler>()
        .AsXmlDataContract();

      //write streamed data block
      ResourceSpace.Has.ResourcesOfType<OperationResult>()
        .AtUri(uris.WriteStreamedDataBlockUri)
        .Named("WriteDataBlock")
        .HandledBy<WriteDataBlockHandler>();
    }



    private static void RegisterOperationHandlers()
    {
      var uris = VfsUris.Default;

      //move folder
      ResourceSpace.Has.ResourcesOfType<VirtualFolderInfo>()
        .AtUri(uris.MoveFolderUri)
        .Named("MoveFolder")
        .HandledBy<MoveFolderHandler>()
        .AsXmlDataContract();

      //copy folder
      ResourceSpace.Has.ResourcesOfType<VirtualFolderInfo>()
        .AtUri(uris.CopyFolderUri)
        .Named("CopyFolder")
        .HandledBy<CopyFolderHandler>()
        .AsXmlDataContract();

      //move file
      ResourceSpace.Has.ResourcesOfType<VirtualFileInfo>()
        .AtUri(uris.MoveFileUri)
        .Named("MoveFile")
        .HandledBy<MoveFileHandler>()
        .AsXmlDataContract();

      //copy file
      ResourceSpace.Has.ResourcesOfType<VirtualFileInfo>()
        .AtUri(uris.CopyFileUri)
        .Named("CopyFile")
        .HandledBy<CopyFileHandler>()
        .AsXmlDataContract();

//      //delete folder
//      ResourceSpace.Has.ResourcesOfType<OperationResult>()
//        .AtUri(uris.DeleteFolderUri)
//        .Named("DeleteFolder")
//        .HandledBy<DeleteFolderHandler>();

//      //create folder
//      ResourceSpace.Has.ResourcesOfType<VirtualFolderInfo>()
//        .AtUri(uris.CreateFolderUri)
//        .HandledBy<CreateFolderHandler>()
//        .AsXmlDataContract();

//      //delete file
//      ResourceSpace.Has.ResourcesOfType<OperationResult>()
//        .AtUri(uris.DeleteFileUri)
//        .Named("DeleteFile")
//        .HandledBy<DeleteFileHandler>();
    }


    private static void RegisterPathCreationHandlers()
    {
      var uris = VfsUris.Default;

      ResourceSpace.Has.ResourcesOfType<Wrapped<string>>()
        .AtUri(uris.CreateFilePathUri)
        .HandledBy<FilePathCreationHandler>()
        .AsXmlDataContract();


      ResourceSpace.Has.ResourcesOfType<Wrapped<string>>()
        .AtUri(uris.CreateFolderPathUri)
        .HandledBy<FolderPathCreationHandler>()
        .AsXmlDataContract();
    }


    private static void RegisterDownloadHandlers()
    {
      var uris = VfsUris.Default;

      //read file as stream
      ResourceSpace.Has.ResourcesOfType<FileDataResource>()
        .AtUri(uris.ReadFileContentsByTokenUri)
        .HandledBy<ReadFileByTokenHandler>();

      //read file as stream
      ResourceSpace.Has.ResourcesOfType<FileDataResource>()
        .AtUri(uris.ReadFileContentsUri)
        .HandledBy<ReadFileByPathHandler>();

      //read data block
      ResourceSpace.Has.ResourcesOfType<StreamedDataBlock>()
        .AtUri(uris.GetDataBlockStreamedUri)
        .HandledBy<ReadDataBlockHandler>()
        .TranscodedBy<DataBlockCodec>();

      //request download token
      ResourceSpace.Has.ResourcesOfType<DownloadToken>()
        .AtUri(uris.GetDownloadTokenWithMaxBlockSizeUri)
        .And
        .AtUri(uris.GetDownloadTokenUri)
        .HandledBy<GetDownloadTokenHandler>()
        .AsXmlDataContract();
      
      //reload download token
      ResourceSpace.Has.ResourcesOfType<DownloadToken>()
        .AtUri(uris.ReloadDownloadTokenUri)
        .HandledBy<ReloadDownloadTokenHandler>()
        .AsXmlDataContract();
    }



    private static void RegisterDownloadStatusHandlers()
    {
      var uris = VfsUris.Default;

      //get max block size
      ResourceSpace.Has.ResourcesOfType<Wrapped<int?>>()
        .AtUri(uris.GetMaxDownloadBlockSizeUri)
        .Named("GetMaxDownloadBlockSize")
        .HandledBy<GetMaxDownloadBlockSizeHandler>()
        .AsXmlDataContract();

      //get transmission capabilities
      ResourceSpace.Has.ResourcesOfType<Wrapped<TransmissionCapabilities>>()
        .AtUri(uris.GetDownloadTransmissionCapabilitiesUri)
        .Named("GetDownloadCapabilities")
        .HandledBy<GetDownloadCapabilitiesHandler>()
        .AsXmlDataContract();

      //get transfer status
      ResourceSpace.Has.ResourcesOfType<Wrapped<TransferStatus>>()
        .AtUri(uris.GetDownloadTransferStatusUri)
        .Named("GetDownloadTransferStatus")
        .HandledBy<GetDownloadTransferStatusHandler>()
        .AsXmlDataContract();

      //complete transfer
      ResourceSpace.Has.ResourcesOfType<Wrapped<TransferStatus>>()
        .AtUri(uris.CompleteDownloadTransferUri)
        .Named("CompleteDownloadTransfer")
        .HandledBy<CompleteDownloadTransferHandler>()
        .AsXmlDataContract();

      //cancel transfer
      ResourceSpace.Has.ResourcesOfType<Wrapped<TransferStatus>>()
        .AtUri(uris.CancelDownloadTransferUri)
        .Named("CancelDownloadTransfer")
        .HandledBy<CancelDownloadTransferHandler>()
        .AsXmlDataContract();


      //pause transfer
      ResourceSpace.Has.ResourcesOfType<OperationResult>()
        .AtUri(uris.PauseDownloadTransferUri)
        .Named("PauseDownloadTransfer")
        .HandledBy<PauseDownloadTransferHandler>();


    }


    private static void RegisterUploadStatusHandlers()
    {
      var uris = VfsUris.Default;

      //get max block size
      ResourceSpace.Has.ResourcesOfType<Wrapped<int?>>()
        .AtUri(uris.GetMaxUploadBlockSizeUri)
        .Named("GetMaxUploadBlockSize")
        .HandledBy<GetMaxUploadBlockSizeHandler>()
        .AsXmlDataContract();

      //get transmission capabilities
      ResourceSpace.Has.ResourcesOfType<Wrapped<TransmissionCapabilities>>()
        .AtUri(uris.GetUploadTransmissionCapabilitiesUri)
        .Named("GetUploadCapabilities")
        .HandledBy<GetUploadCapabilitiesHandler>()
        .AsXmlDataContract();

      //get max file upload size
      ResourceSpace.Has.ResourcesOfType<Wrapped<long?>>()
        .AtUri(uris.GetMaxFileUploadSizeUri)
        .Named("GetMaxFileUploadSize")
        .HandledBy<GetMaxFileUploadSizeHandler>()
        .AsXmlDataContract();

      //get transfer status
      ResourceSpace.Has.ResourcesOfType<Wrapped<TransferStatus>>()
        .AtUri(uris.GetUploadTransferStatusUri)
        .Named("GetUploadTransferStatus")
        .HandledBy<GetUploadTransferStatusHandler>()
        .AsXmlDataContract();

      //complete transfer
      ResourceSpace.Has.ResourcesOfType<Wrapped<TransferStatus>>()
        .AtUri(uris.CompleteUploadTransferUri)
        .Named("CompleteUploadTransfer")
        .HandledBy<CompleteUploadTransferHandler>()
        .AsXmlDataContract();

      //complete with MD5 verification
      ResourceSpace.Has.ResourcesOfType<OperationResult>()
        .AtUri(uris.CompleteUploadTransferWithVerificationUri)
        .Named("CompleteAndVerifyUploadTransfer")
        .HandledBy<CompleteUploadTransferHandler>();

      //cancel transfer
      ResourceSpace.Has.ResourcesOfType<Wrapped<TransferStatus>>()
        .AtUri(uris.CancelUploadTransferUri)
        .Named("CancelUploadTransfer")
        .HandledBy<CancelUploadTransferHandler>()
        .AsXmlDataContract();

      //pause transfer
      ResourceSpace.Has.ResourcesOfType<OperationResult>()
        .AtUri(uris.PauseUploadTransferUri)
        .Named("PauseUploadTransfer")
        .HandledBy<PauseUploadTransferHandler>();
    }



    /// <summary>
    /// Registers handlers and URIs for browsing requests.
    /// </summary>
    private static void RegisterBrowsingHandlers()
    {
      var uris = VfsUris.Default;

      //get folder info
      //needs a name in order to be properly distinguished by ORs mapping
      //engine
      ResourceSpace.Has.ResourcesOfType<VirtualFolderInfo>()
        .AtUri(uris.GetFolderInfoUri)
        .HandledBy<GetCreateOrDeleteFolderHandler>()
        .AsXmlDataContract();

      //get root folder
      ResourceSpace.Has.ResourcesOfType<VirtualFolderInfo>()
        .AtUri(uris.GetFileSystemRootUri)
        .HandledBy<GetRootFolderHandler>()
        .AsXmlDataContract();

      //get file parent
      ResourceSpace.Has.ResourcesOfType<VirtualFolderInfo>()
      .AtUri(uris.GetFileParentUri)
      .Named("GetFileParent")
      .HandledBy<GetFileParentHandler>()
      .AsXmlDataContract();

      //get folder parent
      //needs a name in order to be properly distinguished by ORs mapping
      //engine
      ResourceSpace.Has.ResourcesOfType<VirtualFolderInfo>()
      .AtUri(uris.GetFolderParentUri)
      .Named("GetFolderParent")
      .HandledBy<GetFolderParentHandler>()
      .AsXmlDataContract();

      //get / delete file
      ResourceSpace.Has.ResourcesOfType<VirtualFileInfo>()
        .AtUri(uris.GetFileInfoUri)
        .HandledBy<GetOrDeleteFileHandler>()
        .AsXmlDataContract();

      //get folder's child files
      ResourceSpace.Has.ResourcesOfType<IEnumerable<VirtualFileInfo>>()
        .AtUri(uris.GetChildFilesUri)
        .And
        .AtUri(uris.GetChildFilesFilteredUri)
        .HandledBy<GetChildFilesHandler>()
        .AsXmlDataContract();

      //get folder's child folders
      ResourceSpace.Has.ResourcesOfType<IEnumerable<VirtualFolderInfo>>()
        .AtUri(uris.GetChildFoldersUri)
        .And
        .AtUri(uris.GetChildFoldersFilteredUri)
        .HandledBy<GetChildFoldersHandler>()
        .AsXmlDataContract();

      //get folder contents
      ResourceSpace.Has.ResourcesOfType<FolderContentsInfo>()
        .AtUri(uris.GetFolderContentsUri)
        .And
        .AtUri(uris.GetFolderContentsFilteredUri)
        .HandledBy<GetFolderContentsHandler>()
        .AsXmlDataContract();

      //check file availability
      ResourceSpace.Has.ResourcesOfType<Wrapped<bool>>()
        .AtUri(uris.IsFileAvailableUri)
        .HandledBy<FileAvailabilityCheckHandler>()
        .AsXmlDataContract();

      //check folder availability
      ResourceSpace.Has.ResourcesOfType<Wrapped<bool>>()
        .AtUri(uris.IsFolderAvailableUri)
        .HandledBy<FolderAvailabilityCheckHandler>()
        .AsXmlDataContract();

    }
  }
}