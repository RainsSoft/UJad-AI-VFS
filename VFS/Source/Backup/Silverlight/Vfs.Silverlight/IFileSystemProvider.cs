using System;
using System.Collections.Generic;
using System.IO;
using Vfs.Auditing;
using Vfs.Locking;
using Vfs.Security;
using Vfs.Transfer;
using Vfs.Util;


namespace Vfs
{
  /// <summary>
  /// Provides controller logic to interact with the
  /// managed file system.
  /// </summary>
  public interface IFileSystemProvider : IDisposable
  {
    /// <summary>
    /// Adds an auditor to file system provider, which receives
    /// auditing messages for file system requests and incidents.<br/>
    /// Assigning a null reference should not set the property to null,
    /// but fall back to a <see cref="NullAuditor"/> instead so
    /// this property never returns null but a
    /// valid <see cref="IAuditor"/> instance.
    /// </summary>
    IAuditor Auditor { get; set; }

    /// <summary>
    /// Tracks whether <see cref="IDisposable.Dispose"/> has been called or not.
    /// </summary>
    bool IsDisposed { get; }

    /// <summary>
    /// Gets a lock repository, which maintains the provider's read and write
    /// locks. Per default, this provider operates with a <see cref="ResourceLockRepository"/>
    /// class.<br/>
    /// Assigning a null reference does not set the property to null,
    /// but falls back to a <see cref="NullResourceLockRepository"/> instead so
    /// this property never returns null but a valid <see cref="IResourceLockRepository"/>
    /// instance.
    /// </summary>
    IResourceLockRepository LockRepository { get; set; }

    /// <summary>
    /// Adds a custom authentication and authorization facility to the
    /// file system provider.<br/>
    /// Assigning a null reference should not set the property to null,
    /// but fall back to a <see cref="IFileSystemSecurity"/> instead so
    /// this property never returns null but a
    /// valid <see cref="IAuditor"/> instance.
    /// </summary>
    IFileSystemSecurity Security { get; set; }

    /// <summary>
    /// Manages download requests from the file system.
    /// </summary>
    IDownloadTransferHandler DownloadTransfers { get; }


    /// <summary>
    /// Manages uploads to the file system.
    /// </summary>
    IUploadTransferHandler UploadTransfers { get; }


    /// <summary>
    /// Gets the root of the file system. This is a dummy folder, which
    /// represents the file system as a whole, and provides the top level contents
    /// of the underlying file system as files and folders.
    /// </summary>
    VirtualFolderInfo GetFileSystemRoot();


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
    Stream ReadFileContents(string virtualFilePath);


    /// <summary>
    /// Gets the binary contents as a stream in a blocking operation.
    /// Use the methods in <see cref="ContentUtil"/> class for simplified stream
    /// handling.
    /// </summary>
    /// <param name="fileInfo">Provides meta information about the file to be read.</param>
    /// <returns>A stream that allows the contents of the file to be read.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="fileInfo"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the file that is represented
    /// by <paramref name="fileInfo"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    Stream ReadFileContents(VirtualFileInfo fileInfo);


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
    VirtualFileInfo GetFileInfo(string virtualFilePath);


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
    VirtualFolderInfo GetFolderInfo(string virtualFolderPath);


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
    VirtualFolderInfo GetFileParent(string childFilePath);


    /// <summary>
    /// Gets the parent folder of a given file system resource.
    /// </summary>
    /// <param name="child">An arbitrary file resource of the file system.</param>
    /// <returns>The parent of the file.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="child"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the file that is represented
    /// by <paramref name="child"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of an invalid or prohibited
    /// resource access.</exception>
    VirtualFolderInfo GetFileParent(VirtualFileInfo child);


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
    VirtualFolderInfo GetFolderParent(string childFolderPath);


