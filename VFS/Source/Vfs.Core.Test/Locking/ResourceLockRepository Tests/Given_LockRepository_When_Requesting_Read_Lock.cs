using System;

using NUnit.Framework;
using Vfs.Locking;
using Vfs.Util;


namespace Vfs.Test.Locking.ResourceLockRepository_Tests
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_LockRepository_When_Requesting_Read_Lock : RepositoryTestBase
  {
    [Test]
    public void Mutliple_Read_Locks_Should_Be_Granted()
    {
      for (int i = 0; i < 10; i++)
      {
        TimeSpan? ts = i%2 == 0 ? (TimeSpan?) null : TimeSpan.FromMinutes(i);
        var rl = Repository.TryGetReadLock("xxx", ts);
        Assert.AreEqual(ResourceLockType.Read, rl.LockType);
      }
    }


    [Test]
    public void Expiration_Time_Should_Be_Set_Correctly()
    {
      var now = DateTimeOffset.Now.AddHours(2.5);
      SystemTime.Now = () => now;

      var lockItem = Repository.TryGetReadLock("xxx");
      Assert.IsNull(lockItem.Expiration);

      lockItem = Repository.TryGetReadLock("xxx", null);
      Assert.IsNull(lockItem.Expiration);

      lockItem = Repository.TryGetReadLock("xxx", TimeSpan.FromSeconds(5.5));
      Assert.AreEqual(now.AddMilliseconds(5500), lockItem.Expiration);
    }


    [Test]
    public void Expired_LockItems_Should_Not_Influence_Lock_State()
    {
      var now = DateTimeOffset.Now.AddHours(1);
      SystemTime.Now = () => now;

      TimeSpan? timeout = TimeSpan.FromMilliseconds(1000);
      Repository.TryGetReadLock("xxx", timeout);

      Assert.AreEqual(ResourceLockState.ReadOnly, Repository.GetLockState("xxx"));

      var later = now.AddHours(1);
      SystemTime.Now = () => later;

      Assert.AreEqual(ResourceLockState.Unlocked, Repository.GetLockState("xxx"));
    }


    [Test]
    public void Expired_Write_Lock_Should_Not_Prevent_Locking()
    {
      var now = DateTimeOffset.Now.AddHours(1);
      SystemTime.Now = () => now;

      //get a write lock
      TimeSpan? timeout = TimeSpan.FromMilliseconds(1000);
      Repository.TryGetWriteLock("xxx", timeout);
 
      //we don't get a lock
      Assert.AreEqual(ResourceLockState.Locked, Repository.GetLockState("xxx"));
      var lockItem = Repository.TryGetReadLock("xxx");
      Assert.AreEqual(ResourceLockType.Denied, lockItem.LockType);

      //let the write lock expire
      var later = now.AddHours(1);
      SystemTime.Now = () => later;

      //we do get a lock
      lockItem = Repository.TryGetReadLock("xxx");
      Assert.AreEqual(ResourceLockType.Read, lockItem.LockType);
      Assert.AreEqual(ResourceLockState.ReadOnly, Repository.GetLockState("xxx"));

    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Negative_Timespans_Should_Not_Be_Accepted()
    {
      //get a read lock
      TimeSpan? timeout = TimeSpan.FromMilliseconds(-1000);
      Repository.TryGetReadLock("xxx", timeout);
    }

  }
}
