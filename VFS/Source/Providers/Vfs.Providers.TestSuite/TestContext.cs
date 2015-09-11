using System;
using System.IO;
using System.Linq;
using Hardcodet.Commons.IO;
using Vfs.Test;

namespace Vfs.Providers.TestSuite
{
  /// <summary>
  /// The context of the test suite, which defines constants for test
  /// folders / files, and provides a local repository with folders and
  /// files to be used for up/downloads.
  /// </summary>
  public class TestContext
  {
    //the testing root folder. Will be created beneath the root.
    public const string RootDirectoryName = "TestSuite.Root";

    public const string ChildFolder0Name = "Folder0";
    public const string ChildFolder1Name = "Folder1";

    public const string DownloadFolderName = "Download";
    public const string UploadFolderName = "Upload";
    public const string EmptyFolderName = "Empty";

    public const string DownloadFile0Name = "File0";
    public const string DownloadFile1Name = "File1";


    /// <summary>
    /// Creates the file system during running tests.
    /// </summary>
    protected IFileSystemTestFactory FileSystemFactory { get; set; }


    /// <summary>
    /// The file system under test.
    /// </summary>
    public IFileSystemProvider FileSystem { get; set; }

    /// <summary>
    /// Temporary local test folder - used to store downloaded
    /// files and provide a location to upload from.
    /// </summary>
    public DirectoryInfo LocalTestRoot { get; set; }

    /// <summary>
    /// The first file (<see cref="DownloadFile0Name"/> that is
    /// uploaded to the test folder.
    /// </summary>
    public FileInfo DownloadFile0Template { get; set; }

    /// <summary>
    /// The second file (<see cref="DownloadFile1Name"/>) that is
    /// uploaded to the test folder.
    /// </summary>
    public FileInfo DownloadFile1Template { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public VirtualFolder FileSystemRoot
    {
      get { return VirtualFolder.CreateRootFolder(FileSystem); }
    }

    public VirtualFolder DownloadFolder
    {
      get { return FileSystemRoot.GetFolders(ChildFolder0Name).Single()
                                 .GetFolders(DownloadFolderName).Single(); }
    }

    public VirtualFolder UploadFolder
    {
      get { return FileSystemRoot.GetFolders(ChildFolder0Name).Single()
                                 .GetFolders(UploadFolderName).Single(); }
    }

    public VirtualFolder EmptyFolder
    {
      get { return DownloadFolder.GetFolders(EmptyFolderName).Single(); }
    }

    /// <summary>
    /// Inits the test context with a random local test directory.
    /// </summary>
    public TestContext(IFileSystemTestFactory fileSystemFactory)
    {
      FileSystemFactory = fileSystemFactory;
      FileSystem = fileSystemFactory.GetFileSystem();
    }


    /// <summary>
    /// Creates the local test folder, and uploads
    /// test files to the file system.
    /// </summary>
    public virtual void Init()
    {
      CreateLocalTestDirectory();
      InitFileSystemUnderTest(false);
    }


    /// <summary>
    /// Creates the context and uses a given <see cref="DirectoryInfo"/>
    /// which is assigned to the <see cref="LocalTestRoot"/> property.
    /// Keep in mind that the directory's content might be deleted
    /// from time to time.
    /// </summary>
    /// <param name="localTestDirectory"></param>
    public TestContext(DirectoryInfo localTestDirectory)
    {
      LocalTestRoot = localTestDirectory;
    }




    /// <summary>
    /// Deletes the locally created test folder.
    /// </summary>
    public virtual void DeleteLocalTestDirectory()
    {
      if (LocalTestRoot.Exists)
      {
        LocalTestRoot.Delete(true);
      }

      if(DownloadFile0Template != null) DownloadFile0Template.Refresh();
      if(DownloadFile1Template != null) DownloadFile1Template.Refresh();
    }


    /// <summary>
    /// Deletes and recreates the local test directory.
    /// </summary>
    public virtual void ClearTestDirectoryContents()
    {
      DeleteLocalTestDirectory();
      CreateLocalTestDirectory();
    }
    

    /// <summary>
    /// Resets the <see cref="FileSystem"/>.
    /// </summary>
    public virtual void ResetFileSystemProvider()
    {
      FileSystem = FileSystemFactory.GetFileSystem();
    }


    /// <summary>
    /// Creates the test directory.
    /// </summary>
    /// <param name="root"></param>
    /// <param name="recreateLocalFiles">Whether to recreate the local test
    /// files that are used for up/downloads before uploding them to the
    /// virtual FS.</param>
    public virtual void InitFileSystemUnderTest(bool recreateLocalFiles)
    {
      VirtualFolder root = VirtualFolder.CreateRootFolder(FileSystem);

      //create two top level folders
      var f0 = root.AddFolder(ChildFolder0Name);
      root.AddFolder(ChildFolder1Name);

      //create upload/download folders
      f0.AddFolder(UploadFolderName);
      var download = f0.AddFolder(DownloadFolderName);

      if(recreateLocalFiles)
      {
        //overwrite local files
        CreateLocalTestFiles();
      }

      //upload files and and empty folder to download folder
      download.AddFile(DownloadFile0Template.FullName, DownloadFile0Name, false);
      download.AddFile(DownloadFile1Template.FullName, DownloadFile1Name, false);
      download.AddFolder(EmptyFolderName);
    }


    /// <summary>
    /// Creates the local test directory in the temp folder.
    /// </summary>
    protected virtual void CreateLocalTestDirectory()
    {
      //create root folder
      LocalTestRoot = FileUtil.CreateTempFolder(RootDirectoryName + ".Local");

      //create file infos
      DownloadFile0Template = new FileInfo(Path.Combine(LocalTestRoot.FullName, DownloadFile0Name));
      DownloadFile1Template = new FileInfo(Path.Combine(LocalTestRoot.FullName, DownloadFile1Name));

      CreateLocalTestFiles();
    }



    /// <summary>
    /// Writes the two test files to the <see cref="LocalTestRoot"/>
    /// directory.
    /// </summary>
    private void CreateLocalTestFiles()
    {
      Random rnd = new Random();

      //file0 - 2MB
      var data0 = new byte[1024*1024*2];
      rnd.NextBytes(data0);
      File.WriteAllBytes(DownloadFile0Template.FullName, data0);
      DownloadFile0Template.Refresh();

      //file1 - 4MB
      var data1 = new byte[1024 * 1024 * 4];
      rnd.NextBytes(data1);
      File.WriteAllBytes(DownloadFile1Template.FullName, data1);
      DownloadFile1Template.Refresh();
    }


    /// <summary>
    /// Deletes the local test directory and invokes a cleanup
    /// for the file system factory.
    /// </summary>
    public void Cleanup()
    {
      FileSystemFactory.CleanupFileSystem(FileSystem);
      DeleteLocalTestDirectory();
    }
  }
}
