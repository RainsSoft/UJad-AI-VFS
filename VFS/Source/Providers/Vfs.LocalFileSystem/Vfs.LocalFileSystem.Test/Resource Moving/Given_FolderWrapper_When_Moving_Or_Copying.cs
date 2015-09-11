using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Hardcodet.Commons;
using Hardcodet.Commons.IO;
using Vfs.Util;
using NUnit.Framework;


namespace Vfs.LocalFileSystem.Test.Resource_Creation
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_FolderWrapper_When_Moving_Or_Copying : DirectoryTestBase
  {
    private DirectoryInfo originalDir, targetDir;
    private VirtualFolder originalFolder;

    protected override void InitInternal()
    {
      originalDir = rootDirectory.GetDirectories().First();
      var folder = provider.GetFolderInfo(originalDir.FullName);
      originalFolder = new VirtualFolder(provider, folder);
      targetDir = new DirectoryInfo(Path.Combine(rootDirectory.GetDirectories().Last().FullName, "target"));
    }


    [Test]
    public void Moving_Should_Return_Updated_FolderWrapper()
    {
      originalFolder.Move(targetDir.FullName);
      Assert.AreEqual(targetDir.Name, originalFolder.MetaData.Name);

      var copy = provider.GetFolderInfo(originalFolder.MetaData.FullName);
      Assert.AreEqual(copy.FullName, originalFolder.MetaData.FullName);
    }


    [Test]
    public void Moving_Folder_Should_Create_New_Folder()
    {
      Assert.False(targetDir.Exists);
      originalFolder.Move(targetDir.FullName);

      targetDir.Refresh();
      Assert.True(targetDir.Exists);
    }




    [Test]
    public void Copying_Folder_Should_Create_New_Folder_At_Destination()
    {
      Assert.False(targetDir.Exists);
      var targetFolder = originalFolder.Copy(targetDir.FullName);

      targetDir.Refresh();
      Assert.True(targetDir.Exists);

      Assert.AreEqual(originalDir.GetFiles().Count(), targetDir.GetFiles().Count());
      Assert.AreEqual(originalDir.GetDirectories().Count(), targetDir.GetDirectories().Count());
    }



    [Test]
    public void Copying_Folder_Should_Preserve_Copied_Folder_And_Contents()
    {
      Assert.False(targetDir.Exists);
      originalFolder.Copy(targetDir.FullName);

      originalDir.Refresh();
      Assert.True(originalDir.Exists);
    }


    [Test]
    public void Copying_Should_Return_Updated_FolderWrapper()
    {
      VirtualFolder targetFolder = originalFolder.Copy(targetDir.FullName);
      Assert.AreEqual(targetDir.Name, targetFolder.MetaData.Name);

      //get folder info by request
      var copy = provider.GetFolderInfo(targetFolder.MetaData.FullName);
      Assert.AreEqual(copy.FullName, targetFolder.MetaData.FullName);

      //check the original folder
      copy = provider.GetFolderInfo(originalFolder.MetaData.FullName);
      Assert.AreEqual(copy.FullName, originalFolder.MetaData.FullName);
    }

 
    [Test]
    [ExpectedException(typeof(ResourceAccessException))]
    public void Copying_To_Itself_Should_Fail()
    {
      originalFolder.Copy(originalDir.FullName);
    }

  }
}
