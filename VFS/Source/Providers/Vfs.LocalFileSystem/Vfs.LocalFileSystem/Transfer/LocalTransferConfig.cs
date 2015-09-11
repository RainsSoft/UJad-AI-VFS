using System;
using Vfs.Auditing;
using Vfs.Locking;
using Vfs.Security;

namespace Vfs.LocalFileSystem.Transfer
{
  /// <summary>
  /// Encapsulates configuration for transfer services.
  /// </summary>
  public class LocalTransferConfig
  {
    /// <summary>
    /// The file system configuration.
    /// </summary>
    public LocalFileSystemConfiguration FileSystemConfiguration { get; set; }

    /// <summary>
    /// The parent provider that uses the services.
    /// </summary>
    public LocalFileSystemProvider Provider { get; set; }

    /// <summary>
    /// A function that resolves a resource ID into a file item.
    /// </summary>
    public Func<string, bool, FileSystemTask, FileItem> FileResolverFunc { get; set; }

    /// <summary>
    /// A function that returns the permissions for a given file resource.
    /// </summary>
    public Func<FileItem, FileClaims> ClaimsResolverFunc { get; set; }

    /// <summary>
    /// A function that resolver the permissions for a given folder.
    /// </summary>
    public Func<FolderItem, FolderClaims> FolderClaimsResolverFunc { get; set; }

    /// <summary>
    /// A function that returns a read or write lock for a given file resource.
    /// </summary>
    public Func<FileItem, ResourceLockType, ResourceLockGuard> LockResolverFunc { get; set; }

    /// <summary>
    /// A function that resolves the parent folder item of a given file.
    /// </summary>
    public Func<FileItem, FileSystemTask, FolderItem> FileParentResolverFunc { get; set; }
  }
}
