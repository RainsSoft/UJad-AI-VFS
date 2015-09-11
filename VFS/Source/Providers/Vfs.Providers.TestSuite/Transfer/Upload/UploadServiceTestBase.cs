using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hardcodet.Commons.IO;
using NUnit.Framework;
using Vfs.LocalFileSystem.Transfer;
using Vfs.Transfer;
using Vfs.Util;

namespace Vfs.LocalFileSystem.Test.Transfers.Uploading
{
  public abstract class UploadServiceTestBase : DirectoryTestBase
  {
    public string SourceFilePath { get; private set; }
    public FileInfo SourceFile { get; set; }
    public DirectoryInfo ParentDirectory { get; private set; }
    public byte[] SourceFileContents { get; set; }

    public string TargetFilePath { get; set; }
    public FileInfo TargetFile { get; set; }

    public UploadToken Token { get; set; }


    public IUploadTransferHandler UploadHandler
    {
      get { return provider.UploadTransfers; }
    }


    protected override void InitInternal()
    {
      SourceFileContents = new byte[12345678];
      new Random(DateTime.Now.Millisecond).NextBytes(SourceFileContents);

      //create test folder within root directory
      ParentDirectory = rootDirectory.CreateSubdirectory("Upload_Parent");

      //create file to be uploaded
      SourceFilePath = FileUtil.CreateTempFilePath(ParentDirectory.FullName, "_uploadsource", "bin");
      File.WriteAllBytes(SourceFilePath, SourceFileContents);
      SourceFile = new FileInfo(SourceFilePath);

      TargetFilePath = FileUtil.CreateTempFilePath(ParentDirectory.FullName, "_uploadtarget", "bin");
      TargetFile = new FileInfo(TargetFilePath);
    }



    protected override void CleanupInternal()
    {
      if (Token != null)
      {
        if (UploadHandler.GetTransferStatus(Token.TransferId) != TransferStatus.UnknownTransfer)
        {
          UploadHandler.CancelTransfer(Token.TransferId, AbortReason.Undefined);
        }
      }

      SystemTime.Reset();
      ParentDirectory.Delete(true);

      base.CleanupInternal();
    }


    /// <summary>
    /// Gets a download token for the <see cref="TargetFile"/> and
    /// assigns it to the <see cref="Token"/> property.
    /// </summary>
    /// <returns>The retrieved token.</returns>
    protected UploadToken InitToken()
    {
      Token = UploadHandler.RequestUploadToken(TargetFilePath, false, SourceFile.Length, "binary");
      return Token;
    }

    protected UploadToken GetToken(string resourceId)
    {
      return UploadHandler.GetTransferForResource(resourceId);
    }



    /// <summary>
    /// Tries to get an exclusive file lock on the <see cref="TargetFile"/>.
    /// This causes an exception if the file is in use.
    /// </summary>
    protected void EnsureTargetFileIsUnlocked()
    {
      //getting an exclusive stream only works if the file was released
      using (var fs = TargetFile.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
      {
        fs.Close();
      }
    }



    protected List<BufferedDataBlock> CreateBufferedBlocks()
    {
      var list = new List<BufferedDataBlock>();

      long offset = 0;
      int blockCount = 0;
      byte[] buffer = new byte[Token.MaxBlockSize.Value];
      using (var fs = SourceFile.OpenRead())
      {
        while (true)
        {
          //long offset = fs.Position;
          var read = fs.Read(buffer, 0, buffer.Length);

          //we're done
          if (read == 0) break;

          BufferedDataBlock block = new BufferedDataBlock
          {
            TransferTokenId = Token.TransferId,
            BlockLength = read,
            BlockNumber = blockCount++,
            Offset = offset,
            Data = buffer.CreateCopy(read)
          };

          offset += read;

          list.Add(block);
        }
      }

      return list;
    }

  }
}