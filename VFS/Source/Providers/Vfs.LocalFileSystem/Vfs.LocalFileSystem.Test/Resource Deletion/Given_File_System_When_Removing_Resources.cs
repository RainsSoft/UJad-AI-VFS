using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Hardcodet.Commons.IO;
using NUnit.Framework;
using Vfs.Auditing;

namespace Vfs.LocalFileSystem.Test.Resource_Deletion
{
  [TestFixture]
  public class Given_File_System_When_Removing_Resources : DirectoryTestBase
  {

    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Removing_Virtual_File_Should_Delete_Physical_File ()
    {
      //delete from root with qualified path
      var files = rootDirectory.GetFiles();
      provider.DeleteFile(files[0].FullName);
      Assert.IsFalse(File.Exists(files[0].FullName));

      //delete from root with virtual path
      provider.DeleteFile("/" + files[1].Name);
      Assert.IsFalse(File.Exists(files[1].FullName));

      var dir = rootDirectory.GetDirectories().First();
      files = dir.GetFiles();

      //delete by absolute path
      provider.DeleteFile(files[0].FullName);
      Assert.IsFalse(File.Exists(files[0].FullName));

      //delete from folder with virtual path
      provider.DeleteFile("/" + dir.Name + "/" + files[1].Name);
      Assert.IsFalse(File.Exists(files[1].FullName));
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Removing_Folder_Should_Delete_Directory ()
    {
      var subDir = rootDirectory.GetDirectories().First();
      provider.DeleteFolder(subDir.FullName);
      Assert.IsFalse(Directory.Exists(subDir.FullName));

      subDir = rootDirectory.GetDirectories().First();
      provider.DeleteFolder("/" + subDir.Name);
      Assert.IsFalse(Directory.Exists(subDir.FullName));
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    [ExpectedException(typeof(VirtualResourceNotFoundException))]
    public void Requesting_Deletion_Of_Unknown_Folder_Should_Fail()
    {
      provider.DeleteFolder("/foo/bar/x/foobar/");
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    [ExpectedException(typeof(VirtualResourceNotFoundException))]
    public void Requesting_Deletion_Of_Unknown_File_Should_Fail ()
    {
      provider.DeleteFile("/foo/bar/x/foobar.txt");
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Requestion_Deletion_Of_File_Outside_Scope_Should_Fail()
    {
      string file = Path.GetTempFileName();

      try
      {
        provider.DeleteFile(file);
      }
      catch(ResourceAccessException e)
      {
        StringAssert.Contains(Path.GetFileName(file), e.Message);
      }
      finally
      {
        File.Delete(file);
      }
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Requestion_Deletion_Of_Folder_Outside_Scope_Should_Fail()
    {
      var folder = FileUtil.CreateTempFolder("SomeTest");

      try
      {
        provider.DeleteFolder(folder.FullName);
      }
      catch (ResourceAccessException e)
      {
        StringAssert.Contains(folder.Name, e.Message);
      }
      finally
      {
        folder.Delete();
      }
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Requestion_Deletion_Of_Root_Directory_Should_Fail()
    {
      try
      {
        provider.DeleteFolder("/");
      }
      catch (ResourceAccessException e)
      {
        StringAssert.Contains("root", e.Message.ToLower());
      }

      try
      {
        provider.DeleteFolder(rootDirectory.FullName);
      }
      catch (ResourceAccessException e)
      {
        StringAssert.Contains("root", e.Message.ToLower());
      }
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    [ExpectedException(typeof(ResourceAccessException))]
    public void Requestion_Deletion_Of_Drive_Should_Fail  ()
    {
      Assert.Ignore("Works fine. However: Run this test manually in the debugger and abort if it fails before deleting your stuff!");
 
      var sysProvider = new LocalFileSystemProvider();
      sysProvider.Auditor = new ConsoleAuditor();
      sysProvider.DeleteFolder("G:\\");
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Requestion_Deletion_System_Root_Should_Fail()
    {
      Assert.Ignore("Works fine. However: Run this test manually in the debugger and abort if it fails before deleting your stuff!");

      var sysProvider = new LocalFileSystemProvider();
      try
      {
        string rootName = sysProvider.GetFileSystemRoot().FullName;
        sysProvider.DeleteFolder(rootName);
        Assert.Fail("Could delete system root???");
      }
      catch (ResourceAccessException e)
      {
        StringAssert.Contains("root", e.Message.ToLower());
      }
    }
  }
}
