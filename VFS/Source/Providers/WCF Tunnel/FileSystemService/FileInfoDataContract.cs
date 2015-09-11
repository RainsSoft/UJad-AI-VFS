using System.ServiceModel;

namespace Vfs.FileSystemService
{
  /// <summary>
  /// Encapsulates a single <see cref="VirtualFileInfo"/>
  /// as a message contract.
  /// </summary>
  [MessageContract]
  public class FileInfoDataContract
  {
    [MessageHeader(MustUnderstand = true)]
    public VirtualFileInfo FileInfo { get; set; }
  }
}