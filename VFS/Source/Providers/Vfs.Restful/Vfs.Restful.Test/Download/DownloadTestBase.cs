using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Vfs.Transfer;

namespace Vfs.Restful.Test.Download
{
  public class DownloadTestBase : ServiceTestBase
  {
    /// <summary>
    /// Convenience proeprty which returns the
    /// <see cref="IFileSystemProvider.DownloadTransfers"/>
    /// of the tested <see cref="ServiceTestBase.ClientFileSystem"/>.
    /// </summary>
    public IDownloadTransferHandler ClientDownloads
    {
      get { return ClientFileSystem.DownloadTransfers; }
    }

      /// <summary>
    /// The file that can be downloaded
    /// </summary>
    public FileInfo SourceFile { get; set; }

    public VirtualFileInfo SourceFileInfo { get; set; }

    public DirectoryInfo DownloadFolder { get; set; }




    protected override void InitInternal()
    {
      DownloadFolder = RootDirectory.CreateSubdirectory("Download");
      
      byte[] data = new byte[1024*1024*20];
      Random rnd = new Random();
      rnd.NextBytes(data);
      
      SourceFile = new FileInfo(Path.Combine(DownloadFolder.FullName, "source.bin"));
      File.WriteAllBytes(SourceFile.FullName, data);
      SourceFile.Refresh();

      SourceFileInfo = ServiceFileSystem.GetFileInfo("/Download/source.bin");
      base.InitInternal();
    }
  }
}
