using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Vfs.LocalFileSystem;

namespace Vfs.Test
{
  [TestFixture]
  public class Given_VirtualFolder_When_Browsing_FileSystems
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
    public void Recursively_Browsing_Root_Should_Correlate_To_Filesystem()
    {
      BrowseFolder(root, rootDirectory);
    }

    private void BrowseFolder(VirtualFolder parentFolder, DirectoryInfo parentDir)
    {
      var contents = parentFolder.GetFolderContents();
      var folders = parentFolder.GetFolders();
      var files = parentFolder.GetFiles();

      Assert.AreEqual(contents.Folders.Count(), folders.Count());
      Assert.AreEqual(contents.Files.Count(), files.Count());


      Assert.AreEqual(parentDir.GetDirectories().Count(), contents.Folders.Count());
      Assert.AreEqual(parentDir.GetFiles().Count(), contents.Files.Count());

      foreach (var file in files)
      {
        //make sure contents matches the files collection
        VirtualFile virtualFile = file;
        Assert.AreEqual(contents.Files.Single(f => f.MetaData.FullName == virtualFile.MetaData.FullName).MetaData.Length, file.MetaData.Length);

        FileInfo fi = parentDir.GetFiles(file.MetaData.Name).Single();
        Assert.AreEqual(file.MetaData.Length, fi.Length);
      }

      foreach (var folder in contents.Folders)
      {
        //make sure contents matches the folders collection
        VirtualFolder virtualFolder = folder;
        Assert.AreEqual(contents.Folders.Single(f => f.MetaData.FullName == virtualFolder.MetaData.FullName).MetaData.Name, folder.MetaData.Name);


        DirectoryInfo dir = parentDir.GetDirectories(folder.MetaData.Name).Single();
        Assert.True(dir.Exists);

        //browse recursively
        BrowseFolder(folder, dir);
      }
    }



    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Getting_Parent_Should_Return_Expected_Folder()
    {
      var childFolder = root.GetFolders().First();
      var parent = childFolder.GetParentFolder();
      Assert.True(parent.MetaData.IsRootFolder);

      var file = parent.GetFiles().First();
      var parent2 = file.GetParentFolder();
      Assert.AreEqual(parent.MetaData.FullName, parent2.MetaData.FullName);
    }
  }
}
