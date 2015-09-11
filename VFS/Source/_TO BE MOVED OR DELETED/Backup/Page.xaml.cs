using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml;

using Liquid;
using Liquid.Components;

namespace ServerFileUpload
{
	public partial class Page : UserControl
	{
        private string _website = "http://localhost:15159/Website/";
		private Uploader _uploader;
		private Node _uploadingTo = null;
		private string _autoSelectNodeID = string.Empty;

		public Page()
		{
			InitializeComponent();

			_uploader = new Uploader(_website + "FileUploadService.asmx");
			testTree.BuildRoot();

			_uploader.UploadProgressChange += new UploadEventHandler(Uploader_UploadProgressChange);
			_uploader.UploadFinished += new UploadEventHandler(Uploader_UploadFinished);
		}

		private void TestTree_NodeExpanded(object sender, TreeEventArgs e)
		{
			Node result = ((Node)sender).Get(_autoSelectNodeID);

			if (result != null)
			{
				testTree.SetSelected(result);
			}
		}

		private void Items_DoubleClick(object sender, EventArgs e)
		{
            Node n;
			_autoSelectNodeID = items.Selected.LiquidTag.ToString();

            if (testTree.Selected != null)
            {
                if (testTree.Selected.IsExpanded)
                {
                    n = testTree.Get(_autoSelectNodeID);
                    testTree.SetSelected(n);
                }
                else
                {
                    testTree.Selected.Expand();
                }
            }
		}

		#region Tree Processing

		private void CreateFolder(Node workingNode)
		{
			string id = workingNode.ID + folderName.Text + "/";

			workingNode.HasChildren = true;

			if (workingNode.ChildrenLoaded)
			{
				workingNode.Nodes.Add(new Node(id, folderName.Text, false, "images/folder.png", "images/folderOpen.png", true));
				workingNode.Sort(Tree.SortActions.ContainersFirst);
			}

			testTree.Selected = testTree.Selected;
		}

