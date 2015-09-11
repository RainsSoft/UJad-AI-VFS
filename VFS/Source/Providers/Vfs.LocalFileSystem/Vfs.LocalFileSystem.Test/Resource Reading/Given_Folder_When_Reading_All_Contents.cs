using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Vfs.LocalFileSystem.Test.Resource_Reading
{
  [TestFixture]
  public class Given_Folder_When_Reading_All_Contents : DirectoryTestBase
  {

    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Returned_Item_Should_Provide_The_Parent_Folder_Path()
    {
      var contents = provider.GetFolderContents(rootDirectory.FullName);
      Assert.AreEqual("/", contents.ParentFolderPath);
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Returned_Item_Should_Provide_All_Found_Folders()
    {
      //check root
      var contents = provider.GetFolderContents(root);
      Assert.AreEqual(contents.Folders.Count(), rootDirectory.GetDirectories().Count());

      var folders = contents.Folders.OrderBy(f => f.Name).ToList();
      var dirs = rootDirectory.GetDirectories().OrderBy(d => d.Name).ToList();

      for (int i = 0; i < folders.Count; i++)
      {
        Assert.AreEqual(dirs[i].Name, folders[i].Name);
      }

      //check sub folder
      var subDir = dirs[0];
      contents = provider.GetFolderContents(subDir.Name);

      folders = contents.Folders.OrderBy(f => f.Name).ToList();
      dirs = subDir.GetDirectories().OrderBy(d => d.Name).ToList();

      for (int i = 0; i < folders.Count; i++)
      {
        Assert.AreEqual(dirs[i].Name, folders[i].Name);
      }
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Returned_Item_Should_Provide_All_Found_Files()
    {
      //check root
      var contents = provider.GetFolderContents(root);
      Assert.AreEqual(contents.Files.Count(), rootDirectory.GetFiles().Count());

      var virtualFiles = contents.Files.OrderBy(f => f.Name).ToList();
      var fileInfos = rootDirectory.GetFiles().OrderBy(d => d.Name).ToList();

      for (int i = 0; i < virtualFiles.Count; i++)
      {
        Assert.AreEqual(fileInfos[i].Name, virtualFiles[i].Name);
        Assert.AreEqual(fileInfos[i].Length, virtualFiles[i].Length);
      }

      //check sub folder
      var subDir = rootDirectory.GetDirectories().First();
      contents = provider.GetFolderContents(subDir.Name);

      virtualFiles = contents.Files.OrderBy(f => f.Name).ToList();
      fileInfos = subDir.GetFiles().OrderBy(d => d.Name).ToList();

      for (int i = 0; i < virtualFiles.Count; i++)
      {
        Assert.AreEqual(fileInfos[i].Name, virtualFiles[i].Name);
      }
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    [ExpectedException(typeof(ResourceAccessException))]
    public void Requesting_Of_Contents_Outside_Scope_Should_Fail()
    {
      provider.GetFolderContents(rootDirectory.Parent.FullName);
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Requesting_Contents_Of_System_Root_Should_Provide_Drives_And_No_Files()
    {
      var sysProvider = new LocalFileSystemProvider();
      var r = sysProvider.GetFileSystemRoot();
      var contents = sysProvider.GetFolderContents(r);

      Assert.IsNotNull(contents.Files);
      Assert.AreEqual(0, contents.Files.Count());

      foreach (var f in contents.Folders)
      {
        DirectoryInfo d = new DirectoryInfo(f.FullName);
        Assert.IsNull(d.Parent);
      }

    }
  }
}
