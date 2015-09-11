using System;
using Vfs.Transfer;

namespace Vfs
{
  /// <summary>
  /// Encapsulates common settings for arbitrary file system providers.
  /// </summary>
  public class FileSystemConfiguration<TDownloadTransfer, TUploadTransfer> : IFileSystemConfiguration<TDownloadTransfer, TUploadTransfer>
    where TDownloadTransfer : ITransfer
    where TUploadTransfer : ITransfer
  {
    private string rootName;

    /// <summary>
    /// An artificial name that is returned as the <see cref="VirtualResourceInfo.Name"/>
    /// of file system root item (as returned by <see cref="IFileSystemProvider.GetFileSystemRoot"/>).
    /// This property can be set in order to mask the real folder name.
    /// </summary>
    public virtual string RootName
    {
      get { return rootName; }
      set
      {
        Ensure.ArgumentNotNull(value, "value");
        rootName = value;
      }
    }

    private ITransferStore<TDownloadTransfer> downloadStore;

    /// <summary>
    /// A store that maintains running download transfers. If this property is not set, the
    /// provider will use a transient <see cref="InMemoryTransferStore{TTransfer}"/>.
    /// </summary>
    public virtual ITransferStore<TDownloadTransfer> DownloadStore
    {
      get { return downloadStore; }
      set
      {
        Ensure.ArgumentNotNull(value, "value");
        downloadStore = value;
      }
    }

    private ITransferStore<TUploadTransfer> uploadStore;

    /// <summary>
    /// A store that maintains running upload transfers. If this property is not set, the
    /// provider will use a transient <see cref="InMemoryTransferStore{TTransfer}"/>.
    /// </summary>
    public virtual ITransferStore<TUploadTransfer> UploadStore
    {
      get { return uploadStore; }
      set
      {
        Ensure.ArgumentNotNull(value, "value");
        uploadStore = value;
      }
    }

    private int? maxDownloadBlockSize;

    /// <summary>
    /// The maximum download block size (in bytes) that can be requested via the
    /// <see cref="IDownloadTransferHandler.RequestDownloadToken(string,bool,int)"/>
    /// method.
    /// </summary>
    /// <remarks>There is no validation that compares this value to the <see cref="DefaultDownloadBlockSize"/>.
    /// It is trusted that implementors are capable of submitting valid values, or validate
    /// a submitted configuration object in the provider that receives the configuration.</remarks>
    public virtual int? MaxDownloadBlockSize
    {
      get { return maxDownloadBlockSize; }
      set
      {
        if (value.HasValue) ValidateMinBlockSize(value.Value);
        maxDownloadBlockSize = value;
      }
    }

    private int defaultDownloadBlockSize = 32768;

    /// <summary>
    /// The block size (in bytes) that is being applied for transfers if the
    /// client does not explicitly request blocks of a given size. This
    /// value is applied when requesting a download token via the
    /// <see cref="IDownloadTransferHandler.RequestDownloadToken(string,bool)"/>
    /// method.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">If a value below 10240 bytes
    /// (10 KB) is set.</exception>
    /// <remarks>There is no validation that compares this value to the <see cref="MaxDownloadBlockSize"/>.
    /// It is trusted that implementors are capable of submitting valid values, or validate
    /// a submitted configuration object in the provider that receives the configuration.</remarks>
    public virtual int DefaultDownloadBlockSize
    {
      get { return defaultDownloadBlockSize; }
      set
      {
        ValidateMinBlockSize(value);
        defaultDownloadBlockSize = value;
      }
    }

    private int? maxUploadBlockSize;

    /// <summary>
    /// The maximum size (in bytes) of data block that is being returned as part of an
    /// token that is requested via the  <see cref="IUploadTransferHandler.RequestUploadToken"/>
    /// method.
    /// </summary>
    public virtual int? MaxUploadBlockSize
    {
      get { return maxUploadBlockSize; }
      set
      {
        if (value.HasValue) ValidateMinBlockSize(value.Value);
        maxUploadBlockSize = value;
      }
    }

    /// <summary>
    /// The maximum size (in bytes) of a file that can be uploaded to the file system. This value
    /// is supposed to be validated whenever an upload token is being submitted. A value of
    /// null indicates there is not file limit.
    /// </summary>
    public long? MaxUploadFileSize { get; set; }


    /// <summary>
    /// Defines how long it takes until an issued download token expires.
    /// </summary>
    public virtual TimeSpan? DownloadTokenExpirationTime { get; set; }

    /// <summary>
    /// Defines how long it takes until an issued upload token expires.
    /// </summary>
    public virtual TimeSpan? UploadTokenExpirationTime { get; set; }


    /// <summary>
    /// Makes sure any assigned block size is not smaller than
    /// 10 KB.
    /// </summary>
    /// <param name="value"></param>
    private static void ValidateMinBlockSize(int value)
    {
      if (value < 1024 * 10)
      {
        string msg = "Invalid download block size of {0} bytes specified. The minimum is 10240 bytes (10 KB).";
        msg = String.Format(msg, value);
        throw new ArgumentOutOfRangeException("value", msg);
      }
    }

  }
}