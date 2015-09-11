using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenRasta.Web;
using Vfs.Transfer;

namespace Vfs.Restful.Server.Handlers.Download
{
  public class GetDownloadTokenHandler : DownloadHandler
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public GetDownloadTokenHandler(IFileSystemProvider fileSystem) : base(fileSystem)
    {
    }

    /// <summary>
    /// Requests a download token for a given resource, and specifies the
    /// maximum block size that is supported by the client.
    /// </summary>
    public OperationResult<DownloadToken> Get(string filePath, bool includeFileHash, int maxBlockSize)
    {
      return SecureFunc(() => Downloads.RequestDownloadToken(filePath, includeFileHash, maxBlockSize));
    }


    /// <summary>
    /// Requests a download token for a given resource.
    /// </summary>
    public OperationResult<DownloadToken> Get(string filePath, bool includeFileHash)
    {
      return SecureFunc(() => Downloads.RequestDownloadToken(filePath, includeFileHash));
    }

  }
}
