using System;
using Vfs.Transfer;

namespace Vfs.Auditing
{
    /// <summary>
    /// A static helper class that provides functionality to quickly audit
    /// and escalate common file system scenarios.
    /// </summary>
    public static class AuditHelper
    {
        #region transfers

        /// <summary>
        /// Audits a request for an unknown transfer.
        /// </summary>
        public static void AuditUnknownTransferRequest( IAuditor auditor, FileSystemTask context, string transferId) {
            if (!AuditHelper.IsWarnEnabledFor(auditor, context)) return;

            string msg = String.Format("Request for unknown transfer [{0}] received.", transferId);
            auditor.Audit(AuditLevel.Warning, context, AuditEvent.UnknownTransferRequest, msg);
        }

        /// <summary>
        /// Audits an invalid request to pause a transfer although the transfer is not active.
        /// </summary>
        public static void AuditInvalidTransferPauseRequest( IAuditor auditor, ITransfer transfer, FileSystemTask context, AuditEvent eventId) {
            if (!AuditHelper.IsWarnEnabledFor(auditor,context)) return;

            string msg = "Received request to pause transfer [{0}] for resource [{1}], which was denied because the transfer's status is [{2}].";
            msg = String.Format(msg, transfer.Token.TransferId, transfer.Token.ResourceIdentifier, transfer.Status);
            auditor.Audit(AuditLevel.Warning, context, eventId, msg);
        }


        /// <summary>
        /// Audits an invalid request to pause a transfer although the transfer is not active.
        /// </summary>
        public static void AuditChangedTransferStatus( IAuditor auditor, ITransfer transfer, FileSystemTask context, AuditEvent eventId) {
            if (!AuditHelper.IsInfoEnabledFor(auditor,context)) return;

            string msg = "Transfer [{0}] for resource [{1}] changed status to [{2}].";
            msg = String.Format(msg, transfer.Token.TransferId, transfer.Token.ResourceIdentifier, transfer.Status);
            auditor.Audit(AuditLevel.Info, context, eventId, msg);
        }


        /// <summary>
        /// Audits a successful file system operation that occurred in the context of a given transfer.
        /// </summary>
        public static void AuditTransferOperation<T>( IAuditor auditor, FileSystemTask context, AuditEvent eventId, string transferId, IVirtualResourceItem<T> resource) where T : VirtualResourceInfo {
            if (!AuditHelper.IsInfoEnabledFor(auditor,context)) return;

            string msg = "Successfully performed file system operation [{0}] during transfer [{1}] for resource [{2}].\n\n{3}";
            msg = String.Format(msg, context, transferId, resource.ResourceInfo.Name, AuditHelper.CreateResourceInfoString(resource));

            auditor.Audit(AuditLevel.Info, context, eventId, msg);
        }

        #endregion


        /// <summary>
        /// Audits a request for a given file's meta data (e.g. through <see cref="IFileSystemProvider.GetFileInfo"/>.
        /// Be aware that this might cause verbose audit trails.
        /// </summary>
        public static void AuditFileInfoRequest( IAuditor auditor, FileSystemTask context, IVirtualFileItem file) {
            if (!AuditHelper.IsInfoEnabledFor(auditor,context)) return;

            string msg = String.Format("File information requested:\n{0}", AuditHelper.CreateResourceInfoString(file));
            auditor.Audit(AuditLevel.Info, context, AuditEvent.FileInfoRequested, msg);
        }


        /// <summary>
        /// Audits a request for a given folder's meta data (e.g. through <see cref="IFileSystemProvider.GetFolderInfo"/>.
        /// Be aware that this might cause verbose audit trails.
        /// </summary>
        public static void AuditFolderInfoRequest( IAuditor auditor, FileSystemTask context, IVirtualFolderItem folder) {
            if (!AuditHelper.IsInfoEnabledFor(auditor,context)) return;

            string msg = String.Format("Folder information requested:\n{0}", AuditHelper.CreateResourceInfoString(folder));
            auditor.Audit(AuditLevel.Info, context, AuditEvent.FolderInfoRequested, msg);
        }

        /// <summary>
        /// Audits a request for a folder that was not found on the file system.
        /// </summary>
        public static void AuditRequestedFolderNotFound( IAuditor auditor, IVirtualFolderItem folder, FileSystemTask context) {
            if (!AuditHelper.IsWarnEnabledFor(auditor,context)) return;

            string msg = "Could not handle request for folder [{0}] - the folder was not found on the file system.\n\n{1}";
            msg = String.Format(msg, folder.ResourceInfo.Name, AuditHelper.CreateResourceInfoString(folder));
            auditor.Audit(AuditLevel.Warning, context, AuditEvent.FolderNotFound, msg);
        }

