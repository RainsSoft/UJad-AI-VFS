using Vfs.Transfer;

namespace Vfs.Zip.Transfer
{
  /// <summary>
  /// Manages downloads from a given ZIP file.
  /// </summary>
  public class ZipDownloadTransfer : DownloadTransfer<ZipFileItem>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public ZipDownloadTransfer(DownloadToken token, ZipFileItem fileItem) : base(token, fileItem)
    {
    }
  }
}
