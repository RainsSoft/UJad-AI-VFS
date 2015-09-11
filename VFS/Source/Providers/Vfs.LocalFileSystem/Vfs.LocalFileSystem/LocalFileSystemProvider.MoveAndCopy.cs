using System;
using System.IO;


namespace Vfs.LocalFileSystem
{
	public partial class LocalFileSystemProviderX
	{
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
	    return PerformFolderOperation(virtualFolderPath, destinationPath, FileOperation.Move);
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
      return PerformFileOperation(virtualFilePath, destinationPath, FileOperation.Move);
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
      return PerformFolderOperation(virtualFolderPath, destinationPath, FileOperation.Copy);
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
	    return PerformFileOperation(virtualFilePath, destinationPath, FileOperation.Copy);
	  }



	  private VirtualFileInfo PerformFileOperation(string virtualFilePath, string destinationPath, FileOperation operation)
    {
      if (virtualFilePath == null) throw new ArgumentNullException("virtualFilePath");
      if (destinationPath == null) throw new ArgumentNullException("destinationPath");

      //get folder info for source and destination, thus validating the scope of both
      string absoluteSource;
      GetFileInfoInternal(virtualFilePath, true, out absoluteSource);
      string absoluteDestination;
      GetFileInfoInternal(destinationPath, false, out absoluteDestination);

      string operationName = operation == FileOperation.Move ? "move" : "copy";

      if (String.Equals(absoluteSource, absoluteDestination, StringComparison.InvariantCultureIgnoreCase))
      {
        string msg = String.Format("Cannot {0} file to '{1}' - source and destination are the same.", operationName, destinationPath);
        VfsLog.Debug(msg);
        throw new ResourceAccessException(msg);
      }

      if (File.Exists(absoluteDestination))
      {
        string msg = "Cannot {0} file '{1}' to '{2}' - the destination file already exists.";
        msg = String.Format(msg, operationName, virtualFilePath, destinationPath);
        VfsLog.Debug(msg);
        throw new ResourceOverwriteException(msg);
      }


      try
      {
        switch (operation)
        {
          case FileOperation.Move:
            File.Move(absoluteSource, absoluteDestination);
            break;
          case FileOperation.Copy:
            File.Copy(absoluteSource, absoluteDestination, false);
            break;
          default:
            VfsLog.Fatal("Unsupported file operation received: {0}", operation);
            throw new ArgumentOutOfRangeException("operation");
        }

        return GetFileInfo(absoluteDestination);
      }
      catch (Exception e)
      {
        string msg = String.Format("An error occurred while trying to {0} file '{1}' to '{2}'.",
                                          operationName, virtualFilePath, destinationPath);
        VfsLog.Warn(e, msg);
        throw new ResourceAccessException(msg, e);
      }
    }


    private VirtualFolderInfo PerformFolderOperation(string virtualFolderPath, string destinationPath, FileOperation operation)
    {
      if (virtualFolderPath == null) throw new ArgumentNullException("virtualFolderPath");
      if (destinationPath == null) throw new ArgumentNullException("destinationPath");

      //get folder info for source and destination, thus validating the scope of both
      string absoluteSource;
      var sourceFolder = GetFolderInfoInternal(virtualFolderPath, true, out absoluteSource);
      string absoluteDestination;
      GetFolderInfoInternal(destinationPath, false, out absoluteDestination);

      string operationName = operation == FileOperation.Move ? "move" : "copy";

      if (sourceFolder.IsRootFolder)
      {
        string msg = String.Format("Cannot {0} root folder (attempted destination: '{1}').", operationName, destinationPath);
        VfsLog.Debug(msg);
        throw new ResourceAccessException(msg);
      }

      if (String.Equals(absoluteSource, absoluteDestination, StringComparison.InvariantCultureIgnoreCase))
      {
        string msg = String.Format("Cannot {0} folder to '{1}' - source and destination are the same.", operationName, destinationPath);
        VfsLog.Debug(msg);
        throw new ResourceAccessException(msg);
      }

      var sourceDir = new DirectoryInfo(absoluteSource);
      if (sourceDir.IsParentOf(absoluteDestination))
      {
        string msg = String.Format("Cannot {0} folder '{1}' to '{2}' - destination folder is a child of the folder.", operationName, virtualFolderPath, destinationPath);
        VfsLog.Debug(msg);
        throw new ResourceAccessException(msg);
      }

      if (Directory.Exists(absoluteDestination))
      {
        string msg = "Cannot {0} folder '{1}' to '{2}' - the destination folder already exists.";
        msg = String.Format(msg, operationName, virtualFolderPath, destinationPath);
        VfsLog.Debug(msg);
        throw new ResourceOverwriteException(msg);
      }


      try
      {
        switch(operation)
        {
          case FileOperation.Move:
            Directory.Move(absoluteSource, absoluteDestination);
            break;
          case FileOperation.Copy:
            PathUtil.CopyDirectory(absoluteSource, absoluteDestination, false);
            break;
          default:
            VfsLog.Fatal("Unsupported file operation received: {0}", operation);
            throw new ArgumentOutOfRangeException("operation");
        }

        return GetFolderInfo(absoluteDestination);
      }
      catch (Exception e)
      {
        string msg = String.Format("An error occurred while trying to {0} directory '{1}' to '{2}'.",
                                          operationName, virtualFolderPath, destinationPath);
        VfsLog.Warn(e, msg);
        throw new ResourceAccessException(msg, e);
      }
    }

	  private enum FileOperation
	  {
	    Move,
      Copy
	  }

	}
}
