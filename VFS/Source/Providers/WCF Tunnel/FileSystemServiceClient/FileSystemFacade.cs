using System;
using System.Collections.Generic;
using System.IO;
using ACorns.WCF.DynamicClientProxy;
using Vfs.Auditing;
using Vfs.FileSystemService;
using Vfs.Transfer;
using Vfs.Util;

namespace Vfs.FileSystemServiceClient
{
  /// <summary>
  /// Provides a local façade to a WCF file system service.
  /// </summary>
  public class FileSystemFacade : FileSystemProviderBase
  {
    private IFSOperationService operationService;

    /// <summary>
    /// A service proxy that provides access to the file system
    /// service's common operations.
    /// </summary>
    /// <exception cref="ArgumentNullException">If <paramref name="value"/>
    /// is a null reference.</exception>
    public IFSOperationService OperationService
    {
      get { return operationService; }
      set
      {
        Ensure.ArgumentNotNull(value, "value");
        operationService = value;
      }
    }

    private IFSReaderService readerService;

    /// <summary>
    /// A service proxy that provides access to the file system
    /// service's file download functionality.
    /// </summary>
    /// <exception cref="ArgumentNullException">If <paramref name="value"/>
    /// is a null reference.</exception>
    public IFSReaderService ReaderService
    {
      get { return readerService; }
      set
      {
        Ensure.ArgumentNotNull(value, "value");
        readerService = value;
        downloadFacade.ReaderService = value;
      }
    }


    private IFSDataDownloadService downloadService;

    /// <summary>
    /// A service proxy that provides access to the file system
    /// service's file download functionality.
    /// </summary>
    /// <exception cref="ArgumentNullException">If <paramref name="value"/>
    /// is a null reference.</exception>
    public IFSDataDownloadService DownloadService
    {
      get { return downloadService; }
      set
      {
        Ensure.ArgumentNotNull(value, "value");
        downloadService = value;
        downloadFacade.DownloadService = value;
      }
    }


    private IFSWriterService writerService;

    /// <summary>
    /// A service proxy that provides access to the file system
    /// service's file upload functionality.
    /// </summary>
    public IFSWriterService WriterService
    {
      get { return writerService; }
      set
      {
        Ensure.ArgumentNotNull(value, "value");
        writerService = value;
        uploadFacade.WriterService = value;
      }
    }


    private readonly DownloadTransferFacade downloadFacade = new DownloadTransferFacade();

    /// <summary>
    /// Manages download requests from the file system.
    /// </summary>
    public override IDownloadTransferHandler DownloadTransfers
    {
      get { return downloadFacade; }
    }


    private readonly UploadTransferFacade uploadFacade = new UploadTransferFacade();

    /// <summary>
    /// Manages uploads to the file system.
    /// </summary>
    public override IUploadTransferHandler UploadTransfers
    {
      get { return uploadFacade; }
    }


    /// <summary>
    /// Inits an empty facade. You must set the
    /// <see cref="OperationService"/>, <see cref="ReaderService"/>,
    /// and <see cref="WriterService"/> properties manually in order
    /// to use the façade.
    /// </summary>
    public FileSystemFacade()
    {
    }


    /// <summary>
    /// Inits the façade and assigns the services to be used on the fly.
    /// </summary>
    /// <param name="operationService">File system operation service to be used.</param>
    /// <param name="readerService">Data reader service to be used.</param>
    /// <param name="writerService">Data writer service to be used.</param>
    /// <exception cref="ArgumentNullException">If any of the service parameters is a
    /// null reference.</exception>
    public FileSystemFacade(IFSOperationService operationService, IFSReaderService readerService,
                            IFSWriterService writerService)
    {
      OperationService = operationService;
      ReaderService = readerService;
      WriterService = writerService;
    }


