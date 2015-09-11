using System;
using System.IO;
using Vfs.LocalFileSystem.Transfer;
using Vfs.Transfer;

namespace Vfs.LocalFileSystem
{
  /// <summary>
  /// Encapsulates the configuration settings
  /// for a given <see cref="LocalFileSystemProvider"/>.
  /// </summary>
  public class LocalFileSystemConfiguration : FileSystemConfiguration<LocalDownloadTransfer, LocalUploadTransfer>
  {
    /// <summary>
    /// The configured root folder. If this property is set, the
    /// file system will limit file and directory access to the
    /// contents of this directory. If not set (default), data
    /// on all drives can be accessed.  
    /// </summary>
    public DirectoryInfo RootDirectory { get; set; }


    /// <summary>
    /// Whether to expose only relative paths if data is limited
    /// to a given <see cref="RootDirectory"/>. This property is ignored if the
    /// <see cref="RootDirectory"/> is not set.
    /// </summary>
    public bool UseRelativePaths { get; set; }


    /// <summary>
    /// Protected constructor, which requires client to use the static
    /// <see cref="CreateForMachine"/> or <see cref="CreateForRootDirectory"/>
    /// builder methods.
    /// </summary>
    protected LocalFileSystemConfiguration()
    {
    }


    /// <summary>
    /// Creates a configuration for file system providers that limit access to the
    /// contents of a given directory.
    /// </summary>
    /// <param name="rootDirectory">The root folder which is being managed
    /// by this provider instance.</param>
    /// <param name="useRelativePaths">If true, returned <see cref="VirtualResourceInfo"/>
    /// instances do not provide qualified paths, but virtual paths to the submitted
    /// root directory. This also leverages security in remote access scenarios.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="rootDirectory"/> or
    /// is a null reference.</exception>
    /// <exception cref="DirectoryNotFoundException">If the <paramref name="rootDirectory"/> does
    /// not exist on the local file system.</exception>
    /// <returns>Configuration component that can be used to construct a new instance of
    /// a <see cref="LocalFileSystemProvider"/>.</returns>
    public static LocalFileSystemConfiguration CreateForRootDirectory(DirectoryInfo rootDirectory, bool useRelativePaths)
    {
      Ensure.ArgumentNotNull(rootDirectory, "rootDirectory");

      if (!rootDirectory.Exists)
      {
        string msg = "Root directory [{0}] does not exist.";
        msg = String.Format(msg, rootDirectory.FullName);
        throw new DirectoryNotFoundException(msg);
      }

      var configuration = new LocalFileSystemConfiguration
                            {
                              RootDirectory = rootDirectory,
                              UseRelativePaths = useRelativePaths,
                              RootName = rootDirectory.Name,
                              DownloadStore = new InMemoryTransferStore<LocalDownloadTransfer>(),
                              UploadStore = new InMemoryTransferStore<LocalUploadTransfer>()
                            };

      return configuration;
    }



    /// <summary>
    /// Creates a configuration for file system providers that provide access to the
    /// whole local file system of the machine.
    /// </summary>
    /// <param name="rootName">An artificial name that is returned as the
    /// <see cref="VirtualResourceInfo.Name"/> of file system root item
    /// (as returned by <see cref="IFileSystemProvider.GetFileSystemRoot"/>).
    /// This property can be set in order to mask the real folder name.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="rootName"/>
    /// is a null reference.</exception>
    /// <returns>Configuration component that can be used to construct a new instance of
    /// a <see cref="LocalFileSystemProvider"/>.</returns>
    public static LocalFileSystemConfiguration CreateForMachine(string rootName)
    {
      Ensure.ArgumentNotNull(rootName, "rootName");

      var configuration = new LocalFileSystemConfiguration
                            {
                              RootName = rootName,
                              DownloadStore = new InMemoryTransferStore<LocalDownloadTransfer>(),
                              UploadStore = new InMemoryTransferStore<LocalUploadTransfer>()
                            };

      return configuration;
    }

  }
}
