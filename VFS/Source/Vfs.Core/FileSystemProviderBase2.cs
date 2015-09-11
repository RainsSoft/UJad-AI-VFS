using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vfs.Auditing;
using Vfs.Locking;
using Vfs.Security;
using Vfs.Util;

namespace Vfs
{
  /// <summary>
  /// A comprehensive base class for file system providers, which encapsulates common logic,
  /// validation, and auditing, leaving it up to the implementing classes to provide the
  /// logic that is specific to the exposes file system. 
  /// </summary>
  /// <typeparam name="TFile">Internally used class which encapsulates all information
  /// needed about a given file while processing requests.</typeparam>
  /// <typeparam name="TFolder">Internally used class which encapsulates all information
  /// needed about a given folder while processing requests.</typeparam>
  //public abstract class FileSystemProviderBase2<TFile, TFolder> : FileSystemProviderBase where TFile:IVirtualFileItem where TFolder:IVirtualFolderItem
    public abstract class FileSystemProviderBase2 : FileSystemProviderBase 
{
    #region resolve file / folder paths

    /// <summary>
    /// A method that is invoked on pretty much every file request in order
    /// to resolve a submitted file path into a <see cref="VirtualFileInfo"/>
    /// object.<br/>
    /// The <see cref="VirtualFileInfo"/> is being returned as part of a
    /// <see cref="IVirtualFileItem"/>, which should also provide some additionally
    /// required meta data which is used for further validation and auditing.
    /// </summary>
    /// <param name="submittedFilePath">The path that was received as a part of a file-related
    /// request.</param>
    /// <param name="context">The currently performed file system operation.</param>
    /// <returns>A <see cref="IVirtualFileItem"/> which encapsulates a <see cref="VirtualFileInfo"/>
    /// that represents the requested file on the file system.</returns>
    /// <exception cref="InvalidResourcePathException">In case the format of the submitted path
    /// is invalid, meaning it cannot be interpreted as a valid resource identifier.</exception>
    /// <exception cref="VfsException">Exceptions will be handled by this base class and audited to
    /// the <see cref="FileSystemProviderBase.Auditor"/>. If auditing was already performed or should
    /// be suppressed, implementors can set the <see cref="VfsException.IsAudited"/> and
    /// <see cref="VfsException.SuppressAuditing"/> properties.</exception>
    /// <exception cref="Exception">Any exceptions that are not derived from
    /// <see cref="VfsException"/> will be wrapped and audited.</exception>
        public abstract IVirtualFileItem ResolveFileResourcePath(string submittedFilePath, FileSystemTask context);



    /// <summary>
    /// Internally resolves a given file resource by invoking <see cref="ResolveFileResourcePath"/>,
    /// and performs basic exception handling, auditing, access and availability checks.<br/>
    /// This method may be overridden in case additional work needs to be done in order to resolve
    /// the resource based on the submitted path.
    /// </summary>
    /// <param name="virtualFilePath">The submitted file path that needs to be resolved.</param>
    /// <param name="mustExist">Whether the file must exist on the file system. If this parameter is true
    /// and the received <typeparamref name="TFile"/>'s <see cref="IVirtualFileItem.Exists"/> is
    /// false, and <see cref="VirtualResourceNotFoundException"/> is being thrown.</param>
    /// <param name="context">The file system operation that is being performed during the invocation of
    /// this method. Used for internal auditing.</param>
    /// <returns>A wrapper item that includes the a <see cref="VirtualFileInfo"/> that corresponds
    /// to the submitted <paramref name="virtualFilePath"/>.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="virtualFilePath"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If <paramref name="mustExist"/> is true, and
    /// the folder's <see cref="IVirtualFileItem.Exists"/> property is false.</exception>
        protected IVirtualFileItem ResolveFileResourcePathInternal(string virtualFilePath, bool mustExist, FileSystemTask context)
    {
      //TODO allow null (also for folders), but write test first
      Ensure.ArgumentNotNull(virtualFilePath, "virtualFilePath");

      IVirtualFileItem fileItem;
      try
      {
        fileItem = ResolveFileResourcePath(virtualFilePath, context);
      }
      catch (InvalidResourcePathException e)
      {
        AuditHelper.AuditException(Auditor,e, context, AuditEvent.InvalidFilePathFormat);
        throw;
      }
      catch (VfsException e)
      {
        //audit exception
        AuditHelper.AuditException(Auditor,e, context, AuditEvent.FileResolveFailed);
        throw;
      }
      catch (Exception e)
      {
        //wrap exception and audit
        string msg = String.Format("Unexpected exception while resolving file path [{0}]", virtualFilePath);
        var rae = new ResourceAccessException(msg, e);

        AuditHelper.AuditException(Auditor,rae, context, AuditEvent.FileResolveFailed);
        throw rae;
      }


      if (mustExist && !fileItem.Exists)
      {
        //audit and throw exception
        AuditHelper.AuditRequestedFileNotFound(Auditor,fileItem, context);

        string msg = String.Format("File [{0}] not found on file system.", fileItem.ResourceInfo.FullName);
        throw new VirtualResourceNotFoundException(msg) { Resource = fileItem.ResourceInfo, IsAudited = true };
      }

      return fileItem;
    }


    /// <summary>
    /// A method that is invoked on pretty much every folder request in order
    /// to resolve a submitted folder path into a <see cref="VirtualFolderInfo"/>
    /// object.<br/>
    /// The <see cref="VirtualFolderInfo"/> is being returned as part of a
    /// <see cref="IVirtualFolderItem"/>, which should also provide some additionally
    /// required meta data which is used for further validation and auditing.
    /// </summary>
    /// <param name="submittedFolderPath">The path that was received as a part of a folder-related
    /// request.</param>
    /// <param name="context">The currently performed file system operation.</param>
    /// <returns>A <see cref="IVirtualFolderItem"/> which encapsulates a <see cref="VirtualFolderInfo"/>
    /// that represents the requested folder on the file system.</returns>
    /// <exception cref="InvalidResourcePathException">In case the format of the submitted path
    /// is invalid, meaning it cannot be interpreted as a valid resource identifier.</exception>
    /// <exception cref="VfsException">Exceptions will be handled by this base class and audited to
    /// the <see cref="FileSystemProviderBase.Auditor"/>. If auditing was already performed or should
    /// be suppressed, implementors can set the <see cref="VfsException.IsAudited"/> and
    /// <see cref="VfsException.SuppressAuditing"/> properties.</exception>
    /// <exception cref="Exception">Any exceptions that are not derived from
    /// <see cref="VfsException"/> will be wrapped and audited.</exception>
        public abstract IVirtualFolderItem ResolveFolderResourcePath(string submittedFolderPath, FileSystemTask context);


