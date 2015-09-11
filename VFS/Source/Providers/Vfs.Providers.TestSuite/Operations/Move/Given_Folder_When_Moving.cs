using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using NUnit.Framework;
using Vfs.Transfer;


namespace Vfs.Providers.TestSuite.Operations.Move
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_Folder_When_Moving : TestBase
  {
    private VirtualFolder source;
    private string targetPath;

    protected override void InitInternal()
    {
      base.InitInternal();

      //create source and add two files
      source = Context.EmptyFolder.AddFolder("Source");

      //sub folders
      source.AddFolder("folder0");
      source.AddFolder("folder1");

      //sub files
      source.AddFile(Context.DownloadFile0Template.FullName, "file0.bin", false);
      source.AddFile(Context.DownloadFile1Template.FullName, "file1.txt", false);

      targetPath = FileSystem.CreateFolderPath(Context.EmptyFolder.MetaData.FullName, "Target");
    }



    [Test]
    public void Moving_Folder_Should_Create_New_Folder_At_Destination()
    {
      Assert.IsFalse(FileSystem.IsFolderAvailable(targetPath));
      source.Move(targetPath);
      Assert.IsTrue(FileSystem.IsFolderAvailable(targetPath));
    }


    [Test]
    public void Moving_Folder_Should_Update_Metadata()
    {
      var metaData = source.MetaData;
      source.Move(targetPath);
      Assert.AreNotSame(metaData, source.MetaData);
    }



    [Test]
    public void Moving_Folder_Should_Remove_Source()
    {
      string sourcePath = source.MetaData.FullName;

      Assert.IsTrue(FileSystem.IsFolderAvailable(sourcePath));
      source.Move(targetPath);
      Assert.IsFalse(FileSystem.IsFolderAvailable(sourcePath));
    }


    [Test]
    public void Moving_Should_Return_Updated_Folder_Info()
    {
      var target = FileSystem.MoveFolder(source.MetaData, targetPath);
      Assert.AreEqual("Target", target.Name);
      Assert.AreEqual(Context.EmptyFolder.MetaData.FullName, target.ParentFolderPath);
    }



    [Test]
    public void Moving_Should_Include_SubFolders_And_Files()
    {
      var sourceContents = source.GetFolderContents();

      source.Move(targetPath);
      var target = VirtualFolder.Create(FileSystem, targetPath);
      var targetContents = target.GetFolderContents();

      Assert.AreEqual(sourceContents.Files.Count(), targetContents.Files.Count());
      Assert.AreEqual(sourceContents.Folders.Count(), targetContents.Folders.Count());

      //check first file
      Assert.AreEqual(sourceContents.Files.First().MetaData.Length, targetContents.Files.First().MetaData.Length);
      Assert.AreEqual(sourceContents.Files.First().MetaData.Name, targetContents.Files.First().MetaData.Name);
      Assert.AreEqual(sourceContents.Files.First().MetaData.ContentType, targetContents.Files.First().MetaData.ContentType);
      Assert.AreEqual(sourceContents.Files.First().MetaData.Length, targetContents.Files.First().MetaData.Length);

      //check second file
      Assert.AreEqual(sourceContents.Files.Last().MetaData.Length, targetContents.Files.Last().MetaData.Length);
      Assert.AreEqual(sourceContents.Files.Last().MetaData.Name, targetContents.Files.Last().MetaData.Name);
      Assert.AreEqual(sourceContents.Files.Last().MetaData.ContentType, targetContents.Files.Last().MetaData.ContentType);
      Assert.AreEqual(sourceContents.Files.Last().MetaData.Length, targetContents.Files.Last().MetaData.Length);

      //check first/second folder name
      Assert.AreEqual(sourceContents.Folders.First().MetaData.Name, targetContents.Folders.First().MetaData.Name);
      Assert.AreEqual(sourceContents.Folders.Last().MetaData.Name, targetContents.Folders.Last().MetaData.Name);
    }


    [Test]
    public void Moved_Files_Should_Be_Equal()
    {
      //get download token for source file
      var sourceFile = source.GetFiles().First().MetaData.FullName;
      var sourceToken = FileSystem.DownloadTransfers.RequestDownloadToken(sourceFile, true);
      FileSystem.DownloadTransfers.CancelTransfer(sourceToken.TransferId, AbortReason.ClientAbort);

      source.Move(targetPath);
      var target = VirtualFolder.Create(FileSystem, targetPath);

      //get download token for moved file
      var targetFile = target.GetFiles().First().MetaData.FullName;
      var targetToken = FileSystem.DownloadTransfers.RequestDownloadToken(targetFile, true);
      FileSystem.DownloadTransfers.CancelTransfer(targetToken.TransferId, AbortReason.ClientAbort);

      Assert.AreEqual(sourceToken.Md5FileHash, targetToken.Md5FileHash);
    }


    [Test]
    [ExpectedException(typeof(ResourceAccessException))]
    public void Moving_To_Itself_Should_Fail()
    {
      FileSystem.MoveFolder(source.MetaData, source.MetaData.FullName);
    }


    [Test]
    [ExpectedException(typeof(ResourceAccessException))]
    public void Moving_To_Nested_Folder_Location_Should_Fail()
    {
      var targetFolder = FileSystem.CreateFolderPath(source.MetaData.FullName, "Nested");
      FileSystem.MoveFolder(source.MetaData, targetFolder);
    }


    [Test]
    [ExpectedException(typeof(ResourceOverwriteException))]
    public void Moving_To_Destination_That_Already_Exists_Should_Fail()
    {
      Context.EmptyFolder.AddFolder("Target");
      source.Move(targetPath);
    }
  }
}
