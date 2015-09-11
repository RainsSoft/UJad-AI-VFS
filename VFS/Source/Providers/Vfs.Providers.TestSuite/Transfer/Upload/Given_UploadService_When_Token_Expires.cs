using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

using NUnit.Framework;
using Vfs.Auditing;
using Vfs.Scheduling;
using Vfs.Transfer;
using Vfs.Util;


namespace Vfs.Providers.TestSuite.Transfer.Upload
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_UploadService_When_Token_Expires : UploadTestBase
  {
    private DateTimeOffset now;
    

    protected override void InitInternal()
    {
      base.InitInternal();

      //freeze time
      now = SystemTime.Now();
      SystemTime.Now = () => now;

      //try to get a scheduler
      Scheduler scheduler = Context.TryGetUploadExpirationScheduler();
      if (scheduler == null)
      {
        Assert.Inconclusive("Could not run test - file system does not provide access to its expiration scheduler.");
        return;
      }

      scheduler.SelfTestInterval = 100;
      InitToken();
    }


    private void EnsureTargetFileIsUnlocked()
    {
      var token = Uploads.RequestUploadToken(TargetFilePath, true, 5000, "");
      Uploads.CancelTransfer(token.TransferId, AbortReason.ClientAbort);
    }


    [Test]
    public void Stream_Write_Should_Fail_As_Soon_As_Expiration_Date_Is_Reached()
    {
      //cancel transfer in order to release lock
      Uploads.CancelTransfer(Token.TransferId, AbortReason.ClientAbort);

      //make source file rather big (400MB)
      File.WriteAllBytes(SourceFile.FullName, new byte[1024*1024*400]);
      GC.Collect();

      //make the transfer expire
      ThreadPool.QueueUserWorkItem(s =>
      {
        Thread.Sleep(50);
        SystemTime.Now = () => Token.ExpirationTime.Value.AddSeconds(1);
      });

      //start streaming the file
      try
      {
        FileSystem.WriteFile(SourceFile.FullName, TargetFilePath, false);
        Assert.Fail("Expected status exception due to expiration.");
      }
      catch(TransferStatusException expected)
      {
      }

      //make sure the file is unlocked
      EnsureTargetFileIsUnlocked();
    }




    [ExpectedException(typeof(TransferStatusException))]
    [Test]
    public void Submitting_New_Block_Should_Fail_Once_The_Token_Expired()
    {
      SystemTime.Now = () => Token.ExpirationTime.Value.AddSeconds(1);

      //make sure the scheduler can readjust
      Thread.CurrentThread.Join(500);

      var block = CreateBufferedBlocks().First();
      Uploads.WriteBlock(block);
    }


    [Test]
    public void Status_Exception_Should_Provide_Expiration_Event_Id()
    {
      SystemTime.Now = () => Token.ExpirationTime.Value.AddSeconds(1);

      //make sure the scheduler can readjust
      Thread.CurrentThread.Join(500);

      try
      {
        var block = CreateBufferedBlocks().First();
        Uploads.WriteBlock(block);
        Assert.Fail("Expected status exception due to expiration.");
      }
      catch (TransferStatusException expected)
      {
        Assert.AreEqual((int)AuditEvent.UploadNoLongerActive, expected.EventId);
      }
    }


    [Test]
    public void File_Fragments_Should_Have_Been_Removed_After_Expiration()
    {
      //write a block in order to open the stream
      var block = CreateBufferedBlocks().First();
      Uploads.WriteBlock(block);

      //make sure the scheduler can readjust
      SystemTime.Now = () => Token.ExpirationTime.Value.AddSeconds(1);
      Thread.CurrentThread.Join(500);

      Assert.IsFalse(FileSystem.IsFileAvailable(TargetFilePath));
    }


    [Test]
    public void Starting_To_Write_After_Expiration_Should_Not_Create_A_File()
    {
      SystemTime.Now = () => Token.ExpirationTime.Value.AddSeconds(1);

      //make sure the scheduler can readjust
      Thread.CurrentThread.Join(500);

      //write a block in order to open the stream
      try
      {
        var block = CreateBufferedBlocks().First();
        Uploads.WriteBlock(block);
        Assert.Fail("Expected status exception");
      }
      catch(TransferStatusException ignored)
      { }

      Assert.IsFalse(FileSystem.IsFileAvailable(TargetFilePath));
    }


    [Test]
    public void New_Token_Is_Being_Granted_After_Expiration()
    {
      SystemTime.Now = () => Token.ExpirationTime.Value.AddSeconds(1);

      //make sure the scheduler can readjust
      Thread.CurrentThread.Join(500);

      var token = Uploads.RequestUploadToken(TargetFilePath, false, SourceFile.Length, "");
      Assert.AreNotSame(token, Token);
      Uploads.CancelTransfer(token.TransferId, AbortReason.ClientAbort);
    }

  }
}
