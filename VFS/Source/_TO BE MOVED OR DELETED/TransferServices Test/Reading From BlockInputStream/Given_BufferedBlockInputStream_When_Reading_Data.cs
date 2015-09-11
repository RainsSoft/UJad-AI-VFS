using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

using NUnit.Framework;
using Vfs;
using Vfs.Transfer;
using Vfs.Transfer.Util;


namespace TransferServices_Test.Reading_From_BlockInputStream
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_BufferedBlockInputStream_When_Reading_Data
  {
    List<BufferedDataBlock> Input { get; set; }
    List<byte> ReceivedData { get; set; }
    public BufferedBlockInputStream InputStream { get; set; }

    [SetUp]
    public void Init()
    {
      Input = new List<BufferedDataBlock>();
      for(int i=0; i<10; i++)
      {
        byte[] data = new byte[200000];
        new Random().NextBytes(data);

        BufferedDataBlock db = new BufferedDataBlock
                         {
                           BlockLength = 200000,
                           BlockNumber = i,
                           Data = data,
                           IsLastBlock = i == 9,
                           Offset = i*200000,
                           TransferTokenId = "xxx"
                         };
        Input.Add(db);
      }

      ReceivedData = new List<byte>();
      InputStream = new BufferedBlockInputStream((i) => Input[(int)i], 200000*10);
    }

    [TearDown]
    public void Cleanup()
    {
      //called once after all tests of the fixture are completed
    }


    [Test]
    public void Received_Data_Should_Match_Input()
    {
      int total = 0;
      byte[] buffer = new byte[150325];
      while(true)
      {
        int read =  InputStream.Read(buffer, 0, buffer.Length);
        ReceivedData.AddRange(buffer);
        total += read;
        if(read == 0) break;
      }

      Assert.AreEqual(200000*10, total);

      foreach (BufferedDataBlock db in Input)
      {
        var chunk = ReceivedData.GetRange((int)(db.BlockNumber*db.BlockLength), (int) db.BlockLength);
        CollectionAssert.AreEqual(db.Data, chunk);
      }
    }

  }
}
