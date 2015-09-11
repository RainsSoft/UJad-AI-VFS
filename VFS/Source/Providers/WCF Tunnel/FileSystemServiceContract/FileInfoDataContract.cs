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
#if !SILVERLIGHT
    [MessageHeader(MustUnderstand = true)]
#endif
    public VirtualFileInfo FileInfo { get; set; }
  }
}