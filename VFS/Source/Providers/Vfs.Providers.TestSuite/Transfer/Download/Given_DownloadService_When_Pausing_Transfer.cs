using System;
using System.IO;
using System.Threading;
using System.Xml.Schema;
using NUnit.Framework;
using Vfs.Transfer;
using Vfs.Transfer.Util;


namespace Vfs.Providers.TestSuite.Transfer.Download
{
  [TestFixture]
  public class Given_DownloadService_When_Pausing_Transfer : DownloadTestBase
  {
    private BufferedDataBlock recentBlock;

    protected override void InitInternal()
    {
      base.InitInternal();
      InitToken();
      recentBlock = null;
    }


    /// <summary>
    /// Updates the <see cref="ReceivingBuffer"/> with the data
    /// of a given block, and performs a few simple tests.
    /// </summary>
    /// <param name="block"></param>
    private void ReceiveBlock(BufferedDataBlock block)
    {
      using(var stream = File.Open(TargetFilePath, FileMode.Append))
      {
        stream.Write(block.Data, 0, block.Data.Length);
        recentBlock = block;
      }
    }


    private void CompleteReading()
    {
      while (recentBlock == null || recentBlock.IsLastBlock == false)
      {
        long nextBlock = recentBlock == null ? 0 : recentBlock.BlockNumber + 1;
        var block = Downloads.ReadBlock(Token.TransferId, nextBlock);
        ReceiveBlock(block);
      }

      //make sure we have an exact copy
      FileAssert.AreEqual(Context.DownloadFile0Template.FullName, TargetFilePath);
    }



    private void PauseAndVerifyStatusIsPaused()
    {
      Downloads.PauseTransfer(Token.TransferId);
      var status = Downloads.GetTransferStatus(Token.TransferId);
      Assert.AreEqual(TransferStatus.Paused, status);
    }


    
    [Test]
    public void Pausing_Should_Be_Possible_Before_Reading_First_Block()
    {
      PauseAndVerifyStatusIsPaused();
      CompleteReading();
    }


    [Test]
    public void Pausing_Should_Be_Possible_After_Having_Started_Downloading()
    {
      Downloads.ReadBlock(Token.TransferId, 0);
      PauseAndVerifyStatusIsPaused();
      CompleteReading();
    }

    [Test]
    public void Resuming_Reading_Should_Switch_Status_Back_To_Running()
    {
      Downloads.ReadBlock(Token.TransferId, 0);
      PauseAndVerifyStatusIsPaused();

      //read next block
      Downloads.ReadBlock(Token.TransferId, 1);
      var status = Downloads.GetTransferStatus(Token.TransferId);
      Assert.AreEqual(TransferStatus.Running, status);
    }

    [Test]
    public void Pausing_Should_Be_Possible_After_Having_Read_Last_Block()
    {
      CompleteReading();
      PauseAndVerifyStatusIsPaused();
      
      //read last block again
      var copy = Downloads.ReadBlock(Token.TransferId, recentBlock.BlockNumber);
      CollectionAssert.AreEqual(recentBlock.Data, copy.Data);
    }


    [Test]
    public void Pausing_With_Parallel_Block_Read_In_Progress_Should_Work()
    {
      ThreadPool.QueueUserWorkItem(cb =>
                                     {
                                       Thread.Sleep(200);
                                       try
                                       {
                                         PauseAndVerifyStatusIsPaused();
                                       }
                                       catch(Exception e)
                                       {
                                         Assert.Fail(e.Message);
                                       }
                                     });

      ThreadPool.QueueUserWorkItem(cb =>
                                     {
                                       try
                                       {
                                         Downloads.ReadBlock(Token.TransferId, 1);
                                       }
                                       catch (Exception e)
                                       {
                                         Console.WriteLine(e);
                                         Assert.Fail("Got an exception while reading block.");
                                       }
                                     });

      Thread.Sleep(3000);
    }


  }
}