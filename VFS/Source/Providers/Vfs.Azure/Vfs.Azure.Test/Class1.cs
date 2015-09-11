using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Vfs.Azure;
using Vfs.Util;

namespace Vfs.Test
{
  [TestFixture]
  public class Given_VirtualFolder_When_Modifying_Files_And_Folders
  {
    //private IFileSystemProvider provider;
    //private DirectoryInfo rootDirectory;
    //private VirtualFolder root;

    //[SetUp]
    //public void Init()
    //{
    //  rootDirectory = TestUtil.CreateTestDirectory();

    //  //init provider
      
    //  root = VirtualFolder.CreateRootFolder(provider);

    //}


    //[TearDown]
    //public void Cleanup()
    //{
    //  rootDirectory.Refresh();
    //  if (rootDirectory.Exists) rootDirectory.Delete(true);
    //}


    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Creating_A_Folder_And_Adding_A_File_Should_Create_Items_On_File_System()
    {
      AzureFileSystemConfiguration config = AzureFileSystemConfiguration.CreateForDevelopmentStorage();
      var container = config.BlobClient.GetContainerReference("mycontainer");
      container.Create();
      var md = container.Metadata;
    }

  }

}
