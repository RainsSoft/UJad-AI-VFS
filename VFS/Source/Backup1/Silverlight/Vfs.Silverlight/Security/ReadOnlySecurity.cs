namespace Vfs.Security
{
  /// <summary>
  /// A security provider which allows read-only access
  /// on the file system, meaning that folders can be
  /// browsed, and file data downloaded.
  /// </summary>
  public class ReadOnlySecurity : DelegateSecurity
  {
    public ReadOnlySecurity()
    {
      FolderClaimsResolverFunc = fi =>
                                   {
                                     var claims = FolderClaims.CreateRestricted();
                                     claims.AllowListContents = true;
                                     return claims;
                                   };


      FileClaimsResolverFunc = fi =>
                                 {
                                   var claims = FileClaims.CreateRestricted();
                                   claims.AllowReadData = true;
                                   return claims;
                                 };
    }
  }
}
