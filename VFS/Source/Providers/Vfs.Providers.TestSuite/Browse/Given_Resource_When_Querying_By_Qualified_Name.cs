using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using NUnit.Framework;
using Vfs.Test;


namespace Vfs.Providers.TestSuite.Browse
{
  [TestFixture]
  public class Given_Resource_When_Querying_By_Qualified_Name : TestBase
  {
    [Test]
    public void Requesting_File_Should_Return_Item_With_Correct_Meta_Data()
    {
      using (var stream = Context.DownloadFile0Template.OpenWrite())
      {
        stream.WriteByte(234);
      }

      Context.DownloadFile0Template.Refresh();
      var file = Context.UploadFolder.AddFile(Context.DownloadFile0Template.FullName, "upload.txt", true);
      var copy = FileSystem.GetFileInfo(file.MetaData.FullName);

      file.MetaData.AssertXEqualTo(copy);

      Assert.AreEqual("upload.txt", file.MetaData.Name);
      Assert.AreEqual(Context.DownloadFile0Template.Length, file.MetaData.Length);

      RecurseFileSystem(FileSystemRoot,
        (parent, fi) => fi.MetaData.AssertXEqualTo(FileSystem.GetFileInfo(fi.MetaData.FullName)),
        null);
    }


    [Test]
    public void Requesting_Folder_Should_Return_Item_With_Correct_Meta_Data()
    {
      RecurseFileSystem(FileSystemRoot, null,
      (parent, fi) => fi.MetaData.AssertXEqualTo(FileSystem.GetFolderInfo(fi.MetaData.FullName)));
    }


    [Test]
    public void Requesting_Root_Folder_Should_Return_Folder_With_Matching_Flag()
    {
      var folder = FileSystem.GetFolderInfo(FileSystemRoot.MetaData.FullName);
      Assert.IsTrue(folder.IsRootFolder);
    }
  }
}
