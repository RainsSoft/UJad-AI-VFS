using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using NUnit.Framework;
using Vfs.Test;


namespace Vfs.Providers.TestSuite.Operations.CreateFolder
{
  [TestFixture]
  public class Given_Folder_When_Adding_SubFolder : TestBase
  {
    [Test]
    public void Created_Folder_Should_Exist_On_File_System()
    {
      var parent = Context.EmptyFolder.MetaData;
      var child = FileSystem.CreateFolder(parent, "Child1");
      Assert.IsTrue(FileSystem.IsFolderAvailable(child.FullName));
    }


    [Test]
    public void Created_Folder_Should_Provide_Correct_Meta_Data()
    {
      var parent = Context.EmptyFolder.MetaData;
      var child = FileSystem.CreateFolder(parent, "Child1");

      //run test on returned folder
      Assert.AreEqual("Child1", child.Name);
      child.AssertXEqualTo(FileSystem.GetFolderInfo(child.FullName));
    }


    [Test]
    public void Folder_Should_Be_Listed_As_Child_After_Creation()
    {
      var parent = Context.EmptyFolder.MetaData;
      var child = FileSystem.CreateFolder(parent, "Child1");
      
      //retrieve child
      var copy = FileSystem.GetChildFolders(parent).Single();
      child.AssertXEqualTo(copy);
    }


    [Test]
    public void Returned_Folder_Info_Should_Provide_Link_To_Parent()
    {
      var parent = Context.EmptyFolder.MetaData;
      var child = FileSystem.CreateFolder(parent, "Child1");

      Assert.AreEqual(parent.FullName, child.ParentFolderPath);
    }


    [Test]
    public void Folders_May_Be_Created_Through_Parent_Or_Direct_Paths()
    {
      var parent = Context.EmptyFolder.MetaData;
      var child = FileSystem.CreateFolder(parent, "Child1");
      Assert.IsTrue(FileSystem.IsFolderAvailable(child.FullName));
      FileSystem.DeleteFolder(child.FullName);

      child = FileSystem.CreateFolder(parent.FullName, "Child1");
      Assert.IsTrue(FileSystem.IsFolderAvailable(child.FullName));
      FileSystem.DeleteFolder(child.FullName);

      string path = FileSystem.CreateFolderPath(parent.FullName, "Child1");
      child = FileSystem.CreateFolder(path);
      Assert.IsTrue(FileSystem.IsFolderAvailable(child.FullName));
    }


    [Test]
    [ExpectedException(typeof(ResourceOverwriteException))]
    public void Adding_A_Folder_That_Already_Exists_Should_Fail()
    {
      var parent = Context.EmptyFolder.MetaData;
      FileSystem.CreateFolder(parent, "Child1");
      FileSystem.CreateFolder(parent, "Child1");
    }


    [Test]
    [ExpectedException(typeof(VirtualResourceNotFoundException))]
    public void Adding_A_Folder_To_Unavailable_Parent_Directory_Should_Fail()
    {
      string unavailableParentPath = FileSystem.CreateFolderPath(Context.EmptyFolder.MetaData.FullName, "Unknown");
      FileSystem.CreateFolder(unavailableParentPath, "Child1");
    }


    [Test]
    [ExpectedException(typeof(ResourceAccessException))]
    public void Adding_A_Folder_To_Path_That_Is_Existing_File_Should_File()
    {
      var file = Context.DownloadFolder.GetFiles().First().MetaData;
      FileSystem.CreateFolder(Context.DownloadFolder.MetaData, file.Name);
    }


    [Test]
    [ExpectedException(typeof(ResourceAccessException))]
    public void Adding_Folder_Should_Fail_If_Parent_Folder_Path_Is_File()
    {
      var file = Context.DownloadFolder.GetFiles().First().MetaData;
      FileSystem.CreateFolder(file.FullName);
    }
  }
}
