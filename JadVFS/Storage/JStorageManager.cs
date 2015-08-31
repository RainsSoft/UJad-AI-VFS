#region Using Directives

using System;

#endregion

namespace JadEngine.VFS.Storage
{
	/// <summary>
	/// Returns loaders according to a storage file version
	/// </summary>
	/// <remarks>
	/// Storage files are marked with their version in their first 4 bytes, so a loader must be created
	/// to manage a file depending on the version.
	/// </remarks>
	public static class JStorageManager
	{
		#region Methods

		/// <summary>
		/// Gets a correct loader for a file version
		/// </summary>
		/// <param name="version">File version to check</param>
		/// <returns>The loader for the version</returns>
		/// <exception cref="ArgumentException">If the version is not known</exception>
		public static IJStorageLoader GetLoader(int version)
		{
			switch (version)
			{
				case 1:
					return new JStorageLoaderv1();

				default:
					throw new ArgumentException("Unknown storage version " + version + ". No loader found to work with it");
			}
		}

		#endregion
	}
}
