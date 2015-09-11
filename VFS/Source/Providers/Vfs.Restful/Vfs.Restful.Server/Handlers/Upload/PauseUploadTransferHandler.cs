using OpenRasta.Web;
using Vfs.Transfer;

namespace Vfs.Restful.Server.Handlers.Upload
{
  public class PauseUploadTransferHandler : UploadHandler
  {
    public PauseUploadTransferHandler(IFileSystemProvider fileSystem)
      : base(fileSystem)
    {
    }


    /// <summary>
    /// Tells the transfer service that transmission is being
    /// paused for an unknown period of time. This should keep
    /// the transfer enabled (including lock to protect the resource),
    /// but gives the service time to free or unlock resources.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <exception cref="UnknownTransferException">In case the <paramref name="transferId"/>
    /// does not refer to an active transfer.</exception>
    [HttpOperation(HttpMethod.POST, ForUriName = "PauseUploadTransfer")]
    public virtual OperationResult Post(string transferId)
    {
      return SecureAction(() => Uploads.PauseTransfer(transferId));
    }
  }
}