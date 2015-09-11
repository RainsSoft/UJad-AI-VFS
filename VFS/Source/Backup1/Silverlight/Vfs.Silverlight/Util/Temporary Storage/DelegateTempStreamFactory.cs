using System;
using System.IO;


namespace Vfs.Util.TemporaryStorage
{
  /// <summary>
  /// A factory implementation which itself delegates the
  /// creation of <see cref="TempStream"/> instances through
  /// a delegate.
  /// </summary>
  public class DelegateTempStreamFactory : ITempStreamFactory
  {
    /// <summary>
    /// A builder function that creates a new <see cref="TempStream"/>
    /// instance, which can be returned by the factory's <see cref="CreateTempStream"/>
    /// method.
    /// </summary>
    public Func<TempStream> BuilderFunc { get; private set; }


    /// <summary>
    /// Creates the factory with a builder function that creates the
    /// <see cref="TempStream"/> instances that are returned by the
    /// factory.
    /// </summary>
    /// <param name="builderFunc">A builder function that creates
    /// new <see cref="TempStream"/> instances on demand.</param>
    public DelegateTempStreamFactory(Func<TempStream> builderFunc)
    {
      Ensure.ArgumentNotNull(builderFunc, "builderFunc");
      BuilderFunc = builderFunc;
    }


    /// <summary>
    /// Gets a helper class that maintains temporary storage to read or
    /// write transient data.
    /// </summary>
    /// <returns>Temporary storage.</returns>
    public TempStream CreateTempStream()
    {
      return BuilderFunc();
    }
  }
}
