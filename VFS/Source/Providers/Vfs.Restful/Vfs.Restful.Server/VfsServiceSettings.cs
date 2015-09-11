namespace Vfs.Restful.Server
{
  /// <summary>
  /// Encapsulats settings that are being used by the service.
  /// An instance of this class can be injected into the service handlers
  /// through the <see cref="ConfigurationHelper.RegisterSettings"/>
  /// method.
  /// </summary>
  public class VfsServiceSettings
  {
    /// <summary>
    /// The maximum download block size. Can be set in order to
    /// further limit the block size that is defined by the underlying
    /// <see cref="IFileSystemProvider"/>.
    /// </summary>
    public int? MaxDownloadBlockSize { get; set; }


    /// <summary>
    /// The maximum upload block size. Can be set in order to
    /// further limit the block size that is defined by the underlying
    /// <see cref="IFileSystemProvider"/>.
    /// </summary>
    public int? MaxUploadBlockSize { get; set; }


    /// <summary>
    /// The maximum size of a file that is being uploaded.
    /// </summary>
    public long? MaxFileUploadSize { get; set; }
  }
}
