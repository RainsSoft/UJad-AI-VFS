using System.IO;
using Vfs.Util;

namespace Vfs.Restful.Server.Util
{
  /// <summary>
  /// Decorates a given <see cref="Stream"/>, but always indicates
  /// the stream is not seekable by having the <see cref="CanSeek"/>
  /// property return false.
  /// </summary>
  public class NonSeekableStream : StreamDecorator
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.IO.Stream"/> class. 
    /// </summary>
    public NonSeekableStream(Stream decoratedStream) : base(decoratedStream)
    {
    }

    public override bool CanSeek
    {
      get { return false; }
    }
  }
}