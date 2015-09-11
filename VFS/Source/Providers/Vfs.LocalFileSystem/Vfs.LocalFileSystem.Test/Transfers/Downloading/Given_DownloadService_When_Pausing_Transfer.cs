using System;
using System.Threading;
using NUnit.Framework;
using Vfs.LocalFileSystem.Test.Transfers.Downloading;
using Vfs.Transfer;


namespace TransferServices_Test.Download_Service
{
  [TestFixture]
  public class Given_DownloadService_When_Pausing_Transfer : DownloadServiceTestBase
  {
    protected override void InitInternal()
    {
      base.InitInternal();
      InitToken();
    }


    private void CompleteReading()
    {
      while (LastBlock == null || LastBlock.IsLastBlock == false)
      {
        long nextBlock = LastBlock == null ? 0 : LastBlock.BlockNumber + 1;
        var block = DownloadHandler.ReadBlock(Token.TransferId, nextBlock);
        ReceiveBlock(block);
      }

      CollectionAssert.AreEqual(SourceFileContents, ReceivingBuffer.ToArray());
    }

    private void PauseAndVerifyStatusIsPaused()
    {
      DownloadHandler.PauseTransfer(Token.TransferId);
      var status = DownloadHandler.GetTransferStatus(Token.TransferId);
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
      DownloadHandler.ReadBlock(Token.TransferId, 0);
      PauseAndVerifyStatusIsPaused();
      CompleteReading();
    }

    [Test]
    public void Resuming_Reading_Should_Switch_Status_Back_To_Running()
    {
      DownloadHandler.ReadBlock(Token.TransferId, 0);
      PauseAndVerifyStatusIsPaused();

      //read next block
      DownloadHandler.ReadBlock(Token.TransferId, 1);
      var status = DownloadHandler.GetTransferStatus(Token.TransferId);
      Assert.AreEqual(TransferStatus.Running, status);
    }

    [Test]
    public void Pausing_Should_Be_Possible_After_Having_Read_Last_Block()
    {
      CompleteReading();
      PauseAndVerifyStatusIsPaused();
      
      //read last block again
      var copy = DownloadHandler.ReadBlock(Token.TransferId, LastBlock.BlockNumber);
      CollectionAssert.AreEqual(LastBlock.Data, copy.Data);
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
          DownloadHandler.ReadBlock(Token.TransferId, 1);
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
