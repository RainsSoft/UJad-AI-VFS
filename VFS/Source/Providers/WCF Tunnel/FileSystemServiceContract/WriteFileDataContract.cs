using System;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;

namespace Vfs.FileSystemService
{
  /// <summary>
  /// A data contract which is used to request 
  /// data for a given <see cref="VirtualFileInfo"/>.
  /// </summary>
  [MessageContract]
  public class WriteFileDataContract : IDisposable
  {
#if !SILVERLIGHT
    [MessageHeader(MustUnderstand = true)]
#endif
    public string FilePath { get; set; }

    /// <summary>
    /// Indicates whether an existing file should be
    /// overwritten or not.
    /// </summary>
#if !SILVERLIGHT
    [MessageHeader(MustUnderstand = true)]
#endif
    public bool Overwrite { get; set; }



    /// <summary>
    /// The length of the written file.
    /// </summary>
#if !SILVERLIGHT
    [MessageHeader(MustUnderstand = true)]
#endif
    public long ResourceLength { get; set; }


    /// <summary>
    /// The content type of the written file.
    /// </summary>
#if !SILVERLIGHT
    [MessageHeader(MustUnderstand = true)]
#endif
    public string ContentType { get; set; }


    /// <summary>
    /// The submitted data.
    /// </summary>
    [MessageBodyMember(Order = 0)]
    public Stream Data { get; set; }

    /// <summary>
    /// Disposes the <see cref="Data"/> stream.
    /// </summary>
    /// <remarks>Exceptions while trying to dispose the stream
    /// are written to the Debug output.</remarks>
    public void Dispose()
    {
      if (Data != null)
      {
        try
        {
          Data.Dispose();
        }
        catch (Exception e)
        {
          string msg = "Exception occurred when trying to dispose Stream in WriteFileDataContract of file {0}:\n{1}.";
          msg = String.Format(msg, FilePath, e);
          Debug.WriteLine(msg);
        }
      }
    }
  }
}
