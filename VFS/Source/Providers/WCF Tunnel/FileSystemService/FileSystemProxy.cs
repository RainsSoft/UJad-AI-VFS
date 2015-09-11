using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel.Web;
using Vfs.Auditing;
using Vfs.FileSystemService.Faults;
using Vfs.LocalFileSystem;
using Vfs.Transfer;
using Vfs.Util;

namespace Vfs.FileSystemService
{
  /// <summary>
  /// A proxy class which service enables an underlying file system
  /// provider.
  /// </summary>
  public partial class FileSystemProxy : IFSOperationService, IFSWriterService, IFSReaderService, IFSWebDataService, IFSDataDownloadService, IFSDataUploadService
  {
    

    /// <summary>
    /// The underlying file system provider that is used to
    /// access the service enabled file system.
    /// </summary>
    /// TODO make non-static
    public static IFileSystemProvider Decorated { get; set; }



    public FileSystemProxy()
    {
      var root = new DirectoryInfo(@"D:\Downloads\1-Time Garbage\_VFS-SERVICE-ROOT");
      if (Decorated == null)
      {
        Decorated = new LocalFileSystemProvider(root, true);
      }
    }


    /// <summary>
    /// Gets the root of the file system. This is a dummy folder, which
    /// represents the file system as a whole, and provides the top level contents
    /// of the underlying file system as files and folders.
    /// </summary>
    public VirtualFolderInfo GetFileSystemRoot()
    {
      return FaultUtil.SecureFunc(FileSystemTask.RootFolderInfoRequest, () => Decorated.GetFileSystemRoot());
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
    public VirtualFileInfo GetFileInfo(string virtualFilePath)
    {
      return FaultUtil.SecureFunc(FileSystemTask.FileInfoRequest, () => Decorated.GetFileInfo(virtualFilePath));
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
    public VirtualFolderInfo GetFolderInfo(string virtualFolderPath)
    {
      return FaultUtil.SecureFunc(FileSystemTask.FolderInfoRequest, () => Decorated.GetFolderInfo(virtualFolderPath));
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
    public VirtualFolderInfo GetFileParent(string childFilePath)
    {
      return FaultUtil.SecureFunc(FileSystemTask.FileParentRequest, () => Decorated.GetFileParent(childFilePath));
    }

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
    public VirtualFolderInfo GetFileParent(VirtualFileInfo child)
    {
      return FaultUtil.SecureFunc(FileSystemTask.FileParentRequest, () => Decorated.GetFileParent(child));
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
    public VirtualFolderInfo GetFolderParent(string childFolderPath)
    {
      return FaultUtil.SecureFunc(FileSystemTask.FolderParentRequest, () => Decorated.GetFolderParent(childFolderPath));
    }

    /// <summary>
    /// Gets the parent folder of a given file system resource.
    /// </summary>
    /// <param name="child">An arbitrary folder resource of the file system.</param>
    /// <returns>The parent of the folder.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="child"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the folder that is represented
    /// by <paramref name="child"/> does not exist in the file system.</exception>
    public VirtualFolderInfo GetFolderParent(VirtualFolderInfo child)
    {
      return FaultUtil.SecureFunc(FileSystemTask.FolderParentRequest, () => Decorated.GetFolderParent(child));
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
    public IEnumerable<VirtualFolderInfo> GetChildFolders(string parentFolderPath)
    {
      return FaultUtil.SecureFunc(FileSystemTask.ChildFoldersRequest, () => Decorated.GetChildFolders(parentFolderPath));
    }

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
    public IEnumerable<VirtualFolderInfo> GetChildFolders(VirtualFolderInfo parent)
    {
      return FaultUtil.SecureFunc(FileSystemTask.ChildFoldersRequest, () => Decorated.GetChildFolders(parent));
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
    public IEnumerable<VirtualFolderInfo> GetChildFolders(string parentFolderPath, string searchPattern)
    {
      return FaultUtil.SecureFunc(FileSystemTask.ChildFoldersRequest,
                                  () => Decorated.GetChildFolders(parentFolderPath, searchPattern));
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
    public IEnumerable<VirtualFolderInfo> GetChildFolders(VirtualFolderInfo parent, string searchPattern)
    {
      return FaultUtil.SecureFunc(FileSystemTask.ChildFoldersRequest,
                                  () => Decorated.GetChildFolders(parent, searchPattern));
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
    public IEnumerable<VirtualFileInfo> GetChildFiles(string parentFolderPath)
    {
      return FaultUtil.SecureFunc(FileSystemTask.ChildFilesRequest, () => Decorated.GetChildFiles(parentFolderPath));
    }

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
    public IEnumerable<VirtualFileInfo> GetChildFiles(VirtualFolderInfo parent)
    {
      return FaultUtil.SecureFunc(FileSystemTask.ChildFilesRequest, () => Decorated.GetChildFiles(parent));
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
    public IEnumerable<VirtualFileInfo> GetChildFiles(string parentFolderPath, string searchPattern)
    {
      return FaultUtil.SecureFunc(FileSystemTask.ChildFilesRequest,
                                  () => Decorated.GetChildFiles(parentFolderPath, searchPattern));
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
    public IEnumerable<VirtualFileInfo> GetChildFiles(VirtualFolderInfo parent, string searchPattern)
    {
      return FaultUtil.SecureFunc(FileSystemTask.ChildFilesRequest, () => Decorated.GetChildFiles(parent, searchPattern));
    }

    /// <summary>
    /// Gets all files and folders of a given folder.
    /// </summary>
    /// <param name="parentFolderPath">The <see cref="VirtualResourceInfo.FullName"/>, which
    /// identifies the parent folder within the file system.</param>
    /// <returns>The files and folders of the submitted parent.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="parentFolderPath"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the folder that is represented
    /// by <paramref name="parentFolderPath"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    public FolderContentsInfo GetFolderContents(string parentFolderPath)
    {
      return FaultUtil.SecureFunc(FileSystemTask.FolderContentsRequest,
                                  () => Decorated.GetFolderContents(parentFolderPath));
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
    public FolderContentsInfo GetFolderContents(VirtualFolderInfo parent)
    {
      return FaultUtil.SecureFunc(FileSystemTask.FolderContentsRequest, () => Decorated.GetFolderContents(parent));
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
    /// <exception cref="ArgumentNullException">If <paramref name="parentFolderPath"/>
    /// is a null reference.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="searchPattern"/>
    /// is a null reference.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="searchPattern"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the folder that is represented
    /// by <paramref name="parentFolderPath"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    public FolderContentsInfo GetFolderContents(string parentFolderPath, string searchPattern)
    {
      return FaultUtil.SecureFunc(FileSystemTask.FolderContentsRequest,
                                  () => Decorated.GetFolderContents(parentFolderPath, searchPattern));
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
    public FolderContentsInfo GetFolderContents(VirtualFolderInfo parent, string searchPattern)
    {
      return FaultUtil.SecureFunc(FileSystemTask.FolderContentsRequest,
                                  () => Decorated.GetFolderContents(parent, searchPattern));
    }

    /// <summary>
    /// Checks whether a file resource at a given path exists or not.
    /// </summary>
    /// <param name="virtualFilePath">A path to the requested file.</param>
    /// <returns>True if a matching file was found.</returns>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    public bool IsFileAvailable(string virtualFilePath)
    {
      return FaultUtil.SecureFunc(FileSystemTask.CheckFileAvailability, () => Decorated.IsFileAvailable(virtualFilePath));
    }

    /// <summary>
    /// Checks whether a folder resource at a given path exists or not.
    /// </summary>
    /// <param name="virtualFolderPath">A path to the requested folder.</param>
    /// <returns>True if a matching folder was found.</returns>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    public bool IsFolderAvailable(string virtualFolderPath)
    {
      return FaultUtil.SecureFunc(FileSystemTask.CheckFolderAvailability,
                                  () => Decorated.IsFolderAvailable(virtualFolderPath));
    }

    /// <summary>
    /// Creates a new folder in the file system.
    /// </summary>
    /// <param name="parentFolderPath">The qualified name of the designated parent folder, which
    /// needs to exists, and provide write access.</param>
    /// <param name="folderName">The name of the folder to be created.</param>
    /// <returns>A <see cref="VirtualFileInfo"/> instance which represents
    /// the created folder.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="parentFolderPath"/>
    /// is a null reference.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="folderName"/>
    /// is a null reference.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If no folder exists that
    /// matches the submitted <paramref name="parentFolderPath"/>.</exception>
    /// <exception cref="ResourceOverwriteException">If the folder already exists on the file
    /// system.</exception>
    public VirtualFolderInfo CreateFolder(string parentFolderPath, string folderName)
    {
      return FaultUtil.SecureFunc(FileSystemTask.FolderCreateRequest,
                                  () => Decorated.CreateFolder(parentFolderPath, folderName));
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
    public VirtualFolderInfo CreateFolder(string virtualFolderPath)
    {
      return FaultUtil.SecureFunc(FileSystemTask.FolderCreateRequest, () => Decorated.CreateFolder(virtualFolderPath));
    }

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
    public VirtualFolderInfo CreateFolder(VirtualFolderInfo parent, string folderName)
    {
      return FaultUtil.SecureFunc(FileSystemTask.FolderCreateRequest, () => Decorated.CreateFolder(parent, folderName));
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
    public bool DeleteFolder(string virtualFolderPath)
    {
      FaultUtil.SecureAction(FileSystemTask.FolderDeleteRequest, () => Decorated.DeleteFolder(virtualFolderPath));

      //return type only used to make it a two-way operation that can contain
      //a fault contract
      return true;
    }

    /// <summary>
    /// Deletes a given file from the file system.
    /// </summary>
    /// <param name="virtualFilePath">The qualified path of the file to be created.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="virtualFilePath"/>
    /// is a null reference.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the file does not exist.</exception>
    public bool DeleteFile(string virtualFilePath)
    {
      FaultUtil.SecureAction(FileSystemTask.FileDeleteRequest, () => Decorated.DeleteFile(virtualFilePath));

      //return type only used to make it a two-way operation that can contain
      //a fault contract
      return true;
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
    public VirtualFolderInfo MoveFolder(string virtualFolderPath, string destinationPath)
    {
      return FaultUtil.SecureFunc(FileSystemTask.FolderMoveRequest,
                                  () => Decorated.MoveFolder(virtualFolderPath, destinationPath));
    }

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
    public VirtualFolderInfo MoveFolder(VirtualFolderInfo folder, string destinationPath)
    {
      return FaultUtil.SecureFunc(FileSystemTask.FolderMoveRequest, () => Decorated.MoveFolder(folder, destinationPath));
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
    public VirtualFileInfo MoveFile(string virtualFilePath, string destinationPath)
    {
      return FaultUtil.SecureFunc(FileSystemTask.FileMoveRequest,
                                  () => Decorated.MoveFile(virtualFilePath, destinationPath));
    }

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
    public VirtualFileInfo MoveFile(VirtualFileInfo file, string destinationPath)
    {
      return FaultUtil.SecureFunc(FileSystemTask.FileMoveRequest, () => Decorated.MoveFile(file, destinationPath));
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
    public VirtualFolderInfo CopyFolder(string virtualFolderPath, string destinationPath)
    {
      return FaultUtil.SecureFunc(FileSystemTask.FolderCopyRequest,
                                  () => Decorated.CopyFolder(virtualFolderPath, destinationPath));
    }

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
    public VirtualFolderInfo CopyFolder(VirtualFolderInfo folder, string destinationPath)
    {
      return FaultUtil.SecureFunc(FileSystemTask.FolderCopyRequest, () => Decorated.CopyFolder(folder, destinationPath));
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
    public VirtualFileInfo CopyFile(string virtualFilePath, string destinationPath)
    {
      return FaultUtil.SecureFunc(FileSystemTask.FileCopyRequest,
                                  () => Decorated.CopyFile(virtualFilePath, destinationPath));
    }

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
    public VirtualFileInfo CopyFile(VirtualFileInfo file, string destinationPath)
    {
      return FaultUtil.SecureFunc(FileSystemTask.FileCopyRequest, () => Decorated.CopyFile(file, destinationPath));
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
    public string CreateFilePath(string parentFolder, string fileName)
    {
      return FaultUtil.SecureFunc(FileSystemTask.CreateFilePathRequest,
                                  () => Decorated.CreateFilePath(parentFolder, fileName));
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
    public string CreateFolderPath(string parentFolder, string folderName)
    {
      return FaultUtil.SecureFunc(FileSystemTask.CreateFolderPathRequest,
                                  () => Decorated.CreateFolderPath(parentFolder, folderName));
    }

  }
}