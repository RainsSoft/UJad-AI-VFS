using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Hardcodet.Commons.IO;
using Vfs.Util;
using NUnit.Framework;
using Vfs.Test;
using Guard=Hardcodet.Commons.Guard;


namespace Vfs.LocalFileSystem.Test.Resource_Tests
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_Local_File_When_Accessing_File_Contents
  {
    private LocalFileSystemProvider provider;
    private VirtualFileInfo sourceFile;
    private FileInfo targetFile;

    [SetUp]
    public void Init()
    {
      provider = new LocalFileSystemProvider();
      FileInfo fi = new FileInfo(FileUtil.CreateTempFilePath("txt"));
      var writer = fi.CreateText();
      writer.WriteLine("hello world.");
      writer.Close();

      sourceFile = fi.CreateFileResourceInfo();
      Assert.IsTrue(fi.Exists);

      targetFile = new FileInfo(FileUtil.CreateTempFilePath("txt"));
    }

    [TearDown]
    public void Cleanup()
    {
      File.Delete(sourceFile.FullName);
      targetFile.Delete();
    }





    [Test]
    public void Reading_File_Should_Create_Exact_Copy()
    {
      using (FileStream outputStream = targetFile.OpenWrite())
      {
        provider.ReadFile(sourceFile, outputStream, true);
      }

      string fp1 = FileUtil.ComputeFingerPrint(sourceFile.FullName);
      string fp2 = targetFile.ComputeFingerPrint();
      Assert.AreEqual(fp1, fp2);

      Assert.AreEqual(sourceFile.Length, targetFile.Length);
      Assert.AreEqual(sourceFile.Length, targetFile.CreateFileResourceInfo().Length);
    }


    /// <summary>
    /// Make sure we don't run into issues with the default buffer size...
    /// </summary>
    [Test]
    public void Reading_Empty_File_Should_Work()
    {
      FileInfo fi = new FileInfo(sourceFile.FullName);
      fi.Delete();
      fi.Create().Close();
      Assert.AreEqual(0, fi.Length);

      using (FileStream outputStream = targetFile.OpenWrite())
      {
        provider.ReadFile(sourceFile, outputStream, true);
      }

      string fp1 = FileUtil.ComputeFingerPrint(sourceFile.FullName);
      string fp2 = FileUtil.ComputeFingerPrint(targetFile);
      Assert.AreEqual(fp1, fp2);
      Assert.AreEqual(0, targetFile.Length);
    }


    
    [Test]
    public void Asynchronously_Reading_Big_File_Should_Create_Proper_Copy()
    {
      //create a big file
      FileInfo fi = new FileInfo(sourceFile.FullName);
      fi.Delete();

      //create a 500MB file
      Console.Out.WriteLine("CREATING TEMP FILE - THIS WILL TAKE A WHILE...");
      File.WriteAllBytes(fi.FullName, new byte[1024 * 1024 * 500]);
      Console.Out.WriteLine("fi.Length = {0}", fi.Length / 1024 / 1024);

      FileOperationResult result = null;
      using (FileStream stream = targetFile.OpenWrite())
      {
        Console.Out.WriteLine("starting...");
        provider.BeginReadFile(sourceFile, stream, r => result = r);
        Console.Out.WriteLine("returning...");

        while (result == null)
        {
          Console.Out.WriteLine("waiting...");
          Thread.CurrentThread.Join(1000);
        }
      }

      Assert.IsTrue(result.IsSuccess);
      Assert.IsNull(result.Exception);
      Assert.AreSame(sourceFile, result.FileInfo);
      Console.Out.WriteLine("targetFile.Length = {0}", targetFile.Length);

      Assert.AreEqual(fi.Length, targetFile.Length);
      string fp1 = FileUtil.ComputeFingerPrint(sourceFile.FullName);
      string fp2 = FileUtil.ComputeFingerPrint(targetFile);
      Assert.AreEqual(fp1, fp2);

    }



    [Test]
    public void Saving_File_Asynchronously_With_Path_Of_Existing_File_Should_Return_Results()
    {
      //create temp file
      Assert.IsFalse(targetFile.Exists);
      File.WriteAllBytes(targetFile.FullName, new byte[1024 * 1024 * 20]);

      //run async operation with invalid file path
      FileOperationResult result = null;
      var invalidSource = new VirtualFileInfo {FullName = FileUtil.GetUniqueFileName("VFS_INVALID")};
      provider.BeginSaveFile(invalidSource, targetFile.FullName, c => result = c);
      while (result == null) Thread.CurrentThread.Join(500);

      Assert.IsFalse(result.IsSuccess);
      Assert.IsInstanceOf<VfsException>(result.Exception);
      

      //run async operation with valid data
      result = null;

      provider.BeginSaveFile(sourceFile, targetFile.FullName, c => result = c);

      while (result == null)
      {
        Console.Out.WriteLine("waiting...");
        Thread.CurrentThread.Join(1000);
      }

      Assert.IsTrue(result.IsSuccess, result.IsSuccess ? "" : result.Exception.ToString());
      Assert.AreEqual(FileUtil.ComputeFingerPrint(sourceFile.FullName), targetFile.ComputeFingerPrint());
    }


    [Test]
    [ExpectedException(typeof(ResourceAccessException))]
    public void Requesting_A_File_Outside_The_Allowed_Scope_Should_Fail()
    {
      var dir = TestUtil.CreateTestDirectory();
      using(new Guard(() => dir.Delete(true)))
      {
        provider = new LocalFileSystemProvider(dir, false);
        provider.ReadFileContents(sourceFile);
      }
    }


    [Test]
    [ExpectedException(typeof(ResourceAccessException))]
    public void Submitting_Root_Path_Should_Fail()
    {
      var dir = TestUtil.CreateTestDirectory();
      using (new Guard(() => dir.Delete(true)))
      {
        provider = new LocalFileSystemProvider(dir, false);
        provider.ReadFileContents(sourceFile);
      }

      VirtualFileInfo file = new VirtualFileInfo();
      file.FullName = provider.GetFileSystemRoot().FullName;
      provider.ReadFileContents(file);
    }


    [Test]
    [ExpectedException(typeof(InvalidResourcePathException))]
    public void Submitting_Invalid_File_Uri_Should_Fail()
    {
      VirtualFileInfo file = new VirtualFileInfo();
      file.FullName = "a?:@*%&/";

      provider.GetFileInfo(file.FullName);
      provider.ReadFileContents(file);
    }
  }
}
