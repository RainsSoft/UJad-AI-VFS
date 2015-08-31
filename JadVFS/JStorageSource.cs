#region Using Directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

using JadEngine.VFS.Storage;
using JadEngine.VFS.Storage.Filters;

#endregion

namespace JadEngine.VFS
{
	/// <summary>
	/// Represents a storage file. A storage file is a file 
	/// that contains files and directories inside itself.
	/// </summary>
	public class JStorageSource : JFilesSource, IDisposable
	{
		#region Constants

		/// <summary>
		/// Latest storage file version
		/// </summary>
		public const int LatestVersion = 1;

		#endregion

		#region Fields

		/// <summary>
		/// Version of this storage file
		/// </summary>
		private int _version;

		/// <summary>
		/// Root directory
		/// </summary>
		private JDirectory _root;

		/// <summary>
		/// Stream to the storage file on the hard disk
		/// </summary>
		private FileStream _storageFileStream;

		/// <summary>
		/// List of filters known by this storage file
		/// </summary>
		private List<IJFilter> _filters;

		/// <summary>
		/// Dispose pattern
		/// </summary>
		private bool _disposed = false;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the version of the storage
		/// </summary>
		public int Version
		{
			get { return _version; }
			internal set { _version = value; }
		}

		/// <summary>
		/// Gets or sets the root directory of the storage
		/// </summary>
		public JDirectory Root
		{
			get { return _root; }
			internal set { _root = value; }
		}

		/// <summary>
		/// Gets the list of filters of the storage
		/// </summary>
		public List<IJFilter> Filters
		{
			get { return _filters; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="name">Name of the storage</param>
		public JStorageSource(String name)
		{
			_name = name;
			_filters = new List<IJFilter>();
		}

		/// <summary>
		/// Constructor that reads the storage data
		/// </summary>
		/// <param name="name">Name of the storage</param>
		/// <param name="path">Path of the storage file</param>
		public JStorageSource(String name, String path)
			: this(name)
		{
			if (System.IO.Path.IsPathRooted(path))
				Read(path);

			else
				Read(System.IO.Path.Combine(Jad.VFS.RootDirectory, path));
		}

		/// <summary>
		/// Overloaded constructor
		/// </summary>
		/// <param name="name">Name of the storage</param>
		/// <param name="version">Version of the storage</param>
		/// <param name="filters">Filters known by the storage</param>
		/// <param name="root">Root directory of the storage</param>
		public JStorageSource(String name, int version, List<IJFilter> filters, JDirectory root)
			: this(name)
		{
			_version = version;
			_root = root;

			if (filters != null)
				_filters = filters;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Reads a storage file from disk
		/// </summary>
		/// <param name="storagePath">Path to the storage file</param>
		public void Read(String storagePath)
		{
			IJStorageLoader loader;

			// Open the storage file
			_storageFileStream = File.Open(storagePath, FileMode.Open, FileAccess.Read);
			_path = storagePath;

			// Read it
			using (BinaryReader br = new BinaryReader(_storageFileStream))
			{
				// Get the storage version
				_version = br.ReadInt32();

				// Get a loader to load this file version
				loader = JStorageManager.GetLoader(_version);

				// Load the storage
				loader.Read(this, br);
			}

			// Reopen the file (as closing the BinaryReader closed it)
			this._storageFileStream = File.Open(storagePath, FileMode.Open, FileAccess.Read);
		}

		/// <summary>
		/// Writes the storage to disk
		/// </summary>
		/// <param name="filePath">Path to write the file</param>
		/// <param name="oldStorage">TODO</param>
		public void Write(String filePath, JStorageSource oldStorage)
		{
			IJStorageLoader loader;

			// Get a loader for this storage version
			loader = JStorageManager.GetLoader(this._version);

			// Write the storage
			loader.Write(this, oldStorage, filePath);
		}

		/// <summary>
		/// Extracts the storage file contents
		/// </summary>
		/// <param name="path"></param>
		public void ExtractFiles(String path)
		{
			ExtractDirectoryFiles(path, _root);
		}

		/// <summary>
		/// Disposes the storage
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
		}

		/// <summary>
		/// Dispose pattern
		/// </summary>
		/// <param name="disposing">Dispose managed resouces or not</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (_storageFileStream != null)
						_storageFileStream.Close();
				}
			}
		}

		#endregion

		#region JFilesSource Methods

		/// <summary>
		/// Gets a stream to a file.
		/// </summary>
		/// <param name="path">Relative path of the file.</param>
		/// <param name="fileName">Name of the file.</param>
		/// <param name="recurse">If the search should be recursive or not.</param>
		/// <returns>A stream to the file.</returns>
		public override Stream GetFile(string path, string fileName, bool recurse)
		{
			// The path can't be rooted
			if (System.IO.Path.IsPathRooted(path))
				throw new IOException("The path to get a file from a StorageSource can't be rooted.");

			return GetFileStream(path, fileName, recurse);
		}

