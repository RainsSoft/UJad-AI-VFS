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
  public class Given_FileWrapper_When_Moving_Or_Copying : DirectoryTestBase
  {
    private DirectoryInfo targetDir;
    private FileInfo sourcePath, targetPath;
    private VirtualFile original;

    protected override void InitInternal()
    {
      var dir = rootDirectory.GetDirectories().First();;
      targetDir = new DirectoryInfo(Path.Combine(rootDirectory.GetDirectories().Last().FullName, "target"));
      targetDir.Create();

      sourcePath = new FileInfo(Path.Combine(dir.FullName, "foo.bin"));
      File.WriteAllBytes(sourcePath.FullName, new byte[99999]);
      sourcePath.Refresh();
      
      targetPath = new FileInfo(Path.Combine(targetDir.FullName, "bar.bin"));
      var fileInfo = provider.GetFileInfo(sourcePath.FullName);
      original = new VirtualFile(provider, fileInfo);
    }

    [Test]
    public void Moving_Should_Update_Underlying_MetaData()
    {
      Assert.False(targetPath.Exists);
      original.Move(targetPath.FullName);

      targetPath.Refresh();
      Assert.True(targetPath.Exists);
    }




    [Test]
    public void Moving_Should_Create_New_File()
    {
      Assert.False(targetPath.Exists);
      original.Move(targetPath.FullName);

      Assert.AreEqual(targetPath.Name, original.MetaData.Name);
    }


  
    [Test]
    public void Copying_Should_Return_Updated_FileWrapper_And_Keep_Metadata()
    {
      Assert.False(targetPath.Exists);
      VirtualFile copy = original.Copy(targetPath.FullName);

      Assert.AreEqual(original.MetaData.Length, copy.MetaData.Length);
      Assert.AreEqual(targetPath.Name, copy.MetaData.Name);
      Assert.AreEqual(sourcePath.Name, original.MetaData.Name);
    }


    [Test]
    public void Copying_Should_Create_New_File()
    {
      Assert.False(targetPath.Exists);
      original.Copy(targetPath.FullName);

      targetPath.Refresh();
      Assert.True(targetPath.Exists);
    }


    [Test]
    [ExpectedException(typeof(ResourceAccessException))]
    public void Copying_To_Itself_Should_Fail()
    {
      original.Copy(original.MetaData.FullName);
    }


  }
}
