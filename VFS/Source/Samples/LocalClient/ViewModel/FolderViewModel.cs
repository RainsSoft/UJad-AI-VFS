using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Vfs;

namespace Vfs.Samples.LocalClient.ViewModel
{
  public class FolderViewModel : ResourceViewModel<VirtualFolder, VirtualFolderInfo>
  {
    #region Folders

    /// <summary>
    /// The underlying child folders.
    /// </summary>
    private ObservableCollection<FolderViewModel> folders = null;

    /// <summary>
    /// The underlying child folders.
    /// </summary>
    public ObservableCollection<FolderViewModel> Folders
    {
      get
      {
        if (folders == null)
        {
          //load on demand
          LoadFolders();
        }

        return folders;
      }
      private set
      {
        folders = value;
        OnPropertyChanged("Folders");
      }
    }

    private void LoadFolders()
    {
      var col = Resource.GetFolders().Select(f => new FolderViewModel(f));
      Folders = new ObservableCollection<FolderViewModel>(col);
    }

    #endregion


    #region Files

    /// <summary>
    /// Gets the files of the folder.
    /// </summary>
    private ObservableCollection<FileViewModel> files = null;


    /// <summary>
    /// Gets the files of the folder.
    /// </summary>
    public ObservableCollection<FileViewModel> Files
    {
      get
      {
        if (files == null)
        {
          //load on demand
          LoadFiles();
        }

        return files;
      }
      private set
      {
        files = value;
        OnPropertyChanged("Files");
      }
    }

    private void LoadFiles()
    {
      var col = Resource.GetFiles().Select(f => new FileViewModel(f));
      Files = new ObservableCollection<FileViewModel>(col);
    }

    #endregion


    public FolderViewModel(VirtualFolder resource) : base(resource)
    {
      resource.RefreshMetaData();
    }

    /// <summary>
    /// Refreshes the view model item in order to reflect the
    /// current contents of the file system.
    /// </summary>
    public override void Refresh()
    {
      Resource.RefreshMetaData();

      if (folders != null)
      {
        LoadFolders();
        foreach (var childFolder in Folders)
        {
          childFolder.Refresh();
        }
      }

      if (files != null)
      {
        LoadFiles();
      }
    }
  }
}