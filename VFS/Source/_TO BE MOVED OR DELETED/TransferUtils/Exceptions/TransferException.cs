using System;
using System.Runtime.Serialization;

namespace Vfs.Transfer
{
#if !SILVERLIGHT
  [Serializable]
#endif
  public class TransferException : Exception
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
  }
}