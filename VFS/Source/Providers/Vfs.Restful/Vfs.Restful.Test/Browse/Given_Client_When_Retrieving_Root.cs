using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using NUnit.Framework;
using Vfs.Test;


namespace Vfs.Restful.Test.Browse
{
  [TestFixture]
  public class Given_Client_When_Retrieving_Root : ServiceTestBase
  {
    [Test]
    public void Root_Folder_Should_Be_Returned_Properly()
    {
      Assert.IsTrue(ClientRoot.MetaData.IsRootFolder);

      var root1 = ClientFileSystem.GetFileSystemRoot();
      var root2 = ServiceFileSystem.GetFileSystemRoot();
      
      root2.AssertXEqualTo(root1);
    }


    [Test]
    public void Requesting_Root_By_Name_Should_Work()
    {
      var root = ClientFileSystem.GetFolderInfo("/");
      ClientRoot.MetaData.AssertXEqualTo(root);
    }



    [Test]
    public void Requesting_Root_Folders_Parent_Should_Fail()
    {
      Assert.Fail("Test not implemented yet."); //TODO missing test
    }

  }
}
