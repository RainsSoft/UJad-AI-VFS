using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Schema;
using NUnit.Framework;
using Vfs.Transfer;
using Vfs.Util;


namespace Vfs.Providers.TestSuite.Transfer.Download
{
  [TestFixture]
  public class Given_DownloadService_When_Reading_Blocks : DownloadTestBase
  {
    protected override void InitInternal()
    {
      base.InitInternal();
      InitToken();
    }


    /// <summary>
    /// Updates the <see cref="ReceivingBuffer"/> with the data
    /// of a given block, and performs a few simple tests.
    /// </summary>
    /// <param name="block"></param>
    private void ReceiveBlock(BufferedDataBlock block)
    {
      using (var stream = File.Open(TargetFilePath, FileMode.Append))
      {
        stream.Write(block.Data, 0, block.Data.Length);
      }
    }



    [Test]
    public void Status_Should_Have_Been_Adjusted_After_Reading_First_Block()
    {
      Assert.AreEqual(TransferStatus.Starting, Downloads.GetTransferStatus(Token.TransferId));
      Downloads.ReadBlock(Token.TransferId, 0);
      Assert.AreEqual(TransferStatus.Running, Downloads.GetTransferStatus(Token.TransferId));
    }




    [Test]
    public void Reading_Blocks_Should_Deliver_All_Data()
    {
      long blockNumber = 0;
      //read blocks until last block is indicated
      while(true)
      {
        BufferedDataBlock block = Downloads.ReadBlock(Token.TransferId, blockNumber++);
        ReceiveBlock(block);

        if(block.IsLastBlock) break;
      }

      //make sure we got all the data
      FileAssert.AreEqual(Context.DownloadFile0Template.FullName, TargetFilePath);
    }



    [Test]
    public void Number_Of_Available_Blocks_Should_Match_Token()
    {
      long blockNumber = 0;
      //request data using block numbers according to token info
      for (int i = 0; i < Token.TotalBlockCount; i++)
      {
        BufferedDataBlock block = Downloads.ReadBlock(Token.TransferId, blockNumber++);
        ReceiveBlock(block);
      }

      //make sure we got all the data
      FileAssert.AreEqual(Context.DownloadFile0Template.FullName, TargetFilePath);
    }


    
    [Test]
    public void Received_Blocks_Should_Indicate_Offset_Within_Resource()
    {
      long blockNumber = 0;

      var timestamp = DateTimeOffset.Now.AddMinutes(5);
      SystemTime.Now = () => timestamp;
      while (true)
      {
        BufferedDataBlock block = Downloads.ReadBlock(Token.TransferId, blockNumber++);
        if(block.BlockNumber != 0) Assert.AreEqual(new FileInfo(TargetFilePath).Length, block.Offset);

        ReceiveBlock(block);
        if (block.IsLastBlock) break;
      }
    }


    [Test]
    public void All_But_Last_Block_Should_Indicate_There_Is_More_Data()
    {
      List<BufferedDataBlock> blockList = new List<BufferedDataBlock>();
      
      int blockNumber = 0;
      while (true)
      {
        BufferedDataBlock block = Downloads.ReadBlock(Token.TransferId, blockNumber++);
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
      var maxSize = Downloads.MaxBlockSize ?? 2048;

      //upload a file that consists of a total of 5 blocks
      File.WriteAllBytes(Context.DownloadFile0Template.FullName, new byte[maxSize * 5]);
      Context.DownloadFile0Template.Refresh();
      SourceFile = Context.DownloadFolder.AddFile(Context.DownloadFile0Template.FullName, "multiple.bin", false);

      Downloads.CancelTransfer(Token.TransferId, AbortReason.ClientAbort);
      Token = Downloads.RequestDownloadToken(SourceFileInfo.FullName, false, maxSize);

      Assert.AreEqual(maxSize, Token.DownloadBlockSize);
      Assert.AreEqual(5, Token.TotalBlockCount);


      List<BufferedDataBlock> blockList = new List<BufferedDataBlock>();

      int blockNumber = 0;
      while (true)
      {
        BufferedDataBlock block = Downloads.ReadBlock(Token.TransferId, blockNumber++);
        blockList.Add(block);
        if (block.IsLastBlock) break;
      }

      Assert.AreEqual(5, blockList.Count);
      Assert.AreEqual(maxSize, blockList[4].BlockLength);
    }


    [Test]
    public void If_File_Size_Is_Not_Multitude_Of_Block_Size_Then_Last_Block_Should_Be_Smaller_Then_Others()
    {
      var maxSize = Downloads.MaxBlockSize ?? 2048;

      //upload a file that consists of a total of 5.5 (6) blocks
      File.WriteAllBytes(Context.DownloadFile0Template.FullName, new byte[maxSize * 5 + maxSize / 2]);
      Context.DownloadFile0Template.Refresh();
      SourceFile = Context.DownloadFolder.AddFile(Context.DownloadFile0Template.FullName, "multiple.bin", false);

      Downloads.CancelTransfer(Token.TransferId, AbortReason.ClientAbort);
      Token = Downloads.RequestDownloadToken(SourceFileInfo.FullName, false, maxSize);

      Assert.AreEqual(maxSize, Token.DownloadBlockSize);
      Assert.AreEqual(6, Token.TotalBlockCount);


      List<BufferedDataBlock> blockList = new List<BufferedDataBlock>();

      int blockNumber = 0;
      while (true)
      {
        BufferedDataBlock block = Downloads.ReadBlock(Token.TransferId, blockNumber++);
        blockList.Add(block);
        if (block.IsLastBlock) break;
      }

      Assert.AreEqual(6, blockList.Count);
      Assert.AreEqual(maxSize/2, blockList[5].BlockLength);
    }


    [Test]
    public void Random_Block_Access_Should_Be_Possible()
    {
      Downloads.ReadBlock(Token.TransferId, 5);
      Downloads.ReadBlock(Token.TransferId, 7);
      Downloads.ReadBlock(Token.TransferId, 4);
    }


    [Test]
    [ExpectedException(typeof(DataBlockException))]
    public void Submitting_Negative_Block_Number_Should_Fail()
    {
      Downloads.ReadBlock(Token.TransferId, -1);
    }


    [Test]
    [ExpectedException(typeof(DataBlockException))]
    public void Submitting_Block_Number_Above_Total_Blocks_Should_Fail()
    {
      Downloads.ReadBlock(Token.TransferId, Token.TotalBlockCount);
    }

  }
}