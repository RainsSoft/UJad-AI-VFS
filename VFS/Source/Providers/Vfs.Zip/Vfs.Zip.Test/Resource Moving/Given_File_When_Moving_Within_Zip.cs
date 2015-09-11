using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Xml;

using NUnit.Framework;


namespace Vfs.Zip.Test.Resource_Moving
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_File_When_Moving_Within_Zip : ZipTestBase
  {
    [Test]
    public void Moving_Into_New_Folder_Should_Adjust_File()
    {
      VirtualFolder root = VirtualFolder.CreateRootFolder(Provider);
      var movedFolder = root.GetFolders().First();

      foreach (var virtualFile in movedFolder.GetFiles())
      {
        Console.Out.WriteLine("File: " + virtualFile.MetaData.Name + ", " + virtualFile.MetaData.FullName);
      }

      var testFolder = root.AddFolder("Test");

      string targetPath = Provider.CreateFolderPath("/", "Nested");
      movedFolder.Move(targetPath);

      Console.Out.WriteLine("movedFolder = {0}", movedFolder.MetaData.FullName);
      foreach (var virtualFile in movedFolder.GetFiles())
      {
        Console.Out.WriteLine("File: " + virtualFile.MetaData.Name + ", " + virtualFile.MetaData.FullName);
      }
    }


    [Test]
    public void DummyTest()
    {
      var zip = Provider.NodeRepository.ZipFile;

      //PART1 - Add directory and save
      zip.AddDirectoryByName("XX");
      zip.Save();

      //PART2 - Rename paths (not related to XX directory from above) and save
      var entries = zip.Entries.Where(e => e.FileName.Contains("Download")).ToArray();
      foreach (var zipEntry in entries)
      {
        zipEntry.FileName = zipEntry.FileName.Replace("Download", "Download2");
      }
      zip.Save();

      Thread.CurrentThread.Join(5000);
    }


    [Test]
    public void UploadFileDummy()
    {
      var file = VirtualFolder.CreateRootFolder(Provider).GetFolders().First().GetFolders().First().GetFiles().First();
      Console.Out.WriteLine("file.MetaData.FullName = {0}", file.MetaData.Length);

      var t1 = Provider.DownloadHandler.RequestDownloadToken(file.MetaData.FullName, false);
      var t2 = Provider.DownloadHandler.RequestDownloadToken(file.MetaData.FullName, false);

      Provider.DownloadHandler.ReadBlock(t1.TransferId, 0);
      Provider.DownloadHandler.ReadBlock(t2.TransferId, 0);
    }
  }
}
