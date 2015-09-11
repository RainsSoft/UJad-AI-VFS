using System.IO;
using Vfs.Locking;

namespace Vfs.Transfer
{
  /// <summary>
  /// Provides the context for a given download from
  /// the file system service to a client.
  /// </summary>
  internal class DownloadTransfer
  {
    /// <summary>
    /// The underlying download token, which was also
    /// sent to the sender.
    /// </summary>
    public DownloadToken Token { get; private set; }

    /// <summary>
    /// Contains the resource locks which is used to ensure access to
    /// the resource. Also used to abort a transfer if the
    /// lock expired.
    /// </summary>
    public ResourceLockGuard ResourceLock { get; set; }

    /// <summary>
    /// The public transfer ID. This is a convenience
    /// property which just returns the <see cref="TransferToken.Status"/>
    /// of the underlying <see cref="Token"/>.
    /// </summary>
    public TransferStatus Status
    {
      get { return Token.Status; }
    }


    /// <summary>
    /// The public transfer ID. This is a convenience
    /// property which just returns the <see cref="TransferToken.TransferId"/>
    /// of the underlying <see cref="Token"/>.
    /// </summary>
    public string TransferId
    {
      get { return Token.TransferId; }
    }
    
    /// <summary>
    /// Synchronization token.
    /// </summary>
    public object SyncRoot { get; private set; }

    /// <summary>
    /// The processed file.
    /// </summary>
    public FileInfo File { get; set; }

    /// <summary>
    /// The processed file stream. Created on demand.
    /// </summary>
    public FileStream Stream { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public DownloadTransfer(DownloadToken token)
    {
      Token = token;
      SyncRoot = new object();
    }
  }
}