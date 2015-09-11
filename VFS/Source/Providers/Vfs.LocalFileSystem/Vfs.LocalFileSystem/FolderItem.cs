using System;
using System.IO;

namespace Vfs.LocalFileSystem
{
  /// <summary>
  /// Encapsulates information about a folder on the local file system.
  /// </summary>
  public class FolderItem : VirtualFolderItem
  {
    /// <summary>
    /// The underlying folder.
    /// </summary>
    public DirectoryInfo LocalDirectory { get; set; }

    /// <summary>
    /// The <see cref="QualifiedIdentifier"/> that is returned
    /// if this class represents the file system root.
    /// </summary>
    public const string RootIdentifier = "{ROOT}";

    public FolderItem()
    {
    }

    public FolderItem(DirectoryInfo localDirectory, VirtualFolderInfo virtualFolder)
    {
      LocalDirectory = localDirectory;
      ResourceInfo = virtualFolder;
    }

    /// <summary>
    /// Indicates whether the resource physically exists on the file system
    /// or not.
    /// </summary>
    public override bool Exists
    {
      get { return ResourceInfo.IsRootFolder || LocalDirectory != null && LocalDirectory.Exists; }
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
      get
      {
        if (ResourceInfo.IsRootFolder) return RootIdentifier;
        return LocalDirectory == null ? String.Empty : LocalDirectory.FullName.ToLowerInvariant();
      }
    }
  }
}
