using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vfs.Util;

namespace Vfs
{
  /// <summary>
  /// Provides transparent access to a given folder
  /// of the virtual file system.
  /// </summary>
  public class VirtualFolder : VirtualResource<VirtualFolderInfo>
  {

    /// <summary>
    /// Creates a new <see cref="VirtualFolder"/> instance
    /// which represents a given folder on the submitted file system.
    /// </summary>
    /// <param name="provider">Provides access to the file system.</param>
    /// <param name="metaData">Encapsulates meta information about the represented folder.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="provider"/>
    /// is a null reference.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="metaData"/>
    /// is a null reference.</exception>
    public VirtualFolder(IFileSystemProvider provider, VirtualFolderInfo metaData) : base(provider, metaData)
    {
    }


    /// <summary>
    /// Gets the parent folder of the resource.
    /// </summary>
    /// <returns>The parent of the resource.</returns>
    /// <exception cref="VirtualResourceNotFoundException">If the resource that is represented
    /// by this object does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of an invalid or prohibited
    /// resource access.</exception>
    public override VirtualFolder GetParentFolder()
    {
      return new VirtualFolder(Provider, Provider.GetFolderParent(MetaData.FullName));
    }


    /// <summary>
    /// Checks whether the resource exists on the file system or not.
    /// </summary>
    public override bool Exists
    {
      get { return Provider.IsFolderAvailable(MetaData.FullName); }
    }


    public IEnumerable<VirtualFile> GetFiles()
    {
      return Provider.GetChildFiles(MetaData.FullName).Select(f => new VirtualFile(Provider, f));
    }

    public IEnumerable<VirtualFile> GetFiles(string searchPattern)
    {
      return Provider.GetChildFiles(MetaData.FullName, searchPattern).Select(f => new VirtualFile(Provider, f));
    }


    public IEnumerable<VirtualFolder> GetFolders()
    {
      return Provider.GetChildFolders(MetaData.FullName).Select(f => new VirtualFolder(Provider, f));
    }


    public IEnumerable<VirtualFolder> GetFolders(string searchPattern)
    {
      return Provider.GetChildFolders(MetaData.FullName, searchPattern).Select(f => new VirtualFolder(Provider, f));
    }





    public FolderContents GetFolderContents()
    {
      var contentsInfo = Provider.GetFolderContents(MetaData.FullName);

      return new FolderContents
                       {
                         ParentFolderPath = contentsInfo.ParentFolderPath,
                         Files = contentsInfo.Files.Select(f => new VirtualFile(Provider, f)),
                         Folders = contentsInfo.Folders.Select(f => new VirtualFolder(Provider, f)),
                       };
    }



    /// <summary>
    /// Gets all files and sub folders that match the submitted <paramref name="searchPattern"/>.
    /// </summary>
    /// <param name="searchPattern"></param>
    /// <returns></returns>
    public FolderContents GetFolderContents(string searchPattern)
    {
      var contentsInfo = Provider.GetFolderContents(MetaData.FullName, searchPattern);

      return new FolderContents
      {
        ParentFolderPath = contentsInfo.ParentFolderPath,
        Files = contentsInfo.Files.Select(f => new VirtualFile(Provider, f)),
        Folders = contentsInfo.Folders.Select(f => new VirtualFolder(Provider, f)),
      };
    }



    /// <summary>
    /// Physically removes the represented folder and its contents from the
    /// file system.
    /// </summary>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the folder that is represented
    /// by this object does not exist in the file system.</exception>
    public void Delete()
    {
      Provider.DeleteFolder(MetaData.FullName);  
    }

    
    /// <summary>
    /// Copies the folder and its contents to a different location on the file system.
    /// </summary>
    /// <param name="destinationPath">The new path of the folder on the
    /// file system.</param>
    /// <returns>Returns a <see cref="VirtualFolder"/> that represents the new copy
    /// of this folder on the file system.</returns>
    public VirtualFolder Copy(string destinationPath)
    {
      var folderInfo = Provider.CopyFolder(MetaData, destinationPath);
      return new VirtualFolder(Provider, folderInfo);
    }


    /// <summary>
    /// Moves the folder to a different location on the file system and
    /// updates the underlying <see cref="VirtualResource{T}.MetaData"/> accordingly.
    /// </summary>
    /// <param name="destinationPath">The new path of the folder on the
    /// file system.</param>
    public void Move(string destinationPath)
    {
      MetaData = Provider.MoveFolder(MetaData, destinationPath);
    }


