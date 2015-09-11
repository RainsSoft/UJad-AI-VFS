namespace Vfs.Restful.Server.Handlers.Browse
{
  public class FolderAvailabilityCheckHandler : BrowsingHandler
  {
    public FolderAvailabilityCheckHandler(IFileSystemProvider fileSystem) : base(fileSystem)
    {
    }


    public OperationResult<Wrapped<bool>> Get(string folderPath)
    {
      return SecureFunc(() => new Wrapped<bool> { Value = FileSystem.IsFolderAvailable(folderPath) });
    }
  }
}