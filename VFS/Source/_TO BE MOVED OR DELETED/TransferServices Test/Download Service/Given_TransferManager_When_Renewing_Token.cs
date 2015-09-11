using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hardcodet.Commons.IO;
using NUnit.Framework;
using Vfs.Transfer;
using Vfs.Util;


namespace TransferServices_Test.Download_Service
{
  [TestFixture]
  public class Given_DownloadService_When_Renewing_Token : DownloadServiceTestBase
  {
    public DateTimeOffset CreationTime;

    protected override void InitInternal()
    {
      CreationTime = DateTimeOffset.Now;
      SystemTime.Now = () => CreationTime;
    }



    [Test]
    public void Token_Should_Indicate_The_Number_Of_Blocks_Transmitted_Already()
    {
      throw new NotImplementedException(""); //TODO provide implementation
    }


  }

}
