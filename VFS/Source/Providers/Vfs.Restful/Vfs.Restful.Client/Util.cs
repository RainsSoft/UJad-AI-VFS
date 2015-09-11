using System;
using System.Runtime.Serialization;
using Microsoft.Http;
using Vfs.Auditing;
using Vfs.Exceptions;


namespace Vfs.Restful.Client
{
  internal class Util
  {
    public static T Get<T>(string baseUri, string actionUri)
    {
      return GetResource<T>(baseUri, c => c.Get(actionUri));
    }

    public static T Post<T>(string baseUri, string actionUri, HttpContent content)
    {
      return GetResource<T>(baseUri, c =>
                                       {
                                         if (HttpContent.IsNullOrEmpty(content)) c.DefaultHeaders.ContentLength = 0;
                                         return c.Post(actionUri, content);
                                       });
    }

    public static T Put<T>(string baseUri, string actionUri, HttpContent content)
    {
      return GetResource<T>(baseUri, c =>
                                       {
                                         if(HttpContent.IsNullOrEmpty(content)) c.DefaultHeaders.ContentLength = 0;
                                         return c.Put(actionUri, content);
                                       });
    }

    public static void Delete(string baseUri, string actionUri)
    {
      RunRequest(baseUri, c => c.Delete(actionUri));
    }



    /// <summary>
    /// Runs an HTTP request and returns the returned response.
    /// </summary>
    /// <param name="baseUri"></param>
    /// <param name="serviceInvocation"></param>
    /// <returns></returns>
    public static HttpResponseMessage RunRequest(string baseUri, Func<HttpClient, HttpResponseMessage> serviceInvocation)
    {
      //create HttpClient
      HttpClient client = new HttpClient(baseUri);
      
      //get response
      HttpResponseMessage response = serviceInvocation(client);

      //if the content type is application/vfs-fault, parse the fault into an exception
      if (String.Equals(response.Content.ContentType, VfsFault.FaultContentType,
                        StringComparison.InvariantCultureIgnoreCase))
      {
        //deserialize fault and wrap into exception
        var fault = response.Content.ReadAsDataContract<VfsFault>();
        throw new VfsFaultException(fault.Message, fault);
      }

      return response.EnsureStatusIsSuccessful();
    }



    /// <summary>
    /// Runs a requests and converts the response into a given data contract.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="baseUri"></param>
    /// <param name="serviceInvocation"></param>
    /// <returns></returns>
    public static T GetResource<T>(string baseUri, Func<HttpClient, HttpResponseMessage> serviceInvocation)
    {
      var response = RunRequest(baseUri, serviceInvocation);

      //if status is ok, parse content as data contract
      return response.Content.ReadAsDataContract<T>();
    }



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
      catch (VfsFaultException e)
      {
        //audit as warning - the error has been properly handled by the service
        VfsFault fault = e.Fault;
        auditor.AuditException(e, AuditLevel.Warning, context, AuditEvent.Unknown, fault.CreateFaultMessage());

        //create a matching exception based on the fault type
        throw fault.ToException();
      }
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
      SecureFunc(auditor, context, func, errorMessage);
    }
  }
}