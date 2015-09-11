using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

using NUnit.Framework;


namespace Vfs.Providers.TestSuite.Browse
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_Provider_When_Submitting_Illegal_Uris : TestBase
  {

    /* TESTS SUBMITTING ILLEGAL FILE PATHS */

    private VirtualFileInfo illegalFile;
    private VirtualFolderInfo illegalFolder;


    protected override void InitInternal()
    {

      illegalFile = new VirtualFileInfo { FullName = "a?:<>@*%&/" };
      illegalFolder = new VirtualFolderInfo { FullName = "a?:<>@*%&/" };
    }



    [Test]
    [ExpectedException(typeof(InvalidResourcePathException))]
    public void Requesting_File_With_Illegal_Uri_Should_Fail()
    {
      provider.GetFileInfo(illegalFile.FullName);
    }

    [Test]
    [ExpectedException(typeof(InvalidResourcePathException))]
    public void Requesting_Folder_With_Illegal_Uri_Should_Fail()
    {
      provider.GetFileInfo(illegalFolder.FullName);
    }


    [Test]
    [ExpectedException(typeof(InvalidResourcePathException))]
    public void Requesting_Parent_Folder_Of_Illegal_File_Should_Fail()
    {
      provider.GetFileParent(illegalFile);
    }


    [Test]
    [ExpectedException(typeof(InvalidResourcePathException))]
    public void Requesting_Parent_Folder_Of_Illegal_Folder_Should_Fail()
    {
      provider.GetFolderParent(illegalFolder);
    }

    [Test]
    [ExpectedException(typeof(InvalidResourcePathException))]
    public void Requesting_Child_Folders_Of_Illegal_Folder_Should_Fail()
    {
      provider.GetChildFolders(illegalFolder);
    }

    [Test]
    [ExpectedException(typeof(InvalidResourcePathException))]
    public void Requesting_Child_Files_Of_Illegal_Folder_Should_Fail()
    {
      provider.GetChildFiles(illegalFolder);
    }

    [Test]
    [ExpectedException(typeof(InvalidResourcePathException))]
    public void Submitting_Read_Request_For_Illegal_File_Should_Fail()
    {
      provider.ReadFileContents(illegalFile);
    }


  }
}
