using NUnit.Framework;
using Vfs.LocalFileSystem;
using Vfs.Util;

namespace Vfs.Restful.Test.Download
{
  [TestFixture]
  public class Given_Download_When_Requesting_Max_Block_Size : DownloadTestBase
  {
    private FileSystemDecorator decorator;


    protected override IFileSystemProvider GetServiceProvider()
    {
      //inject a decorator into OpenRasta - allows us to replace the
      //processing provider at runtime
      var provider = base.GetServiceProvider();
      decorator = new FileSystemDecorator(provider);
      return decorator;
    }

    private void OverwriteSettings(int? providerBlockSize, int? serviceBlockSize)
    {
      //overwrite file system
      LocalFileSystemConfiguration config = LocalFileSystemConfiguration.CreateForRootDirectory(RootDirectory, true);
      config.MaxDownloadBlockSize = providerBlockSize;
      decorator.DecoratedFileSystem = new LocalFileSystemProvider(config);

      //adjust settings
      ServiceSettings.MaxDownloadBlockSize = serviceBlockSize;
    }


    [Test]
    public void Block_Size_Should_Not_Be_Limited_If_Neither_Provider_Nor_Server_Specify_Maximum()
    {
      OverwriteSettings(null, null);
      Assert.IsNull(ClientDownloads.MaxBlockSize);
    }

    [Test]
    public void Block_Size_Should_Be_Taken_From_Provider_If_Settings_Have_No_Maximum()
    {
      OverwriteSettings(60000, null);
      Assert.AreEqual(60000, ClientDownloads.MaxBlockSize);
    }

    [Test]
    public void Block_Size_Should_Be_Taken_From_Settings_If_Provider_Has_No_Maximum()
    {
      OverwriteSettings(null, 75000);
      Assert.AreEqual(75000, ClientDownloads.MaxBlockSize);
    }


    [Test]
    public void Block_Size_Should_Be_Taken_From_Provider_If_Smaller_Then_Service_Maximum()
    {
      OverwriteSettings(60000, 100000);
      Assert.AreEqual(60000, ClientDownloads.MaxBlockSize);
    }


    [Test]
    public void Block_Size_Should_Be_Taken_From_Service_Settings_If_Smaller_Then_Provider_Maximum()
    {
      OverwriteSettings(90000, 80000);
      Assert.AreEqual(80000, ClientDownloads.MaxBlockSize);
    }

  }
}
