using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Vfs.Locking;
using Vfs.Test;
using Vfs.Util;

namespace Vfs.Test.Locking.ResourceLockRepository_Tests
{
  /// <summary>
  /// Base class for tests that operate on a test directory
  /// which consists files and folders.
  /// </summary>
  public abstract class RepositoryTestBase
  {
    public ResourceLockRepository Repository { get; set; }


    [SetUp]
    public void Init()
    {
      Repository = new ResourceLockRepository();
      InitInternal();
    }


    protected virtual void InitInternal()
    {
    }


    [TearDown]
    public void Cleanup()
    {
      SystemTime.Reset();
    }


    protected virtual void CleanupInternal()
    {

    }
  }
}
