using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vfs.Auditing;


namespace Vfs.LocalFileSystem
{
  /// <summary>
  /// A file system provider which operates on the machine's
  /// (real) local file system.
  /// </summary>
  public partial class LocalFileSystemProviderX : FileSystemProviderBase
  {
    /// <summary>
    /// The configured root folder. If this property is set, the
    /// file system will limit file and directory access to the
    /// contents of this directory. If not set (default), data
    /// on all drives can be accessed.  
    /// </summary>
    public DirectoryInfo RootDirectory { get; private set; }

    /// <summary>
    /// The name of the folder that acts as the root of this
    /// virtual file system.
    /// </summary>
    public string RootName { get; private set; }

    /// <summary>
    /// Whether to expose only relative paths if data is limited
    /// to a given <see cref="RootDirectory"/>.
    /// </summary>
    public bool UseRelativePaths { get; set; }



    #region construction

    /// <summary>
    /// Initializes a new instance that provides access to the whole
    /// local file system.
    /// </summary>
    public LocalFileSystemProviderX()
    {
      RootName = Environment.MachineName;
    }

    /// <summary>
    /// Initializes a new instance that provides access to the whole
    /// local file system.
    /// </summary>
    /// <param name="rootName">The name of the root folder.</param>
    public LocalFileSystemProviderX(string rootName)
    {
      RootName = rootName;
    }


    /// <summary>
    /// Initializes a new instance that provides access to the contents
    /// of a given directory.
    /// </summary>
    /// <param name="rootDirectory">The root folder which is being managed
    /// by this provider instance.</param>
    /// <param name="useRelativePaths">If true, returned <see cref="VirtualResourceInfo"/>
    /// instances do not provide qualified paths, but virtual paths to the submitted
    /// root directory. This also leverages security in remote access scenarios.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="rootDirectory"/>
    /// is a null reference.</exception>
    /// <exception cref="DirectoryNotFoundException">If the directory does
    /// not exist on the local file system.</exception>
    public LocalFileSystemProviderX(DirectoryInfo rootDirectory, bool useRelativePaths)
        :this(rootDirectory, rootDirectory.Name, useRelativePaths)
    {
    }


    /// <summary>
    /// Initializes a new instance that provides access to the contents
    /// of a given directory.
    /// </summary>
    /// <param name="rootDirectory">The root folder which is being managed
    /// by this provider instance.</param>
    /// <param name="rootName">The <see cref="VirtualResourceInfo.Name"/> of
    /// the root item. Used to mask the real folder name.</param>
    /// <param name="useRelativePaths">If true, returned <see cref="VirtualResourceInfo"/>
    /// instances do not provide qualified paths, but virtual paths to the submitted
    /// root directory. This also leverages security in remote access scenarios.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="rootDirectory"/> or
    /// <paramref name="rootName"/> is a null reference.</exception>
    /// <exception cref="DirectoryNotFoundException">If the directory does
    /// not exist on the local file system.</exception>
    public LocalFileSystemProviderX(DirectoryInfo rootDirectory, string rootName, bool useRelativePaths)
    {
      if (rootName == null) throw new ArgumentNullException("rootName");
      if (rootDirectory == null) throw new ArgumentNullException("rootDirectory");


      if(!rootDirectory.Exists)
      {
        string msg = "Root directory '{0}' does not exist.";
        msg = String.Format(msg, rootDirectory.FullName);
        VfsLog.Debug(msg);

        throw new DirectoryNotFoundException(msg);
      }

      RootDirectory = rootDirectory;
      RootName = rootName;
      UseRelativePaths = useRelativePaths;
    }

    #endregion
    

    #region get FS root

