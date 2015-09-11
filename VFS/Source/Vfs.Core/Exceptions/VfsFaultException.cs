using System;
using System.Runtime.Serialization;

namespace Vfs
{
#if !SILVERLIGHT
  [Serializable]
#endif
  public class VfsFaultException : VfsException
  {
    public VfsFault Fault { get; set; }

    public VfsFaultException()
    {
    }

    public VfsFaultException(string message, VfsFault fault)
      : base(message)
    {
      Fault = fault;
    }

    public VfsFaultException(string message, VfsFault fault, Exception inner) : base(message, inner)
    {
      Fault = fault;
    }


#if !SILVERLIGHT
    protected VfsFaultException(
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
      get { return Fault == null ? VfsFaultType.Undefined : Fault.FaultType; }
    }
  }
}
