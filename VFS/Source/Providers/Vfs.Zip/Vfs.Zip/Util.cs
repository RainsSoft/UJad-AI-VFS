using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;
using Vfs.Util;

namespace Vfs.Zip
{
  public static class Util
  {
    public static VirtualFolderInfo ToFolderInfo(this ZipNode node)
    {
      Ensure.ArgumentNotNull(node, "node");

      if(!node.IsDirectoryNode)
      {
        string msg = "ZIP entry [{0}] does not represent a directory.";
        msg = String.Format(msg, node.FullName);
        throw new InvalidOperationException(msg);
      }

      //get local name
      var fileName = node.FullName;
      if (fileName.EndsWith("/")) fileName = fileName.Substring(0, fileName.Length - 1);
      fileName = Path.GetFileName(fileName);

      var fi = new VirtualFolderInfo
                               {
                                 Name = fileName,
                                 IsRootFolder =  node.IsRootNode,
                                 IsEmpty = node.ChildNodes.Count() == 0
                               };

      SetCommonProperties(fi, node);
      return fi;
    }


    /// <summary>
    /// Gets the local name of a given <see cref="ZipNode"/>.
    /// </summary>
    /// <param name="node">The queried node.</param>
    /// <returns>The non-qualified local file or directory name.</returns>
    public static string GetLocalName(this ZipNode node)
    {
      var fullName = node.FullName.RemoveTrailingSlash();
      return Path.GetFileName(fullName);
    }


    public static VirtualFileInfo ToFileInfo(this ZipNode node)
    {
      Ensure.ArgumentNotNull(node, "node");

      if(node.IsDirectoryNode)
      {
        string msg = "ZIP entry [{0}] does not represent a file.";
        msg = String.Format(msg, node.FileEntry.FileName);
        throw new InvalidOperationException(msg);
      }

      var fi = new VirtualFileInfo
                               {
                                 Name = Path.GetFileName(node.FullName),
                                 ContentType = ContentUtil.ResolveContentType(Path.GetExtension(node.FullName)),
                               };


      SetCommonProperties(fi, node);
      if (node.FileEntry != null)
      {
        fi.Length = node.FileEntry.UncompressedSize;
      }

      return fi;
    }

    private static void SetCommonProperties(VirtualResourceInfo resourceInfo, ZipNode node)
    {
      resourceInfo.FullName = node.FullName;
      resourceInfo.ParentFolderPath = node.ParentNode == null ? null : node.ParentNode.FullName;


      var entry = node.FileEntry;
      if (entry != null)
      {
        resourceInfo.Description = entry.Info;
        resourceInfo.IsHidden = (entry.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
        resourceInfo.IsReadOnly = (entry.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
        resourceInfo.CreationTime = entry.CreationTime == DateTime.MinValue
                            ? (DateTimeOffset?)null
                            : entry.CreationTime.ToLocalTime();
        resourceInfo.LastWriteTime = entry.LastModified == DateTime.MinValue
                             ? (DateTimeOffset?)null
                             : entry.LastModified.ToLocalTime();
      }
    }



    /// <summary>
    /// Gets the parent part of a given path, and submits it to the
    /// <see cref="EnsureDirectoryPath"/> method before returning it.
    /// </summary>
    public static string GetParentPath(this string path)
    {
      if (path == null) return path;

      path = RemoveTrailingSlash(path);
      path = Path.GetDirectoryName(path);
      return EnsureDirectoryPath(path);
    }


    public static string RemoveRootSlash(this string path)
    {
      if (path != null && path.StartsWith("/"))
      {
        return path.Substring(1, path.Length - 1);
      }

      return path;
    }


    /// <summary>
    /// Removes a trailing slash from a file path.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string RemoveTrailingSlash(this string path)
    {
      if (path == null) return path;

      path = EnsureForwardSlashes(path);
      if (path.EndsWith("/"))
      {
        return path.Substring(0, path.Length - 1);
      }

      return path;
    }


    /// <summary>
    /// Ensures that a path (unless null) is being terminated with a slash, and
    /// only contains forward slashes.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string EnsureDirectoryPath(this string path)
    {
      //only add a slash if there is actually some path information
      if (String.IsNullOrEmpty(path)) return path;

      path = EnsureForwardSlashes(path);
      if(!path.EndsWith("/"))
      {
        return path + "/";
      }

      return path;
    }


    /// <summary>
    /// Replaces back slashes in a given path by forward slashes.
    /// </summary>
    public static string EnsureForwardSlashes(this string path)
    {
      return path == null ? null : path.Replace("\\", "/");
    }
  }
}
