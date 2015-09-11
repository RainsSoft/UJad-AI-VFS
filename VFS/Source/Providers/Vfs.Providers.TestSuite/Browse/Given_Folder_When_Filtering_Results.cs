using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Vfs.Providers.TestSuite.Browse
{
  [TestFixture]
  public class Given_Folder_When_Filtering_Results : TestBase
  {
    private VirtualFolder testFolder;

    protected override void InitInternal()
    {
      base.InitInternal();

      testFolder = Context.EmptyFolder;

      testFolder.AddFolder("dir_foo");
      testFolder.AddFolder("dir_bar");
      testFolder.AddFolder("dir_foobar");
      testFolder.AddFolder("dir_barfoo");


      testFolder.AddFile(Context.DownloadFile0Template.FullName, "foo.txt", false);
      testFolder.AddFile(Context.DownloadFile0Template.FullName, "bar.txt", false);
      testFolder.AddFile(Context.DownloadFile1Template.FullName, "foobar.txt", false);
      testFolder.AddFile(Context.DownloadFile1Template.FullName, "barfoo.txt", false);
    }



    private static void CheckResults<T>(IEnumerable<T> results, params string[] names) where T:VirtualResourceInfo
    {
      Assert.AreEqual(names.Length, results.Count());

      foreach (var name in names)
      {
        string s = name;
        results.Single(res => res.Name == s);
      }
    }




    [Test]
    public void Requesting_Files_With_Search_Pattern_Should_Return_Matches_Only()
    {
      var results = testFolder.GetFiles("foo*").Select(f => f.MetaData);
      CheckResults(results, "foo.txt", "foobar.txt");

      results = testFolder.GetFiles("*bar.txt").Select(f => f.MetaData);
      CheckResults(results, "bar.txt", "foobar.txt");

      results = testFolder.GetFiles("*.t?t").Select(f => f.MetaData);
      CheckResults(results, testFolder.GetFiles("*.t?t").Select(fi => fi.MetaData.Name).ToArray());
    }



    [Test]
    public void Requesting_Folders_With_Search_Pattern_Should_Return_Matches_Only()
    {
      var results = testFolder.GetFolders("dir_foo*").Select(f => f.MetaData);
      CheckResults(results, "dir_foo", "dir_foobar");

      results = testFolder.GetFolders("*_*").Select(f => f.MetaData);
      Assert.AreEqual(4, results.Count());
    }


    [Test]
    public void Requesting_Contents_With_Search_Pattern_Should_Return_All_Matching_Files_And_Folders()
    {
      string pattern = "*r*";
     
      var results = testFolder.GetFolderContents(pattern);

      CheckResults(results.Files.Select(f => f.MetaData),
                   testFolder.GetFiles(pattern).Select(f => f.MetaData.Name).ToArray());

      CheckResults(results.Folders.Select(f => f.MetaData),
             testFolder.GetFolders(pattern).Select(f => f.MetaData.Name).ToArray());
    }


  }
}
