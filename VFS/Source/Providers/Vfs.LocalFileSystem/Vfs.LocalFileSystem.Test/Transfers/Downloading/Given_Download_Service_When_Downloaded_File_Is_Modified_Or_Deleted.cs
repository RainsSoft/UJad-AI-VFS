using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hardcodet.Commons.IO;
using NUnit.Framework;
using Vfs;
using Vfs.LocalFileSystem.Test.Transfers.Downloading;
using Vfs.Transfer;
using Vfs.Util;


namespace TransferServices_Test.Download_Service
{
  [TestFixture]
  public class Given_Download_Service_When_Downloaded_File_Is_Modified_Or_Deleted : DownloadServiceTestBase
  {

    protected override void InitInternal()
    {
      base.InitInternal();
      InitToken();
    }


    [Test]
    [ExpectedException(typeof(VirtualResourceNotFoundException))]
    public void Trying_To_Read_First_Block_Of_Deleted_File_Should_Fail()
    {
      SourceFile.Delete();
      DownloadHandler.ReadBlock(Token.TransferId, 0);
    }



    [Test]
    public void Modifying_And_Deleting_Should_Work_While_Downloading()
    {
      DownloadHandler.ReadBlock(Token.TransferId, 0);
      //this doesn't delete the file right away (it's still in use, but leaves it up to the FS to clean up)
      SourceFile.Delete();
    }


  }

}
