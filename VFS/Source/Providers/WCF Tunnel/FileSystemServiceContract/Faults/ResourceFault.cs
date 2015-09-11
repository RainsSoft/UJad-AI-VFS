using System.Runtime.Serialization;
using Vfs.Auditing;

namespace Vfs.FileSystemService.Faults
{
  /// <summary>
  /// Common fault contract which is used to send
  /// VFS fault information over the wire.
  /// </summary>
  [DataContract(Name="ResourceFault")]
  public class ResourceFault
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public ResourceFault()
    {
      FaultType = ResourceFaultType.Undefined;
      Context = FileSystemTask.Undefined;
    }

    /// <summary>
    /// The executed file system task that was running when
    /// the fault occurred.
    /// </summary>
    [DataMember]
    public FileSystemTask Context { get; set; }

    /// <summary>
    /// Defines the underlying exception / fault type.
    /// </summary>
    [DataMember]
    public ResourceFaultType FaultType { get; set; }

    /// <summary>
    /// The ID of the fault or exception, if available.
    /// </summary>
    [DataMember]
    public int EventId { get; set; }

    /// <summary>
    /// The fault / exception message.
    /// </summary>
    [DataMember]
    public string Message { get; set; }
  }

}
