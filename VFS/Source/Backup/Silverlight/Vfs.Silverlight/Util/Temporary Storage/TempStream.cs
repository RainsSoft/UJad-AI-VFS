using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Vfs.Util.TemporaryStorage
{
  /// <summary>
  /// An abstract base class for a stream that provides read and write
  /// access to temporary data, and cleans up after itself.
  /// </summary>
  public abstract class TempStream : StreamDecorator
  {
    /// <summary>
    /// The stream from which the data is read. The first invocation of
    /// this property requests the creation of a temporary strea, through
    /// the <see cref="CreateTempDataStream"/> method.
    /// </summary>
    public override Stream DecoratedStream
    {
      get
      {
        return base.DecoratedStream ?? (DecoratedStream = CreateTempDataStream());
      }
      protected set
      {
        base.DecoratedStream = value;
      }
    }

    
    /// <summary>
    /// Checks whether the underlying temporary stream has been
    /// created already. This method is used rather than testing
    /// the <see cref="DecoratedStream"/> property, because invoking
    /// the property creates the stream on demand.
    /// </summary>
    protected bool IsTempStreamCreated
    {
      get { return base.DecoratedStream == null; }
    }

    
    /// <summary>
    /// Creates the underlying stream. This method is being invoked on the first
    /// request of the <see cref="DecoratedStream"/> property.
    /// </summary>
    /// <returns>A temporary stream that supports seeking.</returns>
    protected abstract Stream CreateTempDataStream();


    /// <summary>
    /// When invoked (which happens during disposal), implementing classes
    /// should clean up temporary resources. Note that at the time this
    /// method is being invoked, the <see cref="DecoratedStream"/> has
    /// already been disposed.
    /// </summary>
    protected abstract void DiscardTempResources();


    /// <summary>
    /// Implementing classes may override this method in order to release
    /// resources if the temporary data is not needed for a while but should
    /// not be discarded yet.<br/>
    /// The base implementation does not perform any work on invocation.
    /// </summary>
    public virtual void Pause()
    {
      //do nothing
    }


    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      DiscardTempResources();
    }

  }
}