    /// <summary>
    /// Internally resolves a given folder resource by invoking <see cref="ResolveFolderResourcePath"/>,
    /// and performs basic exception handling, auditing, access and availability checks.<br/>
    /// This method may be overridden in case additional work needs to be done in order to resolve
    /// the resource based on the submitted path.
    /// </summary>
    /// <param name="virtualFolderPath">The submitted folder path that needs to be resolved.</param>
    /// <param name="mustExist">Whether the folder must exist on the file system. If this parameter is true
    /// and the received <typeparamref name="TFolder"/>'s <see cref="IVirtualFolderItem.Exists"/> is
    /// false, and <see cref="VirtualResourceNotFoundException"/> is being thrown.</param>
    /// <param name="context">The file system operation that is being performed during the invocation of
    /// this method. Used for internal auditing.</param>
    /// <returns>A wrapper item that includes the a <see cref="VirtualFolderInfo"/> that corresponds
    /// to the submitted <paramref name="virtualFolderPath"/>.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="virtualFolderPath"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If <paramref name="mustExist"/> is true, and
    /// the folder's <see cref="IVirtualFolderItem.Exists"/> property is false.</exception>
        protected virtual IVirtualFolderItem ResolveFolderResourcePathInternal(string virtualFolderPath, bool mustExist, FileSystemTask context)
    {
        IVirtualFolderItem folderItem;
      try
      {
        folderItem = ResolveFolderResourcePath(virtualFolderPath, context);
      }
      catch(InvalidResourcePathException e)
      {
        AuditHelper.AuditException(Auditor,e, context, AuditEvent.InvalidFolderPathFormat);
        throw;
      }
      catch (VfsException e)
      {
        //audit exception
        AuditHelper.AuditException(Auditor,e, context, AuditEvent.FolderResolveFailed);
        throw;
      }
      catch (Exception e)
      {
        //wrap exception and audit
        string msg = String.Format("Unexpected exception while resolving folder path [{0}]", virtualFolderPath);
        var rae = new ResourceAccessException(msg, e);

        AuditHelper.AuditException(Auditor,rae, context, AuditEvent.FolderResolveFailed);
        throw rae;
      }


      if (mustExist && !folderItem.Exists)
      {
        //audit and throw exception
        AuditHelper.AuditRequestedFolderNotFound(Auditor,folderItem, context);

        string msg = String.Format("Folder [{0}] not found on file system.", folderItem.ResourceInfo.FullName);
        throw new VirtualResourceNotFoundException(msg) { Resource = folderItem.ResourceInfo, IsAudited = true};
      }

      return folderItem;
    }

    #endregion


    #region get folder info / get file info

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
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    public override VirtualFileInfo GetFileInfo(string virtualFilePath)
    {
      const FileSystemTask context = FileSystemTask.FileInfoRequest;

      Func<VirtualFileInfo> func = () =>
                                     {
                                       var item = ResolveFileResourcePathInternal(virtualFilePath, true, context);
                                       AuditHelper.AuditFileInfoRequest(Auditor, context, item);
                                       return item.ResourceInfo;
                                     };


      return SecureFunc(context,
                        func, () => String.Format("Could not retrieve meta data for file [{0}]", virtualFilePath));
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
      ResolveFolderResourcePathInternal(virtualFolderPath, true, FileSystemTask.FolderInfoRequest);

      const FileSystemTask context = FileSystemTask.FolderInfoRequest;

      Func<VirtualFolderInfo> func = () =>
                                       {
                                         var item = ResolveFolderResourcePathInternal(virtualFolderPath, true, context);
                                         AuditHelper.AuditFolderInfoRequest(Auditor,context, item);
                                         return item.ResourceInfo;
                                       };


      return SecureFunc(context,
                        func, () => String.Format("Could not retrieve meta data for folder [{0}]", virtualFolderPath));
    }

    #endregion


    #region get file system root

    /// <summary>
    /// Internal implementation of the <see cref="GetFileSystemRoot"/>
    /// method, which is invoked by the base class. The base takes
    /// care of auditing and exception handling, so this implementing
    /// method should focus on item creation and custom validation.<br/>
    /// </summary>
    /// <returns>A <see cref="IVirtualFolderItem"/> which encapsulates
    /// a <see cref="VirtualFolderInfo"/> that represents the file
    /// system's root folder.</returns>
    protected abstract IVirtualFolderItem GetFileSystemRootImplementation();


    /// <summary>
    /// Gets the root of the file system. This is a dummy folder, which
    /// represents the file system as a whole, and provides the top level contents
    /// of the underlying file system as files and folders.
    /// </summary>
    public override VirtualFolderInfo GetFileSystemRoot()
    {
      return SecureFunc(FileSystemTask.RootFolderInfoRequest,
                        () =>
                          {
                            var item = GetFileSystemRootImplementation();
                            AuditHelper.AuditRootFolderRequest(Auditor,item);
                            return item.ResourceInfo;
                          }, () => "An error occurred while trying to get the file system root.");
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
      const FileSystemTask context = FileSystemTask.FileParentRequest;

      Func<VirtualFolderInfo> func = () => 
                                       {
                                         //get the submitted file
                                         var fileItem = ResolveFileResourcePathInternal(childFilePath, true, context);

                                         //delegate resolving of the parent path
                                         string parentFolderPath = fileItem.ResourceInfo.ParentFolderPath;

                                         //get the parent info
                                         var folderItem = ResolveFolderResourcePathInternal(parentFolderPath, true, context);
                                         AuditHelper.AuditFileParentRequest(Auditor,fileItem, folderItem);
                                         return folderItem.ResourceInfo;
                                       };

      return SecureFunc(context, func, () => String.Format("Could not retrieve parent folder of file [{0}]", childFilePath));
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
    /// <exception cref="ResourceAccessException">If the submitted child folder already
    /// represents the file system root.</exception>
    public override VirtualFolderInfo GetFolderParent(string childFolderPath)
    {
      const FileSystemTask context = FileSystemTask.FolderParentRequest;

      Func<VirtualFolderInfo> func = () =>
                                       {
                                         //get the submitted folder
                                         var childFolder = ResolveFolderResourcePathInternal(childFolderPath, true, context);

                                         //if the child folder already is the root, abort
                                         if(childFolder.ResourceInfo.IsRootFolder)
                                         {
                                             AuditHelper.AuditInvalidRootParentRequest(Auditor, childFolder);

                                           string msg = "Error while requesting parent of folder [{0}] - the folder itself already is the root.";
                                           msg = String.Format(msg, childFolder.ResourceInfo.FullName);
                                           throw new ResourceAccessException(msg) {Resource = childFolder.ResourceInfo, IsAudited = true};
                                         }

                                         //get the parent info
                                         string parentFolderPath = childFolder.ResourceInfo.ParentFolderPath;
                                         var parentFolder = ResolveFolderResourcePathInternal(parentFolderPath, true, context);
                                         AuditHelper.AuditFolderParentRequest(Auditor, childFolder, parentFolder);
                                         return parentFolder.ResourceInfo;
                                       };

      return SecureFunc(context, func, () => String.Format("Could not retrieve parent of folder [{0}]", childFolderPath));
    }

    #endregion


    #region get folder contents

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
      return GetChildFolders(parentFolderPath, true);
    }

    /// <summary>
    /// Gets all child folders of a given folder.
    /// </summary>
    /// <param name="parentFolderPath">The <see cref="VirtualResourceInfo.FullName"/>, which
    /// identifies the parent folder within the file system.</param>
    /// <param name="auditSuccess">Whether to audit the task or not. This parameter can be
    /// set because the method is invoked under different scenarios.</param>
    /// <returns>The child folders of the folder.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="parentFolderPath"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the folder that is represented
    /// by <paramref name="parentFolderPath"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    protected virtual IEnumerable<VirtualFolderInfo> GetChildFolders(string parentFolderPath, bool auditSuccess)
    {
      const FileSystemTask context = FileSystemTask.ChildFoldersRequest;

      Func<IEnumerable<VirtualFolderInfo>> func = () =>
                                                    {
                                                      var folderItem = ResolveFolderResourcePathInternal(parentFolderPath, true, context);

                                                      //validate user has permission to get child folders
                                                      var claims = Security.GetFolderClaims(folderItem);
                                                      if (!claims.AllowListContents)
                                                      {
                                                        string msg = String.Format("Accessing the contents of folder [{0}] is denied.", parentFolderPath);
                                                        AuditHelper.AuditDeniedFolderContentsRequest(Auditor,context, folderItem);
                                                        throw new ResourceAccessException(msg) {IsAudited = true};
                                                      }

                                                      //get the qualified paths of the child folders and construct folder items
                                                      IEnumerable<string> folderPaths = GetChildFolderPathsInternal(folderItem);
                                                      var folders = folderPaths.Select(path => ResolveFolderResourcePathInternal(path, true, context).ResourceInfo)
                                                                               .ToArray();

                                                      if (auditSuccess) AuditHelper.AuditFolderContentsRequest(Auditor, context, folderItem);
        
                                                      return folders;
                                                    };


      return SecureFunc(context, func, () => String.Format("Could not retrieve child folders of folder [{0}]", parentFolderPath));

    }


