namespace Vfs.Transfer
{
  /// <summary>
  /// A partial block of data of a given transfer.
  /// </summary>
  public class BufferedDataBlock : DataBlockInfo
  {
    /// <summary>
    /// The submitted file data, which should match the
    /// <see cref="DataBlockInfo.BlockLength"/> property. In case of an upload
    /// scenario, the block size should not exceed the maximum
    /// block size as indicated by the <see cref="UploadToken.MaxBlockSize"/>
    /// property.
    /// </summary>
    public byte[] Data { get; set; }
  }
}
