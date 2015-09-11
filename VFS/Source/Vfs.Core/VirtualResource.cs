using System;

namespace Vfs
{
  /// <summary>
  /// Base class for virtual resources, which provides transparent access to the
  /// resource.
  /// </summary>
  public abstract class VirtualResource<T> where T:VirtualResourceInfo
  {
    /// <summary>
    /// The underlying file system.
    /// </summary>
    public IFileSystemProvider Provider { get; private set; }

    /// <summary>
    /// Provides meta data for the resource.
    /// </summary>
    public T MetaData { get; protected set; }


    /// <summary>
    /// Creates the resource.
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="metaData">The corresponding meta data object that represents
    /// the file or folder information on the file system.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="provider"/>
    /// is a null reference.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="metaData"/>
    /// is a null reference.</exception>
    protected VirtualResource(IFileSystemProvider provider, T metaData)
    {
      if (provider == null) throw new ArgumentNullException("provider");
      if (metaData == null) throw new ArgumentNullException("metaData");

      Provider = provider;
      MetaData = metaData;
    }


    /// <summary>
    /// Refreshes the underlying <see cref="MetaData"/>
    /// by requerying the file system.
    /// </summary>
    public abstract void RefreshMetaData();

    /// <summary>
    /// Gets the parent folder of the resource.
    /// </summary>
    /// <returns>The parent of the resource.</returns>
    /// <exception cref="VirtualResourceNotFoundException">If the resource that is represented
    /// by this object does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of an invalid or prohibited
    /// resource access.</exception>
    public abstract VirtualFolder GetParentFolder();


    /// <summary>
    /// Checks whether the resource exists on the file system or not.
    /// </summary>
    public abstract bool Exists { get; }
  }
}