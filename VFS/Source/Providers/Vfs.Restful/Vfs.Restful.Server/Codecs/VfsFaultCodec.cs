using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using OpenRasta.Codecs;
using OpenRasta.TypeSystem;
using OpenRasta.Web;

namespace Vfs.Restful.Server.Codecs
{
  [MediaType(VfsFault.FaultContentType)]
  [SupportedType(typeof(VfsFault))]
  public class VfsFaultCodec : XmlCodec // IMediaTypeWriter
  {
    private static readonly DataContractSerializer serializer = new DataContractSerializer(typeof(VfsFault));

    public object Configuration { get; set; }

    public override void WriteToCore(object entity, IHttpEntity response)
    {
      serializer.WriteObject(Writer, entity);
      Writer.Close();
    }

    public override object ReadFrom(IHttpEntity request, IType destinationType, string memberName)
    {
      throw new NotImplementedException();
    }

//    public void WriteTo(object entity, IHttpEntity response, string[] codecParameters)
//    {
//      serializer.WriteObject(response.Stream, entity);
//      response.Stream.Close();
//    }
  }
}
