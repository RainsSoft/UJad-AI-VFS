using System.Globalization;
using System.Web;
using OpenRasta.Codecs;
using OpenRasta.IO;
using OpenRasta.Web;
using Vfs.Transfer;

namespace Vfs.Restful.Server.Codecs
{
  /// <summary>
  /// A coded that write the stream of a given
  /// <see cref="StreamedDataBlock"/> directly to the
  /// response and writes meta data to the response headers.
  /// </summary>
  [MediaType("application/octet-stream")]
  public class DataBlockCodec : IMediaTypeWriter
  {
    public object Configuration
    {
      get; set;
    }

    public void WriteTo(object entity, IHttpEntity response, string[] codecParameters)
    {
      //get the data block to be transferred
      StreamedDataBlock dataBlock = (StreamedDataBlock)entity;


      if (HttpContext.Current != null)
      {
        //disable buffering
        HttpContext.Current.Response.BufferOutput = false;

        if (dataBlock.BlockLength.HasValue)
        {
          //only set the content lengths, if we actually know it
          response.SetHeader("Content-Length", dataBlock.BlockLength.Value.ToString(CultureInfo.InvariantCulture));
        }

        response.SetHeader("Content-Type", MediaType.ApplicationOctetStream.Name);
      }
      else
      {
        response.ContentLength = dataBlock.BlockLength;
        response.ContentType = MediaType.ApplicationOctetStream;
      }

      //write the HTTP headers
      VfsHttpHeaders headers = VfsHttpHeaders.Default;
      response.SetHeader(headers.TransferId, dataBlock.TransferTokenId);
      response.SetHeader(headers.BlockLength, dataBlock.BlockLength.ToString());
      response.SetHeader(headers.BlockNumber, dataBlock.BlockNumber.ToString());
      response.SetHeader(headers.IsLastBlock, dataBlock.IsLastBlock.ToString());
      response.SetHeader(headers.BlockOffset, dataBlock.Offset.ToString());

      using (dataBlock.Data)
      {
        //write data to response stream, then close the stream
        dataBlock.Data.CopyTo(response.Stream);
      }
    }


  }
}
