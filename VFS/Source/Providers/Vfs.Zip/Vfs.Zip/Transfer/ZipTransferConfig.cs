using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vfs.Auditing;
using Vfs.Locking;
using Vfs.Security;

namespace Vfs.Zip.Transfer
{
  /// <summary>
  /// Encapsulates configuration for transfer services.
  /// </summary>
  public class ZipTransferConfig
  {
    /// <summary>
    /// The file system configuration.
    /// </summary>
    public ZipFileSystemConfiguration FileSystemConfiguration { get; set; }

    /// <summary>
    /// The parent provider that uses the services.
    /// </summary>
    public ZipFileProvider Provider { get; set; }

    /// <summary>
    /// Gets the ZIP file repository of the provider.
    /// </summary>
    public ZipNodeRepository Repository { get; set; }

    /// <summary>
    /// A function that resolves a resource ID into a file item.
    /// </summary>
    public Func<string, bool, FileSystemTask, ZipFileItem> FileResolverFunc { get; set; }

    /// <summary>
    /// A function that returns the permissions for a given file resource.
    /// </summary>
    public Func<ZipFileItem, FileClaims> ClaimsResolverFunc { get; set; }

    /// <summary>
    /// A function that resolver the permissions for a given folder.
    /// </summary>
    public Func<ZipFolderItem, FolderClaims> FolderClaimsResolverFunc { get; set; }

    /// <summary>
    /// A function that returns a read or write lock for a given file resource.
    /// </summary>
    public Func<ZipFileItem, ResourceLockType, ResourceLockGuard> LockResolverFunc { get; set; }

    /// <summary>
    /// A function that resolves the parent folder item of a given file.
    /// </summary>
    public Func<ZipFileItem, FileSystemTask, ZipFolderItem> FileParentResolverFunc { get; set; }
  }
}
