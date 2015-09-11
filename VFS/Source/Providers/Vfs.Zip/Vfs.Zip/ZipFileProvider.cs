using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Zip;
using Vfs.Auditing;
using Vfs.Transfer;
using Vfs.Transfer.Util;
using Vfs.Util;
using Vfs.Util.TemporaryStorage;
using Vfs.Zip.Transfer;

namespace Vfs.Zip
{
  public class ZipFileProvider : FileSystemProviderBase2//<ZipFileItem, ZipFolderItem>
  {
    protected ZipFileSystemConfiguration Configuration { get; private set; }
    public ZipNodeRepository NodeRepository { get; private set; }
    public ZipDownloadHandler DownloadHandler { get; protected set; }
    public ZipUploadHandler UploadHandler { get; protected set; }

    /// <summary>
    /// Creates the provider with the ZIP file to be processed.
    /// </summary>
    public ZipFileProvider(ZipFileSystemConfiguration configuration)
    {
      Ensure.ArgumentNotNull(configuration, "configuration");
      Configuration = configuration;

      //init the repository
      NodeRepository = new ZipNodeRepository(configuration);

      InitTransferServices();
    }
    //add 
    Vfs.Zip.ZipFileItem ResolveFileResourcePathInternal2(string virtualFilePath, bool mustExist, FileSystemTask context) {
        return ResolveFileResourcePathInternal(virtualFilePath, mustExist, context) as Vfs.Zip.ZipFileItem;
    }
    Vfs.Zip.ZipFolderItem ResolveFolderResourcePathInternal2(string virtualFolderPath, bool mustExist, FileSystemTask context) {
        return ResolveFolderResourcePathInternal(virtualFolderPath, mustExist, context) as Vfs.Zip.ZipFolderItem;
    }
    //
    /// <summary>
    /// Inits the upload and download transfer services.
    /// </summary>
    protected void InitTransferServices()
    {
      // ReSharper disable UseObjectOrCollectionInitializer
      var config = new ZipTransferConfig
      // ReSharper restore UseObjectOrCollectionInitializer
      {
        Provider = this,
        FileSystemConfiguration = Configuration,
        FileResolverFunc = ResolveFileResourcePathInternal2,
        ClaimsResolverFunc = fi => Security.GetFileClaims(fi),
        FolderClaimsResolverFunc = fi => Security.GetFolderClaims(fi),
        LockResolverFunc = (fi, lockType) => RequestChainedLockGuard(fi, lockType),
        Repository = NodeRepository
      };

      config.FileParentResolverFunc = (fi, context) =>
      {
        //delegate resolving of the parent path
        string parentFolderPath = fi.ResourceInfo.ParentFolderPath;

        //get the parent info
        return ResolveFolderResourcePathInternal2(parentFolderPath, true, context);
      };

      //init stores
      DownloadHandler = new ZipDownloadHandler(config, Configuration.DownloadStore);
      UploadHandler = new ZipUploadHandler(config, Configuration.UploadStore);
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
    protected  ZipFolderItem GetFileSystemRootImplementation2()
    {
      //delegate creation to the repository
      return NodeRepository.CreateRootItem();   
    }

    /// <summary>
    /// Manages download requests from the file system.
    /// </summary>
    public override IDownloadTransferHandler DownloadTransfers 
    {
      get { return DownloadHandler; }
    }

    /// <summary>
    /// Manages uploads to the file system.
    /// </summary>
    public override IUploadTransferHandler UploadTransfers
    {
      get { return UploadHandler; }
    }


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
    public override IVirtualFileItem ResolveFileResourcePath(string submittedFilePath, FileSystemTask context) {
        return ResolveFileResourcePath2(submittedFilePath,context);
    }
    public  ZipFileItem ResolveFileResourcePath2(string submittedFilePath, FileSystemTask context)
    {
      return NodeRepository.GetFileItem(submittedFilePath);
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
        return ResolveFolderResourcePath2(submittedFolderPath, context);
    }
    public  ZipFolderItem ResolveFolderResourcePath2(string submittedFolderPath, FileSystemTask context)
    {
      if(String.IsNullOrEmpty(submittedFolderPath) || submittedFolderPath == ZipNodeRepository.RootFolderPath)
      {
        return GetFileSystemRootImplementation() as ZipFolderItem;
      }

      return NodeRepository.GetFolderItem(submittedFolderPath);
    }


    protected override IEnumerable<string> GetChildFolderPathsInternal(IVirtualFolderItem parentFolder) {
        return GetChildFolderPathsInternal2(parentFolder as ZipFolderItem);
    }
    protected  IEnumerable<string> GetChildFolderPathsInternal2(ZipFolderItem parentFolder)
    {
      return parentFolder.Node.ChildDirectories.Select(cd => cd.FullName);
    }
    protected override IEnumerable<string> GetChildFilePathsInternal(IVirtualFolderItem parentFolder) {
        return GetChildFilePathsInternal2(parentFolder as ZipFolderItem);
    }
    protected  IEnumerable<string> GetChildFilePathsInternal2(ZipFolderItem parentFolder)
    {
      return parentFolder.Node.ChildFiles.Select(cf => cf.FullName);
    }

    protected override bool IsFileAvailableInternal(string virtualFilePath)
    {
      var node = NodeRepository.TryFindNode(virtualFilePath);
      return node != null && !node.IsDirectoryNode;
    }

    protected override bool IsFolderAvailableInternal(string virtualFolderPath)
    {
      var node = NodeRepository.TryFindNode(virtualFolderPath);
      return node != null && node.IsDirectoryNode;
    }

    protected override IVirtualFolderItem CreateFolderOnFileSystem(IVirtualFolderItem folder) {
        return CreateFolderOnFileSystem2(folder as ZipFolderItem);
    }
    protected  ZipFolderItem CreateFolderOnFileSystem2(ZipFolderItem folder)
    {
      Action action = () => NodeRepository.ZipFile.AddDirectoryByName(folder.QualifiedIdentifier);
      NodeRepository.PerformWriteAction(action);

      return NodeRepository.GetFolderItem(folder.QualifiedIdentifier);
    }
    protected override void DeleteFolderOnFileSystem(IVirtualFolderItem folder) {
         DeleteFolderOnFileSystem2(folder as ZipFolderItem);
    }
    protected  void DeleteFolderOnFileSystem2(ZipFolderItem folder)
    {
      Action action = () =>
                        {
                          var entriesToRemove = folder.Node.GetDescendants(true).Select(n => n.FileEntry).ToArray();
                          NodeRepository.ZipFile.RemoveEntries(entriesToRemove);
                        };

      NodeRepository.PerformWriteAction(action);
    }
    protected override void DeleteFileOnFileSystem(IVirtualFileItem file) {
        DeleteFileOnFileSystem2(file as ZipFileItem);
    }
    protected  void DeleteFileOnFileSystem2(ZipFileItem file)
    {
      Action action = () => NodeRepository.ZipFile.RemoveEntry(file.Node.FileEntry);
      NodeRepository.PerformWriteAction(action);
    }
    protected override void MoveFolderOnFileSystem(IVirtualFolderItem sourceFolder, IVirtualFolderItem targetFolder) {
        MoveFolderOnFileSystem2(sourceFolder as ZipFolderItem,targetFolder as ZipFolderItem);
    }
    protected  void MoveFolderOnFileSystem2(ZipFolderItem sourceFolder, ZipFolderItem targetFolder)
    {
      //rewrite path information on all items (just replace the path part of the folder)
      Action action = () =>
                        {
                          var nodes = sourceFolder.Node.GetDescendants(true);
                          foreach (var node in nodes)
                          {
                            var entry = node.FileEntry;

                            //use the ReplaceFirst extension in case we have path duplicates
                            //(e.g. data/files/x/data/files/
                            entry.FileName = entry.FileName.ReplaceFirst(sourceFolder.QualifiedIdentifier,
                                                                    targetFolder.QualifiedIdentifier, StringComparison.InvariantCultureIgnoreCase);

                          }
                        };

      NodeRepository.PerformWriteAction(action);
    }

    /// <summary>
    /// Copies a physical folder on the file system from one location to the other. This
    /// method is invoked by <see cref="FileSystemProviderBase2{TFile,TFolder}.CopyFolder(string,string)"/>.
    /// </summary>
    /// <param name="sourceFolder">The folder to be copied.</param>
    /// <param name="targetFolder">The designated location of the copy.</param>
    protected override void CopyFolderOnFileSystem(IVirtualFolderItem sourceFolder, IVirtualFolderItem targetFolder) {
        CopyFolderOnFileSystem2(sourceFolder as ZipFolderItem,targetFolder as ZipFolderItem);
    }
    protected  void CopyFolderOnFileSystem2(ZipFolderItem sourceFolder, ZipFolderItem targetFolder)
    {
      //recurse
      NodeRepository.PerformWriteAction(() => CopyFolderContents(sourceFolder.Node, targetFolder.QualifiedIdentifier));
    }


    /// <summary>
    /// Recursively copies 
    /// </summary>
    /// <param name="sourceFolderNode">Represents the folder that needs to copied.</param>
    /// <param name="targetFolderPath">The path of the target folder that corresponds to the
    /// submitted <paramref name="sourceFolderNode"/>.</param>
    protected virtual void CopyFolderContents(ZipNode sourceFolderNode, string targetFolderPath)
    {
      //create the target folder
      NodeRepository.ZipFile.AddDirectoryByName(targetFolderPath);

      foreach (var childFile in sourceFolderNode.ChildFiles)
      {
        //copy file
        string path = CreateFilePath(targetFolderPath, childFile.GetLocalName());

        //create a deferred stream that extracts the file during writing
        var ds = new DeferredStream(() => Configuration.TempStreamFactory.CreateTempStream());
        NodeRepository.ZipFile.AddEntry(path, s => ds, (s, n) => ds.Dispose()); 
      }

      foreach (ZipNode childFolder in sourceFolderNode.ChildDirectories)
      {
        //create new target folder path and recurse
        string childPath = CreateFolderPath(targetFolderPath, childFolder.GetLocalName());
        CopyFolderContents(childFolder, childPath);
      }
    }

    protected override void MoveFileOnFileSystem(IVirtualFileItem sourceFile, IVirtualFileItem targetFile) {
        MoveFileOnFileSystem2(sourceFile as ZipFileItem,targetFile as ZipFileItem);
    }
    protected  void MoveFileOnFileSystem2(ZipFileItem sourceFile, ZipFileItem targetFile)
    {
      NodeRepository.PerformWriteAction(() => sourceFile.Node.FileEntry.FileName = targetFile.QualifiedIdentifier);
    }
    protected override void CopyFileOnFileSystem(IVirtualFileItem sourceFile, IVirtualFileItem targetFile) {
        CopyFileOnFileSystem2(sourceFile as ZipFileItem,targetFile as ZipFileItem);
    }
    protected  void CopyFileOnFileSystem2(ZipFileItem sourceFile, ZipFileItem targetFile)
    {
      using (TempStream tempStream = Configuration.TempStreamFactory.CreateTempStream())
      {
        sourceFile.Node.FileEntry.Extract(tempStream);
        tempStream.Position = 0;

        var zip = NodeRepository.ZipFile;
        NodeRepository.PerformWriteAction(() => zip.AddEntry(targetFile.QualifiedIdentifier, tempStream));
      }
    }

    /// <summary>
    /// Creates a stream to read the data of a given file from the file system.
    /// This method is invoked by <see cref="FileSystemProviderBase2{TFile,TFolder}.ReadFileContents(string)"/> after having
    /// performed access checks.
    /// </summary>
    /// <param name="fileItem">Represents the file to be read.</param>
    /// <returns>A stream that provides the file's binary data.</returns>
    protected override Stream OpenFileStreamFromFileSystem(IVirtualFileItem fileItem) {
        return OpenFileStreamFromFileSystem2(fileItem as ZipFileItem);
    }
    protected  Stream OpenFileStreamFromFileSystem2(ZipFileItem fileItem)
    {
      return fileItem.Node.FileEntry.OpenReader();
    }
    protected override void WriteFileStreamToFileSystem(IVirtualFileItem fileItem, Stream input) {
        WriteFileStreamToFileSystem2(fileItem as ZipFileItem,input);
    }
    protected  void WriteFileStreamToFileSystem2(ZipFileItem fileItem, Stream input)
    {
      NodeRepository.PerformWriteAction(() =>
          {
            NodeRepository.ZipFile.AddEntry(fileItem.QualifiedIdentifier, input);
          });
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
    protected override string CreateFileSystemFilePath(string parentFolder, string fileName)
    {
      //do not apply any custom logic to the submitted paths in order to
      //prevent probing. Just combine and return it - invalid paths will fail
      //once they are used

      if (parentFolder == null) parentFolder = String.Empty;
      if (fileName == null) fileName = String.Empty;

      return Path.Combine(parentFolder, fileName).EnsureForwardSlashes().RemoveRootSlash();
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
    protected override string CreateFileSystemFolderPath(string parentFolder, string folderName)
    {
      //do not apply any custom logic to the submitted paths in order to
      //prevent probing. Just combine and return it - invalid paths will fail
      //once they are used

      if (parentFolder == null) parentFolder = String.Empty;
      if (folderName == null) folderName = String.Empty;
      
      var path = Path.Combine(parentFolder, folderName);

      //make sure we have a proper directory path
      return path.EnsureDirectoryPath().RemoveRootSlash();
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
        return GetResourceLockChain2(file as ZipFileItem);
    }
    protected  List<string> GetResourceLockChain2(ZipFileItem file)
    {
      return file.Node.GetAncestors(false, false)
        .Select(n => n.FullName)
        .ToList();
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
        return GetResourceLockChain2(folder as ZipFolderItem);
    }
    protected  List<string> GetResourceLockChain2(ZipFolderItem folder)
    {
      return folder.Node.GetAncestors(false, false)
        .Select(n => n.FullName)
        .ToList();
    }


    /// <summary>
    /// Disposes the <see cref="NodeRepository"/> and
    /// underlying <see cref="ZipFile"/>.
    /// </summary>
    /// <param name="disposing">If disposing equals <c>false</c>, the method
    /// has been called by the runtime from inside the finalizer.</param>
    protected override void Dispose(bool disposing)
    {
      if(disposing && !IsDisposed)
      {
        NodeRepository.Dispose();  
      }

      base.Dispose(disposing);
    }
  }
}