    /// <summary>
    /// Resolves all child folders of a given parent folder. This method is invoked
    /// by the <see cref="GetChildFolders"/> method in order to resolve the
    /// qualified paths of all child folders within that folder, which can be used in order to
    /// create <see cref="VirtualFolderInfo"/> instances based on the returned paths.
    /// </summary>
    /// <param name="parentFolder">The currently processed folder.</param>
    /// <returns>Folder paths that can be resolved to the folders withing the submitted
    /// <paramref name="parentFolder"/>.</returns>
    protected abstract IEnumerable<string> GetChildFolderPathsInternal(IVirtualFolderItem parentFolder);


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
      return GetChildFiles(parentFolderPath, true);
    }

    /// <summary>
    /// Gets all child files of a given folder.
    /// </summary>
    /// <param name="parentFolderPath">The <see cref="VirtualResourceInfo.FullName"/>, which
    /// identifies the parent folder within the file system.</param>
    /// <param name="auditSuccess">Whether to audit the task or not. This parameter can be
    /// set because the method is invoked under different scenarios.</param>
    /// <returns>The files of the submitted folder.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="parentFolderPath"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the folder that is represented
    /// by <paramref name="parentFolderPath"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    protected virtual IEnumerable<VirtualFileInfo> GetChildFiles(string parentFolderPath, bool auditSuccess)
    {
      const FileSystemTask context = FileSystemTask.ChildFilesRequest;

      Func<IEnumerable<VirtualFileInfo>> func = () =>
          {
            var folderItem = ResolveFolderResourcePathInternal(parentFolderPath, true, context);

            //validate user has permission to get child folders
            var claims = Security.GetFolderClaims(folderItem);
            if (!claims.AllowListContents)
            {
              string msg = String.Format("Accessing the contents of folder [{0}] is denied.", parentFolderPath);
              AuditHelper.AuditDeniedFolderContentsRequest(Auditor,context, folderItem);
              throw new ResourceAccessException(msg) { IsAudited = true };
            }

            //get the qualified paths of the child folders and construct folder items
            IEnumerable<string> filePaths = GetChildFilePathsInternal(folderItem);
            var folders =
              filePaths.Select(path => ResolveFileResourcePathInternal(path, true, context).ResourceInfo)
                       .ToArray();

            if (auditSuccess) AuditHelper.AuditFolderContentsRequest(Auditor, context, folderItem);

            return folders;
          };


      return SecureFunc(context, func, () => String.Format("Could not retrieve child files of folder [{0}]", parentFolderPath));
    }

    /// <summary>
    /// Resolves all child files of a given parent folder. This method is invoked
    /// by the <see cref="GetChildFiles"/> method in order to resolve the
    /// qualified paths of all files within that folder, which can be used in order to
    /// create <see cref="VirtualFileInfo"/> instances based on the returned paths.
    /// </summary>
    /// <param name="parentFolder">The currently processed folder.</param>
    /// <returns>File paths that can be resolved to the files withing the submitted
    /// <paramref name="parentFolder"/>.</returns>
    protected abstract IEnumerable<string> GetChildFilePathsInternal(IVirtualFolderItem parentFolder);


    /// <summary>
    /// Gets all files and folders of a given folder.
    /// </summary>
    /// <param name="parentFolderPath">The <see cref="VirtualResourceInfo.FullName"/>, which
    /// identifies the parent folder within the file system.</param>
    /// <returns>The files and folders of the submitted parent.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="parentFolderPath"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the folder that is represented
    /// by <paramref name="parentFolderPath"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    public override FolderContentsInfo GetFolderContents(string parentFolderPath)
    {
      const FileSystemTask context = FileSystemTask.FolderContentsRequest;

      Func<FolderContentsInfo> func = () =>
        {
          //do not rely on base class in order to ensure auditing
          var parent = ResolveFolderResourcePathInternal(parentFolderPath, true, context);
          var folders = GetChildFolders(parentFolderPath, false);
          var files = GetChildFiles(parentFolderPath, false);

          AuditHelper.AuditFolderContentsRequest(Auditor, context, parent);

          return new FolderContentsInfo(parent.ResourceInfo.FullName, folders, files);
        };

      return SecureFunc(context, func, () => String.Format("Could not retrieve contents of folder [{0}]", parentFolderPath));
    }

    #endregion


    #region check file / folder availability

    /// <summary>
    /// Checks whether a file resource at a given path exists or not.
    /// </summary>
    /// <param name="virtualFilePath">A path to the requested file.</param>
    /// <returns>True if a matching file was found.</returns>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    public override bool IsFileAvailable(string virtualFilePath)
    {
      const FileSystemTask context = FileSystemTask.CheckFileAvailability;
      Func<bool> func = () =>
                          {
                            bool status = IsFileAvailableInternal(virtualFilePath);
                            AuditHelper.AuditResourceAvailabilityCheck(Auditor,context, virtualFilePath, status);
                            return status;
                          };

      return SecureFunc(context, func,
                        () => String.Format("Could not determine availability of file [{0}].", virtualFilePath));
    }

    /// <summary>
    /// Checks whether a given file exists on the file system. This method is
    /// being invoked by <see cref="IsFileAvailable"/>.
    /// </summary>
    /// <param name="virtualFilePath">The received file path to be processed.</param>
    /// <returns>True if the file exists, otherwise false.</returns>
    protected abstract bool IsFileAvailableInternal(string virtualFilePath);


    /// <summary>
    /// Checks whether a folder resource at a given path exists or not.
    /// </summary>
    /// <param name="virtualFolderPath">A path to the requested folder.</param>
    /// <returns>True if a matching folder was found.</returns>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    public override bool IsFolderAvailable(string virtualFolderPath)
    {
      const FileSystemTask context = FileSystemTask.CheckFolderAvailability;
      Func<bool> func = () =>
                          {
                            bool status = IsFolderAvailableInternal(virtualFolderPath);
                            AuditHelper.AuditResourceAvailabilityCheck(Auditor,context, virtualFolderPath, status);
                            return status;
                          };

      return SecureFunc(context, func,
                        () => String.Format("Could not determine availability of folder [{0}].", virtualFolderPath));
    }


    /// <summary>
    /// Checks whether a given folder exists on the file system. This method is
    /// being invoked by <see cref="IsFolderAvailable"/>.
    /// </summary>
    /// <param name="virtualFolderPath">The received folder path to be processed.</param>
    /// <returns>True if the folder exists, otherwise false.</returns>
    protected abstract bool IsFolderAvailableInternal(string virtualFolderPath);

    #endregion


    #region create folder

    /// <summary>
    /// Creates a new folder in the file system.
    /// </summary>
    /// <param name="virtualFolderPath">The qualified path of the folder to be created.</param>
    /// <returns>A <see cref="VirtualFolderInfo"/> instance which represents
    /// the created folder.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="virtualFolderPath"/>
    /// is a null reference.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the designated parent
    /// folder does not exist.</exception>
    /// <exception cref="ResourceOverwriteException">If the folder already exists on the file
    /// system.</exception>
    public override VirtualFolderInfo CreateFolder(string virtualFolderPath)
    {
      Func<VirtualFolderInfo> func = () => CreateFolderImpl(virtualFolderPath);
      return SecureFunc(FileSystemTask.FolderCreateRequest, func, 
        () => String.Format("Could not create folder [{0}].", virtualFolderPath));
    }

