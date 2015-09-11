using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using NUnit.Framework;


namespace Vfs.Restful.Test.Browse
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_Client_When_Creating_Resource_Paths : ServiceTestBase
  {
    [Test]
    public void INPUT()
    {
      var path = ClientFileSystem.CreateFolderPath(":?$ds", "readme.txt");
      Console.Out.WriteLine("path = {0}", path);
    }
  }
}
