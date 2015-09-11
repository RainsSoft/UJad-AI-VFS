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
  public class Given_Tracker_When_Requesting_Write_Lock
  {
    public LockTracker Tracker { get; set; }

    [SetUp]
    public void Init()
    {
      Tracker = new LockTracker("xxx");
    }



    [Test]
    public void Should_Return_Valid_Lock_If__Unlocked()
    {
      Assert.AreEqual(ResourceLockState.Unlocked, Tracker.LockState);

      var l = Tracker.TryGetWriteLock();
      Assert.IsTrue(l.IsEnabled);
    }

    [Test]
    public void Should_Deny_Lock_If_Read_Lock_Is_Active()
    {
      var l1 = Tracker.TryGetReadLock();
      Assert.IsTrue(l1.IsEnabled);

      var l2 = Tracker.TryGetWriteLock();
      Assert.IsFalse(l2.IsEnabled);
    }

    [Test]
    public void Should_Deny_Lock_If_Write_Lock_Is_Active()
    {
      var l1 = Tracker.TryGetWriteLock();
      Assert.IsTrue(l1.IsEnabled);

      var l2 = Tracker.TryGetWriteLock();
      Assert.IsFalse(l2.IsEnabled);
    }


  }
}
