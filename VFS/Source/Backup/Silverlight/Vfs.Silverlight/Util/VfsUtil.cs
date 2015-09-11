using System;
using Vfs.Auditing;

namespace Vfs.Util
{
  public static class VfsUtil
  {
    /// <summary>
    /// Provides exception handling and auditing for a given function.
    /// </summary>
    /// <param name="task">The context, used for auditing exceptions that may occur.</param>
    /// <param name="func">The function to be invoked.</param>
    /// <param name="errorMessage">Returns an error message in case of an unhandled exception
    /// that is not derived from <see cref="VfsException"/>.</param>
    /// <param name="auditor">The auditor that receives the exceptions.</param>
    /// <returns>The result of the submitted <paramref name="func"/> function.</returns>
    public static T SecureFunc<T>(FileSystemTask task, Func<T> func, Func<string> errorMessage, IAuditor auditor)
    {
      try
      {
        return func();
      }
      catch (VfsException e)
      {
        //just audit and rethrow VFS exceptions
        auditor.AuditException(e, task);
        throw;
      }
      catch (Exception e)
      {
        //wrap unhandled exception into VFS exception
        var exception = new ResourceAccessException(errorMessage(), e);
        auditor.AuditException(exception, task);
        throw exception;
      }
    }


    /// <summary>
    /// Provides exception handling and auditing for a given function.
    /// </summary>
    /// <param name="task">The context, used for auditing exceptions that may occur.</param>
    /// <param name="action">The action to be invoked.</param>
    /// <param name="errorMessage">Returns an error message in case of an unhandled exception
    /// that is not derived from <see cref="VfsException"/>.</param>
    /// <param name="auditor">The auditor that receives the exceptions.</param>
    /// <returns>The result of the submitted <paramref name="action"/> function.</returns>
    public static void SecureAction(FileSystemTask task, Action action, Func<string> errorMessage, IAuditor auditor)
    {
      try
      {
        action();
      }
      catch (VfsException e)
      {
        //just audit and rethrow VFS exceptions
        auditor.AuditException(e, task);
        throw;
      }
      catch (Exception e)
      {
        //wrap unhandled exception into VFS exception
        var exception = new ResourceAccessException(errorMessage(), e);
        auditor.AuditException(exception, task);
        throw exception;
      }
    }
  }
}
