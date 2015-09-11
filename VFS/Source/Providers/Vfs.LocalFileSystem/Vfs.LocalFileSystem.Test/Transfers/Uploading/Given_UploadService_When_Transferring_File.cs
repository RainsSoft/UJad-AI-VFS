using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

using NUnit.Framework;
using Vfs.Transfer;
using Vfs.Transfer.Util;
using Vfs.Util;


namespace Vfs.LocalFileSystem.Test.Transfers.Uploading
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_UploadService_When_Transferring_File : UploadServiceTestBase
  {

    protected override void InitInternal()
    {
      base.InitInternal();
      InitToken();
    }


    [Test]
    public void Writing_Whole_File_In_Buffered_Blocks_Should_Create_Exact_Copy()
    {
      BufferedBlockOutputStream os = new BufferedBlockOutputStream(Token, 15000, b => UploadHandler.WriteBlock(b));
      MemoryStream ms = new MemoryStream(SourceFileContents);
      ms.WriteTo(os);

      UploadHandler.CompleteTransfer(Token.TransferId);

      TargetFile.Refresh();
      FileAssert.AreEqual(SourceFilePath, TargetFilePath);
    }


    [Test]
    public void Writing_Whole_File_In_Streamed_Blocks_Should_Create_Exact_Copy()
    {
      //use an output stream which works with buffers
      StreamedBlockOutputStream os = new StreamedBlockOutputStream(Token, 15000, b => UploadHandler.WriteBlockStreamed(b));
      MemoryStream ms = new MemoryStream(SourceFileContents);
      ms.WriteTo(os);

      UploadHandler.CompleteTransfer(Token.TransferId);

      TargetFile.Refresh();
      FileAssert.AreEqual(SourceFilePath, TargetFilePath);


      TargetFile.Delete();
      InitToken();

      //use a source strem
      using(var fs = SourceFile.OpenRead())
      {
        fs.WriteTo(Token, SourceFile.Length, 10000, b => UploadHandler.WriteBlockStreamed(b));
        UploadHandler.CompleteTransfer(Token.TransferId);

        TargetFile.Refresh();
        FileAssert.AreEqual(SourceFilePath, TargetFilePath);
      }
    }


    [Test]
    public void Writing_Last_Buffered_Block_Should_Complete_Transfer()
    {
      BufferedBlockOutputStream os = new BufferedBlockOutputStream(Token, 15000, b => UploadHandler.WriteBlock(b));
      MemoryStream ms = new MemoryStream(SourceFileContents);
      ms.WriteTo(os);

      Assert.AreEqual(TransferStatus.Running, UploadHandler.GetTransferStatus(Token.TransferId));

      //write an empty last block
      BufferedDataBlock db = new BufferedDataBlock
                               {
                                 TransferTokenId = Token.TransferId,
                                 BlockLength = 0,
                                 Offset = SourceFile.Length,
                                 Data = new byte[0],
                                 BlockNumber = 0,
                                 IsLastBlock = true
                               };
      UploadHandler.WriteBlock(db);

      Assert.AreEqual(TransferStatus.Completed, UploadHandler.GetTransferStatus(Token.TransferId));
      FileAssert.AreEqual(SourceFilePath, TargetFilePath);
    }


    [Test]
    public void Writing_Last_Streamed_Block_Should_Complete_Transfer()
    {
      //use a source strem
      using (var fs = SourceFile.OpenRead())
      {
        fs.WriteTo(Token, SourceFile.Length, 10000, b => UploadHandler.WriteBlockStreamed(b));
      }

      Assert.AreEqual(TransferStatus.Completed, UploadHandler.GetTransferStatus(Token.TransferId));
    }



    [Test]
    public void Uploading_An_Empty_File_Should_Create_The_File()
    {
      UploadHandler.CancelTransfer(Token.TransferId, AbortReason.ClientAbort);

      SourceFile.Delete();
      using (var fs = SourceFile.Create())
      {
        fs.Close();
      }

      SourceFile.Refresh();
      Assert.AreEqual(0, SourceFile.Length);

      TargetFile.Refresh();
      Assert.IsFalse(TargetFile.Exists);

      using(var fs = SourceFile.OpenRead())
      {
        UploadHandler.WriteFile(TargetFilePath, fs, true, 0, "");
      }

      TargetFile.Refresh();
      Assert.IsTrue(TargetFile.Exists);
    }
  }
}
