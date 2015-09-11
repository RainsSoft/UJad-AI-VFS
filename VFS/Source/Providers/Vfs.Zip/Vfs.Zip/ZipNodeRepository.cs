using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Ionic.Zip;

namespace Vfs.Zip
{
  /// <summary>
  /// Provides a high-level, hierarchical view on the entries within a given ZIP file.
  /// Rather than processing the entries in the <see cref="ZipFile"/> all the time, builder
  /// methods of the provider rely on this class in order to get their items.
  /// </summary>
  public class ZipNodeRepository : IDisposable
  {
    /// <summary>
    /// The <see cref="VirtualResourceInfo.FullName"/> of the root folder
    /// as created through the <see cref="CreateRootItem"/> method.
    /// </summary>
    public const string RootFolderPath = "/";

    /// <summary>
    /// A token that is used to synchronized fields.
    /// </summary>
    private readonly object writeToken = new object();

    /// <summary>
    /// The file system configuration.
    /// </summary>
    public ZipFileSystemConfiguration Configuration { get; private set; }


    /// <summary>
    /// Represents the encapsulated zip file.
    /// </summary>
    public ZipFile ZipFile { get; private set; }


    /// <summary>
    /// A virtual node that represents the Zip File itself.
    /// </summary>
    public ZipNode RootNode { get; private set; }


    /// <summary>
    /// Creates a browser for a given zip file.
    /// </summary>
    /// <param name="configuration">Represents the processed ZIP file.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="configuration"/> is a
    /// null reference.</exception>
    public ZipNodeRepository(ZipFileSystemConfiguration configuration)
    {
      if(configuration == null)
      {
        throw new ArgumentNullException("configuration");
      }

      Configuration = configuration;

      //create and immediately parse the ZIP file into a repository
      ZipFile = new ZipFile(configuration.ZipFileInfo.FullName);
      Refresh();
    }


    /// <summary>
    /// Checks the registeres ZIP entries for an entry with a
    /// <see cref="ZipEntry.FileName"/> that matches the submitted
    /// name. 
    /// </summary>
    /// <param name="qualifiedName">The qualified name of the item.</param>
    /// <returns>The matching node, if one was found. Otherwise null.</returns>
    public ZipNode TryFindNode(string qualifiedName)
    {
      if(IsRootPath(qualifiedName))
      {
        return RootNode;
      }

      return TryFindNode(qualifiedName, RootNode.ChildNodes);
    }


    private static bool IsRootPath(string path)
    {
      return String.IsNullOrEmpty(path) || path == RootFolderPath;
    }


    /// <summary>
    /// Performs a recursive, case insensitive search for a node with a given qualified name.
    /// </summary>
    /// <returns>The matching node, if found. Otherwise null.</returns>
    private static ZipNode TryFindNode(string qualifiedName, IEnumerable<ZipNode> nodes)
    {
      //run two passes - otherwise, we go to the bottom for every node (which is probably
      //more unlikely) before checking its siblings

      foreach (var node in nodes)
      {
        //check if we have a match
        if (String.Equals(node.FullName, qualifiedName, StringComparison.InvariantCultureIgnoreCase)) return node;
      }

      foreach (var node in nodes)
      {
        if (!node.IsDirectoryNode) continue;
        
        //check the child nodes of the tested item
        var result = TryFindNode(qualifiedName, node.ChildNodes);
        if (result != null) return result;
      }     

      return null;
    }


    /// <summary>
    /// Gets a folder item for a known folder entry within the
    /// maintained ZIP file.
    /// </summary>
    /// <param name="qualifiedName">The qualified name of the ZIP
    /// entry.</param>
    /// <returns>A folder item that can be resolved to the requested ZIP
    /// entry.</returns>
    /// <exception cref="VirtualResourceNotFoundException">If the submitted
    /// path cannot be resolved to a maintained folder entry.</exception>
    public ZipFolderItem GetFolderItem(string qualifiedName)
    {
      qualifiedName = qualifiedName.RemoveRootSlash();    
      var node = TryFindNode(qualifiedName);

      //if there is no such node, create a virtual one
      if (node == null)
      {
        node = new ZipNode(qualifiedName, true);
        node.ParentNode = TryFindNode(qualifiedName.GetParentPath()) ?? RootNode;
      }
      else if (!node.IsDirectoryNode)
      {
        //throw exception if the path is already linked to a directory entry
        string msg = "Submitted folder path [{0}] refers to a file, not a directory.";
        msg = String.Format(msg, qualifiedName);
        throw new ResourceAccessException(msg);
      }

      if(node.IsRootNode)
      {
        return CreateRootItem();
      }

      return new ZipFolderItem(node, node.ToFolderInfo());
    }