    /// <summary>
    /// Gets the root of the file system. This is a dummy folder, which
    /// represents the file system as a whole, and provides the top level contents
    /// of the underlying file system as files and folders.<br/>
    /// In case of this provider, the root either corresponds to
    /// the <see cref="RootDirectory"/>, if set, or the machine itself.
    /// </summary>
    public override VirtualFolderInfo GetFileSystemRoot()
    {
      VirtualFolderInfo root;
      if (RootDirectory == null)
      {
        root = new VirtualFolderInfo
                 {
                     IsRootFolder = true,
                     Name = RootName,
                     FullName = String.Empty
                 };
      }
      else
      {
        root = RootDirectory.CreateFolderResourceInfo();
        root.Name = RootName;
        root.IsRootFolder = true;

        //hide the path
        if(UseRelativePaths) root.FullName = PathUtil.RelativeRootPrefix;
      }

      return root;
    }

    #endregion


    #region get resource by path

    /// <summary>
    /// Gets meta data about a given file which is identified
    /// by its path within the file system.
    /// </summary>
    /// <param name="virtualFilePath">Path information that allows
    /// the provider to identify the requested resource.</param>
    /// <returns>A <see cref="VirtualFileInfo"/> instance which provides
    /// meta data about the file.</returns>
    /// <exception cref="VirtualResourceNotFoundException">If the file cannot
    /// be found.</exception>
    /// <exception cref="ResourceAccessException">If the user does not have
    /// permission to access this resource.</exception>
    public override VirtualFileInfo GetFileInfo(string virtualFilePath)
    {
      string path;
      return GetFileInfoInternal(virtualFilePath, true, out path);
    }


    protected VirtualFileInfo GetFileInfoInternal(string virtualFilePath, bool mustExist, out string absolutePath)
    {
      try
      {
        if (String.IsNullOrEmpty(virtualFilePath))
        {
          VfsLog.Debug("File request without file path received.");
          throw new ResourceAccessException("An empty or null string is not a valid file path");
        }

        //make sure we operate on absolute paths
        absolutePath = PathUtil.GetAbsolutePath(virtualFilePath, RootDirectory);

        if (IsRootPath(absolutePath))
        {
          VfsLog.Debug("Blocked file request with path '{0}' (resolves to root directory).", virtualFilePath);
          throw new ResourceAccessException("Invalid path submitted: " + virtualFilePath);
        }
        
        var fi = new FileInfo(absolutePath);
        VirtualFileInfo fileInfo = fi.CreateFileResourceInfo();

        //convert to relative paths if required (also prevents qualified paths in validation exceptions)
        if (UseRelativePaths) fileInfo.MakePathsRelativeTo(RootDirectory);

        //make sure the user is allowed to access the resource
        ValidateResourceAccess(fileInfo);

        //verify file exists on FS
        if(mustExist) fileInfo.VerifyFileExists(RootDirectory);
        
        return fileInfo;
      }
      catch(VfsException)
      {
        //just bubble internal exceptions
        throw;
      }
      catch (Exception e)
      {
        VfsLog.Debug(e, "Could not create file based on path '{0}' with root '{1}'", virtualFilePath,
                           RootDirectory);
        throw new ResourceAccessException("Invalid path submitted: " + virtualFilePath);
      }
    }


    /// <summary>
    /// Gets meta data about a given folder which is identified
    /// by its path within the file system.
    /// </summary>
    /// <param name="virtualFolderPath">Path information that allows
    /// the provider to identify the requested resource.</param>
    /// <returns>A <see cref="VirtualFolderInfo"/> instance which provides
    /// meta data about the folder.</returns>
    /// <exception cref="VirtualResourceNotFoundException">If the folder cannot
    /// be found.</exception>
    /// <exception cref="ResourceAccessException">If the user does not have
    /// permission to access this resource.</exception>
    public override VirtualFolderInfo GetFolderInfo(string virtualFolderPath)
    {
      string path;
      return GetFolderInfoInternal(virtualFolderPath, true, out path);
    }


