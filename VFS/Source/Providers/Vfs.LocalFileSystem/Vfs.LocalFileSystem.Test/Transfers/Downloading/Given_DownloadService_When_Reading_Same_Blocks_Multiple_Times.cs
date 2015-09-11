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
  public class Given_DownloadService_When_Reading_Same_Blocks_Multiple_Times : DownloadServiceTestBase
  {
    public DateTimeOffset Timestamp;

    protected override void InitInternal()
    {
      base.InitInternal();
      InitToken();
    }



    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Service_Should_Always_Return_Same_Data()
    {
      List<BufferedDataBlock> blockList = new List<BufferedDataBlock>();

      long blockNumber = 0;
      while (true)
      {
        BufferedDataBlock block = DownloadHandler.ReadBlock(Token.TransferId, blockNumber++);
        blockList.Add(block);
        if (block.IsLastBlock) break;
      }

      for (int i = 0; i < blockList.Count; i++)
      {
        BufferedDataBlock block = blockList[i];
        BufferedDataBlock copy = DownloadHandler.ReadBlock(Token.TransferId, i);

        Assert.AreEqual(block.BlockLength, copy.BlockLength);
        Assert.AreEqual(block.BlockNumber, copy.BlockNumber);
        CollectionAssert.AreEqual(block.Data, copy.Data);
        Assert.AreEqual(block.Offset, copy.Offset);
      }

      //the same in the other direction
      for (int i = blockList.Count - 1; i >= 0; i--)
      {
        BufferedDataBlock block = blockList[i];
        BufferedDataBlock copy = DownloadHandler.ReadBlock(Token.TransferId, i);

        Assert.AreEqual(block.BlockLength, copy.BlockLength);
        Assert.AreEqual(block.BlockNumber, copy.BlockNumber);
        CollectionAssert.AreEqual(block.Data, copy.Data);
        Assert.AreEqual(block.Offset, copy.Offset);
      }
    }


 
    [Test]
    public void Requesting_Last_Block_Multiple_Times_Should_Work()
    {
      long blockNumber = 0;
      BufferedDataBlock block;
      while (true)
      {
        block = DownloadHandler.ReadBlock(Token.TransferId, blockNumber++);
        if (block.IsLastBlock) break;
      }

      Assert.IsTrue(block.IsLastBlock);

      BufferedDataBlock copy = DownloadHandler.ReadBlock(Token.TransferId, block.BlockNumber);
      Assert.AreEqual(block.BlockNumber, copy.BlockNumber);
      CollectionAssert.AreEqual(block.Data, copy.Data);
    }



    [Test]
    public void Requesting_First_Block_Multiple_Times_Should_Work()
    {
      var block1 = DownloadHandler.ReadBlock(Token.TransferId, 0);
      var block2 = DownloadHandler.ReadBlock(Token.TransferId, 0);

      Assert.AreEqual(block1.BlockNumber, block2.BlockNumber);
      Assert.AreEqual(block1.BlockLength, block2.BlockLength);
      Assert.AreNotEqual(0, block1.Data.Length);
      CollectionAssert.AreEqual(block1.Data, block2.Data);      
    }


    [Test]
    public void Requesting_Random_Block_Numbers_Should_Work()
    {
      for (int i = 0; i < 10; i++)
      {
        DownloadHandler.ReadBlock(Token.TransferId, i);
      }

      var block20 = DownloadHandler.ReadBlock(Token.TransferId, 20);
      var block4 = DownloadHandler.ReadBlock(Token.TransferId, 4);
      var block20Copy = DownloadHandler.ReadBlock(Token.TransferId, 20);

      Assert.AreEqual(block20.Data, block20Copy.Data);
    }


    [Test]
    public void Transferred_Block_Table_Should_Be_Updated_But_Not_Contain_Duplicate_Entries()
    {
      for (int i = 0; i < 10; i++)
      {
        DownloadHandler.ReadBlock(Token.TransferId, i);
      }

      //1 new block
      var block20 = DownloadHandler.ReadBlock(Token.TransferId, 20);
      var block4 = DownloadHandler.ReadBlock(Token.TransferId, 4);
      var block20Copy = DownloadHandler.ReadBlock(Token.TransferId, 20);

      var blocks = DownloadHandler.GetTransferredBlocks(Token.TransferId);
      Assert.AreEqual(11, blocks.Count());
      Assert.AreEqual(blocks.Single(b => b.BlockNumber == 4).Offset, block4.Offset);
      Assert.AreEqual(blocks.Single(b => b.BlockNumber == 4).BlockLength, block4.BlockLength);
      Assert.AreEqual(blocks.Single(b => b.BlockNumber == 20).Offset, block20.Offset);

    }

  }

}
