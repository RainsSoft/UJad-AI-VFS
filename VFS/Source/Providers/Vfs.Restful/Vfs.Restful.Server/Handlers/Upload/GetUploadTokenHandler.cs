using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vfs.Transfer;

namespace Vfs.Restful.Server.Handlers.Upload
{
  public class GetUploadTokenHandler : UploadHandler
  {
    public GetUploadTokenHandler(IFileSystemProvider fileSystem) : base(fileSystem)
    {
    }

    /// <summary>
    /// Requests a download token for a given resource.
    /// </summary>
    public OperationResult<UploadToken> Get(string filePath, bool overwrite, long fileLength, string contentType)
    {
      return SecureFunc(() => Uploads.RequestUploadToken(filePath, overwrite, fileLength, contentType));
    }
  }
}