    protected VirtualFolderInfo GetFolderInfoInternal(string virtualFolderPath, bool mustExist, out string absolutePath)
    {
      try
      {
        //make sure we operate on absolute paths
        absolutePath = PathUtil.GetAbsolutePath(virtualFolderPath ?? "", RootDirectory);

        if (IsRootPath(absolutePath))
        {
          return GetFileSystemRoot();
        }

        var di = new DirectoryInfo(absolutePath);
        VirtualFolderInfo folderInfo = di.CreateFolderResourceInfo();

        //convert to relative paths if required (also prevents qualified paths in validation exceptions)
        if (UseRelativePaths) folderInfo.MakePathsRelativeTo(RootDirectory);

        //make sure the user is allowed to access the resource
        ValidateResourceAccess(folderInfo);

        //verify folder exists on FS
        if(mustExist) folderInfo.VerifyDirectoryExists(RootDirectory);

        return folderInfo;
      }
      catch (VfsException)
      {
        //just bubble internal exceptions
        throw;
      }
      catch (Exception e)
      {
        VfsLog.Debug(e, "Could not create directory based on path '{0}' with root '{1}'", virtualFolderPath,
                           RootDirectory);
        throw new ResourceAccessException("Invalid path submitted: " + virtualFolderPath);
      }
    }

    #endregion


    #region get parent folder

    /// <summary>
    /// Gets the parent folder of a given file system resource.
    /// </summary>
    /// <param name="childFilePath">The qualified path (<see cref="VirtualResourceInfo.FullName"/>
    /// of a file that is used to resolve the parent folder.</param>
    /// <returns>The parent of the file.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="childFilePath"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the file that is represented
    /// by <paramref name="childFilePath"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of an invalid or prohibited
    /// resource access.</exception>
    public override VirtualFolderInfo GetFileParent(string childFilePath)
    {
      if (childFilePath == null) throw new ArgumentNullException("childFilePath");
      return GetParentInternal(GetFileInfo(childFilePath));

    }

 
    /// <summary>
    /// Gets the parent folder of a given file system resource.
    /// </summary>
    /// <param name="childFolderPath">The qualified name (<see cref="VirtualResourceInfo.FullName"/>)
    /// of an arbitrary folder that is used to resolve the parent folder.</param>
    /// <returns>The parent of the folder.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="childFolderPath"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the folder that is represented
    /// by <paramref name="childFolderPath"/> does not exist in the file system.</exception>
    public override VirtualFolderInfo GetFolderParent(string childFolderPath)
    {
      if (childFolderPath == null) throw new ArgumentNullException("childFolderPath");
      return GetParentInternal(GetFolderInfo(childFolderPath));
    }



    /// <summary>
    /// Resolves the parent folder for a virtual resource.
    /// </summary>
    /// <param name="child">The child file or folder.</param>
    /// <returns>The parent folder info.</returns>
    private VirtualFolderInfo GetParentInternal(VirtualResourceInfo child)
    {
      if (child == null) throw new ArgumentNullException("child");

      string path = PathUtil.GetAbsolutePath(child.FullName, RootDirectory);
      if (IsRootPath(path))
      {
        //only log the request for the root's parent - do not include that
        //info in an exception in case we're hiding path information
        string msg = "Error while requesting parent of resource {0} - the folder itself already is the root.";
        VfsLog.Error(msg, child.FullName);

        //create exception with a relative path, if required
        string exceptionPath = UseRelativePaths ? PathUtil.RelativeRootPrefix : child.FullName;
        msg = String.Format(msg, exceptionPath);
        throw new ResourceAccessException(msg);
      }

      //make sure the processed directory exists
      VirtualFolderInfo folder = child as VirtualFolderInfo;
      if (folder != null)
      {
        folder.VerifyDirectoryExists(RootDirectory);
      }
      else
      {
        VirtualFileInfo file = (VirtualFileInfo) child;
        file.VerifyFileExists(RootDirectory);
      }

      //get the path of the parent (returned value may be null!) 
      string parentPath = Path.GetDirectoryName(path);

      //get folder info
      return GetFolderInfo(parentPath);
    }

    #endregion


    #region get child folders / files

