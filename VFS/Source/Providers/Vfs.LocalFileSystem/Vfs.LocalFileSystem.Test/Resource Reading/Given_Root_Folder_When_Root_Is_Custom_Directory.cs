using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Hardcodet.Commons.IO;
using NUnit.Framework;


namespace Vfs.LocalFileSystem.Test.Resource_Tests
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_Root_Folder_When_Root_Is_Custom_Directory : DirectoryTestBase
  {

    [Test]
    public void Root_Folder_Name_Should_Match_Directory_Name()
    {
      Assert.AreEqual(root.Name, rootDirectory.Name);
    }


    [Test]
    public void Custom_Root_Name_Should_Be_Returned_If_Set()
    {
      var configuration = LocalFileSystemConfiguration.CreateForRootDirectory(rootDirectory, false);
      configuration.RootName = "FOO";

      provider = new LocalFileSystemProvider(configuration);
      root = provider.GetFileSystemRoot();

      Assert.AreEqual("FOO", root.Name);
      Assert.AreEqual(rootDirectory.FullName, root.FullName);
    }


    [Test]
    public void Root_Folder_Should_Point_To_Defined_Root_Directory_And_Provider_Root_Flag()
    {
      provider = new LocalFileSystemProvider(rootDirectory, false);
      root = provider.GetFileSystemRoot();

      Assert.IsTrue(root.IsRootFolder);
      Assert.AreEqual(root.Name, rootDirectory.Name);
      Assert.AreEqual(rootDirectory.FullName, root.FullName);
    }


    [Test]
    [ExpectedException(typeof(DirectoryNotFoundException))]
    public void Creating_Provider_With_Inexisting_Root_Directory_Should_Fail()
    {
      rootDirectory.Delete(true);
      rootDirectory.Refresh(); //there's obviously room for error, but the folder could be deleted any time anyway
      provider = new LocalFileSystemProvider(rootDirectory, false);
    }


    [Test]
    public void Root_Folder_Should_Return_Contained_Sub_Folders()
    {
      var childFolders = provider.GetChildFolders(root);
      var directoryNames = rootDirectory.GetDirectories().Select(d => d.Name).ToList();

      Assert.AreNotEqual(0, childFolders.Count());
      Assert.AreEqual(directoryNames.Count(), childFolders.Count());

      foreach (var folder in childFolders)
      {
        Assert.Contains(folder.Name, directoryNames);
        var dir = rootDirectory.GetDirectories(folder.Name).Single();

        //also check one level below
        var files = provider.GetChildFiles(folder);
        Assert.AreEqual(dir.GetFiles().Count(), files.Count());
      }
    }


    [Test]
    public void Root_Folder_Should_Return_Contained_Files()
    {
      var childFiles = provider.GetChildFiles(root);
      var fileNames = rootDirectory.GetFiles().Select(f => f.Name).ToList();

      Assert.AreNotEqual(0, childFiles.Count());
      Assert.AreEqual(fileNames.Count(), childFiles.Count());

      foreach (var file in childFiles)
      {
        Assert.Contains(file.Name, fileNames);
      }
    }

    [Test]
    public void Popuplated_Root_Folder_Should_Not_Be_Empty()
    {
      Assert.IsFalse(root.IsEmpty);
    }

    [Test]
    public void Empty_Root_Folder_Should_Indicate_It_Is_Empty()
    {
      rootDirectory.GetDirectories().Do(d => d.Delete(true));
      rootDirectory.GetFiles().Do(f => f.Delete());

      root = provider.GetFileSystemRoot();
      Assert.IsTrue(root.IsEmpty);
    }


    [ExpectedException(typeof(ResourceAccessException))]
    [Test]
    public void Requesting_Parent_Of_Root_Should_Fail()
    {
      var folder = provider.GetFolderParent(root);
    }




    [ExpectedException(typeof(ResourceAccessException))]
    [Test]
    public void Requesting_Arbitrary_Folder_Above_Root_Should_Fail()
    {
      foreach (var drive in Directory.GetLogicalDrives())
      {
        try
        {
          VirtualFolderInfo driveFolder = new VirtualFolderInfo {FullName = drive};
          provider.GetFolderParent(driveFolder);
          Assert.Fail("Could request drive resource");
        }
        catch (ResourceAccessException)
        {
        }
      }

      VirtualFolderInfo folder = new VirtualFolderInfo {FullName = rootDirectory.Parent.FullName};
      provider.GetFolderParent(folder);
    }


    [Test]
    public void Requesting_Parent_Of_Immediate_Child_Folder_Should_Return_Root()
    {
      var folders = provider.GetChildFolders(root);
      foreach (var folder in folders)
      {
        var r = provider.GetFolderParent(folder);
        Assert.AreEqual(root.FullName, r.FullName);
        Assert.IsTrue(r.IsRootFolder);
      }
    }


    [Test]
    public void Requesting_Parent_Of_Immediate_Child_Files_Should_Return_Root()
    {
      var files = provider.GetChildFiles(root);
      foreach (var file in files)
      {
        var r = provider.GetFileParent(file);
        Assert.AreEqual(root.FullName, r.FullName);
        Assert.IsTrue(r.IsRootFolder);
      }
    }


    [Test]
    public void Parent_Folder_Of_Root_Should_Be_Null()
    {
      root = provider.GetFileSystemRoot();
      Assert.IsNull(root.ParentFolderPath);

      root = provider.GetFolderInfo(root.FullName);
      Assert.IsNull(root.ParentFolderPath);
    }


  }
}
