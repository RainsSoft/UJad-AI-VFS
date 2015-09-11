using System;
using System.IO;

namespace Vfs.Transfer.Util
{
  /// <summary>
  /// A stream that decorates another stream, and executes a given
  /// action as soon as it is being disposed.
  /// </summary>
  public class ClosingActionStream : Stream
  {
    public Stream Decorated { get; private set; }
    public Action ClosingAction { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.IO.Stream"/> class. 
    /// </summary>
    /// <param name="decorated">The decorated stream.</param>
    /// <param name="closingAction">An action that is being executed as soon as
    /// the stream is closed and/or disposed.</param>
    public ClosingActionStream(Stream decorated, Action closingAction)
    {
      Decorated = decorated;
      ClosingAction = closingAction;
    }

    public override void Flush()
    {
      Decorated.Flush();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      return Decorated.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
      Decorated.SetLength(value);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      return Decorated.Read(buffer, offset, count);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      Decorated.Write(buffer, offset, count);
    }

    public override bool CanRead
    {
      get { return Decorated.CanRead; }
    }

    public override bool CanSeek
    {
      get { return Decorated.CanSeek; }
    }

    public override bool CanWrite
    {
      get { return Decorated.CanWrite; }
    }

    public override long Length
    {
      get { return Decorated.Length; }
    }

    public override long Position
    {
      get { return Decorated.Position; }
      set { Decorated.Position = value; }
    }


    protected override void Dispose(bool disposing)
    {
      if(disposing)
      {
        Decorated.Dispose();
        ClosingAction();
      }

      base.Dispose(disposing);
    }
  }
}
