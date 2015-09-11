using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Vfs;

namespace Vfs.Samples.LocalClient.ViewModel
{
  /// <summary>
  /// View model base class for resources.
  /// </summary>
  public abstract class ResourceViewModel<T, T2> : ViewModelBase, IResourceViewModel where T:VirtualResource<T2> where T2:VirtualResourceInfo
  {
    public T Resource { get; private set; }

    protected ResourceViewModel(T resource)
    {
      Resource = resource;
    }

    public VirtualResourceInfo ResourceInfo
    {
      get { return Resource.MetaData; }
    }


    #region ParentFolder

    /// <summary>
    /// The parent of the resource.
    /// </summary>
    private VirtualFolder parentFolder;


    /// <summary>
    /// The parent of the resource.
    /// </summary>
    public virtual VirtualFolder ParentFolder
    {
      get
      {
        if (parentFolder == null)
        {
          ParentFolder = Resource.GetParentFolder();
        }

        return parentFolder;
      }
      set
      {
        parentFolder = value;
        OnPropertyChanged("ParentFolder");
      }
    }

    #endregion


    /// <summary>
    /// Refreshes the view model item in order to reflect the
    /// current contents of the file system.
    /// </summary>
    public abstract void Refresh();

  }
}