    /// <summary>
    /// Implementation of the <see cref="CreateFolder"/> method, which runs
    /// within <see cref="FileSystemProviderBase.SecureFunc{T}"/>.
    /// </summary>
    private VirtualFolderInfo CreateFolderImpl(string virtualFolderPath)
    {
      const FileSystemTask context = FileSystemTask.FolderCreateRequest;

      //get the folder item without requiring it to already exist
      var folderItem = ResolveFolderResourcePathInternal(virtualFolderPath, false, context);

      //if the parent folder does not allow folders to be added, throw exception
      var parentPath = folderItem.ResourceInfo.ParentFolderPath;
      var parentFolder = ResolveFolderResourcePathInternal(parentPath, true, context);

      var claims = Security.GetFolderClaims(parentFolder);
      if(!claims.AllowAddFolders)
      {
        AuditHelper.AuditDeniedFolderCreation(Auditor,AuditEvent.CreateFolderDenied, folderItem);

        string msg = "Missing authorization to create folder [{0}] on the file system.";
        msg = String.Format(msg, virtualFolderPath);
        throw new ResourceAccessException(msg) { IsAudited = true };
      }

      //if folder already exists throw exception
      if (folderItem.Exists)
      {
        AuditHelper.AuditDeniedFolderCreation(Auditor,AuditEvent.FolderAlreadyExists, folderItem);

        string msg = "Cannot create folder [{0}] - it already exists on the file system.";
        msg = String.Format(msg, virtualFolderPath);
        throw new ResourceOverwriteException(msg) {IsAudited = true};
      }

      //locking the folder to be created also makes the parent folder read-only
      using (RequestChainedLockGuard(folderItem, ResourceLockType.Write))
      {
        folderItem = CreateFolderOnFileSystem(folderItem);
      }

      //audit creation
      AuditHelper.AuditFolderCreation(Auditor, folderItem);

      //just requery like a regular request in order to make sure meta data
      //is up-to-date
      return ResolveFolderResourcePathInternal(virtualFolderPath, true, context).ResourceInfo;
    }


    /// <summary>
    /// This method is invoked by <see cref="CreateFolder"/> in order
    /// to handle the creation of a physical folder on the file system.
    /// </summary>
    /// <param name="folder">Describes the (currently unavailable folder).</param>
    /// <returns>Either the updated <paramref name="folder"/> reference that
    /// was submitted, or a new <see cref="TFolder"/> that represents the
    /// created folder.</returns>
    protected abstract IVirtualFolderItem CreateFolderOnFileSystem(IVirtualFolderItem folder);

    #endregion


    #region delete folder

    /// <summary>
    /// Deletes a given folder from the file system.
    /// </summary>
    /// <param name="virtualFolderPath">The qualified path of the folder to be created.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="virtualFolderPath"/>
    /// is a null reference.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the
    /// folder does not exist.</exception>
    /// <remarks>
    /// This base method validates the following cases before attempting to delete:
    /// <list type="bullet">
    /// <item>Whether the folder exists.</item>
    /// <item>Whether the folder is the file system root.</item>
    /// <item>Whether the folder permissions indicate the folder can be deleted.</item>
    /// </list>
    /// Additional validation can be performed in the implementation of the
    /// <see cref="DeleteFolderOnFileSystem"/> method before performing the task.
    /// </remarks>
    public override void DeleteFolder(string virtualFolderPath)
    {
      SecureAction(FileSystemTask.FolderDeleteRequest,
                   () => DeleteFolderImpl(virtualFolderPath),
                   () => String.Format("Could not delete folder [{0}]", virtualFolderPath));
    }

    private void DeleteFolderImpl(string virtualFolderPath)
    {
      //get the resource
      const FileSystemTask context = FileSystemTask.FolderDeleteRequest;

      IVirtualFolderItem folder = ResolveFolderResourcePathInternal(virtualFolderPath, true, context);

      //the root folder cannot be deleted
      if (folder.ResourceInfo.IsRootFolder)
      {
        AuditHelper.AuditDeleteRootAttempt(Auditor,folder);

        const string msg = "Root folder cannot be deleted.";
        throw new ResourceAccessException(msg) {IsAudited = true};
      }

      //get the security context and check folder permission
      FolderClaims claims = Security.GetFolderClaims(folder);
      if (!claims.AllowDelete)
      {
        AuditHelper.AuditDeniedOperation(Auditor,context, AuditEvent.DeleteFolderDenied, folder);

        string msg = String.Format("Deletion of folder [{0}] is not permitted.", virtualFolderPath);
        throw new ResourceAccessException(msg) { IsAudited = true };
      }

      //get an exclusive lock for the folder
      using (RequestChainedLockGuard(folder, ResourceLockType.Write))
      {
        DeleteFolderOnFileSystem(folder);
      }

      AuditHelper.AuditResourceOperation(Auditor,context, AuditEvent.FolderDeleted, folder);
    }


    /// <summary>
    /// Physically deletes a given folder on the file system. This method is
    /// being invoked by the <see cref="DeleteFolder"/> method.
    /// </summary>
    /// <param name="folder">The folder to be deleted.</param>
    protected abstract void DeleteFolderOnFileSystem(IVirtualFolderItem folder);

    #endregion


    #region delete file

    /// <summary>
    /// Deletes a given file from the file system.
    /// </summary>
    /// <param name="virtualFilePath">The qualified path of the file to be created.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="virtualFilePath"/>
    /// is a null reference.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the
    /// file does not exist.</exception>
    /// <remarks>
    /// This base method validates the following cases before attempting to delete:
    /// <list type="bullet">
    /// <item>Whether the file exists.</item>
    /// <item>Whether the file permissions indicate it can be deleted.</item>
    /// </list>
    /// Additional validation can be performed in the implementation of the
    /// <see cref="DeleteFileOnFileSystem"/> method before performing the task.
    /// </remarks>
    public override void DeleteFile(string virtualFilePath)
    {
      const FileSystemTask context = FileSystemTask.FileDeleteRequest;
      SecureAction(context, () => DeleteFileImpl(virtualFilePath), 
                   () => String.Format("Could not delete file [{0}].", virtualFilePath));
    }
  

    private void DeleteFileImpl(string virtualFilePath)
    {
      const FileSystemTask context = FileSystemTask.FileDeleteRequest;
      var file = ResolveFileResourcePathInternal(virtualFilePath, true, context);

      var claims = Security.GetFileClaims(file);
      if (!claims.AllowDelete)
      {
        AuditHelper.AuditDeniedOperation(Auditor,context, AuditEvent.DeleteFileDenied, file);

        string msg = String.Format("Deletion of file [{0}] is not permitted.", virtualFilePath);
        throw new ResourceAccessException(msg) { IsAudited = true };
      }

      //acquire a lock and execute deletion
      bool status = LockResourceAndExecute(file, context, ResourceLockType.Write, () => DeleteFileOnFileSystem(file));

      //audit deletion
      if (status)
      {
        AuditHelper.AuditResourceOperation(Auditor,context, AuditEvent.FileDeleted, file);
      }
      else
      {
        string msg = "Could not delete file [{0}] - the file is currently locked.";
        msg = String.Format(msg, virtualFilePath);
        throw new ResourceLockedException(msg);
      }
    }



    /// <summary>
    /// Physically deletes a given file on the file system. This method is
    /// being invoked by the <see cref="DeleteFile"/> method.
    /// </summary>
    /// <param name="file">The file to be deleted.</param>
    protected abstract void DeleteFileOnFileSystem(IVirtualFileItem file);

    #endregion


    #region move / copy folder

    /// <summary>
    /// Moves a given folder and all its contents to a new destination.
    /// </summary>
    /// <param name="virtualFolderPath">A qualified name (corresponding to
    /// <see cref="VirtualResourceInfo.FullName"/> that identifies the resource
    /// in the file system.</param>
    /// <param name="destinationPath">The new path of the resource. Can be another name
    /// for the resource itself.</param>
    /// <returns>A <see cref="VirtualFolderInfo"/> object that represents the new
    /// directory in the file system.</returns>
    /// <exception cref="ArgumentNullException">If any of the parameters is a
    /// null reference.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access, or if the operation is not possible (e.g. a resource being
    /// moved/copied to itself).</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the resource that
    /// should be moved does not exist in the file system.</exception>
    /// <exception cref="ResourceOverwriteException">If a resource that matches the
    /// submitted <paramref name="destinationPath"/> already exists.</exception>
    /// <remarks>
    /// This base method validates the following cases before attempting to move the
    /// folder:
    /// <list type="bullet">
    /// <item>Whether the source exists.</item>
    /// <item>Whether permission to access the folder is granted (<see cref="FolderClaims.AllowListContents"/>).</item>
    /// <item>Whether source folder can be deleted.</item>
    /// <item>Whether the target folder already exists or not (causes exception).</item>
    /// <item>Whether the target folder can be created in its designated parent folder.</item>
    /// <item>Whether source and target are the same.</item>
    /// </list>
    /// Additional validation can be performed in the implementation of the
    /// <see cref="MoveFolderOnFileSystem"/> method before performing the task.
    /// </remarks>
    public override VirtualFolderInfo MoveFolder(string virtualFolderPath, string destinationPath)
    {
      Func<VirtualFolderInfo> func = () => MoveFolderImpl(virtualFolderPath, destinationPath);

      return SecureFunc(FileSystemTask.FolderMoveRequest, func,
                        () => String.Format("Could not move folder [{0}] to [{1}].", virtualFolderPath, destinationPath));
    }



