using System;
using System.Collections.Generic;
using System.ServiceModel;
using Vfs.FileSystemService.Faults;

namespace Vfs.FileSystemService
{
  /// <summary>
  /// Provides a service layer for common file system related operations
  /// except up- and downloads.
  /// </summary>
  [ServiceContract(Namespace = Namespace.Main)]
  public interface IFSOperationService
  {

    /// <summary>
    /// Gets the root of the file system. This is a dummy folder, which
    /// represents the file system as a whole, and provides the top level contents
    /// of the underlying file system as files and folders.
    /// </summary>
    [OperationContract]
    [FaultContract(typeof(ResourceFault))]
    VirtualFolderInfo GetFileSystemRoot();

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
    [OperationContract]
    [FaultContract(typeof(ResourceFault))]
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
    [OperationContract]
    [FaultContract(typeof(ResourceFault))]
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
    [OperationContract]
    [FaultContract(typeof(ResourceFault))]
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
    [OperationContract(Name = "GetFileParentByItem")]
    [FaultContract(typeof(ResourceFault))]
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
    [OperationContract]
    [FaultContract(typeof(ResourceFault))]
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
    [OperationContract(Name = "GetFolderParentByItem")]
    [FaultContract(typeof(ResourceFault))]
    VirtualFolderInfo GetFolderParent(VirtualFolderInfo child);

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
    [OperationContract]
    [FaultContract(typeof(ResourceFault))]
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
    [OperationContract(Name = "GetChildFoldersByItem")]
    [FaultContract(typeof(ResourceFault))]
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
    /// <exception cref="ArgumentNullException">If <paramref name="parentFolderPath"/>
    /// is a null reference.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="searchPattern"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the folder that is represented
    /// by <paramref name="parentFolderPath"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    [OperationContract(Name = "GetFilteredChildFolders")]
    [FaultContract(typeof(ResourceFault))]
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
    [OperationContract(Name = "GetFilteredChildFoldersByItem")]
    [FaultContract(typeof(ResourceFault))]
    IEnumerable<VirtualFolderInfo> GetChildFolders(VirtualFolderInfo parent, string searchPattern);

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
    [OperationContract]
    [FaultContract(typeof(ResourceFault))]
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
    [OperationContract(Name = "GetChildFilesByItem")]
    [FaultContract(typeof(ResourceFault))]
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
    /// <exception cref="ArgumentNullException">If <paramref name="parentFolderPath"/>
    /// is a null reference.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="searchPattern"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the folder that is represented
    /// by <paramref name="parentFolderPath"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    [OperationContract(Name = "GetFilteredChildFiles")]
    [FaultContract(typeof(ResourceFault))]
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
    [OperationContract(Name = "GetFilteredChildFilesByItem")]
    [FaultContract(typeof(ResourceFault))]
    IEnumerable<VirtualFileInfo> GetChildFiles(VirtualFolderInfo parent, string searchPattern);

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
    [OperationContract]
    [FaultContract(typeof(ResourceFault))]
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
    [OperationContract(Name = "GetFolderContentsByItem")]
    [FaultContract(typeof(ResourceFault))]
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
    [OperationContract(Name = "GetFilteredFolderContents")]
    [FaultContract(typeof(ResourceFault))]
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
    [OperationContract(Name = "GetFilteredFolderContentsByItem")]
    [FaultContract(typeof(ResourceFault))]
    FolderContentsInfo GetFolderContents(VirtualFolderInfo parent, string searchPattern);

    /// <summary>
    /// Checks whether a file resource at a given path exists or not.
    /// </summary>
    /// <param name="virtualFilePath">A path to the requested file.</param>
    /// <returns>True if a matching file was found.</returns>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    [OperationContract]
    [FaultContract(typeof(ResourceFault))]
    bool IsFileAvailable(string virtualFilePath);

    /// <summary>
    /// Checks whether a folder resource at a given path exists or not.
    /// </summary>
    /// <param name="virtualFolderPath">A path to the requested folder.</param>
    /// <returns>True if a matching folder was found.</returns>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    [OperationContract]
    [FaultContract(typeof(ResourceFault))]
    bool IsFolderAvailable(string virtualFolderPath);

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
    [FaultContract(typeof(ResourceFault))]
    [OperationContract(Name = "CreateFolderUnderParent")]
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
    [OperationContract(Name = "CreateFolder")]
    [FaultContract(typeof(ResourceFault))]
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
    [OperationContract(Name = "CreateFolderUnderParentItem")]
    [FaultContract(typeof(ResourceFault))]
    VirtualFolderInfo CreateFolder(VirtualFolderInfo parent, string folderName);

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
    /// <remarks>The return type makes the operation a two-way operation, which
    /// is capable to return a fault contract if something goes wrong.</remarks>
    [OperationContract]
    [FaultContract(typeof(ResourceFault))]
    bool DeleteFolder(string virtualFolderPath);

    /// <summary>
    /// Deletes a given file from the file system.
    /// </summary>
    /// <param name="virtualFilePath">The qualified path of the file to be created.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="virtualFilePath"/>
    /// is a null reference.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the file does not exist.</exception>
    /// <remarks>The return type makes the operation a two-way operation, which
    /// is capable to return a fault contract if something goes wrong.</remarks>
    [OperationContract]
    [FaultContract(typeof(ResourceFault))]
    bool DeleteFile(string virtualFilePath);

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
    [OperationContract]
    [FaultContract(typeof(ResourceFault))]
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
    [OperationContract(Name = "MoveFolderItem")]
    [FaultContract(typeof(ResourceFault))]
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
    [OperationContract]
    [FaultContract(typeof(ResourceFault))]
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
    [OperationContract(Name="MoveFileItem")]
    [FaultContract(typeof(ResourceFault))]
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
    [OperationContract]
    [FaultContract(typeof(ResourceFault))]
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
    [OperationContract(Name = "CopyFolderItem")]
    [FaultContract(typeof(ResourceFault))]
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
    [OperationContract]
    [FaultContract(typeof(ResourceFault))]
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
    [OperationContract(Name = "CopyFileItem")]
    [FaultContract(typeof(ResourceFault))]
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
    [OperationContract]
    [FaultContract(typeof(ResourceFault))]
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
    [OperationContract]
    [FaultContract(typeof(ResourceFault))]
    string CreateFolderPath(string parentFolder, string folderName);
  }
}
