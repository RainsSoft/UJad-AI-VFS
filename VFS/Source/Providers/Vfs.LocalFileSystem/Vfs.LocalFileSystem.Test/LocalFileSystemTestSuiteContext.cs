using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Hardcodet.Commons.IO;
using Vfs.LocalFileSystem.Transfer;
using Vfs.Scheduling;
using Vfs.Test;

namespace Vfs.LocalFileSystem.Test
{
  public class LocalFileSystemTestSuiteContext : TestContext
  {
    /// <summary>
    /// Returns a file system implementation, which is assigned
    /// to the <see cref="TestContext.FileSystem"/> property.
    /// </summary>
    /// <param name="localTestFolder">The temporary test folder that is
    /// used by the context. This is actually just the <see cref="TestContext.LocalTestRoot"/>
    /// reference.</param>
    protected override IFileSystemProvider InitFileSystem(DirectoryInfo localTestFolder)
    {
      //use independent test folder
      var rootDirectory = FileUtil.CreateTempFolder("_Vfs.Local.TestRepository");

      //init provider
      var downloadTransferStore = new TestTransferStore<LocalDownloadTransfer>();
      var uploadTransferStore = new TestTransferStore<LocalUploadTransfer>();

      var configuration = LocalFileSystemConfiguration.CreateForRootDirectory(rootDirectory, true);
      configuration.DownloadStore = downloadTransferStore;
      configuration.UploadStore = uploadTransferStore;
      configuration.DownloadTokenExpirationTime = TimeSpan.FromHours(24);
      configuration.UploadTokenExpirationTime = TimeSpan.FromHours(24);
      configuration.DefaultDownloadBlockSize = 32768;
      configuration.MaxDownloadBlockSize = 32768 * 2;
      configuration.MaxUploadBlockSize = 32768 * 4;
      configuration.MaxUploadFileSize = (long)1024*1024*2048; //2GB limit

      return new LocalFileSystemProvider(configuration);
    }


    /// <summary>
    /// Should provide cleanup code that is specific to the
    /// file system under test.
    /// </summary>
    protected override void CleanupFileSystem()
    {
      var provider = (LocalFileSystemProvider)FileSystem;
      if (provider.RootDirectory == null) return;
        
      provider.RootDirectory.Refresh();
      if (provider.RootDirectory.Exists)
      {
        provider.RootDirectory.Delete(true);
      }
    }

    /// <summary>
    /// Gets the expiration scheduler that is used to determine
    /// download expirations. Returns null in case this is not
    /// possible.
    /// </summary>
    public override Scheduler TryGetDownloadExpirationScheduler()
    {
      return ((LocalDownloadHandler) FileSystem.DownloadTransfers).ExpirationScheduler;
    }

    /// <summary>
    /// Gets the expiration scheduler that is used to determine
    /// upload expirations. Returns null in case this is not
    /// possible.
    /// </summary>
    public override Scheduler TryGetUploadExpirationScheduler()
    {
      return ((LocalUploadHandler)FileSystem.UploadTransfers).ExpirationScheduler;
    }
  }
}
