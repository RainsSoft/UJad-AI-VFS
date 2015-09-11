using System;
using System.Collections.Generic;
using System.IO;
using Vfs.Auditing;
using Vfs.Util;
using Vfs.Util.TemporaryStorage;

namespace Vfs.Transfer.Upload
{
  /// <summary>
  /// An upload handler that uses temporary files in order to
  /// store files that are being transferred. The class takes care of
  /// maintaining a temporary file, writing received data blocks to the
  /// file, and cleaning up resources after a completed or canceled
  /// transfer. Temporary file creation is delegated to implementing
  /// classes (<see cref="CreateTempDataItem"/>), completed files are being
  /// submitted to the abstract <see cref="CommitCompletedFile"/> method.<br/>
  /// Deriving from this base class instead of <see cref="UploadHandlerBase{TFile,TFolder,TTransfer}"/>
  /// enables implementing classes to not worry about file management. Furthermore,
  /// in case of an upload that is supposed to overwrite an existing file, the
  /// original file can be preserved easily.
  /// </summary>
  public abstract class TempUploadHandlerBase<TFile, TFolder, TTransfer> : UploadHandlerBase<TFile, TFolder, TTransfer>
    where TFile : IVirtualFileItem
    where TFolder : IVirtualFolderItem
    where TTransfer : UploadTransfer<TFile>
  {

    private readonly Dictionary<string, TempStream> fileCache = new Dictionary<string, TempStream>();

    /// <summary>
    /// A dictionary that provides a <see cref="TempStream"/> instance for every initialized transfer,
    /// stored under the ID of the transfer's <see cref="TransferToken.TransferId"/>.
    /// </summary>
    protected Dictionary<string, TempStream> FileCache
    {
      get { return fileCache; }
    }


    /// <summary>
    /// Initializes the service with a specific <see cref="ITransferStore{TTransfer}"/>
    /// that maintains running transfers.
    /// </summary>
    /// <param name="transferStore">Provides a storage mechanism for
    /// managed transfers.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="transferStore"/>
    /// is a null reference.</exception>
    protected TempUploadHandlerBase(ITransferStore<TTransfer> transferStore) : base(transferStore)
    {
    }


    /// <summary>
    /// Inits the service, and uses a default <see cref="ITransferStore{TTransfer}"/>
    /// in order to cache currently running transfers.
    /// </summary>
    protected TempUploadHandlerBase()
    {
    }



    /// <summary>
    /// Performs housekeeping code once a transfer is paused. This implementation
    /// closes an open file stream on the temporary file.
    /// </summary>
    /// <param name="transfer">The paused transfer.</param>
    protected override void PauseTransferImpl(TTransfer transfer)
    {
      if (fileCache.ContainsKey(transfer.TransferId))
      {
        TempStream tempStream = GetCachedTempData(transfer, null);
        tempStream.Pause();
      }
    }


    /// <summary>
    /// Cleans up the temporary file and the local <see cref="FileCache"/>
    /// before invoking finalization on the base class. This method also
    /// invokes the <see cref="CommitCompletedFile"/> method in case of a
    /// successful transfer.
    /// </summary>
    /// <param name="status">The transfer status.</param>
    /// <param name="abortReason">Indicates why the transfer was aborted, in case the
    /// <paramref name="status"/> is <see cref="TransferStatus.Aborted"/>.</param>
    /// <param name="transfer">The closed transfer.</param>
    protected override void CloseTransferImpl(TTransfer transfer, TransferStatus status, AbortReason? abortReason)
    {
      using (var fileData = GetCachedTempData(transfer, null))
      {
        FileCache.Remove(transfer.TransferId);

        if (status == TransferStatus.Completed)
        {
          CommitCompletedFile(transfer, fileData);
        }
      }
    }


    /// <summary>
    /// Handles the actual writing of the submitted data to the file system.
    /// </summary>
    /// <param name="transfer">The processed transfer.</param>
    /// <param name="dataBlock">A data block that contains a chunk of data
    /// which should be written to the file system.</param>
    protected override void WriteBufferedDataBlockImpl(TTransfer transfer, BufferedDataBlock dataBlock)
    {
      TempStream stream = GetCachedTempData(transfer, dataBlock.Offset);

      byte[] data = dataBlock.Data;
      stream.Write(data, 0, data.Length);
    }


    /// <summary>
    /// Handles the actual writing of the submitted data to the file system.
    /// </summary>
    /// <param name="transfer">The processed transfer.</param>
    /// <param name="dataBlock">A data block that provides a data stream that
    /// is written to the file system.</param>
    protected override void WriteStreamedDataBlockImpl(TTransfer transfer, StreamedDataBlock dataBlock)
    {
      TempStream stream = GetCachedTempData(transfer, dataBlock.Offset);

      //when it comes to reading from the source stream, make sure no limits (block or whole file size)
      //are exceeded
      if (!transfer.Token.MaxResourceSize.HasValue)
      {
        if (!transfer.Token.MaxBlockSize.HasValue)
        {
          //just write the stream - there is no limit
          dataBlock.WriteTo(stream);
        }
        else
        {
          //the limit is the maximum block size
          dataBlock.WriteTo(stream, transfer.Token.MaxBlockSize.Value);
        }
      }
      else
      {
        //first we need to make sure we won't go over the max resource size
        long maxSize = transfer.Token.MaxResourceSize.Value - dataBlock.Offset;

        if (transfer.Token.MaxBlockSize.HasValue)
        {
          //the other limit is the block size - take whatever is smaller
          maxSize = Math.Min(maxSize, transfer.Token.MaxBlockSize.Value);
        }

        //the limit is the maximum block size
        dataBlock.WriteTo(stream, maxSize);
      }
    }


    /// <summary>
    /// This method is being invoked right before the first data block
    /// is being written. This implementation creates and stores a new
    /// <see cref="TempStream"/> instance for the submitted
    /// <paramref name="transfer"/> in the <see cref="FileCache"/> dictionary.
    /// </summary>
    /// <param name="transfer">The currently running upload transfer.</param>
    protected override void InitializeFileUploadImpl(TTransfer transfer)
    {
      var tempData = CreateTempDataItem(transfer);
      FileCache.Add(transfer.TransferId, tempData);
    }


    /// <summary>
    /// Returns a reference to temporary data that can be used to
    /// store received resource data.
    /// </summary>
    /// <param name="transfer">The currently processed transfer.</param>
    /// <returns>A <see cref="TempStream"/> items which is used to store
    /// received file chunks.</returns>
    protected abstract TempStream CreateTempDataItem(TTransfer transfer);


    /// <summary>
    /// Verifies uploaded data before committing the transfer. This method is invoked while
    /// processing the <see cref="UploadHandlerBase{TFile,TFolder,TTransfer}.CompleteTransfer(string,string)"/> method.
    /// </summary>
    /// <param name="transfer">The transfer that is being finalized.</param>
    /// <param name="md5FileHash">An MD5 file hash that should match the uploaded file.</param>
    /// <returns>True if the submitted file hash corresponds to the data that was uploaded through
    /// the transfer, otherwise false.</returns>
    /// <remarks>Returning true results in an invocation of the
    /// <see cref="TransferHandlerBase{TFile,TToken,TTransfer}.CompleteTransfer"/>
    /// method, while a return value of false cancels the transfer by invoking
    /// <see cref="TransferHandlerBase{TFile,TToken,TTransfer}.CancelTransfer"/>.</remarks>
    protected override bool VerifyTransfer(TTransfer transfer, string md5FileHash)
    {
      var fileData = GetCachedTempData(transfer, 0);
      var hash = fileData.CalculateMd5Hash();
      return hash.Equals(md5FileHash, StringComparison.InvariantCultureIgnoreCase);
    }

    
    /// <summary>
    /// Gets a successfully completed temporary file for final processing.
    /// </summary>
    /// <param name="transfer">The completed transfer.</param>
    /// <param name="tempStream">The temporary file that provides the uploaded
    /// resource data.</param>
    protected abstract void CommitCompletedFile(TTransfer transfer, TempStream tempStream);


    /// <summary>
    /// Gets the corresponding <see cref="TempStream"/> instance from the
    /// <see cref="FileCache"/>, and audits an exception in case no matching
    /// entry is found.
    /// </summary>
    /// <param name="transfer">The processed transfer.</param>
    /// <param name="position">The expected position within the stream.</param>
    /// <returns>The corresponding <see cref="TempStream"/> entry that was created
    /// during initialization.</returns>
    protected virtual TempStream GetCachedTempData(TTransfer transfer, long? position)
    {
      try
      {
        var stream = FileCache[transfer.TransferId];
        if(position.HasValue) stream.Position = position.Value;
        return stream;
      }
      catch (KeyNotFoundException e)
      {
        string msg = "Internal dictionary does not contain a [{0}] object found for transfer [{1}] on resource [{2}] - this should not happen.";
        msg = String.Format(msg, typeof (TempStream).Name, transfer.TransferId, transfer.FileItem.QualifiedIdentifier);
        Auditor.Audit(AuditLevel.Critical, FileSystemTask.Unknown, AuditEvent.Undefined, msg);

        msg = "An unexpected error occurred - could not access file data for uploaded file [{0}]";
        msg = String.Format(msg, transfer.FileItem.ResourceInfo.FullName);
        throw new ResourceAccessException(msg, e) {IsAudited = true};
      }
    }

  }
}
