using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml;

using NUnit.Framework;
using Vfs.Transfer;


namespace Vfs.Providers.TestSuite.Transfer.Upload
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_UploadService_When_Pausing_Transfer : UploadTestBase
  {
    protected override void InitInternal()
    {
      base.InitInternal();
      InitToken();
    }


    private void PauseAndVerifyStatusIsPaused()
    {
      Uploads.PauseTransfer(Token.TransferId);
      var status = Uploads.GetTransferStatus(Token.TransferId);
      Assert.AreEqual(TransferStatus.Paused, status);
    }


    [Test]
    public void Pausing_And_Resuming_Transfer_Should_Update_Reported_Transfer_Status()
    {
      List<BufferedDataBlock> blocks = CreateBufferedBlocks();

      foreach (var block in blocks)
      {
        Uploads.PauseTransfer(Token.TransferId);        
        Assert.AreEqual(TransferStatus.Paused, Uploads.GetTransferStatus(Token.TransferId));

        Uploads.WriteBlock(block);
        Assert.AreEqual(TransferStatus.Running, Uploads.GetTransferStatus(Token.TransferId));
      }

      //complete and recompare
      Uploads.CompleteTransfer(Token.TransferId);

      CompareUploadToSourceFile();
    }



    [Test]
    public void Pausing_With_Parallel_Block_Read_In_Progress_Should_Work()
    {
      var blocks = CreateBufferedBlocks();
      bool isRunning = true;
      Exception exception = null;

      ThreadPool.QueueUserWorkItem(cb =>
      {
        while (isRunning)
        {
          Thread.Sleep(50);
          try
          {
            if (Uploads.GetTransferStatus(Token.TransferId) == TransferStatus.Running)
            {
              PauseAndVerifyStatusIsPaused();
            }
          }
          catch (Exception e)
          {
            exception = e;
          }
        }
      });


      ThreadPool.QueueUserWorkItem(cb =>
      {
        try
        {
          blocks.Do(b => Uploads.WriteBlock(b));
        }
        catch (Exception e)
        {
          Console.WriteLine(e);
          Assert.Fail("Got an exception while reading block.");
        }
      });

      //both actions run on background threads - make a break
      Thread.CurrentThread.Join(3000);

      Uploads.CompleteTransfer(Token.TransferId);
      Assert.IsNull(exception, exception == null ? "" : exception.Message);

      CompareUploadToSourceFile();
    }
  }
}
