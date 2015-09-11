using System.IO;
using Vfs.Transfer;
using Vfs.Transfer.Upload;

namespace Vfs.LocalFileSystem.Transfer
{
  public class LocalUploadTransfer : UploadTransfer<FileItem>
  {
    /// <summary>
    /// The parent folder of the file to be created.
    /// </summary>
    public FolderItem ParentFolder { get; set; }

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


    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public LocalUploadTransfer(UploadToken token, FileItem fileItem)
      : base(token, fileItem)
    {
    }

  }
}
