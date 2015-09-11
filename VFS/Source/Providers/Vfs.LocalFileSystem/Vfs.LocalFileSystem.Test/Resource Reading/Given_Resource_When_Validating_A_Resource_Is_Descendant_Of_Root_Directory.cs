using System;
using System.IO;
using NUnit.Framework;


namespace Vfs.LocalFileSystem.Test.Resource_Tests
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_Resource_When_Validating_A_Resource_Is_Descendant_Of_Root_Directory
  {
    private DirectoryInfo root;

    [SetUp]
    public void Init()
    {
      root = new DirectoryInfo(@"C:\Test");
    }


    [Test]
    public void Submitting_The_Root_Folder_Itself_Should_Fail()
    {
      Assert.IsFalse(root.IsParentOf(@"C:\Test"));
    }

    [Test]
    public void Submitting_A_Root_Folder_That_Starts_With_Same_Characters_Should_Fail()
    {
      Assert.IsFalse(root.IsParentOf(@"C:\TestABC"));
    }

    [Test]
    public void Submitting_Alternative_Separators_In_Path_Should_Work()
    {
      Assert.IsTrue(root.IsParentOf(@"C:/Test/folder/readme.txt"));
      Assert.IsTrue(root.IsParentOf(@"C:/Test/folder/subfolder/"));
    }

    [Test]
    public void Resolving_Path_If_Root_Has_Terminating_Separator_Char_Should_Work()
    {
      root = new DirectoryInfo(@"C:\Test\");

      //test with forward and backward slashes
      Assert.IsTrue(root.IsParentOf(@"C:\Test\folder\readme.txt"));
      Assert.IsTrue(root.IsParentOf(@"C:\Test\folder\subfolder\"));

      Assert.IsTrue(root.IsParentOf(@"C:/Test/folder/readme.txt"));
      Assert.IsTrue(root.IsParentOf(@"C:/Test/folder/subfolder/"));

      root = new DirectoryInfo(@"C:\Test/");

      Assert.IsTrue(root.IsParentOf(@"C:\Test\folder\readme.txt"));
      Assert.IsTrue(root.IsParentOf(@"C:\Test\folder\subfolder\"));

      Assert.IsTrue(root.IsParentOf(@"C:/Test/folder/readme.txt"));
      Assert.IsTrue(root.IsParentOf(@"C:/Test/folder/subfolder/"));
    }

    [Test]
    public void Resolving_Path_If_Test_Has_Terminating_Separator_Char_Should_Work()
    {
      root = new DirectoryInfo(@"C:\Test");
      Assert.IsFalse(root.IsParentOf(@"C:\Test\"));
    }


    [Test]
    public void Submitting_A_Sibling_Folder_Should_Fail()
    {
      Assert.IsFalse(root.IsParentOf(@"C:\Test2"));
    }

    [Test]
    public void Submitting_A_Direct_Descendant_File_Should_Work()
    {
      Assert.IsTrue(root.IsParentOf(@"C:\Test\file.txt"));
    }


    [Test]
    public void Submitting_A_Direct_Descendant_Directory_Should_Work()
    {
      Assert.IsTrue(root.IsParentOf(@"C:\Test\folder"));
      Assert.IsTrue(root.IsParentOf(@"C:\Test\folder\"));
    }

    [Test]
    public void Submitting_Lower_Level_File_Should_Work()
    {
      Assert.IsTrue(root.IsParentOf(@"C:\Test\folder\readme.txt"));
    }


    [Test]
    public void Submitting_Lower_Level_Folder_Should_Work()
    {
      Assert.IsTrue(root.IsParentOf(@"C:\Test\folder\folder2"));
      Assert.IsTrue(root.IsParentOf(@"C:\Test\folder\folder2\"));
    }

    [Test]
    public void Validation_Should_Work_If_Root_Is_A_Drive()
    {
      root = new DirectoryInfo(@"C:\");
      Assert.IsTrue(root.IsParentOf(@"C:\Test"));
      Assert.IsFalse(root.IsParentOf(@"C:\"));
    }


    [Test]
    public void Submitting_Relative_Path_That_Still_Links_To_Child_Should_Work()
    {
      Assert.IsTrue(root.IsParentOf(@"C:\Test\folder\..\folder2\file.txt"));
      Assert.IsTrue(root.IsParentOf(@"C:\Test\folder0\folder1\..\..\.\folder2\file.txt"));
    }


    [Test]
    public void Submitting_Relative_Path_That_Links_Outside_Should_Fail()
    {
      Assert.IsFalse(root.IsParentOf(@"C:\Test\folder\..\..\folder2\file.txt"));
    }

    [Test]
    public void Submitting_Path_Of_An_Existing_File_Should_Work()
    {
      //the validation routine internally converts the path to a directory
      //make sure this will work, even if the path links to an existing file
      FileInfo fi = new FileInfo(GetType().Assembly.Location);
      root = fi.Directory;
      Assert.IsTrue(root.IsParentOf(fi.FullName));
      Assert.IsFalse(root.IsParentOf(fi.Directory.FullName));
    }

  }
}
