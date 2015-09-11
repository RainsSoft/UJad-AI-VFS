using System.IO;
using System.Linq;
using NUnit.Framework;
using Vfs.Transfer;
using Vfs.Util;


namespace Vfs.Providers.TestSuite.Transfer.Upload
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_UploadService_When_Upload_Overwrites_Existing_File : UploadTestBase
  {
    private VirtualFileInfo target;

    protected override void InitInternal()
    {
      base.InitInternal();
      target = FileSystem.WriteFile(SourceFile.FullName, TargetFilePath, false);
      Assert.IsTrue(FileSystem.IsFileAvailable(TargetFilePath));
    }


    [ExpectedException(typeof(ResourceOverwriteException))]
    [Test]
    public void Requesting_Token_Without_Override_Flag_Should_Be_Denied()
    {
      Token = Uploads.RequestUploadToken(TargetFilePath, false, SourceFile.Length, "");
    }

    [Test]
    public void Requesting_Token_With_Overwrite_Flag_Should_Be_Granted()
    {
      Token = Uploads.RequestUploadToken(TargetFilePath, true, SourceFile.Length, "");
    }


    [Test]
    public void Initiating_Token_Should_Not_Delete_Existing_File_Yet()
    {
      Token = Uploads.RequestUploadToken(TargetFilePath, true, SourceFile.Length, "");

      Assert.IsTrue(FileSystem.IsFileAvailable(TargetFilePath));
      target = FileSystem.GetFileInfo(TargetFilePath);
      Assert.AreEqual(SourceFile.Length, target.Length);
    }



    [Test]
    public void Cancelling_Transfer_Before_Writing_Any_Data_Should_Retain_File_That_Would_Have_Overwritten()
    {
      Token = Uploads.RequestUploadToken(TargetFilePath, true, SourceFile.Length, "");
      Uploads.CancelTransfer(Token.TransferId, AbortReason.ClientAbort);

      Assert.IsTrue(FileSystem.IsFileAvailable(TargetFilePath));
      target = FileSystem.GetFileInfo(TargetFilePath);
      Assert.AreEqual(SourceFile.Length, target.Length);
    }


    [Test]
    public void Cancelling_Transfer_While_Uploading_Should_Delete_Partial_File()
    {
      Token = Uploads.RequestUploadToken(TargetFilePath, true, SourceFile.Length, "");

      var block = CreateBufferedBlocks().First();
      Uploads.WriteBlock(block);
      Uploads.CancelTransfer(Token.TransferId, AbortReason.ClientAbort);

      //this might fail on sophisticated implementations that preserve the original
      Assert.IsFalse(FileSystem.IsFileAvailable(TargetFilePath), "Test might fail on sophisticated implementations that preserve the original file.");
    }

  }
}
