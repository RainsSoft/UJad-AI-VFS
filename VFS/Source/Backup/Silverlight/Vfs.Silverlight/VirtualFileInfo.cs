using System.Runtime.Serialization;

namespace Vfs
{
  /// <summary>
  /// Provides information about a given file of the virtual file system.
  /// </summary>
  [DataContract]
  public class VirtualFileInfo : VirtualResourceInfo
  {
    /// <summary>
    /// The file's content type.
    /// </summary>
    [DataMember]
    public string ContentType { get; set; }

    /// <summary>
    /// The size of the file in bytes. Returns 0 if no data is available yet.
    /// </summary>
    [DataMember]
    public long Length { get; set; }
  }
}