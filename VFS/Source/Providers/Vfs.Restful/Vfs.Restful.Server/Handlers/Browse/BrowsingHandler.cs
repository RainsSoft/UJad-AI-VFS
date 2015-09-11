using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vfs.Restful.Server.Handlers.Browse
{
  public abstract class BrowsingHandler : VfsHandlerBase
  {
    /// <summary>
    /// Initializes the handler class with the file system
    /// provider that receives incoming requests.
    /// </summary>
    protected BrowsingHandler(IFileSystemProvider fileSystem) : base(fileSystem)
    {
    }
  }
}
