using OpenRasta.Configuration;
using Vfs.Restful.Server;

namespace Vfs.Restful.Test
{
  public class TestConfiguration : IConfigurationSource
  {
    public IFileSystemProvider Provider { get; private set; }
    public VfsServiceSettings Settings { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public TestConfiguration(IFileSystemProvider provider, VfsServiceSettings settings)
    {
      Provider = provider;
      Settings = settings;
    }

    public void Configure()
    {
      using (OpenRastaConfiguration.Manual)
      {
        ConfigurationHelper.RegisterFileSystemProvider(Provider);
        ConfigurationHelper.RegisterSettings(Settings);
        ConfigurationHelper.RegisterDefaultHandlers();
      }
    }
  }
}