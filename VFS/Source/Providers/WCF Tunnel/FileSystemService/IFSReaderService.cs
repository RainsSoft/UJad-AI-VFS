using System;
using System.IO;
using System.ServiceModel;
using Vfs.FileSystemService.Faults;
using Vfs.Util;

namespace Vfs.FileSystemService
{
  /// <summary>
  /// A service that provides streaming access to hosted file data.
  /// </summary>
  [ServiceContract]
  public interface IFSReaderService
  {
    /// <summary>
    /// Gets the binary contents as a stream in a blocking operation.
    /// Use the methods in <see cref="ContentUtil"/> class for simplified stream
    /// handling.
    /// </summary>
    /// <param name="virtualFilePath">The path of the file to be read.</param>
    /// <returns>A stream that allows the contents of the file to be read.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="virtualFilePath"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the file that is represented
    /// by <paramref name="virtualFilePath"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    [OperationContract]
    [FaultContract(typeof(ResourceFault))]
    Stream ReadFileContents(string virtualFilePath);


    /// <summary>
    /// Gets the binary contents as a stream in a blocking operation.
    /// Use the methods in <see cref="ContentUtil"/> class for simplified stream
    /// handling.
    /// </summary>
    /// <param name="fileInfo">Provides meta information about the file to be read.</param>
    /// <returns>A stream that allows the contents of the file to be read.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="fileInfo"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the file that is represented
    /// by <paramref name="fileInfo"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    [OperationContract(Name = "ReadFileContents2")]
    [FaultContract(typeof(ResourceFault))]
    Stream ReadFileContents(VirtualFileInfo fileInfo);
  }
}
