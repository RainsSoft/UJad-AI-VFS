using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

using NUnit.Framework;


namespace Vfs.LocalFileSystem.Test.Resource_Tests
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_Resource_When_Checking_Availability_on_File_System : DirectoryTestBase
  {
    
    [Test]
    public void Checking_Existing_File_Should_Work()
    {
      foreach (FileInfo file in rootDirectory.GetFiles())
      {
        var fi = provider.GetFileInfo(file.FullName);
        
        Assert.IsTrue(provider.IsFileAvailable(fi.FullName));
        Assert.IsTrue(provider.IsFileAvailable(file.FullName));
      }
    }

    [Test]
    public void Checking_Existing_Folder_Should_Work()
    {
      foreach (DirectoryInfo dir in rootDirectory.GetDirectories())
      {
        var fi = provider.GetFolderInfo(dir.FullName);

        Assert.IsTrue(provider.IsFolderAvailable(fi.FullName));
        Assert.IsTrue(provider.IsFolderAvailable(dir.FullName));
      }
    }

    [Test]
    public void Checking_Folder_With_File_Name_Should_Report_Unavailable_Resource()
    {
      foreach (FileInfo file in rootDirectory.GetFiles())
      {
        var fi = provider.GetFileInfo(file.FullName);

        Assert.IsFalse(provider.IsFolderAvailable(fi.FullName));
        Assert.IsFalse(provider.IsFolderAvailable(file.FullName));
      }
    }

    [Test]
    public void Checking_File_With_Folder_Name_Should_Report_Unavailable_Resource()
    {
      foreach (DirectoryInfo dir in rootDirectory.GetDirectories())
      {
        var fi = provider.GetFolderInfo(dir.FullName);

        Assert.IsFalse(provider.IsFileAvailable(fi.FullName));
        Assert.IsFalse(provider.IsFileAvailable(dir.FullName));
      }
    }


    [Test]
    public void Checking_For_Root_Should_Work_For_Folders_Only()
    {
      Assert.IsTrue(provider.IsFolderAvailable(rootDirectory.FullName));
      Assert.IsTrue(provider.IsFolderAvailable("/"));
      Assert.IsTrue(provider.IsFolderAvailable(""));
      Assert.IsTrue(provider.IsFolderAvailable(null));

      Assert.IsFalse(provider.IsFileAvailable(rootDirectory.FullName));
      Assert.IsFalse(provider.IsFileAvailable(("/")));
      Assert.IsFalse(provider.IsFileAvailable(""));
      Assert.IsFalse(provider.IsFileAvailable(null));
    }



    [Test]
    [ExpectedException(typeof(InvalidResourcePathException))]
    public void Submitting_Relative_Folder_Path_Should_Not_Work_With_System_Root()
    {
      provider = new LocalFileSystemProvider("ROOT");
      var status = provider.IsFolderAvailable("Foo/Bar");
    }


    [Test]
    [ExpectedException(typeof(InvalidResourcePathException))]
    public void Submitting_Relative_File_Path_Should_Not_Work_With_System_Root()
    {
      provider = new LocalFileSystemProvider("ROOT");
      var status = provider.IsFileAvailable("Foo/Bar.txt");
    }

    [Test]
    [ExpectedException(typeof(InvalidResourcePathException))]
    public void Submitting_Relative_But_Rooted_File_Path_Should_Not_Work_With_System_Root()
    {
      provider = new LocalFileSystemProvider("ROOT");
      var status = provider.IsFileAvailable("/Foo/Bar.txt");
    }


  }
}
