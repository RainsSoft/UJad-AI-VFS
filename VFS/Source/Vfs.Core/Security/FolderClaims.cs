namespace Vfs.Security
{
  /// <summary>
  /// Provides permissions for a given folder resource.
  /// </summary>
  public class FolderClaims : ResourceClaims
  {
    /// <summary>
    /// Whether to create sub files and folders
    /// within the folder.
    /// </summary>
    public bool AllowListContents { get; set; }

    /// <summary>
    /// Whether files can be added to the folder.
    /// </summary>
    public bool AllowAddFiles { get; set; }

    /// <summary>
    /// Whether sub folders can be created or not.
    /// </summary>
    public bool AllowAddFolders { get; set; }


    
    /// <summary>
    /// Creates a <see cref="FolderClaims"/> instance that
    /// allows all operations per default.
    /// </summary>
    public static FolderClaims CreatePermissive()
    {
      return new FolderClaims
               {
                 AllowAddFiles = true,
                 AllowAddFolders = true,
                 AllowListContents = true,
                 AllowDelete = true,
                 AllowRename = true
               };
    }


    /// <summary>
    /// Creates a <see cref="FolderClaims"/> instance that
    /// denies all operations per default.
    /// </summary>
    public static FolderClaims CreateRestricted()
    {
      return new FolderClaims
      {
        AllowAddFiles = false,
        AllowAddFolders = false,
        AllowListContents = false,
        AllowDelete = false,
        AllowRename = false
      };
    }
  }
}