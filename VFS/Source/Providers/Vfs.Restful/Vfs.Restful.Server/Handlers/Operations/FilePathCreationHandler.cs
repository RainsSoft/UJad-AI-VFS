namespace Vfs.Restful.Server.Handlers
{
  /// <summary>
  /// Handler class which takes care of path creation for files
  /// of the file system.
  /// </summary>
  public class FilePathCreationHandler : VfsHandlerBase
  {
    /// <summary>
    /// Initializes the handler class with the file system
    /// provider that receives incoming requests.
    /// </summary>
    public FilePathCreationHandler(IFileSystemProvider fileSystem) : base(fileSystem)
    {
    }


    /// <summary>
    /// Creates a qualified name that can be used as an identifier
    /// for a given file of the file system.
    /// </summary>
    /// <param name="parentFolderPath">The qualified name of the parent
    /// folder.</param>
    /// <param name="fileName">The name of a file within the folder.</param>
    /// <returns>An qualified path name for the submitted
    /// <paramref name="fileName"/>.</returns>
    public OperationResult<Wrapped<string>> Get(string parentFolderPath, string fileName)
    {
      return SecureFunc(() =>
                          {
                            string path = FileSystem.CreateFilePath(parentFolderPath, fileName);
                            return new Wrapped<string>(path);
                          });
    }
  }
}