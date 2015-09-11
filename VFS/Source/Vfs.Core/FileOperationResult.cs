using System;


namespace Vfs
{
  /// <summary>
  /// Encapsulates basic information about a given
  /// file operation, e.g. a file read.
  /// </summary>
  public class FileOperationResult
  {
    /// <summary>
    /// The processed file.
    /// </summary>
    public VirtualFileInfo FileInfo { get; private set; }

    /// <summary>
    /// If the operation failed, the included exception
    /// provides further information about the error.
    /// </summary>
    public Exception Exception { get; set; }

    /// <summary>
    /// Indicates whether the operation succeeded or not.
    /// This is a convenience property that just returns
    /// <c>true</c> if the <see cref="Exception"/> property
    /// is null.
    /// </summary>
    public bool IsSuccess
    {
      get { return Exception == null; }
    }


    /// <summary>
    /// Creates a new instance which refers to a given <see cref="VirtualFileInfo"/>
    /// item.
    /// </summary>
    /// <exception cref="ArgumentNullException">If <paramref name="fileInfo"/>
    /// is a null reference.</exception>
    public FileOperationResult(VirtualFileInfo fileInfo)
    {
      if (fileInfo == null) throw new ArgumentNullException("fileInfo");
      FileInfo = fileInfo;
    }
  }
}
