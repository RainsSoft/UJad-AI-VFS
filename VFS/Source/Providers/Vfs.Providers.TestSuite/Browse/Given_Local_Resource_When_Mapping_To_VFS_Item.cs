using System;
using System.IO;
using System.Security.AccessControl;
using System.Threading;
using Hardcodet.Commons.IO;
using NUnit.Framework;


namespace Vfs.Providers.TestSuite.Browse
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_Local_Resource_When_Mapping_To_VFS_Item : TestBase
  {
    [Test]
    public void Creating_Item_From_Inexisting_File_Should_Provide_Names_And_Default_Values()
    {
      FileInfo fi = new FileInfo("test.txt");
      Assert.IsFalse(fi.Exists);

      VirtualFileInfo item = fi.CreateFileResourceInfo();
      Assert.AreEqual("test.txt", item.Name);
      Assert.AreEqual(fi.FullName, item.FullName);

      //timestamps should be null
      Assert.IsNull(item.CreationTime);
      Assert.IsNull(item.LastAccessTime);
      Assert.IsNull(item.LastWriteTime);

      //the item should not be hidden/read-only
      Assert.IsFalse(item.IsHidden);
      Assert.IsFalse(item.IsReadOnly);

      //length should be zero
      Assert.AreEqual(0, item.Length);
    }


    [Test]
    public void Creating_Item_From_Existing_File_Should_Include_All_Information()
    {
      FileInfo fi = new FileInfo(GetType().Assembly.Location);
      Assert.IsTrue(fi.Exists);

      VirtualFileInfo item = fi.CreateFileResourceInfo();
      Assert.AreEqual(fi.Name, item.Name);
      Assert.AreEqual(fi.FullName, item.FullName);

      //timestamps should match file info
      Assert.AreEqual((DateTimeOffset)fi.CreationTime, item.CreationTime);
      Assert.AreEqual((DateTimeOffset)fi.LastAccessTime, item.LastAccessTime);
      Assert.AreEqual((DateTimeOffset)fi.LastWriteTime, item.LastWriteTime);

      //length should match file     
      Assert.AreNotEqual(0, item.Length);
      Assert.AreEqual(fi.Length, item.Length);
    }


    [Test]
    public void Creating_Item_From_Inexisting_Folder_Should_Provides_Names_And_Default_Properties()
    {
      DirectoryInfo di = new DirectoryInfo("test");
      Assert.IsFalse(di.Exists);

      VirtualFolderInfo item = di.CreateFolderResourceInfo();
      Assert.AreEqual("test", item.Name);
      Assert.AreEqual(di.FullName, item.FullName);

      //timestamps should be null
      Assert.IsNull(item.CreationTime);
      Assert.IsNull(item.LastAccessTime);
      Assert.IsNull(item.LastWriteTime);

      //the item should not be hidden / readonly
      Assert.IsFalse(item.IsHidden);
      Assert.IsFalse(item.IsReadOnly);

      //item is not root folder
      Assert.IsFalse(item.IsRootFolder);
    }


    [Test]
    public void Creating_Item_From_Existing_Directory_Should_Include_All_Information()
    {
      DirectoryInfo di = new FileInfo(GetType().Assembly.Location).Directory;
      Assert.IsTrue(di.Exists);

      VirtualFolderInfo item = di.CreateFolderResourceInfo();
      Assert.AreEqual(di.Name, item.Name);
      Assert.AreEqual(di.FullName, item.FullName);

      //timestamps should match file info
      Assert.AreEqual((DateTimeOffset)di.CreationTime, item.CreationTime);
      Assert.AreEqual((DateTimeOffset)di.LastAccessTime, item.LastAccessTime);
      Assert.AreEqual((DateTimeOffset)di.LastWriteTime, item.LastWriteTime);

      //the item should not be hidden
      Assert.IsFalse(item.IsHidden);

      //directory is not a root folder
      Assert.IsFalse(item.IsRootFolder);
    }


    [Test]
    public void Files_Should_Have_Correct_Hidden_And_Read_Only_Flags()
    {
      FileInfo fi = new FileInfo(Path.GetTempFileName());

      File.SetAttributes(fi.FullName, FileAttributes.ReadOnly);
      Assert.IsTrue(fi.CreateFileResourceInfo().IsReadOnly);
      Assert.IsFalse(fi.CreateFileResourceInfo().IsHidden);

      File.SetAttributes(fi.FullName, fi.Attributes | FileAttributes.Hidden);
      Assert.IsTrue(fi.CreateFileResourceInfo().IsHidden);
      Assert.IsTrue(fi.CreateFileResourceInfo().IsReadOnly);

      //reset attribute so it can be deleted
      File.SetAttributes(fi.FullName, FileAttributes.Hidden);
      Assert.IsTrue(fi.CreateFileResourceInfo().IsHidden);
      Assert.IsFalse(fi.CreateFileResourceInfo().IsReadOnly);

      File.Delete(fi.FullName);
    }


    [Test]
    public void Folders_Should_Have_Correct_Hidden_And_Read_Only_Flags()
    {
      var dir = FileUtil.CreateTempFolder("attribute-test-dir");

      File.SetAttributes(dir.FullName, FileAttributes.ReadOnly);
      Assert.IsTrue(dir.CreateFolderResourceInfo().IsReadOnly);
      Assert.IsFalse(dir.CreateFolderResourceInfo().IsHidden);

      File.SetAttributes(dir.FullName, dir.Attributes | FileAttributes.Hidden);
      Assert.IsTrue(dir.CreateFolderResourceInfo().IsHidden);
      Assert.IsTrue(dir.CreateFolderResourceInfo().IsReadOnly);

      //reset attribute so it can be deleted
      File.SetAttributes(dir.FullName, FileAttributes.Hidden);
      Assert.IsTrue(dir.CreateFolderResourceInfo().IsHidden);
      Assert.IsFalse(dir.CreateFolderResourceInfo().IsReadOnly);

      Directory.Delete(dir.FullName);
    }
  }
}
