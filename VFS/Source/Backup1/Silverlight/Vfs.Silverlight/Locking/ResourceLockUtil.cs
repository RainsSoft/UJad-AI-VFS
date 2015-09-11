using System.Collections.Generic;

namespace Vfs.Locking
{
  public static class ResourceLockUtil
  {
    /// <summary>
    /// Gets a read lock on all parent folders of a given resource, and rolls back the resources
    /// if the locking fails.
    /// </summary>
    /// <param name="repository">The repository that manages locked resources.</param>
    /// <param name="resourceId">The resource to be locked.</param>
    /// <param name="isWriteLock">Whether to acquire a read or write lock for the resource itself.</param>
    /// <param name="parentFolderIds">Ids of the resource's parent folders, which are being locked one
    /// by one. The list should be ordered, with the first item being the immediate parent of the locked
    /// resource, and the last one the topmost folder to be locked.</param>
    /// <returns>A guard which releases the resource and all folders once it is being disposed.</returns>
    public static ResourceLockGuard GetResourceChainLock(this IResourceLockRepository repository, string resourceId,
                                                      bool isWriteLock, List<string> parentFolderIds)
    {
      LockItem lockItem = isWriteLock ? repository.TryGetWriteLock(resourceId) : repository.TryGetReadLock(resourceId);

      //we couldn't get a lock on the resource itself
      if (!lockItem.IsEnabled) return new ResourceLockGuard(lockItem, repository);

      //get read locks for all parent folders
      List<LockItem> folderLocks = new List<LockItem>();
      LockItem folderLock = null;
      foreach (string folderId in parentFolderIds)
      {
        //lock parent folders, one by one for read access (prevents deletion)
        folderLock = repository.TryGetReadLock(folderId);
        if (!folderLock.IsEnabled) break;
        folderLocks.Add(folderLock);
      }

      if (folderLock != null && !folderLock.IsEnabled)
      {
        //roll back all locks...
        repository.ReleaseLock(lockItem);
        folderLocks.ForEach(li => repository.ReleaseLock(li));
        //...and return a dummy lock
        return new ResourceLockGuard(LockItem.CreateDenied(resourceId), repository);
      }

      return new ResourceLockGuard(lockItem, repository) {SecondaryLocks = folderLocks};
    }
  }
}