    /// <summary>
    /// Inits the façade with declarative data taken from the application configuration
    /// file (app.config).
    /// </summary>
    /// <param name="operationServiceEndpointName">Name of the configuration of the
    /// file system operation service to be used.</param>
    /// <param name="readerServiceEndpointName">Name of the configuration of the
    /// data reader service to be used.</param>
    /// <param name="writerServiceEndpointName">Name of the configuration of the
    /// data writer service to be used.</param>
    public FileSystemFacade(string operationServiceEndpointName, string readerServiceEndpointName,
                            string writerServiceEndpointName)
    {
      OperationService = WCFClientProxy<IFSOperationService>.GetReusableInstance(operationServiceEndpointName);
      ReaderService = WCFClientProxy<IFSReaderService>.GetReusableInstance(readerServiceEndpointName);
      WriterService = WCFClientProxy<IFSWriterService>.GetReusableInstance(writerServiceEndpointName);
    }


    /// <summary>
    /// Gets the root of the file system. This is a dummy folder, which
    /// represents the file system as a whole, and provides the top level contents
    /// of the underlying file system as files and folders.
    /// </summary>
    public override VirtualFolderInfo GetFileSystemRoot()
    {
      return SecureFunc(FileSystemTask.RootFolderInfoRequest, () => OperationService.GetFileSystemRoot(),
                        () => "An error occurred while retrieving the file system root");
    }

    /// <summary>
    /// Gets the binary contents as a stream in a blocking operation.
    /// Use the methods in <see cref="ContentUtil"/> class for simplified stream
    /// handling.
    /// </summary>
    /// <param name="virtualFilePath">The path of the file to be read.</param>
    /// <returns>A stream that allows the contents of the file to be read.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="virtualFilePath"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the file that is represented
    /// by <paramref name="virtualFilePath"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    public override Stream ReadFileContents(string virtualFilePath)
    {
      return SecureFunc(FileSystemTask.StreamedFileDownloadRequest,
                        () => DownloadService.ReadFile(virtualFilePath),
                        () => String.Format("Could not read data of file '{0}.", virtualFilePath));
    }

    /// <summary>
    /// Gets meta data about a given file which is identified
    /// by its path within the file system.
    /// </summary>
    /// <param name="virtualFilePath">Path information that allows
    /// the provider to identify the requested resource.</param>
    /// <returns>A <see cref="VirtualFileInfo"/> instance which provides
    /// meta data about the file.</returns>
    /// <exception cref="VirtualResourceNotFoundException">If the file cannot
    /// be found.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    public override VirtualFileInfo GetFileInfo(string virtualFilePath)
    {
      return SecureFunc(FileSystemTask.FileInfoRequest, () => OperationService.GetFileInfo(virtualFilePath),
                        () => String.Format("Could not get meta data for file [{0}].", virtualFilePath));
    }

    /// <summary>
    /// Gets meta data about a given folder which is identified
    /// by its path within the file system.
    /// </summary>
    /// <param name="virtualFolderPath">Path information that allows
    /// the provider to identify the requested resource.</param>
    /// <returns>A <see cref="VirtualFolderInfo"/> instance which provides
    /// meta data about the folder.</returns>
    /// <exception cref="VirtualResourceNotFoundException">If the folder cannot
    /// be found.</exception>
    /// <exception cref="ResourceAccessException">If the user does not have
    /// permission to access this resource.</exception>
    public override VirtualFolderInfo GetFolderInfo(string virtualFolderPath)
    {
      return SecureFunc(FileSystemTask.FolderInfoRequest, () => OperationService.GetFolderInfo(virtualFolderPath),
                        () => String.Format("Could not get meta data for folder [{0}].", virtualFolderPath));
    }

    /// <summary>
    /// Gets the parent folder of a given file system resource.
    /// </summary>
    /// <param name="childFilePath">The qualified path (<see cref="VirtualResourceInfo.FullName"/>
    /// of a file that is used to resolve the parent folder.</param>
    /// <returns>The parent of the file.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="childFilePath"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the file that is represented
    /// by <paramref name="childFilePath"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of an invalid or prohibited
    /// resource access.</exception>
    public override VirtualFolderInfo GetFileParent(string childFilePath)
    {
      return SecureFunc(FileSystemTask.FileParentRequest, () => OperationService.GetFileParent(childFilePath),
                        () => String.Format("Could not get meta data about parent folder of file [{0}].", childFilePath));
    }

