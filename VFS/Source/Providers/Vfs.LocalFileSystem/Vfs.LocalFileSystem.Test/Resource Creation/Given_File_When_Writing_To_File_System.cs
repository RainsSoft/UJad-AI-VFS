using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Hardcodet.Commons;
using Hardcodet.Commons.IO;
using Vfs.Util;
using NUnit.Framework;


namespace Vfs.LocalFileSystem.Test.Resource_Creation
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_File_When_Writing_To_File_System : DirectoryTestBase
  {
    private FileInfo sourceFile;
    private FileInfo targetFile;



    protected override void InitInternal()
    {
      sourceFile = new FileInfo(Path.Combine(rootDirectory.FullName, "source.txt"));
      targetFile = new FileInfo(Path.Combine(rootDirectory.FullName, "target.txt"));

      //create a sample file
      File.WriteAllBytes(sourceFile.FullName, new byte[1024 * 1024 * 5]);
    }



    [Test]
    public void Writing_File_Should_Create_Copy_On_File_System()
    {
      Assert.IsFalse(File.Exists(targetFile.FullName));
      provider.WriteFile(sourceFile.FullName, targetFile.FullName, false);

      targetFile.Refresh();
      Assert.IsTrue(targetFile.Exists);
      Assert.AreEqual(sourceFile.Length, targetFile.Length);
      Assert.AreEqual(sourceFile.ComputeFingerPrint(), targetFile.ComputeFingerPrint());
    }



    [Test]
    public void Submitting_Parent_Folder_And_File_Name_Should_Resolve_Correct_Path()
    {
      Assert.IsFalse(File.Exists(targetFile.FullName));
      
      using (Stream stream = sourceFile.OpenRead())
      {
        provider.WriteFile(root, targetFile.Name, stream, false, sourceFile.Length,
                                   ContentUtil.UnknownContentType);
      }

      targetFile.Refresh();
      Assert.IsTrue(targetFile.Exists);
      Assert.AreEqual(sourceFile.Length, targetFile.Length);
      Assert.AreEqual(sourceFile.ComputeFingerPrint(), targetFile.ComputeFingerPrint());
    }



    [Test]
    public void Created_File_Should_Not_Be_Locked_After_Save()
    {
      using (Stream stream = sourceFile.OpenRead())
      {
        provider.WriteFile(root, targetFile.Name, stream, false, sourceFile.Length, ContentUtil.UnknownContentType);
      }

      using (var readStream = targetFile.OpenRead())
      {
        var b = readStream.ReadByte();
      }

      targetFile.Delete();
      targetFile.Refresh();
      Assert.IsFalse(targetFile.Exists);
    }


    [Test]
    public void Returned_File_Meta_Info_Should_Match_New_File_Data()
    {
      VirtualFileInfo target;
      using (Stream stream = sourceFile.OpenRead())
      {
        target = provider.WriteFile(root, targetFile.Name, stream, false, sourceFile.Length, ContentUtil.UnknownContentType);
      }

      targetFile.Refresh();

      Assert.AreEqual(targetFile.Length, target.Length);
      Assert.AreEqual(targetFile.Name, target.Name);
      Assert.AreEqual((DateTimeOffset)targetFile.CreationTime, target.CreationTime);
      Assert.AreEqual((DateTimeOffset)targetFile.LastAccessTime, target.LastAccessTime);
      Assert.AreEqual((DateTimeOffset)targetFile.LastWriteTime, target.LastWriteTime);

      Assert.IsFalse(target.IsReadOnly);
      Assert.IsFalse(target.IsHidden);
    }



    [Test]
    [ExpectedException(typeof(ResourceAccessException))]
    public void Trying_To_Save_File_Outside_The_File_System_Scope_Should_Not_Work()
    {
      Assert.IsFalse(File.Exists(targetFile.FullName));
      using (Stream stream = sourceFile.OpenRead())
      {
        provider.WriteFile(root, "../../../file.txt", stream, false, sourceFile.Length, ContentUtil.UnknownContentType);
      }

    }


    [Test]
    [ExpectedException(typeof(ResourceAccessException))]
    public void Writing_To_A_Path_That_Matches_A_Folder_Should_Not_Work()
    {
      var folderName = rootDirectory.GetDirectories()[0].Name;
      using (Stream stream = sourceFile.OpenRead())
      {
        provider.WriteFile(root, folderName, stream, true, sourceFile.Length, ContentUtil.UnknownContentType);
      }
    }


    [Test]
    public void Overwriting_Should_Be_Prevented_If_Not_Specified()
    {
      File.WriteAllBytes(targetFile.FullName, new byte[99999]);
      targetFile.Refresh();
      Assert.IsTrue(targetFile.Exists);

      using (Stream stream = sourceFile.OpenRead())
      {
        try
        {
          provider.WriteFile(targetFile.FullName, stream, false, sourceFile.Length, ContentUtil.UnknownContentType);
          Assert.Fail("Could overwrite file.");
        }
        catch (ResourceOverwriteException e)
        {
          StringAssert.Contains(targetFile.Name, e.Message);
        }
      }

      //make sure the original is preserved
      targetFile.Refresh();
      Assert.AreEqual(99999, targetFile.Length);
    }


    [Test]
    public void Overwriting_A_File_Should_Work_If_Specified()
    {
      File.WriteAllBytes(targetFile.FullName, new byte[32768]);
      targetFile.Refresh();
      Assert.IsTrue(targetFile.Exists);
      Assert.AreNotEqual(sourceFile.Length, targetFile.Length);

      using (Stream stream = sourceFile.OpenRead())
      {
        provider.WriteFile(targetFile.FullName, stream, true, sourceFile.Length, ContentUtil.UnknownContentType);
      }

      targetFile.Refresh();
      Assert.AreEqual(sourceFile.Length, targetFile.Length);
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    [ExpectedException(typeof(VirtualResourceNotFoundException))]
    public void Creating_File_In_A_Folder_That_Does_Not_Exist_Should_Fail()
    {
      string folder = Path.Combine(rootDirectory.FullName, "unknownSubDir");
      string file = Path.Combine(folder, "file.txt");

      using (Stream stream = sourceFile.OpenRead())
      {
        provider.WriteFile(file, stream, true, sourceFile.Length, ContentUtil.UnknownContentType);
      }

      Assert.Fail("Could create file");
    }
  }
}