        /// <summary>
        /// Audits a request for a file resource that was not found on the file system.
        /// </summary>
        public static void AuditRequestedFileNotFound( IAuditor auditor, IVirtualFileItem file, FileSystemTask task) {
            if (!AuditHelper.IsWarnEnabledFor(auditor,task)) return;

            string msg = "Could not handle request for file [{0}] - the file was not found on the file system.\n\n{1}";
            msg = String.Format(msg, file.ResourceInfo.Name, AuditHelper.CreateResourceInfoString(file));
            auditor.Audit(AuditLevel.Warning, task, AuditEvent.FileNotFound, msg);
        }

        /// <summary>
        /// Audits a request for the file system's root folder.
        /// </summary>
        public static void AuditRootFolderRequest( IAuditor auditor, IVirtualFolderItem rootFolder) {
            if (!AuditHelper.IsInfoEnabledFor(auditor,FileSystemTask.RootFolderInfoRequest)) return;

            const string msg = "File system root requested.";
            auditor.Audit(AuditLevel.Info, FileSystemTask.RootFolderInfoRequest, AuditEvent.FolderInfoRequested, msg);
        }


        /// <summary>
        /// Audits a request for the file system's root folder.
        /// </summary>
        public static void AuditFileParentRequest( IAuditor auditor, IVirtualFileItem childFile, IVirtualFolderItem parentFolder) {
            if (!AuditHelper.IsInfoEnabledFor(auditor,FileSystemTask.FileParentRequest)) return;

            string msg = "Parent folder of file [{0}] was requested.\nSubmitted file:\n{1}\n\nParent folder:\n{2}";
            msg = String.Format(msg, childFile.ResourceInfo.Name, AuditHelper.CreateResourceInfoString(childFile),
                                AuditHelper.CreateResourceInfoString(parentFolder));


            auditor.Audit(AuditLevel.Info, FileSystemTask.FileParentRequest, AuditEvent.FolderInfoRequested, msg);
        }

        /// <summary>
        /// Audits a request for the file system's root folder.
        /// </summary>
        public static void AuditFolderParentRequest( IAuditor auditor, IVirtualFolderItem childFolder, IVirtualFolderItem parentFolder) {
            if (!AuditHelper.IsInfoEnabledFor(auditor,FileSystemTask.FolderParentRequest)) return;

            string msg = "Parent folder of folder [{0}] was requested.\nSubmitted folder:\n{1}\n\nParent folder:\n{2}";
            msg = String.Format(msg, childFolder.ResourceInfo.Name, AuditHelper.CreateResourceInfoString(childFolder),
                                AuditHelper.CreateResourceInfoString(parentFolder));


            auditor.Audit(AuditLevel.Info, FileSystemTask.FolderParentRequest, AuditEvent.FolderInfoRequested, msg);
        }


        /// <summary>
        /// Audits a request for a folder's files and sub folders.
        /// </summary>
        public static void AuditFolderContentsRequest( IAuditor auditor, FileSystemTask context, IVirtualFolderItem parentFolder) {
            if (!AuditHelper.IsInfoEnabledFor(auditor,context)) return;

            string msg = "Delivered contents of folder [{0}]. Folder details:\n\n{1}";
            msg = String.Format(msg, parentFolder.ResourceInfo.Name, AuditHelper.CreateResourceInfoString(parentFolder));

            auditor.Audit(AuditLevel.Info, context, AuditEvent.FolderContentsRequested, msg);
        }


        /// <summary>
        /// Audits a check for file or folder availability along with the returned result.
        /// </summary>
        public static void AuditResourceAvailabilityCheck( IAuditor auditor, FileSystemTask context, string resourcePath, bool isAvailable) {
            if (!AuditHelper.IsInfoEnabledFor(auditor,context)) return;

            string msg = "Checked availability of resource (file or folder) [{0}]. Available: {1}.";
            msg = String.Format(msg, resourcePath, isAvailable);

            auditor.Audit(AuditLevel.Info, context, AuditEvent.FileInfoRequested, msg);
        }



