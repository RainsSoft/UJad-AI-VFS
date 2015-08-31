#region Using Directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

#endregion

namespace JadEngine.VFS
{
	/// <summary>
	/// Represents the file system of Jade. All file requests must be asked to this class.
	/// </summary>
	/// <remarks>
	/// The preference in the searches within this object is related to the order in with
	/// <see cref="JFilesSource"/> items are aggregated. The first item gets higher priority
	/// and so on.
	/// </remarks>
	public  class JVFS : JFilesSource
	{
		#region Fields

		/// <summary>
		/// The startup directory of the application.
		/// </summary>
		/// <remarks>
		/// This directory is where the first configuration
		/// file is placed.
		/// </remarks>
		private string _rootDirectory;

		/// <summary>
		/// The list of registered files paths of the system.
		/// </summary>
		/// <remarks>
		/// These objects are the ones that give the real references to the files,
		/// the <see cref="JVFS"/> only routes the petitions to them.
		/// </remarks>
		private Collection<JFilesSource> _sources;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the startup directory of the application.
		/// </summary>
		/// <remarks>
		/// This directory is where the first configuration
		/// file is placed.
		/// </remarks>
		public string RootDirectory
		{
			get { return _rootDirectory; }
			set
			{
				if (!System.IO.Path.IsPathRooted(value))
					throw new IOException("The startup path of the VFS must be a rooted path.");

				_rootDirectory = value;
			}
		}

		/// <summary>
		/// Gets the <see cref="JFilesSource"/> of the VFS.
		/// </summary>
		public Collection<JFilesSource> Sources
		{
			get { return _sources; }
		}

