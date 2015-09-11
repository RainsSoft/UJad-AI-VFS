using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Vfs.LocalFileSystem.Test.Resource_Reading
{
  [TestFixture]
  public class Given_Folder_When_Filtering_Results : DirectoryTestBase
  {
    protected override void InitInternal()
    {
      File.Create(Path.Combine(rootDirectory.FullName, "foo.txt")).Close();
      File.Create(Path.Combine(rootDirectory.FullName, "bar.txt")).Close();
      File.Create(Path.Combine(rootDirectory.FullName, "foobar.txt")).Close();
      File.Create(Path.Combine(rootDirectory.FullName, "barfoo.txt")).Close();

      rootDirectory.CreateSubdirectory("dir_foo");
      rootDirectory.CreateSubdirectory("dir_bar");
      rootDirectory.CreateSubdirectory("dir_foobar");
      rootDirectory.CreateSubdirectory("dir_barfoo");
    }



    private void CheckResults<T>(IEnumerable<T> results, params string[] names) where T:VirtualResourceInfo
    {
      Assert.AreEqual(results.Count(), names.Length);

      foreach (var name in names)
      {
        string s = name;
        results.Single(res => res.Name == s);
      }
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Requesting_Files_With_Search_Pattern_Should_Return_Matches_Only()
    {
      var results = provider.GetChildFiles("/", "foo*");
      CheckResults(results, "foo.txt", "foobar.txt");

      results = provider.GetChildFiles(root, "*bar.txt");
      CheckResults(results, "bar.txt", "foobar.txt");

      results = provider.GetChildFiles(root.FullName, "*.t?t");
      CheckResults(results, rootDirectory.GetFiles("*.t?t").Select(fi => fi.Name).ToArray());
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Requesting_Folders_With_Search_Pattern_Should_Return_Matches_Only()
    {
      var results = provider.GetChildFiles("/", "foo*");
      CheckResults(results, "foo.txt", "foobar.txt");

      results = provider.GetChildFiles(root, "*bar.txt");
      CheckResults(results, "bar.txt", "foobar.txt");
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Requesting_Contents_With_Search_Pattern_Should_Return_All_Matching_Files_And_Folders()
    {
      string pattern = "*r*";
     
      var results = provider.GetFolderContents(root, pattern);
      Assert.AreEqual(rootDirectory.GetFiles(pattern).Length, results.Files.Count());
      Assert.AreEqual(rootDirectory.GetDirectories(pattern).Length, results.Folders.Count());
    }


  }
}