    /// <summary>
    /// Gets all child folders of a given folder.
    /// </summary>
    /// <param name="parentFolderPath">The <see cref="VirtualResourceInfo.FullName"/>, which
    /// identifies the parent folder within the file system.</param>
    /// <returns>The child folders of the folder.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="parentFolderPath"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the folder that is represented
    /// by <paramref name="parentFolderPath"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    public override IEnumerable<VirtualFolderInfo> GetChildFolders(string parentFolderPath)
    {
      if (parentFolderPath == null) throw new ArgumentNullException("parentFolderPath");

      string absolutePath;
      var parent = GetFolderInfoInternal(parentFolderPath, true, out absolutePath);

      //if we're dealing with the system root, return the drives as folders
      if (RootDirectory == null && parent.IsRootFolder)
      {
        return GetDriveFolders();
      }

      DirectoryInfo dir = new DirectoryInfo(absolutePath);

      var folders = dir.GetDirectories().Select(di => di.CreateFolderResourceInfo()).ToArray();
      if (UseRelativePaths) folders.Do(f => f.MakePathsRelativeTo(RootDirectory));
      return folders;
    }


    /// <summary>
    /// Gets the top level folders of the file system, which represent the logical
    /// drives of the current system.
    /// </summary>
    /// <returns>A collection of folders that build the root folders
    /// of the file system.</returns>
    private static IEnumerable<VirtualFolderInfo> GetDriveFolders()
    {
      return from driveName in Environment.GetLogicalDrives()
             where Directory.Exists(driveName) //skip inexisting drives
             select new VirtualFolderInfo
             {
               Name = driveName,
               FullName = driveName
             };
    }


    /// <summary>
    /// Gets all child files of a given folder.
    /// </summary>
    /// <param name="parentFolderPath">The <see cref="VirtualResourceInfo.FullName"/>, which
    /// identifies the parent folder within the file system.</param>
    /// <returns>The files of the submitted folder.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="parentFolderPath"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the folder that is represented
    /// by <paramref name="parentFolderPath"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    public override IEnumerable<VirtualFileInfo> GetChildFiles(string parentFolderPath)
    {
      if (parentFolderPath == null) throw new ArgumentNullException("parentFolderPath");

      string absolutePath;
      var parent = GetFolderInfoInternal(parentFolderPath, true, out absolutePath);

      //if we're dealing with the system root, return an empty array - the machine only
      //exposes the drives as the root's folders
      if (RootDirectory == null && parent.IsRootFolder)
      {
        return new VirtualFileInfo[] { };
      }

      DirectoryInfo dir = new DirectoryInfo(absolutePath);

      var files = dir.GetFiles().Select(fi => fi.CreateFileResourceInfo()).ToArray();
      if (UseRelativePaths) files.Do(f => f.MakePathsRelativeTo(RootDirectory));
      return files;
    }

    #endregion


    #region check resource availability

    /// <summary>
    /// Checks whether a file resource at a given path exists or not.
    /// </summary>
    /// <param name="virtualFilePath">A path to the requested file.</param>
    /// <returns>True if a matching file was found.</returns>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    public override bool IsFileAvailable(string virtualFilePath)
    {
      string path;
      GetFileInfoInternal(virtualFilePath, false, out path);
      return File.Exists(path);
    }

    /// <summary>
    /// Checks whether a folder resource at a given path exists or not.
    /// </summary>
    /// <param name="virtualFolderPath"></param>
    /// <returns>True if a matching folder was found.</returns>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    public override bool IsFolderAvailable(string virtualFolderPath)
    {
      string path;
      GetFolderInfoInternal(virtualFolderPath, false, out path);
      return Directory.Exists(path);
    }

    #endregion


    #region internal validation / inspection

