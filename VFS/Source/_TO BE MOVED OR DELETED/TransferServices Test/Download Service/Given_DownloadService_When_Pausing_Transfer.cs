using System;
using System.Threading;
using NUnit.Framework;
using Vfs.Transfer;


namespace TransferServices_Test.Download_Service
{
  [TestFixture]
  public class Given_DownloadService_When_Pausing_Transfer : DownloadServiceTestBase
  {
    private void CompleteReading()
    {
      while (LastBlock == null || LastBlock.IsLastBlock == false)
      {
        long nextBlock = LastBlock == null ? 0 : LastBlock.BlockNumber + 1;
        var block = DownloadService.ReadBlock(Token.TransferId, nextBlock);
        ReceiveBlock(block);
      }

      CollectionAssert.AreEqual(SourceFileContents, ReceivingBuffer);
    }

    private void PauseAndVerifyStatusIsPaused()
    {
      DownloadService.PauseTransfer(Token.TransferId);
      var status = DownloadService.GetTransferStatus(Token.TransferId);
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
      DownloadService.ReadBlock(Token.TransferId, 0);
      PauseAndVerifyStatusIsPaused();
      CompleteReading();
    }


    [Test]
    public void Pausing_Should_Be_Possible_After_Having_Read_Last_Block()
    {
      CompleteReading();
      PauseAndVerifyStatusIsPaused();
      
      //read last block again
      var copy = DownloadService.ReadBlock(Token.TransferId, LastBlock.BlockNumber);
      CollectionAssert.AreEqual(LastBlock.Data, copy.Data);
    }


    [Test]
    public void Pausing_With_Parallel_Block_Read_In_Progress_Should_Work()
    {
      ThreadPool.QueueUserWorkItem(cb =>
                                     {
                                       Thread.Sleep(200);
                                       PauseAndVerifyStatusIsPaused();
                                     });

      ThreadPool.QueueUserWorkItem(cb =>
      {
        try
        {
          DownloadService.ReadBlock(Token.TransferId, 1);
        }
        catch (Exception e)
        {
          Console.WriteLine(e);
        }
      });
    }


  }

}
