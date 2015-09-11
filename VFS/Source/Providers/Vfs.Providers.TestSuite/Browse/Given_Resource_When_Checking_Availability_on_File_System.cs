using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

using NUnit.Framework;
using Vfs.LocalFileSystem.Test;


namespace Vfs.Providers.TestSuite.Browse
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_Resource_When_Checking_Availability_on_File_System : TestBase
  {
    
    [Test]
    public void Checking_Existing_File_Should_Work()
    {
      var files = Context.DownloadFolder.GetFiles();
      CollectionAssert.IsNotEmpty(files);

      foreach (var file in files)
      {
        Assert.IsTrue(FileSystem.IsFileAvailable(file.MetaData.FullName));
      }
    }



    [Test]
    public void Checking_Existing_Folder_Should_Work()
    {
      var folders = FileSystemRoot.GetFolders();
      CollectionAssert.IsNotEmpty(folders);

      foreach (var folder in folders)
      {
        Assert.IsTrue(FileSystem.IsFolderAvailable(folder.MetaData.FullName));
      }
    }


    [Test]
    public void Checking_Folder_With_File_Name_Should_Report_Unavailable_Resource()
    {
      var files = Context.DownloadFolder.GetFiles();
      CollectionAssert.IsNotEmpty(files);

      foreach (var file in files)
      {
        Assert.IsFalse(FileSystem.IsFolderAvailable(file.MetaData.FullName));
      }
    }

    [Test]
    public void Checking_File_With_Folder_Name_Should_Report_Unavailable_Resource()
    {
      var folders = FileSystemRoot.GetFolders();
      CollectionAssert.IsNotEmpty(folders);

      foreach (var folder in folders)
      {
        Assert.IsFalse(FileSystem.IsFileAvailable(folder.MetaData.FullName));
      }
    }


    [Test]
    public void Checking_For_Root_Should_Work_For_Folders_Only()
    {
      Assert.IsTrue(FileSystem.IsFolderAvailable(FileSystemRoot.MetaData.FullName));
      Assert.IsFalse(FileSystem.IsFileAvailable(FileSystemRoot.MetaData.FullName));
    }

  }
}
