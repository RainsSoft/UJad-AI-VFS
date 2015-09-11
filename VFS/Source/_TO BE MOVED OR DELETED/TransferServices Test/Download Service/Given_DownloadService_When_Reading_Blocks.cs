using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Hardcodet.Commons.IO;
using NUnit.Framework;
using Vfs;
using Vfs.Transfer;
using Vfs.Util;


namespace TransferServices_Test.Download_Service
{
  [TestFixture]
  public class Given_DownloadService_When_Reading_Blocks : DownloadServiceTestBase
  {
    public DateTimeOffset Timestamp;

    protected override void InitInternal()
    {
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Status_Should_Have_Been_Adjusted_After_Reading_First_Block()
    {
      Assert.AreEqual(TransferStatus.Starting, DownloadService.GetTransferStatus(Token.TransferId));
      DownloadService.ReadBlock(Token.TransferId, 0);
      Assert.AreEqual(TransferStatus.Running, DownloadService.GetTransferStatus(Token.TransferId));
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Reading_Blocks_Should_Deliver_All_Data()
    {
      long blockNumber = 0;
      while(true)
      {
        BufferedDataBlock block = DownloadService.ReadBlock(Token.TransferId, blockNumber++);

        //the receive method checks offsets and block numbers
        base.ReceiveBlock(block);

        if(block.IsLastBlock) break;
      }

      //make sure we got all the data
      CollectionAssert.AreEqual(SourceFileContents, ReceivingBuffer);
    }



    [Test]
    public void Received_Blocks_Should_Indicate_Offset_Within_Resource()
    {
      long blockNumber = 0;

      var timestamp = DateTimeOffset.Now.AddMinutes(5);
      SystemTime.Now = () => timestamp;
      while (true)
      {
        BufferedDataBlock block = DownloadService.ReadBlock(Token.TransferId, blockNumber++);

        //the receive method checks offsets and block numbers
        base.ReceiveBlock(block);

        if (block.IsLastBlock) break;
      }

      //make sure we got all the data
      CollectionAssert.AreEqual(SourceFileContents, ReceivingBuffer);
    }


    [Test]
    public void All_But_Last_Block_Should_Indicate_There_Is_More_Data()
    {
      List<BufferedDataBlock> blockList = new List<BufferedDataBlock>();
      
      int blockNumber = 0;
      while (true)
      {
        BufferedDataBlock block = DownloadService.ReadBlock(Token.TransferId, blockNumber++);
        blockList.Add(block);
        if (block.IsLastBlock) break;
      }

      Assert.Greater(blockList.Count, 1);
      Assert.IsTrue(blockList.Last().IsLastBlock);
      blockList.RemoveAt(blockList.Count - 1);
      blockList.ForEach(b => Assert.IsFalse(b.IsLastBlock));
    }



    [Test]
    public void If_File_Size_Is_Multitude_Of_Block_Size_Then_Last_Block_Should_Be_Empty()
    {
      //make file of 10 blocks
      var blockSize = (int)Token.DownloadBlockSize;
      SourceFileContents = new byte[blockSize*10];
      File.WriteAllBytes(SourceFilePath, SourceFileContents);
      Token = DownloadService.RequestDownloadToken(SourceFilePath, false);

      List<BufferedDataBlock> blockList = new List<BufferedDataBlock>();

      int blockNumber = 0;
      while (true)
      {
        BufferedDataBlock block = DownloadService.ReadBlock(Token.TransferId, blockNumber++);
        blockList.Add(block);
        if (block.IsLastBlock) break;
      }

      Assert.AreEqual(11, blockList.Count);
      Assert.AreEqual(0, blockList[10].BlockLength);
    }


    [Test]
    public void If_File_Size_Is_Not_Multitude_Of_Block_Size_Then_Last_Block_Should_Be_Smaller_Then_Others()
    {
      //make file of 10 blocks
      var blockSize = (int)Token.DownloadBlockSize;
      SourceFileContents = new byte[blockSize * 10 + blockSize/2]; //add half a block
      File.WriteAllBytes(SourceFilePath, SourceFileContents);
      Token = DownloadService.RequestDownloadToken(SourceFilePath, false);

      List<BufferedDataBlock> blockList = new List<BufferedDataBlock>();

      int blockNumber = 0;
      while (true)
      {
        BufferedDataBlock block = DownloadService.ReadBlock(Token.TransferId, blockNumber++);
        blockList.Add(block);
        if (block.IsLastBlock) break;
      }

      Assert.AreEqual(11, blockList.Count);
      Assert.AreEqual(blockSize/2, blockList[10].BlockLength);
    }


    [Test]
    public void Random_Block_Access_Should_Be_Possible()
    {
      BufferedDataBlock block1 = DownloadService.ReadBlock(Token.TransferId, 5);
      BufferedDataBlock block2 = DownloadService.ReadBlock(Token.TransferId, 7);
    }

  }

}
