using OpenRasta.Web;

namespace Vfs.Restful.Server.Handlers
{
  public class OperationsHandler : VfsHandlerBase
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public OperationsHandler(IFileSystemProvider fileSystem) : base(fileSystem)
    {
    }


    [HttpOperation(HttpMethod.POST, ForUriName = "CreateFolder")]
    public VirtualFolderInfo CreateFolder(string folderpath)
    {
      return FileSystem.CreateFolder(folderpath);
    }


    [HttpOperation(HttpMethod.DELETE, ForUriName = "DeleteFolder")]
    public OperationResult DeleteFolder(string folderpath)
    {
      FileSystem.DeleteFolder(folderpath);
      return new OperationResult.OK();
    }


    [HttpOperation(HttpMethod.DELETE, ForUriName = "DeleteFile")]
    public OperationResult DeleteFile(string filePath)
    {
      FileSystem.DeleteFile(filePath);
      return new OperationResult.OK();
    }


    [HttpOperation(HttpMethod.POST, ForUriName = "MoveFolder")]
    public VirtualFolderInfo MoveFolder(string folderPath, string destinationPath)
    {
      return FileSystem.MoveFolder(folderPath, destinationPath);
    }

    [HttpOperation(HttpMethod.POST, ForUriName = "CopyFolder")]
    public VirtualFolderInfo CopyFolder(string folderPath, string destinationPath)
    {
      return FileSystem.CopyFolder(folderPath, destinationPath);
    }


    [HttpOperation(HttpMethod.POST, ForUriName = "MoveFile")]
    public VirtualFileInfo MoveFile(string filePath, string destinationPath)
    {
      return FileSystem.MoveFile(filePath, destinationPath);
    }

    [HttpOperation(HttpMethod.POST, ForUriName = "CopyFile")]
    public VirtualFileInfo CopyFile(string filePath, string destinationPath)
    {
      return FileSystem.CopyFile(filePath, destinationPath);
    }

  }
}
