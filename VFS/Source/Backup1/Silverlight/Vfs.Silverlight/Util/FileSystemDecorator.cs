using System;
using System.Collections.Generic;
using System.IO;
using Vfs.Auditing;
using Vfs.Locking;
using Vfs.Security;
using Vfs.Transfer;

namespace Vfs.Util
{
  /// <summary>
  /// Provides a base class to extend a given <see cref="IFileSystemProvider"/>
  /// implementation in order to process or intercept FS-related
  /// calls.
  /// </summary>
  public class FileSystemDecorator : FileSystemProviderBase
  {
    private IFileSystemProvider decoratedFileSystem;

    /// <summary>
    /// The decorated file system.
    /// </summary>
    public IFileSystemProvider DecoratedFileSystem
    {
      get { return decoratedFileSystem; }
      set
      {
        if (value == null) throw new ArgumentNullException("value");
        decoratedFileSystem = value;
      }
    }


    /// <summary>
    /// Initializes the class with the <see cref="IFileSystemProvider"/>
    /// to be decorated.
    /// </summary>
    /// <param name="decorated">The decorated file system.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="decorated"/>
    /// is a null reference.</exception>
    public FileSystemDecorator(IFileSystemProvider decorated)
    {
      if (decorated == null) throw new ArgumentNullException("decorated");
      DecoratedFileSystem = decorated;
    }

    public override IAuditor Auditor
    {
      get { return DecoratedFileSystem.Auditor; }
      set { DecoratedFileSystem.Auditor = value; }
    }

    public override IResourceLockRepository LockRepository
    {
      get { return DecoratedFileSystem.LockRepository; }
      set { DecoratedFileSystem.LockRepository = value; }
    }

    public override IFileSystemSecurity Security
    {
      get { return DecoratedFileSystem.Security; }
      set { DecoratedFileSystem.Security = value; }
    }

    public override IDownloadTransferHandler DownloadTransfers
    {
      get { return DecoratedFileSystem.DownloadTransfers; }
    }

    public override IUploadTransferHandler UploadTransfers
    {
      get { return DecoratedFileSystem.UploadTransfers; }
    }

    public override VirtualFolderInfo GetFileSystemRoot()
    {
      return DecoratedFileSystem.GetFileSystemRoot();
    }

    public override Stream ReadFileContents(string virtualFilePath)
    {
      return DecoratedFileSystem.ReadFileContents(virtualFilePath);
    }

    public override VirtualFileInfo GetFileInfo(string virtualFilePath)
    {
      return DecoratedFileSystem.GetFileInfo(virtualFilePath);
    }

    public override VirtualFolderInfo GetFolderInfo(string virtualFolderPath)
    {
      return DecoratedFileSystem.GetFolderInfo(virtualFolderPath);
    }

    public override VirtualFolderInfo GetFileParent(string childFilePath)
    {
      return DecoratedFileSystem.GetFileParent(childFilePath);
    }

    public override VirtualFolderInfo GetFolderParent(string childFolderPath)
    {
      return DecoratedFileSystem.GetFolderParent(childFolderPath);
    }

    public override IEnumerable<VirtualFolderInfo> GetChildFolders(string parentFolderPath)
    {
      return DecoratedFileSystem.GetChildFolders(parentFolderPath);
    }

    public override IEnumerable<VirtualFolderInfo> GetChildFolders(string parentFolderPath, string searchPattern)
    {
      return DecoratedFileSystem.GetChildFolders(parentFolderPath, searchPattern);
    }

    public override IEnumerable<VirtualFileInfo> GetChildFiles(string parentFolderPath)
    {
      return DecoratedFileSystem.GetChildFiles(parentFolderPath);
    }

    public override IEnumerable<VirtualFileInfo> GetChildFiles(string parentFolderPath, string searchPattern)
    {
      return DecoratedFileSystem.GetChildFiles(parentFolderPath, searchPattern);
    }

    public override FolderContentsInfo GetFolderContents(string parentFolderPath)
    {
      return DecoratedFileSystem.GetFolderContents(parentFolderPath);
    }

    public override FolderContentsInfo GetFolderContents(string parentFolderPath, string searchPattern)
    {
      return DecoratedFileSystem.GetFolderContents(parentFolderPath, searchPattern);
    }

    public override bool IsFileAvailable(string virtualFilePath)
    {
      return DecoratedFileSystem.IsFileAvailable(virtualFilePath);
    }

    public override bool IsFolderAvailable(string virtualFolderPath)
    {
      return DecoratedFileSystem.IsFolderAvailable(virtualFolderPath);
    }

    public override VirtualFolderInfo CreateFolder(string virtualFolderPath)
    {
      return DecoratedFileSystem.CreateFolder(virtualFolderPath);
    }

    public override VirtualFileInfo WriteFile(string virtualFilePath, Stream input, bool overwrite, long resourceLength, string contentType)
    {
      return DecoratedFileSystem.WriteFile(virtualFilePath, input, overwrite, resourceLength, contentType);
    }

    public override void DeleteFolder(string virtualFolderPath)
    {
      DecoratedFileSystem.DeleteFolder(virtualFolderPath);
    }

    public override void DeleteFile(string virtualFilePath)
    {
      DecoratedFileSystem.DeleteFile(virtualFilePath);
    }

    public override VirtualFolderInfo MoveFolder(string virtualFolderPath, string destinationPath)
    {
      return DecoratedFileSystem.MoveFolder(virtualFolderPath, destinationPath);
    }

    public override VirtualFileInfo MoveFile(string virtualFilePath, string destinationPath)
    {
      return DecoratedFileSystem.MoveFile(virtualFilePath, destinationPath);
    }

    public override VirtualFolderInfo CopyFolder(string virtualFolderPath, string destinationPath)
    {
      return DecoratedFileSystem.CopyFolder(virtualFolderPath, destinationPath);
    }

    public override VirtualFileInfo CopyFile(string virtualFilePath, string destinationPath)
    {
      return DecoratedFileSystem.CopyFile(virtualFilePath, destinationPath);
    }

    public override string CreateFilePath(string parentFolder, string fileName)
    {
      return DecoratedFileSystem.CreateFilePath(parentFolder, fileName);
    }

    public override string CreateFolderPath(string parentFolder, string folderName)
    {
      return DecoratedFileSystem.CreateFolderPath(parentFolder, folderName);
    }
  }
}