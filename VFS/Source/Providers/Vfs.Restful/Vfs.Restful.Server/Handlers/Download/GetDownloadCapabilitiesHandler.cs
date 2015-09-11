using OpenRasta.Web;
using Vfs.Transfer;

namespace Vfs.Restful.Server.Handlers.Download
{
  public class GetDownloadCapabilitiesHandler : DownloadHandler
  {
    public GetDownloadCapabilitiesHandler(IFileSystemProvider fileSystem) : base(fileSystem)
    {
    }

    /// <summary>
    /// Indicates how restrictively data blocks may be transmitted.
    /// </summary>
    [HttpOperation(HttpMethod.GET, ForUriName = "GetDownloadCapabilities")]
    public virtual OperationResult<Wrapped<TransmissionCapabilities>> Get()
    {
      return SecureFunc(() => new Wrapped<TransmissionCapabilities>(Downloads.TransmissionCapabilities));
    }
  }
}