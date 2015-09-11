using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenRasta.Web;

namespace Vfs.Restful.Server.Handlers
{
  public class BrowsingHandler : VfsHandlerBase
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public BrowsingHandler(IFileSystemProvider fileSystem) : base(fileSystem)
    {
    }

    /// <summary>
    /// Gets the file system root folder from the underlying
    /// file system.
    /// </summary>
    public VirtualFolderInfo GetFileSystemRoot()
    {
      return FileSystem.GetFileSystemRoot();
    }

    public VirtualFileInfo GetFile(string filePath)
    {
      return FileSystem.GetFileInfo(filePath);
    }

    [HttpOperation(HttpMethod.GET, ForUriName = "GetFolder")]
    public VirtualFolderInfo GetFolder(string folderpath)
    {
      return FileSystem.GetFolderInfo(folderpath);
    }

    [HttpOperation(HttpMethod.GET, ForUriName = "GetFolderParent")]
    public VirtualFolderInfo GetFolderParent(string folderPath)
    {
      return FileSystem.GetFolderParent(folderPath);
    }

    [HttpOperation(HttpMethod.GET, ForUriName = "GetFileParent")]
    public VirtualFolderInfo GetFileParent(string filePath)
    {
      return FileSystem.GetFileParent(filePath);
    }


    [HttpOperation(HttpMethod.GET, ForUriName = "GetFiles")]
    public IEnumerable<VirtualFileInfo> GetFiles(string parentFolderPath)
    {
      return FileSystem.GetChildFiles(parentFolderPath).ToArray();
    }

    [HttpOperation(HttpMethod.GET, ForUriName = "GetFiles")]
    public IEnumerable<VirtualFileInfo> GetFiles(string parentFolderPath, string filter)
    {
      return FileSystem.GetChildFiles(parentFolderPath, filter).ToArray();
    }

    [HttpOperation(HttpMethod.GET, ForUriName = "GetFolders")]
    public IEnumerable<VirtualFolderInfo> GetFolders(string parentFolderPath)
    {
      return FileSystem.GetChildFolders(parentFolderPath).ToArray();
    }

    [HttpOperation(HttpMethod.GET, ForUriName = "GetFolders")]
    public IEnumerable<VirtualFolderInfo> GetFolders(string parentFolderPath, string filter)
    {
      return FileSystem.GetChildFolders(parentFolderPath, filter).ToArray();
    }

    [HttpOperation(HttpMethod.GET, ForUriName = "GetContents")]
    public FolderContentsInfo GetContents(string parentFolderPath)
    {
      var contents = FileSystem.GetFolderContents(parentFolderPath);
      contents.Files = contents.Files.ToArray();
      contents.Folders = contents.Folders.ToArray();

      return contents;
    }

    [HttpOperation(HttpMethod.GET, ForUriName = "GetContents")]
    public FolderContentsInfo GetContents(string parentFolderPath, string filter)
    {
      var contents = FileSystem.GetFolderContents(parentFolderPath, filter);
      contents.Files = contents.Files.ToArray();
      contents.Folders = contents.Folders.ToArray();

      return contents;
    }


    [HttpOperation(HttpMethod.GET, ForUriName = "IsFileAvailable")]
    public bool IsFileAvailable(string filePath)
    {
      return FileSystem.IsFileAvailable(filePath);
    }

    [HttpOperation(HttpMethod.GET, ForUriName = "IsFolderAvailable")]
    public bool IsFolderAvailable(string folderPath)
    {
      return FileSystem.IsFolderAvailable(folderPath);
    }

  }
}