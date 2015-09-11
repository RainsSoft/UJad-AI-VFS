using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Vfs.Transfer.Util;
using Vfs.Util.TemporaryStorage;
using Vfs.Zip.Transfer;

namespace Vfs.Zip
{
  public class ZipFileSystemConfiguration : FileSystemConfiguration<ZipDownloadTransfer, ZipUploadTransfer>
  {
    /// <summary>
    /// The file that is exposed through the provider.
    /// </summary>
    public FileInfo ZipFileInfo { get; set; }

    /// <summary>
    /// Creates <see cref="TempStream"/> items on demand.
    /// </summary>
    public ITempStreamFactory TempStreamFactory { get; set; }


    public ZipFileSystemConfiguration(FileInfo zipFileInfo, ITempStreamFactory tempStreamFactory)
    {
      Ensure.ArgumentNotNull(zipFileInfo, "zipFileInfo");
      Ensure.ArgumentNotNull(tempStreamFactory, "tempStreamFactory");

      ZipFileInfo = zipFileInfo;
      TempStreamFactory = tempStreamFactory;
    }
  }
}
