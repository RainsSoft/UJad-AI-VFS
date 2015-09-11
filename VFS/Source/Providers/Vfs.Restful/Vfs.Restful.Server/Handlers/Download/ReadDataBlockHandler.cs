using Vfs.Transfer;

namespace Vfs.Restful.Server.Handlers.Download
{
  public class ReadDataBlockHandler : DownloadHandler
  {
    public ReadDataBlockHandler(IFileSystemProvider fileSystem) : base(fileSystem)
    {
    }


    /// <summary>
    /// Reads a block via a streaming channel, which enables a more resource friendly
    /// data transmission (compared to sending the whole data of the block at once).
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <param name="blockNumber">The number of the requested block.</param>
    /// <returns>A data block which contains the data as an in-memory buffer
    /// (<see cref="BufferedDataBlock.Data"/>).</returns>
    public OperationResult<StreamedDataBlock> Get(string transferId, long blockNumber)
    {
      return SecureFunc(() => Downloads.ReadBlockStreamed(transferId, blockNumber));
    }
  }
}