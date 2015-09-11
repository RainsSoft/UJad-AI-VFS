using System.IO;
using System.Linq;
using NUnit.Framework;
using Vfs.Transfer;
using Vfs.Util;


namespace Vfs.LocalFileSystem.Test.Transfers.Uploading
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_UploadService_When_Upload_Overwrites_Existing_File : UploadServiceTestBase
  {
    
    protected override void InitInternal()
    {
      base.InitInternal();
      File.WriteAllBytes(TargetFilePath, File.ReadAllBytes(SourceFilePath).CreateCopy(2048));
    }


    [ExpectedException(typeof(ResourceOverwriteException))]
    [Test]
    public void Requesting_Token_Without_Override_Flag_Should_Be_Denied()
    {
      Token = UploadHandler.RequestUploadToken(TargetFilePath, false, SourceFile.Length, "");
    }

    [Test]
    public void Requesting_Token_With_Overwrite_Flag_Should_Be_Granted()
    {
      Token = UploadHandler.RequestUploadToken(TargetFilePath, true, SourceFile.Length, "");
    }


    [Test]
    public void Initiating_Token_Should_Not_Delete_Existing_File_Yet()
    {
      Token = UploadHandler.RequestUploadToken(TargetFilePath, true, SourceFile.Length, "");
      
      TargetFile.Refresh();
      Assert.IsTrue(TargetFile.Exists);
      Assert.AreEqual(2048, TargetFile.Length);
    }

    [Test]
    public void Writing_First_Block_Should_Replace_File()
    {
      Token = UploadHandler.RequestUploadToken(TargetFilePath, true, SourceFile.Length, "");

      var block = CreateBufferedBlocks().First();
      UploadHandler.WriteBlock(block);

      TargetFile.Refresh();
      Assert.AreNotEqual(2048, TargetFile.Length);
      Assert.AreEqual(block.BlockLength, TargetFile.Length);
    }


    [Test]
    public void Cancelling_Transfer_Before_Writing_Any_Data_Should_Retain_File_That_Would_Have_Overwritten()
    {
      Token = UploadHandler.RequestUploadToken(TargetFilePath, true, SourceFile.Length, "");
      UploadHandler.CancelTransfer(Token.TransferId, AbortReason.ClientAbort);

      TargetFile.Refresh();
      Assert.IsTrue(TargetFile.Exists);
      Assert.AreEqual(2048, TargetFile.Length);
    }


    [Test]
    public void Cancelling_Transfer_While_Uploading_Should_Delete_Partial_File()
    {
      Token = UploadHandler.RequestUploadToken(TargetFilePath, true, SourceFile.Length, "");

      var block = CreateBufferedBlocks().First();
      UploadHandler.WriteBlock(block);
      UploadHandler.CancelTransfer(Token.TransferId, AbortReason.ClientAbort);

      TargetFile.Refresh();
      Assert.IsFalse(TargetFile.Exists);
    }

  }
}