    /// <summary>
    /// Gets the parent folder of a given file system resource.
    /// </summary>
    /// <param name="childFolderPath">The qualified name (<see cref="VirtualResourceInfo.FullName"/>)
    /// of an arbitrary folder that is used to resolve the parent folder.</param>
    /// <returns>The parent of the folder.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="childFolderPath"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the folder that is represented
    /// by <paramref name="childFolderPath"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">If the submitted child folder already
    /// represents the file system root.</exception>
    public override VirtualFolderInfo GetFolderParent(string childFolderPath)
    {
      return SecureFunc(FileSystemTask.FolderParentRequest, () => OperationService.GetFolderParent(childFolderPath),
                        () =>
                        String.Format("Could not get meta data about parent folder of folder [{0}].", childFolderPath));
    }

    /// <summary>
    /// Gets all child folders of a given folder.
    /// </summary>
    /// <param name="parentFolderPath">The <see cref="VirtualResourceInfo.FullName"/>, which
    /// identifies the parent folder within the file system.</param>
    /// <returns>The child folders of the folder.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="parentFolderPath"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the folder that is represented
    /// by <paramref name="parentFolderPath"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    public override IEnumerable<VirtualFolderInfo> GetChildFolders(string parentFolderPath)
    {
      return SecureFunc(FileSystemTask.ChildFoldersRequest, () => OperationService.GetChildFolders(parentFolderPath),
                        () => String.Format("Could not get child folders of folder [{0}].", parentFolderPath));
    }

    /// <summary>
    /// Gets all child files of a given folder.
    /// </summary>
    /// <param name="parentFolderPath">The <see cref="VirtualResourceInfo.FullName"/>, which
    /// identifies the parent folder within the file system.</param>
    /// <returns>The files of the submitted folder.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="parentFolderPath"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the folder that is represented
    /// by <paramref name="parentFolderPath"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    public override IEnumerable<VirtualFileInfo> GetChildFiles(string parentFolderPath)
    {
      return SecureFunc(FileSystemTask.ChildFilesRequest, () => OperationService.GetChildFiles(parentFolderPath),
                        () => String.Format("Could not get child files of folder [{0}].", parentFolderPath));
    }

    /// <summary>
    /// Checks whether a file resource at a given path exists or not.
    /// </summary>
    /// <param name="virtualFilePath">A path to the requested file.</param>
    /// <returns>True if a matching file was found.</returns>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    public override bool IsFileAvailable(string virtualFilePath)
    {
      return SecureFunc(FileSystemTask.CheckFileAvailability, () => OperationService.IsFileAvailable(virtualFilePath),
                        () => String.Format("Could not check whether file [{0}] is available.", virtualFilePath));
    }

    /// <summary>
    /// Checks whether a folder resource at a given path exists or not.
    /// </summary>
    /// <param name="virtualFolderPath">A path to the requested folder.</param>
    /// <returns>True if a matching folder was found.</returns>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    public override bool IsFolderAvailable(string virtualFolderPath)
    {
      return SecureFunc(FileSystemTask.CheckFolderAvailability,
                        () => OperationService.IsFolderAvailable(virtualFolderPath),
                        () => String.Format("Could not check whether folder [{0}] is available.", virtualFolderPath));
    }

    /// <summary>
    /// Creates a new folder in the file system.
    /// </summary>
    /// <param name="virtualFolderPath">The qualified path of the folder to be created.</param>
    /// <returns>A <see cref="VirtualFileInfo"/> instance which represents
    /// the created folder.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="virtualFolderPath"/>
    /// is a null reference.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the designated parent
    /// folder does not exist.</exception>
    /// <exception cref="ResourceOverwriteException">If the folder already exists on the file
    /// system.</exception>
    public override VirtualFolderInfo CreateFolder(string virtualFolderPath)
    {
      return SecureFunc(FileSystemTask.FolderCreateRequest, () => OperationService.CreateFolder(virtualFolderPath),
                        () => String.Format("Could not create folder [{0}] on file system.", virtualFolderPath));
    }


