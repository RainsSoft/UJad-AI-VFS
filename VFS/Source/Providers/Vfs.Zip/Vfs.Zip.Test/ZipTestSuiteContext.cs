using System;
using System.IO;
using Ionic.Zip;
using Vfs.Scheduling;
using Vfs.Test;
using Vfs.Util;
using Vfs.Util.TemporaryStorage;
using Vfs.Zip.Transfer;

namespace Vfs.Zip.Test
{
  public class ZipTestSuiteContext : TestContext
  {
    protected ZipFileProvider Provider { get; set; }

    public string ZipFilePath { get; set; }

    /// <summary>
    /// Returns a file system implementation, which is assigned
    /// to the <see cref="TestContext.FileSystem"/> property.
    /// </summary>
    /// <param name="localTestFolder">The temporary test folder that is
    /// used by the context. This is actually just the <see cref="TestContext.LocalTestRoot"/>
    /// reference.</param>
    protected override IFileSystemProvider InitFileSystem(DirectoryInfo localTestFolder)
    {
      //init empty zip file
      ZipFilePath = Path.Combine(localTestFolder.FullName, "TestArchive.zip");
      var zip = new ZipFile(ZipFilePath);
      zip.Save();
      zip.Dispose();

      //init transfer stores
      var downloadTransferStore = new TestTransferStore<ZipDownloadTransfer>();
      var uploadTransferStore = new TestTransferStore<ZipUploadTransfer>();

      //init configuration
      var tempFactory = new TempFileStreamFactory(LocalTestRoot.FullName);

      var zipFile = new FileInfo(ZipFilePath);

      var configuration = new ZipFileSystemConfiguration(zipFile, tempFactory);
      configuration.RootName = "Test Root";
      configuration.DownloadStore = downloadTransferStore;
      configuration.UploadStore = uploadTransferStore;
      configuration.DownloadTokenExpirationTime = TimeSpan.FromHours(24);
      configuration.UploadTokenExpirationTime = TimeSpan.FromHours(24);
      configuration.DefaultDownloadBlockSize = 32768;
      configuration.MaxDownloadBlockSize = 32768 * 2;
      configuration.MaxUploadBlockSize = 32768 * 4;
      configuration.MaxUploadFileSize = (long)1024 * 1024 * 2048; //2GB limit



      //create provider
      Provider = new ZipFileProvider(configuration);
      return Provider;
    }

    /// <summary>
    /// Should provide cleanup code that is specific to the
    /// file system under test.
    /// </summary>
    protected override void CleanupFileSystem()
    {
      Provider.Dispose();
    }

    /// <summary>
    /// Gets the expiration scheduler that is used to determine
    /// download expirations. Returns null in case this is not
    /// possible.
    /// </summary>
    public override Scheduler TryGetDownloadExpirationScheduler()
    {
      return ((ZipDownloadHandler)Provider.DownloadTransfers).ExpirationScheduler;
    }

    /// <summary>
    /// Gets the expiration scheduler that is used to determine
    /// upload expirations. Returns null in case this is not
    /// possible.
    /// </summary>
    public override Scheduler TryGetUploadExpirationScheduler()
    {
      return ((ZipUploadHandler)Provider.DownloadTransfers).ExpirationScheduler;
    }
  }
}
