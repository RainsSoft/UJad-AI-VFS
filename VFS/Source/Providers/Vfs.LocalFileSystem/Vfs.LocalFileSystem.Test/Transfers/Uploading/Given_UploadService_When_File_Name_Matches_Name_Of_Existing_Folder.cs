using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using NUnit.Framework;


namespace Vfs.LocalFileSystem.Test.Transfers.Uploading
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_UploadService_When_File_Name_Matches_Name_Of_Existing_Folder : UploadServiceTestBase
  {

    [Test]
    [ExpectedException(typeof(ResourceAccessException))]
    public void Requesting_For_Token_Should_Be_Denied()
    {
      var subDir = ParentDirectory.CreateSubdirectory("SomeFolder");
      UploadHandler.RequestUploadToken(subDir.FullName, true, SourceFilePath.Length, "bin");
    }

  }
}
