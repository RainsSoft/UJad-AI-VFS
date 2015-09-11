using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Hardcodet.Commons.IO;
using NUnit.Framework;

namespace Vfs.Test
{
  public static class TestUtil
  {
    /// <summary>
    /// Creats a temporary test folder with 3 sub folders and dummy files.
    /// The root folder contains 20 files, the first folder 5, the second 10,
    /// and the third sub folder 15 files.
    /// </summary>
    /// <returns></returns>
    public static DirectoryInfo CreateTestDirectory()
    {
      DirectoryInfo root = FileUtil.CreateTempFolder("_VFS_CUSTOM_ROOT");

      var subDir1 = root.CreateSubdirectory("Child_A").FullName;
      var subDir2 = root.CreateSubdirectory("Child_B").FullName;
      var subDir3 = root.CreateSubdirectory("Child_C").FullName;

      //create 5 and 10 files
      for (int i = 0; i < 20; i++)
      {
        File.WriteAllBytes(FileUtil.CreateTempFilePath(root.FullName, "tmp" + i, "txt"), new byte[32768 * i]);
        if (i < 5) File.WriteAllBytes(FileUtil.CreateTempFilePath(subDir1, "subfile_A" + i, "txt"), new byte[32768 * i]);
        if (i < 10) File.WriteAllBytes(FileUtil.CreateTempFilePath(subDir2, "subfile_B" + i, "txt"), new byte[32768 * i]);
        if (i < 15) File.WriteAllBytes(FileUtil.CreateTempFilePath(subDir3, "subfile_C" + i, "txt"), new byte[32768 * i]);
      }

      return root;
    }


    /// <summary>
    /// Checks two objects for equality by performing XML serialization and
    /// comparing the resulting markup.
    /// </summary>
    public static void AssertXEqualTo<T>(this T expected, T actual)
    {
      var xml1 = expected.ToXmlDataContract();
      var xml2 = actual.ToXmlDataContract();
      Assert.AreEqual(xml1, xml2);
    }


    public static string ToXmlDataContract<T>(this T obj)
    {
      var serializer = new DataContractSerializer(obj.GetType());
      using (var sw = new StringWriter())
      {
        using (var xtw = new XmlTextWriter(sw))
        {
          xtw.Formatting = Formatting.Indented;
          serializer.WriteObject(xtw, obj);
          xtw.Flush();
          return sw.ToString();
        }
      }
    }
    
    
    /// <summary>
    /// Checks two objects for equality by performing XML serialization and
    /// comparing the resulting markup.
    /// </summary>
    public static void AssertXEqualTo2<T>(this T expected, T actual)
    {
      XmlSerializer s = new XmlSerializer(typeof (T));
      StringWriter writer = new StringWriter();

      s.Serialize(writer, expected);
      string xml1 = writer.ToString();

      writer = new StringWriter();
      s.Serialize(writer, actual);
      string xml2 = writer.ToString();

      Assert.AreEqual(xml1, xml2);
    }

   

  }
}