    private VirtualFolderInfo MoveFolderImpl(string virtualFolderPath, string destinationPath)
    {
      //make sure the folder exists
      const FileSystemTask context = FileSystemTask.FolderMoveRequest;
      var sourceFolder = ResolveFolderResourcePathInternal(virtualFolderPath, true, context);

      //make sure we have the permission to delete it (after all, moving removes it)
      var claims = Security.GetFolderClaims(sourceFolder);
      if (!claims.AllowDelete)
      {
        AuditHelper.AuditDeniedOperation(Auditor,context, AuditEvent.DeleteFolderDenied, sourceFolder);

        string msg = String.Format("Removing folder [{0}] from its current location is not permitted.", virtualFolderPath);
        throw new ResourceAccessException(msg) { IsAudited = true };
      }

      if (!claims.AllowListContents)
      {
        AuditHelper.AuditDeniedOperation(Auditor,context, AuditEvent.ListFolderContentsDenied, sourceFolder);

        string msg = String.Format("Accessing contents of source folder [{0}] is not permitted.", virtualFolderPath);
        throw new ResourceAccessException(msg) { IsAudited = true };
      }

      //get the destination folder item
      var targetFolder = ResolveFolderResourcePathInternal(destinationPath, false, context);
      
      //get the parent folder of the destination path, must exist
      var targetParentPath = targetFolder.ResourceInfo.ParentFolderPath;
      var targetParent = ResolveFolderResourcePathInternal(targetParentPath, true, context);

      //make sure we can create folders at the destination
      claims = Security.GetFolderClaims(targetParent);
      if(!claims.AllowAddFolders)
      {
        AuditHelper.AuditDeniedOperation(Auditor,context, AuditEvent.CreateFolderDenied, sourceFolder);

        string msg = "Cannot move folder [{0}] to [{1}] - creating a folder at the destination path is not permitted.";
        msg = String.Format(msg, virtualFolderPath, destinationPath);
        throw new ResourceAccessException(msg) { IsAudited = true };
      }


      //make sure source and destination paths are not equal
      if(sourceFolder.QualifiedIdentifier == targetFolder.QualifiedIdentifier)
      {
        AuditHelper.AuditDeniedOperation(Auditor,context, AuditEvent.SourceEqualsDestination, sourceFolder);

        string msg = "Cannot move folder [{0}] to [{1}] - source and target locations are the same.";
        msg = String.Format(msg, virtualFolderPath, destinationPath);
        throw new ResourceAccessException(msg) { IsAudited = true };
      }

      //make sure the destination folder does not exist
      if (targetFolder.Exists)
      {
        AuditHelper.AuditDeniedOperation(Auditor,context, AuditEvent.FolderAlreadyExists, targetFolder);

        string msg = "Cannot move folder [{0}] to [{1}] - a folder already exists at this location.";
        msg = String.Format(msg, virtualFolderPath, destinationPath);
        throw new ResourceOverwriteException(msg) { IsAudited = true };
      }

      //lock source and target folders exclusively
      using(RequestChainedLockGuard(sourceFolder, ResourceLockType.Write))
      {
        using(RequestChainedLockGuard(targetFolder, ResourceLockType.Write))
        {
          //delegate moving on file system
          MoveFolderOnFileSystem(sourceFolder, targetFolder);
        }
      }

      //get a fresh item for the copy
      var copy = ResolveFolderResourcePathInternal(targetFolder.ResourceInfo.FullName, true, context);

      //audit successful operation and return result
      AuditHelper.AuditResourceOperation(Auditor,context, AuditEvent.FolderMoved, sourceFolder, copy);
      return copy.ResourceInfo;
    }


    /// <summary>
    /// Moves a physical folder on the file system from one location to the other. This
    /// method is invoked by <see cref="MoveFolder"/>.
    /// </summary>
    /// <param name="sourceFolder">The folder to be copied.</param>
    /// <param name="targetFolder">The designated new location of the folder.</param>
    protected abstract void MoveFolderOnFileSystem(IVirtualFolderItem sourceFolder, IVirtualFolderItem targetFolder);


    /// <summary>
    /// Copies a given folder and all its contents to a new destination.
    /// </summary>
    /// <param name="virtualFolderPath">A qualified name (corresponding to
    /// <see cref="VirtualResourceInfo.FullName"/> that identifies the resource
    /// in the file system.</param>
    /// <param name="destinationPath">The new path of the resource. Can be another name
    /// for the resource itself.</param>
    /// <returns>A <see cref="VirtualFolderInfo"/> object that represents the new
    /// directory in the file system.</returns>
    /// <exception cref="ArgumentNullException">If any of the parameters is a
    /// null reference.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access, or if the operation is not possible (e.g. a resource being
    /// moved/copied to itself).</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the resource that
    /// should be moved does not exist in the file system.</exception>
    /// <exception cref="ResourceOverwriteException">If a resource that matches the
    /// submitted <paramref name="destinationPath"/> already exists.</exception>
    /// <remarks>
    /// This base method validates the following cases before attempting to copy the
    /// folder:
    /// <list type="bullet">
    /// <item>Whether the source exists.</item>
    /// <item>Whether permission to access the folder is granted (<see cref="FolderClaims.AllowListContents"/>).</item>
    /// <item>Whether the target folder already exists or not (causes exception).</item>
    /// <item>Whether the target folder can be created in its designated parent folder.</item>
    /// <item>Whether source and target are the same.</item>
    /// </list>
    /// Additional validation can be performed in the implementation of the
    /// <see cref="MoveFolderOnFileSystem"/> method before performing the task.
    /// </remarks>
    public override VirtualFolderInfo CopyFolder(string virtualFolderPath, string destinationPath)
    {
      Func<VirtualFolderInfo> func = () => CopyFolderImpl(virtualFolderPath, destinationPath);

      return SecureFunc(FileSystemTask.FolderCopyRequest, func,
                        () => String.Format("Could not copy folder [{0}] to [{1}].", virtualFolderPath, destinationPath));
    }


