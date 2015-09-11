using System;
using System.IO;
using Vfs.Util;

namespace Vfs.LocalFileSystem
{
  public static class Extensions
  {
    /// <summary>
    /// Checks whether the <see cref="VirtualResourceInfo.FullName"/>
    /// matches an existing file on the local file system. If that's
    /// not the case, a <see cref="VirtualResourceNotFoundException"/> is
    /// thrown.
    /// </summary>
    /// <param name="file">The file to check.</param>
    /// <param name="rootDirectory">The used root directory, if any, which is used
    /// to resolve relative paths.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="file"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the submitted
    /// file's <see cref="VirtualResourceInfo.FullName"/> does not link to
    /// an existing file.</exception>
    internal static void VerifyFileExists(this VirtualFileInfo file, DirectoryInfo rootDirectory)
    {
      if (file == null) throw new ArgumentNullException("file");

      string path = PathUtil.GetAbsolutePath(file.FullName, rootDirectory);
      if (String.IsNullOrEmpty(path) || !File.Exists(path))
      {
        string msg = String.Format("File not found on local file system at '{0}'", file.FullName);
        throw new VirtualResourceNotFoundException(msg) {Resource = file};
      }
    }


    /// <summary>
    /// Checks whether the <see cref="VirtualResourceInfo.FullName"/>
    /// matches an existing directory on the local file system. If that's
    /// not the case, a <see cref="VirtualResourceNotFoundException"/> is
    /// thrown.
    /// </summary>
    /// <param name="folder">The folder to check.</param>
    /// <param name="rootDirectory">The used root directory, if any, which is used
    /// to resolve relative paths.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="folder"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the submitted
    /// folder's <see cref="VirtualResourceInfo.FullName"/> does not link to
    /// an existing directory.</exception>
    internal static void VerifyDirectoryExists(this VirtualFolderInfo folder, DirectoryInfo rootDirectory)
    {
      if (folder == null) throw new ArgumentNullException("folder");

      string path = PathUtil.GetAbsolutePath(folder.FullName, rootDirectory);
      if (String.IsNullOrEmpty(path) || !Directory.Exists(path))
      {
        string msg = String.Format("Directory '{0}' not found on local file system.", folder.FullName);
        throw new VirtualResourceNotFoundException(msg) { Resource = folder };
      }
    }


    /// <summary>
    /// Checks whether a given <paramref name="root"/> directory
    /// is indeed a direct or indirect parent of the resource
    /// at the submitted path.
    /// </summary>
    /// <param name="root">The root folder.</param>
    /// <param name="resourcePath">The path of the child resource to be validated.</param>
    /// <returns>True, if the file or directory at <paramref name="resourcePath"/>
    /// is indeed a descendant of the submitted <paramref name="root"/> directory. False
    /// if not, or if the processing of the path caused an exception due to invalid
    /// paths. An exception will also be logged as a warning into <see cref="VfsLog"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">If <paramref name="root"/>
    /// is a null reference.</exception>
    public static bool IsParentOf(this DirectoryInfo root, string resourcePath)
    {
      if (root == null) throw new ArgumentNullException("root");

      //get the root name, but cut and terminating separators
      string rootName = root.FullName;
      if (root.Parent != null && rootName.EndsWith(Path.DirectorySeparatorChar.ToString()))
      {
        rootName = rootName.Substring(0, rootName.Length - 1);
      }

      DirectoryInfo di = new DirectoryInfo(Path.GetFullPath(resourcePath));

      try
      {
        //switch from directory to directory rather than doing string
        //comparisons just to make sure...
        DirectoryInfo parent = di.Parent;
        while(parent != null)
        {
          //return true if the root is the parent
          if(parent.FullName.Equals(rootName, StringComparison.InvariantCultureIgnoreCase))
          {
            return true;
          }

          parent = parent.Parent;
        }
      }
      catch (Exception e)
      {
        string msg = "Could not validate whether file path '{0}' is withing scope of root folder '{1}'.";
        msg = String.Format(msg, resourcePath, root.FullName);
        throw new ResourceAccessException(msg, e);
      }

      return false;
    }

  }
}