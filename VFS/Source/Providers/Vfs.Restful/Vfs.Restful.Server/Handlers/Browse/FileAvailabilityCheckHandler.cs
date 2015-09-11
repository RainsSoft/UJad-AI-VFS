namespace Vfs.Restful.Server.Handlers.Browse
{
  public class FileAvailabilityCheckHandler : BrowsingHandler
  {
    public FileAvailabilityCheckHandler(IFileSystemProvider fileSystem) : base(fileSystem)
    {
    }

    public OperationResult<Wrapped<bool>> Get(string filePath)
    {
      return SecureFunc(() => new Wrapped<bool> {Value = FileSystem.IsFileAvailable(filePath) });
    }

  }
}