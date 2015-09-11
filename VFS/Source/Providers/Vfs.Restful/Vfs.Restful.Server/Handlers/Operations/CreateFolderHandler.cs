using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vfs.Restful.Server.Handlers.Operations
{
  public class CreateFolderHandler : VfsHandlerBase
  {
    public CreateFolderHandler(IFileSystemProvider fileSystem) : base(fileSystem)
    {
    }

    public OperationResult<VirtualFolderInfo> Post(string folderpath)
    {
      return SecureFunc(() => FileSystem.CreateFolder(folderpath));
    }
  }
}
