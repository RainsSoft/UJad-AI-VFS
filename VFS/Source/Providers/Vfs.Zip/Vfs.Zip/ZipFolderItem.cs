using System;


namespace Vfs.Zip
{
  /// <summary>
  /// Represents a directory within a ZIP file.
  /// </summary>
  public class ZipFolderItem : VirtualFolderItem
  {
    /// <summary>
    /// The <see cref="QualifiedIdentifier"/> that is returned
    /// if this class represents the file system root.
    /// </summary>
    public const string RootIdentifier = "{ROOT}";


    /// <summary>
    /// Provides a hierarchical view on the file entry within
    /// the ZIP file.
    /// </summary>
    public ZipNode Node { get; set; }


    public ZipFolderItem()
    {
    }


    public ZipFolderItem(ZipNode folderEntry, VirtualFolderInfo virtualFolder)
    {
      Node = folderEntry;
      ResourceInfo = virtualFolder;
    }


    /// <summary>
    /// Indicates whether the resource physically exists on the file system
    /// or not.
    /// </summary>
    public override bool Exists
    {
      get { return ResourceInfo.IsRootFolder || Node.FileEntry != null; }
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
        if (Node.IsRootNode) return RootIdentifier;
        return Node.FullName;
      }
    }
  }
}
