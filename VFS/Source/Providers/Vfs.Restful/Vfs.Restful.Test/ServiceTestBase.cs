using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using OpenRasta.Hosting.HttpListener;
using Vfs.LocalFileSystem;
using Vfs.Restful.Client;
using Vfs.Restful.Server;
using Vfs.Test;
using Vfs.Transfer;

namespace Vfs.Restful.Test
{
  public class ServiceTestBase
  {
    public HttpListenerHost ServiceHost { get; set; }
    public VfsServiceSettings ServiceSettings { get; set; }
    public DirectoryInfo RootDirectory { get; set; }
    public IFileSystemProvider ServiceFileSystem { get; set; }
    public FileSystemFacade ClientFileSystem { get; set; }
    public string ServiceBaseUri { get; private set; }

    public VirtualFolder ClientRoot { get; set; }
    public VirtualFolder ServiceRoot { get; set; }

    public DownloadToken Token { get; set; }


    public delegate void TraverseAction(VirtualFolder clientFolder, VirtualFolder serviceFolder);


    [SetUp]
    public void Init()
    {
      ServiceFileSystem = GetServiceProvider();
      ServiceSettings = CustomizeSettings(new VfsServiceSettings());
      
      ServiceHost = new TestServiceHost {Configuration = new TestConfiguration(ServiceFileSystem, ServiceSettings)};

      ServiceBaseUri = "http://localhost:33456/";
      ServiceHost.Initialize(new[] { ServiceBaseUri }, "/", null);
      ServiceHost.StartListening();

//      //TODO remove debug code
//      ServiceBaseUri = "http://127.0.0.1:56789/";

      ClientFileSystem = new FileSystemFacade(ServiceBaseUri);

      //get root folders
      ClientRoot = VirtualFolder.CreateRootFolder(ClientFileSystem);
      ServiceRoot = VirtualFolder.CreateRootFolder(ServiceFileSystem);

      InitInternal();
    }

    protected virtual VfsServiceSettings CustomizeSettings(VfsServiceSettings settings)
    {
      return settings; 
    }

    protected virtual IFileSystemProvider GetServiceProvider()
    {
      RootDirectory = TestUtil.CreateTestDirectory();
      return new LocalFileSystemProvider(RootDirectory, true);
    }


    [TearDown]
    public void Cleanup()
    {
      CleanupInternal();

      ServiceHost.StopListening();

      RootDirectory.Refresh();
      if (RootDirectory.Exists) RootDirectory.Delete(true);
    }



    protected virtual void InitInternal()
    {
    }

    protected virtual void CleanupInternal()
    {

    }


    protected void Traverse(TraverseAction assertAction)
    {
      TraverseFolders(ClientRoot, ServiceRoot, assertAction);
    }


    private static void TraverseFolders(VirtualFolder clientFolder, VirtualFolder serviceFolder, TraverseAction assertAction)
    {
      assertAction(clientFolder, serviceFolder);

      //get child folders
      var cFolders = clientFolder.GetFolders().ToArray();
      var sFolders = serviceFolder.GetFolders().ToArray();
      Assert.AreEqual(sFolders.Count(), cFolders.Count());

      for (int i = 0; i < sFolders.Length; i++)
      {
        TraverseFolders(cFolders[i], sFolders[i], assertAction);
      }
    }
  }
}