    private VirtualFolderInfo CopyFolderImpl(string virtualFolderPath, string destinationPath)
    {
      //make sure the folder exists
      const FileSystemTask context = FileSystemTask.FolderCopyRequest;
      var sourceFolder = ResolveFolderResourcePathInternal(virtualFolderPath, true, context);

      //make sure we have the permission to read the folder's contents
      var claims = Security.GetFolderClaims(sourceFolder);
      if (!claims.AllowListContents)
      {
        AuditHelper.AuditDeniedOperation(Auditor,context, AuditEvent.ListFolderContentsDenied, sourceFolder);

        string msg = String.Format("Accessing contents of source folder [{0}] is not permitted.", virtualFolderPath);
        throw new ResourceAccessException(msg) { IsAudited = true };
      }

      //get the destination folder item
      var targetFolder = ResolveFolderResourcePathInternal(destinationPath, false, context);

      //get the parent folder of the destination path, must exist
      var targetParentPath = targetFolder.ResourceInfo.ParentFolderPath;
      var targetParent = ResolveFolderResourcePathInternal(targetParentPath, true, context);

      //make sure we can create folders at the destination
      claims = Security.GetFolderClaims(targetParent);
      if (!claims.AllowAddFolders)
      {
        AuditHelper.AuditDeniedOperation(Auditor,context, AuditEvent.CreateFolderDenied, sourceFolder);

        string msg = "Cannot create a copy of the [{0}] at [{1}] - creating a folder at the destination path is not permitted.";
        msg = String.Format(msg, virtualFolderPath, destinationPath);
        throw new ResourceAccessException(msg) { IsAudited = true };
      }


      //make sure source and destination paths are not equal
      if (sourceFolder.QualifiedIdentifier == targetFolder.QualifiedIdentifier)
      {
        AuditHelper.AuditDeniedOperation(Auditor,context, AuditEvent.SourceEqualsDestination, sourceFolder);

        string msg = "Cannot move copy [{0}] to [{1}] - source and target locations are the same.";
        msg = String.Format(msg, virtualFolderPath, destinationPath);
        throw new ResourceAccessException(msg) { IsAudited = true };
      }

      //make sure the destination folder does not exist
      if (targetFolder.Exists)
      {
        AuditHelper.AuditDeniedOperation(Auditor,context, AuditEvent.FolderAlreadyExists, targetFolder);

        string msg = "Cannot copy folder [{0}] to [{1}] - a folder already exists at this location.";
        msg = String.Format(msg, virtualFolderPath, destinationPath);
        throw new ResourceOverwriteException(msg) { IsAudited = true };
      }

      //lock source for reading, target for writing
      using(RequestChainedLockGuard(sourceFolder, ResourceLockType.Read))
      {
        using(RequestChainedLockGuard(targetFolder, ResourceLockType.Write))
        {
          //delegate copying on file system
          CopyFolderOnFileSystem(sourceFolder, targetFolder);
        }
      }

      //get a fresh item for the copy
      var copy = ResolveFolderResourcePathInternal(targetFolder.ResourceInfo.FullName, true, context);

      //audit successful operation and return result
      AuditHelper.AuditResourceOperation(Auditor,context, AuditEvent.FolderCopied, sourceFolder, copy);
      return copy.ResourceInfo;
    }


    /// <summary>
    /// Copies a physical folder on the file system from one location to the other. This
    /// method is invoked by <see cref="CopyFolder"/>.
    /// </summary>
    /// <param name="sourceFolder">The folder to be copied.</param>
    /// <param name="targetFolder">The designated location of the copy.</param>
    protected abstract void CopyFolderOnFileSystem(IVirtualFolderItem sourceFolder, IVirtualFolderItem targetFolder);

    #endregion


    #region move / copy file

    /// <summary>
    /// Moves a given file to a new destination.
    /// </summary>
    /// <param name="virtualFilePath">A qualified name (corresponding to
    /// <see cref="VirtualResourceInfo.FullName"/> that identifies the resource
    /// in the file system.</param>
    /// <param name="destinationPath">The new path of the resource. Can be another name
    /// for the resource itself.</param>
    /// <returns>A <see cref="VirtualFileInfo"/> object that represents the new
    /// file in the file system.</returns>
    /// <exception cref="ArgumentNullException">If any of the parameters is a
    /// null reference.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access, or if the operation is not possible (e.g. a resource being
    /// moved/copied to itself).</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the resource that
    /// should be moved does not exist in the file system.</exception>
    /// <exception cref="ResourceOverwriteException">If a resource that matches the
    /// submitted <paramref name="destinationPath"/> already exists.</exception>
    /// <remarks>
    /// This base method validates the following cases before attempting to move the
    /// file:
    /// <list type="bullet">
    /// <item>Whether the source exists.</item>
    /// <item>Whether source file can be deleted.</item>
    /// <item>Whether the target file already exists or not (causes exception).</item>
    /// <item>Whether the target file can be created in its designated parent folder.</item>
    /// <item>Whether source and target are the same.</item>
    /// </list>
    /// Additional validation can be performed in the implementation of the
    /// <see cref="MoveFileOnFileSystem"/> method before performing the task.
    /// </remarks>
    public override VirtualFileInfo MoveFile(string virtualFilePath, string destinationPath)
    {
      Func<VirtualFileInfo> func = () => MoveFileImpl(virtualFilePath, destinationPath);

      return SecureFunc(FileSystemTask.FileMoveRequest, func,
                        () => String.Format("Could not move file [{0}] to [{1}].", virtualFilePath, destinationPath));
    }


    private VirtualFileInfo MoveFileImpl(string virtualFilePath, string destinationPath)
    {
      //make sure the folder exists
      const FileSystemTask context = FileSystemTask.FileMoveRequest;
      var sourceFile = ResolveFileResourcePathInternal(virtualFilePath, true, context);

      //make sure we have the permission to delete it (after all, moving removes it)
      var claims = Security.GetFileClaims(sourceFile);
      if (!claims.AllowDelete)
      {
        AuditHelper.AuditDeniedOperation(Auditor,context, AuditEvent.DeleteFileDenied, sourceFile);

        string msg = String.Format("Removing file [{0}] from its current location is not permitted.", virtualFilePath);
        throw new ResourceAccessException(msg) { IsAudited = true };
      }


      //get the destination folder item
      var targetFile = ResolveFileResourcePathInternal(destinationPath, false, context);
      
      //get the parent folder of the destination path, must exist
      var targetParentPath = targetFile.ResourceInfo.ParentFolderPath;
      var targetParent = ResolveFolderResourcePathInternal(targetParentPath, true, context);

      //make sure we can create files at the destination
      var parentClaims = Security.GetFolderClaims(targetParent);
      if(!parentClaims.AllowAddFiles)
      {
        AuditHelper.AuditDeniedOperation(Auditor,context, AuditEvent.CreateFileDenied, sourceFile);

        string msg = "Cannot move file [{0}] to [{1}] - creating a file at the destination path is not permitted.";
        msg = String.Format(msg, virtualFilePath, destinationPath);
        throw new ResourceAccessException(msg) { IsAudited = true };
      }

      //make sure source and destination paths are not equal
      if (sourceFile.QualifiedIdentifier == targetFile.QualifiedIdentifier)
      {
        AuditHelper.AuditDeniedOperation(Auditor,context, AuditEvent.SourceEqualsDestination, sourceFile);

        string msg = "Cannot move file [{0}] to [{1}] - source and target locations are the same.";
        msg = String.Format(msg, virtualFilePath, destinationPath);
        throw new ResourceAccessException(msg) { IsAudited = true };
      }

      //make sure the destination file does not exist
      if (targetFile.Exists)
      {
        AuditHelper.AuditDeniedOperation(Auditor,context, AuditEvent.FileAlreadyExists, targetFile);

        string msg = "Cannot move file [{0}] to [{1}] - a file already exists at this location.";
        msg = String.Format(msg, virtualFilePath, destinationPath);
        throw new ResourceOverwriteException(msg) { IsAudited = true };
      }

      //lock source file exclusively, ensure target is fully locked
      using(RequestChainedLockGuard(sourceFile, ResourceLockType.Write))
      {
        using(RequestChainedLockGuard(targetFile, ResourceLockType.Write))
        {
          //delegate moving on file system
          MoveFileOnFileSystem(sourceFile, targetFile);
        }
      }

      //get a fresh item for the copy
      var copy = ResolveFileResourcePathInternal(targetFile.ResourceInfo.FullName, true, context);

      //audit successful operation and return result
      AuditHelper.AuditResourceOperation(Auditor,context, AuditEvent.FileMoved, sourceFile, copy);
      return copy.ResourceInfo;
    }


    /// <summary>
    /// Moves a physical file on the file system from one location to the other. This
    /// method is invoked by <see cref="MoveFile"/>.
    /// </summary>
    /// <param name="sourceFile">The file to be moved.</param>
    /// <param name="targetFile">The designated new location of the file.</param>
    protected abstract void MoveFileOnFileSystem(IVirtualFileItem sourceFile, IVirtualFileItem targetFile);


