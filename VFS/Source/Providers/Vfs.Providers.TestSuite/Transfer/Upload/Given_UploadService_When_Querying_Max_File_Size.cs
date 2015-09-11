using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using NUnit.Framework;


namespace Vfs.Providers.TestSuite.Transfer.Upload
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_UploadService_When_Querying_Max_File_Size : UploadTestBase
  {
    [Test]
    public void Service_Should_Return_Result()
    {
      var maxSize = Uploads.GetMaxFileUploadSize();
      string msg = "Getting a maximum file upload size works [{0}], but cannot verify whether the result is valid.";
      msg = String.Format(msg, maxSize.HasValue ? maxSize.Value.ToString() : "null");
      Assert.Inconclusive(msg);
    }

  }
}
