using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Vfs.LocalFileSystem.Test.Transfers.Downloading;
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
      base.InitInternal();
      InitToken();
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Status_Should_Have_Been_Adjusted_After_Reading_First_Block()
    {
      Assert.AreEqual(TransferStatus.Starting, DownloadHandler.GetTransferStatus(Token.TransferId));
      DownloadHandler.ReadBlock(Token.TransferId, 0);
      Assert.AreEqual(TransferStatus.Running, DownloadHandler.GetTransferStatus(Token.TransferId));
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
        BufferedDataBlock block = DownloadHandler.ReadBlock(Token.TransferId, blockNumber++);

        //the receive method checks offsets and block numbers
        base.ReceiveBlock(block);

        if(block.IsLastBlock) break;
      }

      //make sure we got all the data
      CollectionAssert.AreEqual(SourceFileContents, ReceivingBuffer);
    }


    [Test]
    public void Number_Of_Available_Blocks_Should_Match_Token()
    {
      long blockNumber = 0;
      for (int i = 0; i < Token.TotalBlockCount; i++)
      {
        BufferedDataBlock block = DownloadHandler.ReadBlock(Token.TransferId, blockNumber++);

        //the receive method checks offsets and block numbers
        base.ReceiveBlock(block);
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
        BufferedDataBlock block = DownloadHandler.ReadBlock(Token.TransferId, blockNumber++);

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
        BufferedDataBlock block = DownloadHandler.ReadBlock(Token.TransferId, blockNumber++);
        blockList.Add(block);
        if (block.IsLastBlock) break;
      }

      Assert.Greater(blockList.Count, 1);
      Assert.IsTrue(blockList.Last().IsLastBlock);
      blockList.RemoveAt(blockList.Count - 1);
      blockList.ForEach(b => Assert.IsFalse(b.IsLastBlock));
    }



    [Test]
    public void If_File_Size_Is_Multitude_Of_Block_Size_Then_Last_Block_Should_Be_Full()
    {
      //make file of 10 blocks
      var blockSize = (int)Token.DownloadBlockSize;
      SourceFileContents = new byte[blockSize*10];
      File.WriteAllBytes(SourceFilePath, SourceFileContents);
      Token = DownloadHandler.RequestDownloadToken(SourceFilePath, false);

      List<BufferedDataBlock> blockList = new List<BufferedDataBlock>();

      int blockNumber = 0;
      while (true)
      {
        BufferedDataBlock block = DownloadHandler.ReadBlock(Token.TransferId, blockNumber++);
        blockList.Add(block);
        if (block.IsLastBlock) break;
      }

      Assert.AreEqual(10, blockList.Count);
      Assert.AreEqual(blockSize, blockList[9].BlockLength);
    }


    [Test]
    public void If_File_Size_Is_Not_Multitude_Of_Block_Size_Then_Last_Block_Should_Be_Smaller_Then_Others()
    {
      //make file of 10 blocks
      var blockSize = (int)Token.DownloadBlockSize;
      SourceFileContents = new byte[blockSize * 10 + blockSize/2]; //add half a block
      File.WriteAllBytes(SourceFilePath, SourceFileContents);
      Token = DownloadHandler.RequestDownloadToken(SourceFilePath, false);

      List<BufferedDataBlock> blockList = new List<BufferedDataBlock>();

      int blockNumber = 0;
      while (true)
      {
        BufferedDataBlock block = DownloadHandler.ReadBlock(Token.TransferId, blockNumber++);
        blockList.Add(block);
        if (block.IsLastBlock) break;
      }

      Assert.AreEqual(11, blockList.Count);
      Assert.AreEqual(blockSize/2, blockList[10].BlockLength);
    }


    [Test]
    public void Random_Block_Access_Should_Be_Possible()
    {
      BufferedDataBlock block1 = DownloadHandler.ReadBlock(Token.TransferId, 5);
      BufferedDataBlock block2 = DownloadHandler.ReadBlock(Token.TransferId, 7);
    }


    [Test]
    public void Read_Blocks_Should_Be_Reflected_In_Block_Table()
    {
      BufferedDataBlock block1 = DownloadHandler.ReadBlock(Token.TransferId, 5);
      BufferedDataBlock block2 = DownloadHandler.ReadBlock(Token.TransferId, 7);

      IEnumerable<DataBlockInfo> transferTable = DownloadHandler.GetTransferredBlocks(Token.TransferId);
      Assert.AreEqual(2, transferTable.Count());
      Assert.AreEqual(block1.BlockNumber, transferTable.First().BlockNumber);
      Assert.AreEqual(block1.Offset, transferTable.First().Offset);
      Assert.AreEqual(block2.BlockNumber, transferTable.Last().BlockNumber);
      Assert.AreEqual(block2.Offset, transferTable.Last().Offset);

    }

    [Test]
    public void Token_And_Blocks_Should_Match_Default_Block_Size()
    {
      var blockSize = FileSystemConfiguration.DefaultDownloadBlockSize;

      Token = DownloadHandler.RequestDownloadToken(SourceFileInfo.FullName, false);
      Assert.AreEqual(blockSize, Token.DownloadBlockSize);

      BufferedDataBlock block = DownloadHandler.ReadBlock(Token.TransferId, 5);
      Assert.AreEqual(blockSize, block.BlockLength);
      Assert.AreEqual(blockSize, block.Data.Length);
    }



    [Test]
    [ExpectedException(typeof(DataBlockException))]
    public void Submitting_Negative_Block_Number_Should_Fail()
    {
      DownloadHandler.ReadBlock(Token.TransferId, -1);
    }


    [Test]
    [ExpectedException(typeof(DataBlockException))]
    public void Submitting_Block_Number_Above_Total_Blocks_Should_Fail()
    {
      DownloadHandler.ReadBlock(Token.TransferId, Token.TotalBlockCount);
    }

  }

}
