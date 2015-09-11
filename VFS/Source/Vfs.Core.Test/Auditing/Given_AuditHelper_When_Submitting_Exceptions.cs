using Moq;
using NUnit.Framework;
using Vfs.Auditing;

namespace Vfs.Test.Auditing
{
  [TestFixture]
  public class Given_AuditHelper_When_Submitting_Exceptions
  {
    private CachingAuditor auditor;

    [SetUp]
    public void Init()
    {
      auditor = new CachingAuditor();
    }


    [Test]
    public void Submitted_Exceptions_Should_Be_Logged()
    {
      VfsException e = new ResourceAccessException("hello");

      AuditHelper.AuditException(auditor, e, FileSystemTask.RootFolderInfoRequest);
      Assert.AreEqual(1, auditor.Items.Count);
      Assert.AreEqual(FileSystemTask.RootFolderInfoRequest, auditor.Items[0].Context);
    }

    [Test]
    public void Already_Audited_Exceptions_Should_Not_Be_Audited_Anymore()
    {
      VfsException e = new ResourceAccessException("hello") {IsAudited = true};

      AuditHelper.AuditException(auditor, e, FileSystemTask.RootFolderInfoRequest);
      Assert.AreEqual(0, auditor.Items.Count);
    }


    [Test]
    public void Exceptions_With_Audit_Suppression_Should_Be_Ignored()
    {
      VfsException e = new ResourceAccessException("hello") { SuppressAuditing = true };

      AuditHelper.AuditException(auditor, e, FileSystemTask.RootFolderInfoRequest);
      Assert.AreEqual(0, auditor.Items.Count);
    }


    [Test]
    public void Submitted_Exceptions_Should_Indicate_They_Have_Been_Audited_After_Processing()
    {
      VfsException e = new ResourceAccessException("hello");

      Assert.IsFalse(e.IsAudited);
      AuditHelper.AuditException(auditor,e, FileSystemTask.RootFolderInfoRequest);
      Assert.IsTrue(e.IsAudited);
    }

  }
}