using System;
using System.Linq;
using Vfs;

namespace Vfs.Samples.LocalClient.ViewModel
{
  public class MainViewModel : ViewModelBase
  {

    #region FileSystem

    /// <summary>
    /// The used file system. Setting it also updates the root folder.
    /// </summary>
    private IFileSystemProvider fileSystem;


    /// <summary>
    /// The used file system. Setting it also updates the root folder.
    /// </summary>
    /// <exception cref="ArgumentNullException">If <paramref name="value"/>
    /// is a null reference.</exception>
    public IFileSystemProvider FileSystem
    {
      get { return fileSystem; }
      set
      {
        if (value == null) throw new ArgumentNullException("value");
        
        fileSystem = value;
        OnPropertyChanged("FileSystem");

        //set the root, and select it
        var vf = new VirtualFolder(fileSystem, fileSystem.GetFileSystemRoot());
        RootFolder = new FolderViewModel(vf);

        ActiveParentFolder = RootFolder;
      }
    }

    #endregion


    #region RootFolder

    private FolderViewModel rootFolder;


    /// <summary>
    /// The root of the used file system. Setting the root also
    /// resets the <see cref="SelectedResource"/>.
    /// </summary>
    public FolderViewModel RootFolder
    {
      get { return rootFolder; }
      set
      {
        rootFolder = value;
        OnPropertyChanged("RootFolder");
      }
    }

    #endregion


    #region ActiveParentFolder

    private FolderViewModel activeParentFolder;


    /// <summary>
    /// The currently selected folder in the navigation area.
    /// </summary>
    public FolderViewModel ActiveParentFolder
    {
      get { return activeParentFolder; }
      set
      {
        activeParentFolder = value;
        OnPropertyChanged("ActiveParentFolder");

        var childFolder = activeParentFolder.Folders.FirstOrDefault();
        if (childFolder != null)
        {
          SelectedResource = childFolder;
          return;
        }

        var childFile = activeParentFolder.Files.FirstOrDefault();
        if (childFile != null)
        {
          SelectedResource = childFile;
          return;
        }

        SelectedResource = null;
      }
    }

    #endregion


    #region SelectedResource

    private IResourceViewModel selectedResource;


    /// <summary>
    /// The currently selected file or folder.
    /// </summary>
    public IResourceViewModel SelectedResource
    {
      get { return selectedResource; }
      set
      {
        selectedResource = value;
        OnPropertyChanged("SelectedResource");
      }
    }

    #endregion
    
  }
}
