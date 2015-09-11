using System;
using System.Runtime.Serialization;

namespace Vfs.Transfer
{
  /// <summary>
  /// Thrown if the transfer service does not or no longer maintains
  /// a given transfer token.
  /// </summary>
#if !SILVERLIGHT
  [Serializable]
#endif
  public class UnknownTransferException : TransferException
  {
    public UnknownTransferException()
    {
    }

    public UnknownTransferException(string message) : base(message)
    {
    }

    public UnknownTransferException(string message, Exception inner) : base(message, inner)
    {
    }

#if !SILVERLIGHT
    protected UnknownTransferException(
      SerializationInfo info,
      StreamingContext context) : base(info, context)
    {
    }
#endif


    /// <summary>
    /// The fault type can be used in order to transfer fault information
    /// in disconnected scenarios.
    /// </summary>
    public override VfsFaultType FaultType
    {
      get { return VfsFaultType.TransferUnknown; }
    }
  }
}