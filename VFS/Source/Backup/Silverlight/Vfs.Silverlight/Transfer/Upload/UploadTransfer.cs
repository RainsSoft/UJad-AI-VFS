namespace Vfs.Transfer.Upload
{
  /// <summary>
  /// Provides the context for a given upload from the client up to the
  /// file system.
  /// </summary>
  public class UploadTransfer<TFile> : TransferBase<TFile, UploadToken> where TFile : IVirtualFileItem
  {
    /// <summary>
    /// This is a flag that is being set during initialization. It indicates
    /// whether the has been started already or not. This flag is set to true
    /// as soon as the first chunk is being written to the target file.
    /// </summary>
    public bool HasUploadStarted { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public UploadTransfer(UploadToken token, TFile fileItem)
      : base(token, fileItem)
    {
    }
  }

}