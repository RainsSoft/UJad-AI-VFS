using System;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using ACorns.WCF.DynamicClientProxy;
using Hardcodet.Commons.IO;
using Vfs;
using Vfs.FileSystemService;
using Vfs.FileSystemService.Faults;
using Vfs.Util;

namespace ConsoleApplication1
{
  class Program
  {
    static void Main(string[] args)
    {

      var proxy = WCFClientProxy<IFSOperationService>.GetReusableInstance("operationService");
      bool available = proxy.IsFileAvailable("/root/hello.txt");

      ReaderClient client = new ReaderClient();
      client.WriteRootFolders();

      Console.ReadLine();
    }



    private static void Upload()
    {
      Console.Out.WriteLine("Press key to start upload");
      Console.ReadLine();

      string sourceFilePath = @"D:\Downloads\1-Time Garbage\_VFS-SERVICE-ROOT\archive.zip";
      var sourceFile = new FileInfo(sourceFilePath);

      string uri = "http://localhost:8088/webfs/webupload/writefile?file=copy2.iso&overwrite=true&length={0}&contenttype={1}";
      uri = String.Format(uri, sourceFile.Length, ContentUtil.ResolveContentType(".zip"));
      HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(uri);
      req.Method = "POST";
      req.KeepAlive = true;
      req.ContentType = ContentUtil.ResolveContentType(".zip"); // "application/octet-stream";

      //disable buffering in order to prevent loading everything to memory first
      req.AllowWriteStreamBuffering = false;
      req.SendChunked = false;
      
      req.ContentLength = sourceFile.Length;
      Stream reqStream = req.GetRequestStream();

      using (var sourceStream = File.OpenRead(sourceFilePath))
      {
        //use default byte sizes
        byte[] buffer = new byte[32768];

        int block = 0;
        while (true)
        {
          int bytesRead = sourceStream.Read(buffer, 0, buffer.Length);
          if (bytesRead > 0)
          {
            reqStream.Write(buffer, 0, bytesRead);
            if (block++ % 500 == 0)
            {
              Console.Out.WriteLine("Wrote block {0} with size {1}", block, bytesRead);
            }
          }
          else
          {
            break;
          }
        }
      }

      reqStream.Flush();
      reqStream.Close();
//      Console.Out.WriteLine("DONE");
//      return;

      HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
      Console.WriteLine("HTTP/{0} {1} {2}", resp.ProtocolVersion, (int)resp.StatusCode, resp.StatusDescription);
      Console.ReadLine();
    }


    private static void SomeStuff()
    {


      //      FSOperationServiceClient client = new FSOperationServiceClient();
      //      FSWriterServiceClient writer = new FSWriterServiceClient();
      //      VirtualFolderInfo root = client.GetFileSystemRoot();

      //      var childFiles = client.GetChildFiles2(root);
      //      FSReaderServiceClient reader = new FSReaderServiceClient();
      //      foreach (var child in childFiles)
      //      {
      //        Console.WriteLine("press key to read file");
      //        Console.ReadLine();
      //
      //        Console.Out.WriteLine("file = {0}, {1}", child.FullName, child.ContentType);
      //        using (Stream s = reader.ReadFileContents2(child))
      //        {
      //          string dir = @"D:\Downloads\1-Time Garbage\bin";
      //          string path = FileUtil.CreateTempFilePath(dir, Path.GetFileNameWithoutExtension(child.Name),
      //                                      Path.GetExtension(child.Name));
      //          s.WriteTo(path);
      //        }
      //      }

      try
      {
//        string path = @"D:\Downloads\1-Time Garbage\bin\NotifyIconWin7.png";
//        using (Stream s = File.OpenRead(path))
//        {
//          writer.WriteFile("/foobar.png", true, s);
//        }

        string path = "";
        return;

        ChannelFactory<IFSOperationService> opclient =
          new ChannelFactory<IFSOperationService>("BasicHttpBinding_IFSOperationService");
        using(opclient)
        {
          opclient.Open();
          var service = opclient.CreateChannel();
          var root = service.GetFileSystemRoot();
          Console.Out.WriteLine("root.FullName = {0}", root.FullName);

          service.GetChildFiles(root.FullName, "no*").Do(f => Console.WriteLine("File: " + f.FullName + ", " + f.ContentType));
          path = service.GetChildFiles(root.FullName).Last().FullName;
        }

        ChannelFactory<IFSDataDownloadService> reader = new ChannelFactory<IFSDataDownloadService>("BasicHttpBinding_IFSReaderService");
        using(reader)
        {
          reader.Open();
          var rs = reader.CreateChannel();
          var stream = rs.ReadFile(path);
          stream.WriteTo(@"D:\Downloads\1-Time Garbage\bin\xxx.iso");
        }

        Console.Out.WriteLine("File uploaded");
        return;
      }
      catch (FaultException<ResourceFault> e)
      {
        Console.Out.WriteLine(e);
        Console.ReadLine();
        return;
      }
    }
  }

  
}
