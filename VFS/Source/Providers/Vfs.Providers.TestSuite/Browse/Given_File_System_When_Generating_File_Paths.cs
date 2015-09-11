using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using NUnit.Framework;
using Vfs.Test;


namespace Vfs.Providers.TestSuite.Browse
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_File_System_When_Generating_File_Paths : TestBase
  {

    [Test]
    public void Combining_Folder_Path_And_File_Name_Should_Construct_File_Path()
    {
      var file = Context.DownloadFolder.GetFiles().Last().MetaData;
      var path = FileSystem.CreateFilePath(Context.DownloadFolder.MetaData.FullName, file.Name);
      var copy = FileSystem.GetFileInfo(path);

      file.AssertXEqualTo(copy);
    }

    [Test]
    public void Using_Relative_Folder_Name_And_Folder_Should_Return_Relative_Path()
    {
      var folder = Context.DownloadFolder.AddFolder("some-child-folder").MetaData;
      var path = FileSystem.CreateFolderPath(Context.DownloadFolder.MetaData.FullName, folder.Name);
      var copy = FileSystem.GetFolderInfo(path);

      folder.AssertXEqualTo(copy);
    }
  }
}
