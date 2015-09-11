using System;
using System.Security.Principal;

namespace Vfs.Security
{
  /// <summary>
  /// A security implementation that relies on delegates
  /// to resolve claims and identities.
  /// </summary>
  public class DelegateSecurity : IFileSystemSecurity
  {
    /// <summary>
    /// Resolves authorization claims when <see cref="GetFolderClaims"/>
    /// is being invoked.
    /// </summary>
    public Func<IVirtualFolderItem, FolderClaims> FolderClaimsResolverFunc { get; set; }

    /// <summary>
    /// Resolves authorization claims when <see cref="GetFileClaims"/>
    /// is being invoked.
    /// </summary>
    public Func<IVirtualFileItem, FileClaims> FileClaimsResolverFunc { get; set; }

    /// <summary>
    /// Resolves the requesting identity when <see cref="GetIdentity"/>
    /// is being invoked.
    /// </summary>
    public Func<IIdentity> IdentityResolverFunc { get; set; }


    /// <summary>
    /// Gets authorization claims for a given folder resource.
    /// </summary>
    /// <param name="folderItem">The currently processed folder.</param>
    /// <returns>The requesting party's permissions for the
    /// submitted <paramref name="folderItem"/>.</returns>
    public FolderClaims GetFolderClaims(IVirtualFolderItem folderItem)
    {
      return FolderClaimsResolverFunc == null ? null : FolderClaimsResolverFunc(folderItem);
    }

    /// <summary>
    /// Gets authorization claims for a given file resource.
    /// </summary>
    /// <param name="fileItem">The currently processed file.</param>
    /// <returns>The requesting party's permissions for the
    /// submitted <paramref name="fileItem"/>.</returns>
    public FileClaims GetFileClaims(IVirtualFileItem fileItem)
    {
      return FileClaimsResolverFunc == null ? null : FileClaimsResolverFunc(fileItem);
    }

    /// <summary>
    /// Resolves an identity that is being associated with a running
    /// request or context. May return null.
    /// </summary>
    /// <returns>An identitiy, or a null reference if the provider does
    /// not support individual identities.</returns>
    public IIdentity GetIdentity()
    {
      return IdentityResolverFunc == null ? null : IdentityResolverFunc();
    }
  }
}
