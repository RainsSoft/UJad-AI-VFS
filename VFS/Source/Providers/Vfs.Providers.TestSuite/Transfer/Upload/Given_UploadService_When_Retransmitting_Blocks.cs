using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using NUnit.Framework;
using Vfs.Transfer;
using Vfs.Util;


namespace Vfs.Providers.TestSuite.Transfer.Upload
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_UploadService_When_Retransmitting_Blocks : UploadTestBase
  {
    protected override void InitInternal()
    {
      base.InitInternal();
      InitToken();
    }


    [Test]
    public void Retransmitted_Blocks_Should_Overwrite_Previously_Transferred_Data()
    {
      List<BufferedDataBlock> blocks = CreateBufferedBlocks();

      //modify data in one of the blocks
      Array.Reverse(blocks[10].Data);

      foreach (var block in blocks)
      {
        Uploads.WriteBlock(block);
      }

      //pause in order to release stream
      Uploads.PauseTransfer(Token.TransferId);

      //write correct block data
      blocks = CreateBufferedBlocks();
      Uploads.WriteBlock(blocks[10]);

      //complete and recompare
      Uploads.CompleteTransfer(Token.TransferId);

      CompareUploadToSourceFile();
    }

  }
}
