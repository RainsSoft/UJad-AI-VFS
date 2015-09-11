using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Vfs.Util
{
  /// <summary>
  /// A stream decorator that defers stream building as long as the
  /// <see cref="DecoratedStream"/> in not used.
  /// </summary>
  public class DeferredStream : StreamDecorator
  {
    public override Stream DecoratedStream
    {
      get
      {
        return base.DecoratedStream ?? (DecoratedStream = StreamBuilderFunc());
      }
      protected set
      {
        base.DecoratedStream = value;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public Func<Stream> StreamBuilderFunc { get; private set; }

    /// <summary>
    /// Creates the deferred stream with a builder function that is
    /// being invokes as soon as the stream is required.
    /// </summary>
    public DeferredStream(Func<Stream> streamBuilderFunc)
    {
      Ensure.ArgumentNotNull(streamBuilderFunc, "streamBuilderFunc");
      StreamBuilderFunc = streamBuilderFunc;
    }
  }
}
