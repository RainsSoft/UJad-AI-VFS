using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Vfs.Transfer;


namespace Vfs.Test
{
  [TestFixture]
  public class Given_BlockOutputStream_When_Resuming_Transfer
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

      //token indicates 10 blocks were already submitted
      token = new UploadToken { TransferId = "MyToken", MaxBlockSize = 3000, TransmittedBlockCount = 10};
    }


    private void WriteBlock(BufferedDataBlock block)
    {
      Assert.AreEqual(block.BlockLength, block.Data.Length);
      Assert.AreEqual(token.TransferId, block.TransferTokenId);

      receivedBlocks.Add(block);
      target.AddRange(block.Data);
    }


    [Test]
    public void Last_Transmitted_Block_Number_Should_Match_Token_After_Flushing()
    {
      token.MaxBlockSize = 3000;
      stream = new BufferedBlockOutputStream(token, 2000, WriteBlock);

      Assert.IsNull(stream.LastTransmittedBlockNumber);

      stream.Write(source, 0, 100);
      stream.Flush();

      Assert.AreEqual(1, receivedBlocks.Count);
      Assert.AreEqual(receivedBlocks[0].BlockNumber, stream.LastTransmittedBlockNumber);
      Assert.AreEqual(token.TransmittedBlockCount, stream.LastTransmittedBlockNumber);
    }


 
    [Test]
    public void Resuming_Takes_Offset_Of_Previously_Written_Blocks_Into_Account()
    {
      token.MaxBlockSize = 3000;
      token.NextBlockOffset = 15000;
      stream = new BufferedBlockOutputStream(token, 2000, WriteBlock);

      stream.Write(source, 0, 100);
      stream.Flush();

      Assert.AreEqual(100, stream.WrittenBytes);
      Assert.AreEqual(1, receivedBlocks.Count); 
      //check the offset of the block
      Assert.AreEqual(15000, receivedBlocks[0].Offset);

      //flush another one
      stream.Write(source, 0, 200);
      stream.Flush();

      Assert.AreEqual(300, stream.WrittenBytes);
      Assert.AreEqual(2, receivedBlocks.Count);
      //check the offset of the block
      Assert.AreEqual(15100, receivedBlocks[1].Offset);
    }

  }

}