		/// <summary>
		/// Gets a stream to a file.
		/// </summary>
		/// <param name="qualifiedName">Relative path and name of the file.</param>
		/// <returns>A stream to the file.</returns>
		/// <remarks>This search is never recursive.</remarks>
		public override Stream GetFile(string qualifiedName)
		{
			if (System.IO.Path.IsPathRooted(qualifiedName))
				throw new IOException("The path to get a file from a StorageSource can't be rooted.");

			return GetFileStream(System.IO.Path.GetDirectoryName(qualifiedName), System.IO.Path.GetFileName(qualifiedName), false);
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

			return GetFileStream(dir, fileName, recurse);
		}

		/// <summary>
		/// Finds a file.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <returns>A stream to the file.</returns>
		/// <remarks>This search is always recursive.</remarks>
		public override Stream FindFile(string fileName)
		{
			return GetFileStream(string.Empty, fileName, true);
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
			if (System.IO.Path.IsPathRooted(path))
				throw new IOException("The path to get list of files from a StorageSource can't be rooted.");

			return GetFilesNames(path, recurse, searchPattern);
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

			return GetFilesNames(dir, recurse, searchPattern);
		}

		#endregion

		#region Helper Methods

		/// <summary>
		/// Gets a stream to read a <see cref="JFile"/> contents
		/// </summary>
		/// <remarks>
		/// The stream can be a <see cref="JVirtualFileStream"/> if no
		/// filters are used or a <see cref="System.IO.MemoryStream"/> if they are
		/// </remarks>
		/// <param name="file">File to read the contents</param>
		/// <returns>A stream to read the file</returns>
		private Stream GetStream(JFile file)
		{
			JVirtualFileStream baseStream;

			if (file == null)
				return null;

			baseStream = new JVirtualFileStream(_storageFileStream, file.Offset, file.Length);
			return file.ApplyFiltersForRead(baseStream);
		}

		/// <summary>
		/// Extracts the contents of one directory and all the subdirectories inside it
		/// </summary>
		/// <param name="path">Path to extract the contents</param>
		/// <param name="rootDirectory">Root directory where the extraction begins</param>
		private void ExtractDirectoryFiles(String path, JDirectory rootDirectory)
		{
			Stream inFile, outFile;
			BinaryWriter bw = null;
			byte[] bytes;
			int bytesRead;

			// If the path doesn't exist, create it
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			// For each file in the root node
			foreach (JFile file in rootDirectory.GetFiles())
			{
				try
				{
					// Create the new hard disk file
					outFile = File.Open(System.IO.Path.Combine(path, file.Name), FileMode.Create, FileAccess.Write);
					bw = new BinaryWriter(outFile);

					inFile = GetStream(file);

					// Write the storage data to it
					bytes = new byte[4096];
					while ((bytesRead = inFile.Read(bytes, 0, bytes.Length)) != 0)
						bw.Write(bytes, 0, bytesRead);

					bw.Flush();
				}

				catch (Exception)
				{ }

				finally
				{
					if (bw != null)
						bw.Close();
				}
			}

			foreach (JDirectory directory in rootDirectory.GetDirectories())
				ExtractDirectoryFiles(System.IO.Path.Combine(path, directory.Name), directory);
		}

		/// <summary>
		/// Helper method to get a stream to a <see cref="JFile"/>.
		/// </summary>
		/// <param name="path">Path of the file.</param>
		/// <param name="fileName">Name of the file.</param>
		/// <param name="recurse">If the search should be recursive or not.</param>
		/// <returns>A stream to the file.</returns>
		private Stream GetFileStream(string path, string fileName, bool recurse)
		{
			Stream stream;

			stream = null;
			if (recurse)
				stream = GetStream(_root.GetFile(path, fileName, SearchOption.AllDirectories));

			else
				stream = GetStream(_root.GetFile(path, fileName, SearchOption.TopDirectoryOnly));

			return stream;
		}

		/// <summary>
		/// Helper method to get the list of files in a <see cref="JDirectory"/>.
		/// </summary>
		/// <param name="path">Path to perform the search.</param>
		/// <param name="recurse">If the search should be recursive or not.</param>
		/// <param name="searchPattern">Pattern for the search.</param>
		/// <returns>A collection of qualified names of the files found.</returns>
		private Collection<string> GetFilesNames(string path, bool recurse, string searchPattern)
		{
			// Get the files names
			if (recurse)
				return _root.GetFileNames(path, searchPattern, SearchOption.AllDirectories);

			else
				return _root.GetFileNames(path, searchPattern, SearchOption.TopDirectoryOnly);
		}

		#endregion
	}
}
