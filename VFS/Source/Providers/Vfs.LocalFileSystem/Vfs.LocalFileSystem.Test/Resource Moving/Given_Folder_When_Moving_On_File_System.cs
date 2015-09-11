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
  public class Given_Folder_When_Moving_On_File_System : DirectoryTestBase
  {
    private DirectoryInfo originalDir, targetDir;
    private VirtualFolderInfo originalFolder;

    protected override void InitInternal()
    {
      originalDir = rootDirectory.GetDirectories().First();
      originalFolder = provider.GetFolderInfo(originalDir.FullName);
      targetDir = new DirectoryInfo(Path.Combine(rootDirectory.GetDirectories().Last().FullName, "target"));
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Moving_Folder_Should_Create_New_Folder()
    {
      Assert.False(targetDir.Exists);
      var targetFolder = provider.MoveFolder(originalFolder, targetDir.FullName);
      
      targetDir.Refresh();
      Assert.True(targetDir.Exists);
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Moving_To_Root_Directory_Should_Work()
    {
      targetDir = new DirectoryInfo(Path.Combine(rootDirectory.FullName, "targetFoo"));
      Assert.False(targetDir.Exists);
      var targetFolder = provider.MoveFolder(originalFolder, targetDir.FullName);
      targetDir.Refresh();
      Assert.True(targetDir.Exists);
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Moving_Should_Return_Updated_Folder_Info()
    {
      var targetFolder = provider.MoveFolder(originalFolder, targetDir.FullName);
      Assert.AreEqual(targetDir.Name, targetFolder.Name);

      var copy = provider.GetFolderInfo(targetFolder.FullName);
      Assert.AreEqual(copy.FullName, targetFolder.FullName);
    }

    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Moving_Should_Remove_Original()
    {
      Assert.True(originalDir.Exists);
      var targetFolder = provider.MoveFolder(originalFolder, targetDir.FullName);
      
      originalDir.Refresh();
      Assert.False(originalDir.Exists);
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Moving_Should_Include_SubFolders_And_Files()
    {
      var subDirInfos = originalDir.GetDirectories();
      var subFileInfos = originalDir.GetFiles();

      var targetFolder = provider.MoveFolder(originalFolder, targetDir.FullName);

      var subDirInfos2 = targetDir.GetDirectories();
      var subFileInfos2 = targetDir.GetFiles();

      Assert.AreEqual(subDirInfos.Count(), subDirInfos2.Count());
      Assert.AreEqual(subFileInfos.Count(), subFileInfos2.Count());

      for (int i = 0; i < subDirInfos.Length; i++)
      {
        Assert.AreEqual(subDirInfos[i].Name, subDirInfos2[i].Name);
      }

      for (int i = 0; i < subFileInfos.Length; i++)
      {
        Assert.AreEqual(subFileInfos[i].Name, subFileInfos2[i].Name);
      }
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    [ExpectedException(typeof(ResourceAccessException))]
    public void Moving_To_Itself_Should_Fail()
    {
      provider.MoveFolder(originalFolder, originalDir.FullName);
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    [ExpectedException(typeof(ResourceAccessException))]
    public void Moving_To_Nested_Folder_Should_Fail()
    {
      targetDir = new DirectoryInfo(Path.Combine(originalDir.FullName, "subsub"));
      Assert.False(targetDir.Exists);
      provider.MoveFolder(originalFolder, targetDir.FullName);
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    [ExpectedException(typeof(ResourceOverwriteException))]
    public void Moving_To_Destination_That_Already_Exists_Should_Fail()
    {
      targetDir = rootDirectory.CreateSubdirectory("AlreayExisting");
      provider.MoveFolder(originalFolder, targetDir.Name);
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    [ExpectedException(typeof(ResourceAccessException))]
    public void Moving_Folder_Outside_Of_Scope_Should_Fail()
    {
      var targetFolder = new DirectoryInfo(FileUtil.CreateTempFilePath("xx"));
      Assert.IsFalse(targetFolder.Exists);

      try
      {
        provider.MoveFolder(originalFolder, targetFolder.FullName);
        targetFolder.Refresh();
        Assert.Fail("Could create directory!!!");
      }
      finally
      {
        if (Directory.Exists(targetFolder.FullName))
        {
          Assert.Fail("Target folder was created!");
          targetFolder.Delete(true);
        }
      }
    }
  }
}
