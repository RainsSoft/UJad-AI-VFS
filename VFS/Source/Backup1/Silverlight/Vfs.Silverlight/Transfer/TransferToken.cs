using System;



namespace Vfs.Transfer
{
  /// <summary>
  /// A shared token that refers to the transmission
  /// of a given resource.
  /// </summary>
  public abstract class TransferToken
  {
    /// <summary>
    /// Identifies the resource transfer.
    /// </summary>
    public string TransferId { get; set; }

    /// <summary>
    /// Identifies the resource to be transferred on the
    /// receiving end.
    /// </summary>
    public string ResourceIdentifier { get; set; }

    /// <summary>
    /// The public name of the transferred resource.
    /// </summary>
    public string ResourceName { get; set; }

    /// <summary>
    /// The length of the transferred file in bytes.
    /// </summary>
    public long ResourceLength { get; set; }

    /// <summary>
    /// The content type of the transferred resource.
    /// </summary>
    public string ContentType { get; set; }
  
    /// <summary>
    /// Timestamp defining the token's initialization. Can
    /// be used to clean up obsolete tokens.
    /// </summary>
    public DateTimeOffset CreationTime { get; set; }

    /// <summary>
    /// The expiration time of the token. It can be expected that
    /// the party that created the token will abort the transfer
    /// after this download request.<br/>
    /// A null value indicates the transfer never expires.
    /// </summary>
    public DateTimeOffset? ExpirationTime { get; set; }
  }
}
