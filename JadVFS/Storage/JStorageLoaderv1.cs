#region Using Directives

using System;
using System.Collections.Generic;
using System.IO;

using JadEngine.VFS.Storage.Filters;

#endregion

namespace JadEngine.VFS.Storage
{
	/// <summary>
	/// Reads and writes storage files of version 1.
	/// </summary>
	internal class JStorageLoaderv1 : IJStorageLoader
	{
		#region Static Fields

		public const string Root = "Root";

		#endregion

		#region Fields

		/// <summary>
		/// New storage read or written
		/// </summary>
		private JStorageSource _newStorage;

		/// <summary>
		/// Old storage (when overwritting one storage)
		/// </summary>
		private JStorageSource _oldStorage;

		/// <summary>
		/// List of file offsets (to create the header on writting)
		/// </summary>
		private List<long> _fileOffsets;

		/// <summary>
		/// Temporary file for the new storage
		/// </summary>
		private string _tempFile;

		/// <summary>
		/// Writer to write the file
		/// </summary>
		private BinaryWriter writer;

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		internal JStorageLoaderv1() { }

		#endregion

		#region Methods

		/// <summary>
		/// Reads a storage file
		/// </summary>
		/// <param name="storageFile">Storage object where to read the data</param>
		/// <param name="br">Reader to read the file</param>
		public void Read(JStorageSource storageFile, BinaryReader br)
		{
			int numberOfFilters, numberOfDirectories, numberOfFiles;

			_newStorage = storageFile;
			_newStorage.Name = br.ReadString();

			// Read the number of things in the storage
			numberOfFilters = br.ReadInt32();
			numberOfDirectories = br.ReadInt32();
			numberOfFiles = br.ReadInt32();

			// Load data
			LoadFilters(br, numberOfFilters);
			LoadDirectories(br, numberOfDirectories);
			LoadFiles(br, numberOfFiles);

			// Change the root directory
			_newStorage.Root = _newStorage.Root.GetDirectory("Root");
			_newStorage.Root.Parent = null;
		}

		/// <summary>
		/// Writes a storage file to disk
		/// </summary>
		/// <param name="newStorage">Storage to write to disk</param>
		/// <param name="oldStorage">Old storage to overwrite or update</param>
		/// <param name="fileName">Name of the storage on disk</param>
		public void Write(JStorageSource newStorage, JStorageSource oldStorage, string fileName)
		{
			FileStream file;

			// Create  a temporal file
			_tempFile = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileName(Path.GetTempFileName()));

			file = new FileStream(_tempFile, FileMode.Create, FileAccess.ReadWrite);
			writer = new BinaryWriter(file);

			_newStorage = newStorage;
			_oldStorage = oldStorage;

			WriteHeader();
			WriteFilters();
			WriteDirectories();

			// Write the files, the list of offsets is needed to be able to jump around writting files headers and files data
			_fileOffsets = new List<long>();
			WriteFilesHeader();
			WriteFiles(newStorage.Root);

			writer.Flush();
			writer.Close();

			// If the file exits, delete it
			if (File.Exists(fileName))
				File.Delete(fileName);

			// Rename the temporal file
			File.Copy(_tempFile, fileName);
			File.Delete(_tempFile);
		}

		#endregion

		#region Load Methods

		/// <summary>
		/// Loads the filters into the storage file.
		/// </summary>
		/// <param name="reader">Reader.</param>
		/// <param name="numberOfFilters">Number of filters in the storage file.</param>
		private void LoadFilters(BinaryReader reader, int numberOfFilters)
		{
			String filterName;

			for (int i = 0; i < numberOfFilters; i++)
			{
				//Create the filter
				filterName = reader.ReadString();
                string fileClassName = filterName.Split(',')[0];
                IJFilter filter = (IJFilter)Activator.CreateInstance(Type.GetType(fileClassName));

				_newStorage.Filters.Add(filter);
			}
		}

		/// <summary>
		/// Loads the directories into the storage file.
		/// </summary>
		/// <param name="reader">Reader.</param>
		/// <param name="numberOfDirectories">Number of directories in the storage file.</param>
		private void LoadDirectories(BinaryReader reader, int numberOfDirectories)
		{
			String name, path;
			JDirectory aux, directory;

			//Create the root directory
			_newStorage.Root = new JDirectory("");

			//Auxiliary directory
			aux = new JDirectory("Root");
			_newStorage.Root.AddDirectory(aux);

			for (int i = 0; i < numberOfDirectories; i++)
			{
				//Create the new directory
				name = reader.ReadString();
				directory = new JDirectory(name);

				//Get the path
				path = reader.ReadString();

				//Get the path and add the directory
				_newStorage.Root.AddDirectory(path, directory);
			}
		}

