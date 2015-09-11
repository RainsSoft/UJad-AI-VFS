using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using NUnit.Framework;
using Vfs.Test;


namespace Vfs.Providers.TestSuite.Operations.Copy
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_File_When_Copying : TestBase
  {
    private VirtualFile source;
    private string targetPath;

    protected override void InitInternal()
    {
      base.InitInternal();

      //create source and add two files
      source = Context.DownloadFolder.GetFiles().First();

      targetPath = FileSystem.CreateFilePath(Context.EmptyFolder.MetaData.FullName, "target.txt");
    }


    [Test]
    public void Copying_File_Should_Create_New_File()
    {
      Assert.False(FileSystem.IsFileAvailable(targetPath));
      source.Copy(targetPath);
      Assert.True(FileSystem.IsFileAvailable(targetPath));
    }


    [Test]
    public void Copying_File_Should_Preserve_Source()
    {
      Assert.True(FileSystem.IsFileAvailable(source.MetaData.FullName));
      source.Copy(targetPath);
      Assert.True(FileSystem.IsFileAvailable(source.MetaData.FullName));
    }


    [Test]
    public void Copying_Should_Return_Updated_File_Info()
    {
      var target = source.Copy(targetPath);
      target.Delete();
      var targetCopy = FileSystem.CopyFile(source.MetaData, targetPath);


      Assert.AreEqual(target.MetaData.Length, targetCopy.Length);
      Assert.AreEqual(target.MetaData.ContentType, targetCopy.ContentType);
      Assert.AreEqual(target.MetaData.Name, targetCopy.Name);
      Assert.AreEqual(target.MetaData.FullName, targetCopy.FullName);
      Assert.AreEqual(Context.EmptyFolder.MetaData.FullName, target.MetaData.ParentFolderPath);

      Assert.AreEqual(source.MetaData.Length, target.MetaData.Length);
      Assert.AreEqual("target.txt", target.MetaData.Name);
    }


    [Test]
    [ExpectedException(typeof(ResourceAccessException))]
    public void Copying_To_Itself_Should_Fail()
    {
      FileSystem.CopyFile(source.MetaData, source.MetaData.FullName);
    }


    [Test]
    [ExpectedException(typeof(ResourceOverwriteException))]
    public void Copying_To_Destination_That_Already_Exists_Should_Fail()
    {
      source.Copy(targetPath);
      source.Copy(targetPath);
    }


    [Test]
    public void Copying_Virtual_File_Should_Return_Updated_FileWrapper_And_Keep_Metadata()
    {
      string fullName = source.MetaData.FullName;
      string parentPath = source.MetaData.ParentFolderPath;

      var target = source.Copy(targetPath);

      Assert.AreEqual(fullName, source.MetaData.FullName);
      Assert.AreEqual(parentPath, source.MetaData.ParentFolderPath);

      Assert.AreNotEqual(source.MetaData.FullName, target.MetaData.FullName);
      Assert.AreNotEqual(source.MetaData.ParentFolderPath, target.MetaData.FullName);
    }
  }
}
