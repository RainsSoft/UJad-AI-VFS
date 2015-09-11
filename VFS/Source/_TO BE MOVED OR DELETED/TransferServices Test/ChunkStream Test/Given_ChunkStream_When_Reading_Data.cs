using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Hardcodet.Commons.IO;
using NUnit.Framework;
using Vfs.Transfer;
using Vfs.Transfer.Util;
using Vfs.Util;


namespace TransferServices_Test.ChunkStream_Test
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_ChunkStream_When_Reading_Data
  {
    public byte[] InputBuffer { get; set; }

    public MemoryStream SourceStream { get; set; }

    public ChunkStream TestStream { get; set; }

    public List<byte> TargetBuffer { get; set; }



    #region setup / teardown

    [SetUp]
    public void Init()
    {
      InputBuffer = new byte[45000];
      new Random(DateTime.Now.Millisecond).NextBytes(InputBuffer);
      SourceStream = new MemoryStream(InputBuffer);

      TargetBuffer = new List<byte>();

    }

    [TearDown]
    public void Cleanup()
    {
      //called once after all tests of the fixture are completed
    }

    #endregion



    [Test]
    public void Reading_Chunk_Should_Work_If_Chunk_Offset_Is_Zero()
    {
      TestStream = new ChunkStream(SourceStream, 5000, 0, false);
      
      byte[] buffer = new byte[1234];
      while(true)
      {
        int read = TestStream.Read(buffer, 0, buffer.Length);
        byte[] copy = new byte[read];
        Array.Copy(buffer, 0, copy, 0, read);
        TargetBuffer.AddRange(copy);

        if(read == 0) break;
      }

      Assert.AreEqual(5000, TargetBuffer.Count);
      for (int i = 0; i < TargetBuffer.Count; i++)
      {
        Assert.AreEqual(InputBuffer[i], TargetBuffer[i]);
      }
    }


    [Test]
    public void Reading_Chunk_Should_Work_If_Chunk_Is_In_Middle_Of_Stream()
    {
      TestStream = new ChunkStream(SourceStream, 5000, 1500, true);

      byte[] buffer = new byte[1234];
      while (true)
      {
        int read = TestStream.Read(buffer, 0, buffer.Length);
        byte[] copy = new byte[read];
        Array.Copy(buffer, 0, copy, 0, read);
        TargetBuffer.AddRange(copy);

        if (read == 0) break;
      }

      Assert.AreEqual(5000, TargetBuffer.Count);
      for (int i = 0; i < TargetBuffer.Count; i++)
      {
        Assert.AreEqual(InputBuffer[i+1500], TargetBuffer[i], "Failed at index " + i);
      }
    }

    [Test]
    public void Reading_Chunk_Should_Work_If_Chunk_Goes_Beyond_Source_Stream()
    {
      //only 3000 bytes left
      long offset = SourceStream.Length - 3000;
      TestStream = new ChunkStream(SourceStream, 5000, offset, true);

      byte[] buffer = new byte[1234];
      while (true)
      {
        int read = TestStream.Read(buffer, 0, buffer.Length);
        byte[] copy = new byte[read];
        Array.Copy(buffer, 0, copy, 0, read);
        TargetBuffer.AddRange(copy);

        if (read == 0) break;
      }

      Assert.AreEqual(3000, TargetBuffer.Count);
      for (int i = 0; i < TargetBuffer.Count; i++)
      {
        Assert.AreEqual(InputBuffer[i + offset], TargetBuffer[i], "Failed at index " + i);
      }
    }


    [Test]
    public void Reading_Chunk_Should_Work_If_Chunk_Ends_Exactly_At_Source_Stream()
    {
      //only 3000 bytes left
      long offset = SourceStream.Length - 5000;
      TestStream = new ChunkStream(SourceStream, 5000, offset, true);

      byte[] buffer = new byte[1234];
      while (true)
      {
        int read = TestStream.Read(buffer, 0, buffer.Length);
        byte[] copy = new byte[read];
        Array.Copy(buffer, 0, copy, 0, read);
        TargetBuffer.AddRange(copy);

        if (read == 0) break;
      }

      Assert.AreEqual(5000, TargetBuffer.Count);
      for (int i = 0; i < TargetBuffer.Count; i++)
      {
        Assert.AreEqual(InputBuffer[i + offset], TargetBuffer[i], "Failed at index " + i);
      }
    }


    [Test]
    public void Stream_Should_Only_Return_Remaining_Data_If_Underlying_Stream_Is_Smaller_Thank_Junk()
    {
      Assert.Fail("Test not implemented yet."); //TODO missing test
    }


    [Test]
    public void Reading_Whole_Stream_Should_Return_Full_Contents()
    {
      TestStream = new ChunkStream(SourceStream, InputBuffer.Length, 0, false);

      byte[] buffer = new byte[1234];
      while (true)
      {
        int read = TestStream.Read(buffer, 0, buffer.Length);
        byte[] copy = new byte[read];
        Array.Copy(buffer, 0, copy, 0, read);
        TargetBuffer.AddRange(copy);

        if (read == 0) break;
      }

      CollectionAssert.AreEqual(InputBuffer, TargetBuffer);
    }


    [Test]
    public void Reading_From_Block_Size_Of_Zero_Should_Work_But_Return_No_Data()
    {
      TestStream = new ChunkStream(SourceStream, 0, 0, false);

      byte[] buffer = new byte[1234];
      
      int read = TestStream.Read(buffer, 0, buffer.Length);
      Assert.AreEqual(0, read);
    }
  }
}
