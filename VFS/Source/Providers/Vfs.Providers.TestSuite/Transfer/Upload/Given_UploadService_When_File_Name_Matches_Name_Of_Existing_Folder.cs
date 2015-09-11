using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using NUnit.Framework;
using Vfs.Util;


namespace Vfs.Providers.TestSuite.Transfer.Upload
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_UploadService_When_File_Name_Matches_Name_Of_Existing_Folder : UploadTestBase
  {

    [Test]
    [ExpectedException(typeof(ResourceAccessException))]
    public void Requesting_For_Token_Should_Be_Denied()
    {
      Uploads.RequestUploadToken(Context.DownloadFolder.MetaData.FullName, true, 5000, ContentUtil.UnknownContentType);
    }

  }
}
