using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using NUnit.Framework;


namespace Vfs.Providers.TestSuite.Operations.Delete
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_File_System_When_Removing_Resources : TestBase
  {

    [Test]
    public void Removing_Virtual_File_Should_Delete_Physical_File()
    {
      //delete from root with qualified path
      var files = Context.DownloadFolder.GetFiles();
      foreach (var file in files)
      {
        file.Delete();
        Assert.IsFalse(FileSystem.IsFileAvailable(file.MetaData.FullName));
      }

      Context.DownloadFolder.RefreshMetaData();
      files = Context.DownloadFolder.GetFiles();
      CollectionAssert.IsEmpty(files);
    }



    [Test]
    public void Removing_Folder_Should_Delete_Directory()
    {
      var f1 = Context.EmptyFolder.AddFolder("one");
      var f2 = Context.EmptyFolder.AddFolder("two");

      Context.EmptyFolder.RefreshMetaData();
      FileSystem.DeleteFolder(f2.MetaData.FullName);
      Assert.AreEqual("one", Context.EmptyFolder.GetFolders().Single().MetaData.Name);
    }



    [Test]
    [ExpectedException(typeof(VirtualResourceNotFoundException))]
    public void Requesting_Deletion_Of_Unknown_Folder_Should_Fail()
    {
      var f1 = Context.EmptyFolder.AddFolder("one");
      var f2 = Context.EmptyFolder.AddFolder("two");

      f2.Delete();
      f2.Delete();
    }



    [Test]
    [ExpectedException(typeof(VirtualResourceNotFoundException))]
    public void Requesting_Folder_Deletion_With_File_Path_Should_Fail()
    {
      var file = Context.DownloadFolder.GetFiles().Last().MetaData;
      FileSystem.DeleteFolder(file.FullName);
    }


    [Test]
    [ExpectedException(typeof(VirtualResourceNotFoundException))]
    public void Requesting_File_Deletion_With_Folder_Path_Should_Fail()
    {
      var folder = Context.DownloadFolder.AddFolder("xxx");
      FileSystem.DeleteFile(folder.MetaData.FullName);
    }




    [Test]
    [ExpectedException(typeof(VirtualResourceNotFoundException))]
    public void Requesting_Deletion_Of_Unknown_File_Should_Fail()
    {
      var f1 = Context.DownloadFolder.GetFiles().First();
      f1.Delete();
      f1.Delete();
    }



    [Test]
    public void Requestion_Deletion_Of_Root_Directory_Should_Fail()
    {
      try
      {
        Assert.Inconclusive("Write custom test in order to verify deletion of root folder - too dangerous as part of test suite.");
        //FileSystem.DeleteFolder(FileSystemRoot.MetaData.FullName);
      }
      catch (ResourceAccessException e)
      {
        StringAssert.Contains("root", e.Message.ToLower());
      }
    }

  }
}
