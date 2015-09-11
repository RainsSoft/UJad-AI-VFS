using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
//using Microsoft.Win32;

namespace Vfs.Util
{
  /// <summary>
  /// Provides helper methods that simplify dealing with file
  /// contents and content data access.
  /// </summary>
  public static class ContentUtil
  {
    //set the default content-type
    public const string UnknownContentType = "application/unknown";
    public const string BinaryContentType = "application/octet-stream";

    /// <summary>
    /// Resolves the content type for a given file based on the file's
    /// extension.
    /// </summary>
    /// <param name="fileExtension">File extension including the dot (.), e.g.
    /// <c>.zip</c> or <c>.txt</c>.</param>
    /// <returns>The resolved content type, or a default of <c>application/unknown</c>
    /// if the type couldn't be resolved.</returns>
    public static string ResolveContentType(string fileExtension)
    {
#if !SILVERLIGHT && UNITY_STANDALONE
      try
      {
        //look for fileExtension
        RegistryKey extensionKey = Registry.ClassesRoot.OpenSubKey(fileExtension);
        if (extensionKey == null) return UnknownContentType;

        //retrieve Content Type value
        const string contentType = "Content Type";
        return extensionKey.GetValue(contentType, UnknownContentType).ToString();
      }
      catch
      {
        return UnknownContentType;
      }
#endif

      return UnknownContentType;
    }


    /// <summary>
    /// This is a convenience extension method for <see cref="IFileSystemProvider"/> instances,
    /// which retrieves the binary contents from a provivder, and writes them into a given file.
    /// </summary>
    /// <param name="provider">The <see cref="IFileSystemProvider"/> to be invoked in order to get access to the
    /// file's contents.</param>
    /// <param name="fileInfo">Provides meta information about the file to be read.</param>
    /// <param name="filePath">The file to be created. If a corresponding file already
    /// exists, it will be overwritten.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="fileInfo"/>
    /// is a null reference.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="filePath"/>
    /// is a null reference.</exception>
    public static void SaveFile(this IFileSystemProvider provider, VirtualFileInfo fileInfo, string filePath)
    {
      using (Stream destination = new FileStream(filePath, FileMode.Create, FileAccess.Write))
      {
        ReadFile(provider, fileInfo, destination, true);
      }
    }

    /// <summary>
    /// This is a convenience extension method for <see cref="IFileSystemProvider"/> instances,
    /// which retrieves the binary contents from a provivder, and writes them into a given file.
    /// This is a non-blocking operation which invokes the submitted <param name="completionCallback" />
    /// once the process has been completed.
    /// </summary>
    /// <param name="provider">The <see cref="IFileSystemProvider"/> to be invoked in order to get access to the
    /// file's contents.</param>
    /// <param name="fileInfo">Provides meta information about the file to be read.</param>
    /// <param name="filePath">The file to be created. If a corresponding file already
    /// exists, it will be overwritten.</param>
    /// <param name="completionCallback">Invoked as soon as the operation has been completed,
    /// or aborted. This is an optional parameter, which can be null.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="fileInfo"/>
    /// is a null reference.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="filePath"/>
    /// is a null reference.</exception>
    public static void BeginSaveFile(this IFileSystemProvider provider, VirtualFileInfo fileInfo, string filePath,
                                     Action<FileOperationResult> completionCallback)
    {
      Stream destination = new FileStream(filePath, FileMode.Create, FileAccess.Write);
      BeginReadFile(provider, fileInfo, destination, completionCallback);
    }


    /// <summary>
    /// This is a convenience extension method for <see cref="IFileSystemProvider"/> instances,
    /// which gets the binary contents as a stream as a blocking operation.
    /// Use the methods in <see cref="ContentUtil"/> class for simplified stream
    /// handling.
    /// </summary>
    /// <param name="provider">The <see cref="IFileSystemProvider"/> to be invoked in order to get access to the
    /// file's contents.</param>
    /// <param name="fileInfo">Provides meta information about the file to be read.</param>
    /// <param name="destination">A stream to which the file's contents are written.</param>
    /// <param name="autoCloseStream">Whether to automatically close the submitted
    /// <paramref name="destination"/> stream.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="fileInfo"/>
    /// is a null reference.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="destination"/>
    /// is a null reference.</exception>
    public static void ReadFile(this IFileSystemProvider provider, VirtualFileInfo fileInfo, Stream destination, bool autoCloseStream)
    {
      if (provider == null) throw new ArgumentNullException("provider");
      if (fileInfo == null) throw new ArgumentNullException("fileInfo");
      if (destination == null) throw new ArgumentNullException("destination");

      try
      {
        using (Stream stream = provider.ReadFileContents(fileInfo.FullName))
        {
          stream.WriteTo(destination);
        }
      }
      finally
      {
        if(autoCloseStream)
        {
          destination.Close();
        }
      }
    }


