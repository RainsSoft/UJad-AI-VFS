using System.Web;
using Vfs.Restful.Server.Resources;
using Vfs.Transfer;

namespace Vfs.Restful.Server.Handlers.Download
{
  public abstract class ReadFileHandlerBase : DownloadHandler
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    protected ReadFileHandlerBase(IFileSystemProvider fileSystem) : base(fileSystem)
    {
    }



    /// <summary>
    /// Makes sure that buffering is disabled and correct header values are set
    /// before streaming file data to the client. This method is invoked deferred
    /// when returning the resource in order to overcome some issues with OpenRasta.
    /// See the implementation in the handlers and the <see cref="FileDataResource"/>.
    /// </summary>
    /// <param name="token"></param>
    protected void SetResponseHeader(TransferToken token)
    {
      if (HttpContext.Current == null) return;

      //we need to disable buffering and set headers before streaming
      var resp = HttpContext.Current.Response;
      resp.BufferOutput = false;
      resp.AppendHeader("Content-Type", token.ContentType);
      resp.AppendHeader("Content-Disposition", "inline; filename=" + token.ResourceName);
      resp.AppendHeader("Content-Length", token.ResourceLength.ToString());
    }
  }
}