using System.IO;
using Vfs.Transfer;

namespace Vfs.LocalFileSystem.Transfer
{
  /// <summary>
  /// Encapsulates everything that's needed to manage a download
  /// transfer from the local file system.
  /// </summary>
  public class LocalDownloadTransfer : DownloadTransfer<FileItem>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public LocalDownloadTransfer(DownloadToken token, FileItem fileItem) : base(token, fileItem)
    {
    }


    /// <summary>
    /// A convenience property which returns the <see cref="FileItem.LocalFile"/>
    /// property.
    /// </summary>
    public FileInfo File
    {
      get { return FileItem.LocalFile; }
    }

    /// <summary>
    /// The processed file stream, which is created on demand.
    /// </summary>
    public FileStream Stream { get; set; }

  }
}