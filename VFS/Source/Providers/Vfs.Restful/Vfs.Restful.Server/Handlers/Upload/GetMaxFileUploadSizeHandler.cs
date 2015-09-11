using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenRasta.Web;

namespace Vfs.Restful.Server.Handlers.Upload
{
  public class GetMaxFileUploadSizeHandler : UploadHandler
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public GetMaxFileUploadSizeHandler(IFileSystemProvider fileSystem) : base(fileSystem)
    {
    }


    /// <summary>
    /// Injected via DI.
    /// </summary>
    public VfsServiceSettings Settings { get; set; }


    [HttpOperation(HttpMethod.GET, ForUriName = "GetMaxFileUploadSize")]
    public OperationResult<Wrapped<long?>> Get()
    {
      return SecureFunc(() =>
                          {
                            //get the minimum of the configured block size and the file system block size
                            //(which are both optional)
                            long? settingsValue = Settings == null ? null : Settings.MaxFileUploadSize;
                            if (settingsValue.HasValue)
                            {
                              long blockSize = Math.Min(settingsValue.Value,
                                                        Uploads.GetMaxFileUploadSize() ?? long.MaxValue);
                              return new Wrapped<long?>(blockSize);
                            }

                            return new Wrapped<long?>(Uploads.GetMaxFileUploadSize());
                          });
    }


  }
}
