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
  /// Simple base class which eliminates the need to implement
  /// method overrides.
  /// </summary>
  public abstract class FileSystemProviderBase : IFileSystemProvider
  {
    private IAuditor auditor = new NullAuditor();

    /// <summary>
    /// Adds an auditor to file system provider, which receives
    /// auditing messages for file system requests and incidents.<br/>
    /// Assigning a null reference does not set the property to null,
    /// but falls back to a <see cref="NullAuditor"/> instead so
    /// this property never returns null but a
    /// valid <see cref="IAuditor"/> instance.
    /// </summary>
    public virtual IAuditor Auditor
    {
      get { return auditor; }
      set
      {
        auditor = value ?? new NullAuditor();
        if (DownloadTransfers != null)
        {
          DownloadTransfers.Auditor = auditor;
        }
        
        if(UploadTransfers != null)
        {
          UploadTransfers.Auditor = auditor;
        }
      }
    }

    private IResourceLockRepository lockRepository = new ResourceLockRepository();

    /// <summary>
    /// Gets a lock repository, which maintains the provider's read and write
    /// locks. Per default, this provider operates with a <see cref="ResourceLockRepository"/>
    /// class.<br/>
    /// Assigning a null reference does not set the property to null,
    /// but falls back to a <see cref="NullResourceLockRepository"/> instead so
    /// this property never returns null but a valid <see cref="IResourceLockRepository"/>
    /// instance.
    /// </summary>
    public virtual IResourceLockRepository LockRepository
    {
      get { return lockRepository; }
      set { lockRepository = value ?? new NullResourceLockRepository(); }
    }

    private IFileSystemSecurity security = new NullSecurity();


    /// <summary>
    /// Adds a custom authentication and authorization facility to the
    /// file system provider.<br/>
    /// Assigning a null reference does not set the property to null,
    /// but falls back to a <see cref="IFileSystemSecurity"/> instead so
    /// this property never returns null but a
    /// valid <see cref="IAuditor"/> instance.
    /// </summary>
    public virtual IFileSystemSecurity Security
    {
      get { return security; }
      set { security = value ?? new NullSecurity(); }
    }


    /// <summary>
    /// Tracks whether <see cref="Dispose"/> has been called or not.
    /// </summary>
    public bool IsDisposed { get; protected set; }

    /// <summary>
    /// Manages download requests from the file system.
    /// </summary>
    public abstract IDownloadTransferHandler DownloadTransfers { get; }


    /// <summary>
    /// Manages uploads to the file system.
    /// </summary>
    public abstract IUploadTransferHandler UploadTransfers { get; }


    /// <summary>
    /// Gets the root of the file system. This is a dummy folder, which
    /// represents the file system as a whole, and provides the top level contents
    /// of the underlying file system as files and folders.
    /// </summary>
    public abstract VirtualFolderInfo GetFileSystemRoot();


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
    public abstract Stream ReadFileContents(string virtualFilePath);


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
    public virtual Stream ReadFileContents(VirtualFileInfo fileInfo)
    {
      Ensure.ArgumentNotNull(fileInfo, "fileInfo");
      return ReadFileContents(fileInfo.FullName);
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
    public abstract VirtualFileInfo GetFileInfo(string virtualFilePath);

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
    public abstract VirtualFolderInfo GetFolderInfo(string virtualFolderPath);

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
    public abstract VirtualFolderInfo GetFileParent(string childFilePath);

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
    public virtual VirtualFolderInfo GetFileParent(VirtualFileInfo child)
    {
      Ensure.ArgumentNotNull(child, "child");
      return GetFileParent(child.FullName);
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
    public abstract VirtualFolderInfo GetFolderParent(string childFolderPath);

    /// <summary>
    /// Gets the parent folder of a given file system resource.
    /// </summary>
    /// <param name="child">An arbitrary folder resource of the file system.</param>
    /// <returns>The parent of the folder.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="child"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the folder that is represented
    /// by <paramref name="child"/> does not exist in the file system.</exception>
    public virtual VirtualFolderInfo GetFolderParent(VirtualFolderInfo child)
    {
      Ensure.ArgumentNotNull(child, "child");
      return GetFolderParent(child.FullName);
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
    public abstract IEnumerable<VirtualFolderInfo> GetChildFolders(string parentFolderPath);

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
    public virtual IEnumerable<VirtualFolderInfo> GetChildFolders(VirtualFolderInfo parent)
    {
      Ensure.ArgumentNotNull(parent, "parent");
      return GetChildFolders(parent.FullName);
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
    public virtual IEnumerable<VirtualFolderInfo> GetChildFolders(string parentFolderPath, string searchPattern)
    {
      Ensure.ArgumentNotNull(searchPattern, "searchPattern");

      var folders = GetChildFolders(parentFolderPath);
      return folders.Filter(searchPattern);
    }

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
    public virtual IEnumerable<VirtualFolderInfo> GetChildFolders(VirtualFolderInfo parent, string searchPattern)
    {
      Ensure.ArgumentNotNull(parent, "parent");
      return GetChildFolders(parent.FullName, searchPattern);
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
    public abstract IEnumerable<VirtualFileInfo> GetChildFiles(string parentFolderPath);

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
    public virtual IEnumerable<VirtualFileInfo> GetChildFiles(VirtualFolderInfo parent)
    {
      Ensure.ArgumentNotNull(parent, "parent");
      return GetChildFiles(parent.FullName);
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
    public virtual IEnumerable<VirtualFileInfo> GetChildFiles(string parentFolderPath, string searchPattern)
    {
      Ensure.ArgumentNotNull(searchPattern, "searchPattern");

      var files = GetChildFiles(parentFolderPath);
      return files.Filter(searchPattern);
    }

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
    public virtual IEnumerable<VirtualFileInfo> GetChildFiles(VirtualFolderInfo parent, string searchPattern)
    {
      Ensure.ArgumentNotNull(parent, "parent");
      return GetChildFiles(parent.FullName, searchPattern);
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
    public virtual FolderContentsInfo GetFolderContents(string parentFolderPath)
    {
      Ensure.ArgumentNotNull(parentFolderPath, "parentFolderPath");

      var parent = GetFolderInfo(parentFolderPath);

      var folders = GetChildFolders(parent);
      var files = GetChildFiles(parent);

      return new FolderContentsInfo(parent.FullName, folders, files);
    }

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
    public virtual FolderContentsInfo GetFolderContents(VirtualFolderInfo parent)
    {
      Ensure.ArgumentNotNull(parent, "parent");
      return GetFolderContents(parent.FullName);
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
    public virtual FolderContentsInfo GetFolderContents(string parentFolderPath, string searchPattern)
    {
      Ensure.ArgumentNotNull(searchPattern, "searchPattern");

      //call overload and apply filter
      var contents = GetFolderContents(parentFolderPath);
      contents.Folders = contents.Folders.Filter(searchPattern);
      contents.Files = contents.Files.Filter(searchPattern);

      return contents;
    }

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
    public virtual FolderContentsInfo GetFolderContents(VirtualFolderInfo parent, string searchPattern)
    {
      Ensure.ArgumentNotNull(parent, "parent");
      return GetFolderContents(parent.FullName, searchPattern);
    }


    /// <summary>
    /// Checks whether a file resource at a given path exists or not.
    /// </summary>
    /// <param name="virtualFilePath">A path to the requested file.</param>
    /// <returns>True if a matching file was found.</returns>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    public abstract bool IsFileAvailable(string virtualFilePath);

    /// <summary>
    /// Checks whether a folder resource at a given path exists or not.
    /// </summary>
    /// <param name="virtualFolderPath">A path to the requested folder.</param>
    /// <returns>True if a matching folder was found.</returns>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    public abstract bool IsFolderAvailable(string virtualFolderPath);

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
    public virtual VirtualFolderInfo CreateFolder(string parentFolderPath, string folderName)
    {
      Ensure.ArgumentNotNull(folderName, "folderName");

      string fullPath = CreateFolderPath(parentFolderPath, folderName);
      return CreateFolder(fullPath);
    }

    /// <summary>
    /// Creates a new folder in the file system.
    /// </summary>
    /// <param name="virtualFolderPath">The qualified path of the folder to be created.</param>
    /// <returns>A <see cref="VirtualFolderInfo"/> instance which represents
    /// the created folder.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="virtualFolderPath"/>
    /// is a null reference.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the designated parent
    /// folder does not exist.</exception>
    /// <exception cref="ResourceOverwriteException">If the folder already exists on the file
    /// system.</exception>
    public abstract VirtualFolderInfo CreateFolder(string virtualFolderPath);

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
    public virtual VirtualFolderInfo CreateFolder(VirtualFolderInfo parent, string folderName)
    {
      Ensure.ArgumentNotNull(parent, "parent");
      return CreateFolder(parent.FullName, folderName);
    }

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
    public virtual VirtualFileInfo WriteFile(string parentFolderPath, string fileName, Stream input, bool overwrite, long resourceLength, string contentType)
    {
      Ensure.ArgumentNotNull(parentFolderPath, parentFolderPath);
      Ensure.ArgumentNotNull(fileName, fileName);

      string filePath = CreateFilePath(parentFolderPath, fileName);
      return WriteFile(filePath, input, overwrite, resourceLength, contentType);
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
    public abstract VirtualFileInfo WriteFile(string virtualFilePath, Stream input, bool overwrite, long resourceLength, string contentType);

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
    public virtual VirtualFileInfo WriteFile(VirtualFolderInfo parent, string fileName, Stream input, bool overwrite, long resourceLength, string contentType)
    {
      Ensure.ArgumentNotNull(parent, "parent");
      return WriteFile(parent.FullName, fileName, input, overwrite, resourceLength, contentType);
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
    public abstract void DeleteFolder(string virtualFolderPath);

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
    public abstract void DeleteFile(string virtualFilePath);

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
    public abstract VirtualFolderInfo MoveFolder(string virtualFolderPath, string destinationPath);

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
    public virtual VirtualFolderInfo MoveFolder(VirtualFolderInfo folder, string destinationPath)
    {
      Ensure.ArgumentNotNull(folder, "folder");
      return MoveFolder(folder.FullName, destinationPath);
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
    public abstract VirtualFileInfo MoveFile(string virtualFilePath, string destinationPath);

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
    public virtual VirtualFileInfo MoveFile(VirtualFileInfo file, string destinationPath)
    {
      Ensure.ArgumentNotNull(file, "file");
      return MoveFile(file.FullName, destinationPath);
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
    public abstract VirtualFolderInfo CopyFolder(string virtualFolderPath, string destinationPath);


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
    public virtual VirtualFolderInfo CopyFolder(VirtualFolderInfo folder, string destinationPath)
    {
      Ensure.ArgumentNotNull(folder, "folder");
      return CopyFolder(folder.FullName, destinationPath);
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
    public abstract VirtualFileInfo CopyFile(string virtualFilePath, string destinationPath);

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
    public virtual VirtualFileInfo CopyFile(VirtualFileInfo file, string destinationPath)
    {
      Ensure.ArgumentNotNull(file, "file");
      return CopyFile(file.FullName, destinationPath);
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
    public abstract string CreateFilePath(string parentFolder, string fileName);

    /// <summary>
    /// Creates a qualified name that can be used as an identifier
    /// for a given folder of the file system.
    /// </summary>
    /// <param name="parentFolder">The qualified name of the parent
    /// folder.</param>
    /// <param name="folderName">The name of the child folder.</param>
    /// <returns>An qualified path name for the submitted
    /// <paramref name="folderName"/>.</returns>
    public abstract string CreateFolderPath(string parentFolder, string folderName);


    /// <summary>
    /// Provides exception handling and auditing for a given function.
    /// </summary>
    /// <param name="task">The context, used for auditing exceptions that may occur.</param>
    /// <param name="func">The function to be invoked.</param>
    /// <param name="errorMessage">Returns an error message in case of an unhandled exception
    /// that is not derived from <see cref="VfsException"/>.</param>
    /// <returns>The result of the submitted <paramref name="func"/> function.</returns>
    protected virtual T SecureFunc<T>(FileSystemTask task, Func<T> func, Func<string> errorMessage)
    {
      return VfsUtil.SecureFunc(task, func, errorMessage, Auditor);
    }


    /// <summary>
    /// Provides exception handling and auditing for a given function.
    /// </summary>
    /// <param name="task">The context, used for auditing exceptions that may occur.</param>
    /// <param name="action">The action to be invoked.</param>
    /// <param name="errorMessage">Returns an error message in case of an unhandled exception
    /// that is not derived from <see cref="VfsException"/>.</param>
    /// <returns>The result of the submitted <paramref name="action"/> function.</returns>
    protected virtual void SecureAction(FileSystemTask task, Action action, Func<string> errorMessage)
    {
      VfsUtil.SecureAction(task, action, errorMessage, Auditor);
    }



    #region finalizer

    /// <summary>
    /// This destructor will run only if the <see cref="Dispose()"/>
    /// method does not get called. This gives this base class the
    /// opportunity to finalize.
    /// <para>
    /// Important: Do not provide destructors in types derived from
    /// this class.
    /// </para>
    /// </summary>
    ~FileSystemProviderBase()
    {
      //delegate disposal
      Dispose(false);
    }

    #endregion


    #region IDisposable pattern

    /// <summary>
    /// Disposes the object.
    /// </summary>
    /// <remarks>This class is not virtual by design. Derived classes
    /// should override <see cref="Dispose(bool)"/>.
    /// </remarks>
    public void Dispose()
    {
      Dispose(true);

      // This object will be cleaned up by the Dispose method.
      // Therefore, you should call GC.SupressFinalize to
      // take this object off the finalization queue 
      // and prevent finalization code for this object
      // from executing a second time.
      GC.SuppressFinalize(this);
    }



    /// <summary>
    /// <c>Dispose(bool disposing)</c> executes in two distinct scenarios.
    /// If disposing equals <c>true</c>, the method has been called directly
    /// or indirectly by a user's code. Managed and unmanaged resources
    /// can be disposed.
    /// </summary>
    /// <param name="disposing">If disposing equals <c>false</c>, the method
    /// has been called by the runtime from inside the finalizer and you
    /// should not reference other objects. Only unmanaged resources can
    /// be disposed.</param>
    /// <remarks>The base implementation of this method sets the <see cref="IsDisposed"/> property,
    /// which may be prevented by overriding the method. If you invoke the base method, you can
    /// always check the <see cref="IsDisposed"/> property to determine whether
    /// the method has already been called or not.</remarks>
    protected virtual void Dispose(bool disposing)
    {
      IsDisposed = true;
    }

    #endregion
  }
}
