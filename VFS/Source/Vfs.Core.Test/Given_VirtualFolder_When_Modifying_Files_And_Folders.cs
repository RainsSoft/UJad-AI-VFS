using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Vfs.LocalFileSystem;
using Vfs.Util;

namespace Vfs.Test
{
  [TestFixture]
  public class Given_VirtualFolder_When_Modifying_Files_And_Folders
  {
    private IFileSystemProvider provider;
    private DirectoryInfo rootDirectory;
    private VirtualFolder root;

    [SetUp]
    public void Init()
    {
      rootDirectory = TestUtil.CreateTestDirectory();

      //init provider
      provider = new LocalFileSystemProvider(rootDirectory, true);
      root = VirtualFolder.CreateRootFolder(provider);

    }


    [TearDown]
    public void Cleanup()
    {
      rootDirectory.Refresh();
      if (rootDirectory.Exists) rootDirectory.Delete(true);
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Creating_A_Folder_And_Adding_A_File_Should_Create_Items_On_File_System()
    {
      var foo = root.AddFolder("foo");
      var bar = foo.AddFolder("bar");
      FileInfo sourceFile = rootDirectory.GetFiles().First();
      using(Stream stream = sourceFile.OpenRead())
      {
        var foobar = bar.AddFile("foobar.bin", stream, false, sourceFile.Length, ContentUtil.UnknownContentType);
        Assert.AreEqual("foobar.bin", foobar.MetaData.Name);
      }

      var fooDir = rootDirectory.GetDirectories("foo").Single();
      var barDir = fooDir.GetDirectories("bar").Single();
      Assert.AreEqual(1, barDir.GetFiles().Count());


      //create a file on root level
      sourceFile = rootDirectory.GetFiles().First();
      using (Stream stream = sourceFile.OpenRead())
      {
        var rootFile = root.AddFile("rootFile.bin", stream, false, sourceFile.Length, ContentUtil.UnknownContentType);
        Assert.AreEqual("rootFile.bin", rootFile.MetaData.Name);
        Assert.True(File.Exists(Path.Combine(rootDirectory.FullName, rootFile.MetaData.Name)));
      }
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Overwriting_A_File_Should_Update_Meta_Data()
    {
      VirtualFile file;

      FileInfo sourceFile = rootDirectory.GetFiles().First();
      using (Stream stream = sourceFile.OpenRead())
      {
        file = root.AddFile("foobar.bin", stream, false, sourceFile.Length, ContentUtil.UnknownContentType);
        Assert.AreEqual("foobar.bin", file.MetaData.Name);
      }

      VirtualFileInfo original = file.MetaData;

      //make a break in order to ensure timestamps are not the same
      Thread.Sleep(1000);

      //overwrite file
      sourceFile = rootDirectory.GetFiles().Last();
      using (Stream stream = sourceFile.OpenRead())
      {
        file = root.AddFile("foobar.bin", stream, true, sourceFile.Length, ContentUtil.UnknownContentType);
        Assert.AreEqual("foobar.bin", file.MetaData.Name);
      }

      VirtualFileInfo update = file.MetaData;

      Assert.AreNotSame(original, update);
      Assert.AreEqual(original.Name, update.Name);
      Assert.AreEqual(original.FullName, update.FullName);
      Assert.AreNotEqual(original.Length, update.Length);
      Assert.AreNotEqual(original.LastWriteTime, update.LastWriteTime);
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Deleting_A_File_Should_Remove_It_From_File_System()
    {
      VirtualFile file;

      FileInfo sourceFile = rootDirectory.GetFiles().First();
      using (Stream stream = sourceFile.OpenRead())
      {
        file = root.AddFile("foobar.bin", stream, false, sourceFile.Length, ContentUtil.UnknownContentType);
      }

      FileInfo fi = rootDirectory.GetFiles(file.MetaData.Name).Single();
      Assert.True(fi.Exists);

      file.Delete();
      fi.Refresh();
      Assert.False(fi.Exists);
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Deleting_A_Folder_Should_Remove_It_From_File_System()
    {
      DirectoryInfo dir = rootDirectory.GetDirectories().First();
      Assert.True(dir.Exists);

      VirtualFolder folder = root.GetFolders(dir.Name).Single();
      folder.Delete();

      dir.Refresh();
      Assert.False(dir.Exists);
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Overwriting_File_Without_Request_Should_Be_Blocked()
    {
      VirtualFile file;

      FileInfo sourceFile = rootDirectory.GetFiles().First();
      using (Stream stream = sourceFile.OpenRead())
      {
        file = root.AddFile("foobar.bin", stream, false, sourceFile.Length, ContentUtil.UnknownContentType);
        Assert.AreEqual("foobar.bin", file.MetaData.Name);
      }

      VirtualFileInfo original = file.MetaData;

      //overwrite file - this will not happen
      sourceFile = rootDirectory.GetFiles().Last();
      using (Stream stream = sourceFile.OpenRead())
      {
        try
        {
          root.AddFile("foobar.bin", stream, false, sourceFile.Length, ContentUtil.UnknownContentType);
          Assert.Fail("Could overwrite file.");
        }
        catch (ResourceOverwriteException e)
        {
          StringAssert.Contains("foobar.bin", e.Message);
        }
      }

      //make sure nothing changed
      var update = root.GetFiles("foobar.bin").Single();
      Assert.AreEqual(original.Length, update.MetaData.Length);
      Assert.AreEqual(original.LastWriteTime, update.MetaData.LastWriteTime);
    }


  }

}