    /// <summary>
    /// Gets the parent folder of a given file system resource.
    /// </summary>
    /// <param name="child">An arbitrary folder resource of the file system.</param>
    /// <returns>The parent of the folder.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="child"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the folder that is represented
    /// by <paramref name="child"/> does not exist in the file system.</exception>
    VirtualFolderInfo GetFolderParent(VirtualFolderInfo child);


    /// <summary>
    /// Gets all child folders of a given folder.
    /// </summary>
    /// <param name="parentFolderPath">The <see cref="VirtualResourceInfo.FullName"/>, which
    /// identifies the parent folder within the file system.</param>
    /// <returns>The child folders of the folder.</returns>
    /// <exception cref="VirtualResourceNotFoundException">If the folder that is represented
    /// by <paramref name="parentFolderPath"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    IEnumerable<VirtualFolderInfo> GetChildFolders(string parentFolderPath);


    /// <summary>
    /// Gets all child folders of a given <paramref name="parent"/> folder.
    /// </summary>
    /// <param name="parent">The processed parent folder.</param>
    /// <returns>The child folders of the folder.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="parent"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the folder that is represented
    /// by <paramref name="parent"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    IEnumerable<VirtualFolderInfo> GetChildFolders(VirtualFolderInfo parent);


    /// <summary>
    /// Gets all child folders of a given folder that match
    /// the specified <paramref name="searchPattern"/>.
    /// </summary>
    /// <param name="parentFolderPath">The <see cref="VirtualResourceInfo.FullName"/>, which
    /// identifies the parent folder within the file system.</param>
    /// <param name="searchPattern">A search string which is used to limit the returned
    /// results to folders with matching names.</param>
    /// <returns>The child folders of the folder.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="searchPattern"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the folder that is represented
    /// by <paramref name="parentFolderPath"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    IEnumerable<VirtualFolderInfo> GetChildFolders(string parentFolderPath, string searchPattern);


    /// <summary>
    /// Gets all child folders of a given <paramref name="parent"/> folder that match
    /// the specified <paramref name="searchPattern"/>.
    /// </summary>
    /// <param name="parent">The processed parent folder.</param>
    /// <param name="searchPattern">A search string which is used to limit the returned
    /// results to folders with matching names.</param>
    /// <returns>The child folders of the folder.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="parent"/>
    /// is a null reference.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="searchPattern"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the folder that is represented
    /// by <paramref name="parent"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    IEnumerable<VirtualFolderInfo> GetChildFolders(VirtualFolderInfo parent, string searchPattern);


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
    IEnumerable<VirtualFileInfo> GetChildFiles(string parentFolderPath);


    /// <summary>
    /// Gets all files of a given <paramref name="parent"/> folder.
    /// </summary>
    /// <param name="parent">The processed parent folder.</param>
    /// <returns>The files in the submitted <paramref name="parent"/> folder.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="parent"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the folder that is represented
    /// by <paramref name="parent"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    IEnumerable<VirtualFileInfo> GetChildFiles(VirtualFolderInfo parent);

    /// <summary>
    /// Gets all child files of a given folder that match
    /// the specified <paramref name="searchPattern"/>.
    /// </summary>
    /// <param name="parentFolderPath">The <see cref="VirtualResourceInfo.FullName"/>, which
    /// identifies the parent folder within the file system.</param>
    /// <param name="searchPattern">A search string which is used to limit the returned
    /// results to folders with matching names.</param>
    /// <returns>The files of the submitted folder.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="searchPattern"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the folder that is represented
    /// by <paramref name="parentFolderPath"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    IEnumerable<VirtualFileInfo> GetChildFiles(string parentFolderPath, string searchPattern);


    /// <summary>
    /// Gets all files of a given <paramref name="parent"/> folder that match
    /// the specified <paramref name="searchPattern"/>.
    /// </summary>
    /// <param name="parent">The processed parent folder.</param>
    /// <param name="searchPattern">A search string which is used to limit the returned
    /// results to folders with matching names.</param>
    /// <returns>The files in the submitted <paramref name="parent"/> folder.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="parent"/>
    /// is a null reference.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="searchPattern"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the folder that is represented
    /// by <paramref name="parent"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    IEnumerable<VirtualFileInfo> GetChildFiles(VirtualFolderInfo parent, string searchPattern);


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
    FolderContentsInfo GetFolderContents(string parentFolderPath);


