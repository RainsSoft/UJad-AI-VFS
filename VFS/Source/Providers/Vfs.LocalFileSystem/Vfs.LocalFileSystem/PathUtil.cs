using System;
using System.IO;

namespace Vfs.LocalFileSystem
{
  public static class PathUtil
  {
    public const string RelativeRootPrefix = "/";

    /// <summary>
    /// Converts the <see cref="VirtualResourceInfo.FullName"/> property of a given
    /// resource into a shortened path that is relative to the submitted
    /// <paramref name="root"/> directory.<br/>
    /// If <paramref name="root"/> is null, the path remains unchanged.
    /// </summary>
    /// <param name="resource">The resource to be processed.</param>
    /// <param name="root">The root path, if any. Can be null.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="resource"/>
    /// is a null reference.</exception>
    internal static void MakePathsRelativeTo(this VirtualResourceInfo resource, DirectoryInfo root)
    {
      if (resource == null) throw new ArgumentNullException("resource");
      if (root == null) return;

      resource.FullName = resource.GetRelativePath(root);
      resource.ParentFolderPath = GetRelativePath(resource.ParentFolderPath, root);
    }


    /// <summary>
    /// Converts the <see cref="VirtualResourceInfo.FullName"/> property of a given
    /// resource into a shortened path that is relative to the submitted
    /// <paramref name="root"/> directory.<br/>
    /// If <paramref name="root"/> is null, the path remains unchanged.
    /// </summary>
    /// <param name="folder">The resource to be processed.</param>
    /// <param name="root">The root path, if any. Can be null.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="folder"/>
    /// is a null reference.</exception>
    internal static void MakePathsRelativeTo(this FolderItem folder, DirectoryInfo root)
    {
      if (folder == null) throw new ArgumentNullException("folder");
      if (root == null) return;

      folder.ResourceInfo.FullName = GetRelativePath(folder.LocalDirectory.FullName, root);
      
      //if the folder is root, there's no parent folder path at all
      if (folder.ResourceInfo.IsRootFolder) return;
      folder.ResourceInfo.ParentFolderPath = GetRelativePath(folder.ResourceInfo.ParentFolderPath, root);
    }


    /// <summary>
    /// Converts the <see cref="VirtualResourceInfo.FullName"/> property of a given
    /// resource into a shortened path that is relative to the submitted
    /// <paramref name="root"/> directory.<br/>
    /// If <paramref name="root"/> is null, the path remains unchanged.
    /// </summary>
    /// <param name="file">The resource to be processed.</param>
    /// <param name="root">The root path, if any. Can be null.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="file"/>
    /// is a null reference.</exception>
    internal static void MakePathsRelativeTo(this FileItem file, DirectoryInfo root)
    {
      if (file == null) throw new ArgumentNullException("file");
      if (root == null) return;

      file.ResourceInfo.FullName = GetRelativePath(file.LocalFile.FullName, root);
      file.ResourceInfo.ParentFolderPath = GetRelativePath(file.ResourceInfo.ParentFolderPath, root);
    }



    /// <summary>
    /// Creates a relative path to a resource regarding a given root.
    /// </summary>
    /// <param name="filePath">The path of the resource to be processed.</param>
    /// <param name="root">The root to be applied. Can be null.</param>
    /// <returns>Path to the resource, if possible relative to the root folder.</returns>
    public static string GetRelativePath(string filePath, DirectoryInfo root)
    {
      if (filePath == null) filePath = String.Empty;
      if (root == null) return filePath;

      //return the unchanged path if there is no root
      string rootPath = root.FullName;
      if (String.IsNullOrEmpty(rootPath)) return filePath;

      //make sure we have a valid directory path
      rootPath = rootPath.Replace("\\", "/");
      if (!rootPath.EndsWith("/")) rootPath += "/";

      if(rootPath.Length - filePath.Length < 2)
      {
        //if the submitted path is infact the root path, return a link to root
        string fp = filePath.Replace("\\", "/");
        if (!fp.EndsWith("/")) fp += "/";
        if (fp.Equals(rootPath, StringComparison.InvariantCultureIgnoreCase)) return RelativeRootPrefix;
      }

      Uri baseUri = new Uri(rootPath);
      Uri fileUri = new Uri(filePath);

      //calculate relative URI and prefix with root character
      return RelativeRootPrefix + baseUri.MakeRelativeUri(fileUri);
    }