    /// <summary>
    /// Creates or updates a given file resource in the file system.
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
    public override VirtualFileInfo WriteFile(string virtualFilePath, Stream input, bool overwrite, long resourceLength,
                                              string contentType)
    {
      var fileContract = SecureFunc(FileSystemTask.StreamedFileUploadRequest,
                                    () =>
                                      {
                                        WriteFileDataContract contract = new WriteFileDataContract
                                                                           {
                                                                             Data = input,
                                                                             Overwrite = overwrite,
                                                                             FilePath = virtualFilePath,
                                                                             ResourceLength = resourceLength,
                                                                             ContentType = contentType
                                                                           };
                                        return WriterService.WriteFile(contract);
                                      },
                                    () =>
                                    String.Format("Could not write data for file [{0}] to file system.", virtualFilePath));

      return fileContract.FileInfo;
    }

    /// <summary>
    /// Deletes a given folder from the file system.
    /// </summary>
    /// <param name="virtualFolderPath">The qualified path of the folder to be created.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="virtualFolderPath"/>
    /// is a null reference.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the
    /// folder does not exist.</exception>
    public override void DeleteFolder(string virtualFolderPath)
    {
      SecureAction(FileSystemTask.FolderDeleteRequest,
                   () => OperationService.DeleteFolder(virtualFolderPath),
                   () => String.Format("Could not delete folder [{0}] from file system.", virtualFolderPath));
    }

    /// <summary>
    /// Deletes a given file from the file system.
    /// </summary>
    /// <param name="virtualFilePath">The qualified path of the file to be created.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="virtualFilePath"/>
    /// is a null reference.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the
    /// file does not exist.</exception>
    public override void DeleteFile(string virtualFilePath)
    {
      SecureAction(FileSystemTask.FileDeleteRequest,
                   () => OperationService.DeleteFile(virtualFilePath),
                   () => String.Format("Could not delete file [{0}] from file system.", virtualFilePath));
    }

    /// <summary>
    /// Moves a given folder and all its contents to a new destination.
    /// </summary>
    /// <param name="virtualFolderPath">A qualified name (corresponding to
    /// <see cref="VirtualResourceInfo.FullName"/> that identifies the resource
    /// in the file system.</param>
    /// <param name="destinationPath">The new path of the resource. Can be another name
    /// for the resource itself.</param>
    /// <returns>A <see cref="VirtualFolderInfo"/> object that represents the new
    /// directory in the file system.</returns>
    /// <exception cref="ArgumentNullException">If any of the parameters is a
    /// null reference.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access, or if the operation is not possible (e.g. a resource being
    /// moved/copied to itself).</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the resource that
    /// should be moved does not exist in the file system.</exception>
    /// <exception cref="ResourceOverwriteException">If a resource that matches the
    /// submitted <paramref name="destinationPath"/> already exists.</exception>
    public override VirtualFolderInfo MoveFolder(string virtualFolderPath, string destinationPath)
    {
      return SecureFunc(FileSystemTask.FolderMoveRequest,
                        () => OperationService.MoveFolder(virtualFolderPath, destinationPath),
                        () =>
                        String.Format("Could not move folder [{0}] to destination [{1}].", virtualFolderPath,
                                      destinationPath));
    }

    /// <summary>
    /// Moves a given file to a new destination.
    /// </summary>
    /// <param name="virtualFilePath">A qualified name (corresponding to
    /// <see cref="VirtualResourceInfo.FullName"/> that identifies the resource
    /// in the file system.</param>
    /// <param name="destinationPath">The new path of the resource. Can be another name
    /// for the resource itself.</param>
    /// <returns>A <see cref="VirtualFileInfo"/> object that represents the new
    /// file in the file system.</returns>
    /// <exception cref="ArgumentNullException">If any of the parameters is a
    /// null reference.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access, or if the operation is not possible (e.g. a resource being
    /// moved/copied to itself).</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the resource that
    /// should be moved does not exist in the file system.</exception>
    /// <exception cref="ResourceOverwriteException">If a resource that matches the
    /// submitted <paramref name="destinationPath"/> already exists.</exception>
    public override VirtualFileInfo MoveFile(string virtualFilePath, string destinationPath)
    {
      return SecureFunc(FileSystemTask.FileMoveRequest,
                        () => OperationService.MoveFile(virtualFilePath, destinationPath),
                        () =>
                        String.Format("Could not move file [{0}] to destination [{1}].", virtualFilePath,
                                      destinationPath));
    }

