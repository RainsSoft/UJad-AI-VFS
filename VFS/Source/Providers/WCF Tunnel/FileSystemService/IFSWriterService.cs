using System;
using System.ServiceModel;
using Vfs.FileSystemService.Faults;

namespace Vfs.FileSystemService
{
  /// <summary>
  /// Exposes service operations to upload binary
  /// data to the service.
  /// </summary>
  [ServiceContract]
  public interface IFSWriterService
  {
    /// <summary>
    /// Creates or updates a given file resource in the file system.
    /// </summary>
    /// <param name="writeContract">Provides required information about the
    /// file to be created along with the data to be written.</param>
    /// <returns>Updated file information.</returns>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the parent folder
    /// of the submitted file path does not exist.</exception>
    /// <exception cref="ResourceOverwriteException">If a file already exists at the
    /// specified location, and the <see cref="WriteFileDataContract.Overwrite"/> flag was not set.</exception>
    /// <exception cref="ArgumentNullException">If any of the parameters is a null reference.</exception>
    [OperationContract]
    [FaultContract(typeof(ResourceFault))]
    FileInfoDataContract WriteFile(WriteFileDataContract writeContract);
  }
}