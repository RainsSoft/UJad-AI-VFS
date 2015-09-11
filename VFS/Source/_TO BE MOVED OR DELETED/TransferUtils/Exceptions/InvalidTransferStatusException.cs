using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Vfs.Transfer
{
  /// <summary>
  /// Thrown if the current <see cref="TransferStatus"/> of a transfer
  /// does not allow the requested operation.
  /// </summary>
#if !SILVERLIGHT
  [Serializable]
#endif
  public class InvalidTransferStatusException : TransferException
  {
    public InvalidTransferStatusException()
    {
    }

    public InvalidTransferStatusException(string message) : base(message)
    {
    }

    public InvalidTransferStatusException(string message, Exception inner) : base(message, inner)
    {
    }

#if !SILVERLIGHT
    protected InvalidTransferStatusException(
      SerializationInfo info,
      StreamingContext context) : base(info, context)
    {
    }
#endif
  }

}
