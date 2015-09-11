using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Moq;
using NUnit.Framework;
using Vfs.Locking;


namespace Vfs.Test.Locking.ResourceLockGuard_Tests
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_Guard_When_Maintaining_Denied_Lock
  {
    public ResourceLockGuard Guard { get; set; }
    public LockItem Lock { get; set; }

    public Mock<IResourceLockRepository> RepositoryMock { get; set; }
    public IResourceLockRepository Repository
    {
      get { return RepositoryMock.Object; }
    }

    [SetUp]
    public void Init()
    {
      Lock = LockItem.CreateDenied("xxx");
      RepositoryMock = new Mock<IResourceLockRepository>();
      Guard = new ResourceLockGuard(Lock, Repository);
    }


    [Test]
    public void Should_Indicate_Is_Not_Enabled()
    {
      Assert.IsFalse(Guard.IsLockEnabled);
    }

    [Test]
    public void Should_Contain_LockItem()
    {
      Assert.AreSame(Lock, Guard.Item);
    }


    [Test]
    public void Should_Not_Attempt_To_Release_Lock_On_Dispose()
    {
      //mock.Setup(rep => rep.ReleaseReadLock(LockItem.ResourceId, LockItem.LockId)).Returns(true);

      //make sure putting the guard into a using construct eventually releases the lock
      using (var grd = new ResourceLockGuard(Lock, RepositoryMock.Object))
      {
      }

      RepositoryMock.Verify(rep => rep.ReleaseWriteLock(Lock.ResourceId, Lock.LockId), Times.Never());
      RepositoryMock.Verify(rep => rep.ReleaseReadLock(Lock.ResourceId, Lock.LockId), Times.Never());
      RepositoryMock.Verify(rep => rep.ReleaseLock(Lock), Times.Never());
    }


  }
}
