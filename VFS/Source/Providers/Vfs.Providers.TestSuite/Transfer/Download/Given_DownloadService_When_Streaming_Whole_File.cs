using System;
using System.IO;
using System.Threading;
using NUnit.Framework;
using Vfs.Locking;
using Vfs.Transfer;
using Vfs.Util;


namespace Vfs.Providers.TestSuite.Transfer.Download
{
  [TestFixture]
  public class Given_DownloadService_When_Streaming_Whole_File : DownloadTestBase
  {

    [Test]
    public void Read_Data_Should_Match_Source_File()
    {     
      //get stream and write to file
      using (Stream stream = Downloads.ReadFile(SourceFileInfo.FullName))
      {
        stream.WriteTo(TargetFilePath);
      }

      //calculate hashes of source and target and compare
      string sourceHash = GetFileHash(SourceFileInfo);
      string targetHash = new FileInfo(TargetFilePath).CalculateMd5Hash();
      Assert.AreEqual(sourceHash, targetHash);
    }


    [Test]
    public void Read_Data_From_Download_Token_Should_Match_Source_File()
    {
      //get stream and write to file
      var token = InitToken();
      using (Stream stream = Downloads.DownloadFile(token.TransferId))
      {
        stream.WriteTo(TargetFilePath);
      }

      //calculate hashes of source and target and compare
      string sourceHash = GetFileHash(SourceFileInfo);
      string targetHash = new FileInfo(TargetFilePath).CalculateMd5Hash();
      Assert.AreEqual(sourceHash, targetHash);
    }



    [Test]
    public void Aborted_Transfer_Should_Cause_Exception_While_Reading_File_Even_If_File_Is_Streamed_At_Once()
    {
      //behind the scenes, many blocks are read, independent of the buffer size. Accordingly,
      //the transfer will abort as soon as we want it

      var token = InitToken();
      using (Stream stream = Downloads.DownloadFile(token.TransferId))
      {
        byte[] buffer = new byte[1234];
        stream.Read(buffer, 0, buffer.Length);
        int r = stream.Read(buffer, 1000, 234);
        Assert.AreEqual(234, r);

        //certain providers may already unlock the file - in this case,
        //we won't be able to complete this test
        var status = Downloads.GetTransferStatus(token.TransferId);
        if (status == TransferStatus.Completed) return;

        //abort the transfer
        Downloads.CancelTransfer(token.TransferId, AbortReason.ClientAbort);

        //try to read again
        try
        {
          var data = stream.ReadIntoBuffer();
          Assert.Fail("Expected exception when attempting to read data after cancelling transfer.");
        }
        catch (TransferStatusException expected)
        {
        }
        //stream.Read(buffer, 0, buffer.Length);
      }
    }


    [Test]
    public void Completed_Transfer_Should_Unlock_File()
    {
      //get stream and write to file
      using (Stream stream = Downloads.ReadFile(SourceFileInfo.FullName))
      {
        stream.WriteTo(TargetFilePath);
      }

      Assert.IsTrue(SourceFile.Exists);
      SourceFile.Delete();
    }


    [Test]
    public void Aborted_Transfer_Should_Unlock_Immediately()
    {
      //behind the scenes, many blocks are read, independent of the buffer size. Accordingly,
      //the transfer will abort as soon as we want it
      string resourceId = SourceFileInfo.FullName;

      var token = InitToken();
      using (Stream stream = Downloads.DownloadFile(token.TransferId))
      {
        byte[] buffer = new byte[1234];
        stream.Read(buffer, 0, buffer.Length);
        int r = stream.Read(buffer, 1000, 234);
        Assert.AreEqual(234, r);

        //abort the transfer
        Downloads.CancelTransfer(token.TransferId, AbortReason.ClientAbort);

        //try to get upload token
        var ut = FileSystem.UploadTransfers.RequestUploadToken(resourceId, true, 1000, "");
        FileSystem.UploadTransfers.CancelTransfer(ut.TransferId, AbortReason.ClientAbort);
      }
    }



    [Test]
    public void Getting_The_Stream_Should_Lock_File()
    {
      //make the stream big enough so it won't be auto-closed
      //on the server side
      byte[] buffer = new byte[1024*1024*200];
      File.WriteAllBytes(Context.DownloadFile0Template.FullName, buffer);
      Context.DownloadFile0Template.Refresh();
      FileSystem.WriteFile(Context.DownloadFile0Template.FullName, SourceFileInfo.FullName, true);

      var token = InitToken();
      using (Stream stream = Downloads.DownloadFile(token.TransferId))
      {
        //certain providers may already unlock the file - in this case,
        //we won't be able to complete this test
        var status = Downloads.GetTransferStatus(token.TransferId);
        if(status == TransferStatus.Completed) return;

        try
        {
          //if we request to delete the file, this should fail
          SourceFile.Delete();
          Assert.Fail("Could delete file although it should be locked.");
        }
        catch (ResourceLockedException e)
        {
        }
      }
    }


    [Test]
    public void Closing_Stream_Should_Unlock_File()
    {
      string resourceId = SourceFileInfo.FullName.ToLowerInvariant();
      using (Stream stream = Downloads.ReadFile(resourceId))
      {

        byte[] buffer = new byte[6000];
        stream.Read(buffer, 0, buffer.Length);

        //close the stream
        stream.Close();
      }

      //attempting to delete the file should work now
      Thread.CurrentThread.Join(1000);
      SourceFile.Delete();
    }


  }
}