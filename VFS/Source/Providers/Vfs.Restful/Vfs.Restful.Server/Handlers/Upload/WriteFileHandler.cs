using System;
using System.IO;
using System.Runtime.InteropServices;
using Vfs.Restful.Server.Util;
using Vfs.Util;

namespace Vfs.Restful.Server.Handlers.Upload
{
  public class WriteFileHandler : UploadHandler
  {
    public WriteFileHandler(IFileSystemProvider fileSystem) : base(fileSystem)
    {
    }

    /// <summary>
    /// Creates or updates a given file resource in the file system.
    /// </summary>
    public VirtualFileInfo Post([Optional]Stream input, string filePath)
    {
      //in case of an empty data block, there is no stream - just use a null stream instead
      if (input == null) input = Stream.Null;  

      VfsHttpHeaders headers = VfsHttpHeaders.Default;

      //get custom headers
      bool overwrite = Convert.ToBoolean(Request.Headers[headers.OverwriteExistingResource]);
      long resourceLength = Request.Headers.ContentLength ?? 0;

      var ct = Request.Headers.ContentType;
      string contentType = ct == null ? ContentUtil.UnknownContentType : ct.Name;
      if (String.IsNullOrEmpty(contentType)) contentType = ContentUtil.UnknownContentType;

      //wrap OpenRasta stream into a non-seekable one - the OR streams indicates it's seekable
      //but does not support setting its position
      var stream = new NonSeekableStream(input);
      return FileSystem.WriteFile(filePath, stream, overwrite, resourceLength, contentType);
    }
  }
}