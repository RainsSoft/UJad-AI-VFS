using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hardcodet.Commons.IO;
using NUnit.Framework;
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
        BufferedDataBlock block = DownloadService.ReadBlock(Token.TransferId, blockNumber++);
        blockList.Add(block);
        if (block.IsLastBlock) break;
      }

      for (int i = 0; i < blockList.Count; i++)
      {
        BufferedDataBlock block = blockList[i];
        BufferedDataBlock copy = DownloadService.ReadBlock(Token.TransferId, i);

        Assert.AreEqual(block.BlockLength, copy.BlockLength);
        Assert.AreEqual(block.BlockNumber, copy.BlockNumber);
        CollectionAssert.AreEqual(block.Data, copy.Data);
        Assert.AreEqual(block.Offset, copy.Offset);
      }

      //the same in the other direction
      for (int i = blockList.Count - 1; i >= 0; i--)
      {
        BufferedDataBlock block = blockList[i];
        BufferedDataBlock copy = DownloadService.ReadBlock(Token.TransferId, i);

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
        block = DownloadService.ReadBlock(Token.TransferId, blockNumber++);
        if (block.IsLastBlock) break;
      }

      Assert.IsTrue(block.IsLastBlock);

      BufferedDataBlock copy = DownloadService.ReadBlock(Token.TransferId, block.BlockNumber);
      Assert.AreEqual(block.BlockNumber, copy.BlockNumber);
      CollectionAssert.AreEqual(block.Data, copy.Data);
    }



    [Test]
    public void Requesting_First_Block_Multiple_Times_Should_Work()
    {
      var block1 = DownloadService.ReadBlock(Token.TransferId, 0);
      var block2 = DownloadService.ReadBlock(Token.TransferId, 0);

      Assert.AreEqual(block1.BlockNumber, block2.BlockNumber);
      Assert.AreEqual(block1.BlockLength, block2.BlockLength);
      Assert.AreNotEqual(0, block1.Data.Length);
      CollectionAssert.AreEqual(block1.Data, block2.Data);      
    }


    [Test]
    [ExpectedException(typeof(DataBlockException))]
    public void Requesting_Block_Number_That_Has_Not_Been_Served_Yet_But_Isnt_Next_Block_Should_Fail()
    {
      for (int i = 0; i < 10; i++)
      {
        DownloadService.ReadBlock(Token.TransferId, i);
      }

      DownloadService.ReadBlock(Token.TransferId, 20);
    }




    [Test]
    public void Refreshing_Token_After_Multiple_Read_Should_Still_Indicate_The_Last_Block_Number()
    {
      for (int i = 0; i < 10; i++)
      {
        DownloadService.ReadBlock(Token.TransferId, i);
      }

      DataBlockInfo lastBlock = null;
      //redownload a few of these blocks
      for (int i = 0; i < 10; i++)
      {
        if (i%2 == 0)
        {
          lastBlock = DownloadService.ReadBlock(Token.TransferId, i);
        }
      }

      DownloadToken copy = DownloadService.RenewToken(Token, false);
      Assert.AreEqual(lastBlock.BlockNumber, copy.LastTransmittedBlockInfo.BlockNumber);
    }

  }

}
