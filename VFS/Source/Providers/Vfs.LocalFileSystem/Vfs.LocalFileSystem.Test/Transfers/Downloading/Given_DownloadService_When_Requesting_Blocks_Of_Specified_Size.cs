using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using NUnit.Framework;
using Vfs.Transfer;


namespace Vfs.LocalFileSystem.Test.Transfers.Downloading
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_DownloadService_When_Requesting_Blocks_Of_Specified_Size : DownloadServiceTestBase
  {

    [Test]
    public void Requesting_Token_With_Block_Size_Below_Default_Should_Return_Small_Blocks()
    {
      var blockSize = 20480;

      Token = DownloadHandler.RequestDownloadToken(SourceFileInfo.FullName, false, blockSize);
      Assert.AreEqual(blockSize, Token.DownloadBlockSize);

      BufferedDataBlock block = DownloadHandler.ReadBlock(Token.TransferId, 0);
      Assert.AreEqual(blockSize, block.BlockLength);
      Assert.AreEqual(blockSize, block.Data.Length);
    }

    [Test]
    public void Requesting_Token_With_Block_Size_Above_Default_Should_Return_Bigger_Blocks()
    {
      var blockSize = FileSystemConfiguration.DefaultDownloadBlockSize + 50;

      Token = DownloadHandler.RequestDownloadToken(SourceFileInfo.FullName, false, blockSize);
      Assert.AreEqual(blockSize, Token.DownloadBlockSize);

      BufferedDataBlock block = DownloadHandler.ReadBlock(Token.TransferId, 0);
      Assert.AreEqual(blockSize, block.BlockLength);
      Assert.AreEqual(blockSize, block.Data.Length);
    }


    [Test]
    public void Requesting_Token_With_Block_Size_Above_Max_Size_Should_Fall_Back_To_Maxium()
    {
      var blockSize = FileSystemConfiguration.MaxDownloadBlockSize.Value + 10000;
      var expectedSize = FileSystemConfiguration.MaxDownloadBlockSize.Value;

      Token = DownloadHandler.RequestDownloadToken(SourceFileInfo.FullName, false, blockSize);
      Assert.AreEqual(expectedSize, Token.DownloadBlockSize);

      BufferedDataBlock block = DownloadHandler.ReadBlock(Token.TransferId, 0);
      Assert.AreEqual(expectedSize, block.BlockLength);
      Assert.AreEqual(expectedSize, block.Data.Length);
    }

  }
}
