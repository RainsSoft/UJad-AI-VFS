using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenRasta.Web;
using Vfs.Util;

namespace Vfs.Restful.Server.Handlers
{
  public abstract class VfsHandlerBase
  {
    /// <summary>
    /// Gets the decorated file system.
    /// </summary>
    public IFileSystemProvider FileSystem
    {
      get; private set;
    }

    /// <summary>
    /// Gets the communication context that is associated with the
    /// request. This property is set via dependency injection.
    /// </summary>
    public ICommunicationContext Context { get; set; }

    protected IRequest Request
    {
      get { return Context.Request; }
    }

    protected  IResponse Response
    {
      get { return Context.Response; }
    }


    /// <summary>
    /// Initializes the handler class with the file system
    /// provider that receives incoming requests.
    /// </summary>
    protected VfsHandlerBase(IFileSystemProvider fileSystem)
    {
      Ensure.ArgumentNotNull(fileSystem, "fileSystem");
      FileSystem = fileSystem;
    }



    protected OperationResult<T> SecureFunc<T>(Func<T> func) where T:class
    {
      try
      {
        T item = func();
        return CreateResult(item);
      }
      catch (Exception e)
      {
        //create VFS exception result
        return CreateFaultResult(e, (statusCode, fault) => new OperationResult<T>(statusCode) { ResponseResource = fault });
      }
    }


    protected OperationResult SecureAction(Action action)
    {
      try
      {
        action();
        return new OperationResult.OK();
      }
      catch (Exception e)
      {
        //create VFS exception result
        return CreateFaultResult(e, (statusCode, fault) => new OperationResult<object>(statusCode) {ResponseResource = fault});
      }
    }


    protected T CreateFaultResult<T>(Exception exception, Func<int, VfsFault, T> func) where T:OperationResult
    {
      //set content type
      //Response.Headers.ContentType = new MediaType(VfsHttpHeaders.Default.VfsFaultContentType);

      //get exception
      VfsException ve = exception as VfsException;
      VfsFault fault = ve == null ? new VfsFault { Message = exception.Message} 
                                  : VfsFault.CreateFromException(ve);
      
      
      int statusCode = 0;

      //set HTTP status code
      switch (fault.FaultType)
      {
        case VfsFaultType.ResourceNotFound:
        case VfsFaultType.TransferUnknown:
          //404
          return func(404, fault);
        case VfsFaultType.ResourceAccess:
        case VfsFaultType.ResourceOverwrite:
        case VfsFaultType.ResourceLocked:
        case VfsFaultType.TransferError:
        case VfsFaultType.TransferStatusError:
        case VfsFaultType.DataBlockError:
        case VfsFaultType.ResourcePathInvalid:
          //forbidden
          return func(403, fault);
        case VfsFaultType.Undefined:
          return func(500, fault);
        default:
          string msg = String.Format("Exception contains unknown fault type [{0}]", fault.FaultType);
          throw new ArgumentOutOfRangeException("exception", msg);
      }      
    }

    protected OperationResult<T> CreateResult<T>(T item) where T:class
    {
      return new OperationResult<T> {Item = item, StatusCode = 200};
    }
  }
}
