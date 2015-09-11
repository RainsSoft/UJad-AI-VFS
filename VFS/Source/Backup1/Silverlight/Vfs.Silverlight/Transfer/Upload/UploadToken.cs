using System;

namespace Vfs.Transfer
{
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
    public int? MaxBlockSize { get; set; }
  }
}