using System;
using System.IO;
using OpenRasta.IO;
using OpenRasta.Web;
using Vfs.Transfer;

namespace Vfs.Restful.Server.Resources
{
  public class FileDataResource :  IFile
  {
    public TransferToken Token { get; private set; }
    public Func<Stream> OpenStreamFunc { get; set; }


    public Stream OpenStream()
    {
      return OpenStreamFunc();
    }

    public MediaType ContentType
    {
      get { return new MediaType(Token.ContentType); }
    }

    public string FileName
    {
      get { return Token.ResourceName; }
    }

    public long Length
    {
      get { return Token.ResourceLength; }
    }


    //currently not in use - derive from IDownloadableFile in order to be required
    //(although with the OpenRasta RC, it appears it has no impact)
    public DownloadableFileOptions Options
    {
      get { return DownloadableFileOptions.Open; }
    }

    public FileDataResource(TransferToken token, Func<Stream> openStreamFunc)
    {
      Token = token;
      OpenStreamFunc = openStreamFunc;
    }
  }
}