    /// <summary>
    /// Gets all files and folders of a given <paramref name="parent"/> folder.
    /// </summary>
    /// <param name="parent">The processed parent folder.</param>
    /// <returns>The files and folders of the submitted parent.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="parent"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the folder that is represented
    /// by <paramref name="parent"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    FolderContentsInfo GetFolderContents(VirtualFolderInfo parent);

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
    FolderContentsInfo GetFolderContents(string parentFolderPath, string searchPattern);


    /// <summary>
    /// Gets all files and folders of a given <paramref name="parent"/> folder that match
    /// the specified <paramref name="searchPattern"/>.
    /// </summary>
    /// <param name="parent">The processed parent folder.</param>
    /// <param name="searchPattern">A search string which is used to limit the returned
    /// results to folders with matching names.</param>
    /// <returns>The files and folders of the submitted parent.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="parent"/>
    /// is a null reference.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="searchPattern"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the folder that is represented
    /// by <paramref name="parent"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    FolderContentsInfo GetFolderContents(VirtualFolderInfo parent, string searchPattern);


    /// <summary>
    /// Checks whether a file resource at a given path exists or not.
    /// </summary>
    /// <param name="virtualFilePath">A path to the requested file.</param>
    /// <returns>True if a matching file was found.</returns>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    bool IsFileAvailable(string virtualFilePath);


    /// <summary>
    /// Checks whether a folder resource at a given path exists or not.
    /// </summary>
    /// <param name="virtualFolderPath">A path to the requested folder.</param>
    /// <returns>True if a matching folder was found.</returns>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    bool IsFolderAvailable(string virtualFolderPath);


    /// <summary>
    /// Creates a new folder in the file system.
    /// </summary>
    /// <param name="parentFolderPath">The qualified name of the designated parent folder, which
    /// needs to exists, and provide write access.</param>
    /// <param name="folderName">The name of the folder to be created.</param>
    /// <returns>A <see cref="VirtualFileInfo"/> instance which represents
    /// the created folder.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="folderName"/>
    /// is a null reference.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If no folder exists that
    /// matches the submitted <paramref name="parentFolderPath"/>.</exception>
    /// <exception cref="ResourceOverwriteException">If the folder already exists on the file
    /// system.</exception>
    VirtualFolderInfo CreateFolder(string parentFolderPath, string folderName);


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
    VirtualFolderInfo CreateFolder(string virtualFolderPath);


    /// <summary>
    /// Creates a new folder in the file system.
    /// </summary>
    /// <param name="parent">The designated parent folder, which
    /// needs to exists, and provide write access.</param>
    /// <param name="folderName">The name of the folder to be created.</param>
    /// <returns>A <see cref="VirtualFileInfo"/> instance which represents
    /// the created folder.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="parent"/>
    /// is a null reference.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="folderName"/>
    /// is a null reference.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the <paramref name="parent"/>
    /// folder does not exist.</exception>
    /// <exception cref="ResourceOverwriteException">If the folder already exists on the file
    /// system.</exception>
    VirtualFolderInfo CreateFolder(VirtualFolderInfo parent, string folderName);


