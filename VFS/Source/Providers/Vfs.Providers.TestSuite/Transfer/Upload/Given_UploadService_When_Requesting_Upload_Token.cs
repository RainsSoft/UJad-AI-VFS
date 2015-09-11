using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

using NUnit.Framework;
using Vfs.Transfer;


namespace Vfs.Providers.TestSuite.Transfer.Upload
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_UploadService_When_Requesting_Upload_Token : UploadTestBase
  {

    [Test]
    public void Transfer_Should_Be_Denied_If_Download_Is_Already_Running()
    {
      //create file
      InitToken();
      var blocks = CreateBufferedBlocks();
      blocks.Do(Uploads.WriteBlock);
      Uploads.CompleteTransfer(Token.TransferId);
      
      //request download
      var downloadToken = FileSystem.DownloadTransfers.RequestDownloadToken(TargetFilePath, false);

      try
      {
        Token = Uploads.RequestUploadToken(TargetFilePath, true, 5000, "");
        Assert.Fail("Got upload token while download is running.");
      }
      catch (ResourceLockedException expected)
      {
      }
      finally
      {
        FileSystem.DownloadTransfers.CancelTransfer(downloadToken.TransferId, AbortReason.ClientAbort);
      }

      //after downloading, it should work
      Token = Uploads.RequestUploadToken(TargetFilePath, true, 5000, "");
    }


    [Test]
    public void Transfer_Should_Be_Denied_If_Upload_Is_Already_Running()
    {
      InitToken();

      try
      {
        var refusedToken = Uploads.RequestUploadToken(TargetFilePath, true, 5000, "");
        Assert.Fail("Got upload token while download is running.");
      }
      catch (ResourceLockedException expected)
      {
      }
      finally
      {
        Uploads.CancelTransfer(Token.TransferId, AbortReason.ClientAbort);
      }

      //after the canceling, it should work
      InitToken();
    }



    [ExpectedException(typeof(ResourceAccessException))]
    [Test]
    public void Transfer_Should_Be_Denied_If_Requested_Upload_Size_Is_Above_Maximum_File_Size()
    {
      long? maxSize = Uploads.GetMaxFileUploadSize();
      if(!maxSize.HasValue || maxSize.Value == long.MaxValue)
      {
        Assert.Inconclusive("Cannot test against max file size - the provider does not set a limit");
        return;
      }

      Token = Uploads.RequestUploadToken(TargetFilePath, false, maxSize.Value + 1, "");
    }


    [ExpectedException(typeof(ResourceAccessException))]
    [Test]
    public void Transfer_Should_Be_Denied_If_Requested_Upload_Size_Is_Negative()
    {
      Token = Uploads.RequestUploadToken(TargetFilePath, false, -1, "");
    }
    
  }
}