    /// <summary>
    /// Creates a sub folder with the submitted name on the file system.
    /// </summary>
    /// <param name="folderName">The name of the folder to be created.</param>
    /// <returns>Folder object that represents the folder on the file system.</returns>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the designated parent
    /// folder (usually this folder, unless <paramref name="folderName"/> is a relative
    /// path) does not exist in the file system.</exception>
    /// <exception cref="ResourceOverwriteException">If the folder already exists on the file
    /// system.</exception>
    public VirtualFolder AddFolder(string folderName)
    {
      var folder = Provider.CreateFolder(MetaData.FullName, folderName);
      return new VirtualFolder(Provider, folder);
    }


    /// <summary>
    /// Adds a file to the folder.
    /// </summary>
    /// <param name="fileName">The name of the file to be created.</param>
    /// <param name="input">A stream that provides the file's binary contents.</param>
    /// <param name="overwrite">Whether to overwrite an already existing file or not.</param>
    /// <param name="resourceLength">The length of the file to be uploaded to the
    /// file system. May be determined via the <see cref="Stream.Length"/> property
    /// of the submitted <paramref name="input"/> stream, if the stream supports
    /// it.</param>
    /// <param name="contentType">The content type of the file. May be resolved via
    /// the <see cref="VirtualFileInfo.ContentType"/> property in case of an update,
    /// or the <see cref="ContentUtil.ResolveContentType"/> helper method.</param>
    /// <returns>A <see cref="VirtualFile"/> instance that represents the file
    /// on the file system.</returns>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the parent folder
    /// does not exist.</exception>
    /// <exception cref="ResourceOverwriteException">If a file already exists at the
    /// specified location, and the <paramref name="overwrite"/> flag was not set.</exception>
    public VirtualFile AddFile(string fileName, Stream input, bool overwrite, long resourceLength, string contentType)
    {
      var file = Provider.WriteFile(MetaData.FullName, fileName, input, overwrite, resourceLength, contentType);
      return new VirtualFile(Provider, file);
    }


    /// <summary>
    /// Adds a file to the folder.
    /// </summary>
    /// <param name="localFilePath">The path of the file to be uploaded.</param>
    /// <param name="destinationFileName">The name of the file that is being created
    /// within the folder.</param>
    /// <param name="overwrite">Whether to overwrite an already existing file or not.</param>
    /// <returns>A <see cref="VirtualFile"/> instance that represents the added file
    /// on the file system.</returns>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the parent folder
    /// does not exist.</exception>
    /// <exception cref="ResourceOverwriteException">If a file already exists at the
    /// specified location, and the <paramref name="overwrite"/> flag was not set.</exception>
    public VirtualFile AddFile(string localFilePath, string destinationFileName, bool overwrite)
    {
      string filePath = Provider.CreateFilePath(MetaData.FullName, destinationFileName);
      var file = Provider.WriteFile(localFilePath, filePath, overwrite);
      return new VirtualFile(Provider, file);
    }


    /// <summary>
    /// Refreshes the underlying <see cref="VirtualResource{T}.MetaData"/>.
    /// </summary>
    public override void RefreshMetaData()
    {
      MetaData = Provider.GetFolderInfo(MetaData.FullName);
    }


    /// <summary>
    /// A builder method which creates a new <see cref="VirtualFolder"/> instance
    /// that represents an (already existing) folder on a given file system.
    /// </summary>
    /// <param name="fileSystem">The file system to be used.</param>
    /// <param name="virtualFolderPath">The qualified path (corresponding to
    /// <see cref="VirtualResourceInfo.FullName"/> that identifies the folder on the
    /// file system.</param>
    /// <returns>A representation of the folder on the file system.</returns>
    /// <exception cref="ArgumentNullException">If any of the parameters
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the folder cannot
    /// be found.</exception>
    /// <exception cref="ResourceAccessException">If the user does not have
    /// permission to access this resource.</exception>
    public static VirtualFolder Create(IFileSystemProvider fileSystem, string virtualFolderPath)
    {
      if (fileSystem == null) throw new ArgumentNullException("fileSystem");
      if (virtualFolderPath == null) throw new ArgumentNullException("virtualFolderPath");

      var folderInfo = fileSystem.GetFolderInfo(virtualFolderPath);
      return new VirtualFolder(fileSystem, folderInfo);
    }


    /// <summary>
    /// Creates a <see cref="VirtualFolder"/> instance that represents
    /// the root folder of a given file system.
    /// </summary>
    /// <param name="fileSystem">The file system to be used.</param>
    /// <returns>The root folder of the submitted <paramref name="fileSystem"/>,
    /// as returned by <see cref="IFileSystemProvider.GetFileSystemRoot"/>.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="fileSystem"/>
    /// is a null reference.</exception>
    public static VirtualFolder CreateRootFolder(IFileSystemProvider fileSystem)
    {
      if (fileSystem == null) throw new ArgumentNullException("fileSystem");

      var rootInfo = fileSystem.GetFileSystemRoot();
      return new VirtualFolder(fileSystem, rootInfo);
    }
  }
}