using System;
using System.IO;
using Hardcodet.Commons.IO;
using OpenRasta.Hosting.HttpListener;
using Vfs.LocalFileSystem;
using Vfs.LocalFileSystem.Transfer;
using Vfs.Restful.Client;
using Vfs.Restful.Server;
using Vfs.Scheduling;
using Vfs.Test;

namespace Vfs.Restful.Test
{
  public class RestfulFacadeTestSuiteContext : TestContext
  {
    public HttpListenerHost ServiceHost { get; set; }
    public IFileSystemProvider ServiceFileSystem { get; set; }
    public string ServiceBaseUri { get; private set; }


    /// <summary>
    /// Returns a file system implementation, which is assigned
    /// to the <see cref="TestContext.FileSystem"/> property.
    /// </summary>
    /// <param name="localTestFolder">The temporary test folder that is
    /// used by the context. This is actually just the <see cref="TestContext.LocalTestRoot"/>
    /// reference.</param>
    protected override IFileSystemProvider InitFileSystem(DirectoryInfo localTestFolder)
    {
      ServiceFileSystem = GetServiceProvider();
      var settings = new VfsServiceSettings();

      ServiceHost = new TestServiceHost { Configuration = new TestConfiguration(ServiceFileSystem, settings) };

      ServiceBaseUri = "http://localhost:33456/";
      ServiceHost.Initialize(new[] { ServiceBaseUri }, "/", null);
      ServiceHost.StartListening();

//      //TODO remove debug code
//      ServiceBaseUri = "http://127.0.0.1:56789/";

      return new FileSystemFacade(ServiceBaseUri);
    }


    /// <summary>
    /// Creates file system provider that is managed on the
    /// server side. This is just a <see cref="LocalFileSystemProvider"/>
    /// which manages an empty temp directory.
    /// </summary>
    /// <returns></returns>
    protected virtual IFileSystemProvider GetServiceProvider()
    {
      var root = FileUtil.CreateTempFolder("_Vfs.Restful.Server.Test");

      var configuration = LocalFileSystemConfiguration.CreateForRootDirectory(root, true);

      var downloadTransferStore = new TestTransferStore<LocalDownloadTransfer>();
      var uploadTransferStore = new TestTransferStore<LocalUploadTransfer>();
      configuration.DownloadStore = downloadTransferStore;
      configuration.UploadStore = uploadTransferStore;
      configuration.DownloadTokenExpirationTime = TimeSpan.FromHours(24);
      configuration.UploadTokenExpirationTime = TimeSpan.FromHours(24);
      configuration.DefaultDownloadBlockSize = 32768;
      configuration.MaxDownloadBlockSize = 32768 * 2;
      configuration.MaxUploadBlockSize = 32768 * 4;
      configuration.MaxUploadFileSize = (long)1024 * 1024 * 2048; //2GB limit

      return new LocalFileSystemProvider(configuration);
    }


    /// <summary>
    /// Should provide cleanup code that is specific to the
    /// file system under test.
    /// </summary>
    protected override void CleanupFileSystem()
    {
      ServiceHost.StopListening();

      //clean up service file system's temp folder
      var fs = (LocalFileSystemProvider)ServiceFileSystem;
      if (fs.RootDirectory == null) return;

      fs.RootDirectory.Refresh();
      if (fs.RootDirectory.Exists)
      {
        fs.RootDirectory.Delete(true);
      }
    }


    /// <summary>
    /// Gets the expiration scheduler that is used to determine
    /// download expirations. Returns null in case this is not
    /// possible.
    /// </summary>
    public override Scheduler TryGetDownloadExpirationScheduler()
    {
      //return the server-side scheduler
      return ((LocalDownloadHandler)ServiceFileSystem.DownloadTransfers).ExpirationScheduler;
    }

    /// <summary>
    /// Gets the expiration scheduler that is used to determine
    /// upload expirations. Returns null in case this is not
    /// possible.
    /// </summary>
    public override Scheduler TryGetUploadExpirationScheduler()
    {
      return ((LocalUploadHandler)ServiceFileSystem.UploadTransfers).ExpirationScheduler;
    }
  }
}
