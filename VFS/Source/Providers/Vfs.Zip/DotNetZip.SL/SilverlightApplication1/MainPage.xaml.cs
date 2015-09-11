using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SilverlightApplication1
{
  public partial class MainPage : UserControl
  {
    public MainPage()
    {
      InitializeComponent();

      var encoding = System.Text.Encoding.GetEncoding("IBM437");
      byte[] bytes = encoding.GetBytes("hello world");
      txt1.Text = encoding.GetString(bytes, 0, bytes.Length);
    }
  }
}
