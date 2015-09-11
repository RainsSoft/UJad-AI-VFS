using System;
using Moq;
using NUnit.Framework;
using Vfs.Locking;
using Vfs.Util;


namespace Vfs.Test.Locking.LockTracker_Tests
{
  /// <summary>
  /// 
  /// </summary>
  [TestFixture]
  public class Given_Tracker_When_Locks_Have_Expired
  {
    public LockTracker Tracker { get; set; }

    public DateTimeOffset Now { get; set; }
    public DateTimeOffset Later { get; set; }


    [SetUp]
    public void Init()
    {
      Tracker = new LockTracker("xxx");
      Now = DateTimeOffset.Now.AddHours(2);
      Later = Now.AddHours(4);

      SystemTime.Now = () => Now;
     
    }


    [Test]
    public void Should_Grant_Read_Lock_If_Write_Lock_Expired()
    {
      var rl = Tracker.TryGetWriteLock(TimeSpan.FromMinutes(60));
      Assert.IsTrue(rl.IsEnabled);

      SystemTime.Now = () => Later;

      rl = Tracker.TryGetReadLock();
      Assert.IsTrue(rl.IsEnabled);
    }

    [Test]
    public void Should_Grant_Read_Lock_If_Older_Read_Lock_Expired()
    {
      var rl = Tracker.TryGetReadLock(TimeSpan.FromMinutes(60));
      Assert.IsTrue(rl.IsEnabled);

      SystemTime.Now = () => Later;

      rl = Tracker.TryGetReadLock();
      Assert.IsTrue(rl.IsEnabled);
    }


    [Test]
    public void Should_Grant_Write_Lock_If_Read_Lock_Expired()
    {
      var rl = Tracker.TryGetReadLock(TimeSpan.FromMinutes(60));
      Assert.IsTrue(rl.IsEnabled);

      SystemTime.Now = () => Later;

      rl = Tracker.TryGetWriteLock();
      Assert.IsTrue(rl.IsEnabled);
    }

    [Test]
    public void Should_Grant_Write_Lock_If_Write_Lock_Expired()
    {
      var rl = Tracker.TryGetWriteLock(TimeSpan.FromMinutes(60));
      Assert.IsTrue(rl.IsEnabled);

      SystemTime.Now = () => Later;

      rl = Tracker.TryGetWriteLock();
      Assert.IsTrue(rl.IsEnabled);
    }
  }
}
