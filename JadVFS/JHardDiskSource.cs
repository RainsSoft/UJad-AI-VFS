#region Using Directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

#endregion

namespace JadEngine.VFS
{
	/// <summary>
	/// Represents a path on a local hard disk (the most usual type of source).
	/// </summary>
	/// <remarks>All streams are returned with FileShare.Read value.</remarks>
	public class JHardDiskSource : JWritableSource
	{
		#region Constructors

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="name">Name of the files path.</param>
		/// <param name="path">Hard disk path of the files path.</param>
		public JHardDiskSource(String name, String path)
		{
			if (!System.IO.Path.IsPathRooted(path))
			    path = System.IO.Path.Combine(Jad.VFS.RootDirectory, path);

			if (!Directory.Exists(path))
				throw new IOException("The path \"" + path + "\" doesn't exist.");

			_path = path;
			_name = name;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Gets a stream to a file.
		/// </summary>
		/// <param name="path">Relative path of the file.</param>
		/// <param name="fileName">Name of the file.</param>
		/// <param name="recurse">If the search should be recursive or not.</param>
		/// <returns>A stream to the file.</returns>
		public override Stream GetFile(string path, string fileName, bool recurse)
		{
			TestPath(path);
			return GetFileStream(System.IO.Path.Combine(_path, path), fileName, recurse, FileAccess.Read);
		}

		/// <summary>
		/// Gets a stream to a file.
		/// </summary>
		/// <param name="qualifiedName">Relative path and name of the file.</param>
		/// <returns>A stream to the file.</returns>
		/// <remarks>This search is never recursive.</remarks>
		public override Stream GetFile(string qualifiedName)
		{
			TestPath(qualifiedName);
			return GetFileStream(System.IO.Path.Combine(_path, System.IO.Path.GetDirectoryName(qualifiedName)), System.IO.Path.GetFileName(qualifiedName), false, FileAccess.Read);
		}

		/// <summary>
		/// Gets a stream to a file.
		/// </summary>
		/// <param name="definedPath">A defined path where to search the file.</param>
		/// <param name="fileName">Name of the file.</param>
		/// <param name="recurse">If the search should be recursive or not.</param>
		/// <returns>A stream to the file.</returns>
		public override Stream GetFileFromDefinedPath(string definedPath, string fileName, bool recurse)
		{
			String dir;

			// If the path is not defined in this source, return
			if (!_definedPaths.TryGetValue(definedPath, out dir))
				return null;

			return GetFileStream(System.IO.Path.Combine(_path, dir), fileName, recurse, FileAccess.Read);
		}

		/// <summary>
		/// Finds a file.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <returns>A stream to the file.</returns>
		/// <remarks>This search is always recursive.</remarks>
		public override Stream FindFile(string fileName)
		{
			return GetFileStream(_path, fileName, true, FileAccess.Read);
		}

		/// <summary>
		/// Gets the collection of files on a directory.
		/// </summary>
		/// <param name="path">Path of the directory</param>
		/// <param name="recurse">If the search should be recursive (include subdirectories) or not.</param>
		/// <param name="searchPattern">Mask to filter the files.</param>
		/// <returns>The collection of files of the directory.</returns>
		public override Collection<string> GetFiles(string path, bool recurse, string searchPattern)
		{
			TestPath(path);
			return GetFilesNames(System.IO.Path.Combine(_path, path), recurse, searchPattern);
		}

		/// <summary>
		/// Gets the collection of files on a defined path.
		/// </summary>
		/// <param name="definedPath">A defined path where to search the file.</param>
		/// <param name="recurse">If the search should be recursive (include subdirectories) or not.</param>
		/// <param name="searchPattern">Mask to filter the files.</param>
		/// <returns>The collection of files of the directory.</returns>
		public override Collection<string> GetFilesFromDefinedPath(string definedPath, bool recurse, string searchPattern)
		{
			String dir;

			// If the path is not defined in this source, return
			if (!_definedPaths.TryGetValue(definedPath, out dir))
				return null;

			return GetFilesNames(System.IO.Path.Combine(_path, dir), recurse, searchPattern);
		}

		/// <summary>
		/// Gets a read/write stream to a file.
		/// </summary>
		/// <param name="path">Relative path of the file.</param>
		/// <param name="fileName">Name of the file.</param>
		/// <param name="recurse">If the search should be recursive or not.</param>
		/// <returns>A stream to the file.</returns>
		public override Stream GetWritableFile(string path, string fileName, bool recurse)
		{
			TestPath(path);
			return GetFileStream(System.IO.Path.Combine(_path, path), fileName, recurse, FileAccess.ReadWrite);
		}

		/// <summary>
		/// Gets a read/write stream to a file.
		/// </summary>
		/// <param name="qualifiedName">Relative path and name of the file.</param>
		/// <returns>A stream to the file.</returns>
		/// <remarks>This search is never recursive.</remarks>
		public override Stream GetWritableFile(string qualifiedName)
		{
			TestPath(qualifiedName);
			return GetFileStream(System.IO.Path.Combine(_path, System.IO.Path.GetDirectoryName(qualifiedName)), System.IO.Path.GetFileName(qualifiedName), false, FileAccess.ReadWrite);
		}

		/// <summary>
		/// Gets a read/write to a file.
		/// </summary>
		/// <param name="definedPath">A defined path where to search the file.</param>
		/// <param name="fileName">Name of the file.</param>
		/// <param name="recurse">If the search should be recursive or not.</param>
		/// <returns>A stream to the file.</returns>
		public override Stream GetWritableFileFromDefinedPath(string definedPath, string fileName, bool recurse)
		{
			String dir;

			// If the path is not defined in this source, return
			if (!_definedPaths.TryGetValue(definedPath, out dir))
				return null;

			return GetFileStream(System.IO.Path.Combine(_path, dir), fileName, recurse, FileAccess.ReadWrite);
		}

		/// <summary>
		/// Finds a file and returns a read/write stream to it.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <returns>A stream to the file.</returns>
		/// <remarks>This search is always recursive.</remarks>
		public override Stream FindWritableFile(string fileName)
		{
			return GetFileStream(_path, fileName, true, FileAccess.ReadWrite);
		}

		/// <summary>
		/// Creates a new file.
		/// </summary>
		/// <param name="qualifiedName">Qualified name of the new file.</param>
		/// <returns>A stream to the new file.</returns>
		/// <remarks>Files are created with FileMode.Create value, this means that if the file already exists it will be truncated.</remarks>
		public override Stream CreateWritableFile(string qualifiedName)
		{
			TestPath(qualifiedName);
			return File.Open(System.IO.Path.Combine(System.IO.Path.Combine(_path, System.IO.Path.GetDirectoryName(qualifiedName)), System.IO.Path.GetFileName(qualifiedName)), FileMode.Create, FileAccess.Write, FileShare.Read);
		}

		/// <summary>
		/// Creates a new file.
		/// </summary>
		/// <param name="path">Path of the new file.</param>
		/// <param name="fileName">Name of the new file.</param>
		/// <returns>A stream to the new file.</returns>
		/// <remarks>Files are created with FileMode.Create value, this means that if the file already exists it will be truncated.</remarks>
		public override Stream CreateWritableFile(string path, string fileName)
		{
			TestPath(path);
			return File.Open(System.IO.Path.Combine(System.IO.Path.Combine(_path, path), fileName), FileMode.Create, FileAccess.Write, FileShare.Read);
		}

		/// <summary>
		/// Creates a new file.
		/// </summary>
		/// <param name="definedPath">Defined path where the file will be located.</param>
		/// <param name="qualifiedName">Qualified name of the new file.</param>
		/// <returns>A stream to the new file.</returns>
		/// <remarks>Files are created with FileMode.Create value, this means that if the file already exists it will be truncated.</remarks>
		public override Stream CreateWritableFileOnDefinedPath(string definedPath, string qualifiedName)
		{
			String dir;

			TestPath(definedPath);

			// If the path is not defined in this source, return
			if (!_definedPaths.TryGetValue(definedPath, out dir))
				return null;

			return File.Open(System.IO.Path.Combine(System.IO.Path.Combine(System.IO.Path.Combine(_path, dir), System.IO.Path.GetDirectoryName(qualifiedName)), System.IO.Path.GetFileName(qualifiedName)), FileMode.Create, FileAccess.Write, FileShare.Read);
		}

		/// <summary>
		/// Creates a new file.
		/// </summary>
		/// <param name="definedPath">Defined path where the file will be located.</param>
		/// <param name="path">Path of the new file.</param>
		/// <param name="fileName">Name of the new file.</param>
		/// <returns>A stream to the new file.</returns>
		/// <remarks>Files are created with FileMode.Create value, this means that if the file already exists it will be truncated.</remarks>
		public override Stream CreateWritableFileOnDefinedPath(string definedPath, string path, string fileName)
		{
			String dir;

			TestPath(path);

			// If the path is not defined in this source, return
			if (!_definedPaths.TryGetValue(definedPath, out dir))
				return null;

			return File.Open(System.IO.Path.Combine(System.IO.Path.Combine(System.IO.Path.Combine(_path, dir), path), fileName), FileMode.Create, FileAccess.Write, FileShare.Read);
		}

		#endregion

		#region Helper Methods

		private void TestPath(string path)
		{
			// The path can't be rooted
			if (System.IO.Path.IsPathRooted(path))
				throw new IOException("The path to get a file from a HardDiskSource can't be rooted.");

			// The path can't contain ".."
			if (path.Contains(".."))
				throw new IOException("The path can't contain the \"..\" modifier.");
		}

		/// <summary>
		/// Helper method to get a stream to a file.
		/// </summary>
		/// <param name="rootedPath">Rootet path to the file.</param>
		/// <param name="fileName">Name of the file</param>
		/// <param name="recurse">If the file search should be recursive or not.</param>
		/// <param name="access">Access permissions to the file.</param>
		/// <returns>A stream to the file.</returns>
		private Stream GetFileStream(string rootedPath, string fileName, bool recurse, FileAccess access)
		{
			DirectoryInfo info;
			FileInfo[] files;

			// Check the path
			if (!Directory.Exists(rootedPath))
				return null;

			// Search the file
			info = new DirectoryInfo(rootedPath);

			if (recurse)
				files = info.GetFiles(fileName, SearchOption.AllDirectories);

			else
				files = info.GetFiles(fileName, SearchOption.TopDirectoryOnly);

			if (files.Length > 0)
				return files[0].Open(FileMode.Open, access, FileShare.Read);

			else
				return null;
		}

		/// <summary>
		/// Helper method to get the list of files in a directory.
		/// </summary>
		/// <param name="rootedPath">Rootet path to perform the search.</param>
		/// <param name="recurse">If the search should be recursive or not.</param>
		/// <param name="searchPattern">Pattern for the search.</param>
		/// <returns>A collection of qualified names of the files found.</returns>
		private Collection<string> GetFilesNames(string rootedPath, bool recurse, string searchPattern)
		{
			String[] directoryFiles;
			Collection<string> files;

			if (!Directory.Exists(rootedPath))
				return null;

			// Get the files names
			if (recurse)
				directoryFiles = Directory.GetFiles(rootedPath, searchPattern, SearchOption.AllDirectories);

			else
				directoryFiles = Directory.GetFiles(rootedPath, searchPattern, SearchOption.TopDirectoryOnly);

			files = new Collection<String>(new List<String>());
			foreach (String fullName in directoryFiles)
				files.Add(fullName.Remove(0, _path.Length + 1));

			return files;
		}

		#endregion
	}
}
