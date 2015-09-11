using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Vfs.Transfer;

namespace Vfs.FileSystemService
{
  public interface IFSDataUploadService
  {
    /// <summary>
    /// Uploads a given data block that contains a chunk of data for an uploaded file.
    /// </summary>
    /// <param name="block">The block to be written.</param>
    /// <exception cref="DataBlockException">If the data block's contents cannot be stored,
    /// either because it's an invalid number, or because only sequential downloads
    /// are possible, and the block does not refer to the current download
    /// position.
    /// </exception>
    /// <exception cref="TransferStatusException">If the transfer has already expired.</exception>
    void WriteDataBlock(BufferedDataBlock block);

    /// <summary>
    /// Uploads a given data block that provides a chunk of data for an uploaded file as a stream.
    /// </summary>
    /// <param name="block">The block to be written.</param>
    /// <exception cref="DataBlockException">If the data block's contents cannot be stored,
    /// either because it's an invalid number, or because only sequential downloads
    /// are possible, and the block does not refer to the current download
    /// position.
    /// </exception>
    /// <exception cref="TransferStatusException">If the transfer has already expired.</exception>
    void WriteDataBlockStreamed(StreamedDataBlock block);


    /// <summary>
    /// This is a more REST friendly overload to the <see cref="WriteBlock(BufferedDataBlock)"/>
    /// method.
    /// </summary>
    /// <param name="transferId">Identifies the currently uploaded file.</param>
    /// <param name="blockNumber">The block number that is being transferred.</param>
    /// <param name="offset">The offset of the submitted data within the file that is being
    /// uploaded.</param>
    /// <param name="data">A chunk of data.</param>
    /// <exception cref="DataBlockException">If the data block's contents cannot be stored,
    /// either because it's an invalid number, or because only sequential downloads
    /// are possible, and the block does not refer to the current download
    /// position.
    /// </exception>
    /// <exception cref="TransferStatusException">If the transfer has already expired.</exception>
#if !SILVERLIGHT
    [OperationContract,
     System.ServiceModel.Web.WebInvoke(Method = "POST",
       UriTemplate = "/writeblock?transfer={transferId}&blocknumber={blockNumber}&offset={offset}",
       BodyStyle = System.ServiceModel.Web.WebMessageBodyStyle.Bare)]
#endif
    void WriteDataBlock(string transferId, int blockNumber, long offset, byte[] data);



    /// <summary>
    /// Uploads a chunk of data that submits 
    /// </summary>
    /// <param name="transferId">Identifies the currently uploaded file.</param>
    /// <param name="blockNumber">The block number that is being transferred.</param>
    /// <param name="offset">The offset of the submitted data within the file that is being
    /// uploaded.</param>
    /// <param name="data">A chunk of data.</param>
    /// <exception cref="DataBlockException">If the data block's contents cannot be stored,
    /// either because it's an invalid number, or because only sequential downloads
    /// are possible, and the block does not refer to the current download
    /// position.
    /// </exception>
    /// <exception cref="TransferStatusException">If the transfer has already expired.</exception>
#if !SILVERLIGHT
    [OperationContract,
     System.ServiceModel.Web.WebInvoke(Method = "POST",
       UriTemplate = "/writeblock?transfer={transferId}&blocknumber={blockNumber}&offset={offset}",
       BodyStyle = System.ServiceModel.Web.WebMessageBodyStyle.Bare)]
#endif
    void WriteDataBlockStreamed(string transferId, int blockNumber, long offset, long blockLength, Stream data);





    /// <summary>
    /// Creates or updates a given file resource in the file system in one blocking operation.
    /// </summary>
    /// <param name="virtualFilePath">The qualified path of the file to be created.</param>
    /// <param name="input">A stream that provides the file's contents.</param>
    /// <param name="overwrite">Whether an existing file should be overwritten
    /// or not. If this parameter is false and the file already exists, a
    /// <see cref="ResourceOverwriteException"/> is thrown.</param>
    /// <param name="resourceLength">The length of the resource to be uploaded in bytes.</param>
    /// <param name="contentType">The content type of the uploaded resource.</param>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    /// <exception cref="ResourceOverwriteException">If a file already exists at the
    /// specified location, and the <paramref name="overwrite"/> flag was not set.</exception>
    /// <exception cref="ArgumentNullException">If any of the parameters is a null reference.</exception>
    void WriteFile(string virtualFilePath, bool overwrite, long resourceLength, string contentType, Stream input);

  }
}
