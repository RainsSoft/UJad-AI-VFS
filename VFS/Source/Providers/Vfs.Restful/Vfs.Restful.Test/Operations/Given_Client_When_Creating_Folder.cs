using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using NUnit.Framework;


namespace Vfs.Restful.Test.Operations
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_Client_When_Creating_Folder : ServiceTestBase
  {
    [Test]
    public void Folder_Should_Be_Created_On_File_System()
    {
      ClientFileSystem.CreateFolder("/FolderX");
    }
  }
}
