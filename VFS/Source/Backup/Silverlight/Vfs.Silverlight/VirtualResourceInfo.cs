using System;
using System.Runtime.Serialization;

namespace Vfs
{
  /// <summary>
  /// Provides information about an item (file or folder) of the virtual file system.
  /// </summary>
  [DataContract]
  public abstract class VirtualResourceInfo
  {
    /// <summary>
    /// The resource's local name.
    /// </summary>
    [DataMember(Order = 0)]
    public string Name { get; set; }

    /// <summary>
    /// The qualified name (path) of the resource.
    /// </summary>
    [DataMember(Order = 1)]
    public string FullName { get; set; }

    /// <summary>
    /// An additional description for the resource,
    /// if supported.
    /// </summary>
    [DataMember(Order = 2)]
    public string Description { get; set; }

    /// <summary>
    /// Indicates when the item was created. Returns null if
    /// the information is not available or not supported
    /// by the file system.
    /// </summary>
    [DataMember(Order = 3)]
    public DateTimeOffset? CreationTime { get; set; }

    /// <summary>
    /// The timestamp of the last update. Returns null if
    /// the information is not available or not supported
    /// by the file system.
    /// </summary>
    [DataMember(Order = 4)]
    public DateTimeOffset? LastWriteTime { get; set; }

    /// <summary>
    /// The timestamp of the last access on the resource. Returns null if
    /// the information is not available or not supported
    /// by the file system.
    /// </summary>
    [DataMember(Order = 5)]
    public DateTimeOffset? LastAccessTime { get; set; }


    /// <summary>
    /// Indicates whether the file on the file system is
    /// read-only or not (regardless of the permissions provided
    /// by the VFS framework).
    /// </summary>
    [DataMember(Order = 6)]
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// Indicates whether the file on the file system is
    /// hidden or not (regardless of the permissions provided
    /// by the VFS framework).<br/>
    /// If this flag is true, a file provider is still
    /// supposed to deliver the resource info. It's up to the
    /// client that retrieves the resource to hide it
    /// or show it differently to the user.
    /// </summary>
    [DataMember(Order = 7)]
    public bool IsHidden { get; set; }


    /// <summary>
    /// The path of the resource's parent folder. If the
    /// resource represents the file system root, a
    /// null reference is being returned.
    /// </summary>
    [DataMember(Order = 7)]
    public string ParentFolderPath { get; set; }
  }
}
