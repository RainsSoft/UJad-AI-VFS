using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Vfs.LocalFileSystem.Test.Resource_Reading
{
  [TestFixture]
  public class Given_File_System_When_Retrieving_Resources : DirectoryTestBase
  {
    protected override void InitInternal()
    {
      File.Create(Path.Combine(rootDirectory.FullName, "foo.txt")).Close();
      File.Create(Path.Combine(rootDirectory.FullName, "bar.txt")).Close();
      File.Create(Path.Combine(rootDirectory.FullName, "foobar.txt")).Close();
      File.Create(Path.Combine(rootDirectory.FullName, "barfoo.txt")).Close();

      rootDirectory.CreateSubdirectory("dir_foo");
      rootDirectory.CreateSubdirectory("dir_bar");
      rootDirectory.CreateSubdirectory("dir_foobar");
      rootDirectory.CreateSubdirectory("dir_barfoo");
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Returned_Items_Should_Provide_Parent_Paths()
    {
      VirtualFolder root = VirtualFolder.CreateRootFolder(provider);
      RecurseFS(root, 
        (parent, file) => Assert.AreEqual(parent.MetaData.FullName, file.MetaData.ParentFolderPath),
        (parent, folder) => Assert.AreEqual(parent.MetaData.FullName, folder.MetaData.ParentFolderPath)
        );
    }


    private static void RecurseFS(VirtualFolder parent, Action<VirtualFolder, VirtualFile> fileTest, Action<VirtualFolder, VirtualFolder> folderTest)
    {
      var contents = parent.GetFolderContents();

      contents.Files.Do(vf => fileTest(parent, vf));
      contents.Folders.Do(vf => folderTest(parent, vf));

      contents.Folders.Do(vf => RecurseFS(vf, fileTest, folderTest));
    }


    [Test]
    public void Folders_Should_Indicate_Whether_They_Are_Empty_Or_Not()
    {
      VirtualFolder r = VirtualFolder.CreateRootFolder(provider);
      var folders = r.GetFolders();
      var files = r.GetFiles();

      bool testedEmpty = false;
      bool testedPopulated = false;

      folders.Do(f =>
      {
        bool empty = f.GetFiles().Count() == 0 && f.GetFolders().Count() == 0;
        if (empty)
          testedEmpty = true;
        else
          testedPopulated = true;

        Assert.AreEqual(empty, f.MetaData.IsEmpty);
        files.First().Copy(provider.CreateFilePath(f.MetaData.FullName, "test.txt"));
        f.RefreshMetaData();
        Assert.IsFalse(f.MetaData.IsEmpty);
      });

      Assert.IsTrue(testedEmpty);
      Assert.IsTrue(testedPopulated);
    }
  }
}
