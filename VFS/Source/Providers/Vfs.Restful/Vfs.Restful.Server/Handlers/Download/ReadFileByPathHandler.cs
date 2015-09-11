using System;
using Vfs.Restful.Server.Resources;
using Vfs.Transfer.Util;

namespace Vfs.Restful.Server.Handlers.Download
{
  public class ReadFileByPathHandler : ReadFileHandlerBase
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public ReadFileByPathHandler(IFileSystemProvider fileSystem) : base(fileSystem)
    {
    }

    /// <summary>
    /// Returns access to a given file as a whole stream.
    /// </summary>
    /// <param name="filePath">Path of the requested file resource.</param>
    /// <returns>OpenRasta file.</returns>
    public OperationResult<FileDataResource> Get(string filePath)
    {
      Func<FileDataResource> func = () =>
                                      {
                                        var token = FileSystem.DownloadTransfers.RequestDownloadToken(filePath, false);
                                        return new FileDataResource(token, () =>
                                                                             {
                                                                               SetResponseHeader(token);
                                                                               return Downloads.DownloadFile(token.TransferId);
                                                                             });
                                      };

      return SecureFunc(func);
    }
  }
}