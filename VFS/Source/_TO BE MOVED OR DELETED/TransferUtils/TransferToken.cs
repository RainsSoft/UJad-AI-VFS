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
    /// The current transfer state.
    /// </summary>
    public TransferStatus Status { get; set; }

    /// <summary>
    /// Indicates why a transfer was aborted. This property
    /// is only set if the <see cref="Status"/> property
    /// is <see cref="TransferStatus.Aborted"/>.
    /// </summary>
    public AbortReason? AbortReason { get; set; }
   
    /// <summary>
    /// Timestamp defining the token's initialization. Can
    /// be used to clean up obsolete tokens.
    /// </summary>
    public DateTimeOffset CreationTime { get; set; }

    /// <summary>
    /// The last transmitted block. Can be used for bookkeeping
    /// and simplified auditing.
    /// </summary>
    public DataBlockInfo LastTransmittedBlockInfo { get; set; }

    /// <summary>
    /// The timestamp of the last sent or received block.
    /// </summary>
    public DateTimeOffset? LastBlockTransmissionTime { get; set; }

    /// <summary>
    /// The timestamp of the transfer completion.
    /// </summary>
    public DateTimeOffset? CompletionTime { get; set; }

    /// <summary>
    /// The expiration time of the token. It can be expected that
    /// the party that created the token will abort the transfer
    /// after this download request.
    /// </summary>
    public DateTimeOffset? ExpirationTime { get; set; }
  }


  /// <summary>
  /// A token that was issued in order to download a given resource.
  /// </summary>
  public class DownloadToken : TransferToken
  {
    /// <summary>
    /// The content type of the transferred resource.
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// The usual size of the downloaded blocks in bytes
    /// (the last block might be smaller, of course). Returns
    /// null in case of variable block sizes.
    /// </summary>
    public long? DownloadBlockSize { get; set; }

    /// <summary>
    /// An optional MD5 hash, which can be used
    /// to verify integrity of the fully transmitted
    /// file data.<br/>
    /// If not set, this property may return null or
    /// <see cref="String.Empty"/>.
    /// </summary>
    public string Md5FileHash { get; set; }

    /// <summary>
    /// The encoding of the transferred data blocks.
    /// Defaults to null.
    /// </summary>
    public string ResourceEncoding { get; set; }

    /// <summary>
    /// If the transfer service is capable of telling,
    /// this property provides the total number of blocks
    /// that need to be downloaded in order to get the
    /// whole resource. Usually, this value could also be
    /// calculated through the <see cref="TransferToken.ResourceLength"/>
    /// property along with the <see cref="DownloadBlockSize"/> property.
    /// </summary>
    public long? TotalBlockCount { get; set; }
  }



  /// <summary>
  /// A token that was issued based on a request
  /// to upload a resource.
  /// </summary>
  public class UploadToken : TransferToken
  {
    /// <summary>
    /// The maximum size of the whole resource. If not set,
    /// the maxium size is not limited.
    /// </summary>
    public long? MaxResourceSize { get; set; }

    /// <summary>
    /// The maximum suggested block size, which should not be
    /// exceeded. If not set, there is no block size maxium.
    /// </summary>
    public long? MaxBlockSize { get; set; }

    /// <summary>
    /// The position of the next block to be transferred. Can be used in order
    /// to properly resume a paused transfer.
    /// </summary>
    /// <remarks>Not necessary for download scenarios - once we've downloaded
    /// a block, the block indicates its offset within the resource.</remarks>
    public long NextBlockOffset { get; set; }

    /// <summary>
    /// The number of received or sent <see cref="BufferedDataBlock"/> instances,
    /// which is also the zero based index of the next block to be requested
    /// from the repository in case of strictly sequential
    /// </summary>
    public long TransmittedBlockCount { get; set; }
  }

 
}
