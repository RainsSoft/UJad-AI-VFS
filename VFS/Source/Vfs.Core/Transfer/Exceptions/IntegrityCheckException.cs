using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Vfs.Transfer
{
  /// <summary>
  /// Indicates a failed integrity check of a transferred resource,
  /// which means that the transfer should be regarded invalid.
  /// </summary>
#if !SILVERLIGHT
  [Serializable]
#endif
  public class IntegrityCheckException : TransferException
  {
    public IntegrityCheckException()
    {
    }

    public IntegrityCheckException(string message)
      : base(message)
    {
    }

    public IntegrityCheckException(string message, Exception inner)
      : base(message, inner)
    {
    }

#if !SILVERLIGHT
    protected IntegrityCheckException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    {
    }
#endif
  }

}
