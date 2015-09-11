using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Vfs.Test
{
  [TestFixture]
  public class Given_Resources_When_Filtering_By_Search_Pattern
  {
    private List<VirtualResourceInfo> resources;

    [SetUp]
    public void Init()
    {
      resources = new List<VirtualResourceInfo>();

      resources.Add(new VirtualFileInfo() {Name = "foo"});
      resources.Add(new VirtualFileInfo() {Name = "bar"});
      resources.Add(new VirtualFileInfo() {Name = "foobar"});
      resources.Add(new VirtualFileInfo() {Name = "foo.bar"});
      resources.Add(new VirtualFileInfo() {Name = "bar.foo"});
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Applying_Simple_Asterisk_Should_Return_All_Items()
    {
      var result = resources.Filter("*");
      Assert.AreEqual(resources.Count, result.Count());
    }


    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void Filtering_Sub_String_Should_Work()
    {
      var result = resources.Filter("*oo*").ToList();
      Assert.AreEqual(4, result.Count());

      Assert.Contains(resources[0], result);
      Assert.Contains(resources[2], result);
      Assert.Contains(resources[3], result);
      Assert.Contains(resources[4], result);

      CollectionAssert.DoesNotContain(result, resources[1]);
    }

  }
}
