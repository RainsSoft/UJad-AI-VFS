using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

using NUnit.Framework;
using Vfs.Transfer;
using Vfs.Transfer.Util;
using Vfs.Util;


namespace Vfs.Providers.TestSuite.Transfer.Upload
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_UploadService_When_Transferring_File : UploadTestBase
  {

    protected override void InitInternal()
    {
      base.InitInternal();
      InitToken();
    }


    [Test]
    public void Writing_Whole_File_In_Buffered_Blocks_Should_Create_Exact_Copy()
    {
      BufferedBlockOutputStream os = new BufferedBlockOutputStream(Token, 2048, b => Uploads.WriteBlock(b));
      
      using(var fs = SourceFile.OpenRead())
      {
        fs.WriteTo(os);
      }

      Uploads.CompleteTransfer(Token.TransferId);
      CompareUploadToSourceFile();
    }


    [Test]
    public void Writing_Whole_File_In_Streamed_Blocks_Should_Create_Exact_Copy()
    {
      //use an output stream which works with streams
      StreamedBlockOutputStream os = new StreamedBlockOutputStream(Token, 2048, b => Uploads.WriteBlockStreamed(b));

      using (var fs = SourceFile.OpenRead())
      {
        fs.WriteTo(os);
      }
      
      Uploads.CompleteTransfer(Token.TransferId);
      CompareUploadToSourceFile();


      //delete file
      FileSystem.DeleteFile(TargetFilePath);
      InitToken();

      //use a source strem
      using(var fs = SourceFile.OpenRead())
      {
        fs.WriteTo(Token, SourceFile.Length, 2048, b => Uploads.WriteBlockStreamed(b));
      }

      Uploads.CompleteTransfer(Token.TransferId);
      CompareUploadToSourceFile();
    }



    [Test]
    public void Writing_An_Empty_Block_Should_Be_Supported()
    {
      //write an empty last block
      BufferedDataBlock db = new BufferedDataBlock
      {
        TransferTokenId = Token.TransferId,
        BlockLength = 0,
        Offset = 0,
        Data = new byte[0],
        BlockNumber = 0,
        IsLastBlock = false
      };

      Uploads.WriteBlock(db);
    }



    [Test]
    public void Writing_Last_Buffered_Block_Should_Complete_Transfer()
    {
      using (var fs = SourceFile.OpenRead())
      {
        fs.WriteTo(Token, SourceFile.Length, 2048, b =>
                                                     {
                                                       b.IsLastBlock = false;
                                                       Uploads.WriteBlockStreamed(b);
                                                     });
      }

      Assert.AreEqual(TransferStatus.Running, Uploads.GetTransferStatus(Token.TransferId));

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
      Uploads.WriteBlock(db);

      CompareUploadToSourceFile();

      TransferStatus status = Uploads.GetTransferStatus(Token.TransferId);
      Assert.IsTrue(status.Is(TransferStatus.Completed, TransferStatus.UnknownTransfer));
    }


    [Test]
    public void Streaming_Whole_File_Should_Create_Exact_Copy()
    {
      Random rnd = new Random();
      byte[] buffer = new byte[12];
      rnd.NextBytes(buffer);

      MemoryStream ms = new MemoryStream(buffer);
      var uploadedFile = UploadFolder.AddFile("rnd.bin", ms, false, 12, "");

      byte[] copy = new byte[12];
      using(var stream = uploadedFile.GetContents())
      {
        stream.Read(copy, 0, 12);
      }

      for (int i = 0; i < 12; i++)
      {
        Assert.AreEqual(buffer[i], copy[i]);
      }


      Console.Out.WriteLine(Environment.CurrentDirectory);
      Console.Out.WriteLine("done");
    }


    [Test]
    public void Writing_Last_Streamed_Block_Should_Complete_Transfer()
    {
      //use a source strem
      using (var fs = SourceFile.OpenRead())
      {
        fs.WriteTo(Token, SourceFile.Length, 10000, b => Uploads.WriteBlockStreamed(b));
      }

      TransferStatus status = Uploads.GetTransferStatus(Token.TransferId);
      Assert.IsTrue(status.Is(TransferStatus.Completed, TransferStatus.UnknownTransfer));
    }



    [Test]
    public void Uploading_An_Empty_File_Should_Create_The_File()
    {
      Uploads.CancelTransfer(Token.TransferId, AbortReason.ClientAbort);

      //create source file
      SourceFile.Delete();
      using (var fs = SourceFile.Create())
      {
        fs.Close();
      }

      SourceFile.Refresh();
      Assert.AreEqual(0, SourceFile.Length);

      Assert.IsFalse(FileSystem.IsFileAvailable(TargetFilePath));
      using(var fs = SourceFile.OpenRead())
      {
        Uploads.WriteFile(TargetFilePath, fs, true, 0, "");
      }

      var target = FileSystem.GetFileInfo(TargetFilePath);
      Assert.AreEqual(0, target.Length);

      Context.DownloadFile1Template.Refresh();
      Assert.AreNotEqual(0, Context.DownloadFile1Template.Length);
      FileSystem.SaveFile(target, Context.DownloadFile1Template.FullName);
      Context.DownloadFile1Template.Refresh();
      Assert.AreEqual(0, Context.DownloadFile1Template.Length);
    }
  }
}
