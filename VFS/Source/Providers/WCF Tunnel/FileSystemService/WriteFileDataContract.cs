using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Vfs.FileSystemService
{
  /// <summary>
  /// A data contract which is used to request 
  /// data for a given <see cref="VirtualFileInfo"/>.
  /// </summary>
  [MessageContract]
  public class WriteFileDataContract
  {
    [MessageHeader(MustUnderstand = true)]
    public string QualifiedFilePath { get; set; }

    /// <summary>
    /// Indicates whether an existing file should be
    /// overwritten or not.
    /// </summary>
    [MessageHeader(MustUnderstand = true)]
    public bool Overwrite { get; set; }

    /// <summary>
    /// The submitted data.
    /// </summary>
    [MessageBodyMember(Order = 0)]
    public Stream Data { get; set; }
  }
}
