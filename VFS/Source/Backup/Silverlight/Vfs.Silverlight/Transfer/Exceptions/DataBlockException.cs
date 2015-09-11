using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Vfs.Transfer
{
  /// <summary>
  /// Thrown in case a request for a given data block
  /// could not be processed, e.g. because of an invalid block number.
  /// </summary>
#if !SILVERLIGHT
  [Serializable]
#endif
  public class DataBlockException : TransferException
  {
    /// <summary>
    /// The fault type can be used in order to transfer fault information
    /// in disconnected scenarios.
    /// </summary>
    public override VfsFaultType FaultType
    {
      get { return VfsFaultType.DataBlockError; }
    }


    public DataBlockException()
    {
    }

    public DataBlockException(string message) : base(message)
    {
    }

    public DataBlockException(string message, Exception inner) : base(message, inner)
    {
    }

#if !SILVERLIGHT
    protected DataBlockException(
      SerializationInfo info,
      StreamingContext context) : base(info, context)
    {
    }
#endif
  }

}
