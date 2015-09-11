using System.Linq;
using NUnit.Framework;


namespace Vfs.Providers.TestSuite.Browse
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_File_System_When_Querying_Resources_With_Unknown_File_Paths : TestBase
  {

    [ExpectedException(typeof(VirtualResourceNotFoundException))]
    [Test]
    public void Submitting_Unknown_File_Path_Should_Throw_Exception()
    {
      FileSystem.GetFileInfo("alsfjdasls");
    }

    [ExpectedException(typeof(VirtualResourceNotFoundException))]
    [Test]
    public void Submitting_Unknown_Folder_Path_Should_Throw_Exception()
    {
      FileSystem.GetFolderInfo("alsfjdasls");
    }


    [ExpectedException(typeof(VirtualResourceNotFoundException))]
    [Test]
    public void Requesting_File_With_Folder_Path_Should_Indicate_Unknown_File()
    {
      FileSystem.GetFileInfo(Context.DownloadFolder.MetaData.FullName);
    }


    [Test]
    public void Requesting_File_With_Root_Folder_Path_Should_Indicate_Unknown_File()
    {
      try
      {
        FileSystem.GetFileInfo(FileSystemRoot.MetaData.FullName);
        Assert.Fail("Got file when submitting root folder path.");
      }
      catch(VfsException e)
      {
        //depending on the root folder name, we might end up with one or another exception,
        //lets not be too harsh
      }
    }


    [ExpectedException(typeof(VirtualResourceNotFoundException))]
    [Test]
    public void Requesting_Folder_With_File_Path_Should_Indicate_Unknown_Folder()
    {
      FileSystem.GetFolderInfo(Context.DownloadFolder.GetFiles().First().MetaData.FullName);
    }

  }
}
