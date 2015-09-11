using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Vfs.Silverlight.FileBrowser.ViewModel
{
  public class FolderViewModel
  {
    public VirtualFolder Model { get; set; }

    public ObservableCollection<FolderViewModel> ChildFolders { get; set; }

    public bool IsSelected { get; set; }

    public bool IsExpanded { get; set; }

  }
}
