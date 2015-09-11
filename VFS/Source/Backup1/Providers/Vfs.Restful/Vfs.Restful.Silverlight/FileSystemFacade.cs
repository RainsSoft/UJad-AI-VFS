using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Http;
using Vfs.Auditing;
using Vfs.Transfer;
using Vfs.Util;

namespace Vfs.Restful.Client
{
  /// <summary>
  /// Provides a local façade to a WCF file system service.
  /// </summary>
  public class FileSystemFacade : FileSystemProviderBase
  {
    /// <summary>
    /// The base URI of the service that is being accessed to submit
    /// file system requests.
    /// </summary>
    public string ServiceBaseUri { get; private set; }

    /// <summary>
    /// Gets the service URIs.
    /// </summary>
    public VfsUris Uris
    {
      get { return VfsUris.Default; }
    }


    private readonly DownloadTransferFacade downloadFacade;

    /// <summary>
    /// Manages download requests from the file system.
    /// </summary>
    public override IDownloadTransferHandler DownloadTransfers
    {
      get { return downloadFacade; }
    }


    private readonly UploadTransferFacade uploadFacade;

    /// <summary>
    /// Manages uploads to the file system.
    /// </summary>
    public override IUploadTransferHandler UploadTransfers
    {
      get { return uploadFacade; }
    }


    /// <summary>
    /// Inits an empty facade.
    /// </summary>
    public FileSystemFacade(string serviceBaseUri)
    {
      ServiceBaseUri = serviceBaseUri;
      downloadFacade = new DownloadTransferFacade(serviceBaseUri);
      uploadFacade = new UploadTransferFacade(serviceBaseUri);
    }


    /// <summary>
    /// Gets the root of the file system. This is a dummy folder, which
    /// represents the file system as a whole, and provides the top level contents
    /// of the underlying file system as files and folders.
    /// </summary>
    public override VirtualFolderInfo GetFileSystemRoot()
    {
      string actionUri = Uris.GetFileSystemRootUri;
      Func<string> errorMessage = () => "An error occurred while retrieving the file system root";
      return SecureGet<VirtualFolderInfo>(FileSystemTask.RootFolderInfoRequest, actionUri, errorMessage); 
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
      return DownloadTransfers.ReadFile(virtualFilePath);
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
      string actionUri = Uris.GetFileInfoUri;
      actionUri = actionUri.ConstructUri(Uris.PatternFilePath, virtualFilePath);

      Func<string> errorMessage = () => String.Format("Could not get meta data for file [{0}].", virtualFilePath);
      return SecureGet<VirtualFileInfo>(FileSystemTask.FileInfoRequest, actionUri, errorMessage); 
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
      string actionUri = Uris.GetFolderInfoUri;
      actionUri = actionUri.ConstructUri(Uris.PatternFolderPath, virtualFolderPath);

      Func<string> errorMessage = () => String.Format("Could not get meta data for folder [{0}].", virtualFolderPath);
      return SecureGet<VirtualFolderInfo>(FileSystemTask.FolderInfoRequest, actionUri, errorMessage); 
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
      Ensure.ArgumentNotNull(childFilePath, "childFilePath");

      string actionUri = Uris.GetFileParentUri;
      actionUri = actionUri.ConstructUri(Uris.PatternFilePath, childFilePath);

      Func<string> errorMessage = () => String.Format("Could not get meta data about parent folder of file [{0}].", childFilePath);
      return SecureGet<VirtualFolderInfo>(FileSystemTask.FileParentRequest, actionUri, errorMessage); 
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
      Ensure.ArgumentNotNull(childFolderPath, "parentFolderPath");

      string actionUri = Uris.GetFolderParentUri;
      actionUri = actionUri.ConstructUri(Uris.PatternFolderPath, childFolderPath);

      Func<string> errorMessage = () => String.Format("Could not get meta data about parent folder of folder [{0}].", childFolderPath);
      return SecureGet<VirtualFolderInfo>(FileSystemTask.FolderParentRequest, actionUri, errorMessage); 
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
      Ensure.ArgumentNotNull(parentFolderPath, "parentFolderPath");

      string actionUri = Uris.GetChildFoldersUri;
      actionUri = actionUri.ConstructUri(Uris.PatternParentFolderPath, parentFolderPath);

      Func<string> errorMessage = () => String.Format("Could not get child folders of folder [{0}].", parentFolderPath);
      return SecureGet<IEnumerable<VirtualFolderInfo>>(FileSystemTask.ChildFoldersRequest, actionUri, errorMessage); 
    }


