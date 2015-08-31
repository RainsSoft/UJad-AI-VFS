#region Using Directives

using System;
using System.IO;

#endregion

namespace JadEngine.VFS
{
	/// <summary>
	/// A <see cref="JWritableSource"/> is a special type of <see cref="JFilesSource"/> that
	/// allows write operations on files, or to create files inside the source.
	/// </summary>
	public abstract class JWritableSource : JFilesSource
	{
		#region Methods

		/// <summary>
		/// Gets a read/write stream to a file.
		/// </summary>
		/// <param name="path">Relative path of the file.</param>
		/// <param name="fileName">Name of the file.</param>
		/// <param name="recurse">If the search should be recursive or not.</param>
		/// <returns>A stream to the file.</returns>
		public abstract Stream GetWritableFile(string path, string fileName, bool recurse);

		/// <summary>
		/// Gets a read/write stream to a file.
		/// </summary>
		/// <param name="qualifiedName">Relative path and name of the file.</param>
		/// <returns>A stream to the file.</returns>
		/// <remarks>This search is never recursive.</remarks>
		public abstract Stream GetWritableFile(string qualifiedName);

		/// <summary>
		/// Gets a read/write to a file.
		/// </summary>
		/// <param name="definedPath">A defined path where to search the file.</param>
		/// <param name="fileName">Name of the file.</param>
		/// <param name="recurse">If the search should be recursive or not.</param>
		/// <returns>A stream to the file.</returns>
		public abstract Stream GetWritableFileFromDefinedPath(string definedPath, string fileName, bool recurse);

		/// <summary>
		/// Finds a file and returns a read/write stream to it.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <returns>A stream to the file.</returns>
		/// <remarks>This search is always recursive.</remarks>
		public abstract Stream FindWritableFile(string fileName);

		/// <summary>
		/// Creates a new file.
		/// </summary>
		/// <param name="qualifiedName">Qualified name of the new file.</param>
		/// <returns>A stream to the new file.</returns>
		public abstract Stream CreateWritableFile(string qualifiedName);

		/// <summary>
		/// Creates a new file.
		/// </summary>
		/// <param name="path">Path of the new file.</param>
		/// <param name="fileName">Name of the new file.</param>
		/// <returns>A stream to the new file.</returns>
		public abstract Stream CreateWritableFile(string path, string fileName);

		/// <summary>
		/// Creates a new file.
		/// </summary>
		/// <param name="definedPath">Defined path where the file will be located.</param>
		/// <param name="qualifiedName">Qualified name of the new file.</param>
		/// <returns>A stream to the new file.</returns>
		public abstract Stream CreateWritableFileOnDefinedPath(string definedPath, string qualifiedName);

		/// <summary>
		/// Creates a new file.
		/// </summary>
		/// <param name="definedPath">Defined path where the file will be located.</param>
		/// <param name="path">Path of the new file.</param>
		/// <param name="fileName">Name of the new file.</param>
		/// <returns>A stream to the new file.</returns>
		public abstract Stream CreateWritableFileOnDefinedPath(string definedPath, string path, string fileName);

		#endregion
	}
}
