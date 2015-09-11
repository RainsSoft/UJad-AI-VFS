using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

using NUnit.Framework;
using Vfs.Transfer;


namespace Vfs.Providers.TestSuite.Operations.Copy
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_Folder_When_Copying : TestBase
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
    public void Copying_Folder_Should_Create_New_Folder_At_Destination()
    {
      var target = source.Copy(targetPath);
      Assert.IsTrue(FileSystem.IsFolderAvailable(targetPath));
      Assert.IsTrue(FileSystem.IsFolderAvailable(target.MetaData.FullName));
    }



    [Test]
    public void Copying_Folder_Should_Preserve_Copied_Folder_And_Contents()
    {
      var sourcePath = source.MetaData.FullName;
      
      //perform copy
      source.Copy(targetPath);

      //make sure our source still exists
      Assert.IsTrue(FileSystem.IsFolderAvailable(sourcePath));

      //check contents
      var original = VirtualFolder.Create(FileSystem, sourcePath);
      Assert.AreEqual(2, original.GetFiles().Count());
      Assert.AreEqual(2, original.GetFolders().Count());
    }

    [Test]
    public void Copying_Should_Return_Updated_Folder_Info()
    {
      var target = source.Copy(targetPath);
      Assert.AreEqual("Target",  target.MetaData.Name);
      Assert.AreEqual(Context.EmptyFolder.MetaData.FullName, target.MetaData.ParentFolderPath);
    }



    [Test]
    public void Copying_Should_Include_SubFolders_And_Files()
    {
      var target = source.Copy(targetPath);

      var sourceContents = source.GetFolderContents();
      var targetContents = target.GetFolderContents();

      Assert.AreNotEqual(sourceContents.ParentFolderPath, targetContents.ParentFolderPath);

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
    public void Copied_Files_Should_Be_Equal()
    {
      var target = source.Copy(targetPath);
      var sourceFile = source.GetFiles().First().MetaData.FullName;
      var targetFile = target.GetFiles().First().MetaData.FullName;

      var sourceToken = FileSystem.DownloadTransfers.RequestDownloadToken(sourceFile, true);
      var targetToken = FileSystem.DownloadTransfers.RequestDownloadToken(targetFile, true);

      FileSystem.DownloadTransfers.CancelTransfer(sourceToken.TransferId, AbortReason.ClientAbort);
      FileSystem.DownloadTransfers.CancelTransfer(targetToken.TransferId, AbortReason.ClientAbort);

      Assert.AreEqual(sourceToken.Md5FileHash, targetToken.Md5FileHash);
    }


    [Test]
    [ExpectedException(typeof(ResourceAccessException))]
    public void Copying_To_Itself_Should_Fail()
    {
      FileSystem.CopyFolder(source.MetaData, source.MetaData.FullName);
    }


    [Test]
    [ExpectedException(typeof(ResourceAccessException))]
    public void Copying_To_Nested_Folder_Location_Should_Fail()
    {
      var targetFolder = FileSystem.CreateFolderPath(source.MetaData.FullName, "Nested");
      FileSystem.CopyFolder(source.MetaData, targetFolder);
    }


    [Test]
    [ExpectedException(typeof(ResourceOverwriteException))]
    public void Copying_To_Destination_That_Already_Exists_Should_Fail()
    {
      Context.EmptyFolder.AddFolder("Target");
      source.Copy(targetPath);
    }




  }
}
