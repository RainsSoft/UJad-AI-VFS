using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;


namespace Vfs.Providers.TestSuite.Browse
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_Resource_Requests_When_Working_With_Relative_Paths : TestBase
  {

    [Test]
    public void Root_Path_Should_Be_A_Slash()
    {
      Assert.AreEqual("/", provider.GetFileSystemRoot().FullName);
    }


    [Test]
    public void Immediate_Childs_Should_Be_Child_Names_Prefixed_By_Slashes()
    {
      foreach (var folder in provider.GetChildFolders(root))
      {
        Assert.IsTrue(folder.FullName.StartsWith("/"));
        Assert.IsFalse(folder.Name.StartsWith("/"));
      }

      foreach (var file in provider.GetChildFiles(root))
      {
        Assert.IsTrue(file.FullName.StartsWith("/"));
        Assert.IsFalse(file.Name.StartsWith("/"));
      }
    }


    [Test]
    public void Submitting_An_Empty_Or_Null_String_Should_Return_Root_Folder()
    {
      var folder = provider.GetFolderInfo("");
      Assert.AreEqual(root.Name, folder.Name);
      Assert.IsTrue(folder.IsRootFolder);
      Assert.AreEqual("/", folder.FullName);
    }


    [Test]
    public void Submitting_A_Slash_Should_Return_Root_Folder()
    {
      var folder = provider.GetFolderInfo("/");
      Assert.IsTrue(folder.IsRootFolder);
      Assert.AreEqual(root.Name, folder.Name);
      Assert.AreEqual("/", folder.FullName);
    }


    [Test]
    public void Requesting_Files_And_Folders_With_Relative_Uris_Should_Work()
    {
      var folder = VirtualFolder.CreateRootFolder(provider);

      foreach (var subFolder in folder.GetFolders())
      {
        string name = subFolder.MetaData.FullName;
        Assert.AreEqual(name, provider.GetFolderInfo(name).FullName);

        foreach (var subFile in subFolder.GetFiles())
        {
          name = subFile.MetaData.FullName;
          Assert.AreEqual(name, provider.GetFileInfo(name).FullName);
        }
      }
    }


    [Test]
    public void Submitting_A_Qualified_Path_Should_Properly_Resolve()
    {
      var file = rootDirectory.GetDirectories()[0].GetFiles()[0];
      var fileInfo = provider.GetFileInfo(file.FullName);
      Assert.AreEqual(file.Name, fileInfo.Name);

      var folder = file.Directory;
      var folderInfo = provider.GetFolderInfo(folder.FullName);
      Assert.AreEqual(folder.Name, folderInfo.Name);
    }



    [Test]
    public void Requesting_Parent_Folder_Should_Return_Resource_With_Relative_Path()
    {
      var file = rootDirectory.GetDirectories()[0].GetFiles()[0];
      var fileInfo = provider.GetFileInfo(file.FullName);

      var parentFolder = provider.GetFileParent(fileInfo);
      StringAssert.StartsWith("/", parentFolder.FullName);

      var dir = rootDirectory.GetDirectories()[0];
      var sub = dir.CreateSubdirectory("xx");
      var folderInfo = provider.GetFolderInfo(sub.FullName);
      parentFolder = provider.GetFolderParent(folderInfo.FullName);
      StringAssert.StartsWith("/", parentFolder.FullName);
    }



    [Test]
    public void Browsing_Should_Return_Resources_With_Relative_Paths()
    {
      var folders = provider.GetChildFolders(root);

      foreach (var folder in folders)
      {
        StringAssert.StartsWith("/", folder.FullName);
        StringAssert.DoesNotContain(rootDirectory.Name, folder.FullName);

        //request the resource by its path
        Assert.AreEqual(folder.FullName, provider.GetFolderInfo(folder.FullName).FullName);

        var files = provider.GetChildFiles(folder);
        foreach (var file in files)
        {
          StringAssert.StartsWith("/", file.FullName);
          StringAssert.DoesNotContain(rootDirectory.Name, file.FullName);

          //request the resource by its path
          Assert.AreEqual(file.FullName, provider.GetFileInfo(file.FullName).FullName);
        }
      }
    }



    [Test]
    public void Requesting_A_Folder_With_Qualified_Path_Should_Return_Resource_With_Relative_Path()
    {
      foreach (var dir in rootDirectory.GetDirectories())
      {
        var folder = provider.GetFolderInfo(dir.FullName);

        string substring = dir.FullName.Substring(rootDirectory.FullName.Length);
        Assert.AreEqual(substring.Length, folder.FullName.Length);
        Assert.AreEqual(dir.Name, folder.Name);

        foreach (var fi in dir.GetFiles())
        {
          var file = provider.GetFileInfo(fi.FullName);
          substring = fi.FullName.Substring(rootDirectory.FullName.Length);
          Assert.AreEqual(substring.Length, file.FullName.Length);
          Assert.AreEqual(fi.Name, file.Name);
        }
      }
    }


    [Test]
    [ExpectedException(typeof(ResourceAccessException))]
    public void Submitting_Relative_File_Paths_That_Go_Outside_The_Root_Scope_Should_Fail()
    {
      provider.GetFileInfo("../../" + "readme.txt");
    }


    [Test]
    [ExpectedException(typeof(ResourceAccessException))]
    public void Submitting_Relative_Folder_Paths_That_Go_Outside_The_Root_Scope_Should_Fail()
    {
      provider.GetFileInfo("../../" + rootDirectory.Parent.Name);
    }

    [Test]
    [ExpectedException(typeof(VirtualResourceNotFoundException))]
    public void Requesting_File_With_Folder_Path_Should_Fail()
    {
      provider.GetFileInfo("/" + rootDirectory.GetDirectories()[0].Name);
    }


    [Test]
    [ExpectedException(typeof(VirtualResourceNotFoundException))]
    public void Requesting_Folder_With_File_Path_Should_Fail()
    {
      provider.GetFolderInfo("/" + rootDirectory.GetFiles()[0].Name);
    }


  }
}
