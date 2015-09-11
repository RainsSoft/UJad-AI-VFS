using System;
using System.IO;

namespace Vfs.Util.TemporaryStorage
{
  /// <summary>
  /// A factory implementation that returns
  /// <see cref="TempFileStream"/> instances which
  /// use local temporary files.
  /// </summary>
  public class TempFileStreamFactory : ITempStreamFactory
  {
    public string TempFileRootDirectory { get; set; }

    public TempFileStreamFactory(string tempFileRootDirectory)
    {
      TempFileRootDirectory = tempFileRootDirectory;
    }


    /// <summary>
    /// Gets a <see cref="Stream"/> that automatically cleans up
    /// after itself.
    /// </summary>
    /// <returns>A stream that allows to write and read temporary
    /// data.</returns>
    public TempStream CreateTempStream()
    {
      //create unique file name using a GUID
      string fileName = Guid.NewGuid().ToString();
      var tempFilePath = TempFileUtil.CreateTempFilePath(TempFileRootDirectory, fileName, "tmp");
      
      //return temp stream
      var fi = new FileInfo(tempFilePath);
      return new TempFileStream(fi);
    }
  }
}
