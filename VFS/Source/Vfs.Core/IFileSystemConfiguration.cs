using System;
using Vfs.Transfer;

namespace Vfs
{
  /// <summary>
  /// Encapsulates common settings for arbitrary file system providers.
  /// </summary>
  public interface IFileSystemConfiguration<TDownloadTransfer, TUploadTransfer>
    where TDownloadTransfer : ITransfer
    where TUploadTransfer : ITransfer
  {
    /// <summary>
    /// An artificial name that is returned as the <see cref="VirtualResourceInfo.Name"/>
    /// of file system root item (as returned by <see cref="IFileSystemProvider.GetFileSystemRoot"/>).
    /// This property can be set in order to mask the real folder name.
    /// </summary>
    string RootName { get; }

    /// <summary>
    /// A store that maintains running download transfers. If this property is not set, the
    /// provider will use a transient <see cref="InMemoryTransferStore{TDownloadTransfer}"/>.
    /// </summary>
    ITransferStore<TDownloadTransfer> DownloadStore { get; }

    /// <summary>
    /// A store that maintains running upload transfers. If this property is not set, the
    /// provider will use a transient <see cref="InMemoryTransferStore{TUploadTransfer}"/>.
    /// </summary>
    ITransferStore<TUploadTransfer> UploadStore { get; }

    /// <summary>
    /// The maximum download block size that can be requested via the
    /// <see cref="IDownloadTransferHandler.RequestDownloadToken(string,bool,int)"/>
    /// method.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">In case of an invalid value.</exception>
    int? MaxDownloadBlockSize { get; }

    /// <summary>
    /// The block size that is being applied for transfers if the
    /// client does not explicitly request blocks of a given size. This
    /// value is applied when requesting a download token via the
    /// <see cref="IDownloadTransferHandler.RequestDownloadToken(string,bool)"/>
    /// method.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">In case of an invalid value.</exception>
    int DefaultDownloadBlockSize { get; }

    /// <summary>
    /// The maximum size of data block that is being returned as part of an upload
    /// token that is requested via the  <see cref="IUploadTransferHandler.RequestUploadToken"/>
    /// method.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">In case of an invalid value.</exception>
    int? MaxUploadBlockSize { get; }

    /// <summary>
    /// Defines how long it takes until an issued download token expires.
    /// </summary>
    TimeSpan? DownloadTokenExpirationTime { get; }

    /// <summary>
    /// Defines how long it takes until an issued upload token expires.
    /// </summary>
    TimeSpan? UploadTokenExpirationTime { get; }
  }
}
