using System;
using Vfs.Restful.Server.Resources;
using Vfs.Transfer;

namespace Vfs.Restful.Server.Handlers.Download
{
  public class ReadFileByTokenHandler : ReadFileHandlerBase
  {
    public ReadFileByTokenHandler(IFileSystemProvider fileSystem) : base(fileSystem)
    {
    }


    /// <summary>
    /// Returns access to a given file as a whole stream.
    /// </summary>
    /// <param name="transferId">The <see cref="TransferToken.TransferId"/>
    /// of an issued download token.</param>
    public OperationResult<FileDataResource> Get(string transferId)
    {
      Func<FileDataResource> func = () =>
                                      {
                                        var token = FileSystem.DownloadTransfers.ReloadToken(transferId);
                                        return new FileDataResource(token, () =>
                                                                             {
                                                                               SetResponseHeader(token);
                                                                               return Downloads.DownloadFile(transferId);
                                                                             });
                                      };

      return SecureFunc(func);
    }
  }
}