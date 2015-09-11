using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using NUnit.Framework;


namespace Vfs.Providers.TestSuite.Browse
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_Resource_When_Requesting_Parent_Folder : TestBase
  {
    [Test]
    public void Querying_Parent_Of_File_Should_Return_Matching_Folder()
    {
      bool hasFiles = false;
      RecurseFileSystem(FileSystemRoot,
                        (fo, fi) =>
                          {
                            hasFiles = true;
                            Assert.AreEqual(fo.MetaData.FullName, fi.GetParentFolder().MetaData.FullName);
                          },
                        null);

      Assert.IsTrue(hasFiles);
    }


    [Test]
    public void Querying_Parent_Of_Folder_Should_Return_Matching_Folder()
    {
      bool hasFolders = false;
      RecurseFileSystem(FileSystemRoot, null,
                        (parent, child) =>
                          {
                            hasFolders = true;
                            Assert.AreEqual(parent.MetaData.FullName, child.GetParentFolder().MetaData.FullName);
                          });

      Assert.IsTrue(hasFolders);
    }



    [ExpectedException(typeof(ResourceAccessException))]
    [Test]
    public void Querying_Parent_Of_Root_Folder_Should_Not_Work()
    {
      var folder = FileSystemRoot.GetParentFolder();
    }
  }
}
