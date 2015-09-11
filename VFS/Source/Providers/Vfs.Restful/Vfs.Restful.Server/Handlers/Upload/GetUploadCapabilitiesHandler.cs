using OpenRasta.Web;
using Vfs.Transfer;

namespace Vfs.Restful.Server.Handlers.Upload
{
  public class GetUploadCapabilitiesHandler : UploadHandler
  {
    public GetUploadCapabilitiesHandler(IFileSystemProvider fileSystem)
      : base(fileSystem)
    {
    }

    /// <summary>
    /// Indicates how restrictively data blocks may be transmitted.
    /// </summary>
    [HttpOperation(HttpMethod.GET, ForUriName = "GetUploadCapabilities")]
    public virtual OperationResult<Wrapped<TransmissionCapabilities>> Get()
    {
      return SecureFunc(() => new Wrapped<TransmissionCapabilities>(Uploads.TransmissionCapabilities));
    }
  }
}