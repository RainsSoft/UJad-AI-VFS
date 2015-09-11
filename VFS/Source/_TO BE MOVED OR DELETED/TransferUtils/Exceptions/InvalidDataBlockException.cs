using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Vfs.Transfer
{
  /// <summary>
  /// Thrown if an invalid data block was received, e.g. because
  /// of an unexpected block number or invalid block contents.
  /// </summary>
#if !SILVERLIGHT
  [Serializable]
#endif
  public class InvalidDataBlockException : TransferException
  {
    public InvalidDataBlockException()
    {
    }

    public InvalidDataBlockException(string message) : base(message)
    {
    }

    public InvalidDataBlockException(string message, Exception inner) : base(message, inner)
    {
    }

#if !SILVERLIGHT
    protected InvalidDataBlockException(
      SerializationInfo info,
      StreamingContext context) : base(info, context)
    {
    }
#endif
  }

}