    /// <summary>
    /// Gets all child folders of a given folder that match
    /// the specified <paramref name="searchPattern"/>.
    /// </summary>
    /// <param name="parentFolderPath">The <see cref="VirtualResourceInfo.FullName"/>, which
    /// identifies the parent folder within the file system.</param>
    /// <param name="searchPattern">A search string which is used to limit the returned
    /// results to folders with matching names.</param>
    /// <returns>The child folders of the folder.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="parentFolderPath"/>
    /// is a null reference.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="searchPattern"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the folder that is represented
    /// by <paramref name="parentFolderPath"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    public override IEnumerable<VirtualFolderInfo> GetChildFolders(string parentFolderPath, string searchPattern)
    {
      Ensure.ArgumentNotNull(parentFolderPath, "parentFolderPath");
      Ensure.ArgumentNotNull(searchPattern, "searchPattern");

      string actionUri = Uris.GetChildFoldersFilteredUri;
      actionUri = actionUri.ConstructUri(Uris.PatternParentFolderPath, parentFolderPath);
      actionUri = actionUri.ConstructUri(Uris.PatternFilter, searchPattern);

      Func<string> errorMessage = () => String.Format("Could not get child folders of folder [{0}].", parentFolderPath);
      return SecureGet<IEnumerable<VirtualFolderInfo>>(FileSystemTask.ChildFoldersRequest, actionUri, errorMessage); 
    }


    /// <summary>
    /// Gets all child files of a given folder.
    /// </summary>
    /// <param name="parentFolderPath">The <see cref="VirtualResourceInfo.FullName"/>, which
    /// identifies the parent folder within the file system.</param>
    /// <returns>The files of the submitted folder.</returns>
    /// <exception cref="VirtualResourceNotFoundException">If the folder that is represented
    /// by <paramref name="parentFolderPath"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    public override IEnumerable<VirtualFileInfo> GetChildFiles(string parentFolderPath)
    {
      string actionUri = Uris.GetChildFilesUri;
      actionUri = actionUri.ConstructUri(Uris.PatternParentFolderPath, parentFolderPath);
      
      Func<string> errorMessage = () => String.Format("Could not get child files of folder [{0}].", parentFolderPath);
      return SecureGet<IEnumerable<VirtualFileInfo>>(FileSystemTask.ChildFilesRequest, actionUri, errorMessage); 
    }


    /// <summary>
    /// Gets all child files of a given folder that match
    /// the specified <paramref name="searchPattern"/>.
    /// </summary>
    /// <param name="parentFolderPath">The <see cref="VirtualResourceInfo.FullName"/>, which
    /// identifies the parent folder within the file system.</param>
    /// <param name="searchPattern">A search string which is used to limit the returned
    /// results to folders with matching names.</param>
    /// <returns>The files of the submitted folder.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="parentFolderPath"/>
    /// is a null reference.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="searchPattern"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the folder that is represented
    /// by <paramref name="parentFolderPath"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    public override IEnumerable<VirtualFileInfo> GetChildFiles(string parentFolderPath, string searchPattern)
    {
      Ensure.ArgumentNotNull(searchPattern, "searchPattern");

      string actionUri = Uris.GetChildFilesFilteredUri;
      actionUri = actionUri.ConstructUri(Uris.PatternParentFolderPath, parentFolderPath);
      actionUri = actionUri.ConstructUri(Uris.PatternFilter, searchPattern);

      Func<string> errorMessage = () => String.Format("Could not get child files of folder [{0}].", parentFolderPath);
      return SecureGet<IEnumerable<VirtualFileInfo>>(FileSystemTask.ChildFoldersRequest, actionUri, errorMessage); 
    }

    /// <summary>
    /// Gets all files and folders of a given folder.
    /// </summary>
    /// <param name="parentFolderPath">The <see cref="VirtualResourceInfo.FullName"/>, which
    /// identifies the parent folder within the file system.</param>
    /// <returns>The files and folders of the submitted parent.</returns>
    /// <exception cref="VirtualResourceNotFoundException">If the folder that is represented
    /// by <paramref name="parentFolderPath"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    public override FolderContentsInfo GetFolderContents(string parentFolderPath)
    {
      string actionUri = Uris.GetFolderContentsUri;
      actionUri = actionUri.ConstructUri(Uris.PatternParentFolderPath, parentFolderPath);

      Func<string> errorMessage = () => String.Format("Could not get contents of folder [{0}].", parentFolderPath);
      return SecureGet<FolderContentsInfo>(FileSystemTask.FolderContentsRequest, actionUri, errorMessage); 
    }

