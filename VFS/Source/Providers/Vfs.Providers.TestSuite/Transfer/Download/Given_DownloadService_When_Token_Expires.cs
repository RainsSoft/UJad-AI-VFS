using System;
using System.Threading;
using NUnit.Framework;
using Vfs.Scheduling;
using Vfs.Transfer;
using Vfs.Util;


namespace Vfs.Providers.TestSuite.Transfer.Download
{
  [TestFixture]
  public class Given_DownloadService_When_Token_Expires : DownloadTestBase
  {
    private DateTimeOffset now;

    protected override void InitInternal()
    {
      base.InitInternal();

      //freeze time
      now = SystemTime.Now();
      SystemTime.Now = () => now;

      //try to get a scheduler
      Scheduler scheduler = Context.TryGetDownloadExpirationScheduler();
      if (scheduler == null)
      {
        Assert.Inconclusive("Could not run test - file system does not provide access to its expiration scheduler.");
        return;
      }

      scheduler.SelfTestInterval = 100;

      InitToken();
    }


    [Test]
    [ExpectedException(typeof(TransferStatusException))]
    public void Request_Should_Throw_Expiration_Exception()
    {
      //expire and wait for the scheduler to readjust
      SystemTime.Now = () => Token.ExpirationTime.Value.AddSeconds(1);
      Thread.CurrentThread.Join(500);

      Downloads.ReadBlock(Token.TransferId, 0);
    }


    [Test]
    public void Resource_And_Parent_Folder_Should_Have_Been_Unlocked()
    {
      //expire and wait for the scheduler to readjust
      SystemTime.Now = () => Token.ExpirationTime.Value.AddSeconds(1);
      Thread.CurrentThread.Join(500);

      //get upload token for resource - only granted in case there's no locks
      var token = FileSystem.UploadTransfers.RequestUploadToken(SourceFileInfo.FullName, true, 5000, "");
      FileSystem.UploadTransfers.CancelTransfer(token.TransferId, AbortReason.ClientAbort);

      //try to delete parent folder
      FileSystem.DeleteFolder(SourceFileInfo.ParentFolderPath);
    }



    [ExpectedException(typeof(TransferStatusException))]
    [Test]
    public void Requesting_New_Block_Should_Fail_Once_The_Token_Expired()
    {
      try
      {
        Downloads.ReadBlock(Token.TransferId, 0);
      }
      catch
      {
        Assert.Fail("Reading first block should have worked.");
      }

      //expire and wait for the scheduler to readjust
      SystemTime.Now = () => Token.ExpirationTime.Value.AddSeconds(1);
      Thread.CurrentThread.Join(500);

      Downloads.ReadBlock(Token.TransferId, 1);
    }


    [Test]
    public void Write_Request_Should_Be_Granted_Without_Cancelling_Download()
    {
      try
      {
        FileSystem.UploadTransfers.RequestUploadToken(SourceFileInfo.FullName, true, 5000, "");
        Assert.Fail("Got upload token with active download.");
      }
      catch (ResourceLockedException expected)
      {
      }

      //expire and wait for the scheduler to readjust
      SystemTime.Now = () => Token.ExpirationTime.Value.AddSeconds(1);
      Thread.CurrentThread.Join(500);

      //this time, it should work
      var token = FileSystem.UploadTransfers.RequestUploadToken(SourceFileInfo.FullName, true, 5000, "");
      FileSystem.UploadTransfers.CancelTransfer(token.TransferId, AbortReason.ClientAbort);
    }



    [Test]
    public void Stream_Read_Should_Fail_As_Soon_As_Expiration_Date_Is_Reached()
    {
      try
      {
        using (var stream = Downloads.ReadFile(SourceFileInfo.FullName))
        {
          byte[] buffer = new byte[20000];
          stream.Read(buffer, 0, buffer.Length);

          //expire
          SystemTime.Now = () => Token.ExpirationTime.Value.AddSeconds(1);
          Thread.CurrentThread.Join(500);

          stream.Read(buffer, 0, buffer.Length);
        }
      }
      catch (TransferStatusException expected)
      {
        Console.Out.WriteLine(expected.Message);
      }
    }

  }
}