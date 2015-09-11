using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenRasta.Web;

namespace Vfs.Restful.Server.Handlers
{
  /// <summary>
  /// Handler class which takes care of path creation for folders
  /// of the file system.
  /// </summary>
  public class FolderPathCreationHandler : VfsHandlerBase
  {
    /// <summary>
    /// Initializes the handler class with the file system
    /// provider that receives incoming requests.
    /// </summary>
    public FolderPathCreationHandler(IFileSystemProvider fileSystem)
      : base(fileSystem)
    {
    }


    /// <summary>
    /// Creates a qualified name that can be used as an identifier
    /// for a given folder of the file system.
    /// </summary>
    /// <param name="parentFolderPath">The qualified name of the parent
    /// folder.</param>
    /// <param name="folderName">The name of the child folder.</param>
    /// <returns>An qualified path name for the submitted
    /// <paramref name="folderName"/>.</returns>
    public OperationResult<Wrapped<string>> Get(string parentFolderPath, string folderName)
    {
      return SecureFunc(() =>
      {
        string path = FileSystem.CreateFolderPath(parentFolderPath, folderName);
        return new Wrapped<string>(path);
      });
    }
  }
}