    /// <summary>
    /// Gets all files and folders of a given folder that match
    /// the specified <paramref name="searchPattern"/>.
    /// </summary>
    /// <param name="parentFolderPath">The <see cref="VirtualResourceInfo.FullName"/>, which
    /// identifies the parent folder within the file system.</param>
    /// <param name="searchPattern">A search string which is used to limit the returned
    /// results to folders with matching names.</param>
    /// <returns>The files and folders of the submitted parent.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="searchPattern"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the folder that is represented
    /// by <paramref name="parentFolderPath"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    public override FolderContentsInfo GetFolderContents(string parentFolderPath, string searchPattern)
    {
      Ensure.ArgumentNotNull(searchPattern, "searchPattern");

      string actionUri = Uris.GetFolderContentsFilteredUri;
      actionUri = actionUri.ConstructUri(Uris.PatternParentFolderPath, parentFolderPath);
      actionUri = actionUri.ConstructUri(Uris.PatternFilter, searchPattern);

      Func<string> errorMessage = () => String.Format("Could not get contents of folder [{0}].", parentFolderPath);
      return SecureGet<FolderContentsInfo>(FileSystemTask.FolderContentsRequest, actionUri, errorMessage); 
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
      string actionUri = Uris.IsFileAvailableUri;
      actionUri = actionUri.ConstructUri(Uris.PatternFilePath, virtualFilePath);

      Func<string> errorMessage = () => String.Format("Could not check whether file [{0}] is available.", virtualFilePath);
      const FileSystemTask context = FileSystemTask.CheckFileAvailability;
      return SecureGet<Wrapped<bool>>(context, actionUri, errorMessage).Value; 
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
      string actionUri = Uris.IsFolderAvailableUri;
      actionUri = actionUri.ConstructUri(Uris.PatternFolderPath, virtualFolderPath);

      Func<string> errorMessage = () => String.Format("Could not check whether folder [{0}] is available.", virtualFolderPath);
      const FileSystemTask context = FileSystemTask.CheckFolderAvailability;
      
      return SecureGet<Wrapped<bool>>(context, actionUri, errorMessage).Value; 
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
      string actionUri = Uris.CreateFolderUri;
      actionUri = actionUri.ConstructUri(Uris.PatternFolderPath, virtualFolderPath);


      Func<string> errorMessage = () => String.Format("Could not create folder [{0}] on file system.", virtualFolderPath);
      const FileSystemTask context = FileSystemTask.FolderCreateRequest;

      HttpContent content = HttpContent.CreateEmpty();
      return SecurePost<VirtualFolderInfo>(context, actionUri, content, errorMessage);
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
      //set headers accordingly
      UploadTransfers.WriteFile(virtualFilePath, input, overwrite, resourceLength, contentType);
      return GetFileInfo(virtualFilePath);
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
      string actionUri = Uris.DeleteFolderUri;
      actionUri = actionUri.ConstructUri(Uris.PatternFolderPath, virtualFolderPath);

      Func<string> errorMessage = () => String.Format("Could not delete folder [{0}] from file system.", virtualFolderPath);
      const FileSystemTask context = FileSystemTask.FolderDeleteRequest;

      SecureDelete(context, actionUri, errorMessage);
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
      string actionUri = Uris.DeleteFileUri;
      actionUri = actionUri.ConstructUri(Uris.PatternFilePath, virtualFilePath);

      Func<string> errorMessage = () => String.Format("Could not delete file [{0}] from file system.", virtualFilePath);
      const FileSystemTask context = FileSystemTask.FileDeleteRequest;

      SecureDelete(context, actionUri, errorMessage);
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
      string actionUri = Uris.MoveFolderUri;
      actionUri = actionUri.ConstructUri(Uris.PatternFolderPath, virtualFolderPath);
      actionUri = actionUri.ConstructUri(Uris.PatternDestinationPath, destinationPath);

      
      Func<string> errorMessage = () => String.Format("Could not move folder [{0}] to destination [{1}].", virtualFolderPath,
                                      destinationPath);
      const FileSystemTask context = FileSystemTask.FolderMoveRequest;

      return SecurePost<VirtualFolderInfo>(context, actionUri, HttpContent.CreateEmpty(), errorMessage);
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
      string actionUri = Uris.MoveFileUri;
      actionUri = actionUri.ConstructUri(Uris.PatternFilePath, virtualFilePath);
      actionUri = actionUri.ConstructUri(Uris.PatternDestinationPath, destinationPath);


      Func<string> errorMessage = () => String.Format("Could not move file [{0}] to destination [{1}].", virtualFilePath,
                                      destinationPath);
      const FileSystemTask context = FileSystemTask.FileMoveRequest;

      return SecurePost<VirtualFileInfo>(context, actionUri, HttpContent.CreateEmpty(), errorMessage);
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
      string actionUri = Uris.CopyFolderUri;
      actionUri = actionUri.ConstructUri(Uris.PatternFolderPath, virtualFolderPath);
      actionUri = actionUri.ConstructUri(Uris.PatternDestinationPath, destinationPath);

      Func<string> errorMessage = () => String.Format("Could not copy folder [{0}] to destination [{1}].", virtualFolderPath,
                                      destinationPath);
      const FileSystemTask context = FileSystemTask.FolderCopyRequest;

      return SecurePost<VirtualFolderInfo>(context, actionUri, HttpContent.CreateEmpty(), errorMessage);
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
      string actionUri = Uris.CopyFileUri;
      actionUri = actionUri.ConstructUri(Uris.PatternFilePath, virtualFilePath);
      actionUri = actionUri.ConstructUri(Uris.PatternDestinationPath, destinationPath);
      
      Func<string> errorMessage = () => String.Format("Could not copy file [{0}] to destination [{1}].", virtualFilePath,
                                      destinationPath);
      const FileSystemTask context = FileSystemTask.FileCopyRequest;

      return SecurePost<VirtualFileInfo>(context, actionUri, HttpContent.CreateEmpty(), errorMessage);
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
      string actionUri = Uris.CreateFilePathUri;
      actionUri = actionUri.ConstructUri(Uris.PatternParentFolderPath, parentFolder);
      actionUri = actionUri.ConstructUri(Uris.PatternFileName, fileName);


      Func<string> errorMessage = () => String.Format("Could not get a file path for parent folder [{0}] and file name [{1}].",
                                      parentFolder, fileName);
      const FileSystemTask context = FileSystemTask.CreateFilePathRequest;

      return SecureGet<Wrapped<string>>(context, actionUri, errorMessage).Value;
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
      string actionUri = Uris.CreateFolderPathUri;
      actionUri = actionUri.ConstructUri(Uris.PatternParentFolderPath, parentFolder);
      actionUri = actionUri.ConstructUri(Uris.PatternFolderName, folderName);


      Func<string> errorMessage = () => String.Format("Could not get a folder path for parent folder [{0}] and folder name [{1}].",
                                      parentFolder, folderName);
      const FileSystemTask context = FileSystemTask.CreateFolderPathRequest;
      return SecureGet<Wrapped<string>>(context, actionUri, errorMessage).Value;
    }



    protected override T SecureFunc<T>(FileSystemTask task, Func<T> func, Func<string> errorMessage)
    {
      return Util.SecureFunc(Auditor, task, func, errorMessage);
    }


    protected override void SecureAction(FileSystemTask task, Action action, Func<string> errorMessage)
    {
      Util.SecureAction(Auditor, task, action, errorMessage);
    }


    protected T SecureGet<T>(FileSystemTask context, string actionUri, Func<string> errorMessage)
    {
      return Util.SecureFunc(Auditor, context,
                             () => Util.Get<T>(ServiceBaseUri, actionUri),
                             errorMessage);
    }

    protected T SecurePost<T>(FileSystemTask context, string actionUri, HttpContent content, Func<string> errorMessage)
    {
      return Util.SecureFunc(Auditor, context,
                             () => Util.Post<T>(ServiceBaseUri, actionUri, content),
                             errorMessage);
    }

    protected void SecureDelete(FileSystemTask context, string actionUri, Func<string> errorMessage)
    {
      Util.SecureAction(Auditor, context,
                             () => Util.Delete(ServiceBaseUri, actionUri),
                             errorMessage);
    }
  }
}