    /// <summary>
    /// Creates a relative path to a resource regarding a given root.
    /// </summary>
    /// <param name="resource">The resource to be processed.</param>
    /// <param name="root">The root to be applied. Can be null.</param>
    /// <returns>Path to the resource, if possible relative to the root folder.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="resource"/>
    /// is a null reference.</exception>
    public static string GetRelativePath(this VirtualResourceInfo resource, DirectoryInfo root)
    {
      if (resource == null) throw new ArgumentNullException("resource");
      return GetRelativePath(resource.FullName, root);
    }



    /// <summary>
    /// Gets an absolute path for a given <paramref name="resourcePath" />,
    /// based on a given root location.
    /// </summary>
    /// <param name="resourcePath">The (optionally relative) path of processed resource.</param>
    /// <param name="root">The root that is used to form the path. Can be null.</param>
    /// <returns>An absolute file path. If a relative path was submitted,
    /// an absolute path, based on the submitted <paramref name="root"/> will be
    /// returned. If an absolute path was submitted, the unchanged path
    /// is being returned.</returns>
    /// <exception cref="InvalidResourcePathException">If the submitted data is invalid
    /// and does not allow a path to be constructed. This also includes a relative
    /// path without a root folder</exception>
    public static string GetAbsolutePath(string resourcePath, DirectoryInfo root)
    {
      if (resourcePath == null) resourcePath = String.Empty;
      if (resourcePath.Length > 0 && resourcePath.StartsWith(RelativeRootPrefix))
      {
        //remove the prefix on relative paths
        resourcePath = resourcePath.Substring(RelativeRootPrefix.Length);
      }

      try
      {
        if (root == null)
        {
          //if we get the root path itself or a rooted path, return it unchanged
          if (resourcePath.Length == 0 || Path.IsPathRooted(resourcePath)) return resourcePath;

          string msg = "Cannot construct absolute path for relative path [{0}] without root directory";
          msg = String.Format(msg, resourcePath);
          throw new InvalidResourcePathException(msg);
        }

        //combine relative path with the location of the application's entry point
        return Path.Combine(root.FullName ?? "", resourcePath).Replace("/", "\\");
      }
      catch(InvalidResourcePathException)
      {
        throw;
      }
      catch (Exception e)
      {
        //TODO audit detailed info?
//        string error = "Resource request for '{0}' caused exception when validating against root directory '{1}'.";
//        error = String.Format(error, resourcePath, root == null ? String.Empty : root.FullName);
//        VfsLog.Debug(e, error);

        //do not expose too much path information (e.g. absolute paths if disabled)
        string error = String.Format("Invalid resource path: '{0}'.", resourcePath);
        throw new InvalidResourcePathException(error, e);
      }
    }


    #region copy directory

    /// <summary>
    /// Performs a recursive copy of a given directory.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    /// <param name="overwrite"></param>
    public static void CopyDirectory(string source, string destination, bool overwrite)
    {
      // Create the destination folder if missing.
      if (!Directory.Exists(destination))
        Directory.CreateDirectory(destination);

      DirectoryInfo dirInfo = new DirectoryInfo(source);

      // Copy all files.
      foreach (FileInfo fileInfo in dirInfo.GetFiles())
        fileInfo.CopyTo(Path.Combine(destination, fileInfo.Name), overwrite);

      // Recursively copy all sub-directories.
      foreach (DirectoryInfo subDirectoryInfo in dirInfo.GetDirectories())
        CopyDirectory(subDirectoryInfo.FullName, Path.Combine(destination, subDirectoryInfo.Name), overwrite);
    }

    #endregion

    #region is directory read-only

    /// <summary>
    /// Checks whether a given folder is read-only or not.
    /// </summary>
    /// <param name="directory">The directory to be looked at.</param>
    /// <returns>True if the directory is readonly.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="directory"/>
    /// is a null reference.</exception>
    public static bool IsReadOnly(this DirectoryInfo directory)
    {
      if (directory == null) throw new ArgumentNullException("directory");
      var attributes = directory.Attributes;
      return (attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
    }

    #endregion

  }
}