        /// <summary>
        /// Audits a denied request for a folder's contents.
        /// </summary>
        public static void AuditDeniedFolderContentsRequest( IAuditor auditor, FileSystemTask context, IVirtualFolderItem parentFolder) {
            if (!AuditHelper.IsWarnEnabledFor(auditor,context)) return;

            string msg = "Denied access to contents of folder [{0}]. Folder details:\n\n{1}";
            msg = String.Format(msg, parentFolder.ResourceInfo.Name, AuditHelper.CreateResourceInfoString(parentFolder));

            auditor.Audit(AuditLevel.Warning, context, AuditEvent.ListFolderContentsDenied, msg);
        }



        /// <summary>
        /// Audits a request for the file system's root folder.
        /// </summary>
        public static void AuditInvalidRootParentRequest( IAuditor auditor, IVirtualFolderItem childFolder) {
            if (!AuditHelper.IsWarnEnabledFor(auditor,FileSystemTask.FolderParentRequest)) return;

            string msg = "Parent folder request for file system root was blocked. Submitted folder:\n{0}";
            msg = String.Format(msg, AuditHelper.CreateResourceInfoString(childFolder));


            auditor.Audit(AuditLevel.Warning, FileSystemTask.FolderParentRequest, AuditEvent.FolderInfoRequested, msg);
        }


        /// <summary>
        /// Audits an attempt to delete the root folder.
        /// </summary>
        public static void AuditDeleteRootAttempt( IAuditor auditor, IVirtualFolderItem folder) {
            if (!AuditHelper.IsWarnEnabledFor(auditor,FileSystemTask.FolderDeleteRequest)) return;

            string msg = String.Format("Blocked attempt to delete root folder.\n\n{0}", AuditHelper.CreateResourceInfoString(folder));
            auditor.Audit(AuditLevel.Warning, FileSystemTask.FolderDeleteRequest, AuditEvent.DeleteFileSystemRoot, msg);
        }


        /// <summary>
        /// Audits that a folder was created on the file system.
        /// </summary>
        public static void AuditFolderCreation( IAuditor auditor, IVirtualFolderItem folder) {
            const FileSystemTask context = FileSystemTask.FolderCreateRequest;
            if (!AuditHelper.IsInfoEnabledFor(auditor,context)) return;

            string msg = "Created folder [{0}] on the file system.\n\n{1}";
            msg = String.Format(msg, folder.ResourceInfo.Name, AuditHelper.CreateResourceInfoString(folder));

            auditor.Audit(AuditLevel.Info, context, AuditEvent.FolderCreated, msg);
        }


        public static void AuditDeniedFolderCreation( IAuditor auditor, AuditEvent eventId, IVirtualFolderItem folder) {
            if (!AuditHelper.IsWarnEnabledFor(auditor,FileSystemTask.FolderCreateRequest)) return;

            string msg = String.Format("Blocked attempt to create folder on file system. Folder information:\n{0}", AuditHelper.CreateResourceInfoString(folder));
            auditor.Audit(AuditLevel.Warning, FileSystemTask.FolderCreateRequest, eventId, msg);
        }



        /// <summary>
        /// Audits a successful file system operation for a given resource.
        /// </summary>
        public static void AuditResourceOperation<T>( IAuditor auditor, FileSystemTask context, AuditEvent eventId, IVirtualResourceItem<T> resource) where T : VirtualResourceInfo {
            if (!AuditHelper.IsInfoEnabledFor(auditor,context)) return;

            string msg = String.Format("Successfully performed file system operation '{0}' for resource [{1}].\n\n{2}",
                                       context, resource.ResourceInfo.Name, AuditHelper.CreateResourceInfoString(resource));

            auditor.Audit(AuditLevel.Info, context, eventId, msg);
        }


        /// <summary>
        /// Audits a successful file system operation for a given resource.
        /// </summary>
        public static void AuditResourceOperation<T>( IAuditor auditor, FileSystemTask context, AuditEvent eventId, IVirtualResourceItem<T> resource, string message) where T : VirtualResourceInfo {
            if (!AuditHelper.IsInfoEnabledFor(auditor,context)) return;

            string msg = String.Format("Successfully performed file system operation '{0}' for resource [{1}].\n\n{2}",
                                       context, resource.ResourceInfo.Name, AuditHelper.CreateResourceInfoString(resource));

            if (!String.IsNullOrEmpty(message)) {
                msg = String.Format("{0}\n{1}", msg, message);
            }

            auditor.Audit(AuditLevel.Info, context, eventId, msg);
        }


