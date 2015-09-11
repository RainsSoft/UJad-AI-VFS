using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vfs
{
  /// <summary>
  /// A simple class that is used to exchange all contents
  /// (files and sub folders) of a given folder.
  /// </summary>
  public class FolderContents
  {
        /// <summary>
    /// The <see cref="VirtualResourceInfo.FullName"/> of the parent folder.
    /// </summary>
    public string ParentFolderPath { get; set; }

    /// <summary>
    /// All files of the folder.
    /// </summary>
    public IEnumerable<VirtualFile> Files { get; set; }

    /// <summary>
    /// All sub folders.
    /// </summary>
    public IEnumerable<VirtualFolder> Folders { get; set; }


    public FolderContents()
    {
    }


    public FolderContents(string parentFolderName, IEnumerable<VirtualFolder> folders, IEnumerable<VirtualFile> files)
    {
      ParentFolderPath = parentFolderName;
      Folders = folders;
      Files = files;
    }
  }
}
