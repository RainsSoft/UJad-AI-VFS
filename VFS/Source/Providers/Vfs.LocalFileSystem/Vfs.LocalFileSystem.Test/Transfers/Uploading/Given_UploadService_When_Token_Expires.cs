using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

using NUnit.Framework;
using Vfs.Auditing;
using Vfs.LocalFileSystem.Transfer;
using Vfs.Transfer;
using Vfs.Util;


namespace Vfs.LocalFileSystem.Test.Transfers.Uploading
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_UploadService_When_Token_Expires : UploadServiceTestBase
  {
    private DateTimeOffset now;

    protected override void InitInternal()
    {
      base.InitInternal();

      //freeze time
      now = SystemTime.Now();
      SystemTime.Now = () => now;

      ((LocalUploadHandler) UploadHandler).ExpirationScheduler.SelfTestInterval = 100;

      InitToken();
    }



    [Test]
    public void Stream_Write_Should_Fail_As_Soon_As_Expiration_Date_Is_Reached()
    {
      //cancel transfer in order to release lock
      UploadHandler.CancelTransfer(Token.TransferId, AbortReason.ClientAbort);

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
        provider.WriteFile(SourceFile.FullName, TargetFile.FullName, false);
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
      UploadHandler.WriteBlock(block);
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
        UploadHandler.WriteBlock(block);
        Assert.Fail("Expected status exception due to expiration.");
      }
      catch (TransferStatusException expected)
      {
        Assert.AreEqual((int)AuditEvent.UploadNoLongerActive, expected.EventId);
      }
    }


    [Test]
    public void Underlying_Stream_Should_Have_Been_Closed_After_Throwing_Exception()
    {
      //write a block in order to open the stream
      var block = CreateBufferedBlocks().First();
      UploadHandler.WriteBlock(block);

      //make sure the scheduler can readjust
      SystemTime.Now = () => Token.ExpirationTime.Value.AddSeconds(1);
      Thread.CurrentThread.Join(500);

      //getting an exclusive stream only works if the file was released
      using (var fs = TargetFile.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
      {
        fs.Close();
      }
    }

    [Test]
    public void Starting_To_Write_After_Expiration_Should_Not_Open_The_Stream()
    {
      SystemTime.Now = () => Token.ExpirationTime.Value.AddSeconds(1);

      //make sure the scheduler can readjust
      Thread.CurrentThread.Join(500);

      //write a block in order to open the stream
      try
      {
        var block = CreateBufferedBlocks().First();
        UploadHandler.WriteBlock(block);
        Assert.Fail("Expected status exception");
      }
      catch(TransferStatusException ignored)
      { }

      //getting an exclusive stream only works if the file was released
      using (var fs = TargetFile.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
      {
        fs.Close();
      }
    }


    [Test]
    public void New_Token_Is_Being_Granted_After_Expiration()
    {
      SystemTime.Now = () => Token.ExpirationTime.Value.AddSeconds(1);

      //make sure the scheduler can readjust
      Thread.CurrentThread.Join(500);

      var token = UploadHandler.RequestUploadToken(TargetFilePath, false, SourceFile.Length, "");
      Assert.AreNotSame(token, Token);
      UploadHandler.CancelTransfer(token.TransferId, AbortReason.ClientAbort);
    }

  }
}
