using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vfs.Auditing;
using Vfs.LocalFileSystem.Transfer;
using Vfs.Transfer;
using Vfs.Util;

namespace Vfs.LocalFileSystem
{
    public class LocalFileSystemProvider : FileSystemProviderBase2//<FileItem, FolderItem>
    {
        protected internal LocalFileSystemConfiguration FileSystemConfiguration { get; private set; }

        /// <summary>
        /// The configured root folder. If this property is set, the
        /// file system will limit file and directory access to the
        /// contents of this directory. If not set (default), data
        /// on all drives can be accessed.  
        /// </summary>
        public DirectoryInfo RootDirectory {
            get { return FileSystemConfiguration.RootDirectory; }
        }

        /// <summary>
        /// The name of the folder that acts as the root of this
        /// virtual file system.
        /// </summary>
        public string RootName {
            get { return FileSystemConfiguration.RootName; }
        }

        /// <summary>
        /// Whether to expose only relative paths if data is limited
        /// to a given <see cref="RootDirectory"/>.
        /// </summary>
        public bool UseRelativePaths {
            get { return FileSystemConfiguration.UseRelativePaths; }
        }


        private LocalDownloadHandler downloadHandler;

        /// <summary>
        /// Manages download requests from the file system.
        /// </summary>
        public override IDownloadTransferHandler DownloadTransfers {
            get { return downloadHandler; }
        }

        private LocalUploadHandler uploadHandler;

        /// <summary>
        /// Manages uploads to the file system.
        /// </summary>
        public override IUploadTransferHandler UploadTransfers {
            get { return uploadHandler; }
        }


        #region construction

        /// <summary>
        /// Creates a file system provider that provides access to the whole
        /// local file system, using the <see cref="Environment.MachineName"/>
        /// as the root folder name.
        /// </summary>
        public LocalFileSystemProvider()
            : this(Environment.MachineName) {
        }

        /// <summary>
        /// Initializes a new instance that provides access to the whole
        /// local file system.
        /// </summary>
        /// <param name="rootName">The name of the root folder.</param>
        public LocalFileSystemProvider(string rootName)
            : this(LocalFileSystemConfiguration.CreateForMachine(rootName)) {
        }


        /// <summary>
        /// Initializes a new instance that provides access to the contents of a given root
        /// directory.
        /// </summary>
        /// <param name="rootDirectory">The root folder which is being managed
        /// by this provider instance.</param>
        /// <param name="useRelativePaths">If true, returned <see cref="VirtualResourceInfo"/>
        /// instances do not provide qualified paths, but virtual paths to the submitted
        /// root directory. This also leverages security in remote access scenarios.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="rootDirectory"/> or
        /// is a null reference.</exception>
        /// <exception cref="DirectoryNotFoundException">If the <paramref name="rootDirectory"/> does
        /// not exist on the local file system.</exception>
        public LocalFileSystemProvider(DirectoryInfo rootDirectory, bool useRelativePaths)
            : this(LocalFileSystemConfiguration.CreateForRootDirectory(rootDirectory, useRelativePaths)) {
        }


