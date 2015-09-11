using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Vfs.Transfer.Util
{
  public class StreamedBlockOutputStream : BufferedBlockOutputStream
  {
    public new Action<StreamedDataBlock> OutputAction { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.IO.Stream"/> class that initiates
    /// a new upload (starting with block 0 at offset 0).
    /// </summary>
    /// <param name="token">The upload token that identifies the transfer.</param>
    /// <param name="autoFlushThreshold">A threshold that causes the stream to aumatically flush
    /// its internal buffer once it reaches the defined size.<br/>
    /// If the threshold is 0, every write is immediately flushed.</param>
    /// <param name="outputAction">An action that is being invoked with created
    /// <see cref="BufferedDataBlock"/> instances when flushing the internal buffer.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="token"/> or <paramref name="outputAction"
    /// is a null reference.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Inf case the <paramref name="autoFlushThreshold"/>
    /// is negative.</exception>
    public StreamedBlockOutputStream(UploadToken token, int autoFlushThreshold, Action<StreamedDataBlock> outputAction)
      : base(token, autoFlushThreshold, b => { })
    {
      OutputAction = outputAction;
      base.OutputAction = ParseBuffer;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.IO.Stream"/> class. 
    /// </summary>
    /// <param name="token">The upload token that identifies the transfer.</param>
    /// <param name="lastTransmittedBlockNumber">The last transferred block number, if this stream
    /// is created for an upload that is already in progress. Can be set along with the <paramref name="startOffset"/> in
    /// order to resume an upload. Defaults to null in case of an initial upload.</param>
    /// <param name="startOffset">The initial offset of the upload. Can be set along with the
    /// <paramref name="lastTransmittedBlockNumber"/> in order to resume an upload. Defaults to
    /// zero in case of an initial upload.</param>
    /// <param name="autoFlushThreshold">A threshold that causes the stream to aumatically flush
    /// its internal buffer once it reaches the defined size.<br/>
    /// If the threshold is 0, every write is immediately flushed.</param>
    /// <param name="outputAction">An action that is being invoked with created
    /// <see cref="BufferedDataBlock"/> instances when flushing the internal buffer.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="token"/> or <paramref name="outputAction"
    /// is a null reference.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Inf case the <paramref name="autoFlushThreshold"/>
    /// is negative.</exception>
    public StreamedBlockOutputStream(UploadToken token, int autoFlushThreshold, long startOffset,
                                     long? lastTransmittedBlockNumber, Action<StreamedDataBlock> outputAction)
      : base(token, autoFlushThreshold, startOffset, lastTransmittedBlockNumber, b => { })
    {
      OutputAction = outputAction;
      base.OutputAction = ParseBuffer;
    }



    /// <summary>
    /// Parses the received buffer and wraps it into a memory stream.
    /// </summary>
    /// <param name="bufferedBlock">A buffered block.</param>
    private void ParseBuffer(BufferedDataBlock bufferedBlock)
    {
      StreamedDataBlock streamedBlock = new StreamedDataBlock
                                          {
                                            TransferTokenId = bufferedBlock.TransferTokenId,
                                            BlockLength = bufferedBlock.BlockLength,
                                            BlockNumber = bufferedBlock.BlockNumber,
                                            IsLastBlock = bufferedBlock.IsLastBlock,
                                            Offset = bufferedBlock.Offset,
                                            Data = new MemoryStream(bufferedBlock.Data)
                                          };
      OutputAction(streamedBlock);
    }
  }
}