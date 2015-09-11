using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Vfs.Transfer
{
  /// <summary>
  /// Indicates that the transfer's status is no longer valid,
  /// e.g. because it expired or because it was cancelled.
  /// </summary>
#if !SILVERLIGHT
  [Serializable]
#endif
  public class TransferStatusException : TransferException
  {
    public TransferStatusException()
    {
    }

    public TransferStatusException(string message) : base(message)
    {
    }

    public TransferStatusException(string message, Exception inner) : base(message, inner)
    {
    }

#if !SILVERLIGHT
    protected TransferStatusException(
      SerializationInfo info,
      StreamingContext context) : base(info, context)
    {
    }
#endif
  }

}
