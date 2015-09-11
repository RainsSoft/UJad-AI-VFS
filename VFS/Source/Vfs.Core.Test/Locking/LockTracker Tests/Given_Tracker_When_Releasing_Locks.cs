using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using NUnit.Framework;
using Vfs.Locking;
using Vfs.Util;


namespace Vfs.Test.Locking.LockTracker_Tests
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_Tracker_When_Releasing_Locks
  {
    public LockTracker Tracker { get; set; }

    [SetUp]
    public void Init()
    {
      Tracker = new LockTracker("xxx");
    }


    [Test]
    public void Releasing_All_Read_Locks_Should_Enable_Write_Locking()
    {
      List<LockItem> rls = new List<LockItem>();
      for (int i = 0; i < 10; i++)
      {
        var rl = Tracker.TryGetReadLock();
        Assert.AreEqual(ResourceLockType.Read, rl.LockType);
        rls.Add(rl);
      }

      var wl = Tracker.TryGetWriteLock();
      Assert.IsFalse(wl.IsEnabled);

      //release all but one lock
      for (int i = 0; i < 10; i++)
      {
        Assert.IsTrue(Tracker.ReleaseReadLock(rls[i].LockId));
      }

      wl = Tracker.TryGetWriteLock();
      Assert.IsTrue(wl.IsEnabled);
    }



    [Test]
    public void Releasing_Some_Read_Locks_Should_Not_Enable_Write_Locking()
    {
      List<LockItem> rls = new List<LockItem>();
      for(int i=0; i<10; i++)
      {
        var rl = Tracker.TryGetReadLock();
        Assert.AreEqual(ResourceLockType.Read, rl.LockType);
        rls.Add(rl);
      }

      var wl = Tracker.TryGetWriteLock();
      Assert.IsFalse(wl.IsEnabled);

      //release all but one lock
      for (int i = 1; i < 10; i++)
      {
        if(i == 5) continue;
        Assert.IsTrue(Tracker.ReleaseReadLock(rls[i].LockId));
      }

      wl = Tracker.TryGetWriteLock();
      Assert.IsFalse(wl.IsEnabled);
    }



    [Test]
    public void Releasing_Write_Lock_Should_Enable_Another_Write_Lock()
    {
      var l1 = Tracker.TryGetWriteLock();
      Assert.IsTrue(l1.IsEnabled);

      var l2 = Tracker.TryGetWriteLock();
      Assert.IsFalse(l2.IsEnabled);

      Assert.IsTrue(Tracker.ReleaseWriteLock(l1.LockId));
      l2 = Tracker.TryGetWriteLock();
      Assert.IsTrue(l2.IsEnabled);
    }

    [Test]
    public void Releasing_Write_Lock_Should_Enable_Read_Locking()
    {
      var l1 = Tracker.TryGetWriteLock();
      Assert.IsTrue(l1.IsEnabled);

      var l2 = Tracker.TryGetReadLock();
      Assert.IsFalse(l2.IsEnabled);

      Assert.IsTrue(Tracker.ReleaseWriteLock(l1.LockId));
      l2 = Tracker.TryGetReadLock();
      Assert.IsTrue(l2.IsEnabled);
    }


    [Test]
    public void Releasing_Unknown_Lock_Id_Should_Return_False()
    {
      Assert.IsFalse(Tracker.ReleaseReadLock("abc"));
      Assert.IsFalse(Tracker.ReleaseWriteLock("abc"));
    }


    [Test]
    public void Releasing_Unknown_Read_Lock_With_Active_Write_Lock_Should_Just_Return_Status()
    {
      var wl = Tracker.TryGetWriteLock();
      Assert.IsFalse(Tracker.ReleaseReadLock("abc"));
    }

    [Test]
    public void Releasing_Unknown_Write_Lock_With_Active_Read_Lock_Should_Just_Return_Status()
    {
      var rl = Tracker.TryGetReadLock();
      Assert.IsFalse(Tracker.ReleaseWriteLock("abc"));
    }


    [Test]
    public void Releasing_Already_Released_Lock_Should_Return_False()
    {
      //read lock
      var rl = Tracker.TryGetReadLock();
      Assert.IsTrue(Tracker.ReleaseReadLock(rl.LockId));
      Assert.IsFalse(Tracker.ReleaseReadLock(rl.LockId));

      //write lock
      var wl = Tracker.TryGetWriteLock();
      Assert.IsTrue(Tracker.ReleaseWriteLock(wl.LockId));
      Assert.IsFalse(Tracker.ReleaseWriteLock(wl.LockId));
    }

  }
}
