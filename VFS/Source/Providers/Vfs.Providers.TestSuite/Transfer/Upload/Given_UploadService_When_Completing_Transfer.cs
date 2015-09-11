using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Vfs.Util;

namespace Vfs.Providers.TestSuite.Transfer.Upload
{
  [TestFixture]
  public class Given_UploadService_When_Completing_Transfer : UploadTestBase
  {

    [Test]
    public void Completion_Should_Succeed_If_No_Verification_Hash_Is_Sent()
    {
      Assert.Fail("Test not implemented yet."); //TODO missing test
    }

    [Test]
    public void Completion_Should_Succeed_With_Valid_File_Hash()
    {
      Assert.Fail("Test not implemented yet."); //TODO missing test
    }


    [Test]
    public void Completion_Should_Fail_With_Invalid_File_Hash()
    {
      Assert.Fail("Test not implemented yet."); //TODO missing test
    }

  }
}
