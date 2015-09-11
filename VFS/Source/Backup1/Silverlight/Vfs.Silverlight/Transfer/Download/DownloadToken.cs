namespace Vfs.Transfer
{
  /// <summary>
  /// A token that was issued in order to download a given resource.
  /// </summary>
  public class DownloadToken : TransferToken
  {
    /// <summary>
    /// An optional MD5 hash, which can be used
    /// to verify integrity of the fully transmitted
    /// file data.<br/>
    /// If not set, this property may return null or
    /// <see cref="string.Empty"/>.
    /// </summary>
    public string Md5FileHash { get; set; }

    /// <summary>
    /// The encoding of the transferred data blocks.
    /// Defaults to null.
    /// </summary>
    public string ResourceEncoding { get; set; }

    /// <summary>
    /// The usual size of the downloaded blocks in bytes
    /// (the last block might be smaller, of course).
    /// </summary>
    public int DownloadBlockSize { get; set; }

    /// <summary>
    /// Gets the total number of blocks
    /// that need to be downloaded in order to get the
    /// whole resource.
    /// </summary>
    public long TotalBlockCount { get; set; }
  }
}