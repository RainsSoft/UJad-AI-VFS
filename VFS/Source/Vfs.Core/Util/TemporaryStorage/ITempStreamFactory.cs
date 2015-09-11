using System.IO;

namespace Vfs.Util.TemporaryStorage
{
  /// <summary>
  /// Responsible for the creation of <see cref="Stream"/>
  /// instances which can be used to read and write temporary
  /// data.
  /// </summary>
  public interface ITempStreamFactory
  {
    /// <summary>
    /// Gets a <see cref="Stream"/> that automatically cleans up
    /// after itself.
    /// </summary>
    /// <returns>A stream that allows to write and read temporary
    /// data.</returns>
    TempStream CreateTempStream();
  }
}
