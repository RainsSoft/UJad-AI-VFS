using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Hardcodet.Commons.IO;
using NUnit.Framework;
using Vfs.Transfer;
using Vfs.Transfer.Util;
using Vfs.Util;

namespace Vfs.Providers.TestSuite.Transfer.Upload
{
  public class UploadTestBase : TestBase
  {
    /// <summary>
    /// The upload folder on the file system under test.
    /// </summary>
    public VirtualFolder UploadFolder
    {
      get { return Context.UploadFolder; }
    }

    /// <summary>
    /// Local source file for uploading.
    /// </summary>
    public FileInfo SourceFile
    {
      get { return Context.DownloadFile0Template; }
    }

    /// <summary>
    /// Gets the upload target path on the file system.
    /// </summary>
    public string TargetFilePath
    {
      get { return FileSystem.CreateFilePath(UploadFolder.MetaData.FullName, "target.txt"); }
    }


    public UploadToken Token { get; set; }



    protected override void InitInternal()
    {
    }



    protected override void CleanupInternal()
    {
      if (Token != null)
      {
        if (Uploads.GetTransferStatus(Token.TransferId) != TransferStatus.UnknownTransfer)
        {
          Uploads.CancelTransfer(Token.TransferId, AbortReason.Undefined);
        }
      }

      SystemTime.Reset();
      base.CleanupInternal();
    }


    /// <summary>
    /// Gets a download token for the <see cref="TargetFile"/> and
    /// assigns it to the <see cref="Token"/> property.
    /// </summary>
    /// <returns>The retrieved token.</returns>
    protected UploadToken InitToken()
    {
      Token = Uploads.RequestUploadToken(TargetFilePath, false, SourceFile.Length, "binary");
      return Token;
    }


    protected UploadToken GetToken(string resourceId)
    {
      return Uploads.GetTransferForResource(resourceId);
    }



    /// <summary>
    /// Creates a list of data blocks based on the source file, which
    /// can be uploaded to the target.
    /// </summary>
    protected List<BufferedDataBlock> CreateBufferedBlocks()
    {
      var list = new List<BufferedDataBlock>();


      long offset = 0;
      int blockCount = 0;
      byte[] buffer = new byte[Token.MaxBlockSize ?? 2048];
      
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

      var bytesCount = list.Select(b => b.BlockLength).Sum();
      Assert.AreEqual(bytesCount.Value, SourceFile.Length);

      return list;
    }



    protected void CompareUploadToSourceFile()
    {
      var target = FileSystem.GetFileInfo(TargetFilePath);

      var localCopyPath = FileUtil.CreateTempFilePath(Context.LocalTestRoot.FullName, "bin");
      FileSystem.SaveFile(target, localCopyPath);

      FileAssert.AreEqual(SourceFile.FullName, localCopyPath);
    }
  }
}
