using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

using NUnit.Framework;
using Vfs.Transfer;
using Vfs.Transfer.Util;


namespace Vfs.LocalFileSystem.Test.Transfers.Uploading
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_UploadService_When_Submitting_Invalid_Blocks : UploadServiceTestBase
  {

    /// <summary>
    /// Contains the first 10000 bytes of the source file.
    /// </summary>
    public BufferedDataBlock BufferedBlock { get; set; }

    /// <summary>
    /// Streams the first 10000 bytes of the source file.
    /// </summary>
    public StreamedDataBlock StreamedBlock { get; set; }

    protected override void InitInternal()
    {
      base.InitInternal();
      InitToken();

      byte[] buffer = new byte[10000];
      Array.Copy(SourceFileContents, buffer, 10000);

      BufferedBlock = new BufferedDataBlock
                        {
                          TransferTokenId = Token.TransferId,
                          BlockLength = 10000,
                          Data = buffer
                        };

      StreamedBlock = new StreamedDataBlock
                        {
                          TransferTokenId = Token.TransferId,
                          BlockLength = 10000,
                          Data = new ChunkStream(new MemoryStream(SourceFileContents), 10000, 0, false)
                        };
    }



    [ExpectedException(typeof(DataBlockException))]
    [Test]
    public void Blocks_With_Negative_Block_Length_Should_Be_Rejected()
    {
      BufferedBlock.BlockLength = -5;
      UploadHandler.WriteBlock(BufferedBlock);
    }


    [ExpectedException(typeof(DataBlockException))]
    [Test]
    public void Blocks_With_Length_Different_Than_Their_Real_Buffer_Should_Be_Rejected()
    {
      BufferedBlock.BlockLength = BufferedBlock.Data.Length - 1;
      UploadHandler.WriteBlock(BufferedBlock);
    }



    [ExpectedException(typeof(DataBlockException))]
    [Test]
    public void Blocks_That_Exceed_The_Max_Block_Size_Should_Be_Rejected()
    {
      BufferedBlock.Data = new byte[Token.MaxBlockSize.Value + 1];
      BufferedBlock.BlockLength = BufferedBlock.Data.Length;
      UploadHandler.WriteBlock(BufferedBlock);
    }


    [ExpectedException(typeof(DataBlockException))]
    [Test]
    public void Submitting_Block_And_Offset_That_Go_Beyond_Max_File_Size_Should_Be_Rejected()
    {
      BufferedBlock.Offset = Token.MaxResourceSize.Value - BufferedBlock.BlockLength.Value + 1;
      UploadHandler.WriteBlock(BufferedBlock);
    }



    [ExpectedException(typeof(DataBlockException))]
    [Test]
    public void Submitting_A_Stream_Which_Is_Longer_Than_The_Max_Block_Length_Should_Abort_Transfer()
    {
      long streamLength = Token.MaxBlockSize.Value + 1;
      StreamedBlock.Data = new ChunkStream(new MemoryStream(SourceFileContents), streamLength, 0, false);
      UploadHandler.WriteBlockStreamed(StreamedBlock);
    }




    [ExpectedException(typeof(DataBlockException))]
    [Test]
    public void Submitting_A_Stream_Which_Writes_Beyond_Maximum_File_Size_Should_Abort_Transfer()
    {
      //the stream will write 1 byte too far - it indicates its only 4999 bytes, but will write 5000
      StreamedBlock.BlockLength = 4999;
      StreamedBlock.Offset = SourceFileContents.Length - 5000;
      
      long streamLength = 5001;
      StreamedBlock.Data = new ChunkStream(new MemoryStream(SourceFileContents), streamLength, 0, false);

      UploadHandler.WriteBlockStreamed(StreamedBlock);
    }



    [ExpectedException(typeof(UnknownTransferException))]
    [Test]
    public void Blocks_Of_Unknown_Transfers_Should_Be_Rejected()
    {
      var block = CreateBufferedBlocks().First();
      block.TransferTokenId = "AAA";
      UploadHandler.WriteBlock(block);
    }

    [Test]
    public void Rejected_Block_Should_Not_Be_Added_To_Transferred_Block_Table()
    {
      try
      {
        var block = CreateBufferedBlocks().First();
        block.BlockLength++;
        UploadHandler.WriteBlock(block);
        Assert.Fail("Expected exception for invalid data block.");
      }
      catch (DataBlockException expected)
      {
      }

      var blocks = UploadHandler.GetTransferredBlocks(Token.TransferId);
      Assert.AreEqual(0, blocks.Count());
    }
  }
}