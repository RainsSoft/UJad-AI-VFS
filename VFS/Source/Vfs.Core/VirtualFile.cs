using System;
using System.IO;
using Vfs.Util;

namespace Vfs
{
  /// <summary>
  /// Provides transparent access to a given file of the virtual file system.
  /// </summary>
  public class VirtualFile : VirtualResource<VirtualFileInfo>
  {

    /// <summary>
    /// Creates a new <see cref="VirtualFile"/> instance
    /// which represents a given file on the submitted file system.
    /// </summary>
    /// <param name="provider">Provides access to the file system.</param>
    /// <param name="metaData">Encapsulates meta information about the represented file.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="provider"/>
    /// is a null reference.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="metaData"/>
    /// is a null reference.</exception>
    public VirtualFile(IFileSystemProvider provider, VirtualFileInfo metaData) : base(provider, metaData)
    {
    }


    /// <summary>
    /// Checks whether the resource exists on the file system or not.
    /// </summary>
    public override bool Exists
    {
      get { return Provider.IsFileAvailable(MetaData.FullName); }
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
      VirtualFolderInfo folderInfo = Provider.GetFileParent(MetaData.FullName);
      return new VirtualFolder(Provider, folderInfo);
    }


    /// <summary>
    /// Refreshes the underlying <see cref="VirtualResource{T}.MetaData"/>.
    /// </summary>
    public override void RefreshMetaData()
    {
      MetaData = Provider.GetFileInfo(MetaData.FullName);
    }


    /// <summary>
    /// Creates or overwrites the current file contents on the file system.
    /// </summary>
    /// <param name="input">A stream that provides the file's contents.</param>
    /// <param name="overwrite">Whether an existing file should be overwritten
    /// or not. If this parameter is false and the file already exists, a
    /// <see cref="ResourceOverwriteException"/> is thrown.</param>
    /// <param name="resourceLength">The length of the file to be uploaded to the
    /// file system. May be determined via the <see cref="Stream.Length"/> property
    /// of the submitted <paramref name="input"/> stream, if the stream supports
    /// it.</param>
    /// <param name="contentType">The content type of the file. May be resolved via
    /// the <see cref="VirtualFileInfo.ContentType"/> property in case of an update,
    /// or the <see cref="ContentUtil.ResolveContentType"/> helper method.</param>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    /// <exception cref="ResourceOverwriteException">If a file already exists at the
    /// specified location, and the <paramref name="overwrite"/> flag was not set.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="input"/> is a
    /// null reference.</exception>
    public void CreateOrUpdateContents(Stream input, bool overwrite, long resourceLength, string contentType)
    {
      MetaData = Provider.WriteFile(MetaData.FullName, input, overwrite, resourceLength, contentType);
    }


    /// <summary>
    /// Creates or overwrites a given file on the file system with the
    /// contents of a local file.
    /// </summary>
    /// <param name="localFilePath">The path of the file to be uploaded.</param>
    /// <param name="overwrite">Whether an existing file should be overwritten
    /// or not. If this parameter is false and the file already exists, a
    /// <see cref="ResourceOverwriteException"/> is thrown.</param>
    public void CreateOrUpdateContents(string localFilePath, bool overwrite)
    {
      MetaData = Provider.WriteFile(localFilePath, MetaData.FullName, overwrite);
    }


    /// <summary>
    /// Gets the binary contents of the file as a stream in a blocking operation.
    /// Use the methods in <see cref="ContentUtil"/> class for simplified stream
    /// handling. Important: Make sure the returned stream is disposed properly!
    /// </summary>
    /// <exception cref="VirtualResourceNotFoundException">If the file that is represented
    /// by this object does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    public Stream GetContents()
    {
      return Provider.ReadFileContents(MetaData.FullName);
    }


    /// <summary>
    /// This is a convenience extension method for <see cref="IFileSystemProvider"/> instances,
    /// which retrieves the binary contents of the file from the file system, and writes them to
    /// a local file.
    /// </summary>
    /// <param name="filePath">The file to be created. If a corresponding file already
    /// exists, it will be overwritten.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="filePath"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the file that is represented
    /// by this object does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    public void SaveContentsAs(string filePath)
    {
      Provider.SaveFile(MetaData, filePath);
    }

    
    /// <summary>
    /// Physically removes the represented file from the underyling
    /// file system.
    /// </summary>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the file that is represented
    /// by this object does not exist in the file system.</exception>
    public void Delete()
    {
      Provider.DeleteFile(MetaData.FullName);
    }

    /// <summary>
    /// Creates a copy of the file at a different location on the file system.
    /// </summary>
    /// <param name="destinationPath">The new path of the file on the
    /// file system.</param>
    /// <returns>Returns a <see cref="VirtualFolder"/> that represents the new copy
    /// of this file on the file system.</returns>
    public VirtualFile Copy(string destinationPath)
    {
      var copy = Provider.CopyFile(MetaData, destinationPath);
      return new VirtualFile(Provider, copy);
    }


    /// <summary>
    /// Moves the file to a different location on the file system and
    /// updates the underlying <see cref="VirtualResource{T}.MetaData"/> accordingly.
    /// </summary>
    /// <param name="destinationPath">The new path of the file on the
    /// file system.</param>
    public void Move(string destinationPath)
    {
      MetaData = Provider.MoveFile(MetaData, destinationPath);
    }


    /// <summary>
    /// Creates a new <see cref="VirtualFile"/> instance that represents a file
    /// on a given file system.
    /// </summary>
    /// <param name="fileSystem">The file system to be used.</param>
    /// <param name="virtualFilePath">The qualified path (corresponding to
    /// <see cref="VirtualResourceInfo.FullName"/> that identifies the file on the
    /// file system.</param>
    /// <returns>A representation of the file on the file system.</returns>
    /// <exception cref="ArgumentNullException">If any of the parameters
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the file cannot
    /// be found.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    public static VirtualFile Create(IFileSystemProvider fileSystem, string virtualFilePath)
    {
      if (fileSystem == null) throw new ArgumentNullException("fileSystem");
      if (virtualFilePath == null) throw new ArgumentNullException("virtualFilePath");

      var fileInfo = fileSystem.GetFileInfo(virtualFilePath);
      return new VirtualFile(fileSystem, fileInfo);
    }

  }



}