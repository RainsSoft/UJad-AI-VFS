using System;
using System.Collections.Generic;
using System.IO;
using Hardcodet.Commons.IO;
using NUnit.Framework;
using Vfs;
using Vfs.LocalFileSystem;
using Vfs.LocalFileSystem.Transfer;
using Vfs.Transfer;
using Vfs.Util;

namespace TransferServices_Test.Download_Service
{
  public abstract class DownloadServiceTestBase
  {
    public LocalFileSystemDownloadService DownloadService { get; private set; }
    public string SourceFilePath { get; private set; }
    public FileInfo SourceFile { get; set; }
    public DirectoryInfo RootDir { get; private set; }
    public DownloadToken Token { get; set; }
    public byte[] SourceFileContents { get; set; }
    public List<byte> ReceivingBuffer { get; set; }

    /// <summary>
    /// The last received block. Set by the <see cref="ReceiveBlock"/> method.
    /// </summary>
    public BufferedDataBlock LastBlock { get; set; }


    [SetUp]
    public void Init()
    {
      RootDir = FileUtil.CreateTempFolder("_" + GetType().Name);

      var provider = new LocalFileSystemProvider(RootDir, true);
      //prepare test dir and paths
      DownloadService = (LocalFileSystemDownloadService) provider.DownloadTransfers;
      
      
      SourceFilePath = FileUtil.CreateTempFilePath(RootDir.FullName, "source", "bin");

      //create file
      SourceFileContents = new byte[5 * 1024 * 1024];
      new Random(DateTime.Now.Millisecond).NextBytes(SourceFileContents);
      File.WriteAllBytes(SourceFilePath, SourceFileContents);
      SourceFile = new FileInfo(SourceFilePath);

      //prepare target buffer
      ReceivingBuffer = new List<byte>();
      LastBlock = null;

      //get token
      Token = DownloadService.RequestDownloadToken(SourceFilePath, false);

      InitInternal();
    }


    protected virtual void InitInternal()
    {
    }

    [TearDown]
    public void Cleanup()
    {
      CleanupInternal();
      
      if(DownloadService.GetTransferStatus(Token.TransferId) != TransferStatus.UnknownTransfer)
      {
        DownloadService.CancelTransfer(Token.TransferId, AbortReason.Undefined);
      }

      SystemTime.Reset();
      RootDir.Delete(true);
    }


    protected virtual void CleanupInternal()
    {

    }


    /// <summary>
    /// Updates the <see cref="ReceivingBuffer"/> with the data
    /// of a given block, and performs a few simple tests.
    /// </summary>
    /// <param name="block"></param>
    protected virtual void ReceiveBlock(BufferedDataBlock block)
    {
      Assert.AreEqual(Token.TransferId, block.TransferTokenId);
      Assert.AreEqual(ReceivingBuffer.Count, block.Offset);

      Assert.AreEqual(block.BlockLength, block.Data.Length);
      ReceivingBuffer.AddRange(block.Data);
      LastBlock = block;
    }
  }
}
