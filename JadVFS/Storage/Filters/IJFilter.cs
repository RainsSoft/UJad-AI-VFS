#region Using Directives

using System;
using System.IO;

#endregion

namespace JadEngine.VFS.Storage.Filters
{
	/// <summary>
	/// Common interface for all filters. A filter is a class that modifies
	/// how a file inside a <see cref="JStorageSource"/> is read or writen.
	/// </summary>
	public interface IJFilter
	{
		#region Properties

		/// <summary>
		/// Long name of the filter
		/// </summary>
		string LongName { get; }

		/// <summary>
		/// Short name of the filter
		/// </summary>
		string ShortName { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Applies the filter for reading. This method modifies 
		/// the input stream and creates a new one that is returned.
		/// </summary>
		/// <param name="input">The input stream</param>
		/// <returns>The input stream with the filter applied</returns>
		Stream ApplyForRead(Stream input);

		/// <summary>
		/// Applies the filter for writing. This method modifies 
		/// the input stream and creates a new one that is returned.
		/// </summary>
		/// <param name="input">The input stream</param>
		/// <returns>The input stream with the filter applied</returns>
		Stream ApplyForWrite(Stream input);

		#endregion
	}
}
