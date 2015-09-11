using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenRasta.OperationModel;
using OpenRasta.OperationModel.Interceptors;
using OpenRasta.Web;

namespace Vfs.Restful.Server.Handlers
{
  /// <summary>
  /// Catches runtime exceptions, and wraps them into dedicated faults
  /// that are returned to the invoking client.
  /// </summary>
  public class VfsExceptionInterceptor : OperationInterceptor
  {
    public ICommunicationContext Context { get; set; }


    public VfsExceptionInterceptor(ICommunicationContext context)
    {
      Context = context;
    }


    public override Func<IEnumerable<OutputMember>> RewriteOperation(Func<IEnumerable<OutputMember>> operationBuilder)
    {
      return () =>
               {
                 try
                 {
                   return operationBuilder();
                 }
                 catch (Exception e)
                 {
                   if (Context != null)
                   {
                     Context.OperationResult = new OperationResult.NotFound()
                                                 {ResponseResource = "hello world"};
                   }
                   throw;
                 }

               };
    }

  }
}