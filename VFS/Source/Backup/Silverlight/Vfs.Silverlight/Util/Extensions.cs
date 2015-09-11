using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Vfs.Util;

namespace Vfs
{
  public static class Extensions
  {
    /// <summary>
    /// Performs a given action on every item of a given sequence.
    /// </summary>
    /// <typeparam name="T">The collection's content.</typeparam>
    /// <param name="source">The sequence to be processed.</param>
    /// <param name="action">An action delegate that is being invoked
    /// for every item of the <paramref name="source"/> sequence.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="action"/>
    /// is a null reference.</exception>
    public static void Do<T>(this IEnumerable<T> source, Action<T> action)
    {
      if (action == null) throw new ArgumentNullException("action");

      foreach (var item in source)
      {
        action(item);
      }
    }


    /// <summary>
    /// Checks whether a timestamp has expired or not.
    /// </summary>
    /// <param name="timestamp">Expiration timestamp to evaluate.</param>
    /// <returns>True if the timestamp has expired. Null values are regarded
    /// valid.</returns>
    public static bool IsExpired(this DateTimeOffset? timestamp)
    {
      return timestamp != null && timestamp.Value < SystemTime.Now();
    }


    /// <summary>
    /// Checks a list of candidates for equality to a given
    /// reference value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value">The evaluated value.</param>
    /// <param name="candidates">A liste of possible values that are
    /// regarded valid.</param>
    /// <returns>True if one of the submitted <paramref name="candidates"/>
    /// matches the evaluated value. If the <paramref name="candidates"/>
    /// parameter itself is null, too, the method returns false as well,
    /// which allows to check with null values, too.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="candidates"/>
    /// is a null reference.</exception>
    public static bool Is<T>(this T value, params T[] candidates)
    {
      if (candidates == null) return false;

      foreach (var t in candidates)
      {
        if (value.Equals(t)) return true;
      }

      return false;
    }


    /// <summary>
    /// Filters a collection of file system resources by applying a given
    /// regular expression to the resource's <see cref="VirtualResourceInfo.Name"/>.
    /// </summary>
    /// <typeparam name="T">The resource type.</typeparam>
    /// <param name="resources">The unfiltered collection.</param>
    /// <param name="searchPattern">A regex pattern to be applied.</param>
    /// <returns>A filtered collection.</returns>
    public static IEnumerable<T> Filter<T>(this IEnumerable<T> resources, string searchPattern) where T:VirtualResourceInfo
    {
      //convert wildcard syntax to regex
      string escapedPattern = Regex.Escape(searchPattern);
      escapedPattern = escapedPattern.Replace("\\*", ".*");
      escapedPattern = escapedPattern.Replace("\\?", ".");
      escapedPattern = "^" + escapedPattern + "$";

      Regex regex = new Regex(escapedPattern);
      return resources.Where(resource => regex.IsMatch(resource.Name));
    }


    #region create resource infos for local files / directories

    /// <summary>
    /// Creates meta information for a given file.
    /// </summary>
    /// <param name="localDirectory">The local directory.</param>
    /// <returns>A <see cref="VirtualFolderInfo"/> that matches the submitted directory.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="localDirectory"/>
    /// is a null reference.</exception>
    public static VirtualFolderInfo CreateFolderResourceInfo(this DirectoryInfo localDirectory)
    {
      Ensure.ArgumentNotNull(localDirectory, "localDirectory");

      localDirectory.Refresh();
      var item = new VirtualFolderInfo();
      MapProperties(localDirectory, item);
      item.ParentFolderPath = localDirectory.Parent == null ? String.Empty : localDirectory.Parent.FullName;
      item.IsEmpty = localDirectory.IsEmpty();

      return item;
    }


    /// <summary>
    /// Checks whether a given directory is empty or not. Returns false
    /// if the directory does not exist.
    /// </summary>
    /// <param name="directory">The directory to check.</param>
    /// <returns></returns>
    internal static bool IsEmpty(this DirectoryInfo directory)
    {
      try
      {
        string fullName = directory.FullName;
        if (!directory.Exists) return false;
#if !SILVERLIGHT
        return Directory.GetFiles(fullName).Length == 0 &&
               Directory.GetDirectories(fullName).Length == 0;
#endif

#if SILVERLIGHT
        return Directory.EnumerateFileSystemEntries(fullName).FirstOrDefault() != null;
#endif

      }
      catch (DirectoryNotFoundException)
      {
        //apparently, the folder was just removed
        return false;
      }
    }


    /// <summary>
    /// Creates meta information for a given file.
    /// </summary>
    /// <param name="localFile">The local file.</param>
    /// <returns>A <see cref="VirtualFileInfo"/> that matches the submitted file.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="localFile"/>
    /// is a null reference.</exception>
    public static VirtualFileInfo CreateFileResourceInfo(this FileInfo localFile)
    {
      if (localFile == null) throw new ArgumentNullException("localFile");
      localFile.Refresh();
      var item = new VirtualFileInfo
                   {
                     Length = localFile.Exists ? localFile.Length : 0,
                     ContentType = ContentUtil.ResolveContentType(localFile.Extension),
                     ParentFolderPath = localFile.DirectoryName,
                   };

      MapProperties(localFile, item);
      return item;
    }


    /// <summary>
    /// Maps common properties for files and folders.
    /// </summary>
    private static void MapProperties(FileSystemInfo localItem, VirtualResourceInfo virtualItem)
    {
      virtualItem.Name = localItem.Name;
      virtualItem.FullName = localItem.FullName;
      virtualItem.Description = String.Empty;

      bool exists = localItem.Exists;
      virtualItem.CreationTime = exists ? localItem.CreationTime : (DateTimeOffset?)null;
      virtualItem.LastWriteTime = exists ? localItem.LastWriteTime : (DateTimeOffset?)null;
      virtualItem.LastAccessTime = exists ? localItem.LastAccessTime : (DateTimeOffset?)null;

      if(exists)
      {
        var attributes = localItem.Attributes;
        virtualItem.IsHidden = ((attributes & FileAttributes.Hidden) == FileAttributes.Hidden);
        virtualItem.IsReadOnly = ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly);
      }
      else
      {
        virtualItem.IsHidden = false;
        virtualItem.IsReadOnly = false;
      }
    }

    #endregion


    /// <summary>
    /// Replaces the first occurence of a string with the replacement value.
    /// </summary>
    /// <param name="input">The string to examine.</param>
    /// <param name="oldValue">The value to replace.</param>
    /// <param name="newValue">The new value to be inserted.</param>
    /// <param name="comparison">String comparison directive.</param>
    /// <returns>Updated string. If the input value does not contain the
    /// submitted string, the original value is returned.</returns>
    public static string ReplaceFirst(this string input, string oldValue, string newValue, StringComparison comparison)
    {
      if (String.IsNullOrEmpty(input)) return input;
      if(String.IsNullOrEmpty(oldValue)) throw new ArgumentException("Search string cannot be null or empty");


      int pos = input.IndexOf(oldValue, comparison);
      if (pos < 0) return input;

      return input.Substring(0, pos) + newValue + input.Substring(pos + oldValue.Length);
    }

  }
}
