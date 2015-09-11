namespace Vfs.FileSystemService.Faults
{
  /// <summary>
  /// Known VFS fault types.
  /// </summary>
  public enum ResourceFaultType
  {
    Undefined = 0,
    ResourceAccess = 1,
    ResourceNotFound = 2,
    ResourceOverwrite =3
  }
}