    /// <summary>
    /// Copies a given file to a new destination.
    /// </summary>
    /// <param name="virtualFilePath">A qualified name (corresponding to
    /// <see cref="VirtualResourceInfo.FullName"/> that identifies the resource
    /// in the file system.</param>
    /// <param name="destinationPath">The new path of the resource. Can be another name
    /// for the resource itself.</param>
    /// <returns>A <see cref="VirtualFileInfo"/> object that represents the new
    /// file in the file system.</returns>
    /// <exception cref="ArgumentNullException">If any of the parameters is a
    /// null reference.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access, or if the operation is not possible (e.g. a resource being
    /// moved/copied to itself).</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the resource that
    /// should be moved does not exist in the file system.</exception>
    /// <exception cref="ResourceOverwriteException">If a resource that matches the
    /// submitted <paramref name="destinationPath"/> already exists.</exception>
    /// <remarks>
    /// This base method validates the following cases before attempting to copy the
    /// file:
    /// <list type="bullet">
    /// <item>Whether the source exists.</item>
    /// <item>Whether the target file already exists or not (causes exception).</item>
    /// <item>Whether the target file can be created in its designated parent folder.</item>
    /// <item>Whether source and target are the same.</item>
    /// </list>
    /// Additional validation can be performed in the implementation of the
    /// <see cref="CopyFileOnFileSystem"/> method before performing the task.
    /// </remarks>
    public override VirtualFileInfo CopyFile(string virtualFilePath, string destinationPath)
    {
      Func<VirtualFileInfo> func = () => CopyFileImpl(virtualFilePath, destinationPath);

      return SecureFunc(FileSystemTask.FileCopyRequest, func,
                        () => String.Format("Could not copy file [{0}] to [{1}].", virtualFilePath, destinationPath));
    }


    private VirtualFileInfo CopyFileImpl(string virtualFilePath, string destinationPath)
    {
      //make sure the folder exists
      const FileSystemTask context = FileSystemTask.FileCopyRequest;
      var sourceFile = ResolveFileResourcePathInternal(virtualFilePath, true, context);

      //get the destination folder item
      var targetFile = ResolveFileResourcePathInternal(destinationPath, false, context);

      //get the parent folder of the destination path, must exist
      var targetParentPath = targetFile.ResourceInfo.ParentFolderPath;
      var targetParent = ResolveFolderResourcePathInternal(targetParentPath, true, context);

      //make sure we can create files at the destination
      var parentClaims = Security.GetFolderClaims(targetParent);
      if (!parentClaims.AllowAddFiles)
      {
        AuditHelper.AuditDeniedOperation(Auditor,context, AuditEvent.CreateFileDenied, sourceFile);

        string msg = "Cannot copy file [{0}] to [{1}] - creating a file at the destination path is not permitted.";
        msg = String.Format(msg, virtualFilePath, destinationPath);
        throw new ResourceAccessException(msg) { IsAudited = true };
      }


      //make sure source and destination paths are not equal
      if (sourceFile.QualifiedIdentifier == targetFile.QualifiedIdentifier)
      {
        AuditHelper.AuditDeniedOperation(Auditor,context, AuditEvent.SourceEqualsDestination, sourceFile);

        string msg = "Cannot copy file [{0}] to [{1}] - source and target locations are the same.";
        msg = String.Format(msg, virtualFilePath, destinationPath);
        throw new ResourceAccessException(msg) { IsAudited = true };
      }

      //make sure the destination file does not exist
      if (targetFile.Exists)
      {
        AuditHelper.AuditDeniedOperation(Auditor,context, AuditEvent.FileAlreadyExists, targetFile);

        string msg = "Cannot copy file [{0}] to [{1}] - a file already exists at this location.";
        msg = String.Format(msg, virtualFilePath, destinationPath);
        throw new ResourceOverwriteException(msg) { IsAudited = true };
      }

      //lock source file for reading, ensure target is fully locked
      using (RequestChainedLockGuard(sourceFile, ResourceLockType.Read))
      {
        using (RequestChainedLockGuard(targetFile, ResourceLockType.Write))
        {
          //delegate copying on file system
          CopyFileOnFileSystem(sourceFile, targetFile);
        }
      }

      //get a fresh item for the copy
      var copy = ResolveFileResourcePathInternal(targetFile.ResourceInfo.FullName, true, context);

      //audit successful operation and return result
      AuditHelper.AuditResourceOperation(Auditor,context, AuditEvent.FileCopied, sourceFile, copy);

      return copy.ResourceInfo;
    }

    /// <summary>
    /// Copies a physical file on the file system from one location to the other. This
    /// method is invoked by <see cref="CopyFile"/>.
    /// </summary>
    /// <param name="sourceFile">The file to be copied.</param>
    /// <param name="targetFile">The designated location of the file copy.</param>
    protected abstract void CopyFileOnFileSystem(IVirtualFileItem sourceFile, IVirtualFileItem targetFile);

    #endregion


    #region read / write file data

    /// <summary>
    /// Gets the binary contents as a stream in a blocking operation.
    /// Use the methods in <see cref="ContentUtil"/> class for simplified stream
    /// handling.
    /// </summary>
    /// <param name="virtualFilePath">The path of the file to be read.</param>
    /// <returns>A stream that allows the contents of the file to be read.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="virtualFilePath"/>
    /// is a null reference.</exception>
    /// <exception cref="VirtualResourceNotFoundException">If the file that is represented
    /// by <paramref name="virtualFilePath"/> does not exist in the file system.</exception>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    public override Stream ReadFileContents(string virtualFilePath)
    {
      const FileSystemTask context = FileSystemTask.StreamedFileDownloadRequest;

      Func<Stream> func = () => DownloadTransfers.ReadFile(virtualFilePath);
      
      return SecureFunc(context, func, () => String.Format("Could not read data of file [{0}]", virtualFilePath));
    }


    /// <summary>
    /// Creates a stream to read the data of a given file from the file system.
    /// This method is invoked by <see cref="ReadFileContents"/> after having
    /// performed access checks.
    /// </summary>
    /// <param name="fileItem">Represents the file to be read.</param>
    /// <returns>A stream that provides the file's binary data.</returns>
    protected abstract Stream OpenFileStreamFromFileSystem(IVirtualFileItem fileItem);


    /// <summary>
    /// Creates or updates a given file resource in the file system.
    /// </summary>
    /// <param name="virtualFilePath">The qualified path of the file to be created.</param>
    /// <param name="input">A stream that provides the file's contents.</param>
    /// <param name="overwrite">Whether an existing file should be overwritten
    /// or not. If this parameter is false and the file already exists, a
    /// <see cref="ResourceOverwriteException"/> is thrown.</param>
    /// <param name="resourceLength">The length of the resource to be uploaded in bytes.</param>
    /// <param name="contentType">The content type of the uploaded resource.</param>
    /// <exception cref="ResourceAccessException">In case of invalid or prohibited
    /// resource access.</exception>
    /// <exception cref="ResourceOverwriteException">If a file already exists at the
    /// specified location, and the <paramref name="overwrite"/> flag was not set.</exception>
    /// <exception cref="ArgumentNullException">If any of the parameters is a null reference.</exception>
    public override VirtualFileInfo WriteFile(string virtualFilePath, Stream input, bool overwrite, long resourceLength, string contentType)
    {
      Func<VirtualFileInfo> func = () =>
                                     {
                                       UploadTransfers.WriteFile(virtualFilePath, input, overwrite, resourceLength, contentType);

                                       //update file info
                                       var file = ResolveFileResourcePathInternal(virtualFilePath, false, FileSystemTask.UploadTokenRequest);
                                       return file.ResourceInfo;
                                     };

      return SecureFunc(FileSystemTask.UploadTokenRequest, func,
                        () => String.Format("Could not write streamed data of file [{0}] to file system.", virtualFilePath));
    }


