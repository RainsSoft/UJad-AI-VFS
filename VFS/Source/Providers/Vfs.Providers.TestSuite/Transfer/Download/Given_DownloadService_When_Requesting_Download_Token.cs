using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

using NUnit.Framework;
using Vfs.LocalFileSystem.Test.Transfers.Downloading;
using Vfs.Locking;
using Vfs.Transfer;
using Vfs.Util;


namespace Vfs.Providers.TestSuite.Transfer.Download
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_DownloadService_When_Requesting_Download_Token : DownloadTestBase
  {
    protected override void InitInternal()
    {
      base.InitInternal();
      InitToken();
    }


    [Test]
    public void Read_Lock_Should_Have_Been_Established_For_Resource()
    {
      Assert.Inconclusive("Cannot test without knowing the internally used qualified identifiers.");
//      Assert.AreEqual(ResourceLockState.ReadOnly, LockRepository.GetLockState(SourceFileInfo.FullName.ToLowerInvariant()));
    }


    [Test]
    public void Read_Lock_Should_Have_Been_Established_For_Resources_Parent_Folders()
    {
      Assert.Inconclusive("Cannot test without knowing the internally used qualified identifiers.");

//      VirtualFolder parent = SourceFile.GetParentFolder();
//      while(true)
//      {
//        //assume key is path in lower case - this is how the base class works
//        //might fail in custom implementations
//        var lockState = LockRepository.GetLockState(parent.MetaData.FullName.ToLowerInvariant());
//        Assert.AreEqual(ResourceLockState.ReadOnly, lockState, "Folder is not locked: " + parent.MetaData.FullName);
//
//        if(parent.MetaData.IsRootFolder) break;
//        parent = parent.GetParentFolder();
//      }
    }


    [Test]
    public void Token_Expiration_Time_Should_Be_Set()
    {
      Assert.IsTrue(Token.ExpirationTime.HasValue);
      Assert.IsTrue(Token.ExpirationTime > SystemTime.Now());
    }



    [Test]
    public void Token_Should_Provide_Proper_Initialization_State()
    {
      Assert.AreEqual(Token.ResourceName, SourceFileInfo.Name);
      Assert.AreEqual(Token.ResourceIdentifier, SourceFileInfo.FullName);
      Assert.IsNotEmpty(Token.TransferId);

      Assert.AreEqual(SourceFileInfo.Length, Token.ResourceLength);
      Assert.AreEqual(ContentUtil.ResolveContentType(Path.GetExtension(SourceFileInfo.Name)), Token.ContentType);
      Assert.Greater(Token.DownloadBlockSize, 0);

      Assert.IsNull(Token.ResourceEncoding);
      Assert.IsNull(Token.Md5FileHash);
    }



    [Test]
    public void Token_Should_Calculate_MD5_File_Hash_If_Explicitly_Requested()
    {
      //cancel old download
      Assert.IsNullOrEmpty(Token.Md5FileHash);
      Downloads.CancelTransfer(Token.TransferId, AbortReason.ClientAbort);

      //get new token with hash
      Token = Downloads.RequestDownloadToken(Token.ResourceIdentifier, true);
      Assert.IsNotEmpty(Token.Md5FileHash);

      var hash = Context.DownloadFile0Template.CalculateMd5Hash();
      StringAssert.AreEqualIgnoringCase(hash, Token.Md5FileHash);
    }

  }
}