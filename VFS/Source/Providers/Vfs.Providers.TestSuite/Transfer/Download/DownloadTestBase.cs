using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Hardcodet.Commons.IO;
using Vfs.Transfer;
using Vfs.Util;

namespace Vfs.Providers.TestSuite.Transfer.Download
{
  /// <summary>
  /// Base class for download transfer tests.
  /// </summary>
  public class DownloadTestBase : TestBase
  {
    public DownloadToken Token { get; set; }

    public VirtualFile SourceFile { get; set; }

    public VirtualFileInfo SourceFileInfo
    {
      get { return SourceFile.MetaData; }
    }
    
    /// <summary>
    /// Provides a path in the local test directory for a downloaded file.
    /// Default name is <c>target.txt</c>.
    /// </summary>
    public string TargetFilePath { get; set; }


    protected override void InitInternal()
    {
      base.InitInternal();
      SourceFile = Context.DownloadFolder.GetFiles().First();
      TargetFilePath = FileUtil.CreateTempFilePath(Context.LocalTestRoot.FullName, "target", "txt");
    }



    /// <summary>
    /// Cancels the current download, if the <see cref="Token"/>
    /// is set, and resets the <see cref="SystemTime"/>.
    /// </summary>
    protected override void CleanupInternal()
    {
      if (Token != null)
      {
        if (Downloads.GetTransferStatus(Token.TransferId) != TransferStatus.UnknownTransfer)
        {
          Downloads.CancelTransfer(Token.TransferId, AbortReason.Undefined);
        }
      }

      SystemTime.Reset();
      base.CleanupInternal();
    }


    /// <summary>
    /// Gets a download token for the <see cref="SourceFile"/> and
    /// assigns it to the <see cref="Token"/> property.
    /// </summary>
    /// <returns>The retrieved token.</returns>
    protected DownloadToken InitToken()
    {
      Token = Downloads.RequestDownloadToken(SourceFileInfo.FullName, false);
      return Token;
    }


    /// <summary>
    /// Gets a single download token for a given file.
    /// </summary>
    protected DownloadToken GetToken(string filePath)
    {
      return Downloads.GetTransfersForResource(filePath).Single();
    }
  }
}
