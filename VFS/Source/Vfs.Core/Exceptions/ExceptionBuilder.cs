using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Vfs.Transfer;

namespace Vfs.Exceptions
{
  /// <summary>
  /// Helper class that assists in creating verbose
  /// exceptions for common FS and VFS related
  /// error conditions.
  /// </summary>
  public static class ExceptionBuilder
  {
    public static VfsException ToException(this VfsFault fault)
    {
      VfsException exception;

      switch (fault.FaultType)
      {
        case VfsFaultType.ResourceNotFound:
          exception = new VirtualResourceNotFoundException(fault.CreateFaultMessage());
          break;
        case VfsFaultType.ResourceAccess:
          exception = new ResourceAccessException(fault.CreateFaultMessage());
          break;
        case VfsFaultType.ResourceOverwrite:
          exception = new ResourceOverwriteException(fault.CreateFaultMessage());
          break;
        case VfsFaultType.ResourceLocked:
          exception = new ResourceLockedException(fault.CreateFaultMessage());
          break;
        case VfsFaultType.ResourcePathInvalid:
          exception = new InvalidResourcePathException(fault.CreateFaultMessage());
          break;
        case VfsFaultType.TransferError:
          exception = new TransferException(fault.CreateFaultMessage());
          break;
        case VfsFaultType.TransferUnknown:
          exception = new UnknownTransferException(fault.CreateFaultMessage());
          break;
        case VfsFaultType.TransferStatusError:
          exception = new TransferStatusException(fault.CreateFaultMessage());
          break;
        case VfsFaultType.DataBlockError:
          exception = new DataBlockException(fault.CreateFaultMessage());
          break;
        case VfsFaultType.Undefined:
          exception = new VfsFaultException(fault.CreateFaultMessage(), fault);
          break;
        default:
          Debug.WriteLine("Unsupported VFS fault type: " + fault.FaultType);
          exception = new VfsFaultException(fault.CreateFaultMessage(), fault);
          break;
      }

      exception.EventId = fault.EventId;
      return exception;
    }
  }
}
