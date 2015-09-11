using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

using NUnit.Framework;
using Vfs.Transfer;


namespace Vfs.LocalFileSystem.Test.Transfers.Uploading
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_UploadService_When_Creating_Transfer : UploadServiceTestBase
  {
    protected override void InitInternal()
    {
      base.InitInternal();
      InitToken();
    }

    [Test]
    public void Transfer_Should_Point_To_Target_File()
    {
      var transfer = UploadTransferStore.TryGetTransfer(Token.TransferId);
      Assert.AreEqual(TargetFilePath, transfer.File.FullName);
    }

    [Test]
    public void Transfer_Should_Contain_Parent_Folder_Item()
    {
      var transfer = UploadTransferStore.TryGetTransfer(Token.TransferId);
      Assert.AreEqual(ParentDirectory.FullName, transfer.ParentFolder.LocalDirectory.FullName);
    }

    [Test]
    public void Transfer_Should_Not_Have_Opened_File_Stream_Yet()
    {
      var transfer = UploadTransferStore.TryGetTransfer(Token.TransferId);
      Assert.IsNull(transfer.Stream);
    }


    [Test]
    public void Transfer_Should_Be_Denied_If_Download_Is_Already_Running()
    {
      //create file
      var blocks = CreateBufferedBlocks();
      blocks.Do(UploadHandler.WriteBlock);
      UploadHandler.CompleteTransfer(Token.TransferId);
      
      //request download
      var downloadToken = provider.DownloadTransfers.RequestDownloadToken(TargetFilePath, false);

      try
      {
        Token = UploadHandler.RequestUploadToken(TargetFilePath, true, 5000, "");
        Assert.Fail("Got upload token while download is running.");
      }
      catch (ResourceLockedException expected)
      {
      }
      finally
      {
        provider.DownloadTransfers.CancelTransfer(downloadToken.TransferId, AbortReason.ClientAbort);
      }

      //after downloading, it should work
      Token = UploadHandler.RequestUploadToken(TargetFilePath, true, 5000, "");
    }


    [Test]
    public void Transfer_Should_Be_Denied_If_Upload_Is_Already_Running()
    {
      try
      {
        Token = UploadHandler.RequestUploadToken(TargetFilePath, true, 5000, "");
        Assert.Fail("Got upload token while download is running.");
      }
      catch (ResourceLockedException expected)
      {
      }
      finally
      {
        UploadHandler.CancelTransfer(Token.TransferId, AbortReason.ClientAbort);
      }

      //after the canceling, it should work
      InitToken();
    }


    [Test]
    public void Should_Be_Able_To_Request_Token_For_File_In_Root_Folder()
    {
      DirectoryInfo downloadFolder = new DirectoryInfo(Path.GetTempPath());
      var f2 = downloadFolder.FullName.Substring(0, downloadFolder.FullName.Length-1);

      var result = Uri.Compare(new Uri(downloadFolder.FullName), new Uri(f2), UriComponents.AbsoluteUri, UriFormat.UriEscaped,
                  StringComparison.InvariantCultureIgnoreCase);

      Console.Out.WriteLine("result = {0}", result);
      return;

      
      var fs = new LocalFileSystemProvider(downloadFolder, true);
      fs.GetChildFiles("/");

      var token = fs.UploadTransfers.RequestUploadToken("/MYROOTFILE.TXT", false, 102400, "text/plain");
    }


  }
}
