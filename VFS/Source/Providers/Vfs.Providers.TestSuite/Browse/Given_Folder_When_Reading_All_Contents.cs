using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Vfs.Providers.TestSuite.Browse
{
  [TestFixture]
  public class Given_Folder_When_Reading_All_Contents : TestBase
  {

    /// <summary>
    /// 
    /// </summary>


    /// <summary>
    /// 
    /// </summary>
    [Test]
    [ExpectedException(typeof(ResourceAccessException))]
    public void Requesting_Of_Contents_Outside_Scope_Should_Fail()
    {
      provider.GetFolderContents(rootDirectory.Parent.FullName);
    }

  }
}
