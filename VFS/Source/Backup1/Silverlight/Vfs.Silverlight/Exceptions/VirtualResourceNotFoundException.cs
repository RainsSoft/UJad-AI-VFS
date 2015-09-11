using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Vfs.Auditing;
using Vfs.Locking;
using Vfs.Security;
using Vfs.Transfer;
using Vfs.Util;


namespace Vfs
{
#if !SILVERLIGHT
  [Serializable]
#endif
  public class VirtualResourceNotFoundException : VfsException
  {
    /// <summary>
    /// The fault type can be used in order to transfer fault information
    /// in disconnected scenarios.
    /// </summary>
    public override VfsFaultType FaultType
    {
      get { return VfsFaultType.ResourceNotFound; }
    }


    /// <summary>
    /// The resource in question, if any.
    /// </summary>
    public VirtualResourceInfo Resource { get; set; }

    public VirtualResourceNotFoundException()
    {
    }

    public VirtualResourceNotFoundException(string message) : base(message)
    {
    }

    public VirtualResourceNotFoundException(string message, Exception inner) : base(message, inner)
    {
    }

#if !SILVERLIGHT
    protected VirtualResourceNotFoundException(
        SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
    }
#endif


  }
}
