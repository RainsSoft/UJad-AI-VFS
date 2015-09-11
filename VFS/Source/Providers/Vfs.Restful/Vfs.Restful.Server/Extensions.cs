using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using OpenRasta.Web;

namespace Vfs.Restful.Server
{
  public static class Extensions
  {
    /// <summary>
    /// Sets a header in both self-hosted and ASP.net environments. Ensures
    /// that header values are sent to the client in ASP.net by setting the
    /// header values directly to the <see cref="HttpResponse"/> of the
    /// current <see cref="HttpContext"/>.
    /// </summary>
    public static void SetHeader(this IResponse response, string key, string value)
    {
      if (HttpContext.Current != null)
      {
        //write header to context - using OR's Headers collection
        //doesn't send the headers to the client
        HttpContext.Current.Response.AppendHeader(key, value);
      }
      else
      {
        //use OR's API in self-hosted environments
        response.Headers.Add(key, value);
      }
    }


    /// <summary>
    /// Sets a header in both self-hosted and ASP.net environments. Ensures
    /// that header values are sent to the client in ASP.net by setting the
    /// header values directly to the <see cref="HttpResponse"/> of the
    /// current <see cref="HttpContext"/>.
    /// </summary>
    public static void SetHeader(this IHttpEntity entity, string key, string value)
    {
      if (HttpContext.Current != null)
      {
        //write header to context - using OR's Headers collection
        //doesn't send the headers to the client
        HttpContext.Current.Response.AppendHeader(key, value);
      }
      else
      {
        //use OR's API in self-hosted environments
        entity.Headers.Add(key, value);
      }
    }
  }
}
