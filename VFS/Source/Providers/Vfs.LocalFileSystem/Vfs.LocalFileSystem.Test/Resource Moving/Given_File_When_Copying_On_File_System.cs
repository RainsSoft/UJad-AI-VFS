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
  public class Given_File_When_Copying_On_File_System : DirectoryTestBase
  {
    private DirectoryInfo targetDir;
    private FileInfo sourcePath, targetPath;
    private VirtualFileInfo original;

    protected override void InitInternal()
    {
      var dir = rootDirectory.GetDirectories().First();;
      targetDir = new DirectoryInfo(Path.Combine(rootDirectory.GetDirectories().Last().FullName, "target"));
      targetDir.Create();

      sourcePath = new FileInfo(Path.Combine(dir.FullName, "foo.bin"));
      File.WriteAllBytes(sourcePath.FullName, new byte[99999]);
      sourcePath.Refresh();
      
      targetPath = new FileInfo(Path.Combine(targetDir.FullName, "bar.bin"));
      original = provider.GetFileInfo(sourcePath.FullName);
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Moving_File_Should_Create_New_File()
    {
      Assert.False(targetPath.Exists);
      provider.CopyFile(original, targetPath.FullName);

      targetPath.Refresh();
      Assert.True(targetPath.Exists);
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Moving_To_Root_Directory_Should_Work()
    {
      targetPath = new FileInfo(Path.Combine(rootDirectory.FullName, "targetFoo"));
      Assert.False(targetPath.Exists);
      var target = provider.CopyFile(original, targetPath.FullName);
      targetPath.Refresh();
      Assert.True(targetPath.Exists);
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Copying_File_Should_Preserve_Copied_Folder_And_Contents()
    {
      targetPath = new FileInfo(Path.Combine(rootDirectory.FullName, "targetFoo"));
      Assert.True(sourcePath.Exists);
      var target = provider.CopyFile(original, targetPath.FullName);
      sourcePath.Refresh();
      Assert.True(sourcePath.Exists);
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Copying_Should_Return_Updated_File_Info()
    {
      var target = provider.CopyFile(original, targetPath.FullName);
      Assert.AreEqual(targetPath.Name, target.Name);
      Assert.AreEqual(sourcePath.Length, target.Length);

      var copy = provider.GetFileInfo(target.FullName);
      Assert.AreEqual(copy.FullName, target.FullName);
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    [ExpectedException(typeof(ResourceAccessException))]
    public void Copying_To_Itself_Should_Fail()
    {
      provider.CopyFile(original, original.FullName);
    }



    /// <summary>
    /// 
    /// </summary>
    [Test]
    [ExpectedException(typeof(ResourceOverwriteException))]
    public void Copying_To_Destination_That_Already_Exists_Should_Fail()
    {
      File.WriteAllBytes(targetPath.FullName, new byte[32768]);
      provider.CopyFile(original, targetPath.FullName);
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    [ExpectedException(typeof(ResourceAccessException))]
    public void Copying_File_Outside_Of_Scope_Should_Fail()
    {
      targetPath = new FileInfo(FileUtil.CreateTempFilePath("xx"));
      Assert.IsFalse(targetPath.Exists);

      try
      {
        provider.CopyFile(original, targetPath.FullName);
        Assert.Fail("Could create file outside scope!!!");
      }
      finally
      {
        if (File.Exists(targetPath.FullName))
        {
          Assert.Fail("Target file was created!");
          targetPath.Delete();
        }
      }
    }
  }
}
