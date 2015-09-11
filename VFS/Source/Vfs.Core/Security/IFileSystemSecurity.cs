using System.Security.Principal;

namespace Vfs.Security
{
  /// <summary>
  /// Provides business logic to resolve file and folder
  /// permissions for a requesting party.
  /// </summary>
  public interface IFileSystemSecurity
  {
    /// <summary>
    /// Gets authorization claims for a given folder resource.
    /// </summary>
    /// <param name="folderItem">The currently processed folder.</param>
    /// <returns>The requesting party's permissions for the
    /// submitted <paramref name="folderItem"/>.</returns>
    FolderClaims GetFolderClaims(IVirtualFolderItem folderItem);


    /// <summary>
    /// Gets authorization claims for a given file resource.
    /// </summary>
    /// <param name="fileItem">The currently processed file.</param>
    /// <returns>The requesting party's permissions for the
    /// submitted <paramref name="fileItem"/>.</returns>
    FileClaims GetFileClaims(IVirtualFileItem fileItem);


    /// <summary>
    /// Resolves an identity that is being associated with a running
    /// request or context. May return null.
    /// </summary>
    /// <returns>An identitiy, or a null reference if the provider does
    /// not support individual identities.</returns>
    IIdentity GetIdentity();
  }

}