		private void PopulateChildren(Node workingNode, string xml, bool populateNode)
		{
			XmlReader reader = XmlReader.Create(new StringReader(xml));
            List<ItemViewerItem> fileItems = new List<ItemViewerItem>();
			string id = string.Empty;
			int param = 0;
			string icon = string.Empty;
			string iconExpanded = string.Empty;

			if (populateNode)
			{
				workingNode.BulkUpdateBegin();
			}
			else
			{
				items.Clear();
			}

			while (reader.Read())
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
                    if (reader.Name == "UploaderFileNode")
					{
						while (reader.Read())
						{
							if (reader.NodeType == XmlNodeType.Element)
							{
								if (reader.Name == "ID")
								{
									id = reader.ReadInnerXml();
								}
								else if (reader.Name == "Param")
								{
									param = int.Parse(reader.ReadInnerXml());
								}
							}
                            if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "UploaderFileNode")
							{
								if (populateNode)
								{
									if (id.EndsWith("/"))
									{	// Indicates this node is a folder
										workingNode.Nodes.Add(new Node(id, GetTitle(id), (param > 0), "images/folder.png", "images/folderOpen.png", true) { EnableCheckboxes = true });
									}
								}
								else
								{
									FileItem item = new FileItem();

									item.Text = GetTitle(id);
									item.LiquidTag = id;

									if (id.EndsWith("/"))
									{
										item.Icon = "images/large/folder.png";
									}
									else
									{
										item.Icon = "images/large/" + GetIcon(id);
										item.OtherText = (Math.Round((double)param / 1024, 2)).ToString() + "KB";
									}

									fileItems.Add(item);
								}

								break;
							}
						}
					}
				}
			}

			reader.Close();

			if (populateNode)
			{
				workingNode.BulkUpdateEnd();
				workingNode.IsBusy = false;
			}
			else
			{
                items.Add(fileItems);
			}
		}

		private void Tree_NodeClick(object sender, EventArgs e)
		{
			Node node = (Node)sender;

			startUpload.IsEnabled = (node.Icon.EndsWith("folder.png"));
			createFolder.IsEnabled = startUpload.IsEnabled;

			BeginGetChildren(node);
		}

		private void Tree_Populate(object sender, TreeEventArgs e)
		{
			if (sender is Tree)
			{	// We are populating the root nodes collection
				((Tree)sender).Nodes.Add(new Node("documents/", "My Documents", true, "images/folder.png", "images/folderOpen.png", true));
			}
			else
			{	// Otherwise we are populating a node, add a node to say we are working
				((Node)sender).IsBusy = true;

				// Call the webservice
				BeginGetNodes((Node)sender);
			}
		}

		private void BeginGetChildren(Node node)
		{
            Comms helper = new Comms(new Uri(_website + "FileUploadService.asmx/GetFilesInFolder"), "POST", false, new KeyValuePair<string, string>("folder", HttpUtility.UrlEncode(node.ID)));
			helper.ResponseComplete += new HttpResponseCompleteEventHandler(EndGetChildren);
			helper.Execute(node);
		}

		private void EndGetChildren(HttpResponseCompleteEventArgs e)
		{
			PopulateChildren((Node)e.Tag, e.Response, false);
		}

		private void BeginGetNodes(Node node)
		{	// GetFilesInFolder
            Comms helper = new Comms(new Uri(_website + "FileUploadService.asmx/GetFoldersInFolder"), "POST", false, new KeyValuePair<string, string>("folder", HttpUtility.UrlEncode(node.ID)));
			helper.ResponseComplete += new HttpResponseCompleteEventHandler(EndGetNodes);
			helper.Execute(node);
		}

		private void EndGetNodes(HttpResponseCompleteEventArgs e)
		{
			PopulateChildren((Node)e.Tag, e.Response, true);
		}

		private void BeginNewFolder(string path, string folderName)
		{	// GetFilesInFolder
            Comms helper = new Comms(new Uri(_website + "FileUploadService.asmx/NewFolder"), "POST", false, new KeyValuePair<string, string>("path", HttpUtility.UrlEncode(path)),
				new KeyValuePair<string, string>("folderName", HttpUtility.UrlEncode(folderName)));
			helper.ResponseComplete += new HttpResponseCompleteEventHandler(EndNewFolder);
			helper.Execute(testTree.Selected);
		}

		private void EndNewFolder(HttpResponseCompleteEventArgs e)
		{
			if (e.Response.Contains("ok"))
			{
				CreateFolder((Node)e.Tag);
			}
		}

		private string GetTitle(string filename)
		{
			string[] split = filename.TrimEnd('/').Split('/');
			string title = filename;

			if (split.Length > 0)
			{
				title = split[split.Length - 1];
			}

			return title;
		}

		private string GetIcon(string filename)
		{
			string[] split = filename.Split('.');
			string extension = string.Empty;

			if (split.Length > 0)
			{
				extension = split[split.Length - 1].ToLower();
			}

			if (extension != "pdf" && extension != "xls" && extension != "doc" && extension != "gif" && extension != "mp3" &&
				extension != "ascx" && extension != "asmx" && extension != "aspx" && extension != "avi" && extension != "config" &&
				extension != "cs" && extension != "css" && extension != "htm" && extension != "html" && extension != "jpg" &&
				extension != "js" && extension != "mp4" && extension != "png" && extension != "txt" && extension != "xaml" &&
				extension != "xml" && extension != "zip")
			{
				extension = "unknown";
			}

			return extension + ".png";
		}

		#endregion

		#region Upload Processing

		private void UpdateUploadStatus(UploadEventArgs e)
		{
			int itemsUploaded = _uploader.ItemsUploaded + 1;

			if (itemsUploaded > _uploader.ItemsTotal)
			{
				itemsUploaded = _uploader.ItemsTotal;
			}

			itemsCopied.Text = "Copying file " + itemsUploaded.ToString() + " of " + _uploader.ItemsTotal.ToString();
			progressText.Text = "Uploading " + e.Text + "...";
			progressBar.Text = Math.Round(e.Progress) + "%";
			progressBar.Complete = e.Progress;

			CenterText();
		}

		private void Uploader_UploadFinished(object sender, UploadEventArgs e)
		{
			if (uploadProgress.IsOpen)
			{
				uploadProgress.Close();

				if (_uploadingTo != null)
				{
					testTree.Selected = testTree.Selected;

					_uploadingTo = null;
				}
			}
		}

		private void Uploader_UploadProgressChange(object sender, UploadEventArgs e)
		{
			UpdateUploadStatus(e);
		}

		private void Delete_Click(object sender, RoutedEventArgs e)
		{
		}

		private void StartUpload_Click(object sender, RoutedEventArgs e)
		{
			if (!_uploader.Busy)
			{
				OpenFileDialog dialog = new OpenFileDialog();
				string target = "documents/";

				dialog.Filter = "All files (*.*)|*.*";
				dialog.Multiselect = true;

				if (dialog.ShowDialog() == true)
				{
					_uploadingTo = testTree.Selected;

					target = _uploadingTo.ID;
                    _uploader.UploadData("myUpload", dialog.File.OpenRead(), dialog.File.Name, target, dialog.File.Name, true, "");
					//_uploader.UploadFiles("myUpload", dialog.Files, target, true, "");

					progressBar.Complete = 0;
					itemsCopied.Text = "Waiting to upload " + _uploader.ItemsTotal.ToString() + " file(s)";
					progressBar.Text = "0%";
					progressText.Text = "Contacting " + _website + " ...";

					CenterText();

					uploadProgress.Show();
				}
			}
		}

		private void CenterText()
		{
			itemsCopied.SetValue(Canvas.LeftProperty, (uploadProgress.Width - itemsCopied.ActualWidth) * 0.5);
			progressText.SetValue(Canvas.LeftProperty, (uploadProgress.Width - progressText.ActualWidth) * 0.5);
		}

		private void CreateFolder_Click(object sender, RoutedEventArgs e)
		{
			newFolder.Show();
		}

		private void NewFolder_Closed(object sender, EventArgs e)
		{
			if (newFolder.Result == DialogButtons.OK)
			{
				BeginNewFolder(testTree.Selected.ID, folderName.Text);
			}
		}

		private void UploadProgress_Closed(object sender, EventArgs e)
		{
			_uploader.Abort();
		}

		#endregion

        private void items_ItemSelected(object sender, EventArgs e)
        {

        }
	}
}
