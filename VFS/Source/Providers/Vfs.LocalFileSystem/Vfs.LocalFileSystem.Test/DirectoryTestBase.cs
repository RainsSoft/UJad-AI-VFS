using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Vfs.LocalFileSystem.Transfer;
using Vfs.Locking;
using Vfs.Test;

namespace Vfs.LocalFileSystem.Test
{
  /// <summary>
  /// Base class for tests that operate on a test directory
  /// which consists files and folders.
  /// </summary>
  public abstract class DirectoryTestBase
  {
    protected IFileSystemProvider provider;
    protected DirectoryInfo rootDirectory;
    protected VirtualFolderInfo root;

    protected LocalFileSystemConfiguration FileSystemConfiguration { get; private set; }

    /// <summary>
    /// The used transfer store of the provider's download service.
    /// </summary>
    public TestTransferStore<LocalDownloadTransfer> DownloadTransferStore
    {
      get; set;
    }

    protected TestTransferStore<LocalUploadTransfer> UploadTransferStore { get; set; }

    public IResourceLockRepository LockRepository
    {
      get { return provider.LockRepository; }
    }


    [SetUp]
    public void Init()
    {
      rootDirectory = TestUtil.CreateTestDirectory();

      //init provider
      DownloadTransferStore = new TestTransferStore<LocalDownloadTransfer>();
      UploadTransferStore = new TestTransferStore<LocalUploadTransfer>();

      var configuration = LocalFileSystemConfiguration.CreateForRootDirectory(rootDirectory, true);
      configuration.DownloadStore = DownloadTransferStore;
      configuration.UploadStore = UploadTransferStore;
      configuration.DownloadTokenExpirationTime = TimeSpan.FromHours(24);
      configuration.UploadTokenExpirationTime = TimeSpan.FromHours(24);
      configuration.DefaultDownloadBlockSize = 32768;
      configuration.MaxDownloadBlockSize = 32768 * 2;
      configuration.MaxUploadBlockSize = 32768 * 4;

      AdjustFileSystemConfiguration(configuration);
      FileSystemConfiguration = configuration;

      provider = new LocalFileSystemProvider(FileSystemConfiguration);
      root = provider.GetFileSystemRoot();

      InitInternal();
    }


    /// <summary>
    /// Allows to make adjustments to the configuration that is being
    /// used to construct the provider.
    /// </summary>
    /// <param name="configuration">The preconfigured configuration.</param>
    protected virtual void AdjustFileSystemConfiguration(LocalFileSystemConfiguration configuration)
    {
    }


    protected virtual void InitInternal()
    {
    }


    [TearDown]
    public void Cleanup()
    {
      CleanupInternal();

      rootDirectory.Refresh();
      if (rootDirectory.Exists) rootDirectory.Delete(true);
    }


    protected virtual void CleanupInternal()
    {
    }
  }
}
