using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using NUnit.Framework;
using Vfs.Test;


namespace Vfs.Restful.Test.Browse
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_Client_When_Browsing_Files_And_Folders : ServiceTestBase
  {
 

    [Test]
    public void Retrieved_Folders_Should_Be_Equal()
    {
      Traverse((clientFolder, serviceFolder) =>
                 {
                   //compare folders
                   serviceFolder.MetaData.AssertXEqualTo(clientFolder.MetaData);

                   //get child folder collection
                   var cFolders = clientFolder.GetFolders().ToArray();
                   var sFolders = serviceFolder.GetFolders().ToArray();
                   Assert.AreEqual(sFolders.Count(), cFolders.Count());
                 });
    }


    [Test]
    public void Retrieved_Files_Should_Be_Equal()
    {
      Traverse((clientFolder, serviceFolder) =>
      {
        //get child files
        var cFiles = clientFolder.GetFiles().ToArray();
        var sFiles = serviceFolder.GetFiles().ToArray();
        Assert.AreEqual(sFiles.Count(), cFiles.Count());

        //compare files
        for (int i = 0; i < sFiles.Length; i++)
        {
          var sFile = sFiles[i];
          var cFile = sFiles[i];
          sFile.MetaData.AssertXEqualTo(cFile.MetaData);
        }
      });
    }


    [Test]
    public void Requesting_File_Parent_Should_Return_Expected_Folder()
    {
      Traverse((clientFolder, serviceFolder) =>
                 {
                   var cFiles = clientFolder.GetFiles().ToArray();
                   var sFiles = serviceFolder.GetFiles().ToArray();

                   for (int i = 0; i < sFiles.Length; i++)
                   {
                     var sparent = sFiles[i].GetParentFolder();
                     var cparent = cFiles[i].GetParentFolder();
                     sparent.MetaData.AssertXEqualTo(cparent.MetaData);
                     clientFolder.MetaData.AssertXEqualTo(cparent.MetaData);
                   }
                 });
    }



    [Test]
    public void Requesting_File_Contents_Should_Return_Expected_Folders_And_Files()
    {
      Traverse((clientFolder, serviceFolder) =>
      {
        var sContents = serviceFolder.GetFolderContents();
        var cContents = clientFolder.GetFolderContents();

        Assert.AreEqual(cContents.Files.Count(), sContents.Files.Count());
        Assert.AreEqual(cContents.Folders.Count(), sContents.Folders.Count());
      });
    }


    [Test]
    public void Requesting_Filtered_Files_Should_Work()
    {
      int matches = 0;
      Traverse((clientFolder, serviceFolder) =>
      {
        var sFiles1 = serviceFolder.GetFiles("*.txt").ToArray();
        var sFiles2 = serviceFolder.GetFolderContents("*.txt").Files.ToArray();

        Assert.AreEqual(sFiles1.Count(), sFiles2.Count());
        sFiles1.Do(f => Assert.IsTrue(f.MetaData.Name.EndsWith(".txt")));
        sFiles2.Do(f => Assert.IsTrue(f.MetaData.Name.EndsWith(".txt")));
        matches += sFiles1.Count();
      });

      Assert.AreNotEqual(0, matches);
    }



    [Test]
    public void Requesting_Filtered_Folders_Should_Work()
    {
      for (int i = 0; i < RootDirectory.GetDirectories().Length; i++)
      {
        //create a few folders with a common naming scheme
        var dir = RootDirectory.GetDirectories()[i];
        dir.CreateSubdirectory("TestXXX" + i);
        dir.CreateSubdirectory("XXXTest" + i);
      }

      int matches = 0;
      Traverse((clientFolder, serviceFolder) =>
      {
        var sFolders1 = serviceFolder.GetFolders("*XXX*").ToArray();
        var sFolders2 = serviceFolder.GetFolderContents("*XXX*").Folders.ToArray();

        Assert.AreEqual(sFolders1.Count(), sFolders2.Count());
        sFolders1.Do(f => Assert.IsTrue(f.MetaData.Name.Contains("XXX")));
        sFolders2.Do(f => Assert.IsTrue(f.MetaData.Name.Contains("XXX")));

        if(serviceFolder.MetaData.IsRootFolder)
        {
          //root folder doesn't have matches
          Assert.IsEmpty(sFolders1);
          Assert.IsEmpty(sFolders2);
        }

        matches += sFolders1.Count();
      });

      Assert.AreEqual(6, matches);
    }


    
    [Test]
    public void Requesting_Folder_Parent_Should_Return_Expected_Folder()
    {
      Traverse((clientFolder, serviceFolder) =>
      {
        Assert.AreEqual(clientFolder.MetaData.IsRootFolder, serviceFolder.MetaData.IsRootFolder);
        if (serviceFolder.MetaData.IsRootFolder)
        {
          //invalid request for root has its own test
          return;
        }

        VirtualFolder clientParent = clientFolder.GetParentFolder();
        VirtualFolder serviceParent = serviceFolder.GetParentFolder();
        serviceParent.MetaData.AssertXEqualTo(clientParent.MetaData);
      });
    }

  }
}
