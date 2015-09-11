using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hardcodet.Commons.IO;
using NUnit.Framework;
using Vfs.LocalFileSystem.Transfer;
using Vfs.Transfer;
using Vfs.Util;

namespace Vfs.LocalFileSystem.Test.Transfers.Downloading
{
  public abstract class DownloadServiceTestBase : DirectoryTestBase
  {
    public string SourceFilePath { get; private set; }
    public FileInfo SourceFile { get; set; }
    public DirectoryInfo ParentDirectory { get; private set; }
    public DownloadToken Token { get; set; }
    public byte[] SourceFileContents { get; set; }
    public List<byte> ReceivingBuffer { get; set; }
    public VirtualFileInfo SourceFileInfo { get; set; }


    public LocalDownloadHandler DownloadHandler
    {
      get { return (LocalDownloadHandler)provider.DownloadTransfers; }
    }

    /// <summary>
    /// The last received block. Set by the <see cref="ReceiveBlock"/> method.
    /// </summary>
    public BufferedDataBlock LastBlock { get; set; }


    protected override void InitInternal()
    {
      SourceFileContents = new byte[12345678];
      new Random(DateTime.Now.Millisecond).NextBytes(SourceFileContents);

      //create parent folder
      ParentDirectory = rootDirectory.CreateSubdirectory("Dwnld_Parent");

      //create file
      SourceFilePath = FileUtil.CreateTempFilePath(ParentDirectory.FullName, "_dwlndsource", "bin");
      File.WriteAllBytes(SourceFilePath, SourceFileContents);
      SourceFile = new FileInfo(SourceFilePath);
      SourceFileInfo = provider.GetFileInfo(SourceFilePath);


      //prepare target buffer
      ReceivingBuffer = new List<byte>();
      LastBlock = null;
    }


    /// <summary>
    /// Gets a download token for the <see cref="SourceFileInfo"/> and
    /// assigns it to the <see cref="Token"/> property.
    /// </summary>
    /// <returns>The retrieved token.</returns>
    protected DownloadToken InitToken()
    {
      Token = DownloadHandler.RequestDownloadToken(SourceFileInfo.FullName, false);
      return Token;
    }

    protected DownloadToken GetToken(string resourceId)
    {
      return DownloadHandler.GetTransfersForResource(resourceId).Single();
    }


    protected override void CleanupInternal()
    {
      if (Token != null)
      {
        if (DownloadHandler.GetTransferStatus(Token.TransferId) != TransferStatus.UnknownTransfer)
        {
          DownloadHandler.CancelTransfer(Token.TransferId, AbortReason.Undefined);
        }
      }

      SystemTime.Reset();
      ParentDirectory.Delete(true);

      base.CleanupInternal();
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