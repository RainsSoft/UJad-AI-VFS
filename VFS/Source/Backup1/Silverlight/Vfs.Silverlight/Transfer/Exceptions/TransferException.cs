using System;
using System.Runtime.Serialization;

namespace Vfs.Transfer
{
  /// <summary>
  /// Base class for transfer related exceptions.
  /// </summary>
#if !SILVERLIGHT
  [Serializable]
#endif
  public class TransferException : VfsException
  {

    public TransferException()
    {
    }

    public TransferException(string message) : base(message)
    {
    }

    public TransferException(string message, Exception inner) : base(message, inner)
    {
    }

#if !SILVERLIGHT
    protected TransferException(
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
      get { return VfsFaultType.TransferError; }
    }
  }
}