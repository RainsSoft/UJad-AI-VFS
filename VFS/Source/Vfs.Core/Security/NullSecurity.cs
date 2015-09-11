using System;
using System.Security.Principal;

namespace Vfs.Security
{
  /// <summary>
  /// A security plug-in that authorizes all requests.
  /// </summary>
  public class NullSecurity : IFileSystemSecurity
  {
    /// <summary>
    /// Gets authorization claims for a given folder resource.
    /// </summary>
    /// <param name="folderItem">The currently processed folder.</param>
    /// <returns>The requesting party's permissions for the
    /// submitted <paramref name="folderItem"/>.</returns>
    public FolderClaims GetFolderClaims(IVirtualFolderItem folderItem)
    {
      return FolderClaims.CreatePermissive();
    }

    /// <summary>
    /// Gets authorization claims for a given file resource.
    /// </summary>
    /// <param name="fileItem">The currently processed file.</param>
    /// <returns>The requesting party's permissions for the
    /// submitted <paramref name="fileItem"/>.</returns>
    public FileClaims GetFileClaims(IVirtualFileItem fileItem)
    {
      return FileClaims.CreatePermissive();
    }

    /// <summary>
    /// Resolves an identity that is being associalted with a running
    /// request or context. May return null.
    /// </summary>
    /// <returns>An identitiy, or a null reference if the provider does
    /// not support individual identities.</returns>
    public IIdentity GetIdentity()
    {
      return null;
    }
  }
}
