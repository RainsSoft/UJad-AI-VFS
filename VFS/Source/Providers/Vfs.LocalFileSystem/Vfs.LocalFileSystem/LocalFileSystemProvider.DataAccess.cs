using System;
using System.IO;
using Vfs.Util;

namespace Vfs.LocalFileSystem
{
  public partial class LocalFileSystemProviderX
  {
    #region read file contents

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
      if (virtualFilePath == null) throw new ArgumentNullException("virtualFilePath");

      string absolutePath;
      GetFileInfoInternal(virtualFilePath, true, out absolutePath);

      try
      {
        return File.OpenRead(absolutePath);
      }
      catch (Exception e)
      {
        VfsLog.Error(e, "Could not open file [{0}] requested through virtual file path [{1}]", absolutePath, virtualFilePath);
        string msg = String.Format("An error occurred while attempting to open file [{0}].", virtualFilePath);
        throw new ResourceAccessException(msg, e);
      }
    }

    #endregion


    #region create files / folders

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
    public override VirtualFolderInfo CreateFolder(string parentFolderPath, string folderName)
    {
      if (parentFolderPath == null) throw new ArgumentNullException("parentFolderPath");
      if (folderName == null) throw new ArgumentNullException("folderName");

      string absoluteParentPath;
      var parent = GetFolderInfoInternal(parentFolderPath, true, out absoluteParentPath);

      if (RootDirectory == null && parent.IsRootFolder)
      {
        VfsLog.Debug("Blocked attempt to create a folder '{0}' at system root.", folderName);
        throw new ResourceAccessException("Folders cannot be created at the system root.");
      }
      
      //create path of the child
      string childPath = PathUtil.GetAbsolutePath(folderName, new DirectoryInfo(absoluteParentPath));

      //make sure the folder name is not a relative path that points outside the scope
      if (RootDirectory != null && !RootDirectory.IsParentOf(childPath))
      {
        string msg = "Blocked attempt to create folder outside of root through with parent '{0}' and folder name '{1}'";
        VfsLog.Warn(msg, absoluteParentPath, folderName);

        throw new ResourceAccessException("Invalid file path: " + folderName);
      }

      var directory = new DirectoryInfo(childPath);
      if (directory.Exists)
      {
        //log and create exception if the directory already exists
        VfsLog.Debug("Blocked attempt to recreate directory '{0}'", directory.FullName);
        string relativePath = PathUtil.GetRelativePath(childPath, RootDirectory);
        string msg = String.Format("The folder '{0}' already exists.", relativePath);
        throw new ResourceOverwriteException(msg);
      }

      try
      {
        //create directory
        directory.Create();
      }
      catch (Exception e)
      {
        const string msg = "Exception occurred when trying to create new folder '{0}' for parent '{1}'";
        VfsLog.Debug(e, msg, folderName, parent.FullName);

        throw new ResourceAccessException("Could not create folder", e);
      }

      var folder = directory.CreateFolderResourceInfo();

      //adjust and return
      if (UseRelativePaths) folder.MakePathsRelativeTo(RootDirectory);
      return folder;
    }


