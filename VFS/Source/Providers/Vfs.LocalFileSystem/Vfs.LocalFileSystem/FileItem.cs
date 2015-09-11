using System;
using System.IO;

namespace Vfs.LocalFileSystem
{
  /// <summary>
  /// Encapsulates information about a file on the local file system.
  /// </summary>
  public class FileItem : VirtualFileItem
  {
    /// <summary>
    /// The underlying file.
    /// </summary>
    public FileInfo LocalFile { get; set; }

    public FileItem()
    {
    }

    public FileItem(FileInfo localFile, VirtualFileInfo virtualFile)
    {
      LocalFile = localFile;
      ResourceInfo = virtualFile;
    }

    /// <summary>
    /// Indicates whether the resource physically exists on the file system
    /// or not.
    /// </summary>
    public override bool Exists
    {
      get { return LocalFile != null && File.Exists(LocalFile.FullName); } //FileInfo may not be up-to-date...
    }

    /// <summary>
    /// Gets a string that provides the fully qualified string of the resource (as opposite to the
    /// <see cref="VirtualResourceInfo.FullName"/>, which is publicly exposed to
    /// clients, e.g. in exception messages).<br/>
    /// It should be ensured that this identifier always looks the same for different requests,
    /// as it is being used for internal processes such as resource locking or auditing.
    /// </summary>
    public override string QualifiedIdentifier
    {
      get { return LocalFile == null ? String.Empty : LocalFile.FullName.ToLowerInvariant(); }
    }
  }
}
