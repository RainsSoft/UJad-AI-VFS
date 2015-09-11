using System;
using System.Collections.Generic;
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
  public class Given_UploadService_When_Retransmitting_Blocks : UploadServiceTestBase
  {
    protected override void InitInternal()
    {
      base.InitInternal();
      InitToken();
    }


    [Test]
    public void Retransmitted_Blocks_Should_Overwrite_Previously_Transferred_Data()
    {
      List<BufferedDataBlock> blocks = CreateBufferedBlocks();

      //modify data in one of the blocks
      Array.Reverse(blocks[10].Data);

      foreach (var block in blocks)
      {
        UploadHandler.WriteBlock(block);
      }

      //pause in order to release stream
      UploadHandler.PauseTransfer(Token.TransferId);
      FileAssert.AreNotEqual(SourceFile, TargetFile);

      //write correct block data
      blocks = CreateBufferedBlocks();
      UploadHandler.WriteBlock(blocks[10]);

      //complete and recompare
      UploadHandler.CompleteTransfer(Token.TransferId);
      FileAssert.AreEqual(SourceFile, TargetFile);
    }


    [Test]
    public void Retransmitted_Blocks_Should_Replace_Entries_In_Transferred_Block_Table()
    {
      var block = CreateBufferedBlocks()[10];

      //modify data in one of the blocks
      int blockLength = block.BlockLength.Value/2;
      block.BlockLength = blockLength;
      block.Data = block.Data.CreateCopy(blockLength);

      //write block
      UploadHandler.WriteBlock(block);

      //check block
      var blocks = UploadHandler.GetTransferredBlocks(Token.TransferId);
      Assert.AreEqual(1, blocks.Count());
      Assert.AreEqual(blockLength, blocks.Single().BlockLength);

      UploadHandler.PauseTransfer(Token.TransferId);
      TargetFile.Refresh();
      Assert.AreEqual(block.Offset + blockLength, TargetFile.Length);

      //get unmodified block and write that one
      block = CreateBufferedBlocks()[10];
      UploadHandler.WriteBlock(block);

      blocks = UploadHandler.GetTransferredBlocks(Token.TransferId);
      Assert.AreEqual(1, blocks.Count());
      Assert.AreNotEqual(blockLength, blocks.Single().BlockLength);
      Assert.AreEqual(block.BlockLength, blocks.Single().BlockLength);

      UploadHandler.PauseTransfer(Token.TransferId);
      TargetFile.Refresh();
      Assert.AreEqual(block.Offset + block.BlockLength, TargetFile.Length);

      UploadHandler.CompleteTransfer(Token.TransferId);
    }
  }
}
