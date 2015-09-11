using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hardcodet.Commons.IO;
using NUnit.Framework;
using Vfs;
using Vfs.Transfer;
using Vfs.Util;


namespace TransferServices_Test.Download_Service
{
  [TestFixture]
  public class Given_Download_Service_When_Downloaded_File_Is_Modified_Or_Deleted : DownloadServiceTestBase
  {


    [Test]
    [ExpectedException(typeof(VirtualResourceNotFoundException))]
    public void Trying_To_Read_First_Block_Of_Deleted_File_Should_Fail()
    {
      SourceFile.Delete();
      DownloadService.ReadBlock(Token.TransferId, 0);
    }



    [Test]
    public void Modifying_And_Deleting_Should_Work_While_Downloading()
    {
      DownloadService.ReadBlock(Token.TransferId, 0);
      SourceFile.Delete();
    }

 

    [Test]
    public void Deleted_File_Should_Cause()
    {
      SourceFile.Delete();
      Assert.Fail("Test not implemented yet."); //TODO missing test
    }

  }

}
