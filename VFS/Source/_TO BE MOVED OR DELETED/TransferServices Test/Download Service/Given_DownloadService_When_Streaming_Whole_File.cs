using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Hardcodet.Commons.IO;
using NUnit.Framework;
using Vfs;
using Vfs.Transfer;
using Vfs.Util;


namespace TransferServices_Test.Download_Service
{
  [TestFixture]
  public class Given_DownloadService_When_Streaming_Whole_File : DownloadServiceTestBase
  {
    public DateTimeOffset Timestamp;

    protected override void InitInternal()
    {
    }


    [Test]
    public void Read_Data_Should_Match_Source_File()
    {
      string targetPath = FileUtil.CreateTempFilePath(RootDir.FullName, "copy", "bin");
      
      //get stream and write to file
      using (Stream stream = DownloadService.ReadResourceStreamed(Token.TransferId))
      {
        stream.WriteTo(targetPath);
      }

      //calculate hashes of source and target and compare
      string sourceHash = SourceFile.CalculateMd5Hash();
      string targetHash = new FileInfo(targetPath).CalculateMd5Hash();
      Assert.AreEqual(sourceHash, targetHash);
    }



    [Test]
    public void Aborted_Transfer_Should_Hit_While_Reading_Even_If_We_Try_To_Read_In_One_Buffer()
    {
      //behind the scenes, many blocks are read, independent of the buffer size. Accordingly,
      //the transfer will abort as soon as we want it
      Assert.Fail("Test not implemented yet."); //TODO missing test
    }


  }

}
