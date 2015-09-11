using System.Collections.Generic;


namespace Vfs
{
  /// <summary>
  /// A simple class that is used to exchange all contents
  /// (files and sub folders) of a given folder.
  /// </summary>
  public class FolderContentsInfo
  {
    /// <summary>
    /// The <see cref="VirtualResourceInfo.FullName"/> of the parent folder.
    /// </summary>
    public string ParentFolderPath { get; set; }

    /// <summary>
    /// All files of the folder.
    /// </summary>
    public IEnumerable<VirtualFileInfo> Files { get; set; }

    /// <summary>
    /// All sub folders.
    /// </summary>
    public IEnumerable<VirtualFolderInfo> Folders { get; set; }


    public FolderContentsInfo()
    {
    }


    public FolderContentsInfo(string parentFolderName, IEnumerable<VirtualFolderInfo> folders, IEnumerable<VirtualFileInfo> files)
    {
      ParentFolderPath = parentFolderName;
      Folders = folders;
      Files = files;
    }
  }
}
