using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

using NUnit.Framework;


namespace Vfs.LocalFileSystem.Test.Resource_Tests
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_Root_Folder_When_Scope_Is_Full_System
  {
    private IFileSystemProvider provider;
    private VirtualFolderInfo root;

    [SetUp]
    public void Init()
    {
      provider = new LocalFileSystemProvider();
      root = provider.GetFileSystemRoot();
    }


    [Test]
    public void Root_Folder_Name_Should_Match_Computer()
    {
      Assert.AreEqual(Environment.MachineName, root.Name);
    }


    [Test]
    public void Root_Should_Not_Be_Empty()
    {
      Assert.IsFalse(root.IsEmpty);
    }


    [Test]
    public void Custom_Root_Name_Should_Be_Returned_If_Set()
    {
      provider = new LocalFileSystemProvider("FOO");
      root = provider.GetFileSystemRoot();

      Assert.AreEqual("FOO", root.Name);
    }


    [Test]
    public void Root_Folder_Should_Indicate_It_Is_The_Root()
    {
      Assert.IsTrue(root.IsRootFolder);
    }

    [Test]
    public void Root_Folder_Should_Return_Existing_Drives_As_Folders_Which_Indicate_They_Are_Not_Root()
    {
      var driveFolders = provider.GetChildFolders(root);
      var driveNames = DriveInfo.GetDrives().Where(di => di.RootDirectory.Exists).Select(di => di.RootDirectory.FullName).ToList();

      Assert.IsNotEmpty(driveFolders.ToArray());
      Assert.AreEqual(driveNames.Count(), driveFolders.Count());
      foreach (var folder in driveFolders)
      {
        Assert.IsTrue(driveNames.Contains(folder.Name));
        Assert.Contains(folder.Name, driveNames);
        Assert.IsFalse(folder.IsRootFolder);
      }

    }



    [Test]
    public void Root_Folders_Full_Name_Should_Be_An_Empty_String()
    {
      Assert.AreEqual(String.Empty, root.FullName);
    }


    [Test]
    public void Root_Folder_Should_Not_Return_Any_Files()
    {
      Assert.AreEqual(0, provider.GetChildFiles(root).Count());
    }



    [ExpectedException(typeof(ResourceAccessException))]
    [Test]
    public void Requesting_Parent_Of_Root_Should_Fail()
    {
      var folder = provider.GetFolderParent(root);
    }



    [Test]
    public void Requesting_Parent_Of_Drive_Folder_Should_Return_Root()
    {
      var driveFolders = provider.GetChildFolders(root);
      Assert.AreNotEqual(0, driveFolders.Count());

      foreach (var driveFolder in driveFolders)
      {
        var r = provider.GetFolderParent(driveFolder);
        Assert.IsTrue(r.IsRootFolder);
        Assert.AreEqual(root.Name, r.Name);
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



    [Test]
    public void Parent_Folder_Path_Of_Drive_Folder_Should_Be_Root()
    {
      var driveFolders = provider.GetChildFolders(root);
      foreach (var driveFolder in driveFolders)
      {
        var r = provider.GetFolderInfo(driveFolder.ParentFolderPath);
        Assert.IsTrue(r.IsRootFolder);
      }
    }



    [Test]
    public void Changing_The_Root_Folder_Path_On_Retrieved_Folder_Does_Not_Change_Configuration()
    {
      //make sure we have independent instances
      root.FullName = @"C:\Test";
      Assert.IsEmpty(provider.GetFileSystemRoot().FullName);
    }


    [Test]
    [ExpectedException(typeof(InvalidResourcePathException))]
    public void Requesting_A_Folder_Name_Directly_Under_Root_Should_Cause_Exception()
    {
      var folderInfo = provider.GetFolderInfo("Illegal");
    }


    [Test]
    [ExpectedException(typeof(InvalidResourcePathException))]
    public void Requesting_A_File_Name_Directly_Under_Root_Should_Cause_Exception()
    {
      var fileInfo = provider.GetFileInfo("Illegal");
    }
  }
}