    /// <summary>
    /// Checks whether a given path (whether null, relative, or absolute)
    /// will be resolved as the root path.
    /// </summary>
    /// <param name="path">The path to be inspected.</param>
    /// <returns>True if the folder path resolves to the system root.</returns>
    private bool IsRootPath(string path)
    {
      if(RootDirectory == null)
      {
        return String.IsNullOrEmpty(path);
      }

      path = PathUtil.GetAbsolutePath(path, RootDirectory);
      return String.Equals(path, RootDirectory.FullName, StringComparison.InvariantCultureIgnoreCase);
    }



    /// <summary>
    /// Validates whether a  <see cref="LocalFileSystemProvider"/> was configured
    /// with access restricted to a given <see cref="LocalFileSystemProvider.RootDirectory"/>,
    /// and makes sure that the requested <paramref name="resource"/> is indeed contained
    /// within that folder.
    /// </summary>
    /// <param name="resource">The requested resource.</param>
    /// <exception cref="ResourceAccessException">If the requested resource is not
    /// a descendant of a configured <see cref="LocalFileSystemProvider.RootDirectory"/>.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="resource"/>
    /// is a null reference.</exception>
    private void ValidateResourceAccess(VirtualResourceInfo resource)
    {
      if (resource == null) throw new ArgumentNullException("resource");

      //if there isn't a restricted custom root, every file resource can be accessed
      //(if the path is invalid, this will fail later, depending on the action)
      if (RootDirectory == null) return;

      try
      {
        string path = PathUtil.GetAbsolutePath(resource.FullName, RootDirectory);

        //if the root path was submitted, we're within the scope, too
        if (IsRootPath(path)) return;

        //if we have a custom root, make sure the resource is indeed a descendant of the root
        if (RootDirectory.IsParentOf(path)) return;
      }
      catch(ResourceAccessException e)
      {
        //just bubble a resource access exception
        if (e.Resource == null) e.Resource = resource;
        throw;
      }
      catch (Exception e)
      {
        //exceptions can happen in case of invalid file paths

        //log detailed info
        string error = "Resource request for '{0}' caused exception when validating against root directory '{1}'.";
        error = String.Format(error, resource.FullName, RootDirectory.FullName);
        VfsLog.Debug(e, error);

        //do not expose too much path information (e.g. absolute paths if disabled)
        error = String.Format("Invalid resource path: '{0}'.", resource.FullName);
        throw new ResourceAccessException(error) { Resource = resource };
      }

      //if none of the above is true, the request is invalid

      //log detailed info
      string msg = "Resource request for '{0}' was blocked. The resource is outside the root directory '{1}'.";
      msg = String.Format(msg, resource.FullName, RootDirectory.FullName);
      VfsLog.Debug(msg);

      //do not expose too much path information (e.g. absolute paths if disabled)
      msg = String.Format("Invalid resource path: '{0}'.", resource.FullName);
      throw new ResourceAccessException(msg) { Resource = resource };
    }

    #endregion

    /// <summary>
    /// Creates a qualified name that can be used as an identifier
    /// for a given file of the file system.
    /// </summary>
    /// <param name="parentFolder">The qualified name of the parent
    /// folder.</param>
    /// <param name="fileName">The name of a file within the folder.</param>
    /// <returns>An qualified path name for the submitted
    /// <paramref name="fileName"/>.</returns>
    public override string CreateFilePath(string parentFolder, string fileName)
    {
      if (parentFolder == null) parentFolder = String.Empty;
      if (fileName == null) fileName = String.Empty;

      return Path.Combine(parentFolder, fileName);
    }

    /// <summary>
    /// Creates a qualified name that can be used as an identifier
    /// for a given folder of the file system.
    /// </summary>
    /// <param name="parentFolder">The qualified name of the parent
    /// folder.</param>
    /// <param name="folderName">The name of the child folder.</param>
    /// <returns>An qualified path name for the submitted
    /// <paramref name="folderName"/>.</returns>
    public override string CreateFolderPath(string parentFolder, string folderName)
    {
      if (parentFolder == null) parentFolder = String.Empty;
      if (folderName == null) folderName = String.Empty;

      return Path.Combine(parentFolder, folderName);
    }
  }
}