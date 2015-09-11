using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using NUnit.Framework;
using Vfs.Transfer;


namespace Vfs.Providers.TestSuite.Transfer.Download
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_DownloadService_When_Requesting_Blocks_Of_Specified_Size : DownloadTestBase
  {

    [Test]
    public void Requesting_Token_With_Block_Size_Below_Maximum_Should_Return_Small_Blocks()
    {
      if (!Downloads.MaxBlockSize.HasValue)
      {
        Assert.Inconclusive("Download service does not provide maximum block size");
      }

      var blockSize = Downloads.MaxBlockSize.Value/2;

      Token = Downloads.RequestDownloadToken(SourceFileInfo.FullName, false, blockSize);
      Assert.AreEqual(blockSize, Token.DownloadBlockSize);

      BufferedDataBlock block = Downloads.ReadBlock(Token.TransferId, 0);
      Assert.AreEqual(blockSize, block.BlockLength);
      Assert.AreEqual(blockSize, block.Data.Length);
    }


    [Test]
    public void Requesting_Block_Size_With_Specified_Value_Should_Be_Allowed_If_Service_Does_Not_Have_Limit()
    {
      if (Downloads.MaxBlockSize.HasValue)
      {
        Assert.Inconclusive("Download service has a maximum block size - did not run test.");
      }

      var blockSize = 2048;

      Token = Downloads.RequestDownloadToken(SourceFileInfo.FullName, false, blockSize);
      Assert.AreEqual(blockSize, Token.DownloadBlockSize);

      BufferedDataBlock block = Downloads.ReadBlock(Token.TransferId, 0);
      Assert.AreEqual(blockSize, block.BlockLength);
      Assert.AreEqual(blockSize, block.Data.Length);
    }


    [Test]
    public void Requesting_Token_With_Block_Size_Above_Max_Size_Should_Fall_Back_To_Maxium()
    {
      if(!Downloads.MaxBlockSize.HasValue)
      {
        Assert.Inconclusive("Download service does not provide maximum block size");
      }

      var expectedSize = Downloads.MaxBlockSize.Value;
      var blockSize = expectedSize + 10000;
 
      Token = Downloads.RequestDownloadToken(SourceFileInfo.FullName, false, blockSize);
      Assert.AreEqual(expectedSize, Token.DownloadBlockSize);

      BufferedDataBlock block = Downloads.ReadBlock(Token.TransferId, 0);
      Assert.AreEqual(expectedSize, block.BlockLength);
      Assert.AreEqual(expectedSize, block.Data.Length);
    }



  }
}