    /// <summary>
    /// Creates a stream to write the data of a given file to the file system.
    /// This method is invoked by <see cref="WriteFile"/> after having
    /// performed access checks.
    /// </summary>
    /// <param name="fileItem">Represents the file to be created or updated.</param>
    /// <param name="input">A stream that provides the file's contents.</param>
    protected abstract void WriteFileStreamToFileSystem(IVirtualFileItem fileItem, Stream input);

    #endregion


    #region create file / folder paths

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
      return SecureFunc(FileSystemTask.CreateFilePathRequest,
                        () => CreateFileSystemFilePath(parentFolder, fileName),
                        () => String.Format("Could not create file path based on folder [{0}] and file [{1}]", parentFolder, fileName));
    }


    /// <summary>
    /// Combines two virtual paths to a string that can be interpreted by the provider.
    /// This implementing method is invoked by <see cref="CreateFilePath"/>.
    /// </summary>
    /// <param name="parentFolder">The qualified name of the parent
    /// folder.</param>
    /// <param name="fileName">The name of a file within the folder.</param>
    /// <returns>An qualified path name for the submitted
    /// <paramref name="fileName"/>.</returns>
    protected abstract string CreateFileSystemFilePath(string parentFolder, string fileName);


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
      return SecureFunc(FileSystemTask.CreateFilePathRequest,
                        () => CreateFileSystemFolderPath(parentFolder, folderName),
                        () => String.Format("Could not create file path based on folder [{0}] and file [{1}]", parentFolder, folderName));
    }


    /// <summary>
    /// Creates a qualified name that can be used as an identifier
    /// for a given folder of the file system.<br/>
    /// This implementing method is invoked by <see cref="CreateFolderPath"/>.
    /// </summary>
    /// <param name="parentFolder">The qualified name of the parent
    /// folder.</param>
    /// <param name="folderName">The name of the child folder.</param>
    /// <returns>An qualified path name for the submitted
    /// <paramref name="folderName"/>.</returns>
    protected abstract string CreateFileSystemFolderPath(string parentFolder, string folderName);

    #endregion


    #region locking

    /// <summary>
    /// Creates a <see cref="ResourceLockGuard"/> that encapsulates
    /// a read or write lock for a given file as well as read locks
    /// for all its parent folders.
    /// </summary>
    /// <param name="file">The file to be locked.</param>
    /// <param name="lockType">Whether a read or a write lock is being
    /// required for the <paramref name="file"/>.</param>
    /// <returns>A guard which releases the resource and all folders once it is being disposed.
    /// If locking did not succeed because an element in the chain could not be locked,
    /// the returned <see cref="ResourceLockGuard.IsLockEnabled"/> property
    /// is false.</returns>
    protected virtual ResourceLockGuard RequestChainedLockGuard(IVirtualFileItem file, ResourceLockType lockType)
    {
      List<string> parentFolders = GetResourceLockChain(file);
      
      switch(lockType)
      {
        case ResourceLockType.Read:
          return LockRepository.GetResourceChainLock(file.QualifiedIdentifier, false, parentFolders);
        case ResourceLockType.Write:
          return LockRepository.GetResourceChainLock(file.QualifiedIdentifier, true, parentFolders);
        default:
          throw new ArgumentOutOfRangeException("lockType", "Unknown lock type: " + lockType);
      }

    }


    /// <summary>
    /// Creates a <see cref="ResourceLockGuard"/> that encapsulates
    /// a read or write lock for a given folder as well as read locks
    /// for all its parent folders.
    /// </summary>
    /// <param name="folder">The folder to be locked.</param>
    /// <param name="lockType">Whether a read or a write lock is being
    /// required for the <paramref name="folder"/>.</param>
    /// <returns>A guard which releases folder resource and all folders once it is being disposed.
    /// If locking did not succeed because an element in the chain could not be locked,
    /// the returned <see cref="ResourceLockGuard.IsLockEnabled"/> property
    /// is false.</returns>
    protected virtual ResourceLockGuard RequestChainedLockGuard(IVirtualFolderItem folder, ResourceLockType lockType)
    {
      List<string> parentFolders = GetResourceLockChain(folder);

      switch (lockType)
      {
        case ResourceLockType.Read:
          return LockRepository.GetResourceChainLock(folder.QualifiedIdentifier, false, parentFolders);
        case ResourceLockType.Write:
          return LockRepository.GetResourceChainLock(folder.QualifiedIdentifier, true, parentFolders);
        default:
          throw new ArgumentOutOfRangeException("lockType", "Unknown lock type: " + lockType);
      }
    }


    /// <summary>
    /// Tries to acquire a lock for a given file and its parent folders,
    /// and executes the submitted <paramref name="action"/> if the lock
    /// was granted. Otherwise audits a warning.
    /// </summary>
    /// <param name="file">The file to be locked.</param>
    /// <param name="context">The currently performed file system operation.</param>
    /// <param name="lockType">Whether a read or a write lock is required for the
    /// <paramref name="file"/>.</param>
    /// <param name="action">An action that is being executed if the locking
    /// succeeded.</param>
    /// <returns>True if locking succeeded and the <paramref name="action"/> was invoked. False
    /// if the lock was not granted.</returns>
    protected virtual bool LockResourceAndExecute(IVirtualFileItem file, FileSystemTask context, ResourceLockType lockType, Action action)
    {
      using(var guard = RequestChainedLockGuard(file, lockType))
      {
        if (!guard.IsLockEnabled)
        {
          AuditEvent ae = lockType == ResourceLockType.Read
                            ? AuditEvent.FileReadLockDenied
                            : AuditEvent.FileWriteLockDenied;
          AuditHelper.AuditDeniedOperation(Auditor,context, ae, file);
          return false;
        }
        
        action();
        return true;
      }
    }


    /// <summary>
    /// Tries to acquire a lock for a given folder and its parent folders,
    /// and executes the submitted <paramref name="action"/> if the lock
    /// was granted. Otherwise audits a warning.
    /// </summary>
    /// <param name="folder">The folder to be locked.</param>
    /// <param name="context">The currently performed file system operation.</param>
    /// <param name="lockType">Whether a read or a write lock is required for the
    /// <paramref name="folder"/>.</param>
    /// <param name="action">An action that is being executed if the locking
    /// succeeded.</param>
    /// <returns>True if locking succeeded and the <paramref name="action"/> was invoked. False
    /// if the lock was not granted.</returns>
    protected virtual bool LockResourceAndExecute(IVirtualFolderItem folder, FileSystemTask context, ResourceLockType lockType, Action action)
    {
      using (var guard = RequestChainedLockGuard(folder, lockType))
      {
        if (!guard.IsLockEnabled)
        {
          AuditEvent ae = lockType == ResourceLockType.Read
                            ? AuditEvent.FolderReadLockDenied
                            : AuditEvent.FolderWriteLockDenied;
          AuditHelper.AuditDeniedOperation(Auditor,context, ae, folder);
          return false;
        }
        
        action();
        return true;
      }
    }


    /// <summary>
    /// Gets all resources that need to be read-locked (and thus write-protected)
    /// in order to savely access a given file. With hierarchical file systems,
    /// these are usually the parent folders of the file, because deleting,
    /// moving, or renaming the folder would cause file access issues if the
    /// file is still accessed).
    /// </summary>
    /// <param name="file">The currently processed file.</param>
    /// <returns>All resources that need to be write-protected in order to
    /// process the file.</returns>
    protected abstract List<string> GetResourceLockChain(IVirtualFileItem file);


    /// <summary>
    /// Gets all resources that need to be read-locked (and thus write-protected)
    /// in order to savely access a given folder. With hierarchical folder systems,
    /// these are usually the parent folders of the folder, because deleting,
    /// moving, or renaming the folder would cause folder access issues if the
    /// folder is still accessed).
    /// </summary>
    /// <param name="folder">The currently processed folder.</param>
    /// <returns>All resources that need to be write-protected in order to
    /// process the folder.</returns>
    protected abstract List<string> GetResourceLockChain(IVirtualFolderItem folder);

    #endregion


  }
}