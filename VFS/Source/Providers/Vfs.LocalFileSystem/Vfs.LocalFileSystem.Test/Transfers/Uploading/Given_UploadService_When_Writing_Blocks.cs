using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using NUnit.Framework;
using Vfs.Transfer;
using Vfs.Util;


namespace Vfs.LocalFileSystem.Test.Transfers.Uploading
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_UploadService_When_Writing_Blocks : UploadServiceTestBase
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
        UploadHandler.WriteBlock(block);
      }

      TransferStatus status = UploadHandler.CompleteTransfer(Token.TransferId);

      FileAssert.AreEqual(SourceFile, TargetFile);
      Assert.AreEqual(TransferStatus.Completed, status);
    }


    [Test]
    public void Writing_Streamed_Blocks_Should_Create_Clean_Copy()
    {
      using(var fs = SourceFile.OpenRead())
      {
        fs.WriteTo(Token, SourceFile.Length, Token.MaxBlockSize.Value, UploadHandler.WriteBlockStreamed);
      }
      
      UploadHandler.CompleteTransfer(Token.TransferId);

      FileAssert.AreEqual(SourceFile, TargetFile);
    }



    [Test]
    public void Unordered_Writing_Should_Be_Supported()
    {
      var blocks = new Dictionary<long, BufferedDataBlock>();

      Random r = new Random();
      var list = CreateBufferedBlocks();

      var count = list.Count;
      for (int i = 0; i < count; i++ )
      {
        int index = r.Next(list.Count);
        var block = list[index];
        blocks.Add(block.BlockNumber, block);

        UploadHandler.WriteBlock(list[index]);
        list.RemoveAt(index);
      }

      UploadHandler.CompleteTransfer(Token.TransferId);
      TargetFile.Refresh();
      FileAssert.AreEqual(SourceFile, TargetFile);
    }



    [Test]
    public void Submitting_Blocks_Should_Update_Transferred_Block_Table()
    {
      var blocks = new Dictionary<long, BufferedDataBlock>();

      Random r = new Random();
      var list = CreateBufferedBlocks();

      var count = list.Count;
      for (int i = 0; i < count; i++)
      {
        int index = r.Next(list.Count);
        var block = list[index];
        blocks.Add(block.BlockNumber, block);

        UploadHandler.WriteBlock(list[index]);
        list.RemoveAt(index);

        //get transmission table and compare
        var transferredBlocks = UploadHandler.GetTransferredBlocks(Token.TransferId);
        Assert.AreEqual(blocks.Count, transferredBlocks.Count());
        transferredBlocks.Do(b =>
                               {
                                 Assert.AreEqual(blocks[b.BlockNumber].Offset, b.Offset);
                                 Assert.AreEqual(blocks[b.BlockNumber].BlockLength, b.BlockLength);
                               });

      }

      UploadHandler.CompleteTransfer(Token.TransferId);
    }




    [Test]
    public void Submitting_Last_Block_Should_Automatically_Close_Transfer()
    {
      var blocks = CreateBufferedBlocks();
      var block = blocks[20];
      block.IsLastBlock = true;
      blocks.Remove(block);

      //write all blocks in the list
      blocks.Do(UploadHandler.WriteBlock);
      Assert.AreEqual(TransferStatus.Running, UploadHandler.GetTransferStatus(Token.TransferId));

      //write last block
      UploadHandler.WriteBlock(block);
      Assert.AreEqual(TransferStatus.Completed, UploadHandler.GetTransferStatus(Token.TransferId));

      FileAssert.AreEqual(SourceFile, TargetFile);
    }


    [Test]
    public void Writing_A_Single_Empty_Block_Should_Create_File()
    {
      UploadHandler.CancelTransfer(Token.TransferId, AbortReason.ClientAbort);

      //get token for empty file
      Token = UploadHandler.RequestUploadToken(TargetFilePath, false, 0, "");

      TargetFile.Refresh();
      Assert.IsFalse(TargetFile.Exists);

      var block = new BufferedDataBlock { TransferTokenId = Token.TransferId, BlockLength = 0, BlockNumber = 0, Data = new byte[0], IsLastBlock = true};
      UploadHandler.WriteBlock(block);

      TargetFile.Refresh();
      Assert.IsTrue(TargetFile.Exists);
    }
  }
}