using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Vfs.Transfer;


namespace Vfs.Test
{
  [TestFixture]
  public class Given_BlockOutputStream_When_Writing_Data
  {
    private byte[] source;
    private List<byte> target;
    private List<BufferedDataBlock> receivedBlocks;
    
    private UploadToken token;
    private BufferedBlockOutputStream stream;

    [SetUp]
    public void Init()
    {
      receivedBlocks = new List<BufferedDataBlock>();
      source = new byte[100000];
      target = new List<byte>();
      new Random(DateTime.Now.Millisecond).NextBytes(source);

      token = new UploadToken { TransferId = "MyToken", MaxBlockSize = 3000 };
    }


    private void WriteBlock(BufferedDataBlock block)
    {
      Assert.AreEqual(block.BlockLength, block.Data.Length);
      Assert.AreEqual(token.TransferId, block.TransferTokenId);

      receivedBlocks.Add(block);
      Assert.AreEqual(receivedBlocks.Count - 1, block.BlockNumber);

      target.AddRange(block.Data);

    }
    

    private void VerifyTransmission()
    {
      Assert.AreEqual(source.Length, target.Count);
      CollectionAssert.AreEqual(source, target);
      Assert.AreEqual(source.Length, stream.WrittenBytes);
    }


    [Test]
    public void Writing_All_Data_Should_Create_Exact_Copy()
    {
      stream = new BufferedBlockOutputStream(token, 2000, WriteBlock);
      int count = source.Length/20;
      for(int i=0; i<20; i++)
      {
        if(i%2 == 0)
        {
          byte[] copy = new byte[count];
          Array.Copy(source, i*count, copy, 0, count);
          stream.Write(copy, 0, copy.Length);
        }
        else
        {
          stream.Write(source, i*count, count);
        }
      }

      stream.Flush();
      VerifyTransmission();
    }



    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Flushing_Buffer_Should_Create_Multiple_Blocks_If_Buffer_Is_Bigger_Than_Maximum_BlockSize()
    {
      //set auto-stream so it doesn't auto-flush
      stream = new BufferedBlockOutputStream(token, 10000, WriteBlock);
      token.MaxBlockSize = 3000;

      //write 7000 bytes - no flush
      stream.Write(source, 0, 7000);
      Assert.IsEmpty(receivedBlocks);
      Assert.AreEqual(0, stream.WrittenBytes);
      Assert.AreEqual(7000, stream.InternalBufferSize);

      stream.Flush();
      Assert.AreEqual(3, receivedBlocks.Count);
      Assert.AreEqual(3000, receivedBlocks[0].BlockLength);
      Assert.AreEqual(3000, receivedBlocks[1].BlockLength);
      Assert.AreEqual(1000, receivedBlocks[2].BlockLength);
    }


