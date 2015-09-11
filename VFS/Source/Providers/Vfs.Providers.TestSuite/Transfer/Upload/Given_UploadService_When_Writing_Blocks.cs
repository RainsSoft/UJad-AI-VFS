using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using NUnit.Framework;
using Vfs.Transfer;
using Vfs.Util;


namespace Vfs.Providers.TestSuite.Transfer.Upload
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_UploadService_When_Writing_Blocks : UploadTestBase
  {
    protected override void InitInternal()
    {
      base.InitInternal();
      InitToken();
    }


    [Test]
    public void Writing_All_Blocks_Should_Create_Clean_Copy()
    {
      foreach (var block in CreateBufferedBlocks())
      {
        Uploads.WriteBlock(block);
      }

      TransferStatus status = Uploads.CompleteTransfer(Token.TransferId);
      Assert.AreEqual(TransferStatus.Completed, status);

      CompareUploadToSourceFile();
    }




    [Test]
    public void Writing_Streamed_Blocks_Should_Create_Clean_Copy()
    {
      using(var fs = SourceFile.OpenRead())
      {
        fs.WriteTo(Token, SourceFile.Length, Token.MaxBlockSize.Value, Uploads.WriteBlockStreamed);
      }
      
      Uploads.CompleteTransfer(Token.TransferId);

      CompareUploadToSourceFile();
    }



    [Test]
    public void Unordered_Writing_Should_Be_Supported()
    {
      if (Uploads.TransmissionCapabilities != TransmissionCapabilities.Random)
      {
        Assert.Inconclusive("File system under test does not support random block access - test skipped.");
      }

      var blocks = new Dictionary<long, BufferedDataBlock>();

      Random r = new Random();
      var list = CreateBufferedBlocks();

      var count = list.Count;
      for (int i = 0; i < count; i++ )
      {
        int index = r.Next(list.Count);
        var block = list[index];
        blocks.Add(block.BlockNumber, block);

        Uploads.WriteBlock(list[index]);
        list.RemoveAt(index);
      }

      Uploads.CompleteTransfer(Token.TransferId);

      CompareUploadToSourceFile();
    }



    [Test]
    public void Submitting_Last_Block_Should_Automatically_Close_Transfer()
    {
      var blocks = CreateBufferedBlocks();
      var block = blocks.Last();
      block.IsLastBlock = true;
      blocks.Remove(block);

      //write all blocks in the list
      blocks.Do(Uploads.WriteBlock);
      Assert.AreEqual(TransferStatus.Running, Uploads.GetTransferStatus(Token.TransferId));

      try
      {
        FileSystem.DownloadTransfers.RequestDownloadToken(TargetFilePath, false);
        Assert.Fail("Expected exception due to locked resource.");
      }
      catch (ResourceLockedException expected)
      {
      }

      //write last block
      Uploads.WriteBlock(block);

      //test if we can get a download token without a lock
      var workingToken = FileSystem.DownloadTransfers.RequestDownloadToken(TargetFilePath, false);
      FileSystem.DownloadTransfers.CancelTransfer(workingToken.TransferId, AbortReason.ClientAbort);
    }


    [ExpectedException(typeof(DataBlockException))]
    [Test]
    public void Writing_Bigger_File_Than_Advertised_Should_Be_Denied()
    {
      Uploads.CancelTransfer(Token.TransferId, AbortReason.ClientAbort);

      var resourceLength = SourceFile.Length / 2;
      Token = Uploads.RequestUploadToken(TargetFilePath, false, resourceLength, "binary");
      
      //get the block that will beyond the indicated file size
      var block = CreateBufferedBlocks().First();
      block.Offset = resourceLength - block.BlockLength.Value + 1;
      Uploads.WriteBlock(block);
    }
  }
}