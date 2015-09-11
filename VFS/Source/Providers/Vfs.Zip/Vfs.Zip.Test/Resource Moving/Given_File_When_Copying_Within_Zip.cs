using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using Ionic.Zip;
using NUnit.Framework;


namespace Vfs.Zip.Test.Resource_Moving
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_File_When_Copying_Within_Zip : ZipTestBase
  {
    [Test]
    public void Copy_Should_Equal_Source()
    {
      VirtualFolder root = VirtualFolder.CreateRootFolder(Provider);
      var movedFolder = root.GetFolders().First().GetFolders().First();

      foreach (var subFolder in movedFolder.GetFolders())
      {
        Console.Out.WriteLine("subFolder.MetaData.FullName = {0}", subFolder.MetaData.FullName);
      }

      foreach (var virtualFile in movedFolder.GetFiles().ToArray())
      {
        Console.Out.WriteLine("File: " + virtualFile.MetaData.Name + ", " + virtualFile.MetaData.FullName);
        virtualFile.Copy(virtualFile.MetaData.FullName + ".copy");
      }
    }

  }
}
