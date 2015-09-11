using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Zip;

namespace Vfs.Zip
{
  /// <summary>
  /// Encapsulates a single <see cref="ZipEntry"/> instance, but provides
  /// links to the entry's parent and child entries.
  /// </summary>
  public class ZipNode
  {
    /// <summary>
    /// Represents the file within the ZIP file.
    /// </summary>
    public ZipEntry FileEntry { get; private set; }

    /// <summary>
    /// Whether the node represents the file itself.
    /// </summary>
    public bool IsRootNode { get; set; }

    /// <summary>
    /// Whether the node represents a directory entry within the
    /// ZIP file. This is also true for the root node.
    /// </summary>
    public bool IsDirectoryNode { get; private set; }

    /// <summary>
    /// Returns path information for the node.
    /// </summary>
    public string FullName { get; private set; }

    private readonly List<ZipNode> childNodes = new List<ZipNode>();


    /// <summary>
    /// Gets all child nodes of the current node.
    /// </summary>
    /// <exception cref="InvalidOperationException">In case the current
    /// node is not a directory node, as indicated through the
    /// <see cref="IsDirectoryNode"/> property.</exception>
    public IEnumerable<ZipNode> ChildNodes
    {
      get
      {
        if (!IsDirectoryNode)
        {
          string msg = "ZIP file node [{0}] is not a directory node.";
          msg = String.Format(msg, FullName);
          throw new InvalidOperationException(msg);
        }

        lock (childNodes)
        {
          return childNodes.ToArray();
        }
      }
    }


    /// <summary>
    /// Gets all child nodes of the current node that represent file entries.
    /// </summary>
    /// <exception cref="InvalidOperationException">In case the current
    /// node is not a directory node, as indicated through the
    /// <see cref="IsDirectoryNode"/> property.</exception>
    public IEnumerable<ZipNode> ChildFiles
    {
      get
      {
        return ChildNodes.Where(n => !n.IsDirectoryNode).ToArray();
      }
    }


    /// <summary>
    /// Gets all child nodes of the current node that represent directory entries.
    /// </summary>
    /// <exception cref="InvalidOperationException">In case the current
    /// node is not a directory node, as indicated through the
    /// <see cref="IsDirectoryNode"/> property.</exception>
    public IEnumerable<ZipNode> ChildDirectories
    {
      get
      {
        return ChildNodes.Where(n => n.IsDirectoryNode).ToArray();
      }
    }


    /// <summary>
    /// Gets a link to the node's parent, if any. If the
    /// node is a root node, this property returns null.
    /// </summary>
    public ZipNode ParentNode { get; set; }




    public ZipNode(string fullName, bool isDirectoryNode)
    {
      FullName = fullName;
      IsDirectoryNode = isDirectoryNode;
    }

    public ZipNode(ZipEntry entry)
    {
      Ensure.ArgumentNotNull(entry, "entry");

      FileEntry = entry;
      FullName = entry.FileName;
      IsDirectoryNode = entry.IsDirectory;
    }


    /// <summary>
    /// Registers a child node, which will be added to the
    /// <see cref="ChildNodes"/> collection.
    /// </summary>
    /// <param name="childNode">The node to be registered.</param>
    public void RegisterChildNode(ZipNode childNode)
    {
      lock (childNodes)
      {
        childNodes.Add(childNode);
      }
    }

    /// <summary>
    /// Removes a given node from the internal <see cref="ChildNodes"/>
    /// collection.
    /// </summary>
    /// <param name="childNode">The node to be removed.</param>
    /// <returns>True if the node was found and removed. Otherwise false.</returns>
    public bool RemoveChildNode(ZipNode childNode)
    {
      lock (childNodes)
      {
        return childNodes.Remove(childNode);
      }
    }

    /// <summary>
    /// Gets a flat list that contains all descendents of the current node,
    /// and optionally the node itself.
    /// </summary>
    /// <param name="includeSelf">Whether the returned list should contain the
    /// node itself or not.</param>
    /// <returns>A list of all descendant nodes, optionally including the node
    /// itself. If this method is invoked on a file node, the returned list is either
    /// empty or contains a single entry.</returns>
    public List<ZipNode> GetDescendants(bool includeSelf)
    {
      var list = new List<ZipNode>();
      if(includeSelf) list.Add(this);

      //if the processed node is a file, there are no descendants
      if (!IsDirectoryNode) return list;
      
      //recurse tree
      foreach (var childNode in ChildNodes)
      {
        list.AddRange(childNode.GetDescendants(true));
      }

      return list;
    }


    /// <summary>
    /// Gets all ancestors of the current node as indicated by
    /// the <see cref="ParentNode"/> property.
    /// </summary>
    /// <param name="includeSelf">Whether the returned list should
    /// include the node itself or not.</param>
    /// <returns>A list of all parents of the current  node, optionally
    /// including the node itself.</returns>
    public List<ZipNode> GetAncestors(bool includeSelf, bool includeRoot)
    {
      var list = new List<ZipNode>();

      var current = includeSelf ? this : this.ParentNode;
      while(current != null)
      {
        //stop here if we are at root
        if (current.IsRootNode && !includeRoot) break;

        list.Add(current);
        current = current.ParentNode;
      }

      return list;
    }
  }
}
