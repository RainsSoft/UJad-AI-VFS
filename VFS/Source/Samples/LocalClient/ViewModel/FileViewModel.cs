using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vfs;

namespace Vfs.Samples.LocalClient.ViewModel
{
  public class FileViewModel : ResourceViewModel<VirtualFile, VirtualFileInfo>
  {
    public FileViewModel(VirtualFile resource) : base(resource)
    {
    }

    /// <summary>
    /// Refreshes the view model item in order to reflect the
    /// current contents of the file system.
    /// </summary>
    public override void Refresh()
    {
      Resource.RefreshMetaData();
    }
  }
}