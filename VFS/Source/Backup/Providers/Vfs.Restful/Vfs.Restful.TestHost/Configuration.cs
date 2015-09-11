using System.Collections.Generic;
using System.IO;
using OpenRasta.Configuration;
using OpenRasta.DI;
using OpenRasta.Hosting.AspNet;
using OpenRasta.OperationModel.Interceptors;
using OpenRasta.Web;
using RESTful_Filesystem.Handlers;
using Vfs;
using Vfs.LocalFileSystem;
using Vfs.Restful;
using Vfs.Restful.Server;
using Vfs.Restful.Server.Handlers;
using Vfs.Restful.Server.Resources;
using Vfs.Transfer;

namespace RESTful_Filesystem
{

  public class Configuration : IConfigurationSource
  {
    public void Configure()
    {
      var root = new DirectoryInfo(@"D:\Downloads\1-Time Garbage\_VFS-SERVICE-ROOT");
      var fsConfig = LocalFileSystemConfiguration.CreateForRootDirectory(root, true);
      fsConfig.RootName = "Downloads";
      var provider = new LocalFileSystemProvider(fsConfig);

      using (OpenRastaConfiguration.Manual)
      {
        var uris = VfsUris.Default;

//        ConfigurationHelper.RegisterExceptionInterceptor();
        ConfigurationHelper.RegisterFileSystemProvider(provider);
        ConfigurationHelper.RegisterDefaultHandlers();

        
//        //read file as stream
//        ResourceSpace.Has.ResourcesOfType<FileDataResource>()
//          .AtUri("/files/{filepath}/data")
//          .HandledBy<DownloadHandler>();
//
//        //get download token
//        ResourceSpace.Has.ResourcesOfType<DownloadToken>()
//          .AtUri("/files/{filepath}/download/token?includefilehash={includeFileHash}")
//          .And
//          .AtUri("/files/{filepath}/download/token?includefilehash={includeFileHash}&maxblocksize={maxBlockSize}")
//          .HandledBy<DownloadHandler>()
//          .AsXmlDataContract();



      }
    }


  }
}