    /// <summary>
    /// This is a convenience extension method for <see cref="IFileSystemProvider"/> instances,
    /// which writes the data of a given file to the submitted output stream as a non-blocking
    /// operation, and invokes the submitted delegate once the process has been completed.
    /// </summary>
    /// <param name="provider">The <see cref="IFileSystemProvider"/> to be invoked in order to get access to the
    /// file's contents.</param>
    /// <param name="fileInfo">Provides meta information about the file to be read.</param>
    /// <param name="output">A stream to which the file's contents are written. The stream will
    /// be automatically closed as soon as the operations finishes.</param>
    /// <param name="completionCallback">Invoked as soon as the operation has been completed,
    /// or aborted. This is an optional parameter, which can be null.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="fileInfo"/>
    /// is a null reference.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="output"/>
    /// is a null reference.</exception>
    public static void BeginReadFile(this IFileSystemProvider provider, VirtualFileInfo fileInfo, Stream output,
                                     Action<FileOperationResult> completionCallback)
    {
      ThreadPool.QueueUserWorkItem(delegate
                                     {
                                       FileOperationResult result = new FileOperationResult(fileInfo);
                                       try
                                       {
                                         ReadFile(provider, fileInfo, output, true);
                                         if (completionCallback != null) completionCallback(result);
                                       }
                                       catch (Exception e)
                                       {
                                         if (completionCallback != null)
                                         {
                                           result.Exception = e;
                                           completionCallback(result);
                                         }
                                         else
                                         {
                                           //log an error in order to make sure this doesn't go unnoticed
                                           string msg = "Async file read operation failed silently for file '{0}':\n{1}";
                                           msg = String.Format(msg, fileInfo.FullName, e);
                                           Debug.WriteLine(msg);
                                         }
                                       }
                                     });
    }



    /// <summary>
    /// An extension method that creates or overwrites a given file on the file system with the
    /// contents of a local file.
    /// </summary>
    /// <param name="provider">The <see cref="IFileSystemProvider"/> to be invoked in order to get access to the
    /// file's contents.</param>
    /// <param name="localFilePath">The path of the file to be uploaded.</param>
    /// <param name="destinationPath">The path of the file to be created on the file system.</param>
    /// <param name="overwrite">Whether an existing file should be overwritten
    /// or not. If this parameter is false and the file already exists, a
    /// <see cref="ResourceOverwriteException"/> is thrown.</param>
    public static VirtualFileInfo WriteFile(this IFileSystemProvider provider, string localFilePath, string destinationPath, bool overwrite)
    {
      Ensure.ArgumentNotNull(provider, "provider");
      Ensure.ArgumentNotNull(localFilePath, "localFilePath");
      Ensure.ArgumentNotNull(destinationPath, "destinationPath");

      FileInfo source = new FileInfo(localFilePath);
      if (!source.Exists)
      {
        string msg = "Local file [{0}] not found on disk.";
        msg = String.Format(msg, source.FullName);
        throw new FileNotFoundException(msg);
      }

      string contentType = ResolveContentType(source.Extension);
      using (var fileStream = source.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
      {
        return provider.WriteFile(destinationPath, fileStream, overwrite, source.Length, contentType);
      }
    }


    /// <summary>
    /// This is a convenience extension method for <see cref="IFileSystemProvider"/> instances,
    /// which writes the data of a given file to the submitted output stream as a non-blocking
    /// operation, and invokes the submitted delegate once the process has been completed.
    /// </summary>
    /// <param name="provider">The <see cref="IFileSystemProvider"/> to be invoked in order to get access to the
    /// file's contents.</param>
    /// <param name="localFilePath">The path of the file to be uploaded.</param>
    /// <param name="destinationPath">The path of the file to be created on the file system.</param>
    /// <param name="overwrite">Whether an existing file should be overwritten
    /// or not. If this parameter is false and the file already exists, a
    /// <see cref="ResourceOverwriteException"/> is thrown.</param>
    /// <param name="completionCallback">Invoked as soon as the operation has been completed,
    /// or aborted. This is an optional parameter, which can be null.</param>
    public static void BeginWriteFile(this IFileSystemProvider provider, string localFilePath, string destinationPath, bool overwrite,
                                     Action<FileOperationResult> completionCallback)
    {
      ThreadPool.QueueUserWorkItem(delegate
      {
        try
        {
          var fileInfo = WriteFile(provider, localFilePath, destinationPath, overwrite);
          if (completionCallback != null)
          {
            FileOperationResult result = new FileOperationResult(fileInfo);
            completionCallback(result);
          }
        }
        catch (Exception e)
        {
          if (completionCallback != null)
          {
            VirtualFileInfo fileInfo = new VirtualFileInfo();
            fileInfo.FullName = destinationPath;
            FileOperationResult result = new FileOperationResult(fileInfo) {Exception = e};
            completionCallback(result);
          }
          else
          {
            //log an error in order to make sure this doesn't go unnoticed
            string msg = "Async file write operation failed silently for file '{0}':\n{1}";
            msg = String.Format(msg, destinationPath, e);
            Debug.WriteLine(msg);
          }
        }
      });
    }



    /// <summary>
    /// Gets a buffer that contains all data of a given stream.
    /// </summary>
    /// <param name="source">The stream to read from.</param>
    /// <returns>All data that was read from the buffer.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="source"/>
    /// is a null reference.</exception>
    public static byte[] ReadIntoBuffer(this Stream source)
    {
      Ensure.ArgumentNotNull(source, "source");

      MemoryStream destination = source as MemoryStream;
      if (destination == null)
      {
        destination = new MemoryStream();
        source.WriteTo(destination);
      }

      return destination.ToArray();
    }


    /// <summary>
    /// Reads a given <paramref name="source"/> stream and writes its contents
    /// to the submitted <paramref name="destination"/> stream, using a default
    /// buffer size of <c>32768</c> bytes.
    /// </summary>
    /// <param name="source">The stream to source data from.</param>
    /// <param name="destination">The stream to write data to.</param>
    public static void WriteTo(this Stream source, Stream destination)
    {
      source.WriteTo(destination, 32768);
    }


    /// <summary>
    /// Reads a given <paramref name="source"/> stream and writes its contents
    /// to a given file.
    /// </summary>
    /// <param name="source">The stream to source data from.</param>
    /// <param name="filePath">The file to be created. If a corresponding file already
    /// exists, it will be overwritten.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="source"/>
    /// is a null reference.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="filePath"/>
    /// is a null reference.</exception>
    public static void WriteTo(this Stream source, string filePath)
    {
      if (source == null) throw new ArgumentNullException("source");
      if (filePath == null) throw new ArgumentNullException("filePath");

      using (Stream destination = new FileStream(filePath, FileMode.Create, FileAccess.Write))
      {
        source.WriteTo(destination);
      }
    }



    /// <summary>
    /// Reads a given <paramref name="source"/> stream and writes its contents
    /// to the submitted <paramref name="destination"/> stream.
    /// </summary>
    /// <param name="source">The stream to source data from.</param>
    /// <param name="destination">The stream to write data to.</param>
    /// <param name="bufferSize">The buffer size to be used to read the
    /// <paramref name="source"/> stream.
    /// </param>
    public static void WriteTo(this Stream source, Stream destination, int bufferSize)
    {
      if (source == null) throw new ArgumentNullException("source");
      if (destination == null) throw new ArgumentNullException("destination");

      //use default byte sizes
      byte[] buffer = new byte[bufferSize];

      while (true)
      {
        int bytesRead = source.Read(buffer, 0, buffer.Length);
        if (bytesRead > 0)
        {
          destination.Write(buffer, 0, bytesRead);
        }
        else
        {
          destination.Flush();
          break;
        }
      }
    }


    /// <summary>
    /// Calculates the MD5 hash of a given file.
    /// </summary>
    /// <param name="file">The file to be hashed.</param>
    /// <returns>String representation of the hash.</returns>
    public static string CalculateMd5Hash(this FileInfo file)
    {
      using (var fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096))
      {
        return CalculateMd5Hash(fs);
      }
    }


