namespace Vfs.Silverlight.FileBrowser.ViewModel
{
  public class MainViewModel
  {
    /// <summary>
    /// The used file system.
    /// </summary>
    public IFileSystemProvider FileSystem { get; set; }

    public VirtualFolder RootFolder { get; set; }

  }
}
