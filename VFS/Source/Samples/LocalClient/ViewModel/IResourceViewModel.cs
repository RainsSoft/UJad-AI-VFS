using Vfs;

namespace Vfs.Samples.LocalClient.ViewModel
{
  public interface IResourceViewModel
  {
    /// <summary>
    /// Gets meta data for the underlying item.
    /// </summary>
    VirtualResourceInfo ResourceInfo { get; }
  }
}