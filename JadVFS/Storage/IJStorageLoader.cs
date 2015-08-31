#region Using Directives

using System;
using System.IO;

#endregion

namespace JadEngine.VFS
{
	/// <summary>
	/// Defines the minimum functionality for a class that reads and writes storage files
	/// </summary>
	public interface IJStorageLoader
	{
		#region Methods

		/// <summary>
		/// Reads the storage file
		/// </summary>
		/// <param name="storage">Storage object where to read the data</param>
		/// <param name="br">Reader to read the file</param>
		void Read(JStorageSource storage, BinaryReader br);

		/// <summary>
		/// Writes a storage file to disk
		/// </summary>
		/// <param name="newStorage">Storage to write to disk</param>
		/// <param name="oldStorage">Old storage to overwrite or update</param>
		/// <param name="fileName">Name of the storage on disk</param>
		void Write(JStorageSource newStorage, JStorageSource oldStorage, String fileName);

		#endregion
	}
}
