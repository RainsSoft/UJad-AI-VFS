using System;
using System.IO;
using System.Security.Principal;


namespace Vfs.Security
{
  public class WindowsIdentityClaimsProvider : IFileSystemSecurity
  {
    /// <summary>
    /// The folder that is used as a base in order to process
    /// relative file paths.
    /// </summary>
    public DirectoryInfo RootDirectory { get; set; }



    /// <summary>
    /// Gets authorization claims for a given folder resource.
    /// </summary>
    /// <param name="folderItem">The currently processed folder.</param>
    /// <returns>The requesting party's permissions for the
    /// submitted <paramref name="folderItem"/>.</returns>
    public FolderClaims GetFolderClaims(IVirtualFolderItem folderItem)
    {
      throw new NotImplementedException(""); //TODO provide implementation
    }


    /// <summary>
    /// Gets authorization claims for a given file resource.
    /// </summary>
    /// <param name="fileItem">The currently processed file.</param>
    /// <returns>The requesting party's permissions for the
    /// submitted <paramref name="fileItem"/>.</returns>
    public FileClaims GetFileClaims(IVirtualFileItem fileItem)
    {
      throw new NotImplementedException(""); //TODO provide implementation
    }


    /// <summary>
    /// Resolves an identity that is being associated with a running
    /// request or context. May return null.
    /// </summary>
    /// <returns>An identitiy, or a null reference if the provider does
    /// not support individual identities.</returns>
    public IIdentity GetIdentity()
    {
      return WindowsIdentity.GetCurrent();
    }
  }
}
