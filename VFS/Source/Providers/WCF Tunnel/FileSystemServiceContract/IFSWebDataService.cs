using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Vfs.Transfer;

namespace Vfs.FileSystemService
{
  [ServiceContract]
  public interface IFSWebDataService
  {



    /// <summary>
    /// This is a RESTful method that complements the SOAP enabled
    /// <see cref="IFSWriterService"/>, which encapsulate the submitted
    /// stream in a message contract.
    /// </summary>
#if !SILVERLIGHT
    [OperationContract,
     System.ServiceModel.Web.WebInvoke(Method = "POST",
       UriTemplate =
         "/writefile?file={filePath}&overwrite={overwrite}&length={resourceLength}&contenttype={contentType}",
       BodyStyle = System.ServiceModel.Web.WebMessageBodyStyle.Bare)]
#endif
    string WriteFile(string filePath, bool overwrite, long resourceLength, string contentType, Stream data);


#if !SILVERLIGHT
    [OperationContract,
     System.ServiceModel.Web.WebInvoke(Method = "POST",
       UriTemplate =
         "/writeblock?transfer={transferId}&blocknumber={blockNumber}&blocklength={blockLength}&offset={offset}",
       BodyStyle = System.ServiceModel.Web.WebMessageBodyStyle.Bare)]
#endif
    void WriteDataBlock(string transferId, int blockNumber, int blockLength, long offset, byte[] data);
  }
}