using System.Linq;

namespace Vfs.Restful.Server.Handlers.Browse
{
  public class GetFolderContentsHandler : BrowsingHandler
  {
    public GetFolderContentsHandler(IFileSystemProvider fileSystem) : base(fileSystem)
    {
    }

    public OperationResult<FolderContentsInfo> Get(string parentFolderPath)
    {
      return SecureFunc(() =>
                          {
                            var contents = FileSystem.GetFolderContents(parentFolderPath);
                            contents.Files = contents.Files.ToArray();
                            contents.Folders = contents.Folders.ToArray();

                            return contents;
                          });
    }

    public OperationResult<FolderContentsInfo> Get(string parentFolderPath, string filter)
    {
      return SecureFunc(() =>
                          {
                            var contents = FileSystem.GetFolderContents(parentFolderPath, filter);
                            contents.Files = contents.Files.ToArray();
                            contents.Folders = contents.Folders.ToArray();

                            return contents;
                          });
    }
  }
}