        /// <summary>
        /// Initializes a new instance that provides access to the contents
        /// of a given directory.
        /// </summary>
        /// <exception cref="ArgumentNullException">If <paramref name="configuration"/>
        /// is a null reference.</exception>
        public LocalFileSystemProvider(LocalFileSystemConfiguration configuration) {
            Ensure.ArgumentNotNull(configuration, "configuration");
            FileSystemConfiguration = configuration;

            InitTransferServices();
        }
        //add 
        Vfs.LocalFileSystem.FileItem ResolveFileResourcePathInternal2(string virtualFilePath, bool mustExist, FileSystemTask context) {
            return ResolveFileResourcePathInternal(virtualFilePath, mustExist, context) as Vfs.LocalFileSystem.FileItem;
        }
        Vfs.LocalFileSystem.FolderItem ResolveFolderResourcePathInternal2(string virtualFolderPath, bool mustExist, FileSystemTask context) {
            return ResolveFolderResourcePathInternal(virtualFolderPath, mustExist, context) as Vfs.LocalFileSystem.FolderItem;
        }
        //
        /// <summary>
        /// Inits the upload and download transfer services.
        /// </summary>
        protected void InitTransferServices() {
            // ReSharper disable UseObjectOrCollectionInitializer
            var config = new LocalTransferConfig
                // ReSharper restore UseObjectOrCollectionInitializer
                           {
                               Provider = this,
                               FileSystemConfiguration = FileSystemConfiguration,
                               FileResolverFunc = ResolveFileResourcePathInternal2,
                               ClaimsResolverFunc = fi => Security.GetFileClaims(fi),
                               FolderClaimsResolverFunc = fi => Security.GetFolderClaims(fi),
                               LockResolverFunc = (fi, lockType) => RequestChainedLockGuard(fi, lockType)
                           };

            config.FileParentResolverFunc = (fi, context) => {
                //delegate resolving of the parent path
                string parentFolderPath = fi.ResourceInfo.ParentFolderPath;

                //get the parent info
                return ResolveFolderResourcePathInternal2(parentFolderPath, true, context);
            };

            //init stores
            downloadHandler = new LocalDownloadHandler(FileSystemConfiguration.DownloadStore, config);
            uploadHandler = new LocalUploadHandler(FileSystemConfiguration.UploadStore, config);
        }

        #endregion


