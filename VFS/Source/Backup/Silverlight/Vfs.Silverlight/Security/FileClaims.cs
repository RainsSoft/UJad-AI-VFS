namespace Vfs.Security
{
  /// <summary>
  /// Provides permissions for a given file resource.
  /// </summary>
  public class FileClaims : ResourceClaims
  {
    /// <summary>
    /// Whether the file's data can be read or not.
    /// </summary>
    public bool AllowReadData { get; set; }

    /// <summary>
    /// Whether an existing file can be overwritten or not.
    /// </summary>
    public bool AllowOverwrite { get; set; }

    /// <summary>
    /// Creates a <see cref="FileClaims"/> instance that
    /// allows all operations per default.
    /// </summary>
    public static FileClaims CreatePermissive()
    {
      return new FileClaims
               {
                 AllowDelete = true,
                 AllowRename = true,
                 AllowReadData = true,
                 AllowOverwrite = true
               };
    }


    /// <summary>
    /// Creates a <see cref="FileClaims"/> instance that
    /// denies all operations per default.
    /// </summary>
    public static FileClaims CreateRestricted()
    {
      return new FileClaims
               {
                 AllowDelete = true,
                 AllowRename = true,
                 AllowReadData = true,
                 AllowOverwrite = true
               };
    }
  }
}