namespace Vfs.Transfer
{
  /// <summary>
  /// Provides the context for a given download from
  /// the file system service to a client.
  /// </summary>
  public class DownloadTransfer<TFile> : TransferBase<TFile, DownloadToken> where TFile : IVirtualFileItem
  {
    /// <summary>
    /// Whether the transfer should clean up its resources after having
    /// delivered the last block without waiting for an explicit request
    /// to clean up. Defaults to <c>false</c>.
    /// </summary>
    public bool AutoCloseAfterLastBlockDelivery { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public DownloadTransfer(DownloadToken token, TFile fileItem) : base(token, fileItem)
    {
    }
  }
}