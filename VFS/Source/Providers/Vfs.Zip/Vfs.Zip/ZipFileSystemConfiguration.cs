using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Vfs.Transfer;
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

    public static ZipFileSystemConfiguration CreateDefaultConfig(string zipfile,string tempDir) {
        //init transfer stores
        InMemoryTransferStore<ZipDownloadTransfer> downloadTransferStore = new InMemoryTransferStore<ZipDownloadTransfer>();
        InMemoryTransferStore<ZipUploadTransfer> uploadTransferStore = new InMemoryTransferStore<ZipUploadTransfer>();

        //init configuration
        var tempFactory = new TempFileStreamFactory(tempDir);
        var configuration = new ZipFileSystemConfiguration(new FileInfo(zipfile), tempFactory);

        configuration.DownloadStore = downloadTransferStore;
        configuration.UploadStore = uploadTransferStore;
        configuration.DownloadTokenExpirationTime = TimeSpan.FromHours(24);
        configuration.UploadTokenExpirationTime = TimeSpan.FromHours(24);
        configuration.DefaultDownloadBlockSize = 32768;
        configuration.MaxDownloadBlockSize = 32768 * 2;
        configuration.MaxUploadBlockSize = 32768 * 4;
        configuration.MaxUploadFileSize = (long)1024 * 1024 * 2048; //2GB limit
        //
        return configuration;
    }

  }
}
