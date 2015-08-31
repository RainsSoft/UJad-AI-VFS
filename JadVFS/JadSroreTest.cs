using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JadEngine.VFS.Storage;
using JadEngine.VFS.Storage.Filters;

namespace JadEngine.VFS 
{
   /// <summary>
   ///来源文件树形结构
   /// </summary>
    public class JStoreFileTreeNode
    { 
        //List<T>  
        public JStoreFileTreeNode Parent;
        public JStoreFileTreeNode[] Nodes;
        public List<JSoreFileItemInfo> Tag;
        /// <summary>
        /// 当前层目录名称
        /// </summary>
        public string DirName { get; set; }
    }
    /// <summary>
    /// 来源文件的具体信息
    /// </summary>
    public class JSoreFileItemInfo {
        #region Fields

        /// <summary>
        /// Information about the file.
        /// </summary>
        private JFileInfo _info;

        /// <summary>
        /// List of filter of the file.
        /// </summary>
        private List<IJFilter> _filters;

        /// <summary>
        /// Length of the file.
        /// </summary>
        private long _length;

        /// <summary>
        /// Path of the file.
        /// </summary>
        private string _path;

        #endregion
        #region Properties
        public string fileName; 
        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        public string FileName {
            get { return fileName; }
        }

        /// <summary>
        /// Gets the file info.
        /// </summary>
        public JFileInfo Info {
            get { return _info; }
        }

        /// <summary>
        /// Gets the file type.
        /// </summary>
        public JFileType Type {
            get { return _info.Type; }
        }

        /// <summary>
        /// Gets the file length.
        /// </summary>
        public long Length {
            get { return _length; }
        }

        /// <summary>
        /// Gets the file path.
        /// </summary>
        public string Path {
            get { return _path; }
        }

        public List<IJFilter> Filters {
            get { return _filters; }
            set {
                string filtersString = string.Empty;

                _filters.Clear();
                _filters.AddRange(value);

                foreach (IJFilter filter in _filters)
                    filtersString += filter.ShortName + " ";

                //SubItems[3].Text = filtersString.Trim();
            }
        }

        #endregion


        public JSoreFileItemInfo(FileInfo file) {
            _info = new JFileInfo(file);
            _length = file.Length;
            _path = file.Directory.FullName;
            _filters = new List<IJFilter>();            
            // Create the view data
            fileName = file.Name;
        }
    }
    public class JadSroreHelper
    {
        /// <summary>
        /// 当前存储VFS的对象，外面赋值
        /// </summary>
        public JStorageSource JadStorePack;
        public JadSroreHelper() {
           
        }
        /// <summary>
        /// 打包并创建一个VFS文件
        /// </summary>
        /// <param name="storepackerName">要保存的VFS文件名</param>
        /// <param name="rootNode">要打包的文件来源(本地磁盘的)</param>
        /// <param name="filters">加密列表,可为null</param>
        /// <returns></returns>
        public JStorageSource CreateJadStorePacker(string storepackerName, JStoreFileTreeNode rootNode, List<IJFilter> filters) {
            return CreateStorage(storepackerName,1,"",filters,rootNode,null);            
        }

        /// <summary>
        /// Creates a new storage.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="version">Version of the storage.</param>
        /// <param name="name">Name of the storage.</param>
        /// <param name="filters">Filters used in the storage.</param>
        /// <param name="rootNode">Root node of the storage.</param>
        /// <param name="oldStorage">Old storage (it can contain some files used in the new storage).</param>
        /// <returns>A new storage.</returns>
        public static JStorageSource CreateStorage(string fileName, int version, string name, List<IJFilter> filters, JStoreFileTreeNode rootNode, JStorageSource oldStorage) {
            JDirectory rootDirectory;
            JStorageSource newStorage;

            // Create the directories from the treeview
            rootDirectory = new JDirectory("Root");
            CreateDirectories(rootDirectory, rootNode);

            // Create the storage
            newStorage = new JStorageSource(name, version, filters, rootDirectory);

            // Write it
            newStorage.Write(fileName, oldStorage);

            return newStorage;
        }

        /// <summary>
        /// Creates a set of directories based on a treenode information.
        /// </summary>
        /// <param name="parentDirectory">Parent directory.</param>
        /// <param name="treeNode">Tree node to map.</param>
        public static void CreateDirectories(JDirectory parentDirectory, JStoreFileTreeNode treeNode) {
            //List<FileListViewItem> fileItems;
            JFile file;
            JDirectory childDirectory;
            List<JSoreFileItemInfo> fileItems = treeNode.Tag;
            // Create the files of the directory
            //fileItems = (List<FileListViewItem>)treeNode.Tag;
            foreach (JSoreFileItemInfo fileItem in fileItems) {
                file = new JFile(fileItem.fileName);
                file.Filters = fileItem.Filters;
                file.FileInfo = fileItem.Info;

                parentDirectory.AddFile(file);
            }

            // Create the subdirectories
            foreach (JStoreFileTreeNode node in treeNode.Nodes) {
                childDirectory = new JDirectory(node.DirName);
                parentDirectory.AddDirectory(childDirectory);

                CreateDirectories(childDirectory, node);
            }
        }
        public JStorageSource OpenJadStorePacker(string storepackerFileName) {
            return new JStorageSource("", storepackerFileName); ;
        }
       
        
        public void Extract(string ExtractAllToPath) { 
            JadStorePack.ExtractFiles(ExtractAllToPath);
        }

    }
}
