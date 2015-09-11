using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using NUnit.Framework;


namespace Vfs.Restful.Test.Browse
{
  [TestFixture]
  public class Given_Client_When_Querying_For_Unknown_Resources : ServiceTestBase
  {

    [Test]
    public void Requesting_Unknown_Folder_Should_Raise_Not_Found_Exception()
    {
      Assert.Fail("Test not implemented yet."); //TODO missing test
    }


    [Test]
    public void Requesting_Unknown_File_Should_Raise_Not_Found_Exception()
    {
      Assert.Fail("Test not implemented yet."); //TODO missing test
    }

  }
}