    /// <summary>
    /// Gets a file item for a known file entry within the
    /// maintained ZIP file.
    /// </summary>
    /// <param name="qualifiedName">The qualified name of the ZIP
    /// entry.</param>
    /// <returns>A file item that can be resolved to the requested ZIP
    /// entry.</returns>
    /// <exception cref="VirtualResourceNotFoundException">If the submitted
    /// path cannot be resolved to a maintained file entry.</exception>
    public ZipFileItem GetFileItem(string qualifiedName)
    {
      qualifiedName = qualifiedName.RemoveRootSlash();
      var node = TryFindNode(qualifiedName);

      //if there is no such node, create a virtual one
      if (node == null)
      {
        node = new ZipNode(qualifiedName, false);
        node.ParentNode = TryFindNode(qualifiedName.GetParentPath()) ?? RootNode;
      }
      else if (node.IsDirectoryNode)
      {
        //throw exception if the path is already linked to a directory entry
        string msg = "Submitted file path [{0}] refers to a directory, not a file.";
        msg = String.Format(msg, qualifiedName);
        throw new ResourceAccessException(msg);
      }

      return new ZipFileItem(node, node.ToFileInfo());
    }



    /// <summary>
    /// Links the entries of the encapsulated <see cref="ZipFile"/> and refreshes the
    /// <see cref="RootNodes"/> collection.
    /// </summary>
    public void Refresh()
    {
      RootNode = new ZipNode(RootFolderPath, true) { IsRootNode = true };

      var entries = ZipFile.Entries.Select(e => new ZipNode(e)).ToArray();

      for (int index = 0; index < entries.Length; index++)
      {
        var node = entries[index];
        string parentPath = node.FullName.GetParentPath();

        if (!IsRootPath(parentPath))
        {
          //TODO currently, we require the node to have a parent entry if there's path
          //information that indicates so. According to ZIP specs, this might not necessarily
          //be the case...
          node.ParentNode = entries.Single(e => e.FullName == parentPath && e.IsDirectoryNode);
          node.ParentNode.RegisterChildNode(node);
        }
      }

      //get top level entries
      var topLevelEntries = entries.Where(e => e.ParentNode == null).ToArray();

      //assign root entries to root node
      foreach (var entry in topLevelEntries)
      {
        RootNode.RegisterChildNode(entry);
        entry.ParentNode = RootNode;
      }
    }


    /// <summary>
    /// Creates a folder item that represents the ZIP file itself and thus the
    /// root of the file system. Accordingly the returned <see cref="ZipFolderItem.Node"/>
    /// returns the <see cref="RootNode"/> of the repository.
    /// </summary>
    /// <returns>A folder item that represents the file system root.</returns>
    public ZipFolderItem CreateRootItem()
    {
      string rootName = String.IsNullOrEmpty(Configuration.RootName)
                          ? Configuration.ZipFileInfo.Name
                          : Configuration.RootName;


      var root = new VirtualFolderInfo
                                 {
                                   Name = rootName,
                                   FullName = RootFolderPath,
                                   IsRootFolder = true
                                 };

      return new ZipFolderItem { Node = RootNode, ResourceInfo = root};   
    }


    /// <summary>
    /// Tries to attempt an exclusive lock on the ZIP file, and invokes
    /// the submitted <paramref name="action"/> if the lock was granted.
    /// Also saves the file after the update, and refreshes the internal
    /// nodes.
    /// </summary>
    /// <param name="action">The action to be invoked if the write lock
    /// was granted.</param>
    public void PerformWriteAction(Action action)
    {
      Ensure.ArgumentNotNull(action, "action");

      bool status = false;
      try
      {
        status = Monitor.TryEnter(writeToken);
        if(!status)
        {
          string msg = "Write access is currently prohibited - parallel writing is not supported.";
          throw new ResourceLockedException(msg);
        }

        action();

        //refresh node structure immediately (VFS locks are still in place anyways)
        Refresh();

        //update file
        ZipFile.Save();
      }
      finally 
      {
        if(status) Monitor.Exit(writeToken);
      }


    }

    public void Dispose()
    {
      ZipFile.Dispose();
    }
  }
}
