using System;
using Vfs.Transfer;

namespace Vfs.Restful.Server.Handlers
{
  public abstract class MaxBlockSizeHandler<T> : VfsHandlerBase where T:TransferToken
  {
    /// <summary>
    /// The service settings, which are being injected automatically
    /// via dependency injection, if available.
    /// </summary>
    public VfsServiceSettings Settings { get; set; }


    /// <summary>
    /// Initializes the handler class with the file system
    /// provider that receives incoming requests.
    /// </summary>
    protected MaxBlockSizeHandler(IFileSystemProvider fileSystem) : base(fileSystem)
    {
    }

    /// <summary>
    /// Gets the maximum block size that can be transferred, if such
    /// a limit exists. Returns null in case of arbitrary block sizes.
    /// </summary>
    protected  Wrapped<int?> GetMaxBlockSize(ITransferHandler<T> transferHandler)
    {
      //get the minimum of the configured block size and the file system block size
      //(which are both optional)
      int? settingsValue = Settings == null ? null : GetSettingsMaxBlockSize(Settings);
      if (settingsValue.HasValue)
      {
        int blockSize = Math.Min(settingsValue.Value, transferHandler.MaxBlockSize ?? int.MaxValue);
        return new Wrapped<int?>(blockSize);
      }

      return new Wrapped<int?>(transferHandler.MaxBlockSize);
    }


    /// <summary>
    /// Gets the maximum block size for this kind of transfers.
    /// </summary>
    protected abstract int? GetSettingsMaxBlockSize(VfsServiceSettings settings);
  }
}