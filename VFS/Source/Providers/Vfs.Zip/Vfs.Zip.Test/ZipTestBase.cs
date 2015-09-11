using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Vfs.Locking;
using Vfs.Test;
using Vfs.Util;
using Vfs.Util.TemporaryStorage;
using Vfs.Zip.Test.Properties;
using Vfs.Zip.Transfer;

namespace Vfs.Zip.Test
{
  public abstract class ZipTestBase
  {
    protected ZipFileProvider Provider { get; set; }
    protected DirectoryInfo TempDirectory { get; set; }
    protected FileInfo TestZipFile { get; set; }

    protected ZipFileSystemConfiguration FileSystemConfiguration { get; private set; }

    /// <summary>
    /// The used transfer store of the provider's download service.
    /// </summary>
    protected TestTransferStore<ZipDownloadTransfer> DownloadTransferStore { get; set; }

    /// <summary>
    /// The used transfer store of the provider's upload service.
    /// </summary>
    protected TestTransferStore<ZipUploadTransfer> UploadTransferStore { get; set; }


    public IResourceLockRepository LockRepository
    {
      get { return Provider.LockRepository; }
    }


    [SetUp]
    public void Init()
    {
      //init temp folder
      TempDirectory = TestUtil.CreateTestDirectory();

      //create copy of test ZIP file within temp folder
      string path = Path.Combine(TempDirectory.FullName, "TestFile.zip");
      File.WriteAllBytes(path, Resources.TestFile);
      TestZipFile = new FileInfo(path);

      //init transfer stores
      DownloadTransferStore = new TestTransferStore<ZipDownloadTransfer>();
      UploadTransferStore = new TestTransferStore<ZipUploadTransfer>();

      //init configuration
      var tempFactory = new TempFileStreamFactory(TempDirectory.FullName);
      var configuration = new ZipFileSystemConfiguration(TestZipFile, tempFactory);

      configuration.RootName = "Test Root";
      configuration.DownloadStore = DownloadTransferStore;
      configuration.UploadStore = UploadTransferStore;
      configuration.DownloadTokenExpirationTime = TimeSpan.FromHours(24);
      configuration.UploadTokenExpirationTime = TimeSpan.FromHours(24);
      configuration.DefaultDownloadBlockSize = 32768;
      configuration.MaxDownloadBlockSize = 32768 * 2;
      configuration.MaxUploadBlockSize = 32768 * 4;

      AdjustFileSystemConfiguration(configuration);
      FileSystemConfiguration = configuration;

      //create provider
      Provider = new ZipFileProvider(FileSystemConfiguration);

      InitInternal();
    }


    /// <summary>
    /// Allows to make adjustments to the configuration that is being
    /// used to construct the provider.
    /// </summary>
    /// <param name="configuration">The preconfigured configuration.</param>
    protected virtual void AdjustFileSystemConfiguration(ZipFileSystemConfiguration configuration)
    {
    }


    protected virtual void InitInternal()
    {
    }


    [TearDown]
    public void Cleanup()
    {
      CleanupInternal();

      Provider.Dispose();
      TempDirectory.Refresh();
      if (TempDirectory.Exists) TempDirectory.Delete(true);
    }


    protected virtual void CleanupInternal()
    {
    }
  }
}
