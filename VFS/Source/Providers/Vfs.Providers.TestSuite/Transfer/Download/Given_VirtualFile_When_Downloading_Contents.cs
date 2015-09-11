using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Hardcodet.Commons.IO;
using NUnit.Framework;
using Vfs.Util;


namespace Vfs.Providers.TestSuite.Transfer.Download
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_VirtualFile_When_Downloading_Contents : DownloadTestBase
  {

    [Test]
    public void Downloaded_Stream_Should_Match_Source_Data()
    {
      using(var stream = SourceFile.GetContents())
      {
        Assert.IsFalse(File.Exists(TargetFilePath));
        stream.WriteTo(TargetFilePath);
        Assert.IsTrue(File.Exists(TargetFilePath));
      }

      FileInfo fi = new FileInfo(TargetFilePath);
      FileAssert.AreEqual(Context.DownloadFile0Template, fi);
    }


    [Test]
    public void Saving_To_Path_Should_Create_Exact_Copy()
    {
      Assert.IsFalse(File.Exists(TargetFilePath));
      SourceFile.SaveContentsAs(TargetFilePath);
      Assert.IsTrue(File.Exists(TargetFilePath));

      FileInfo fi = new FileInfo(TargetFilePath);
      FileAssert.AreEqual(Context.DownloadFile0Template, fi);
    }
  }
}
