using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Hardcodet.Commons.IO;
using NUnit.Framework;
using Vfs.Auditing;


namespace Vfs.LocalFileSystem.Test.Resource_Creation
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_FileSystem_When_Adding_Folder : DirectoryTestBase
  {

    [Test]
    public void Adding_A_Folder_To_Directory_Root_Should_Create_Directory()
    {
      string subFolder = "NewChild";
      string subFolderPath = Path.Combine(rootDirectory.FullName, subFolder);

      Assert.IsFalse(Directory.Exists(subFolderPath));

      var folder = provider.CreateFolder(root, subFolder);
      Assert.AreEqual(subFolder, folder.Name);
      Assert.AreEqual("/" + subFolder, folder.FullName);

      Assert.IsTrue(Directory.Exists(subFolderPath));
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Folders_May_Be_Created_Through_Parent_Or_Direct_Paths()
    {
      string childPath = rootDirectory.GetDirectories().First().Name + "/abcxyz";
      string qualifiedPath = Path.Combine(rootDirectory.FullName, childPath);

      Assert.IsFalse(Directory.Exists(qualifiedPath));
      provider.CreateFolder(childPath);
      Assert.IsTrue(Directory.Exists(qualifiedPath));
      Directory.Delete(qualifiedPath);

      provider.CreateFolder("/" + rootDirectory.GetDirectories().First().Name, "abcxyz");
      Assert.IsTrue(Directory.Exists(qualifiedPath));
    }


    [Test]
    public void Adding_A_Folder_To_Nested_Parent_Should_Create_Directory()
    {
      var parent = rootDirectory.CreateSubdirectory("l1/l2");
      var parentFolder = provider.GetFolderInfo(parent.FullName);
      Assert.AreEqual(0, parent.GetDirectories().Length);

      Assert.IsFalse(provider.IsFolderAvailable("l1/l2/Nested"));
      var nestedFolder = provider.CreateFolder(parentFolder, "Nested");
      Assert.AreEqual("/l1/l2/Nested", nestedFolder.FullName);
      Assert.IsTrue(provider.IsFolderAvailable("l1/l2/Nested"));

      Assert.IsTrue(Directory.Exists(Path.Combine(parent.FullName, "Nested")));
    }



    [Test]
    [ExpectedException(typeof(InvalidResourcePathException))]
    public void Adding_A_Folder_To_System_Root_Should_Cause_Exception()
    {
      provider = new LocalFileSystemProvider();
      provider.CreateFolder(root, "Illegal");
    }


    [Test]
    [ExpectedException(typeof(ResourceAccessException))]
    public void Adding_A_Folder_To_Directory_Outside_Of_Root_Should_Fail()
    {
      var dir = rootDirectory.Parent.Parent;
      provider.CreateFolder(dir.CreateFolderResourceInfo(), "Illegal");
    }


    [Test]
    [ExpectedException(typeof(VirtualResourceNotFoundException))]
    public void Adding_A_Folder_To_Unavailable_Parent_Directory_Should_Fail()
    {
      var parent = new VirtualFolderInfo {FullName = "/Foo/Bar"};
      provider.CreateFolder(parent, "ChildOfUnknownParent");
    }


    [Test]
    [ExpectedException(typeof(ResourceAccessException))]
    public void Adding_A_Folder_Path_That_Points_Too_High_Up_Should_Fail()
    {
      provider.CreateFolder(root, "/../../Test");
    }


    [Test]
    public void Adding_A_Folder_That_Already_Exists_Should_Fail()
    {
      provider.CreateFolder(root, "Foo");
      Assert.IsTrue(Directory.Exists(Path.Combine(rootDirectory.FullName, "Foo")));

      try
      {
        provider.CreateFolder(root, "foo");
        Assert.Fail("Attempt to create a folder that already exists succeeded.");
      }
      catch (ResourceOverwriteException e)
      {
        StringAssert.Contains("foo", e.Message);
      }
    }


    [Test]
    [ExpectedException(typeof(ResourceAccessException))]
    public void Adding_A_Folder_With_Invalid_Path_Name_Should_Fail()
    {
      string folderName = "a:?*<>;";
      provider.CreateFolder(root, folderName);
    }

  }

}
