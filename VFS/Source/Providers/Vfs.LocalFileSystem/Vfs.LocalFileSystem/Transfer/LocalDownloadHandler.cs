using System;
using System.IO;
using Vfs.Auditing;
using Vfs.Locking;
using Vfs.Scheduling;
using Vfs.Security;
using Vfs.Transfer;
using Vfs.Transfer.Util;
using Vfs.Util;

namespace Vfs.LocalFileSystem.Transfer
{
  /// <summary>
  /// Manages downloads from the local file system.
  /// </summary>
  public class LocalDownloadHandler : DownloadHandlerBase<FileItem, LocalDownloadTransfer>
  {
    /// <summary>
    /// Provides access to file system functionality that is required
    /// to manage transfers.
    /// </summary>
    public LocalTransferConfig Config { get; private set; }


    /// <summary>
    /// Gets the maximum block size that can be transferred, if such
    /// a limit exists. Returns null in case of arbitrary block sizes.
    /// </summary>
    public override int? MaxBlockSize
    {
      get { return Config.FileSystemConfiguration.MaxDownloadBlockSize; }
    }

    /// <summary>
    /// Indicates how data blocks can be transmitted. The local file system
    /// allows random access to all blocks.
    /// </summary>
    public override TransmissionCapabilities TransmissionCapabilities
    {
      get { return TransmissionCapabilities.Random; }
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    /// <param name="config">Provides access to file system functionality
    /// that is required to manage transfers.</param>
    public LocalDownloadHandler(LocalTransferConfig config)
    {
      Ensure.ArgumentNotNull(config, "config");
      Config = config;
    }

    /// <summary>
    /// Initializes the service with a specific <see cref="ITransferStore{TTransfer}"/>
    /// that maintains running transfers.
    /// </summary>
    /// <param name="config">Provides access to file system functionality
    /// that is required to manage transfers.</param>
    /// <param name="transferStore">Provides a storage mechanism for
    /// managed transfers.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="transferStore"/>
    /// is a null reference.</exception>
    public LocalDownloadHandler(ITransferStore<LocalDownloadTransfer> transferStore,
                                          LocalTransferConfig config) : base(transferStore)
    {
      Ensure.ArgumentNotNull(config, "config");
      Config = config;
    }


    /// <summary>
    /// Resolves the submitted resource path and returns a matching
    /// file item. This process should include all steps that a regular
    /// request for a <see cref="VirtualFileInfo"/> includes, e.g. checking
    /// for file availability if requested.
    /// </summary>
    /// <param name="submittedResourceFilePath">Identifies the requested file resource.</param>
    /// <param name="mustExist">Whether the file is expected to exist on the file system. If
    /// this parameter is true, the implementing method should throw a
    /// <see cref="VirtualResourceNotFoundException"/> for the requested resource.
    /// </param>
    /// <param name="context">The file system operation that is being performed during the invocation of
    /// this method. Used for internal auditing.</param>
    /// <returns>A file item that represents the requested file resource.</returns>
    /// <exception cref="VirtualResourceNotFoundException">In case the submitted
    /// <paramref name="submittedResourceFilePath"/> does not match a known resource,
    /// and the <paramref name="mustExist"/> flag is true.</exception>
    /// <exception cref="ResourceAccessException">In case the requesting party is not
    /// authorized to access the resource.</exception>
    protected override FileItem CreateFileItemImpl(string submittedResourceFilePath, bool mustExist, FileSystemTask context)
    {
      return Config.FileResolverFunc(submittedResourceFilePath, mustExist, context);
    }


    /// <summary>
    /// Creates a transfer object for a given requested resource.
    /// </summary>
    /// <param name="submittedResourceFilePath">The resource identifier as submitted.</param>
    /// <param name="fileItem">Represents the requested file resource.</param>
    /// <param name="token">The token that is being issued for the transfer.</param>
    /// <param name="claims">The access rights for the resource.</param>
    /// <param name="lockGuard">File locks, if necessary. Can be a null reference
    /// if no locking takes place.</param>
    /// <param name="expirationJob">A scheduled job that invokes the
    /// <see cref="TransferHandlerBase{TFile,TToken,TTransfer}.OnTransferExpiration"/>"/>
    /// method once the transfer expires. May be null if the token does not expire.</param>
    /// <returns>A transfer object which encapsulates the information required to perform
    /// the transfer.</returns>
    protected override LocalDownloadTransfer CreateTransfer(string submittedResourceFilePath, FileItem fileItem,
                                                            DownloadToken token, FileClaims claims,
                                                            ResourceLockGuard lockGuard,
                                                            Job<DownloadToken> expirationJob)
    {
      var transfer = new LocalDownloadTransfer(token, fileItem) {Status = TransferStatus.Starting};

      if (expirationJob != null) transfer.ExpirationNotificationJob = expirationJob;
      if (lockGuard != null) transfer.ResourceLock = lockGuard;
      transfer.Owner = Config.Provider.Security.GetIdentity(); 

      return transfer;
    }


    /// <summary>
    /// Creates a token for the currently processed resource.
    /// </summary>
    /// <param name="submittedResourceFilePath">The identifier (file path)
    /// for the requested resource, as submitted by the requesting party.</param>
    /// <param name="fileItem">Represents the requested file.</param>
    /// <param name="clientMaxBlockSize">The maximum size of a downloadable block, as requested by
    /// the client. It is up to the implementing method to correct this value (e.g. in order
    /// to comply to the <see cref="TransferHandlerBase{TFile,TToken,TTransfer}.MaxBlockSize"/>
    /// of the service itself.</param>
    /// <param name="includeFileHash">Whether a file hash for the
    /// requested resource should be calculated and assigned to the
    /// <see cref="DownloadToken.Md5FileHash"/> property of the returned
    /// <see cref="DownloadToken"/>.</param>
    /// <returns>A token for the request.</returns>
    protected override DownloadToken CreateDownloadToken(string submittedResourceFilePath, FileItem fileItem,
                                                         int? clientMaxBlockSize, bool includeFileHash)
    {
      var fileInfo = fileItem.ResourceInfo;
      string transferId = Guid.NewGuid().ToString();

      //try to comply to the requested block size, if specified. Otherwise
      //apply the default
      int blockSize = clientMaxBlockSize.HasValue
                        ? Math.Min(clientMaxBlockSize.Value, MaxBlockSize ?? int.MaxValue)
                        : Config.FileSystemConfiguration.DefaultDownloadBlockSize;


      DownloadToken token = new DownloadToken
                           {
                             TransferId = transferId,
                             ResourceIdentifier = submittedResourceFilePath,
                             CreationTime = SystemTime.Now(),
                             ContentType = fileInfo.ContentType,
                             ResourceName = fileInfo.Name,
                             ResourceLength = fileInfo.Length,
                             DownloadBlockSize = blockSize
                           };

      TimeSpan? expiration = Config.FileSystemConfiguration.DownloadTokenExpirationTime;
      if(expiration.HasValue)
      {
        token.ExpirationTime = SystemTime.Now().Add(expiration.Value);
      }

      //calculate number of blocks
      long blockCount = token.ResourceLength/blockSize;
      if ((token.ResourceLength%token.DownloadBlockSize) != 0) blockCount++;
      token.TotalBlockCount = blockCount;

      if (includeFileHash)
      {
        token.Md5FileHash = fileItem.LocalFile.CalculateMd5Hash();
      }

      return token;
    }


    /// <summary>
    /// Gets the access rights for the requested file. This method
    /// is invoked in order to check whether the requesting party
    /// is allowed to read the resource.
    /// </summary>
    /// <param name="file">Represents the requested resource.</param>
    /// <returns>Access rights for the file.</returns>
    protected override FileClaims GetFileClaims(FileItem file)
    {
      return Config.ClaimsResolverFunc(file);
    }


    /// <summary>
    /// Locks the resource if necessary, and returns a <see cref="ResourceLockGuard"/>
    /// that contains all the required locks. The invoking method will check whether
    /// the lock was granted through the <see cref="ResourceLockGuard.IsLockEnabled"/>
    /// property and throw an exception if the lock was not available.<br/>
    /// If no locking is necessary, this method should just return a null reference.
    /// </summary>
    /// <param name="fileItem">Represents the requested file resource.</param>
    /// <param name="claims">The access rights for the resource as returned by
    /// <see cref="GetFileClaims"/>.</param>
    /// <returns>Locks for the file, or <c>null</c> if no locking is necessary.</returns>
    protected override ResourceLockGuard LockResourceForDownload(FileItem fileItem, FileClaims claims)
    {
      return Config.LockResolverFunc(fileItem, ResourceLockType.Read);
    }


    /// <summary>
    /// Performs housekeeping code once a transfer is paused. In case of the
    /// local file system, invoking this method closes the underlying file
    /// stream.
    /// </summary>
    /// <param name="transfer">The paused transfer.</param>
    protected override void PauseTransferImpl(LocalDownloadTransfer transfer)
    {
      CloseStream(transfer);
    }


    /// <summary>
    /// Cleans up a transfer and used FS specific resources after the transfer
    /// was closed. This method is being invoked during the executino of
    /// <see cref="TransferHandlerBase{TFile,TToken,TTransfer}.CloseTransferInternal"/>.
    /// This implementation closes the underlying file stream.
    /// </summary>
    /// <param name="status">The transfer status.</param>
    /// <param name="abortReason">Indicates why the transfer was aborted, in case the
    /// <paramref name="status"/> is <see cref="TransferStatus.Aborted"/>.</param>
    /// <param name="transfer">The closed transfer.</param>
    protected override void CloseTransferImpl(LocalDownloadTransfer transfer, TransferStatus status, AbortReason? abortReason)
    {
      CloseStream(transfer);
      transfer.ClearTransferredBlocks();
    }


    /// <summary>
    /// Handles the creation of an actual data block based on the underlying resource.
    /// </summary>
    /// <param name="transfer">The processed transfer.</param>
    /// <param name="blockNumber">The number of the downloaded block.</param>
    /// <param name="previouslyTransferredBlock">If this data block was already transferred, this parameter
    /// contains the information about the block. Can be used in order to ensure proper retransmission in case
    /// of variable block sizes.</param>
    /// <returns>A data block which contains the data as an in-memory buffer
    /// (<see cref="BufferedDataBlock.Data"/>).</returns>
    protected override BufferedDataBlock CreateBufferedDataBlockImpl(LocalDownloadTransfer transfer, long blockNumber, DataBlockInfo previouslyTransferredBlock)
    {
      //this func creates the returned DataBlock by reading a chunk of data
      //from the underlying stream.
      Func<long, BufferedDataBlock> func = position =>
                                             {
                                               DownloadToken token = transfer.Token;

                                               //read data
                                               byte[] data = new byte[token.DownloadBlockSize];
                                               int read = transfer.Stream.Read(data, 0, data.Length);
                                               if (read < token.DownloadBlockSize)
                                               {
                                                 Array.Resize(ref data, read);
                                               }

                                               return new BufferedDataBlock
                                                        {
                                                          TransferTokenId = transfer.TransferId,
                                                          BlockNumber = blockNumber,
                                                          BlockLength = data.Length,
                                                          Offset = position,
                                                          Data = data,
                                                          IsLastBlock = blockNumber == token.TotalBlockCount - 1
                                                        };
                                             };

      var dataBlock = PrepareAndRunBlockReading(transfer, blockNumber, func);

      if (dataBlock.IsLastBlock)
      {
        //assume the last block will be processed successfully and
        //already close the stream (don't wait for the transfer to be
        //closed by client). This would still reopen the stream if
        //necessary
        CloseStream(transfer);
      }

      return dataBlock;
    }



    /// <summary>
    /// Handles the creation of an actual data block based on the underlying resource.
    /// </summary>
    /// <param name="transfer">The processed transfer.</param>
    /// <param name="blockNumber">The number of the downloaded block.</param>
    /// <param name="previouslyTransferredBlock">If this data block was already transferred, this parameter
    /// contains the information about the block. Can be used in order to ensure proper retransmission in case
    /// of variable block sizes.</param>
    /// <returns>A data block which exposes the data as a resource-friendly stream
    /// (<see cref="BufferedDataBlock.Data"/>).</returns>
    protected override StreamedDataBlock CreateStreamedDataBlockImpl(LocalDownloadTransfer transfer, long blockNumber, DataBlockInfo previouslyTransferredBlock)
    {
      //this func creates the returned DataBlock by reading a chunk of data
      //from the underlying stream.
      Func<long, StreamedDataBlock> func = position =>
                                             {
                                               DownloadToken token = transfer.Token;

                                               //check if we can use the max block size
                                               long streamLength = transfer.Stream.Length;
                                               int blockLength = (int)Math.Min(token.DownloadBlockSize,
                                                                           streamLength - position);
                                               if (blockLength < 0) blockLength = 0;

                                               ChunkStream stream = new ChunkStream(transfer.Stream, blockLength,
                                                                                    position, false);


                                               return new StreamedDataBlock
                                                        {
                                                          TransferTokenId = transfer.TransferId,
                                                          BlockNumber = blockNumber,
                                                          BlockLength = blockLength,
                                                          Offset = position,
                                                          Data = stream,
                                                          IsLastBlock = blockNumber == token.TotalBlockCount - 1
                                                        };
                                             };

      return PrepareAndRunBlockReading(transfer, blockNumber, func);
    }


    /// <summary>
    /// Handles validation and stream preparation of a given transfer in order to read
    /// a given block of data. The actual reading (either into a buffer, or a returned
    /// stream) is being delegated via the <paramref name="dataReaderFunc"/>.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="IDataBlock"/> that is being returned.</typeparam>
    /// <param name="transfer">The processed transfer.</param>
    /// <param name="blockNumber">The block number to be read.</param>
    /// <param name="dataReaderFunc">A function that receives the designated stream offset,
    /// and returns the required <see cref="IDataBlock"/> that provides the block's data.</param>
    /// <returns>The <see cref="IDataBlock"/> that is being created by the <paramref name="dataReaderFunc"/>.</returns>
    private static T PrepareAndRunBlockReading<T>(LocalDownloadTransfer transfer, long blockNumber,
                                                  Func<long, T> dataReaderFunc) where T : IDataBlock
    {
      DownloadToken token = transfer.Token;
      long position = blockNumber*token.DownloadBlockSize;

      //in case of an invalid position, throw error
      if (position > token.ResourceLength)
      {
        string msg = "Cannot deliver block {0} - invalid block number (beyond actual file size).";
        msg = String.Format(msg, blockNumber);
        throw new DataBlockException(msg);
      }

      if (transfer.Stream == null)
      {
        //open stream, share full access (it's up to the application layer to restrict write access, VFS does not block)
        transfer.Stream = transfer.File.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
      }

      //reposition stream if necessary
      if (position != transfer.Stream.Position)
      {
        transfer.Stream.Seek(position, SeekOrigin.Begin);
      }

      T dataBlock = dataReaderFunc(position);
      return dataBlock;
    }


    /// <summary>
    /// Closes and disposes a transfer's <see cref="LocalDownloadTransfer.Stream"/>
    /// and sets the property to null.
    /// </summary>
    /// <param name="transfer">The processed transfer.</param>
    private static void CloseStream(LocalDownloadTransfer transfer)
    {
      if (transfer.Stream != null)
      {
        transfer.Stream.Dispose();
        transfer.Stream = null;
      }
    }
  }
}