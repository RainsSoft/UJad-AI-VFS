using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vfs.Transfer;

namespace Vfs.Test
{
  /// <summary>
  /// A simple transfer store that doesn't clean up inactive transfers.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class TestTransferStore<T> : InMemoryTransferStore<T> where T : ITransfer
  {
    public override void SetInactive(T transfer)
    {
      //do not remove the transfer
    }
  }
}