        /// <summary>
        /// A method that is invoked on pretty much every file request in order
        /// to resolve a submitted file path into a <see cref="VirtualFileInfo"/>
        /// object.<br/>
        /// The <see cref="VirtualFileInfo"/> is being returned as part of a
        /// <see cref="VirtualFileItem"/>, which should also provide some additionally
        /// required meta data which is used for further validation and auditing.
        /// </summary>
        /// <param name="submittedFilePath">The path that was received as a part of a file-related
        /// request.</param>
        /// <param name="context">The currently performed file system operation.</param>
        /// <returns>A <see cref="VirtualFileItem"/> which encapsulates a <see cref="VirtualFileInfo"/>
        /// that represents the requested file on the file system.</returns>
        /// <exception cref="InvalidResourcePathException">In case the format of the submitted path
        /// is invalid, meaning it cannot be interpreted as a valid resource identifier.</exception>
        /// <exception cref="VfsException">Exceptions will be handled by this base class and audited to
        /// the <see cref="FileSystemProviderBase.Auditor"/>. If auditing was already performed or should
        /// be suppressed, implementors can set the <see cref="VfsException.IsAudited"/> and
        /// <see cref="VfsException.SuppressAuditing"/> properties.</exception>
        /// <exception cref="Exception">Any exceptions that are not derived from
        /// <see cref="VfsException"/> will be wrapped and audited.</exception>
        public override IVirtualFileItem ResolveFileResourcePath(string submittedFilePath, FileSystemTask context) {
            return ResolveFileResourcePath2(submittedFilePath, context);
        }        
        public  FileItem ResolveFileResourcePath2(string submittedFilePath, FileSystemTask context) {
            if (String.IsNullOrEmpty(submittedFilePath)) {
                throw new InvalidResourcePathException("An empty or null string is not a valid file path");
            }

            //make sure we operate on absolute paths
            var absolutePath = PathUtil.GetAbsolutePath(submittedFilePath, RootDirectory);

            if (IsRootPath(absolutePath)) {
                throw new InvalidResourcePathException("Invalid path submitted: " + submittedFilePath);
            }

            var localFile = new FileInfo(absolutePath);
            VirtualFileInfo virtualFile = localFile.CreateFileResourceInfo();

            var item = new FileItem(localFile, virtualFile);

            //convert to relative paths if required (also prevents qualified paths in validation exceptions)
            if (UseRelativePaths) item.MakePathsRelativeTo(RootDirectory);

            ValidateFileRequestAccess(item, submittedFilePath, context);
            return item;
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
        public override IVirtualFolderItem ResolveFolderResourcePath(string submittedFolderPath, FileSystemTask context) {
            return ResolveFolderResourcePath2(submittedFolderPath,context);
        }
        public  FolderItem ResolveFolderResourcePath2(string submittedFolderPath, FileSystemTask context) {
            //make sure we operate on absolute paths
            string absolutePath = PathUtil.GetAbsolutePath(submittedFolderPath ?? "", RootDirectory);

            if (IsRootPath(absolutePath)) {
                return GetFileSystemRootImplementation() as FolderItem;
            }

            var di = new DirectoryInfo(absolutePath);
            VirtualFolderInfo folderInfo = di.CreateFolderResourceInfo();

            var item = new FolderItem(di, folderInfo);

            //convert to relative paths if required (also prevents qualified paths in validation exceptions)
            if (UseRelativePaths) item.MakePathsRelativeTo(RootDirectory);

            ValidateFolderRequestAccess(item, submittedFolderPath, context);
            return item;
        }


        /// <summary>
        /// Validates whether a  <see cref="LocalFileSystemProvider"/> was configured
        /// with access restricted to a given <see cref="LocalFileSystemProvider.RootDirectory"/>,
        /// and makes sure that the requested <paramref name="folder"/> is indeed contained
        /// within that folder.
        /// </summary>
        /// <param name="folder">The requested folder resource.</param>
        /// <param name="submittedFolderPath">The path that was submitted in the original request.</param>
        /// <param name="context">The currently performed file system operation.</param>
        /// <exception cref="ResourceAccessException">If the requested resource is not
        /// a descendant of a configured <see cref="LocalFileSystemProvider.RootDirectory"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="folder"/>
        /// is a null reference.</exception>
        private void ValidateFolderRequestAccess(FolderItem folder, string submittedFolderPath, FileSystemTask context) {
            if (folder == null) throw new ArgumentNullException("folder");

            //if there isn't a restricted custom root, every resource can be accessed
            //(if the path is invalid, this will fail otherwise, depending on the action)
            if (RootDirectory == null) return;

            try {
                //if we are dealing with root, we're already done
                if (folder.ResourceInfo.IsRootFolder) return;

                //if we have a custom root, make sure the resource is indeed a descendant of the root
                if (RootDirectory.IsParentOf(folder.LocalDirectory.FullName)) return;
            }
            catch (ResourceAccessException e) {
                //just bubble a resource access exception
                if (e.Resource == null) e.Resource = folder.ResourceInfo;
                throw;
            }
            catch (Exception e) {
                //exceptions can happen in case of invalid folder paths

                //log detailed info
                string error =
                  "Resource request for folder [{0}] caused exception when validating against root directory [{1}].";
                error = String.Format(error, submittedFolderPath, RootDirectory.FullName);
                AuditHelper.AuditException(Auditor, e, AuditLevel.Warning, context, AuditEvent.InvalidFolderPathFormat, error);

                //do not expose too much path information (e.g. absolute paths if disabled)
                error = String.Format("Invalid folder path:  [{0}].", submittedFolderPath);
                throw new ResourceAccessException(error, e) { Resource = folder.ResourceInfo, IsAudited = true };
            }

            //if none of the above is true, the request is invalid

            //log detailed info
            string msg = "Resource request for folder [{0}] was blocked. The resource is outside the root directory [{1}].";
            msg = String.Format(msg, folder.ResourceInfo.FullName, RootDirectory.FullName);
            Auditor.Audit(AuditLevel.Warning, context, AuditEvent.InvalidResourceLocationRequested, msg);

            //do not expose too much path information (e.g. absolute paths if disabled)
            msg = String.Format("Invalid folder path: [{0}].", submittedFolderPath);
            throw new ResourceAccessException(msg) { Resource = folder.ResourceInfo, IsAudited = true };
        }


        /// <summary>
        /// Validates whether a  <see cref="LocalFileSystemProvider"/> was configured
        /// with access restricted to a given <see cref="LocalFileSystemProvider.RootDirectory"/>,
        /// and makes sure that the requested <paramref name="file"/> is indeed contained
        /// within that folder.
        /// </summary>
        /// <param name="file">The requested file resource.</param>
        /// <param name="submittedFilePath">The path that was submitted in the original request.</param>
        /// <param name="context">The currently performed file system operation.</param>
        /// <exception cref="ResourceAccessException">If the requested resource is not
        /// a descendant of a configured <see cref="LocalFileSystemProvider.RootDirectory"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="file"/>
        /// is a null reference.</exception>
        private void ValidateFileRequestAccess(FileItem file, string submittedFilePath, FileSystemTask context) {
            if (file == null) throw new ArgumentNullException("file");

            //if there isn't a restricted custom root, every file resource can be accessed
            //(if the path is invalid, this will fail otherwise, depending on the action)
            if (RootDirectory == null) return;

            try {
                //if we have a custom root, make sure the resource is indeed a descendant of the root
                if (RootDirectory.IsParentOf(file.LocalFile.FullName)) return;
            }
            catch (ResourceAccessException e) {
                //just bubble a resource access exception
                if (e.Resource == null) e.Resource = file.ResourceInfo;
                throw;
            }
            catch (Exception e) {
                //exceptions can happen in case of invalid file paths

                //log detailed info
                string error = "Resource request for file [{0}] caused exception when validating against root directory [{1}].";
                error = String.Format(error, submittedFilePath, RootDirectory.FullName);
                AuditHelper.AuditException(Auditor, e, AuditLevel.Warning, context, AuditEvent.InvalidFilePathFormat, error);

                //do not expose too much path information (e.g. absolute paths if disabled)
                error = String.Format("Invalid file path: [{0}].", submittedFilePath);
                throw new ResourceAccessException(error, e) { Resource = file.ResourceInfo, IsAudited = true };
            }

            //if none of the above is true, the request is invalid

            //log detailed info
            string msg = "Resource request for file [{0}] was blocked. The resource is outside the root directory [{1}].";
            msg = String.Format(msg, file.ResourceInfo.FullName, RootDirectory.FullName);
            Auditor.Audit(AuditLevel.Warning, context, AuditEvent.InvalidResourceLocationRequested, msg);

            //do not expose too much path information (e.g. absolute paths if disabled)
            msg = String.Format("Invalid file path: [{0}].", submittedFilePath);
            throw new ResourceAccessException(msg) { Resource = file.ResourceInfo, IsAudited = true };
        }


        /// <summary>
        /// Internal implementation of the <see cref="FileSystemProviderBase2{TFile,TFolder}.GetFileSystemRoot"/>
        /// method, which is invoked by the base class. The base takes
        /// care of auditing and exception handling, so this implementing
        /// method should focus on item creation and custom validation.<br/>
        /// </summary>
        /// <returns>A <see cref="IVirtualFolderItem"/> which encapsulates
        /// a <see cref="VirtualFolderInfo"/> that represents the file
        /// system's root folder.</returns>
        protected override IVirtualFolderItem GetFileSystemRootImplementation() {
            return GetFileSystemRootImplementation2();
        }
        protected  FolderItem GetFileSystemRootImplementation2() {
            VirtualFolderInfo rootInfo;
            if (RootDirectory == null) {
                //create artificial one
                rootInfo = new VirtualFolderInfo { FullName = String.Empty, IsEmpty = false };
            }
            else {
                //create from root directory
                rootInfo = RootDirectory.CreateFolderResourceInfo();
                if (UseRelativePaths) {
                    rootInfo.FullName = PathUtil.RelativeRootPrefix;
                    rootInfo.ParentFolderPath = null;
                }
            }

            rootInfo.Name = RootName;
            rootInfo.IsRootFolder = true;
            return new FolderItem(RootDirectory, rootInfo);
        }


        /// <summary>
        /// Resolves all child folders of a given parent folder. This method is invoked
        /// by the <see cref="FileSystemProviderBase2{TFile,TFolder}.GetChildFolders(string)"/> method in order to resolve the
        /// qualified paths of all child folders within that folder, which can be used in order to
        /// create <see cref="VirtualFolderInfo"/> instances based on the returned paths.
        /// </summary>
        /// <param name="parentFolder">The currently processed folder.</param>
        /// <returns>Folder paths that can be resolved to the folders withing the submitted
        /// <paramref name="parentFolder"/>.</returns>
        protected override IEnumerable<string> GetChildFolderPathsInternal(IVirtualFolderItem parentFolder) {
           return GetChildFolderPathsInternal2(parentFolder as FolderItem);
        }
        protected  IEnumerable<string> GetChildFolderPathsInternal2(FolderItem parentFolder) {
            //the second tests are redundant, but they should be given. otherwise let the routine cause
            //an exception
            if (parentFolder.LocalDirectory == null && parentFolder.ResourceInfo.IsRootFolder && RootDirectory == null) {
                //get drives
                return GetDriveFolders();
            }

            //LocalDirectory should *not* be null - this is only allowed for system roots
            var directory = parentFolder.LocalDirectory;

            if (directory == null) {
                string msg =
                  "The LocalDirectory property of folder [{0}] is not set although it's not the system root. Cannot resolve child folders.";
                msg = String.Format(msg, parentFolder.ResourceInfo.FullName);
                Auditor.Audit(AuditLevel.Critical, FileSystemTask.ChildFoldersRequest, AuditEvent.InternalError, msg);

                msg = "Cannot resolve child folders of parent folder [{0}].";
                msg = String.Format(msg, parentFolder.ResourceInfo.FullName);
                throw new ResourceAccessException(msg) { IsAudited = true };
            }

            return directory.GetDirectories().Select(di => di.FullName);
        }


        /// <summary>
        /// Gets the top level folders of the file system, which represent the logical
        /// drives of the current system.
        /// </summary>
        /// <returns>A collection of folders that build the root folders
        /// of the file system.</returns>
        private static IEnumerable<string> GetDriveFolders() {
            return from driveName in Environment.GetLogicalDrives()
                   where Directory.Exists(driveName)
                   //skip inexisting drives
                   select driveName;
        }


        /// <summary>
        /// Resolves all child files of a given parent folder. This method is invoked
        /// by the <see cref="FileSystemProviderBase2{TFile,TFolder}.GetChildFiles(string)"/> method in order to resolve the
        /// qualified paths of all files within that folder, which can be used in order to
        /// create <see cref="VirtualFileInfo"/> instances based on the returned paths.
        /// </summary>
        /// <param name="parentFolder">The currently processed folder.</param>
        /// <returns>File paths that can be resolved to the files withing the submitted
        /// <paramref name="parentFolder"/>.</returns>
        protected override IEnumerable<string> GetChildFilePathsInternal(IVirtualFolderItem parentFolder) {
            return GetChildFilePathsInternal2(parentFolder as FolderItem);
        }
        protected  IEnumerable<string> GetChildFilePathsInternal2(FolderItem parentFolder) {
            //if we're dealing with the system root, return an empty array - the machine only
            //exposes the drives as the root's folders
            if (RootDirectory == null && parentFolder.ResourceInfo.IsRootFolder) {
                return new string[] { };
            }

            return parentFolder.LocalDirectory.GetFiles().Select(fi => fi.FullName);
        }

        /// <summary>
        /// Checks whether a given file exists on the file system. This method is
        /// being invoked by <see cref="FileSystemProviderBase2{TFile,TFolder}.IsFileAvailable"/>.
        /// </summary>
        /// <param name="virtualFilePath">The received file path to be processed.</param>
        /// <returns>True if the file exists, otherwise false.</returns>
        protected override bool IsFileAvailableInternal(string virtualFilePath) {
            string path = PathUtil.GetAbsolutePath(virtualFilePath ?? "", RootDirectory);
            return File.Exists(path);
        }

        /// <summary>
        /// Checks whether a given folder exists on the file system. This method is
        /// being invoked by <see cref="FileSystemProviderBase2{TFile,TFolder}.IsFolderAvailable"/>.
        /// </summary>
        /// <param name="virtualFolderPath">The received folder path to be processed.</param>
        /// <returns>True if the folder exists, otherwise false.</returns>
        protected override bool IsFolderAvailableInternal(string virtualFolderPath) {
            string path = PathUtil.GetAbsolutePath(virtualFolderPath ?? "", RootDirectory);
            return Directory.Exists(path);
        }

        /// <summary>
        /// This method is invoked by <see cref="FileSystemProviderBase2{TFile,TFolder}.CreateFolder(string)"/> in order
        /// to handle the creation of a physical folder on the file system.
        /// </summary>
        /// <param name="folder">Describes the (currently unavailable folder).</param>
        /// <returns>Either the updated <paramref name="folder"/> reference that
        /// was submitted, or a new <see cref="FolderItem"/> that represents the
        /// created folder.</returns>
        protected override IVirtualFolderItem CreateFolderOnFileSystem(IVirtualFolderItem folder) {
            return CreateFolderOnFileSystem2(folder as FolderItem);
        }
        protected  FolderItem CreateFolderOnFileSystem2(FolderItem folder) {
            //if a file with the same path already exists, quite here
            if (File.Exists(folder.LocalDirectory.FullName)) {
                string pattern = "Cannot create folder at location [{0}] - a file with the same name exists in the folder.";
                string msg = String.Format(pattern, folder.LocalDirectory.FullName);
                AuditHelper.AuditDeniedOperation(Auditor, FileSystemTask.FolderCreateRequest, AuditEvent.CreateFolderDenied, folder, msg);

                msg = String.Format(pattern, folder.ResourceInfo.FullName);
                throw new ResourceAccessException(msg) { IsAudited = true };
            }

            folder.LocalDirectory.Create();
            folder.LocalDirectory.Refresh();
            return folder;
        }


        /// <summary>
        /// Physically deletes a given folder on the file system. This method is
        /// being invoked by the <see cref="FileSystemProviderBase2{TFile,TFolder}.DeleteFolder"/> method.
        /// </summary>
        /// <param name="folder">The folder to be deleted.</param>
        protected override void DeleteFolderOnFileSystem(IVirtualFolderItem folder) {
            DeleteFolderOnFileSystem2(folder as FolderItem);
        }
        protected  void DeleteFolderOnFileSystem2(FolderItem folder) {
            //if the directory is infact a local drive, deny it
            DirectoryInfo dir = folder.LocalDirectory;
            if (dir.Parent == null) {
                string msg = String.Format("Denied deletion of drive [{0}]", dir.Name);
                AuditHelper.AuditDeniedOperation(Auditor, FileSystemTask.FolderDeleteRequest, AuditEvent.DeleteFolderDenied, folder, msg);

                msg = String.Format("Deleting drive [{0}] denied.", folder.ResourceInfo.FullName);
                throw new ResourceAccessException(msg) { IsAudited = true };
            }

            dir.Delete(true);
            dir.Refresh();
        }

        /// <summary>
        /// Physically deletes a given file on the file system. This method is
        /// being invoked by the <see cref="FileSystemProviderBase2{TFile,TFolder}.DeleteFile"/> method.
        /// </summary>
        /// <param name="file">The file to be deleted.</param>
        protected override  void DeleteFileOnFileSystem(IVirtualFileItem file) {
            DeleteFileOnFileSystem2(file as FileItem);
        }
        protected  void DeleteFileOnFileSystem2(FileItem file) {
            file.LocalFile.Delete();
            file.LocalFile.Refresh();
        }

        /// <summary>
        /// Moves a physical folder on the file system from one location to the other. This
        /// method is invoked by <see cref="FileSystemProviderBase2{TFile,TFolder}.MoveFolder(string,string)"/>.
        /// </summary>
        /// <param name="sourceFolder">The folder to be copied.</param>
        /// <param name="targetFolder">The designated new location of the folder.</param>
        /// <returns>Either the updated <paramref name="targetFolder"/> instance, or a new
        /// <see cref="FolderItem"/> instance that represents the created target folder.</returns>
        protected override void MoveFolderOnFileSystem(IVirtualFolderItem sourceFolder, IVirtualFolderItem targetFolder) {
            MoveFolderOnFileSystem2(sourceFolder as FolderItem,targetFolder as FolderItem);
        }
        protected  void MoveFolderOnFileSystem2(FolderItem sourceFolder, FolderItem targetFolder) {
            sourceFolder.LocalDirectory.MoveTo(targetFolder.LocalDirectory.FullName);
            targetFolder.LocalDirectory.Refresh();
        }

        /// <summary>
        /// Copies a physical folder on the file system from one location to the other. This
        /// method is invoked by <see cref="FileSystemProviderBase2{TFile,TFolder}.CopyFolder(string,string)"/>.
        /// </summary>
        /// <param name="sourceFolder">The folder to be copied.</param>
        /// <param name="targetFolder">The designated location of the copy.</param>
        protected override void CopyFolderOnFileSystem(IVirtualFolderItem sourceFolder, IVirtualFolderItem targetFolder) {
            CopyFolderOnFileSystem2(sourceFolder as FolderItem, targetFolder as FolderItem);
        }
        protected  void CopyFolderOnFileSystem2(FolderItem sourceFolder, FolderItem targetFolder) {
            PathUtil.CopyDirectory(sourceFolder.LocalDirectory.FullName, targetFolder.LocalDirectory.FullName, false);
            targetFolder.LocalDirectory.Refresh();
        }

        /// <summary>
        /// Moves a physical file on the file system from one location to the other. This
        /// method is invoked by <see cref="FileSystemProviderBase2{TFile,TFolder}.MoveFile(string,string)"/>.
        /// </summary>
        /// <param name="sourceFile">The file to be moved.</param>
        /// <param name="targetFile">The designated new location of the file.</param>
        /// <returns>Either the updated <paramref name="targetFile"/> instance, or a new
        /// <see cref="FileItem"/> instance that represents the created target file.</returns>
        protected override void MoveFileOnFileSystem(IVirtualFileItem sourceFile, IVirtualFileItem targetFile) {
            MoveFileOnFileSystem2(sourceFile as FileItem,targetFile as FileItem);
        }
        protected  void MoveFileOnFileSystem2(FileItem sourceFile, FileItem targetFile) {
            sourceFile.LocalFile.MoveTo(targetFile.LocalFile.FullName);
            targetFile.LocalFile.Refresh();
        }

        /// <summary>
        /// Copies a physical file on the file system from one location to the other. This
        /// method is invoked by <see cref="FileSystemProviderBase2{TFile,TFolder}.CopyFile(string,string)"/>.
        /// </summary>
        /// <param name="sourceFile">The file to be copied.</param>
        /// <param name="targetFile">The designated location of the file copy.</param>
        /// <returns>Either the updated <paramref name="targetFile"/> instance, or a new
        /// <see cref="FileItem"/> instance that represents the created target file.</returns>
        protected override void CopyFileOnFileSystem(IVirtualFileItem sourceFile, IVirtualFileItem targetFile) {
            CopyFileOnFileSystem2(sourceFile as FileItem,targetFile as FileItem);
        }
        protected  void CopyFileOnFileSystem2(FileItem sourceFile, FileItem targetFile) {
            sourceFile.LocalFile.CopyTo(targetFile.LocalFile.FullName, true);
            targetFile.LocalFile.Refresh();
        }

        /// <summary>
        /// Creates a stream to read the data of a given file from the file system.
        /// This method is invoked by <see cref="FileSystemProviderBase2{TFile,TFolder}.ReadFileContents(string)"/> after having
        /// performed access checks.
        /// </summary>
        /// <param name="fileItem">Represents the file to be read.</param>
        /// <returns>A stream that provides the file's binary data.</returns>
        protected override Stream OpenFileStreamFromFileSystem(IVirtualFileItem fileItem) {
            return OpenFileStreamFromFileSystem2(fileItem as FileItem);
        }
        protected  Stream OpenFileStreamFromFileSystem2(FileItem fileItem) {
            return fileItem.LocalFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        /// <summary>
        /// Creates a stream to write the data of a given file to the file system.
        /// This method is invoked by <see cref="FileSystemProviderBase2{TFile,TFolder}.WriteFile(string,System.IO.Stream,bool, long, string)"/> after having
        /// performed access checks.
        /// </summary>
        /// <param name="fileItem">Represents the file to be created or updated.</param>
        /// <param name="input">A stream that provides the file's contents.</param>
        protected override void WriteFileStreamToFileSystem(IVirtualFileItem fileItem, Stream input) {
            WriteFileStreamToFileSystem2(fileItem as FileItem, input);
        }
        protected  void WriteFileStreamToFileSystem2(FileItem fileItem, Stream input) {
            input.WriteTo(fileItem.LocalFile.FullName);
        }

        /// <summary>
        /// Checks whether a given path (whether null, relative, or absolute)
        /// will be resolved as the root path.
        /// </summary>
        /// <param name="path">The path to be inspected.</param>
        /// <returns>True if the folder path resolves to the system root.</returns>
        private bool IsRootPath(string path) {
            if (RootDirectory == null) {
                return String.IsNullOrEmpty(path);
            }

            path = PathUtil.GetAbsolutePath(path, RootDirectory);
            return String.Equals(path, RootDirectory.FullName, StringComparison.InvariantCultureIgnoreCase);
        }


        /// <summary>
        /// Combines two virtual paths to a string that can be interpreted by the provider.
        /// This implementing method is invoked by <see cref="FileSystemProviderBase2{TFile,TFolder}.CreateFilePath"/>.
        /// </summary>
        /// <param name="parentFolder">The qualified name of the parent
        /// folder.</param>
        /// <param name="fileName">The name of a file within the folder.</param>
        /// <returns>An qualified path name for the submitted
        /// <paramref name="fileName"/>.</returns>
        protected override string CreateFileSystemFilePath(string parentFolder, string fileName) {
            //do not apply any custom logic to the submitted paths in order to
            //prevent probing. Just combine and return it - invalid paths will fail
            //once they are used

            if (parentFolder == null) parentFolder = String.Empty;
            if (fileName == null) fileName = String.Empty;

            return Path.Combine(parentFolder, fileName);
        }


        /// <summary>
        /// Creates a qualified name that can be used as an identifier
        /// for a given folder of the file system.<br/>
        /// This implementing method is invoked by <see cref="FileSystemProviderBase2{TFile,TFolder}.CreateFolderPath"/>.
        /// </summary>
        /// <param name="parentFolder">The qualified name of the parent
        /// folder.</param>
        /// <param name="folderName">The name of the child folder.</param>
        /// <returns>An qualified path name for the submitted
        /// <paramref name="folderName"/>.</returns>
        protected override string CreateFileSystemFolderPath(string parentFolder, string folderName) {
            //do not apply any custom logic to the submitted paths in order to
            //prevent probing. Just combine and return it - invalid paths will fail
            //once they are used

            if (parentFolder == null) parentFolder = String.Empty;
            if (folderName == null) folderName = String.Empty;

            return Path.Combine(parentFolder, folderName);
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
        protected override List<string> GetResourceLockChain(IVirtualFileItem file) {
            return GetResourceLockChain2(file as FileItem );
        }
        protected  List<string> GetResourceLockChain2(FileItem file) {
            List<string> folders = new List<string>();
            DirectoryInfo parent = file.LocalFile.Directory;

            //go up to the root, but don't include it
            while (parent != null && !IsRootPath(parent.FullName)) {
                folders.Add(parent.FullName.ToLowerInvariant());
                parent = parent.Parent;
            }

            return folders;
        }

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
         protected override List<string> GetResourceLockChain(IVirtualFolderItem folder) {
             return GetResourceLockChain2(folder as FolderItem);
        }
        protected  List<string> GetResourceLockChain2(FolderItem folder) {
            List<string> folders = new List<string>();
            DirectoryInfo parent = folder.LocalDirectory.Parent;

            //go up to the root, but don't include it (root cannot be altered)
            while (parent != null && !IsRootPath(parent.FullName)) {
                folders.Add(parent.FullName.ToLowerInvariant());
                parent = parent.Parent;
            }

            return folders;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="virFolderPath">相对root路径或者绝对路径</param>
        /// <param name="relativeRoot">默认true</param>
        /// <returns></returns>
        public bool ExistFolder(string virFolderPath, bool relativeRoot) {
            if (this.RootDirectory != null) {               
                return Directory.Exists(Path.Combine(this.RootDirectory.FullName,virFolderPath));
            }
            return Directory.Exists(virFolderPath);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="virFilePath">相对root路径或者绝对路径</param>
        /// <param name="relativeRoot">默认true</param>
        /// <returns></returns>
        public bool ExistFile(string virFilePath, bool relativeRoot) {
            if (this.RootDirectory != null) {
                return File.Exists(Path.Combine(this.RootDirectory.FullName, virFilePath));
            }
            return File.Exists(virFilePath);
        }
    }
}