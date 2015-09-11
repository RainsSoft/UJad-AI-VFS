using System;
using System.IO;
using System.ServiceModel;

namespace Vfs.Transfer
{



  /// <summary>
  /// A message contract that provides streaming capabilities rather
  /// than submitting the whole data as a buffer.
  /// </summary>
  [MessageContract]
  public class StreamedDataBlock : DataBlockInfo
  {
    [MessageBodyMember(Order = 0)]
    public Stream Data { get; set; }

    /// <summary>
    /// Identifies the <see cref="TransferToken"/> to which
    /// this file block belongs to.
    /// </summary>
    [MessageHeader(MustUnderstand = true)]
    public override string TransferTokenId { get; set; }

    /// <summary>
    /// The transferred block number.
    /// </summary>
    [MessageHeader(MustUnderstand = true)]
    public override long BlockNumber { get; set; }

    /// <summary>
    /// The resource's offset that defines the starting
    /// point of the block.
    /// </summary>
    [MessageHeader(MustUnderstand = true)]
    public override long Offset { get; set; }

    /// <summary>
    /// Whether this is the last block that completes
    /// the transmission.
    /// </summary>
    [MessageHeader(MustUnderstand = true)]
    public override bool IsLastBlock { get; set; }

    /// <summary>
    /// Thex submitted block length. This is the length
    /// of the block data (stream length or the length of the
    /// <see cref="BufferedDataBlock.Data"/> buffer). In case of an upload
    /// scenario, it should not be bigger than the
    /// <see cref="UploadToken.MaxBlockSize"/>.
    /// </summary>
    [MessageHeader(MustUnderstand = true)]
    public override long BlockLength { get; set; }

  }


  /// <summary>
  /// A partial block of data of a given transfer.
  /// </summary>
  public class BufferedDataBlock : DataBlockInfo
  {
    /// <summary>
    /// The submitted file data, which should match the
    /// <see cref="DataBlockInfo.BlockLength"/> property. In case of an upload
    /// scenario, the block size should not exceed the maximum
    /// block size as indicated by the <see cref="UploadToken.MaxBlockSize"/>
    /// property.
    /// </summary>
    public byte[] Data { get; set; }
  }


  /// <summary>
  /// Meta data about a given block of data which is part of a resource transfer.
  /// </summary>
  public interface IDataBlockInfo
  {
    /// <summary>
    /// Identifies the <see cref="TransferToken"/> to which
    /// this file block belongs to.
    /// </summary>
    string TransferTokenId { get; set; }

    /// <summary>
    /// The transferred block number.
    /// </summary>
    long BlockNumber { get; set; }

    /// <summary>
    /// The resource's offset that defines the starting
    /// point of the block.
    /// </summary>
    long Offset { get; set; }

    /// <summary>
    /// Whether this is the last block that completes
    /// the transmission.
    /// </summary>
    bool IsLastBlock { get; set; }

    /// <summary>
    /// The submitted block length. This is the length
    /// of the block data (stream length or the length of the
    /// <see cref="BufferedDataBlock.Data"/> buffer). In case of an upload
    /// scenario, it should not be bigger than the
    /// <see cref="UploadToken.MaxBlockSize"/>.
    /// </summary>
    long BlockLength { get; set; }
  }

  /// <summary>
  /// Encapsulates a partial block of data of a given transfer.
  /// </summary>
  public class DataBlockInfo : IDataBlockInfo
  {
    /// <summary>
    /// Identifies the <see cref="TransferToken"/> to which
    /// this file block belongs to.
    /// </summary>
    public virtual string TransferTokenId { get; set; }

    /// <summary>
    /// The transferred block number.
    /// </summary>
    public virtual long BlockNumber { get; set; }

    /// <summary>
    /// The resource's offset that defines the starting
    /// point of the block.
    /// </summary>
    public virtual long Offset { get; set; }

    /// <summary>
    /// Whether this is the last block that completes
    /// the transmission.
    /// </summary>
    public virtual bool IsLastBlock { get; set; }

    /// <summary>
    /// The submitted block length. This is the length
    /// of the block's data (stream length or the length of the
    /// <see cref="BufferedDataBlock.Data"/> buffer). In case of an upload
    /// scenario, it should not be bigger than the
    /// <see cref="UploadToken.MaxBlockSize"/>.
    /// </summary>
    public virtual long BlockLength { get; set; }


    /// <summary>
    /// Creates an independent <see cref="DataBlockInfo"/> instance
    /// base on another one.
    /// </summary>
    /// <param name="other">The object to copy.</param>
    /// <returns>Independent <see cref="DataBlockInfo"/> whose
    /// properties match the submitted instance.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="other"/>
    /// is a null reference.</exception>
    public static DataBlockInfo FromDataBlock(IDataBlockInfo other)
    {
      if (other == null) throw new ArgumentNullException("other");

      return new DataBlockInfo
               {
                 TransferTokenId = other.TransferTokenId,
                 BlockNumber = other.BlockNumber,
                 BlockLength = other.BlockLength,
                 Offset = other.Offset,
                 IsLastBlock = other.IsLastBlock
               };
    }

  }
}
