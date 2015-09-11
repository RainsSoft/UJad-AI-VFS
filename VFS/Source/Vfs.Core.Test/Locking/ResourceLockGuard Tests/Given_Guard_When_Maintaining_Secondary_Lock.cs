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
  public class Given_Guard_When_Maintaining_Secondary_Locks
  {
    public ResourceLockGuard Guard { get; set; }
    public LockItem Lock { get; set; }
    public List<LockItem> SecondaryLocks { get; set; }

    public Mock<IResourceLockRepository> RepositoryMock { get; set; }
    public IResourceLockRepository Repository
    {
      get { return RepositoryMock.Object; }
    }

    [SetUp]
    public void Init()
    {
      RepositoryMock = new Mock<IResourceLockRepository>();
      Lock = LockItem.CreateRead("xxx", null);
      
      //add both read and write locks as secondary ones
      SecondaryLocks = new List<LockItem>();
      for (int i = 0; i < 10; i++)
      {
        var item = i%2 == 0 ? LockItem.CreateRead(i.ToString(), null) : LockItem.CreateWrite(i.ToString(), null);
        SecondaryLocks.Add(item);
      }
    }


    [Test]
    public void Should_Contain_Secondary_Locks()
    {
      Guard = new ResourceLockGuard(Lock, Repository) {SecondaryLocks = this.SecondaryLocks};
      Assert.AreSame(SecondaryLocks, Guard.SecondaryLocks);
    }


    [Test]
    public void Should_Release_Write_Main_And_Secondardy_Locks()
    {
      bool mainReleased = false;

      RepositoryMock.Setup(rep => rep.ReleaseReadLock("xxx", It.IsAny<string>()))
        .Callback(() => mainReleased = true);

      RepositoryMock.Setup(rep => rep.ReleaseLock(It.IsAny<LockItem>()))
        .Callback(() => Assert.IsTrue(mainReleased));

      //make sure putting the guard into a using construct eventually releases the lock
      using (var grd = new ResourceLockGuard(Lock, RepositoryMock.Object) {SecondaryLocks = SecondaryLocks})
      {
      }


      RepositoryMock.Verify(rep => rep.ReleaseReadLock(Lock.ResourceId, Lock.LockId), Times.Once());
      RepositoryMock.Verify(rep => rep.ReleaseLock(It.IsAny<LockItem>()), Times.Exactly(SecondaryLocks.Count));
      RepositoryMock.Verify(rep => rep.ReleaseWriteLock(Lock.ResourceId, Lock.LockId), Times.Never());
    }


    [Test]
    public void Should_Release_Main_Lock_Before_Secondary_Ones()
    {
      bool mainReleased = false;

      //set the bool to true on release
      RepositoryMock.Setup(rep => rep.ReleaseReadLock(Lock.ResourceId, Lock.LockId))
        .Callback(() => mainReleased = true);

      //verify flag was switched
      RepositoryMock.Setup(rep => rep.ReleaseLock(It.IsAny<LockItem>()))
        .Callback(() => Assert.IsTrue(mainReleased));

      //make sure putting the guard into a using construct eventually releases the lock
      using (var grd = new ResourceLockGuard(Lock, RepositoryMock.Object) { SecondaryLocks = SecondaryLocks })
      {
      }
    }


  }
}
