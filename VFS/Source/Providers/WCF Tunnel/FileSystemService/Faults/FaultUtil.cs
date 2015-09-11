using System;
using System.ServiceModel;
using Vfs.Auditing;

namespace Vfs.FileSystemService.Faults
{
  public static class FaultUtil
  {
    internal static void SecureAction(FileSystemTask context, Action action)
    {
      try
      {
        action();
      }
      catch (ResourceAccessException e)
      {
        var fault = new ResourceFault { Context = context, FaultType = ResourceFaultType.ResourceAccess, Message = e.Message, EventId = e.EventId };
        throw CreateFault(fault, e);
      }
      catch (VirtualResourceNotFoundException e)
      {
        var fault = new ResourceFault { Context = context, FaultType = ResourceFaultType.ResourceNotFound, Message = e.Message, EventId = e.EventId };
        throw CreateFault(fault, e);
      }
      catch (ResourceOverwriteException e)
      {
        var fault = new ResourceFault { Context = context, FaultType = ResourceFaultType.ResourceOverwrite, Message = e.Message, EventId = e.EventId };
        throw CreateFault(fault, e);
      }
      catch (Exception e)
      {
        var fault = new ResourceFault { Context = context, FaultType = ResourceFaultType.Undefined, Message = e.Message };
        throw CreateFault(fault, e);
      }
    }



    internal static T SecureFunc<T>(FileSystemTask context, Func<T> func)
    {
      try
      {
        return func();
      }
      catch (ResourceAccessException e)
      {
        var fault = new ResourceFault { Context = context, FaultType = ResourceFaultType.ResourceAccess, Message = e.Message, EventId = e.EventId };
        throw CreateFault(fault, e);
      }
      catch (VirtualResourceNotFoundException e)
      {
        var fault = new ResourceFault { Context = context, FaultType = ResourceFaultType.ResourceNotFound, Message = e.Message, EventId = e.EventId };
        throw CreateFault(fault, e);
      }
      catch (ResourceOverwriteException e)
      {
        var fault = new ResourceFault { Context = context, FaultType = ResourceFaultType.ResourceOverwrite, Message = e.Message, EventId = e.EventId };
        throw CreateFault(fault, e);
      }
      catch (Exception e)
      {
        var fault = new ResourceFault { Context = context, FaultType = ResourceFaultType.Undefined, Message = e.Message };
        throw CreateFault(fault, e);
      }
    }



    public static FaultException<T> CreateFault<T>(T detail, Exception exception) where T:ResourceFault
    {
      detail.Message = exception.Message;
      return new FaultException<T>(detail, new FaultReason(exception.Message), new FaultCode(detail.FaultType.ToString()));
    }
  }
}
