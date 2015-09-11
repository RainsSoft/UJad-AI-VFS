using Moq;
using NUnit.Framework;
using Vfs.Auditing;

namespace Vfs.Test
{
  [TestFixture]
  public class Given_FileSystem_When_Using_The_Default_Auditor
  {
    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void NullAuditor_Should_Be_Installed_Per_Default()
    {
      var mock = new Mock<FileSystemProviderBase>();
      Assert.IsInstanceOf<NullAuditor>(mock.Object.Auditor);
    }


    [Test]
    public void Setting_The_Auditor_Property_To_Null_Should_Fall_Back_To_NullAuditor()
    {
      var mock = new Mock<FileSystemProviderBase>();
      mock.Object.Auditor = null;
      Assert.IsInstanceOf<NullAuditor>(mock.Object.Auditor);
    }



  }
}