    /// <summary>
    /// Copies a given folder and all its contents to a new destination.
    /// </summary>
    /// <param name="virtualFolderPath">A qualified name (corresponding to
    /// <see cref="VirtualResourceInfo.FullName"/> that identifies the resource
    /// in the file system.</param>
    /// <param name="destinationPath">The new path of the resource. Can be another name
    /// for the resource itself.</param>
    /// <returns>A <see cref="VirtualFolderInfo"/> object that represents the new
    /// directory in the file system.</returns>
    /// <exception cref="ArgumentNullException">If any of the parameters is a
    /// null reference.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access, or if the operation is not possible (e.g. a resource being
    /// moved/copied to itself).</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the resource that
    /// should be moved does not exist in the file system.</exception>
    /// <exception cref="ResourceOverwriteException">If a resource that matches the
    /// submitted <paramref name="destinationPath"/> already exists.</exception>
    public override VirtualFolderInfo CopyFolder(string virtualFolderPath, string destinationPath)
    {
      return SecureFunc(FileSystemTask.FolderCopyRequest,
                        () => OperationService.CopyFolder(virtualFolderPath, destinationPath),
                        () =>
                        String.Format("Could not copy folder [{0}] to destination [{1}].", virtualFolderPath,
                                      destinationPath));
    }

    /// <summary>
    /// Copies a given file to a new destination.
    /// </summary>
    /// <param name="virtualFilePath">A qualified name (corresponding to
    /// <see cref="VirtualResourceInfo.FullName"/> that identifies the resource
    /// in the file system.</param>
    /// <param name="destinationPath">The new path of the resource. Can be another name
    /// for the resource itself.</param>
    /// <returns>A <see cref="VirtualFileInfo"/> object that represents the new
    /// file in the file system.</returns>
    /// <exception cref="ArgumentNullException">If any of the parameters is a
    /// null reference.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access, or if the operation is not possible (e.g. a resource being
    /// moved/copied to itself).</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the resource that
    /// should be moved does not exist in the file system.</exception>
    /// <exception cref="ResourceOverwriteException">If a resource that matches the
    /// submitted <paramref name="destinationPath"/> already exists.</exception>
    public override VirtualFileInfo CopyFile(string virtualFilePath, string destinationPath)
    {
      return SecureFunc(FileSystemTask.FileCopyRequest,
                        () => OperationService.CopyFile(virtualFilePath, destinationPath),
                        () =>
                        String.Format("Could not copy file [{0}] to destination [{1}].", virtualFilePath,
                                      destinationPath));
    }

    /// <summary>
    /// Creates a qualified name that can be used as an identifier
    /// for a given file of the file system.
    /// </summary>
    /// <param name="parentFolder">The qualified name of the parent
    /// folder.</param>
    /// <param name="fileName">The name of a file within the folder.</param>
    /// <returns>An qualified path name for the submitted
    /// <paramref name="fileName"/>.</returns>
    public override string CreateFilePath(string parentFolder, string fileName)
    {
      return SecureFunc(FileSystemTask.CreateFilePathRequest,
                        () => OperationService.CreateFilePath(parentFolder, fileName),
                        () =>
                        String.Format("Could not get a file path for parent folder [{0}] and file name [{1}].",
                                      parentFolder, fileName));
    }

    /// <summary>
    /// Creates a qualified name that can be used as an identifier
    /// for a given folder of the file system.
    /// </summary>
    /// <param name="parentFolder">The qualified name of the parent
    /// folder.</param>
    /// <param name="folderName">The name of the child folder.</param>
    /// <returns>An qualified path name for the submitted
    /// <paramref name="folderName"/>.</returns>
    public override string CreateFolderPath(string parentFolder, string folderName)
    {
      return SecureFunc(FileSystemTask.CreateFolderPathRequest,
                        () => OperationService.CreateFolderPath(parentFolder, folderName),
                        () =>
                        String.Format("Could not get a folder path for parent folder [{0}] and folder name [{1}].",
                                      parentFolder, folderName));
    }


    protected override T SecureFunc<T>(FileSystemTask task, Func<T> func, Func<string> errorMessage)
    {
      return Util.SecureFunc(Auditor, task, func, errorMessage);
    }
  }
}