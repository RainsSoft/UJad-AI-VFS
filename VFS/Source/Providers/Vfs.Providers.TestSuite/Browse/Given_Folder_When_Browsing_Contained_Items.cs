using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using NUnit.Framework;
using Vfs.Test;


namespace Vfs.Providers.TestSuite.Browse
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_Folder_When_Browsing_Contained_Items : TestBase
  {
    protected VirtualFolder folderA;
    protected VirtualFolder folderB;
    protected VirtualFolder folderC;
    private VirtualFolder testFolder;


    protected override void InitInternal()
    {
      base.InitInternal();

      testFolder = Context.EmptyFolder;
      folderA = testFolder.AddFolder("a");
      folderB = testFolder.AddFolder("b");
      folderC = testFolder.AddFolder("c");

      testFolder.AddFile(Context.DownloadFile0Template.FullName, "file0.doc", false);
      testFolder.AddFile(Context.DownloadFile0Template.FullName, "file0.txt", false);

      testFolder.AddFile(Context.DownloadFile1Template.FullName, "file1.doc", false);
      testFolder.AddFile(Context.DownloadFile1Template.FullName, "file1.txt", false);
    }


    [Test]
    public void Querying_SubFolders_Should_Return_Contents()
    {
      var subFolders = testFolder.GetFolders()
        .OrderBy(f => f.MetaData.Name)
        .ToList();

      Assert.AreEqual(3, subFolders.Count);

      Assert.AreEqual("a", subFolders[0].MetaData.Name);
      Assert.AreEqual("b", subFolders[1].MetaData.Name);
      Assert.AreEqual("c", subFolders[2].MetaData.Name);

      subFolders.Do(vf => Assert.AreEqual(testFolder.MetaData.FullName, vf.MetaData.ParentFolderPath));
    }


    [Test]
    public void Querying_Files_Should_Return_Contents()
    {
      var files = testFolder.GetFiles()
        .OrderBy(f => f.MetaData.Name)
        .ToList();

      Assert.AreEqual(4, files.Count);

      Assert.AreEqual("file0.doc", files[0].MetaData.Name);
      Assert.AreEqual("file0.txt", files[1].MetaData.Name);
      Assert.AreEqual("file1.doc", files[2].MetaData.Name);
      Assert.AreEqual("file1.txt", files[3].MetaData.Name);

      files.Do(vf => Assert.AreEqual(testFolder.MetaData.FullName, vf.MetaData.ParentFolderPath));
    }


    [Test]
    public void Folders_Should_Indicate_Whether_They_Are_Empty_Or_Not()
    {
      //the test folder is not empty, but contains not sub folders and files
      testFolder.RefreshMetaData();
      Assert.IsFalse(testFolder.MetaData.IsEmpty);

      //add sub folder to folder a
      folderA.AddFolder("x");
      folderA.RefreshMetaData();
      Assert.IsFalse(folderA.MetaData.IsEmpty);

      //add sub files to folder b
      folderA.AddFile(Context.DownloadFile0Template.FullName, "xxx.bin", false);
      folderA.RefreshMetaData();
      Assert.IsFalse(folderA.MetaData.IsEmpty);

      //folder c is still empty
      folderC.RefreshMetaData();
      Assert.IsTrue(folderC.MetaData.IsEmpty);
    }


    [Test]
    public void Returned_Items_Should_Provide_The_Parent_Folder_Path()
    {
      var contents = testFolder.GetFolderContents();
      Assert.AreEqual(testFolder.MetaData.FullName, contents.ParentFolderPath);
    }


    [Test]
    public void Returned_Item_Should_Provide_All_Found_Folders()
    {
      var contents = testFolder.GetFolderContents();

      var subFolders = contents.Folders
        .OrderBy(f => f.MetaData.Name)
        .ToList();

      Assert.AreEqual(3, subFolders.Count);

      Assert.AreEqual("a", subFolders[0].MetaData.Name);
      Assert.AreEqual("b", subFolders[1].MetaData.Name);
      Assert.AreEqual("c", subFolders[2].MetaData.Name);

      subFolders.Do(vf => Assert.AreEqual(testFolder.MetaData.FullName, vf.MetaData.ParentFolderPath));
    }


    [Test]
    public void Returned_Item_Should_Provide_All_Found_Files()
    {
      var contents = testFolder.GetFolderContents();

      var files = contents.Files
        .OrderBy(f => f.MetaData.Name)
        .ToList();

      Assert.AreEqual(4, files.Count);

      Assert.AreEqual("file0.doc", files[0].MetaData.Name);
      Assert.AreEqual("file0.txt", files[1].MetaData.Name);
      Assert.AreEqual("file1.doc", files[2].MetaData.Name);
      Assert.AreEqual("file1.txt", files[3].MetaData.Name);

      files.Do(vf => Assert.AreEqual(testFolder.MetaData.FullName, vf.MetaData.ParentFolderPath));
    }


    [Test]
    public void Returned_Items_Should_Provide_Parent_Paths()
    {
      VirtualFolder root = VirtualFolder.CreateRootFolder(FileSystem);
      RecurseFileSystem(root,
                        (parent, file) => Assert.AreEqual(parent.MetaData.FullName, file.MetaData.ParentFolderPath),
                        (parent, folder) => Assert.AreEqual(parent.MetaData.FullName, folder.MetaData.ParentFolderPath)
        );
    }
  }
}