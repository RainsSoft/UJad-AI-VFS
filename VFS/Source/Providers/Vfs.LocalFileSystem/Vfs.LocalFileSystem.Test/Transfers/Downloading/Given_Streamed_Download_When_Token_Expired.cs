using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;

using NUnit.Framework;
using Vfs.Auditing;
using Vfs.LocalFileSystem.Transfer;
using Vfs.Transfer;
using Vfs.Util;


namespace Vfs.LocalFileSystem.Test.Transfers.Downloading
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_Streamed_Download_When_Token_Expired : DownloadServiceTestBase
  {
    private DateTimeOffset now;

    protected override void InitInternal()
    {
      base.InitInternal();

      //freeze time
      now = SystemTime.Now();
      SystemTime.Now = () => now;

      DownloadHandler.ExpirationScheduler.SelfTestInterval = 100;
      InitToken();
    }



    [Test]
    public void Stream_Read_Should_Fail_As_Soon_As_Expiration_Date_Is_Reached ()
    {
      try
      {
        using(var stream = DownloadHandler.ReadFile(SourceFileInfo.FullName))
        {
          byte[] buffer = new byte[20000];
          Assert.AreEqual(buffer.Length, stream.Read(buffer, 0, buffer.Length));

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


    [ExpectedException(typeof(TransferStatusException))]
    [Test]
    public void Requesting_New_Block_Should_Fail_Once_The_Token_Expired()
    {
      SystemTime.Now = () => Token.ExpirationTime.Value.AddSeconds(1);

      //make sure the scheduler can readjust
      Thread.CurrentThread.Join(500);

      DownloadHandler.ReadBlock(Token.TransferId, 0);
    }


    [Test]
    public void Status_Exception_Should_Provide_Expiration_Event_Id()
    {
      SystemTime.Now = () => Token.ExpirationTime.Value.AddSeconds(1);

      //make sure the scheduler can readjust
      Thread.CurrentThread.Join(500);

      try
      {
        DownloadHandler.ReadBlock(Token.TransferId, 0);
        Assert.Fail("Expected status exception due to expiration.");
      }
      catch (TransferStatusException expected)
      {
        Assert.AreEqual((int)AuditEvent.DownloadNoLongerActive, expected.EventId);
      }
    }




    [Test]
    public void Underlying_Stream_Should_Have_Been_Closed_After_Throwing_Exception()
    {
      SystemTime.Now = () => Token.ExpirationTime.Value.AddSeconds(1);

      //make sure the scheduler can readjust
      Thread.CurrentThread.Join(500);

      try
      {
        DownloadHandler.ReadBlock(Token.TransferId, 0);
        Assert.Fail("Expected status exception due to expiration.");
      }
      catch (TransferStatusException expected)
      {
      }

      //ensure we get access to the file
      using(var fs = SourceFile.Open(FileMode.Open, FileAccess.Write, FileShare.None))
      {
        fs.Close();
      }
    }

  }
}
