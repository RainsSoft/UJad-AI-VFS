using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Hardcodet.Commons.IO;
using Vfs.LocalFileSystem.Transfer;
using Vfs.Transfer;

namespace Vfs.LocalFileSystem.Test.Transfers.Downloading
{
  /// <summary>
  /// Provides a source file for downloads.
  /// </summary>
  public class DownloadTestBase : DirectoryTestBase
  {
    public byte[] FileContents { get; set; }
    public FileInfo SourceFile { get; set; }

    DirectoryInfo ParentDirectory { get; set; }

    public VirtualFileInfo SourceFileInfo { get; set; }

    public  LocalDownloadHandler DownloadsHandler
    {
      get { return (LocalDownloadHandler) provider.DownloadTransfers; }
    }


    protected override void InitInternal()
    {
      base.InitInternal();

      FileContents = new byte[123456789];
      new Random(DateTime.Now.Millisecond).NextBytes(FileContents);

      ParentDirectory = rootDirectory.CreateSubdirectory("Dwnld_Parent");

      string path = FileUtil.CreateTempFilePath(ParentDirectory.FullName, "_dwlndsource", "bin");
      File.WriteAllBytes(path, FileContents);
      SourceFile = new FileInfo(path);

      SourceFileInfo = provider.GetFileInfo(path);
    }

    protected DownloadToken GetToken()
    {
      return DownloadsHandler.RequestDownloadToken(SourceFileInfo.FullName, false);
    }
  }
}
