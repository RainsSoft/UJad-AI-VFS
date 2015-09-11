using Vfs.Transfer;
using Vfs.Transfer.Upload;

namespace Vfs.Zip.Transfer
{
  public class ZipUploadTransfer : UploadTransfer<ZipFileItem>
  {
    /// <summary>
    /// The parent folder of the file to be created.
    /// </summary>
    public ZipFolderItem ParentFolder { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public ZipUploadTransfer(UploadToken token, ZipFileItem fileItem) : base(token, fileItem)
    {
    }
  }
}
