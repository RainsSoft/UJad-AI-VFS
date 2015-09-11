using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Vfs.Transfer;
using Vfs.Transfer.Util;
using Vfs.Util;

namespace Vfs.Restful.Test.Download
{
  [TestFixture]
  public class Given_File_When_Reading_In_One_Stream : DownloadTestBase
  {
    [Test]
    public void INPUT()
    {
      using (var stream = ClientFileSystem.ReadFileContents(SourceFileInfo))
      {
        MemoryStream s = new MemoryStream();
        stream.WriteTo(s);
        Assert.AreEqual(SourceFile.Length, s.Length);
      }
    }


    [Test]
    public void Reading_Stream_From_Token_Should_Automatically_Close_Transfer_Once_File_Was_Streamed()
    {
      var token = ClientDownloads.RequestDownloadToken(SourceFileInfo.FullName, false);
      using (var stream = ClientDownloads.DownloadFile(token.TransferId))
      {
        stream.WriteTo(Stream.Null);
      }

      var status = ClientDownloads.GetTransferStatus(token.TransferId);
      Assert.AreEqual(TransferStatus.Completed, status);
    }


    [Test]
    public void INPUTx()
    {
      var token = ClientDownloads.RequestDownloadToken(SourceFileInfo.FullName, false);
      using (var stream = ClientDownloads.DownloadFile(token.TransferId))
      {
        MemoryStream s = new MemoryStream();
        stream.WriteTo(s);
        Assert.AreEqual(SourceFile.Length, s.Length);
      }
    }


    [Test]
    public void INPUT3()
    {
      var token = ClientDownloads.RequestDownloadToken(SourceFileInfo.FullName, false);

      for (int i = 0; i < token.TotalBlockCount; i++)
      {
        var block = ClientDownloads.ReadBlock(token.TransferId, i);
        Console.Out.WriteLine("Offset: " + block.Offset);

        if (i==5) break;
      }

      ServiceFileSystem.DownloadTransfers.CancelTransfer(token.TransferId, AbortReason.ClientAbort);
    }

  
  }

}