    [Test]
    public void Submitting_Data_Below_Threshold_Should_Not_Flush()
    {
      //configure stream to flush after 2000 bytes
      stream = new BufferedBlockOutputStream(token, 10000, WriteBlock);
      token.MaxBlockSize = 3000;

      stream.Write(source, 0, 7000);
      Assert.IsEmpty(receivedBlocks);
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Submitting_Data_At_Or_Above_Threshold_Should_Flush_Immediately()
    {
      //configure stream to flush after 1000 bytes
      stream = new BufferedBlockOutputStream(token, 1000, WriteBlock);
      token.MaxBlockSize = 3000;

      //write 1000 bytes - this should flush 
      stream.Write(source, 0, 1000);
      Assert.AreEqual(1, receivedBlocks.Count);
      Assert.AreEqual(1000, receivedBlocks.First().BlockLength);

      //write another 2000 bytes - this should flush again
      stream.Write(source, 0, 2000);
      Assert.AreEqual(2, receivedBlocks.Count);
      Assert.AreEqual(2000, receivedBlocks[1].BlockLength);
    }



    [Test]
    public void AutoFlushing_Should_Create_Blocks_As_Big_As_Max_Block_Size()
    {
      //configure stream to flush after 1000 bytes
      stream = new BufferedBlockOutputStream(token, 1000, WriteBlock);
      token.MaxBlockSize = 3000;

      //this should create a block with the max size, the remaining 500
      //if below the threshold, so the should not be written
      stream.Write(source, 0, 3500);
      Assert.AreEqual(1, receivedBlocks.Count);
      Assert.AreEqual(3000, receivedBlocks.First().BlockLength);
    }


    [Test]
    public void AutoFlushing_Should_Write_As_Much_As_Possible_Without_Creating_Blocks_Smaller_Than_Threshold()
    {
      //configure stream to flush after 1000 bytes
      stream = new BufferedBlockOutputStream(token, 1000, WriteBlock);
      token.MaxBlockSize = 3000;

      //this should create a block with the max size, the remaining 500
      //if below the threshold, so the should not be written
      stream.Write(source, 0, 4500);
      Assert.AreEqual(2, receivedBlocks.Count);
      Assert.AreEqual(3000, receivedBlocks[0].BlockLength);
      Assert.AreEqual(1500, receivedBlocks[1].BlockLength);

      stream.Write(source, 0, 3500);
      Assert.AreEqual(3, receivedBlocks.Count);
      Assert.AreEqual(3000, receivedBlocks[2].BlockLength);
      Assert.AreEqual(500, stream.InternalBufferSize);
    }


    [Test]
    public void If_Max_Block_Size_Is_Below_Auto_Flush_Trigger_Should_Even_Send_Full_Blocks_If_Under_Threshold_While_Flushing ()
    {
      //configure stream to flush after 1000 bytes
      stream = new BufferedBlockOutputStream(token, 3000, WriteBlock);
      token.MaxBlockSize = 1000;

      //this should create 4 blocks with the max size, the remaining 500
      //if below the threshold AND the max size, so the should not be written
      stream.Write(source, 0, 4500);
      Assert.AreEqual(4, receivedBlocks.Count);
      receivedBlocks.Do(b => Assert.AreEqual(1000, b.BlockLength));

      Assert.AreEqual(500, stream.InternalBufferSize);
      Assert.AreEqual(4000, stream.WrittenBytes);
    }



    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Exceeding_Threshold_In_Multiple_Writes_Should_Only_Flush_After_Exceeding()
    {
      //configure stream to flush after 5000 bytes
      stream = new BufferedBlockOutputStream(token, 5000, WriteBlock);
      token.MaxBlockSize = 2500;

      //write 6000 bytes - this should flush the first 5000 bytes
      stream.Write(source, 0, 2000);
      Assert.AreEqual(0, receivedBlocks.Count);
      stream.Write(source, 0, 4000);

      Assert.AreEqual(2, receivedBlocks.Count);
      Assert.AreEqual(2500, receivedBlocks[0].BlockLength);
      Assert.AreEqual(2500, receivedBlocks[1].BlockLength);
    }



    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Flushing_Stream_Should_Submit_All_Buffered_Data()
    {
      stream = new BufferedBlockOutputStream(token, 50000, WriteBlock);
      token.MaxBlockSize = 3000;

      stream.Write(source, 0, 7000);
      Assert.AreEqual(0, receivedBlocks.Count);
      stream.Flush();
      Assert.AreEqual(3, receivedBlocks.Count);
      Assert.AreEqual(3000, receivedBlocks[0].BlockLength);
      Assert.AreEqual(3000, receivedBlocks[1].BlockLength);
      Assert.AreEqual(1000, receivedBlocks[2].BlockLength);
      Assert.AreEqual(0, stream.InternalBufferSize);
    }


    [Test]
    public void Setting_The_AutoFlush_Threshold_To_Zero_Should_Flush_Every_Write()
    {
      stream = new BufferedBlockOutputStream(token, 0, WriteBlock);
      token.MaxBlockSize = 1000;

      for(int i=0; i<5; i++)
      {
        //the first write writes 0 bytes
        stream.Write(source, 0, i * 100);
        Assert.AreEqual(i, receivedBlocks.Count);
        Assert.AreEqual(0, stream.InternalBufferSize);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Submitting_An_Empty_Buffer_Should_Work_But_Doesnt_Flash()
    {
      stream = new BufferedBlockOutputStream(token, 0, WriteBlock);
      token.MaxBlockSize = 1000;

      //write 0 bytes
      byte[] src = new byte[0];
      stream.Write(src, 0, 0);
      Assert.AreEqual(0, receivedBlocks.Count);
      Assert.AreEqual(0, stream.InternalBufferSize);
      Assert.AreEqual(0, stream.WrittenBytes);
    }



    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Buffer_And_Written_Bytes_Indication_Should_Remain_Untouched_If_Submission_Causes_Exception()
    {
      bool throwException = true;
      Action<BufferedDataBlock> action = (block) =>
                                   {
                                     if(throwException) throw new InvalidOperationException("test");
                                     receivedBlocks.Add(block);
                                   };

      stream = new BufferedBlockOutputStream(token, 3000, action);
      token.MaxBlockSize = 20000;

      try
      {
        stream.Write(source, 0, 5000);
        Assert.Fail("Expected exception.");
      }
      catch (InvalidOperationException)
      {
      }

      //nothing should have written or even discarded
      Assert.AreEqual(0, receivedBlocks.Count);
      Assert.AreEqual(0, stream.WrittenBytes);
      Assert.AreEqual(5000, stream.InternalBufferSize);

      //disable exception
      throwException = false;

      //write - should trigger another autoflush
      stream.WriteByte(3);

      //whole buffer should have been flushed
      Assert.AreEqual(1, receivedBlocks.Count);
      Assert.AreEqual(5001, stream.WrittenBytes);
      Assert.AreEqual(0, stream.InternalBufferSize);
    }
  }

}
