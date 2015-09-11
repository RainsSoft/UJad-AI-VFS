using System;
using System.IO;
using Vfs;
using Vfs.FileSystemServiceClient;
using Vfs.Transfer;
using Vfs.Transfer.Util;

namespace ConsoleApplication1
{
  public class ReaderClient
  {
    public FileSystemFacade Facade { get; private set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public ReaderClient()
    {
      Facade = new FileSystemFacade("operationService", "readerService", "writerService");
    }


    public void WriteRootFolders()
    {
      VirtualFolder root = VirtualFolder.CreateRootFolder(Facade);

      foreach (var fileInfo in root.GetFiles("*.txt"))
      {
        var token = Facade.DownloadTransfers.RequestDownloadToken(fileInfo.MetaData.FullName, false);

        Func<long, StreamedDataBlock> func = n =>
                                               {
                                                 var db = Facade.DownloadTransfers.ReadBlockStreamed(token.TransferId, n);
                                                 return db;
                                               };

        using(var s = new StreamedBlockInputStream(func, token.ResourceLength))
        {
          StreamReader reader = new StreamReader(s);
          var text = reader.ReadToEnd();
          Console.Out.WriteLine("Read text:\n" + text);
          Console.Out.WriteLine("");
        }


//        using(var stream = Facade.DownloadTransfers.ReadFile(fileInfo.MetaData.FullName))
//        {
//          StreamReader reader = new StreamReader(stream);
//          var text = reader.ReadToEnd();
//
//        }
      }  
    }
  }
}