        /// <summary>
        /// Audits a successful file system operation that involves a source and a target resource (move / copy).
        /// </summary>
        public static void AuditResourceOperation<T>( IAuditor auditor, FileSystemTask context, AuditEvent eventId, IVirtualResourceItem<T> sourceFolder, IVirtualResourceItem<T> targetFolder) where T : VirtualResourceInfo {
            if (!AuditHelper.IsInfoEnabledFor(auditor,context)) return;

            string msg = String.Format("Successfully performed file system operation '{0}'.\n\nSource:\n{1}\n\nTarget:\n{2}",
                                       context, AuditHelper.CreateResourceInfoString(sourceFolder), AuditHelper.CreateResourceInfoString(targetFolder));

            auditor.Audit(AuditLevel.Info, context, eventId, msg);
        }



        /// <summary>
        /// Audits a file system operation that was blocked as a warning.
        /// </summary>
        public static void AuditDeniedOperation<T>( IAuditor auditor, FileSystemTask context, AuditEvent eventId, IVirtualResourceItem<T> resource) where T : VirtualResourceInfo {
            AuditDeniedOperation(auditor, context, eventId, resource, null);
        }


        /// <summary>
        /// Audits a file system operation that was blocked as a warning.
        /// </summary>
        public static void AuditDeniedOperation<T>( IAuditor auditor, FileSystemTask context, AuditEvent eventId, IVirtualResourceItem<T> resource, string message) where T : VirtualResourceInfo {
            if (!AuditHelper.IsWarnEnabledFor(auditor,context)) return;

            string msg = String.Format("Blocked operation '{0}' due to event '{1}'.", context, eventId);


            if (!String.IsNullOrEmpty(message)) {
                msg = String.Format("{0}\n{1}", msg, message);
            }

            msg = String.Format("{0}\n\n{1}", msg, AuditHelper.CreateResourceInfoString(resource));
            auditor.Audit(AuditLevel.Warning, context, eventId, msg);
        }


        #region audit exceptions

        /// <summary>
        /// Audits a given exception as an incident of <see cref="AuditLevel.Critical"/>, with an
        /// event ID of <see cref="AuditEvent.Unknown"/>.
        /// </summary>
        public static void AuditException( IAuditor auditor, Exception exception, FileSystemTask context) {
            AuditException(auditor, exception, AuditLevel.Critical, context, AuditEvent.Unknown);
        }

        public static void AuditException( IAuditor auditor, Exception exception, FileSystemTask context, AuditEvent eventId) {
            AuditException(auditor, exception, AuditLevel.Critical, context, eventId);
        }

        public static void AuditException( IAuditor auditor, Exception exception, AuditLevel level, FileSystemTask context, AuditEvent eventId) {
            AuditException(auditor, exception, level, context, eventId, null);
        }

        public static void AuditException( IAuditor auditor, Exception exception, AuditLevel level, FileSystemTask context, AuditEvent eventId, string message) {
            if (!auditor.IsAuditEnabled(level, context)) return;

            VfsException vfsException = exception as VfsException;
            if (vfsException != null) {
                //don't create duplicate or unneccessary entries
                if (vfsException.IsAudited || vfsException.SuppressAuditing) return;
            }

            if (String.IsNullOrEmpty(message)) {
                message = exception.ToString();
            }
            else {
                message = String.Format("{0}\n\n{1}", message, exception);
            }

#if !SILVERLIGHT
            if (exception.StackTrace == null) {
                //add stack trace, if not available yet
                message = String.Format("{0}\n{1}", message, Environment.StackTrace);
            }
#endif

            //submit incident
            auditor.Audit(level, context, eventId, message);

            //set audition flag
            if (vfsException != null) vfsException.IsAudited = true;
        }

        #endregion



        /// <summary>
        /// Creates a simple string that lists the resource's name and path information.
        /// </summary>
        private static string CreateResourceInfoString<T>( IVirtualResourceItem<T> item) where T : VirtualResourceInfo {
            const string msg = "Processed resource: [{0}]\nPublic path: [{1}]\nInternal qualified path: [{2}]";
            return String.Format(msg, item.ResourceInfo.Name, item.ResourceInfo.FullName, item.QualifiedIdentifier);
        }


        #region check if level is enabled

        public static bool IsInfoEnabledFor( IAuditor auditor, FileSystemTask task) {
            return auditor.IsAuditEnabled(AuditLevel.Info, task);
        }

        public static bool IsWarnEnabledFor( IAuditor auditor, FileSystemTask task) {
            return auditor.IsAuditEnabled(AuditLevel.Warning, task);
        }

        public static bool IsCrititcalEnabledFor( IAuditor auditor, FileSystemTask task) {
            return auditor.IsAuditEnabled(AuditLevel.Critical, task);
        }

        #endregion

    }
}
