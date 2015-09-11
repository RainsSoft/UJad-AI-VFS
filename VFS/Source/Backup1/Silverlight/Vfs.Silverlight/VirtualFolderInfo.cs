using System.Runtime.Serialization;

namespace Vfs
{
  /// <summary>
  /// Provides information about a given folder of the virtual file system.
  /// </summary>
  [DataContract]
  public class VirtualFolderInfo : VirtualResourceInfo
  {
    /// <summary>
    /// Whether the folder is the root of the file system or not (e.g. a drive in
    /// a regular file system).
    /// </summary>
    [DataMember]
    public bool IsRootFolder { get; set; }


    /// <summary>
    /// Indicates whether the folder is empty (does not contain
    /// any files or sub folders).
    /// </summary>
    [DataMember]
    public bool IsEmpty { get; set; }
  }
}