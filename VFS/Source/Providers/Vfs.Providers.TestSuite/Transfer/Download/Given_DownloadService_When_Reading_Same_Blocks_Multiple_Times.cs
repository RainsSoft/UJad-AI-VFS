using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hardcodet.Commons.IO;
using NUnit.Framework;
using Vfs.LocalFileSystem.Test.Transfers.Downloading;
using Vfs.Transfer;
using Vfs.Util;


namespace Vfs.Providers.TestSuite.Transfer.Download
{
  [TestFixture]
  public class Given_DownloadService_When_Reading_Same_Blocks_Multiple_Times : DownloadTestBase
  {
    protected override void InitInternal()
    {
      base.InitInternal();
      InitToken();
    }


    [Test]
    public void Service_Should_Always_Return_Same_Data()
    {
      List<BufferedDataBlock> blockList = new List<BufferedDataBlock>();

      long blockNumber = 0;
      while (true)
      {
        BufferedDataBlock block = Downloads.ReadBlock(Token.TransferId, blockNumber++);
        blockList.Add(block);
        if (block.IsLastBlock) break;
      }

      for (int i = 0; i < blockList.Count; i++)
      {
        BufferedDataBlock block = blockList[i];
        BufferedDataBlock copy = Downloads.ReadBlock(Token.TransferId, i);

        Assert.AreEqual(block.BlockLength, copy.BlockLength);
        Assert.AreEqual(block.BlockNumber, copy.BlockNumber);
        CollectionAssert.AreEqual(block.Data, copy.Data);
        Assert.AreEqual(block.Offset, copy.Offset);
      }

      //the same in the other direction
      for (int i = blockList.Count - 1; i >= 0; i--)
      {
        BufferedDataBlock block = blockList[i];
        BufferedDataBlock copy = Downloads.ReadBlock(Token.TransferId, i);

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
        block = Downloads.ReadBlock(Token.TransferId, blockNumber++);
        if (block.IsLastBlock) break;
      }

      Assert.IsTrue(block.IsLastBlock);

      BufferedDataBlock copy = Downloads.ReadBlock(Token.TransferId, block.BlockNumber);
      Assert.AreEqual(block.BlockNumber, copy.BlockNumber);
      CollectionAssert.AreEqual(block.Data, copy.Data);
    }



    [Test]
    public void Requesting_First_Block_Multiple_Times_Should_Work()
    {
      var block1 = Downloads.ReadBlock(Token.TransferId, 0);
      var block2 = Downloads.ReadBlock(Token.TransferId, 0);

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
        Downloads.ReadBlock(Token.TransferId, i);
      }

      var block20 = Downloads.ReadBlock(Token.TransferId, 20);
      var block4 = Downloads.ReadBlock(Token.TransferId, 4);
      var block20Copy = Downloads.ReadBlock(Token.TransferId, 20);

      Assert.AreEqual(block20.Data, block20Copy.Data);
    }

  }

}