		/// <summary>
		/// Gets the <see cref="JWritableSource"/> of the VFS.
		/// </summary>
		public Collection<JWritableSource> WritableSources
		{
			get
			{
				JWritableSource writableSource;
				Collection<JWritableSource> writableSources;

				writableSources = new Collection<JWritableSource>();
				foreach (JFilesSource source in _sources)
				{
					writableSource = source as JWritableSource;
					if (writableSource != null)
						writableSources.Add(writableSource);
				}

				return WritableSources;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Default static constructor
		/// </summary>
		/// <param name="name">Name of the virtual file system.</param>
		/// <param name="rootDirectory">Root directory of the virtual file system.</param>
		public JVFS(string name, string rootDirectory)
		{
			_name = name;
			_rootDirectory = rootDirectory;
			_sources = new Collection<JFilesSource>(new List<JFilesSource>());
		}

		#endregion

		#region Methods

		/// <summary>
		/// Gets a specific <see cref="JFilesSource"/> by its name.
		/// </summary>
		/// <param name="sourceName">Name of the source to get.</param>
		/// <returns>The searched source.</returns>
		public JFilesSource Source(string sourceName)
		{
			foreach(JFilesSource source in _sources)
				if (source.Name.Equals(sourceName))
					return source;

			throw new IOException("The source \"" + sourceName + "\" doesn't exist in the Virtual File System \"" + _name + "\".");
		}

		/// <summary>
		/// Gets a specific <see cref="JWritableSource"/> by its name.
		/// </summary>
		/// <param name="sourceName">Name of the source to get.</param>
		/// <returns>The searched source.</returns>
		public JWritableSource WritableSource(string sourceName)
		{
			JWritableSource writableSource;

			foreach (JFilesSource source in _sources)
				if (source.Name.Equals(sourceName))
				{
					writableSource = source as JWritableSource;
					if (writableSource != null)
						return writableSource;

					throw new IOException("The source \"" + sourceName + "\" is not a JWritableSource in the Virtual File System \"" + _name + "\".");
				}

			throw new IOException("The source \"" + sourceName + "\" doesn't exist in the Virtual File System \"" + _name + "\".");
		}

		/// <summary>
		/// Creates a <see cref="JFilesSource"/>.
		/// </summary>
		/// <param name="type">Type of the source.</param>
		/// <param name="name">Name of the source.</param>
		/// <param name="path">Path of the source.</param>
		/// <returns>A new <see cref="JFilesSource"/>.</returns>
		public JFilesSource CreateSource(string type, string name, string path)
		{
			if ("HardDisk".Equals(type, StringComparison.InvariantCultureIgnoreCase))
				return new JHardDiskSource(name, path);

			if ("Storage".Equals(type, StringComparison.InvariantCultureIgnoreCase))
				return new JStorageSource(name, path);

			return null;
		}

		/// <summary>
		/// Gets a stream to a file.
		/// </summary>
		/// <param name="path">Relative path of the file.</param>
		/// <param name="fileName">Name of the file.</param>
		/// <param name="recurse">If the search should be recursive or not.</param>
		/// <returns>A stream to the file.</returns>
		public override Stream GetFile(string path, string fileName, bool recurse)
		{
			Stream result;

			foreach (JFilesSource source in _sources)
			{
				result = source.GetFile(path, fileName, recurse);
				if (result != null)
					return result;
			}

			throw new FileNotFoundException("The file \"" + fileName + "\" located on the path \"" + path + "\" wasn't found.");
		}

		/// <summary>
		/// Gets a stream to a file.
		/// </summary>
		/// <param name="qualifiedName">Relative path and name of the file.</param>
		/// <returns>A stream to the file.</returns>
		/// <remarks>This search is never recursive.</remarks>
		public override Stream GetFile(string qualifiedName)
		{
			Stream result;

			foreach (JFilesSource source in _sources)
			{
				result = source.GetFile(qualifiedName);
				if (result != null)
					return result;
			}

			throw new FileNotFoundException("The qualified file \"" + qualifiedName + "\" wasn't found.");
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
			Stream result;

			foreach (JFilesSource source in _sources)
			{
				result = source.GetFileFromDefinedPath(definedPath, fileName, recurse);
				if (result != null)
					return result;
			}

			throw new FileNotFoundException("The file \"" + fileName + "\" located on the defined path \"" + definedPath + "\" wasn't found.");
		}

		/// <summary>
		/// Finds a file.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <returns>A stream to the file.</returns>
		/// <remarks>This search is always recursive.</remarks>
		public override Stream FindFile(string fileName)
		{
			Stream result;

			foreach (JFilesSource source in _sources)
			{
				result = source.FindFile(fileName);
				if (result != null)
					return result;
			}

			throw new FileNotFoundException("The file \"" + fileName + "\" wasn't found.");
		}

		/// <summary>
		/// Gets the collection of files on a directory.
		/// </summary>
		/// <param name="path">Path of the directory</param>
		/// <param name="recurse">If the search should be recursive (include subdirectories) or not.</param>
		/// <param name="searchPattern">Mask to filter the files.</param>
		/// <returns>The collection of files of the directory.</returns>
		/// <remarks>
		/// Note to implementers: the file names should be correct names for the method "Stream GetFile(string qualifiedName)".
		/// </remarks>
		public override Collection<string> GetFiles(string path, bool recurse, string searchPattern)
		{
			Collection<string> totalResult, partialResult;

			totalResult = new Collection<string>(new List<string>());
			foreach (JFilesSource source in _sources)
			{
				partialResult = source.GetFiles(path, recurse, searchPattern);

				if (partialResult != null)
					foreach (string qualifiedName in partialResult)
						totalResult.Add(qualifiedName);
			}

			return totalResult;
		}

		/// <summary>
		/// Gets the collection of files on a defined path.
		/// </summary>
		/// <param name="definedPath">A defined path where to search the file.</param>
		/// <param name="recurse">If the search should be recursive (include subdirectories) or not.</param>
		/// <param name="searchPattern">Mask to filter the files.</param>
		/// <returns>The collection of files of the directory.</returns>
		/// <remarks>
		/// Note to implementers: the file names should be correct names for the method "Stream GetFile(string qualifiedName)".
		/// </remarks>
		public override Collection<string> GetFilesFromDefinedPath(string definedPath, bool recurse, string searchPattern)
		{
			Collection<string> totalResult, partialResult;

			totalResult = new Collection<string>(new List<string>());
			foreach (JFilesSource source in _sources)
			{
				partialResult = source.GetFilesFromDefinedPath(definedPath, recurse, searchPattern);

				if (partialResult != null)
					foreach (string qualifiedName in partialResult)
						totalResult.Add(qualifiedName);
			}

			return totalResult;
		}

		#endregion
	}
}