		/// <summary>
		/// Loads the files into the storage file.
		/// </summary>
		/// <param name="reader">Reader.</param>
		/// <param name="numberOfFiles">Number of files in the storage file.</param>
		private void LoadFiles(BinaryReader reader, int numberOfFiles)
		{
			String name, path;
			long offset, length;
			int numberOfFilters, filterIndex;
			JFile file;

			for (int i = 0; i < numberOfFiles; i++)
			{
				//Read the file fields
				name = reader.ReadString();
				path = reader.ReadString();
				offset = reader.ReadInt64();
				length = reader.ReadInt64();

				//Create the file and add it to the directory
				file = new JFile(name, offset, length);
				_newStorage.Root.AddFile(path, file);

				//Read the filters that affect this file
				numberOfFilters = reader.ReadInt32();
				for (int j = 0; j < numberOfFilters; j++)
				{
					filterIndex = reader.ReadInt32();
					file.Filters.Add(_newStorage.Filters[filterIndex]);
				}
			}
		}

		#endregion

		#region Write Methods

		/// <summary>
		/// Writes the storage header into the storage file.
		/// </summary>
		private void WriteHeader()
		{
			writer.Write(_newStorage.Version);
			writer.Write(_newStorage.Name);

			writer.Write(_newStorage.Filters.Count);
			writer.Write(_newStorage.Root.GetTotalNumberOfDirectories());
			writer.Write(_newStorage.Root.GetTotalNumberOfFiles());
		}

		/// <summary>
		/// Writes the filters into the storage file.
		/// </summary>
		private void WriteFilters()
		{
			List<IJFilter> filters;

			filters = _newStorage.Filters;
			foreach (IJFilter filter in filters)
				writer.Write(filter.GetType().AssemblyQualifiedName);
		}

		/// <summary>
		/// Writes the directories into the file.
		/// </summary>
		/// <remarks>
		/// Driver method for recursivity.
		/// </remarks>
		private void WriteDirectories()
		{
			String path;
			List<JDirectory> directories;

			path = JStorageLoaderv1.Root;

			directories = _newStorage.Root.GetDirectories();
			foreach (JDirectory directory in directories)
				WriteDirectories(directory, path);
		}

		/// <summary>
		/// Writes a <see cref="JDirectory"/> and all its subdirectories into the storage file.
		/// </summary>
		/// <param name="directory">Directory to strat writing.</param>
		/// <param name="path">Path of the directory.</param>
		private void WriteDirectories(JDirectory directory, string path)
		{
			String newPath;
			List<JDirectory> directories;

			writer.Write(directory.Name);
			writer.Write(path);

			newPath = path + Path.DirectorySeparatorChar + directory.Name;
			directories = directory.GetDirectories();

			foreach (JDirectory childDirectory in directories)
				WriteDirectories(childDirectory, newPath);
		}

		/// <summary>
		/// Writes the files headers into the storage file.
		/// </summary>
		/// <remarks>
		/// Driver method for recursivity.
		/// </remarks>
		private void WriteFilesHeader()
		{
			String path;
			List<JFile> files;
			List<JDirectory> directories;

			path = JStorageLoaderv1.Root;

			files = _newStorage.Root.GetFiles();
			foreach (JFile file in files)
				WriteFileHeader(file, path);

			directories = _newStorage.Root.GetDirectories();
			foreach (JDirectory directory in directories)
				WriteFilesHeader(directory, path);
		}

		/// <summary>
		/// Writes all <see cref="JFile"/> headers in a directory and its subdirectories into the storage file.
		/// </summary>
		/// <param name="directory">Directory to start writing.</param>
		/// <param name="path">Path of the directory.</param>
		private void WriteFilesHeader(JDirectory directory, string path)
		{
			String newPath;
			List<JFile> files;
			List<JDirectory> directories;

			newPath = path + Path.DirectorySeparatorChar + directory.Name;

			files = directory.GetFiles();
			foreach (JFile file in files)
				WriteFileHeader(file, newPath);

			directories = directory.GetDirectories();
			foreach (JDirectory childDirectory in directories)
				WriteFilesHeader(childDirectory, newPath);
		}

