using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Vfs;
using Vfs.Restful.Client;

namespace Silverlight_File_Browser
{
  public partial class MainPage : UserControl
  {
    private FileSystemFacade facade;

    public MainPage()
    {
      InitializeComponent();
      InitFs();
    }

    private void InitFs()
    {
      Uri serviceBaseUri = Application.Current.Host.Source;
      serviceBaseUri = new Uri(serviceBaseUri, "/");

      facade = new FileSystemFacade(serviceBaseUri.ToString());

      Dispatcher.RunAsync(() => VirtualFolder.CreateRootFolder(facade).GetFolders(), f => directories.ItemsSource = f);
    }



  }


}
