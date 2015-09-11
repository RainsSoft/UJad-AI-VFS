using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Vfs.Auditing;
using Vfs.FileSystemService.Faults;

namespace Vfs.FileSystemServiceClient
{
  internal class Util
  {

    /// <summary>
    /// Invokes a given function and provides error handling and auditing in case the method causes
    /// an exception.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="auditor"></param>
    /// <param name="context"></param>
    /// <param name="func"></param>
    /// <param name="errorMessage"></param>
    /// <returns></returns>
    public static T SecureFunc<T>(IAuditor auditor, FileSystemTask context, Func<T> func, Func<string> errorMessage)
    {
      try
      {
        return func();
      }
//      catch (FaultException<ResourceFault> e)
//      {
//        throw; //TODO provide implementation
//      }
//      catch (FaultException e)
//      {
//        //unwrap exception details
//        throw; //TODO provide implementation
//      }
//      catch (CommunicationException e)
//      {
//        throw; //TODO provide implementation
//      }
      catch (VfsException e)
      {
        //just audit and rethrow VFS exceptions
        auditor.AuditException(e, context);
        throw;
      }
      catch (Exception e)
      {
        //wrap unhandled exception into VFS exception
        var exception = new ResourceAccessException(errorMessage(), e);
        auditor.AuditException(exception, context);
        throw exception;
      }
    }


    public static void SecureAction(IAuditor auditor, FileSystemTask context, Action action, Func<string> errorMessage)
    {
      //TODO replace wrapper once SecureFunc is properly implemented
      Func<bool> func = () =>
                          {
                            action();
                            return true;
                          };
      SecureFunc(auditor, context, () => func, errorMessage);
    }
  }
}
