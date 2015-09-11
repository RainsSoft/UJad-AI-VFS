using System.Linq;
using NUnit.Framework;


namespace Vfs.Providers.TestSuite.Operations.Move
{
  [TestFixture]
  public class Given_File_When_Moving : TestBase
  {
    private VirtualFile source;
    private string targetPath;

    protected override void InitInternal()
    {
      base.InitInternal();

      //create source and add two files
      source = Context.DownloadFolder.GetFiles().First();

      targetPath = FileSystem.CreateFilePath(Context.EmptyFolder.MetaData.FullName, "target.txt");
    }


    [Test]
    public void Moving_File_Should_Create_New_File()
    {
      Assert.False(FileSystem.IsFileAvailable(targetPath));
      source.Move(targetPath);
      Assert.True(FileSystem.IsFileAvailable(targetPath));
    }


    [Test]
    public void Moving_File_Should_Remove_Source()
    {
      string sourcePath = source.MetaData.FullName;
      Assert.True(FileSystem.IsFileAvailable(sourcePath));
      source.Move(targetPath);
      Assert.False(FileSystem.IsFileAvailable(sourcePath));
    }


    [Test]
    public void Moving_Should_Return_Updated_File_Info()
    {
      var sourceInfo = FileSystem.GetFileInfo(source.MetaData.FullName);

      var target = FileSystem.MoveFile(source.MetaData, targetPath);
      Assert.AreEqual("target.txt", target.Name);
      Assert.AreEqual(Context.EmptyFolder.MetaData.FullName, target.ParentFolderPath);
      Assert.AreEqual(sourceInfo.Length, target.Length);
    }



    [Test]
    [ExpectedException(typeof(ResourceAccessException))]
    public void Moving_To_Itself_Should_Fail()
    {
      FileSystem.MoveFile(source.MetaData, source.MetaData.FullName);
    }


    [Test]
    [ExpectedException(typeof(ResourceOverwriteException))]
    public void Moving_To_Destination_That_Already_Exists_Should_Fail()
    {
      var fileInfo = source.MetaData;
      //create a copy of the file
      FileSystem.CopyFile(fileInfo, targetPath);

      //try to move to the location
      FileSystem.MoveFile(fileInfo, targetPath);
    }



    [Test]
    public void Moving_Virtual_File_Should_Update_Internal_Metadata()
    {
      string fullName = source.MetaData.FullName;
      string parentPath = source.MetaData.ParentFolderPath;

      source.Move(targetPath);

      Assert.AreNotEqual(fullName, source.MetaData.FullName);
      Assert.AreNotEqual(parentPath, source.MetaData.ParentFolderPath);
    }
  }
}