		/// <summary>
		/// Writes a <see cref="JFile"/> header into the storage file.
		/// </summary>
		/// <param name="file"><see cref="JFile"/> to write.</param>
		/// <param name="path">Path of the <see cref="JFile"/></param>
		private void WriteFileHeader(JFile file, string path)
		{
			int index;

			// Write the file header
			writer.Write(file.Name);
			writer.Write(path);

			// Mark the offset position for this file
			_fileOffsets.Add(writer.BaseStream.Position);

			writer.Write((long) -1); // The real offset has to be calculated later on
			writer.Write((long) -1); // The real length has to be calculated later on (it can change if the file has filters applied to it)

			// Write the filters data
			writer.Write(file.Filters.Count);
			for (int i = 0; i < file.Filters.Count; i++)
			{
				// Find the filter in the filters list
				index = -1;
				for (int j = 0; j < _newStorage.Filters.Count; j++)
					if (file.Filters[i] == _newStorage.Filters[j])
					{
						index = j;
						break;
					}

				if (index == -1)
					throw new ApplicationException("The file " + file.Name + " has a filter called " + file.Filters[i].GetType().ToString() + " that doesn´t exist in the filter list of the storage file.");

				writer.Write(index);
			}
		}

		/// <summary>
		/// Writes all <see cref="JFile"/> data into the storage file.
		/// </summary>
		/// <param name="directory">Root directory to start writing.</param>
		private void WriteFiles(JDirectory directory)
		{
			List<JFile> files;
			List<JDirectory> directories;

			files = directory.GetFiles();
			foreach (JFile file in files)
				WriteFile(file);

			directories = directory.GetDirectories();
			foreach (JDirectory childDirectory in directories)
				WriteFiles(childDirectory);
		}

		/// <summary>
		/// Writes a <see cref="JFile"/> data into the storage file.
		/// </summary>
		/// <param name="file"><see cref="JFile"/> to write.</param>
		private void WriteFile(JFile file)
		{
			Stream inputStream;
			long start, end, totalBytesWriten;
			int bytesRead;
			byte[] bytes;

			// Get the offset position of the file
			file.Offset = writer.BaseStream.Position;

			// Check the file's location and get the stream to read it
			inputStream = null;

			if (file.FileInfo.Type == JFileType.Disk)
				inputStream = file.FileInfo.Info.Open(FileMode.Open, FileAccess.Read);

			if (file.FileInfo.Type == JFileType.Storage)
				inputStream = _oldStorage.GetFile(Path.GetDirectoryName(file.GetQualifiedName()), file.Name, false);

			if (inputStream == null)
				throw new ArgumentException("The JFileType of the file " + file.Name + " is not correct.");

			// Auxiliar buffer for reading
			bytes = new byte[4096];

			// No filters, live is nice and lovely
			if (file.Filters.Count == 0)
			{
				// Calculate the real length as the file could have being compressed
				totalBytesWriten = 0;
				while ((bytesRead = inputStream.Read(bytes, 0, bytes.Length)) != 0)
				{
					writer.Write(bytes, 0, bytesRead);
					totalBytesWriten += bytesRead;
				}

				end = writer.BaseStream.Position;				
			}

			else // The file has filters, apply them to write and prepare to have fun
			{
				// Get the start position
				start = writer.BaseStream.Position;

				// Close the writer (we are going to modify the stream)
				writer.Close();

				// Reopen the file stream and apply the filters
				FileStream auxFile = new FileStream(_tempFile, FileMode.Append, FileAccess.Write);
				Stream auxStream = file.ApplyFiltersForWrite(auxFile);

				// Create the new writer and write the data
				BinaryWriter auxWriter = new BinaryWriter(auxStream);
				while ((bytesRead = inputStream.Read(bytes, 0, bytes.Length)) != 0)
					auxWriter.Write(bytes, 0, bytesRead);

				auxWriter.Flush();

				// Get the end position and calculate the length
				end = auxFile.Position;
				totalBytesWriten = end - start;

				// Close the auxiliar writer and reopen the file without the filters
				auxWriter.Close();
				writer = new BinaryWriter(new FileStream(_tempFile, FileMode.Open, FileAccess.ReadWrite));
			}

			// Close the input file
			inputStream.Close();

			// Write the file offset and length in the file header info
			writer.Seek((int) _fileOffsets[0], SeekOrigin.Begin);
			writer.Write(file.Offset);
			writer.Write(totalBytesWriten);

			// Remove the header offset of this file
			_fileOffsets.RemoveAt(0);

			// Seek to the end
			writer.Seek(0, SeekOrigin.End);
		}

		#endregion
	}
}
