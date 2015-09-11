using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OpenRasta.Web;
using Vfs.Transfer;

namespace RESTful_Filesystem.Handlers
{
  public class DummyHandler
  {
    [HttpOperation(HttpMethod.GET)]
    public DownloadToken ReloadToken(string transferId)
    {
      return new DownloadToken();
    }
  }
}
