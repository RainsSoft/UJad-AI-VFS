using System;
using Autofac;
using Autofac.Configuration;
using NUnit.Framework;
using Vfs.Locking;
using Vfs.Test;
using Vfs.Transfer;

namespace Vfs.Providers.TestSuite
{
  /// <summary>
  /// Base class for tests that operate on a test directory
  /// which consists files and folders.
  /// </summary>
  public abstract class TestBase
  {
    protected IContainer container;

    /// <summary>
    /// The test context, which is resolved during initialization.
    /// </summary>
    public TestContext Context { get; set; }

    public IFileSystemProvider FileSystem
    {
      get { return Context.FileSystem; }
    }

    public IUploadTransferHandler Uploads
    {
      get { return FileSystem.UploadTransfers; }
    }

    public IDownloadTransferHandler Downloads
    {
      get { return FileSystem.DownloadTransfers; }
    }

    public IResourceLockRepository LockRepository
    {
      get { return FileSystem.LockRepository; }
    }

    /// <summary>
    /// Convenience property, which returns the virtual root folder
    /// of the tested file system.
    /// </summary>
    public VirtualFolder FileSystemRoot
    {
      get { return Context.FileSystemRoot; }
    }



    [SetUp]
    public void Init()
    {
      //resolve the context
      InitContainer();

      Context = container.Resolve<TestContext>();

      //create test hierarchy
      Context.Init();
      Context.InitFileSystemUnderTest(false);
      
      //create template method
      InitInternal();
    }


    protected virtual void InitContainer()
    {
      var builder = new ContainerBuilder();
      builder.RegisterModule(new ConfigurationSettingsReader("testcomponents"));
      container = builder.Build();
    }


    protected virtual void InitInternal()
    {
    }


    [TearDown]
    public void Cleanup()
    {
      CleanupInternal();
      Context.Cleanup();
    }

    protected virtual void CleanupInternal()
    {
    }




    /// <summary>
    /// Recursively browses the file system and performs tests on either
    /// files, folders, or both. The submitted parent folder is not tested.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="fileTest"></param>
    /// <param name="folderTest"></param>
    protected static void RecurseFileSystem(VirtualFolder parent, Action<VirtualFolder, VirtualFile> fileTest, Action<VirtualFolder, VirtualFolder> folderTest)
    {
      var contents = parent.GetFolderContents();

      if (fileTest != null)
      {
        contents.Files.Do(vf => fileTest(parent, vf));
      }

      if (folderTest != null)
      {
        contents.Folders.Do(vf => folderTest(parent, vf));
      }

      contents.Folders.Do(vf => RecurseFileSystem(vf, fileTest, folderTest));
    }





    /// <summary>
    /// Gets the MD5 file hash of a given file on the file system.
    /// </summary>
    protected string GetFileHash(VirtualFileInfo file)
    {
      var token = FileSystem.DownloadTransfers.RequestDownloadToken(file.FullName, true);
      FileSystem.DownloadTransfers.CancelTransfer(token.TransferId, AbortReason.ClientAbort);
      return token.Md5FileHash;
    }
  }
}