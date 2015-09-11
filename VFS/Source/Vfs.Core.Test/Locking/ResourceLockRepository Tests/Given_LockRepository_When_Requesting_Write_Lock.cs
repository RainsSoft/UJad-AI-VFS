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
  public class Given_LockRepository_When_Requesting_Write_Lock : RepositoryTestBase
  {
    [Test]
    public void Only_One_Write_Lock_Should_Be_Granted()
    {
      var wl = Repository.TryGetWriteLock("xxx");
      Assert.IsTrue(wl.IsEnabled);

      for (int i = 0; i < 10; i++)
      {
        var rl = Repository.TryGetWriteLock("xxx");
        Assert.IsFalse(rl.IsEnabled);
      }
    }


    [Test]
    public void Expiration_Time_Should_Be_Set_Correctly()
    {
      var now = DateTimeOffset.Now.AddHours(2.5);
      SystemTime.Now = () => now;

      var wl = Repository.TryGetWriteLock("xxx");
      Assert.IsNull(wl.Expiration);
      Repository.ReleaseLock(wl);

      wl = Repository.TryGetWriteLock("xxx", null);
      Assert.IsNull(wl.Expiration);
      Repository.ReleaseLock(wl);

      wl = Repository.TryGetWriteLock("xxx", TimeSpan.FromSeconds(5.5));
      Assert.AreEqual(now.AddMilliseconds(5500), wl.Expiration);
    }


    [Test]
    public void Expired_LockItem_Should_Not_Influence_Lock_State()
    {
      var now = DateTimeOffset.Now.AddHours(1);
      SystemTime.Now = () => now;

      TimeSpan? timeout = TimeSpan.FromMilliseconds(1000);
      Repository.TryGetWriteLock("xxx", timeout);

      Assert.AreEqual(ResourceLockState.Locked, Repository.GetLockState("xxx"));

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
      var wl = Repository.TryGetWriteLock("xxx");
      Assert.AreEqual(ResourceLockType.Denied, wl.LockType);

      //let the write lock expire
      var later = now.AddHours(1);
      SystemTime.Now = () => later;

      //we do get a new lock
      wl = Repository.TryGetWriteLock("xxx");
      Assert.AreEqual(ResourceLockType.Write, wl.LockType);
      Assert.AreEqual(ResourceLockState.Locked, Repository.GetLockState("xxx"));

    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Negative_Timespans_Should_Not_Be_Accepted()
    {
      //get a write lock
      TimeSpan? timeout = TimeSpan.FromMilliseconds(-1000);
      Repository.TryGetWriteLock("xxx", timeout);
    }

  }
}