    /// <summary>
    /// Calculates the MD5 hash for a given stream.
    /// </summary>
    /// <param name="stream">The stream to be hashed.</param>
    /// <returns>String representation of the hash.</returns>
    public static string CalculateMd5Hash(this Stream stream)
    {
      var md5 = new MD5CryptoServiceProvider();
      byte[] hash = md5.ComputeHash(stream);     
      return BitConverter.ToString(hash).ToLowerInvariant();
    }


#if !SILVERLIGHT

    /// <summary>
    /// Returns a resized copy of a given buffer. This is a convenience
    /// extension method to the <see cref="Array.Copy(Array, Array, long"/> method.
    /// </summary>
    /// <param name="buffer">The buffer to be copied.</param>
    /// <param name="length">The size of the returned copy.</param>
    /// <returns>A copy of the submitted array with the adjusted length.</returns>
    public static byte[] CreateCopy(this byte[] buffer, long length)
    {
      //the Array.Copy method already throws all exceptionsin case of invalid params
      byte[] copy = new byte[length];
      Array.Copy(buffer, copy, length);
      return copy;
    }
#else
    /// <summary>
    /// Returns a resized copy of a given buffer. This is a convenience
    /// extension method to the <see cref="Array.Copy(Array, Array, int"/> method.
    /// </summary>
    /// <param name="buffer">The buffer to be copied.</param>
    /// <param name="length">The size of the returned copy.</param>
    /// <returns>A copy of the submitted array with the adjusted length.</returns>
    public static byte[] CreateCopy(this byte[] buffer, int length)
    {
      //the Array.Copy method already throws all exceptionsin case of invalid params
      byte[] copy = new byte[length];
      Array.Copy(buffer, copy, length);
      return copy;
    }
#endif
  }
}