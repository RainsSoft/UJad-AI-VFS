#region Using Directives

using System;
using System.IO;
using System.IO.Compression;

#endregion

namespace JadEngine.VFS.Storage.Filters
{
	/// <summary>
	/// This filter applies the deflate compression algorithm.
	/// </summary>
	public class JDeflateCompressionFilter : IJFilter
	{
		#region Properties

		/// <summary>
		/// Long name of the filter
		/// </summary>
		/// <remarks>Long name is "Deflate Compression"</remarks>
		public string LongName
		{
			get { return "Deflate Compression"; }
		}

		/// <summary>
		/// Short name of the filter
		/// </summary>
		/// <remarks>Short name is "DFLT"</remarks>
		public string ShortName
		{
			get { return "DFLT"; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		public JDeflateCompressionFilter() { }

		#endregion

		#region Methods

		/// <summary>
		/// Applies the filter for reading. This method modifies 
		/// the input stream and creates a new one that is returned.
		/// </summary>
		/// <param name="input">The input stream</param>
		/// <returns>The input stream with the filter applied</returns>
		public Stream ApplyForRead(Stream input)
		{
			return new DeflateStream(input, CompressionMode.Decompress);
		}

		/// <summary>
		/// Applies the filter for writing. This method modifies 
		/// the input stream and creates a new one that is returned.
		/// </summary>
		/// <param name="input">The input stream</param>
		/// <returns>The input stream with the filter applied</returns>
		public Stream ApplyForWrite(Stream input)
		{
			return new DeflateStream(input, CompressionMode.Compress);
		}

		#endregion
	}
}