    /// <summary>
    /// Creates a new folder in the file system.
    /// </summary>
    /// <param name="virtualFolderPath">The qualified path of the folder to be created. parent folder, which
    /// needs to exists, and provide write access.</param>
    /// <returns>A <see cref="VirtualFileInfo"/> instance which represents
    /// the created folder.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="virtualFolderPath"/>
    /// is a null reference.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the parent folder
    /// does not exist.</exception>
    public override VirtualFolderInfo CreateFolder(string virtualFolderPath)
    {
      string parentPath, directoryName;
      try
      {
        string absolutePath;
        GetFolderInfoInternal(virtualFolderPath, false, out absolutePath);

        var dir = new DirectoryInfo(absolutePath);
        var parent = dir.Parent;
        parentPath = parent == null ? "" : parent.FullName;
        directoryName = dir.Name;
      }
      catch (Exception e)
      {
        string msg = String.Format("Submitted path [{0}] caused exception when trying to create folder.", virtualFolderPath);
        VfsLog.Debug(e, msg);
        throw new ResourceAccessException(msg, e);
      }

      return CreateFolder(parentPath, directoryName);
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
    /// <exception cref="VirtualResourceNotFoundException">If the parent folder
    /// does not exist.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    /// <exception cref="ResourceOverwriteException">If a file already exists at the
    /// specified location, and the <paramref name="overwrite"/> flag was not set.</exception>
    /// <exception cref="ArgumentNullException">If any of the parameters is a null reference.</exception>
    public override VirtualFileInfo WriteFile(string parentFolderPath, string fileName, Stream input, bool overwrite)
    {
      if (parentFolderPath == null) throw new ArgumentNullException("parentFolderPath");
      if (fileName == null) throw new ArgumentNullException("fileName");
      if (input == null) throw new ArgumentNullException("input");


      //get the parent and make sure it exists
      string absoluteParentPath;
      var parent = GetFolderInfoInternal(parentFolderPath, true, out absoluteParentPath);

      if(RootDirectory == null && parent.IsRootFolder)
      {
        VfsLog.Debug("Blocked attempt to create a file '{0}' at system root (which is the machine itself - no root directory was set).", fileName);
        throw new ResourceAccessException("Files cannot be created at the system root.");        
      }

      //combine to file path and get virtual file (also makes sure we don't get out of scope)
      string absoluteFilePath = PathUtil.GetAbsolutePath(fileName, new DirectoryInfo(absoluteParentPath));

      FileInfo fi = new FileInfo(absoluteFilePath);
      if (fi.Exists && !overwrite)
      {
        VfsLog.Debug("Blocked attempt to overwrite file '{0}'", fi.FullName);
        string msg = String.Format("The file [{0}] already exists.", fileName);
        throw new ResourceOverwriteException(msg);
      }

      try
      {
        using (Stream writeStream = new FileStream(fi.FullName, FileMode.Create, FileAccess.Write, FileShare.None))
        {
          input.WriteTo(writeStream);
        }
      }
      catch (Exception e)
      {
        //log exception with full path
        string msg = "Could not write write submitted content to file '{0}'.";
        VfsLog.Error(e, msg, fi.FullName);
        //generate exception with relative path
        msg = String.Format(msg, PathUtil.GetRelativePath(fi.FullName, RootDirectory));
        throw new ResourceAccessException(msg, e);
      }

      //return update file info
      var file = fi.CreateFileResourceInfo();

      //adjust and return
      if (UseRelativePaths) file.MakePathsRelativeTo(RootDirectory);
      return file;
    }


    /// <summary>
    /// Creates or updates a given file resource in the file system.
    /// </summary>
    /// <param name="filePath">The qualified path of the file to be created.</param>
    /// <param name="input">A stream that provides the file's contents.</param>
    /// <param name="overwrite">Whether an existing file should be overwritten
    /// or not. If this parameter is false and the file already exists, a
    /// <see cref="ResourceOverwriteException"/> is thrown.</param>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    /// <exception cref="ResourceOverwriteException">If a file already exists at the
    /// specified location, and the <paramref name="overwrite"/> flag was not set.</exception>
    /// <exception cref="ArgumentNullException">If any of the parameters is a null reference.</exception>
    public override VirtualFileInfo WriteFile(string filePath, Stream input, bool overwrite)
    {
      if (filePath == null) throw new ArgumentNullException("filePath");
      string absolutePath = PathUtil.GetAbsolutePath(filePath, RootDirectory);
      FileInfo fi = new FileInfo(absolutePath);

      return WriteFile(fi.DirectoryName, fi.Name, input, overwrite);
    }

    #endregion


    #region delete files / folders

    /// <summary>
    /// Deletes a given folder from the file system.
    /// </summary>
    /// <param name="virtualFolderPath">The qualified path of the folder to be created.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="virtualFolderPath"/>
    /// is a null reference.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If no file exists under the submitted
    /// <paramref name="virtualFolderPath"/>.</exception>
    public override void DeleteFolder(string virtualFolderPath)
    {
      string absolutePath;
      var folderInfo = GetFolderInfoInternal(virtualFolderPath, true, out absolutePath);

      //do not delete the root
      if (folderInfo.IsRootFolder)
      {
        VfsLog.Warn("Blocked attempt to delete root folder.");
        throw new ResourceAccessException(String.Format("Cannot delete root folder '{0}'", virtualFolderPath));
      }

      DirectoryInfo dir = new DirectoryInfo(absolutePath);

      //do not delete drives
      if (dir.Parent == null)
      {
        VfsLog.Warn("Blocked attempt to delete drive by path [{0}]", virtualFolderPath);
        throw new ResourceAccessException("Cannot delete drive " + virtualFolderPath);
      }

      //delete the folder
      try
      {
        dir.Delete(true);
      }
      catch (Exception e)
      {
        VfsLog.Warn(e, "Error while trying to delete folder '{0}' from file system", virtualFolderPath);
        string msg = String.Format("Could not delete file '{0}'", virtualFolderPath);
        throw new ResourceAccessException(msg);
      }
    }


    /// <summary>
    /// Deletes a given file from the file system.
    /// </summary>
    /// <param name="virtualFilePath">The qualified path of the file to be created.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="virtualFilePath"/>
    /// is a null reference.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If no file exists under the submitted
    /// <paramref name="virtualFilePath"/>.</exception>
    public override void DeleteFile(string virtualFilePath)
    {
      string absolutePath;
      GetFileInfoInternal(virtualFilePath, true, out absolutePath);

      //delete the folder
      try
      {
        File.Delete(absolutePath);
      }
      catch (Exception e)
      {
        VfsLog.Warn(e, "Error while trying to delete file '{0}' from file system", virtualFilePath);
        string msg = String.Format("Could not delete file '{0}'", virtualFilePath);
        throw new ResourceAccessException(msg);
      }
    }

    #endregion
  }
}
