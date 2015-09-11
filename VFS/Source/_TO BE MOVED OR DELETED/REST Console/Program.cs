using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Vfs.Restful.Test;
using Vfs.Test;
using Vfs.Transfer;
using Vfs.Util;

namespace REST_Console
{
  class Program
  {
    static void Main(string[] args)
    {
      var context = new RestfulFacadeTestSuiteContext();

      try
      {
        context.Init();
        var fileSystem = context.FileSystem;

        var folder = fileSystem.CreateFolder("/F1");
        //fileSystem.DeleteFolder(folder.FullName);

        var folder2 = fileSystem.GetFolderInfo(folder.FullName);
        Console.Out.WriteLine("folder2.ToXmlDataContract() = {0}", folder2.ToXmlDataContract());

        fileSystem.DeleteFolder(folder.FullName);
        Console.Out.WriteLine("fileSystem = {0}", fileSystem.IsFolderAvailable(folder.FullName));
//
//        var maxBlockSize = fileSystem.DownloadTransfers.MaxBlockSize;
//        Console.Out.WriteLine("maxBlockSize = {0}", maxBlockSize);
//
//        Console.Out.WriteLine("Creating file...");
//
//       
////        context.FileSystem.CreateFolder("/Folder1");
//
//        MemoryStream ms = new MemoryStream(new byte[] {1,2,3,4,5});
//
//
//
//        StreamedDataBlock block = new StreamedDataBlock
//                                    {
//                                      TransferTokenId = "abc",
//                                      IsLastBlock = false,
//                                      BlockLength = 123,
//                                      BlockNumber = 99,
//                                      Offset = 200,
//                                      Data = ms
//                                    };
//        fileSystem.UploadTransfers.WriteBlockStreamed(block);

        Console.WriteLine("Written");
        Console.ReadLine();
        return;
      }
      finally
      {
        context.Cleanup();
      }
    }
  }
}
