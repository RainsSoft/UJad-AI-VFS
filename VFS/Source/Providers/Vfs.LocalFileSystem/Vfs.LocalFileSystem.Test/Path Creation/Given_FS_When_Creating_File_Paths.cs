using System;
using System.Collections.Generic;
using System.IO;
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
  public class Given_FS_When_Creating_File_Paths
  {
    private IFileSystemProvider provider = new LocalFileSystemProvider();

    [Test]
    public void Using_Relative_Folder_Name_And_File_Should_Return_Relative_Path ()
    {
      string folder = @"C:\myfolder";
      string file = "myfile.txt";

      string expected = String.Format("{0}{1}{2}", folder, Path.DirectorySeparatorChar, file);
      Assert.AreEqual(expected, provider.CreateFilePath(folder, file));
    }



    [Test]
    public void Using_Absolute_Folder_Name_And_File_Should_Return_Absolute_Path()
    {
      string folder = "myfolder";
      string file = "myfile.txt";

      string expected = String.Format("{0}{1}{2}", folder, Path.DirectorySeparatorChar, file);
      Assert.AreEqual(expected, provider.CreateFilePath(folder, file));
    }

    [Test]
    public void Using_Relative_Folder_Name_And_Folder_Should_Return_Relative_Path()
    {
      string folder = @"C:\myfolder";
      string childFolder = "mychildfolder";

      string expected = String.Format("{0}{1}{2}", folder, Path.DirectorySeparatorChar, childFolder);
      Assert.AreEqual(expected, provider.CreateFolderPath(folder, childFolder));
    }



    [Test]
    public void Using_Absolute_Folder_Name_And_Folder_Should_Return_Absolute_Path()
    {
      string folder = "myfolder";
      string childFolder = "mychildfolder";

      string expected = String.Format("{0}{1}{2}", folder, Path.DirectorySeparatorChar, childFolder);
      Assert.AreEqual(expected, provider.CreateFolderPath(folder, childFolder));
    }

  }
}
