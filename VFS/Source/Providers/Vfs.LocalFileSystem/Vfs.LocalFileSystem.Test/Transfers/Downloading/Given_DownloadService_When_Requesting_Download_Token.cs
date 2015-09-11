using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

using NUnit.Framework;
using Vfs.Locking;
using Vfs.Util;


namespace Vfs.LocalFileSystem.Test.Transfers.Downloading
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_DownloadService_When_Requesting_Download_Token : DownloadServiceTestBase
  {
    protected override void InitInternal()
    {
      base.InitInternal();
      InitToken();
    }

    [Test]
    public void Read_Lock_Should_Have_Been_Established_For_Resource()
    {
      Assert.AreEqual(ResourceLockState.ReadOnly, LockRepository.GetLockState(SourceFile.FullName.ToLowerInvariant()));
    }

    [Test]
    public void Read_Lock_Should_Have_Been_Established_For_Resources_Parent_Folders()
    {
      DirectoryInfo dir = SourceFile.Directory;
      while(dir.Name != rootDirectory.Name)
      {
        var lockState = LockRepository.GetLockState(dir.FullName.ToLowerInvariant());
        Assert.AreEqual(ResourceLockState.ReadOnly, lockState, "Folder is not locked: " + dir.FullName);

        dir = dir.Parent;
      }
    }


    [Test]
    public void Tokens_Expiration_Should_Be_Set()
    {
      Assert.IsTrue(Token.ExpirationTime.HasValue);
      Assert.IsTrue(Token.ExpirationTime > SystemTime.Now());
    }

  }
}
