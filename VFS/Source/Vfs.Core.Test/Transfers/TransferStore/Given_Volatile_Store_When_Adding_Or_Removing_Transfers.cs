using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

using NUnit.Framework;
using Vfs.LocalFileSystem;
using Vfs.Transfer;


namespace Vfs.Test.Transfers.TransferStore
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_Volatile_Store_When_Adding_Or_Removing_Transfers
  {
    private InMemoryTransferStore<DownloadTransfer<FileItem>> store;
    private List<DownloadTransfer<FileItem>> transfers;
 
    [SetUp]
    public void Init()
    {
      store = new InMemoryTransferStore<DownloadTransfer<FileItem>>();

      transfers = new List<DownloadTransfer<FileItem>>();
      for (int i = 0; i < 10; i++)
      {
        FileItem item = new FileItem {LocalFile = new FileInfo(i.ToString())};        
        var token = new DownloadToken {TransferId = i.ToString()};
        transfers.Add(new DownloadTransfer<FileItem>(token, item));
      }
    }


    [Test]
    public void Adding_Transfers_Should_Store_Them()
    {
      foreach (var transfer in transfers)
      {
        store.AddTransfer(transfer);
        Assert.AreSame(transfer, store.TryGetTransfer(transfer.TransferId));
      }

      foreach (var transfer in transfers)
      {
        Assert.AreSame(transfer, store.TryGetTransfer(transfer.TransferId));
      }
    }


    [Test]
    public void Marking_Transfers_Inactive_Should_Remove_Transfers()
    {
      foreach (var transfer in transfers)
      {
        store.AddTransfer(transfer);
      }

      foreach (var transfer in transfers)
      {
        store.SetInactive(transfer);
        Assert.IsNull(store.TryGetTransfer(transfer.TransferId));
      }
    }


    [Test]
    public void Updating_Transfer_Should_Replace_Transfer_Instance()
    {
      foreach (var transfer in transfers)
      {
        store.AddTransfer(transfer);
      }

      var original = transfers[5];
      DownloadTransfer<FileItem> copy = new DownloadTransfer<FileItem>(original.Token, original.FileItem);
      store.UpdateTransferState(copy);

      Assert.AreNotSame(original, store.TryGetTransfer(original.TransferId));
      Assert.AreSame(copy, store.TryGetTransfer(original.TransferId));
    }

    
  }
}