    /// <summary>
    /// Creates or updates a given file resource in the file system.
    /// </summary>
    /// <param name="parentFolderPath">The qualified path of the parent folder that will
    /// contain the file.</param>
    /// <param name="fileName">The name of the file to be created.</param>
    /// <param name="input">A stream that provides the file's contents.</param>
    /// <param name="overwrite">Whether an existing file should be overwritten
    /// or not. If this parameter is false and the file already exists, a
    /// <see cref="ResourceOverwriteException"/> is thrown.</param>
    /// <param name="resourceLength">The length of the resource to be uploaded in bytes.</param>
    /// <param name="contentType">The content type of the uploaded resource.</param>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the parent folder
    /// does not exist.</exception>
    /// <exception cref="ResourceOverwriteException">If a file already exists at the
    /// specified location, and the <paramref name="overwrite"/> flag was not set.</exception>
    /// <exception cref="ArgumentNullException">If any of the parameters is a null reference.</exception>
    VirtualFileInfo WriteFile(string parentFolderPath, string fileName, Stream input, bool overwrite, long resourceLength, string contentType);

    
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
    VirtualFileInfo WriteFile(string virtualFilePath, Stream input, bool overwrite, long resourceLength, string contentType);


    /// <summary>
    /// Creates or updates a given file resource in the file system.
    /// </summary>
    /// <param name="parent">The designated parent folder, which
    /// needs to exists, and provide write access.</param>
    /// <param name="fileName">The name of the file to be created.</param>
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
    VirtualFileInfo WriteFile(VirtualFolderInfo parent, string fileName, Stream input, bool overwrite, long resourceLength, string contentType);


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
    void DeleteFolder(string virtualFolderPath);


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
    void DeleteFile(string virtualFilePath);

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
    VirtualFolderInfo MoveFolder(string virtualFolderPath, string destinationPath);

    /// <summary>
    /// Moves a given folder and all its contents to a new destination.
    /// </summary>
    /// <param name="folder">Represents the resource in the file system.</param>
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
    VirtualFolderInfo MoveFolder(VirtualFolderInfo folder, string destinationPath);

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
    VirtualFileInfo MoveFile(string virtualFilePath, string destinationPath);

    /// <summary>
    /// Moves a given file to a new destination.
    /// </summary>
    /// <param name="file">Represents the resource in the file system.</param>
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
    VirtualFileInfo MoveFile(VirtualFileInfo file, string destinationPath);

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
    VirtualFolderInfo CopyFolder(string virtualFolderPath, string destinationPath);

    /// <summary>
    /// Copies a given folder and all its contents to a new destination.
    /// </summary>
    /// <param name="folder">Represents the resource in the file system.</param>
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
    VirtualFolderInfo CopyFolder(VirtualFolderInfo folder, string destinationPath);

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
    VirtualFileInfo CopyFile(string virtualFilePath, string destinationPath);

    /// <summary>
    /// Copies a given file to a new destination.
    /// </summary>
    /// <param name="file">Represents the resource in the file system.</param>
    /// <param name="destinationPath">The new path of the resource. Can be another name
    /// for the resource itself.</param>
    /// <returns>A <see cref="VirtualFileInfo"/> object that represents the new
    /// file in the file system.</returns>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access, or if the operation is not possible (e.g. a resource being
    /// moved/copied to itself).</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the resource that
    /// should be moved does not exist in the file system.</exception>
    /// <exception cref="ResourceOverwriteException">If a resource that matches the
    /// submitted <paramref name="destinationPath"/> already exists.</exception>
    VirtualFileInfo CopyFile(VirtualFileInfo file, string destinationPath);


    /// <summary>
    /// Creates a qualified name that can be used as an identifier
    /// for a given file of the file system.
    /// </summary>
    /// <param name="parentFolder">The qualified name of the parent
    /// folder.</param>
    /// <param name="fileName">The name of a file within the folder.</param>
    /// <returns>An qualified path name for the submitted
    /// <paramref name="fileName"/>.</returns>
    string CreateFilePath(string parentFolder, string fileName);


    /// <summary>
    /// Creates a qualified name that can be used as an identifier
    /// for a given folder of the file system.
    /// </summary>
    /// <param name="parentFolder">The qualified name of the parent
    /// folder.</param>
    /// <param name="folderName">The name of the child folder.</param>
    /// <returns>An qualified path name for the submitted
    /// <paramref name="folderName"/>.</returns>
    string CreateFolderPath(string parentFolder, string folderName);
  }

}