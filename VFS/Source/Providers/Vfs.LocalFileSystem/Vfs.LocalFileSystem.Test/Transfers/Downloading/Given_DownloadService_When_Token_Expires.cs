using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml;

using NUnit.Framework;
using Vfs.LocalFileSystem.Transfer;
using Vfs.Locking;
using Vfs.Transfer;
using Vfs.Util;


namespace Vfs.LocalFileSystem.Test.Transfers.Downloading
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_DownloadService_When_Token_Expires : DownloadServiceTestBase
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
    [ExpectedException(typeof(TransferStatusException))]
    public void Request_Should_Throw_Expiration_Exception()
    {
      //expire and wait for the scheduler to readjust
      SystemTime.Now = () => Token.ExpirationTime.Value.AddSeconds(1);
      Thread.CurrentThread.Join(500);

      DownloadHandler.ReadBlock(Token.TransferId, 0);
    }

    [Test]
    public void Resource_And_Parent_Folder_Should_Have_Been_Unlocked()
    {
      Assert.AreEqual(ResourceLockState.ReadOnly, LockRepository.GetLockState(SourceFile.FullName.ToLowerInvariant()));
      Assert.AreEqual(ResourceLockState.ReadOnly, LockRepository.GetLockState(SourceFile.Directory.FullName.ToLowerInvariant()));

      //expire and wait for the scheduler to readjust
      SystemTime.Now = () => Token.ExpirationTime.Value.AddSeconds(1);
      Thread.CurrentThread.Join(500);

      Assert.AreEqual(ResourceLockState.Unlocked, LockRepository.GetLockState(SourceFile.FullName.ToLowerInvariant()));
      Assert.AreEqual(ResourceLockState.Unlocked, LockRepository.GetLockState(SourceFile.Directory.FullName.ToLowerInvariant()));
    }


    [Test]
    public void Write_Request_Should_Be_Granted_Without_Cancelling_Download()
    {
      try
      {
        provider.UploadTransfers.RequestUploadToken(SourceFileInfo.FullName, true, 5000, "");
        Assert.Fail("Got upload token with active download.");
      }
      catch (ResourceLockedException expected)
      {
      }

      //expire and wait for the scheduler to readjust
      SystemTime.Now = () => Token.ExpirationTime.Value.AddSeconds(1);
      Thread.CurrentThread.Join(500);

      //this time, it should work
      var token = provider.UploadTransfers.RequestUploadToken(SourceFileInfo.FullName, true, 5000, "");
      provider.UploadTransfers.CancelTransfer(token.TransferId, AbortReason.ClientAbort);

    }
  }
}
