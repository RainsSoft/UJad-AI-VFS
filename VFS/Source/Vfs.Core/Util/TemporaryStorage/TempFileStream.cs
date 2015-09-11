using System.IO;

namespace Vfs.Util.TemporaryStorage
{
  /// <summary>
  /// Provides access to a temporary file on the local file system.
  /// This class maintains a regular file (<see cref="TempFile"/>
  /// property), opens the file stream on demand, and deletes
  /// the file once it's no longer required.
  /// </summary>
  public class TempFileStream : TempStream
  {
    /// <summary>
    /// The maintained temporary file.
    /// </summary>
    public FileInfo TempFile { get; private set; }

    /// <summary>
    /// Creates the class with a given temporary file.
    /// </summary>
    /// <param name="tempFile">The temporary file that is managed and eventually
    /// deleted.</param>
    public TempFileStream(FileInfo tempFile)
    {
      Ensure.ArgumentNotNull(tempFile, "tempFile");
      TempFile = tempFile;
    }


    /// <summary>
    /// Creates the underlying stream. This method is being invoked on the first
    /// request of the <see cref="TempStream.DecoratedStream"/> property.
    /// </summary>
    /// <returns>A temporary, seekeable stream.</returns>
    protected override Stream CreateTempDataStream()
    {
      //open stream, share full access in order not to block the file if no cleanup happens
      return TempFile.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);
    }

    /// <summary>
    /// When invoked (which happens during disposal), implementing classes
    /// should clean up temporary resources.
    /// </summary>
    protected override void DiscardTempResources()
    {
      TempFile.Refresh();
      if (TempFile.Exists) TempFile.Delete();
    }

    /// <summary>
    /// Temporarily closes the underlying file stream if the temporary data
    /// is not needed for a while but should not be discarded yet.
    /// </summary>
    public override void Pause()
    {
      CloseStream();
      base.Pause();
    }


    /// <summary>
    /// Closes and disposes the maintained <see cref="Stream"/> if set, and
    /// and sets the property to null.
    /// </summary>
    protected void CloseStream()
    {
      if (IsTempStreamCreated)
      {
        var stream = DecoratedStream;
        stream.Dispose();
        DecoratedStream = null;
      }
    }

  }
}