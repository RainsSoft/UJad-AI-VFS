using System;
using System.IO;

// ReSharper disable CheckNamespace
namespace Vfs.Util
// ReSharper restore CheckNamespace
{
  public static class TempFileUtil
  {
    /// <summary>
    /// Generates a path for a temporary file.
    /// </summary>
    /// <param name="prefix">The base name of the file.</param>
    /// <param name="extension">Extension of the file (without dot).</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">If <paramref name="prefix"/>
    /// is a null reference.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="extension"/>
    /// is a null reference.</exception>
    public static string CreateTempFilePath(string prefix, string extension)
    {
      if (prefix == null) throw new ArgumentNullException("prefix");
      if (extension == null) throw new ArgumentNullException("extension");
      string path = String.Format("{0}.{1}.{2}", prefix, Guid.NewGuid(), extension);
      return Path.Combine(Path.GetTempPath(), path);
    }

    /// <summary>
    /// Generates a path for a temporary file.
    /// </summary>
    /// <param name="rootFolder">The folder that contains the file.</param>
    /// <param name="prefix">The base name of the file.</param>
    /// <param name="extension">Extension of the file (without dot).</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">If <paramref name="prefix"/>
    /// is a null reference.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="extension"/>
    /// is a null reference.</exception>
    public static string CreateTempFilePath(string rootFolder, string prefix, string extension)
    {
      if (prefix == null) throw new ArgumentNullException("prefix");
      if (extension == null) throw new ArgumentNullException("extension");
      string path = String.Format("{0}.{1}.{2}", prefix, Guid.NewGuid(), extension);
      return Path.Combine(rootFolder, path);
    }



    /// <summary>
    /// Creates a folder withing the system's temporary files folder
    /// that matches the submitted <paramref name="folderName"/>.
    /// If a folder with the submitted name already exists, a new
    /// folder with a numeric suffix (001, 002, ...) is being created.
    /// </summary>
    /// <param name="folderName">The suggested folder name.</param>
    public static DirectoryInfo CreateTempFolder(string folderName)
    {
      return CreateTempFolder(new DirectoryInfo(Path.GetTempPath()), folderName);
    }


    /// <summary>
    /// Creates a folder the at a given location that matches the submitted
    /// <paramref name="folderName"/>. If a folder with the submitted name
    /// already exists, a new folder with a numeric suffix (001, 002, ...)
    /// is being created.
    /// </summary>
    /// <param name="rootFolder">The root folder in which the temporary
    /// folder is being created.</param>
    /// <param name="folderName">The suggested folder name.</param>
    public static DirectoryInfo CreateTempFolder(DirectoryInfo rootFolder, string folderName)
    {
      var tempFolder = Path.Combine(rootFolder.FullName, folderName);

      //make sure the folder is new
      int fileCounter = 0;
      while (Directory.Exists(tempFolder))
      {
        //create a numeric suffix (001, 002, ...) until a unique folder name was found
        fileCounter++;
        tempFolder = Path.Combine(rootFolder.FullName, folderName + fileCounter.ToString("000"));
      }

      //create temp folder
      return Directory.CreateDirectory(tempFolder);
    }

  }
}
