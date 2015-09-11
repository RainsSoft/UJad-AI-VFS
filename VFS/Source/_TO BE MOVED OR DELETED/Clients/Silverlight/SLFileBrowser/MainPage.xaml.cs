using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Browser;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ServiceModel;
using Vfs.FileSystemService;
using System.IO;


namespace SLFileBrowser
{
  public partial class MainPage : UserControl
  {
    public MainPage()
    {
      InitializeComponent();
    }

    private void OnReceivingStream(IAsyncResult ar)
    {
      HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
      HttpWebResponse response =  (HttpWebResponse)request.EndGetResponse(ar);

      //use default byte sizes
      byte[] buffer = new byte[32768];

      var source = response.GetResponseStream();
      int block = 0;
      while (true)
      {
        int bytesRead = source.Read(buffer, 0, buffer.Length);
        if (bytesRead > 0)
        {
          textBlock1.Text = String.Format("Read block {0} with length {1}", block++, bytesRead);
        }
        else
        {
          textBlock1.Text = "Completed download. Read blocks: " + block;
          break;
        }
      }
    }




    private void button1_Click(object sender, RoutedEventArgs e)
    {
      var uri = new Uri("http://localhost:6744/FileSystem.svc/webdownload/download?file=iso.iso");
      //var request = (HttpWebRequest)WebRequestCreator.ClientHttp.Create(uri);
      //request.BeginGetResponse(OnReceivingStream, request);

      WebClient client = new WebClient {AllowReadStreamBuffering = false};
      client.OpenReadCompleted += OnReadCompleted;
      client.OpenReadAsync(uri);

    }

    private void OnReadCompleted(object sender, OpenReadCompletedEventArgs e)
    {
      ThreadPool.QueueUserWorkItem(cb =>
                                     {
                                       //use default byte sizes
                                       byte[] buffer = new byte[32768];
                                       int block = 0;
                                       var source = e.Result;
                                       try
                                       {
                                         while (true)
                                         {
                                           int bytesRead = source.Read(buffer, 0, buffer.Length);
                                           if (bytesRead > 0)
                                           {
                                             int blockNumber = block++;
                                             if(blockNumber % 500 == 0) continue;

                                             Dispatcher.BeginInvoke(() =>
                                                                      textBlock1.Text =
                                                                          String.Format(
                                                                            "Read block {0} with length {1}", blockNumber,
                                                                            bytesRead));
                                           }
                                           else
                                           {
                                             Dispatcher.BeginInvoke(() =>
                                                                    textBlock1.Text =
                                                                    "Completed download. Read blocks: " + block);
                                             break;
                                           }
                                         }
                                       }
                                       catch (Exception exception)
                                       {
                                         textBlock1.Text = exception.ToString();
                                       }
                                     });



      //while (true)
      //{
      //  int bytesRead = source.Read(buffer, 0, buffer.Length);
      //  if (bytesRead > 0)
      //  {
      //    textBlock1.Text = String.Format("Read block {0} with length {1}", block++, bytesRead);
      //  }
      //  else
      //  {
      //    textBlock1.Text = "Completed download. Read blocks: " + block;
      //    break;
      //  }
      //}
    }
  }
}
