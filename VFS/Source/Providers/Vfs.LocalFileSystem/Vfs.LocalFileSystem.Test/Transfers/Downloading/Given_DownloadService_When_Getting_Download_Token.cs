using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hardcodet.Commons.IO;
using NUnit.Framework;
using Vfs.LocalFileSystem.Test.Transfers.Downloading;
using Vfs.Transfer;
using Vfs.Util;


namespace TransferServices_Test.Download_Service
{
  [TestFixture]
  public class Given_DownloadService_When_Getting_Download_Token : DownloadServiceTestBase
  {

    protected override void InitInternal()
    {
      base.InitInternal();
      InitToken();
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Token_Should_Provide_Proper_Initialization_State()
    {
      Assert.AreEqual(Token.ResourceName, Path.GetFileName(SourceFilePath));
      Assert.AreEqual(Token.ResourceIdentifier, SourceFileInfo.FullName);
      Assert.IsNotEmpty(Token.TransferId);

      Assert.AreEqual(SourceFile.Length, Token.ResourceLength);
      Assert.AreEqual(ContentUtil.ResolveContentType(SourceFile.Extension), Token.ContentType);
      Assert.Greater(Token.DownloadBlockSize, 0);

      Assert.IsNull(Token.ResourceEncoding);
      Assert.IsNull(Token.Md5FileHash);
    }


    [Test]
    public void Token_Creation_Should_Set_Timestamp_To_System_Time()
    {
      var timestamp = DateTimeOffset.Now.AddHours(2);
      SystemTime.Now = () => timestamp;

      Token = DownloadHandler.RequestDownloadToken(SourceFilePath, false);
      Assert.AreEqual(timestamp, Token.CreationTime);
    }



    [Test]
    public void Requesting_Token_State_Should_Match_Received_Token()
    {
      Assert.AreEqual(TransferStatus.Starting, DownloadHandler.GetTransferStatus(Token.TransferId));
    }




    [Test]
    public void Token_Should_Only_Calculate_File_Hash_If_Explicitly_Requested()
    {
      Assert.IsNullOrEmpty(Token.Md5FileHash);

      Token = DownloadHandler.RequestDownloadToken(Token.ResourceIdentifier, true);
      Assert.IsNotEmpty(Token.Md5FileHash);
    }


  }

}
